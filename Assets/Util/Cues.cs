using UnityEngine;
using System;
using System.Collections.Generic;

namespace SciFi.Util {
    struct TimeCueTuple {
        public float time;
        public Action action;
    }

    public class Cues : MonoBehaviour {
        List<TimeCueTuple> cues;
        int index;
        float startTime;

        public Cues() {
            cues = new List<TimeCueTuple>();
            Reset();
        }

        void Update() {
            if (index >= cues.Count) {
                return;
            }

            if (Time.time > cues[index].time + startTime) {
                cues[index].action();
                ++index;
            }
        }

        /// Cues must be added in the order they are expected to run.
        public void Add(float time, Action action) {
            cues.Add(new TimeCueTuple { time = time, action = action });
        }

        public void Reset() {
            index = 0;
            startTime = Time.time;
        }
    }
}