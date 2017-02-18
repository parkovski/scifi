using UnityEngine;

namespace SciFi.Scenes {
    /// Displays a win or lose screen depending
    /// on the value set in <see cref="TransitionParams" />.
    public class GameOver : MonoBehaviour {
        public Sprite winScreen;
        public Sprite loseScreen;

        void Start() {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            if (TransitionParams.isWinner) {
                spriteRenderer.sprite = winScreen;
            } else {
                spriteRenderer.sprite = loseScreen;
            }
        }
    }
}