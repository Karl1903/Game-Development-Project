using UnityEngine;
using SoundWaves;

namespace NPC {
    [System.Serializable]
    public struct NPCBrainSoundInput {
        public float timestamp; // Time.time timestamp
        public Vector3 soundOrigin; // Sound position
        public float soundDistance; // Initial distance to the sound
        public SoundTag soundTag; // Sound identifier

        public NPCBrainSoundInput(float timestamp, Vector3 soundOrigin, float soundDistance, SoundTag soundTag) {
            this.timestamp = timestamp;
            this.soundOrigin = soundOrigin;
            this.soundDistance = soundDistance;
            this.soundTag = soundTag;
        }
    }
}