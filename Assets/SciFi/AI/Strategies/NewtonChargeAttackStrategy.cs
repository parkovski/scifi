using UnityEngine;
using UnityEngine.Scripting;

using SciFi.Players;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.AI.Strategies {
    [Preserve]
    [StrategyList(0)]
    public class NewtonChargeAttackStrategy : AttackStrategy {
        Player me;
        Player opponent;

        Rigidbody2D opponentRb;

        int control;

        const float beginChargingDistance = 5;
        const float endChargingDistance = 1.5f;
        const float bufferDistance = 1;

        public NewtonChargeAttackStrategy(
            [StrategyParam(StrategyParamType.Me)] Player me,
            [StrategyParam(StrategyParamType.Opponent)] Player opponent
        ) {
            this.me = me;
            this.opponent = opponent;
            this.opponentRb = opponent.GetComponent<Rigidbody2D>();

            control = Control.None;
        }

        public override void OnActivate() {
            control = Control.Attack2;
        }

        public override void OnDeactivate() {
            control = Control.None;
        }

        public override float advantage {
            get {
                if (opponent.IsFacing(me.gameObject) && Mathf.Abs(opponentRb.velocity.x) > 1) {
                    return 1;
                }
                return -1;
            }
        }
        
        static bool WithinDistance(Player p1, Player p2, float distance) {
            return Mathf.Abs(p1.transform.position.x - p2.transform.position.x) < distance;
        }

        public override int GetControl() {
            if (!WithinDistance(me, opponent, beginChargingDistance + bufferDistance)) {
                return Control.None;
            }
            if (WithinDistance(me, opponent, endChargingDistance)) {
                control = Control.None;
            }
            return control;
        }

        public override void GetTargetDistanceRange(out float target, out float buffer) {
            target = beginChargingDistance;
            buffer = bufferDistance;
        }
    }
}