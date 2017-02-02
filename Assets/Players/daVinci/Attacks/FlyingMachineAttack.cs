using UnityEngine;

namespace SciFi.Players.Attacks {
    public class FlyingMachineAttack : Attack {
        GameObject flyingMachinePrefab;

        public FlyingMachineAttack(Player player, GameObject flyingMachinePrefab)
            : base(player, true)
        {
            this.flyingMachinePrefab = flyingMachinePrefab;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            Object.Instantiate(flyingMachinePrefab, player.transform.position, Quaternion.identity);
        }
    }
}