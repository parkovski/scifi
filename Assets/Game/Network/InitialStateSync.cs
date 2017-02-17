using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    /// The Unity docs claim that you can set forces on an object
    /// and it will be included in the initial update - unfortunately
    /// this is not true, and they are only converted into velocities
    /// by the first update. So we have to use velocities, and sync
    /// them manually.
    public class InitialStateSync : NetworkBehaviour {
        public override void OnStartServer() {
            Resync();
        }

        public void Resync() {
            var rb = GetComponent<Rigidbody2D>();
            RpcSetVelocities(rb.velocity, rb.angularVelocity);
        }

        [ClientRpc]
        void RpcSetVelocities(Vector2 velocity, float angularVelocity) {
            if (isServer) {
                return;
            }

            var rb = GetComponent<Rigidbody2D>();
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;
        }

        public override int GetNetworkChannel() {
            return 0;
        }
    }
}