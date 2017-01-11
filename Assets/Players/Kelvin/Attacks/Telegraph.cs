using UnityEngine;

namespace SciFi.Items {
    public class Telegraph : MonoBehaviour {
        public GameObject spawnedBy;

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.tag == "Player") {
                GameController.Instance.TakeDamage(collision.gameObject, 5);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 2f);
                Destroy(gameObject);
            }
        }
    }
}