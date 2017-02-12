using UnityEngine;

namespace SciFi.Players.Attacks {
    public class DynamiteAttack : Attack {
        bool hasPlantedDynamite = false;
        const float cooldownInterval = 1f;
        float lastFireTime;

        public DynamiteAttack(Nobel player)
            : base(player, 0f, true)
        {
        }

        public override void OnBeginCharging(Direction direction) {
            if (hasPlantedDynamite) {
                ((Nobel)player).CmdPlantOrExplodeDynamite(0);
                RequestCancel();
            } else {
                if (Time.time < lastFireTime + cooldownInterval) {
                    RequestCancel();
                }
            }
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            int sticks = 1 + (int)Mathf.Clamp(chargeTime * 2, 0, 2f);
            if (!hasPlantedDynamite) {
                ((Nobel)player).CmdPlantOrExplodeDynamite(sticks);
                lastFireTime = Time.time;
            }
        }

        public override void OnCancel() {
        }

        public void SetHasPlantedDynamite(bool hasPlantedDynamite) {
            this.hasPlantedDynamite = hasPlantedDynamite;
        }
    }
}