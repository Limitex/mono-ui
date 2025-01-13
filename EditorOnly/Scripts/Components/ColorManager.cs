using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Limitex.MonoUI.Editor.Data;

#if UNITY_EDITOR
namespace Limitex.MonoUI.Editor.Components
{
    [Serializable]
    public struct ComponentColor
    {
        public Component component;
        public ColorType colorType;
        public TransitionColorType transitionColorType;
    }

    public class ColorManager : MonoBehaviour
    {
        [SerializeField] private ComponentColor[] componentColors;
        [SerializeField] private ColorPresetAsset colorPreset;

        public void Reset()
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:ColorPresetAsset",
                new[] { "Packages/dev.limitex.mono-ui/Editor/Assets/ColorPreset/" });

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                colorPreset = AssetDatabase.LoadAssetAtPath<ColorPresetAsset>(path);
            }
            else
            {
                Debug.LogError("Default ColorPresetAsset not found! Please assign one manually.");
            }
        }

        public void OnValidate()
        {
            if (colorPreset == null)
            {
                Debug.LogError("ColorPresetAsset is not assigned!");
                return;
            }

            if (componentColors == null) return;

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

                if (componentColors[i].component is
                    Button or Toggle or Slider or Scrollbar or TMP_Dropdown or TMP_InputField)
                {
                    componentColors[i].colorType = ColorType.None;
                }
                else
                {
                    componentColors[i].transitionColorType = TransitionColorType.None;
                }

                ApplyColors(ref componentColors[i]);
            }

            if (componentError.Count > 0)
            {
                Debug.LogWarning($"Component(s) at index(es) {string.Join(", ", componentError)} is/are not assigned. {GetLocationLog()}");
            }

            if (colorError.Count > 0)
            {
                Debug.LogWarning($"Color(s) at index(es) {string.Join(", ", colorError)} is/are not assigned. {GetLocationLog()}");
            }
        }

        public void ApplyColors(ref ComponentColor cc)
        {
            if (cc.component == null) return;
            Color? color = colorPreset?.GetColorByType(cc.colorType);
            TransitionColor? transitionColor = colorPreset?.GetTransitionColorByType(cc.transitionColorType);

            if (cc.component is Graphic graphic)
            {
                if (color.HasValue)
                {
                    ApplyColorToUIElement(graphic, color.Value);
                }
            }
            else if (cc.component is Selectable selectable)
            {
                if (transitionColor.HasValue)
                {
                    ApplyTransitionColors(selectable, transitionColor.Value);
                }
            }
        }

        #region Helper Methods

        public void SetColorPreset(ColorPresetAsset newPreset)
        {
            colorPreset = newPreset;
            OnValidate();
        }

        private void ApplyColorToUIElement<T>(T uiElement, Color color) where T : Graphic
        {
            if (uiElement != null)
            {
                uiElement.color = color;
            }
        }

        private void ApplyTransitionColors(Selectable selectable, TransitionColor transitionColor)
        {
            if (selectable != null)
            {
                ColorBlock colorBlock = selectable.colors;
                colorBlock.normalColor = transitionColor.Normal;
                colorBlock.highlightedColor = transitionColor.Highlighted;
                colorBlock.pressedColor = transitionColor.Pressed;
                colorBlock.selectedColor = transitionColor.Selected;
                colorBlock.disabledColor = transitionColor.Disabled;
                selectable.colors = colorBlock;
            }
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