using UnityEngine;

namespace SciFi {
    /// Tracks a control that can be pressed multiple times 
    /// in a small amount of time.
    public class MultiPressControl {
        IInputManager inputManager;
        int control;
        float timeout;
        float lastPressTime;
        bool active;
        int presses;

        /// <param name="inputManager">The InputManager to poll for control values</param>
        /// <param name="control">The control to track</param>
        /// <param name="timeout">How long to wait for another press</param>
        public MultiPressControl(IInputManager inputManager, int control, float timeout) {
            this.inputManager = inputManager;
            this.control = control;
            this.timeout = timeout;
        }

        /// Update multi-press state.
        public void Update() {
            if (inputManager.IsControlActive(control)) {
                if (!active) {
                    active = true;
                    lastPressTime = Time.realtimeSinceStartup;
                    if (Time.realtimeSinceStartup < lastPressTime + timeout) {
                        ++presses;
                    }
                }
            } else {
                active = false;
                if (Time.realtimeSinceStartup > lastPressTime + timeout) {
                    presses = 0;
                }
            }
        }

        /// Is the control currently down?
        /// This may be false even though a multi-press is in progress.
        public bool IsActive() {
            return active;
        }

        /// How many times was this control pressed?
        /// If this is positive, the control may not be currently pressed,
        /// but the timeout hasn't elapsed yet.
        public int GetPresses() {
            return presses;
        }
    }
}