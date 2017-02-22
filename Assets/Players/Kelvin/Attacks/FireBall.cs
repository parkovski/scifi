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
        float destroyTime;
        float gravityScale;

        IPooledObject pooled;

        void Awake() {
            pooled = PooledObject.Get(gameObject);
            isCharging = true;
        }

        void Start() {
            gravityScale = GetComponent<Rigidbody2D>().gravityScale;
            Reinit();
        }

        void Reinit() {
            isCharging = true;
            chargingStartTime = Time.time;
            accumulatedKnockback = 0;
            targetPlayer = null;
            GetComponent<Rigidbody2D>().gravityScale = 0;
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }

            if (isCharging) {
                var time = Mathf.Clamp(Time.time - chargingStartTime, 0f, 1.5f);
                var scale = time.Scale(0f, 1f, minScale, maxScale);
                transform.localScale = new Vector3(scale, scale, 1);
            } else if (isServer && Time.time > destroyTime) {
                pooled.Release();
                return;
            }

            if (targetPlayer != null) {
                gameObject.transform.position = targetPlayer.transform.position + targetOffset;

                if (isServer && Time.time > nextDamageTime) {
                    nextDamageTime = Time.time + nextDamageWait;
                    DoAttack();
                }
            }
        }

        [Server]
        public void Throw(Direction direction) {
            SetInitialVelocity(new Vector3(5, 2, 0).FlipDirection(direction));
            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = gravityScale;
            rb.velocity = initialVelocity;
            RpcThrow(initialVelocity);
            destroyTime = Time.time + 1.5f;
            isCharging = false;
            gameObject.layer = Layers.projectiles;
        }

        [ClientRpc]
        void RpcThrow(Vector2 velocity) {
            if (isServer) {
                return;
            }

            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = gravityScale;
            rb.velocity = velocity;
            isCharging = false;
            gameObject.layer = Layers.projectiles;
        }

        [Server]
        void StartAttacking() {
            rounds = Random.Range(minRounds, maxRounds + 1);
            var player = targetPlayer.GetComponent<Player>();
            player.AddModifier(ModId.OnFire);
            player.AddModifier(ModId.Fast);
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
                RpcSetTargetPlayer(targetPlayer.GetComponent<NetworkIdentity>().netId, targetOffset);
                gameObject.layer = Layers.displayOnly;
                StartAttacking();
            } else if (!isCharging) {
                pooled.Release();
            }
        }

        [ClientRpc]
        void RpcSetTargetPlayer(NetworkInstanceId netId, Vector2 offset) {
            if (netId == NetworkInstanceId.Invalid) {
                targetPlayer = null;
            } else {
                targetPlayer = ClientScene.FindLocalObject(netId);
                targetOffset = offset;
            }
        }

        public override AttackProperty Properties {
            get {
                return AttackProperty.OnFire;
            }
        }

        void IPoolNotificationHandler.OnAcquire() {
            PooledObject.Enable(gameObject);
            Reinit();
        }

        void IPoolNotificationHandler.OnRelease() {
            PooledObject.Disable(gameObject);
            if (targetPlayer != null) {
                var player = targetPlayer.GetComponent<Player>();
                player.RemoveModifier(ModId.OnFire);
                player.RemoveModifier(ModId.Fast);
            }
            Disable();
        }
    }
}