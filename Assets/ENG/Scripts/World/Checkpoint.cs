using UnityEngine;
using Player;

namespace World {
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour {
        [SerializeField] private string id = "Default Checkpoint";
        public string ID => id;

        [Tooltip("Increasing number that will prevent from old checkpoints with lower progress being triggered")]
        [SerializeField, Min(0)] private int progress = 0;
        public int Progress => progress;

        [SerializeField] private Transform playerSpawn;
        public Transform PlayerSpawn => playerSpawn;

        private Collider coll;

        private void Awake() {
            coll = GetComponent<Collider>();
            coll.isTrigger = true;

            if (!playerSpawn) Debug.LogWarning($"Checkpoint: <{id}> has no playerSpawn location assigned");
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == PlayerController.PLAYER_TAG) {
                GameManager.Inst.CheckpointReached(this);
            }
        }
    }
}