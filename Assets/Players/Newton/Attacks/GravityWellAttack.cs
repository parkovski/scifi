using UnityEngine;
using System.Linq;

namespace SciFi.Players.Attacks {
    public class GravityWellAttack : Attack, IAttackSource {
        GameObject gravityWellPrefab;
        GameObject gravityWellObj;
        GravityWell gravityWell;
        float power;

        const float minChargeTime = 0.001f;
        const float maxChargeTime = 1.5f;

        public GravityWellAttack(Player player, GameObject gravityWell)
            : base(player, true)
        {
            gravityWellPrefab = gravityWell;
        }

        public override void OnBeginCharging(Direction direction) {
            gravityWellObj = Object.Instantiate(gravityWellPrefab, player.transform.position, Quaternion.identity);
            gravityWell = gravityWellObj.GetComponent<GravityWell>();
            gravityWell.player = player.gameObject;
            power = 1f;
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            chargeTime = Mathf.Clamp(chargeTime, minChargeTime, maxChargeTime);
            var scale = 150f * (1f + Mathf.Log10(chargeTime * 50));
            gravityWellObj.transform.localScale = new Vector3(scale, scale, 1);
            // From 1-10.
            power = 1f + chargeTime / maxChargeTime * 9f;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            gravityWell.shrinking = true;
            FindCollisions();
        }

        public override void OnCancel() {
            gravityWell.shrinking = true;
        }

        void FindCollisions() {
            var radius = gravityWell.GetComponent<SpriteRenderer>().bounds.extents.x;
            // The LINQ stuff makes sure we only hit a player once,
            // since they have multiple components and hit boxes.
            var objs = Physics2D.CircleCastAll(
                origin: gravityWellObj.transform.position,
                radius: radius,
                direction: Vector2.zero,
                distance: Mathf.Infinity,
                layerMask: Attack.LayerMask
            ).Where(h => h.rigidbody != null && h.rigidbody.gameObject != null)
             .Select(h => h.rigidbody.gameObject)
             .Where(go => go.transform.parent == null)
             .Distinct();

            foreach (var go in objs) {
                if (go == player.gameObject) {
                    continue;
                }
                var p = go.GetComponent<Player>();
                if (p != null) {
                    GameController.Instance.Hit(go, this, player.gameObject, (int)(1.3f * power), power * -3.5f);
                }
            }
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.AffectsGravity; } }
    }
}