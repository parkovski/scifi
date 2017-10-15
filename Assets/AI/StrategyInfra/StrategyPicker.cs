using UnityEngine;
using SciFi.Util;

namespace SciFi.AI.Strategies {
    public class StrategyPicker {
        Strategy[] strategies;

        public StrategyPicker(Strategy[] strategies) {
            this.strategies = strategies;
        }

        public void LogStrategies(DataLogger logger) {
            foreach (var s in strategies) {
                logger.AddProvider(s);
            }
        }

        public Strategy Pick() {
            if (strategies.Length == 0) {
                return null;
            }
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

            if (maxAdvantage == -1) {
                return null;
            }
            return strategies[index];
        }
    }
}