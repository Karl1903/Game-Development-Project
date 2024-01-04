using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Slider))]
    public class BrightnessCalibration : MonoBehaviour {
        private void Start() {
            GetComponent<Slider>().value = PlayerPrefs.GetFloat(PrefKeys.Options.BRIGHTNESS, 1f);
        }

        public void OnBrighntessChanged(float value) {
            PlayerPrefs.SetFloat(PrefKeys.Options.BRIGHTNESS, value);
        }

        public void Save() {
            if (!PlayerPrefs.HasKey(PrefKeys.Options.BRIGHTNESS)) PlayerPrefs.SetFloat(PrefKeys.Options.BRIGHTNESS, 1f);
            PlayerPrefs.Save();
        }
    }
}