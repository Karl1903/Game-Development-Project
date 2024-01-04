using UnityEngine;
using UnityEngine.Events;

namespace SoundWaves {
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class SoundReceiver : MonoBehaviour, ISoundReceiver {
        public const string SOUND_NOTIFIER_TAG = "SoundNotifier";

        [SerializeField] private UnityEvent<SoundNotifier> onSound = new UnityEvent<SoundNotifier>();
        public UnityEvent<SoundNotifier> OnSound => onSound;

        public void OnTriggerEnter(Collider other) {
            if (other.tag == SOUND_NOTIFIER_TAG) {
                onSound?.Invoke(other.GetComponent<SoundNotifier>());
            }
        }
    }
}