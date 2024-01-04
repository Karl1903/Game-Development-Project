using UnityEngine;

public static class ManagersInit {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init() {
        Object prefab = Resources.Load("Managers");
        Object managers = Object.Instantiate(prefab);
        managers.name = prefab.name;
        Object.DontDestroyOnLoad(managers);
    }
}