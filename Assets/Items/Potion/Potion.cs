using UnityEngine;

using SciFi.Players;

namespace SciFi.Items {
    public class Potion : Item {
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
            var otherLayer = collision.gameObject.layer;
            if (otherLayer == Layers.projectiles || otherLayer == Layers.players || otherLayer == Layers.items) {
                Instantiate(brokenPotionPrefab, transform.position, Quaternion.identity);
                if (!used) {
                    SpillJuice();
                }
                Destroy(gameObject);
            }
        }

        public void StopAnimation() {
            animator.enabled = false;
        }

        public void SpillJuice() {
            //
        }
    }
}