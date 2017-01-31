using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class Gelignite : Projectile {
        Player stuckToPlayer;

        void Start() {
            BaseStart();
        }

        void Update() {
            if (stuckToPlayer != null) {
                transform.position = stuckToPlayer.transform.position;
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
            //
        }

        void HandleFreestandingCollision(Collision2D collision) {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null) {
                stuckToPlayer = player;
            }
        }
    }
}