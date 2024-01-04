using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI {
    public class ControlsPanelUI : MonoBehaviour {
        [SerializeField] private Button backBtn;

        private GameObject lastSelected;

        private void OnEnable() {
            lastSelected = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(backBtn.gameObject);
        }

        private void OnDisable() {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }
}