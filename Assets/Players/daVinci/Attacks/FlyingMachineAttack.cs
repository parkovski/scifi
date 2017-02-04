using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Players.Attacks {
    public class FlyingMachineAttack : Attack {
        GameObject flyingMachinePrefab;

        public FlyingMachineAttack(Player player, GameObject flyingMachinePrefab)
            : base(player, true)
        {
            this.flyingMachinePrefab = flyingMachinePrefab;
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            var fmObj = Object.Instantiate(flyingMachinePrefab, player.transform.position, Quaternion.identity);
            var fm = fmObj.GetComponent<FlyingMachine>();
            fm.spawnedBy = player.netId;
            fm.spawnedByExtra = player.GetItemNetId();
            if (direction == Direction.Left) {
                fm.dx = -1.5f;
            } else {
                fm.dx = 1.5f;
            }
            NetworkServer.Spawn(fmObj);
        }
    }
}