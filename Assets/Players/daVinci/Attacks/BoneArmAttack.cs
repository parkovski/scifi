namespace SciFi.Players.Attacks {
    public class BoneArmAttack : Attack {
        public BoneArmAttack(Player player)
            : base(player, false)
        {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
        }
    }
}