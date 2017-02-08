using UnityEngine;

using SciFi.Players.Attacks;
using SciFi.Util;

namespace SciFi.Players {
    public class Kelvin : Player {
        public GameObject iceBall;
        public GameObject fireBall;
        public GameObject fireBallInactive;
        public GameObject telegraph;

        private CompoundSpriteFlip spriteFlip;

        protected override void OnInitialize() {
            eAttack1 = new IceBallAttack(this, iceBall);
            eAttack2 = new FireBallAttack(this, fireBall);
            eSpecialAttack = new TelegraphAttack(this, telegraph);
            spriteFlip = new CompoundSpriteFlip(gameObject, defaultDirection);
        }

        void FixedUpdate() {
            if (!isLocalPlayer) {
                return;
            }

            BaseInput();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        void OnCollisionExit2D(Collision2D collision) {
            BaseCollisionExit2D(collision);
        }

        protected override void OnChangeDirection() {
            spriteFlip.Flip(eDirection);
        }
    }
}