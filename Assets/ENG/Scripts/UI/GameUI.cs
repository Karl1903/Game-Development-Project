using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using Scenes;

namespace UI {
    public class GameUI : MonoBehaviour {
        public static GameUI Current { get; private set; }

        [SerializeField] private TMP_Text smallNotifyText;
        [SerializeField] private TMP_Text bigNotifyText;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private SceneReference mainMenuScene;
        [SerializeField] private GameObject initialSelectedObject;
        [SerializeField] private GameObject controlsPanel;

        private void Awake() {
            // Singleton handling
            if (Current) {
                SceneManager.UnloadSceneAsync(gameObject.scene);
                return;
            } else Current = this;

            pauseMenu.SetActive(false);
        }

        public async void SmallNotifyText(string text, float duration) {
            smallNotifyText.text = text;
            await Task.Delay((int)(duration * 1000f));
            smallNotifyText.text = "";
        }

        public async void BigNotifyText(string text, float duration) {
            bigNotifyText.text = text;
            await Task.Delay((int)(duration * 1000f));
            bigNotifyText.text = "";
        }

        public void ShowMenu() {
            pauseMenu.SetActive(true);
            controlsPanel.SetActive(false);
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(initialSelectedObject);
        }

        public void HideMenu() {
            pauseMenu.SetActive(false);
        }

        public void ContinueClickHandler() {
            GameManager.Inst.HidePauseMenu();
        }

        public void BackToMenuHandler() {
            Time.timeScale = 1f;
            _ = GameManager.Inst.LoadSceneAsync(mainMenuScene, LoadSceneMode.Single, true, true);
        }
    }
}