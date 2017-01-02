using UnityEngine;

public class Arrow : Projectile {
    void Start() {
        BaseStart();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        Destroy(gameObject);
    }
}