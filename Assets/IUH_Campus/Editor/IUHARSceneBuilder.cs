#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;
using IUHCampus.Guardian.AR;
using IUHCampus.Guardian.Match3;
using IUHCampus.Minigames.GreenCampus;

namespace IUHCampus.Editor
{
    public static class IUHARSceneBuilder
    {
        private const string AR_SCENE_PATH = "Assets/IUH_Campus/Scenes/IUH_Campus_AR_Encounter.unity";
        private const string MONSTER_MODEL_PATH = "Assets/IUH_Campus/Models/Monsters/BlueDemon.obj";
        private const string MONSTER_MAT_PATH = "Assets/IUH_Campus/Models/Monsters/M_Monster_Atlas.mat";

        [MenuItem("IUH Campus/AR/Build AR iPhone Encounter Scene", false, 160)]
        public static void BuildAREncounterScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera & Lighting
            GameObject camObj = new GameObject("AR Main Camera", typeof(Camera), typeof(AudioListener), typeof(IUHCameraShake));
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 1.4f, -2.5f);
            camObj.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            var cam = camObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            GameObject dirLight = new GameObject("Directional Light", typeof(Light));
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var dl = dirLight.GetComponent<Light>();
            dl.type = LightType.Directional;
            dl.intensity = 1.2f;
            dl.color = new Color(1f, 0.96f, 0.9f);

            // 2. Editor Simulated Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Editor_Simulated_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            Material floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Main_Stone.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Asphalt_Road.mat");
            if (floorMat != null) floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 3. Concentric Hologram Reticle
            var reticlePack = CreateHologramReticle(null);
            GameObject reticle = reticlePack.reticleRoot;

            // 4. AR Canvas & UI
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            GameObject canvasGO = new GameObject("AR_Encounter_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var cComp = canvasGO.GetComponent<Canvas>();
            cComp.renderMode = RenderMode.ScreenSpaceOverlay;
            var cs = canvasGO.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920); // Portrait for mobile AR
            cs.matchWidthOrHeight = 0.5f;

