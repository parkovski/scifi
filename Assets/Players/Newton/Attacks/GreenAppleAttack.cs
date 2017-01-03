using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class GreenAppleAttack : Attack {
        int appleIndex;
        float nextSpawnTime;
        int count;

        const int maxApples = 5;
        GameObject[] apples;

        public GreenAppleAttack(Player player, GameObject apple)
            : base(player, true)
        {
            this.appleIndex = GameController.PrefabToIndex(apple);
            this.apples = new GameObject[maxApples];
        }

        void Spawn(int n, bool backwards) {
            // x: -1, 0, 1
            // y: 0, 1, 0
            var r = Mathf.PI * (float)n / (maxApples - 1);
            var x = Mathf.Cos(r);
            var y = Mathf.Sin(r);
            if (backwards) {
                x = -x;
                r = -r;
            } else {
                r += Mathf.PI;
            }

            player.CmdSpawnProjectile(
                appleIndex,
                player.netId,
                player.GetItemNetId(),
                player.gameObject.transform.position + new Vector3(x, y),
                Quaternion.identity,
                Vector2.zero,
                0f
            );
            //apples[n].GetComponent<GreenApple>().explodeRotation = Quaternion.Euler(0f, 0f, r * Mathf.Rad2Deg);
        }

        public override void OnBeginCharging(Direction direction) {
            Spawn(0, direction == Direction.Left);
            count = 0;
            nextSpawnTime = .25f;
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            if (chargeTime > nextSpawnTime && ++count < maxApples) {
                Spawn(count, direction == Direction.Left);
                nextSpawnTime += .25f;
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            for (var i = 0; i < apples.Length; i++) {
                var a = apples[i];
                if (a != null) {
                    a.GetComponent<GreenApple>().Explode();
                    apples[i] = null;
                }
            }
        }
    }
}