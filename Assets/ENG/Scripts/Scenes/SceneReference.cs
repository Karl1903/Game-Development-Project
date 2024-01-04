using System.IO;

namespace Scenes {
    [System.Serializable]
    public struct SceneReference {
        public string scenePath;

        public bool IsNull => string.IsNullOrWhiteSpace(scenePath);

        public SceneReference(string scenePath) {
            this.scenePath = scenePath;
        }

        public string GetSceneName() {
            return PathToFilename(scenePath);
        }

        public static string GetSceneName(SceneReference sceneRef) {
            return sceneRef.GetSceneName();
        }

        public static string GetSceneName(string scenePath) {
            return PathToFilename(scenePath);
        }

        private static string PathToFilename(string path) {
            return Path.GetFileNameWithoutExtension(path);
        }
    }
}