using UnityEngine;

namespace SciFi.Players.Attacks {
    public class IceBallAttack : Attack {
        const float verticalVelocity = 4f;
        const float horizontalVelocity = 10f;
        const float angularVelocityRange = 3000f;
        int iceBallPrefabIndex;

        public IceBallAttack(Player player, GameObject iceBall)
            : base(player, .25f, false)
        {
            this.iceBallPrefabIndex = GameController.PrefabToIndex(iceBall);
            canFireDown = true;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var velocity = new Vector2(0f, verticalVelocity);
            if (direction == Direction.Down) {
                velocity = new Vector2(0f, -verticalVelocity);
            } else if (direction == Direction.Left) {
                velocity += new Vector2(-horizontalVelocity, 0f);
            } else {
                velocity += new Vector2(horizontalVelocity, 0f);
            }

            var angularVelocity = Random.Range(-angularVelocityRange, angularVelocityRange);
            player.CmdSpawnProjectile(
                iceBallPrefabIndex,
                player.gameObject.transform.position,
                Quaternion.identity,
                velocity,
                angularVelocity
            );
        }

        public override void OnCancel() {
        }
    }
}