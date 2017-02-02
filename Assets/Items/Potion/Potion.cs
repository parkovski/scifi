using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;
using SciFi.Players.Attacks;

namespace SciFi.Items {
    public class Potion : Item {
        public GameObject juicePrefab;
        public GameObject brokenPotionPrefab;

        bool used = false;
        Animator animator;

        void Start() {
            BaseStart(false);
            animator = GetComponent<Animator>();
        }

        void Update() {
            BaseUpdate();
        }

        public override bool ShouldCharge() {
            return false;
        }

        public override bool ShouldThrow() {
            return used;
        }

        protected override void OnEndCharging(float chargeTime) {
            animator.enabled = true;
            if (eDirection == Direction.Left) {
                animator.SetTrigger("SpillLeft");
            } else {
                animator.SetTrigger("SpillRight");
            }
            used = true;
        }

        protected override void OnChangeDirection(Direction direction) {
            spriteRenderer.flipX = direction == Direction.Left;
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
            var hit = Attack.GetAttackHit(collision.gameObject.layer);
            if (hit != AttackHit.None) {
                if (!used) {
                    SpillJuice(Direction.Down);
                }
                if (hit == AttackHit.HitAndDamage) {
                    GameController.Instance.TakeDamage(collision.gameObject, 5);
                    GameController.Instance.Knockback(gameObject, collision.gameObject, 3f);
                }
                Instantiate(brokenPotionPrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }

        public void StopAnimation() {
            animator.enabled = false;
        }

        Vector3 GetJuiceOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.316f, -.32f);
            } else if (direction == Direction.Right) {
                return new Vector3(.316f, -.32f);
            } else {
                // Down
                return Vector3.zero;
            }
        }

        public void SpillJuiceLeft() {
            SpillJuice(Direction.Left);
        }

        public void SpillJuiceRight() {
            SpillJuice(Direction.Right);
        }

        public void SpillJuice(Direction direction) {
            var juice = Instantiate(juicePrefab, transform.position + GetJuiceOffset(direction), Quaternion.identity);
            var pj = juice.GetComponent<PotionJuice>();
            pj.spawnedBy = netId;
            if (eOwner != null) {
                pj.spawnedByExtra = eOwner.netId;
            }
            NetworkServer.Spawn(juice);
        }
    }
}