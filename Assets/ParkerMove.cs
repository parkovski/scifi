using System.Collections.Generic;
using UnityEngine;

public class ParkerMove : MonoBehaviour {
    Rigidbody2D rb;
    bool movingLeft = false;
    bool movingRight = false;
    bool shouldJump = false;
    int touchControlLayer;
    int? leftBtnFingerId = null;
    int? rightBtnFingerId = null;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        touchControlLayer = LayerMask.NameToLayer("Touch Controls");
    }

    // Update is called once per frame
    void Update () {
        HandleInput();

        if (movingLeft || (leftBtnFingerId != null)) {
            rb.AddForce(transform.right * -10f);
        }
        if (movingRight || (rightBtnFingerId != null)) {
            rb.AddForce(transform.right * 10f);
        }
        if (shouldJump) {
            rb.AddForce(transform.up * 8f, ForceMode2D.Impulse);
            shouldJump = false;
        }
    }

    void HandleInput() {
        var horizontalAxis = Input.GetAxis("Horizontal");
        var verticalAxis = Input.GetAxis("Vertical");
        if (horizontalAxis < 0) {
            movingLeft = true;
            movingRight = false;
        } else if (horizontalAxis > 0) {
            movingLeft = false;
            movingRight = true;
        } else {
            movingLeft = movingRight = false;
        }

        if (verticalAxis != 0) {
            // TODO: Check for collision with floor instead.
            if (rb.velocity.y == 0f) {
                shouldJump = true;
            }
        }

        if (Input.touchCount > 0) {
            foreach (var touch in Input.touches) {
                if (touch.phase == TouchPhase.Began) {
                    var ray = Camera.main.ScreenToWorldPoint(touch.position);
                    var hit = Physics2D.Raycast(ray, Vector2.zero, Mathf.Infinity, 1 << touchControlLayer);
                    if (hit) {
                        if (hit.rigidbody.gameObject.name == "left-button") {
                            leftBtnFingerId = touch.fingerId;
                        } else if (hit.rigidbody.gameObject.name == "right-button") {
                            rightBtnFingerId = touch.fingerId;
                        }
                    }
                } else if (touch.phase == TouchPhase.Ended) {
                    if (touch.fingerId == leftBtnFingerId) {
                        leftBtnFingerId = null;
                    } else if (touch.fingerId == rightBtnFingerId) {
                        rightBtnFingerId = null;
                    }
                }
            }
        }
    }
}
