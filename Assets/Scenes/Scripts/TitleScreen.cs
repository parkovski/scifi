using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace SciFi.Scenes {
    /// Title screen - pick single or multiplayer.
    public class TitleScreen : MonoBehaviour {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        extern static int SciFiGetQuickAction();
        static bool isFirstRun = true;

        /// On iOS, handle the 3D touch quick actions.
        void Start() {
            if (!isFirstRun) {
                return;
            }
            isFirstRun = false;
            /// These numbers are documented in SFQuickActions.m
            int action = SciFiGetQuickAction();
            if (action == 1) {
                SinglePlayer();
            } else if (action == 2) {
                MultiPlayer();
            }
        }
#endif

#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
        /// For Linux standalone/headless, start the server.
        void Start() {
            TransitionParams.gameType = GameType.Multi;
            SceneManager.LoadScene("Lobby");
        }
#endif

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