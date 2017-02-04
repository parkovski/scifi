using UnityEngine;

using SciFi.Players.Attacks;

namespace SciFi.Items {
    public class Arrow : Projectile {
        void Start() {
            BaseStart();
        }

        void Update() {
            var velocity = GetComponent<Rigidbody2D>().velocity;
            if (velocity.x > 0) {
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            } else {
                transform.rotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(velocity.y, -velocity.x) * Mathf.Rad2Deg);
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                GameController.Instance.Hit(collision.gameObject, this, gameObject, 5, 1f);
            }
            Destroy(gameObject);
        }
    }
}