namespace SciFi.Players.Hooks {
    public class StandardMaxSpeed : MaxSpeedHook {
        public override bool Call(float axisAmount, ref float maxSpeed) {
            if (axisAmount < .05f) {
                maxSpeed = 0f;
                return false;
            }
            if (axisAmount < .55f) {
                maxSpeed /= 2;
            }
            return true;
        }
    }
}