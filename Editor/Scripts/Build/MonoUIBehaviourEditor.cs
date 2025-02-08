#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using VRC.Udon;
using UdonSharpEditor;
using Limitex.MonoUI.Udon;

namespace Limitex.MonoUI.Editor.Build
{
    public class MonoUIBehaviourEditor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            MonoUIBehaviour[] behaviours = Object.FindObjectsOfType<MonoUIBehaviour>(true);
            foreach (MonoUIBehaviour behaviour in behaviours)
            {
                SetupUIEventHandlers(behaviour);
                EditorUtility.SetDirty(behaviour);
            }
        }

        private void SetupUIEventHandlers(MonoUIBehaviour behaviour)
        {
            RegisterEventHandler<Button>(
                behaviour,
                nameof(MonoUIBehaviour.button),
                nameof(MonoUIBehaviour.OnButtonClick),
                SetEvent);
            RegisterEventHandler<Toggle>(
                behaviour,
                nameof(MonoUIBehaviour.toggle),
                nameof(MonoUIBehaviour.OnToggleValueChanged),
                SetEvent);
            RegisterEventHandler<ToggleGroup>(
                behaviour,
                nameof(MonoUIBehaviour.toggleGroup),
                nameof(MonoUIBehaviour.OnToggleGroupValueChanged),
                SetToggleGroupEvent);
            RegisterEventHandler<Slider>(
                behaviour,
                nameof(MonoUIBehaviour.slider),
                nameof(MonoUIBehaviour.OnSliderValueChanged),
                SetEvent);
            RegisterEventHandler<Scrollbar>(
                behaviour,
                nameof(MonoUIBehaviour.scrollbar),
                nameof(MonoUIBehaviour.OnScrollbarValueChanged),
                SetEvent);
            RegisterEventHandler<ScrollRect>(
                behaviour,
                nameof(MonoUIBehaviour.scrollRect),
                nameof(MonoUIBehaviour.OnScrollRectValueChanged),
                SetEvent);
            RegisterEventHandler<TMP_Dropdown>(
                behaviour,
                nameof(MonoUIBehaviour.tmpDropdown),
                nameof(MonoUIBehaviour.OnDropdownValueChanged),
                SetEvent);
        }

        private void RegisterEventHandler<T>(MonoUIBehaviour behaviour, string fieldName, string eventName, System.Action<T, MonoUIBehaviour, string> action) where T : Component
        {
            T component = behaviour.GetComponent<T>();
            if (component == null) return;

            SerializedObject serializedObject = new SerializedObject(behaviour);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property != null)
            {
                property.objectReferenceValue = component;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError($"Field {fieldName} not found on {behaviour.name}");
            }

            action(component, behaviour, eventName);
        }

        private void SetEvent<T>(T component, MonoUIBehaviour behaviour, string eventName) where T : Component
        {
            UdonUIEventSetter eventSetter = new UdonUIEventSetter(component, behaviour);
            if (eventSetter.IsValid)
            {
                eventSetter.SetEvent(eventName);
            }
            else
            {
                Debug.LogError($"UdonBehaviour not found on {behaviour.name}");
            }
        }

        private void SetToggleGroupEvent(ToggleGroup toggleGroup, MonoUIBehaviour behaviour, string eventName)
        {
            Toggle[] toggle = toggleGroup.GetComponentsInChildren<Toggle>();
            foreach (Toggle t in toggle)
            {
                SetEvent(t, behaviour, eventName);
            }
        }

        private class UdonUIEventSetter
        {
            private readonly Component uiComponent;
            private readonly UdonBehaviour udonBehaviour;

            public UdonUIEventSetter(Component component, MonoUIBehaviour behaviour)
            {
                uiComponent = component;
                udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(behaviour);
            }

            public bool IsValid => udonBehaviour != null;

            public void SetEvent(string eventName)
            {
                if (!IsValid) return;

                UnityEventBase eventBase = GetEventBase();
                if (eventBase == null) return;

                UnityEventTools.AddStringPersistentListener(
                    eventBase,
                    udonBehaviour.SendCustomEvent,
                    eventName
                );

                EditorUtility.SetDirty(uiComponent);
            }

            private UnityEventBase GetEventBase()
            {
                return uiComponent switch
                {
                    Button button => button.onClick,
                    Toggle toggle => toggle.onValueChanged,
                    Slider slider => slider.onValueChanged,
                    Scrollbar scrollbar => scrollbar.onValueChanged,
                    ScrollRect scrollRect => scrollRect.onValueChanged,
                    TMP_Dropdown dropdown => dropdown.onValueChanged,
                    _ => null
                };
            }
        }
    }
}

#endif
