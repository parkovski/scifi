using UnityEngine;

namespace SciFi.Players.Attacks {
    public class AppleAttack : Attack {
        const float verticalForce = 50f;
        const float horizontalForce = 20f;
        const float torqueRange = 5f;
        int applePrefabIndex;
        int greenApplePrefabIndex;
        const int chanceOfGreenApple = 10;

        public AppleAttack(Player player, GameObject apple, GameObject greenApple)
            : base(player, false)
        {
            this.applePrefabIndex = GameController.PrefabToIndex(apple);
            this.greenApplePrefabIndex = GameController.PrefabToIndex(greenApple);
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
            var prefabIndex = Random.Range(0, chanceOfGreenApple) == 1 ? greenApplePrefabIndex : applePrefabIndex;
            player.CmdSpawnProjectile(
                prefabIndex,
                player.gameObject.transform.position,
                Quaternion.identity,
                force,
                torque
            );
        }
    }
}