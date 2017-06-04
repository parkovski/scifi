using System;

namespace SciFi.AI.Strategies {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class StrategyTypeAttribute : Attribute {
        public StrategyType type { get; private set; }

        public StrategyTypeAttribute(StrategyType type) {
            this.type = type;
        }
    }
}