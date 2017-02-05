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
            fm.power = Mathf.Clamp((int)(chargeTime * 7.5f), 1, 10);
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