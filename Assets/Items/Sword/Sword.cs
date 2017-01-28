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
            animator = GetComponentInChildren<Animator>();
            isTriggerItem = true;
            detectsCollisionInChild = true;
        }

        void Update() {
            BaseUpdate();
        }

        public void StartAttacking() {
            //gameObject.layer = Layers.noncollidingItems;
            isAttacking = true;
            lRb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            lRb.WakeUp();
        }

        public void StopAttacking() {
            //gameObject.layer = Layers.displayOnly;
            isAttacking = false;
            lRb.sleepMode = RigidbodySleepMode2D.StartAwake;
            ClearHits();
        }

        public void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        public void OnTriggerStay2D(Collider2D collider) {
            if (!isAttacking) {
                return;
            }

            if (DidHit(collider.gameObject)) {
                return;
            }
            LogHit(collider.gameObject);

            var layer = collider.gameObject.layer;
            if (layer == Layers.projectiles || layer == Layers.players || layer == Layers.items) {
                Effects.Star(collider.bounds.ClosestPoint(transform.position));
                GameController.Instance.TakeDamage(collider.gameObject, 5);
                GameController.Instance.Knockback(gameObject, collider.gameObject, 2f);
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
                return new Vector3(-.7f, .3f);
            } else {
                return new Vector3(.7f, .3f);
            }
        }
    }
}