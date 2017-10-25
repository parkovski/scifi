using UnityEngine;

namespace SciFi.Players.Hooks {
    public class StandardWalkForce : WalkForceHook {
        public override bool Call(float axisAmount, float velocityPercent, ref float walkForce) {
            if (axisAmount < .05f || velocityPercent > .95f) {
                walkForce = 0f;
                return false;
            } else if (axisAmount > .9f) {
                return false;
            }

            walkForce *= 1 - velocityPercent * velocityPercent;
            walkForce *= axisAmount;
            return true;
        }
    }
}