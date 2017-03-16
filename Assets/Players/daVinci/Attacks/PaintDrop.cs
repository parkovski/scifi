using UnityEngine;
using UnityEngine.Networking;

using SciFi.Items;
using SciFi.UI;
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
                var damage = transform.localScale.x > 0.375f ? 2 : 1;
                GameController.Instance.HitNoVelocityReset(collision.gameObject, this, gameObject, damage, 0f);
                pooled.Release();
            } else {
                if (projectile == null || !projectile.HasSameOwner(this)) {
                    pooled.Release();
                }
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
            Disable();
            PooledObject.Disable(gameObject);
        }
    }
}