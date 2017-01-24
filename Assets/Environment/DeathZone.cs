using UnityEngine;

namespace SciFi.Environment {
    /// Any object passing through triggers with this behaviour set
    /// will be destroyed, and players will lose a life and respawn.
    public class DeathZone : MonoBehaviour {
        void OnTriggerEnter2D(Collider2D collider) {
            if (collider.gameObject.tag != "Player") {
                Destroy(collider.gameObject);
                return;
            }
            GameController.Instance.CmdDie(collider.gameObject);
        }
    }
}