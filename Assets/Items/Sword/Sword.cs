using UnityEngine;

using SciFi.Environment.Effects;
using SciFi.Players;

namespace SciFi.Items {
    public enum SwordType {
        Standard,
        Wood,
        Fire,
        Ice,
    }

    public class Sword : Item {
        public SwordType swordType;
        public bool isAttacking;

        Animator animator;

        void Start() {
            BaseStart(false);
            animator = GetComponent<Animator>();
        }

        void Update() {
            BaseUpdate();
        }

        public void StartAttacking() {
            isAttacking = true;
        }

        public void StopAttacking() {
            isAttacking = false;
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);

            if (!isAttacking) {
                return;
            }

            var layer = collision.gameObject.layer;
            if (layer == Layers.projectiles || layer == Layers.players || layer == Layers.items) {
                Effects.Star(collision.contacts[0].point);
            }
        }

        public override bool ShouldCharge() {
            return false;
        }

        public override bool ShouldThrow() {
            return false;
        }

        protected override void OnEndCharging(float chargeTime) {
            if (eDirection == Direction.Left) {
                animator.SetTrigger("SwingLeft");
            } else {
                animator.SetTrigger("SwingRight");
            }
        }
    }
}