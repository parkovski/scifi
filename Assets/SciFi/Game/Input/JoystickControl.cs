#if !UNITY_EDITOR
# if UNITY_IOS || UNITY_ANDROID
#  define TOUCH_ONLY
# endif
#endif

using UnityEngine;

namespace SciFi {
    /// Abstraction over the joystick controller to support both touch input
    /// and keyboard input.
    public struct JoystickControl {
#if !TOUCH_ONLY
        MultiPressControl leftButton;
        MultiPressControl rightButton;
#endif
        IInputManager inputManager;

        public JoystickControl(IInputManager inputManager) {
            this.inputManager = inputManager;
#if !TOUCH_ONLY
            // The InputManager is only passed to a human controlled player with
            // authority on this instance - otherwise you either get an
            // AIInputManager or NullInputManager.
            if (inputManager is InputManager) {
                leftButton = new MultiPressControl(inputManager, Control.Left, .3f);
                rightButton = new MultiPressControl(inputManager, Control.Right, .3f);
            } else {
                leftButton = null;
                rightButton = null;
            }
#endif
        }

        public float GetHorizontalAxis() {
#if !TOUCH_ONLY
            if (leftButton != null) {
                var axis = Input.GetAxis("Horizontal");
                int presses = 0;
                if (axis < -.5f) {
                    presses = leftButton.GetPresses();
                    axis = -.5f;
                } else if (axis > .5f) {
                    presses = rightButton.GetPresses();
                    axis = .5f;
                }
                if (presses == 1) {
                    return axis;
                } else if (presses > 1) {
                    return axis + axis;
                }
            }
#endif
            if (inputManager.IsControlActive(Control.Left)) {
                return -inputManager.GetControlAmount(Control.Left);
            } else if (inputManager.IsControlActive(Control.Right)) {
                return inputManager.GetControlAmount(Control.Right);
            }
            return 0;
        }

        public float GetVerticalAxis() {
            if (inputManager.IsControlActive(Control.Down)) {
                return -inputManager.GetControlAmount(Control.Down);
            } else if (inputManager.IsControlActive(Control.Up)) {
                return inputManager.GetControlAmount(Control.Up);
            }
            return 0;
        }

        /// Does nothing on platforms with no keyboard available.
        public void KeyboardUpdate() {
#if !TOUCH_ONLY
            if (leftButton != null) {
                leftButton.Update();
                rightButton.Update();
            }
#endif
        }
    }
}