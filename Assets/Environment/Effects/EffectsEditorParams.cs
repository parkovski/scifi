using UnityEngine;

namespace SciFi.Environment.Effects {
    public class EffectsEditorParams : MonoBehaviour {
        public static EffectsEditorParams Instance;

        public GameObject star;
        public GameObject explosion;

        void Awake() {
            Instance = this;
        }
    }
}