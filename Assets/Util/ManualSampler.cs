using System;
using UnityEngine;

namespace SciFi.Util {
    public class ManualSampler {
        float interval;
        float nextSampleTime;
        Action action;

        public ManualSampler(float interval, Action action, float delay = 0) {
            this.interval = interval;
            this.nextSampleTime = Time.time + delay;
            this.action = action;
        }

        public void Run() {
            var time = Time.time;

            if (time >= nextSampleTime) {
                nextSampleTime = time + interval;
                action();
            }
        }
    }
}