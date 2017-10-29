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