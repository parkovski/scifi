using UnityEngine;

namespace SciFi.Players.Attacks {
    public class AppleAttack : Attack {
        const float verticalVelocity = 14f;
        const float horizontalVelocity = 4f;
        const float angularVelocityRange = 2000f;
        int applePrefabIndex;
        int greenApplePrefabIndex;
        const int chanceOfGreenApple = 10;

        public AppleAttack(Player player, GameObject apple, GameObject greenApple)
            : base(player, 0.25f, false)
        {
            this.applePrefabIndex = GameController.PrefabToIndex(apple);
            this.greenApplePrefabIndex = GameController.PrefabToIndex(greenApple);
            canFireDown = true;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var velocity = new Vector2(0f, verticalVelocity);
            if (direction == Direction.Down) {
                velocity = new Vector2(0f, -horizontalVelocity);
            } else if (direction == Direction.Left) {
                velocity += new Vector2(-horizontalVelocity, 0f);
            } else {
                velocity += new Vector2(horizontalVelocity, 0f);
            }

            var angularVelocity = Random.Range(-angularVelocityRange, angularVelocityRange);
            var prefabIndex = Random.Range(0, chanceOfGreenApple) == 1 ? greenApplePrefabIndex : applePrefabIndex;
            player.CmdSpawnProjectile(
                prefabIndex,
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