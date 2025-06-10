#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Limitex.MonoUI.Editor.Utils;

namespace Limitex.MonoUI.Editor.Utils
{
    public static class ReferencedByDrawer
    {
        public static void Draw<TTarget, TTrigger>(ref bool foldoutState, TTarget targetObject, string referenceFieldName)
            where TTarget : Component
            where TTrigger : Component
        {
            foldoutState = EditorGUILayout.Foldout(foldoutState, "Referenced By (Detected in Scene)", true, EditorStyles.foldoutHeader);
            if (!foldoutState) return;
            List<TTrigger> references = FindReferences<TTarget, TTrigger>(targetObject, referenceFieldName);

            DrawReferenceList(references);
        }

        private static void DrawReferenceList<T>(List<T> references) where T : Component
        {
            EditorGUI.indentLevel++;

            if (references.Count > 0)
            {
                foreach (var trigger in references)
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

        private static void DrawReferenceRow(Component trigger)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(trigger.gameObject, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static List<TTrigger> FindReferences<TTarget, TTrigger>(TTarget targetObject, string referenceFieldName)
            where TTarget : Component
            where TTrigger : Component
        {
            var references = new List<TTrigger>();

            using (var finder = new HierarchyComponentFinder<TTrigger>(includeInactive: true))
            {
                foreach (var trigger in finder)
                {
                    var serializedTrigger = new SerializedObject(trigger);
                    var targetProperty = serializedTrigger.FindProperty(referenceFieldName);

                    if (targetProperty != null && targetProperty.objectReferenceValue == targetObject)
                    {
                        references.Add(trigger);
                    }
                }
            }
            return references;
        }
    }
}

#endif
