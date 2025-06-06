#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.Linq;
using VRC.Udon;
using Limitex.MonoUI.Editor.Utils;
using Limitex.MonoUI.Udon;

class ObjectToggleBuildProcessor : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        ProcessSceneToggles(scene);
    }

    #region Main Processing Logic

    private void ProcessSceneToggles(Scene scene)
    {
        var objectToggles = FindComponentsInScene<ObjectToggle>(scene);
        var objectToggleTriggers = FindComponentsInScene<ObjectToggleTrigger>(scene);

        var links = BuildLinkMap(objectToggles, objectToggleTriggers);

        ApplyLinks(links);
    }

    #endregion

    #region Type-Safe Helper Methods

    private List<T> FindComponentsInScene<T>(Scene scene) where T : Component
    {
        var foundComponents = new List<T>();
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            using (var finder = new HierarchyComponentFinder<T>(includeInactive: true, parent: rootGo.transform))
            {
                foundComponents.AddRange(finder);
            }
        }
        return foundComponents;
    }

    private Dictionary<ObjectToggle, List<ObjectToggleTrigger>> BuildLinkMap(List<ObjectToggle> toggles, List<ObjectToggleTrigger> triggers)
    {
        var linkMap = new Dictionary<ObjectToggle, List<ObjectToggleTrigger>>();

        foreach (var ot in toggles)
        {
            linkMap[ot] = new List<ObjectToggleTrigger>();
        }

        foreach (var trigger in triggers)
        {
            var targetObjectToggle = (ObjectToggle)trigger.GetProgramVariable(ObjectToggleTrigger.TargetObjectToggleName);

            if (targetObjectToggle != null && linkMap.ContainsKey(targetObjectToggle))
            {
                linkMap[targetObjectToggle].Add(trigger);
            }
        }
        return linkMap;
    }

    private void ApplyLinks(Dictionary<ObjectToggle, List<ObjectToggleTrigger>> links)
    {
        if (links.Count == 0)
        {
            return;
        }

        int totalLinks = 0;
        foreach (var pair in links)
        {
            ObjectToggle objectToggle = pair.Key;

            List<ObjectToggleTrigger> togglesToLink = pair.Value;

            if (togglesToLink.Count > 0)
            {
                objectToggle.SetProgramVariable(ObjectToggle.LinkedTogglesName, togglesToLink.ToArray());
                totalLinks += togglesToLink.Count;
            }
        }
    }

    #endregion
}

#endif
