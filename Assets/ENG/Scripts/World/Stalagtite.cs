using System.Collections;
using UnityEngine;

namespace World {
    public class Stalagtite : MonoBehaviour {
        [SerializeField] private StalagtiteDrop pfDrop;
        [SerializeField] private Transform spawnLocation;
        [SerializeField, Min(0f)] private float dropIntervalMin = 1; // Random min
        [SerializeField, Min(0f)] private float dropIntervalMax = 5; // Random max

        private void Start() {
            StartCoroutine(SpawnDropRoutine());
        }

        private IEnumerator SpawnDropRoutine() {
            while (true) {
                yield return new WaitForSeconds(Random.Range(dropIntervalMin, dropIntervalMax));
                GameObject go = Instantiate(pfDrop.gameObject, spawnLocation.position, transform.rotation, spawnLocation);
                go.name = "Stalactite Drop";
                Physics.IgnoreCollision(GetComponentInChildren<Collider>(), go.GetComponentInChildren<Collider>(), true);
            }
        }
    }
}
