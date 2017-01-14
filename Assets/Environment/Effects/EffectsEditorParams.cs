using UnityEngine;

namespace SciFi.Environment {
    public class EffectsEditorParams : MonoBehaviour {
        public static EffectsEditorParams Instance;

        public GameObject star;

        void Awake() {
            Instance = this;
        }
    }
}