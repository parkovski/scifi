using UnityEngine;
using System.Collections;

namespace SciFi.Environment {
    public class StageSettings : MonoBehaviour {
        public bool overrideGravity;
        public float gravity;
        public bool unlimitedJumps;

        void Start() {
            if (overrideGravity) {
                StartCoroutine(ChangePlayerSettings());
            }
        }

        IEnumerator ChangePlayerSettings() {
            yield return new WaitUntil(() => GameController.Instance != null);
            GameController.Instance.PlayersInitialized += players => {
                foreach (var p in players) {
                    p.GetComponent<Rigidbody2D>().gravityScale = gravity / -Physics2D.gravity.y;
                    if (unlimitedJumps) {
                        p.SetJumpBehaviour(Players.JumpBehaviour.Unlimited);
                    }
                }
            };
        }
    }
}