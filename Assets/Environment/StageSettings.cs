using UnityEngine;
using System.Collections;

namespace SciFi.Environment {
    public class StageSettings : MonoBehaviour {
        public bool overrideGravity;
        public float gravity;

        void Start() {
            if (overrideGravity) {
                StartCoroutine(ChangePlayerGravity());
            }
        }

        IEnumerator ChangePlayerGravity() {
            yield return new WaitUntil(() => GameController.Instance != null);
            GameController.Instance.PlayersInitialized += players => {
                foreach (var p in players) {
                    p.GetComponent<Rigidbody2D>().gravityScale = gravity / -Physics2D.gravity.y;
                }
            };
        }
    }
}