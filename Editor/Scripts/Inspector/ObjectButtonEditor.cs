#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Limitex.MonoUI.Udon;
using Limitex.MonoUI.Editor.Utils;

namespace Limitex.MonoUI.Editor.Inspector
{
    [CustomEditor(typeof(ObjectButton))]
    public class ObjectButtonEditor : UnityEditor.Editor
    {
        private const string targetFieldName = "_targetObjectButton";
        private static bool showReferencedBy = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            showReferencedBy = EditorGUILayout.Foldout(showReferencedBy, "Referenced By (Detected in Scene)", true, EditorStyles.foldoutHeader);

            if (showReferencedBy)
            {
                DrawReferencedByList();
            }
        }

        #region Draw Methods

        private void DrawReferencedByList()
        {
            var currentObjectButton = (ObjectButton)target;

            List<MonoUIBehaviour> foundTriggers = FindReferencesInScene(currentObjectButton);

            EditorGUI.indentLevel++;

            if (foundTriggers.Count > 0)
            {
                foreach (var trigger in foundTriggers)
                {
                    if (trigger == null) continue;
                    DrawReferenceRow(trigger);
                }
            }
            else
            {
                EditorGUILayout.LabelField("None");
            }

            EditorGUI.indentLevel--;
        }

        private void DrawReferenceRow(MonoUIBehaviour trigger)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(trigger.gameObject.name);
            EditorGUILayout.ObjectField(trigger.gameObject, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Helper Methods

        private List<MonoUIBehaviour> FindReferencesInScene(ObjectButton targetButton)
        {
            var references = new List<MonoUIBehaviour>();

            FindAndAddReferences<ObjectButtonTrigger>(targetButton, references);

            return references;
        }

        private void FindAndAddReferences<T>(ObjectButton targetButton, List<MonoUIBehaviour> references) where T : MonoUIBehaviour
        {
            using (var finder = new HierarchyComponentFinder<T>(includeInactive: true))
            {
                foreach (var trigger in finder)
                {
                    var serializedTrigger = new SerializedObject(trigger);
                    var targetProperty = serializedTrigger.FindProperty(targetFieldName);

                    if (targetProperty != null && targetProperty.objectReferenceValue == targetButton)
                    {
                        references.Add(trigger);
                    }
                }
            }
        }

        #endregion
    }
}

#endif
