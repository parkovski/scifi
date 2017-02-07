using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Players.Attacks {
    public class DynamiteAttack : Attack {
        GameObject[] dynamitePrefabs;
        Dynamite dynamite;
        bool shouldCharge = true;

        public DynamiteAttack(Nobel player, GameObject[] dynamitePrefabs)
            : base(player, true)
        {
            this.dynamitePrefabs = dynamitePrefabs;
        }

        public override void OnBeginCharging(Direction direction) {
            if (!shouldCharge) {
                ((Nobel)player).CmdPlantOrExplodeDynamite();
                RequestCancel();
            }
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            if (shouldCharge) {
                ((Nobel)player).CmdPlantOrExplodeDynamite();
            }
        }

        public void SetShouldCharge(bool shouldCharge) {
            this.shouldCharge = shouldCharge;
        }
    }
}