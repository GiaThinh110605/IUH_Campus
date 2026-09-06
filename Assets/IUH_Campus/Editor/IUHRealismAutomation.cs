using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    [InitializeOnLoad]
    public static class IUHRealismAutomation
    {
        private const string TriggerFile = "Assets/IUH_Campus/Editor/.realism_trigger";
        private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity-ide\brain\90eb314c-5d87-4a56-9144-aab594d4439c\";
        private const string ScreenshotDir = "Assets/IUH_Campus/Screenshots/";

        static IUHRealismAutomation()
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

                ExecuteRealismPassAndCapture();
            }
        }

        [MenuItem("IUH Campus/Execute Realism Pass & Capture All Views")]
        public static void ExecuteRealismPassAndCapture()
        {
            Debug.Log("[IUH Realism Automation] Starting Realism Pass & 4-Camera Verification Capture...");

            // 1. Rebuild Right Cluster Scene
            IUHRightClusterBuilder.BuildRightClusterScene();

            // 2. Open Right Cluster Scene
            var scene = EditorSceneManager.OpenScene("Assets/IUH_Campus/Scenes/IUH_RightCluster.unity", OpenSceneMode.Single);

            if (!Directory.Exists(ScreenshotDir)) Directory.CreateDirectory(ScreenshotDir);
            if (!Directory.Exists(ArtifactDir)) Directory.CreateDirectory(ArtifactDir);

            // Disable unbaked runtime reflection probes if any
            foreach (var probe in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                probe.enabled = false;
            }

            // Find or setup the 4 verification cameras
            Camera camAerial = null;
            Camera camCourtyard = null;
            Camera camEntrance = null;
            Camera camPlayerHeight = null;

            foreach (var rootGO in scene.GetRootGameObjects())
            {
                foreach (var cam in rootGO.GetComponentsInChildren<Camera>(true))
                {
                    string n = cam.gameObject.name;
                    if (n == "Camera_Aerial" || n.Contains("Aerial")) camAerial = cam;
                    else if (n == "Camera_Courtyard" || n.Contains("Courtyard")) camCourtyard = cam;
                    else if (n == "Camera_Entrance" || n.Contains("Entrance")) camEntrance = cam;
                    else if (n == "Camera_PlayerHeight" || n.Contains("PlayerHeight")) camPlayerHeight = cam;
                }
            }

            Color tropicalSky = new Color(0.68f, 0.82f, 0.96f);

            // 1. Camera_Aerial: Balanced campus flycam oblique view showing U-shape, courtyard, and background city
            if (camAerial == null)
            {
                GameObject go = new GameObject("Camera_Aerial");
                camAerial = go.AddComponent<Camera>();
            }
            camAerial.transform.position = new Vector3(-2.0f, 40.0f, -32.0f);
            camAerial.transform.rotation = Quaternion.Euler(36.0f, 30.0f, 0.0f);
            camAerial.fieldOfView = 65.0f;
            camAerial.farClipPlane = 450.0f;
            camAerial.clearFlags = CameraClearFlags.SolidColor;
            camAerial.backgroundColor = tropicalSky;

            // 2. Camera_Courtyard: Diagonal perspective inside courtyard showing motorbikes, trees, benches, students
            if (camCourtyard == null)
            {
                GameObject go = new GameObject("Camera_Courtyard");
                camCourtyard = go.AddComponent<Camera>();
            }
            camCourtyard.transform.position = new Vector3(8.0f, 2.0f, -2.0f);
            camCourtyard.transform.rotation = Quaternion.Euler(8.0f, 45.0f, 0.0f);
            camCourtyard.fieldOfView = 68.0f;
            camCourtyard.farClipPlane = 300.0f;
            camCourtyard.clearFlags = CameraClearFlags.SolidColor;
            camCourtyard.backgroundColor = tropicalSky;

            // 3. Camera_Entrance: Grounding, curbs, drainage, zebra crossing & main campus entrance view
            if (camEntrance == null)
            {
                GameObject go = new GameObject("Camera_Entrance");
                camEntrance = go.AddComponent<Camera>();
            }
            camEntrance.transform.position = new Vector3(13.5f, 1.70f, -19.0f);
            camEntrance.transform.rotation = Quaternion.Euler(5.0f, 0.0f, 0.0f);
            camEntrance.fieldOfView = 65.0f;
            camEntrance.farClipPlane = 250.0f;
            camEntrance.clearFlags = CameraClearFlags.SolidColor;
            camEntrance.backgroundColor = tropicalSky;

            // 4. Camera_PlayerHeight: Human eye-level 1.70m perspective looking across courtyard towards Building G
            if (camPlayerHeight == null)
            {
                GameObject go = new GameObject("Camera_PlayerHeight");
                camPlayerHeight = go.AddComponent<Camera>();
            }
            camPlayerHeight.transform.position = new Vector3(11.0f, 1.70f, -5.5f);
            camPlayerHeight.transform.rotation = Quaternion.Euler(3.0f, 32.0f, 0.0f);
            camPlayerHeight.fieldOfView = 68.0f;
            camPlayerHeight.farClipPlane = 250.0f;
            camPlayerHeight.clearFlags = CameraClearFlags.SolidColor;
            camPlayerHeight.backgroundColor = tropicalSky;

            // Capture all 4 Camera Views (1920x1080)
            Debug.Log("[IUH Realism Automation] Capturing Camera_Aerial...");
            CaptureCamera(camAerial, 1920, 1080, "camera_aerial.png");

            Debug.Log("[IUH Realism Automation] Capturing Camera_Courtyard...");
            CaptureCamera(camCourtyard, 1920, 1080, "camera_courtyard.png");

            Debug.Log("[IUH Realism Automation] Capturing Camera_Entrance...");
            CaptureCamera(camEntrance, 1920, 1080, "camera_entrance.png");

            Debug.Log("[IUH Realism Automation] Capturing Camera_PlayerHeight...");
            CaptureCamera(camPlayerHeight, 1920, 1080, "camera_player_height.png");

            AssetDatabase.Refresh();
            Debug.Log("[IUH Realism Automation] Realism Pass & 4-Camera Verification Completed Successfully!");
        }

        private static void CaptureCamera(Camera cam, int width, int height, string fileName)
        {
            cam.enabled = true;
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

            string projPath = Path.Combine(ScreenshotDir, fileName);
            File.WriteAllBytes(projPath, bytes);

            string artPath = Path.Combine(ArtifactDir, fileName);
            try
            {
                File.WriteAllBytes(artPath, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[IUH Realism Automation] Could not copy to artifact dir: " + ex.Message);
            }
        }
    }
}
