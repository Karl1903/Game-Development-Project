using UnityEngine;

namespace SoundWaves {
    public interface ISoundReceiver {
        public void OnTriggerEnter(Collider other);
    }
}