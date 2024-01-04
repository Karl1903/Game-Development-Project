using UnityEngine;
using Cinemachine;

namespace Player {
    [RequireComponent(typeof(CinemachineFreeLook))]
    public class CamInit : MonoBehaviour {
        [SerializeField] private float initAxisValueY = 0.32f;

        private void Start() {
            CinemachineFreeLook cam = GetComponent<CinemachineFreeLook>();
            cam.m_XAxis.Value = PlayerController.Current.transform.rotation.eulerAngles.y;
            cam.m_YAxis.Value = initAxisValueY;
        }
    }
}