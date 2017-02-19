using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;
using SciFi.Util;

namespace SciFi.Players {
    public class Kelvin : Player {
        public GameObject iceballPrefab;
        public GameObject fireballPrefab;
        public GameObject telegraph;
        
        private GameObject chargingFireball;

        private CompoundSpriteFlip spriteFlip;

        protected override void OnInitialize() {
            eAttack1 = new IceBallAttack(this, iceballPrefab);
            eAttack2 = new FireBallAttack(this);
            eAttack3 = new TelegraphAttack(this, telegraph);
            spriteFlip = new CompoundSpriteFlip(gameObject, defaultDirection);
        }

        void FixedUpdate() {
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

        [Command]
        public void CmdChargeOrThrowFireball(Vector2 position) {
            if (chargingFireball == null) {
                chargingFireball = SpawnPooledProjectile(
                    GameController.PrefabToIndex(fireballPrefab),
                    position,
                    Quaternion.identity,
                    Vector2.zero,
                    0,
                    false
                );
            } else {
                chargingFireball.GetComponent<FireBall>().Throw(eDirection);
                chargingFireball = null;
            }
        }

        [Command]
        public void CmdStopChargingFireball() {
            if (chargingFireball != null) {
                PooledObject.Get(chargingFireball).Release();
                chargingFireball = null;
            }
        }
    }
}