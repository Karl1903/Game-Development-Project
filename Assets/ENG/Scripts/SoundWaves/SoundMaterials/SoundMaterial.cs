using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Sounds;

namespace SoundWaves.SoundMaterials {
    public class SoundMaterial : MonoBehaviour {

        // The base class to be put on any object that can emit sound

        public const SoundMaterialType DEFAULT_TYPE = SoundMaterialType.Water;
        public const float DEFAULT_PITCH = 0.5f;

        [Header("Sound Producing Properties")]
        [Tooltip("Influences the color set in the SoundWaveManager")]
        [SerializeField] private SoundMaterialType soundType = DEFAULT_TYPE;
        public SoundMaterialType SoundType => soundType;

        [Tooltip("The relative pitch of the emitted sound from low (0) to high (1)")]
        [SerializeField, Range(0f, 1f)] private float soundPitch = DEFAULT_PITCH;
        public float SoundPitch => soundPitch;

        [Header("Sound")]
        [SerializeField] private RandomSound audioClips;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField, Min(0f)] private float baseVolume = 1f;
        [SerializeField, Min(0.1f)] private float pitch = 1f;

        private void Awake() {
            if (mixerGroup == null) mixerGroup = SoundManager.Inst.DefaultMixerGroup;
        }

        public void EmitSound(Vector3 position, SoundTag tag, float waveRadius, float waveBrightness, List<int> historyObjectIDs = null) {
            SWParams swParams = CalculateWaveParams(waveBrightness, waveRadius);

            float normAudio = Mathf.Clamp01(waveRadius / SoundWaveManager.Inst.swsMaxRadius * SoundWaveManager.Inst.audioVolumeMultiplier);

            if (audioClips.Count > 0) {
                SoundManager.Inst.PlayAtPosition(name, transform.position, audioClips.GetRandom(), mixerGroup, baseVolume * normAudio, false, 0f, pitch);
                SoundWaveManager.Inst.AddSoundWave(gameObject, position, tag, swParams, historyObjectIDs);
            }
        }

        private SWParams CalculateWaveParams(float waveBrightness, float waveRadius) {
            float waveWidth = Mathf.Lerp(
                SoundWaveManager.Inst.swsMaxWidth,
                SoundWaveManager.Inst.swsMinWidth,
                soundPitch
            );

            Color waveColor;
            switch (soundType) {
                case SoundMaterialType.Water:
                    waveColor = SoundWaveManager.Inst.swsColorWater;
                    break;
                case SoundMaterialType.Ceramic:
                    waveColor = SoundWaveManager.Inst.swsColorCeramic;
                    break;
                case SoundMaterialType.Stone:
                    waveColor = SoundWaveManager.Inst.swsColorStone;
                    break;
                case SoundMaterialType.Metal:
                    waveColor = SoundWaveManager.Inst.swsColorMetal;
                    break;
                default:
                    waveColor = SWParams.DEFAULT_WAVE_COLOR;
                    break;
            }
            waveColor.a = waveBrightness; // Brightness = Alpha Channel

            return new SWParams(waveRadius, SoundWaveManager.Inst.swsSpeed, waveWidth, SoundWaveManager.Inst.swsHardWave, SoundWaveManager.Inst.swsSoftWave, waveColor, SoundWaveManager.Inst.swsFadeOut, SoundWaveManager.Inst.swsBorderWidth);
        }
    }
}
