using UnityEngine;

namespace SoundWaves.SoundMaterials {
    [RequireComponent(typeof(SoundReceiver), typeof(SoundMaterial))]
    public class ResonanceMaterial : MonoBehaviour {

        // An additional class to be put on any object that can emit sound by resonating

        // NOTE: Each "Resonance" object also requires a "Sound Receiver"
        // NOTE: Each "Resonance" object also requires a "Sound Material"

        [SerializeField] private SoundTag soundTag = SoundTag.General;
        public SoundTag SoundTag => soundTag;

        [SerializeField, Min(0f)] private float resonance = 1f;
        public float Resonance => resonance;

        [SerializeField] private Transform resonanceLocation;

        [SerializeField] Collider trigger;

        private SoundReceiver soundReceiver;
        private SoundMaterial soundMaterial;

        private void Awake() {
            soundReceiver = GetComponent<SoundReceiver>();
            soundMaterial = GetComponent<SoundMaterial>();

            soundReceiver.OnSound.AddListener(OnSoundHandler);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.contacts[0].thisCollider == trigger && collision.collider.gameObject.GetComponent<MoveableSoundMaterial>().SoundTag == SoundTag.Stone)
            {
                float velo = collision.relativeVelocity.magnitude;

                float waveRadius = Mathf.Clamp(
                    velo * resonance * SoundWaveManager.Inst.swsResonanceRadiusMultiplier,
                    SoundWaveManager.Inst.swsMinRadius,
                    SoundWaveManager.Inst.swsMaxRadius
                );

                float waveBrightness = Mathf.Clamp(
                    velo * resonance * SoundWaveManager.Inst.swsResonanceRadiusMultiplier,
                    SoundWaveManager.Inst.swsMinBrightness,
                    SoundWaveManager.Inst.swsMaxBrightness
                );   

                soundMaterial.EmitSound(collision.transform.position, soundTag, waveRadius, waveBrightness);
            }    
        }

        private void OnSoundHandler(SoundNotifier notif) {
            if (notif.HistoryObjectIDs.Contains(gameObject.GetInstanceID())) return;

            if (notif.SoundTag == SoundTag.Resonance || notif.SoundTag == SoundTag.Lyre) {
                float distanceDamper = 1 - (Vector3.Distance(notif.SoundOrigin, transform.position) / notif.WaveParams.soundRadius); // [0, 1]

                float waveRadius = Mathf.Clamp(
                    notif.WaveParams.soundRadius * distanceDamper * resonance * SoundWaveManager.Inst.swsResonanceRadiusMultiplier,
                    SoundWaveManager.Inst.swsMinRadius,
                    SoundWaveManager.Inst.swsMaxRadius
                );

                float waveBrightness = Mathf.Clamp(
                    notif.WaveParams.waveColor.a * distanceDamper * resonance * SoundWaveManager.Inst.swsResonanceBrightnessMultiplier,
                    SoundWaveManager.Inst.swsMinBrightness,
                    SoundWaveManager.Inst.swsMaxBrightness
                );

                //Debug.Log("[resonance] radius: " + waveRadius);
                //Debug.Log("[resonance] brightness: " + waveBrightness);

                soundMaterial.EmitSound(resonanceLocation.position, soundTag, waveRadius, waveBrightness, notif.HistoryObjectIDs);
            }
        }

        private void OnDestroy() {
            soundReceiver.OnSound.RemoveListener(OnSoundHandler);
        }
    }
}
