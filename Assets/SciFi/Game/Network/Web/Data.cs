using System;

namespace SciFi.Network.Web {
    /// Matches the JSON returned by /players/:id/stats/:type
    [Serializable]
    public struct PlayerStats {
        public int id;
        public string name;
        public int matches;
        public int wins;
        public int kills;
        public int deaths;
    }

    [Serializable]
    public struct PlayerMatchInfo {
        public int id;
        public int kills;
        public int deaths;
    }

    /// Parameters for /match/new
    [Serializable]
    public struct MatchResult {
        public PlayerMatchInfo[] players;
        public int winner;
    }
}