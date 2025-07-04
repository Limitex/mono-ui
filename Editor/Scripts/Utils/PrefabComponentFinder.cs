#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Limitex.MonoUI.Editor.Utils
{
    public struct PrefabAssetData
    {
        public string guid;
        public string assetPath;
        public GameObject prefab;
    }

    public class PrefabComponentFinder<T> : ComponentFinderBase<T> where T : Component
    {
        private List<PrefabAssetData> prefabAssetDatas = new List<PrefabAssetData>();

        public PrefabComponentFinder(string guid, bool includeInactive = false, Transform parent = null) : base(guid, includeInactive, parent) { }

        protected override void FindComponents(string guid = null, bool includeInactive = false, Transform parent = null)
        {
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError("GUID is null or empty.");
                return;
            }

            if (parent != null)
            {
                Debug.LogWarning("Parent is not null. Use HierarchyComponentFinder instead.");
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning($"Invalid GUID: {guid}. Asset not found.");
                return;
            }

            GameObject prefab = PrefabUtility.LoadPrefabContents(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Failed to load prefab at path: {assetPath}");
                return;
            }

            components.AddRange(prefab.GetComponentsInChildren<T>(includeInactive));
            prefabAssetDatas.Add(new PrefabAssetData
            {
                guid = guid,
                assetPath = assetPath,
                prefab = prefab
            });
        }

        public override void Dispose()
        {
            foreach (PrefabAssetData prefabData in prefabAssetDatas)
            {
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabData.prefab, prefabData.assetPath);
                    PrefabUtility.UnloadPrefabContents(prefabData.prefab);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error processing prefab at {prefabData.assetPath}: {e.Message}");
                }
            }
            prefabAssetDatas.Clear();
            base.Dispose();
        }
    }
}

#endif
