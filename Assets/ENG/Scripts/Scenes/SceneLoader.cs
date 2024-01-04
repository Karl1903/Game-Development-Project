using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Scenes {
    public class SceneLoader : MonoBehaviour {
        [SerializeField] private bool triggerOnStart = false;
        [SerializeField, ShowIf(nameof(triggerOnStart), true)] private float triggerDelay = 0f;
        [SerializeField] private bool loadFirstLevel = false;
        [SerializeField, ShowIf(nameof(loadFirstLevel), false, "SceneReferenceDrawer")] private SceneReference scene;
        [SerializeField, ShowIf(nameof(loadFirstLevel), false)] private bool showLoadingScreen = true;
        [SerializeField, ShowIf(nameof(loadFirstLevel), false)] private bool showFade = true;

        private void Start() {
            if (triggerOnStart) Invoke(nameof(LoadScene), triggerDelay);
        }

        [ContextMenu("Load Scene")]
        public void LoadScene() {
            if (loadFirstLevel) GameManager.Inst.LoadFirstLevel();
            else _ = GameManager.Inst.LoadSceneAsync(scene, LoadSceneMode.Single, showLoadingScreen, showFade);
        }
    }
}