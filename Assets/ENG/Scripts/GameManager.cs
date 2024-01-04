using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Scenes;
using Player;
using World;
using Sounds;
using UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour {
    public static GameManager Inst { get; private set; }

    [Header("Setup")]
    [SerializeField] private PlayerController pfPlayer = null;

    [Header("Scene setup")]
    [SerializeField] private SceneReference loadingScene;
    [SerializeField] private SceneReference startupScene;
    [SerializeField] private SceneReference gameUIScene;
    [SerializeField] private SceneReference[] menuScenes = new SceneReference[0];
    [SerializeField] private SceneReference[] levelScenes = new SceneReference[0];

    [Header("Audio")]
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip respawnSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private RandomSound checkpointSound;
    [SerializeField] private AudioClip pauseMenuSound;

    [SerializeField] private AudioMixerGroup mixerGroupNarrator;
    [SerializeField] private AudioClip respawnNarratorSound;

    [Header("Settings")]
    [SerializeField] private InputActionReference cameraInput;
    [SerializeField] private float respawnDelay = 3f;

    public delegate void SceneLoaded();
    public event SceneLoaded OnSceneLoaded;

    private AsyncOperation asyncSceneLoad = null;
    public float AsyncSceneLoadProgress => asyncSceneLoad != null ? asyncSceneLoad.progress : 0f;

    private bool respawning = false;
    public bool Respawning => respawning;

    private bool pauseMenuShown = false;
    public bool PauseMenuShown => pauseMenuShown;

    private void Awake() {
        // Singleton handling
        if (Inst) {
            Destroy(gameObject);
            return;
        } else {
            // Dont destroy on load handled by the ManagersInit script
            Inst = this;
        }

        if (loadingScene.IsNull) Debug.LogWarning("GameManager: no loadingScene assigned");
        if (startupScene.IsNull) Debug.LogWarning("GameManager: no startupScene assigned");
        if (gameUIScene.IsNull) Debug.LogWarning("GameManager: no gameUIScene assigned");
        if (menuScenes.Length == 0) Debug.LogWarning("GameManager: no menuScenes assigned; assign at least one");
        if (levelScenes.Length == 0) Debug.LogWarning("GameManager: no levelScenes assigned; assign at least one");

        SceneLoadedHandler(SceneManager.GetActiveScene(), LoadSceneMode.Single); // Trigger scene loaded for the first scene load
        SceneManager.sceneLoaded += SceneLoadedHandler; // Trigger scene loaded for the next scene loads, except the first one
    }

    private void Start() {
        // Handle game start in builds where the loading scene is active first -> load startup scene
        if (SceneManager.GetActiveScene().path == loadingScene.scenePath && asyncSceneLoad == null)
            _ = LoadSceneAsync(startupScene);

        // Check for new game version
        if (Application.version != PlayerPrefs.GetString(PrefKeys.VERSION, "-1")) {
            // New version -> reset settings
            ResetPlayerPrefs();
            PlayerPrefs.SetString(PrefKeys.VERSION, Application.version);
            PlayerPrefs.Save();
        }
    }

    public void LoadScene(SceneReference sceneRef, LoadSceneMode mode = LoadSceneMode.Single) {
        LoadScene(sceneRef.GetSceneName(), mode);
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single) {
        SceneManager.LoadScene(sceneName, mode);
    }

    public async Task<AsyncOperation> LoadSceneAsync(SceneReference sceneRef, LoadSceneMode mode = LoadSceneMode.Single, bool showLoadingScene = false, bool fade = false, float? fadeDuration = null) {
        return await LoadSceneAsync(sceneRef.GetSceneName(), mode, showLoadingScene, fade, fadeDuration);
    }

    public async Task<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool showLoadingScene = false, bool fade = false, float? fadeDuration = null) {
        if (fade) await BlendUI.Inst.FadeOut(fadeDuration);
        if (showLoadingScene) LoadScene(loadingScene.GetSceneName(), mode);

        asyncSceneLoad = SceneManager.LoadSceneAsync(sceneName, mode);
        asyncSceneLoad.allowSceneActivation = !showLoadingScene;

        if (showLoadingScene || fade) asyncSceneLoad.completed += HideLoadingScene;

        return asyncSceneLoad;

        void HideLoadingScene(AsyncOperation op) {
            if (mode == LoadSceneMode.Additive) {
                StartCoroutine(HideLoadingSceneRoutine());
            } else {
                ShowScene();
            }
            asyncSceneLoad.completed -= HideLoadingScene;
        }

        IEnumerator HideLoadingSceneRoutine() {
            AsyncOperation op = SceneManager.UnloadSceneAsync(loadingScene.GetSceneName());
            yield return new WaitUntil(() => op.isDone);
            ShowScene();
        }

        void ShowScene() {
            if (fade) {
                if (showLoadingScene) BlendUI.Inst.ShowBlend();
                _ = BlendUI.Inst.FadeIn(fadeDuration); // _ = --> Discard Task --> supresses warning
            }
            asyncSceneLoad.allowSceneActivation = true;
        }
    }

    [ContextMenu("Load/Load First Level")]
    public void LoadFirstLevel() {
        // Start at the first checkpoint in the first level
        PlayerPrefs.DeleteKey(PrefKeys.Save.CHECKPOINT_SCENENAME);
        PlayerPrefs.DeleteKey(PrefKeys.Save.CHECKPOINT_ID);
        PlayerPrefs.DeleteKey(PrefKeys.Save.CHECKPOINT_PROGRESS);
        PlayerPrefs.Save();
        _ = LoadSceneAsync(levelScenes[0], LoadSceneMode.Single, true, true);
    }

    [ContextMenu("Load/Load Savegame")]
    public void LoadSavegame() {
        string loadSceneName = PlayerPrefs.GetString(PrefKeys.Save.CHECKPOINT_SCENENAME, "");

        if (string.IsNullOrWhiteSpace(loadSceneName)) {
            // No savegame -> new start
            LoadFirstLevel();
        } else {
            // Load savegame
            SceneReference lvl = levelScenes.FirstOrDefault(lvl => lvl.GetSceneName() == loadSceneName);
            if (lvl.IsNull) Debug.LogWarning($"GameManager: loading scene <{loadSceneName}> does not exist");
            else _ = LoadSceneAsync(lvl, LoadSceneMode.Single, true, true);
        }
    }

    [ContextMenu("Load/Load Next Level")]
    public void LoadNextLevel() {
        int index = 0;
        for (; index < levelScenes.Length; index++) {
            if (levelScenes[index].scenePath == SceneManager.GetActiveScene().path) break;
        }
        if (index >= levelScenes.Length) Debug.LogWarning("GameManager: loading next level failed; current level does not exist in levelScenes");
        else {
            if (index + 1 >= levelScenes.Length) Debug.LogWarning("GameManager: loading next level failed; current level is last level");
            else _ = LoadSceneAsync(levelScenes[index + 1], LoadSceneMode.Single, true, true);
        }
    }

    /// <summary>
    /// Show the cursor and no window locking.
    /// </summary>
    [ContextMenu("Cursor/Show Cursor")]
    public void SetCursorModeMenu() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Hide the cursor and window locking.
    /// </summary>
    [ContextMenu("Cursor/Hide Cursor")]
    public void SetCursorModeLevel() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    [ContextMenu("Respawn")]
    public void Respawn() {
        Respawn(1f);
    }

    public async void Respawn(float respawnDelayModifier) {
        // Just reload the scene at the last checkpoint (alias for LoadSavegame())
        if (respawning) return;
        respawning = true;
        SoundManager.Inst.Play(deathSound, mixerGroup);
        GameUI.Current.BigNotifyText("YOU DIED", 5f);
        await Task.Delay((int)(respawnDelay * respawnDelayModifier * 1000f));
        LoadSavegame();
        await Task.Delay(3000); // Sorry for the magic number, but its late -.-
        SoundManager.Inst.Play(respawnNarratorSound, mixerGroupNarrator);
    }

    public void CheckpointReached(Checkpoint checkpoint) {
        if (checkpoint.Progress > PlayerPrefs.GetInt(PrefKeys.Save.CHECKPOINT_PROGRESS, -1)) {
            PlayerPrefs.SetString(PrefKeys.Save.CHECKPOINT_SCENENAME, checkpoint.gameObject.scene.name);
            PlayerPrefs.SetString(PrefKeys.Save.CHECKPOINT_ID, checkpoint.ID);
            PlayerPrefs.SetInt(PrefKeys.Save.CHECKPOINT_PROGRESS, checkpoint.Progress);
            PlayerPrefs.Save();
            SoundManager.Inst.Play(checkpointSound.GetRandom(), mixerGroup);
            GameUI.Current.SmallNotifyText("Checkpoint Reached", 3f);
        }
    }

    [ContextMenu("Pause Menu/Show")]
    public void ShowPauseMenu() {
        if (!GameUI.Current || pauseMenuShown) return;
        pauseMenuShown = true;
        SoundManager.Inst.Play(pauseMenuSound, mixerGroup);
        Time.timeScale = 0f; // Pause game
        GameUI.Current.ShowMenu();
        SetCursorModeMenu();
    }

    [ContextMenu("Pause Menu/Hide")]
    public void HidePauseMenu() {
        if (!GameUI.Current || !pauseMenuShown) return;
        pauseMenuShown = false;
        SoundManager.Inst.Play(pauseMenuSound, mixerGroup);
        Time.timeScale = 1f; // Continue game
        GameUI.Current.HideMenu();
        SetCursorModeLevel();
    }

    [ContextMenu("Pause Menu/Toggle")]
    public void TogglePauseMenu() {
        if (pauseMenuShown) HidePauseMenu();
        else ShowPauseMenu();
    }

    [ContextMenu("Reset PlayerPrefs")]
    public void ResetPlayerPrefs() {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private void SceneLoadedHandler(Scene scene, LoadSceneMode mode) {
        if (scene.name == gameUIScene.GetSceneName()) return;
        OnSceneLoaded?.Invoke();
        respawning = false;
        pauseMenuShown = false;
        if (!levelScenes.FirstOrDefault(lvl => lvl.GetSceneName() == scene.name).IsNull) {
            // Level was loaded
            SetCursorModeLevel();
            _ = LoadSceneAsync(gameUIScene, LoadSceneMode.Additive);

            string loadCheckpointID = PlayerPrefs.GetString(PrefKeys.Save.CHECKPOINT_ID, "");
            Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
            Checkpoint checkpoint = checkpoints.FirstOrDefault(c => c.ID == loadCheckpointID);

            if (string.IsNullOrWhiteSpace(loadCheckpointID) || checkpoint == null) {
                // No saved checkpoint -> start at the first one (lowest progress)
                int min = int.MaxValue;
                foreach (Checkpoint c in checkpoints) {
                    if (c.Progress < min) {
                        checkpoint = c;
                        min = c.Progress;
                    }
                }
            }

            if (!checkpoint) Debug.LogWarning("GameManager: no checkpoint found for spawning the player");
            else SpawnPlayer(checkpoint.PlayerSpawn);
        } else {
            if (scene.name == loadingScene.GetSceneName()) {
                // Loading screen was loaded
                SetCursorModeLevel();
            } else {
                // Menu or something else was loaded
                SetCursorModeMenu();
            }
        }
    }

    private void SpawnPlayer(in Transform transform) {
        Instantiate(pfPlayer.gameObject, transform.position, transform.rotation).name = pfPlayer.gameObject.name;
        SoundManager.Inst?.Play(respawnSound, mixerGroup, 1f, false, 2f, 1f);
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= SceneLoadedHandler;
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (loadingScene.IsNull || startupScene.IsNull || gameUIScene.IsNull || menuScenes.Any(mc => mc.IsNull) || levelScenes.Any(lvl => lvl.IsNull)) {
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
            return;
        }

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(loadingScene.scenePath, true));
        scenes.Add(new EditorBuildSettingsScene(startupScene.scenePath, true));
        scenes.Add(new EditorBuildSettingsScene(gameUIScene.scenePath, true));

        for (int i = 0; i < menuScenes.Length; i++) {
            if (scenes.FirstOrDefault(s => s.path == menuScenes[i].scenePath) == null)
                scenes.Add(new EditorBuildSettingsScene(menuScenes[i].scenePath, true));
        }
        for (int i = 0; i < levelScenes.Length; i++)
            if (scenes.FirstOrDefault(s => s.path == levelScenes[i].scenePath) == null)
                scenes.Add(new EditorBuildSettingsScene(levelScenes[i].scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
#endif
}