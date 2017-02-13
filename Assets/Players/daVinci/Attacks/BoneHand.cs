using UnityEngine;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class BoneHand : Projectile {
        void Start() {
            BaseStart();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                Effects.Star(collision.contacts[0].point);
                GameController.Instance.Hit(collision.gameObject, this, gameObject, 10, 7f);
            }
            Destroy(gameObject);
        }
    }
}