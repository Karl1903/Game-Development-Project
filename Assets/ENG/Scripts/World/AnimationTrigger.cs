using UnityEngine;

namespace World {
    [RequireComponent(typeof(Animator))]
    public class AnimationTrigger : MonoBehaviour {
        [SerializeField] private string triggerName;

        private Animator animator;

        private void Awake() {
            animator = GetComponent<Animator>();
        }

        [ContextMenu("Trigger")]
        public void Trigger() {
            animator.SetTrigger(triggerName);
        }
    }
}