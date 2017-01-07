using UnityEngine.Networking;
using System.Collections.Generic;

namespace SciFi.Scenes {
    public enum GameType {
        Single,
        Multi,
    }

    public static class TransitionParams {
        #region Title screen -> Player picker
        public static GameType gameType = GameType.Single;
        #endregion

        #region Player picker -> Lobby
        public static string playerName = "Newton";
        private static Dictionary<NetworkConnection, string> players;
        private static object playersLock;
        #endregion

        static TransitionParams() {
            players = new Dictionary<NetworkConnection, string>();
            playersLock = new object();
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
    }
}