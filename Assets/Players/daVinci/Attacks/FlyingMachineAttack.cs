using UnityEngine;
using UnityEngine.Networking;

using SciFi.Util.Extensions;

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
            fm.dx = 1.5f.FlipDirection(direction);
            NetworkServer.Spawn(fmObj);
        }
    }
}