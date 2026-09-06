using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    public static class IUHGameReadyAutomation
    {
        private const string ScreenshotDir = "Assets/IUH_Campus/Screenshots/";
        private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity-ide\brain\31a41bcf-49ce-42f0-bc20-c50eb4f25547\";

        [MenuItem("IUH Campus/Build Game-Ready Campus & Capture All Views")]
        public static void BuildAndCaptureAll7Views()
        {
            Debug.Log("[IUH Game-Ready Automation] Starting Complete Generation and 7-View Verification...");

            // 1. Build Entire Game-Ready Scene
            IUHGameReadyMasterBuilder.BuildGameReadyScene();

            // 2. Setup Screenshot Directories
            if (!Directory.Exists(ScreenshotDir)) Directory.CreateDirectory(ScreenshotDir);
            if (!Directory.Exists(ArtifactDir)) Directory.CreateDirectory(ArtifactDir);

            // 3. Create or Find Verification Camera
            GameObject camGO = new GameObject("GameReady_VerificationCamera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.fieldOfView = 65.0f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 450.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.68f, 0.82f, 0.96f);

            // ── Shot 1: Aerial View ──
            Debug.Log("[IUH Game-Ready] Capturing Shot 1: Aerial View...");
            cam.transform.position = new Vector3(-6.0f, 46.0f, -34.0f);
            cam.transform.rotation = Quaternion.Euler(39.0f, 33.0f, 0.0f);
            cam.fieldOfView = 64.0f;
            CaptureView(cam, 1920, 1080, "screenshot_01_aerial.png");

            // ── Shot 2: Courtyard at Human Height (1.70m) ──
            Debug.Log("[IUH Game-Ready] Capturing Shot 2: Courtyard Human Height...");
            cam.transform.position = new Vector3(9.0f, 1.70f, -3.0f);
            cam.transform.rotation = Quaternion.Euler(3.0f, 38.0f, 0.0f);
            cam.fieldOfView = 68.0f;
            CaptureView(cam, 1920, 1080, "screenshot_02_courtyard_human.png");

            // ── Shot 3: Building G Public Entrance ──
            Debug.Log("[IUH Game-Ready] Capturing Shot 3: Building G Entrance...");
            cam.transform.position = new Vector3(11.5f, 1.75f, 8.0f);
            cam.transform.rotation = Quaternion.Euler(2.0f, 90.0f, 0.0f);
            cam.fieldOfView = 66.0f;
            CaptureView(cam, 1920, 1080, "screenshot_03_building_g_entrance.png");

            // ── Shot 4: Lobby & Reception Desk ──
            Debug.Log("[IUH Game-Ready] Capturing Shot 4: Reception Lobby...");
            cam.transform.position = new Vector3(18.0f, 1.75f, 7.0f);
            cam.transform.rotation = Quaternion.Euler(5.0f, 42.0f, 0.0f);
            cam.fieldOfView = 68.0f;
            CaptureView(cam, 1920, 1080, "screenshot_04_lobby.png");

            // ── Shot 5: Corridor & Circulation Spine ──
            Debug.Log("[IUH Game-Ready] Capturing Shot 5: Corridor Spine...");
            cam.transform.position = new Vector3(24.0f, 1.75f, 7.0f);
            cam.transform.rotation = Quaternion.Euler(2.0f, 180.0f, 0.0f);
            cam.fieldOfView = 68.0f;
            CaptureView(cam, 1920, 1080, "screenshot_05_corridor.png");

            // ── Shot 6: Computer Training Lab ──
            Debug.Log("[IUH Game-Ready] Capturing Shot 6: Computer Training Lab...");
            cam.transform.position = new Vector3(24.0f, 1.75f, 2.0f);
            cam.transform.rotation = Quaternion.Euler(5.0f, 180.0f, 0.0f);
            cam.fieldOfView = 68.0f;
            CaptureView(cam, 1920, 1080, "screenshot_06_computer_training_room.png");

            // ── Shot 7: Motorcycle Parking Area ──
            Debug.Log("[IUH Game-Ready] Capturing Shot 7: Motorcycle Parking...");
            cam.transform.position = new Vector3(9.0f, 1.70f, 1.0f);
            cam.transform.rotation = Quaternion.Euler(3.0f, 0.0f, 0.0f);
            cam.fieldOfView = 68.0f;
            CaptureView(cam, 1920, 1080, "screenshot_07_motorcycle_parking.png");

            // Clean up temporary camera
            GameObject.DestroyImmediate(camGO);

            // Re-save scene
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.Refresh();

            Debug.Log("[IUH Game-Ready Automation] All 7 Verification Shots Successfully Captured!");
        }

        private static void CaptureView(Camera cam, int width, int height, string fileName)
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

            // Save to artifact directory
            string artifactPath = Path.Combine(ArtifactDir, fileName);
            File.WriteAllBytes(artifactPath, bytes);
        }
    }
}
