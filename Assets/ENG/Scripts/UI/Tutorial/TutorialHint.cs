using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Tutorial {
    [System.Serializable]
    public struct TutorialHint {
        [Header("Hint")]
        public Sprite[] keyboardImages;
        public Sprite[] gamepadImages;
        [Multiline] public string text;

        [Header("Dismiss")]
        public InputActionReference dismissAfterInput;
        public bool dismissAfterTime;
        [Min(0f)] public float dismissTime;
    }
}