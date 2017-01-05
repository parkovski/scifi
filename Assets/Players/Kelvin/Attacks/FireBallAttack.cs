using UnityEngine;

namespace SciFi.Players.Attacks {
    public class FireBallAttack : Attack {
        const float horizontalForce = 20f;
        GameObject fireBall;
        int fireBallPrefabIndex;
        GameObject chargingFireBall;

        public FireBallAttack(Player player, GameObject fireBall)
            : base(player, true)
        {
            this.fireBall = fireBall;
            this.fireBallPrefabIndex = GameController.PrefabToIndex(fireBall);
        }

        public override void OnBeginCharging(Direction direction) {
            var offset = player.direction == Direction.Left
                ? new Vector3(-1f, .5f)
                : new Vector3(1f, .5f);
            chargingFireBall = Object.Instantiate(
                fireBall,
                player.gameObject.transform.position + offset,
                Quaternion.identity
            );
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            Vector2 force;
            if (direction == Direction.Left) {
                force = new Vector2(-horizontalForce, 0f);
            } else {
                force = new Vector2(horizontalForce, 0f);
            }

            var position = chargingFireBall.transform.position;
            Object.Destroy(chargingFireBall);

            player.CmdSpawnProjectile(
                fireBallPrefabIndex,
                player.netId,
                player.GetItemNetId(),
                position,
                Quaternion.identity,
                force,
                0f
            );
        }
    }
}
