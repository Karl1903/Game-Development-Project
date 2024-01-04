using UnityEngine;

namespace Effects {
    public class BrightnessSetting : MonoBehaviour {

        private Material material;

        private void Awake() {
            Shader shader = Shader.Find("_Orpheus/Hidden/BrightnessImageEffect");
            material = new Material(shader);

            float brightness = PlayerPrefs.GetFloat(PrefKeys.Options.BRIGHTNESS, 1f);
            material.SetFloat(Shader.PropertyToID("_Brightness"), brightness);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Graphics.Blit(source, destination, material);
        }

        public void SetBrightness(float brightness) {
            material.SetFloat(Shader.PropertyToID("_Brightness"), brightness);
        }
    }
}
