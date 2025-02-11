#if UNITY_EDITOR

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEditor;
using UnityEngine;

namespace Limitex.MonoUI.Editor.Data
{
    #region Type Definitions

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

    // Random IDs are assigned to ensure stability against reordering and for serialization.
    public enum ColorType
    {
        None = 412340151,
        Background = 121473958,
        Foreground = 790920719,
        Primary = 598842521,
        PrimaryForeground = 711723875,
        Secondary = 471083450,
        SecondaryForeground = 533222481,
        Accent = 342849487,
        AccentForeground = 364665978,
        Muted = 432419634,
        MutedForeground = 952807518,
        Destructive = 578251783,
        DestructiveForeground = 173638956,
        Chart1 = 406805723,
        Chart2 = 464079137,
        Chart3 = 534066984,
        Chart4 = 288490277,
        Chart5 = 876561572,
        Border = 694730041,
        Ring = 179126290,
        MutedHoverBackground = 974697568,
    }

    // Random IDs are assigned to ensure stability against reordering and for serialization.
    public enum TransitionColorType
    {
        None = 897758581,
        Primary = 607538827,
        Ghost = 194200482,
        Transparent = 353005088,
    }

    #endregion

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

        #region Purse Methods

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

        #endregion

        #region Json Serialization

        public static string ConvertToJson(ColorPresetAsset target, Formatting formatting)
        {
            var preset = new
            {
                baseColors = new
                {
                    background = ColorToHex(target.Background),
                    foreground = ColorToHex(target.Foreground)
                },
                primary = new
                {
                    @base = ColorToHex(target.Primary),
                    foreground = ColorToHex(target.PrimaryForeground)
                },
                secondary = new
                {
                    @base = ColorToHex(target.Secondary),
                    foreground = ColorToHex(target.SecondaryForeground)
                },
                accent = new
                {
                    @base = ColorToHex(target.Accent),
                    foreground = ColorToHex(target.AccentForeground)
                },
                muted = new
                {
                    @base = ColorToHex(target.Muted),
                    foreground = ColorToHex(target.MutedForeground)
                },
                destructive = new
                {
                    @base = ColorToHex(target.Destructive),
                    foreground = ColorToHex(target.DestructiveForeground)
                },
                chart = new
                {
                    color1 = ColorToHex(target.Chart1),
                    color2 = ColorToHex(target.Chart2),
                    color3 = ColorToHex(target.Chart3),
                    color4 = ColorToHex(target.Chart4),
                    color5 = ColorToHex(target.Chart5)
                },
                others = new
                {
                    border = ColorToHex(target.Border),
                    ring = ColorToHex(target.Ring),
                    mutedHoverBackground = ColorToHexWithAlpha(target.MutedHoverBackground)
                },
                transitions = new
                {
                    primary = new
                    {
                        normal = ColorToHex(target.PrimaryTransition.Normal),
                        highlighted = ColorToHex(target.PrimaryTransition.Highlighted),
                        pressed = ColorToHex(target.PrimaryTransition.Pressed),
                        selected = ColorToHex(target.PrimaryTransition.Selected),
                        disabled = ColorToHex(target.PrimaryTransition.Disabled)
                    },
                    ghost = new
                    {
                        normal = ColorToHexWithAlpha(target.GhostTransition.Normal),
                        highlighted = ColorToHexWithAlpha(target.GhostTransition.Highlighted),
                        pressed = ColorToHexWithAlpha(target.GhostTransition.Pressed),
                        selected = ColorToHexWithAlpha(target.GhostTransition.Selected),
                        disabled = ColorToHex(target.GhostTransition.Disabled)
                    },
                    transparent = new
                    {
                        normal = ColorToHexWithAlpha(target.TranspanentTransition.Normal),
                        highlighted = ColorToHexWithAlpha(target.TranspanentTransition.Highlighted),
                        pressed = ColorToHexWithAlpha(target.TranspanentTransition.Pressed),
                        selected = ColorToHexWithAlpha(target.TranspanentTransition.Selected),
                        disabled = ColorToHex(target.TranspanentTransition.Disabled)
                    }
                }
            };

            return JsonConvert.SerializeObject(preset, formatting);
        }

