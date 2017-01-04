using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SciFi.Scenes {
    public class Lobby : MonoBehaviour {
        public NetworkLobbyManager lobbyManager;
        public InputField hostName;
        public InputField nickname;

        public void Host() {
            lobbyManager.StartHost();
        }

        public void Join() {
            lobbyManager.networkAddress = hostName.text;
            lobbyManager.StartClient();
        }

        public void Back() {
            // TODO: This will not work, because the client is already connected
            // so the new player will never be sent.
            SceneManager.LoadScene("PlayerPicker");
        }
    }
}