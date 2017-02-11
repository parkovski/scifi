using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;

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
        Vector2 targetPosition;
        Vector2 originalPosition;
        Rigidbody2D rb;
        /// Null if none, but only players can call CmdSyncState.
        Player player;

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            player = GetComponent<Player>();

            targetPosition = transform.position;
            lastMessageReceivedTime = Time.realtimeSinceStartup;
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

        /// This should only be called from player objects - they
        /// track their client connections.
        [Command]
        void CmdSyncState(Vector2 position, float timestamp) {
            var conn = GameController.Instance.ConnectionForPlayer(player.eId);
            var clockOffset = NetworkController.GetClientClockOffset(conn);
            timestamp += clockOffset.Value;
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
            timestamp += NetworkController.serverClock.clockOffset;
            if (timestamp < lastTimestamp) {
                return;
            }
            targetPosition = position;
            originalPosition = transform.position;
            UpdateStats(timestamp);
        }

        /// Timestamp is already corrected with the remote clock offset.
        /// It represents the local time when the remote object was at the updated position.
        void UpdateStats(float timestamp) {
            /// Local time since the object was at this position
            float clientDeltaTime = Time.realtimeSinceStartup - timestamp;
            float serverDeltaTime = timestamp - lastTimestamp;
            if (serverDeltaTime > interpolationTime) {
                timeToTarget = interpolationTime;
            } else {
                timeToTarget = interpolationTime - serverDeltaTime;
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

        public override int GetNetworkChannel() {
            return 2;
        }

        public override float GetNetworkSendInterval() {
            return syncInterval;
        }
    }
}