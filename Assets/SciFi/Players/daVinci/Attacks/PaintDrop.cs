using UnityEngine;
using UnityEngine.Networking;

using SciFi.Items;
using SciFi.UI;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class PaintDrop : Projectile, IPoolNotificationHandler {
        float destroyTime;
        IPooledObject pooled;
        /// When several paint drops hit a player, the combined
        /// knockback is too much no matter how low the individual
        /// values are, so we only let the first one in a group
        /// do knockback - this hit set will be shared by all
        /// paint drops in the group.
        public HitSet sSharedHitSet;

        public const float minScale = 0.25f;
        public const float maxScale = 0.5f;

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
            var attackHit = Attack.GetAttackHit(collision.gameObject.layer);
            if (attackHit == AttackHit.HitAndDamage) {
                var damage = (int)transform.localScale.x.Scale(minScale, maxScale, 1.5f, 5.5f);
                float knockback;
                if (sSharedHitSet == null || sSharedHitSet.CheckOrFlag(collision.gameObject)) {
                    knockback = 0f;
                } else {
                    knockback = 5f;
                }
                GameController.Instance.HitNoVelocityReset(collision.gameObject, this, gameObject, damage, knockback);
                pooled.Release();
            } else if (attackHit == AttackHit.HitOnly) {
                NoDamageCollision(collision.gameObject);
            } else {
                pooled.Release();
            }
        }

        void OnTriggerEnter2D(Collider2D collider) {
            NoDamageCollision(collider.gameObject);
        }

        void NoDamageCollision(GameObject obj) {
            if (Attack.GetAttackHit(obj.layer) != AttackHit.None) {
                var projectile = obj.GetComponent<Projectile>();
                if (projectile != null && projectile.HasSameOwner(this)) {
                    // Don't hit other paint drops, but do hit other projectiles
                    // created by this player (the flying machine).
                    if (projectile is PaintDrop) {
                        return;
                    }
                }
                GameController.Instance.HitNoVelocityReset(obj, this, gameObject, 1, 1);
                pooled.Release();
            }
        }

        public void SetColor(Color color) {
            GetComponent<SpriteOverlay>().SetColor(color);
            RpcSetColor(color);
        }

        [ClientRpc]
        void RpcSetColor(Color color) {
            if (isServer) {
                return;
            }
            GetComponent<SpriteOverlay>().SetColor(color);
        }

        void IPoolNotificationHandler.OnAcquire() {
            Reinit();
            PooledObject.Enable(gameObject);
        }

        void IPoolNotificationHandler.OnRelease() {
            sSharedHitSet = null;
            Disable();
            PooledObject.Disable(gameObject);
        }
    }
}