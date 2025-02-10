#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Linq;
using Limitex.MonoUI.Editor.Data;
using Newtonsoft.Json;
using System;

namespace Limitex.MonoUI.Editor.Inspector
{
    [CustomEditor(typeof(ColorPresetAsset))]
    public class ColorPresetEditor : UnityEditor.Editor
    {
        private string textareaInput = "";
        private bool showInput = false;
        private Vector2 _scrollPosition = Vector2.zero;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            showInput = EditorGUILayout.Foldout(showInput, "Import Presets");

            if (showInput)
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200f));
                textareaInput = EditorGUILayout.TextArea(textareaInput, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Apply"))
                {
                    ApplyToPreset();
                }
            }
            
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Color preset JSON to Clipboard"))
            {
                var jsonString = GenerateJson();
                EditorGUIUtility.systemCopyBuffer = jsonString;
                Debug.Log("Color preset JSON has been copied to clipboard!");
            }
        }

        #region Parsing Methods

        private string GenerateJson()
        {
            var target = (ColorPresetAsset)serializedObject.targetObject;
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

            return JsonConvert.SerializeObject(preset, Formatting.Indented);
        }

        private void ApplyToPreset()
        {
            try
            {
                var target = (ColorPresetAsset)serializedObject.targetObject;
                var preset = JsonConvert.DeserializeObject<dynamic>(textareaInput);

                Undo.RecordObject(target, "Apply Color Preset");

                // Base Colors
                target.Background = ParseHexColor(preset.baseColors.background.ToString());
                target.Foreground = ParseHexColor(preset.baseColors.foreground.ToString());

                // Primary
                target.Primary = ParseHexColor(preset.primary.@base.ToString());
                target.PrimaryForeground = ParseHexColor(preset.primary.foreground.ToString());

                // Secondary
                target.Secondary = ParseHexColor(preset.secondary.@base.ToString());
                target.SecondaryForeground = ParseHexColor(preset.secondary.foreground.ToString());

                // Accent
                target.Accent = ParseHexColor(preset.accent.@base.ToString());
                target.AccentForeground = ParseHexColor(preset.accent.foreground.ToString());

                // Muted
                target.Muted = ParseHexColor(preset.muted.@base.ToString());
                target.MutedForeground = ParseHexColor(preset.muted.foreground.ToString());

                // Destructive
                target.Destructive = ParseHexColor(preset.destructive.@base.ToString());
                target.DestructiveForeground = ParseHexColor(preset.destructive.foreground.ToString());

                // Chart
                target.Chart1 = ParseHexColor(preset.chart.color1.ToString());
                target.Chart2 = ParseHexColor(preset.chart.color2.ToString());
                target.Chart3 = ParseHexColor(preset.chart.color3.ToString());
                target.Chart4 = ParseHexColor(preset.chart.color4.ToString());
                target.Chart5 = ParseHexColor(preset.chart.color5.ToString());

                // Others
                target.Border = ParseHexColor(preset.others.border.ToString());
                target.Ring = ParseHexColor(preset.others.ring.ToString());
                target.MutedHoverBackground = ParseHexColor(preset.others.mutedHoverBackground.ToString());

                // Transitions
                target.PrimaryTransition = new TransitionColor(
                    ParseHexColor(preset.transitions.primary.normal.ToString()),
                    ParseHexColor(preset.transitions.primary.highlighted.ToString()),
                    ParseHexColor(preset.transitions.primary.pressed.ToString()),
                    ParseHexColor(preset.transitions.primary.selected.ToString()),
                    ParseHexColor(preset.transitions.primary.disabled.ToString())
                );

                target.GhostTransition = new TransitionColor(
                    ParseHexColor(preset.transitions.ghost.normal.ToString()),
                    ParseHexColor(preset.transitions.ghost.highlighted.ToString()),
                    ParseHexColor(preset.transitions.ghost.pressed.ToString()),
                    ParseHexColor(preset.transitions.ghost.selected.ToString()),
                    ParseHexColor(preset.transitions.ghost.disabled.ToString())
                );

                target.TranspanentTransition = new TransitionColor(
                    ParseHexColor(preset.transitions.transparent.normal.ToString()),
                    ParseHexColor(preset.transitions.transparent.highlighted.ToString()),
                    ParseHexColor(preset.transitions.transparent.pressed.ToString()),
                    ParseHexColor(preset.transitions.transparent.selected.ToString()),
                    ParseHexColor(preset.transitions.transparent.disabled.ToString())
                );

                serializedObject.Update();

                Debug.Log("Color preset successfully applied!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to apply color preset: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private string ColorToHex(Color color) => $"#{ColorUtility.ToHtmlStringRGB(color)}";

        private string ColorToHexWithAlpha(Color color) => $"#{ColorUtility.ToHtmlStringRGBA(color)}";

        private Color ParseHexColor(string hexColor)
        {
            if (hexColor.StartsWith("#")) hexColor = hexColor.Substring(1);
            if (ColorUtility.TryParseHtmlString("#" + hexColor, out Color color)) return color;
            Debug.LogWarning($"Failed to parse color: {hexColor}");
            return Color.white;
        }

        #endregion
    }
}
#endif
