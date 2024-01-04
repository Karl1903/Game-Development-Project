using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace SoundWaves {
    public class SoundWaveManager : MonoBehaviour {
        public static SoundWaveManager Inst { get; private set; }

        // Align these constants with their shader counterpart
        public const int MAX_SOUNDS = 32;

        [Header("Setup")]
        [SerializeField] private SoundNotifier pfSoundNotifier = null;
        [SerializeField] private Shader soundSphereShader;
        [SerializeField, Range(0f, 2f)] private float soundSphereEffectIntensity = 0.5f;

        [Header("Balancing")]
        [ColorUsage(true, true)] public Color swsColorWater = SWParams.DEFAULT_WAVE_COLOR;
        [ColorUsage(true, true)] public Color swsColorCeramic = SWParams.DEFAULT_WAVE_COLOR;
        [ColorUsage(true, true)] public Color swsColorStone = SWParams.DEFAULT_WAVE_COLOR;
        [ColorUsage(true, true)] public Color swsColorMetal = SWParams.DEFAULT_WAVE_COLOR;

        [Min(0f)] public float swsSpeed = 8f;
        [Range(0f, 0.3f)] public float swsBorderWidth = 0.15f;
        [Range(0f, 3f)] public float swsFadeOut = 2f;
        [Min(0f)] public float swsMinRadius = 0f;
        [Min(0f)] public float swsMaxRadius = 50f;
        [Min(0f)] public float swsCollisionRadiusMultiplier = 0.5f;
        [Min(0f)] public float swsResonanceRadiusMultiplier = 1.0f;
        [Range(0f, 5f)] public float swsMinBrightness = 0.33f;
        [Range(0f, 5f)] public float swsMaxBrightness = 3f;
        [Min(0f)] public float swsCollisionBrightnessMultiplier = 0.003f;
        [Min(0f)] public float swsResonanceBrightnessMultiplier = 1.0f;
        [Min(0f)] public float swsMinWidth = 0.5f;
        [Min(0f)] public float swsMaxWidth = 15f;
        public bool swsHardWave = true;
        public bool swsSoftWave = false;
        public float audioVolumeMultiplier = 1.2f;

        [Header("Debug")]
        [SerializeField] private bool dbgSpawnAtClick = false;

        [SerializeField, ShowIf(nameof(dbgSpawnAtClick), true)]
        private SoundTag dbgSoundTag = SoundTag.Debug;

        [SerializeField, ShowIf(nameof(dbgSpawnAtClick), true, "SWPatternDrawer")]
        private SWPattern dbgSoundWavePattern = SWPattern.DEFAULT;

        [Header("Runtime")]
        [SerializeField, ReadOnly] private SWShaderDataContainer shaderData = new SWShaderDataContainer();
        public SWShaderDataContainer ShaderData => shaderData;

        private Camera cam;
        private GameObject[] spheres = new GameObject[MAX_SOUNDS];


        private void Awake() {
            // Singleton handling
            if (Inst) {
                Destroy(gameObject);
                return;
            } else {
                Inst = this;
                DontDestroyOnLoad(gameObject);
            }

            if (!pfSoundNotifier) Debug.LogWarning("SoundWaveManager: no Sound Notifier prefab assigned");

            cam = Camera.main;

            ResetSoundWaves();

            // Reset transform for safety
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void Start() {
            GameManager.Inst.OnSceneLoaded += ResetSoundWaves;
        }

        private void Update() {
            shaderData.Update();
            shaderData.TransferToGPU();

            if (dbgSpawnAtClick && Input.GetMouseButtonDown(0)) {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    AddSoundWavePattern(gameObject, hit.point, dbgSoundTag, dbgSoundWavePattern);
                }
            }
        }

        private void LateUpdate() {
            // synchronize sound spheres with shaderData
            for (int i = 0; i < MAX_SOUNDS; i++) {
                float currRadius = shaderData.CurrFrontWaveRadii[i];
                float radius = shaderData.SoundRadii[i];
                if (shaderData.IsSoundActive(i)) {
                    if (currRadius < radius) {
                        spheres[i].transform.localScale = Vector3.one * 2 * currRadius;
                        Color color = shaderData.WaveColors[i];
                        color *= soundSphereEffectIntensity;
                        color *= 1 - (currRadius / radius);
                        spheres[i].GetComponent<MeshRenderer>().material.SetColor("_Color", color);
                    }
                    else {
                        StopSoundWave(i);
                    }
                }
            }
        }

        private void OnDestroy() {
            ResetSoundWaves();
            GameManager.Inst.OnSceneLoaded -= ResetSoundWaves;
        }

        /// <summary>
        /// Trigger a new sound wave at <paramref name="soundOrigin"/>.
        /// </summary>
        /// <param name="originObject">Game object that triggered the sound</param>
        /// <param name="soundOrigin">The center point of the sound</param>
        /// <param name="tag">Identifier for the sound creator</param>
        /// <param name="parameters">Additional parameters to define the sound wave. At least provide a soundWaveColor (otherwise black)</param>
        /// <param name="historyObjectIDs">History of the sound triggering game objects for the "same" wave (e.g. resonance). Dont set this if the sound is "new" and was not triggered by another wave. This list prevents retriggering sound waves. This list prevents retriggering sound waves. This list prevents retriggering sound waves</param>
        /// <returns>true if the sound wave was added, false if the sound wave capacity was reached</returns>
        public bool AddSoundWave(GameObject originObject, Vector3 soundOrigin, SoundTag tag, SWParams parameters, List<int> historyObjectIDs = null) {
            int index = shaderData.AddSoundWave(soundOrigin, parameters);
            bool success = index >= 0;

            // spawn sphere
            spheres[index] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spheres[index].transform.position = soundOrigin;
            Object.Destroy(spheres[index].GetComponent<SphereCollider>());
            spheres[index].GetComponent<MeshRenderer>().material = new Material(soundSphereShader);

            if (success)
                Instantiate(pfSoundNotifier.gameObject, soundOrigin, Quaternion.identity, transform).GetComponent<SoundNotifier>().Init(originObject, tag, parameters, historyObjectIDs).name = "Sound Notifier " + tag.ToString();
            return success;
        }

        /// <summary>
        /// Can be used to spawn multiple sound waves in a timed pattern at a moving transform.
        /// </summary>
        /// <param name="originObject">Game object that triggered the sound</param>
        /// <param name="soundOrigin">The center point of the sound (transform can be moved during the pattern)</param>
        /// <param name="tag">Identifier for the sound creator</param>
        /// <param name="pattern">The sound wave pattern information</param>
        /// <param name="historyObjectIDs">History of the sound triggering game objects for the "same" wave (e.g. resonance). Dont set this if the sound is "new" and was not triggered by another wave. This list prevents retriggering sound waves. This list prevents retriggering sound waves. This list prevents retriggering sound waves</param>
        public void AddSoundWavePattern(GameObject originObject, in Transform soundOrigin, SoundTag tag, SWPattern pattern, List<int> historyObjectIDs = null) {
            for (int i = 0; i < pattern.waves.Length; i++) {
                StartCoroutine(DelayedSoundWaveRoutine(originObject, soundOrigin, tag, pattern.waves[i].parameters, pattern.waves[i].timeOffset, historyObjectIDs));
            }
        }

        /// <summary>
        /// Can be used to spawn multiple sound waves in a timed pattern at a static position.
        /// </summary>
        /// <param name="originObject">Game object that triggered the sound</param>
        /// <param name="soundOrigin">The center point of the sound (static)</param>
        /// <param name="tag">Identifier for the sound creator</param>
        /// <param name="pattern">The sound wave pattern information</param>
        /// <param name="historyObjectIDs">History of the sound triggering game objects for the "same" wave (e.g. resonance). Dont set this if the sound is "new" and was not triggered by another wave. This list prevents retriggering sound waves. This list prevents retriggering sound waves. This list prevents retriggering sound waves</param>
        public void AddSoundWavePattern(GameObject originObject, Vector3 soundOrigin, SoundTag tag, SWPattern pattern, List<int> historyObjectIDs = null) {
            for (int i = 0; i < pattern.waves.Length; i++) {
                StartCoroutine(DelayedSoundWaveRoutine(originObject, soundOrigin, tag, pattern.waves[i].parameters, pattern.waves[i].timeOffset, historyObjectIDs));
            }
        }

        public void StopSoundWave(int index) {
            if (index >= 0 && index < MAX_SOUNDS) {
                shaderData.StopSoundWave(index);
                Object.Destroy(spheres[index]);
            }
            else Debug.LogWarning($"SoundWaveManager: could not stop sound wave because the index is out of range [0,{MAX_SOUNDS})");
        }

        public void ResetSoundWaves() {
            shaderData.Reset();
            shaderData.TransferToGPU();
            foreach (var sphere in spheres) {
                if (sphere != null) Object.Destroy(sphere);
            }
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
        }

        private IEnumerator DelayedSoundWaveRoutine(GameObject originObject, Transform soundOrigin, SoundTag tag, SWParams parameters, float delay, List<int> historyObjectIDs = null) {
            yield return new WaitForSeconds(delay);
            if (originObject == null) yield break;
            AddSoundWave(originObject, soundOrigin.position, tag, parameters, historyObjectIDs);
        }

        private IEnumerator DelayedSoundWaveRoutine(GameObject originObject, Vector3 soundOrigin, SoundTag tag, SWParams parameters, float delay, List<int> historyObjectIDs = null) {
            yield return new WaitForSeconds(delay);
            if (originObject == null) yield break;
            AddSoundWave(originObject, soundOrigin, tag, parameters, historyObjectIDs);
        }
    }
}