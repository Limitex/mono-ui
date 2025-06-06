#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Limitex.MonoUI.Udon;
using Limitex.MonoUI.Editor.Utils;

namespace Limitex.MonoUI.Editor.Inspector
{
    [CustomEditor(typeof(ObjectToggle))]
    public class ObjectToggleEditor : UnityEditor.Editor
    {
        private static bool showReferencedBy = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            ReferencedByDrawer.Draw<ObjectToggle, ObjectToggleTrigger>(
                ref showReferencedBy,
                (ObjectToggle)target,
                ObjectToggleTrigger.TargetObjectToggleName);
        }
    }
}

#endif
