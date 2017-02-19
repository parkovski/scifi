using UnityEngine;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class IceBall : Projectile, IPoolNotificationHandler {
        public bool alwaysFreeze = false;
        public GameObject iceBlockPrefab;

        IPooledObject pooled;

        const int chanceOfFreezing = 10;

        void Awake() {
            pooled = PooledObject.Get(gameObject);
        }

        void Start() {
            Reinit();
        }

        void Reinit() {
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                var player = collision.gameObject.GetComponent<Player>();
                var damage = 6;
                var knockback = 3.5f;
                if (player != null) {
                    if (Random.Range(0, chanceOfFreezing) == 0
#if UNITY_EDITOR
                        || alwaysFreeze
#endif
                    ) {
                        var iceblock = Instantiate(iceBlockPrefab, Vector3.zero, Quaternion.identity);
                        iceblock.GetComponent<IceBlock>().frozenPlayer = player;
                        damage = 8;
                        knockback = 5f;
                    }
                }
                GameController.Instance.HitNoVelocityReset(collision.gameObject, this, gameObject, damage, knockback);
            }
            pooled.Release();
        }

        public override AttackProperty Properties {
            get {
                return AttackProperty.Frozen;
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
            Disable();
        }
    }
}