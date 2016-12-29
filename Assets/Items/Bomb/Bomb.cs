using UnityEngine;

public class Bomb : Item {
    void Start() {
        BaseStart(aliveTime: 10f);
    }

    void Update() {
        BaseUpdate();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (!isServer) {
            return;
        }

        if (collision.gameObject.tag == "Ground") {
            gameObject.layer = LayerMask.NameToLayer("Items");
        }

        if (collision.gameObject.tag == "Player") {
            GameController.Instance.TakeDamage(collision.gameObject, 15);
            GameController.Instance.Knockback(gameObject, collision.gameObject, 10f);
            Destroy(gameObject);
        }
    }
}