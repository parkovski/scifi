using UnityEngine;

namespace SciFi.Players.Attacks {
    public class PaintStreak : MonoBehaviour {
        float startTime;
        const float lifetime = 1.5f;
        SpriteRenderer spriteRenderer;
        /// The thing that got painted, that the paint streak should follow.
        public GameObject paintedObject;
        Vector3 paintedObjectOffset;

        void Start() {
            startTime = Time.time;
            spriteRenderer = GetComponent<SpriteRenderer>();
            paintedObjectOffset = transform.position - paintedObject.transform.position;
        }

        void Update() {
            if (Time.time > startTime + lifetime) {
                Destroy(gameObject);
            }
            transform.position = paintedObject.transform.position + paintedObjectOffset;
        }
    }
}