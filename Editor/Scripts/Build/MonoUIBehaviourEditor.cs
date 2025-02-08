#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Limitex.MonoUI.Editor.Build
{
    public class MonoUIBehaviourEditor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {

        }
    }
}

#endif
