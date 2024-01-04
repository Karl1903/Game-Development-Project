using UnityEngine;
using Player;

namespace World {
    [RequireComponent(typeof(Collider))]
    public class DeathCollider : MonoBehaviour {
        [SerializeField] private bool fallingDeath = true;

        private Collider coll;

        private void Awake() {
            coll = GetComponent<Collider>();
            coll.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.tag == PlayerController.PLAYER_TAG) {
                if (fallingDeath) PlayerController.Current.DieFalling();
                else PlayerController.Current.Die();
            }
        }
    }
}