﻿using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;

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

        void Start() {
            BaseStart();

            eAttack1 = new AppleAttack(this, apple, greenApple);
            eAttack2 = new CalcBookAttack(this, new [] { calc1, calc2, calc3 });
            eSpecialAttack = new GravityWellAttack(this, gravityWell);
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

        void Update() {
            animator.SetFloat("Velocity", lRb.velocity.x);
        }

        [ClientRpc]
        protected override void RpcChangeDirection(Direction direction) {
            foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
                sr.flipX = !sr.flipX;
            }
            for (var i = 0; i < gameObject.transform.childCount; i++) {
                var child = gameObject.transform.GetChild(i);
                child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
            }
        }
}
}