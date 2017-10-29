#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#    define INPUT_TOUCHONLY
#else
#    define INPUT_MIXED
#endif

using UnityEngine;
using System.Collections.Generic;

using SciFi.Util.Extensions;

// Types of input we need to handle:
// - Immediate button down/button up response.
// - On button up, we need to know how long it was held down.
// - A button is pressed and then the finger slides to another button.
// - Two buttons are pressed "at the same time" (within a small amount of time).
//   - An event should include the single button that was previously pressed so
//     that event can be canceled.

// Since we support both touch and traditional input,
// we store each in a separate flag. If one is set when the other is released,
// we should not reset this button.

namespace SciFi {
    /// States for each control, including touch and keyboard input.
    struct ButtonState {
        public bool isPressed;
        public bool isTouched;
        public bool isInvalidated;
        public float axisAmount;
        public float timeHeld;
    }

    /// Control IDs.
    public static class Control {
        public const int None = -1;
        public const int Left = 0;
        public const int Right = 1;
        public const int Up = 2;
        public const int Down = 3;
        public const int Attack1 = 4;
        public const int Attack2 = 5;
        public const int Attack3 = 6;
        public const int SpecialAttack = 7;
        public const int Item = 8;
        /// Not used in game - player may set a mouse button
        /// as one of the other controls - these are used
        /// only in menus.
        public const int MouseButton1 = 9;
        /// Not used in game - player may set a mouse button
        /// as one of the other controls - these are used
        /// only in menus.
        public const int MouseButton2 = 10;
        public const int DodgeLeft = 11;
        public const int DodgeRight = 12;
        public const int Jump = 13;
        public const int Block = 14;
        /// The total number of controls.
        public const int ArrayLength = 15;
    }

    /// Updates state based on input events.
    class InputState {
        public ButtonState[] states;
        public Vector2 mousePosition;

        public InputState() {
            states = new ButtonState[Control.ArrayLength];
            for (var i = 0; i < states.Length; i++) {
                states[i] = new ButtonState();
                ForceReset(i);
            }
        }

        /// Marks the control as invalidated and not active.
        public void Invalidate(int button) {
            // Only invalidate controls that are active.
            if (!states[button].isPressed & !states[button].isTouched) {
                return;
            }
            states[button].isInvalidated = true;
            states[button].isPressed = false;
            states[button].isTouched = false;
            states[button].axisAmount = 0f;
        }

        /// Reset all state for a control.
        public void ForceReset(int button) {
            states[button].axisAmount = 0f;
            states[button].isPressed = false;
            states[button].isTouched = false;
            states[button].isInvalidated = false;
        }

        /// Reset keyboard state for a control.
        public void Reset(int button) {
            states[button].isPressed = false;
            if (states[button].isTouched) {
                return;
            }
            states[button].axisAmount = 0f;
            states[button].isInvalidated = false;
        }

        /// Reset touch state for a control.
        public void TouchReset(int button) {
            states[button].isTouched = false;
            if (states[button].isPressed) {
                return;
            }
            states[button].axisAmount = 0f;
            states[button].isInvalidated = false;
        }

        /// Set/update the button's press state.
        public void ButtonPress(int button, float amount) {
            PressOrTouch(button, amount, true);
        }

        /// Set/update the button's touch state.
        public void ButtonTouch(int button, float amount) {
            PressOrTouch(button, amount, false);
        }

        /// Update the button's press or touch state and set it to active.
        public void PressOrTouch(int button, float amount, bool press) {
            if (states[button].isPressed || states[button].isTouched) {
                states[button].timeHeld += Time.deltaTime;
            } else {
                states[button].timeHeld = 0f;
            }
            if (press) {
                states[button].isPressed = true;
            } else {
                states[button].isTouched = true;
            }
            states[button].axisAmount = amount;
        }

        /// Update keyboard state for <c>activeAxis</c>, setting <c>inactiveAxis</c> to 0.
        public void UpdateAxis(int activeAxis, int inactiveAxis, float amount) {
            UpdateButton(activeAxis, true, amount);
            ForceReset(inactiveAxis);
        }

        /// Update keyboard state for <c>button</c> with axis value <c>amount</c>.
        public void UpdateButton(int button, bool active, float amount) {
            if (active) {
                if (states[button].isInvalidated) {
                    return;
                }

                ButtonPress(button, amount);
            } else {
                Reset(button);
            }
        }

