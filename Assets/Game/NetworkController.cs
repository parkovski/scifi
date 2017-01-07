// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Scenes;

namespace SciFi.Network {
    public class NetworkController : NetworkLobbyManager {
        Dictionary<NetworkConnection, GameObject> players;

        public NetworkController() {
            players = new Dictionary<NetworkConnection, GameObject>();
        }

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
            players.Add(conn, obj);
            return obj;
        }

        public override void OnLobbyServerSceneChanged(string sceneName) {
            if (sceneName == "MainGame") {
                GameController.Instance.StartGame();
            }
        }
    }
}