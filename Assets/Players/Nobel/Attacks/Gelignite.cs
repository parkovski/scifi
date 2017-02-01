using UnityEngine;

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
            if (stuckToPlayer) {
                HandleStuckCollision(collision);
            } else {
                HandleFreestandingCollision(collision);
            }
        }

        void HandleStuckCollision(Collision2D collision) {
            Effects.Explosion(transform.position);
            GameController.Instance.TakeDamage(stuckToPlayer.gameObject, 5);
            GameController.Instance.Knockback(gameObject, stuckToPlayer.gameObject, 1f);
            Destroy(gameObject);
        }

        void HandleFreestandingCollision(Collision2D collision) {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null) {
                stuckToPlayer = player;
                gameObject.layer = Layers.projectileInteractables;
            }
        }
    }
}