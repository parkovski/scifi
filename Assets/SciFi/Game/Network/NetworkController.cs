// http://answers.unity3d.com/questions/1149937/multiple-player-prefabs-in-the-network-manager.html

using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

using SciFi.Scenes;

namespace SciFi.Network {
    public struct ConnectionClockOffset {
        public float clockOffset;
        public int pings;
    }

    /// Handle the multiplayer lobby.
    public class NetworkController : NetworkLobbyManager {
        List<GameObject> playersToRegister;
        List<string> displayNames;
        List<NetworkConnection> clientConnections;
        object threadLock;

        /// Unity's network documentation is shit and I can't figure out
        /// how to get this from within GameController.
        public static NetworkConnection clientConnectionToServer;

        /// On the client, Time.realtimeSinceStartup - serverClockOffset == server's Time.realtimeSinceStartup.
        public static ConnectionClockOffset serverClock = new ConnectionClockOffset();
        Dictionary<NetworkConnection, ConnectionClockOffset> clientClocks;

        public static NetworkController Instance { get { return (NetworkController)singleton; } }

        /// Set up message handlers.
        public override void OnStartServer() {
            base.OnStartServer();
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerName, SetPlayerName);
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerDisplayName, SetPlayerDisplayName);
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerTeam, SetPlayerTeam);
            NetworkServer.RegisterHandler(NetworkMessages.SyncClock, ServerSyncClock);
            NetworkServer.RegisterHandler(NetworkMessages.SetPlayerLeaderboardId, SetPlayerLeaderboardId);

            playersToRegister = new List<GameObject>();
            displayNames = new List<string>();
            clientConnections = new List<NetworkConnection>();
            threadLock = new object();
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

            if (TransitionParams.leaderboardId != -1) {
                writer.StartMessage(NetworkMessages.SetPlayerLeaderboardId);
                writer.Write(TransitionParams.leaderboardId);
                writer.FinishMessage();
                NetworkController.clientConnectionToServer.SendWriter(writer, 0);
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

        public static Nullable<float> GetClientClockOffset(NetworkConnection conn) {
            var instance = (NetworkController)singleton;
            ConnectionClockOffset offset;
            if (instance.clientClocks.TryGetValue(conn, out offset)) {
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

        void SetPlayerLeaderboardId(NetworkMessage msg) {
            TransitionParams.AddLeaderboardId(msg.conn, msg.reader.ReadInt32());
        }

        /// Create the player for <c>conn</c> and register it with <see cref="GameController" />.
        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
            string playerName;
            if ((playerName = TransitionParams.GetPlayerName(conn)) == null) {
                playerName = "Newton";
            }
            var prefab = spawnPrefabs.Find(p => p.name == playerName);
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            lock(threadLock) {
                playersToRegister.Add(obj);
                displayNames.Add(TransitionParams.GetDisplayName(conn));
                clientConnections.Add(conn);
            }
            return obj;
        }

        /// Start the game when the scene changes to MainGame.
        public override void OnLobbyServerSceneChanged(string sceneName) {
            base.OnLobbyServerSceneChanged(sceneName);
            if (sceneName == "MainGame") {
                StartCoroutine(InitializeWhenGameControllerReady());
            } else if (sceneName == "Lobby") {
                playersToRegister.Clear();
                displayNames.Clear();
                clientConnections.Clear();
            }
        }

        IEnumerator InitializeWhenGameControllerReady() {
            yield return new WaitUntil(() => GameController.Instance != null);
            GameController.Instance.SetClientCount(numPlayers);
            yield return new WaitUntil(() => playersToRegister.Count == numPlayers);
            lock(threadLock) {
                for (int i = 0; i < playersToRegister.Count; i++) {
                    var player = playersToRegister[i];
                    var displayName = displayNames[i];
                    var conn = clientConnections[i];
                    var team = TransitionParams.GetTeam(conn);
                    var leaderboardId = TransitionParams.GetLeaderboardId(conn);
                    var playerId = GameController.Instance.RegisterNewPlayer(player, displayName, team, conn);
                    if (leaderboardId != -1) {
                        GameController.Instance.SetLeaderboardId(playerId, leaderboardId);
                    }
                }
            }
        }

        /// Destroy the lobby player.
        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
            base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
            //Destroy(lobbyPlayer);
            return true;
        }
    }
}