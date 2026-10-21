using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace IUHCampus.Minigames.PhotoHunter
{
    /// <summary>
    /// Captures pristine 3D scene photographs from the game camera.
    /// Excludes all 2D HUD and UI layers using camera culling mask manipulation.
    /// Manages file encoding, local disk persistence, and strictly deallocates temporary textures to prevent memory leaks.
    /// </summary>
    public class IUHPhotoCaptureService : MonoBehaviour
    {
        private static IUHPhotoCaptureService s_Instance;
        public static IUHPhotoCaptureService Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = new GameObject("IUH_PhotoCaptureService");
                    s_Instance = go.AddComponent<IUHPhotoCaptureService>();
                    DontDestroyOnLoad(go);
                }
                return s_Instance;
            }
        }

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Captures a snapshot from the specified camera and persists it to disk.
        /// Temporarily removes the UI layer from the camera's culling mask to guarantee no UI watermark or HUD leakage.
        /// Immediately deallocates the temporary raw capture texture after encoding to avoid memory growth.
        /// </summary>
        /// <param name="cam">Active scene camera.</param>
        /// <param name="targetId">Unique target identifier.</param>
        /// <param name="sessionId">Current mission session ID to discard obsolete callbacks upon cancellation.</param>
        /// <param name="isSessionValid">Predicate checking if session is still alive.</param>
        /// <param name="onComplete">Callback returning (success, savedFilePath).</param>
        public void CaptureAndSavePhoto(
            Camera cam,
            string targetId,
            int sessionId,
            Func<int, bool> isSessionValid,
            Action<bool, string> onComplete)
        {
            StartCoroutine(CaptureRoutine(cam, targetId, sessionId, isSessionValid, onComplete));
        }

        private IEnumerator CaptureRoutine(
            Camera cam,
            string targetId,
            int sessionId,
            Func<int, bool> isSessionValid,
            Action<bool, string> onComplete)
        {
            if (cam == null)
            {
                Debug.LogError("[PhotoCaptureService] Camera chụp ảnh là null!");
                onComplete?.Invoke(false, string.Empty);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            // Check cancellation before rendering
            if (isSessionValid != null && !isSessionValid(sessionId))
            {
                yield break;
            }

            string savedPath = string.Empty;
            bool success = false;

            RenderTexture tempRT = null;
            RenderTexture prevCameraRT = cam.targetTexture;
            RenderTexture prevActiveRT = RenderTexture.active;

            int prevCullingMask = cam.cullingMask;
            int uiLayer = LayerMask.NameToLayer("UI");
            bool cullingMaskModified = false;

            Texture2D capturedTex = null;

            try
            {
                int captureWidth = 1280;
                int captureHeight = 720;

                // 1. Exclude UI layer completely so no HUD or overlays appear in the photo
                if (uiLayer >= 0)
                {
                    cam.cullingMask = prevCullingMask & ~(1 << uiLayer);
                    cullingMaskModified = true;
                }

                // 2. Allocate temporary render texture
                tempRT = RenderTexture.GetTemporary(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = tempRT;
                cam.Render();

                RenderTexture.active = tempRT;
                capturedTex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
                capturedTex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
                capturedTex.Apply();

                // 3. Encode to JPEG (quality 85)
                byte[] jpegBytes = capturedTex.EncodeToJPG(85);

                // 4. Persist to storage directory
                string dir = IUHPhotoHunterStorage.EnsurePhotosDirectory();
                string timestampStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                string fileName = $"photo_{targetId}_{timestampStr}_{Guid.NewGuid():N}.jpg";
                savedPath = Path.Combine(dir, fileName);

                File.WriteAllBytes(savedPath, jpegBytes);
                success = true;

                Debug.Log($"[PhotoCaptureService] Đã chụp và lưu ảnh sạch 3D thành công: {savedPath} ({jpegBytes.Length / 1024} KB)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhotoCaptureService] Lỗi khi render / ghi ảnh ra đĩa: {ex.Message}");
                savedPath = string.Empty;
                success = false;
            }
            finally
            {
                // Always restore camera state and release textures
                if (cullingMaskModified)
                {
                    cam.cullingMask = prevCullingMask;
                }

                cam.targetTexture = prevCameraRT;
                RenderTexture.active = prevActiveRT;

                if (tempRT != null)
                {
                    RenderTexture.ReleaseTemporary(tempRT);
                }

                // Immediately destroy the raw capture texture to prevent memory leaks
                if (capturedTex != null)
                {
                    Destroy(capturedTex);
                    capturedTex = null;
                }
            }

            // Verify session is still active before invoking callback
            if (isSessionValid == null || isSessionValid(sessionId))
            {
                onComplete?.Invoke(success, savedPath);
            }
        }

        /// <summary>
        /// Loads a saved photo file from disk into a managed Texture2D for UI display.
        /// Caller is responsible for destroying the returned texture when replacing or closing.
        /// </summary>
        public static Texture2D LoadTextureFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (tex.LoadImage(bytes))
                {
                    return tex;
                }
                Destroy(tex);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PhotoCaptureService] Không thể đọc ảnh từ {filePath}: {ex.Message}");
                return null;
            }
        }
    }
}
