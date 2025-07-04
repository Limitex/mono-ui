#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Limitex.MonoUI.Editor.Data;
using Newtonsoft.Json;

namespace Limitex.MonoUI.Editor.Inspector
{
    [CustomEditor(typeof(ColorPresetAsset))]
    public class ColorPresetEditor : UnityEditor.Editor
    {
        private string textareaInput = "";
        private bool showInput = false;
        private Vector2 scrollPosition = Vector2.zero;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Color Preset Editor", EditorStyles.boldLabel);

            showInput = EditorGUILayout.Foldout(showInput, "Import Presets");

            if (showInput)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200f));
                var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                textareaInput = EditorGUILayout.TextArea(textareaInput, style, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Apply"))
                {
                    ApplyToPreset();
                }
            }
            
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Color preset JSON to Clipboard"))
            {
                GenerateJson();
            }
        }

        private void ApplyToPreset()
        {
            var target = (ColorPresetAsset)serializedObject.targetObject;
            Undo.RecordObject(target, "Apply Color Preset");
            ColorPresetAsset.ParseJson(ref target, textareaInput);
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            Debug.Log("Color preset has been applied!");
        }

        private void GenerateJson()
        {
            var target = (ColorPresetAsset)serializedObject.targetObject;
            var json = ColorPresetAsset.ConvertToJson(target, Formatting.None);
            EditorGUIUtility.systemCopyBuffer = json;
            Debug.Log("Color preset JSON has been copied to clipboard!");
        }
    }
}
#endif