        public static void ParseJson(ref ColorPresetAsset target, string json)
        {
            try
            {
                JObject preset = JsonConvert.DeserializeObject<JObject>(json);

                target.Background = ParseHexColor(preset["baseColors"]["background"].ToString());
                target.Foreground = ParseHexColor(preset["baseColors"]["foreground"].ToString());

                target.Primary = ParseHexColor(preset["primary"]["base"].ToString());
                target.PrimaryForeground = ParseHexColor(preset["primary"]["foreground"].ToString());

                target.Secondary = ParseHexColor(preset["secondary"]["base"].ToString());
                target.SecondaryForeground = ParseHexColor(preset["secondary"]["foreground"].ToString());

                target.Accent = ParseHexColor(preset["accent"]["base"].ToString());
                target.AccentForeground = ParseHexColor(preset["accent"]["foreground"].ToString());

                target.Muted = ParseHexColor(preset["muted"]["base"].ToString());
                target.MutedForeground = ParseHexColor(preset["muted"]["foreground"].ToString());

                target.Destructive = ParseHexColor(preset["destructive"]["base"].ToString());
                target.DestructiveForeground = ParseHexColor(preset["destructive"]["foreground"].ToString());

                target.Chart1 = ParseHexColor(preset["chart"]["color1"].ToString());
                target.Chart2 = ParseHexColor(preset["chart"]["color2"].ToString());
                target.Chart3 = ParseHexColor(preset["chart"]["color3"].ToString());
                target.Chart4 = ParseHexColor(preset["chart"]["color4"].ToString());
                target.Chart5 = ParseHexColor(preset["chart"]["color5"].ToString());

                target.Border = ParseHexColor(preset["others"]["border"].ToString());
                target.Ring = ParseHexColor(preset["others"]["ring"].ToString());
                target.MutedHoverBackground = ParseHexColor(preset["others"]["mutedHoverBackground"].ToString());

                target.PrimaryTransition = new TransitionColor(
                    ParseHexColor(preset["transitions"]["primary"]["normal"].ToString()),
                    ParseHexColor(preset["transitions"]["primary"]["highlighted"].ToString()),
                    ParseHexColor(preset["transitions"]["primary"]["pressed"].ToString()),
                    ParseHexColor(preset["transitions"]["primary"]["selected"].ToString()),
                    ParseHexColor(preset["transitions"]["primary"]["disabled"].ToString())
                );

                target.GhostTransition = new TransitionColor(
                    ParseHexColor(preset["transitions"]["ghost"]["normal"].ToString()),
                    ParseHexColor(preset["transitions"]["ghost"]["highlighted"].ToString()),
                    ParseHexColor(preset["transitions"]["ghost"]["pressed"].ToString()),
                    ParseHexColor(preset["transitions"]["ghost"]["selected"].ToString()),
                    ParseHexColor(preset["transitions"]["ghost"]["disabled"].ToString())
                );

                target.TranspanentTransition = new TransitionColor(
                    ParseHexColor(preset["transitions"]["transparent"]["normal"].ToString()),
                    ParseHexColor(preset["transitions"]["transparent"]["highlighted"].ToString()),
                    ParseHexColor(preset["transitions"]["transparent"]["pressed"].ToString()),
                    ParseHexColor(preset["transitions"]["transparent"]["selected"].ToString()),
                    ParseHexColor(preset["transitions"]["transparent"]["disabled"].ToString())
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse JSON: {ex.Message}");
            }
        }

        #region Helper Methods

        private static string ColorToHex(Color color) => $"#{ColorUtility.ToHtmlStringRGB(color)}";

        private static string ColorToHexWithAlpha(Color color) => $"#{ColorUtility.ToHtmlStringRGBA(color)}";

        private static Color ParseHexColor(string hexColor)
        {
            if (hexColor.StartsWith("#")) hexColor = hexColor.Substring(1);
            if (ColorUtility.TryParseHtmlString("#" + hexColor, out Color color)) return color;
            Debug.LogWarning($"Failed to parse color: {hexColor}");
            return Color.white;
        }

        #endregion

        #endregion
    }
}

#endif
