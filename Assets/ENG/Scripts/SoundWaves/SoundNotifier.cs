using UnityEngine;
using System.Collections.Generic;

namespace SoundWaves {
    [RequireComponent(typeof(SphereCollider))]
    public class SoundNotifier : MonoBehaviour {
        public Vector3 SoundOrigin => transform.position;

        [SerializeField] private SoundTag soundTag = SoundTag.General;
        public SoundTag SoundTag => soundTag;

        [SerializeField] private SWParams waveParams = default(SWParams);
        public SWParams WaveParams => waveParams;

        [SerializeField] private int originObjectID;
        public int OriginObjectID => originObjectID;

        [SerializeField] private List<int> historyObjectIDs = new List<int>(); // Object IDs of gameobjects that (re-)triggered the same wave (e.g. resonance)
        public List<int> HistoryObjectIDs => historyObjectIDs;

        private SphereCollider coll;

        /// <summary>
        /// Call this to init this class.
        /// </summary>
        /// <param name="originObject">Game object that triggered the sound. The ID of it (gameobject.GetInstanceID()) is added to the historyObjectIDs list</param>
        /// <param name="soundTag">Sound identifier</param>
        /// <param name="waveParams">Parameters of the sound wave</param>
        /// <param name="historyObjectIDs">History of the sound triggering game objects for the "same" wave (e.g. resonance). Dont set this if the sound is "new" and was not triggered by another wave. This list prevents retriggering sound waves</param>
        /// <returns>this</returns>
        public SoundNotifier Init(GameObject originObject, SoundTag soundTag, SWParams waveParams, List<int> historyObjectIDs = null) {
            originObjectID = originObject.GetInstanceID();
            if (historyObjectIDs != null) this.historyObjectIDs = historyObjectIDs;
            this.historyObjectIDs.Add(originObjectID);
            this.soundTag = soundTag;
            this.waveParams = waveParams;

            coll.radius = 0f;

            return this;
        }

        private void Awake() {
            coll = GetComponent<SphereCollider>();
            coll.isTrigger = true;
        }

        private void Update() {
            if (coll.radius < waveParams.soundRadius) {
                coll.radius += waveParams.soundSpeed * Time.deltaTime;
            } else {
                Destroy(gameObject);
                return;
            }
        }
    }
}