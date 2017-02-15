using UnityEngine;

namespace SciFi.AI {
    struct AIButtonState {
        public bool isPressed;
        public float axisAmount;
        public float startHoldTime;
    }

    public class AIInputManager : IInputManager {
        AIButtonState[] state;

        public AIInputManager() {
            state = new AIButtonState[Control.ArrayLength];
        }

        public void Press(int control) {
            state[control].isPressed = true;
            state[control].axisAmount = 1f;
            state[control].startHoldTime = Time.time;
        }

        public void Release(int control) {
            state[control].isPressed = false;
        }

        public bool IsControlActive(int control) {
            return state[control].isPressed;
        }

        public float GetControlHoldTime(int control) {
            return Time.time - state[control].startHoldTime;
        }

        public float GetControlAmount(int control) {
            return state[control].axisAmount;
        }

        public void InvalidateControl(int control) {
            state[control].isPressed = false;
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