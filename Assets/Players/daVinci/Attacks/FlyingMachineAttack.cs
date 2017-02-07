namespace SciFi.Players.Attacks {
    public class FlyingMachineAttack : Attack {
        public FlyingMachineAttack(daVinci player)
            : base(player, true)
        {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            ((daVinci)player).CmdSpawnFlyingMachine(chargeTime);
        }
    }
}