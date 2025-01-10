using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR

namespace Limitex.MonoUI.Theme
{
    [Serializable]
    public struct ComponentColor
    {
        public Component component;
        public ColorType colorType;
        public TransitionColorType transitionColorType;
    }

    public enum ColorType
    {
        None,
        Background,
        Foreground,
        Primary,
        Secondary,
        PrimaryForeground,
        SecondaryForeground,
        AccentForeground,
        MutedForeground,
        DestructiveForeground,
        Accent,
        Muted,
        Destructive,
        Border,
        Ring,
        Chart1,
        Chart2,
        Chart3,
        Chart4,
        Chart5,
        MutedHoverBackground,
    }

    public enum TransitionColorType
    {
        None,
        Primary,
        Ghost,
        Transparent,
    }

    public class ColorManager : MonoBehaviour
    {
        [SerializeField] private ComponentColor[] componentColors;
        [SerializeField] private ColorPresetAsset colorPreset;

        public void Reset()
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:ColorPresetAsset",
                new[] { "Packages/dev.limitex.mono-ui/Runtime/Assets/Color" });

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

            for (int i = 0; i < componentColors.Length; i++)
            {
                if (componentColors[i].component == null)
                {
                    Debug.LogError($"ColorManager component is not assigned to {this.gameObject.name}");
                    continue;
                }

                if (componentColors[i].colorType == ColorType.None &&
                    componentColors[i].transitionColorType == TransitionColorType.None)
                {
                    Debug.LogError($"ColorManager colorType or transitionColorType is not assigned to {this.gameObject.name}");
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
        }

        public void ApplyColors(ref ComponentColor cc)
        {
            if (cc.component == null) return;
            Color? color = GetColorFromPreset(cc.colorType);
            TransitionColor? transitionColor = GetTransitionColorFromPreset(cc.transitionColorType);

            if (cc.component is Image)
            {
                if (color == null) return;
                Image image = cc.component as Image;
                image.color = (Color)color;
            }
            else if (cc.component is TMP_Text)
            {
                if (color == null) return;
                TMP_Text text = cc.component as TMP_Text;
                text.color = (Color)color;
            }
            else if (cc.component is RawImage)
            {
                if (color == null) return;
                RawImage rawImage = cc.component as RawImage;
                rawImage.color = (Color)color;
            }
            else if (cc.component is Toggle)
            {
                if (transitionColor == null) return;
                Toggle toggle = cc.component as Toggle;
                TransitionColor tc = (TransitionColor)transitionColor;
                ColorBlock colorBlock = toggle.colors;
                colorBlock.normalColor = tc.Normal;
                colorBlock.highlightedColor = tc.Highlighted;
                colorBlock.pressedColor = tc.Pressed;
                colorBlock.selectedColor = tc.Selected;
                colorBlock.disabledColor = tc.Disabled;
                toggle.colors = colorBlock;
            }
            else if (cc.component is Slider)
            {
                if (transitionColor == null) return;
                Slider slider = cc.component as Slider;
                TransitionColor tc = (TransitionColor)transitionColor;
                ColorBlock colorBlock = slider.colors;
                colorBlock.normalColor = tc.Normal;
                colorBlock.highlightedColor = tc.Highlighted;
                colorBlock.pressedColor = tc.Pressed;
                colorBlock.selectedColor = tc.Selected;
                colorBlock.disabledColor = tc.Disabled;
                slider.colors = colorBlock;
            }
            else if (cc.component is Scrollbar)
            {
                if (transitionColor == null) return;
                Scrollbar scrollbar = cc.component as Scrollbar;
                TransitionColor tc = (TransitionColor)transitionColor;
                ColorBlock colorBlock = scrollbar.colors;
                colorBlock.normalColor = tc.Normal;
                colorBlock.highlightedColor = tc.Highlighted;
                colorBlock.pressedColor = tc.Pressed;
                colorBlock.selectedColor = tc.Selected;
                colorBlock.disabledColor = tc.Disabled;
                scrollbar.colors = colorBlock;
            }
            else if (cc.component is Button)
            {
                if (transitionColor == null) return;
                Button button = cc.component as Button;
                TransitionColor tc = (TransitionColor)transitionColor;
                ColorBlock colorBlock = button.colors;
                colorBlock.normalColor = tc.Normal;
                colorBlock.highlightedColor = tc.Highlighted;
                colorBlock.pressedColor = tc.Pressed;
                colorBlock.selectedColor = tc.Selected;
                colorBlock.disabledColor = tc.Disabled;
                button.colors = colorBlock;
            }
            else if (cc.component is TMP_Dropdown)
            {
                if (transitionColor == null) return;
                TMP_Dropdown dropdown = cc.component as TMP_Dropdown;
                TransitionColor tc = (TransitionColor)transitionColor;
                ColorBlock colorBlock = dropdown.colors;
                colorBlock.normalColor = tc.Normal;
                colorBlock.highlightedColor = tc.Highlighted;
                colorBlock.pressedColor = tc.Pressed;
                colorBlock.selectedColor = tc.Selected;
                colorBlock.disabledColor = tc.Disabled;
                dropdown.colors = colorBlock;
            }
            else if (cc.component is TMP_InputField)
            {
                if (transitionColor == null) return;
                TMP_InputField inputField = cc.component as TMP_InputField;
                TransitionColor tc = (TransitionColor)transitionColor;
                ColorBlock colorBlock = inputField.colors;
                colorBlock.normalColor = tc.Normal;
                colorBlock.highlightedColor = tc.Highlighted;
                colorBlock.pressedColor = tc.Pressed;
                colorBlock.selectedColor = tc.Selected;
                colorBlock.disabledColor = tc.Disabled;
                inputField.colors = colorBlock;
            }
        }

        public void SetColorPreset(ColorPresetAsset newPreset)
        {
            colorPreset = newPreset;
            OnValidate();
        }

        #region Helper Methods

        private Color? GetColorFromPreset(ColorType colorType)
        {
            if (colorPreset == null)
            {
                Debug.LogWarning("ColorPresetAsset is not assigned!");
                return null;
            }

            switch (colorType)
            {
                case ColorType.Background: return colorPreset.Background;
                case ColorType.Foreground: return colorPreset.Foreground;
                case ColorType.Primary: return colorPreset.Primary;
                case ColorType.Secondary: return colorPreset.Secondary;
                case ColorType.PrimaryForeground: return colorPreset.PrimaryForeground;
                case ColorType.SecondaryForeground: return colorPreset.SecondaryForeground;
                case ColorType.AccentForeground: return colorPreset.AccentForeground;
                case ColorType.MutedForeground: return colorPreset.MutedForeground;
                case ColorType.DestructiveForeground: return colorPreset.DestructiveForeground;
                case ColorType.Accent: return colorPreset.Accent;
                case ColorType.Muted: return colorPreset.Muted;
                case ColorType.Destructive: return colorPreset.Destructive;
                case ColorType.Border: return colorPreset.Border;
                case ColorType.Ring: return colorPreset.Ring;
                case ColorType.Chart1: return colorPreset.Chart1;
                case ColorType.Chart2: return colorPreset.Chart2;
                case ColorType.Chart3: return colorPreset.Chart3;
                case ColorType.Chart4: return colorPreset.Chart4;
                case ColorType.Chart5: return colorPreset.Chart5;
                case ColorType.MutedHoverBackground: return colorPreset.MutedHoverBackground;
                default:
                    return null;
            }
        }

        private TransitionColor? GetTransitionColorFromPreset(TransitionColorType transitionColorType)
        {
            if (colorPreset == null)
            {
                Debug.LogWarning("ColorPresetAsset is not assigned!");
                return null;
            }

            switch (transitionColorType)
            {
                case TransitionColorType.Primary: return colorPreset.PrimaryTransition;
                case TransitionColorType.Ghost: return colorPreset.GhostTransition;
                case TransitionColorType.Transparent: return colorPreset.TranspanentTransition;
                default:
                    return null;
            }
        }

        #endregion
    }
}

#endif