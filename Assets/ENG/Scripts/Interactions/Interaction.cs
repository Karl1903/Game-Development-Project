using System;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions {
    [RequireComponent(typeof(Collider))]
    public class Interaction : MonoBehaviour {
        [SerializeField] private UnityEvent onInteract;
        public UnityEvent OnInteract => onInteract;

        [SerializeField, Tooltip("The player will teleport to this position when interacting")]
        private Transform teleportPlayerTo;
        public Transform TeleportPlayerTo => teleportPlayerTo;

        [SerializeField] private bool triggerOnce = false;
        [SerializeField, Range(0.0f, 1.0f)] private float highlightIntensity = 1;

        private bool triggered = false;
        private MaterialPropertyBlock properties;

        private void Awake() {
            properties = new MaterialPropertyBlock();
            UpdateHighlightIntensity();
        }

        public void SetHighlight(bool isHighlighted) {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>()) {
                renderer.GetPropertyBlock(properties);
                properties.SetInteger("_IsHighlighted", System.Convert.ToInt32(isHighlighted)); // Highlight handled by SWS shader
                renderer.SetPropertyBlock(properties);
            }
        }

        public bool Interact() {
            if (triggerOnce) {
                if (triggered) return false;
                else triggered = true;
            }
            onInteract?.Invoke();
            return true;
        }

        private void UpdateHighlightIntensity() {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>()) {
                properties = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(properties);
                properties.SetFloat("_HighlightIntensity", highlightIntensity); // Highlight handled by SWS shader
                renderer.SetPropertyBlock(properties);
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (Application.isPlaying) UpdateHighlightIntensity();
        }
#endif
    }
}