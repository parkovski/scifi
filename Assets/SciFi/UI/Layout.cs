using System;
using UnityEngine;
using SciFi.Util.Extensions;

namespace SciFi.UI {
    [ExecuteInEditMode]
    public class Layout : MonoBehaviour, IRefreshComponent {
        public enum Anchor {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, Center, MiddleRight,
            BottomLeft, BottomCenter, BottomRight,
        }

        public enum OffsetMode {
            /// Offset is a percentage of my size.
            Self,
            /// Offset is a percentage of the screen size.
            Screen,
            /// Offset is in Unity units.
            UnityUnits,
            /// Offset is in pixels.
            Pixels,
        }

        public enum ScaleMode {
            /// Size is specified on this object.
            Self,
            /// Size is adjusted to maintain original aspect ratio.
            Aspect,
            /// Size is based on parent (reference) size.
            Parent,
            /// Size is based on screen size.
            Screen,
        }

        public enum ScreenAspectScaleMode {
            /// No aspect scaling.
            None,
            /// Scale by shrinking the bigger side.
            Shrink,
            /// Scale by growing the smaller side.
            Grow,
            /// Scale by changing the width.
            Width,
            /// Scale by changing the height.
            Height,
            /// Scale maintaining the same area.
            Area,
            /// Scale maintaining the same diagonal.
            Diagonal,
        }

        private const float screenDiagonal = 13.333333f;

        public Layout reference = null;
        public Anchor anchor = Anchor.Center;
        public bool overlapParent = false;
        public ScreenAspectScaleMode screenAspectScaling = ScreenAspectScaleMode.None;
        public Vector2Int designAspect = new Vector2Int(16, 9);
        public ScaleMode widthMode = ScaleMode.Self;
        public ScaleMode heightMode = ScaleMode.Self;
        public OffsetMode offsetMode = OffsetMode.UnityUnits;
        public float percentWidth = 1;
        public float percentHeight = 1;
        public Vector2 offset = new Vector2(0, 0);
        public bool rectTransformScaling = false;
        public Vector2 rtBaseSize = new Vector2(200, 100);

#if UNITY_EDITOR
        public RefreshButton resyncSize = new RefreshButton("sync-size");
        public RefreshButton refreshAllLayouts = new RefreshButton("refresh");

        void OnValidate() {
            if (!Application.isPlaying) {
                isCurrent = false;
                CalculateLayoutRecursive();
            }
        }
#endif
        private bool isCurrent = false;

        void IRefreshComponent.RefreshComponent(string action) {
            if (action == "refresh") {
                var all = FindObjectsOfType<Layout>();
                foreach (var layout in all) {
                    layout.isCurrent = false;
                }
                foreach (var layout in all) {
                    layout.CalculateLayoutRecursive();
                }
            } else if (action == "sync-size") {
                var rt = GetComponent<RectTransform>();
                if (rt == null) { return; }
                rtBaseSize = rt.rect.size;
            }
        }

        void Start() {
            CalculateLayoutRecursive();
        }

        void CalculateLayoutRecursive() {
            if (isCurrent) {
                return;
            }
            if (reference != null) {
                reference.CalculateLayoutRecursive();
            }
            CalculateLayout();
            isCurrent = true;
        }

        void CalculateLayout() {
            Vector3 position;
            SetScale();
            if (reference == null) {
                position = GetScreenAnchor();
            } else {
                position = GetReferenceAnchor();
            }
            Vector3 overlap = GetOverlap();
            if (overlapParent) {
                position += overlap + (Vector3)GetOffsets();
            } else {
                position -= overlap - (Vector3)GetOffsets();
            }
            transform.position = position;
        }

        Vector2 GetSize() {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) {
                return sr.bounds.size;
            }
            var rt = GetComponent<RectTransform>();
            var r = rt.rect;
            var x = (r.max.x - r.min.x) * transform.lossyScale.x;
            var y = (r.max.y - r.min.y) * transform.lossyScale.y;
            return new Vector2(x, y);
        }

