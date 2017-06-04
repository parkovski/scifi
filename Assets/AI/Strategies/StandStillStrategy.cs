namespace SciFi.AI.Strategies {
    /// Stand still strategy - do nothing.
    [StrategyType(StrategyType.Movement)]
    [StrategyList(0)]
    public class StandStillStrategy : Strategy {
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