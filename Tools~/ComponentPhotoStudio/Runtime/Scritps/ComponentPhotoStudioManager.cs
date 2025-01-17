#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Limitex.MonoUI.Tools.Component
{
    public class ComponentPhotoStudioManager : MonoBehaviour
    {
        [Serializable]
        public class ScreenshotSettings
        {
            public Camera TargetCamera;
            public Vector2Int Resolution = new Vector2Int(1920, 1080);
        }

        [Serializable]
        public class StudioSettings
        {
            public Transform Canvas;
        }

        [Header("Screenshot Settings")]
        [SerializeField] private ScreenshotSettings _screenshotSettings = new ScreenshotSettings();

        [Header("Photo Studio Settings")]
        [SerializeField] private StudioSettings _studioSettings = new StudioSettings();

        private static readonly string[] SEARCH_DIRECTORIES = { "Packages/dev.limitex.mono-ui/Runtime/Assets/Prefab" };
        private const string FILE_EXTENSION = "png";

        private readonly List<Transform> _prefabs = new List<Transform>();
        private int _currentPrefabIndex;

        private void Reset()
        {
            _screenshotSettings.TargetCamera = Camera.main;
        }

        public void SaveScreenshot()
        {
            if (!ValidateCamera()) return;

            string baseDirectory = EditorUtility.OpenFolderPanel("Select Screenshot Save Directory", "", "");

            if (string.IsNullOrEmpty(baseDirectory)) return;

            for (int i = 0; i < _prefabs.Count; i++)
            {
                SetAllPrefabsInactive();
                _prefabs[i].gameObject.SetActive(true);

                using (var screenshotCapture = new ScreenshotCapture(_screenshotSettings.TargetCamera, _screenshotSettings.Resolution))
                {
                    var screenshot = screenshotCapture.CaptureScreenshot();
                    var filename = $"{_prefabs[i].name}.{FILE_EXTENSION}";
                    var path = Path.Combine(baseDirectory, filename);
                    SaveScreenshotToFile(screenshot, _prefabs[i].name, path);
                }
            }

            SetAllPrefabsInactive();
            if (_prefabs.Count > 0)
            {
                _prefabs[_currentPrefabIndex].gameObject.SetActive(true);
            }

            Debug.Log($"All screenshots have been saved to: {baseDirectory}");
        }

        public void RefreshPrefabs()
        {
            if (!ValidateCanvas()) return;

            ClearExistingPrefabs();
            LoadAndInstantiatePrefabs();
        }

        public void ReloadPrefabs()
        {
            if (!ValidateCanvas()) return;

            _prefabs.Clear();
            _currentPrefabIndex = 0;
            for (int i = 0; i < _studioSettings.Canvas.childCount; i++)
            {
                Transform child = _studioSettings.Canvas.GetChild(i);
                child.gameObject.SetActive(i == 0);
                _prefabs.Add(child);
            }
        }

        public void NavigatePrefab(bool next)
        {
            if (_prefabs.Count == 0) return;

            var newIndex = next ?
                Math.Min(_currentPrefabIndex + 1, _prefabs.Count - 1) :
                Math.Max(_currentPrefabIndex - 1, 0);

            if (newIndex == _currentPrefabIndex) return;

            UpdatePrefabVisibility(_currentPrefabIndex, newIndex);
            _currentPrefabIndex = newIndex;
        }

        public void UpPrefab() => NavigatePrefab(true);
        public void DownPrefab() => NavigatePrefab(false);

        private bool ValidateCamera()
        {
            if (_screenshotSettings.TargetCamera != null) return true;

            Debug.LogError("Target camera is not assigned!");
            return false;
        }

        private bool ValidateCanvas()
        {
            if (_studioSettings.Canvas != null) return true;

            Debug.LogError("Canvas is not assigned!");
            return false;
        }

        private void SetAllPrefabsInactive()
        {
            foreach (var prefab in _prefabs)
            {
                prefab.gameObject.SetActive(false);
            }
        }

        private void ClearExistingPrefabs()
        {
            Transform[] children = new Transform[_studioSettings.Canvas.childCount];
            
            for (int i = 0; i < _studioSettings.Canvas.childCount; i++)
            {
                children[i] = _studioSettings.Canvas.GetChild(i);
            }

            foreach (var child in children)
            {
                DestroyImmediate(child.gameObject);
            }

            _prefabs.Clear();
            _currentPrefabIndex = 0;
        }

        private void LoadAndInstantiatePrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", SEARCH_DIRECTORIES);
            if (guids.Length == 0)
            {
                Debug.LogError("No prefabs found in specified directories!");
                return;
            }

            foreach (var guid in guids)
            {
                InstantiatePrefab(guid);
            }
        }

        private void InstantiatePrefab(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            if (instance == null) return;

            SetupPrefabInstance(instance, prefab.name);
            _prefabs.Add(instance.transform);
        }

        private void SetupPrefabInstance(GameObject instance, string prefabName)
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.MoveGameObjectToScene(instance, activeScene);

            var originalScale = instance.transform.localScale;
            instance.transform.SetParent(_studioSettings.Canvas);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = originalScale;

            instance.SetActive(_prefabs.Count == 0);
            instance.name = prefabName;
        }

        private void UpdatePrefabVisibility(int currentIndex, int newIndex)
        {
            _prefabs[currentIndex].gameObject.SetActive(false);
            _prefabs[newIndex].gameObject.SetActive(true);
            Selection.activeGameObject = _prefabs[newIndex].gameObject;
        }

        private void SaveScreenshotToFile(Texture2D screenshot, string prefabName, string path)
        {
            try
            {
                var bytes = screenshot.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                Debug.Log($"Screenshot saved: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save screenshot for {prefabName}: {e.Message}");
            }
            finally
            {
                DestroyImmediate(screenshot);
            }
        }

        private class ScreenshotCapture : IDisposable
        {
            private readonly Camera _camera;
            private readonly RenderTexture _renderTexture;
            private readonly RenderTexture _previousRenderTexture;
            private readonly Rect _previousRect;

            public ScreenshotCapture(Camera camera, Vector2Int resolution)
            {
                _camera = camera;
                _previousRenderTexture = camera.targetTexture;
                _previousRect = camera.rect;

                _renderTexture = new RenderTexture(resolution.x, resolution.y, 24);
                RenderTexture.active = _renderTexture;
                camera.targetTexture = _renderTexture;
                camera.rect = new Rect(0, 0, 1, 1);
            }

            public Texture2D CaptureScreenshot()
            {
                _camera.Render();

                var screenshot = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);
                screenshot.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
                screenshot.Apply();

                return screenshot;
            }

            public void Dispose()
            {
                _camera.targetTexture = _previousRenderTexture;
                _camera.rect = _previousRect;
                RenderTexture.active = null;

                if (_renderTexture != null)
                {
                    DestroyImmediate(_renderTexture);
                }
            }
        }
    }
}

#endif
