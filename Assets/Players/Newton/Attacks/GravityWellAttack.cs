using UnityEngine;

namespace SciFi.Players.Attacks {
    public class GravityWellAttack : Attack {
        GameObject gravityWellPrefab;
        GameObject gravityWell;

        public GravityWellAttack(Player player, GameObject gravityWell)
            : base(player, true)
        {
            gravityWellPrefab = gravityWell;
        }

        public override void OnBeginCharging(Direction direction) {
            gravityWell = Object.Instantiate(gravityWellPrefab, player.transform.position, Quaternion.identity);
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            chargeTime = Mathf.Clamp(chargeTime, .001f, 1.5f);
            var scale = 200f * (1f + Mathf.Log10(chargeTime * 50));
            gravityWell.transform.localScale = new Vector3(scale, scale, 1);
            gravityWell.transform.position = player.transform.position;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            Object.Destroy(gravityWell);
        }
    }
}