using UnityEditor;
using UnityEngine;

namespace Settings
{
    [CustomEditor(typeof(SettingsManager))]
    public class SettingsManagerEditor : Editor
    {
        SettingsManager tg;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            tg = (SettingsManager)target;

            if (GUILayout.Button("Save Settings"))
                tg.SaveSettings();

            if (GUILayout.Button("Apply Settings"))
                tg.ApplySettings();
        }
    }
}