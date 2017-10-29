using UnityEngine;

using SciFi.Environment.Effects;
using SciFi.Players.Attacks;

namespace SciFi.Items {
    public class Bomb : Item {
        void Start() {
            BaseStart();
        }

        void Update() {
            BaseUpdate();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            BaseCollisionEnter2D(collision);

            var hit = Attack.GetAttackHit(collision.gameObject.layer);
            if (hit == AttackHit.HitOnly) {
                Effects.Explosion(transform.position);
                Destroy(gameObject);
            } else if (hit == AttackHit.HitAndDamage) {
                GameController.Instance.Hit(collision.gameObject, this, gameObject, 15, 7.5f);
                Effects.Explosion(transform.position);
                Destroy(gameObject);
            }
        }

        public override void TakeDamage(int amount) {
            Effects.Explosion(transform.position);
            Destroy(gameObject);
        }

        public override bool ShouldThrow() {
            return true;
        }

        public override bool ShouldCharge() {
            return false;
        }

        public override AttackType Type { get { return AttackType.Projectile; } }
        public override AttackProperty Properties { get { return AttackProperty.Explosive; } }
    }
}