using UnityEngine;
using UnityEditor;

namespace SoundWaves.Editors {
    [CustomEditor(typeof(SoundWave))]
    public class SoundWaveEditor : Editor {
        public override void OnInspectorGUI() {
            bool temp = GUI.enabled;
            if (!Application.isPlaying) GUI.enabled = false;
            SoundWave sw = target as SoundWave;
            if (GUILayout.Button("Trigger Now", GUILayout.Height(50f))) sw.Trigger();
            GUI.enabled = temp;

            base.OnInspectorGUI();
        }
    }
}