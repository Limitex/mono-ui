using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Limitex.MonoUI.Editor.Components;
using Limitex.MonoUI.Editor.Data;
using Limitex.MonoUI.Editor.Utils;

#if UNITY_EDITOR
namespace Limitex.MonoUI.Editor.Inspector
{
    [CustomEditor(typeof(ColorManager))]
    public class ColorManagerEditor : UnityEditor.Editor
    {
        private enum TargetScope
        {
            Hierarchy,
            Prefab
        }

        private struct ProcessingStats
        {
            public int Total;
            public int Processed;

            public ProcessingStats(int total = 0, int processed = 0)
            {
                Total = total;
                Processed = processed;
            }

            public float Progress => Total > 0 ? (float)Processed / Total : 0f;

            public override string ToString() => $"Processed {Processed}/{Total} ({Progress * 100:F2}%)";

            public static ProcessingStats operator +(ProcessingStats a, ProcessingStats b) => new ProcessingStats
            {
                Total = a.Total + b.Total,
                Processed = a.Processed + b.Processed
            };
        }

        private readonly string[] SEATCH_DIRECTORIES = new[] { "Packages/dev.limitex.mono-ui/", "Assets/" };

        private static bool showChildColorManagers = false;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Color Manager Actions", EditorStyles.boldLabel);

            DrawActionRow("Refresh All Color Managers",
                () => UpdateAllColors(TargetScope.Hierarchy),
                () => UpdateAllColors(TargetScope.Prefab));
            DrawActionRow("Apply Preset to All Managers",
                () => ApplyPresetToAllManagers(TargetScope.Hierarchy),
                () => ApplyPresetToAllManagers(TargetScope.Prefab));
            DrawActionRow("Remove Invalid ComponentColors",
                () => RemoveInvalidComponentColors(TargetScope.Hierarchy),
                () => RemoveInvalidComponentColors(TargetScope.Prefab));

            EditorGUILayout.Space(10);

            showChildColorManagers = EditorGUILayout.Foldout(showChildColorManagers, "Child ColorManagers", true, EditorStyles.foldoutHeader);
            if (showChildColorManagers)
            {
                DrawChildColorManagerPositions();
            }
        }

        #region Action Methods

        private void UpdateAllColors(TargetScope targetScope)
        {
            ProcessingStats processingStats = ProcessManagersIn(targetScope, "Update Colors", 
                manager => manager.ValidateComponentColors());
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Updated {processingStats} ColorManager(s) in {logMessage}.");
        }

        private void ApplyPresetToAllManagers(TargetScope targetScope)
        {
            ColorPresetAsset newPreset = GetColorPresetAsset();
            if (newPreset == null) return;
            ProcessingStats processingStats = ProcessManagersIn(targetScope, "Apply New Preset", 
                manager => manager.SetColorPreset(newPreset));
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Applied new preset to {processingStats} ColorManager(s) in {logMessage}.");
        }

        private void RemoveInvalidComponentColors(TargetScope targetScope)
        {
            ProcessingStats processingStats = ProcessManagersIn(targetScope, "Remove Invalid ComponentColors", 
                manager => RemoveInvalidComponents(manager));
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Removed {processingStats} invalid ComponentColors from {logMessage}.");
        }

        #endregion

        #region Draw Methods

        private void DrawActionRow(string label, Action hierarchiesAction, Action prefabsAction)
        {
            EditorGUILayout.BeginHorizontal();

            try
            {
                EditorGUILayout.LabelField(label);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Hierarchies", GUILayout.Width(80)))
                {
                    hierarchiesAction.Invoke();
                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("Prefabs", GUILayout.Width(80)))
                {
                    prefabsAction.Invoke();
                    GUIUtility.ExitGUI();
                }
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawChildColorManagerPositions()
        {
            ColorManager targetManager = (ColorManager)target;
            Transform targetTransform = targetManager.transform;

            int foundColorManager = DrawColorManagerPositionsRecursive(targetTransform);

            if (foundColorManager != 0) return;

            EditorGUILayout.LabelField("No ColorManager found in children.");
        }

        private int DrawColorManagerPositionsRecursive(Transform parentTransform)
        {
            int foundColorManager = 0;
            foreach (Transform child in parentTransform)
            {
                ColorManager childManager = child.GetComponent<ColorManager>();
                if (childManager != null)
                {
                    foundColorManager++;

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(child.name);

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginChangeCheck();
                    GameObject selectedObject = (GameObject)EditorGUILayout.ObjectField(child.gameObject, typeof(GameObject), true);
                    if (EditorGUI.EndChangeCheck() && selectedObject != null)
                    {
                        Selection.activeGameObject = selectedObject;
                        EditorGUIUtility.PingObject(selectedObject);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                foundColorManager += DrawColorManagerPositionsRecursive(child);
            }

            return foundColorManager;
        }

        #endregion

        #region Helper Methods

        private ProcessingStats ProcessManagersIn(TargetScope targetScope, string undoRecordText, Func<ColorManager, bool> action)
        {
            ProcessingStats ProcessManagerAction(ColorManager manager) => new ProcessingStats(1, action(manager) ? 1 : 0);
            ProcessingStats processingStats = new ProcessingStats();
            switch (targetScope)
            {
                case TargetScope.Hierarchy:
                    processingStats = ProcessManagersIn<HierarchyComponentFinder<ColorManager>>(ProcessManagerAction, undoRecordText);
                    break;
                case TargetScope.Prefab:
                    if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                    {
                        Debug.LogWarning("Prefab mode detected. Prefab actions cannot be performed.");
                        return new ProcessingStats();
                    }
                    string[] guids = AssetDatabase.FindAssets("t:Prefab", SEATCH_DIRECTORIES);
                    foreach (string guid in guids)
                        processingStats += ProcessManagersIn<PrefabComponentFinder<ColorManager>>(ProcessManagerAction, undoRecordText, guid);
                    break;
            }
            return processingStats;
        }

        private ProcessingStats ProcessManagersIn<T>(Func<ColorManager, ProcessingStats> action, string undoRecordText, string guid = null) where T : ComponentFinderBase<ColorManager>
        {
            ProcessingStats processingStats = new ProcessingStats();
            using (var finder = (T)Activator.CreateInstance(typeof(T), guid))
            {
                foreach (var manager in finder)
                {
                    Undo.RecordObject(manager, undoRecordText);
                    processingStats += action(manager);
                    EditorUtility.SetDirty(manager);
                }
            }
            return processingStats;
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

        private bool RemoveInvalidComponents(ColorManager manager)
        {
            var serializedObject = new SerializedObject(manager);
            var componentColorsProperty = serializedObject.FindProperty("componentColors");

            if (componentColorsProperty == null || !componentColorsProperty.isArray) return false;

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
            return indicesToRemove.Count > 0;
        }

        #endregion
    }
}
#endif
