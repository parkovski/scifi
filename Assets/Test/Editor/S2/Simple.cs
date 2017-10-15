using System.Threading;
using UnityEngine;
using NUnit.Framework;
using SciFi.AI;
using SciFi.AI.S2;
using SciFi.Environment.State;

namespace SciFi.Test {
    public class SimpleS2AITest {
        [Test]
        public void TestStayOnStage() {
            var ai = new S2AI(2, 1);
            var keepWalking = new KeepWalking(0);
            Strategy[] strategies = {
                new StayOnStage(0),
                keepWalking
            };
            var player = new DummyPlayerSnapshotProvider();
            var env = new AIEnvironment(
                1,
                new DummyGameSnapshotProvider(),
                new DummyStageSnapshotProvider(20),
                new [] { player }
            );
            var inp = new [] {
                new AIInputManager()
            };
            ai.Ready(env, strategies, inp);
            ai.BeginEvaluate();
            float min = 0, max = 0;
            for (int i = 0; i < 30; i++) {
                ai.ExecAndMoveNext();
                if (inp[0].IsControlActive(Control.Left)) {
                    player.Move(-1);
                    keepWalking.direction = Control.Left;
                } else if (inp[0].IsControlActive(Control.Right)) {
                    player.Move(1);
                    keepWalking.direction = Control.Right;
                }
                if (player.x < min) { min = player.x; }
                if (player.x > max) { max = player.x; }
                Thread.Sleep(25);
            }
            ai.Dispose();
            Debug.Assert(min < -7f && max > 7f);
        }
    }

    class KeepWalking : Strategy {
        public int direction;

        public KeepWalking(int aiIndex) : base(aiIndex, ActionGroup.Movement) {}

        protected override float OnEvaluate(AIEnvironment env) {
            return 0.25f;
        }

        protected override int OnExecute(AIEnvironment env) {
            return this.direction;
        }
    }
}