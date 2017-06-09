using UnityEngine;
using UnityEngine.Scripting;

using SciFi.Players;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.AI.Strategies {
    [Preserve]
    [StrategyList(0)]
    public class ShootAppleStrategy : AttackStrategy {
        Player me;
        Player opponent;

        ManualSampler controlToggleSampler;
        int control;

        const float targetDistance = 3;
        const float bufferDistance = 1;

        public ShootAppleStrategy(
            [StrategyParam(StrategyParamType.Me)] Player me,
            [StrategyParam(StrategyParamType.Opponent)] Player opponent
        ) {
            this.me = me;
            this.opponent = opponent;

            control = Control.None;
            controlToggleSampler = new ManualSampler(1, ToggleControl, 3);
        }

        void ToggleControl() {
            if (control == Control.None) {
                control = Control.Attack1;
            } else {
                control = Control.None;
            }
        }

        public override float advantage {
            get {
                return 0.5f;
            }
        }
        
        public override int GetControl() {
            if (Mathf.Abs(me.transform.position.x - opponent.transform.position.x) > targetDistance + bufferDistance) {
                return Control.None;
            }
            controlToggleSampler.Run();
            return control;
        }

        public override void GetTargetDistanceRange(out float target, out float buffer) {
            target = targetDistance;
            buffer = bufferDistance;
        }
    }
}