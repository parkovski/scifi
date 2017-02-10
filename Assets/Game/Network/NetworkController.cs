// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

using SciFi.Scenes;
using SciFi.UI;
using SciFi.Players;

namespace SciFi.Network {
    public struct ConnectionClockOffset {
        public float clockOffset;
        public int pings;
    }

    /// Handle the multiplayer lobby.
    public class NetworkController : NetworkLobbyManager {
        List<GameObject> playersToRegister;
        List<string> displayNames;

        /// Unity's network documentation is shit and I can't figure out
        /// how to get this from within GameController.
        public static NetworkConnection clientConnectionToServer;

        /// On the client, Time.realtimeSinceStartup - serverClockOffset == server's Time.realtimeSinceStartup.
        public static ConnectionClockOffset serverClock = new ConnectionClockOffset();
        Dictionary<NetworkConnection, ConnectionClockOffset> clientClocks;

        /// Set up message handlers.
        public override void OnStartServer() {
            base.OnStartServer();
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerName, SetPlayerName);
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerDisplayName, SetPlayerDisplayName);
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerTeam, SetPlayerTeam);
            NetworkServer.RegisterHandler(NetworkMessages.SyncClock, ServerSyncClock);

            playersToRegister = new List<GameObject>();
            displayNames = new List<string>();
            clientClocks = new Dictionary<NetworkConnection, ConnectionClockOffset>();
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

            if (TransitionParams.team != -1) {
                writer.StartMessage(NetworkMessages.SetPlayerTeam);
                writer.Write(TransitionParams.team);
                writer.FinishMessage();
                conn.SendWriter(writer, 0);
            }

            this.client.connection.RegisterHandler(NetworkMessages.SyncClock, ClientSyncClock);
            StartCoroutine(SyncClockCoroutine(conn));
        }

        IEnumerator SyncClockCoroutine(NetworkConnection conn) {
            var writer = new NetworkWriter();
            for (int i = 0; i < 5; i++) {
                writer.StartMessage(NetworkMessages.SyncClock);
                writer.Write(Time.realtimeSinceStartup);
                writer.FinishMessage();
                conn.SendWriter(writer, 1);
                yield return new WaitForSeconds(.5f);
            }
        }

        public Nullable<float> GetClientClockOffset(NetworkConnection conn) {
            ConnectionClockOffset offset;
            if (clientClocks.TryGetValue(conn, out offset)) {
                return offset.clockOffset;
            }
            return null;
        }

        /// Records the average offset between the client/server clocks.
        /// The first time a client sends this message, it starts sending them back too.
        void ServerSyncClock(NetworkMessage msg) {
            float timeOffset = Time.realtimeSinceStartup - msg.reader.ReadSingle();
            ConnectionClockOffset clientClock;
            if (!clientClocks.TryGetValue(msg.conn, out clientClock)) {
                // On the first message, also start syncing the clock to the client.
                StartCoroutine(SyncClockCoroutine(msg.conn));
            }
            clientClock.clockOffset = (clientClock.clockOffset * clientClock.pings + timeOffset) / (clientClock.pings + 1);
            ++clientClock.pings;
            clientClocks[msg.conn] = clientClock;
        }

        /// Records the average offset between the client/server clocks.
        void ClientSyncClock(NetworkMessage msg) {
            float timeOffset = Time.realtimeSinceStartup - msg.reader.ReadSingle();
            serverClock.clockOffset = (serverClock.clockOffset * serverClock.pings + timeOffset) / (serverClock.pings + 1);
            ++serverClock.pings;
        }

        /// Receive a player selection message from the client.
        void SetPlayerName(NetworkMessage msg) {
            TransitionParams.AddPlayer(msg.conn, msg.reader.ReadString());
        }

        /// Receive a display name set message from the client.
        void SetPlayerDisplayName(NetworkMessage msg) {
            TransitionParams.AddDisplayName(msg.conn, msg.reader.ReadString());
        }

        void SetPlayerTeam(NetworkMessage msg) {
            TransitionParams.AddTeam(msg.conn, msg.reader.ReadInt32());
        }

        /// Create the player for <c>conn</c> and register it with <see cref="GameController" />.
        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
            string playerName;
            if ((playerName = TransitionParams.GetPlayerName(conn)) == null) {
                playerName = "Newton";
            }
            var prefab = spawnPrefabs.Find(p => p.name == playerName);
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            int team = TransitionParams.GetTeam(conn);
            if (team != -1) {
                obj.GetComponent<SpriteOverlay>().SetColor(TeamToColor(team));
            }
            playersToRegister.Add(obj);
            displayNames.Add(TransitionParams.GetDisplayName(conn));
            return obj;
        }

        public static Color TeamToColor(int team) {
            switch (team) {
            case 0:
                return Player.blueTeamColor;
            case 1:
                return Player.redTeamColor;
            case 2:
                return Player.greenTeamColor;
            case 3:
                return Player.yellowTeamColor;
            default:
                return Color.clear;
            }
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