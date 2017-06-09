using UnityEngine;
using UnityEngine.Scripting;
using System;

using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.AI.Strategies {
    /// Fight strategy - run towards a player.
    [Preserve]
    [StrategyType(StrategyType.Movement)]
    [StrategyList(0)]
    public class FoFFightStrategy : Strategy {
        Player me;
        Player opponent;

        Func<StrategyType, Strategy> getActiveStrategy;

        bool reachedTarget;

        public FoFFightStrategy(
            [StrategyParam(StrategyParamType.Me)] Player me,
            [StrategyParam(StrategyParamType.Opponent)] Player opponent,
            [StrategyParam(StrategyParamType.GetActiveStrategy)] Func<StrategyType, Strategy> getActiveStrategy
        ) {
            this.me = me;
            this.opponent = opponent;
            this.getActiveStrategy = getActiveStrategy;
        }

        public override void OnActivate() {
            reachedTarget = false;
        }

        public override float advantage {
            get {
                return ((float)me.eDamage).ScaleClamped(0, 150, 1, -1);
            }
        }

        public override int GetControl() {
            var attackStrategy = (AttackStrategy)getActiveStrategy(StrategyType.Attack);
            float targetDistance, bufferDistance;
            attackStrategy.GetTargetDistanceRange(out targetDistance, out bufferDistance);

            if (reachedTarget) {
                if (Mathf.Abs(me.transform.position.x - opponent.transform.position.x) > targetDistance + bufferDistance) {
                    reachedTarget = false;
                    return Control.Left;
                } else if (Mathf.Abs(me.transform.position.x - opponent.transform.position.x) < targetDistance - bufferDistance) {
                    reachedTarget = false;
                    return Control.Right;
                } else {
                    return Control.None;
                }
            } else {
                if (me.transform.position.x < opponent.transform.position.x) {
                    if (opponent.transform.position.x - me.transform.position.x > targetDistance) {
                        reachedTarget = true;
                        return Control.None;
                    }
                    return Control.Right;
                } else {
                    if (me.transform.position.x - opponent.transform.position.x > targetDistance) {
                        reachedTarget = true;
                        return Control.None;
                    }
                    return Control.Left;
                }
            }
        }
    }
}