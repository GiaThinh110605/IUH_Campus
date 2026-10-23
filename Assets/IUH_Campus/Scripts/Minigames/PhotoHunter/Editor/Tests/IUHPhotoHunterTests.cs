using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using StarterAssets;
using IUHCampus.Minigames.PhotoHunter;
using IUHCampus.UI;

namespace IUHCampus.Minigames.PhotoHunter.Editor.Tests
{
    /// <summary>
    /// Rigorous automated regression test suite for Mini-Game 2.4 "Photo Hunter".
    /// Executes real production logic for idempotency, persistence rollback, geometric checks,
    /// building occlusion, feature coverage ratio (under/equal/over threshold),
    /// UI texture memory cleanup, session cancellation, and two-way mission switching.
    /// </summary>
    public static class IUHPhotoHunterTests
    {
        public struct TestReport
        {
            public int totalTests;
            public int passedTests;
            public int failedTests;
            public string logDetails;
        }

        [MenuItem("IUH Campus/Tests/Run Photo Hunter Automated Tests", false, 120)]
        public static void RunTestsMenu()
        {
            var report = RunAllTests();
            Debug.Log($"<color={(report.failedTests == 0 ? "#10b981" : "#ef4444")}><b>[PhotoHunter Tests Result]</b> Passed: {report.passedTests}/{report.totalTests} | Failed: {report.failedTests}</color>\n{report.logDetails}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Photo Hunter Test Runner",
                    $"Kết quả kiểm thử tự động (Production Code):\n\n" +
                    $"Tổng số assertions: {report.totalTests}\n" +
                    $"Passed: {report.passedTests}\n" +
                    $"Failed: {report.failedTests}\n\n" +
                    (report.failedTests == 0 ? "TẤT CẢ TEST ĐÃ ĐẠT 100%!" : "CÓ TEST THẤT BẠI - XEM CONSOLE ĐỂ BIẾT CHI TIẾT"),
                    "OK");
            }
        }

