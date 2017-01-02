using UnityEngine;

namespace SciFi.Players.Attacks {
    public class GreenAppleAttack : Attack {
        GameObject apple;
        float nextSpawnTime;
        int count;

        const int maxApples = 4;

        public GreenAppleAttack(Player player, GameObject apple)
            : base(player, true)
        {
            this.apple = apple;
        }

        void Spawn(int n, bool backwards) {
            // x: -1, 0, 1
            // y: 0, 1, 0
            var x = Mathf.Cos(Mathf.PI * (float)n / maxApples);
            var y = Mathf.Sin(Mathf.PI * (float)n / maxApples);
            if (backwards) {
                x = -x;
            }

            GameController.Instance.CmdSpawnProjectile(
                apple,
                player.netId,
                player.GetItemNetId(),
                player.gameObject.transform.position + new Vector3(x, y),
                Quaternion.identity,
                Vector2.zero,
                0f
            );
        }

        public override void OnBeginCharging(Direction direction) {
            Spawn(0, direction == Direction.Left);
            count = 0;
            nextSpawnTime = .25f;
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            if (chargeTime > nextSpawnTime && count < maxApples) {
                Spawn(++count, direction == Direction.Left);
                nextSpawnTime += .25f;
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
        }
    }
}