using UnityEngine;

namespace Sounds {
    [System.Serializable]
    public struct RandomSound {
        [SerializeField] private AudioClip[] clips;
        public int Count => clips.Length;

        public AudioClip GetRandom() {
            if (clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }
    }
}