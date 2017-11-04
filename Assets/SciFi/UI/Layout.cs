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

        public enum ScaleMode {
            /// Size is in units with no scale adjustment.
            UnadjustedUnits,
            /// Size is in units normalized to a standard screen unit.
            ScaleNormalizedUnits,
            /// Size is a percentage of regular size at normalized scale.
            ScaleNormalizedPercent,
            /// Size is a percentage of regular size with the same scale
            /// as the reference object.
            ReferenceScalePercent,
            /// Size is a percentage of the size of the reference object.
            ReferenceSizePercent,
            /// Size is a percentage of screen size.
            ScreenPercent,
            /// Size is calculated from the other side to maintain the aspect ratio.
            MaintainAspect,
            /// Size is in pixels scaled to standard DPI.
            DpiScaledPixels,
            /// Size is in pixels.
            Pixels,
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
        private const float invStandardDpi = 1f / 96f;

        public Layout positionRef = null;
        public Layout sizeRef = null;
        public Anchor anchor = Anchor.Center;
        public bool overlapParent = false;
        public ScreenAspectScaleMode screenAspectScaling = ScreenAspectScaleMode.None;
        public Vector2Int designAspect = new Vector2Int(16, 9);
        public ScaleMode widthMode = ScaleMode.ScaleNormalizedUnits;
        public ScaleMode heightMode = ScaleMode.ScaleNormalizedUnits;
        public ScaleMode offsetMode = ScaleMode.ScaleNormalizedUnits;
        public float width = 1;
        public float height = 1;
        public Vector2 offset = new Vector2(0, 0);
        public bool rectTransformScaling = false;
        public Vector2 rtBaseSize = new Vector2(200, 100);

#if UNITY_EDITOR
        public RefreshButton resyncRtSize = new RefreshButton("sync-rect-size");
        public RefreshButton refreshAllLayouts = new RefreshButton("refresh");

        void OnValidate() {
            if (!Application.isPlaying) {
                isCurrent = false;
                staticsInitialized = false;
                InitializeStatics();
                CalculateLayoutRecursive();
            }
        }
#endif
        private bool isCurrent = false;
        private Vector2 unscaledSize;
        private Vector2 scaledSize;
        private Vector2 baseScale;
        private Vector2 adjustedScale;

        private static bool staticsInitialized = false;
        private static Vector2 screenSize;
        private static Vector2 cameraPixels;
        private static float dpiScale;

        static void InitializeStatics() {
            if (staticsInitialized) {
                return;
            }

            screenSize.y = Camera.main.orthographicSize * 2;
            screenSize.x = screenSize.y * Camera.main.aspect;
            cameraPixels.y = Camera.main.pixelHeight;
            cameraPixels.x = Camera.main.pixelWidth;
            // Round scales to the nearest .5x.
            dpiScale = Mathf.Round(2 * Screen.dpi * invStandardDpi) * .5f;

            staticsInitialized = true;
        }

        void IRefreshComponent.RefreshComponent(string action) {
            if (action == "refresh") {
                staticsInitialized = false;
                InitializeStatics();
                var all = FindObjectsOfType<Layout>();
                foreach (var layout in all) {
                    layout.isCurrent = false;
                }
                foreach (var layout in all) {
                    layout.CalculateLayoutRecursive();
                }
            } else if (action == "sync-rect-size") {
                var rt = GetComponent<RectTransform>();
                if (rt == null) { return; }
                rtBaseSize = rt.rect.size;
            }
        }

        void Start() {
            InitializeStatics();
            CalculateLayoutRecursive();
        }

        void CalculateLayoutRecursive(Layout cycle = null) {
            if (isCurrent) {
                return;
            }
            if (cycle == null) {
                cycle = this;
            } else if (object.ReferenceEquals(cycle, this)) {
                throw new InvalidOperationException(
                    "Infinite recursion while calculating layout"
                );
            }
            if (positionRef != null) {
                positionRef.CalculateLayoutRecursive(cycle);
            }
            if (sizeRef != null) {
                sizeRef.CalculateLayoutRecursive(cycle);
            }
            CalculateLayout();
            isCurrent = true;
        }

        void CalculateLayout() {
            var rt = GetComponent<RectTransform>();
            if (rt != null && rectTransformScaling) {
                CalculateRectTransformLayout(rt);
                return;
            }

            Vector3 position;
            transform.localScale = Vector3.one;
            unscaledSize = GetSize();
            baseScale = transform.lossyScale;
            adjustedScale = GetScale();
            transform.localScale = new Vector3(adjustedScale.x, adjustedScale.y, 1);
            adjustedScale.x *= baseScale.x;
            adjustedScale.y *= baseScale.y;
            var offsets = GetOffsets();
            scaledSize = unscaledSize;
            scaledSize.x *= adjustedScale.x;
            scaledSize.y *= adjustedScale.y;
            if (positionRef == null) {
                position = GetReferenceAnchor(Vector2.zero, screenSize);
            } else {
                position = GetReferenceAnchor(
                    positionRef.transform.position,
                    positionRef.scaledSize
                );
            }
            Vector3 overlap = GetOverlap();
            if (overlapParent) {
                position += overlap + (Vector3)offsets;
            } else {
                position -= overlap - (Vector3)offsets;
            }
            position.z = 0;
            transform.position = position;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                transform.localPosition.y,
                0
            );
        }

        void CalculateRectTransformLayout(RectTransform rt) {
            Vector3 position;
            baseScale = transform.lossyScale;
            baseScale.x /= transform.localScale.x;
            baseScale.y /= transform.localScale.y;
            unscaledSize.x = rtBaseSize.x * baseScale.x;
            unscaledSize.y = rtBaseSize.y * baseScale.y;
            var rectScale = GetScale();
            adjustedScale = rectScale;
            adjustedScale.x *= transform.lossyScale.x;
            adjustedScale.y *= transform.lossyScale.y;
            var offsets = GetOffsets();
            scaledSize = unscaledSize;
            scaledSize.x *= adjustedScale.x;
            scaledSize.y *= adjustedScale.y;
            if (positionRef == null) {
                position = GetReferenceAnchor(Vector2.zero, screenSize);
            } else {
                position = GetReferenceAnchor(
                    positionRef.transform.position,
                    positionRef.scaledSize
                );
            }
            Vector3 overlap = GetOverlap();
            if (overlapParent) {
                position += overlap + (Vector3)offsets;
            } else {
                position -= overlap - (Vector3)offsets;
            }
            position.x /= baseScale.x;
            position.y /= baseScale.y;
            position.z = 0;
            rt.localPosition = position;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rtBaseSize.x * adjustedScale.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rtBaseSize.y * adjustedScale.y);
        }

        /// Returns original size without scaling.
        Vector2 GetSize() {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) {
                var size = sr.bounds.size;
                size.x /= transform.lossyScale.x;
                size.y /= transform.lossyScale.y;
                return size;
            }
            var rt = GetComponent<RectTransform>();
            var r = rt.rect;
            var x = (r.max.x - r.min.x);
            var y = (r.max.y - r.min.y);
            return new Vector2(x, y);
        }

        static float VectorGetX(Vector2 vec) {
            return vec.x;
        }

        static float VectorGetY(Vector2 vec) {
            return vec.y;
        }

        float GetScaleValue(
            ScaleMode mode,
            float value,
            Layout reference,
            Func<Vector2, float> axis
        )
        {
            float newValue;
            switch (mode) {
            case ScaleMode.UnadjustedUnits:
                return value;
            case ScaleMode.ScaleNormalizedUnits:
                newValue = value / axis(unscaledSize);
                break;
            case ScaleMode.ScaleNormalizedPercent:
                newValue = value;
                break;
            case ScaleMode.ReferenceScalePercent:
                if (reference == null) {
                    goto case ScaleMode.ScaleNormalizedPercent;
                }
                newValue
                    = value
                    * axis(reference.adjustedScale);
                break;
            case ScaleMode.ReferenceSizePercent:
                if (reference == null) {
                    goto case ScaleMode.ScaleNormalizedPercent;
                }
                newValue
                    = value
                    * axis(reference.scaledSize)
                    / axis(unscaledSize);
                break;
            case ScaleMode.ScreenPercent:
                newValue = value * axis(screenSize) / axis(unscaledSize);
                break;
            case ScaleMode.DpiScaledPixels:
                newValue = value * axis(screenSize) * dpiScale / axis(cameraPixels);
                break;
            case ScaleMode.Pixels:
                newValue = value * axis(screenSize) / axis(cameraPixels);
                break;
            default:
                return 0;
            }
            return newValue / axis(baseScale);
        }

        float GetOffsetValue(
            ScaleMode mode,
            float value,
            Layout reference,
            Func<Vector2, float> axis
        )
        {
            switch (mode)
            {
            case ScaleMode.UnadjustedUnits:
                return value * axis(baseScale);
            case ScaleMode.ScaleNormalizedUnits:
                return value * axis(adjustedScale);
            case ScaleMode.ScaleNormalizedPercent:
                return value * axis(scaledSize);
            case ScaleMode.ReferenceScalePercent:
                if (reference == null) {
                    goto case ScaleMode.ScaleNormalizedPercent;
                }
                return value * axis(unscaledSize) * axis(reference.adjustedScale);
            case ScaleMode.ReferenceSizePercent:
                if (reference == null) {
                    goto case ScaleMode.ReferenceSizePercent;
                }
                return value * axis(reference.scaledSize);
            case ScaleMode.ScreenPercent:
                return value * axis(screenSize);
            case ScaleMode.DpiScaledPixels:
                return value * axis(screenSize) * dpiScale / axis(cameraPixels);
            case ScaleMode.Pixels:
                return value * axis(screenSize) / axis(cameraPixels);
            default:
                return 0;
            }
        }

        Vector2 GetScale() {
            Vector2 scale;
            var reference = sizeRef ?? positionRef;
            scale.x = GetScaleValue(widthMode, width, reference, VectorGetX);
            scale.y = GetScaleValue(heightMode, height, reference, VectorGetY);
            if (heightMode == ScaleMode.MaintainAspect) {
                scale.y = scale.x * baseScale.x / baseScale.y;
            } else if (widthMode == ScaleMode.MaintainAspect) {
                scale.x = scale.y * baseScale.y / baseScale.x;
            }
            SetAspectScaling(ref scale);
            return scale;
        }

        // FIXME - Shrink and grow modes are wrong.
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

        Vector3 GetReferenceAnchor(Vector2 refPosition, Vector2 refSize) {
            Vector2 referenceExtents = refSize / 2;
            float x = refPosition.x;
            float y = refPosition.y;
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
            return new Vector3(x, y, transform.position.z);
        }

        Vector2 GetOverlap() {
            Vector2 myExtents = scaledSize / 2;
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
            Vector2 offsets;
            var mode = offsetMode == ScaleMode.MaintainAspect ? ScaleMode.Pixels : offsetMode;
            offsets.x = GetOffsetValue(mode, offset.x, positionRef, VectorGetX);
            offsets.y = GetOffsetValue(mode, offset.y, positionRef, VectorGetY);
            // TODO: Make this be scaled to the standard aspect ratio.
            if (offsetMode == ScaleMode.MaintainAspect) {
                if (cameraPixels.x > cameraPixels.y) {
                    offsets.x *= cameraPixels.y / cameraPixels.x;
                } else {
                    offsets.x *= cameraPixels.x / cameraPixels.y;
                }
            }
            return offsets;
        }
    }
}