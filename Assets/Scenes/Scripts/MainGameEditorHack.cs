using UnityEngine;
using UnityEngine.SceneManagement;

namespace SciFi.Scenes {
    public class MainGameEditorHack : MonoBehaviour {
        public GameObject newtonPrefab;
        public GameObject kelvinPrefab;

        #if UNITY_EDITOR
        void Start() {
            GameObject.Find("Audio").GetComponent<AudioSource>().enabled = false;

            if (FindObjectOfType<GameController>() == null) {
                TransitionParams.gameType = GameType.Single;
                SceneManager.LoadScene("Lobby");
            }
        }
        #endif
    }
}