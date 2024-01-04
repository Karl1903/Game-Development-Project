using UnityEngine;
using UnityEditor;

namespace SoundWaves.Editors {
    [CustomPropertyDrawer(typeof(SWParams))]
    public class SWParamsDrawer : PropertyDrawer {
        private readonly float LINE_HEIGHT = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true) + (property.isExpanded ? LINE_HEIGHT : 0f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Draw default property
            EditorGUI.PropertyField(position, property, label, true);

            // If expanded also show the wave duration
            if (property.isExpanded) {
                EditorGUI.indentLevel++;

                float soundRadius = property.FindPropertyRelative("soundRadius").floatValue;
                float soundSpeed = property.FindPropertyRelative("soundSpeed").floatValue;
                float duration = soundRadius / soundSpeed;
                Rect durationRect = position;
                durationRect.height = EditorGUIUtility.singleLineHeight;
                durationRect.y += position.height - LINE_HEIGHT;

                Color tempColor = GUI.contentColor;
                GUI.contentColor = Color.cyan;
                EditorGUI.LabelField(durationRect, $"Duration: {duration.ToString("0.00")}s", EditorStyles.boldLabel);
                GUI.contentColor = tempColor;

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}