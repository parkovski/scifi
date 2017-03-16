using UnityEngine;
using System.Collections;

using SciFi.Players.Hooks;
using SciFi.UI;

namespace SciFi.Environment {
    public class StageSettings : MonoBehaviour {
        public AudioClip song;
        public uint songBpm;
        public uint songStartAtBeat;
        public bool overrideGravity;
        public float gravity;
        public bool overrideJumps;
        public int maxJumps;

        void Start() {
            FindObjectOfType<Countdown>().Setup(song, songBpm, songStartAtBeat);
            if (overrideGravity) {
                StartCoroutine(ChangePlayerSettings());
            }
            SetupDeathZone(GameObject.Find("DeathZone"));
        }

        IEnumerator ChangePlayerSettings() {
            yield return new WaitUntil(() => GameController.Instance != null);
            GameController.Instance.PlayersInitialized += players => {
                foreach (var p in players) {
                    p.GetComponent<Rigidbody2D>().gravityScale = gravity / -Physics2D.gravity.y;
                    if (overrideJumps) {
                        p.SetJumpBehaviour(new UnlimitedJumps(maxJumps));
                    }
                }
            };
        }

        void SetupDeathZone(GameObject deathZone) {
            var bgExtent = GetComponent<SpriteRenderer>().bounds.extents;
            print(bgExtent);
            var colliders = deathZone.GetComponents<BoxCollider2D>();
            var colliderSize = 10;
            var horizontalOffset = 2;
            var topOffset = bgExtent.y * 2;
            var bottomOffset = 5;
            var colliderWidth = bgExtent.x * 2 + colliderSize + horizontalOffset * 2;
            var colliderHeight = bgExtent.y * 2 + colliderSize + topOffset + bottomOffset;
            // Left
            colliders[0].offset = new Vector2(-bgExtent.x - colliderSize / 2 - horizontalOffset, 0);
            colliders[0].size = new Vector2(colliderSize, bgExtent.y * 2 + colliderSize * 2 + topOffset + bottomOffset);
            // Right
            colliders[1].offset = new Vector2(bgExtent.x + colliderSize / 2 + horizontalOffset, 0);
            colliders[1].size = new Vector2(colliderSize, bgExtent.y * 2 + colliderSize * 2 + topOffset + bottomOffset);
            // Bottom
            colliders[2].offset = new Vector2(0, -bgExtent.y - colliderSize / 2 - bottomOffset);
            colliders[2].size = new Vector2(bgExtent.x * 2 + colliderSize * 2 + horizontalOffset * 2, colliderSize);
            // Top
            colliders[3].offset = new Vector2(0, bgExtent.y + colliderSize / 2 + topOffset);
            colliders[3].size = new Vector2(bgExtent.x * 2 + colliderSize * 2 + horizontalOffset * 2, colliderSize);
        }
    }
}