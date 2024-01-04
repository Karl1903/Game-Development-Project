using System.Collections;
using UnityEngine;
using Utils;

namespace SoundWaves {
    public class SoundWave : MonoBehaviour {
        [Header("Trigger")]
        [SerializeField] private bool triggerOnEnable = false;
        [SerializeField, ShowIf(nameof(triggerOnEnable), true)] private float delayOnEnable = 0f;

        [SerializeField] private bool triggerOnStart = true;
        [SerializeField, ShowIf(nameof(triggerOnStart), true)] private float delayOnStart = 0f;

        [Header("Sound Origin")]
        [SerializeField] private bool thisSoundOrigin = true;
        [SerializeField, ShowIf(nameof(thisSoundOrigin), false)] private Transform otherSoundOrigin;

        [Header("Sound Wave")]
        [SerializeField] private SoundTag soundTag = SoundTag.General;
        [SerializeField] private bool usePattern = false;
        [SerializeField, ShowIf(nameof(usePattern), false, "SWParamsDrawer")] private SWParams parameters = SWParams.DEFAULT;
        [SerializeField, ShowIf(nameof(usePattern), true, "SWPatternDrawer")] private SWPattern pattern = SWPattern.DEFAULT;

        private void OnEnable() {
            if (triggerOnEnable) {
                StartCoroutine(OnEnableRoutine());
            }
        }

        private IEnumerator Start() {
            if (triggerOnStart) {
                if (delayOnStart > 0f) yield return new WaitForSeconds(delayOnStart);
                Trigger();
            }
        }

        public void Trigger() {
            if (usePattern) SoundWaveManager.Inst.AddSoundWavePattern(gameObject, thisSoundOrigin ? transform : otherSoundOrigin, soundTag, pattern);
            else SoundWaveManager.Inst.AddSoundWave(gameObject, thisSoundOrigin ? transform.position : otherSoundOrigin.position, soundTag, parameters);
        }

        private IEnumerator OnEnableRoutine() {
            if (delayOnEnable > 0f) yield return new WaitForSeconds(delayOnEnable);
            Trigger();
        }
    }
}
