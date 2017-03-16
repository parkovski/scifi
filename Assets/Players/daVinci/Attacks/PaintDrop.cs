using UnityEngine;

using SciFi.Items;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class PaintDrop : Projectile, IPoolNotificationHandler {
        float destroyTime;
        IPooledObject pooled;

        void Start() {
            pooled = PooledObject.Get(gameObject);
            Reinit();
        }

        void Reinit() {
            destroyTime = Time.time + 3f;
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }

            if (Time.time > destroyTime) {
                pooled.Release();
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            var projectile = collision.gameObject.GetComponent<Projectile>();
            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                var knockback = transform.localScale.x.Scale(.25f, .5f, 1f, 3f);
                var damage = (int)knockback;
                GameController.Instance.HitNoVelocityReset(collision.gameObject, this, gameObject, damage, knockback);
                pooled.Release();
            } else {
                if (projectile == null || !projectile.HasSameOwner(this)) {
                    pooled.Release();
                }
            }
        }

        void IPoolNotificationHandler.OnAcquire() {
            Reinit();
            PooledObject.Enable(gameObject);
        }

        void IPoolNotificationHandler.OnRelease() {
            Disable();
            PooledObject.Disable(gameObject);
        }
    }
}