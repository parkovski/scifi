using UnityEngine;

namespace SciFi.Players.Attacks {
    public class GeligniteAttack : Attack {
        GameObject gelignitePrefab;

        public GeligniteAttack(Player player, GameObject gelignitePrefab)
            : base(player, 1f, false)
        {
            this.gelignitePrefab = gelignitePrefab;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            player.CmdSpawnProjectile(
                GameController.PrefabToIndex(gelignitePrefab),
                player.transform.position + GetGeligniteOffset(direction),
                Quaternion.identity,
                Vector2.zero,
                0f
            );
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