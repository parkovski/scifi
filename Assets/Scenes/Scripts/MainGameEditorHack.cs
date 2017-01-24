using UnityEngine;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    public class MainGameEditorHack : MonoBehaviour {
        public bool playAudioInEditor;
        const float beat = 0.5357f;

        void Start() {
            var audioSource = GameObject.Find("Audio").GetComponent<AudioSource>();
            audioSource.time = beat * 12;
#if UNITY_EDITOR
            audioSource.enabled = playAudioInEditor;
#endif

            if (FindObjectOfType<GameController>() == null) {
                TransitionParams.gameType = GameType.Single;
                SceneManager.LoadScene("Lobby");
            }
        }
    }
}