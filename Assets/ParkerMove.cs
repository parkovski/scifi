﻿using UnityEngine;
using UnityEngine.Networking;

public class ParkerMove : NetworkBehaviour {
    Rigidbody2D rb;
    bool movingLeft = false;
    bool movingRight = false;
    bool shouldJump = false;
    bool canJump = false;
    int groundCollisions = 0;
    int touchControlLayer;
    int? leftBtnFingerId = null;
    int? rightBtnFingerId = null;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        touchControlLayer = LayerMask.NameToLayer("Touch Controls");

        if (!Input.touchSupported) {
            Destroy(GameObject.Find("left-button"));
            Destroy(GameObject.Find("right-button"));
        }
    }

    public override void OnStartLocalPlayer() {
        GetComponent<SpriteRenderer>().color = new Color(.8f, .9f, 1f, .8f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            ++groundCollisions;
            canJump = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            if (--groundCollisions == 0) {
                canJump = false;
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer) {
            return;
        }

        HandleInput();

        if (movingLeft || (leftBtnFingerId != null)) {
            rb.AddForce(transform.right * -10f);
        }
        if (movingRight || (rightBtnFingerId != null)) {
            rb.AddForce(transform.right * 10f);
        }
        if (shouldJump && canJump) {
            rb.AddForce(transform.up * 5f, ForceMode2D.Impulse);
            canJump = shouldJump = false;
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

        if (verticalAxis > 0) {
            shouldJump = true;
        } else {
            shouldJump = false;
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
