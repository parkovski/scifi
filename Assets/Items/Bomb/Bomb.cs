using UnityEngine;

namespace SciFi.Items {
    public class Bomb : Item {
        void Start() {
            BaseStart(aliveTime: 10f);
        }

        void Update() {
            BaseUpdate();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            BaseCollisionEnter2D(collision);

            if (collision.gameObject.tag == "Player") {
                GameController.Instance.TakeDamage(collision.gameObject, 15);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 10f);
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