            // Header instruction banner
            GameObject bannerObj = new GameObject("HeaderBanner", typeof(RectTransform), typeof(Image));
            bannerObj.transform.SetParent(canvasGO.transform, false);
            var bnRt = bannerObj.GetComponent<RectTransform>();
            bnRt.anchorMin = new Vector2(0.5f, 0.93f);
            bnRt.anchorMax = new Vector2(0.5f, 0.93f);
            bnRt.sizeDelta = new Vector2(900f, 80f);
            bannerObj.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.12f, 0.88f);

            GameObject bannerTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            bannerTxtGo.transform.SetParent(bannerObj.transform, false);
            var bnTxtRt = bannerTxtGo.GetComponent<RectTransform>();
            bnTxtRt.anchorMin = Vector2.zero; bnTxtRt.anchorMax = Vector2.one; bnTxtRt.sizeDelta = Vector2.zero;
            var bnTxt = bannerTxtGo.GetComponent<Text>();
            bnTxt.font = font;
            bnTxt.fontSize = 28;
            bnTxt.fontStyle = FontStyle.Bold;
            bnTxt.alignment = TextAnchor.MiddleCenter;
            bnTxt.color = new Color(0.35f, 0.95f, 1f);
            bnTxt.text = "📱 <b>HÃY QUÉT CAMERA VÀO SÀN NHÀ / MẶT ĐẤT</b>";

            // 5. Corrupted Object Spawner Root
            GameObject spawnerRoot = new GameObject("AR_CORRUPTED_ENCOUNTER");
            spawnerRoot.transform.position = new Vector3(0f, 0f, 1.8f);

            // 3 Loại đồ vật bị nhiễm độc
            GameObject backpack = CreateCorruptedBackpack(spawnerRoot.transform);
            GameObject laptop = CreateCorruptedLaptop(spawnerRoot.transform);
            GameObject textbook = CreateCorruptedTextbook(spawnerRoot.transform);

            // Dark Purple Smoke & Glitch Pixels
            var smokePS = CreatePurpleSmoke(spawnerRoot.transform);
            var glitchPS = CreateGlitchPixels(spawnerRoot.transform);

            // Point Light Aura
            GameObject auraLightGO = new GameObject("PurpleAuraLight", typeof(Light));
            auraLightGO.transform.SetParent(spawnerRoot.transform, false);
            auraLightGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var auraLight = auraLightGO.GetComponent<Light>();
            auraLight.type = LightType.Point;
            auraLight.color = new Color(0.8f, 0.15f, 1f);
            auraLight.intensity = 4.0f;
            auraLight.range = 5.0f;

            // 3D Monster Instance
            var monsterPack = CreateCyberDemonMonster(spawnerRoot.transform, font);

            // Add Audio and Guardian Inventory under Canvas
            new GameObject("IUH_Combat_Audio", typeof(IUHCombatAudio)).transform.SetParent(canvasGO.transform, false);
            new GameObject("IUH_Guardian_Inventory", typeof(IUHGuardianInventory)).transform.SetParent(canvasGO.transform, false);

            // Wire Reticle
            var reticleComp = camObj.AddComponent<IUHARPlaneReticle>();
            var encComp = spawnerRoot.AddComponent<IUHARCorruptedObjectEncounter>();

            var soReticle = new SerializedObject(reticleComp);
            soReticle.FindProperty("m_ReticleObject").objectReferenceValue = reticle;
            soReticle.FindProperty("m_OuterHoloRing").objectReferenceValue = reticlePack.outerRing;
            soReticle.FindProperty("m_InnerHoloRing").objectReferenceValue = reticlePack.innerRing;
            soReticle.FindProperty("m_StatusInstructionText").objectReferenceValue = bnTxt;
            soReticle.FindProperty("m_EncounterSpawner").objectReferenceValue = encComp;
            soReticle.ApplyModifiedProperties();

            // Wire Encounter
            var soEnc = new SerializedObject(encComp);
            soEnc.FindProperty("m_CorruptedBackpack").objectReferenceValue = backpack;
            soEnc.FindProperty("m_CorruptedLaptop").objectReferenceValue = laptop;
            soEnc.FindProperty("m_CorruptedTextbook").objectReferenceValue = textbook;
            soEnc.FindProperty("m_MonsterObject").objectReferenceValue = monsterPack.monsterRoot;
            soEnc.FindProperty("m_PurpleSmokeParticles").objectReferenceValue = smokePS;
            soEnc.FindProperty("m_GlitchPixelParticles").objectReferenceValue = glitchPS;
            soEnc.FindProperty("m_PurpleAuraLight").objectReferenceValue = auraLight;
            soEnc.FindProperty("m_ReticleObject").objectReferenceValue = reticle;
            soEnc.FindProperty("m_TapPromptCanvas").objectReferenceValue = monsterPack.tapPrompt;
            soEnc.ApplyModifiedProperties();

            // Save Scene
            EditorSceneManager.SaveScene(scene, AR_SCENE_PATH);
            Debug.Log("<color=#00E5FF>[IUH AR]</color> Đã dựng thành công Scene AR iPhone Encounter tại: " + AR_SCENE_PATH);

            // Add to Build Settings
            var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in buildScenes) if (s.path == AR_SCENE_PATH) { found = true; break; }
            if (!found)
            {
                buildScenes.Insert(0, new EditorBuildSettingsScene(AR_SCENE_PATH, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }
        }

        [MenuItem("IUH Campus/AR/Setup Corrupted Encounters in Main Campus Scene", false, 161)]
        public static void SetupAllCorruptedEncountersInCampusScene()
        {
            const string MAIN_CAMPUS_PATH = "Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity";
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != MAIN_CAMPUS_PATH)
            {
                activeScene = EditorSceneManager.OpenScene(MAIN_CAMPUS_PATH, OpenSceneMode.Single);
            }

            Debug.Log("<color=#00E5FF>[IUH AR]</color> Khởi tạo 3 Điểm Dị Tượng Nhiễm Độc trên sân trường: " + activeScene.path);

            // Dọn dẹp các object cũ nếu có
            var oldBackpack = GameObject.Find("CORRUPTED_OBJECT_STUDENT_BACKPACK");
            if (oldBackpack != null) Object.DestroyImmediate(oldBackpack);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Điểm 1: Cạnh Đài Phun Nước / Sân chính (Balo Sinh Viên IUH)
            CreateSingleCorruptedEncounter(
                "CORRUPTED_ENCOUNTER_FOUNTAIN",
                new Vector3(4.0f, 0.05f, -28.0f),
                CorruptedObjectType.Backpack,
                "Tiểu Quỷ Dữ Liệu - Balo IUH",
                800,
                "CAMPUS_CORRUPTED_FOUNTAIN",
                font
            );

            // Điểm 2: Dưới Gốc Cây / Khuôn Viên Cây Xanh (Sách Giáo Trình Thư Viện)
            CreateSingleCorruptedEncounter(
                "CORRUPTED_ENCOUNTER_GARDEN",
                new Vector3(-16.0f, 0.05f, -14.0f),
                CorruptedObjectType.LibraryTextbook,
                "Quỷ Dữ Liệu - Sách Giáo Trình",
                1000,
                "CAMPUS_CORRUPTED_GARDEN",
                font
            );

            // Điểm 3: Sảnh Tòa Nhà E / Trước Cột Cờ (Laptop Đồ Án Kỹ Thuật)
            CreateSingleCorruptedEncounter(
                "CORRUPTED_ENCOUNTER_BUILDING_E",
                new Vector3(-6.0f, 0.05f, 6.0f),
                CorruptedObjectType.EngineeringLaptop,
                "Chúa Tể Mã Độc - Laptop Đồ Án",
                1200,
                "CAMPUS_CORRUPTED_BUILDING_E",
                font
            );

            // Điểm 4: Khu Vực Căn Tin Sinh Viên (Balo Tiếp Tế)
            CreateSingleCorruptedEncounter(
                "CORRUPTED_ENCOUNTER_CANTEEN",
                new Vector3(-22.0f, 0.05f, -36.0f),
                CorruptedObjectType.Backpack,
                "Mã Độc Tiếp Tế - Balo Căn Tin",
                900,
                "CAMPUS_CORRUPTED_CANTEEN",
                font
            );

            // Điểm 5: Sân Thể Thao Đa Năng (Laptop Điều Khiển Sân Đấu)
            CreateSingleCorruptedEncounter(
                "CORRUPTED_ENCOUNTER_SPORTS",
                new Vector3(20.0f, 0.05f, -10.0f),
                CorruptedObjectType.EngineeringLaptop,
                "Quỷ Mạng Lưới - Laptop Thể Thao",
                1100,
                "CAMPUS_CORRUPTED_SPORTS",
                font
            );

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#00FF88>[IUH AR] ĐÃ CÀI ĐẶT THÀNH CÔNG 5 ĐIỂM DỊ TƯỢNG VÀO SÂN TRƯỜNG!</color>");
        }

        public static GameObject CreateSingleCorruptedEncounter(
            string name, 
            Vector3 pos, 
            CorruptedObjectType objType, 
            string monsterName, 
            int monsterHp, 
            string milestoneId, 
            Font font)
        {
            var old = GameObject.Find(name);
            if (old != null) Object.DestroyImmediate(old);

            GameObject root = new GameObject(name);
            root.transform.position = pos;

            GameObject backpack = CreateCorruptedBackpack(root.transform);
            GameObject laptop = CreateCorruptedLaptop(root.transform);
            GameObject textbook = CreateCorruptedTextbook(root.transform);

            var smokePS = CreatePurpleSmoke(root.transform);
            var glitchPS = CreateGlitchPixels(root.transform);

            GameObject auraLightGO = new GameObject("PurpleAuraLight", typeof(Light));
            auraLightGO.transform.SetParent(root.transform, false);
            auraLightGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var auraLight = auraLightGO.GetComponent<Light>();
            auraLight.type = LightType.Point;
            auraLight.color = new Color(0.85f, 0.15f, 1f);
            auraLight.intensity = 5.5f;
            auraLight.range = 8.0f;

            // Cột sáng tím neon cao vút lên trời (Beacon Pillar) để dễ thấy từ xa & từ Flycam Drone
            GameObject beaconBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beaconBeam.name = "Beacon_Pillar_Beam";
            beaconBeam.transform.SetParent(root.transform, false);
            beaconBeam.transform.localPosition = new Vector3(0f, 6.0f, 0f);
            beaconBeam.transform.localScale = new Vector3(0.4f, 6.0f, 0.4f);
            Object.DestroyImmediate(beaconBeam.GetComponent<Collider>());
            var bRend = beaconBeam.GetComponent<Renderer>();
            var beamMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/CombatArena/ArenaGlowPurple.mat");
            if (beamMat != null) bRend.sharedMaterial = beamMat;

            var reticlePack = CreateHologramReticle(root.transform);
            var monsterPack = CreateCyberDemonMonster(root.transform, font);

            // Add IUHMonsterEncounter component
            var monsterEnc = root.AddComponent<IUHMonsterEncounter>();
            var soMon = new SerializedObject(monsterEnc);
            soMon.FindProperty("m_MonsterName").stringValue = monsterName;
            soMon.FindProperty("m_MilestoneId").stringValue = milestoneId;
            soMon.FindProperty("m_MonsterHp").intValue = monsterHp;
            soMon.FindProperty("m_RewardGold").intValue = 100;
            soMon.FindProperty("m_TriggerRadius").floatValue = 4.8f;
            soMon.ApplyModifiedProperties();

            // Wire IUHARCorruptedObjectEncounter
            var encComp = root.AddComponent<IUHARCorruptedObjectEncounter>();
            var soEnc = new SerializedObject(encComp);
            soEnc.FindProperty("m_AutoPlaceOnStart").boolValue = true;
            soEnc.FindProperty("m_ConfiguredType").enumValueIndex = (int)objType;
            soEnc.FindProperty("m_MonsterName").stringValue = monsterName;
            soEnc.FindProperty("m_MonsterHp").intValue = monsterHp;
            soEnc.FindProperty("m_CorruptedBackpack").objectReferenceValue = backpack;
            soEnc.FindProperty("m_CorruptedLaptop").objectReferenceValue = laptop;
            soEnc.FindProperty("m_CorruptedTextbook").objectReferenceValue = textbook;
            soEnc.FindProperty("m_MonsterObject").objectReferenceValue = monsterPack.monsterRoot;
            soEnc.FindProperty("m_PurpleSmokeParticles").objectReferenceValue = smokePS;
            soEnc.FindProperty("m_GlitchPixelParticles").objectReferenceValue = glitchPS;
            soEnc.FindProperty("m_PurpleAuraLight").objectReferenceValue = auraLight;
            soEnc.FindProperty("m_ReticleObject").objectReferenceValue = reticlePack.reticleRoot;
            soEnc.FindProperty("m_TapPromptCanvas").objectReferenceValue = monsterPack.tapPrompt;
            soEnc.ApplyModifiedProperties();

            encComp.PlaceCorruptedObject(pos, Quaternion.identity);
            return root;
        }

        [MenuItem("IUH Campus/UI/Setup Cyberpunk Mobile HUD", false, 170)]
        public static void SetupMobileControlsMenu()
        {
            SetupMobileControlsInScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        public static void SetupMobileControlsInScene()
        {
            // Dọn dẹp các đối tượng UI cũ nếu có
            string[] cleanupNames = new string[] { "UI_TouchScreenInput", "IUH_MOBILE_HUD", "IUH_MOBILE_INTERACT_BTN", "IUH_MOBILE_CONTROLS_CANVAS" };
            foreach (var name in cleanupNames)
            {
                var oldObj = GameObject.Find(name);
                if (oldObj != null) Object.DestroyImmediate(oldObj);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Load Sprites
            Sprite spBtnInteract = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_btn_interact.png");
            Sprite spBtnRun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_btn_run.png");
            Sprite spBtnJump = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_btn_jump.png");
            Sprite spBtnFireworks = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_btn_fireworks.png");
            Sprite spBtnVehicle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_btn_vehicle.png");
            Sprite spBtnTop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_btn_top.png");
            Sprite spBtnFire = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_btn_fire.png") ?? spBtnInteract;

            Sprite spIconHand = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_hand_tap.png");
            Sprite spIconGunFire = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_gun_fire.png");
            Sprite spIconRun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_run.png");
            Sprite spIconJump = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_jump.png");
            Sprite spIconFireworks = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_fireworks.png");
            Sprite spIconVehicle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_vehicle.png");
            Sprite spIconBackpack = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_backpack.png");
            Sprite spIconUpgrade = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_upgrade.png");
            Sprite spIconSettings = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_settings.png");
            Sprite spIconShield = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/icon_shield_health.png");

            Sprite spJoyBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_joystick_base.png");
            Sprite spJoyHandle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_joystick_handle.png");
            Sprite spPlayerStatus = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_player_status_panel.png");
            Sprite spAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/student_avatar_portrait.png");
            Sprite spHealthBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_health_bar_bg.png");
            Sprite spHealthFill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/IUH_Campus/UI/Textures/hud_health_bar_fill.png");

            // 1. Tạo Canvas điều khiển chuyên biệt cho Màn hình ngang (Landscape Mobile Controls Canvas)
            GameObject canvasGO = new GameObject("IUH_MOBILE_CONTROLS_CANVAS", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90; // Nổi lên trên môi trường 3D

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // Tỉ lệ chuẩn màn hình ngang 16:9
            scaler.matchWidthOrHeight = 0.5f;

            var mobileCtrl = canvasGO.AddComponent<IUHCampus.UI.IUHMobileControlsUI>();

            // 2. Vùng vuốt xoay camera 360 độ (Touch Look Zone bao phủ nửa phải màn hình)
            GameObject lookZoneGO = new GameObject("TouchLookZone", typeof(RectTransform), typeof(Image));
            lookZoneGO.transform.SetParent(canvasGO.transform, false);
            var lookRt = lookZoneGO.GetComponent<RectTransform>();
            lookRt.anchorMin = new Vector2(0.35f, 0f);
            lookRt.anchorMax = new Vector2(1f, 1f);
            lookRt.sizeDelta = Vector2.zero;
            var lookImg = lookZoneGO.GetComponent<Image>();
            lookImg.color = new Color(0f, 0f, 0f, 0f); // Trong suốt không che tầm nhìn
            lookImg.raycastTarget = true;

            // ================= 3. HUD_TOP ================= //
            GameObject hudTopGO = new GameObject("HUD_Top", typeof(RectTransform));
            hudTopGO.transform.SetParent(canvasGO.transform, false);
            var topRt = hudTopGO.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0f, 120f);

            // 3.A. HUD_PlayerStatus (Góc trên trái)
            GameObject statusGO = new GameObject("HUD_PlayerStatus", typeof(RectTransform), typeof(Image));
            statusGO.transform.SetParent(hudTopGO.transform, false);
            var statusRt = statusGO.GetComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0f, 1f); statusRt.anchorMax = new Vector2(0f, 1f);
            statusRt.pivot = new Vector2(0f, 1f);
            statusRt.anchoredPosition = new Vector2(25f, -20f);
            statusRt.sizeDelta = new Vector2(360f, 95f);
            var statusImg = statusGO.GetComponent<Image>();
            statusImg.sprite = spPlayerStatus;
            statusImg.color = Color.white;
            statusImg.raycastTarget = false;

            // Avatar Frame & Image
            GameObject avatarFrameGO = new GameObject("Avatar_Frame", typeof(RectTransform), typeof(Image));
            avatarFrameGO.transform.SetParent(statusGO.transform, false);
            var afRt = avatarFrameGO.GetComponent<RectTransform>();
            afRt.anchorMin = new Vector2(0f, 0.5f); afRt.anchorMax = new Vector2(0f, 0.5f);
            afRt.pivot = new Vector2(0.5f, 0.5f);
            afRt.anchoredPosition = new Vector2(50f, 0f);
            afRt.sizeDelta = new Vector2(76f, 76f);
            var afImg = avatarFrameGO.GetComponent<Image>();
            afImg.sprite = spAvatar;
            afImg.raycastTarget = false;

            // Level Badge
            GameObject lvGO = new GameObject("Level_Badge", typeof(RectTransform));
            lvGO.transform.SetParent(statusGO.transform, false);
            var lvRt = lvGO.GetComponent<RectTransform>();
            lvRt.anchorMin = new Vector2(0f, 1f); lvRt.anchorMax = new Vector2(0f, 1f);
            lvRt.pivot = new Vector2(0f, 1f);
            lvRt.anchoredPosition = new Vector2(98f, -14f);
            lvRt.sizeDelta = new Vector2(80f, 28f);

            GameObject lvTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            lvTextGO.transform.SetParent(lvGO.transform, false);
            var lvtRt = lvTextGO.GetComponent<RectTransform>();
            lvtRt.anchorMin = Vector2.zero; lvtRt.anchorMax = Vector2.one; lvtRt.sizeDelta = Vector2.zero;
            var lvText = lvTextGO.GetComponent<Text>();
            lvText.font = font;
            lvText.text = "Lv.1";
            lvText.fontSize = 20;
            lvText.fontStyle = FontStyle.Bold;
            lvText.color = Color.white;
            lvText.alignment = TextAnchor.MiddleLeft;
            var lvOut = lvTextGO.GetComponent<Outline>();
            lvOut.effectColor = new Color(0.14f, 0.89f, 1f, 0.85f);
            lvOut.effectDistance = new Vector2(1.5f, -1.5f);

            // Health Bar Capsule
            GameObject healthBarGO = new GameObject("Health_Bar", typeof(RectTransform), typeof(Image));
            healthBarGO.transform.SetParent(statusGO.transform, false);
            var hbRt = healthBarGO.GetComponent<RectTransform>();
            hbRt.anchorMin = new Vector2(0f, 0f); hbRt.anchorMax = new Vector2(0f, 0f);
            hbRt.pivot = new Vector2(0f, 0f);
            hbRt.anchoredPosition = new Vector2(98f, 18f);
            hbRt.sizeDelta = new Vector2(235f, 30f);
            var hbImg = healthBarGO.GetComponent<Image>();
            hbImg.sprite = spHealthBg;
            hbImg.type = Image.Type.Sliced;
            hbImg.raycastTarget = false;

            // Health Bar Fill
            GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(healthBarGO.transform, false);
            var fillRt = fillGO.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one; fillRt.sizeDelta = Vector2.zero;
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.sprite = spHealthFill;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1.0f;
            fillImg.raycastTarget = false;

            // Shield Icon on Health Bar
            GameObject shieldIconGO = new GameObject("Shield_Icon", typeof(RectTransform), typeof(Image));
            shieldIconGO.transform.SetParent(healthBarGO.transform, false);
            var sRt = shieldIconGO.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0f, 0.5f); sRt.anchorMax = new Vector2(0f, 0.5f);
            sRt.pivot = new Vector2(0.5f, 0.5f);
            sRt.anchoredPosition = new Vector2(15f, 0f);
            sRt.sizeDelta = new Vector2(20f, 20f);
            var sImg = shieldIconGO.GetComponent<Image>();
            sImg.sprite = spIconShield;
            sImg.raycastTarget = false;

            // Health Text "100 / 100"
            GameObject hpTextGO = new GameObject("HealthText", typeof(RectTransform), typeof(Text), typeof(Outline));
            hpTextGO.transform.SetParent(healthBarGO.transform, false);
            var hptRt = hpTextGO.GetComponent<RectTransform>();
            hptRt.anchorMin = Vector2.zero; hptRt.anchorMax = Vector2.one; hptRt.sizeDelta = Vector2.zero;
            var hpText = hpTextGO.GetComponent<Text>();
            hpText.font = font;
            hpText.text = "100 / 100";
            hpText.fontSize = 13;
            hpText.fontStyle = FontStyle.Bold;
            hpText.alignment = TextAnchor.MiddleCenter;
            hpText.color = Color.white;
            var hpOut = hpTextGO.GetComponent<Outline>();
            hpOut.effectColor = new Color(0.04f, 0.2f, 0.1f, 0.9f);
            hpOut.effectDistance = new Vector2(1f, -1f);

            // 3.B. HUD_TopRight (Kho đồ, Nâng cấp, Cài đặt)
            GameObject hudTopRightGO = new GameObject("HUD_TopRight", typeof(RectTransform));
            hudTopRightGO.transform.SetParent(hudTopGO.transform, false);
            var trRt = hudTopRightGO.GetComponent<RectTransform>();
            trRt.anchorMin = new Vector2(1f, 1f); trRt.anchorMax = new Vector2(1f, 1f);
            trRt.pivot = new Vector2(1f, 1f);
            trRt.anchoredPosition = new Vector2(-25f, -20f);
            trRt.sizeDelta = new Vector2(270f, 90f);

            var btnInv = CreateModernTopButton(hudTopRightGO.transform, "Inventory_Button", "Kho đồ", spBtnTop, spIconBackpack, new Vector2(-180f, 0f), font);
            var btnUpg = CreateModernTopButton(hudTopRightGO.transform, "Upgrade_Button", "Nâng cấp", spBtnTop, spIconUpgrade, new Vector2(-90f, 0f), font);
            var btnSet = CreateModernTopButton(hudTopRightGO.transform, "Settings_Button", "Cài đặt", spBtnTop, spIconSettings, new Vector2(0f, 0f), font);

            // ================= 4. HUD_MOVEMENT (VIRTUAL JOYSTICK) ================= //
            GameObject hudMoveGO = new GameObject("HUD_Movement", typeof(RectTransform));
            hudMoveGO.transform.SetParent(canvasGO.transform, false);
            var mvRt = hudMoveGO.GetComponent<RectTransform>();
            mvRt.anchorMin = Vector2.zero; mvRt.anchorMax = Vector2.zero;
            mvRt.pivot = Vector2.zero;
            mvRt.anchoredPosition = Vector2.zero;
            mvRt.sizeDelta = new Vector2(400f, 400f);

            GameObject joyBaseGO = new GameObject("JoystickBase", typeof(RectTransform), typeof(Image));
            joyBaseGO.transform.SetParent(hudMoveGO.transform, false);
            var jbRt = joyBaseGO.GetComponent<RectTransform>();
            jbRt.anchorMin = Vector2.zero; jbRt.anchorMax = Vector2.zero;
            jbRt.pivot = new Vector2(0.5f, 0.5f);
            jbRt.anchoredPosition = new Vector2(190f, 190f);
            jbRt.sizeDelta = new Vector2(205f, 205f);
            var jbImg = joyBaseGO.GetComponent<Image>();
            jbImg.sprite = spJoyBase;
            jbImg.color = Color.white;
            jbImg.raycastTarget = true;

            GameObject joyHandleGO = new GameObject("JoystickHandle", typeof(RectTransform), typeof(Image));
            joyHandleGO.transform.SetParent(joyBaseGO.transform, false);
            var jhRt = joyHandleGO.GetComponent<RectTransform>();
            jhRt.anchorMin = new Vector2(0.5f, 0.5f); jhRt.anchorMax = new Vector2(0.5f, 0.5f);
            jhRt.pivot = new Vector2(0.5f, 0.5f);
            jhRt.anchoredPosition = Vector2.zero;
            jhRt.sizeDelta = new Vector2(80f, 80f);
            var jhImg = joyHandleGO.GetComponent<Image>();
            jhImg.sprite = spJoyHandle;
            jhImg.color = Color.white;
            jhImg.raycastTarget = false;

            // ================= 5. HUD_ACTIONS (ACTION BUTTONS CLUSTER - GỌN GÀNG, THOÁNG ĐÃNG) ================= //
            GameObject hudActGO = new GameObject("HUD_Actions", typeof(RectTransform));
            hudActGO.transform.SetParent(canvasGO.transform, false);
            var actRt = hudActGO.GetComponent<RectTransform>();
            actRt.anchorMin = new Vector2(1f, 0f); actRt.anchorMax = new Vector2(1f, 0f);
            actRt.pivot = new Vector2(1f, 0f);
            actRt.anchoredPosition = Vector2.zero;
            actRt.sizeDelta = new Vector2(460f, 420f);

            // Nút 1: Bắn Súng (Lớn nhất 140px, Cam Đỏ Neon rực rỡ #FF3D00, icon tâm ngắm/bắn) - CHỈ BẮN KHI BẤM NÚT NÀY
            var btnFire = CreateModernActionButton(hudActGO.transform, "Fire_Button", "BẮN", spBtnFire, spIconGunFire, new Vector2(-125f, 125f), 140f, 68f, font, 14);

            // Nút 2: Tương tác [E] (120px, Cam Vàng Neon, icon bàn tay - Lên khinh khí cầu / Trận đấu / NPC)
            var btnInt = CreateModernActionButton(hudActGO.transform, "Interact_Button", "Tương tác\n[E]", spBtnInteract, spIconHand, new Vector2(-265f, 135f), 120f, 54f, font, 13);

            // Nút 3: Pháo hoa (84px, Tím Neon, icon pháo hoa)
            var btnFireworks = CreateModernActionButton(hudActGO.transform, "Firework_Button", "Pháo hoa", spBtnFireworks, spIconFireworks, new Vector2(-155f, 260f), 84f, 40f, font, 12);

            // Gán SerializedObject vào IUHMobileControlsUI
            var so = new SerializedObject(mobileCtrl);
            SetSerializedRef(so, "m_JoystickBase", jbRt);
            SetSerializedRef(so, "m_JoystickHandle", jhRt);
            SetSerializedRef(so, "m_TouchLookZone", lookRt);
            SetSerializedRef(so, "m_BtnFire", btnFire.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnInteract", btnInt.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnFireworks", btnFireworks.GetComponent<Button>());

            SetSerializedRef(so, "m_BtnInventory", btnInv.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnUpgrade", btnUpg.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnSettings", btnSet.GetComponent<Button>());

            SetSerializedRef(so, "m_HealthBarFill", fillImg);
            SetSerializedRef(so, "m_HealthText", hpText);
            SetSerializedRef(so, "m_LevelText", lvText);
            SetSerializedRef(so, "m_AvatarImage", afImg);
            so.ApplyModifiedProperties();
        }

        private static GameObject CreateModernActionButton(
            Transform parent, 
            string name, 
            string text, 
            Sprite bgSprite, 
            Sprite iconSprite, 
            Vector2 anchoredPos, 
            float size, 
            float iconSize, 
            Font font, 
            int fontSize)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback));
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(size, size);

            var img = btnGO.GetComponent<Image>();
            img.sprite = bgSprite;
            img.color = Color.white;
            img.raycastTarget = true;

            var btn = btnGO.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            // Icon Image
            if (iconSprite != null)
            {
                GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(btnGO.transform, false);
                var icRt = iconGO.GetComponent<RectTransform>();
                icRt.anchorMin = new Vector2(0.5f, 0.5f); icRt.anchorMax = new Vector2(0.5f, 0.5f);
                icRt.pivot = new Vector2(0.5f, 0.5f);
                icRt.anchoredPosition = new Vector2(0f, size * 0.12f);
                icRt.sizeDelta = new Vector2(iconSize, iconSize);

                var icImg = iconGO.GetComponent<Image>();
                icImg.sprite = iconSprite;
                icImg.color = Color.white;
                icImg.raycastTarget = false;
            }

            // Text Label
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            textGO.transform.SetParent(btnGO.transform, false);
            var tRt = textGO.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0f); tRt.anchorMax = new Vector2(1f, 0.45f);
            tRt.pivot = new Vector2(0.5f, 0f);
            tRt.anchoredPosition = new Vector2(0f, 6f);
            tRt.sizeDelta = Vector2.zero;

            var t = textGO.GetComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.raycastTarget = false;

            var outl = textGO.GetComponent<Outline>();
            outl.effectColor = new Color(0.02f, 0.05f, 0.12f, 0.9f);
            outl.effectDistance = new Vector2(1.5f, -1.5f);

            return btnGO;
        }

        private static GameObject CreateModernTopButton(
            Transform parent, 
            string name, 
            string label, 
            Sprite bgSprite, 
            Sprite iconSprite, 
            Vector2 anchoredPos, 
            Font font)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback));
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(68f, 68f);

            var img = btnGO.GetComponent<Image>();
            img.sprite = bgSprite;
            img.color = Color.white;
            img.raycastTarget = true;

            var btn = btnGO.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            // Icon
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(btnGO.transform, false);
            var icRt = iconGO.GetComponent<RectTransform>();
            icRt.anchorMin = new Vector2(0.5f, 0.5f); icRt.anchorMax = new Vector2(0.5f, 0.5f);
            icRt.pivot = new Vector2(0.5f, 0.5f);
            icRt.anchoredPosition = new Vector2(0f, 4f);
            icRt.sizeDelta = new Vector2(34f, 34f);
            var icImg = iconGO.GetComponent<Image>();
            icImg.sprite = iconSprite;
            icImg.color = Color.white;
            icImg.raycastTarget = false;

            // Label Text under button
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            textGO.transform.SetParent(btnGO.transform, false);
            var tRt = textGO.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.5f, 0f); tRt.anchorMax = new Vector2(0.5f, 0f);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.anchoredPosition = new Vector2(0f, -4f);
            tRt.sizeDelta = new Vector2(90f, 22f);

            var t = textGO.GetComponent<Text>();
            t.font = font;
            t.text = label;
            t.fontSize = 12;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.raycastTarget = false;

            var outl = textGO.GetComponent<Outline>();
            outl.effectColor = new Color(0.05f, 0.1f, 0.2f, 0.95f);
            outl.effectDistance = new Vector2(1.2f, -1.2f);

            return btnGO;
        }

        private static void SetSerializedRef(SerializedObject so, string propName, UnityEngine.Object target)
        {
            var prop = so.FindProperty(propName);
            if (prop != null) prop.objectReferenceValue = target;
        }

        public static (GameObject reticleRoot, Transform outerRing, Transform innerRing) CreateHologramReticle(Transform parent)
        {
            GameObject reticle = new GameObject("AR_Hologram_Reticle");
            if (parent != null) reticle.transform.SetParent(parent, false);
            reticle.transform.localPosition = Vector3.zero;

            Material holoMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Waypoint_Holo.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/CombatArena/ArenaGlowCyan.mat");

            // Vòng ngoài
            GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            outer.name = "Holo_OuterRing";
            outer.transform.SetParent(reticle.transform, false);
            outer.transform.localScale = new Vector3(1.8f, 0.01f, 1.8f);
            if (outer.GetComponent<Collider>() != null) Object.DestroyImmediate(outer.GetComponent<Collider>());
            if (holoMat != null) outer.GetComponent<Renderer>().sharedMaterial = holoMat;

            // Vòng trong
            GameObject inner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            inner.name = "Holo_InnerRing";
            inner.transform.SetParent(reticle.transform, false);
            inner.transform.localScale = new Vector3(1.1f, 0.015f, 1.1f);
            if (inner.GetComponent<Collider>() != null) Object.DestroyImmediate(inner.GetComponent<Collider>());
            if (holoMat != null) inner.GetComponent<Renderer>().sharedMaterial = holoMat;

            // Đèn phát quang tâm
            GameObject rLight = new GameObject("ReticlePulseLight", typeof(Light));
            rLight.transform.SetParent(reticle.transform, false);
            rLight.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            var l = rLight.GetComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.1f, 0.95f, 1f);
            l.intensity = 2.5f;
            l.range = 3.5f;

            return (reticle, outer.transform, inner.transform);
        }

        public static GameObject CreateCorruptedBackpack(Transform parent)
        {
            GameObject bp = new GameObject("Corrupted_Backpack");
            bp.transform.SetParent(parent, false);
            bp.transform.localPosition = new Vector3(0f, 0.25f, 0f);

            Material blueMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Student_BluePolo.mat");
            Material redMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Cloth_Red.mat");
            Material goldMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_DailyCoin_Gold.mat");

            // Thân balo chính
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Backpack_Body";
            body.transform.SetParent(bp.transform, false);
            body.transform.localScale = new Vector3(0.55f, 0.65f, 0.35f);
            if (blueMat != null) body.GetComponent<Renderer>().sharedMaterial = blueMat;

            // Túi trước đỏ
            GameObject pocket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pocket.name = "Front_Pocket";
            pocket.transform.SetParent(bp.transform, false);
            pocket.transform.localPosition = new Vector3(0f, -0.08f, 0.18f);
            pocket.transform.localScale = new Vector3(0.42f, 0.35f, 0.12f);
            if (pocket.GetComponent<Collider>() != null) Object.DestroyImmediate(pocket.GetComponent<Collider>());
            if (redMat != null) pocket.GetComponent<Renderer>().sharedMaterial = redMat;

            // Khóa kéo vàng
            GameObject zipper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zipper.name = "Zipper_Puller";
            zipper.transform.SetParent(bp.transform, false);
            zipper.transform.localPosition = new Vector3(0f, 0.12f, 0.19f);
            zipper.transform.localScale = new Vector3(0.38f, 0.03f, 0.04f);
            if (zipper.GetComponent<Collider>() != null) Object.DestroyImmediate(zipper.GetComponent<Collider>());
            if (goldMat != null) zipper.GetComponent<Renderer>().sharedMaterial = goldMat;

            return bp;
        }

        public static GameObject CreateCorruptedLaptop(Transform parent)
        {
            GameObject laptop = new GameObject("Corrupted_Laptop");
            laptop.transform.SetParent(parent, false);
            laptop.transform.localPosition = new Vector3(0f, 0.05f, 0f);

            Material darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Sign_Dark.mat");
            Material neonMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_CyberRifle_Neon.mat");

            // Đế laptop
            GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "Laptop_Chassis";
            chassis.transform.SetParent(laptop.transform, false);
            chassis.transform.localScale = new Vector3(0.6f, 0.035f, 0.42f);
            if (darkMat != null) chassis.GetComponent<Renderer>().sharedMaterial = darkMat;

            // Bàn phím
            GameObject kb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kb.name = "Keyboard_Area";
            kb.transform.SetParent(laptop.transform, false);
            kb.transform.localPosition = new Vector3(0f, 0.02f, 0.02f);
            kb.transform.localScale = new Vector3(0.52f, 0.01f, 0.26f);
            if (kb.GetComponent<Collider>() != null) Object.DestroyImmediate(kb.GetComponent<Collider>());

            // Màn hình mở nghiêng 115 độ
            GameObject screenRoot = new GameObject("Screen_Lid");
            screenRoot.transform.SetParent(laptop.transform, false);
            screenRoot.transform.localPosition = new Vector3(0f, 0.02f, -0.2f);
            screenRoot.transform.localRotation = Quaternion.Euler(-115f, 0f, 0f);

            GameObject screenFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screenFrame.name = "Screen_Frame";
            screenFrame.transform.SetParent(screenRoot.transform, false);
            screenFrame.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            screenFrame.transform.localScale = new Vector3(0.6f, 0.4f, 0.025f);
            if (darkMat != null) screenFrame.GetComponent<Renderer>().sharedMaterial = darkMat;

            // Màn hình phát sáng matrix cyan
            GameObject display = GameObject.CreatePrimitive(PrimitiveType.Cube);
            display.name = "Matrix_Display";
            display.transform.SetParent(screenRoot.transform, false);
            display.transform.localPosition = new Vector3(0f, 0.2f, 0.015f);
            display.transform.localScale = new Vector3(0.55f, 0.35f, 0.01f);
            if (display.GetComponent<Collider>() != null) Object.DestroyImmediate(display.GetComponent<Collider>());
            if (neonMat != null) display.GetComponent<Renderer>().sharedMaterial = neonMat;

            return laptop;
        }

        public static GameObject CreateCorruptedTextbook(Transform parent)
        {
            GameObject stack = new GameObject("Corrupted_Textbooks");
            stack.transform.SetParent(parent, false);
            stack.transform.localPosition = new Vector3(0f, 0.06f, 0f);

            Material blueMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Student_BluePolo.mat");
            Material redMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Cloth_Red.mat");
            Material goldMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_DailyCoin_Gold.mat");

            // Quyển 1 (Dưới cùng: Xanh IUH)
            GameObject b1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b1.name = "Book_Base";
            b1.transform.SetParent(stack.transform, false);
            b1.transform.localScale = new Vector3(0.52f, 0.08f, 0.38f);
            if (blueMat != null) b1.GetComponent<Renderer>().sharedMaterial = blueMat;

            // Quyển 2 (Ở giữa: Đỏ, xoay lệch 12 độ)
            GameObject b2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b2.name = "Book_Middle";
            b2.transform.SetParent(stack.transform, false);
            b2.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            b2.transform.localRotation = Quaternion.Euler(0f, 12f, 0f);
            b2.transform.localScale = new Vector3(0.48f, 0.075f, 0.36f);
            if (redMat != null) b2.GetComponent<Renderer>().sharedMaterial = redMat;

            // Quyển 3 (Trên cùng: Vàng kim có ruy băng kẹp sách)
            GameObject b3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b3.name = "Book_Top";
            b3.transform.SetParent(stack.transform, false);
            b3.transform.localPosition = new Vector3(0f, 0.155f, 0f);
            b3.transform.localRotation = Quaternion.Euler(0f, -8f, 0f);
            b3.transform.localScale = new Vector3(0.45f, 0.07f, 0.34f);
            if (goldMat != null) b3.GetComponent<Renderer>().sharedMaterial = goldMat;

            return stack;
        }

        public static ParticleSystem CreatePurpleSmoke(Transform parent)
        {
            GameObject smokeGO = new GameObject("PurpleSmoke", typeof(ParticleSystem));
            smokeGO.transform.SetParent(parent, false);
            smokeGO.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            var ps = smokeGO.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.8f;
            main.startSpeed = 0.9f;
            main.startSize = 0.65f;
            main.maxParticles = 60;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.6f, 0.1f, 0.9f, 0.85f),
                new Color(0.85f, 0.2f, 1f, 0.95f)
            );
            var emission = ps.emission;
            emission.rateOverTime = 25f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.3f;

            return ps;
        }

        public static ParticleSystem CreateGlitchPixels(Transform parent)
        {
            GameObject glitchGO = new GameObject("GlitchPixels", typeof(ParticleSystem));
            glitchGO.transform.SetParent(parent, false);
            glitchGO.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            var ps = glitchGO.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.6f;
            main.startSpeed = 2.2f;
            main.startSize = 0.12f;
            main.maxParticles = 40;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.1f, 0.5f, 1f),
                new Color(0.2f, 0.95f, 1f, 1f)
            );
            var emission = ps.emission;
            emission.rateOverTime = 18f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.45f;

            return ps;
        }

        public static (GameObject monsterRoot, GameObject tapPrompt) CreateCyberDemonMonster(Transform parent, Font font)
        {
            GameObject monsterRoot = new GameObject("AR_Monster");
            monsterRoot.transform.SetParent(parent, false);
            monsterRoot.transform.localPosition = new Vector3(0f, 0f, 0.65f);

            GameObject monsterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MONSTER_MODEL_PATH);
            if (monsterPrefab != null)
            {
                var monsterInst = (GameObject)PrefabUtility.InstantiatePrefab(monsterPrefab, monsterRoot.transform);
                monsterInst.transform.localPosition = Vector3.zero;
                monsterInst.transform.localScale = Vector3.one * 1.35f;
                Material monsterMat = AssetDatabase.LoadAssetAtPath<Material>(MONSTER_MAT_PATH);
                if (monsterMat != null)
                {
                    foreach (var r in monsterInst.GetComponentsInChildren<Renderer>()) r.sharedMaterial = monsterMat;
                }
            }

            var monsterCol = monsterRoot.AddComponent<CapsuleCollider>();
            monsterCol.center = new Vector3(0f, 0.8f, 0f);
            monsterCol.radius = 0.65f;
            monsterCol.height = 1.85f;

            // 3D Billboard Tap Prompt
            GameObject tapPrompt = new GameObject("PROMPT_TAP", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(IUHGreenBillboard));
            tapPrompt.transform.SetParent(monsterRoot.transform, false);
            tapPrompt.transform.localPosition = new Vector3(0f, 2.25f, 0f);
            var tc = tapPrompt.GetComponent<Canvas>();
            tc.renderMode = RenderMode.WorldSpace;
            var trt = tapPrompt.GetComponent<RectTransform>();
            trt.sizeDelta = new Vector2(320f, 85f);
            trt.localScale = Vector3.one * 0.007f;

            GameObject tpCard = new GameObject("Card", typeof(RectTransform), typeof(Image));
            tpCard.transform.SetParent(tapPrompt.transform, false);
            var tpCardRt = tpCard.GetComponent<RectTransform>();
            tpCardRt.anchorMin = Vector2.zero; tpCardRt.anchorMax = Vector2.one; tpCardRt.sizeDelta = Vector2.zero;
            tpCard.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.16f, 0.94f);

            var tpText = CreateText(tpCard.transform, "Text", "⚡ <b>CHẠM ĐỂ CHIẾN ĐẤU!</b>\n<size=12><color=#FDE047>[MATCH-3 RPG DUEL • PHÍM [E]]</color></size>", 15, new Vector2(0.5f, 0.5f), new Vector2(300f, 65f), font, new Color(0.2f, 0.95f, 1f));
            tpText.alignment = TextAnchor.MiddleCenter;

            return (monsterRoot, tapPrompt);
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 anchor, Vector2 size, Font font, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = size;
            var t = go.GetComponent<Text>();
            t.text = text; t.fontSize = fontSize; t.font = font; t.alignment = TextAnchor.MiddleCenter; t.color = color; t.raycastTarget = false;
            return t;
        }
    }
}
#endif
