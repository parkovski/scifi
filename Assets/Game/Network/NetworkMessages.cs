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
    }
}