using UnityEngine;

namespace SciFi.Players.Attacks {
    public class FlyingMachineAttack : Attack {
        GameObject flyingMachinePrefab;

        public FlyingMachineAttack(daVinci player, GameObject flyingMachinePrefab)
            : base(player, true)
        {
            this.flyingMachinePrefab = flyingMachinePrefab;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            ((daVinci)player).CmdSpawnFlyingMachine(chargeTime);
        }
    }
}