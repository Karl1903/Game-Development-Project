using UnityEngine;

namespace Utils {
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class ShowIfAttribute : PropertyAttribute {
        public readonly string fieldName;
        public readonly object comparedValue;
        public readonly string customDrawerClassName;

        public ShowIfAttribute(string fieldName, object comparedValue, string customDrawerClassName = null) {
            this.fieldName = fieldName;
            this.comparedValue = comparedValue;
            this.customDrawerClassName = customDrawerClassName;
        }
    }
}