namespace SciFi.Players.Hooks {
    public class StandardJumpForce : JumpForceHook {
        public override bool Call(bool touchingGround, int jumps, ref float jumpForce) {
            if (touchingGround) {
                return true;
            } else if (jumps < 2) {
                jumpForce /= 2;
                return true;
            } else {
                jumpForce = 0f;
                return false;
            }
        }
    }
}