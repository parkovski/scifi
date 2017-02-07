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
            if (!isServer) {
                return;
            }

            var hit = Attack.GetAttackHit(collision.gameObject.layer);
            if (hit != AttackHit.None) {
                if (hit == AttackHit.HitAndDamage) {
                    GameController.Instance.Hit(collision.gameObject, this, gameObject, damage, knockback);
                }
                Destroy(gameObject);
            }
        }

        public override AttackProperty Properties { get { return AttackProperty.Explosive; } }
    }
}