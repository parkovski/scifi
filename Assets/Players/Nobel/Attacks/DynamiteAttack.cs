using UnityEngine;

namespace SciFi.Players.Attacks {
    public class DynamiteAttack : Attack {
        GameObject[] dynamitePrefabs;
        Dynamite dynamite;

        public DynamiteAttack(Player player, GameObject[] dynamitePrefabs)
            : base(player, true)
        {
            this.dynamitePrefabs = dynamitePrefabs;
        }

        public override void OnBeginCharging(Direction direction) {
            if (dynamite != null) {
                dynamite.Explode();
                RequestCancel();
                return;
            }

            var position = player.transform.position;
            if (direction == Direction.Left) {
                position += new Vector3(-1f, -.5f);
            } else {
                position += new Vector3(1f, -.5f);
            }
            var dynamiteGo = Object.Instantiate(dynamitePrefabs[0], position, Quaternion.identity);
            dynamite = dynamiteGo.GetComponent<Dynamite>();
            dynamite.spawnedBy = player.netId;
            dynamite.spawnedByExtra = player.GetItemNetId();
            dynamite.explodeCallback = OnDynamiteExploded;
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {

        }

        void OnDynamiteExploded() {
            dynamite = null;
        }
    }
}