using System.Collections.Generic;
using UnityEngine;
using SciFi.Util.Extensions;

namespace SciFi.UI.Layout {
    [ExecuteInEditMode]
    public class RectTransformLayout : SpriteLayout {
        public Vector2 rtBaseSize = new Vector2(100, 100);

#if UNITY_EDITOR
        public RefreshButton resyncRtSize = new RefreshButton("sync-rect-size");
#endif

        protected override IComponentSizer CreateSizer() {
            return new RectTransformSizer(this, GetComponent<RectTransform>(), rtBaseSize);
        }

        protected override void RefreshComponent(string action) {
            if (action == "sync-rect-size") {
                rtBaseSize = GetComponent<RectTransform>().rect.size;
            }
        }
    }
}