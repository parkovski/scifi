using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

using SciFi.Items;
using SciFi.UI;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class Gelignite : Projectile, IPoolNotificationHandler {
        public GameObject explosionPrefab;
        Player stuckToPlayer;
        SpriteRenderer spriteRenderer;
        SpriteRenderer flameSpriteRenderer;

        float lastBurnTime;

        const float burnTime = 4.5f;
        const float burnDamageInterval = 0.5f;
        const int fadeSteps = 20;
        const float fadeStepInterval = burnTime / fadeSteps;

        IPooledObject pooled;

        void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            flameSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            pooled = PooledObject.Get(gameObject);
        }

        void Start() {
            Reinit();
        }

        void Reinit() {
            var spawnedByPlayer = ClientScene.FindLocalObject(spawnedBy).GetComponent<Player>();
            if (spawnedByPlayer.eTeam != -1) {
                GetComponent<SpriteOverlay>().SetColor(Player.TeamToColor(spawnedByPlayer.eTeam));
            }

            StartCoroutine(BurnUp());
        }

        void Update() {
            if (pooled.IsFree()) {
                return;
            }
            if (stuckToPlayer != null) {
                transform.position = stuckToPlayer.transform.position + GetPlayerOffset(stuckToPlayer.eDirection);
                if (isServer) {
                    if (Time.time > lastBurnTime + burnDamageInterval) {
                        GameController.Instance.Hit(stuckToPlayer.gameObject, this, gameObject, 1, 0);
                        lastBurnTime = Time.time;
                    }
                }
            }
        }

        IEnumerator BurnUp() {
            int fadeStep = fadeSteps;
            while (fadeStep > 0) {
                if (pooled.IsFree()) {
                    yield break;
                }
                var alpha = ((float)fadeStep) / fadeSteps;
                spriteRenderer.color = spriteRenderer.color.WithAlpha(alpha);
                //flameSpriteRenderer.color = flameSpriteRenderer.color.WithAlpha(alpha);
                --fadeStep;
                yield return new WaitForSeconds(fadeStepInterval);
            }

            if (isServer) {
                pooled.Release();
            }
        }

        Vector3 GetPlayerOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.1f, .2f);
            } else {
                return new Vector3(.1f, .2f);
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (!isServer) {
                return;
            }

            if (stuckToPlayer == null) {
                HandleFreestandingCollision(collision);
            }
        }

        /// Registers a callback on the player, so that when they are hit,
        /// this can explode.
        [Server]
        void PlayerHit(AttackType type, AttackProperty properties) {
            if ((properties & AttackProperty.Explosive) == 0) {
                return;
            }

            var explosionGo = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var explosion = explosionGo.GetComponent<Explosion>();
            explosion.damage = 5;
            explosion.knockback = 5f;
            NetworkServer.Spawn(explosionGo);
            pooled.Release();
        }

        [Server]
        void HandleFreestandingCollision(Collision2D collision) {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null) {
                stuckToPlayer = player;
                gameObject.layer = Layers.displayOnly;
                player.sAttackHit += PlayerHit;
                RpcSetStuckToPlayer(player.netId);
            }
        }

        [ClientRpc]
        void RpcSetStuckToPlayer(NetworkInstanceId playerId) {
            stuckToPlayer = ClientScene.FindLocalObject(playerId).GetComponent<Player>();
        }

        public override AttackProperty Properties { get { return AttackProperty.OnFire; } }

        void IPoolNotificationHandler.OnAcquire() {
            GetComponent<SpriteRenderer>().enabled = true;
            var flame = transform.GetChild(0);
            flame.GetComponent<Animator>().enabled = true;
            flame.GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<Collider2D>().enabled = true;
            GetComponent<Rigidbody2D>().isKinematic = false;
            Reinit();
        }

        void IPoolNotificationHandler.OnRelease() {
            GetComponent<SpriteRenderer>().enabled = false;
            var flame = transform.GetChild(0);
            flame.GetComponent<Animator>().enabled = false;
            flame.GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
            var rb = GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;
            if (NetworkServer.active && stuckToPlayer != null) {
                stuckToPlayer.sAttackHit -= PlayerHit;
            }
            stuckToPlayer = null;
            Disable();
        }
    }
}