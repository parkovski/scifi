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
    public const int Attack = 4;
    public const int ArrayLength = 5;
}

class InputState {
    public ButtonState[] states;

    public InputState() {
        states = new ButtonState[Control.ArrayLength];
        for (var i = 0; i < states.Length; i++) {
            states[i] = new ButtonState();
            ForceReset(i);
        }
    }

    public void Invalidate(int button) {
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
public delegate void ControlCanceledHandler(object sender, ControlCanceledEventArgs args);

public class InputManager : MonoBehaviour {
    int touchControlLayerId;
    InputState state = new InputState();
    // Maps from finger ID to Control.
    Dictionary<int, int> activeTouches;

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

    public event ControlCanceledHandler ControlCanceled;

    void Start() {
        touchControlLayerId = LayerMask.NameToLayer("Touch Controls");
        activeTouches = new Dictionary<int, int>();
    }

    void CheckUnityInput() {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var fire = Input.GetButton("Fire1");

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

        state.UpdateButton(Control.Attack, fire);
    }

    string GetControlAtTouch(Touch touch) {
        var ray = Camera.main.ScreenToWorldPoint(touch.position);
        var hit = Physics2D.Raycast(ray, Vector2.zero, Mathf.Infinity, 1 << touchControlLayerId);
        if (!hit) {
            return null;
        }
        return hit.rigidbody.gameObject.name;
    }

    int GetTouchControl(string controlName) {
        switch (controlName) {
        case "left-button":
            return Control.Left;
        case "right-button":
            return Control.Right;
        case "fire-button":
            return Control.Attack;
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
        case Control.Attack:
            state.TouchUpdateButton(Control.Attack, true);
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
        case Control.Attack:
            state.TouchReset(Control.Attack);
            break;
        }
    }

    void UpdateTouchTime(int control) {
        state.states[control].timeHeld += Time.deltaTime;
    }

    int GetTouchCombo(int first, int second) {
        return -1;
    }

    void CheckTouchInput() {
        if (Input.touchCount == 0) {
            return;
        }

        foreach (var touch in Input.touches) {
            if (touch.phase == TouchPhase.Began) {
                var controlName = GetControlAtTouch(touch);
                if (controlName == null) {
                    continue;
                }
                var control = GetTouchControl(controlName);
                if (control == -1) {
                    continue;
                }
                BeginTouch(control);
                activeTouches.Add(touch.fingerId, control);
            } else if (touch.phase == TouchPhase.Moved) {
                var newControlName = GetControlAtTouch(touch);
                if (newControlName == null) {
                    continue;
                }
                var newControl = GetTouchControl(newControlName);
                if (newControl == -1) {
                    continue;
                }
                var currentControl = activeTouches[touch.fingerId];
                var combo = GetTouchCombo(activeTouches[touch.fingerId], newControl);
                if (combo != -1) {
                    InvalidateControl(currentControl);
                    ControlCanceled(this, new ControlCanceledEventArgs(currentControl));
                    activeTouches[touch.fingerId] = combo;
                    BeginTouch(combo);
                }
            } else if (touch.phase == TouchPhase.Stationary) {
                UpdateTouchTime(activeTouches[touch.fingerId]);
            } else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                EndTouch(activeTouches[touch.fingerId]);
                activeTouches.Remove(touch.fingerId);
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