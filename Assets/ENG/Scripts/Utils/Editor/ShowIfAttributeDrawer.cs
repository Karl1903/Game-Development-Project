using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Utils.Editors {
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (Show(property)) {
                PropertyDrawer pd = GetCustomDrawer(property);
                return pd != null ? pd.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);
            } else return 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (Show(property)) {
                PropertyDrawer pd = GetCustomDrawer(property);
                if (pd != null) pd.OnGUI(position, property, label);
                else EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private bool Show(SerializedProperty property) {
            ShowIfAttribute attr = attribute as ShowIfAttribute;
            SerializedProperty targetProp = property.serializedObject.FindProperty(attr.fieldName);

            if (targetProp == null) {
                Debug.LogWarning($"ShowIfAttribute for <{property.name}> could not find fieldName <{attr.fieldName}>");
                return false;
            }

            try {
                switch (targetProp.type) {
                    case "bool":
                        return targetProp.boolValue == (bool)attr.comparedValue;
                    case "int":
                        return targetProp.intValue == (int)attr.comparedValue;
                    case "float":
                        return targetProp.floatValue == (float)attr.comparedValue;
                    case "string":
                        return targetProp.stringValue == (string)attr.comparedValue;
                    case "Enum":
                        return targetProp.enumValueIndex == (int)attr.comparedValue;
                    default:
                        Debug.LogWarning($"ShowIfAttribute for <{property.name}> references unsupported fieldName type <{targetProp.type}>");
                        return false;
                }
            } catch (System.InvalidCastException) {
                Debug.LogWarning($"ShowIfAttribute for <{property.name}>: the comparedValue's type <{attr.comparedValue.GetType().Name}> does not match with fieldNames's type <{targetProp.type}>");
                return false;
            }
        }

        private PropertyDrawer GetCustomDrawer(SerializedProperty property) {
            ShowIfAttribute attr = attribute as ShowIfAttribute;
            if (attr.customDrawerClassName != null) {
                Type[] types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (Type type in types) {
                    if (type.Name == attr.customDrawerClassName) return (PropertyDrawer)Activator.CreateInstance(type);
                }
                Debug.LogWarning($"ShowIfAttribute for <{property.name}>: the supplied custom property drawer class name <{attr.customDrawerClassName}> does not exist");
                return null;
            } else return null;
        }
    }
}
