// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace SciFi.Network {
    public class NetworkController : NetworkManager {
        public Vector3 spawnPosition;

        //Called on client when connect
        public override void OnClientConnect(NetworkConnection conn) {
            // Create message to set the player
            var msg = new StringMessage(TransitionParams.playerName);

            // Call Add player and pass the message
            ClientScene.AddPlayer(conn, 0, msg);
        }

        // Server
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {
            // Read client message and receive index
            var stream = extraMessageReader.ReadMessage<StringMessage>();
            var playerName = stream.value;

            GameController.Instance.RegisterNewPlayer(conn, playerControllerId, playerName);
        }
    }
}