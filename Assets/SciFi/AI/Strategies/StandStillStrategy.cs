using UnityEngine.Scripting;

namespace SciFi.AI.Strategies {
    /// Stand still strategy - do nothing.
    [Preserve]
    [StrategyType(StrategyType.Movement)]
    [StrategyList(0)]
    public class StandStillStrategy : Strategy {
        public override float advantage {
            get {
                return 0;
            }
        }

        public override int GetControl() {
            return Control.None;
        }
    }
}