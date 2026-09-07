using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    [InitializeOnLoad]
    public static class IUHRestructureAutomation
    {
        private const string TriggerFile = "Assets/IUH_Campus/Editor/.rebuild_trigger";
        private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity-ide\brain\90eb314c-5d87-4a56-9144-aab594d4439c\";
        private const string ScreenshotDir = "Assets/IUH_Campus/Screenshots/";

        static IUHRestructureAutomation()
        {
            EditorApplication.delayCall += CheckAndExecute;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            CheckAndExecute();
        }

        private static void CheckAndExecute()
        {
            if (File.Exists(TriggerFile))
            {
                try
                {
                    File.Delete(TriggerFile);
                }
                catch {}

                ExecuteRestructureAndCapture();
            }
        }

        [MenuItem("IUH Campus/Execute Restructure & Capture Screenshots")]
        public static void ExecuteRestructureAndCapture()
        {
            Debug.Log("[IUH Automation] Starting Full Campus Restructure & Capture...");

            // 1. Rebuild Right Cluster Scene & Prefab
            IUHRightClusterBuilder.BuildRightClusterScene();

            // 2. Rebuild Master Campus Scene
            IUHCampusMasterBuilder.BuildMasterCampus();

            // 3. Open Right Cluster Scene for Camera Captures
            var scene = EditorSceneManager.OpenScene("Assets/IUH_Campus/Scenes/IUH_RightCluster.unity", OpenSceneMode.Single);

            if (!Directory.Exists(ScreenshotDir))
            {
                Directory.CreateDirectory(ScreenshotDir);
            }
            if (!Directory.Exists(ArtifactDir))
            {
                Directory.CreateDirectory(ArtifactDir);
            }

            Camera aerialCam = null;
            Camera topDownCam = null;

            foreach (var rootGO in scene.GetRootGameObjects())
            {
                foreach (var cam in rootGO.GetComponentsInChildren<Camera>(true))
                {
                    Debug.Log("[IUH Automation] Found camera in scene: " + cam.gameObject.name);
                    if (cam.gameObject.name == "Camera_AerialReference" || cam.gameObject.name.Contains("Aerial"))
                    {
                        aerialCam = cam;
                    }
                    else if (cam.gameObject.name == "Camera_TopDown" || cam.gameObject.name.Contains("TopDown"))
                    {
                        topDownCam = cam;
                    }
                }
            }

            // Configure camera transforms to frame the U-shaped cluster accurately
            if (aerialCam == null)
            {
                GameObject camGO = new GameObject("Camera_AerialReference");
                camGO.transform.position = new Vector3(2.0f, 56.0f, -34.0f);
                camGO.transform.rotation = Quaternion.Euler(46.0f, 16.0f, 0.0f);
                aerialCam = camGO.AddComponent<Camera>();
                aerialCam.fieldOfView = 64.0f;
                aerialCam.farClipPlane = 400.0f;
            }
            else
            {
                aerialCam.transform.position = new Vector3(2.0f, 56.0f, -34.0f);
                aerialCam.transform.rotation = Quaternion.Euler(46.0f, 16.0f, 0.0f);
                aerialCam.fieldOfView = 64.0f;
                aerialCam.farClipPlane = 400.0f;
            }

            if (topDownCam == null)
            {
                GameObject camGO = new GameObject("Camera_TopDown");
                camGO.transform.position = new Vector3(18.0f, 85.0f, 16.0f);
                camGO.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                topDownCam = camGO.AddComponent<Camera>();
                topDownCam.orthographic = true;
                topDownCam.orthographicSize = 38.0f;
                topDownCam.farClipPlane = 200.0f;
            }
            else
            {
                topDownCam.transform.position = new Vector3(18.0f, 85.0f, 16.0f);
                topDownCam.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                topDownCam.orthographic = true;
                topDownCam.orthographicSize = 38.0f;
                topDownCam.farClipPlane = 200.0f;
            }

            // 4. Capture Aerial Reference Camera
            aerialCam.enabled = true;
            CaptureCameraToFiles(aerialCam, 1920, 1080, "aerial_oblique_reference.png");
            Debug.Log("[IUH Automation] Successfully captured aerial_oblique_reference.png");

            // 5. Capture Top-Down Orthographic Camera
            topDownCam.enabled = true;
            CaptureCameraToFiles(topDownCam, 1600, 1200, "topdown_orthographic.png");
            Debug.Log("[IUH Automation] Successfully captured topdown_orthographic.png");

            // 6. Log Building Transforms Table
            PrintBuildingTransformTable();

            AssetDatabase.Refresh();
            Debug.Log("[IUH Automation] Restructure & Capture Completed Successfully!");
        }

        private static void CaptureCameraToFiles(Camera cam, int width, int height, string fileName)
        {
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            RenderTexture prevRT = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            cam.targetTexture = prevRT;
            RenderTexture.active = prevActive;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);

            // Save to project screenshots folder
            string projectPath = Path.Combine(ScreenshotDir, fileName);
            File.WriteAllBytes(projectPath, bytes);

            // Save to assistant artifact directory
            string artifactPath = Path.Combine(ArtifactDir, fileName);
            try
            {
                File.WriteAllBytes(artifactPath, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[IUH Automation] Could not copy to artifact dir: " + ex.Message);
            }
        }

        private static void PrintBuildingTransformTable()
        {
            string log = "\n================== IUH CAMPUS RIGHT CLUSTER TRANSFORM TABLE ==================\n" +
                         "Building | Position XYZ | Rotation XYZ | Scale XYZ | Role in U-shape | Notes\n" +
                         "-------------------------------------------------------------------------------------------------------------------------\n" +
                         "Building G (KTX Nữ) | (30.0, 0.0, 12.0) | (0.0, -90.0, 0.0) | (49.0, 31.0, 16.0) | Cánh chính dọc bên phải | Mặt dài vuông góc với trục X (chạy dọc trục Z), ban công quay vào sân trong\n" +
                         "Building I (KTX Nam) | (17.0, 0.0, 38.0) | (0.0, 0.0, 0.0) | (38.0, 42.0, 16.0) | Cạnh sau/trên của chữ U | Nằm phía sau nối góc với G, cao 42m (13 tầng) vươn cao tạo silhouette chồng lấp\n" +
                         "Building C (Giảng đường) | (6.0, 0.0, 18.0) | (0.0, 90.0, 0.0) | (28.0, 18.0, 16.0) | Cánh phụ dọc bên trái | Chạy dọc trục Z bên trái sân trong, kết nối skybridge sang G\n" +
                         "Low Podium (Khối đế trước G) | (18.0, 0.0, -6.0) | (0.0, 0.0, 0.0) | (26.0, 9.8, 12.0) | Khối đáy/phần nối trước | Khép kín góc trước của chữ U, kết nối lối đi bộ có mái che\n" +
                         "Bridge C to G | (18.0, 7.5, 26.0) | (0.0, 0.0, 0.0) | (8.5, 3.8, 3.8) | Cầu nối trên không tầng 2-3 | Kết nối trực tiếp cánh trái C và cánh phải G ở góc sau\n" +
                         "Ground Walkway Canopy | (14.0, 3.0, 0.0) | (0.0, 0.0, 0.0) | (4.0, 0.2, 6.0) | Mái che kết nối mặt đất | Nối khối đế Podium vào lòng sân trong có bãi xe\n" +
                         "Inner Courtyard (Sân trong) | (18.0, 0.0, 14.0) | (0.0, 0.0, 0.0) | (18.0, 0.1, 32.0) | Trung tâm lòng chữ U | Bãi đỗ xe máy (~130 chiếc), cây xanh nhiệt đới, lối đi nội bộ\n" +
                         "=========================================================================================\n";
            Debug.Log(log);
        }
    }
}
