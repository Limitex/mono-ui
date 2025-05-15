#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Limitex.MonoUI.Editor.Data;
using System.Reflection;
using System.Linq;

namespace Limitex.MonoUI.Editor.Components
{
    [AddComponentMenu("Mono UI/MI Color Manager")]
    [DisallowMultipleComponent]
    public class ColorManager : MonoBehaviour
    {
        [Serializable]
        public struct ComponentColorFieldInfo
        {
            public string fieldName;
            public ColorType colorType;
        }

        [Serializable]
        public struct ComponentColor
        {
            public Component component;
            public ColorType colorType;
            public TransitionColorType transitionColorType;
            public ComponentColorFieldInfo[] colorFields;
        }

        [SerializeField] private ComponentColor[] componentColors;
        [SerializeField] private ColorPresetAsset colorPreset;

        private readonly string DEFAULT_COLOR_PRESET_NAME = "Color Preset (Default)";
        private readonly string[] SEATCH_DIRECTORIES = new[] { "Packages/dev.limitex.mono-ui/Editor/Assets/ColorPresets/" };

        private void Reset()
        {
            string guid = AssetDatabase.FindAssets($"t:ColorPresetAsset {DEFAULT_COLOR_PRESET_NAME}", SEATCH_DIRECTORIES)[0];

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError("Default ColorPresetAsset not found! Please assign one manually.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            colorPreset = AssetDatabase.LoadAssetAtPath<ColorPresetAsset>(path);
        }

        private void OnValidate()
        {
            ValidateComponentColors();
        }

        public int ValidateComponentColors()
        {
            if (colorPreset == null)
            {
                Debug.LogError("ColorPresetAsset is not assigned!");
                return 0;
            }

            if (componentColors == null) return 0;

            int success = 0;
            List<int> componentError = new List<int>();
            List<int> colorError = new List<int>();

            for (int i = 0; i < componentColors.Length; i++)
            {
                if (componentColors[i].component == null)
                {
                    componentError.Add(i);
                    continue;
                }

                if (componentColors[i].colorType == ColorType.None &&
                    componentColors[i].transitionColorType == TransitionColorType.None && 
                    componentColors[i].colorFields.Length == 0)
                {
                    colorError.Add(i);
                    continue;
                }

                if (componentColors[i].colorFields.Any(item =>
                    string.IsNullOrEmpty(item.fieldName) || item.colorType == ColorType.None))
                {
                    colorError.Add(i);
                }

                if (componentColors[i].component is Graphic)
                {
                    componentColors[i].transitionColorType = TransitionColorType.None;
                    if (componentColors[i].colorType == 0)
                        componentColors[i].colorType = ColorType.None;
                    componentColors[i].colorFields = new ComponentColorFieldInfo[0];
                }
                else if (componentColors[i].component is Selectable)
                {
                    componentColors[i].colorType = ColorType.None;
                    if (componentColors[i].transitionColorType == 0)
                        componentColors[i].transitionColorType = TransitionColorType.None;
                    componentColors[i].colorFields = new ComponentColorFieldInfo[0];
                }
                else
                {
                    componentColors[i].colorType = ColorType.None;
                    componentColors[i].transitionColorType = TransitionColorType.None;
                    for (int j = 0; j < componentColors[i].colorFields.Length; j++)
                    {
                        if (componentColors[i].colorFields[j].colorType == 0)
                            componentColors[i].colorFields[j].colorType = ColorType.None;
                    }
                }

                success += ApplyColors(ref componentColors[i]);
            }

            if (componentError.Count > 0)
            {
                Debug.LogWarning($"Component(s) at index(es) {string.Join(", ", componentError)} is/are not assigned. {GetLocationLog()}");
            }

            if (colorError.Count > 0)
            {
                Debug.LogWarning($"Color(s) at index(es) {string.Join(", ", colorError)} is/are not assigned. {GetLocationLog()}");
            }

            return success;
        }

        public int SetColorPreset(ColorPresetAsset newPreset)
        {
            if (colorPreset == newPreset) return 0;
            colorPreset = newPreset;
            OnValidate();
            return 1;
        }

        #region Helper Methods

        private int ApplyColors(ref ComponentColor cc)
        {
            if (cc.component == null) return 0;

            if (cc.component is Graphic graphic)
            {
                Color? color = colorPreset?.GetColorByType(cc.colorType);
                if (color.HasValue)
                {
                    return ApplyColorToUIElement(graphic, color.Value);
                }
            }
            else if (cc.component is Selectable selectable)
            {
                TransitionColor? transitionColor = colorPreset?.GetTransitionColorByType(cc.transitionColorType);
                if (transitionColor.HasValue)
                {
                    return ApplyTransitionColors(selectable, transitionColor.Value);
                }
            }
            else if (cc.component is MonoBehaviour mono)
            {
                int changed = 0;
                foreach (var fieldInfo in cc.colorFields)
                {
                    var field = mono.GetType().GetField(fieldInfo.fieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null) continue;

                    Color? color = colorPreset?.GetColorByType(fieldInfo.colorType);
                    if (!color.HasValue) continue;

                    if ((Color)field.GetValue(mono) == color.Value) continue;

                    field.SetValue(mono, color.Value);
                    changed++;
                }
                return changed;
            }

            return 0;
        }

        private int ApplyColorToUIElement<T>(T uiElement, Color color) where T : Graphic
        {
            if (uiElement == null) return 0;
            if (uiElement.color == color) return 0;

            uiElement.color = color;
            return 1;
        }

        private int ApplyTransitionColors(Selectable selectable, TransitionColor transitionColor)
        {
            if (selectable == null) return 0;
            if (selectable.colors.normalColor == transitionColor.Normal &&
                    selectable.colors.highlightedColor == transitionColor.Highlighted &&
                    selectable.colors.pressedColor == transitionColor.Pressed &&
                    selectable.colors.selectedColor == transitionColor.Selected &&
                    selectable.colors.disabledColor == transitionColor.Disabled)
                return 0;

            ColorBlock colorBlock = selectable.colors;
            colorBlock.normalColor = transitionColor.Normal;
            colorBlock.highlightedColor = transitionColor.Highlighted;
            colorBlock.pressedColor = transitionColor.Pressed;
            colorBlock.selectedColor = transitionColor.Selected;
            colorBlock.disabledColor = transitionColor.Disabled;
            selectable.colors = colorBlock;

            return 1;
        }

        private string GetHierarchyPath(Transform transform)
        {
            var path = new StringBuilder(transform.name);
            for (var parent = transform.parent; parent != null; parent = parent.parent)
                path.Insert(0, parent.name + "/");
            return path.ToString();
        }

        private string GetLocationLog() => 
            $"GameObject: {this.gameObject.name} at path: {GetHierarchyPath(this.transform)}";

        #endregion
    }
}

#endif
