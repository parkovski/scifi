using UnityEngine;

namespace SciFi.Players.Attacks {
    public class BoneArm : MonoBehaviour {
        SpriteRenderer[] spriteRenderers;

        void Start() {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            Hide();
        }

        public void HandMaybeDetach() {
            if (Random.Range(0, 15) == 7) {
                spriteRenderers[spriteRenderers.Length - 1].enabled = false;
            }
        }

        public void Show() {
            ShowHide(true);
        }

        public void Hide() {
            ShowHide(false);
        }

        void ShowHide(bool show) {
            foreach (var sr in spriteRenderers) {
                sr.enabled = show;
            }
        }
    }
}