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
            eAttacks[0] = new IceBallAttack(this, iceballPrefab);
            eAttacks[1] = new FireBallAttack(this);
            eAttacks[2] = new NetworkAttack(new TelegraphAttack(this, telegraph));
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
        public void CmdStartChargingFireball(Vector2 position) {
            if (chargingFireball == null) {
                chargingFireball = SpawnPooledProjectile(
                    GameController.PrefabToIndex(fireballPrefab),
                    position,
                    Quaternion.identity,
                    Vector2.zero,
                    0,
                    false
                );
                chargingFireball.GetComponent<FireBall>().destroyCallback = OnFireballDestroyed;
                RpcSetFireballActive(true);
            }
        }

        [Command]
        public void CmdEndChargingFireball(Direction direction) {
            if (chargingFireball != null) {
                chargingFireball.GetComponent<FireBall>().StopCharging();
            }
        }

        [Command]
        public void CmdThrowFireball(Direction direction) {
            if (chargingFireball != null) {
                chargingFireball.GetComponent<FireBall>().Throw(direction);
            }
        }

        [Server]
        void OnFireballDestroyed(GameObject objectBeingDestroyed) {
            RpcSetFireballActive(false);
            chargingFireball = null;
        }

        [ClientRpc]
        void RpcSetFireballActive(bool isActive) {
            ((FireBallAttack)eAttacks[1]).SetHasActiveFireball(isActive);
        }

        [Command]
        public void CmdCancelFireball(bool onPurpose) {
            if (onPurpose) {
                return;
            }
            if (chargingFireball != null) {
                PooledObject.Get(chargingFireball).Release();
            }
        }
    }
}