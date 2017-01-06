using UnityEngine;

using SciFi.Scenes;
using SciFi.Network;

namespace SciFi.Util {
    /// The GameController and NetworkManager are created
    /// in the multiplayer lobby, which never gets loaded
    /// in single player mode, so this class creates the
    /// GameController and creates player instances like
    /// the lobby would do.
    public class SinglePlayerHack : MonoBehaviour {
        void Start() {
            if (TransitionParams.gameType != GameType.Single) {
                return;
            }

            var nc = GetComponent<NetworkController>();
            nc.minPlayers = 1;

            nc.StartHost();
            nc.TryToAddPlayer();
            nc.lobbySlots[0].SendReadyToBeginMessage();
        }
    }
}