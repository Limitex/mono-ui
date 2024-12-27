/*
 * This tool uses lucide-react
 * © 2022 Lucide Contributors - ISC License
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using Limitex.MonoUI.Lucide;

#if UNITY_EDITOR
namespace Limitex.MonoUI.Editor.Lucide
{
    [CustomEditor(typeof(LucideManager))]
    public class LucideManagerEditor : UnityEditor.Editor
    {
        private struct StatusMessage
        {
            public string Text { get; }
            public MessageType Type { get; }

            public StatusMessage(string text, MessageType type)
            {
                Text = text;
                Type = type;
            }

            public static StatusMessage Warning(string text) => new StatusMessage(text, MessageType.Warning);
            public static StatusMessage Info(string text) => new StatusMessage(text, MessageType.Info);
            public static StatusMessage None => new StatusMessage(null, MessageType.None);
        }

        private const string SPRITE_SEARCH_PATH = "Packages/dev.limitex.mono-ui/Runtime/Assets/Lucide";
        private const string LUCIDE_BROWSE_URL = "https://lucide.dev/icons/";
        private const string LUCIDE_REPO_URL = "https://github.com/lucide-icons/lucide";
        private const string PACKAGE_VERSION = "lucide-react@0.469.0";
        private const string COPYRIGHT_TEXT = "© 2022 Lucide Contributors - ISC License";

        private const string DESCRIPTION_TEXT = "This component helps manage Lucide icons for UI Images. Enter the icon name and click Apply to set the sprite.";

        private const string MSG_ENTER_NAME = "Please enter an icon name.";
        private const string MSG_DIR_NOT_FOUND = "Directory {0} does not exist.";
        private const string MSG_ICON_NOT_FOUND = "Icon '{0}' not found.";
        private const string MSG_FAILED_LOAD = "Failed to load icon at path: {0}";
        private const string MSG_SUCCESS = "Successfully applied icon: {0}";

        private SerializedProperty imageFileNameProperty;
        private StatusMessage statusMessage;

        private void OnEnable()
        {
            imageFileNameProperty = serializedObject.FindProperty("imageFileName");

            LucideManager manager = (LucideManager)target;
            if (!manager.GetComponent<Image>())
            {
                manager.gameObject.AddComponent<Image>();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(DESCRIPTION_TEXT, MessageType.None);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(imageFileNameProperty, new GUIContent("Icon Name"));
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                Application.OpenURL(LUCIDE_BROWSE_URL);
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(statusMessage.Text))
            {
                EditorGUILayout.HelpBox(statusMessage.Text, statusMessage.Type);
            }

            if (GUILayout.Button("Apply"))
            {
                ApplyImage();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Icons are converted from Lucide React SVGs to Unity sprites.", EditorStyles.miniLabel);
            if (GUILayout.Button("View on GitHub", GUILayout.Width(120)))
            {
                Application.OpenURL(LUCIDE_REPO_URL);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(PACKAGE_VERSION, EditorStyles.miniLabel);

            EditorGUILayout.LabelField(COPYRIGHT_TEXT, EditorStyles.miniLabel);

            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyImage()
        {
            LucideManager manager = (LucideManager)target;
            Image imageComponent = manager.GetComponent<Image>();
            statusMessage = StatusMessage.None;

            if (string.IsNullOrEmpty(manager.imageFileName))
            {
                statusMessage = StatusMessage.Warning(MSG_ENTER_NAME);
                return;
            }

            if (!System.IO.Directory.Exists(SPRITE_SEARCH_PATH))
            {
                statusMessage = StatusMessage.Warning(string.Format(MSG_DIR_NOT_FOUND, SPRITE_SEARCH_PATH));
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:sprite", new[] { SPRITE_SEARCH_PATH });

            var exactMatch = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => System.IO.Path.GetFileNameWithoutExtension(path) == manager.imageFileName)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(exactMatch))
            {
                statusMessage = StatusMessage.Warning(string.Format(MSG_ICON_NOT_FOUND, manager.imageFileName));
                imageComponent.sprite = null;
                EditorUtility.SetDirty(imageComponent);
                return;
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(exactMatch);

            if (sprite != null)
            {
                imageComponent.sprite = sprite;
                EditorUtility.SetDirty(imageComponent);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                statusMessage = StatusMessage.Info(string.Format(MSG_SUCCESS, sprite.name));
            }
            else
            {
                statusMessage = StatusMessage.Warning(string.Format(MSG_FAILED_LOAD, exactMatch));
                imageComponent.sprite = null;
                EditorUtility.SetDirty(imageComponent);
            }
        }
    }
}
#endif