// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;

using SciFi.Scenes;

namespace SciFi.Network {
    /// Handle the multiplayer lobby.
    public class NetworkController : NetworkLobbyManager {
        /// Set up message handlers.
        public override void OnStartServer() {
            base.OnStartServer();
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerName, SetPlayerName);
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerDisplayName, SetPlayerDisplayName);
        }

        /// Send the server a message indicating which player the client has chosen.
        public override void OnClientConnect(NetworkConnection conn) {
            base.OnClientConnect(conn);

            // Set the character
            var writer = new NetworkWriter();
            writer.StartMessage(NetworkMessages.SetPlayerName);
            writer.Write(TransitionParams.playerName);
            writer.FinishMessage();
            conn.SendWriter(writer, 0);

            // Set the display name
            if (TransitionParams.displayName != null) {
                writer.StartMessage(NetworkMessages.SetPlayerDisplayName);
                writer.Write(TransitionParams.displayName);
                writer.FinishMessage();
                conn.SendWriter(writer, 0);
            }
        }

        /// Receive a player selection message from the client.
        void SetPlayerName(NetworkMessage msg) {
            TransitionParams.AddPlayer(msg.conn, msg.reader.ReadString());
        }

        /// Receive a display name set message from the client.
        void SetPlayerDisplayName(NetworkMessage msg) {
            TransitionParams.AddDisplayName(msg.conn, msg.reader.ReadString());
        }

        /// Create the player for <c>conn</c> and register it with <see cref="GameController" />.
        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
            string playerName;
            if ((playerName = TransitionParams.GetPlayerName(conn)) == null) {
                playerName = "Newton";
            }
            var prefab = spawnPrefabs.Find(p => p.name == playerName);
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, obj, playerControllerId);
            GameController.Instance.RegisterNewPlayer(obj, TransitionParams.GetDisplayName(conn));
            return obj;
        }

        /// Start the game when the scene changes to MainGame.
        public override void OnLobbyServerSceneChanged(string sceneName) {
            if (sceneName == "MainGame") {
                // Temporary hack until you can add computer players to a single player game
                if (TransitionParams.gameType == GameType.Single) {
                    var newtonPrefab = spawnPrefabs.Find(p => p.name == "Newton");
                    var obj = Instantiate(newtonPrefab, Vector3.zero, Quaternion.identity);
                    obj.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
                    GameController.Instance.RegisterNewPlayer(obj, TransitionParams.displayName);
                }
                GameController.Instance.StartGame(
#if UNITY_EDITOR
                    false
#endif
                );
            }
        }

        /// Destroy the lobby player.
        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
            Destroy(lobbyPlayer);
            return true;
        }
    }
}