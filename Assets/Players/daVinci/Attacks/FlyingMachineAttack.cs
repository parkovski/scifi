namespace SciFi.Players.Attacks {
    public class FlyingMachineAttack : Attack {
        public FlyingMachineAttack(Player player)
            : base(player, true)
        {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
        }
    }
}