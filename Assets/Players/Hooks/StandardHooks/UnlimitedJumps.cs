namespace SciFi.Players.Hooks {
    public class UnlimitedJumps : JumpForceHook {
        public override bool Call(bool touchingGround, int numJumps, ref float jumpForce) {
            if (!touchingGround) {
                jumpForce *= .3333f;
            } else {
                jumpForce *= .8f;
            }
            return true;
        }
    }
}