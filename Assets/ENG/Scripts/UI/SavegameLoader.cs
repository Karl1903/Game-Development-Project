using UnityEngine;

namespace UI {
    public class SavegameLoader : MonoBehaviour {
        [ContextMenu("Load savegame")]
        public void LoadSavegame() {
            GameManager.Inst.LoadSavegame();
        }
    }
}