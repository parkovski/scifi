using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControls : MonoBehaviour {
    ParkerMove _moveScript;
    int touchControlLayer;
    int? leftBtnTouchId = null;
    int? rightBtnTouchId = null;

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
        touchControlLayer = LayerMask.NameToLayer("Touch Controls");
        print("init");
    }

    // Update is called once per frame
    void Update () {
        //
        if (Input.touchCount == 0) return;
        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began) {
            var ray = Camera.main.ScreenToWorldPoint(touch.position);
            var hit = Physics2D.Raycast(ray, Vector2.zero, Mathf.Infinity, touchControlLayer);
            if (hit) {
                if (hit.rigidbody.gameObject.name == "left-button") {
                    leftBtnTouchId = touch.fingerId;
                    moveScript.LeftBtnDown();
                } else if (hit.rigidbody.gameObject.name == "right-button") {
                    rightBtnTouchId = touch.fingerId;
                    moveScript.RightBtnDown();
                }
            }
        } else if (touch.phase == TouchPhase.Ended) {
            if (touch.fingerId == leftBtnTouchId) {
                leftBtnTouchId = null;
                moveScript.LeftBtnUp();
            } else if (touch.fingerId == rightBtnTouchId) {
                rightBtnTouchId = null;
                moveScript.RightBtnUp();
            }
        }
    }
}