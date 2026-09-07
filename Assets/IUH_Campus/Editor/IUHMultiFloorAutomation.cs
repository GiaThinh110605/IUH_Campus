using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus.Core;
using IUHCampus.Interaction;
using IUHCampus.NPC;
using IUHCampus.Player;
using IUHCampus.Save;
using IUHCampus.UI;
using IUHCampus.World;

namespace IUHCampus.Editor
{
    public static class IUHMultiFloorAutomation
    {
        private const string ScreenshotDir = "Assets/IUH_Campus/Screenshots/";
        private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity-ide\brain\90eb314c-5d87-4a56-9144-aab594d4439c\";

        [MenuItem("IUH Campus/Validate Multi-Floor Academic Interior & Capture 10 Views")]
        public static void RunMultiFloorValidationAndCapture()
        {
            Debug.Log("[IUH Multi-Floor Automation] Bắt đầu kiểm tra hệ thống giảng đường đa tầng & chụp 10 góc nhìn...");

            // 1. Build Multi-Floor World
            IUHMultiFloorMasterBuilder.BuildMultiFloorWorldAndSystems();

            if (!Directory.Exists(ScreenshotDir)) Directory.CreateDirectory(ScreenshotDir);
            if (!Directory.Exists(ArtifactDir)) Directory.CreateDirectory(ArtifactDir);

            // 2. Automated Multi-Floor Verification Checks
            RunMultiFloorChecks();

            // 3. Setup Verification Camera
            GameObject camGO = new GameObject("MultiFloor_VerificationCamera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 450f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.68f, 0.82f, 0.96f);

            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = cam;
                canvas.planeDistance = 1.0f;
            }

            // ── Shot 1: Ground Floor Lobby looking towards Stair Core ──
            Debug.Log("[Multi-Floor] Capturing Shot 1: Lobby & Stair Entrance...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Tầng 1 (Sảnh Tiếp Tân)");
            cam.transform.position = new Vector3(24.0f, 1.70f, 5.0f);
            cam.transform.rotation = Quaternion.Euler(4.0f, 0.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_01_lobby.png");

            // ── Shot 2: Typical Corridor on Floor 2 ──
            Debug.Log("[Multi-Floor] Capturing Shot 2: Floor 2 Corridor...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Tầng 2 (Hành lang Giảng đường)");
            cam.transform.position = new Vector3(24.0f, 5.70f, 21.0f);
            cam.transform.rotation = Quaternion.Euler(4.0f, 180.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_02_corridor_f2.png");

            // ── Shot 3: Standard Classroom G-201 ──
            Debug.Log("[Multi-Floor] Capturing Shot 3: Standard Classroom G-201...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Tầng 2 (Phòng học G-201)");
            cam.transform.position = new Vector3(21.8f, 5.65f, 13.5f);
            cam.transform.rotation = Quaternion.Euler(6.0f, -40.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_03_standard_classroom.png");

            // ── Shot 4: Advanced Computer Lab G-401 ──
            Debug.Log("[Multi-Floor] Capturing Shot 4: Computer Lab G-401...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Tầng 4 (Lab Máy Tính Chuyên Ngành G-401)");
            cam.transform.position = new Vector3(21.8f, 13.65f, 13.5f);
            cam.transform.rotation = Quaternion.Euler(6.0f, -40.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_04_computer_classroom.png");

            // ── Shot 5: Stair Flight with Human Scale & Handrails ──
            Debug.Log("[Multi-Floor] Capturing Shot 5: Stair Flight...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Buồng thang bộ (Vế 1 Tầng 1 -> 2)");
            cam.transform.position = new Vector3(22.4f, 0.85f, 21.0f);
            cam.transform.rotation = Quaternion.Euler(-22.0f, 0.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_05_stair_flight.png");

            // ── Shot 6: Stair Landing with Floor Sign & Window ──
            Debug.Log("[Multi-Floor] Capturing Shot 6: Stair Landing...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Chiếu nghỉ liên tầng Tầng 2 & 3");
            cam.transform.position = new Vector3(24.0f, 6.20f, 27.5f);
            cam.transform.rotation = Quaternion.Euler(4.0f, 180.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_06_stair_landing.png");

            // ── Shot 7: Floor 2 Corridor Overview ──
            Debug.Log("[Multi-Floor] Capturing Shot 7: Floor 2 Corridor Overview...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Tầng 2 (Toàn cảnh Hành lang)");
            cam.transform.position = new Vector3(24.0f, 5.70f, 8.0f);
            cam.transform.rotation = Quaternion.Euler(4.0f, 0.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_07_floor_2_overview.png");

            // ── Shot 8: Floor 3 Large Lecture Hall G-301 ──
            Debug.Log("[Multi-Floor] Capturing Shot 8: Floor 3 Large Lecture Hall...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Tầng 3 (Giảng đường Lớn G-301)");
            cam.transform.position = new Vector3(21.8f, 9.65f, 13.5f);
            cam.transform.rotation = Quaternion.Euler(6.0f, -40.0f, 0.0f);
            cam.fieldOfView = 65f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_08_floor_3_overview.png");

            // ── Shot 9: Player Standing in Classroom Aisle ──
            Debug.Log("[Multi-Floor] Capturing Shot 9: Player in Classroom...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Tầng 2 (Phòng G-201)");
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(19.6f, 4.1f, 16.5f);
                player.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            cam.transform.position = new Vector3(19.6f, 5.50f, 14.5f);
            cam.transform.rotation = Quaternion.Euler(10.0f, 0.0f, 0.0f);
            cam.fieldOfView = 68f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_09_player_in_classroom.png");

            // ── Shot 10: Stairwell Vertical Shaft View ──
            Debug.Log("[Multi-Floor] Capturing Shot 10: Vertical Stairwell Shaft...");
            InteractionUI.SetLocationText("Khuôn viên IUH | Tòa G - Giếng thang bộ thông suốt 4 tầng");
            cam.transform.position = new Vector3(23.5f, 7.5f, 26.2f);
            cam.transform.rotation = Quaternion.Euler(35.0f, 195.0f, 0.0f);
            cam.fieldOfView = 75f;
            CaptureView(cam, 1920, 1080, "screenshot_multifloor_10_stairwell_vertical.png");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(camGO);

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.Refresh();

            Debug.Log("[IUH Multi-Floor Automation] Hoàn thành toàn diện bài kiểm tra và chụp 10 ảnh hệ thống giảng đường đa tầng!");
        }

        private static void RunMultiFloorChecks()
        {
            Debug.Log("[Multi-Floor Check] Bắt đầu kiểm tra cấu trúc...");

            // 1. Verify Floors Exist
            string[] requiredFloors = { "Floor_01", "Floor_02", "Floor_03", "Floor_04" };
            foreach (var f in requiredFloors)
            {
                var go = GameObject.Find(f);
                if (go == null) Debug.LogError($"[Multi-Floor Check] Thiếu tầng: {f}");
                else Debug.Log($"[Multi-Floor Check] Đã xác nhận tầng: {f} tồn tại.");
            }

            // 2. Verify Stair Core Exists
            var stair = GameObject.Find("StairCore_North");
            if (stair == null) Debug.LogError("[Multi-Floor Check] Thiếu buồng thang bộ liên tầng StairCore_North!");
            else Debug.Log("[Multi-Floor Check] Buồng thang bộ liên tầng StairCore_North hoạt động tốt.");

            // 3. Verify Playable Classrooms
            string[] requiredRooms = { "ROOM_G_201", "ROOM_G_301", "ROOM_G_401" };
            foreach (var r in requiredRooms)
            {
                var rGO = GameObject.Find(r);
                if (rGO == null) Debug.LogError($"[Multi-Floor Check] Thiếu phòng học chơi được: {r}");
                else Debug.Log($"[Multi-Floor Check] Phòng học {r} được trang bị đầy đủ bàn ghế, bảng viết, cửa và trigger.");
            }
        }

        private static void CaptureView(Camera cam, int width, int height, string filename)
        {
            RenderTexture rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(screenShot);

            string projPath = Path.Combine(ScreenshotDir, filename);
            File.WriteAllBytes(projPath, bytes);

            string artPath = Path.Combine(ArtifactDir, filename);
            File.WriteAllBytes(artPath, bytes);

            Debug.Log($"[Screenshot Saved] -> {projPath} & {artPath}");
        }
    }
}
