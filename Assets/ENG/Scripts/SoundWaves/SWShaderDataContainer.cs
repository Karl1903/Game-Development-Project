using UnityEngine;

namespace SoundWaves {
    [System.Serializable]
    public class SWShaderDataContainer {
        private const int MAX_SOUNDS = SoundWaveManager.MAX_SOUNDS; // Shorten var name

        private const float MASK_OFF = 0f;
        private const float MASK_ON = 1f;

        [Header("Sound Waves")]
        [SerializeField]
        private float[] activeSoundsMask = new float[MAX_SOUNDS]; // Cannot pass bool or int array to material shader
        public float[] ActiveSoundsMask => activeSoundsMask;

        [SerializeField]
        private Vector4[] soundWorldSpaceOrigins = new Vector4[MAX_SOUNDS];
        public Vector4[] SoundWorldSpaceOrigins => soundWorldSpaceOrigins;

        [SerializeField]
        private float[] soundRadii = new float[MAX_SOUNDS];
        public float[] SoundRadii => soundRadii;

        [SerializeField]
        private float[] currFrontWaveRadii = new float[MAX_SOUNDS];
        public float[] CurrFrontWaveRadii => currFrontWaveRadii;

        [SerializeField]
        private float[] soundSpeeds = new float[MAX_SOUNDS];
        public float[] SoundSpeeds => soundSpeeds;

        [SerializeField]
        private float[] waveWidths = new float[MAX_SOUNDS];
        public float[] WaveWidths => waveWidths;

        // If the wave is hard (1), soft (2), both (3) or neither/invisible (0)
        [SerializeField] private float[] waveModes = new float[MAX_SOUNDS];
        public float[] WaveModes => waveModes;

        [SerializeField]
        private Vector4[] waveColors = new Vector4[MAX_SOUNDS];
        public Vector4[] WaveColors => waveColors;

        [SerializeField]
        private float[] waveFadeOutStrengths = new float[MAX_SOUNDS];
        public float[] WaveFadeOutStrengths => waveFadeOutStrengths;

        [SerializeField]
        private float[] waveBorderWidths = new float[MAX_SOUNDS];
        public float[] WaveBorderWidths => waveBorderWidths;

        public void Reset() {
            activeSoundsMask = new float[MAX_SOUNDS];
            soundRadii = new float[MAX_SOUNDS];
            currFrontWaveRadii = new float[MAX_SOUNDS];
            soundSpeeds = new float[MAX_SOUNDS];
            waveWidths = new float[MAX_SOUNDS];
            waveModes = new float[MAX_SOUNDS];
            waveColors = new Vector4[MAX_SOUNDS];
            waveFadeOutStrengths = new float[MAX_SOUNDS];
            waveBorderWidths = new float[MAX_SOUNDS];
        }

        public void Update() {
            // Increase the progress/radius of each sound wave
            for (int i = 0; i < MAX_SOUNDS; i++) {
                if (IsSoundActive(i)) {
                    if (currFrontWaveRadii[i] < soundRadii[i]) {
                        currFrontWaveRadii[i] += soundSpeeds[i] * Time.deltaTime;
                    }
                }
            }
        }

        public int AddSoundWave(Vector3 soundOrigin, SWParams parameters) {
            // Only add the new sound wave if the arrays are not full
            int index = 0;
            for (; index < MAX_SOUNDS; index++) if (!IsSoundActive(index)) break;
            if (index >= MAX_SOUNDS) {
                Debug.LogWarning("SWShaderDataContainer: could not add sound wave, because the array capacity is reached");
                return -1;
            }

            // Apply the parameters to the different data arrays
            activeSoundsMask[index] = MASK_ON;
            soundWorldSpaceOrigins[index] = soundOrigin;
            soundRadii[index] = parameters.soundRadius;
            currFrontWaveRadii[index] = 0f;
            soundSpeeds[index] = parameters.soundSpeed;
            waveWidths[index] = parameters.waveWidth;
            waveModes[index] = (parameters.hardWave ? 1 : 0) + (parameters.softWave ? 2 : 0);
            waveColors[index] = parameters.waveColor;
            waveFadeOutStrengths[index] = parameters.waveFadeOutStrength;
            waveBorderWidths[index] = parameters.waveBorderWidth;
            
            return index;
        }

        public void StopSoundWave(int index) {
            activeSoundsMask[index] = MASK_OFF;
        }

        public bool IsSoundActive(int index) {
            return activeSoundsMask[index] == MASK_ON;
        }

        public void TransferToGPU() {
            Shader.SetGlobalFloatArray(Shader.PropertyToID("_ActiveSoundsMask"), activeSoundsMask);
            Shader.SetGlobalVectorArray(Shader.PropertyToID("_SoundWorldSpaceOrigins"), soundWorldSpaceOrigins);
            Shader.SetGlobalFloatArray(Shader.PropertyToID("_SoundRadii"), soundRadii);
            Shader.SetGlobalFloatArray(Shader.PropertyToID("_CurrFrontWaveRadii"), currFrontWaveRadii);
            Shader.SetGlobalFloatArray(Shader.PropertyToID("_WaveWidths"), waveWidths);
            Shader.SetGlobalFloatArray(Shader.PropertyToID("_WaveModes"), waveModes);
            Shader.SetGlobalVectorArray(Shader.PropertyToID("_WaveColors"), waveColors);
            Shader.SetGlobalFloatArray(Shader.PropertyToID("_WaveFadeOutStrengths"), waveFadeOutStrengths);
            Shader.SetGlobalFloatArray(Shader.PropertyToID("_WaveBorderWidths"), waveBorderWidths);
        }
    }
}