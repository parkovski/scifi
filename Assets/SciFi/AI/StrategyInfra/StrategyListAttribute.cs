using System;

namespace SciFi.AI.Strategies {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class StrategyListAttribute : Attribute {
        public int list { get; private set; }

        public StrategyListAttribute(int list) {
            this.list = list;
        }
    }
}