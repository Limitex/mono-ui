#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
            var behaviours = Object.FindObjectsOfType<MonoUIBehaviour>(true);
            foreach (var behaviour in behaviours)
            {
                SetupUIEventHandlers(behaviour);
                EditorUtility.SetDirty(behaviour);
            }
        }

        private void SetupUIEventHandlers(MonoUIBehaviour behaviour)
        {
            var components = behaviour.GetComponents<Component>();
            if (components == null || components.Length == 0) return;
            var componentTypes = new HashSet<System.Type>(components.Where(c => c != null).Select(c => c.GetType()));
            var serializedObject = new SerializedObject(behaviour);

            if (componentTypes.Contains(typeof(Button)))
                RegisterEventHandler<Button>(
                    behaviour, serializedObject,
                    nameof(MonoUIBehaviour.button),
                    nameof(MonoUIBehaviour.OnButtonClick),
                    SetEvent);
            if (componentTypes.Contains(typeof(Toggle)))
                RegisterEventHandler<Toggle>(
                    behaviour, serializedObject,
                    nameof(MonoUIBehaviour.toggle),
                    nameof(MonoUIBehaviour.OnToggleValueChanged),
                    SetEvent);
            if (componentTypes.Contains(typeof(ToggleGroup)))
                RegisterEventHandler<ToggleGroup>(
                    behaviour, serializedObject,
                    nameof(MonoUIBehaviour.toggleGroup),
                    nameof(MonoUIBehaviour.OnToggleGroupValueChanged),
                    SetToggleGroupEvent);
            if (componentTypes.Contains(typeof(Slider)))
                RegisterEventHandler<Slider>(
                    behaviour, serializedObject,
                    nameof(MonoUIBehaviour.slider),
                    nameof(MonoUIBehaviour.OnSliderValueChanged),
                    SetEvent);
            if (componentTypes.Contains(typeof(Scrollbar)))
                RegisterEventHandler<Scrollbar>(
                    behaviour, serializedObject,
                    nameof(MonoUIBehaviour.scrollbar),
                    nameof(MonoUIBehaviour.OnScrollbarValueChanged),
                    SetEvent);
            if (componentTypes.Contains(typeof(ScrollRect)))
                RegisterEventHandler<ScrollRect>(
                    behaviour, serializedObject,
                    nameof(MonoUIBehaviour.scrollRect),
                    nameof(MonoUIBehaviour.OnScrollRectValueChanged),
                    SetEvent);
            if (componentTypes.Contains(typeof(TMP_Dropdown)))
                RegisterEventHandler<TMP_Dropdown>(
                    behaviour, serializedObject,
                    nameof(MonoUIBehaviour.tmpDropdown),
                    nameof(MonoUIBehaviour.OnDropdownValueChanged),
                    SetEvent);
            if (componentTypes.Contains(typeof(TMP_InputField)))
            {
                var fieldName = nameof(MonoUIBehaviour.tmpInputField);
                var inputFieldEvents = new string[]
                {
                    nameof(MonoUIBehaviour.OnInputFieldValueChanged),
                    nameof(MonoUIBehaviour.OnInputFieldEndEdit),
                    nameof(MonoUIBehaviour.OnInputFieldSelect),
                    nameof(MonoUIBehaviour.OnInputFieldDeselect)
                };

                foreach (var eventName in inputFieldEvents)
                {
                    RegisterEventHandler<TMP_InputField>(
                        behaviour, serializedObject, fieldName, eventName, SetEvent);
                }
            }
        }

        private void RegisterEventHandler<T>(MonoUIBehaviour behaviour, SerializedObject serializedObject, 
            string fieldName, string eventName, System.Action<T, MonoUIBehaviour, string> action) where T : Component
        {
            var component = behaviour.GetComponent<T>();
            if (component == null) return;
            var property = serializedObject.FindProperty(fieldName);
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
            var eventSetter = new UdonUIEventSetter(component, behaviour);
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
            var toggles = toggleGroup.GetComponentsInChildren<Toggle>();
            foreach (var t in toggles)
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
                var eventBase = GetEventBase(eventName);
                if (eventBase == null) return;
                UnityEventTools.AddStringPersistentListener(eventBase, udonBehaviour.SendCustomEvent, eventName);
                EditorUtility.SetDirty(uiComponent);
            }

            private UnityEventBase GetEventBase(string eventName = "")
            {
                return uiComponent switch
                {
                    Button button => button.onClick,
                    Toggle toggle => toggle.onValueChanged,
                    Slider slider => slider.onValueChanged,
                    Scrollbar scrollbar => scrollbar.onValueChanged,
                    ScrollRect scrollRect => scrollRect.onValueChanged,
                    TMP_Dropdown dropdown => dropdown.onValueChanged,
                    TMP_InputField inputField => eventName switch
                    {
                        nameof(MonoUIBehaviour.OnInputFieldValueChanged) => inputField.onValueChanged,
                        nameof(MonoUIBehaviour.OnInputFieldEndEdit) => inputField.onEndEdit,
                        nameof(MonoUIBehaviour.OnInputFieldSelect) => inputField.onSelect,
                        nameof(MonoUIBehaviour.OnInputFieldDeselect) => inputField.onDeselect,
                        _ => null
                    },
                    _ => null
                };
            }
        }
    }
}

#endif
