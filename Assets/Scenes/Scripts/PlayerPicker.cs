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

        void ObjectSelected(ObjectSelectedEventArgs args) {
            ((Behaviour)selected.GetComponent("Halo")).enabled = false;
            ((Behaviour)args.gameObject.GetComponent("Halo")).enabled = true;
            selected = args.gameObject;
        }

        void GoClicked() {
            TransitionParams.playerName = selected.name;

            SceneManager.LoadScene("Lobby");
        }
    }
}