using UnityEngine;
using UnityEngine.Audio;
using Player;
using Utils;

namespace Sounds {
    public class SoundTriggerAtPosition : MonoBehaviour {
        [Header("Sound")]
        [SerializeField] private string nameOfSound = "TriggeredAtPos";
        [SerializeField] private RandomSound clips;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField, Min(0f)] private float volume = 1f;
        [SerializeField] private bool loop = false;
        [SerializeField, Min(0f)] private float delay = 0f;
        [SerializeField, Min(0f)] private float pitch = 1f;
        [SerializeField] private bool otherSoundOrigin = false;
        [SerializeField, ShowIf(nameof(otherSoundOrigin), true)]
        private Transform otherSoundOriginTransform = null;

        [Header("Trigger")]
        [SerializeField] private bool retriggerable = false;
        [SerializeField] private bool triggerOnEnable = false;
        [SerializeField] private bool triggerOnStart = false;
        [SerializeField, Tooltip("If true a collider on the component's gameobject is needed")]
        private bool triggerOnPlayer = false;

        private Collider coll;
        private bool triggered = false;

        private void Awake() {
            if (triggerOnPlayer) {
                coll = GetComponent<Collider>();
                if (coll == null) Debug.LogWarning("SoundTriggerAtPosition: triggerOnPlayer is true, but the gameobject does not have a collider");
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
            SoundManager.Inst.PlayAtPosition(nameOfSound, otherSoundOrigin ? otherSoundOriginTransform.position : transform.position, clips.GetRandom(), mixerGroup, volume, loop, delay, pitch);
        }

        private void OnTriggerEnter(Collider other) {
            if (!triggerOnPlayer) return;
            if (other.tag == PlayerController.PLAYER_TAG) {
                Trigger();
            }
        }
    }
}