using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    public class PlayerPicker : MonoBehaviour {
        InputManager inputManager;

        GameObject selected;

        public Button goButton;

        void Start() {
            inputManager = GetComponent<InputManager>();
            inputManager.ObjectSelected += ObjectSelected;

            selected = GameObject.Find("Newton");

            goButton.onClick.AddListener(GoClicked);
        }

        void ObjectSelected(GameObject gameObject) {
            ((Behaviour)selected.GetComponent("Halo")).enabled = false;
            ((Behaviour)gameObject.GetComponent("Halo")).enabled = true;
            selected = gameObject;
        }

        void GoClicked() {
            TransitionParams.playerName = selected.name;

            SceneManager.LoadScene("Lobby");
        }
    }
}