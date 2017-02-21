using UnityEngine;

using SciFi.Util.Extensions;

namespace SciFi.UI {
    public class SpriteOverlay : MonoBehaviour {
        public SpriteRenderer[] spriteRenderers;
        public float alpha = .3f;

        /// Sets the color of the sprite renderers,
        /// using the alpha setting on this component.
        public void SetColor(Color c) {
            foreach (var sr in spriteRenderers) {
                sr.material.color = c.WithAlpha(alpha);
            }
        }

        /// Ignores the alpha setting on the sprite overlay,
        /// using the alpha from <c>c</c>.
        public void SetColorWithAlpha(Color c) {
            foreach (var sr in spriteRenderers) {
                sr.material.color = c;
            }
        }
    }
}