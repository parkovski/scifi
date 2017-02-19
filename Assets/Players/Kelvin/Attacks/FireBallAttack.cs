using UnityEngine;

using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    // The fire ball grows bigger while charging,
    // when released flys in a straight horizontal line,
    // and latches onto the player it hits, doing between
    // 3-5 rounds of damage.
    public class FireBallAttack : Attack {
        const float horizontalForce = 20f;

        public FireBallAttack(Kelvin player)
            : base(player, true)
        {
        }

        public override void OnBeginCharging(Direction direction) {
            ((Kelvin)player).CmdChargeOrThrowFireball(
                player.transform.position + new Vector3(1f, 0f).FlipDirection(player.eDirection)
            );
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            ((Kelvin)player).CmdChargeOrThrowFireball(Vector2.zero);
        }

        public override void OnCancel() {
            ((Kelvin)player).CmdStopChargingFireball();
        }
    }
}
