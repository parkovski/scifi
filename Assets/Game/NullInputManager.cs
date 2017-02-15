using UnityEngine;

namespace SciFi {
    public class NullInputManager : IInputManager {
        public bool IsControlActive(int control) {
            return false;
        }

        public float GetControlHoldTime(int control) {
            return 0;
        }

        public float GetControlAmount(int control) {
            return 0;
        }

        public void InvalidateControl(int control) {
        }

        public Vector2 GetMousePosition() {
            return Vector2.zero;
        }

        public event ControlCanceledHandler ControlCanceled;
        public event ObjectSelectedHandler ObjectSelected;
        public event ObjectSelectedHandler ObjectDeselected;
        public event TouchControlStateChangedHandler TouchControlStateChanged;

    }
}