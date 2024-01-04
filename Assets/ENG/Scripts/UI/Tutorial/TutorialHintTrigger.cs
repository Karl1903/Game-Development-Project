using UnityEngine;
using Player;

namespace UI.Tutorial {
    public class TutorialHintTrigger : MonoBehaviour {
        [SerializeField] private TutorialHint hint;

        [Header("Trigger")]
        [Tooltip("Trigger collider needed on this gameobject for this to work")]
        [SerializeField] private bool triggerOnPlayer = true;
        [SerializeField] private bool triggerOnStart = false;
        [SerializeField] private bool retriggerable = false;

        private bool triggered = false;

        private void Start() {
            if (triggerOnStart) Trigger();
        }

        private void OnTriggerEnter(Collider other) {
            if (triggerOnPlayer && other.CompareTag(PlayerController.PLAYER_TAG))
                Trigger();
        }

        [ContextMenu("Trigger")]
        public void Trigger() {
            if (!retriggerable && triggered) return;
            triggered = true;
            TutorialHintManager.Inst.ShowHint(hint);
        }
    }
}