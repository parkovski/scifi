using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using SciFi.Network;

namespace SciFi.Scenes {
    /// Displays a win or lose screen depending
    /// on the value set in <see cref="TransitionParams" />.
    public class GameOver : MonoBehaviour {
        public Sprite winScreen;
        public Sprite loseScreen;
        public InputManager inputManager;
        public Text text;

        void Start() {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            if (TransitionParams.isWinner) {
                spriteRenderer.sprite = winScreen;
                text.text = "You win!";
            } else {
                spriteRenderer.sprite = loseScreen;
                text.text = "You lose.";
            }

            inputManager.ObjectSelected += ObjectSelected;
        }

        void ObjectSelected(GameObject obj) {
            if (TransitionParams.gameType == GameType.Single) {
                SceneManager.LoadScene("TitleScreen");
            } else {
                //SceneManager.LoadScene("Lobby");
                if (NetworkServer.active) {
                    NetworkController.Instance.ServerReturnToLobby();
                }
            }
        }
    }
}