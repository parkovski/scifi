using static UnityEngine.Mathf;

namespace SciFi.AI.S2 {
    /// For most of the stage, this is unimportant. At the edges, we have two
    /// shifts: at `E - 3 units`, it begins to become important to move away.
    /// At `E - 1.5 units`, it quickly becomes the most important movement
    /// action.
    public class StayOnStage : Strategy
    {
        const float redZoneSize = 1.5f;
        const float yellowZoneSize = 3f;

        public StayOnStage(int aiId, AIInputManager inputManager)
            : base(aiId, ActionGroup.Movement, inputManager)
        {}

        protected override float OnEvaluate(AIEnvironment env) {
            // Note: assumes the center is at x=0.
            var x = Abs(env.players[env.AiPlayerId(aiId)].position.x);
            var edge = env.stage.rightEdge;
            var red = edge - redZoneSize;
            var yellow = edge - yellowZoneSize;

            if (x < yellow) {
                // [0, .1]
                return .1f * x / yellow;
            } else if (x < red) {
                // [.1, .7]
                var pct = (x - yellow) / (yellowZoneSize - redZoneSize);
                return .1f + .6f * pct * pct;
            } else {
                // [.7, 1]
                return .7f + .3f * (x - red) / redZoneSize;
            }
        }

        protected override int OnExecute(AIEnvironment env) {
            if (env.players[env.AiPlayerId(aiId)].position.x < 0) {
                return Control.Right;
            } else {
                return Control.Left;
            }
        }
    }
}