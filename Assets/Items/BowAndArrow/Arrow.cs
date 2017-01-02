using UnityEngine;

namespace SciFi.Items {
    public class Arrow : Projectile {
        void Start() {
            BaseStart();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            Destroy(gameObject);
        }
    }
}