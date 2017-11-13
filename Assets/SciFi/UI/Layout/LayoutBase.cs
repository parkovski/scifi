using System;
using System.Collections.Generic;
using UnityEngine;
using SciFi.Util.Extensions;

namespace SciFi.UI.Layout {
    [ExecuteInEditMode]
    public abstract class LayoutBase : MonoBehaviour, IRefreshComponent {
        public enum Anchor {
            None,
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

        private const float invStandardDpi = 1f / 96f;
        [NonSerialized]
        private bool isCurrent;
        private static bool staticsInitialized;
        [NonSerialized]
        private bool thisInitialized;

        /// Screen size in units
        protected static Vector2 screenSize { get; private set; }
        /// Screen/camera size in pixels
        protected static Vector2 cameraPixels { get; private set; }
        /// Pixels per unit at the current DPI.
        protected static Vector2 unitsPerPixel { get; private set; }
        /// Pixels per unit at standard DPI.
        protected static Vector2 unitsPerDpiPixel { get; private set; }
        /// Current DPI / standard DPI.
        protected static float dpiScale { get; private set; }

        /// Size without any extra scaling applied.
        public Vector2 unscaledSize { get; protected set; }
        /// Actual size on screen in units.
        public Vector2 scaledSize { get; protected set; }
        /// Default parent scaling - lossyScale if localScale is 1.
        public Vector2 baseScale { get; protected set; }
        /// Applied scale to get from default to scaled size.
        public Vector2 totalScale { get; protected set; }

#if UNITY_EDITOR
        public RefreshButton refreshAllLayouts = new RefreshButton("refresh");
        public RefreshButton printDebug = new RefreshButton("debug");

        void OnValidate() {
            if (!Application.isPlaying) {
                isCurrent = false;
                InitializeStatics();
                GenerateLayoutRecursive();
            }
        }
#endif

        static LayoutBase() {
            staticsInitialized = false;
        }

        public LayoutBase() {
            thisInitialized = false;
            isCurrent = false;
        }

        private static void InitializeStatics() {
            if (staticsInitialized) {
                return;
            }

            Vector2 screenSize;
            screenSize.y = Camera.main.orthographicSize * 2;
            screenSize.x = screenSize.y * Camera.main.aspect;
            LayoutBase.screenSize = screenSize;

            LayoutBase.cameraPixels = Camera.main.pixelRect.size;

            // Round scales to the nearest .5x.
            dpiScale = Mathf.Round(2 * Screen.dpi * invStandardDpi) * .5f;
            unitsPerPixel = screenSize.InvScaledBy(cameraPixels);
            unitsPerDpiPixel = unitsPerPixel * dpiScale;

            staticsInitialized = true;
        }

        void Start() {
            InitializeStatics();
            GenerateLayoutRecursive();
        }

        protected delegate float Vector2Axis(Vector2 vec);

        protected static float VectorGetX(Vector2 vec) {
            return vec.x;
        }

        protected static float VectorGetY(Vector2 vec) {
            return vec.y;
        }

        /// Returns the size in screen units for a scale mode and a value.
        protected float GetUnitsForScale(
            ScaleMode mode,
            float value,
            LayoutBase reference,
            Vector2Axis axis
        )
        {
            switch (mode) {
            case ScaleMode.UnadjustedUnits:
                return value * axis(baseScale);
            case ScaleMode.ScaleNormalizedUnits:
                return value;
            case ScaleMode.ScaleNormalizedPercent:
                return value * axis(unscaledSize);
            case ScaleMode.ReferenceScalePercent:
                if (reference == null) {
                    goto case ScaleMode.ScaleNormalizedPercent;
                }
                return value
                    * axis(reference.totalScale)
                    * axis(unscaledSize);
            case ScaleMode.ReferenceSizePercent:
                if (reference == null) {
                    goto case ScaleMode.ScreenPercent;
                }
                return value * axis(reference.scaledSize);
            case ScaleMode.ScreenPercent:
                return value * axis(screenSize);
            case ScaleMode.DpiScaledPixels:
                return value * axis(unitsPerDpiPixel);
            case ScaleMode.Pixels:
                return value * axis(unitsPerPixel);
            default:
                return 0;
            }
        }

