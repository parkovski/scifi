using UnityEngine;
using System.Collections;

using SciFi.Scenes;
using SciFi.Network;

namespace SciFi.Util {
    /// Hack to start a single player game through the multiplayer lobby.
    /// Sets minPlayers to 1 and adds a player automatically.
    public class SinglePlayerHack : MonoBehaviour {
        void Start() {
            var networkController = GetComponent<NetworkController>();
            // The game defaults to multiplayer if the title screen
            // is skipped, so for testing let it run with just one
            // player.
#if UNITY_EDITOR
            GetComponent<NetworkController>().minPlayers = 1;
#endif

            if (TransitionParams.gameType != GameType.Single) {
                return;
            }

            networkController.StartHost();

            StartCoroutine(AddPlayer());
        }

        IEnumerator AddPlayer() {
            yield return new WaitForEndOfFrame();
            var networkController = GetComponent<NetworkController>();
            networkController.TryToAddPlayer();
            networkController.lobbySlots[0].SendReadyToBeginMessage();
        }
    }
}