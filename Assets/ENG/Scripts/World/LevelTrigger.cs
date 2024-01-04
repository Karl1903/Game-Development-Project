using UnityEngine;
using Scenes;
using Player;
using Utils;

namespace World {
    [RequireComponent(typeof(Collider))]
    public class LevelTrigger : MonoBehaviour {
        [SerializeField] private bool nextLevel = true;
        public bool NextLevel => nextLevel;

        [SerializeField, ShowIf(nameof(nextLevel), false, "SceneReferenceDrawer")]
        private SceneReference scene;
        public SceneReference Scene => scene;

        private Collider coll;

        private void Awake() {
            coll = GetComponent<Collider>();
            coll.isTrigger = true;

            if (!nextLevel && scene.IsNull) Debug.LogWarning("Level Trigger: nextLevel is false and no scene was set");
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == PlayerController.PLAYER_TAG) {
                if (nextLevel) GameManager.Inst.LoadNextLevel();
                else _ = GameManager.Inst.LoadSceneAsync(scene, UnityEngine.SceneManagement.LoadSceneMode.Single, true, true);
            }
        }
    }
}