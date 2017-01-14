using UnityEngine;

namespace SciFi.Items {
    public class Arrow : Projectile {
        void Start() {
            BaseStart();
        }

        void Update() {
            var velocity = GetComponent<Rigidbody2D>().velocity;
            if (velocity.x > 0) {
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            } else {
                transform.rotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(velocity.y, -velocity.x) * Mathf.Rad2Deg);
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (collision.gameObject.tag == "Player") {
                GameController.Instance.TakeDamage(collision.gameObject, 5);
                GameController.Instance.Knockback(gameObject, collision.gameObject, 1f);
            }
            Destroy(gameObject);
        }
    }
}