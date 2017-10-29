namespace SciFi.Players.Attacks {
    public class FlyingMachineAttack : Attack {
        public FlyingMachineAttack(daVinci player)
            : base(player, 2f, true)
        {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            ((daVinci)player).CmdSpawnFlyingMachine(chargeTime);
        }

        public override void OnCancel() {
        }
    }
}