namespace SciFi.Players.Hooks {
    public class StandardMaxSpeed : MaxSpeedHook {
        public override bool Call(float axisAmount, ref float maxSpeed) {
            if (axisAmount < .05f) {
                maxSpeed = 0f;
                return false;
            } else if (axisAmount < 0.25f) {
                // Sneaky mode
                maxSpeed *= .1f;
            } else if (axisAmount > 0.9f) {
                return false;
            }
            maxSpeed *= axisAmount;
            return true;
        }
    }
}