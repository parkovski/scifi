using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SciFi.Environment {
    /// Any object passing through triggers with this behaviour set
    /// will be destroyed, and players will lose a life and respawn.
    public class DeathZone : MonoBehaviour {
        /// Record the times so that players with multiple colliders
        /// don't die more than once.
        Dictionary<GameObject, float> timeOfDeath;
        float ignoreDeathTime = .25f;

        void Start() {
            timeOfDeath = new Dictionary<GameObject, float>();
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (!NetworkServer.active) {
                return;
            }
            if (collider.gameObject.layer != Layers.players) {
                Destroy(collider.gameObject);
                return;
            }
            float time;
            if (timeOfDeath.TryGetValue(collider.gameObject, out time)) {
                if (time + ignoreDeathTime > Time.time) {
                    return;
                }
            }
            timeOfDeath[collider.gameObject] = Time.time;
            GameController.Instance.Die(collider.gameObject);
        }
    }
}