#if UNITY_EDITOR

using UnityEngine;

namespace Limitex.MonoUI.Editor.Utils
{
    public class HierarchyComponentFinder<T> : ComponentFinderBase<T> where T : Component
    {
        public HierarchyComponentFinder(string guid = null) : base(guid) { }

        protected override void FindComponents(string guid)
        {
            if (!string.IsNullOrEmpty(guid))
                Debug.LogWarning("GUID is not null or empty. Use PrefabComponentFinder instead.");
            components.AddRange(Object.FindObjectsOfType<T>());
        }
    }
}

#endif
