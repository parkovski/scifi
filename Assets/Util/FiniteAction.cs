using UnityEngine;
using System;
using System.Collections;

namespace SciFi.Util {
    /// An action with a definite end condition.
    /// Checks periodically whether the condition
    /// has been met so you can't forget to end it.
    public abstract class FiniteAction {
        bool active;
        Func<bool> shouldEnd;
        float checkInterval;
        MonoBehaviour coroutineRunner;
        Coroutine checkToEnd;

        public FiniteAction(MonoBehaviour coroutineRunner, float checkInterval, Func<bool> shouldEnd) {
            this.coroutineRunner = coroutineRunner;
            this.checkInterval = checkInterval;
            this.shouldEnd = shouldEnd;
        }

        protected abstract void OnStart();
        protected abstract void OnEnd();

        public void Start() {
            if (active) {
                return;
            }
            active = true;
            OnStart();
            checkToEnd = coroutineRunner.StartCoroutine(CheckToEnd(checkInterval));
        }

        public void End() {
            if (!active) {
                return;
            }
            active = false;
            coroutineRunner.StopCoroutine(checkToEnd);
            checkToEnd = null;
            OnEnd();
        }

        IEnumerator CheckToEnd(float checkInterval) {
            do {
                yield return new WaitForSeconds(checkInterval);
            } while (!shouldEnd());
            End();
        }
    }
}