using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;
using SciFi.Util;

namespace SciFi {
    public class ServerPlayerData {
        GameObject _playerGo;
        public GameObject playerGo {
            set {
                _playerGo = value;
                if (value == null) {
                    player = null;
                } else {
                    player = value.GetComponent<Player>();
                }
            }
            get {
                return _playerGo;
            }
        }
        public Player player { get; private set; }
        public ManualCacheSampler<Vector2> positionSampler;
        public string displayName;
        public int team;
        public NetworkConnection clientConnection;
        public int aiLevel;
        public int leaderboardPlayerId = -1;
    }
}