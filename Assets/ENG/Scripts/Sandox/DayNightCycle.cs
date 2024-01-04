using UnityEngine;

namespace Sandbox {
    public class DayNightCycle : MonoBehaviour {
        [SerializeField, Range(0f, 1f)] private float speed = 0.1f;

        // Update is called once per frame
        private void Update() {
            transform.Rotate(new Vector3(0f, speed, 0f), Space.Self);
            if (transform.localRotation.eulerAngles.y > 180)
                RenderSettings.ambientIntensity = Mathf.InverseLerp(0f, 180f, transform.localRotation.eulerAngles.y % 180);
            else
                RenderSettings.ambientIntensity = Mathf.InverseLerp(180f, 0f, transform.localRotation.eulerAngles.y);
        }
    }
}