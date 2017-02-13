using UnityEngine;
using UnityEngine.Networking;

namespace SciFi.Network {
    public class SFNetworkTransform : NetworkBehaviour {
        public bool useDefaults = true;
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
            originalPosition = transform.position;
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
            originalPosition = transform.position;
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
        public static float syncInterval = 0.05f;
        public static float interpolationTime = 0.1f;
        public static float closeEnoughPosition = 0.01f;
        public static float closeEnoughVelocity = 0.1f;
        public static float snapDistance = 3f;
    }
}

namespace SciFi.Editor {
    using UnityEditor;
    using SFNetworkTransform = SciFi.Network.SFNetworkTransform;
    using GP = SciFi.Network.SFNetworkTransformGlobalParams;

    [CustomEditor(typeof(SFNetworkTransform))]
    [CanEditMultipleObjects]
    public class SFNetworkTransformEditor : Editor {
        SerializedProperty useDefaults;
        SerializedProperty syncInterval;
        SerializedProperty interpolationTime;
        SerializedProperty closeEnoughPosition;
        SerializedProperty closeEnoughVelocity;
        SerializedProperty snapDistance;

        void OnEnable() {
            useDefaults = serializedObject.FindProperty("useDefaults");
            syncInterval = serializedObject.FindProperty("syncInterval");
            interpolationTime = serializedObject.FindProperty("interpolationTime");
            closeEnoughPosition = serializedObject.FindProperty("closeEnoughPosition");
            closeEnoughVelocity = serializedObject.FindProperty("closeEnoughVelocity");
            snapDistance = serializedObject.FindProperty("snapDistance");
        }

        void ShowFloatField(string label, SerializedProperty property, float globalDefault, bool useDefault) {
            float floatValue = 0f;
            EditorGUI.BeginChangeCheck();

            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            if (useDefault) {
                floatValue = EditorGUILayout.FloatField(label, property.floatValue);
            } else {
                EditorGUILayout.LabelField(label, globalDefault.ToString());
            }

            if (EditorGUI.EndChangeCheck()) {
                property.floatValue = floatValue;
            }
        }

        public override void OnInspectorGUI() {
            bool boolValue = false;
            bool fieldsAreEditable = false;

            EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = useDefaults.hasMultipleDifferentValues;
                if (!useDefaults.hasMultipleDifferentValues) {
                    boolValue = EditorGUILayout.Toggle("Use Global Defaults", useDefaults.boolValue);
                }
            if (EditorGUI.EndChangeCheck()) {
                useDefaults.boolValue = boolValue;
            }
            fieldsAreEditable = !useDefaults.hasMultipleDifferentValues && !boolValue;

            if (!fieldsAreEditable) {
                EditorGUILayout.HelpBox("Globals are defined in the SFNetworkTransformGlobalParams class.", MessageType.Info, true);
            }

            ShowFloatField("Sync Interval", syncInterval, GP.syncInterval, fieldsAreEditable);
            ShowFloatField("Interpolation Time", interpolationTime, GP.interpolationTime, fieldsAreEditable);
            ShowFloatField("Close Enough Position", closeEnoughPosition, GP.closeEnoughPosition, fieldsAreEditable);
            ShowFloatField("Close Enough Velocity", closeEnoughVelocity, GP.closeEnoughVelocity, fieldsAreEditable);
            ShowFloatField("Snap Distance", snapDistance, GP.snapDistance, fieldsAreEditable);

            serializedObject.ApplyModifiedProperties();
        }
    }
}