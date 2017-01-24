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
        public static GameType gameType = GameType.Multi;
        #endregion

        #region Player picker -> Lobby
        /// The prefab name of this client's player.
        public static string playerName = "Newton";
        /// The custom display name for this client's player, if any.
        public static string displayName = null;

        /// The prefab name of each player connected to the server.
        private static Dictionary<NetworkConnection, string> players;
        /// The display name of each client connect to the server, if they set one.
        private static Dictionary<NetworkConnection, string> displayNames;
        /// Thread-safety
        private static object playersLock;
        /// Thread-safety
        private static object displayNamesLock;
        #endregion

        #region Accessors for private fields
        static TransitionParams() {
            players = new Dictionary<NetworkConnection, string>();
            playersLock = new object();
            displayNames = new Dictionary<NetworkConnection, string>();
            displayNamesLock = new object();
        }

        /// Add a player for <c>conn</c> with prefab <c>name</c>.
        public static void AddPlayer(NetworkConnection conn, string name) {
            lock(playersLock) {
                players.Add(conn, name);
            }
        }

        /// Get the player prefab name for <c>conn</c>.
        public static string GetPlayerName(NetworkConnection conn) {
            lock(playersLock) {
                string name = null;
                players.TryGetValue(conn, out name);
                return name;
            }
        }

        /// Add a display name (<c>name</c>) for player <c>conn</c>.
        public static void AddDisplayName(NetworkConnection conn, string name) {
            lock(displayNamesLock) {
                displayNames.Add(conn, name);
            }
        }

        /// Get the display name for <c>conn</c> or null if none was set.
        public static string GetDisplayName(NetworkConnection conn) {
            lock(displayNamesLock) {
                string name = null;
                displayNames.TryGetValue(conn, out name);
                return name;
            }
        }
        #endregion
    }
}