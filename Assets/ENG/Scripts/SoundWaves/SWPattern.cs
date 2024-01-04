namespace SoundWaves {
    [System.Serializable]
    public struct SWPattern {
        public static readonly SWPattern DEFAULT = new SWPattern(new SWPatternWave[] { new SWPatternWave(0f, SWParams.DEFAULT) });

        public SWPatternWave[] waves;

        public SWPattern(SWPatternWave[] waves) {
            this.waves = waves;
        }

        public float GetDuration() {
            float max = 0f;
            for (int i = 0; i < waves.Length; i++) {
                float curr = waves[i].GetDuration();
                if (curr > max) max = curr;
            }
            return max;
        }
    }
}