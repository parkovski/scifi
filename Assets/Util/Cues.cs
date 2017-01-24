using UnityEngine;
using System;
using System.Collections.Generic;

namespace SciFi.Util {
    /// No System.Tuple in this old version of Mono :(.
    struct TimeCueTuple {
        public float time;
        public Action action;
    }

    /// A list of actions to be executed at certain times.
    public class Cues : MonoBehaviour {
        List<TimeCueTuple> cues;
        int index;
        float startTime;
        bool running;

        public Cues() {
            cues = new List<TimeCueTuple>();
        }

        void Start() {
            Reset();
        }

        void Update() {
            if (!running) {
                return;
            }
            if (index >= cues.Count) {
                running = false;
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

        /// Reset the timer and cue index.
        public void Reset() {
            index = 0;
            startTime = Time.time;
            running = false;
        }

        /// Stop executing cues.
        public void Pause() {
            running = false;
        }

        /// Start executing cues.
        public void Resume() {
            running = true;
        }
    }
}