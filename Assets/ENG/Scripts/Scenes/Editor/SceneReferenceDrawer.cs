using UnityEngine;
using UnityEditor;

namespace Scenes.Editors {
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty scenePathProp = property.FindPropertyRelative("scenePath");
            if (label.text == scenePathProp.stringValue) label.text = SceneReference.GetSceneName(label.text);
            SceneAsset selScene = EditorGUI.ObjectField(
                position,
                label,
                AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProp.stringValue),
                typeof(SceneAsset),
                false
            ) as SceneAsset;
            if (selScene) scenePathProp.stringValue = AssetDatabase.GetAssetPath(selScene);
            else scenePathProp.stringValue = "";
        }
    }
}