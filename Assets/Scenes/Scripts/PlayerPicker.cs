using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using SciFi.UI;
using SciFi.Players;

namespace SciFi.Scenes {
    /// Choose your character!
    public class PlayerPicker : MonoBehaviour {
        InputManager inputManager;

        /// Currently selected player (default is Newton).
        GameObject selected;
        Color selectedColor = Color.clear;

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
            selected.GetComponent<SpriteOverlay>().SetColor(Color.clear);
            gameObject.GetComponent<SpriteOverlay>().SetColor(selectedColor);
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

        public void P1Clicked() {
            selected.GetComponent<SpriteOverlay>().SetColor(selectedColor = Player.blueTeamColor);
            TransitionParams.team = 0;
        }

        public void P2Clicked() {
            selected.GetComponent<SpriteOverlay>().SetColor(selectedColor = Player.redTeamColor);
            TransitionParams.team = 1;
        }

        public void P3Clicked() {
            selected.GetComponent<SpriteOverlay>().SetColor(selectedColor = Player.greenTeamColor);
            TransitionParams.team = 2;
        }

        public void P4Clicked() {
            selected.GetComponent<SpriteOverlay>().SetColor(selectedColor = Player.yellowTeamColor);
            TransitionParams.team = 3;
        }
    }
}