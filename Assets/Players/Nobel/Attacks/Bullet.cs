using UnityEngine;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class Bullet : Projectile, IPoolNotificationHandler {
        public int damage;
        public float knockback;
        Vector2 originalPosition;
        IPooledObject pooled;

        public void Start() {
            pooled = PooledObject.Get(gameObject);
            Reinit();
        }

        void Reinit() {
            originalPosition = transform.position;

            if (isServer) {
                Effects.Smoke(transform.position);
            }
        }

        void Update() {
            if (!isServer) {
                return;
            }

            if (((Vector2)transform.position - originalPosition).magnitude > 2f) {
                pooled.Release();
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            var hit = Attack.GetAttackHit(collision.gameObject.layer);
            if (hit != AttackHit.None) {
                if (hit == AttackHit.HitAndDamage) {
                    GameController.Instance.HitNoVelocityReset(collision.gameObject, this, gameObject, damage, knockback);
                }
                pooled.Release();
            }
        }

        public override AttackProperty Properties { get { return AttackProperty.Explosive; } }

        void IPoolNotificationHandler.OnAcquire() {
            Reinit();
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Rigidbody2D>().isKinematic = false;
        }

        void IPoolNotificationHandler.OnRelease() {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
            var rb = GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;
            Disable();
        }
    }
}