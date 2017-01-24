using UnityEngine;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    public class MainGameEditorHack : MonoBehaviour {
        public bool playAudioInEditor;

#if UNITY_EDITOR
        void Start() {
            GameObject.Find("Audio").GetComponent<AudioSource>().enabled = playAudioInEditor;

            if (FindObjectOfType<GameController>() == null) {
                TransitionParams.gameType = GameType.Single;
                SceneManager.LoadScene("Lobby");
            }
        }
#endif
    }
}