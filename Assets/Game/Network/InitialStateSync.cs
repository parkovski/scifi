using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    public class InitialStateSync : NetworkBehaviour {
        [SyncVar]
        Vector2 velocity;
        [SyncVar]
        float angularVelocity;
        [SyncVar]
        bool sentState = false;
        bool initialized = false;

        void Update() {
            if (isServer) {
                if (sentState) {
                    return;
                }
                var rb = GetComponent<Rigidbody2D>();
                velocity = rb.velocity;
                angularVelocity = rb.angularVelocity;
                sentState = true;
            } else {
                if (initialized) {
                    return;
                }
                if (!sentState) {
                    return;
                }
                var rb = GetComponent<Rigidbody2D>();
                rb.velocity = velocity;
                rb.angularVelocity = angularVelocity;
                initialized = true;
            }
        }

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
    }
}