using UnityEngine.Scripting;

using SciFi.Players;

namespace SciFi.AI.Strategies {
    /// Fight strategy - run towards a player.
    [Preserve]
    [StrategyType(StrategyType.Movement)]
    [StrategyList(0)]
    public class FoFFightStrategy : Strategy {
        Player me;
        Player opponent;

        public FoFFightStrategy(
            [StrategyParam(StrategyParamType.Me)] Player me,
            [StrategyParam(StrategyParamType.Opponent)] Player opponent
        ) {
            this.me = me;
            this.opponent = opponent;
        }

        public override float advantage {
            get {
                return 0f;
            }
        }

        public override int GetControl() {
            if (me.transform.position.x < opponent.transform.position.x) {
                return Control.Right;
            } else {
                return Control.Left;
            }
        }
    }
}