namespace SciFi.Players.Hooks {
    public class StandardJumpForce : JumpForceHook {
        public override bool Call(bool touchingGround, int jumps, ref float jumpForce) {
            return true;
        }
    }
}