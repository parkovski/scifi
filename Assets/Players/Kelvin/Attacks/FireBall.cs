using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

using SciFi.Items;
using SciFi.Players.Modifiers;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class FireBall : Projectile, IPoolNotificationHandler {
        /// How many times the player gets hit by this attack
        int rounds;
        const int minRounds = 3;
        const int maxRounds = 5;
        const float minScale = .25f;
        const float maxScale = .5f;

        /// Accumulated knockback dealt on last round.
        float accumulatedKnockback;
        const float knockbackPerRound = 2f;

        /// How long to wait between damage rounds
        float nextDamageTime;
        const float nextDamageWait = .25f;

        GameObject targetPlayer;
        Vector3 targetOffset;
        bool isCharging;
        float chargingStartTime;
        /// If the fireball is being held and charging, it can't
        /// be destroyed - only when it is flying.
        bool canDestroy = false;
        float destroyTime;

        IPooledObject pooled;

        void Awake() {
            pooled = PooledObject.Get(gameObject);
            isCharging = true;
        }

        void Start() {
            Reinit();
        }

        void Reinit() {
            isCharging = true;
            chargingStartTime = Time.time;
            accumulatedKnockback = 0;
            targetOffset = transform.position;
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }

            if (isCharging) {
                var time = Mathf.Clamp(Time.time - chargingStartTime, 0f, 1.5f);
                var scale = time.Scale(0f, 1f, minScale, maxScale);
                transform.localScale = new Vector3(scale, scale, 1);
                transform.position = targetOffset;
            }

            if (!isServer) {
                return;
            }

            if (!isCharging && Time.time > destroyTime) {
                pooled.Release();
                return;
            }

            if (targetPlayer != null) {
                gameObject.transform.position = targetPlayer.transform.position + targetOffset;

                if (Time.time > nextDamageTime) {
                    nextDamageTime = Time.time + nextDamageWait;
                    DoAttack();
                }
            }
        }

        public void Throw(Direction direction) {
            SetInitialVelocity(new Vector3(5, 2, 0).FlipDirection(direction));
            GetComponent<Rigidbody2D>().velocity = initialVelocity;
            destroyTime = Time.time + 1.5f;
            isCharging = false;
            gameObject.layer = Layers.projectiles;
        }

        [Server]
        void StartAttacking() {
            rounds = Random.Range(minRounds, maxRounds + 1);
            var player = targetPlayer.GetComponent<Player>();
            player.AddModifier(Modifier.OnFire);
            player.AddModifier(Modifier.Fast);
            nextDamageTime = Time.time + nextDamageWait;
            DoAttack();
        }

        [Server]
        void DoAttack() {
            if (rounds <= 0) {
                pooled.Release();
                return;
            }

            var scale = transform.localScale.x.Scale(minScale, maxScale, 1f, 2f);
            float knockback = 0;
            accumulatedKnockback += 1.5f;
            if (--rounds <= 0) {
                knockback = accumulatedKnockback * scale;
            }
            GameController.Instance.HitNoVelocityReset(targetPlayer, this, gameObject, (int)(2 * scale), knockback);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }
            if (targetPlayer != null) {
                return;
            }

            var player = collision.gameObject.GetComponent<Player>();
            if (player != null) {
                if (isCharging) {
                    Throw(player.eDirection);
                }
                targetPlayer = collision.gameObject;
                targetOffset = gameObject.transform.position - targetPlayer.transform.position;
                gameObject.layer = Layers.displayOnly;
                StartAttacking();
            } else if (!isCharging) {
                pooled.Release();
            }
        }

        public override AttackProperty Properties {
            get {
                return AttackProperty.OnFire;
            }
        }

        void IPoolNotificationHandler.OnAcquire() {
            for (var i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = true;
            }
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Rigidbody2D>().isKinematic = false;
            Reinit();
        }

        void IPoolNotificationHandler.OnRelease() {
            for (var i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
            }
            GetComponent<Collider2D>().enabled = false;
            var rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            if (targetPlayer != null) {
                var player = targetPlayer.GetComponent<Player>();
                player.RemoveModifier(Modifier.OnFire);
                player.RemoveModifier(Modifier.Fast);
                targetPlayer = null;
            }
        }
    }
}