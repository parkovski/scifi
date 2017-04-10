using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Diagnostics;

using SciFi.Util;

namespace SciFi.Scenes {
    /// <brief>Multiplayer lobby.</brief>
    /// Handles host/join controls and
    /// setting a nickname.
    public class Lobby : MonoBehaviour {
        public NetworkLobbyManager lobbyManager;
        public InputField hostName;
        public InputField nickname;

        void Start() {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) {
                StartHeadlessServer();
                return;
            }

            nickname.onValueChanged.AddListener(n => {
                if (string.IsNullOrEmpty(n)) {
                    TransitionParams.displayName = null;
                } else {
                    TransitionParams.displayName = n;
                }
            });
            hostName.text = Config.GetKey("multiplayer server");
        }

        [Conditional("UNITY_STANDALONE_LINUX")]
        void StartHeadlessServer() {
            lobbyManager.StartServer();
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