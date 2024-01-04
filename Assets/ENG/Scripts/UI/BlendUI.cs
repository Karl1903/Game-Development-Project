using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

namespace UI {
    public class BlendUI : MonoBehaviour {
        public static BlendUI Inst { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(0f)] private float defaultFadeDuration = 1f;

        public bool BlendIsShown => gameObject.activeInHierarchy && canvasGroup.alpha > 0f;

        private void Awake() {
            // Singleton handling
            if (Inst) {
                Destroy(gameObject);
                return;
            } else {
                // Dont destroy on load handled by the ManagersInit script
                Inst = this;
            }

            gameObject.SetActive(false);
        }

        public async Task FadeOut(float? duration = null) {
            gameObject.SetActive(true);
            if (duration == null) duration = defaultFadeDuration;
            StartCoroutine(FadeRoutine(1f, (float)duration));
            await Task.Delay((int)(duration * 1000f));
            ShowBlend(); // For safety make sure that the alpha is at 1
        }

        public async Task FadeIn(float? duration = null) {
            if (duration == null) duration = defaultFadeDuration;
            StartCoroutine(FadeRoutine(0f, (float)duration));
            await Task.Delay((int)(duration * 1000f));
            HideBlend(); // For safety make sure that the alpha is at 0
            gameObject.SetActive(false);
        }

        public async Task FadeTo(float targetAlpha, float? duration = null) {
            gameObject.SetActive(true);
            if (duration == null) duration = defaultFadeDuration;
            StartCoroutine(FadeRoutine(1f - targetAlpha, (float)duration));
            await Task.Delay((int)(duration * 1000f));
            SetBlend(targetAlpha); // For safety make sure that the alpha is at the target alpha
            if (targetAlpha == 0f) gameObject.SetActive(false);
        }

        public void SetBlend(float alpha) {
            gameObject.SetActive(alpha > 0f);
            canvasGroup.alpha = alpha;
        }

        public void ShowBlend() {
            SetBlend(1f);
        }

        public void HideBlend() {
            SetBlend(0f);
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration) {
            float alphaDiff = targetAlpha - canvasGroup.alpha;

            Func<bool> fadeCondition;
            if (targetAlpha > canvasGroup.alpha) fadeCondition = () => canvasGroup.alpha < targetAlpha;
            else fadeCondition = () => canvasGroup.alpha > targetAlpha;

            while (fadeCondition.Invoke()) {
                canvasGroup.alpha += alphaDiff / (duration / Time.deltaTime);
                yield return null;
            }
        }
    }
}