using UnityEngine;

namespace SciFi.Players.Hooks {
    public class StandardWalkForce : WalkForceHook {
        public override bool Call(float axisAmount, float velocityPercent, ref float walkForce) {
            if (axisAmount < .05f) {
                walkForce = 0f;
                return false;
            }

            walkForce *= MagicForceFunction(velocityPercent);
            return true;
        }

        /// sqrt(15)
        private const float C = 3.872983346207417f;
        /// 2*sqrt(15)
        private const float twoC = C * 2;
        /// 1/(2*C)
        private const float oneOverTwoC = 1 / twoC;
        /// sqrt(sqrt(15))
        private const float sqrtC = 1.9679896712654306f;
        /// sqrt(2*sqrt(15))
        private const float sqrt2C = 2.783157683713741f;
        /// n(L) = sqrt(2L) + sqrt(L)
        private const float sqrt2CPlusSqrtC = sqrtC + sqrt2C;

        /// This applies .5 walk force at v=0, 0 walk force at v=1,
        /// for an inverse parabolic curve with a max force of 1.
        /// I'm not really sure how this works, there was lots of pen & paper,
        /// Julia, Wolfram Alpha and guessing involved.
        /// C = 3.87083 = sqrt(15)
        /// m(L) = sqrt(L)
        /// n(L) = √(2*L)+m(L)
        /// f(x) = (2*C - (n(C)*x - m(C))^2)*.5/C
        private float MagicForceFunction(float v) {
            float stuffToSquare = v * sqrt2CPlusSqrtC - sqrtC;
            return (twoC - stuffToSquare * stuffToSquare) * oneOverTwoC;
        }
    }
}