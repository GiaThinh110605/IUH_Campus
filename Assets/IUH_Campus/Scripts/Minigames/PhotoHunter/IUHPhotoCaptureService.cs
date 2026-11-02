using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace IUHCampus.Minigames.PhotoHunter
{

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

            if(cam==null){onComplete?.Invoke(false,string.Empty);yield break;}

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

                if (uiLayer >= 0)
                {
                    cam.cullingMask = prevCullingMask & ~(1 << uiLayer);
                    cullingMaskModified = true;
                }

                tempRT = RenderTexture.GetTemporary(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = tempRT;
                cam.Render();

                RenderTexture.active = tempRT;
                capturedTex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
                capturedTex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
                capturedTex.Apply();

                byte[] jpegBytes = capturedTex.EncodeToJPG(85);

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

                if (capturedTex != null)
                {
                    ReleaseTexture(capturedTex);
                    capturedTex = null;
                }
            }

            if (isSessionValid == null || isSessionValid(sessionId))
            {
                onComplete?.Invoke(success, savedPath);
            }
        }

        public static Texture2D LoadTextureFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            Texture2D tex=null;
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (tex.LoadImage(bytes))
                {
                    return tex;
                }
                ReleaseTexture(tex);
                return null;
            }
            catch (Exception ex)
            {
                if(tex!=null)ReleaseTexture(tex);
                Debug.LogWarning($"[PhotoCaptureService] Không thể đọc ảnh từ {filePath}: {ex.Message}");
                return null;
            }
        }
        private static void ReleaseTexture(Texture2D texture)
        {
            if(texture==null)return;
            if(Application.isPlaying)Destroy(texture);else DestroyImmediate(texture);
        }
    }
}
