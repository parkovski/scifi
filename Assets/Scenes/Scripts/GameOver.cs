using UnityEngine;

namespace SciFi.Scenes {
    public class GameOver : MonoBehaviour {
        public Sprite winScreen;
        public Sprite loseScreen;

        void Start() {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            if (GameController.Instance.IsWinner()) {
                spriteRenderer.sprite = winScreen;
            } else {
                spriteRenderer.sprite = loseScreen;
            }
        }
    }
}