using UnityEngine;

namespace SciFi.Players.Attacks {
    public class GravityWell : MonoBehaviour {
        const float shrinkTime = .25f;
        float startShrinkTime = 0f;
        float startScale;

        [HideInInspector]
        public bool shrinking = false;
        [HideInInspector]
        public GameObject player;

        void Update() {
            transform.position = player.transform.position;
            if (!shrinking) {
                return;
            }

            if (startShrinkTime == 0f) {
                startShrinkTime = Time.time;
                startScale = this.transform.localScale.x;
            } else if (Time.time >= startShrinkTime + shrinkTime) {
                Destroy(gameObject);
            } else {
                var scale = startScale * (1 - (Time.time - startShrinkTime) / shrinkTime);
                this.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
    }
}