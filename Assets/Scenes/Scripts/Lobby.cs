using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SciFi.Scenes {
    /// <brief>Multiplayer lobby.</brief>
    /// Handles host/join controls and
    /// setting a nickname.
    public class Lobby : MonoBehaviour {
        public NetworkLobbyManager lobbyManager;
        public InputField hostName;
        public InputField nickname;

        void Start() {
            nickname.onValueChanged.AddListener(n => {
                if (string.IsNullOrEmpty(n)) {
                    TransitionParams.displayName = null;
                } else {
                    TransitionParams.displayName = n;
                }
            });
#if UNITY_EDITOR
            hostName.text = "localhost";
#endif
        }

        /// Set this client as the game host.
        public void Host() {
            lobbyManager.StartHost();
        }

        /// Try to join another game.
        public void Join() {
            lobbyManager.networkAddress = hostName.text;
            lobbyManager.StartClient();
        }

        /// Go back to the player picker.
        public void Back() {
            lobbyManager.StopClient();
            SceneManager.LoadScene("PlayerPicker");
        }
    }
}