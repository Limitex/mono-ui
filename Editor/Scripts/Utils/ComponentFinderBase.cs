#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Limitex.MonoUI.Editor.Utils
{
    public abstract class ComponentFinderBase<T> : IEnumerable<T>, IDisposable where T : Component
    {
        protected List<T> components = new List<T>();

        public ComponentFinderBase(string guid = null, bool includeInactive = false)
        {
            FindComponents(guid, includeInactive);
        }

        protected abstract void FindComponents(string guid, bool includeInactive);

        public IEnumerator<T> GetEnumerator() => components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Dispose()
        {
            components.Clear();
        }
    }
}

#endif
