using UnityEngine;
using static UnityEngine.Mathf;
using Random = System.Random;
using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.AI.S2 {
    public class Wander : Strategy {
        float nextChangeTime;
        Direction direction;

        public Wander(int aiIndex)
            : base(aiIndex, ActionGroup.Movement)
        {}

        private void ChangeTimer(AIEnvironment env) {
            var time = env.time;
            if (time > nextChangeTime) {
                var dt = (float)(env.threadRandom.NextDouble() * 24 + 1);
                // Use Sqrt to weight this toward the upper range.
                nextChangeTime = time + Sqrt(dt);
                var r = env.threadRandom.Next(0, 3);
                if (r == 0) {
                    direction = Direction.Left;
                } else if (r == 2) {
                    direction = Direction.Right;
                } else {
                    direction = Direction.Invalid;
                }
            }
        }

        protected override float OnEvaluate(AIEnvironment env) {
            ChangeTimer(env);
            if (direction == Direction.Invalid) {
                return 0;
            }
            return .01f;
        }

        protected override int OnExecute(AIEnvironment env) {
            return direction.AsControl();
        }
    }
}