using UnityEngine;

namespace SciFi.Players.Attacks {
    public class PaintStreak : MonoBehaviour {
        float startTime;
        const float lifetime = 1.5f;
        /// The thing that got painted, that the paint streak should follow.
        [HideInInspector]
        public GameObject paintedObject;
        Vector3 paintedObjectOffset;

        void Start() {
            startTime = Time.time;
            paintedObjectOffset = transform.position - paintedObject.transform.position;
        }

        void Update() {
            if (Time.time > startTime + lifetime || paintedObject == null) {
                Destroy(gameObject);
            }
            transform.position = paintedObject.transform.position + paintedObjectOffset;
        }
    }
}