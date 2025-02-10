#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Limitex.MonoUI.Editor.Components;
using Limitex.MonoUI.Editor.Data;
using Limitex.MonoUI.Editor.Utils;
using System.Reflection;
using UnityEngine.UI;

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
            var colorManager = ((ColorManager)target).transform;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Color Manager Actions", EditorStyles.boldLabel);

            DrawActionRow("Refresh All Color Managers", new() {
                { "Children", () => UpdateAllColors(TargetScope.Hierarchy, colorManager) },
                { "Hierarchies", () => UpdateAllColors(TargetScope.Hierarchy) },
                { "Prefabs", () => UpdateAllColors(TargetScope.Prefab) }});
            DrawActionRow("Apply Preset to All Managers", new() {
                { "Children", () => ApplyPresetToAllManagers(TargetScope.Hierarchy, colorManager) },
                { "Hierarchies", () => ApplyPresetToAllManagers(TargetScope.Hierarchy) },
                { "Prefabs", () => ApplyPresetToAllManagers(TargetScope.Prefab) }});
            DrawActionRow("Remove Invalid ComponentColors", new() {
                { "Children", () => RemoveInvalidComponentColors(TargetScope.Hierarchy, colorManager) },
                { "Hierarchies", () => RemoveInvalidComponentColors(TargetScope.Hierarchy) },
                { "Prefabs",() => RemoveInvalidComponentColors(TargetScope.Prefab) }});

            EditorGUILayout.Space(10);

            showChildColorManagers = EditorGUILayout.Foldout(showChildColorManagers, "Child ColorManagers", true, EditorStyles.foldoutHeader);
            if (showChildColorManagers)
            {
                DrawChildColorManagerPositions();
            }
        }

        #region Action Methods

        private void UpdateAllColors(TargetScope targetScope, Transform parent = null)
        {
            int processingStats = ProcessManagersIn(targetScope, "Update Colors", 
                manager => manager.ValidateComponentColors(), parent);
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Updated {processingStats} ColorManager(s) in {logMessage}.");
        }

        private void ApplyPresetToAllManagers(TargetScope targetScope, Transform parent = null)
        {
            ColorPresetAsset newPreset = GetColorPresetAsset();
            if (newPreset == null) return;
            int processingStats = ProcessManagersIn(targetScope, "Apply New Preset", 
                manager => manager.SetColorPreset(newPreset), parent);
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Applied new preset to {processingStats} ColorManager(s) in {logMessage}.");
        }

        private void RemoveInvalidComponentColors(TargetScope targetScope, Transform parent = null)
        {
            int processingStats = ProcessManagersIn(targetScope, "Remove Invalid ComponentColors", 
                manager => RemoveInvalidComponents(manager), parent);
            string logMessage = targetScope == TargetScope.Hierarchy ? "hierarchy" : "prefabs";
            Debug.Log($"Removed {processingStats} invalid ComponentColors from {logMessage}.");
        }

        #endregion

        #region Draw Methods

        private void DrawActionRow(string label, Dictionary<string, Action> actions)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            foreach (var action in actions)
            {
                if (GUILayout.Button(action.Key))
                {
                    action.Value.Invoke();
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndHorizontal();
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

        private int ProcessManagersIn(TargetScope targetScope, string undoRecordText, Func<ColorManager, int> action, Transform parent)
        {
            int processingStats = 0;
            switch (targetScope)
            {
                case TargetScope.Hierarchy:
                    processingStats = ProcessManagersIn<HierarchyComponentFinder<ColorManager>>(action, undoRecordText, includeInactive: true, parent: parent);
                    break;
                case TargetScope.Prefab:
                    if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                    {
                        Debug.LogWarning("Prefab mode detected. Prefab actions cannot be performed.");
                        return 0;
                    }
                    string[] guids = AssetDatabase.FindAssets("t:Prefab", SEATCH_DIRECTORIES);
                    foreach (string guid in guids)
                        processingStats += ProcessManagersIn<PrefabComponentFinder<ColorManager>>(action, undoRecordText, guid, includeInactive: true);
                    break;
            }
            return processingStats;
        }

        private int ProcessManagersIn<T>(Func<ColorManager, int> action, string undoRecordText, string guid = null, bool includeInactive = false, Transform parent = null) where T : ComponentFinderBase<ColorManager>
        {
            int processingStats = 0;
            using (var finder = (T)Activator.CreateInstance(typeof(T), guid, includeInactive, parent))
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

        private int RemoveInvalidComponents(ColorManager manager)
        {
            var serializedObject = new SerializedObject(manager);
            var componentColorsProperty = serializedObject.FindProperty("componentColors");

            if (componentColorsProperty == null || !componentColorsProperty.isArray) return 0;

            var components = new Dictionary<UnityEngine.Object, SerializedProperty>();
            var invalidIndices = new List<int>();

            for (int i = 0; i < componentColorsProperty.arraySize; i++)
            {
                var element = componentColorsProperty.GetArrayElementAtIndex(i);
                var component = element.FindPropertyRelative("component").objectReferenceValue;
                var colorType = (ColorType)element.FindPropertyRelative("colorType").intValue;
                var transitionColorType = (TransitionColorType)element.FindPropertyRelative("transitionColorType").intValue;

                if (component == null ||
                    ((component is Graphic or Selectable) && colorType == ColorType.None && transitionColorType == TransitionColorType.None) ||
                    components.ContainsKey(component))
                {
                    invalidIndices.Add(i);
                    continue;
                }

                components.Add(component, element);

                var colorFields = element.FindPropertyRelative("colorFields");
                if (colorFields == null || !colorFields.isArray) continue;

                CleanupInvalidColorFields(component, colorFields);
            }

            foreach (int index in invalidIndices.OrderByDescending(i => i))
            {
                componentColorsProperty.DeleteArrayElementAtIndex(index);
            }

            serializedObject.ApplyModifiedProperties();
            return invalidIndices.Count;
        }

        private void CleanupInvalidColorFields(UnityEngine.Object component, SerializedProperty colorFields)
        {
            var validFields = new HashSet<string>(
                component.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(field => field.Name));

            var validColorFields = new Dictionary<string, SerializedProperty>();
            var invalidFieldIndices = new List<int>();
            for (int j = 0; j < colorFields.arraySize; j++)
            {
                var fieldElement = colorFields.GetArrayElementAtIndex(j);
                var fieldName = fieldElement.FindPropertyRelative("fieldName").stringValue;
                var fieldColorType = (ColorType)fieldElement.FindPropertyRelative("colorType").intValue;
                if (string.IsNullOrEmpty(fieldName) || fieldColorType == ColorType.None || 
                    !validFields.Contains(fieldName) || validColorFields.ContainsKey(fieldName))
                {
                    invalidFieldIndices.Add(j);
                    continue;
                }
                validColorFields.Add(fieldName, fieldElement);
            }

            foreach (int index in invalidFieldIndices.OrderByDescending(i => i))
            {
                colorFields.DeleteArrayElementAtIndex(index);
            }
        }

        #endregion
    }
}

#endif
