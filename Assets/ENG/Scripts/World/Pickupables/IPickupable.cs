using UnityEngine;

namespace World.Pickupables {
    public interface IPickupable {
        public void Pickup();
        public void Drop();
        public void Throw(float force, float rotation);
        public Transform GetTransform();
    }
}