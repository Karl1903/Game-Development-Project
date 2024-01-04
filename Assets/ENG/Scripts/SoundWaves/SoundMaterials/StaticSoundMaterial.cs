using UnityEngine;
using Sounds;

namespace SoundWaves.SoundMaterials {
    [RequireComponent(typeof(Collider))]
    public class StaticSoundMaterial : MonoBehaviour {

        // An additional class to be put on any surface that can alter another object's sound emission on collision

        // Influence on SWParams:
        // Softness    -> Radius
        // Softness    -> Brightness (Color Alpha)

        public const float DEFAULT_SOFTNESS = 0.2f;

        [Header("Sound Producing Properties")]
        [Tooltip("The relative softness of the surface from hard, e.g. metal (ca. 0.1), to soft, e.g. moss (ca. 0.66). Soft surfaces cushion the sound wave")]
        [Range(0f, 1f)] public float objectSoftness = DEFAULT_SOFTNESS;

        [Header("Sound")]
        [SerializeField] private RandomSound footstepSoundsOverride;

        public RandomSound GetFootstepSoundsOverride(RandomSound fallback) {
            if (footstepSoundsOverride.Count > 0) return footstepSoundsOverride;
            return fallback;
        }
    }
}