        public static TestReport RunAllTests()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run these tests in Edit Mode.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save your open scenes before running isolated Photo Hunter tests.");
            }
            var scenes = EditorSceneManager.GetSceneManagerSetup();
            var cursorLock = Cursor.lockState;
            bool cursorVisible = Cursor.visible;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                return RunCoreTests();
            }
            finally
            {
                IUHPhotoHunterStorage.SetSimulateWriteFailureForTest(false);
                IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(null);
                EditorSceneManager.RestoreSceneManagerSetup(scenes);
                Cursor.lockState = cursorLock;
                Cursor.visible = cursorVisible;
            }
        }

        private static void InitializeEditModeFixture(MonoBehaviour component)
        {
            // Runtime Awake is not invoked by AddComponent in Edit Mode.
            component.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)?.Invoke(component, null);
        }

        private static TestReport RunCoreTests()
        {
            var report = new TestReport();
            var sb = new System.Text.StringBuilder();

            void AssertTrue(string testName, bool condition, string detail)
            {
                report.totalTests++;
                if (condition)
                {
                    report.passedTests++;
                    sb.AppendLine($"[PASS] {testName}");
                }
                else
                {
                    report.failedTests++;
                    sb.AppendLine($"[FAIL] {testName} - {detail}");
                }
            }

            // ================= TEST 1: IDEMPOTENCY & DUPLICATE REWARD (PRODUCTION CONTAINER) =================
            {
                var container = new PhotoHunterSaveContainer();
                bool firstCapture = container.RecordCompletion("photo_target_gate_01", "Cổng 1", "path1.jpg", 100, out int points1);
                AssertTrue("Test 1.1: Lần chụp đầu tiên phải được ghi nhận và thưởng 100 điểm", firstCapture && points1 == 100 && container.totalScore == 100, $"points={points1}, total={container.totalScore}");

                // Rapid spam click / re-capture same target
                bool secondCapture = container.RecordCompletion("photo_target_gate_01", "Cổng 1", "path1_new.jpg", 100, out int points2);
                AssertTrue("Test 1.2: Chụp lại/Spam click cùng mục tiêu KHÔNG ĐƯỢC cộng điểm trùng (0 điểm)", !secondCapture && points2 == 0 && container.totalScore == 100, $"points={points2}, total={container.totalScore}");

                // Add second distinct target
                bool thirdCapture = container.RecordCompletion("photo_target_central_courtyard", "Sân Trường", "path2.jpg", 100, out int points3);
                AssertTrue("Test 1.3: Mục tiêu mới khác ID phải được cộng thêm 100 điểm (Tổng 200)", thirdCapture && points3 == 100 && container.totalScore == 200, $"points={points3}, total={container.totalScore}");
            }

            // ================= TEST 2: PERSISTENCE, CORRUPT SANITIZATION & REAL DISK ROLLBACK =================
            {
                string testTempDir = Path.Combine(Application.temporaryCachePath, "IUH_Test_SaveDir_" + Guid.NewGuid().ToString("N"));
                string validSaveFile = Path.Combine(testTempDir, "IUH_PhotoHunter_ValidSave.json");

                try
                {
                    if (!Directory.Exists(testTempDir)) Directory.CreateDirectory(testTempDir);

                    // 2.1: Real disk serialization & deserialization integrity
                    var original = new PhotoHunterSaveContainer();
                    original.RecordCompletion("target_a", "Mục Tiêu A", "a.jpg", 100, out _);
                    original.RecordCompletion("target_b", "Mục Tiêu B", "b.jpg", 100, out _);

                    string json = JsonUtility.ToJson(original, true);
                    File.WriteAllText(validSaveFile, json);

                    string readJson = File.ReadAllText(validSaveFile);
                    var loaded = JsonUtility.FromJson<PhotoHunterSaveContainer>(readJson);

                    bool loadMatch = loaded != null &&
                                     loaded.totalScore == 200 &&
                                     loaded.completedTargetIds.Count == 2 &&
                                     loaded.completedTargetIds.Contains("target_a") &&
                                     loaded.completedTargetIds.Contains("target_b");
                    AssertTrue("Test 2.1: Tiến trình lưu và đọc lại từ file đĩa phải toàn vẹn 100%", loadMatch, "Data loaded did not match original saved container");

                    // 2.2: Production SanitizeAndValidate logic with corrupt input
                    var corruptContainer = new PhotoHunterSaveContainer();
                    corruptContainer.completedTargetIds.Add("target_a");
                    corruptContainer.completedTargetIds.Add("target_a"); // duplicate
                    corruptContainer.completedTargetIds.Add("ghost_invalid_target"); // defunct ID
                    corruptContainer.totalScore = 9999; // tampered score

                    var validTargets = new List<string> { "target_a", "target_b", "target_c" };
                    bool sanitized = corruptContainer.SanitizeAndValidate(validTargets);
                    bool sanitizePassed = sanitized &&
                                          corruptContainer.completedTargetIds.Count == 1 &&
                                          corruptContainer.completedTargetIds[0] == "target_a" &&
                                          corruptContainer.totalScore == 100;
                    AssertTrue("Test 2.2: Sanitize dữ liệu hỏng/trùng ID/mục tiêu ma và tính lại điểm chuẩn qua production method", sanitizePassed, $"count={corruptContainer.completedTargetIds.Count}, score={corruptContainer.totalScore}");

                    // 2.3: Real disk write failure triggering transaction rollback through production manager & storage
                    var workingContainer = new PhotoHunterSaveContainer();
                    workingContainer.RecordCompletion("target_a", "Mục Tiêu A", "a.jpg", 100, out _);

                    // Configure isolated test storage directory so user save is never touched
                    IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(testTempDir);
                    IUHPhotoHunterStorage.SaveProgress(workingContainer);

                    GameObject txMgrGo = new GameObject("TestManagerTx", typeof(IUHPhotoHunterManager));
                    var txManager = txMgrGo.GetComponent<IUHPhotoHunterManager>();
                    txManager.SetCustomSaveContainerForTest(workingContainer.Clone());

                    string photoBPath = Path.Combine(testTempDir, "test_b_temp.jpg");
                    File.WriteAllBytes(photoBPath, new byte[] { 0xFF, 0xD8, 0xFF }); // dummy JPEG

                    // Simulate disk write failure in storage layer
                    IUHPhotoHunterStorage.SetSimulateWriteFailureForTest(true);

                    // Execute real production transaction - must fail and rollback memory + delete file!
                    bool txFailed = !txManager.ExecutePhotoTransaction("target_b", "Mục Tiêu B", photoBPath, 100, out int pointsFail);

                    bool rollbackSuccess = txFailed &&
                                           pointsFail == 0 &&
                                           txManager.CurrentScore == 100 &&
                                           txManager.CompletedCount == 1 &&
                                           !txManager.CurrentSaveContainer.completedTargetIds.Contains("target_b") &&
                                           !File.Exists(photoBPath); // Verify photo file was deleted by rollback

                    AssertTrue("Test 2.3: Ghi đĩa thất bại kích hoạt rollback tức thì qua production transaction và xóa file rác", rollbackSuccess, $"score={txManager.CurrentScore}, fileExists={File.Exists(photoBPath)}");
                    AssertTrue("Test 2.3b: Save cũ vẫn giữ 100 điểm sau giao dịch thất bại",
                        IUHPhotoHunterStorage.LoadProgress().totalScore == 100, "Previous save was overwritten");

                    // 2.4: Restore failure simulation and verify normal transaction succeeds
                    IUHPhotoHunterStorage.SetSimulateWriteFailureForTest(false);
                    File.WriteAllBytes(photoBPath, new byte[] { 0xFF, 0xD8, 0xFF });
                    bool txSuccess = txManager.ExecutePhotoTransaction("target_b", "Mục Tiêu B", photoBPath, 100, out int pointsSuccess);

                    bool commitSuccess = txSuccess &&
                                         pointsSuccess == 100 &&
                                         txManager.CurrentScore == 200 &&
                                         txManager.CompletedCount == 2 &&
                                         txManager.CurrentSaveContainer.completedTargetIds.Contains("target_b");

                    AssertTrue("Test 2.4: Ghi đĩa thành công commit tiến trình bền vững 200 điểm qua transaction thật", commitSuccess, $"score={txManager.CurrentScore}, count={txManager.CompletedCount}");

                    UnityEngine.Object.DestroyImmediate(txMgrGo);
                }
                finally
                {
                    IUHPhotoHunterStorage.SetSimulateWriteFailureForTest(false);
                    IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(null);
                    try { if (Directory.Exists(testTempDir)) Directory.Delete(testTempDir, true); } catch { }
                }
            }

            // ================= TEST 3: GEOMETRIC DISTANCE VALIDATION (PRODUCTION TARGET) =================
            {
                GameObject camGo = new GameObject("TestCamera", typeof(Camera));
                Camera cam = camGo.GetComponent<Camera>();
                cam.transform.position = Vector3.zero;
                cam.transform.forward = Vector3.forward;

                GameObject targetGo = new GameObject("TestTarget", typeof(IUHPhotoTarget));
                IUHPhotoTarget target = targetGo.GetComponent<IUHPhotoTarget>();
                target.Configure("target_test", "Địa Điểm Test", "Mô tả", Vector3.zero, Vector3.forward, 10f, 40f);

                try
                {
                    // Case 3.1: Too Close (distance 4m < min 10m)
                    targetGo.transform.position = new Vector3(0, 0, 4.0f);
                    bool validTooClose = target.ValidatePhoto(cam, null, ~0, out string reasonTooClose);
                    AssertTrue("Test 3.1: Khoảng cách quá gần (4m < 10m) phải bị từ chối", !validTooClose && reasonTooClose.Contains("Quá gần"), $"reason={reasonTooClose}");

                    // Case 3.2: Too Far (distance 55m > max 40m)
                    targetGo.transform.position = new Vector3(0, 0, 55.0f);
                    bool validTooFar = target.ValidatePhoto(cam, null, ~0, out string reasonTooFar);
                    AssertTrue("Test 3.2: Khoảng cách quá xa (55m > 40m) phải bị từ chối", !validTooFar && reasonTooFar.Contains("quá xa"), $"reason={reasonTooFar}");

                    // Case 3.3: In Range (distance 20m in [10..40])
                    targetGo.transform.position = new Vector3(0, 0, 20.0f);
                    bool validInRange = target.ValidatePhoto(cam, null, ~0, out string reasonInRange);
                    AssertTrue("Test 3.3: Khoảng cách hợp lệ (20m) phải được chấp nhận", validInRange && string.IsNullOrEmpty(reasonInRange), $"reason={reasonInRange}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(camGo);
                    UnityEngine.Object.DestroyImmediate(targetGo);
                }
            }

            // ================= TEST 4: VIEWPORT & ANGLE VALIDATION =================
            {
                GameObject camGo = new GameObject("TestCameraAngle", typeof(Camera));
                Camera cam = camGo.GetComponent<Camera>();
                cam.transform.position = Vector3.zero;
                cam.transform.forward = Vector3.forward;

                GameObject targetGo = new GameObject("TestTargetAngle", typeof(IUHPhotoTarget));
                IUHPhotoTarget target = targetGo.GetComponent<IUHPhotoTarget>();
                target.Configure("target_angle", "Địa Điểm Góc", "Mô tả", Vector3.zero, Vector3.forward, 5f, 50f);

                try
                {
                    // Case 4.1: Target Behind Camera (Z = -15m)
                    targetGo.transform.position = new Vector3(0, 0, -15.0f);
                    bool validBehind = target.ValidatePhoto(cam, null, ~0, out string reasonBehind);
                    AssertTrue("Test 4.1: Mục tiêu nằm sau máy ảnh (Z < 0) phải bị từ chối", !validBehind && (reasonBehind.Contains("phía sau") || reasonBehind.Contains("phía trước")), $"reason={reasonBehind}");

                    // Case 4.2: Target severely off-center (> 50 degrees)
                    targetGo.transform.position = new Vector3(35.0f, 0, 5.0f);
                    bool validOffCenter = target.ValidatePhoto(cam, null, ~0, out string reasonOffCenter);
                    AssertTrue("Test 4.2: Mục tiêu lệch góc lớn (> 50°) phải bị từ chối", !validOffCenter, $"reason={reasonOffCenter}");

                    // Case 4.3: Direct frontal angle
                    targetGo.transform.position = new Vector3(0, 0, 20.0f);
                    float angle = target.GetFacingAngle(cam);
                    AssertTrue("Test 4.3: Hướng nhìn thẳng trực diện có góc ngắm nhỏ (< 15°)", angle < 15.0f, $"angle={angle}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(camGo);
                    UnityEngine.Object.DestroyImmediate(targetGo);
                }
            }

            // ================= TEST 5: REAL MANAGER MASK & BUILDING LAYER OCCLUSION =================
            {
                GameObject mgrGo = new GameObject("TestManagerOcc", typeof(IUHPhotoHunterManager));
                var manager = mgrGo.GetComponent<IUHPhotoHunterManager>();
                manager.ConfigureObstacleLayerMask();

                GameObject camGo = new GameObject("TestCameraOcc", typeof(Camera));
                Camera cam = camGo.GetComponent<Camera>();
                cam.transform.position = Vector3.zero;
                cam.transform.forward = Vector3.forward;

                GameObject targetGo = new GameObject("TestTargetOcc", typeof(IUHPhotoTarget));
                IUHPhotoTarget target = targetGo.GetComponent<IUHPhotoTarget>();
                target.Configure("target_occ", "Mục Tiêu Che Khuất", "Mô tả", Vector3.zero, Vector3.forward, 5f, 50f);
                targetGo.transform.position = new Vector3(0, 0, 25.0f);

                GameObject wallGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallGo.name = "BucTuongBuilding";
                wallGo.transform.position = new Vector3(0, 0, 10.0f);
                wallGo.transform.localScale = new Vector3(10, 10, 1);

                try
                {
                    int buildingLayer = LayerMask.NameToLayer("Building");
                    int playerLayer = LayerMask.NameToLayer("Player");
                    if (buildingLayer >= 0) wallGo.layer = buildingLayer;

                    Physics.SyncTransforms();

                    // 5.1: Verify real mask configured by production manager includes Building and excludes Player
                    LayerMask actualMask = manager.ObstacleLayerMask;
                    bool buildingIncluded = buildingLayer < 0 || (actualMask.value & (1 << buildingLayer)) != 0;
                    bool playerExcluded = playerLayer < 0 || (actualMask.value & (1 << playerLayer)) == 0;
                    AssertTrue("Test 5.1: Mask do IUHPhotoHunterManager cấu hình bao gồm Layer 8 (Building) và loại Layer 10 (Player)", buildingIncluded && playerExcluded, $"buildingIncluded={buildingIncluded}, playerExcluded={playerExcluded}");

                    // 5.2: Occlusion check through actual Building layer wall
                    bool validOccluded = target.ValidatePhoto(cam, null, actualMask, out string reasonOccluded);
                    AssertTrue("Test 5.2: Raycast xuyên qua tường chắn Layer Building phải phát hiện che khuất", !validOccluded && reasonOccluded.Contains("che khuất"), $"reason={reasonOccluded}");

                    // 5.3: Remove wall -> clear line of sight
                    UnityEngine.Object.DestroyImmediate(wallGo);
                    wallGo = null;
                    Physics.SyncTransforms();
                    bool validClear = target.ValidatePhoto(cam, null, actualMask, out string reasonClear);
                    AssertTrue("Test 5.3: Góc nhìn thông thoáng không có vật cản phải được chấp nhận", validClear && string.IsNullOrEmpty(reasonClear), $"reason={reasonClear}");
                }
                finally
                {
                    if (wallGo != null) UnityEngine.Object.DestroyImmediate(wallGo);
                    UnityEngine.Object.DestroyImmediate(camGo);
                    UnityEngine.Object.DestroyImmediate(targetGo);
                    UnityEngine.Object.DestroyImmediate(mgrGo);
                }
            }

            // ================= TEST 6: LANDMARK COVERAGE THRESHOLDS (UNDER, EQUAL, OVER) =================
            {
                GameObject camGo = new GameObject("TestCameraFeat", typeof(Camera));
                Camera cam = camGo.GetComponent<Camera>();
                cam.transform.position = Vector3.zero;
                cam.transform.forward = Vector3.forward;

                GameObject targetGo = new GameObject("TestTargetFeat", typeof(IUHPhotoTarget));
                IUHPhotoTarget target = targetGo.GetComponent<IUHPhotoTarget>();
                target.Configure("target_feat", "Công trình bao phủ", "Mô tả", Vector3.zero, Vector3.forward, 5f, 50f);
                targetGo.transform.position = new Vector3(0, 0, 20.0f);
                target.RequiredFeatureVisibilityRatio = 0.5f;

                // Add structure root and collider reference
                GameObject structGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                structGo.transform.position = targetGo.transform.position;
                structGo.transform.parent = targetGo.transform;
                var structCollider = structGo.GetComponent<Collider>();
                target.SetStructureReferences(structGo.transform, structCollider);
                Physics.SyncTransforms();

                try
                {
                    // 6.1: Direct structure match without string guessing
                    bool isPartOf = target.IsPartOfTarget(structCollider);
                    AssertTrue("Test 6.1: Nhận diện structure collider trực tiếp qua reference không đoán chuỗi", isPartOf, "IsPartOfTarget failed for direct reference");

                    // 6.2: Under Threshold Check (Center visible + 2 points out of view: visible = 1/3 = 0.33 < 0.5)
                    target.SetKeyFeatureOffsets(new List<Vector3> {
                        new Vector3(150f, 0, 0),
                        new Vector3(-150f, 0, 0)
                    });
                    bool underRatioValid = target.ValidatePhoto(cam, null, ~0, out string reasonUnder);
                    AssertTrue("Test 6.2: Tỷ lệ bao phủ dưới ngưỡng (1/3 < 0.5) phải bị từ chối", !underRatioValid && reasonUnder.Contains("một phần nhỏ"), $"reason={reasonUnder}");

                    // 6.3: Equal to Threshold Check (Center visible + 1 point out of view: visible = 1/2 = 0.5 >= 0.5)
                    target.SetKeyFeatureOffsets(new List<Vector3> {
                        new Vector3(150f, 0, 0)
                    });
                    bool equalRatioValid = target.ValidatePhoto(cam, null, ~0, out string reasonEqual);
                    AssertTrue("Test 6.3: Tỷ lệ bao phủ bằng ngưỡng (1/2 = 0.5) được chấp nhận hợp lệ", equalRatioValid, $"reason={reasonEqual}");

                    // 6.4: Over Threshold Check (Center visible + 2 points in view: visible = 3/3 = 1.0 >= 0.5)
                    target.SetKeyFeatureOffsets(new List<Vector3> {
                        new Vector3(-1.5f, 0, 0),
                        new Vector3(1.5f, 0, 0)
                    });
                    bool overRatioValid = target.ValidatePhoto(cam, null, ~0, out string reasonOver);
                    AssertTrue("Test 6.4: Tỷ lệ bao phủ trên ngưỡng (3/3 = 1.0) nằm trọn trong khung hình", overRatioValid, $"reason={reasonOver}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(camGo);
                    UnityEngine.Object.DestroyImmediate(targetGo);
                }
            }

            // ================= TEST 7: REAL UI TEXTURE MEMORY LIFECYCLE (IUHPhotoHunterUI) =================
            {
                GameObject uiGo = new GameObject("TestPhotoHunterUI", typeof(Canvas), typeof(IUHPhotoHunterUI));
                var ui = uiGo.GetComponent<IUHPhotoHunterUI>();

                // Build genuine serialized UI fixture for Victory dialog and thumbnail slots
                GameObject victoryGo = new GameObject("VictoryDialog");
                victoryGo.transform.SetParent(uiGo.transform, false);

                GameObject scoreGo = new GameObject("TxtScore", typeof(Text));
                scoreGo.transform.SetParent(victoryGo.transform, false);
                Text scoreText = scoreGo.GetComponent<Text>();

                RawImage[] thumbs = new RawImage[3];
                Text[] labels = new Text[3];
                for (int i = 0; i < 3; i++)
                {
                    GameObject slotGo = new GameObject($"Slot_{i}");
                    slotGo.transform.SetParent(victoryGo.transform, false);

                    GameObject thumbGo = new GameObject("Thumb", typeof(RawImage));
                    thumbGo.transform.SetParent(slotGo.transform, false);
                    thumbs[i] = thumbGo.GetComponent<RawImage>();

                    GameObject labelGo = new GameObject("TxtLabel", typeof(Text));
                    labelGo.transform.SetParent(slotGo.transform, false);
                    labels[i] = labelGo.GetComponent<Text>();
                }

                var soUI = new SerializedObject(ui);
                soUI.FindProperty("m_VictoryDialog").objectReferenceValue = victoryGo;
                soUI.FindProperty("m_TxtVictoryScore").objectReferenceValue = scoreText;
                SerializedProperty propThumbs = soUI.FindProperty("m_PhotoThumbnails");
                SerializedProperty propLabels = soUI.FindProperty("m_PhotoLabels");
                propThumbs.arraySize = 3;
                propLabels.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    propThumbs.GetArrayElementAtIndex(i).objectReferenceValue = thumbs[i];
                    propLabels.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
                }
                soUI.ApplyModifiedProperties();

                try
                {
                    // Generate temp photos on disk
                    string tempImgA = Path.Combine(Application.temporaryCachePath, "test_gallery_a.jpg");
                    string tempImgB = Path.Combine(Application.temporaryCachePath, "test_gallery_b.jpg");
                    var sampleTex = new Texture2D(16, 16);
                    byte[] jpgBytes = sampleTex.EncodeToJPG();
                    File.WriteAllBytes(tempImgA, jpgBytes);
                    File.WriteAllBytes(tempImgB, jpgBytes);
                    UnityEngine.Object.DestroyImmediate(sampleTex);

                    var records = new List<PhotoRecord>
                    {
                        new PhotoRecord("p1", "Điểm 1", tempImgA, 100),
                        new PhotoRecord("p2", "Điểm 2", tempImgB, 100)
                    };

                    ui.PopulateVictoryDialog(200, records);
                    int loadedBefore = ui.LoadedTextureCount;
                    bool thumbsLoaded = thumbs[0].texture != null && thumbs[1].texture != null && thumbs[2].texture == null;
                    AssertTrue("Test 7.1: IUHPhotoHunterUI nạp và theo dõi textures ảnh khi mở gallery", loadedBefore == 2 && thumbsLoaded, $"loadedBefore={loadedBefore}");

                    // Close gallery -> textures must be destroyed immediately
                    ui.HideAllDialogs();
                    int loadedAfter = ui.LoadedTextureCount;
                    bool thumbsCleared = thumbs[0].texture == null && thumbs[1].texture == null;
                    AssertTrue("Test 7.2: IUHPhotoHunterUI giải phóng triệt để toàn bộ textures khi đóng gallery/dialog", loadedAfter == 0 && thumbsCleared, $"loadedAfter={loadedAfter}");

                    // Cleanup files
                    try { if (File.Exists(tempImgA)) File.Delete(tempImgA); } catch { }
                    try { if (File.Exists(tempImgB)) File.Delete(tempImgB); } catch { }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(uiGo);
                }
            }

            // ================= TEST 8: REAL MANAGER SESSION TOKEN & COROUTINE SAFETY =================
            {
                GameObject mgrGo = new GameObject("TestManagerSession", typeof(IUHPhotoHunterManager));
                var manager = mgrGo.GetComponent<IUHPhotoHunterManager>();

                try
                {
                    manager.StartMission(false);
                    int session1 = manager.CurrentSessionId;
                    bool active1 = manager.IsMissionActive;

                    // Cancelling mission must increment session token and deactivate mission
                    manager.CancelMission(restoreCursorAndController: false);
                    int session2 = manager.CurrentSessionId;
                    bool active2 = manager.IsMissionActive;

                    AssertTrue("Test 8.1: CancelMission tăng SessionId thực tế ngăn coroutine cũ ghi điểm muộn", session2 > session1 && !active2, $"session1={session1}, session2={session2}, active2={active2}");
                    AssertTrue("Test 8.2: Trạng thái manager chuyển về Inactive sạch sẽ sau khi hủy", manager.CurrentState == IUHPhotoHunterManager.PhotoHunterState.Inactive, $"state={manager.CurrentState}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(mgrGo);
                }
            }

            // ================= TEST 9: 3/3 STATE & ALBUM ACCESS WITHOUT RE-AWARD =================
            {
                GameObject mgrGo = new GameObject("TestManager3Of3", typeof(IUHPhotoHunterManager));
                var manager = mgrGo.GetComponent<IUHPhotoHunterManager>();

                // Build real UI fixture for manager
                GameObject uiGo = new GameObject("TestPhotoHunterUI_9", typeof(Canvas), typeof(IUHPhotoHunterUI));
                var ui = uiGo.GetComponent<IUHPhotoHunterUI>();

                GameObject victoryGo = new GameObject("VictoryDialog");
                victoryGo.transform.SetParent(uiGo.transform, false);

                GameObject scoreGo = new GameObject("TxtScore", typeof(Text));
                scoreGo.transform.SetParent(victoryGo.transform, false);
                Text scoreText = scoreGo.GetComponent<Text>();

                RawImage[] thumbs = new RawImage[3];
                Text[] labels = new Text[3];
                for (int i = 0; i < 3; i++)
                {
                    GameObject slotGo = new GameObject($"Slot_{i}");
                    slotGo.transform.SetParent(victoryGo.transform, false);

                    GameObject thumbGo = new GameObject("Thumb", typeof(RawImage));
                    thumbGo.transform.SetParent(slotGo.transform, false);
                    thumbs[i] = thumbGo.GetComponent<RawImage>();

                    GameObject labelGo = new GameObject("TxtLabel", typeof(Text));
                    labelGo.transform.SetParent(slotGo.transform, false);
                    labels[i] = labelGo.GetComponent<Text>();
                }

                var soUI = new SerializedObject(ui);
                soUI.FindProperty("m_VictoryDialog").objectReferenceValue = victoryGo;
                soUI.FindProperty("m_TxtVictoryScore").objectReferenceValue = scoreText;
                SerializedProperty propThumbs = soUI.FindProperty("m_PhotoThumbnails");
                SerializedProperty propLabels = soUI.FindProperty("m_PhotoLabels");
                propThumbs.arraySize = 3;
                propLabels.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    propThumbs.GetArrayElementAtIndex(i).objectReferenceValue = thumbs[i];
                    propLabels.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
                }
                soUI.ApplyModifiedProperties();

                // Wire UI and 3 targets to manager via SerializedObject
                List<IUHPhotoTarget> targetsList = new List<IUHPhotoTarget>();
                for (int i = 1; i <= 3; i++)
                {
                    GameObject tGo = new GameObject($"Target_{i}", typeof(IUHPhotoTarget));
                    tGo.transform.SetParent(mgrGo.transform, false);
                    var t = tGo.GetComponent<IUHPhotoTarget>();
                    t.Configure($"t{i}", $"Địa Điểm {i}", "Mô tả", Vector3.zero, Vector3.forward, 5f, 50f);
                    targetsList.Add(t);
                }

                var soMgr = new SerializedObject(manager);
                soMgr.FindProperty("m_UI").objectReferenceValue = ui;
                SerializedProperty propMgrTargets = soMgr.FindProperty("m_Targets");
                propMgrTargets.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    propMgrTargets.GetArrayElementAtIndex(i).objectReferenceValue = targetsList[i];
                }
                soMgr.ApplyModifiedProperties();

                // Generate 3 real temp JPEG photos on disk
                string p1 = Path.Combine(Application.temporaryCachePath, "test_p1.jpg");
                string p2 = Path.Combine(Application.temporaryCachePath, "test_p2.jpg");
                string p3 = Path.Combine(Application.temporaryCachePath, "test_p3.jpg");
                var dummyTex = new Texture2D(8, 8);
                byte[] jpgData = dummyTex.EncodeToJPG();
                File.WriteAllBytes(p1, jpgData);
                File.WriteAllBytes(p2, jpgData);
                File.WriteAllBytes(p3, jpgData);
                UnityEngine.Object.DestroyImmediate(dummyTex);

                try
                {
                    var fullContainer = new PhotoHunterSaveContainer();
                    fullContainer.RecordCompletion("t1", "Địa Điểm 1", p1, 100, out _);
                    fullContainer.RecordCompletion("t2", "Địa Điểm 2", p2, 100, out _);
                    fullContainer.RecordCompletion("t3", "Địa Điểm 3", p3, 100, out _);

                    manager.SetCustomSaveContainerForTest(fullContainer);

                    // 9.1: Verify manager state and progress calculation (3/3 and 300 points)
                    bool managerStatePassed = manager.CurrentScore == 300 &&
                                              manager.CompletedCount == 3 &&
                                              manager.IsAllCompleted;
                    AssertTrue("Test 9.1: Manager phát hiện chính xác trạng thái 3/3 địa điểm với tổng điểm 300", managerStatePassed, $"score={manager.CurrentScore}, count={manager.CompletedCount}, isAllCompleted={manager.IsAllCompleted}");

                    // 9.2: Open Album from Briefing - verify no duplicate reward and UI populates
                    int scoreBefore = manager.CurrentScore;
                    manager.OpenAlbumFromBriefing();
                    int scoreAfter = manager.CurrentScore;

                    bool noReAward = scoreBefore == 300 && scoreAfter == 300 && manager.CurrentState == IUHPhotoHunterManager.PhotoHunterState.Victory;
                    bool uiTextValid = scoreText.text.Contains("300") && scoreText.text.Contains("3/3");
                    bool uiTexturesLoaded = ui.LoadedTextureCount == 3 && thumbs[0].texture != null && thumbs[1].texture != null && thumbs[2].texture != null;
                    bool labelsValid = labels[0].text.Contains("Địa Điểm 1") && labels[1].text.Contains("Địa Điểm 2") && labels[2].text.Contains("Địa Điểm 3");

                    AssertTrue("Test 9.2: Mở xem Album/Kết quả từ Briefing không cấp thêm điểm thưởng trùng", noReAward, $"before={scoreBefore}, after={scoreAfter}, state={manager.CurrentState}");
                    AssertTrue("Test 9.3: Giao diện Album hiển thị đúng 300 điểm, nhãn 3/3 và nạp đủ 3 texture thật từ đĩa", uiTextValid && uiTexturesLoaded && labelsValid, $"text={scoreText.text}, texturesLoaded={ui.LoadedTextureCount}");

                    // 9.4: Clean teardown frees textures
                    manager.CancelMission(restoreCursorAndController: false);
                    AssertTrue("Test 9.4: Đóng Album giải phóng toàn bộ 3 texture và đưa manager về Inactive", ui.LoadedTextureCount == 0 && manager.CurrentState == IUHPhotoHunterManager.PhotoHunterState.Inactive, $"loaded={ui.LoadedTextureCount}, state={manager.CurrentState}");
                }
                finally
                {
                    try { if (File.Exists(p1)) File.Delete(p1); } catch { }
                    try { if (File.Exists(p2)) File.Delete(p2); } catch { }
                    try { if (File.Exists(p3)) File.Delete(p3); } catch { }
                    UnityEngine.Object.DestroyImmediate(uiGo);
                    UnityEngine.Object.DestroyImmediate(mgrGo);
                }
            }

            // ================= TEST 10: TWO-WAY MISSION SWITCHING & MODAL EXCLUSIVITY =================
            {
                // Setup Portal UI fixture
                GameObject portalGo = new GameObject("TestPortalUI", typeof(Canvas), typeof(IUHMiniGamePortalUI));
                var portalUI = portalGo.GetComponent<IUHMiniGamePortalUI>();

                GameObject menuPanelGo = new GameObject("MenuPanel");
                menuPanelGo.transform.SetParent(portalGo.transform, false);

                var soPortal = new SerializedObject(portalUI);
                soPortal.FindProperty("m_MenuPanel").objectReferenceValue = menuPanelGo;
                soPortal.ApplyModifiedProperties();

                // Setup Photo Hunter Manager
                GameObject photoMgrGo = new GameObject("TestPhotoHunterMgr", typeof(IUHPhotoHunterManager));
                var photoManager = photoMgrGo.GetComponent<IUHPhotoHunterManager>();

                // Setup Grab Campus Manager
                GameObject grabMgrGo = new GameObject("TestGrabCampusMgr", typeof(IUHCampus.Minigames.GrabCampus.IUHGrabCampusManager));
                var grabManager = grabMgrGo.GetComponent<IUHCampus.Minigames.GrabCampus.IUHGrabCampusManager>();

                InitializeEditModeFixture(portalUI);
                InitializeEditModeFixture(photoManager);
                InitializeEditModeFixture(grabManager);

                try
                {
                    // 10.1: Switch Grab -> Photo Hunter:
                    // Start Grab mission first
                    grabManager.StartMission(0);
                    AssertTrue("Test 10.1a: Grab Campus mission đang hoạt động trước khi chuyển đổi", grabManager.IsMissionActive, "Grab mission was not active");

                    // Call Portal's real LaunchPhotoHunterGame method
                    portalUI.LaunchPhotoHunterGame();

                    bool grabCancelled = !grabManager.IsMissionActive;
                    bool photoBriefingActive = photoManager.CurrentState == IUHPhotoHunterManager.PhotoHunterState.Briefing && photoManager.HasActiveModal;

                    AssertTrue("Test 10.1: Chuyển Grab sang Photo Hunter qua Portal: Hủy nhiệm vụ Grab và mở Briefing Photo Hunter", grabCancelled && photoBriefingActive, $"grabActive={grabManager.IsMissionActive}, photoState={photoManager.CurrentState}");

                    // 10.2: Modal exclusivity when toggling Menu M during Briefing
                    portalUI.SetMenuOpen(true);
                    portalUI.SetMenuOpen(false);

                    bool cursorPreservedInBriefing = Cursor.lockState == CursorLockMode.None && Cursor.visible == true;
                    AssertTrue("Test 10.2: Đóng menu M khi đang ở Briefing giữ nguyên con trỏ và không mở khóa nhầm controller", cursorPreservedInBriefing && photoManager.HasActiveModal, $"lockState={Cursor.lockState}, visible={Cursor.visible}");

                    // 10.3: Switch Photo Hunter -> Grab via Portal
                    portalUI.LaunchGrabCampusGame(0);

                    bool photoCancelled = !photoManager.HasActiveSessionOrUI && photoManager.CurrentState == IUHPhotoHunterManager.PhotoHunterState.Inactive;
                    AssertTrue("Test 10.3: Chuyển Photo Hunter sang Grab qua Portal: Hủy sạch modal Photo Hunter", photoCancelled, $"hasActiveUI={photoManager.HasActiveSessionOrUI}, state={photoManager.CurrentState}");

                    // 10.4: Modal exclusivity when toggling Menu M during Victory
                    photoManager.ShowVictoryDialog();
                    portalUI.SetMenuOpen(true);
                    portalUI.SetMenuOpen(false);

                    bool cursorPreservedInVictory = Cursor.lockState == CursorLockMode.None && Cursor.visible == true;
                    AssertTrue("Test 10.4: Đóng menu M khi đang ở Victory giữ nguyên con trỏ thao tác xem Album", cursorPreservedInVictory && photoManager.HasActiveModal, $"lockState={Cursor.lockState}, visible={Cursor.visible}");

                    // 10.5: Clean close from Victory restores controller and cursor
                    photoManager.CancelMission(restoreCursorAndController: true);
                    bool closedCleanly = Cursor.lockState == CursorLockMode.Locked && !Cursor.visible && !photoManager.HasActiveSessionOrUI;
                    AssertTrue("Test 10.5: Đóng sạch Photo Hunter từ Victory trả lại quyền điều khiển và khóa con trỏ", closedCleanly, $"lockState={Cursor.lockState}, visible={Cursor.visible}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(grabMgrGo);
                    UnityEngine.Object.DestroyImmediate(photoMgrGo);
                    UnityEngine.Object.DestroyImmediate(portalGo);
                }
            }

            report.logDetails = sb.ToString();
            return report;
        }
    }
}
