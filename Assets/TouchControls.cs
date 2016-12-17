using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript;

public class TouchControls : MonoBehaviour {
    ParkerMove _moveScript;
    ParkerMove moveScript {
        get {
            if (_moveScript == null) {
                _moveScript = GameObject.Find("Parker").GetComponent<ParkerMove>();
            }
            return _moveScript;
        }
    }

    // Use this for initialization
    void Start () {
        //
    }

    // Update is called once per frame
    void Update () {
        //
    }

    /// <returns>Name of first touched object, or empty string.</returns>
    string GetTouchObjectName(TouchEventArgs args) {
        var rb = args.Touches[0].Hit.RaycastHit2D.rigidbody;
        if (rb == null) {
            return "";
        }
        return rb.gameObject.name;
    }
    void OnEnable() {
        TouchManager.Instance.TouchesBegan += (sender, args) => {
            var name = GetTouchObjectName(args);
            if (name == "left-button") {
                moveScript.LeftBtnDown();
            } else if (name == "right-button") {
                moveScript.RightBtnDown();
            }

        };
        TouchManager.Instance.TouchesEnded += (sender, args) => {
            var name = GetTouchObjectName(args);
            if (name == "left-button") {
                moveScript.LeftBtnUp();
            } else if (name == "right-button") {
                moveScript.RightBtnUp();
            }
        };
    }
}