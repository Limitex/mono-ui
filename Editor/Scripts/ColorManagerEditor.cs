using UnityEngine;
using UnityEditor;
using Limitex.MonoUI.Theme;
using System;
using Limitex.MonoUI.Editor.Utils;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
namespace Limitex.MonoUI.Editor.Theme
{
    [CustomEditor(typeof(ColorManager))]
    public class ColorManagerEditor : UnityEditor.Editor
    {
        private enum TargetScope
        {
            Hierarchy,
            Prefab
        }

        private readonly string[] SEATCH_DIRECTORIES = new[] { "Packages/dev.limitex.mono-ui/", "Assets/" };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            DrawActionRow("Refresh All Color Managers",
                () => UpdateAllColors(TargetScope.Hierarchy),
                () => UpdateAllColors(TargetScope.Prefab));
            DrawActionRow("Apply Preset to All Managers",
                () => ApplyPresetToAllManagers(TargetScope.Hierarchy),
                () => ApplyPresetToAllManagers(TargetScope.Prefab));
            DrawActionRow("Remove Invalid ComponentColors",
                () => RemoveInvalidComponentColors(TargetScope.Hierarchy),
                () => RemoveInvalidComponentColors(TargetScope.Prefab));
        }

        private void UpdateAllColors(TargetScope targetScope)
        {
            int totalProcessed = ProcessManagersIn(targetScope, "Update Colors", 
                manager => manager.OnValidate());
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Updated {totalProcessed} ColorManager(s) in {logMessage}.");
        }

        private void ApplyPresetToAllManagers(TargetScope targetScope)
        {
            ColorPresetAsset newPreset = GetColorPresetAsset();
            if (newPreset == null) return;
            int totalProcessed = ProcessManagersIn(targetScope, "Apply New Preset", 
                manager => manager.SetColorPreset(newPreset));
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Applied new preset to {totalProcessed} ColorManager(s) in {logMessage}.");
        }

        private void RemoveInvalidComponentColors(TargetScope targetScope)
        {
            int totalRemoved = ProcessManagersIn(targetScope, "Remove Invalid ComponentColors", 
                manager => RemoveInvalidComponents(manager));
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Removed {totalRemoved} invalid ComponentColors from {logMessage}.");
        }

        #region Helper Methods

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

        private int ProcessManagersIn(TargetScope targetScope, string undoRecordText, Action<ColorManager> action)
        {
            int count = 0;
            switch (targetScope)
            {
                case TargetScope.Hierarchy:
                    count = ProcessManagersIn<HierarchyComponentFinder<ColorManager>>(action, undoRecordText);
                    break;
                case TargetScope.Prefab:
                    if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                    {
                        Debug.LogWarning("Prefab mode detected. Prefab actions cannot be performed.");
                        return 0;
                    }
                    string[] guids = AssetDatabase.FindAssets("t:Prefab", SEATCH_DIRECTORIES);
                    foreach (string guid in guids)
                        count += ProcessManagersIn<PrefabComponentFinder<ColorManager>>(action, undoRecordText, guid);
                    break;
            }
            return count;
        }

        private int ProcessManagersIn<T>(Action<ColorManager> action, string undoRecordText, string guid = null) where T : ComponentFinderBase<ColorManager>
        {
            int totalProcessed = 0;
            using (var finder = (T)Activator.CreateInstance(typeof(T), guid))
            {
                foreach (var manager in finder)
                {
                    Undo.RecordObject(manager, undoRecordText);
                    action(manager);
                    EditorUtility.SetDirty(manager);
                }
                totalProcessed += finder.Count();
            }

            return totalProcessed;
        }

        private ColorPresetAsset GetColorPresetAsset()
        {
            string presetPath = EditorUtility.OpenFilePanel("Select Color Preset Asset", "Assets", "asset");
            if (string.IsNullOrEmpty(presetPath))
            {
                Debug.LogWarning("Preset selection canceled.");
                return null;
            }

            string relativePath = FileUtil.GetProjectRelativePath(presetPath);
            ColorPresetAsset newPreset = AssetDatabase.LoadAssetAtPath<ColorPresetAsset>(relativePath);

            if (newPreset == null)
            {
                Debug.LogError($"Failed to load ColorPresetAsset at path: {relativePath}");
                return null;
            }

            return newPreset;
        }

        private void RemoveInvalidComponents(ColorManager manager)
        {
            var serializedObject = new SerializedObject(manager);
            var componentColorsProperty = serializedObject.FindProperty("componentColors");

            if (componentColorsProperty == null || !componentColorsProperty.isArray) return;

            HashSet<UnityEngine.Object> seenComponents = new HashSet<UnityEngine.Object>();
            List<int> indicesToRemove = new List<int>();

            for (int i = 0; i < componentColorsProperty.arraySize; i++)
            {
                var element = componentColorsProperty.GetArrayElementAtIndex(i);
                var component = element.FindPropertyRelative("component").objectReferenceValue;
                var colorType = (ColorType)element.FindPropertyRelative("colorType").intValue;
                var transitionColorType = (TransitionColorType)element.FindPropertyRelative("transitionColorType").intValue;

                if (component != null && !seenComponents.Add(component))
                {
                    indicesToRemove.Add(i);
                    continue;
                }

                if (component == null || (colorType == ColorType.None && transitionColorType == TransitionColorType.None))
                {
                    indicesToRemove.Add(i);
                }
            }

            foreach (int index in indicesToRemove.OrderByDescending(i => i))
            {
                componentColorsProperty.DeleteArrayElementAtIndex(index);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
#endif
