namespace SciFi.AI.Strategies {
    [StrategyType(StrategyType.Attack)]
    public abstract class AttackStrategy : Strategy {
        /// Target is the ideal distance for this strategy, buffer is the distance
        /// on either side that it is acceptable to be off by.
        public abstract void GetTargetDistanceRange(out float target, out float buffer);
    }
}