using UnityEngine.Networking;
using System.Collections.Generic;

namespace SciFi.Scenes {
    public enum GameType {
        Single,
        Multi,
    }

    public static class TransitionParams {
        #region Title screen -> Player picker
        public static GameType gameType = GameType.Multi;
        #endregion

        #region Player picker -> Lobby
        public static string playerName = "Newton";
        public static string displayName = null;

        private static Dictionary<NetworkConnection, string> players;
        private static Dictionary<NetworkConnection, string> displayNames;
        private static object playersLock;
        private static object displayNamesLock;
        #endregion

        #region Accessors for private fields
        static TransitionParams() {
            players = new Dictionary<NetworkConnection, string>();
            playersLock = new object();
            displayNames = new Dictionary<NetworkConnection, string>();
            displayNamesLock = new object();
        }

        public static void AddPlayer(NetworkConnection conn, string name) {
            lock(playersLock) {
                players.Add(conn, name);
            }
        }

        public static string GetPlayerName(NetworkConnection conn) {
            lock(playersLock) {
                string name = null;
                players.TryGetValue(conn, out name);
                return name;
            }
        }

        public static void AddDisplayName(NetworkConnection conn, string name) {
            lock(displayNamesLock) {
                displayNames.Add(conn, name);
            }
        }

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