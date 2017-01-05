using UnityEngine;

namespace SciFi.Items {
    public class FireBall : Projectile {
        void Start() {
            BaseStart();
            Destroy(gameObject, 3f);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }
            if (collision.gameObject.tag == "Player") {
                GameController.Instance.TakeDamage(collision.gameObject, 5);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 3f);
            }
            Destroy(gameObject);
        }
    }
}