using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    public class SFNetworkTransform : NetworkBehaviour {
        public float syncInterval;
        public float closeEnoughPosition = 0.01f;
        public float closeEnoughVelocity = 0.1f;

        bool isUpdatedByClient;
        float lastMessageSentTime;
        float lastMessageReceivedTime;
        uint messagesReceived;
        double averageMessageInterval;
        Vector2 targetVelocity;
        Vector2 targetPosition;
        Rigidbody2D rb;

        void Start() {
            isUpdatedByClient = GetComponent<NetworkIdentity>().localPlayerAuthority;
            rb = GetComponent<Rigidbody2D>();
        }

        void Update() {
            // Possibilities:
            // - This copy is on a client with authority - it needs to notify
            // the server of its state, which will then notify the rest of
            // the clients. The server and client will do interpolation, but
            // the server will pass on the values unmodified.
            //
            // - This copy is on the server and does not have authority -
            // it interpolates in Update and passes on values to clients
            // when they are received.
            //
            // - This copy is on the server with authority - it just sends
            // values to the clients.
            //
            // - This copy is on a client without authority - it just receives
            // values and does interpolation.

            if (!isServer && hasAuthority) {
                if (Time.realtimeSinceStartup > lastMessageSentTime + syncInterval) {
                    if (!PositionCloseEnough(transform.position, targetPosition) || !VelocityCloseEnough(rb.velocity, targetVelocity)) {
                        int skippedMessages = (int)((Time.realtimeSinceStartup - lastMessageReceivedTime) / syncInterval) - 1;
                        lastMessageSentTime = Time.realtimeSinceStartup;
                        targetPosition = transform.position;
                        targetVelocity = rb.velocity;
                        CmdSyncState(GetSyncVector(), skippedMessages);
                    }
                }
            } else if (isServer && !hasAuthority) {
                Interpolate();
            } else if (isServer && hasAuthority) {
                if (Time.realtimeSinceStartup > lastMessageSentTime + syncInterval) {
                    if (!PositionCloseEnough(transform.position, targetPosition) || !VelocityCloseEnough(rb.velocity, targetVelocity)) {
                        int skippedMessages = (int)((Time.realtimeSinceStartup - lastMessageReceivedTime) / syncInterval) - 1;
                        lastMessageSentTime = Time.realtimeSinceStartup;
                        targetPosition = transform.position;
                        targetVelocity = rb.velocity;
                        RpcSyncState(GetSyncVector(), skippedMessages);
                    }
                }
            } else if (!isServer && !hasAuthority) {
                Interpolate();
            }
        }

        bool PositionCloseEnough(Vector2 sourceVec, Vector2 targetVec) {
            return Mathf.Abs((sourceVec - targetVec).magnitude) < closeEnoughPosition;
        }

        bool VelocityCloseEnough(Vector2 sourceVec, Vector2 targetVec) {
            return Mathf.Abs((sourceVec - targetVec).magnitude) < closeEnoughVelocity;
        }

        Vector4 GetSyncVector() {
            return new Vector4(rb.velocity.x, rb.velocity.y, transform.position.x, transform.position.y);
        }

        void GetVelPosVectors(Vector4 syncVector, out Vector2 velocity, out Vector2 position) {
            velocity = new Vector2(syncVector.x, syncVector.y);
            position = new Vector2(syncVector.z, syncVector.w);
        }

        [Command]
        void CmdSyncState(Vector4 syncVector, int skippedMessages) {
            RpcSyncState(syncVector, skippedMessages);
            GetVelPosVectors(syncVector, out targetVelocity, out targetPosition);
            UpdateStats(skippedMessages);
            lastMessageReceivedTime = Time.realtimeSinceStartup;
        }

        [ClientRpc]
        void RpcSyncState(Vector4 syncVector, int skippedMessages) {
            GetVelPosVectors(syncVector, out targetVelocity, out targetPosition);
            UpdateStats(skippedMessages);
            lastMessageReceivedTime = Time.realtimeSinceStartup;
        }

        void UpdateStats(int skippedMessages) {
            if (messagesReceived == 0) {
                lastMessageReceivedTime = Time.realtimeSinceStartup;
                averageMessageInterval = .01;
                messagesReceived = 1;
                return;
            }

            var timeSinceLastMessage = (Time.realtimeSinceStartup - lastMessageReceivedTime);
            if (skippedMessages > 1) {
                timeSinceLastMessage /= skippedMessages + 1;
            }
            lastMessageReceivedTime = Time.realtimeSinceStartup;
            averageMessageInterval = (timeSinceLastMessage + messagesReceived*averageMessageInterval) / (messagesReceived + 1);
            if (messagesReceived < 10) {
                ++messagesReceived;
            }
        }

        bool IsPositionTolerable(float deltaPosition, float velocity) {
            return (!Mathf.Approximately(velocity, 0f)) && (deltaPosition / velocity) < closeEnoughVelocity;
        }

        void Interpolate() {
            var deltaTime = (float)((Time.realtimeSinceStartup - lastMessageReceivedTime) / averageMessageInterval);
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, deltaTime);
            var deltaPosition = targetPosition - (Vector2)transform.position;

            if (IsPositionTolerable(deltaPosition.magnitude, rb.velocity.magnitude)) {
                rb.velocity += deltaPosition;
            } else {
                transform.position = Vector2.Lerp(transform.position, targetPosition, Mathf.Pow(deltaTime, 2));
            }
        }

        public override int GetNetworkChannel() {
            return 2;
        }
    }
}