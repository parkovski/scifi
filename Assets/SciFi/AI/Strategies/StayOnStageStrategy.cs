using UnityEngine;
using UnityEngine.Scripting;

using SciFi.Players;

namespace SciFi.AI.Strategies {
    /// Don't let the player run off the stage.
    [Preserve]
    [StrategyType(StrategyType.Movement)]
    [StrategyList(0)]
    public class StayOnStageStrategy : Strategy {
        Player me;
        float min, max;
        const float buffer = 2f;

        public StayOnStageStrategy(
            [StrategyParam(StrategyParamType.Me)] Player player,
            [StrategyParam(StrategyParamType.Ground)] GameObject ground
        ) {
            this.me = player;
            max = ground.GetComponent<BoxCollider2D>().bounds.extents.x - buffer;
            min = -max;
        }

        /// This strategy is not important at all when not near the edge,
        /// but most important when about to fall off.
        public override float advantage {
            get {
                if (me.transform.position.x < min || me.transform.position.x > max) {
                    return 1;
                }
                return -1;
            }
        }

        public override int GetControl() {
            if (me.transform.position.x < min) {
                return Control.Right;
            } else if (me.transform.position.x > max) {
                return Control.Left;
            }
            return Control.None;
        }
    }
}