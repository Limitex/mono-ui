using UnityEngine;
using UnityEditor;
using Limitex.MonoUI.Theme;
using System;

#if UNITY_EDITOR
namespace Limitex.MonoUI.Editor.Theme
{
    [CustomEditor(typeof(ColorManager))]
    public class ColorManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            DrawActionRow("Refresh All Color Managers",
                UpdateAllColorsInHierarchy,
                UpdateAllColorsInPrefab);
            DrawActionRow("Apply Preset to All Managers",
                ApplyPresetToAllManagersInHierarchy,
                ApplyPresetToAllManagersInPrefab);
            DrawActionRow("Remove Invalid ComponentColors",
                RemoveInvalidComponentColorsFromHierarchy,
                RemoveInvalidComponentColorsFromPrefabs);
        }

        private void DrawActionRow(string label, Action hierarchiesAction, Action prefabsAction)
        {
            EditorGUILayout.BeginHorizontal();

            try
            {
                EditorGUILayout.LabelField(label);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Hierarchies", GUILayout.Width(80)))
                    hierarchiesAction.Invoke();

                if (GUILayout.Button("Prefabs", GUILayout.Width(80)))
                    prefabsAction.Invoke();
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);
        }

        private void UpdateAllColorsInHierarchy()
        {
            ColorManager[] colorManagers = FindObjectsOfType<ColorManager>();

            foreach (var manager in colorManagers)
            {
                manager.OnValidate();
            }

            Debug.Log($"Updated colors for {colorManagers.Length} ColorManager(s) in the hierarchy.");
        }

        private void ApplyPresetToAllManagersInHierarchy()
        {
            string presetPath = EditorUtility.OpenFilePanel("Select Color Preset Asset", "Assets", "asset");
            if (string.IsNullOrEmpty(presetPath))
            {
                Debug.LogWarning("Preset selection canceled.");
                return;
            }

            string relativePath = FileUtil.GetProjectRelativePath(presetPath);
            ColorPresetAsset newPreset = AssetDatabase.LoadAssetAtPath<ColorPresetAsset>(relativePath);

            if (newPreset == null)
            {
                Debug.LogError($"Failed to load ColorPresetAsset at path: {relativePath}");
                return;
            }

            ColorManager[] colorManagers = FindObjectsOfType<ColorManager>();
            foreach (var manager in colorManagers)
            {
                Undo.RecordObject(manager, "Apply New Preset");
                manager.SetColorPreset(newPreset);
                EditorUtility.SetDirty(manager);
            }

            Debug.Log($"Applied new preset to {colorManagers.Length} ColorManager(s) in the hierarchy.");
        }

        private void RemoveInvalidComponentColorsFromHierarchy()
        {
            ColorManager[] colorManagers = FindObjectsOfType<ColorManager>();
            int totalRemoved = 0;

            foreach (var manager in colorManagers)
            {
                var serializedObject = new SerializedObject(manager);
                var componentColorsProperty = serializedObject.FindProperty("componentColors");

                if (componentColorsProperty == null || !componentColorsProperty.isArray) continue;

                for (int i = componentColorsProperty.arraySize - 1; i >= 0; i--)
                {
                    var element = componentColorsProperty.GetArrayElementAtIndex(i);
                    var component = element.FindPropertyRelative("component").objectReferenceValue;
                    var colorType = (ColorType)element.FindPropertyRelative("colorType").intValue;
                    var transitionColorType = (TransitionColorType)element.FindPropertyRelative("transitionColorType").intValue;

                    if (component != null && !(colorType == ColorType.None && transitionColorType == TransitionColorType.None))
                        continue;

                    componentColorsProperty.DeleteArrayElementAtIndex(i);
                    totalRemoved++;
                }

                serializedObject.ApplyModifiedProperties();
            }

            Debug.Log($"Removed {totalRemoved} invalid ComponentColors in the hierarchy.");
        }

        private void UpdateAllColorsInPrefab()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Packages/dev.limitex.mono-ui/", "Assets/" });
            int updatedCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = PrefabUtility.LoadPrefabContents(assetPath);

                if (prefab == null)
                {
                    Debug.LogWarning($"Failed to load prefab at {assetPath}");
                    continue;
                }

                ColorManager[] colorManagers = prefab.GetComponentsInChildren<ColorManager>(true);
                foreach (var manager in colorManagers)
                {
                    manager.OnValidate();
                }

                PrefabUtility.SaveAsPrefabAsset(prefab, assetPath);
                PrefabUtility.UnloadPrefabContents(prefab);
                updatedCount += colorManagers.Length;
            }

            Debug.Log($"Updated {updatedCount} ColorManager(s) in prefabs.");
        }

        private void ApplyPresetToAllManagersInPrefab()
        {
            string presetPath = EditorUtility.OpenFilePanel("Select Color Preset Asset", "Assets", "asset");
            if (string.IsNullOrEmpty(presetPath))
            {
                Debug.LogWarning("Preset selection canceled.");
                return;
            }

            string relativePath = FileUtil.GetProjectRelativePath(presetPath);
            ColorPresetAsset newPreset = AssetDatabase.LoadAssetAtPath<ColorPresetAsset>(relativePath);

            if (newPreset == null)
            {
                Debug.LogError($"Failed to load ColorPresetAsset at path: {relativePath}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Packages/dev.limitex.mono-ui/", "Assets/" });
            int updatedCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = PrefabUtility.LoadPrefabContents(assetPath);

                if (prefab == null)
                {
                    Debug.LogWarning($"Failed to load prefab at {assetPath}");
                    continue;
                }

                ColorManager[] colorManagers = prefab.GetComponentsInChildren<ColorManager>(true);
                foreach (var manager in colorManagers)
                {
                    manager.SetColorPreset(newPreset);
                }

                PrefabUtility.SaveAsPrefabAsset(prefab, assetPath);
                PrefabUtility.UnloadPrefabContents(prefab);
                updatedCount += colorManagers.Length;
            }

            Debug.Log($"Applied new preset to {updatedCount} ColorManager(s) in prefabs.");
        }

        private void RemoveInvalidComponentColorsFromPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Packages/dev.limitex.mono-ui/", "Assets/" });
            int totalRemoved = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = PrefabUtility.LoadPrefabContents(assetPath);

                if (prefab == null)
                {
                    Debug.LogWarning($"Failed to load prefab at {assetPath}");
                    continue;
                }

                ColorManager[] colorManagers = prefab.GetComponentsInChildren<ColorManager>(true);
                foreach (var manager in colorManagers)
                {
                    var serializedObject = new SerializedObject(manager);
                    var componentColorsProperty = serializedObject.FindProperty("componentColors");

                    if (componentColorsProperty == null || !componentColorsProperty.isArray) continue;

                    for (int i = componentColorsProperty.arraySize - 1; i >= 0; i--)
                    {
                        var element = componentColorsProperty.GetArrayElementAtIndex(i);
                        var component = element.FindPropertyRelative("component").objectReferenceValue;
                        var colorType = (ColorType)element.FindPropertyRelative("colorType").intValue;
                        var transitionColorType = (TransitionColorType)element.FindPropertyRelative("transitionColorType").intValue;

                        if (component != null && !(colorType == ColorType.None && transitionColorType == TransitionColorType.None))
                            continue;

                        componentColorsProperty.DeleteArrayElementAtIndex(i);
                        totalRemoved++;
                    }

                    serializedObject.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(prefab, assetPath);
                PrefabUtility.UnloadPrefabContents(prefab);
            }

            Debug.Log($"Removed {totalRemoved} invalid ComponentColors from prefabs.");
        }
    }
}
#endif
