namespace SciFi.Players.Hooks {
    public class SlowMaxSpeedHook : MaxSpeedHook {
        public override bool Call(float axisAmount, ref float maxSpeed) {
            maxSpeed *= .5f;
            return true;
        }
    }
}