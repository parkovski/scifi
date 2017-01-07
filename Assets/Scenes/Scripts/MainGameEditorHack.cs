using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Scenes {
    public class MainGameEditorHack : MonoBehaviour {
        #if UNITY_EDITOR
        void Start() {
            GameObject.Find("Audio").GetComponent<AudioSource>().enabled = false;

            var gameController = FindObjectOfType<GameController>();
            if (gameController == null) {
                var go = new GameObject("GameController");
                go.AddComponent<NetworkIdentity>();
                var gc = go.AddComponent<GameController>();
                go.AddComponent<InputManager>();

                gc.StartGame();
            }
        }
        #endif
    }
}