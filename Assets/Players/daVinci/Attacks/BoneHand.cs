using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class BoneHand : Projectile {
        void Start() {
            BaseStart();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                GameController.Instance.TakeDamage(collision.gameObject, 10);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 3.5f);
            }
            Destroy(gameObject);
        }
    }
}