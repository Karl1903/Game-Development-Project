using UnityEngine;

namespace SoundWaves {
    [System.Serializable]
    public struct SWPatternWave {
        [Min(0f)] public float timeOffset; // absolute time offset from the start
        public SWParams parameters;

        public SWPatternWave(float timeOffset, SWParams waveParams) {
            this.timeOffset = timeOffset;
            this.parameters = waveParams;
        }

        /// <summary>
        /// time offset + wave duration
        /// </summary>
        public float GetDuration() {
            return timeOffset + parameters.GetDuration();
        }
    }
}