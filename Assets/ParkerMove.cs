using System.Collections.Generic;
using UnityEngine;

public class ParkerMove : MonoBehaviour {
    Rigidbody2D rb;
    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update () {
        /*var x = Input.GetAxis("Horizontal") * 0.1f;
        var y = Input.GetAxis("Vertical") * 0.1f;
        transform.Translate(x, 0, y);*/
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        if (horizontal != 0) {
            rb.AddForce(transform.right * 10f * horizontal);
        }
        if (vertical > 0) {
            if (rb.velocity.y == 0f) {
                rb.AddForce(transform.up * 8f, ForceMode2D.Impulse);
            }
        }
    }
}
