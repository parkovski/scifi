using UnityEngine;

using SciFi.Environment.Effects;
using SciFi.Players;
using SciFi.Players.Attacks;

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
        AudioSource audioSource;

        bool isAttacking;

        void Start() {
            BaseStart(false);
            animator = GetComponentInChildren<Animator>();
            audioSource = GetComponent<AudioSource>();
            isTriggerItem = true;
            detectsCollisionInChild = true;
        }

        void Update() {
            BaseUpdate();
        }

        public void StartAttacking() {
            isAttacking = true;
            lRb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            lRb.WakeUp();
        }

        public void StopAttacking() {
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

            var hit = Attack.GetAttackHit(collider.gameObject.layer);
            if (hit == AttackHit.HitAndDamage) {
                Effects.Star(collider.bounds.ClosestPoint(transform.position));
                audioSource.Play();
                GameController.Instance.Hit(collider.gameObject, this, gameObject, 5, 2f);
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

        public override AttackType Type {
            get {
                return eOwner == null ? AttackType.Projectile : AttackType.Melee;
            }
        }
        public override AttackProperty Properties {
            get {
                if (swordType == SwordType.Fire) {
                    return AttackProperty.OnFire;
                } else if (swordType == SwordType.Ice) {
                    return AttackProperty.Frozen;
                } else {
                    return AttackProperty.None;
                }
            }
        }
    }
}