        /// Update keyboard state for <c>button</c> with full axis value.
        public void UpdateButton(int button, bool active) {
            UpdateButton(button, active, 1f);
        }

        /// Update touch state for <c>activeAxis</c>, setting <c>inactiveAxis</c> to 0.
        public void TouchUpdateAxis(int activeAxis, int inactiveAxis, float amount) {
            ForceReset(inactiveAxis);
            TouchUpdateButton(activeAxis, true, amount);
        }

        /// Update touch state for <c>button</c>.
        public void TouchUpdateButton(int button, bool active, float amount) {
            if (active) {
                if (states[button].isInvalidated) {
                    return;
                }

                ButtonTouch(button, amount);
            } else {
                Reset(button);
            }
        }

        /// Update touch state for <c>button</c> with full axis value.
        public void TouchUpdateButton(int button, bool active) {
            TouchUpdateButton(button, active, 1f);
        }
    }

    /// Event listener for a control canceled event.
    public delegate void ControlCanceledHandler(int control);

    /// Event listener for an object selected event, fired when the user
    /// touches or clicks an object on the screen.
    public delegate void ObjectSelectedHandler(GameObject gameObject);

    /// Event listener for a touch control state change event -
    /// press or release. For more in depth handling, poll from <see cref="InputManager" />.
    public delegate void TouchControlStateChangedHandler(string control, bool active);

    public interface IInputManager {
        bool IsControlActive(int control);
        float GetControlHoldTime(int control);
        float GetControlAmount(int control);
        void InvalidateControl(int control);
        Vector2 GetMousePosition();

        event ControlCanceledHandler ControlCanceled;
        event ObjectSelectedHandler ObjectSelected;
        event ObjectSelectedHandler ObjectDeselected;
        event TouchControlStateChangedHandler TouchControlStateChanged;
    }

    /// Intermediate layer between Unity's input and game handling.
    /// This merges touch and keyboard/mouse input and provides
    /// input invalidation.
    public class InputManager : MonoBehaviour, IInputManager {
        /// Touch control layer - <see cref="SciFi.Layers" /> may not be initialized yet.
        int touchControlLayerId;
        /// Layers that we want to be able to select with touch/click.
        int layerMask;
        /// Underlying state for controls.
        InputState state = new InputState();
        /// Maps from finger ID to control name.
        Dictionary<int, string> activeTouches;
        /// The first button in a combo, if a combo is active.
        string firstComboButton;
        /// The second button in a combo, if a combo is active.
        string secondComboButton;
        Vector2 joystickTouchOffset;
        GameObject joystickInner;
        GameObject joystickOuter;
        float joystickInnerRadius;
        float joystickOuterRadius;
        const float joystickDeadZonePercent = 0.15f;

        /// Is <c>control</c> currently pressed or touched?
        public bool IsControlActive(int control) {
            var s = state.states;
            return s[control].isPressed || s[control].isTouched;
        }

        /// How long has <c>control</c> been active?
        public float GetControlHoldTime(int control) {
            return state.states[control].timeHeld;
        }

        /// What is <c>control</c>'s axis value?
        public float GetControlAmount(int control) {
            return state.states[control].axisAmount;
        }

        /// Mark the control as invalidated. After this is called,
        /// it will be reported as inactive until it is released
        /// and pressed again.
        public void InvalidateControl(int control) {
            state.Invalidate(control);
        }

        /// Where is the mouse on the screen?
        public Vector2 GetMousePosition() {
            return state.mousePosition;
        }

        public event ControlCanceledHandler ControlCanceled;
        public event ObjectSelectedHandler ObjectSelected;
        public event ObjectSelectedHandler ObjectDeselected;
        public event TouchControlStateChangedHandler TouchControlStateChanged;

        /// Initialize state.
        /// If touch is not supported, destroy the touch controls.
        void Start() {
            // The Layers class is not initialized yet
            layerMask
                = 1 << LayerMask.NameToLayer("Touch Controls")
                | 1 << LayerMask.NameToLayer("Items");
            activeTouches = new Dictionary<int, string>();

#if !UNITY_EDITOR
            if (!Input.touchSupported) {
                Destroy(GameObject.Find("JoyStickOuter"));
                Destroy(GameObject.Find("JoyStickInner"));
                Destroy(GameObject.Find("ItemButton"));
                Destroy(GameObject.Find("AttackButton1"));
                Destroy(GameObject.Find("AttackButton2"));
                Destroy(GameObject.Find("AttackButton3"));
                Destroy(GameObject.Find("JumpButton"));
                Destroy(GameObject.Find("ShieldButton"));
            }
#endif
        }

