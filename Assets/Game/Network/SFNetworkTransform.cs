using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    public class SFNetworkTransform : NetworkBehaviour {
        public float syncInterval;
        public float closeEnoughPosition = 0.01f;
        public float closeEnoughVelocity = 0.1f;
        public float snapDistance = 1.0f;

        float lastMessageSentTime;
        float lastMessageReceivedTime;
        float timeToTarget;
        float lastTimestamp;
        Vector2 targetPosition;
        Rigidbody2D rb;

        void Start() {
            rb = GetComponent<Rigidbody2D>();

            targetPosition = transform.position;
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
                    if (!PositionCloseEnough(transform.position, targetPosition)) {
                        targetPosition = transform.position;
                        CmdSyncState(GetSyncVector(), Time.realtimeSinceStartup);
                    }
                }
            } else if (isServer && !hasAuthority) {
                rb.velocity = Vector2.zero;
                Interpolate();
            } else if (isServer && hasAuthority) {
                if (Time.realtimeSinceStartup > lastMessageSentTime + syncInterval) {
                    if (!PositionCloseEnough(transform.position, targetPosition)) {
                        targetPosition = transform.position;
                        RpcSyncState(GetSyncVector(), Time.realtimeSinceStartup);
                    }
                }
            } else if (!isServer && !hasAuthority) {
                rb.velocity = Vector2.zero;
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

        [Command]
        void CmdSyncState(Vector2 position, float timestamp) {
            RpcSyncState(position, timestamp);
            targetPosition = position;
            UpdateStats(timestamp);
        }

        [ClientRpc]
        void RpcSyncState(Vector2 position, float timestamp) {
            targetPosition = position;
            UpdateStats(timestamp);
        }

        void UpdateStats(float timestamp) {
            float clientDeltaTime = Time.realtimeSinceStartup - lastMessageReceivedTime;
            float serverDeltaTime = timestamp - lastTimestamp;
            float interpolationTime = 0.2f;
            if (serverDeltaTime > interpolationTime) {
                timeToTarget = interpolationTime;
            } else {
                timeToTarget += serverDeltaTime - clientDeltaTime;
            }

            lastMessageReceivedTime = Time.realtimeSinceStartup;
            lastTimestamp = timestamp;
        }

        bool NeedsSnap(Vector2 sourcePosition, Vector2 targetPosition) {
            var deltaPosition = targetPosition - sourcePosition;
            return deltaPosition.magnitude >= snapDistance;
        }

        void Interpolate() {
            var dt = Time.realtimeSinceStartup - lastMessageReceivedTime;
            float interpTime;
            if (dt >= timeToTarget) {
                interpTime = 1f;
            } else {
                interpTime = timeToTarget - dt;
            }

            if (PositionCloseEnough(transform.position, targetPosition) || NeedsSnap(transform.position, targetPosition)) {
                transform.position = targetPosition;
            } else {
                transform.position = Vector2.Lerp(transform.position, targetPosition, interpTime);
            }
        }

        public override int GetNetworkChannel() {
            return 2;
        }
    }
}