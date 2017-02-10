using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace SciFi.Network {
    public class SFNetworkTransform : NetworkBehaviour {
        public float syncInterval;
        public float interpolationTime = 0.2f;
        public float closeEnoughPosition = 0.01f;
        public float closeEnoughVelocity = 0.1f;
        public float snapDistance = 3f;

        float snapTimer;
        float lastMessageSentTime;
        float lastMessageReceivedTime;
        float timeToTarget;
        float lastTimestamp;
        float clockOffset;
        Vector2 targetPosition;
        Vector2 originalPosition;
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
                        lastMessageSentTime = Time.realtimeSinceStartup;
                        targetPosition = transform.position;
                        CmdSyncState(transform.position, Time.realtimeSinceStartup);
                    }
                }
            } else if (isServer && !hasAuthority) {
                rb.velocity = Vector2.zero;
                Interpolate();
            } else if (isServer && hasAuthority) {
                if (Time.realtimeSinceStartup > lastMessageSentTime + syncInterval) {
                    if (!PositionCloseEnough(transform.position, targetPosition)) {
                        lastMessageSentTime = Time.realtimeSinceStartup;
                        targetPosition = transform.position;
                        RpcSyncState(transform.position, Time.realtimeSinceStartup);
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

        [Command]
        void CmdSyncState(Vector2 position, float timestamp) {
            if (timestamp < lastTimestamp) {
                return;
            }
            targetPosition = position;
            originalPosition = transform.position;
            UpdateStats(timestamp);
            RpcSyncState(position, timestamp);
        }

        [ClientRpc]
        void RpcSyncState(Vector2 position, float timestamp) {
            if (isServer) {
                return;
            }
            if (timestamp < lastTimestamp) {
                return;
            }
            targetPosition = position;
            originalPosition = transform.position;
            UpdateStats(timestamp);
        }

        void UpdateStats(float timestamp) {
            float clientDeltaTime = Time.realtimeSinceStartup - lastMessageReceivedTime;
            float serverDeltaTime = timestamp - lastTimestamp;
            if (Mathf.Approximately(clockOffset, 0f)) {
                clockOffset = Time.realtimeSinceStartup - timestamp;
            }
            float messageClockOffset = (Time.realtimeSinceStartup - timestamp) - clockOffset;
            timeToTarget = interpolationTime - messageClockOffset;

            lastMessageReceivedTime = Time.realtimeSinceStartup;
            lastTimestamp = timestamp;
            clockOffset = (clockOffset * 4 + (Time.realtimeSinceStartup - timestamp)) / 5;
        }

        bool NeedsSnap(Vector2 sourcePosition, Vector2 targetPosition) {
            if ((targetPosition - sourcePosition).magnitude > snapDistance) {
                if (Mathf.Approximately(snapTimer, 0f)) {
                    snapTimer = Time.realtimeSinceStartup;
                } else {
                    if (Time.realtimeSinceStartup - snapTimer > interpolationTime) {
                        snapTimer = 0;
                        return true;
                    }
                }
            } else {
                snapTimer = 0;
            }

            return false;
        }

        void Interpolate() {
            var dt = Time.realtimeSinceStartup - lastMessageReceivedTime;
            float interpTime = dt / timeToTarget;

            if (PositionCloseEnough(transform.position, targetPosition) || NeedsSnap(transform.position, targetPosition)) {
                transform.position = targetPosition;
            } else {
                transform.position = Vector2.Lerp(transform.position, targetPosition, interpTime);
            }
        }

        public override int GetNetworkChannel() {
            return 2;
        }

        public override float GetNetworkSendInterval() {
            return syncInterval;
        }
    }
}