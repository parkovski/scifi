using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class IceBall : Projectile {
        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                GameController.Instance.Hit(collision.gameObject, this, gameObject, 3, 5f);
            }
            Destroy(gameObject);
        }

        public override AttackProperty Properties {
            get {
                return AttackProperty.Frozen;
            }
        }
    }
}