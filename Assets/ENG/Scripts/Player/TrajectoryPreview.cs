using UnityEngine;
using UnityEngine.SceneManagement;
using World.Pickupables;

namespace Player {
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPreview : MonoBehaviour {
        private LineRenderer lineRenderer;

        private Scene simulationScene;
        private PhysicsScene physicsScene;

        private void Awake() {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void Init() {
            simulationScene = SceneManager.CreateScene("Trajectory Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            physicsScene = simulationScene.GetPhysicsScene();
        }

        public void SimulateTrajectory(IPickupable pickupable, float force, float rotation, int trajectoryLength) {
            Transform ghostTransform = pickupable.GetTransform();
            GameObject ghostObj = Instantiate(ghostTransform.gameObject, ghostTransform.position, ghostTransform.rotation);
            SceneManager.MoveGameObjectToScene(ghostObj, simulationScene);

            ghostObj.GetComponent<IPickupable>().Throw(force, rotation);

            lineRenderer.positionCount = trajectoryLength;

            for (int i = 0; i < trajectoryLength; i++) {
                physicsScene.Simulate(Time.fixedDeltaTime);
                lineRenderer.SetPosition(i, ghostObj.transform.position);
            }

            Destroy(ghostObj);
        }

        public void Dispose() {
            lineRenderer.positionCount = 0;
            SceneManager.UnloadSceneAsync(simulationScene);
        }
    }
}