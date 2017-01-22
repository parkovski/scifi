using UnityEngine;

namespace SciFi.Items {
    public class Bomb : Item {
        void Start() {
            BaseStart(false);
        }

        void Update() {
            BaseUpdate();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            BaseCollisionEnter2D(collision);

            var layer = collision.gameObject.layer;
            if (layer == Layers.projectiles) {
                Destroy(gameObject);
            } else if (layer == Layers.players || layer == Layers.items) {
                GameController.Instance.TakeDamage(collision.gameObject, 15);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 7.5f);
                Destroy(gameObject);
            }
        }

        public override bool ShouldThrow() {
            return true;
        }

        public override bool ShouldCharge() {
            return false;
        }
    }
}