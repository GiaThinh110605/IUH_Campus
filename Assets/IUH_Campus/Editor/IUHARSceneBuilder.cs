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

            // Điểm 1: Cạnh Đài Phun Nước / Cổng Trường (Balo Sinh Viên IUH)
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

            // Cài đặt Mobile Touch Controls cho iPhone 12
            SetupMobileControlsInScene();

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#00FF88>[IUH AR] ĐÃ CÀI ĐẶT THÀNH CÔNG 3 ĐIỂM DỊ TƯỢNG VÀ MOBILE CONTROLS VÀO SÂN TRƯỜNG!</color>");
        }

        private static GameObject CreateSingleCorruptedEncounter(
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
            auraLight.intensity = 4.5f;
            auraLight.range = 6.0f;

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

            // 3. Cần xoay ảo Joystick di chuyển (Góc dưới bên trái)
            GameObject joyBaseGO = new GameObject("JoystickBase", typeof(RectTransform), typeof(Image));
            joyBaseGO.transform.SetParent(canvasGO.transform, false);
            var jbRt = joyBaseGO.GetComponent<RectTransform>();
            jbRt.anchorMin = Vector2.zero; jbRt.anchorMax = Vector2.zero;
            jbRt.pivot = new Vector2(0.5f, 0.5f);
            jbRt.anchoredPosition = new Vector2(210f, 210f);
            jbRt.sizeDelta = new Vector2(180f, 180f);
            var jbImg = joyBaseGO.GetComponent<Image>();
            jbImg.color = new Color(0.04f, 0.08f, 0.16f, 0.65f);
            var jbOutline = joyBaseGO.AddComponent<Outline>();
            jbOutline.effectColor = new Color(0.2f, 0.85f, 1f, 0.8f);
            jbOutline.effectDistance = new Vector2(3f, 3f);

            GameObject joyHandleGO = new GameObject("JoystickHandle", typeof(RectTransform), typeof(Image));
            joyHandleGO.transform.SetParent(joyBaseGO.transform, false);
            var jhRt = joyHandleGO.GetComponent<RectTransform>();
            jhRt.anchorMin = new Vector2(0.5f, 0.5f); jhRt.anchorMax = new Vector2(0.5f, 0.5f);
            jhRt.sizeDelta = new Vector2(75f, 75f);
            var jhImg = joyHandleGO.GetComponent<Image>();
            jhImg.color = new Color(0.2f, 0.9f, 1f, 0.95f);

            // 4. Cụm nút hành động góc dưới bên phải (Ergonomic Cyberpunk RPG Layout)
            Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            if (jhImg != null && circleSprite != null) { jhImg.sprite = circleSprite; }

            // Nút 1: BẮN SÚNG PLASMA CHÍNH [Fire / Attack] (To nhất, màu đỏ cam rực lửa)
            var btnFire = CreateLandscapeActionButton(canvasGO.transform, "BtnFire", "🔫\n<size=15>BẮN</size>", new Vector2(-125f, 160f), 115f, new Color(1f, 0.25f, 0.12f, 0.95f), new Color(1f, 0.85f, 0.3f, 1f), circleSprite, font);

            // Nút 2: NGẮM ADS CAMERA [Aim / Scope] (Màu xanh neon hologram)
            var btnAim = CreateLandscapeActionButton(canvasGO.transform, "BtnAim", "🎯\n<size=13>NGẮM</size>", new Vector2(-125f, 290f), 85f, new Color(0.05f, 0.72f, 0.95f, 0.92f), new Color(0.6f, 0.95f, 1f, 1f), circleSprite, font);

            // Nút 3: TÊN LỬA CAO XẠ PHÒNG KHÔNG [Q] (Màu vàng cam lửa)
            var btnMissile = CreateLandscapeActionButton(canvasGO.transform, "BtnMissile", "🚀\n<size=12>TÊN LỬA</size>", new Vector2(-240f, 250f), 82f, new Color(1f, 0.65f, 0.05f, 0.92f), new Color(1f, 0.9f, 0.4f, 1f), circleSprite, font);

            // Nút 4: Tương tác [E] (Vào trận Match-3 / Dị tượng / Trò chuyện - Xanh dương điện tử)
            var btnInt = CreateLandscapeActionButton(canvasGO.transform, "BtnInteract", "⚔️ [E]\n<size=12>MATCH-3</size>", new Vector2(-240f, 125f), 92f, new Color(0.12f, 0.55f, 1f, 0.95f), new Color(0.5f, 0.85f, 1f, 1f), circleSprite, font);

            // Nút 5: Nhảy [Space] (Xanh ngọc lục bảo)
            var btnJump = CreateLandscapeActionButton(canvasGO.transform, "BtnJump", "🦘\n<size=12>NHẢY</size>", new Vector2(-345f, 95f), 85f, new Color(0.05f, 0.82f, 0.4f, 0.92f), new Color(0.6f, 1f, 0.8f, 1f), circleSprite, font);

            // Nút 6: Chạy nhanh [Shift / Turbo] (Tím neon sấm sét)
            var btnSprint = CreateLandscapeActionButton(canvasGO.transform, "BtnSprint", "⚡\n<size=12>CHẠY</size>", new Vector2(-345f, 200f), 80f, new Color(0.85f, 0.12f, 0.95f, 0.92f), new Color(1f, 0.6f, 1f, 1f), circleSprite, font);

            // Nút 7: Lên/xuống xe máy & Khinh khí cầu [V] (Tím đậm công nghệ)
            var btnVeh = CreateLandscapeActionButton(canvasGO.transform, "BtnVehicle", "🛵\n<size=12>XE / BAY</size>", new Vector2(-445f, 95f), 76f, new Color(0.45f, 0.2f, 0.95f, 0.92f), new Color(0.8f, 0.6f, 1f, 1f), circleSprite, font);

            // Nút 8: Kỹ năng Bắn Pháo Hoa Lễ Hội [K/B] (Hồng neon rực rỡ)
            var btnFireworks = CreateLandscapeActionButton(canvasGO.transform, "BtnFireworks", "🎆\n<size=12>PHÁO HOA</size>", new Vector2(-440f, 195f), 76f, new Color(1f, 0.15f, 0.55f, 0.92f), new Color(1f, 0.65f, 0.85f, 1f), circleSprite, font);

            // Gán các tham chiếu an toàn vào script IUHMobileControlsUI
            var so = new SerializedObject(mobileCtrl);
            SetSerializedRef(so, "m_JoystickBase", jbRt);
            SetSerializedRef(so, "m_JoystickHandle", jhRt);
            SetSerializedRef(so, "m_TouchLookZone", lookRt);
            SetSerializedRef(so, "m_BtnFire", btnFire.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnAim", btnAim.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnMissile", btnMissile.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnInteract", btnInt.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnJump", btnJump.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnSprint", btnSprint.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnVehicle", btnVeh.GetComponent<Button>());
            SetSerializedRef(so, "m_BtnFireworks", btnFireworks.GetComponent<Button>());
            so.ApplyModifiedProperties();
        }

        private static void SetSerializedRef(SerializedObject so, string propName, UnityEngine.Object target)
        {
            var prop = so.FindProperty(propName);
            if (prop != null) prop.objectReferenceValue = target;
        }

        private static GameObject CreateLandscapeActionButton(Transform parent, string name, string text, Vector2 anchoredPos, float size, Color col, Color glowCol, Sprite sprite, Font font)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(size, size);

            var img = btnGO.GetComponent<Image>();
            img.sprite = sprite;
            img.color = col;
            img.raycastTarget = true;

            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = glowCol;
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            var btn = btnGO.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            btn.colors = colors;

            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);
            var tRt = textGO.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one; tRt.sizeDelta = Vector2.zero;
            var t = textGO.GetComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = (int)(size * 0.22f);
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.raycastTarget = false;

            return btnGO;
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
