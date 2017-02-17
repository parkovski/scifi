using UnityEngine;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    // The fire ball grows bigger while charging,
    // when released flys in a straight horizontal line,
    // and latches onto the player it hits, doing between
    // 3-5 rounds of damage.
    public class FireBallAttack : Attack {
        const float horizontalForce = 20f;
        GameObject fireBall;
        GameObject chargingFireBall;

        public FireBallAttack(Player player, GameObject fireBall)
            : base(player, true)
        {
            this.fireBall = fireBall;
        }

        public override void OnBeginCharging(Direction direction) {
            player.CmdSpawnProjectile(
                GameController.PrefabToIndex(fireBall),
                player.transform.position + new Vector3(1f, .5f).FlipDirection(player.eDirection),
                Quaternion.identity,
                Vector2.zero,
                0f
            );
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            if (chargeTime > 1f) {
                chargeTime = 1f;
            }
            var scale = .25f + chargeTime / 4f;
            chargingFireBall.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            Vector2 force;
            if (direction == Direction.Left) {
                force = new Vector2(-horizontalForce, 0f);
            } else {
                force = new Vector2(horizontalForce, 0f);
            }

            var fb = chargingFireBall.GetComponent<FireBall>();
            fb.SetInitialVelocity(force);
            fb.SetCanDestroy(true);
        }

        public override void OnCancel() {
            if (chargingFireBall != null) {
                Object.Destroy(chargingFireBall);
            }
        }
    }
}
