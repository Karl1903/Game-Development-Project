using UnityEngine;
using UnityEditor;

namespace SoundWaves.Editors {
    [CustomPropertyDrawer(typeof(SWPatternWave))]
    public class SWPatternWaveDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Switch "Element" with "Wave"
            label.text = label.text.Split(" ")[1];
            label.text = $"Wave {int.Parse(label.text) + 1}:        ";

            // Show time offset and end time of the wave in the header
            SerializedProperty waveParamsProp = property.FindPropertyRelative("parameters");
            float soundRadius = waveParamsProp.FindPropertyRelative("soundRadius").floatValue;
            float soundSpeed = waveParamsProp.FindPropertyRelative("soundSpeed").floatValue;
            float timeOffset = property.FindPropertyRelative("timeOffset").floatValue;
            float duration = timeOffset + soundRadius / soundSpeed;
            string durationTxt = duration.ToString("0.00") + "s";
            string timeOffsetTxt = timeOffset.ToString("0.00") + "s";

            label.text += $"{timeOffsetTxt}    >>>    {durationTxt}";

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }
    }
}