        protected float GetUnitsForOffset(
            ScaleMode mode,
            float value,
            LayoutBase reference,
            Vector2Axis axis
        )
        {
            switch (mode) {
            case ScaleMode.ScaleNormalizedPercent:
                return value * axis(scaledSize);
            case ScaleMode.ReferenceScalePercent:
                if (reference == null) {
                    goto case ScaleMode.ScaleNormalizedPercent;
                }
                return value
                    * axis(scaledSize)
                    * axis(reference.totalScale)
                    / axis(totalScale);
            default:
                return GetUnitsForScale(mode, value, reference, axis);
            }
        }

        /// Returns the distance from the anchor point on this object
        /// to the object's center.
        protected static Vector2 GetDistanceToCenter(LayoutBase layout, Anchor anchor) {
            Vector2 extents = (layout?.scaledSize ?? screenSize) / 2;
            switch (anchor) {
            case Anchor.TopLeft:
                return new Vector2(extents.x, -extents.y);
            case Anchor.TopCenter:
                return new Vector2(0, -extents.y);
            case Anchor.TopRight:
                return new Vector2(-extents.x, -extents.y);
            case Anchor.MiddleLeft:
                return new Vector2(extents.x, 0);
            case Anchor.Center:
                return Vector2.zero;
            case Anchor.MiddleRight:
                return new Vector2(-extents.x, 0);
            case Anchor.BottomLeft:
                return new Vector2(extents.x, extents.y);
            case Anchor.BottomCenter:
                return new Vector2(0, extents.y);
            case Anchor.BottomRight:
                return new Vector2(-extents.x, extents.y);
            default:
                return Vector2.zero;
            }
        }

        protected static Vector2 GetAnchorPosition(LayoutBase layout, Anchor anchor) {
            var position = ((Vector2?)layout?.transform.position) ?? Vector2.zero;
            return position - GetDistanceToCenter(layout, anchor);
        }

        /// Should _not_ be included in `totalScale`. This way each component
        /// can choose aspect scaling regardless of its reference layout.
        protected Vector2 GetAspectScale() {
            return Vector2.one;
        }

        private void GenerateLayoutRecursive(LayoutBase cycle = null) {
            if (isCurrent) {
                return;
            }
            if (!thisInitialized) {
                Initialize();
                thisInitialized = true;
            }
            if (cycle == null) {
                cycle = this;
            } else if (object.ReferenceEquals(cycle, this)) {
                throw new InvalidOperationException(
                    "Infinite recursion while calculating layout"
                );
            }
            var refLayouts = RefLayouts;
            if (refLayouts != null) {
                foreach (var r in refLayouts) {
                    r?.GenerateLayoutRecursive(cycle);
                }
            }
            GenerateLayout();
            isCurrent = true;
        }

        void IRefreshComponent.RefreshComponent(string action) {
            if (action == "refresh") {
                staticsInitialized = false;
                InitializeStatics();
                var all = FindObjectsOfType<LayoutBase>();
                foreach (var layout in all) {
                    layout.isCurrent = false;
                    layout.thisInitialized = false;
                }
                foreach (var layout in all) {
                    layout.GenerateLayoutRecursive();
                }
            } else if (action == "debug") {
                const string fmt = "0.####";
@"baseScale: {0}
unscaledSize: {1}
totalScale: {2}
scaledSize: {3}
lossy: {4}
local: {5}
position: {6}
screenUnits: {7}
screenPixels: {8}
unitsPerPixel: {9}
unitsPerDpiPixel: {10}"
                .LogF(
                    baseScale.ToString(fmt),
                    unscaledSize.ToString(fmt),
                    totalScale.ToString(fmt),
                    scaledSize.ToString(fmt),
                    transform.lossyScale.ToString(fmt),
                    transform.localScale.ToString(fmt),
                    transform.position.ToString(fmt),
                    screenSize.ToString(fmt),
                    cameraPixels.ToString(fmt),
                    unitsPerPixel.ToString(fmt),
                    unitsPerDpiPixel.ToString(fmt)
                );
            } else {
                this.RefreshComponent(action);
            }
        }

        protected virtual void Initialize() {}
        protected virtual void RefreshComponent(string action) {}
        protected abstract IEnumerable<LayoutBase> RefLayouts { get; }
        protected abstract void GenerateLayout();
    }
}