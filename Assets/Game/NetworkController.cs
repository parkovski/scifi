// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;

using SciFi.Scenes;

namespace SciFi.Network {
    public class NetworkController : NetworkLobbyManager {
        public override void OnStartServer() {
            base.OnStartServer();
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerName, SetPlayerName);
        }

        public override void OnClientConnect(NetworkConnection conn) {
            base.OnClientConnect(conn);

            var writer = new NetworkWriter();
            writer.StartMessage(NetworkMessages.SetPlayerName);
            writer.Write(TransitionParams.playerName);
            writer.FinishMessage();
            conn.SendWriter(writer, 0);
        }

        void SetPlayerName(NetworkMessage msg) {
            TransitionParams.AddPlayer(msg.conn, msg.reader.ReadString());
        }

        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
            string playerName;
            if ((playerName = TransitionParams.GetPlayerName(conn)) == null) {
                playerName = "Newton";
            }
            var prefab = spawnPrefabs.Find(p => p.name == playerName);
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            GameController.Instance.RegisterNewPlayer(obj);
            return obj;
        }

        public override void OnLobbyServerSceneChanged(string sceneName) {
            if (sceneName == "MainGame") {
                // Temporary hack until you can add computer players to a single player game
                if (TransitionParams.gameType == GameType.Single) {
                    var newtonPrefab = spawnPrefabs.Find(p => p.name == "Newton");
                    var obj = Instantiate(newtonPrefab, Vector3.zero, Quaternion.identity);
                    obj.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
                    GameController.Instance.RegisterNewPlayer(obj);
                    NetworkServer.Spawn(obj);
                }
                GameController.Instance.StartGame(
                    #if UNITY_EDITOR
                    false
                    #endif
                );
            }
        }

        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
            Destroy(lobbyPlayer);
            return true;
        }
    }
}