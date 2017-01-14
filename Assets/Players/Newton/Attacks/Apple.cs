using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Items {
    public class Apple : Projectile {
        public GameObject explodingApple;
        public int damage;
        public float knockback;
        public int postGroundDamage;
        public float postGroundKnockback;

        /// After the apple hits the ground, it causes less damage.
        bool hasHitGround = false;

        void Start () {
            BaseStart();
            Destroy(gameObject, 3f);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }
            if (collision.gameObject.tag == "Ground") {
                hasHitGround = true;
            } else if (collision.gameObject.tag == "Player") {
                var damage = hasHitGround ? postGroundDamage : this.damage;
                var knockback = hasHitGround ? postGroundKnockback : this.knockback;
                GameController.Instance.TakeDamage(collision.gameObject, damage);
                GameController.Instance.Knockback(gameObject, collision.gameObject, knockback);
                var exploding = Instantiate(explodingApple, gameObject.transform.position, gameObject.transform.rotation);
                NetworkServer.Spawn(exploding);
                Destroy(gameObject);
            }
        }
    }
}