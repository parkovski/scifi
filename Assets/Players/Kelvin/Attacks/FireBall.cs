using UnityEngine;
using UnityEngine.Networking;
using System;
using Random = UnityEngine.Random;

using SciFi.Items;
using SciFi.Players.Modifiers;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class FireBall : Projectile, IPoolNotificationHandler {
        public Action<GameObject> destroyCallback;

        /// How many times the player gets hit by this attack
        int rounds;
        const int minRounds = 3;
        const int maxRounds = 5;
        const float minScale = .25f;
        const float maxScale = .5f;
        /// The max scale hit while charging.
        float scale;

        /// Accumulated knockback dealt on last round.
        float accumulatedKnockback;
        const float knockbackPerRound = 2f;

        /// How long to wait between damage rounds
        const float nextDamageWait = .25f;
        const float destroyAfterFadeTime = 4.5f;
        const float maxChargeTime = 1.5f;
        const float destroyAfterThrowTime = 1.5f;

        SpriteRenderer spriteRenderer;

        GameObject targetPlayer;
        GameObject owner;
        Vector3 targetOffset;
        const float gravityScale = 0.5f;

        IPooledObject pooled;

        State state;
        float stateStartTime;

        enum State {
            Charging,
            Fading,
            Throwing,
            Attacking,
        }

        void Awake() {
            pooled = PooledObject.Get(gameObject);
            state = State.Charging;
            spriteRenderer = transform.Find("FireBall").GetComponent<SpriteRenderer>();
        }

        void Start() {
            Reinit();
        }

        void Reinit() {
            ChangeState(State.Charging);
            accumulatedKnockback = 0;
            targetPlayer = null;
            owner = ClientScene.FindLocalObject(spawnedBy);
            targetOffset = transform.position - owner.transform.position;
            scale = minScale;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            transform.localScale = new Vector3(scale, scale, 1);
            spriteRenderer.color = spriteRenderer.color.WithAlpha(1f);
        }

        void ChangeState(State newState) {
            state = newState;
            stateStartTime = Time.time;
            if (isServer) {
                RpcChangeState(newState);
            }
        }

        [ClientRpc]
        void RpcChangeState(State newState) {
            if (isServer) {
                return;
            }
            ChangeState(newState);
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }

            switch (state) {
            case State.Charging:
                UpdateCharging();
                break;
            case State.Fading:
                UpdateFading();
                break;
            case State.Throwing:
                UpdateThrowing();
                break;
            case State.Attacking:
                UpdateAttacking();
                break;
            }
        }

        /// Grow the fireball.
        void UpdateCharging() {
            var time = Time.time - stateStartTime;
            time = Mathf.Clamp(time, 0f, maxChargeTime);
            scale = time.Scale(0f, maxChargeTime, minScale, maxScale);
            transform.localScale = new Vector3(scale, scale, 1);
            transform.position = owner.transform.position + targetOffset;
        }

        /// Shrink and fade the fireball.
        void UpdateFading() {
            var time = Mathf.Clamp(Time.time - stateStartTime, 0f, destroyAfterFadeTime);
            if (isServer && time >= destroyAfterFadeTime) {
                pooled.Release();
                return;
            }
            var fadeScale = time.Scale(0f, destroyAfterFadeTime, scale, minScale);
            var alpha = time.Scale(0f, destroyAfterFadeTime, 1f, .5f);
            transform.localScale = new Vector3(fadeScale, fadeScale, 1);
            spriteRenderer.color = spriteRenderer.color.WithAlpha(alpha);
        }

        /// Destroy the fireball after the time limit.
        void UpdateThrowing() {
            if (isServer && Time.time > stateStartTime + destroyAfterThrowTime) {
                pooled.Release();
            }
        }

        /// Give damage to the target player.
        void UpdateAttacking() {
            gameObject.transform.position = targetPlayer.transform.position + targetOffset;

            if (isServer && Time.time > stateStartTime + nextDamageWait) {
                stateStartTime = Time.time;
                DoAttack();
            }
        }

        [Server]
        public void StopCharging() {
            if (state == State.Fading || state == State.Attacking) {
                return;
            }
            ChangeState(State.Fading);
            GetComponent<Rigidbody2D>().gravityScale = 0.01f;
        }

        [Server]
        public void Throw(Direction direction) {
            if (state == State.Attacking) {
                return;
            }

            SetInitialVelocity(new Vector3(5, 2, 0).FlipDirection(direction));
            var rb = GetComponent<Rigidbody2D>();
            rb.velocity = initialVelocity;
            rb.gravityScale = gravityScale;
            RpcThrow(initialVelocity);
            ChangeState(State.Throwing);
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
            SetInitialVelocity(velocity);
            ChangeState(State.Throwing);
            gameObject.layer = Layers.projectiles;
        }

        [Server]
        void StartAttacking() {
            rounds = Random.Range(minRounds, maxRounds + 1);
            var player = targetPlayer.GetComponent<Player>();
            player.AddModifier(ModId.OnFire);
            player.AddModifier(ModId.Fast);
            ChangeState(State.Attacking);
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
                Throw(player.eDirection);
                targetPlayer = collision.gameObject;
                targetOffset = gameObject.transform.position - targetPlayer.transform.position;
                RpcSetTargetPlayer(targetPlayer.GetComponent<NetworkIdentity>().netId, targetOffset);
                gameObject.layer = Layers.displayOnly;
                StartAttacking();
            } else {
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
                ChangeState(State.Attacking);
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
            if (destroyCallback != null) {
                destroyCallback(gameObject);
                destroyCallback = null;
            }
        }
    }
}