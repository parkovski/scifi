using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

using SciFi.Scenes;
using SciFi.UI;

namespace SciFi.Network {
    /// The dummy NetworkManager that handles single player games.
    public class SinglePlayerNetworkManager : NetworkManager {
        public GameObject[] playerPrefabs;
        /// This only applies if the player is not set through the player picker.
        public string humanPlayer;
        public string computerPlayer;

        public override void OnStartServer() {
        }

        public override void OnClientConnect(NetworkConnection conn) {
            // Sending a message makes it call OnServerAddPlayer instead of
            // trying to do it itself.
            var msg = new IntegerMessage(0);
            ClientScene.AddPlayer(conn, 0, msg);
        }

        GameObject FindPrefab(string name) {
            for (int i = 0; i < playerPrefabs.Length; i++) {
                if (playerPrefabs[i].name == name) {
                    return playerPrefabs[i];
                }
            }

            return playerPrefabs[0];
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {
            if (TransitionParams.playerName != null) {
                humanPlayer = TransitionParams.playerName;
            }

            var p = Instantiate(FindPrefab(humanPlayer), Vector3.zero, Quaternion.identity);
            GameController.Instance.RegisterNewPlayer(p, "P1", TransitionParams.team, conn);
            NetworkServer.AddPlayerForConnection(conn, p, playerControllerId);

            p = Instantiate(FindPrefab(computerPlayer), Vector3.zero, Quaternion.identity);
            p.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
            GameController.Instance.RegisterNewPlayer(p, "COM", -1, null);
            NetworkServer.Spawn(p);

            GameController.Instance.StartGame(
#if UNITY_EDITOR
                false
#endif
            );
        }
    }
}