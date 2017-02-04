using UnityEngine;
using UnityEngine.Networking;

using SciFi.Items;
using SciFi.Environment.Effects;

namespace SciFi.Players.Attacks {
    public class Gelignite : Projectile {
        Player stuckToPlayer;

        void Start() {
            BaseStart();
        }

        void Update() {
            if (stuckToPlayer != null) {
                transform.position = stuckToPlayer.transform.position + GetPlayerOffset(stuckToPlayer.eDirection);
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

        [Server]
        void PlayerHit(AttackType type, AttackProperty properties) {
            if ((properties & AttackProperty.Explosive) == 0) {
                return;
            }

            stuckToPlayer.sAttackHit -= PlayerHit;
            GameController.Instance.Hit(stuckToPlayer.gameObject, this, gameObject, 10, 5f);
            Effects.Explosion(transform.position);
            Destroy(gameObject);
        }

        void HandleFreestandingCollision(Collision2D collision) {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null) {
                stuckToPlayer = player;
                gameObject.layer = Layers.projectileInteractables;
                if (isServer) {
                    player.sAttackHit += PlayerHit;
                }
            }
        }

        public override AttackProperty Properties { get { return AttackProperty.OnFire; } }
    }
}