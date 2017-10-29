namespace SciFi.Players.Hooks {
    public class UnlimitedJumps : JumpForceHook {
        int jumps;
        public UnlimitedJumps(int jumps) {
            this.jumps = jumps;
        }

        public override bool Call(bool touchingGround, int numJumps, ref float jumpForce) {
            if (numJumps >= jumps) {
                jumpForce = 0f;
                return false;
            }
            if (!touchingGround) {
                jumpForce *= .3333f;
            } else {
                jumpForce *= .8f;
            }
            return true;
        }
    }
}