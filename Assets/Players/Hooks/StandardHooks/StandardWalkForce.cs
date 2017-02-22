namespace SciFi.Players.Hooks {
    public class StandardWalkForce : WalkForceHook {
        public override bool Call(Direction direction, float axisAmount, ref float walkForce) {
            if (axisAmount < .05f) {
                walkForce = 0f;
                return false;
            }
            if (axisAmount < 0.55f) {
                walkForce /= 1.5f;
            }
            if (direction == Direction.Left) {
                walkForce = -walkForce;
            }
            return true;
        }
    }
}