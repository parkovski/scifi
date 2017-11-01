namespace SciFi.Editor {
    using System;
    using UnityEngine;
    using UnityEditor;
    using SciFi.UI;

    [CustomPropertyDrawer(typeof(RefreshButton))]
    [CanEditMultipleObjects]
    public class RefreshButtonEditor : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            if (GUI.Button(position, label)) {
                var action = property.FindPropertyRelative("action").stringValue;
                foreach (var o in property.serializedObject.targetObjects) {
                    (o as IRefreshComponent)?.RefreshComponent(action);
                }
            }

            EditorGUI.EndProperty();
        }
    }
}