using UnityEngine;
using Scenes;

namespace UI {
    public class PhotosensitivityWarningUI : MonoBehaviour {
        [SerializeField] private float showWarningDuration = 5f;
        [SerializeField] private SceneReference mainMenuScene;
        [SerializeField] private SceneReference brightnessCalibScene;

        private void Start() {
            Invoke(nameof(LoadNextScene), showWarningDuration);
        }

        private void LoadNextScene() {
            _ = GameManager.Inst.LoadSceneAsync(PlayerPrefs.HasKey(PrefKeys.Options.BRIGHTNESS) ? mainMenuScene : brightnessCalibScene);
        }
    }
}