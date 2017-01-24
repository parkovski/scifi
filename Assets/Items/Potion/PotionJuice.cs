using UnityEngine;

namespace SciFi.Items {
    public class PotionJuice : Projectile {
        void Start() {
            BaseStart();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }
            GameController.Instance.TakeDamage(collision.gameObject, 10);
            GameController.Instance.Knockback(gameObject, collision.gameObject, 3f);
        }
    }
}