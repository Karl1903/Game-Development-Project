using UnityEngine;

namespace SoundWaves {
    /// <summary>
    /// Contains the parameters for the Sound Wave Shader (SWS). Calling the non-default constructor is recommended.
    /// </summary>
    [System.Serializable]
    public struct SWParams {
        public static readonly Color DEFAULT_WAVE_COLOR = new Color(2.0392158f, 3.60784316f, 5.96078444f, 1f);
        public static readonly SWParams DEFAULT = new SWParams(waveColor: SWParams.DEFAULT_WAVE_COLOR);

        [Min(0f)] public float soundRadius;
        [Min(0f)] public float soundSpeed;
        [Min(0f)] public float waveWidth;
        public bool hardWave;
        public bool softWave;
        [ColorUsage(true, true)] public Color waveColor; // HDR supported for bloom post process effect
        [Range(0f, 3f)] public float waveFadeOutStrength;
        [Range(0f, 0.3f)] public float waveBorderWidth;

        /// <summary>
        /// Set some of the Sound Wave Shader (SWS) parameters. You should at least set the  <paramref name="waveColor"/>, because otherwise it will be set to black (not visible).
        /// </summary>
        /// <param name="soundRadius">Radius of the sound waves in meters</param>
        /// <param name="soundSpeed">Speed of the sound waves in m/s</param>
        /// <param name="waveWidth">Width of a wave in meters</param>
        /// <param name="hardWave">If true, the wave has a hard edge</param>
        /// <param name="softWave">If true, the wave looks more like a glow</param>
        /// <param name="waveColor">Color of a wave, HDR is supported</param>
        /// <param name="waveFadeOutStrength">Fade out speed of a wave</param>
        /// <param name="waveBorderWidth">Width of the border of a wave</param>
        public SWParams(float soundRadius = 20f, float soundSpeed = 8f, float waveWidth = 5f, bool hardWave = true, bool softWave = false, Color waveColor = default, float waveFadeOutStrength = 2f, float waveBorderWidth = 0.15f) {
            this.soundRadius = soundRadius;
            this.soundSpeed = soundSpeed;
            this.waveWidth = waveWidth;
            this.hardWave = hardWave;
            this.softWave = softWave;
            this.waveColor = waveColor;
            this.waveFadeOutStrength = waveFadeOutStrength;
            this.waveBorderWidth = waveBorderWidth;
        }

        public float GetDuration() {
            return soundRadius / soundSpeed;
        }
    }
}