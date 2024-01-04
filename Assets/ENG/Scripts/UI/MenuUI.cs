using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI {
    public class MenuUI : MonoBehaviour {
        [SerializeField] private Button loadGameBtn;
        [SerializeField] private Color disabledTextColor = Color.gray;

        private void Start() {
            // Check if the savegame is valid
            if (!PlayerPrefs.HasKey(PrefKeys.Save.CHECKPOINT_SCENENAME) ||
                !PlayerPrefs.HasKey(PrefKeys.Save.CHECKPOINT_ID) ||
                !PlayerPrefs.HasKey(PrefKeys.Save.CHECKPOINT_PROGRESS)) {
                // Savegame is invalid -> disable load game button
                loadGameBtn.interactable = false;
                loadGameBtn.GetComponentInChildren<TMP_Text>().color = disabledTextColor;
            }
        }
    }
}