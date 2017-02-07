using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    public class SinglePlayerNetworkManagerHack : MonoBehaviour {
        public NetworkConnection clientConnection;
        public GameObject[] playerPrefabs;
        public string humanPlayer;
        public string computerPlayer;

        void Start() {
            if (FindObjectOfType<NetworkManager>() == null) {
                var nm = gameObject.AddComponent<SinglePlayerNetworkManager>();
                nm.autoCreatePlayer = false;
                nm.playerPrefabs = playerPrefabs;
                nm.humanPlayer = humanPlayer;
                nm.computerPlayer = computerPlayer;
                clientConnection = nm.StartHost().connection;
            }
        }
    }
}