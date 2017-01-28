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

        Animator animator;

        bool isAttacking;

        void Start() {
            BaseStart(false);
            animator = GetComponent<Animator>();
        }

        void Update() {
            BaseUpdate();
        }

        public void StartAttacking() {
            //gameObject.layer = Layers.noncollidingItems;
            isAttacking = true;
        }

        public void StopAttacking() {
            //gameObject.layer = Layers.displayOnly;
            isAttacking = false;
            ClearHits();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        void OnCollisionStay2D(Collision2D collision) {
            print(isAttacking);
            if (!isAttacking) {
                return;
            }

            if (DidHit(collision.gameObject)) {
                return;
            }
            LogHit(collision.gameObject);

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

        protected override Vector3 GetOwnerOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.7f, 0);
            } else {
                return new Vector3(.7f, 0);
            }
        }
    }
}