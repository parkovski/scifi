using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;

namespace SciFi.Players {
    public class Nobel : Player {
        public GameObject dynamitePrefab;

        void Start() {
            BaseStart();

            eAttack1 = new DynamiteAttack(this, new[] { dynamitePrefab });
            eAttack2 = new DynamiteAttack(this, new[] { dynamitePrefab });
            eSpecialAttack = new DynamiteAttack(this, new[] { dynamitePrefab });
        }

        void FixedUpdate() {
            if (!isLocalPlayer) {
                return;
            }

            BaseInput();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        void OnCollisionExit2D(Collision2D collision) {
            BaseCollisionExit2D(collision);
        }


        [ClientRpc]
        protected override void RpcChangeDirection(Direction direction) {
            foreach (var sr in gameObject.GetComponentsInChildren<SpriteRenderer>()) {
                sr.flipX = !sr.flipX;
            }
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                child.localPosition = new Vector3(-child.localPosition.x, child.localPosition.y, child.localPosition.z);
            }
        }
    }
}