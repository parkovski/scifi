using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    /// The Unity docs claim that you can set forces on an object
    /// and it will be included in the initial update - unfortunately
    /// this is not true, and they are only converted into velocities
    /// by the first update. This waits till then to sync them.
    public class InitialStateSync : NetworkBehaviour {
        [SyncVar]
        Vector2 velocity;
        [SyncVar]
        float angularVelocity;

        public override void OnStartServer() {
            var rb = GetComponent<Rigidbody2D>();
            this.velocity = rb.velocity;
            this.angularVelocity = rb.angularVelocity;
        }

        public override void OnStartClient() {
            var rb = GetComponent<Rigidbody2D>();
            rb.velocity = this.velocity;
            rb.angularVelocity = this.angularVelocity;
        }

        public override int GetNetworkChannel() {
            return 0;
        }
    }
}