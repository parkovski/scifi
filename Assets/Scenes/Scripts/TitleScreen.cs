using UnityEngine;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    /// Title screen - pick single or multiplayer.
    public class TitleScreen : MonoBehaviour {
        /// Start a single player game and go to player picker.
        public void SinglePlayer() {
            TransitionParams.gameType = GameType.Single;
            SceneManager.LoadScene("PlayerPicker");
        }

        /// Start a multiplayer game and go to player picker.
        public void MultiPlayer() {
            TransitionParams.gameType = GameType.Multi;
            SceneManager.LoadScene("PlayerPicker");
        }

        /// Open the directory, where you can find the stage maker,
        /// credits, etc.
        public void Directory() {
            SceneManager.LoadScene("Directory");
        }
    }
}