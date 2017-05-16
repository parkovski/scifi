namespace SciFi.AI.Strategies {
    /// Stand still strategy - do nothing.
    public class StandStillStrategy : Strategy {
        public StandStillStrategy() {
        }

        public override float advantage {
            get {
                return 0;
            }
        }

        public override int Step() {
            return Control.None;
        }
    }
}