        /// Update keyboard and mouse state.
        void CheckUnityInput() {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            var attack1 = Input.GetButton("Fire1");
            var attack2 = Input.GetButton("Fire2");
            var attack3 = Input.GetButton("Fire3");
            var item = Input.GetButton("Item");
            var mouse1 = Input.GetKey("mouse 0");
            var mouse2 = Input.GetKey("mouse 1");

            if (horizontal > 0f) {
                state.UpdateAxis(Control.Right, Control.Left, horizontal);
            } else if (horizontal < 0f) {
                state.UpdateAxis(Control.Left, Control.Right, -horizontal);
            } else {
                state.Reset(Control.Right);
                state.Reset(Control.Left);
            }

            if (vertical > 0f) {
                state.UpdateButton(Control.Jump, true, 1f);
                state.Reset(Control.Block);
                state.UpdateAxis(Control.Up, Control.Down, vertical);
            } else if (vertical < 0f) {
                state.UpdateButton(Control.Block, true, 1f);
                state.Reset(Control.Jump);
                state.UpdateAxis(Control.Down, Control.Up, -vertical);
            } else {
                state.Reset(Control.Jump);
                state.Reset(Control.Block);
                state.Reset(Control.Up);
                state.Reset(Control.Down);
            }

            state.UpdateButton(Control.Attack1, attack1);
            state.UpdateButton(Control.Attack2, attack2);
            state.UpdateButton(Control.Attack3, attack3);
            state.UpdateButton(Control.Item, item);

            state.mousePosition = Input.mousePosition;
            UpdateMouse(Control.MouseButton1, mouse1);
            UpdateMouse(Control.MouseButton2, mouse2);
        }

        /// Update mouse button state, and if an object was selected
        /// fire the ObjectSelected event.
        void UpdateMouse(int control, bool active) {
            // If the mouse button was just pressed,
            // we may need to fire an object selected message.
            GameObject selectedObject = null;
            if (active && !state.states[control].isPressed) {
                selectedObject = GetObjectAtPosition(state.mousePosition);
            }
            if (!active && state.states[control].isPressed) {
                selectedObject = GetObjectAtPosition(state.mousePosition);
            }
            state.UpdateButton(control, active);
            if (selectedObject != null) {
                if (active && ObjectSelected != null) {
                    ObjectSelected(selectedObject);
                } else if (!active && ObjectDeselected != null) {
                    ObjectDeselected(selectedObject);
                }
            }
        }

        /// Get the GameObject at the given position, or null if there is none.
        /// Only objects in <see cref="InputManager.layerMask" /> will be detected.
        GameObject GetObjectAtPosition(Vector2 position) {
            var ray = Camera.main.ScreenToWorldPoint(position);
            var hit = Physics2D.Raycast(ray, Vector2.zero, Mathf.Infinity, layerMask);
            if (!hit) {
                return null;
            }
            return hit.collider.gameObject;
        }

        /// Convert a touch control name to an ID from the
        /// <see cref="Control" /> class.
        int GetTouchControl(string controlName) {
            switch (controlName) {
            case "JumpButton":
                return Control.Jump;
            case "ShieldButton":
                return Control.Block;
            case "AttackButton1":
                return Control.Attack1;
            case "AttackButton2":
                return Control.Attack2;
            case "AttackButton3":
                return Control.Attack3;
            case "DodgeLeftButton": // Fake button for combo
                return Control.DodgeLeft;
            case "DodgeRightButton":
                return Control.DodgeRight;
            case "ItemButton":
                return Control.Item;
            default:
                return -1;
            }
        }

        /// Update <c>control</c> setting its touch value to true.
        void BeginTouch(int control) {
            state.TouchUpdateButton(control, true);
        }

        /// Update <c>control</c> setting its touch value to false.
        void EndTouch(int control) {
            state.TouchReset(control);
        }

        /// Update the time the control has been held.
        void UpdateTouchTime(int control) {
            state.states[control].timeHeld += Time.deltaTime;
        }

