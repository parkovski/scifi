using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

using SciFi.Items;
using SciFi.Environment.Effects;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class Gelignite : Projectile {
        Player stuckToPlayer;
        SpriteRenderer spriteRenderer;
        SpriteRenderer flameSpriteRenderer;

        const float burnTime = 3f;
        const int fadeSteps = 20;
        const float fadeStepInterval = burnTime / fadeSteps;

        void Start() {
            BaseStart();

            spriteRenderer = GetComponent<SpriteRenderer>();
            flameSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

            StartCoroutine(BurnUp());
        }

        void OnDestroy() {
            // So isServer doesn't work in here, good to know...
            if (NetworkServer.active && stuckToPlayer != null) {
                stuckToPlayer.sAttackHit -= PlayerHit;
            }
        }

        void Update() {
            if (stuckToPlayer != null) {
                transform.position = stuckToPlayer.transform.position + GetPlayerOffset(stuckToPlayer.eDirection);
            }
        }

        IEnumerator BurnUp() {
            int fadeStep = fadeSteps;
            while (fadeStep > 0) {
                var alpha = ((float)fadeStep) / fadeSteps;
                spriteRenderer.color = spriteRenderer.color.WithAlpha(alpha);
                flameSpriteRenderer.color = flameSpriteRenderer.color.WithAlpha(alpha);
                --fadeStep;
                yield return new WaitForSeconds(fadeStepInterval);
            }

            if (isServer) {
                Destroy(gameObject);
            }
        }

        Vector3 GetPlayerOffset(Direction direction) {
            if (direction == Direction.Left) {
                return new Vector3(-.3f, .2f);
            } else {
                return new Vector3(.3f, .2f);
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

            GameController.Instance.Hit(stuckToPlayer.gameObject, this, gameObject, 10, 5f);
            Effects.Explosion(transform.position);
            Destroy(gameObject);
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
    }
}