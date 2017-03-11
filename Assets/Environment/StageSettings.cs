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
    }
}