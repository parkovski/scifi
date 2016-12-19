using UnityEngine;

public class AppleBehavior : MonoBehaviour {
    // Use this for initialization
    void Start () {
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerProxy>().TakeDamage(5);
            Destroy(gameObject);
        }
    }
}
