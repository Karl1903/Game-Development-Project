using UnityEngine;
using Player;

namespace World.Pickupables {
    [RequireComponent(typeof(Collider))]
    public class Rock : MonoBehaviour, IPickupable {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider coll;

        private Transform initParent = null;
        private bool thrown = false;

        private void Start() {
            if (!rb) Debug.LogWarning("Rock: no rigidbody assigned");
            if (!coll) Debug.LogWarning("Rock: no collider assigned");

            initParent = transform.parent;

            Physics.IgnoreCollision(coll, PlayerController.Current.CharacterController, true);
        }

        public void Pickup() {
            if (thrown) return;
            rb.isKinematic = true;
            PlayerController.Current.Pickup(this);
        }

        public void Drop() {
            transform.parent = initParent;
            rb.isKinematic = false;
        }

        public void Throw(float force, float rotation) {
            Drop();
            rb.AddForce(Quaternion.AngleAxis(-rotation, Camera.main.transform.right) * Camera.main.transform.forward * force, ForceMode.Impulse);
            thrown = true;
        }

        public Transform GetTransform() {
            return transform;
        }

        private void OnCollisionEnter(Collision other) {
            if (thrown) Destroy(gameObject);
        }
    }
}