        /// Returns the pseudo control represented by
        /// the combination of pressing <c>first</c> and <c>second</c>.
        int GetTouchCombo(int first, int second) {
            if (first > second) {
                var tmp = first;
                first = second;
                second = tmp;
            }

            if (first == Control.Left && second == Control.Down) {
                return Control.DodgeLeft;
            }
            if (first == Control.Right && second == Control.Down) {
                return Control.DodgeRight;
            }
            return -1;
        }

        /// Convert a combo pseudo control to a name -
        /// the name does not identify an object on the screen.
        string GetComboName(int combo) {
            if (combo == Control.DodgeLeft) {
                return "DodgeLeftButton";
            }
            if (combo == Control.DodgeRight) {
                return "DodgeRightButton";
            }
            return "";
        }

        /// Is <c>control</c> a combo? (Not a screen control).
        bool IsCombo(int control) {
            if (control == Control.DodgeLeft) {
                return true;
            }
            if (control == Control.DodgeRight) {
                return true;
            }
            return false;
        }

        Vector3 CircleClamp(Vector3 point, Vector3 center, float radius) {
            var dx = point.x - center.x;
            var dy = point.y - center.y;
            var xsq = dx * dx;
            var ysq = dy * dy;
            if (Mathf.Sqrt(xsq + ysq) > radius) {
                var angle = Mathf.Atan2(dy, dx);
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                return new Vector3(
                    center.x + x,
                    center.y + y,
                    center.z
                );
            }
            point.z = center.z;
            return point;
        }

        void GetJoystickInputPercent(out float x, out float y) {
            var inner = joystickInner.transform.position;
            var outer = joystickOuter.transform.position;
            var dx = inner.x - outer.x;
            var dy = inner.y - outer.y;
            var angle = Mathf.Atan2(dy, dx);
            var unitX = Mathf.Cos(angle);
            var unitY = Mathf.Sin(angle);
            var maxX = unitX * joystickOuterRadius;
            var maxY = unitY * joystickOuterRadius;
            x = dx / maxX;
            y = dy / maxY;
            if (dx < 0) {
                x = -x;
            }
            if (dy < 0) {
                y = -y;
            }
        }

        void JoystickInput(Touch touch) {
            if (joystickInner == null) {
                joystickInner = GameObject.Find("JoyStickInner");
                joystickOuter = GameObject.Find("JoyStickOuter");
                joystickInnerRadius = joystickInner.GetComponent<SpriteRenderer>().bounds.extents.x;
                joystickOuterRadius
                    = joystickOuter.GetComponent<SpriteRenderer>().bounds.extents.x
                    - joystickInnerRadius * 0.25f;
            }
            switch (touch.phase) {
            case TouchPhase.Began:
                joystickTouchOffset = Camera.main.ScreenToWorldPoint(touch.position) - joystickInner.transform.position;
                break;
            case TouchPhase.Moved:
                joystickInner.transform.position = CircleClamp(
                    Camera.main.ScreenToWorldPoint(touch.position) - (Vector3)joystickTouchOffset,
                    joystickOuter.transform.position,
                    joystickOuterRadius
                );
                float x, y;
                GetJoystickInputPercent(out x, out y);
                if (x < -joystickDeadZonePercent) {
                    state.TouchUpdateAxis(Control.Left, Control.Right, (-x).Scale(joystickDeadZonePercent, 1, 0, 1));
                } else if (x > joystickDeadZonePercent) {
                    state.TouchUpdateAxis(Control.Right, Control.Left, x.Scale(joystickDeadZonePercent, 1, 0, 1));
                } else {
                    state.TouchReset(Control.Left);
                    state.TouchReset(Control.Right);
                }
                if (y < -joystickDeadZonePercent) {
                    state.TouchUpdateAxis(Control.Down, Control.Up, (-y).Scale(joystickDeadZonePercent, 1, 0, 1));
                } else if (y > joystickDeadZonePercent) {
                    state.TouchUpdateAxis(Control.Up, Control.Down, y.Scale(joystickDeadZonePercent, 1, 0, 1));
                } else {
                    state.TouchReset(Control.Up);
                    state.TouchReset(Control.Down);
                }
                break;
            case TouchPhase.Stationary:
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                state.TouchReset(Control.Left);
                state.TouchReset(Control.Right);
                state.TouchReset(Control.Up);
                state.TouchReset(Control.Down);
                joystickInner.transform.position = joystickOuter.transform.position;
                break;
            }
        }

