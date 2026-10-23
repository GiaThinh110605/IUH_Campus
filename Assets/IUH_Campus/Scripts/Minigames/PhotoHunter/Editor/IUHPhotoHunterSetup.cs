#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.UI;

namespace IUHCampus.Minigames.PhotoHunter.Editor
{
    /// <summary>
    /// Idempotent, non-destructive Editor setup utility for Mini-Game 2.4 "Photo Hunter".
    /// Creates or updates target landmarks, UI hierarchy, and manager bindings with full Undo support.
    /// Safely saves scene to disk upon completion.
    /// </summary>
    public static class IUHPhotoHunterSetup
    {
        private const string RootName = "GAMEPLAY_PHOTO_HUNTER";
        private const string TargetsContainerName = "PHOTO_TARGETS";
        private const string UiRootName = "PhotoHunter_UI_Root";

        [MenuItem("IUH Campus/Setup Photo Hunter Minigame (One-Click)", false, 110)]
        [MenuItem("IUH Campus/Minigames/Setup Photo Hunter (Safe Mode)", false, 111)]
        public static void SetupPhotoHunterMenu()
        {
            SetupPhotoHunter(true);
        }

        public const string TargetSceneName = "IUH_Campus_Main";
        public const string TargetScenePath = "Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity";

        public static bool SetupPhotoHunter(bool interactive = false)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != TargetSceneName)
            {
                if (!interactive)
                {
                    Debug.LogError($"[PhotoHunter Setup] CHẶN: Scene hiện tại là '{activeScene.name}' ({activeScene.path}), không phải '{TargetSceneName}'. Chế độ không tương tác sẽ dừng lại ngay lập tức để không làm sai lệch scene khác.");
                    return false;
                }

                if (!EditorUtility.DisplayDialog("Xác nhận Scene",
                    $"Scene đang mở là '{activeScene.name}'. Photo Hunter yêu cầu thiết lập trên scene '{TargetSceneName}'.\n\nBạn có muốn mở scene '{TargetSceneName}' để thiết lập không?",
                    $"Mở {TargetSceneName} và tiếp tục", "Hủy"))
                {
                    return false;
                }

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    activeScene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
                }
                else
                {
                    return false;
                }
            }

            // 1. Locate or create root GameObject
            GameObject rootGo = GameObject.Find(RootName);
            if (rootGo == null)
            {
                rootGo = new GameObject(RootName);
                Undo.RegisterCreatedObjectUndo(rootGo, "Create Photo Hunter Root");
                Debug.Log("[PhotoHunter Setup] Đã tạo root object: " + RootName);
            }
            Undo.RecordObject(rootGo, "Configure Photo Hunter Root");

            // Manager component
            var manager = rootGo.GetComponent<IUHPhotoHunterManager>();
            if (manager == null)
            {
                manager = Undo.AddComponent<IUHPhotoHunterManager>(rootGo);
            }

            // 2. Setup Targets Container and 3 Core Landmarks
            Transform targetsContainer = rootGo.transform.Find(TargetsContainerName);
            if (targetsContainer == null)
            {
                var go = new GameObject(TargetsContainerName);
                go.transform.SetParent(rootGo.transform, false);
                targetsContainer = go.transform;
                Undo.RegisterCreatedObjectUndo(go, "Create Photo Targets Container");
            }

            var targetsList = new List<IUHPhotoTarget>();

            // Target 1: Gate 01 (Nguyen Van Bao Main Entrance)
            Transform gate01Struct = GameObject.Find("campus_P0_progress/Gate_01_SteelGate")?.transform
                                  ?? GameObject.Find("campus_P0_progress/Gate_01_Arch_Beam")?.transform;
            var target1 = SetupTarget(
                targetsContainer,
                "PhotoTarget_Gate01",
                "photo_target_gate_01",
                "Cổng Chính Trường (Gate 01)",
                "Cổng chào số 1 đường Nguyễn Văn Bảo với vòm thép và biển tên trường đặc trưng.",
                new Vector3(418.40f, 3.5f, -12.10f),
                new Vector3(398.0f, 0.65f, -28.0f),
                new Vector3(20.4f, 1.25f, 15.9f),
                6.0f, 44.0f,
                new Vector3(12.0f, 6.0f, 4.0f),
                gate01Struct,
                new List<Vector3> {
                    new Vector3(0f, 2.0f, 0f),       // Arch top
                    new Vector3(-4.5f, -0.5f, 0f),   // Left pillar
                    new Vector3(4.5f, -0.5f, 0f)     // Right pillar
                });
            targetsList.Add(target1);

            // Target 2: Central Courtyard (San Truong Trung Tam)
            Transform courtStruct = GameObject.Find("campus_P0_progress/Courtyard_Main")?.transform;
            var target2 = SetupTarget(
                targetsContainer,
                "PhotoTarget_CentralCourtyard",
                "photo_target_central_courtyard",
                "Sân Trung Tâm (Central Courtyard)",
                "Khuôn viên sân gạch trung tâm trường với các bồn cây, cột đèn và không gian mở.",
                new Vector3(375.84f, 1.5f, -52.10f),
                new Vector3(375.84f, 0.65f, -38.0f),
                -Vector3.forward,
                8.0f, 58.0f,
                new Vector3(28.0f, 4.0f, 28.0f),
                courtStruct,
                new List<Vector3> {
                    new Vector3(0f, 0.5f, 0f),       // Courtyard center
                    new Vector3(-7f, -0.5f, 6f),     // Left planter zone
                    new Vector3(7f, -0.5f, 6f)       // Right planter zone
                });
            targetsList.Add(target2);

            // Target 3: Block A Facade (Nha Hieu Bo / Block A)
            Transform blockAStruct = GameObject.Find("campus_P0_progress/Block_A_Sign_Letter")?.transform
                                  ?? GameObject.Find("campus_P0_progress/Block_A_Entrance_Canopy")?.transform;
            var target3 = SetupTarget(
                targetsContainer,
                "PhotoTarget_BlockA",
                "photo_target_block_a",
                "Mặt Tiền Tòa Nhà A (Block A)",
                "Sảnh chính và mặt tiền tòa nhà hành chính A với mái đón canopy và biển hiệu.",
                new Vector3(429.04f, 4.35f, -14.68f),
                new Vector3(420.0f, 0.65f, -14.68f),
                Vector3.right,
                5.0f, 40.0f,
                new Vector3(16.0f, 8.0f, 6.0f),
                blockAStruct,
                new List<Vector3> {
                    new Vector3(0f, 1.0f, 0f),       // Canopy center
                    new Vector3(-5.0f, -1.5f, 0f),   // Left pillar
                    new Vector3(5.0f, -1.5f, 0f)     // Right pillar
                });
            targetsList.Add(target3);

            // 3. Find or create UI under IUH_Minigame_Canvas
            Canvas targetCanvas = FindOrCreateCanvas();
            IUHPhotoHunterUI uiComponent = SetupUIHierarchy(targetCanvas);

            // 4. Wire serialized properties on Manager via SerializedObject
            SerializedObject soManager = new SerializedObject(manager);
            soManager.Update();

            SerializedProperty propTargets = soManager.FindProperty("m_Targets");
            propTargets.ClearArray();
            for (int i = 0; i < targetsList.Count; i++)
            {
                propTargets.InsertArrayElementAtIndex(i);
                propTargets.GetArrayElementAtIndex(i).objectReferenceValue = targetsList[i];
            }

            SerializedProperty propCam = soManager.FindProperty("m_ShootingCamera");
            if (propCam.objectReferenceValue == null)
            {
                propCam.objectReferenceValue = Camera.main;
            }

            SerializedProperty propUI = soManager.FindProperty("m_UI");
            propUI.objectReferenceValue = uiComponent;

            soManager.ApplyModifiedProperties();

            // 5. Update Card 4 in Portal UI if found
            UpdatePortalMenuCard();

            if (IUHCampus.World.IUHReferenceLayout.IsActive)
                IUHCampus.Editor.IUHReferenceCampusBuilder.RelinkGames(false);

            // Mark Scene Dirty and Save Scene to disk
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log($"<color=#10b981><b>[PhotoHunter Setup] HOÀN TẤT & ĐÃ LƯU SCENE!</b></color> Đã tích hợp 3 địa điểm, UI HUD, Viewfinder, Victory và Manager cho Photo Hunter.");
            if (interactive)
            {
                EditorUtility.DisplayDialog("Photo Hunter Setup",
                    "Đã thiết lập thành công Mini-Game 2.4 'Thợ Săn Khoảnh Khắc' và lưu scene!\n\n" +
                    "3 Địa điểm:\n" +
                    "1. Cổng Chính Trường (Gate 01)\n" +
                    "2. Sân Trung Tâm (Central Courtyard)\n" +
                    "3. Mặt Tiền Tòa Nhà A (Block A)\n\n" +
                    "Mở Menu [M] chọn Game 4 để bắt đầu trải nghiệm!",
                    "OK");
            }
            return true;
        }

        private static IUHPhotoTarget SetupTarget(
            Transform parent,
            string goName,
            string id,
            string displayName,
            string desc,
            Vector3 worldPos,
            Vector3 vantagePoint,
            Vector3 lookDir,
            float minDist,
            float maxDist,
            Vector3 triggerSize,
            Transform structureRoot,
            List<Vector3> featurePoints)
        {
            Transform tTrans = parent.Find(goName);
            GameObject tGo;
            if (tTrans == null)
            {
                tGo = new GameObject(goName);
                tGo.transform.SetParent(parent, false);
                Undo.RegisterCreatedObjectUndo(tGo, "Create Photo Target " + goName);
            }
            else
            {
                tGo = tTrans.gameObject;
            }

            tGo.transform.position = worldPos;

            var comp = tGo.GetComponent<IUHPhotoTarget>();
            if (comp == null) comp = Undo.AddComponent<IUHPhotoTarget>(tGo);

            var boxCol = tGo.GetComponent<BoxCollider>();
            if (boxCol == null) boxCol = Undo.AddComponent<BoxCollider>(tGo);
            boxCol.isTrigger = true;
            boxCol.size = triggerSize;

            comp.Configure(id, displayName, desc, vantagePoint, lookDir, minDist, maxDist, structureRoot, boxCol);
            comp.SetKeyFeatureOffsets(featurePoints);

            EditorUtility.SetDirty(comp);
            return comp;
        }

        private static Canvas FindOrCreateCanvas()
        {
            var existing = GameObject.Find("IUH_Minigame_Canvas")?.GetComponent<Canvas>();
            if (existing != null) return existing;

            var portalUI = UnityEngine.Object.FindAnyObjectByType<IUHMiniGamePortalUI>(FindObjectsInactive.Include);
            if (portalUI != null)
            {
                var c = portalUI.GetComponentInParent<Canvas>();
                if (c != null) return c;
            }

            var canvasGo = new GameObject("IUH_PhotoHunter_Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Photo Hunter Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static IUHPhotoHunterUI SetupUIHierarchy(Canvas canvas)
        {
            Transform uiRootTrans = canvas.transform.Find(UiRootName);
            GameObject uiRootGo;
            if (uiRootTrans == null)
            {
                uiRootGo = new GameObject(UiRootName, typeof(RectTransform));
                uiRootGo.transform.SetParent(canvas.transform, false);
                Undo.RegisterCreatedObjectUndo(uiRootGo, "Create PhotoHunter UI Root");
            }
            else
            {
                uiRootGo = uiRootTrans.gameObject;
            }

            RectTransform rootRect = uiRootGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var uiComponent = uiRootGo.GetComponent<IUHPhotoHunterUI>();
            if (uiComponent == null) uiComponent = Undo.AddComponent<IUHPhotoHunterUI>(uiRootGo);

            // 1. Briefing Panel
            GameObject briefingPanel = SetupBriefingPanel(uiRootGo.transform);

            // 2. Exploration HUD
            GameObject hudPanel = SetupHUDPanel(uiRootGo.transform);

            // 3. Viewfinder Overlay
            GameObject viewfinderPanel = SetupViewfinderPanel(uiRootGo.transform);

            // 4. Flash Effect Overlay
            GameObject flashOverlayGo = SetupFlashOverlay(uiRootGo.transform);

            // 5. Victory Dialog
            GameObject victoryDialog = SetupVictoryDialog(uiRootGo.transform);

            // Serialize into IUHPhotoHunterUI
            SerializedObject soUI = new SerializedObject(uiComponent);
            soUI.Update();

            soUI.FindProperty("m_ParentCanvas").objectReferenceValue = canvas;
            soUI.FindProperty("m_BriefingPanel").objectReferenceValue = briefingPanel;
            soUI.FindProperty("m_TxtBriefingStatus").objectReferenceValue = briefingPanel.transform.Find("Card/TxtStatus")?.GetComponent<Text>();
            soUI.FindProperty("m_BtnStartMission").objectReferenceValue = briefingPanel.transform.Find("Card/BtnStart")?.GetComponent<Button>();
            soUI.FindProperty("m_BtnStartAndTeleportGate1").objectReferenceValue = briefingPanel.transform.Find("Card/BtnTeleportGate1")?.GetComponent<Button>();
            soUI.FindProperty("m_BtnViewCompletedAlbum").objectReferenceValue = briefingPanel.transform.Find("Card/BtnViewAlbum")?.GetComponent<Button>();
            soUI.FindProperty("m_BtnCloseBriefing").objectReferenceValue = briefingPanel.transform.Find("Card/BtnClose")?.GetComponent<Button>();

            soUI.FindProperty("m_HudPanel").objectReferenceValue = hudPanel;
            soUI.FindProperty("m_TxtProgress").objectReferenceValue = hudPanel.transform.Find("TxtProgress")?.GetComponent<Text>();
            soUI.FindProperty("m_TxtScore").objectReferenceValue = hudPanel.transform.Find("TxtScore")?.GetComponent<Text>();
            soUI.FindProperty("m_TxtTargetList").objectReferenceValue = hudPanel.transform.Find("TxtTargetList")?.GetComponent<Text>();
            soUI.FindProperty("m_BtnOpenViewfinder").objectReferenceValue = hudPanel.transform.Find("BtnOpenCam")?.GetComponent<Button>();
            soUI.FindProperty("m_BtnHudViewAlbum").objectReferenceValue = hudPanel.transform.Find("BtnViewAlbumHUD")?.GetComponent<Button>();
            soUI.FindProperty("m_BtnCancelMission").objectReferenceValue = hudPanel.transform.Find("BtnCancel")?.GetComponent<Button>();

            soUI.FindProperty("m_ViewfinderPanel").objectReferenceValue = viewfinderPanel;
            soUI.FindProperty("m_TxtCameraSpecs").objectReferenceValue = viewfinderPanel.transform.Find("SpecsBar/TxtSpecs")?.GetComponent<Text>();
            soUI.FindProperty("m_TxtToastMessage").objectReferenceValue = viewfinderPanel.transform.Find("TxtToast")?.GetComponent<Text>();
            soUI.FindProperty("m_BtnCapturePhoto").objectReferenceValue = viewfinderPanel.transform.Find("BottomBar/BtnCapture")?.GetComponent<Button>();
            soUI.FindProperty("m_BtnCloseViewfinder").objectReferenceValue = viewfinderPanel.transform.Find("TopBar/BtnExitCam")?.GetComponent<Button>();
            soUI.FindProperty("m_FlashOverlay").objectReferenceValue = flashOverlayGo.GetComponent<Image>();

            soUI.FindProperty("m_VictoryDialog").objectReferenceValue = victoryDialog;
            soUI.FindProperty("m_TxtVictoryTitle").objectReferenceValue = victoryDialog.transform.Find("Card/TxtTitle")?.GetComponent<Text>();
            soUI.FindProperty("m_TxtVictoryScore").objectReferenceValue = victoryDialog.transform.Find("Card/TxtScore")?.GetComponent<Text>();
            soUI.FindProperty("m_BtnRestartMission").objectReferenceValue = victoryDialog.transform.Find("Card/Footer/BtnRestart")?.GetComponent<Button>();
            soUI.FindProperty("m_BtnExitToCampus").objectReferenceValue = victoryDialog.transform.Find("Card/Footer/BtnExit")?.GetComponent<Button>();

            // Setup thumbnail arrays in victory dialog
            Transform gallery = victoryDialog.transform.Find("Card/Gallery");
            if (gallery != null && gallery.childCount >= 3)
            {
                SerializedProperty propThumbs = soUI.FindProperty("m_PhotoThumbnails");
                SerializedProperty propLabels = soUI.FindProperty("m_PhotoLabels");
                propThumbs.arraySize = 3;
                propLabels.arraySize = 3;

                for (int i = 0; i < 3; i++)
                {
                    var slot = gallery.GetChild(i);
                    propThumbs.GetArrayElementAtIndex(i).objectReferenceValue = slot.Find("Thumb")?.GetComponent<RawImage>();
                    propLabels.GetArrayElementAtIndex(i).objectReferenceValue = slot.Find("TxtLabel")?.GetComponent<Text>();
                }
            }

            soUI.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiComponent);
            return uiComponent;
        }

        // ================= UI BUILDERS =================

        private static GameObject SetupBriefingPanel(Transform parent)
        {
            Transform panelTrans = parent.Find("BriefingPanel");
            GameObject panel;
            Transform cardTrans;

            if (panelTrans == null)
            {
                panel = CreateUIElement("BriefingPanel", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var imgBg = panel.AddComponent<Image>();
                imgBg.color = new Color(0.03f, 0.05f, 0.10f, 0.88f);

                // Card
                GameObject card = CreateUIElement("Card", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-380, -280), new Vector2(380, 280));
                var cardImg = card.AddComponent<Image>();
                cardImg.color = new Color(0.08f, 0.12f, 0.22f, 0.98f);
                var outline = card.AddComponent<Outline>();
                outline.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.6f);
                outline.effectDistance = new Vector2(2, -2);
                cardTrans = card.transform;
            }
            else
            {
                panel = panelTrans.gameObject;
                cardTrans = panelTrans.Find("Card") ?? panelTrans;
            }

            // Title
            if (cardTrans.Find("TxtTitle") == null)
            {
                GameObject title = CreateUIElement("TxtTitle", cardTrans, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -55), new Vector2(-20, -15));
                var txtTitle = title.AddComponent<Text>();
                txtTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtTitle.fontSize = 22;
                txtTitle.fontStyle = FontStyle.Bold;
                txtTitle.alignment = TextAnchor.MiddleCenter;
                txtTitle.color = new Color(0.22f, 0.74f, 0.97f, 1.0f);
                txtTitle.text = "📷 THỢ SĂN KHOẢNH KHẮC — PHOTO HUNTER";
            }

            // Status notice
            if (cardTrans.Find("TxtStatus") == null)
            {
                GameObject statusGo = CreateUIElement("TxtStatus", cardTrans, new Vector2(0, 1), new Vector2(1, 1), new Vector2(25, -95), new Vector2(-25, -60));
                var txtStatus = statusGo.AddComponent<Text>();
                txtStatus.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtStatus.fontSize = 13;
                txtStatus.alignment = TextAnchor.MiddleCenter;
                txtStatus.color = new Color(0.98f, 0.75f, 0.14f);
                txtStatus.text = "Chào mừng bạn đến với thử thách khám phá khuôn viên trường!";
            }

            // Description
            if (cardTrans.Find("TxtDesc") == null)
            {
                GameObject desc = CreateUIElement("TxtDesc", cardTrans, new Vector2(0, 0), new Vector2(1, 1), new Vector2(30, 80), new Vector2(-30, -100));
                var txtDesc = desc.AddComponent<Text>();
                txtDesc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtDesc.fontSize = 14;
                txtDesc.lineSpacing = 1.25f;
                txtDesc.color = new Color(0.95f, 0.96f, 0.98f, 1.0f);
                txtDesc.text =
                    "<b>MỤC TIÊU NHIỆM VỤ:</b>\n" +
                    "Khám phá khuôn viên <b>Đại học Công nghiệp TP. Hồ Chí Minh</b> và chụp đủ <b>3 địa điểm danh tiếng</b>:\n" +
                    "  1. <color=#38bdf8>Cổng Chính Trường (Gate 01)</color> — Đường Nguyễn Văn Bảo\n" +
                    "  2. <color=#38bdf8>Sân Trung Tâm (Central Courtyard)</color> — Khuôn viên sân trường thoáng đãng\n" +
                    "  3. <color=#38bdf8>Mặt Tiền Tòa Nhà A (Block A)</color> — Sảnh chính nhà hiệu bộ\n\n" +
                    "<b>HƯỚNG DẪN THAO TÁC:</b>\n" +
                    "• Nhấn phím <b>[C]</b> hoặc bấm nút <b>[Mở Máy Ảnh]</b> trên HUD để vào ống ngắm.\n" +
                    "• Rê chuột căn chỉnh góc máy sao cho công trình nằm rõ trong khung hình.\n" +
                    "• Nhấn <b>[Space]</b> / Click chuột để chụp ảnh.\n" +
                    "• Mỗi địa điểm hoàn thành lần đầu nhận ngay <color=#fbbf24><b>+100 điểm</b></color>.\n" +
                    "• Nhấn <b>[Q]</b> hoặc <b>[ESC]</b> để thoát ống ngắm quay về di chuyển tự do.";
            }

            // Action Buttons
            if (cardTrans.Find("BtnStart") == null)
                CreateButton("BtnStart", cardTrans, new Vector2(0.04f, 0.04f), new Vector2(0.38f, 0.15f), "⚡ Bắt Đầu Săn Ảnh", new Color(0.06f, 0.58f, 0.85f));
            if (cardTrans.Find("BtnTeleportGate1") == null)
                CreateButton("BtnTeleportGate1", cardTrans, new Vector2(0.40f, 0.04f), new Vector2(0.68f, 0.15f), "📍 Đến Cổng 1", new Color(0.09f, 0.65f, 0.45f));
            if (cardTrans.Find("BtnViewAlbum") == null)
                CreateButton("BtnViewAlbum", cardTrans, new Vector2(0.70f, 0.04f), new Vector2(0.88f, 0.15f), "📖 Xem Album", new Color(0.85f, 0.55f, 0.15f));
            if (cardTrans.Find("BtnClose") == null)
                CreateButton("BtnClose", cardTrans, new Vector2(0.90f, 0.04f), new Vector2(0.96f, 0.15f), "✕", new Color(0.80f, 0.20f, 0.20f));

            panel.SetActive(false);
            return panel;
        }

        private static GameObject SetupHUDPanel(Transform parent)
        {
            Transform panelTrans = parent.Find("PhotoHunterHUD");
            GameObject hud;

            if (panelTrans == null)
            {
                hud = CreateUIElement("PhotoHunterHUD", parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(25, -240), new Vector2(350, -20));
                var img = hud.AddComponent<Image>();
                img.color = new Color(0.06f, 0.10f, 0.18f, 0.88f);
                var outline = hud.AddComponent<Outline>();
                outline.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.45f);
            }
            else
            {
                hud = panelTrans.gameObject;
            }

            // Title
            if (hud.transform.Find("TxtTitle") == null)
            {
                GameObject title = CreateUIElement("TxtTitle", hud.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(15, -32), new Vector2(-15, -5));
                var txtTitle = title.AddComponent<Text>();
                txtTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtTitle.fontSize = 15;
                txtTitle.fontStyle = FontStyle.Bold;
                txtTitle.color = new Color(0.22f, 0.74f, 0.97f, 1.0f);
                txtTitle.text = "📷 THỢ SĂN KHOẢNH KHẮC";
            }

            // Progress & Score
            if (hud.transform.Find("TxtProgress") == null)
            {
                GameObject progress = CreateUIElement("TxtProgress", hud.transform, new Vector2(0, 1), new Vector2(0.6f, 1), new Vector2(15, -55), new Vector2(0, -32));
                var txtProg = progress.AddComponent<Text>();
                txtProg.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtProg.fontSize = 13;
                txtProg.fontStyle = FontStyle.Bold;
                txtProg.color = Color.white;
                txtProg.text = "Tiến độ: 0/3 địa điểm";
            }

            if (hud.transform.Find("TxtScore") == null)
            {
                GameObject score = CreateUIElement("TxtScore", hud.transform, new Vector2(0.6f, 1), new Vector2(1, 1), new Vector2(0, -55), new Vector2(-15, -32));
                var txtScore = score.AddComponent<Text>();
                txtScore.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtScore.fontSize = 13;
                txtScore.fontStyle = FontStyle.Bold;
                txtScore.alignment = TextAnchor.MiddleRight;
                txtScore.color = new Color(0.98f, 0.75f, 0.14f);
                txtScore.text = "Điểm: 0 đ";
            }

            // Target List
            if (hud.transform.Find("TxtTargetList") == null)
            {
                GameObject listGo = CreateUIElement("TxtTargetList", hud.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(15, 52), new Vector2(-15, -60));
                var txtList = listGo.AddComponent<Text>();
                txtList.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtList.fontSize = 12;
                txtList.lineSpacing = 1.25f;
                txtList.color = new Color(0.90f, 0.92f, 0.96f);
                txtList.text = "1. Cổng Chính Trường [○]\n2. Sân Trung Tâm [○]\n3. Mặt Tiền Tòa Nhà A [○]";
            }

            // Action Buttons
            if (hud.transform.Find("BtnOpenCam") == null)
                CreateButton("BtnOpenCam", hud.transform, new Vector2(0.04f, 0.06f), new Vector2(0.62f, 0.22f), "📸 Mở Máy Ảnh [C]", new Color(0.06f, 0.58f, 0.85f));
            if (hud.transform.Find("BtnViewAlbumHUD") == null)
                CreateButton("BtnViewAlbumHUD", hud.transform, new Vector2(0.64f, 0.06f), new Vector2(0.82f, 0.22f), "📖", new Color(0.85f, 0.55f, 0.15f));
            if (hud.transform.Find("BtnCancel") == null)
                CreateButton("BtnCancel", hud.transform, new Vector2(0.84f, 0.06f), new Vector2(0.96f, 0.22f), "✕", new Color(0.65f, 0.20f, 0.20f));

            hud.SetActive(false);
            return hud;
        }

        private static GameObject SetupViewfinderPanel(Transform parent)
        {
            Transform panelTrans = parent.Find("ViewfinderOverlay");
            if (panelTrans != null) return panelTrans.gameObject;

            GameObject vf = CreateUIElement("ViewfinderOverlay", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Reticle Center Box
            GameObject reticle = CreateUIElement("Reticle", vf.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-120, -80), new Vector2(120, 80));
            var reticleImg = reticle.AddComponent<Image>();
            reticleImg.color = new Color(1f, 1f, 1f, 0.04f);
            var reticleOutline = reticle.AddComponent<Outline>();
            reticleOutline.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.75f);
            reticleOutline.effectDistance = new Vector2(2, -2);

            // Center crosshair dot
            GameObject dot = CreateUIElement("Dot", reticle.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-4, -4), new Vector2(4, 4));
            var dotImg = dot.AddComponent<Image>();
            dotImg.color = new Color(0.22f, 0.74f, 0.97f, 0.9f);

            // Top Bar
            GameObject topBar = CreateUIElement("TopBar", vf.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), Vector2.zero);
            var topBarImg = topBar.AddComponent<Image>();
            topBarImg.color = new Color(0.03f, 0.05f, 0.10f, 0.70f);

            GameObject txtMode = CreateUIElement("TxtMode", topBar.transform, new Vector2(0, 0), new Vector2(0.4f, 1), new Vector2(30, 0), Vector2.zero);
            var tMode = txtMode.AddComponent<Text>();
            tMode.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tMode.fontSize = 15;
            tMode.fontStyle = FontStyle.Bold;
            tMode.alignment = TextAnchor.MiddleLeft;
            tMode.color = new Color(0.22f, 0.74f, 0.97f);
            tMode.text = "● REC  IUH-OPTICAL 35mm";

            CreateButton("BtnExitCam", topBar.transform, new Vector2(0.85f, 0.15f), new Vector2(0.98f, 0.85f), "✕ Thoát Ngắm [Q/ESC]", new Color(0.60f, 0.15f, 0.15f));

            // Specs Bar
            GameObject specsBar = CreateUIElement("SpecsBar", vf.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -95), new Vector2(0, -60));
            GameObject txtSpecs = CreateUIElement("TxtSpecs", specsBar.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var tSpecs = txtSpecs.AddComponent<Text>();
            tSpecs.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tSpecs.fontSize = 13;
            tSpecs.alignment = TextAnchor.MiddleCenter;
            tSpecs.color = new Color(0.94f, 0.96f, 0.98f, 0.85f);
            tSpecs.text = "ISO 100  |  f/2.8  |  1/500s  |  AF-C ACTIVE  |  35mm";

            // Toast Message Text
            GameObject toastGo = CreateUIElement("TxtToast", vf.transform, new Vector2(0.15f, 0.20f), new Vector2(0.85f, 0.32f), Vector2.zero, Vector2.zero);
            var txtToast = toastGo.AddComponent<Text>();
            txtToast.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtToast.fontSize = 17;
            txtToast.fontStyle = FontStyle.Bold;
            txtToast.alignment = TextAnchor.MiddleCenter;
            txtToast.color = Color.white;
            txtToast.text = "";
            var toastOutline = toastGo.AddComponent<Outline>();
            toastOutline.effectColor = Color.black;
            toastOutline.effectDistance = new Vector2(1.5f, -1.5f);
            toastGo.SetActive(false);

            // Bottom Bar
            GameObject bottomBar = CreateUIElement("BottomBar", vf.transform, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 90));
            var btmImg = bottomBar.AddComponent<Image>();
            btmImg.color = new Color(0.03f, 0.05f, 0.10f, 0.70f);

            CreateButton("BtnCapture", bottomBar.transform, new Vector2(0.38f, 0.15f), new Vector2(0.62f, 0.85f), "📸 CHỤP ẢNH [Space/Click]", new Color(0.06f, 0.58f, 0.85f));

            vf.SetActive(false);
            return vf;
        }

        private static GameObject SetupFlashOverlay(Transform parent)
        {
            Transform flashTrans = parent.Find("FlashOverlay");
            if (flashTrans != null) return flashTrans.gameObject;

            GameObject flash = CreateUIElement("FlashOverlay", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = flash.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = false;
            flash.SetActive(false);
            return flash;
        }

        private static GameObject SetupVictoryDialog(Transform parent)
        {
            Transform dialogTrans = parent.Find("PhotoVictoryDialog");
            if (dialogTrans != null) return dialogTrans.gameObject;

            GameObject dialog = CreateUIElement("PhotoVictoryDialog", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImg = dialog.AddComponent<Image>();
            bgImg.color = new Color(0.03f, 0.05f, 0.10f, 0.92f);

            // Card
            GameObject card = CreateUIElement("Card", dialog.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-460, -300), new Vector2(460, 300));
            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.08f, 0.12f, 0.22f, 0.98f);
            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.06f, 0.72f, 0.51f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            // Title
            GameObject title = CreateUIElement("TxtTitle", card.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -55), new Vector2(-20, -15));
            var txtTitle = title.AddComponent<Text>();
            txtTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtTitle.fontSize = 22;
            txtTitle.fontStyle = FontStyle.Bold;
            txtTitle.alignment = TextAnchor.MiddleCenter;
            txtTitle.color = new Color(0.06f, 0.72f, 0.51f);
            txtTitle.text = "🎉 XUẤT SẮC! BỘ ẢNH KHUÔN VIÊN HOÀN THÀNH";

            // Score
            GameObject score = CreateUIElement("TxtScore", card.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -90), new Vector2(-20, -55));
            var txtScore = score.AddComponent<Text>();
            txtScore.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtScore.fontSize = 16;
            txtScore.fontStyle = FontStyle.Bold;
            txtScore.alignment = TextAnchor.MiddleCenter;
            txtScore.color = new Color(0.98f, 0.75f, 0.14f);
            txtScore.text = "TỔNG ĐIỂM: 300 / 300 ĐIỂM (Hoàn thành 3/3 địa điểm)";

            // Gallery Slots (3 slots)
            GameObject gallery = CreateUIElement("Gallery", card.transform, new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.82f), Vector2.zero, Vector2.zero);
            var glg = gallery.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(265, 170);
            glg.spacing = new Vector2(15, 10);
            glg.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < 3; i++)
            {
                GameObject slot = CreateUIElement($"Slot_{i}", gallery.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var slotBg = slot.AddComponent<Image>();
                slotBg.color = new Color(0.04f, 0.07f, 0.14f, 0.9f);

                GameObject rawImgGo = CreateUIElement("Thumb", slot.transform, new Vector2(0.02f, 0.25f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
                var rawImg = rawImgGo.AddComponent<RawImage>();
                rawImg.color = Color.white;

                GameObject labelGo = CreateUIElement("TxtLabel", slot.transform, new Vector2(0, 0), new Vector2(1, 0.25f), new Vector2(4, 2), new Vector2(-4, -2));
                var txtL = labelGo.AddComponent<Text>();
                txtL.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtL.fontSize = 11;
                txtL.alignment = TextAnchor.MiddleCenter;
                txtL.color = new Color(0.90f, 0.92f, 0.96f);
                txtL.text = $"Địa điểm {i + 1}";
            }

            // Footer
            GameObject footer = CreateUIElement("Footer", card.transform, new Vector2(0, 0), new Vector2(1, 0.20f), Vector2.zero, Vector2.zero);
            CreateButton("BtnRestart", footer.transform, new Vector2(0.15f, 0.25f), new Vector2(0.48f, 0.85f), "🔄 Chơi Lại Thử Thách", new Color(0.20f, 0.50f, 0.80f));
            CreateButton("BtnExit", footer.transform, new Vector2(0.52f, 0.25f), new Vector2(0.85f, 0.85f), "🚶 Trở Về Khám Phá Campus", new Color(0.06f, 0.65f, 0.45f));

            dialog.SetActive(false);
            return dialog;
        }

        private static GameObject CreateUIElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string label, Color color)
        {
            var btnGo = CreateUIElement(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var img = btnGo.AddComponent<Image>();
            img.color = color;
            var btn = btnGo.AddComponent<Button>();

            var colors = btn.colors;
            colors.highlightedColor = color * 1.15f;
            colors.pressedColor = color * 0.85f;
            btn.colors = colors;

            var txtGo = CreateUIElement("Text", btnGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 14;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = label;

            return btnGo;
        }

        private static void UpdatePortalMenuCard()
        {
            var portalUI = UnityEngine.Object.FindAnyObjectByType<IUHMiniGamePortalUI>(FindObjectsInactive.Include);
            if (portalUI == null) return;

            var card4 = portalUI.transform.Find("PortalMenuPanel/CardsArea/Card_3_Game4");
            if (card4 != null)
            {
                var txtDesc = card4.Find("Desc")?.GetComponent<Text>();
                if (txtDesc != null)
                {
                    txtDesc.text = "Săn ảnh 3 địa điểm danh tiếng campus: Cổng 1, Sân trung tâm & Nhà A. Xác thực góc ngắm thực tế.";
                }

                var btnText = card4.Find("BtnBar/ActionBtn/Text")?.GetComponent<Text>();
                if (btnText != null)
                {
                    btnText.text = "⚡ Chơi Ngay";
                }
            }
        }
    }
}
#endif
