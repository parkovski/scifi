using UnityEngine;

using SciFi.Util.Extensions;

namespace SciFi.UI {
    public class SpriteOverlay : MonoBehaviour {
        public SpriteRenderer[] spriteRenderers;
        public float alpha = .3f;

        public void SetColor(Color c) {
            foreach (var sr in spriteRenderers) {
                sr.material.color = c.WithAlpha(alpha);
            }
        }

        public void SetColorWithAlpha(Color c) {
            foreach (var sr in spriteRenderers) {
                sr.material.color = c;
            }
        }
    }
}