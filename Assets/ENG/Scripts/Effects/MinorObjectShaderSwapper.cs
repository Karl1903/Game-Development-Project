using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Effects {
    /// <summary>
    /// This class switches the Shader of Objects with the Layer "No Camera Collision" when they are (roughly) between camera and player (blocking the view).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MinorObjectShaderSwapper : MonoBehaviour {
        [SerializeField] private Transform playerMeshTransform;
        [SerializeField] private CharacterController playerController;
        [SerializeField] private Shader defaultShader;
        [SerializeField] private Shader blockingViewShader;

        // SphereCast Properties
        private float radius;
        private int layerMask;

        // GameObjects between camera and player
        private HashSet<GameObject> inFrontLastFrame;
        private HashSet<GameObject> inFrontThisFrame;

        private void Awake() {
            radius = playerController.height / 2;
            layerMask = LayerMask.GetMask("No Camera Collision");
            inFrontLastFrame = new HashSet<GameObject>();
            inFrontThisFrame = new HashSet<GameObject>();
        }

        private void Update() {
            Vector3 camPos = transform.position;
            Vector3 playerPos = playerMeshTransform.position;
            Vector3 camPlayerDir = (playerPos - camPos).normalized;
            float camPlayerDistance = Vector3.Distance(camPos, playerPos);

            RaycastHit[] hits = Physics.SphereCastAll(camPos, radius, camPlayerDir, camPlayerDistance - radius, layerMask, QueryTriggerInteraction.Ignore);

            inFrontThisFrame.Clear();
            foreach (var hit in hits) {
                GameObject gameObject = hit.transform.gameObject;
                inFrontThisFrame.Add(gameObject);
            }

            IEnumerable<GameObject> addedThisFrame = inFrontThisFrame.Except(inFrontLastFrame);
            IEnumerable<GameObject> removedThisFrame = inFrontLastFrame.Except(inFrontThisFrame);

            foreach (var gameObject in addedThisFrame) {
                SwapShader(gameObject, blockingViewShader);
            }
            foreach (var gameObject in removedThisFrame) {
                SwapShader(gameObject, defaultShader);
            }

            inFrontLastFrame.ExceptWith(removedThisFrame.ToArray());
            inFrontLastFrame.UnionWith(addedThisFrame);
        }

        private void SwapShader(GameObject gameObject, Shader shader) {
            MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers) {
                foreach (var material in renderer.materials) {
                    material.shader = shader;
                }
            }
        }
    }
}