        void SetScale() {
            var screenHeight = Camera.main.orthographicSize * 2;
            var screenWidth = Camera.main.aspect * screenHeight;
            var rt = GetComponent<RectTransform>();
            if (rt == null || !rectTransformScaling) {
                if (transform.parent != null) {
                    transform.localScale = new Vector3(
                        1 / transform.parent.lossyScale.x,
                        1 / transform.parent.lossyScale.y,
                        transform.localScale.z
                    );
                } else {
                    transform.localScale = new Vector3(1, 1, transform.localScale.z);
                }
            } else {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rtBaseSize.x);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rtBaseSize.y);
            }
            var mySize = GetSize();
            Vector2 scale = Vector2.one;
            switch (heightMode) {
            case ScaleMode.Self:
                scale.y = percentHeight;
                break;
            case ScaleMode.Parent:
                if (reference == null) {
                    goto case ScaleMode.Screen;
                }
                scale.y = percentHeight * reference.GetSize().y / mySize.y;
                break;
            case ScaleMode.Screen:
                scale.y = screenHeight * percentHeight / mySize.y;
                break;
            }
            switch (widthMode) {
            case ScaleMode.Self:
                scale.x = percentWidth;
                break;
            case ScaleMode.Parent:
                if (reference == null) {
                    goto case ScaleMode.Screen;
                }
                scale.x = percentWidth * reference.GetSize().x / mySize.x;
                break;
            case ScaleMode.Screen:
                scale.x = screenWidth * percentWidth / mySize.x;
                break;
            }
            if (heightMode == ScaleMode.Aspect && widthMode == ScaleMode.Aspect) {
                scale.x = percentWidth;
                scale.y = percentHeight;
            } else if (heightMode == ScaleMode.Aspect) {
                scale.y = scale.x;
            } else if (widthMode == ScaleMode.Aspect) {
                scale.x = scale.y;
            }
            SetAspectScaling(ref scale);
            if (rt != null && rectTransformScaling) {
                rt.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    rtBaseSize.x * scale.x
                );
                rt.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    rtBaseSize.y * scale.y
                );
            } else {
                transform.localScale = new Vector3(scale.x, scale.y, transform.localScale.z);
            }
        }

        void SetAspectScaling(ref Vector2 scale)
        {
            switch (screenAspectScaling)
            {
            case ScreenAspectScaleMode.None:
                break;
            case ScreenAspectScaleMode.Shrink: {
                if (Camera.main.orthographicSize > designAspect.y / designAspect.x) {
                    goto case ScreenAspectScaleMode.Width;
                } else {
                    goto case ScreenAspectScaleMode.Height;
                }
            }
            case ScreenAspectScaleMode.Grow: {
                if (Camera.main.aspect < designAspect.x / designAspect.y) {
                    goto case ScreenAspectScaleMode.Width;
                } else {
                    goto case ScreenAspectScaleMode.Height;
                }
            }
            case ScreenAspectScaleMode.Height:
                scale.y *= designAspect.x / (Camera.main.aspect * designAspect.y);
                break;
            case ScreenAspectScaleMode.Width:
                scale.x *= designAspect.y * Camera.main.aspect / designAspect.x;
                break;
            default:
                "Mode {0} not implemented".WarnF(screenAspectScaling);
                break;
            }
        }

        Vector3 GetScreenAnchor() {
            float screenHeightExtent = Camera.main.orthographicSize;
            float screenWidthExtent = screenHeightExtent * Camera.main.aspect;
            float z = transform.position.z;
            switch (anchor) {
            case Anchor.TopLeft:
                return new Vector3(-screenWidthExtent, screenHeightExtent, z);
            case Anchor.TopCenter:
                return new Vector3(0, screenHeightExtent, z);
            case Anchor.TopRight:
                return new Vector3(screenWidthExtent, screenHeightExtent, z);
            case Anchor.MiddleLeft:
                return new Vector3(-screenWidthExtent, 0, z);
            case Anchor.Center:
                return new Vector3(0, 0, z);
            case Anchor.MiddleRight:
                return new Vector3(screenWidthExtent, 0, z);
            case Anchor.BottomLeft:
                return new Vector3(-screenWidthExtent, -screenHeightExtent, z);
            case Anchor.BottomCenter:
                return new Vector3(0, -screenHeightExtent, z);
            case Anchor.BottomRight:
                return new Vector3(screenWidthExtent, -screenHeightExtent, z);
            default:
                throw new ArgumentOutOfRangeException(nameof(anchor));
            }
        }

        Vector3 GetReferenceAnchor() {
            Vector2 referenceExtents = reference.GetSize() / 2;
            float x = reference.transform.position.x;
            float y = reference.transform.position.y;
            float z = transform.position.z;
            switch (anchor) {
            case Anchor.TopLeft:
                x -= referenceExtents.x;
                y += referenceExtents.y;
                break;
            case Anchor.TopCenter:
                y += referenceExtents.y;
                break;
            case Anchor.TopRight:
                x += referenceExtents.x;
                y += referenceExtents.y;
                break;
            case Anchor.MiddleLeft:
                x -= referenceExtents.x;
                break;
            case Anchor.Center:
                break;
            case Anchor.MiddleRight:
                x += referenceExtents.x;
                break;
            case Anchor.BottomLeft:
                x -= referenceExtents.x;
                y -= referenceExtents.y;
                break;
            case Anchor.BottomCenter:
                y -= referenceExtents.y;
                break;
            case Anchor.BottomRight:
                x += referenceExtents.x;
                y -= referenceExtents.y;
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(anchor));
            }
            return new Vector3(x, y, z);
        }

        Vector2 GetOverlap() {
            Vector2 myExtents = GetSize() / 2;
            switch (anchor) {
            case Anchor.TopLeft:
                return new Vector2(myExtents.x, -myExtents.y);
            case Anchor.TopCenter:
                return new Vector2(0, -myExtents.y);
            case Anchor.TopRight:
                return new Vector2(-myExtents.x, -myExtents.y);
            case Anchor.MiddleLeft:
                return new Vector2(myExtents.x, 0);
            case Anchor.Center:
                return Vector2.zero;
            case Anchor.MiddleRight:
                return new Vector2(-myExtents.x, 0);
            case Anchor.BottomLeft:
                return new Vector2(myExtents.x, myExtents.y);
            case Anchor.BottomCenter:
                return new Vector2(0, myExtents.y);
            case Anchor.BottomRight:
                return new Vector2(-myExtents.x, myExtents.y);
            default:
                throw new ArgumentOutOfRangeException(nameof(anchor));
            }
        }

        Vector2 GetOffsets() {
            switch (offsetMode)
            {
            case OffsetMode.Self: {
                var size = GetSize();
                return new Vector2(offset.x * size.x, offset.y * size.y);
            }
            case OffsetMode.Screen: {
                var ortho = Camera.main.orthographicSize;
                return new Vector2(offset.x * ortho / Camera.main.aspect, offset.y * ortho);
            }
            case OffsetMode.UnityUnits:
                return offset;
            case OffsetMode.Pixels: {
                var unitHeight = Camera.main.orthographicSize;
                var unitWidth = unitHeight / Camera.main.aspect;
                return new Vector2(
                    offset.x * unitWidth / Camera.main.pixelWidth,
                    offset.y * unitHeight / Camera.main.pixelHeight
                );
            }
            default:
                return Vector2.zero;
            }
        }
    }
}