using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;
using SciFi.Util.Extensions;

namespace SciFi.Players {
    public class Newton : Player {
        public GameObject apple;
        public GameObject greenApple;
        public GameObject calc1;
        public GameObject calc2;
        public GameObject calc3;
        public GameObject gravityWell;

        private Animator animator;
        private bool walkAnimationPlaying;

        protected override void OnInitialize() {
            eAttack1 = new AppleAttack(this, apple, greenApple);
            eAttack2 = new NetworkAttack(new CalcBookAttack(this, new [] { calc1, calc2, calc3 }), 0.1f);
            eSpecialAttack = new NetworkAttack(new GravityWellAttack(this, gravityWell), 0.1f);
            animator = GetComponent<Animator>();
        }

        public override void OnStartLocalPlayer() {
            //GetComponent<SpriteRenderer>().color = new Color(.8f, .9f, 1f, .8f);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        void OnCollisionExit2D(Collision2D collision) {
            BaseCollisionExit2D(collision);
        }

        void FixedUpdate() {
            if (!isLocalPlayer) {
                return;
            }

            BaseInput();
        }

        new void Update() {
            base.Update();
            if (animator == null) {
                return;
            }
            animator.SetFloat("Velocity", lRb.velocity.x);
        }

        [ClientRpc]
        protected override void RpcChangeDirection(Direction direction) {
            animator.SetBool("FacingLeft", direction == Direction.Left);
            foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
                sr.flipX = direction == Direction.Left;
            }
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                child.localPosition = child.localPosition.FlipDirection(direction);
            }
        }
    }
}