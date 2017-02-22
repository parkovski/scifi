namespace SciFi.Players.Hooks {
    public class FastMaxSpeedHook : MaxSpeedHook {
        public override bool Call(float axisAmount, ref float maxSpeed) {
            maxSpeed *= 1.5f;
            return true;
        }
    }
}