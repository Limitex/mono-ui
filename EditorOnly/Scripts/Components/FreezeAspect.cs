#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Limitex.MonoUI.Editor.Components
{
    [ExecuteAlways]
    public class AspectRatioController : MonoBehaviour
    {
        [SerializeField, Tooltip("Target camera to adjust aspect ratio")]
        private Camera targetCamera;

        [SerializeField, Tooltip("Desired aspect ratio (e.g. 16:9)")]
        private Vector2Int desiredAspect = new Vector2Int(16, 9);

        private void Reset()
        {
            targetCamera = Camera.main;
        }

        private void Update()
        {
            if (targetCamera == null) return;

            targetCamera.rect = CalculateViewportRect();
        }

        private Rect CalculateViewportRect()
        {
            float currentScreenAspect = Screen.width / (float)Screen.height;
            float targetAspectRatio = desiredAspect.x / (float)desiredAspect.y;
            float magnificationRate = targetAspectRatio / currentScreenAspect;

            return CreateAdjustedViewportRect(magnificationRate);
        }

        private Rect CreateAdjustedViewportRect(float magnificationRate)
        {
            var viewportRect = new Rect(0, 0, 1, 1);

            if (magnificationRate < 1)
            {
                viewportRect.width = magnificationRate;
                viewportRect.x = 0.5f - viewportRect.width * 0.5f;
            }
            else
            {
                viewportRect.height = 1 / magnificationRate;
                viewportRect.y = 0.5f - viewportRect.height * 0.5f;
            }

            return viewportRect;
        }
    }
}

#endif
