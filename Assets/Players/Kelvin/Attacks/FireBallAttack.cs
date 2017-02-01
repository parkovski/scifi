using UnityEngine;
using UnityEngine.Networking;

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
            var offset = player.eDirection == Direction.Left
                ? new Vector3(-1f, .5f)
                : new Vector3(1f, .5f);
            chargingFireBall = Object.Instantiate(
                fireBall,
                player.gameObject.transform.position + offset,
                Quaternion.identity
            );
            var fb = chargingFireBall.GetComponent<FireBall>();
            fb.spawnedBy = player.netId;
            fb.spawnedByExtra = player.GetItemNetId();
            NetworkServer.Spawn(chargingFireBall);
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
            fb.AddInitialForce(force);
            fb.SetCanDestroy(true);
        }
    }
}
