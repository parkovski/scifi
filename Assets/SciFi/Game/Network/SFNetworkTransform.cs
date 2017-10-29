using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    public class SFNetworkTransform : NetworkBehaviour {
        public bool useDefaults = true;
        /// How often sync messages will be broadcast.
        public float syncInterval = SFNetworkTransformGlobalParams.syncInterval;
        /// Constant client lag factor - this is kept constant
        /// by synchronizing the client and server clocks.
        public float interpolationTime = SFNetworkTransformGlobalParams.interpolationTime;
        /// Threshold for sending position updates - if the position
        /// has changed by less than this value, an update will not be sent.
        public float closeEnoughPosition = SFNetworkTransformGlobalParams.closeEnoughPosition;
        /// Threshold for sending velocity updates - if the velocity
        /// has changed by less than this value, an update will not be sent.
        public float closeEnoughVelocity = SFNetworkTransformGlobalParams.closeEnoughVelocity;
        /// Max distance the object can be out of sync by for one
        /// interpolation period before it snaps to the new position.
        public float snapDistance = SFNetworkTransformGlobalParams.snapDistance;

        /// When the object gets out of sync, this timer starts, and after
        /// one interpolation period, it will snap to the new position.
        float snapTimer;
        float lastMessageSentTime;
        float lastMessageReceivedTime;
        /// How long it takes to reach the new position. This is equal to
        /// the interpolation period (the constant lag) minus the lag for
        /// the message, calculated using the clock offset.
        float timeToTarget;
        /// Local time for the last message received.
        float lastTimestamp;
        /// Where we want to end up at the end of the current time period.
        Vector2 targetPosition;
        Rigidbody2D rb;
        /// Used to identify the sender in CmdSyncState.
        NetworkIdentity networkIdentity;

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            networkIdentity = GetComponent<NetworkIdentity>();

            targetPosition = transform.position;
            lastMessageReceivedTime = Time.realtimeSinceStartup;

            if (useDefaults) {
                syncInterval = SFNetworkTransformGlobalParams.syncInterval;
                interpolationTime = SFNetworkTransformGlobalParams.interpolationTime;
                closeEnoughPosition = SFNetworkTransformGlobalParams.closeEnoughPosition;
                closeEnoughVelocity = SFNetworkTransformGlobalParams.closeEnoughVelocity;
                snapDistance = SFNetworkTransformGlobalParams.snapDistance;
            }
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

            if (hasAuthority) {
                if (Time.realtimeSinceStartup > lastMessageSentTime + syncInterval) {
                    if (!PositionCloseEnough(transform.position, targetPosition)) {
                        lastMessageSentTime = Time.realtimeSinceStartup;
                        targetPosition = transform.position;
                        if (isServer) {
                            RpcSyncState(targetPosition, lastMessageSentTime);
                        } else {
                            CmdSyncState(targetPosition, lastMessageSentTime);
                        }
                    }
                }
            } else {
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

        /// This should only be called from player objects - they
        /// track their client connections.
        [Command]
        void CmdSyncState(Vector2 position, float timestamp) {
            var conn = networkIdentity.clientAuthorityOwner;
            if (conn == null) {
                return;
            }
            var clockOffset = NetworkController.GetClientClockOffset(conn);
            timestamp += clockOffset.Value;
            if (timestamp < lastTimestamp) {
                return;
            }
            targetPosition = position;
            UpdateStats(timestamp);
            RpcSyncState(position, timestamp);
        }

        [ClientRpc]
        void RpcSyncState(Vector2 position, float timestamp) {
            if (isServer || hasAuthority) {
                return;
            }
            timestamp += NetworkController.serverClock.clockOffset;
            if (timestamp < lastTimestamp) {
                return;
            }
            targetPosition = position;
            UpdateStats(timestamp);
        }

        [Command]
        void CmdSnapTo(Vector2 position) {
            SnapTo(position);
            RpcSnapTo(position);
        }

        [ClientRpc]
        void RpcSnapTo(Vector2 position) {
            if (isServer || hasAuthority) {
                return;
            }
            SnapTo(position);
        }

        /// Timestamp is already corrected with the remote clock offset.
        /// It represents the local time when the remote object was at the updated position.
        void UpdateStats(float timestamp) {
            /// Local time since the object was at this position
            float deltaTime = Time.realtimeSinceStartup - timestamp;
            if (deltaTime > interpolationTime) {
                timeToTarget = interpolationTime;
            } else {
                timeToTarget = interpolationTime - deltaTime;
            }

            lastMessageReceivedTime = Time.realtimeSinceStartup;
            lastTimestamp = timestamp;
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

        /// Must be called on an authoritative copy.
        public void SnapTo(Vector2 position) {
            targetPosition = position;
            transform.position = position;
            if (isServer) {
                RpcSnapTo(position);
            } else {
                CmdSnapTo(position);
            }
        }

        public override int GetNetworkChannel() {
            return 2;
        }

        public override float GetNetworkSendInterval() {
            return syncInterval;
        }
    }

    public static class SFNetworkTransformGlobalParams {
        public const float syncInterval = 0.05f;
        public const float interpolationTime = 0.1f;
        public const float closeEnoughPosition = 0.01f;
        public const float closeEnoughVelocity = 0.1f;
        public const float snapDistance = 3f;
    }
}