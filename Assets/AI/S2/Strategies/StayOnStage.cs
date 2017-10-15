using static UnityEngine.Mathf;

namespace SciFi.AI.S2 {
    public class StayOnStage : Strategy
    {
        public StayOnStage(int aiIndex)
            : base(aiIndex, ActionGroup.Movement)
        {}

        protected override float OnEvaluate(AIEnvironment env) {
            // Note: assumes the center is at x=0.
            var x = env.playerState[aiIndex].position.x;
            var pct = Abs(x / env.stageState.rightEdge);
            // -6x+1; 0
            if (pct > 2/3f) {
                return 3*pct - 2;
            }
            return 0;
        }

        protected override int OnExecute(AIEnvironment env) {
            if (env.playerState[aiIndex].position.x < 0) {
                return Control.Right;
            } else {
                return Control.Left;
            }
        }
    }
}