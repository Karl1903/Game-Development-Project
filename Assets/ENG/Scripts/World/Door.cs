using UnityEngine;
using Sounds;
using SoundWaves;

namespace World {
    [RequireComponent(typeof(AnimationTrigger), typeof(SoundTriggerAtPosition), typeof(SoundWave))]
    public class Door : MonoBehaviour {
        private AnimationTrigger anim;
        private SoundTriggerAtPosition sound;
        private SoundWave soundWave;

        private void Awake() {
            anim = GetComponent<AnimationTrigger>();
            sound = GetComponent<SoundTriggerAtPosition>();
            soundWave = GetComponent<SoundWave>();
        }

        [ContextMenu("Open")]
        public void Open() {
            anim.Trigger();
            sound.Trigger();
            soundWave.Trigger();
        }
    }
}