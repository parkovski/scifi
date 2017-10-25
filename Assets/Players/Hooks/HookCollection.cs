using System.Collections.Generic;

using SciFi.Players.Modifiers;

namespace SciFi.Players.Hooks {
    public class HookCollection {
        List<MaxSpeedHook> maxSpeedHooks;
        List<WalkForceHook> walkForceHooks;
        List<JumpForceHook> jumpForceHooks;
        List<ModifierStateChangedHook> modifierStateChangedHooks;

        public HookCollection() {
            maxSpeedHooks = new List<MaxSpeedHook>();
            walkForceHooks = new List<WalkForceHook>();
            jumpForceHooks = new List<JumpForceHook>();
            modifierStateChangedHooks = new List<ModifierStateChangedHook>();
        }

        public void AddMaxSpeedHook(MaxSpeedHook hook) {
            maxSpeedHooks.Add(hook);
        }

        public void RemoveMaxSpeedHook(MaxSpeedHook hook) {
            maxSpeedHooks.Remove(hook);
        }

        public float CallMaxSpeedHooks(float axisAmount, float maxSpeed) {
            foreach (var hook in maxSpeedHooks) {
                if (!hook.IsEnabled) {
                    continue;
                }
                if (!hook.Call(axisAmount, ref maxSpeed)) {
                    break;
                }
            }
            return maxSpeed;
        }

        public void AddWalkForceHook(WalkForceHook hook) {
            walkForceHooks.Add(hook);
        }

        public void RemoveWalkForceHook(WalkForceHook hook) {
            walkForceHooks.Remove(hook);
        }

        public float CallWalkForceHooks(float axisAmount, float velocityPercent, float walkForce) {
            foreach (var hook in walkForceHooks) {
                if (!hook.IsEnabled) {
                    continue;
                }
                if (!hook.Call(axisAmount, velocityPercent, ref walkForce)) {
                    break;
                }
            }
            return walkForce;
        }

        public void AddJumpForceHook(JumpForceHook hook) {
            jumpForceHooks.Add(hook);
        }

        public void RemoveJumpForceHook(JumpForceHook hook) {
            jumpForceHooks.Remove(hook);
        }

        public float CallJumpForceHooks(bool touchingGround, int jumps, float jumpForce) {
            foreach (var hook in jumpForceHooks) {
                if (!hook.IsEnabled) {
                    continue;
                }
                if (!hook.Call(touchingGround, jumps, ref jumpForce)) {
                    break;
                }
            }
            return jumpForce;
        }

        public void AddModifierStateChangedHook(ModifierStateChangedHook hook) {
            modifierStateChangedHooks.Add(hook);
        }

        public void RemoveModifierStateChangedHook(ModifierStateChangedHook hook) {
            modifierStateChangedHooks.Remove(hook);
        }

        public bool CallModifierStateChangedHooks(ModifierCollection modifiers, ModId id, bool newState) {
            foreach (var hook in modifierStateChangedHooks) {
                if (!hook.IsEnabled) {
                    continue;
                }
                if (!hook.Call(modifiers, id, ref newState)) {
                    break;
                }
            }
            return newState;
        }
    }
}