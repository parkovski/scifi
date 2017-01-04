// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;

using SciFi.Scenes;

namespace SciFi.Network {
    public class NetworkController : NetworkLobbyManager {
        Dictionary<NetworkConnection, string> players;

        public NetworkController() {
            players = new Dictionary<NetworkConnection, string>();
        }

        //Called on client when connect
        public override void OnClientConnect(NetworkConnection conn) {
            // Create message to set the player
            var msg = new StringMessage(TransitionParams.playerName);

            // Call Add player and pass the message
            ClientScene.AddPlayer(conn, 0, msg);
        }

        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
            return spawnPrefabs.Find(p => p.name == TransitionParams.playerName);
        }

        public override void OnLobbyServerSceneChanged(string sceneName) {
            if (sceneName == "MainGame") {
                foreach (var p in players) {
                    GameController.Instance.RegisterNewPlayer(p.Key, 0, p.Value);
                }

                GameController.Instance.StartGame();
            }
        }

        // Server
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {
            // Read client message and receive index
            /*
            var stream = extraMessageReader.ReadMessage<StringMessage>();
            var playerName = stream.value;
            */
            var playerName = "Newton";

            //GameController.Instance.RegisterNewPlayer(conn, playerControllerId, playerName);
            players.Add(conn, playerName);
        }
    }
}