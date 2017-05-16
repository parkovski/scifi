using SciFi.Players;

namespace SciFi.AI.Strategies {
    /// Fight strategy - run towards a player.
    public class FoFFightStrategy : Strategy {
        Player me;
        Player opponent;

        public FoFFightStrategy(Player me, Player opponent) {
            this.me = me;
            this.opponent = opponent;
        }

        public override float advantage {
            get {
                return 0f;
            }
        }

        public override int Step() {
            if (me.transform.position.x < opponent.transform.position.x) {
                return Control.Right;
            } else {
                return Control.Left;
            }
        }
    }
}