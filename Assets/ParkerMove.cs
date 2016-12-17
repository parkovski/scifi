using System.Collections.Generic;
using UnityEngine;

public class ParkerMove : MonoBehaviour {
    Rigidbody2D rb;
    bool movingLeft;
    bool movingRight;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update () {
        /*var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        if (horizontal != 0) {
            rb.AddForce(transform.right * 10f * horizontal);
        }
        if (vertical > 0) {
            if (rb.velocity.y == 0f) {
                rb.AddForce(transform.up * 8f, ForceMode2D.Impulse);
            }
        }*/
        if (movingLeft) {
            rb.AddForce(transform.right * -10f);
        }
        if (movingRight) {
            rb.AddForce(transform.right * 10f);
        }
    }

    public void LeftBtnDown() {
        movingLeft = true;
    }

    public void LeftBtnUp() {
        movingLeft = false;
    }

    public void RightBtnDown() {
        movingRight = true;
    }

    public void RightBtnUp() {
        movingRight = false;
    }
}
