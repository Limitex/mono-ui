#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Limitex.MonoUI.Tools.Component;

namespace Limitex.MonoUI.Tools.Editor
{
    [CustomEditor(typeof(ComponentPhotoStudioManager))]
    public class ComponentPhotoStudioManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            ComponentPhotoStudioManager controller = (ComponentPhotoStudioManager)target;
            if (GUILayout.Button("Take Screenshot"))
            {
                controller.SaveScreenshot();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Reflesh Prefabs"))
            {
                controller.RefreshPrefabs();
            }

            if (GUILayout.Button("Reload Prefabs"))
            {
                controller.ReloadPrefabs();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Down"))
            {
                controller.DownPrefab();
            }

            if (GUILayout.Button("Up"))
            {
                controller.UpPrefab();
            }
        }
    }
}

#endif
