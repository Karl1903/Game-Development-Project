using UnityEngine;
using UnityEngine.Video;

namespace UI {
    public class StorySceneUI : MonoBehaviour {
        [SerializeField] private VideoPlayer videoPlayer;

        private void Start() {
            videoPlayer.loopPointReached += VideoEndHandler;
        }

        private void VideoEndHandler(VideoPlayer source) {
            GameManager.Inst.LoadFirstLevel();
        }

        private void OnDestroy() {
            videoPlayer.loopPointReached -= VideoEndHandler;
        }
    }
}