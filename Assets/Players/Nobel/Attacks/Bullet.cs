using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class Bullet : Projectile {
        public int damage;
        public float knockback;

        public void Start() {
            BaseStart();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            var hit = Attack.GetAttackHit(collision.gameObject.layer);
            if (hit != AttackHit.None) {
                if (hit == AttackHit.HitAndDamage) {
                    GameController.Instance.TakeDamage(collision.gameObject, damage);
                    GameController.Instance.Knockback(gameObject, collision.gameObject, knockback);
                }
                Destroy(gameObject);
            }
        }
    }
}