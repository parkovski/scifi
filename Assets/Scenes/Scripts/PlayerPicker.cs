using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    /// Choose your character!
    public class PlayerPicker : MonoBehaviour {
        InputManager inputManager;

        /// Currently selected player (default is Newton).
        GameObject selected;

        public Button goButton;
        public Button backButton;

        void Start() {
            inputManager = GetComponent<InputManager>();
            inputManager.ObjectSelected += ObjectSelected;

            selected = GameObject.Find("Newton");

            goButton.onClick.AddListener(GoClicked);
            backButton.onClick.AddListener(BackClicked);
        }

        /// A different player was selected.
        void ObjectSelected(GameObject gameObject) {
            ((Behaviour)selected.GetComponent("Halo")).enabled = false;
            ((Behaviour)gameObject.GetComponent("Halo")).enabled = true;
            selected = gameObject;
        }

        /// Set player and transition to the lobby.
        void GoClicked() {
            TransitionParams.playerName = selected.name;

            if (TransitionParams.gameType == GameType.Single) {
                SceneManager.LoadScene("MainGame");
            } else {
                SceneManager.LoadScene("Lobby");
            }
        }

        void BackClicked() {
            SceneManager.LoadScene("TitleScreen");
        }
    }
}