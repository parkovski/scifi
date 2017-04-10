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

            var isFirstRun = PlayerPrefs.HasKey("has opened player picker");
#if !UNITY_EDITOR
            if (isFirstRun) {
                PlayerPrefs.SetInt("has opened player picker", 1);
            }
#endif
        }

        /// A different player was selected.
        void ObjectSelected(GameObject gameObject) {
            ((Behaviour)selected.GetComponent("Halo")).enabled = false;
            ((Behaviour)gameObject.GetComponent("Halo")).enabled = true;
            if (TransitionParams.team != -1) {
                selected.GetComponent<SpriteOverlay>().SetColorWithAlpha(Color.clear);
                gameObject.GetComponent<SpriteOverlay>().SetColor(selectedColor);
            }
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

        /// Set the team, or if this team is already selected,
        /// unset it.
        void SetTeam(int team) {
            if (TransitionParams.team == team) {
                TransitionParams.team = -1;
                selected.GetComponent<SpriteOverlay>().SetColorWithAlpha(Color.clear);
                return;
            }

            TransitionParams.team = team;
            switch (team) {
            case 0:
                selectedColor = Player.blueTeamColor;
                break;
            case 1:
                selectedColor = Player.redTeamColor;
                break;
            case 2:
                selectedColor = Player.greenTeamColor;
                break;
            case 3:
                selectedColor = Player.yellowTeamColor;
                break;
            }
            selected.GetComponent<SpriteOverlay>().SetColor(selectedColor);
        }

        public void P1Clicked() {
            SetTeam(0);
        }

        public void P2Clicked() {
            SetTeam(1);
        }

        public void P3Clicked() {
            SetTeam(2);
        }

        public void P4Clicked() {
            SetTeam(3);
        }
    }
}