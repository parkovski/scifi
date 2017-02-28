using UnityEngine;

namespace SciFi.Environment {
    public class StageSettings : MonoBehaviour {
        public bool overrideGravity;
        public float gravity;

        void Start() {
            if (overrideGravity) {
                Physics2D.gravity = new Vector2(0, -gravity);
            }
        }
    }
}