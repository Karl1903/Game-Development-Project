using UnityEngine;

namespace Utils {
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class ReadOnlyAttribute : PropertyAttribute { }
}