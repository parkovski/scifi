// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SciFi.Network {
    public class NetworkController : NetworkLobbyManager {
        Dictionary<NetworkConnection, GameObject> players;

        public NetworkController() {
            players = new Dictionary<NetworkConnection, GameObject>();
        }

        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
            var playerName = "Kelvin";
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