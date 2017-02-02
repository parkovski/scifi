namespace SciFi.Players.Attacks {
    public class PaintbrushAttack : Attack {
        public PaintbrushAttack(Player player)
            : base(player, true)
        {
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
        }
    }
}