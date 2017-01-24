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

        void Start() {
            inputManager = GetComponent<InputManager>();
            inputManager.ObjectSelected += ObjectSelected;

            selected = GameObject.Find("Newton");

            goButton.onClick.AddListener(GoClicked);
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

            SceneManager.LoadScene("Lobby");
        }
    }
}