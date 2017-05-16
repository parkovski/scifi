namespace SciFi.AI.Strategies {
    public abstract class VariableAxisStrategy : Strategy {
        /// Called after Step, returns the desired axis amount
        /// for the control returned in Step.
        public abstract float StepAxisAmount();
    }
}