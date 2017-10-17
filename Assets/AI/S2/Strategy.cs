using System;

namespace SciFi.AI.S2 {
    public abstract class Strategy {
        public int aiIndex { get; }
        public uint actionGroupMask { get; }

        private int activeControl;

        /// <param name="aiIndex">Provided by `GameController`.</param>
        /// <param name="actionGroupMask">
        ///   Only the lowest bit is considered currently.
        /// </param>
        public Strategy(int aiIndex, uint actionGroupMask) {
            this.aiIndex = aiIndex;
            this.actionGroupMask = actionGroupMask;
        }

        /// Must be thread safe.
        protected abstract float OnEvaluate(AIEnvironment env);
        /// Returns the desired control.
        protected abstract int OnExecute(AIEnvironment env);

        protected virtual void OnActivate(AIEnvironment env) {}
        protected virtual void OnDeactivate(AIEnvironment env) {}
        /// Must be thread safe.
        public virtual bool CanTransitionTo(Type type) => true;

        // --

        public float Evaluate(AIEnvironment env) {
            return UnityEngine.Mathf.Clamp01(OnEvaluate(env));
        }

        public void Execute(AIEnvironment env, AIInputManager inputManager) {
            var c = OnExecute(env);
            if (c < Control.None || c >= Control.ArrayLength) {
                c = Control.None;
            }
            if (c != activeControl) {
                inputManager.Release(activeControl);
            }
            activeControl = c;
            inputManager.Press(c);
        }

        public void Activate(AIEnvironment env) {
            this.activeControl = -1;
            OnActivate(env);
        }

        public void Deactivate(AIEnvironment env, AIInputManager inputManager) {
            if (activeControl != Control.None) {
                inputManager.Release(activeControl);
            }
            OnDeactivate(env);
        }
    }
}