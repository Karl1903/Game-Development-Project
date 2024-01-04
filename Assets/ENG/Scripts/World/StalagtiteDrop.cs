using UnityEngine;

namespace World {
    public class StalagtiteDrop : MonoBehaviour {
        private void OnCollisionEnter(Collision collision) {
            Destroy(gameObject);
        }
    }
}
