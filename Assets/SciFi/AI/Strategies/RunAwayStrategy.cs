using UnityEngine;
using UnityEngine.Scripting;

using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.AI.Strategies {
    /// Flight strategy - run away from a player.
    [Preserve]
    [StrategyType(StrategyType.Movement)]
    [StrategyList(0)]
    public class RunAwayStrategy : Strategy {
        Player me;
        Player opponent;

        const float targetDistance = 5f;

        public RunAwayStrategy(
            [StrategyParam(StrategyParamType.Me)] Player me,
            [StrategyParam(StrategyParamType.Opponent)] Player opponent
        ) {
            this.me = me;
            this.opponent = opponent;
        }

        public override float advantage {
            get {
                var delta = Mathf.Abs(me.transform.position.x - opponent.transform.position.x);
                if (delta > targetDistance) {
                    return -1f;
                }
                return delta.Scale(0, targetDistance, .5f, 0);
            }
        }

        public override int GetControl() {
            if (me.transform.position.x < opponent.transform.position.x) {
                return Control.Left;
            } else {
                return Control.Right;
            }
        }
    }
}