using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace Limitex.MonoUI.Editor.Util
{
    public class ContextMenu
    {
        private const int MENU_PRIORITY = 0;
        private const string MENU_ROOT = "GameObject/Mono UI/";
        private const string PREFAB_ROOT = "Packages/dev.limitex.mono-ui/Runtime/Assets/Prefab/";

        private static GameObject SpawnPrefab(string path, Transform parent = null)
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

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeGameObject = instance;

            return instance;
        }

        private static GameObject SpawnPrefabWithCanvas(string path, bool forceUnderCanvas = false)
        {
            Canvas canvas = Selection.activeTransform?.GetComponentInParent<Canvas>();

            if (canvas != null)
                return SpawnPrefab(path, forceUnderCanvas ? canvas.transform : null);

            GameObject canvasObject = SpawnPrefab(PREFAB_ROOT + "Layout/Canvas.prefab");
            return SpawnPrefab(path, canvasObject.transform);
        }


        [MenuItem(MENU_ROOT + "Sample/UI Sample", false, MENU_PRIORITY)]
        private static void CreateUISample() => SpawnPrefab(PREFAB_ROOT + "Sample/UI Sample.prefab");

        [MenuItem(MENU_ROOT + "Sample/Quiz Menu", false, MENU_PRIORITY + 1)]
        private static void CreateQuizMenu() => SpawnPrefab(PREFAB_ROOT + "Sample/Quiz Menu.prefab");

        [MenuItem(MENU_ROOT + "Sample/World Log", false, MENU_PRIORITY + 2)]
        private static void CreateWorldLog() => SpawnPrefab(PREFAB_ROOT + "Sample/World Log.prefab");


        [MenuItem(MENU_ROOT + "Canvas", false, MENU_PRIORITY + 100)]
        private static void CreateCanvas() => SpawnPrefab(PREFAB_ROOT + "Layout/Canvas.prefab");

        [MenuItem(MENU_ROOT + "Canvas Outline", false, MENU_PRIORITY + 101)]
        private static void CreateCanvasOutline() => SpawnPrefab(PREFAB_ROOT + "Layout/Canvas Outline.prefab");

        [MenuItem(MENU_ROOT + "Sidebar", false, MENU_PRIORITY + 102)]
        private static void CreateSidebar() => SpawnPrefab(PREFAB_ROOT + "Layout/Sidebar.prefab");


        [MenuItem(MENU_ROOT + "Button/Primary", false, MENU_PRIORITY + 200)]
        private static void CreateButtonPrimary() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button.prefab");

        [MenuItem(MENU_ROOT + "Button/Secondary", false, MENU_PRIORITY + 201)]
        private static void CreateButtonSecondary() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Secondary.prefab");

        [MenuItem(MENU_ROOT + "Button/Destructive", false, MENU_PRIORITY + 202)]
        private static void CreateButtonDestructive() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Destructive.prefab");

        [MenuItem(MENU_ROOT + "Button/Outline", false, MENU_PRIORITY + 203)]
        private static void CreateButtonOutline() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Outline.prefab");

        [MenuItem(MENU_ROOT + "Button/Ghost", false, MENU_PRIORITY + 204)]
        private static void CreateButtonGhost() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Button/Button Ghost.prefab");


        [MenuItem(MENU_ROOT + "Text/Text (TMP)", false, MENU_PRIORITY + 210)]
        private static void CreateText() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text (TMP).prefab");

        [MenuItem(MENU_ROOT + "Text/Text h1 (TMP)", false, MENU_PRIORITY + 211)]
        private static void CreateTextH1() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h1 (TMP).prefab");

        [MenuItem(MENU_ROOT + "Text/Text h2 (TMP)", false, MENU_PRIORITY + 212)]
        private static void CreateTextH2() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h2 (TMP).prefab");

        [MenuItem(MENU_ROOT + "Text/Text h3 (TMP)", false, MENU_PRIORITY + 213)]
        private static void CreateTextH3() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h3 (TMP).prefab");

        [MenuItem(MENU_ROOT + "Text/Text h4 (TMP)", false, MENU_PRIORITY + 214)]
        private static void CreateTextH4() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h4 (TMP).prefab");

        [MenuItem(MENU_ROOT + "Text/Text h5 (TMP)", false, MENU_PRIORITY + 215)]
        private static void CreateTextH5() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h5 (TMP).prefab");

        [MenuItem(MENU_ROOT + "Text/Text h6 (TMP)", false, MENU_PRIORITY + 216)]
        private static void CreateTextH6() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Text/Text h6 (TMP).prefab");


        [MenuItem(MENU_ROOT + "Toggle/Toggle", false, MENU_PRIORITY + 220)]
        private static void CreateToggle() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Toggle.prefab");

        [MenuItem(MENU_ROOT + "Toggle/Toggle Outline", false, MENU_PRIORITY + 221)]
        private static void CreateToggleOutline() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Toggle Outline.prefab");

        [MenuItem(MENU_ROOT + "Toggle/Toggle Group", false, MENU_PRIORITY + 222)]
        private static void CreateToggleGroup() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Toggle Group.prefab");

        [MenuItem(MENU_ROOT + "Toggle/Radio Toggle", false, MENU_PRIORITY + 223)]
        private static void CreateRadioToggle() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Radio Toggle.prefab");

        [MenuItem(MENU_ROOT + "Toggle/Radio Toggle Group", false, MENU_PRIORITY + 224)]
        private static void CreateRadioToggleGroup() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Toggle/Radio Toggle Group.prefab");


        [MenuItem(MENU_ROOT + "Card/Card", false, MENU_PRIORITY + 230)]
        private static void CreateCard() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Card/Card.prefab");

        [MenuItem(MENU_ROOT + "Card/Card Outline", false, MENU_PRIORITY + 231)]
        private static void CreateCardOutline() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Card/Card Outline.prefab");

        
        [MenuItem(MENU_ROOT + "Switch/Switch", false, MENU_PRIORITY + 240)]
        private static void CreateSwitch() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Switch/Switch.prefab");

        [MenuItem(MENU_ROOT + "Switch/Switch with Text (Left)", false, MENU_PRIORITY + 241)]
        private static void CreateSwitchWithTextLeft() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Switch/Switch with Text (Left).prefab");

        [MenuItem(MENU_ROOT + "Switch/Switch with Text (Right)", false, MENU_PRIORITY + 242)]
        private static void CreateSwitchWithTextRight() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Switch/Switch with Text (Right).prefab");


        [MenuItem(MENU_ROOT + "Dropdown", false, MENU_PRIORITY + 300)]
        private static void CreateDropdown() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Dropdown.prefab");

        [MenuItem(MENU_ROOT + "InputField", false, MENU_PRIORITY + 301)]
        private static void CreateInputField() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/InputField.prefab");

        [MenuItem(MENU_ROOT + "Slider", false, MENU_PRIORITY + 302)]
        private static void CreateSlider() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Slider.prefab");


        [MenuItem(MENU_ROOT + "Scroll View", false, MENU_PRIORITY + 400)]
        private static void CreateScrollView() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Scroll View.prefab");

        [MenuItem(MENU_ROOT + "Scrollbar", false, MENU_PRIORITY + 401)]
        private static void CreateScrollbar() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Scrollbar.prefab");


        [MenuItem(MENU_ROOT + "Progress", false, MENU_PRIORITY + 500)]
        private static void CreateProgress() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Progress.prefab");

        [MenuItem(MENU_ROOT + "Span", false, MENU_PRIORITY + 501)]
        private static void CreateSpan() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Span.prefab");

        [MenuItem(MENU_ROOT + "Separator", false, MENU_PRIORITY + 502)]
        private static void CreateSeparator() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components/Separator.prefab");


        [MenuItem(MENU_ROOT + "Provider/Dialog", false, MENU_PRIORITY + 600)]
        private static void CreateProvider() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components 2/Dialog Provider.prefab", true);

        [MenuItem(MENU_ROOT + "Provider/Toast", false, MENU_PRIORITY + 601)]
        private static void CreateToast() => SpawnPrefabWithCanvas(PREFAB_ROOT + "Components 2/Toast Provider.prefab", true);
    }
}
