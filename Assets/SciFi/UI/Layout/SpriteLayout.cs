using System;
using System.Collections.Generic;
using UnityEngine;
using SciFi.Util.Extensions;

namespace SciFi.UI.Layout {
    [ExecuteInEditMode]
    public class SpriteLayout : LayoutBase {
        public LayoutBase positionRef = null;
        public LayoutBase sizeRef = null;
        public Anchor anchor = Anchor.Center;
        public bool overlapParent = false;
        public ScaleMode widthMode = ScaleMode.ScaleNormalizedUnits;
        public ScaleMode heightMode = ScaleMode.ScaleNormalizedUnits;
        public ScaleMode offsetMode = ScaleMode.ScaleNormalizedUnits;
        public float width = 1;
        public float height = 1;
        public Vector2 offset = new Vector2(0, 0);
        public ScreenAspectScaleMode screenAspectScaling = ScreenAspectScaleMode.None;
        public Vector2 designAspect = new Vector2(16, 10);

        IComponentSizer sizer;

        protected override void Initialize() {
            _refLayouts = new [] { positionRef, sizeRef };
            sizer = CreateSizer();
        }

        protected virtual IComponentSizer CreateSizer() {
            return new SpriteSizer(this, GetComponent<SpriteRenderer>());
        }

        LayoutBase[] _refLayouts;
        protected override IEnumerable<LayoutBase> RefLayouts => _refLayouts;

        protected override void GenerateLayout() {
            sizer.ConfigureBaseScale();
            baseScale = transform.lossyScale;
            unscaledSize = sizer.GetUnscaledSize();
            scaledSize
              = GetScaledSize(widthMode, heightMode, width, height, sizeRef, sizeRef);
            totalScale = scaledSize.InvScaledBy(unscaledSize);
            // TODO: Aspect scaling?
            if (anchor != Anchor.None) {
                var position = GetAnchorPosition(positionRef, anchor);
                var overlap = GetDistanceToCenter(this, anchor);
                var offsets = GetOffsets();
                if (overlapParent) {
                    position += overlap + offsets;
                } else {
                    position -= overlap - offsets;
                }
                transform.position = position.ToVec3(z: 0);
                transform.localPosition = transform.localPosition.ToVec2().ToVec3(z: 0);
            }
            sizer.SetFinalSize();
        }

        protected Vector2 GetScaledSize(
            ScaleMode widthMode,
            ScaleMode heightMode,
            float width,
            float height,
            LayoutBase widthRef,
            LayoutBase heightRef
        )
        {
            var scaledWidth = GetUnitsForScale(widthMode, width, widthRef, VectorGetX);
            var scaledHeight = GetUnitsForScale(heightMode, height, heightRef, VectorGetY);
            if (heightMode == ScaleMode.MaintainAspect) {
                scaledHeight = scaledWidth * unscaledSize.y / unscaledSize.x;
            } else if (widthMode == ScaleMode.MaintainAspect) {
                scaledWidth = scaledHeight * unscaledSize.x / unscaledSize.y;
            }
            return new Vector2(scaledWidth, scaledHeight);
        }

        protected Vector2 GetOffsets() {
            Vector2 offsets;
            var mode = offsetMode == ScaleMode.MaintainAspect ? ScaleMode.Pixels : offsetMode;
            offsets.x = GetUnitsForOffset(mode, offset.x, positionRef, VectorGetX);
            offsets.y = GetUnitsForOffset(mode, offset.y, positionRef, VectorGetY);
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

        // FIXME - Shrink and grow modes are wrong.
        protected void SetAspectScaling(ref Vector2 scale)
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
    }
}