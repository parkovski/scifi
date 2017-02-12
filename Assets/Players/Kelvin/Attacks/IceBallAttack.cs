using UnityEngine;

namespace SciFi.Players.Attacks {
    public class IceBallAttack : Attack {
        const float verticalForce = 80f;
        const float horizontalForce = 200f;
        const float torqueRange = 8f;
        int iceBallPrefabIndex;

        public IceBallAttack(Player player, GameObject iceBall)
            : base(player, false)
        {
            this.iceBallPrefabIndex = GameController.PrefabToIndex(iceBall);
            canFireDown = true;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var force = new Vector2(0f, verticalForce);
            if (direction == Direction.Down) {
                force = new Vector2(0f, -verticalForce);
            } else if (direction == Direction.Left) {
                force += new Vector2(-horizontalForce, 0f);
            } else {
                force += new Vector2(horizontalForce, 0f);
            }

            var torque = Random.Range(-torqueRange, torqueRange);
            player.CmdSpawnProjectile(
                iceBallPrefabIndex,
                player.gameObject.transform.position,
                Quaternion.identity,
                force,
                torque
            );
        }

        public override void OnCancel() {
        }
    }
}