using UnityEngine;
using UnityEngine.Audio;
using Player;

namespace Sounds {
    public class SoundTrigger : MonoBehaviour {
        [Header("Sound")]
        [SerializeField] private AudioClip clip;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField, Min(0f)] private float volume = 1f;
        [SerializeField] private bool loop = false;
        [SerializeField, Min(0f)] private float delay = 0f;
        [SerializeField, Min(0f)] private float pitch = 1f;

        [Header("Mode")]
        [Tooltip("If false the clip is stopped")]
        [SerializeField] private bool play = true;

        [Header("Trigger")]
        [SerializeField] private bool retriggerable = false;
        [SerializeField] private bool triggerOnEnable = false;
        [SerializeField] private bool triggerOnStart = false;
        [SerializeField, Tooltip("If true a collider on the component's gameobject is needed")]
        private bool triggerOnPlayer = false;

        private AudioSource linkedSource = null;
        private Collider coll;

        private bool triggered = false;

        private void Awake() {
            if (triggerOnPlayer) {
                coll = GetComponent<Collider>();
                if (coll == null) Debug.LogWarning("SoundTrigger: triggerOnPlayer is true, but the gameobject does not have a collider");
                else coll.isTrigger = true;
            }
        }

        private void OnEnable() {
            if (triggerOnEnable) Trigger();
        }

        private void Start() {
            if (triggerOnStart) Trigger();
        }

        [ContextMenu("Trigger")]
        public void Trigger() {
            if (!retriggerable && triggered) return;
            triggered = true;
            if (play) linkedSource = SoundManager.Inst.Play(clip, mixerGroup, volume, loop, delay, pitch);
            else {
                if (!SoundManager.Inst.Stop(linkedSource)) {
                    // Try to stop the clip by name if the linked source was not found/active
                    if (!SoundManager.Inst.Stop(clip)) Debug.LogWarning("SoundTrigger: clip could not be stopped, because it is not playing");
                }
                linkedSource = null;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!triggerOnPlayer) return;
            if (other.tag == PlayerController.PLAYER_TAG) {
                Trigger();
            }
        }
    }
}