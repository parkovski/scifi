using UnityEngine;

using SciFi.Environment.Effects;

namespace SciFi.Items {
    public class CalcBook : MonoBehaviour {
        public GameObject spawnedBy;
        public int power;
        public bool attacking = false;

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
        }

        /// This can get called before Start -
        /// this seems like a bug :(.
        void OnTriggerEnter2D(Collider2D collider) {
            if (!attacking) {
                return;
            }

            if (collider.gameObject.tag == "Player") {
                Item.IgnoreCollisions(gameObject, collider.gameObject);
                GameController.Instance.TakeDamage(collider.gameObject, power * 2);
                GameController.Instance.Knockback(spawnedBy, collider.gameObject, power);
                Effects.Star(collider.bounds.ClosestPoint(transform.position));
            }
        }
    }
}