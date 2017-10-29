namespace SciFi.Players.Hooks {
    public class CantMoveMaxSpeedHook : MaxSpeedHook {
        public override bool Call(float axisAmount, ref float maxSpeed) {
            maxSpeed = 0;
            return false;
        }
    }
}