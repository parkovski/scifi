using UnityEngine;

namespace SciFi.Players.Attacks {
    public class GeligniteAttack : Attack {
        GameObject gelignitePrefab;

        public GeligniteAttack(Player player, GameObject gelignitePrefab)
            : base(player, false)
        {
            this.gelignitePrefab = gelignitePrefab;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var geligniteGo = Object.Instantiate(
                gelignitePrefab,
                player.transform.position + GetGeligniteOffset(direction),
                Quaternion.identity
            );
            var gelignite = geligniteGo.GetComponent<Gelignite>();
            gelignite.spawnedBy = player.netId;
            gelignite.spawnedByExtra = player.GetItemNetId();
        }

        Vector3 GetGeligniteOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-1f, 0f);
            } else {
                return new Vector3(1f, 0f);
            }
        }
    }
}