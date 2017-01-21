using UnityEngine;
using System;
using System.Collections.Generic;

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
    struct ButtonState {
        public bool isPressed;
        public bool isTouched;
        public bool isInvalidated;
        public float axisAmount;
        public float timeHeld;
    }

    public static class Control {
        public const int Left = 0;
        public const int Right = 1;
        public const int Up = 2;
        public const int Down = 3;
        public const int Attack1 = 4;
        public const int Attack2 = 5;
        public const int SpecialAttack = 6;
        public const int SuperAttack = 7;
        public const int Item = 8;
        /// Not used in game - player may set a mouse button
        /// as one of the other controls - these are used
        /// only in menus.
        public const int MouseButton1 = 9;
        public const int MouseButton2 = 10;
        public const int DodgeLeft = 11;
        public const int DodgeRight = 12;
        public const int ArrayLength = 13;
    }

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

        public void ForceReset(int button) {
            states[button].axisAmount = 0f;
            states[button].isPressed = false;
            states[button].isTouched = false;
            states[button].isInvalidated = false;
        }

        public void Reset(int button) {
            states[button].isPressed = false;
            if (states[button].isTouched) {
                return;
            }
            states[button].axisAmount = 0f;
            states[button].isInvalidated = false;
        }

        public void TouchReset(int button) {
            states[button].isTouched = false;
            if (states[button].isPressed) {
                return;
            }
            states[button].axisAmount = 0f;
            states[button].isInvalidated = false;
        }

        public void ButtonPress(int button, float amount) {
            PressOrTouch(button, amount, true);
        }

        public void ButtonTouch(int button, float amount) {
            PressOrTouch(button, amount, false);
        }

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

        public void UpdateAxis(int activeAxis, int inactiveAxis, float amount) {
            UpdateButton(activeAxis, true, amount);
            ForceReset(inactiveAxis);
        }

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

        public void UpdateButton(int button, bool active) {
            UpdateButton(button, active, 1f);
        }

        public void TouchUpdateAxis(int activeAxis, int inactiveAxis, float amount) {
            // On touch, you can press both buttons at the same time, unlike with Input.GetAxis.
            // If the opposite axis is already active, do what Unity does and ignore the most recent one.
            if (states[inactiveAxis].isTouched || states[inactiveAxis].isPressed) {
                ForceReset(activeAxis);
                return;
            } else {
                TouchUpdateButton(activeAxis, true, amount);
            }
            ForceReset(inactiveAxis);
        }

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

        public void TouchUpdateButton(int button, bool active) {
            TouchUpdateButton(button, active, 1f);
        }
    }

    public class ControlCanceledEventArgs : EventArgs {
        public int Control { get; private set; }

        public ControlCanceledEventArgs(int control) {
            this.Control = control;
        }
    }
    public delegate void ControlCanceledHandler(ControlCanceledEventArgs args);

    public class ObjectSelectedEventArgs : EventArgs {
        public GameObject gameObject;

        public ObjectSelectedEventArgs(GameObject gameObject) {
            this.gameObject = gameObject;
        }
    }
    public delegate void ObjectSelectedHandler(ObjectSelectedEventArgs args);

    public delegate void TouchControlStateChangedHandler(string control, bool active);

    public class InputManager : MonoBehaviour {
        int touchControlLayerId;
        // Layers that we want to be able to select with touch/click.
        int layerMask;
        InputState state = new InputState();
        // Maps from finger ID to Control.
        Dictionary<int, string> activeTouches;
        string firstComboButton;
        string secondComboButton;

        public bool IsControlActive(int control) {
            var s = state.states;
            return s[control].isPressed || s[control].isTouched;
        }

        public float GetControlHoldTime(int control) {
            return state.states[control].timeHeld;
        }

        public float GetControlAmount(int control) {
            return state.states[control].axisAmount;
        }

        public void InvalidateControl(int control) {
            state.Invalidate(control);
        }

        public Vector2 GetMousePosition() {
            return state.mousePosition;
        }

        public event ControlCanceledHandler ControlCanceled;
        public event ObjectSelectedHandler ObjectSelected;
        public event TouchControlStateChangedHandler TouchControlStateChanged;

        void Start() {
            // The Layers class is not initialized yet
            layerMask
                = 1 << LayerMask.NameToLayer("Touch Controls")
                | 1 << LayerMask.NameToLayer("Items");
            activeTouches = new Dictionary<int, string>();

            if (!Input.touchSupported) {
                Destroy(GameObject.Find("LeftButton"));
                Destroy(GameObject.Find("RightButton"));
                Destroy(GameObject.Find("UpButton"));
                Destroy(GameObject.Find("UpButton2"));
                Destroy(GameObject.Find("DownButton"));
                Destroy(GameObject.Find("AttackButton1"));
                Destroy(GameObject.Find("AttackButton2"));
                Destroy(GameObject.Find("ItemButton"));
            }
        }

        void CheckUnityInput() {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            var attack1 = Input.GetButton("Fire1");
            var attack2 = Input.GetButton("Fire2");
            var specialAttack = Input.GetButton("Fire3");
            var item = Input.GetKey("return");
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
                state.UpdateAxis(Control.Up, Control.Down, vertical);
            } else if (vertical < 0f) {
                state.UpdateAxis(Control.Down, Control.Up, -vertical);
            } else {
                state.Reset(Control.Up);
                state.Reset(Control.Down);
            }

            state.UpdateButton(Control.Attack1, attack1);
            state.UpdateButton(Control.Attack2, attack2);
            state.UpdateButton(Control.SpecialAttack, specialAttack);
            state.UpdateButton(Control.Item, item);

            state.mousePosition = Input.mousePosition;
            UpdateMouse(Control.MouseButton1, mouse1);
            UpdateMouse(Control.MouseButton2, mouse2);
        }

        void UpdateMouse(int control, bool active) {
            // If the mouse button was just pressed,
            // we may need to fire an object selected message.
            GameObject selectedObject = null;
            if (active && !state.states[control].isPressed) {
                selectedObject = GetObjectAtPosition(state.mousePosition);
            }
            state.UpdateButton(control, active);
            if (selectedObject != null) {
                ObjectSelected(new ObjectSelectedEventArgs(selectedObject));
            }
        }

        GameObject GetObjectAtPosition(Vector2 position) {
            var ray = Camera.main.ScreenToWorldPoint(position);
            var hit = Physics2D.Raycast(ray, Vector2.zero, Mathf.Infinity, layerMask);
            if (!hit) {
                return null;
            }
            return hit.collider.gameObject;
        }

        int GetTouchControl(string controlName) {
            switch (controlName) {
            case "LeftButton":
                return Control.Left;
            case "RightButton":
                return Control.Right;
            case "UpButton":
            case "UpButton2":
                return Control.Up;
            case "DownButton":
                return Control.Down;
            case "AttackButton1":
                return Control.Attack1;
            case "AttackButton2":
                return Control.Attack2;
            case "SpecialAttackButton": // Fake button for combo
                return Control.SpecialAttack;
            case "DodgeLeftButton":
                return Control.DodgeLeft;
            case "DodgeRightButton":
                return Control.DodgeRight;
            case "ItemButton":
                return Control.Item;
            default:
                return -1;
            }
        }

        void BeginTouch(int control) {
            switch (control) {
            case Control.Left:
                state.TouchUpdateAxis(Control.Left, Control.Right, 1f);
                break;
            case Control.Right:
                state.TouchUpdateAxis(Control.Right, Control.Left, 1f);
                break;
            case Control.Up:
                state.TouchUpdateAxis(Control.Up, Control.Down, 1f);
                break;
            case Control.Down:
                state.TouchUpdateAxis(Control.Down, Control.Up, 1f);
                break;
            case Control.Attack1:
                state.TouchUpdateButton(Control.Attack1, true);
                break;
            case Control.Attack2:
                state.TouchUpdateButton(Control.Attack2, true);
                break;
            case Control.Item:
                state.TouchUpdateButton(Control.Item, true);
                break;
            case Control.SpecialAttack:
                state.TouchUpdateButton(Control.SpecialAttack, true);
                break;
            case Control.DodgeLeft:
                state.TouchUpdateButton(Control.DodgeLeft, true);
                break;
            case Control.DodgeRight:
                state.TouchUpdateButton(Control.DodgeRight, true);
                break;
            }
        }

        void EndTouch(int control) {
            switch (control) {
            case Control.Left:
            case Control.Right:
                state.TouchReset(Control.Left);
                state.TouchReset(Control.Right);
                break;
            case Control.Up:
            case Control.Down:
                state.TouchReset(Control.Up);
                state.TouchReset(Control.Down);
                break;
            case Control.Attack1:
                state.TouchReset(Control.Attack1);
                break;
            case Control.Attack2:
                state.TouchReset(Control.Attack2);
                break;
            case Control.Item:
                state.TouchReset(Control.Item);
                break;
            case Control.SpecialAttack:
                state.TouchReset(Control.SpecialAttack);
                break;
            case Control.DodgeLeft:
                state.TouchReset(Control.DodgeLeft);
                break;
            case Control.DodgeRight:
                state.TouchReset(Control.DodgeRight);
                break;
            }
        }

        void UpdateTouchTime(int control) {
            state.states[control].timeHeld += Time.deltaTime;
        }

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
            if (first == Control.Attack1 && second == Control.Attack2) {
                return Control.SpecialAttack;
            }
            return -1;
        }

        string GetComboName(int combo) {
            if (combo == Control.DodgeLeft) {
                return "DodgeLeftButton";
            }
            if (combo == Control.DodgeRight) {
                return "DodgeRightButton";
            }
            if (combo == Control.SpecialAttack) {
                return "SpecialAttackButton";
            }
            return "";
        }

        bool IsCombo(int control) {
            if (control == Control.DodgeLeft) {
                return true;
            }
            if (control == Control.DodgeRight) {
                return true;
            }
            if (control == Control.SpecialAttack) {
                return true;
            }
            return false;
        }

        void CheckTouchInput() {
            if (Input.touchCount == 0) {
                return;
            }

            foreach (var touch in Input.touches) {
                if (touch.phase == TouchPhase.Began) {
                    var obj = GetObjectAtPosition(touch.position);
                    var controlName = obj == null ? null : obj.name;
                    if (controlName == null) {
                        continue;
                    }
                    var control = GetTouchControl(controlName);
                    if (control == -1) {
                        ObjectSelected(new ObjectSelectedEventArgs(obj));
                        continue;
                    }
                    BeginTouch(control);
                    activeTouches.Add(touch.fingerId, controlName);
                    if (TouchControlStateChanged != null) {
                        TouchControlStateChanged(controlName, true);
                    }
                } else if (touch.phase == TouchPhase.Moved) {
                    string currentControlName;
                    if (!activeTouches.TryGetValue(touch.fingerId, out currentControlName)) {
                        continue;
                    }
                    var currentControl = GetTouchControl(currentControlName);
                    UpdateTouchTime(currentControl);
                    var newObj = GetObjectAtPosition(touch.position);
                    var newControlName = newObj == null ? null : newObj.name;
                    if (newControlName == null) {
                        continue;
                    }
                    var newControl = GetTouchControl(newControlName);
                    if (newControl == -1) {
                        continue;
                    }
                    var combo = GetTouchCombo(currentControl, newControl);
                    if (combo != -1) {
                        InvalidateControl(currentControl);
                        EndTouch(currentControl);
                        ControlCanceled(new ControlCanceledEventArgs(currentControl));
                        firstComboButton = currentControlName;
                        secondComboButton = newControlName;
                        activeTouches[touch.fingerId] = GetComboName(combo);
                        BeginTouch(combo);
                        if (TouchControlStateChanged != null) {
                            TouchControlStateChanged(newControlName, true);
                        }
                    }
                } else if (touch.phase == TouchPhase.Stationary) {
                    string control;
                    if (activeTouches.TryGetValue(touch.fingerId, out control)) {
                        UpdateTouchTime(GetTouchControl(control));
                    }
                } else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                    string control;
                    if (activeTouches.TryGetValue(touch.fingerId, out control)) {
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
            }
        }

        void Update() {
            CheckUnityInput();
            if (Input.touchSupported) {
                CheckTouchInput();
            }
        }
    }

    public class MultiPressControl {
        InputManager inputManager;
        int control;
        float timeout;
        float lastPressTime;
        bool active;
        int presses;

        public MultiPressControl(InputManager inputManager, int control, float timeout) {
            this.inputManager = inputManager;
            this.control = control;
            this.timeout = timeout;
        }

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

        public bool IsActive() {
            return active;
        }

        public int GetPresses() {
            return presses;
        }
    }
}