using UnityEngine;
using static UnityEngine.Mathf;
using Random = System.Random;
using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.AI.S2 {
    public class Wander : Strategy {
        float nextChangeTime;
        Direction direction;

        public Wander(int aiId, AIInputManager inputManager)
            : base(aiId, ActionGroup.Movement, inputManager)
        {
            // To start, we don't want to just stand still.
            // Initialized in main thread, so this is ok.
            if (UnityEngine.Random.value < .5f) {
                direction = Direction.Left;
            } else {
                direction = Direction.Right;
            }
        }

        /// Returns true if timer expired.
        private bool UpdateTimer(AIEnvironment env) {
            var time = env.time;
            if (time > nextChangeTime) {
                var dt = (float)(env.threadRandom.NextDouble() * 24 + 1);
                // Use Sqrt to weight this toward the upper range.
                nextChangeTime = time + Sqrt(dt);
                return true;
            }
            return false;
        }

        private void ChangeDirection(Random random) {
            var r = random.Next(0, 5);
            if (r < 2) {
                direction = Direction.Left;
            } else if (r < 4) {
                direction = Direction.Right;
            } else {
                direction = Direction.Invalid;
            }
        }

        protected override void OnActivate(AIEnvironment env) {
            var vx = env.players[1].velocity.x;
            if (Abs(vx) < .25f) {
                ChangeDirection(env.threadRandom);
            } else if (vx < 0) {
                direction = Direction.Left;
            } else {
                direction = Direction.Right;
            }
        }

        protected override float OnEvaluate(AIEnvironment env) {
            if (UpdateTimer(env)) {
                ChangeDirection(env.threadRandom);
            }
            if (direction == Direction.Invalid) {
                return 0;
            }
            return .1f;
        }

        protected override int OnExecute(AIEnvironment env) {
            return direction.AsControl();
        }
    }
}