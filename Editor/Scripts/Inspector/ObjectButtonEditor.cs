#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Limitex.MonoUI.Udon;
using Limitex.MonoUI.Editor.Utils;

namespace Limitex.MonoUI.Editor.Inspector
{
    [CustomEditor(typeof(ObjectButton))]
    public class ObjectButtonEditor : UnityEditor.Editor
    {
        private static bool showReferencedBy = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            ReferencedByDrawer.Draw<ObjectButton, ObjectButtonTrigger>(
                ref showReferencedBy,
                (ObjectButton)target,
                nameof(ObjectButtonTrigger._targetObjectButton));
        }
    }
}

#endif
