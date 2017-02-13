using UnityEngine;

namespace SciFi.Players.Attacks {
    public class DynamiteAttack : Attack {
        bool hasPlantedDynamite = false;
        const float cooldownInterval = 1f;
        float lastFireTime;
        int sticks;

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
                } else {
                    ((Nobel)player).CmdPlantOrExplodeDynamite(sticks = 1);
                }
            }
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            int newSticks = 1 + (int)Mathf.Clamp(chargeTime * 2, 0, 2f);
            if (newSticks > sticks) {
                ((Nobel)player).CmdPlantOrExplodeDynamite(sticks = newSticks);
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            lastFireTime = Time.time;
        }

        public override void OnCancel() {
        }

        public void SetHasPlantedDynamite(bool hasPlantedDynamite) {
            this.hasPlantedDynamite = hasPlantedDynamite;
        }
    }
}