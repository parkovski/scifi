using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    /// The game requires a NetworkManager to be present, but
    /// the multiplayer one runs the lobby too, so when that
    /// one isn't present we need to add a dummy one.
    public class SinglePlayerNetworkManagerHack : MonoBehaviour {
        public NetworkConnection clientConnection;
        public GameObject[] playerPrefabs;
        public string humanPlayer;
        public string computerPlayer;
        [Range(0, 2)]
        public int cpuLevel;

        void Start() {
            if (FindObjectOfType<NetworkManager>() == null) {
                var nm = gameObject.AddComponent<SinglePlayerNetworkManager>();
                nm.autoCreatePlayer = false;
                nm.playerPrefabs = playerPrefabs;
                nm.humanPlayer = humanPlayer;
                nm.computerPlayer = computerPlayer;
                nm.cpuLevel = cpuLevel;
                clientConnection = nm.StartHost().connection;
            }
        }
    }
}