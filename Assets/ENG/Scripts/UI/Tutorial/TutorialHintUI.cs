using UnityEngine;
using UnityEngine.UI;
using TMPro;
using World;

namespace UI.Tutorial {
    public class TutorialHintUI : MonoBehaviour {
        [SerializeField] private RectTransform imageList;
        [SerializeField] private TMP_Text text;

        public TutorialHint Hint { get; private set; }
        public float Timestamp { get; private set; }

        public TutorialHintUI Setup(TutorialHint hint, InputDevice inputDevice) {
            Hint = hint;
            Timestamp = Time.time;

            var sprites = (inputDevice == InputDevice.Keyboard || inputDevice == InputDevice.Mouse)
                ? hint.keyboardImages
                : hint.gamepadImages;

            foreach (Sprite s in sprites) {
                GameObject imgGO = new GameObject("Hint Image", typeof(Image));
                imgGO.transform.SetParent(imageList);
                Image img = imgGO.GetComponent<Image>();
                img.sprite = s;
                img.preserveAspect = true;
            }

            text.text = hint.text;
            return this;
        }

        public void Hide() {
            GetComponent<AnimationTrigger>().Trigger();
            Destroy(gameObject, 1f); // Should be as long as the animation
        }
    }
}
