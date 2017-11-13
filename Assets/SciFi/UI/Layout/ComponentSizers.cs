using UnityEngine;
using SciFi.Util.Extensions;

namespace SciFi.UI.Layout {
    public interface IComponentSizer {
        void ConfigureBaseScale();
        Vector2 GetUnscaledSize();
        void SetFinalSize();
    }

    public class SpriteSizer : IComponentSizer {
        LayoutBase obj;
        SpriteRenderer spriteRenderer;

        public SpriteSizer(LayoutBase obj, SpriteRenderer spriteRenderer) {
            this.obj = obj;
            this.spriteRenderer = spriteRenderer;
        }

        public void ConfigureBaseScale() {
            obj.transform.localScale = Vector3.one;
        }

        public Vector2 GetUnscaledSize() {
            return spriteRenderer.bounds.size.ToVec2().InvScaledBy(obj.baseScale);
        }

        public void SetFinalSize() {
            obj.transform.localScale = obj.totalScale.InvScaledBy(obj.baseScale).ToVec3(z: 1);
        }
    }

    public class RectTransformSizer : IComponentSizer {
        LayoutBase obj;
        RectTransform rectTransform;
        Vector2 baseSize;

        public RectTransformSizer(LayoutBase obj, RectTransform rectTransform, Vector2 baseSize) {
            this.obj = obj;
            this.rectTransform = rectTransform;
            this.baseSize = baseSize;
        }

        public void ConfigureBaseScale() {}

        public Vector2 GetUnscaledSize() {
            return baseSize.ScaledBy(obj.baseScale);
        }

        public void SetFinalSize() {
            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                obj.scaledSize.x * rectTransform.localScale.x / rectTransform.lossyScale.x
            );
            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                obj.scaledSize.y * rectTransform.localScale.y / rectTransform.lossyScale.y
            );
        }
    }
}