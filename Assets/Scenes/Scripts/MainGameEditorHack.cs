using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Scenes {
    public class MainGameEditorHack : MonoBehaviour {
        public GameObject newtonPrefab;
        public GameObject kelvinPrefab;

        #if UNITY_EDITOR
        void Start() {
            GameObject.Find("Audio").GetComponent<AudioSource>().enabled = false;

            var gameController = FindObjectOfType<GameController>();
            if (gameController == null) {
                var go = new GameObject("GameController");
                go.AddComponent<NetworkIdentity>();
                gameController = go.AddComponent<GameController>();
                go.AddComponent<InputManager>();
                gameController.StartGame(false);
            }
        }
        #endif
    }
}