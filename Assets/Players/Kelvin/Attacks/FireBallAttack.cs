using UnityEngine;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    // The fire ball grows bigger while charging,
    // when released flys in a straight horizontal line,
    // and latches onto the player it hits, doing between
    // 3-5 rounds of damage.
    public class FireBallAttack : Attack {
        const float horizontalForce = 20f;
        bool hasActiveFireball = false;
        bool cancelRequested = false;
        float lastFireTime;

        public FireBallAttack(Kelvin player)
            : base(player, true)
        {
        }

        public override void OnBeginCharging(Direction direction) {
            if (hasActiveFireball) {
                ((Kelvin)player).CmdThrowFireball(direction);
                cancelRequested = true;
                RequestCancel();
            } else {
                ((Kelvin)player).CmdStartChargingFireball(
                    player.transform.position + new Vector3(1f, 0f).FlipDirection(player.eDirection)
                );
                cancelRequested = false;
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            ((Kelvin)player).CmdEndChargingFireball(direction);
        }

        public override void OnCancel() {
            ((Kelvin)player).CmdCancelFireball(cancelRequested);
        }

        public void SetHasActiveFireball(bool isActive) {
            hasActiveFireball = isActive;
        }
    }
}
