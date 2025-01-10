using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Limitex.MonoUI.Theme
{
    [Serializable]
    public struct TransitionColor
    {
        public Color Normal;
        public Color Highlighted;
        public Color Pressed;
        public Color Selected;
        public Color Disabled;

        public TransitionColor(Color normal, Color highlighted, Color pressed, Color selected, Color disabled)
        {
            Normal = normal;
            Highlighted = highlighted;
            Pressed = pressed;
            Selected = selected;
            Disabled = disabled;
        }
    }

    public enum ColorType
    {
        None,
        Background,
        Foreground,
        Primary,
        PrimaryForeground,
        Secondary,
        SecondaryForeground,
        Accent,
        AccentForeground,
        Muted,
        MutedForeground,
        Destructive,
        DestructiveForeground,
        Chart1,
        Chart2,
        Chart3,
        Chart4,
        Chart5,
        Border,
        Ring,
        MutedHoverBackground,
    }

    public enum TransitionColorType
    {
        None,
        Primary,
        Ghost,
        Transparent,
    }

    [CreateAssetMenu(fileName = "NewColorPreset", menuName = "Mono UI/Color Preset")]
    public class ColorPresetAsset : ScriptableObject
    {
        [Header("Base Colors")]
        public Color Background = new Color(0.03137254901960784f, 0.03137254901960784f, 0.0392156862745098f);
        public Color Foreground = new Color(0.9764705882352941f, 0.9764705882352941f, 0.9764705882352941f);

        [Header("Primary")]
        public Color Primary = new Color(0.9764705882352941f, 0.9764705882352941f, 0.9764705882352941f);
        public Color PrimaryForeground = new Color(0.09019607843137255f, 0.09019607843137255f, 0.10588235294117647f);

        [Header("Secondary")]
        public Color Secondary = new Color(0.15294117647058825f, 0.15294117647058825f, 0.16470588235294117f);
        public Color SecondaryForeground = new Color(0.9764705882352941f, 0.9764705882352941f, 0.9764705882352941f);

        [Header("Accent")]
        public Color Accent = new Color(0.15294117647058825f, 0.15294117647058825f, 0.16470588235294117f);
        public Color AccentForeground = new Color(0.9764705882352941f, 0.9764705882352941f, 0.9764705882352941f);

        [Header("Muted")]
        public Color Muted = new Color(0.15294117647058825f, 0.15294117647058825f, 0.16470588235294117f);
        public Color MutedForeground = new Color(0.6313725490196078f, 0.6313725490196078f, 0.6627450980392157f);

        [Header("Destructive")]
        public Color Destructive = new Color(0.4980392156862745f, 0.11372549019607843f, 0.11372549019607843f);
        public Color DestructiveForeground = new Color(0.9764705882352941f, 0.9764705882352941f, 0.9764705882352941f);

        [Header("Chart")]
        public Color Chart1 = new Color(0.14901960784313725f, 0.3803921568627451f, 0.8470588235294118f);
        public Color Chart2 = new Color(0.17647058823529413f, 0.7176470588235294f, 0.5372549019607843f);
        public Color Chart3 = new Color(0.9098039215686274f, 0.5490196078431373f, 0.18823529411764706f);
        public Color Chart4 = new Color(0.6862745098039216f, 0.33725490196078434f, 0.8588235294117647f);
        public Color Chart5 = new Color(0.8862745098039215f, 0.21176470588235294f, 0.43529411764705883f);

        [Header("Others")]
        public Color Border = new Color(0.15294117647058825f, 0.15294117647058825f, 0.16470588235294117f);
        public Color Ring = new Color(0.8274509803921568f, 0.8274509803921568f, 0.8431372549019608f);
        public Color MutedHoverBackground = new Color(0.03137254901960784f, 0.03137254901960784f, 0.0392156862745098f, 0.99f);

        [Header("Transitions")]
        public TransitionColor PrimaryTransition = new TransitionColor(
            new Color(1.0f, 1.0f, 1.0f),
            new Color(0.8f, 0.8f, 0.8f),
            new Color(0.7f, 0.7f, 0.7f),
            new Color(1.0f, 1.0f, 1.0f),
            new Color(0.6313725490196078f, 0.6313725490196078f, 0.6627450980392157f));
        public TransitionColor GhostTransition = new TransitionColor(
            new Color(1.0f, 1.0f, 1.0f, 0.0f),
            new Color(1.0f, 1.0f, 1.0f, 0.8f),
            new Color(1.0f, 1.0f, 1.0f, 0.7f),
            new Color(1.0f, 1.0f, 1.0f, 0.0f),
            new Color(0.6313725490196078f, 0.6313725490196078f, 0.6627450980392157f));
        public TransitionColor TranspanentTransition = new TransitionColor(
            new Color(1.0f, 1.0f, 1.0f, 0.0f),
            new Color(1.0f, 1.0f, 1.0f, 0.8f),
            new Color(1.0f, 1.0f, 1.0f, 0.7f),
            new Color(1.0f, 1.0f, 1.0f, 0.8f),
            new Color(0.6313725490196078f, 0.6313725490196078f, 0.6627450980392157f));

        public Color? GetColorByType(ColorType colorType)
        {
            return colorType switch
            {
                ColorType.Background => Background,
                ColorType.Foreground => Foreground,
                ColorType.Primary => Primary,
                ColorType.PrimaryForeground => PrimaryForeground,
                ColorType.Secondary => Secondary,
                ColorType.SecondaryForeground => SecondaryForeground,
                ColorType.Accent => Accent,
                ColorType.AccentForeground => AccentForeground,
                ColorType.Muted => Muted,
                ColorType.MutedForeground => MutedForeground,
                ColorType.Destructive => Destructive,
                ColorType.DestructiveForeground => DestructiveForeground,
                ColorType.Chart1 => Chart1,
                ColorType.Chart2 => Chart2,
                ColorType.Chart3 => Chart3,
                ColorType.Chart4 => Chart4,
                ColorType.Chart5 => Chart5,
                ColorType.Border => Border,
                ColorType.Ring => Ring,
                ColorType.MutedHoverBackground => MutedHoverBackground,
                _ => null
            };
        }

        public TransitionColor? GetTransitionColorByType(TransitionColorType transitionColorType)
        {
            return transitionColorType switch
            {
                TransitionColorType.Primary => PrimaryTransition,
                TransitionColorType.Ghost => GhostTransition,
                TransitionColorType.Transparent => TranspanentTransition,
                _ => null
            };
        }
    }
}
