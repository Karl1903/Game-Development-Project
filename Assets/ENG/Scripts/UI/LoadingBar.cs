using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class LoadingBar : MonoBehaviour {
        [SerializeField] private Image progressBarImg;

        private void Awake() {
            if (!progressBarImg) Debug.LogWarning("LoadingBar: no progressBarImg assigned");
            else progressBarImg.fillAmount = 0f;
        }

        private void Update() {
            progressBarImg.fillAmount = GameManager.Inst.AsyncSceneLoadProgress;
        }
    }
}
