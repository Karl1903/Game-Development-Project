using UnityEngine;

namespace SoundWaves.SoundMaterials {
    [RequireComponent(typeof(Collision), typeof(Rigidbody), typeof(SoundMaterial))]
    public class MoveableSoundMaterial : MonoBehaviour {

        // An additional class to be put on any object that can emit sound by colliding with another object

        // NOTE: Any object that uses MoveableSoundMaterial requires a collider component
        // NOTE: Any object that uses MoveableSoundMaterial requires a rigidbody component with a reasonable mass in kg 
        // NOTE: Any object that uses MoveableSoundMaterial requires a SoundMaterial

        // Influence on SWParams:
        // Mass and Velocity    -> Radius
        // Mass and Velocity    -> Brightness (Color Alpha)
        // Pitch                -> Wave width
        // Type                 -> Color

        // TODO: Only trigger one wave when two moveable objects collide? But of which object?

        [SerializeField] private SoundTag soundTag = SoundTag.General;
        public SoundTag SoundTag => soundTag;

        private SoundMaterial soundMaterial;
        private float objectMass = 1f;

        private void Start() {
            soundMaterial = gameObject.GetComponent<SoundMaterial>();
            objectMass = GetComponent<Rigidbody>().mass;
        }

        private void OnCollisionEnter(Collision collision) {
            Vector3 collisionPos = collision.GetContact(0).point;
            Vector3 collisionVelo = collision.relativeVelocity;

            StaticSoundMaterial otherStaticSoundMat = collision.gameObject.GetComponent<StaticSoundMaterial>();
            float surfaceSoftness = otherStaticSoundMat ? otherStaticSoundMat.objectSoftness : StaticSoundMaterial.DEFAULT_SOFTNESS;

            float loudness = objectMass * collisionVelo.magnitude * (1f - surfaceSoftness);

            float waveRadius = Mathf.Clamp(
                loudness * SoundWaveManager.Inst.swsCollisionRadiusMultiplier,
                SoundWaveManager.Inst.swsMinRadius,
                SoundWaveManager.Inst.swsMaxRadius
            );

            float waveBrightness = Mathf.Clamp(
                loudness * SoundWaveManager.Inst.swsCollisionBrightnessMultiplier,
                SoundWaveManager.Inst.swsMinBrightness,
                SoundWaveManager.Inst.swsMaxBrightness
            );
            //Debug.Log("[moveable] radius: " + waveRadius);
            //Debug.Log("[moveable] brightness: " + waveBrightness);

            soundMaterial.EmitSound(collisionPos, soundTag, waveRadius, waveBrightness);
        }
    }
}
