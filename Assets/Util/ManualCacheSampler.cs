using System;
using System.Collections.Generic;
using UnityEngine;

namespace SciFi.Util {
    /// Only does the action if the new value is different
    /// from the old value.
    public class ManualCacheSampler<T> {
        IEqualityComparer<T> comparer;
        float interval;
        float nextSampleTime;
        Action action;
        T currentValue;

        public ManualCacheSampler(IEqualityComparer<T> comparer, float interval, Action action, float delay = 0) {
            this.comparer = comparer;
            this.interval = interval;
            this.nextSampleTime = Time.time + delay;
            this.action = action;
        }

        public ManualCacheSampler(float interval, Action action, float delay = 0)
            : this(EqualityComparer<T>.Default, interval, action, delay)
        {
        }

        public void Run(T newValue) {
            var time = Time.time;

            if (time < nextSampleTime) {
                return;
            }

            if (comparer.Equals(currentValue, newValue)) {
                return;
            }

            currentValue = newValue;
            nextSampleTime = time + interval;
            action();
        }
    }
}