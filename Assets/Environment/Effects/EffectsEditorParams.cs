using UnityEngine;

namespace SciFi.Environment.Effects {
    /// Parameters for effects set via the Unity editor (e.g. prefabs).
    public class EffectsEditorParams : MonoBehaviour {
        public static EffectsEditorParams Instance;

        public GameObject star;
        public GameObject explosion;

        void Awake() {
            Instance = this;
        }
    }
}