using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Slider))]
    public class CameraSensitivityCalibration : MonoBehaviour {
        private void Start() {
            GetComponent<Slider>().value = PlayerPrefs.GetFloat(PrefKeys.Options.CAMERA_SENSITIVITY, 1f);
        }

        public void OnSensitivityChanged(float value) {
            PlayerPrefs.SetFloat(PrefKeys.Options.CAMERA_SENSITIVITY, value);
        }

        public void Save() {
            if (!PlayerPrefs.HasKey(PrefKeys.Options.CAMERA_SENSITIVITY)) PlayerPrefs.SetFloat(PrefKeys.Options.CAMERA_SENSITIVITY, 1f);
            PlayerPrefs.Save();
        }
    }
}