using UnityEngine;

using SciFi.Players.Attacks;

namespace SciFi.Players {
    public class Kelvin : Player {
        public GameObject iceBall;
        public GameObject fireBall;
        public GameObject fireBallInactive;

        private GameObject chargingFireBall;

        const float iceBallHorizontalForce = 200f;
        const float iceBallVerticalForce = 100f;
        const float iceBallTorqueRange = 10f;

        const float fireBallHorizontalForce = 50f;

        void Start() {
            BaseStart();

            eAttack1 = new IceBallAttack(this, iceBall);
            eAttack2 = new FireBallAttack(this, fireBall);
            eSpecialAttack = eAttack1;
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
    }
}