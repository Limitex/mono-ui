#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Limitex.MonoUI.Editor.Data;

namespace Limitex.MonoUI.Editor.Components
{
    [DisallowMultipleComponent]
    public class ColorManager : MonoBehaviour
    {
        [Serializable]
        public struct ComponentColor
        {
            public Component component;
            public ColorType colorType;
            public TransitionColorType transitionColorType;
        }

        [SerializeField] private ComponentColor[] componentColors;
        [SerializeField] private ColorPresetAsset colorPreset;

        private readonly string DEFAULT_COLOR_PRESET_NAME = "DefaultColorPreset";
        private readonly string[] SEATCH_DIRECTORIES = new[] { "Packages/dev.limitex.mono-ui/Editor/Assets/ColorPreset/" };

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

        public bool ValidateComponentColors()
        {
            if (colorPreset == null)
            {
                Debug.LogError("ColorPresetAsset is not assigned!");
                return false;
            }

            if (componentColors == null) return false;

            bool success = false;
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
                    componentColors[i].transitionColorType == TransitionColorType.None)
                {
                    colorError.Add(i);
                    continue;
                }

                if (componentColors[i].component is Selectable)
                {
                    componentColors[i].colorType = ColorType.None;
                }
                else
                {
                    componentColors[i].transitionColorType = TransitionColorType.None;
                }

                success = ApplyColors(ref componentColors[i]);
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

        public bool SetColorPreset(ColorPresetAsset newPreset)
        {
            if (colorPreset == newPreset) return false;
            colorPreset = newPreset;
            OnValidate();
            return true;
        }

        #region Helper Methods

        private bool ApplyColors(ref ComponentColor cc)
        {
            if (cc.component == null) return false;
            Color? color = colorPreset?.GetColorByType(cc.colorType);
            TransitionColor? transitionColor = colorPreset?.GetTransitionColorByType(cc.transitionColorType);

            if (cc.component is Graphic graphic)
            {
                if (color.HasValue)
                {
                    return ApplyColorToUIElement(graphic, color.Value);
                }
            }
            else if (cc.component is Selectable selectable)
            {
                if (transitionColor.HasValue)
                {
                    return ApplyTransitionColors(selectable, transitionColor.Value);
                }
            }

            return false;
        }

        private bool ApplyColorToUIElement<T>(T uiElement, Color color) where T : Graphic
        {
            if (uiElement == null) return false;
            if (uiElement.color == color) return false;

            uiElement.color = color;
            return true;
        }

        private bool ApplyTransitionColors(Selectable selectable, TransitionColor transitionColor)
        {
            if (selectable == null) return false;
            if (selectable.colors.normalColor == transitionColor.Normal &&
                    selectable.colors.highlightedColor == transitionColor.Highlighted &&
                    selectable.colors.pressedColor == transitionColor.Pressed &&
                    selectable.colors.selectedColor == transitionColor.Selected &&
                    selectable.colors.disabledColor == transitionColor.Disabled)
                return false;

            ColorBlock colorBlock = selectable.colors;
            colorBlock.normalColor = transitionColor.Normal;
            colorBlock.highlightedColor = transitionColor.Highlighted;
            colorBlock.pressedColor = transitionColor.Pressed;
            colorBlock.selectedColor = transitionColor.Selected;
            colorBlock.disabledColor = transitionColor.Disabled;
            selectable.colors = colorBlock;

            return true;
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
