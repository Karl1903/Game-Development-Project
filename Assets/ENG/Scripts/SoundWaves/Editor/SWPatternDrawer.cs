using UnityEngine;
using UnityEditor;

namespace SoundWaves.Editors {
    [CustomPropertyDrawer(typeof(SWPattern))]
    public class SWPatternDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty wavesProp = property.FindPropertyRelative("waves");
            return EditorGUI.GetPropertyHeight(wavesProp, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // If expanded also show the pattern duration
            SerializedProperty wavesProp = property.FindPropertyRelative("waves");
            if (wavesProp.isExpanded) {
                float maxDuration = 0f;
                for (int i = 0; i < wavesProp.arraySize; i++) {
                    SerializedProperty waveProp = wavesProp.GetArrayElementAtIndex(i);
                    SerializedProperty waveParamsProp = waveProp.FindPropertyRelative("parameters");
                    float duration = waveParamsProp.FindPropertyRelative("soundRadius").floatValue / waveParamsProp.FindPropertyRelative("soundSpeed").floatValue;
                    duration = waveProp.FindPropertyRelative("timeOffset").floatValue + duration;
                    if (duration > maxDuration) maxDuration = duration;
                }

                label.text += $" ({maxDuration.ToString("0.00")}s)";
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, wavesProp, label, true);
            EditorGUI.EndProperty();
        }
    }
}