        void TouchBegan(Touch touch) {
            var obj = GetObjectAtPosition(touch.position);
            var controlName = obj == null ? null : obj.name;
            if (controlName == null) {
                return;
            }
            if (controlName == "JoyStickInner") {
                activeTouches.Add(touch.fingerId, controlName);
                JoystickInput(touch);
                return;
            } else if (controlName == "JoyStickOuter") {
                controlName = "JoyStickInner";
                activeTouches.Add(touch.fingerId, controlName);
                var position = touch.position;
                touch.position //= Camera.main.WorldToScreenPoint(joystickInner.transform.position);
                    = Camera.main.WorldToScreenPoint(CircleClamp(
                        Camera.main.ScreenToWorldPoint(touch.position),
                        joystickOuter.transform.position,
                        joystickInnerRadius * (joystickDeadZonePercent + 0.1f)
                    ));
                JoystickInput(touch);
                touch.position = position;
                touch.phase = TouchPhase.Moved;
                JoystickInput(touch);
                return;
            }
            var control = GetTouchControl(controlName);
            if (control == -1) {
                if (ObjectSelected != null) {
                    ObjectSelected(obj);
                }
                return;
            }
            BeginTouch(control);
            activeTouches.Add(touch.fingerId, controlName);
            if (TouchControlStateChanged != null) {
                TouchControlStateChanged(controlName, true);
            }
        }

        void TouchMoved(Touch touch) {
            string currentControlName;
            if (!activeTouches.TryGetValue(touch.fingerId, out currentControlName)) {
                return;
            }
            if (currentControlName == "JoyStickInner") {
                JoystickInput(touch);
                return;
            }
            var currentControl = GetTouchControl(currentControlName);
            UpdateTouchTime(currentControl);
            var newObj = GetObjectAtPosition(touch.position);
            var newControlName = newObj == null ? null : newObj.name;
            if (newControlName == null) {
                return;
            }
            var newControl = GetTouchControl(newControlName);
            if (newControl == -1) {
                return;
            }
            var combo = GetTouchCombo(currentControl, newControl);
            if (combo != -1) {
                InvalidateControl(currentControl);
                EndTouch(currentControl);
                ControlCanceled(currentControl);
                firstComboButton = currentControlName;
                secondComboButton = newControlName;
                activeTouches[touch.fingerId] = GetComboName(combo);
                BeginTouch(combo);
                if (TouchControlStateChanged != null) {
                    TouchControlStateChanged(newControlName, true);
                }
            }
        }

        void TouchStationary(Touch touch) {
            string control;
            if (activeTouches.TryGetValue(touch.fingerId, out control)) {
                if (control == "JoyStickInner") {
                    JoystickInput(touch);
                    return;
                }
                UpdateTouchTime(GetTouchControl(control));
            }
        }

        void TouchEnded(Touch touch) {
            string control;
            if (activeTouches.TryGetValue(touch.fingerId, out control)) {
                if (control == "JoyStickInner") {
                    JoystickInput(touch);
                    activeTouches.Remove(touch.fingerId);
                    return;
                }
                var controlValue = GetTouchControl(control);
                EndTouch(controlValue);
                activeTouches.Remove(touch.fingerId);
                if (TouchControlStateChanged != null) {
                    if (IsCombo(controlValue)) {
                        TouchControlStateChanged(firstComboButton, false);
                        TouchControlStateChanged(secondComboButton, false);
                    } else {
                        TouchControlStateChanged(control, false);
                    }
                }
            }
        }

        void TouchCanceled(Touch touch) {
            TouchEnded(touch);
        }

        /// Handle touch input and update state.
        void CheckTouchInput() {
            if (Input.touchCount == 0) {
                return;
            }

            foreach (var touch in Input.touches) {
                if (touch.phase == TouchPhase.Began) {
                    TouchBegan(touch);
                } else if (touch.phase == TouchPhase.Moved) {
                    TouchMoved(touch);
                } else if (touch.phase == TouchPhase.Stationary) {
                    TouchStationary(touch);
                } else if (touch.phase == TouchPhase.Ended) {
                    TouchEnded(touch);
                } else if (touch.phase == TouchPhase.Canceled) {
                    TouchCanceled(touch);
                }
            }
        }

        /// Update input state.
        void Update() {
#if INPUT_TOUCHONLY
            CheckTouchInput();
#else
            CheckUnityInput();
            if (Input.touchSupported) {
                CheckTouchInput();
            }
#endif
        }
    }
}