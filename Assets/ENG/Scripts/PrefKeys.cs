/// <summary>
/// Contains the keys for the various used PlayerPrefs.
/// </summary>
public static class PrefKeys {
    public const string VERSION = "orpheus.version";

    public static class Options {
        public const string BRIGHTNESS = "orpheus.options.brightness";
        public const string CAMERA_SENSITIVITY = "orpheus.options.cameraSensitivity";
    }

    public static class Save {
        public const string CHECKPOINT_SCENENAME = "orpheus.save.checkpoint.scenename";
        public const string CHECKPOINT_ID = "orpheus.save.checkpoint.id";
        public const string CHECKPOINT_PROGRESS = "orpheus.save.checkpoint.progress";
    }
}