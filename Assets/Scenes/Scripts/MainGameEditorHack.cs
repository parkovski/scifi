using UnityEngine;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    /// Disable audio and load the lobby scene.
    /// For editor testing when the main game scene
    /// is started.
    public class MainGameEditorHack : MonoBehaviour {
        public bool playMusicInEditor;

        void Start() {
#if UNITY_EDITOR
            GameObject.Find("Music").GetComponent<AudioSource>().enabled = playMusicInEditor;
#endif

            if (FindObjectOfType<GameController>() == null) {
                TransitionParams.gameType = GameType.Single;
                SceneManager.LoadScene("Lobby");
            }
        }
    }
}