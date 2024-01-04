using UnityEngine;
using Utils;

namespace UI {
    public class BlendTrigger : MonoBehaviour {
        [Header("Blend Settings")]
        [SerializeField] private bool fadeIn = false;
        [SerializeField] private bool fadeOut = false;
        [SerializeField] private bool fadeTo = false;
        [SerializeField, ShowIf(nameof(fadeTo), true), Range(0f, 1f)] private float fadeToAlpha = 0f;
        [SerializeField] private bool showBlend = false;
        [SerializeField] private bool hideBlend = false;
        [SerializeField] private bool setBlend = false;
        [SerializeField, ShowIf(nameof(setBlend), true), Range(0f, 1f)] private float setBlendAlpha = 0f;

        [Header("Trigger")]
        [SerializeField] private bool triggerOnStart = false;

        private void Start() {
            if (triggerOnStart) Trigger();
        }

        [ContextMenu("Trigger")]
        public void Trigger() {
            if (fadeIn) _ = BlendUI.Inst.FadeIn();
            if (fadeOut) _ = BlendUI.Inst.FadeOut();
            if (fadeTo) _ = BlendUI.Inst.FadeTo(fadeToAlpha);
            if (showBlend) BlendUI.Inst.ShowBlend();
            if (hideBlend) BlendUI.Inst.HideBlend();
            if (setBlend) BlendUI.Inst.SetBlend(setBlendAlpha);
        }
    }
}