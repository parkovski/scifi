using UnityEngine;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class Bullet : Projectile {
        public int damage;
        public float knockback;
        Vector2 originalPosition;

        public void Start() {
            BaseStart();
            originalPosition = transform.position;

            if (isServer) {
                Effects.Smoke(transform.position);
            }
        }

        void Update() {
            if (!isServer) {
                return;
            }

            if (((Vector2)transform.position - originalPosition).magnitude > 2f) {
                Destroy(gameObject);
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            var hit = Attack.GetAttackHit(collision.gameObject.layer);
            if (hit != AttackHit.None) {
                if (hit == AttackHit.HitAndDamage) {
                    GameController.Instance.HitNoVelocityReset(collision.gameObject, this, gameObject, damage, knockback);
                }
                Destroy(gameObject);
            }
        }

        public override AttackProperty Properties { get { return AttackProperty.Explosive; } }
    }
}