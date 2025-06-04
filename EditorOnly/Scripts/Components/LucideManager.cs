/*
 * This tool uses lucide-react
 * © 2022 Lucide Contributors - ISC License
 */

#if UNITY_EDITOR

using UnityEngine;

namespace Limitex.MonoUI.Editor.Components
{
    [AddComponentMenu("Mono UI/MI Lucide Manager")]
    public class LucideManager : MonoBehaviour
    {
        public string imageFileName;
        public int maxResolution = 1024;
    }
}
#endif
