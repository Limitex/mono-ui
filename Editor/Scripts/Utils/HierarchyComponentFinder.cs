#if UNITY_EDITOR

using UnityEngine;

namespace Limitex.MonoUI.Editor.Utils
{
    public class HierarchyComponentFinder<T> : ComponentFinderBase<T> where T : Component
    {
        public HierarchyComponentFinder(string guid = null, bool includeInactive = false, Transform parent = null) : base(guid, includeInactive, parent) { }

        protected override void FindComponents(string guid = null, bool includeInactive = false, Transform parent = null)
        {
            if (!string.IsNullOrEmpty(guid))
                Debug.LogWarning("GUID is not null or empty. Use PrefabComponentFinder instead.");

            if (parent == null)
                components.AddRange(Object.FindObjectsOfType<T>(includeInactive));
            else
                components.AddRange(parent.GetComponentsInChildren<T>(includeInactive));
        }
    }
}

#endif
