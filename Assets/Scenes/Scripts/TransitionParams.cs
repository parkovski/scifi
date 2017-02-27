using UnityEngine.Networking;
using System.Collections.Generic;

namespace SciFi.Scenes {
    /// Single or multi player?
    public enum GameType {
        /// Single player
        Single,
        /// Multiplayer
        Multi,
    }

    /// Parameters to be passed between scenes.
    public static class TransitionParams {
        #region Title screen -> Player picker
        public static GameType gameType = GameType.Single;
        #endregion

        #region Player picker -> Lobby
        /// The prefab name of this client's player.
        public static string playerName = null;
        /// The custom display name for this client's player, if any.
        public static string displayName = null;
        /// Currently just sets the player's color.
        public static int team = -1;

        /// The prefab name of each player connected to the server.
        private static Dictionary<NetworkConnection, string> players;
        /// The display name of each client connect to the server, if they set one.
        private static Dictionary<NetworkConnection, string> displayNames;
        /// The teams of each connected player, or -1 for none.
        private static Dictionary<NetworkConnection, int> teams;
        /// Thread-safety
        private static object threadLock;

        #endregion

        #region Main game -> Game over
        public static bool isWinner = false;
        #endregion

        #region Accessors for private fields
        static TransitionParams() {
            players = new Dictionary<NetworkConnection, string>();
            displayNames = new Dictionary<NetworkConnection, string>();
            teams = new Dictionary<NetworkConnection, int>();
            threadLock = new object();
        }

        /// Add a player for <c>conn</c> with prefab <c>name</c>.
        public static void AddPlayer(NetworkConnection conn, string name) {
            lock(threadLock) {
                players[conn] = name;
            }
        }

        /// Get the player prefab name for <c>conn</c>.
        public static string GetPlayerName(NetworkConnection conn) {
            lock(threadLock) {
                string name = null;
                players.TryGetValue(conn, out name);
                return name;
            }
        }

        /// Add a display name (<c>name</c>) for player <c>conn</c>.
        public static void AddDisplayName(NetworkConnection conn, string name) {
            lock(threadLock) {
                displayNames[conn] = name;
            }
        }

        /// Get the display name for <c>conn</c> or null if none was set.
        public static string GetDisplayName(NetworkConnection conn) {
            lock(threadLock) {
                string name = null;
                displayNames.TryGetValue(conn, out name);
                return name;
            }
        }

        public static void AddTeam(NetworkConnection conn, int team) {
            lock(threadLock) {
                teams[conn] = team;
            }
        }

        public static int GetTeam(NetworkConnection conn) {
            lock(threadLock) {
                int team;
                if (teams.TryGetValue(conn, out team)) {
                    return team;
                }
                return -1;
            }
        }
        #endregion
    }
}