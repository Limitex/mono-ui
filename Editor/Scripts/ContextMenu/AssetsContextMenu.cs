#if UNITY_EDITOR

using Limitex.MonoUI.Editor.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Core;

namespace Limitex.MonoUI.Editor.ContextMenu
{
    public class AssetsContextMenu
    {
        #region Constants

        private const string ASSET_ROOT_PATH = "Packages/dev.limitex.mono-ui/Editor/Assets/";
        private const string ASSET_COLOR_DEFAULT_PATH = ASSET_ROOT_PATH + "ColorPresets/Color Preset (Default).asset";
        private const string ASSET_COLOR_LIGHT_PATH = ASSET_ROOT_PATH + "ColorPresets/Color Preset (Light).asset";
        
        private const string MENU_ROOT = "Assets/Create/Mono UI/";
        private const string MENU_COLOR = MENU_ROOT + "Color Preset/";

        private const int MENU_PRIORITY = 11;
        private const int COLOR_PRIORITY = MENU_PRIORITY;

        #endregion

        #region Utility Methods

        public static void SpawnAsset(string path)
        {
            ColorPresetAsset preset = AssetDatabase.LoadAssetAtPath<ColorPresetAsset>(path);
            if (preset == null)
            {
                Debug.LogError("Failed to load preset.");
                return;
            }
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string destinationPath = AssetDatabase.IsValidFolder(assetPath) ? assetPath : "Assets";
            string newAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{destinationPath}/NewColorPreset.asset");
            ColorPresetAsset newConfig = Object.Instantiate(preset);
            AssetDatabase.CreateAsset(newConfig, newAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var newAsset = AssetDatabase.LoadAssetAtPath<ColorPresetAsset>(newAssetPath);
            Selection.activeObject = newAsset;
        }

        #endregion

        #region Color Preset

        [MenuItem(MENU_COLOR + "Dark Preset", false, COLOR_PRIORITY)]
        public static void CreateDefaultColorPreset() => SpawnAsset(ASSET_COLOR_DEFAULT_PATH);

        [MenuItem(MENU_COLOR + "Light Preset", false, COLOR_PRIORITY)]
        public static void CreateLightColorPreset() => SpawnAsset(ASSET_COLOR_LIGHT_PATH);

        #endregion
    }
}

 #endif
