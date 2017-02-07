namespace SciFi.Players.Attacks {
    public class DynamiteAttack : Attack {
        bool shouldCharge = true;

        public DynamiteAttack(Nobel player)
            : base(player, true)
        {
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