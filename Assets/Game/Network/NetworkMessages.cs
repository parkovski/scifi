using UnityEngine.Networking;

namespace SciFi.Network {
    /// Custom messages for client/server interaction.
    public static class NetworkMessages {
        /// Name of the character this player will use.
        public static short SetPlayerName = MsgType.Highest + 1;
        /// Display name / nickname
        public static short SetPlayerDisplayName = MsgType.Highest + 2;
        /// Client GameController initialized
        public static short ClientGameReady = MsgType.Highest + 3;
        /// The dummy network manager has connected;
        /// create players for single player mode.
        public static short SpawnSinglePlayer = MsgType.Highest + 4;
        /// Team/color.
        public static short SetPlayerTeam = MsgType.Highest + 5;
        /// Clock synchronization for movements.
        public static short SyncClock = MsgType.Highest + 6;
        /// Position sync for SFNetworkTransform.
        public static short ServerSyncPosition = MsgType.Highest + 7;
        public static short ClientSyncPosition = MsgType.Highest + 8;
    }
}