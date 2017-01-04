using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace SciFi.Scenes {
    public class PlayerPicker : MonoBehaviour {
        InputManager inputManager;

        Dictionary<string, GameObject> characters;
        GameObject selected;

        public Button goButton;

        static readonly string[] characterNames = new string[] {
            "Newton",
            "Kelvin",
        };

        void Start() {
            inputManager = GetComponent<InputManager>();
            inputManager.ObjectSelected += ObjectSelected;

            characters = new Dictionary<string, GameObject>();
            foreach (var n in characterNames) {
                characters.Add(n, GameObject.Find(n));
            }

            selected = characters["Newton"];

            goButton.onClick.AddListener(GoClicked);
        }

        void ObjectSelected(ObjectSelectedEventArgs args) {
            ((Behaviour)selected.GetComponent("Halo")).enabled = false;
            ((Behaviour)args.gameObject.GetComponent("Halo")).enabled = true;
            selected = args.gameObject;
        }

        void GoClicked() {
            TransitionParams.playerName = selected.name;

            if (TransitionParams.gameType == GameType.Single) {
                SceneManager.LoadScene("MainGame");
            } else {
                SceneManager.LoadScene("Lobby");
            }
        }
    }
}