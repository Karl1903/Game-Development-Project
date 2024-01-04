using UnityEngine;

namespace Effects {
    /// <summary>
    /// This class is intended to be used in combination with the _Orpheus/Unlit/Glowing shader
    /// It synchronizes the shader variable _GlowOriginWorldSpace with a given transform
    /// </summary>
    public class GlowOriginSynchronizer : MonoBehaviour {
        [SerializeField] private Transform glowOrigin;
        [SerializeField] private MeshRenderer meshRenderer;

        private void Update() {
            foreach (var material in meshRenderer.materials) {
                material.SetVector("_GlowOriginWorldSpace", glowOrigin.position);
            }
        }
    }
}
