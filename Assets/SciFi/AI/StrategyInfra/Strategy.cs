using SciFi.Util;

namespace SciFi.AI.Strategies {
    public abstract class Strategy: IDataPointProvider {
        public Strategy() {
            this.tag = GetType().Name;
        }

        /// The estimated advantage to using this strategy.
        /// From -1 to 1, where -1 means absolutely do not use this,
        /// 1 means absolutely crucial to use this, and 0 means
        /// there is no benefit or detriment.
        public abstract float advantage { get; }

        /// Returns the control that the strategy wants to press.
        /// Strategies are grouped into sets, so multiple controls
        /// can be active at the same time, but only one per set.
        public abstract int GetControl();

        /// Returns the axis amount for a variable axis control.
        /// For on-off controls, this does not need to be overridden.
        public virtual float GetAxisAmount() {
            return 1;
        }

        /// Called when this strategy becomes active.
        public virtual void OnActivate() {}

        /// Called when this strategy becomes inactive.
        public virtual void OnDeactivate() {}

        // IDataPointProvider
        public string tag { get; }
        public string GetLogValue() => advantage.ToString();
    }
}