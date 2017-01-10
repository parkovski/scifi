using UnityEngine;

namespace SciFi.Players.Attacks {
    public class AppleAttack : Attack {
        const float verticalForce = 50f;
        const float horizontalForce = 20f;
        const float torqueRange = 5f;
        int applePrefabIndex;

        public AppleAttack(Player player, GameObject apple)
            : base(player, false)
        {
            this.applePrefabIndex = GameController.PrefabToIndex(apple);
            canFireDown = true;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var force = new Vector2(0f, verticalForce);
            if (direction == Direction.Down) {
                force = new Vector2(0f, -horizontalForce);
            } else if (direction == Direction.Left) {
                force += new Vector2(-horizontalForce, 0f);
            } else {
                force += new Vector2(horizontalForce, 0f);
            }

            var torque = Random.Range(-torqueRange, torqueRange);
            player.CmdSpawnProjectile(
                applePrefabIndex,
                player.gameObject.transform.position,
                Quaternion.identity,
                force,
                torque
            );
        }
    }
}