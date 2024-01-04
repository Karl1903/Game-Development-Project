using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class KeepFocus : MonoBehaviour {
        private GameObject lastFocused;
        private EventSystem eventSystem;

        private void Start() {
            eventSystem = EventSystem.current;
        }

        private void Update() {
            eventSystem.enabled = !BlendUI.Inst.BlendIsShown;
            if (eventSystem.enabled) {
                if (eventSystem.currentSelectedGameObject == null) {
                    eventSystem.SetSelectedGameObject(lastFocused);
                } else {
                    lastFocused = eventSystem.currentSelectedGameObject;
                }
            }
        }
    }
}