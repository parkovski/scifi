using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players;

namespace SciFi.Network {
    [NetworkSettings(channel = 2)]
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

        public override void OnStartServer() {
            NetworkServer.RegisterHandler(NetworkMessages.SyncPosition, ServerSyncPosition);
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
                        var writer = new NetworkWriter();
                        writer.StartMessage(NetworkMessages.SyncPosition);
                        writer.Write((Vector2)transform.position);
                        writer.Write(Time.realtimeSinceStartup);
                        writer.FinishMessage();
                        NetworkController.clientConnectionToServer.SendWriter(writer, 2);
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
                        var writer = new NetworkWriter();
                        writer.StartMessage(NetworkMessages.SyncPosition);
                        writer.Write((Vector2)transform.position);
                        writer.Write(Time.realtimeSinceStartup);
                        writer.FinishMessage();
                        foreach (var conn in NetworkServer.connections) {
                            conn.SendWriter(writer, 2);
                        }
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

        /// Assumes the connection is tracked by NetworkController.
        void ServerSyncPosition(NetworkMessage msg) {
            var position = msg.reader.ReadVector2();
            var timestamp = msg.reader.ReadSingle();

            SyncPosition(position, timestamp, NetworkController.GetClientClockOffset(msg.conn).Value);

            var writer = new NetworkWriter();
            writer.StartMessage(NetworkMessages.SyncPosition);
            writer.Write(position);
            writer.Write(timestamp);
            writer.FinishMessage();
            foreach (var conn in NetworkServer.connections) {
                conn.SendWriter(writer, 2);
            }
        }

        void ClientSyncPosition(NetworkMessage msg) {
            var position = msg.reader.ReadVector2();
            var timestamp = msg.reader.ReadSingle();

            SyncPosition(position, timestamp, NetworkController.serverClock.clockOffset);
        }

        void SyncPosition(Vector2 position, float timestamp, float clockOffset) {
            timestamp += clockOffset;
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
            if (clientDeltaTime > interpolationTime) {
                timeToTarget = interpolationTime;
            } else {
                timeToTarget = interpolationTime - clientDeltaTime;
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
    }
}