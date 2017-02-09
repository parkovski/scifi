using UnityEngine;

namespace SciFi.UI {
    public class SpriteOverlay : MonoBehaviour {
        public SpriteRenderer[] spriteRenderers;

        public void SetColor(Color c) {
            foreach (var sr in spriteRenderers) {
                sr.material.color = c;
            }
        }
    }
}