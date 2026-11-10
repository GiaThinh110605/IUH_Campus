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

        [MenuItem("IUH Campus/AR/Setup Corrupted Object in Main Campus Scene", false, 161)]
        public static void SetupCorruptedEncounterInCampusScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            Debug.Log("<color=#00E5FF>[IUH AR]</color> Cài đặt Dị Tượng Nhiễm Độc vào Scene hiện tại: " + activeScene.path);

            var existing = GameObject.Find("CORRUPTED_OBJECT_STUDENT_BACKPACK");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            Vector3 spawnPos = new Vector3(4.0f, 0.05f, -28.0f);
            GameObject root = new GameObject("CORRUPTED_OBJECT_STUDENT_BACKPACK");
            root.transform.position = spawnPos;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 3 Loại đồ vật bị nhiễm
            GameObject backpack = CreateCorruptedBackpack(root.transform);
            GameObject laptop = CreateCorruptedLaptop(root.transform);
            GameObject textbook = CreateCorruptedTextbook(root.transform);

            // VFX
            var smokePS = CreatePurpleSmoke(root.transform);
            var glitchPS = CreateGlitchPixels(root.transform);

            // Point Light
            GameObject auraLightGO = new GameObject("PurpleAuraLight", typeof(Light));
            auraLightGO.transform.SetParent(root.transform, false);
            auraLightGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var auraLight = auraLightGO.GetComponent<Light>();
            auraLight.type = LightType.Point;
            auraLight.color = new Color(0.85f, 0.15f, 1f);
            auraLight.intensity = 4.5f;
            auraLight.range = 6.0f;

            // Reticle
            var reticlePack = CreateHologramReticle(root.transform);

            // 3D Monster
            var monsterPack = CreateCyberDemonMonster(root.transform, font);

            // Attach & Wire Encounter Component
            var encComp = root.AddComponent<IUHARCorruptedObjectEncounter>();
            var soEnc = new SerializedObject(encComp);
            soEnc.FindProperty("m_AutoPlaceOnStart").boolValue = true;
            soEnc.FindProperty("m_ConfiguredType").enumValueIndex = (int)CorruptedObjectType.Random;
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

            // Kích hoạt đặt sẵn tại sân trường để sinh viên tiếp cận là chiến đấu được ngay
            encComp.PlaceCorruptedObject(spawnPos, Quaternion.identity);

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#00FF88>[IUH AR] ĐÃ CÀI ĐẶT THÀNH CÔNG DỊ TƯỢNG VÀO SÂN TRƯỜNG!</color>");
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
