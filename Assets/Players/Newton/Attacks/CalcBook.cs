using UnityEngine;

namespace SciFi.Items {
    public class CalcBook : MonoBehaviour {
        public GameObject spawnedBy;
        public int power;
        /// OnCollisionEnter2D can get called before Start -
        /// this seems like a bug :(.
        bool initialized = false;

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);

            initialized = true;
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (!initialized) {
                return;
            }

            if (collider.gameObject.tag == "Player") {
                GameController.Instance.TakeDamage(collider.gameObject, power * 2);
                GameController.Instance.Knockback(gameObject, collider.gameObject, power);
            }
        }
    }
}