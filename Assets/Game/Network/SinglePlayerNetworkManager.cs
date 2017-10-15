using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

using SciFi.Scenes;
using SciFi.Network.Web;

namespace SciFi.Network {
    /// The dummy NetworkManager that handles single player games.
    public class SinglePlayerNetworkManager : NetworkManager {
        public GameObject[] playerPrefabs;
        /// This only applies if the player is not set through the player picker.
        public string humanPlayer;
        public string computerPlayer;
        public int cpuLevel;

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
#pragma warning disable 0219 // Unused variable
            var playerId = GameController.Instance.RegisterNewPlayer(p, "P1", TransitionParams.team, conn);
#pragma warning restore 0219
#if UNITY_EDITOR
            //StartCoroutine(SetLeaderboardIdForFacebookId(playerId));
#endif
            NetworkServer.AddPlayerForConnection(conn, p, playerControllerId);

            p = Instantiate(FindPrefab(computerPlayer), Vector3.zero, Quaternion.identity);
            p.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
            GameController.Instance.RegisterNewComputerPlayer(p, "COM", -1, cpuLevel);
            NetworkServer.Spawn(p);

            GameController.Instance.StartGame();
        }

        IEnumerator SetLeaderboardIdForFacebookId(int playerId) {
            if (FacebookLogin.globalLogin == null) {
                yield return new FacebookLogin(new [] { "public_profile" });
                if (FacebookLogin.globalLogin == null) {
                    yield break;
                }
                if (!string.IsNullOrEmpty(FacebookLogin.globalLogin.loginResult.Error)) {
                    print("facebook login error");
                    yield break;
                }
            }
            var request = Leaderboard.GetPlayerIdForFacebookIdRequest(FacebookLogin.globalLogin.fbid);
            yield return request.Send();
            var leaderboardId = Leaderboard.GetPlayerIdForFacebookIdResult(request);
            if (leaderboardId != -1) {
                print(string.Format("Set player {0} to leaderboard ID {1}", playerId, leaderboardId));
                GameController.Instance.SetLeaderboardId(playerId, leaderboardId);
            }
        }
    }
}