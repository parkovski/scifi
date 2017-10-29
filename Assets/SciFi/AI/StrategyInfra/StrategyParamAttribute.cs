using System;

namespace SciFi.AI.Strategies {
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class StrategyParamAttribute : Attribute {
        public StrategyParamType type { get; private set; }

        public StrategyParamAttribute(StrategyParamType type) {
            this.type = type;
        }
    }
}