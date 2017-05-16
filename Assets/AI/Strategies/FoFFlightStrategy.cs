using UnityEngine;

using SciFi.Players;
using SciFi.Util.Extensions;

namespace SciFi.AI.Strategies {
    /// Flight strategy - run away from a player.
    public class FoFFlightStrategy : Strategy {
        Player me;
        Player opponent;

        const float targetDistance = 5f;

        public FoFFlightStrategy(Player me, Player opponent) {
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

        public override int Step() {
            if (me.transform.position.x < opponent.transform.position.x) {
                return Control.Left;
            } else {
                return Control.Right;
            }
        }
    }
}