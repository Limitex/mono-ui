#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Limitex.MonoUI.Editor.ContextMenu
{
    public class GameObjectContextMenu
    {
        #region Constants
        private const string PREFAB_ROOT = "Packages/dev.limitex.mono-ui/Runtime/Assets/Prefab/";

        private const string MENU_ROOT = "GameObject/Mono UI/";
        private const string MENU_SAMPLE = MENU_ROOT + "Sample/";
        private const string MENU_LAYOUT = MENU_ROOT + "Layout/";
        private const string MENU_BUTTON = MENU_ROOT + "Button/";
        private const string MENU_TEXT = MENU_ROOT + "Text/";
        private const string MENU_TOGGLE = MENU_ROOT + "Toggle/";
        private const string MENU_CARD = MENU_ROOT + "Card/";
        private const string MENU_SWITCH = MENU_ROOT + "Switch/";
        private const string MENU_CHECKBOX = MENU_ROOT + "Checkbox/";
        private const string MENU_INPUT = MENU_ROOT + "Input Controls/";
        private const string MENU_NAVIGATION = MENU_ROOT + "Navigation/";
        private const string MENU_UTILITY = MENU_ROOT + "Utility/";
        private const string MENU_LOADING = MENU_ROOT + "Loading/";
        private const string MENU_PROVIDER = MENU_ROOT + "Provider/";

        private const int MENU_PRIORITY = 0;
        private const int SAMPLE_PRIORITY = MENU_PRIORITY;
        private const int LAYOUT_PRIORITY = MENU_PRIORITY + 100;
        private const int BUTTON_PRIORITY = MENU_PRIORITY + 200;
        private const int TEXT_PRIORITY = MENU_PRIORITY + 210;
        private const int TOGGLE_PRIORITY = MENU_PRIORITY + 220;
        private const int CARD_PRIORITY = MENU_PRIORITY + 230;
        private const int SWITCH_PRIORITY = MENU_PRIORITY + 240;
        private const int CHECKBOX_PRIORITY = MENU_PRIORITY + 250;
        private const int ICON_PRIORITY = MENU_PRIORITY + 260;
        private const int INPUT_PRIORITY = MENU_PRIORITY + 300;
        private const int NAVIGATION_PRIORITY = MENU_PRIORITY + 310;
        private const int UTILITY_PRIORITY = MENU_PRIORITY + 320;
        private const int LOADING_PRIORITY = MENU_PRIORITY + 330;
        private const int PROVIDER_PRIORITY = MENU_PRIORITY + 400;
        #endregion

        #region Utility Methods
        private static GameObject SpawnPrefab(string path, Transform parent = null, bool resetOffset = false)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"Cannot find prefab at {path}");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"Cannot instantiate prefab at {path}");
                return null;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.MoveGameObjectToScene(instance, activeScene);

            Vector3 originalScale = instance.transform.localScale;

            instance.transform.SetParent(parent ?? Selection.activeTransform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = originalScale;

            if (resetOffset)
            {
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.offsetMax = Vector2.zero;
                    rectTransform.offsetMin = Vector2.zero;
                }
            }

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeGameObject = instance;

            return instance;
        }

        private static GameObject SpawnPrefabWithCanvas(string path, bool forceUnderCanvas = false, bool resetOffset = false)
        {
            Canvas canvas = Selection.activeTransform?.GetComponentInParent<Canvas>();

            if (canvas != null)
                return SpawnPrefab(path, forceUnderCanvas ? canvas.transform : null, resetOffset);

            GameObject canvasObject = SpawnPrefab(PREFAB_ROOT + "Layout/Canvas.prefab");
            return SpawnPrefab(path, canvasObject.transform, resetOffset);
        }
        #endregion

        #region Sample Menu Items
        [MenuItem(MENU_SAMPLE + "UI Sample", false, SAMPLE_PRIORITY)]
        private static void CreateUISample() => SpawnPrefab(PREFAB_ROOT + "Sample/UI Sample.prefab");

        [MenuItem(MENU_SAMPLE + "Quiz Menu", false, SAMPLE_PRIORITY + 1)]
        private static void CreateQuizMenu() => SpawnPrefab(PREFAB_ROOT + "Sample/Quiz Menu.prefab");

        [MenuItem(MENU_SAMPLE + "Simple Log", false, SAMPLE_PRIORITY + 2)]
        private static void CreateSimpleLog() => SpawnPrefab(PREFAB_ROOT + "Sample/Simple Log.prefab");

        [MenuItem(MENU_SAMPLE + "World Log", false, SAMPLE_PRIORITY + 3)]
        private static void CreateWorldLog() => SpawnPrefab(PREFAB_ROOT + "Sample/World Log.prefab");

        [MenuItem(MENU_SAMPLE + "Platform Statistics", false, SAMPLE_PRIORITY + 4)]
        private static void CreatePlatformStatistics() => SpawnPrefab(PREFAB_ROOT + "Sample/Platform Statistics.prefab");

        [MenuItem(MENU_SAMPLE + "Simpe Mirror", false, SAMPLE_PRIORITY + 5)]
        private static void CreateSimpleMirror() => SpawnPrefab(PREFAB_ROOT + "Sample/Simple Mirror.prefab");

        #endregion

        #region Layout Menu Items
        [MenuItem(MENU_LAYOUT + "Canvas", false, LAYOUT_PRIORITY)]
        private static void CreateCanvas() => SpawnPrefab(PREFAB_ROOT + "Layout/Canvas.prefab");

        [MenuItem(MENU_LAYOUT + "Canvas Outline", false, LAYOUT_PRIORITY + 1)]
        private static void CreateCanvasOutline() => SpawnPrefab(PREFAB_ROOT + "Layout/Canvas Outline.prefab");

        [MenuItem(MENU_LAYOUT + "Sidebar", false, LAYOUT_PRIORITY + 2)]
        private static void CreateSidebar() => SpawnPrefab(PREFAB_ROOT + "Layout/Sidebar.prefab");
        #endregion

        #region Button Menu Items
        [MenuItem(MENU_BUTTON + "Primary", false, BUTTON_PRIORITY)]
        private static void CreateButtonPrimary() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button.prefab");

        [MenuItem(MENU_BUTTON + "Secondary", false, BUTTON_PRIORITY + 1)]
        private static void CreateButtonSecondary() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Secondary.prefab");

        [MenuItem(MENU_BUTTON + "Destructive", false, BUTTON_PRIORITY + 2)]
        private static void CreateButtonDestructive() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Destructive.prefab");

        [MenuItem(MENU_BUTTON + "Outline", false, BUTTON_PRIORITY + 3)]
        private static void CreateButtonOutline() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Outline.prefab");

        [MenuItem(MENU_BUTTON + "Ghost", false, BUTTON_PRIORITY + 4)]
        private static void CreateButtonGhost() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Ghost.prefab");
        #endregion

        #region Text Menu Items
        [MenuItem(MENU_TEXT + "Text (TMP)", false, TEXT_PRIORITY)]
        private static void CreateText() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text (TMP).prefab");

        [MenuItem(MENU_TEXT + "Text h1 (TMP)", false, TEXT_PRIORITY + 1)]
        private static void CreateTextH1() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h1 (TMP).prefab");

        [MenuItem(MENU_TEXT + "Text h2 (TMP)", false, TEXT_PRIORITY + 2)]
        private static void CreateTextH2() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h2 (TMP).prefab");

        [MenuItem(MENU_TEXT + "Text h3 (TMP)", false, TEXT_PRIORITY + 3)]
        private static void CreateTextH3() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h3 (TMP).prefab");

        [MenuItem(MENU_TEXT + "Text h4 (TMP)", false, TEXT_PRIORITY + 4)]
        private static void CreateTextH4() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h4 (TMP).prefab");

        [MenuItem(MENU_TEXT + "Text h5 (TMP)", false, TEXT_PRIORITY + 5)]
        private static void CreateTextH5() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h5 (TMP).prefab");

        [MenuItem(MENU_TEXT + "Text h6 (TMP)", false, TEXT_PRIORITY + 6)]
        private static void CreateTextH6() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h6 (TMP).prefab");

        [MenuItem(MENU_TEXT + "Text (Legacy)", false, TEXT_PRIORITY + 7)]
        private static void CreateLegacyText() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text (Legacy).prefab");
        #endregion

        #region Toggle Menu Items
        [MenuItem(MENU_TOGGLE + "Toggle", false, TOGGLE_PRIORITY)]
        private static void CreateToggle() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Toggle.prefab");

        [MenuItem(MENU_TOGGLE + "Toggle Outline", false, TOGGLE_PRIORITY + 1)]
        private static void CreateToggleOutline() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Toggle Outline.prefab");

        [MenuItem(MENU_TOGGLE + "Toggle Group", false, TOGGLE_PRIORITY + 2)]
        private static void CreateToggleGroup() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Toggle Group.prefab");

        [MenuItem(MENU_TOGGLE + "Radio Toggle", false, TOGGLE_PRIORITY + 3)]
        private static void CreateRadioToggle() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Radio Toggle.prefab");

        [MenuItem(MENU_TOGGLE + "Radio Toggle Group", false, TOGGLE_PRIORITY + 4)]
        private static void CreateRadioToggleGroup() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Radio Toggle Group.prefab");
        #endregion

        #region Card Menu Items
        [MenuItem(MENU_CARD + "Card", false, CARD_PRIORITY)]
        private static void CreateCard() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Card/Card.prefab");

        [MenuItem(MENU_CARD + "Card Outline", false, CARD_PRIORITY + 1)]
        private static void CreateCardOutline() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Card/Card Outline.prefab");
        #endregion

        #region Switch Menu Items
        [MenuItem(MENU_SWITCH + "Switch", false, SWITCH_PRIORITY)]
        private static void CreateSwitch() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Switch/Switch.prefab");

        [MenuItem(MENU_SWITCH + "Switch with Text (Left)", false, SWITCH_PRIORITY + 1)]
        private static void CreateSwitchWithTextLeft() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Switch/Switch with Text (Left).prefab");

        [MenuItem(MENU_SWITCH + "Switch with Text (Right)", false, SWITCH_PRIORITY + 2)]
        private static void CreateSwitchWithTextRight() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Switch/Switch with Text (Right).prefab");
        #endregion

        #region Checkbox Menu Items
        [MenuItem(MENU_CHECKBOX + "Checkbox", false, CHECKBOX_PRIORITY)]
        private static void CreateCheckbox() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Checkbox.prefab");
        #endregion

        #region Icon Menu Items
        [MenuItem(MENU_ROOT + "Lucide Icon", false, ICON_PRIORITY)]
        private static void CreateLucideIcon() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Lucide Icon.prefab");
        #endregion

        #region Input Control Menu Items
        [MenuItem(MENU_INPUT + "Dropdown", false, INPUT_PRIORITY)]
        private static void CreateDropdown() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Dropdown.prefab");

        [MenuItem(MENU_INPUT + "Input Field", false, INPUT_PRIORITY + 1)]
        private static void CreateInputField() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/InputField.prefab");

        [MenuItem(MENU_INPUT + "Slider", false, INPUT_PRIORITY + 2)]
        private static void CreateSlider() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Slider.prefab");
        #endregion

        #region Navigation Menu Items
        [MenuItem(MENU_NAVIGATION + "Scroll View", false, NAVIGATION_PRIORITY)]
        private static void CreateScrollView() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Scroll View.prefab");

        [MenuItem(MENU_NAVIGATION + "Scrollbar", false, NAVIGATION_PRIORITY + 1)]
        private static void CreateScrollbar() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Scrollbar.prefab");

        [MenuItem(MENU_NAVIGATION + "Carousel", false, NAVIGATION_PRIORITY + 2)]
        private static void CreateCarousel() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Carousel.prefab");

        #endregion

        #region Utility Menu Items
        [MenuItem(MENU_UTILITY + "Progress", false, UTILITY_PRIORITY)]
        private static void CreateProgress() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Progress.prefab");

        [MenuItem(MENU_UTILITY + "Span", false, UTILITY_PRIORITY + 1)]
        private static void CreateSpan() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Span.prefab");

        [MenuItem(MENU_UTILITY + "Separator", false, UTILITY_PRIORITY + 2)]
        private static void CreateSeparator() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Separator.prefab");

        [MenuItem(MENU_UTILITY + "Table", false, UTILITY_PRIORITY + 3)]
        private static void CreateTable() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Table.prefab");
        #endregion

        #region Loading Menu Items
        [MenuItem(MENU_LOADING + "Loading Bar", false, LOADING_PRIORITY)]
        private static void CreateLoadingBar() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Loading/Loading Bar.prefab");

        [MenuItem(MENU_LOADING + "Loading Circle", false, LOADING_PRIORITY + 1)]
        private static void CreateLoadingCircle() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Loading/Loading Circle.prefab");
        #endregion

        #region Provider Menu Items
        [MenuItem(MENU_PROVIDER + "Dialog", false, PROVIDER_PRIORITY)]
        private static void CreateProvider() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Providers/Dialog Provider.prefab", true, true);

        [MenuItem(MENU_PROVIDER + "Dialog (bool)", false, PROVIDER_PRIORITY + 1)]
        private static void CreateProviderBool() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Providers/Dialog Provider (bool).prefab", true, true);

        [MenuItem(MENU_PROVIDER + "Toast", false, PROVIDER_PRIORITY + 2)]
        private static void CreateToast() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Providers/Toast Provider.prefab", true, true);
        #endregion

        #region Credit
        [MenuItem(MENU_ROOT + "Credit", false, 999)]
        private static void CreateCredit() => SpawnPrefab(PREFAB_ROOT + "Credit.prefab");
        #endregion
    }
}

#endif
