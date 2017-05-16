using UnityEngine;

namespace SciFi.AI.Strategies {
    public class StrategyPicker<TStrategy> where TStrategy : Strategy {
        TStrategy[] strategies;

        public StrategyPicker(TStrategy[] strategies) {
            this.strategies = strategies;
        }

        public TStrategy Pick() {
            int index = 0;
            float maxAdvantage = strategies[0].advantage;
            if (maxAdvantage > 1 || maxAdvantage < -1) {
                Debug.LogWarning(string.Format(
                    "First strategy advantage out of range ({0:0.0##}/{1}).",
                    maxAdvantage, strategies[0].GetType().Name
                ));
                maxAdvantage = 0;
            }
            for (var i = 1; i < strategies.Length; i++) {
                var adv = strategies[i].advantage;
                if (adv > 1 || adv < -1) {
                    Debug.LogWarning(string.Format(
                        "Strategy advantage out of range ({0:0.0##}/{1}).",
                        adv, strategies[i].GetType().Name
                    ));
                    continue;
                }
                if (adv > maxAdvantage) {
                    maxAdvantage = adv;
                    index = i;
                }
            }

            return strategies[index];
        }
    }
}