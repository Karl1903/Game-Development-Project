using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

namespace UI.Tutorial {
    public class TutorialHintManager : MonoBehaviour {
        public static TutorialHintManager Inst { get; private set; }

        [SerializeField] private TutorialHintUI pfHintUI;
        [SerializeField] private RectTransform hintsList;

        [SerializeField] private InputActionAsset inputActionAsset;

        private InputDevice lastDeviceUsed = InputDevice.Keyboard;

        private InputActionReference keyboardActionRef;
        private InputActionReference mouseActionRef;
        private InputActionReference controllerActionRef;

        private List<TutorialHintUI> activeHintUIs = new List<TutorialHintUI>();

        private void Awake() {
            // Singleton handling
            if (Inst) {
                Destroy(gameObject);
                return;
            } else {
                // Dont destroy on load handled by the ManagersInit script
                Inst = this;
            }

            keyboardActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            mouseActionRef = ScriptableObject.CreateInstance<InputActionReference>();
            controllerActionRef = ScriptableObject.CreateInstance<InputActionReference>();

            keyboardActionRef.Set(inputActionAsset, "Determine Input Device", "Keyboard");
            mouseActionRef.Set(inputActionAsset, "Determine Input Device", "Mouse");
            controllerActionRef.Set(inputActionAsset, "Determine Input Device", "Controller");
        }

        private void Start() {
            GameManager.Inst.OnSceneLoaded += ResetHints;
        }

        private void Update() {
            InputDevice? deviceUsed = GetUsedInputDevice();
            if (deviceUsed != null) lastDeviceUsed = (InputDevice)deviceUsed;

            for (int i = 0; i < activeHintUIs.Count; i++) {
                TutorialHint h = activeHintUIs[i].Hint;
                if (h.dismissAfterTime && Time.time > activeHintUIs[i].Timestamp + h.dismissTime) {
                    HideHint(i--);
                } else if (h.dismissAfterInput != null && h.dismissAfterInput.action.triggered) {
                    HideHint(i--);
                }
            }
        }

        private void OnDestroy() {
            GameManager.Inst.OnSceneLoaded -= ResetHints;
        }

        public void ShowHint(TutorialHint hint) {
            activeHintUIs.Add(Instantiate(pfHintUI.gameObject, hintsList).GetComponent<TutorialHintUI>().Setup(hint, lastDeviceUsed));
        }

        private void HideHint(int index) {
            activeHintUIs[index].Hide();
            activeHintUIs.RemoveAt(index);
        }

        private void ResetHints() {
            activeHintUIs.Clear();
            foreach (Transform child in hintsList.transform) {
                Destroy(child.gameObject);
            }
        }

        private InputDevice? GetUsedInputDevice() {
            if (controllerActionRef.action.triggered) return InputDevice.Controller;
            if (keyboardActionRef.action.triggered) return InputDevice.Keyboard;
            if (mouseActionRef.action.triggered) return InputDevice.Mouse;
            return null;
        }
    }
}