// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

using SciFi.Scenes;

namespace SciFi.Network {
    /// Handle the multiplayer lobby.
    public class NetworkController : NetworkLobbyManager {
        List<GameObject> playersToRegister;
        List<string> displayNames;

        /// Unity's network documentation is shit and I can't figure out
        /// how to get this from within GameController.
        public static NetworkConnection clientConnectionToServer;

        /// Set up message handlers.
        public override void OnStartServer() {
            base.OnStartServer();
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerName, SetPlayerName);
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerDisplayName, SetPlayerDisplayName);

            playersToRegister = new List<GameObject>();
            displayNames = new List<string>();
        }

        /// Send the server a message indicating which player the client has chosen.
        public override void OnClientConnect(NetworkConnection conn) {
            base.OnClientConnect(conn);

            clientConnectionToServer = conn;

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
            playersToRegister.Add(obj);
            displayNames.Add(TransitionParams.GetDisplayName(conn));
            return obj;
        }

        /// Start the game when the scene changes to MainGame.
        public override void OnLobbyServerSceneChanged(string sceneName) {
            base.OnLobbyServerSceneChanged(sceneName);
            if (sceneName == "MainGame") {
                StartCoroutine(InitializeWhenGameControllerReady());
            }
        }

        IEnumerator InitializeWhenGameControllerReady() {
            yield return new WaitUntil(() => GameController.Instance != null);
            GameController.Instance.SetClientCount(numPlayers);
            yield return new WaitUntil(() => playersToRegister.Count == numPlayers);
            for (int i = 0; i < playersToRegister.Count; i++) {
                var player = playersToRegister[i];
                var displayName = displayNames[i];
                GameController.Instance.RegisterNewPlayer(player, displayName);
            }
        }

        /// Destroy the lobby player.
        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
            base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
            Destroy(lobbyPlayer);
            return true;
        }
    }
}