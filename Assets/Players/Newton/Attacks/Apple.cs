using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class Apple : Projectile, IPoolNotificationHandler {
        public GameObject explodingApple;
        public int damage;
        public float knockback;
        public int postGroundDamage;
        public float postGroundKnockback;
        float startTime;
        const float lifetime = 3f;
        /// Just because Unity calls OnCollisionEnter2D before Start.
        /// Thanks a lot Unity.
        bool releaseOnUpdate;
        IPooledObject pooled;

        /// After the apple hits the ground, it causes less damage.
        bool hasHitGround = false;

        void Start() {
            pooled = PooledObject.Get(gameObject);
            Reinit();
        }

        void Release() {
            if (pooled == null) {
                releaseOnUpdate = true;
            } else {
                pooled.Release();
            }
        }

        void Reinit() {
            startTime = Time.time;
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }
            if (releaseOnUpdate) {
                releaseOnUpdate = false;
                Release();
            }

            if (Time.time > startTime + lifetime) {
                Release();
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            var hit = Attack.GetAttackHit(collision.gameObject.layer);
            if (collision.gameObject.tag == "Ground") {
                hasHitGround = true;
            } else if (hit == AttackHit.HitAndDamage) {
                var damage = hasHitGround ? postGroundDamage : this.damage;
                var knockback = hasHitGround ? postGroundKnockback : this.knockback;
                GameController.Instance.HitNoVelocityReset(collision.gameObject, this, gameObject, damage, knockback);
                Release();
            } else if (hit == AttackHit.HitOnly) {
                Release();
            }
        }

        void IPoolNotificationHandler.OnAcquire() {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Rigidbody2D>().isKinematic = false;
            Reinit();
        }

        void IPoolNotificationHandler.OnRelease() {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
            var rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            GameController.Instance.GetFromLocalPool(explodingApple, transform.position, transform.rotation);
        }
    }
}