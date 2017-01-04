using UnityEngine;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    public class TitleScreen : MonoBehaviour {
        public void SinglePlayer() {
            TransitionParams.gameType = GameType.Single;
            SceneManager.LoadScene("PlayerPicker");
        }

        public void MultiPlayer() {
            TransitionParams.gameType = GameType.Multi;
            SceneManager.LoadScene("PlayerPicker");
        }
    }
}