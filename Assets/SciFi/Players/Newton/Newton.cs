﻿using UnityEngine;

using SciFi.Players.Attacks;
using SciFi.Util;

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
        private CompoundSpriteFlip spriteFlip;

        protected override void OnInitialize() {
            eAttacks[0] = new AppleAttack(this, apple, greenApple);
            eAttacks[1] = new NetworkAttack(new CalcBookAttack(this, InstantiateBooks()));
            eAttacks[2] = new NetworkAttack(new GravityWellAttack(this, gravityWell));
            animator = GetComponent<Animator>();
            spriteFlip = new CompoundSpriteFlip(gameObject, defaultDirection);
        }

        GameObject[] InstantiateBooks() {
            return new [] {
                InstantiateBook(calc1),
                InstantiateBook(calc2),
                InstantiateBook(calc3),
            };
        }

        GameObject InstantiateBook(GameObject prefab) {
            var book = Instantiate(prefab, transform.position, Quaternion.identity, transform);
            book.GetComponent<CalcBook>().spawnedBy = gameObject;
            return book;
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
            BaseInput();
        }

        new void Update() {
            base.Update();
            if (animator == null) {
                return;
            }
            animator.SetFloat("Velocity", lRb.velocity.x);
        }

        protected override void OnChangeDirection() {
            animator.SetBool("FacingLeft", eDirection == Direction.Left);
            spriteFlip.Flip(eDirection);
        }
    }
}