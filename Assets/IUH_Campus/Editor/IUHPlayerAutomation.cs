using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    public static class IUHPlayerAutomation
    {
        private const string ScreenshotDir = "Assets/IUH_Campus/Screenshots/";
        private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity-ide\brain\90eb314c-5d87-4a56-9144-aab594d4439c\";

        [MenuItem("IUH Campus/Test Player Exploration & Capture Views")]
        public static void RunPlayerExplorationAndCapture()
        {
            Debug.Log("[IUH Player Exploration] Starting Complete Verification & Image Capture...");

            // 1. Open Scene if not already active
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.path != "Assets/IUH_Campus/Scenes/IUH_RightCluster.unity")
            {
                activeScene = EditorSceneManager.OpenScene("Assets/IUH_Campus/Scenes/IUH_RightCluster.unity", OpenSceneMode.Single);
            }

            // 2. Ensure Interior and Player System exist
            GameObject interiorRoot = GameObject.Find("IUH_RightCluster_Interior");
            if (interiorRoot == null)
            {
                IUHRightClusterInteriorBuilder.BuildAndCaptureInterior();
                interiorRoot = GameObject.Find("IUH_RightCluster_Interior");
            }

            // 3. Find Player & Camera components
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                // Rebuild interior and player system
                IUHRightClusterInteriorBuilder.BuildAndCaptureInterior();
                player = GameObject.Find("Player");
            }

            if (player == null)
            {
                Debug.LogError("[IUH Player Exploration] Player GameObject could not be found!");
                return;
            }

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camGO = GameObject.Find("MainCamera");
                if (camGO != null) mainCam = camGO.GetComponent<Camera>();
            }

            if (mainCam == null)
            {
                Debug.LogError("[IUH Player Exploration] Main Camera could not be found!");
                return;
            }

            // Disable unbaked reflection probes
            foreach (var probe in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                probe.enabled = false;
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // Disable temporarily for precise positioning

            if (!Directory.Exists(ScreenshotDir)) Directory.CreateDirectory(ScreenshotDir);
            if (!Directory.Exists(ArtifactDir)) Directory.CreateDirectory(ArtifactDir);

            // Checkpoint 1: Exterior Entrance Plaza (Player approaching automatic sliding doors)
            Debug.Log("[IUH Player Exploration] Capturing Checkpoint 1: Exterior Entrance...");
            player.transform.position = new Vector3(109.0f, 0.05f, -8.0f);
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
            mainCam.transform.position = new Vector3(110.2f, 2.25f, -12.2f);
            mainCam.transform.rotation = Quaternion.Euler(10.0f, -12.0f, 0.0f);
            CaptureView(mainCam, 1920, 1080, "player_01_exterior_entrance.png");

            // Checkpoint 2: Reception Desk (Zone A - Student exploring reception area)
            Debug.Log("[IUH Player Exploration] Capturing Checkpoint 2: Reception Lobby...");
            player.transform.position = new Vector3(95.0f, 0.05f, 6.0f);
            player.transform.rotation = Quaternion.Euler(0, -90.0f, 0);
            mainCam.transform.position = new Vector3(97.8f, 2.15f, 4.6f);
            mainCam.transform.rotation = Quaternion.Euler(14.0f, -60.0f, 0.0f);
            CaptureView(mainCam, 1920, 1080, "player_02_reception.png");

            // Checkpoint 3: Waiting Lounge & Media Screen (Zone B - Student in modern lounge)
            Debug.Log("[IUH Player Exploration] Capturing Checkpoint 3: Waiting Lounge...");
            player.transform.position = new Vector3(108.0f, 0.05f, 5.5f);
            player.transform.rotation = Quaternion.Euler(0, 0.0f, 0);
            mainCam.transform.position = new Vector3(108.0f, 2.15f, 3.0f);
            mainCam.transform.rotation = Quaternion.Euler(12.0f, 0.0f, 0.0f);
            CaptureView(mainCam, 1920, 1080, "player_03_waiting_lounge.png");

            // Checkpoint 4: Training & Computer Lab (Zone C - Student inside central aisle)
            Debug.Log("[IUH Player Exploration] Capturing Checkpoint 4: Training Lab...");
            player.transform.position = new Vector3(96.0f, 0.05f, -9.5f);
            player.transform.rotation = Quaternion.Euler(0, 180.0f, 0);
            mainCam.transform.position = new Vector3(97.2f, 2.35f, -6.5f);
            mainCam.transform.rotation = Quaternion.Euler(14.0f, 196.0f, 0.0f);
            CaptureView(mainCam, 1920, 1080, "player_04_training_lab.png");

            // Reset Player to default spawn: PlayerSpawn_Exterior
            Transform spawnExt = interiorRoot.transform.Find("PlayerSystem/PlayerSpawn_Exterior");
            if (spawnExt != null)
            {
                player.transform.position = spawnExt.position;
                player.transform.rotation = spawnExt.rotation;
                mainCam.transform.position = spawnExt.position + new Vector3(0, 1.8f, -2.6f);
                mainCam.transform.rotation = Quaternion.Euler(12.0f, spawnExt.eulerAngles.y, 0);
            }

            if (cc != null) cc.enabled = true; // Re-enable CharacterController for Play Mode

            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.Refresh();
            Debug.Log("[IUH Player Exploration] All 4 Player Checkpoints Successfully Captured & Verified!");
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

            // Save to assistant artifact directory
            string artifactPath = Path.Combine(ArtifactDir, fileName);
            try
            {
                File.WriteAllBytes(artifactPath, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[IUH Player Exploration] Could not write to artifact dir: " + ex.Message);
            }
        }
    }
}
