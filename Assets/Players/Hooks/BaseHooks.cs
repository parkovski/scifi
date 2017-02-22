using SciFi.Players.Modifiers;

namespace SciFi.Players.Hooks {
    public abstract class MaxSpeedHook : Hook {
        public abstract bool Call(float axisAmount, ref float maxSpeed);

        public override void Install(HookCollection hooks) {
            hooks.AddMaxSpeedHook(this);
        }

        public override void Remove(HookCollection hooks) {
            hooks.RemoveMaxSpeedHook(this);
        }
    }

    public abstract class WalkForceHook : Hook {
        public abstract bool Call(Direction direction, float axisAmount, ref float walkForce);

        public override void Install(HookCollection hooks) {
            hooks.AddWalkForceHook(this);
        }

        public override void Remove(HookCollection hooks) {
            hooks.RemoveWalkForceHook(this);
        }
    }

    public abstract class JumpForceHook : Hook {
        public abstract bool Call(bool touchingGround, int jumps, ref float jumpForce);

        public override void Install(HookCollection hooks) {
            hooks.AddJumpForceHook(this);
        }

        public override void Remove(HookCollection hooks) {
            hooks.RemoveJumpForceHook(this);
        }
    }

    public abstract class ModifierStateChangedHook : Hook {
        public abstract bool Call(ModifierCollection modifiers, ModId id, ref bool newState);

        public override void Install(HookCollection hooks) {
            hooks.AddModifierStateChangedHook(this);
        }

        public override void Remove(HookCollection hooks) {
            hooks.RemoveModifierStateChangedHook(this);
        }
    }
}