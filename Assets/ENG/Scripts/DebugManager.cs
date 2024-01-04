using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour {
    public static DebugManager Inst { get; private set; }

    public static bool InvincibleMode { get; private set; } = false;

    private static readonly List<DebugAction> debugActions = new List<DebugAction>() {
        new DebugAction {
            name = "Load First Level",
            action = () => GameManager.Inst.LoadFirstLevel(),
            state = () => ""
        },
        new DebugAction {
            name = "Load Savegame",
            action = () => GameManager.Inst.LoadSavegame(),
            state = () => ""
        },
        new DebugAction {
            name = "Load Next Level",
            action = () => GameManager.Inst.LoadNextLevel(),
            state = () => ""
        },
        new DebugAction {
            name = "Respawn",
            action = () => GameManager.Inst.Respawn(),
            state = () => ""
        },
        new DebugAction {
            name = "Reset PlayerPrefs",
            action = () => GameManager.Inst.ResetPlayerPrefs(),
            state = () => ""
        },
        new DebugAction {
            name = "Toggle Invincible Mode",
            action = () => InvincibleMode = !InvincibleMode,
            state = () => InvincibleMode ? "On" : "Off"
        },
        new DebugAction {
            name = "Kinda Thick",
            action = () => {
                if (Player.PlayerController.Current != null) {
                    Player.PlayerController.Current.transform.localScale = new Vector3(4f, 2f, 4f);
                }
            },
            state = () => ""
        }
    };

    private bool menuShown = false;

    private const float guiBtnHeight = 34f;
    private readonly Rect guiAreaRect = new Rect(50f, 50f, 400f, Screen.height - 100f);

    private void Awake() {
        // Singleton handling
        if (Inst) {
            Destroy(gameObject);
            return;
        } else {
            // Dont destroy on load handled by the ManagersInit script
            Inst = this;
        }
    }

    private void Update() {
        if (Keyboard.current.f1Key.wasPressedThisFrame) ToggleMenu();
    }

    private void OnGUI() {
        if (menuShown) RenderMenu();
    }

    [ContextMenu("Toggle Debug Menu")]
    public void ToggleMenu() {
        menuShown = !menuShown;
        if (menuShown) GameManager.Inst.SetCursorModeMenu();
    }

    private void RenderMenu() {
        GUILayout.BeginArea(guiAreaRect, "Debug Menu", GUI.skin.window);

        foreach (DebugAction action in debugActions) {
            string state = action.state.Invoke();
            if (GUILayout.Button(action.name + (string.IsNullOrWhiteSpace(state) ? "" : ": " + state), GUILayout.Height(guiBtnHeight))) {
                action.action.Invoke();
            }
        }

        GUILayout.EndArea();
    }

    private struct DebugAction {
        public string name;
        public Action action;
        public Func<string> state;
    }
}
