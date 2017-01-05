using UnityEngine;

namespace SciFi.Items {
    public class IceBall : Projectile {
        void Start() {
            BaseStart();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (collision.gameObject.tag == "Player") {
                GameController.Instance.TakeDamage(collision.gameObject, 3);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 5);
            }
            Destroy(gameObject);
        }
    }
}