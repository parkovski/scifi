using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class TelegraphAttack : Attack {
        GameObject telegraphPrefab;
        GameObject telegraph;

        public TelegraphAttack(Player player, GameObject telegraph)
            : base(player, true)
        {
            this.telegraphPrefab = telegraph;
        }

        public override void OnBeginCharging(Direction direction) {
            var offset = player.eDirection == Direction.Left
                ? new Vector3(-1f, .5f)
                : new Vector3(1f, .5f);
            telegraph = Object.Instantiate(
                telegraphPrefab,
                player.gameObject.transform.position + offset,
                Quaternion.identity
            );
            telegraph.transform.parent = player.gameObject.transform;
            var behaviour = telegraph.GetComponent<Telegraph>();
            behaviour.spawnedBy = player.gameObject;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            Object.Destroy(telegraph);
        }
    }
}