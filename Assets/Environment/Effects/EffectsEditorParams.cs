using UnityEngine;
using System.Collections;

namespace SciFi.Environment.Effects {
    /// Parameters for effects set via the Unity editor (e.g. prefabs).
    public class EffectsEditorParams : MonoBehaviour {
        public static EffectsEditorParams Instance;

        public GameObject star;
        public GameObject explosion;
        public GameObject smoke;
        public GameObject fadeOverlay;

        void Awake() {
            Instance = this;
        }

        public static void RunCoroutine(IEnumerator coro) {
            Instance.StartCoroutine(coro);
        }
    }
}