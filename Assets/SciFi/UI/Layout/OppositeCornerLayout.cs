using System.Collections.Generic;
using UnityEngine;
using SciFi.Util.Extensions;

namespace SciFi.UI.Layout {
    [ExecuteInEditMode]
    public class OppositeCornerLayout : LayoutBase {
        public Anchor firstMode;
        public Anchor secondMode;
        public LayoutBase firstAnchor;
        public LayoutBase secondAnchor;
        public ScaleMode lMarginMode;
        public ScaleMode rMarginMode;
        public ScaleMode tMarginMode;
        public ScaleMode bMarginMode;
        public float leftMargin;
        public float rightMargin;
        public float topMargin;
        public float bottomMargin;

        IComponentSizer sizer;

        protected override void Initialize() {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) {
                sizer = new SpriteSizer(this, sr);
            } else {
                sizer = new RectTransformSizer(this, GetComponent<RectTransform>(), Vector2.one);
            }
            _refLayouts = new [] { firstAnchor, secondAnchor };
        }

        LayoutBase[] _refLayouts;
        protected override IEnumerable<LayoutBase> RefLayouts => _refLayouts;

        protected override void GenerateLayout() {
            sizer.ConfigureBaseScale();
            baseScale = transform.lossyScale;
            unscaledSize = sizer.GetUnscaledSize();
            var firstPos = GetAnchorPosition(firstAnchor, firstMode);
            var secondPos = GetAnchorPosition(secondAnchor, secondMode);
            var size = (secondPos - firstPos).Abs();
            var pos = (firstPos + secondPos) * .5f;
            scaledSize = size;
            totalScale = scaledSize.InvScaledBy(unscaledSize);
            var lref = firstPos.x < secondPos.x ? firstAnchor : secondAnchor;
            var rref = firstPos.x > secondPos.x ? firstAnchor : secondAnchor;
            var tref = firstPos.y > secondPos.y ? firstAnchor : secondAnchor;
            var bref = firstPos.y < secondPos.y ? firstAnchor : secondAnchor;
            var lm = GetUnitsForOffset(lMarginMode, leftMargin, lref, VectorGetX);
            var rm = GetUnitsForOffset(rMarginMode, rightMargin, rref, VectorGetX);
            var tm = GetUnitsForOffset(tMarginMode, topMargin, tref, VectorGetY);
            var bm = GetUnitsForOffset(bMarginMode, bottomMargin, bref, VectorGetY);
            Vector2 offset = new Vector2((lm - rm) * .5f, (bm - tm) * .5f);
            size.x -= lm + rm;
            size.y -= tm + bm;
            scaledSize = size;
            totalScale = scaledSize.InvScaledBy(unscaledSize);
            transform.position = (pos + offset).ToVec3(z: 0);
            transform.localPosition = transform.localPosition.ToVec2().ToVec3(z: 0);
            sizer.SetFinalSize();
        }
    }
}