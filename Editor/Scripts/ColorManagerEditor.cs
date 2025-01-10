using UnityEngine;
using UnityEditor;
using Limitex.MonoUI.Theme;

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

            if (GUILayout.Button("Refresh All Color Manager"))
            {
                UpdateAllColorsInHierarchy();
            }

            if (GUILayout.Button("Update All Color Manager Presets"))
            {
                ApplyPresetToAllManagers();
            }

            if (GUILayout.Button("Remove Invalid ComponentColors"))
            {
                RemoveInvalidComponentColors();
            }
        }

        private void UpdateAllColorsInHierarchy()
        {
            ColorManager[] colorManagers = FindObjectsOfType<ColorManager>();

            foreach (var manager in colorManagers)
            {
                manager.OnValidate();
            }

            Debug.Log($"Updated colors for {colorManagers.Length} ColorManager(s).");
        }

        private void ApplyPresetToAllManagers()
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

            Debug.Log($"Applied new preset to {colorManagers.Length} ColorManager(s).");
        }

        private void RemoveInvalidComponentColors()
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
                    var colorType = (ColorType)element.FindPropertyRelative("colorType").enumValueIndex;
                    var transitionColorType = (TransitionColorType)element.FindPropertyRelative("transitionColorType").enumValueIndex;

                    if (component != null && !(colorType == ColorType.None && transitionColorType == TransitionColorType.None))
                        continue;

                    componentColorsProperty.DeleteArrayElementAtIndex(i);
                    totalRemoved++;
                }

                serializedObject.ApplyModifiedProperties();
            }

            Debug.Log($"Removed {totalRemoved} invalid ComponentColors.");
        }
    }
}
#endif
