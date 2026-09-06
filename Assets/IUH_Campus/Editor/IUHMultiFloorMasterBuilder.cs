using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus.Core;
using IUHCampus.Debugging;
using IUHCampus.Interaction;
using IUHCampus.NPC;
using IUHCampus.Player;
using IUHCampus.Save;
using IUHCampus.UI;
using IUHCampus.World;

namespace IUHCampus.Editor
{
    public static class IUHMultiFloorMasterBuilder
    {
        private const string ScenePath = "Assets/IUH_Campus/Scenes/IUH_RightCluster.unity";
        private const string MatPath = "Assets/IUH_Campus/Materials/";

        // Material Cache
        private static Material m_WallWeathered, m_ConcreteGround, m_ConcreteCurb, m_AsphaltRoad;
        private static Material m_DrainGrate, m_ManholeCover, m_SignWayfinding, m_SignNotice;
        private static Material m_ACLouvers, m_WindowBlinds, m_TreeBark, m_MatureFoliage;
        private static Material m_BenchGranite, m_FireExtinguisher, m_AccessControl, m_UrbanHouse;
        private static Material m_G_Wall, m_G_Glass, m_G_Roof, m_G_Sign;
        private static Material m_I_Wall, m_I_Roof;
        private static Material m_C_Wall, m_C_Mint;
        private static Material m_Podium_Turquoise, m_Podium_Roof;
        private static Material m_DarkMetal, m_ParkingLine, m_FireCabinet;
        private static Material m_WaterTankBlack, m_WaterTankStainless;
        private static Material m_BlackMarble, m_LightTile, m_LightWood, m_SageGreen, m_CyanSofa;
        private static Material m_LEDDisplay, m_ScreenPC, m_GlassClear;
        private static Material m_NavyChair, m_CeilingGrid, m_MatteWhite, m_DarkMullion;
        private static Material[] m_BikeMats;
        private static Material[] m_HelmetMats;
        private static Material m_StudentWhiteShirt, m_StudentBluePolo, m_StudentJeans, m_StudentDarkPants, m_StudentSkin, m_StudentHair;

        [MenuItem("IUH Campus/Build Multi-Floor World & Systems (Complete Academic)")]
        public static void BuildMultiFloorWorldAndSystems()
        {
            Debug.Log("[IUH Multi-Floor Builder] Bắt đầu xây dựng thế giới giảng đường đa tầng hoàn chỉnh...");

            // 1. Ensure assets exist
            IUHTextureGenerator.GenerateAllTextures();
            IUHMaterialGenerator.GenerateAllMaterials();
            LoadMaterials();

            // 2. Open Scene
            var activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // 3. Clean old scene roots
            string[] oldRoots = { "IUH_World", "Campus_RightCluster", "IUH_RightCluster", "IUH_RightCluster_Interior", "RightBuildingsCluster" };
            foreach (string rootName in oldRoots)
            {
                GameObject oldGo = GameObject.Find(rootName);
                if (oldGo != null) Object.DestroyImmediate(oldGo);
            }

            // 4. Create Root
            GameObject worldRoot = new GameObject("IUH_World");

            // 5. Build Sub-branches
            GameObject goSystems = CreateChild(worldRoot, "_SYSTEMS");
            GameObject goPlayer = CreateChild(worldRoot, "PLAYER");
            GameObject goWorld = CreateChild(worldRoot, "WORLD");
            GameObject goNPC = CreateChild(worldRoot, "NPC");
            GameObject goGameplay = CreateChild(worldRoot, "GAMEPLAY");
            GameObject goNav = CreateChild(worldRoot, "NAVIGATION");
            GameObject goAudio = CreateChild(worldRoot, "AUDIO");
            GameObject goLighting = CreateChild(worldRoot, "LIGHTING");
            GameObject goDebug = CreateChild(worldRoot, "DEBUG");

            // --- BUILD SYSTEMS ---
            BuildSystems(goSystems);

            // --- BUILD WORLD (LOCKED EXTERIOR & MULTI-FLOOR INTERIORS) ---
            BuildWorldContent(goWorld);

            // --- BUILD GAMEPLAY ANCHORS & INTERACTABLES ---
            BuildGameplayContent(goGameplay, goWorld);

            // --- BUILD PLAYER & SPAWNS ---
            BuildPlayerContent(goPlayer, goSystems);

            // --- BUILD NPC ROUTES & RUNTIME ---
            BuildNPCContent(goNPC);

            // --- BUILD LIGHTING & AUDIO ---
            BuildLightingAndAudio(goLighting, goAudio);

            // --- BUILD DEBUG ---
            BuildDebugContent(goDebug);

            // Disable unbaked reflection probes
            foreach (var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                probe.enabled = false;
            }

            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.Refresh();
            Debug.Log("[IUH Multi-Floor Builder] Thế giới giảng đường đa tầng hoàn chỉnh đã được xây dựng thành công!");
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void LoadMaterials()
        {
            m_WallWeathered = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Wall.mat");
            m_ConcreteGround = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Concrete_Ground.mat");
            m_ConcreteCurb = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Concrete_Curb.mat");
            m_AsphaltRoad = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Asphalt_Road.mat");
            m_DrainGrate = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Drain_Grate.mat");
            m_ManholeCover = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Manhole_Cover.mat");
            m_SignWayfinding = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Sign_Wayfinding.mat");
            m_SignNotice = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Sign_NoticeBoard.mat");
            m_ACLouvers = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_AC_Louvers.mat");
            m_WindowBlinds = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Window_Blinds.mat");
            m_TreeBark = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Tree_Bark.mat");
            m_MatureFoliage = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_MatureFoliage.mat");
            m_BenchGranite = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Bench_Granite.mat");
            m_FireExtinguisher = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Fire_Extinguisher.mat");
            m_AccessControl = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Access_Control.mat");
            m_UrbanHouse = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Urban_House.mat");

            m_G_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Wall.mat");
            m_G_Glass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_WindowGlass.mat");
            m_G_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Roof.mat");
            m_G_Sign = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Sign.mat");

            m_I_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Wall.mat");
            m_I_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Roof.mat");

            m_C_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_Wall.mat");
            m_C_Mint = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_AccentMint.mat");

            m_Podium_Turquoise = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Podium_Turquoise.mat");
            m_Podium_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Podium_Roof.mat");

            m_DarkMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");
            m_ParkingLine = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Parking_Line.mat");
            m_FireCabinet = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_FireCabinet.mat");
            m_WaterTankBlack = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_WaterTank_Black.mat");
            m_WaterTankStainless = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_WaterTank_Stainless.mat");

            m_BlackMarble = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_BlackMarble.mat");
            m_LightTile = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_LightTile.mat");
            m_LightWood = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_LightWood.mat");
            m_SageGreen = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_SageGreen.mat");
            m_CyanSofa = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_CyanSofa.mat");
            m_LEDDisplay = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_LEDDisplay.mat");
            m_ScreenPC = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_ScreenPC.mat");
            m_GlassClear = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_GlassClear.mat");
            m_NavyChair = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_NavyChair.mat");
            m_CeilingGrid = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_CeilingGrid.mat");
            m_MatteWhite = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_MatteWhite.mat");
            m_DarkMullion = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_DarkMullion.mat");

            m_BikeMats = new Material[]
            {
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Red.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Dark.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Blue.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_White.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Silver.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Teal.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Yellow.mat")
            };

            m_HelmetMats = new Material[]
            {
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Helmet_Red.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Helmet_Blue.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Helmet_White.mat")
            };

            m_StudentWhiteShirt = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Student_WhiteShirt.mat");
            m_StudentBluePolo = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Student_BluePolo.mat");
            m_StudentJeans = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Student_Jeans.mat");
            m_StudentDarkPants = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Student_DarkPants.mat");
            m_StudentSkin = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Student_Skin.mat");
            m_StudentHair = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Student_Hair.mat");
        }

        // ================= 1. SYSTEMS =================
        private static void BuildSystems(GameObject parent)
        {
            GameObject gm = CreateChild(parent, "GameManager");
            gm.AddComponent<GameManager>();

            GameObject ws = CreateChild(parent, "SaveSystem");
            ws.AddComponent<SaveSystem>();
            ws.AddComponent<WorldState>();

            GameObject im = CreateChild(parent, "InteractionManager");
            BuildInteractionUI(im);

            GameObject nm = CreateChild(parent, "NPCManager");
            GameObject am = CreateChild(parent, "AudioManager");

            GameObject dm = CreateChild(parent, "DebugManager");
            dm.AddComponent<DebugManager>();
            dm.AddComponent<DebugTeleport>();
        }

        private static void BuildInteractionUI(GameObject parent)
        {
            GameObject canvasGO = new GameObject("InteractionCanvas");
            canvasGO.transform.SetParent(parent.transform, false);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 1.0f;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Location HUD (Top Left)
            GameObject locRoot = new GameObject("LocationHUD");
            locRoot.transform.SetParent(canvasGO.transform, false);
            RectTransform locRT = locRoot.AddComponent<RectTransform>();
            locRT.anchorMin = new Vector2(0.02f, 0.92f);
            locRT.anchorMax = new Vector2(0.40f, 0.98f);
            locRT.sizeDelta = Vector2.zero;

            Image locBG = locRoot.AddComponent<Image>();
            locBG.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);

            GameObject locTextGO = new GameObject("LocationText");
            locTextGO.transform.SetParent(locRoot.transform, false);
            Text locText = locTextGO.AddComponent<Text>();
            locText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            locText.fontSize = 20;
            locText.fontStyle = FontStyle.Bold;
            locText.alignment = TextAnchor.MiddleLeft;
            locText.color = new Color(0.3f, 0.9f, 1.0f);
            locText.text = "Khuôn viên IUH | Tòa G";
            RectTransform ltRT = locTextGO.GetComponent<RectTransform>();
            ltRT.anchorMin = new Vector2(0.05f, 0f);
            ltRT.anchorMax = new Vector2(0.95f, 1f);
            ltRT.sizeDelta = Vector2.zero;

            // Interaction Prompt Root (Bottom Center)
            GameObject promptRoot = new GameObject("InteractionPrompt");
            promptRoot.transform.SetParent(canvasGO.transform, false);
            RectTransform pRT = promptRoot.AddComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.5f, 0.2f);
            pRT.anchorMax = new Vector2(0.5f, 0.2f);
            pRT.sizeDelta = new Vector2(450f, 60f);

            GameObject pTextGO = new GameObject("PromptText");
            pTextGO.transform.SetParent(promptRoot.transform, false);
            Text pText = pTextGO.AddComponent<Text>();
            pText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pText.fontSize = 24;
            pText.alignment = TextAnchor.MiddleCenter;
            pText.color = Color.yellow;
            pText.text = "[E] Tương tác";
            RectTransform ptRT = pTextGO.GetComponent<RectTransform>();
            ptRT.anchorMin = Vector2.zero;
            ptRT.anchorMax = Vector2.one;
            ptRT.sizeDelta = Vector2.zero;

            var intUI = parent.AddComponent<InteractionUI>();
            intUI.promptRoot = promptRoot;
            intUI.promptText = pText;
            intUI.locationText = locText;

            // Simple Modal Panel
            GameObject panelRoot = new GameObject("SimpleInteractionPanel");
            panelRoot.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.2f, 0.2f);
            panelRT.anchorMax = new Vector2(0.8f, 0.8f);
            panelRT.sizeDelta = Vector2.zero;

            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.18f, 0.94f);

            // Title
            GameObject titleGO = new GameObject("PanelTitle");
            titleGO.transform.SetParent(panelRoot.transform, false);
            Text titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 26;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(0.2f, 0.8f, 1f);
            titleTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform tRT = titleGO.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0f, 0.85f);
            tRT.anchorMax = new Vector2(1f, 1f);
            tRT.sizeDelta = Vector2.zero;

            // Content
            GameObject bodyGO = new GameObject("PanelBody");
            bodyGO.transform.SetParent(panelRoot.transform, false);
            Text bodyTxt = bodyGO.AddComponent<Text>();
            bodyTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bodyTxt.fontSize = 18;
            bodyTxt.color = Color.white;
            bodyTxt.alignment = TextAnchor.UpperLeft;
            RectTransform bRT = bodyGO.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.05f, 0.2f);
            bRT.anchorMax = new Vector2(0.95f, 0.82f);
            bRT.sizeDelta = Vector2.zero;

            // Close Button
            GameObject btnGO = new GameObject("CloseButton");
            btnGO.transform.SetParent(panelRoot.transform, false);
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.8f, 0.2f, 0.2f);
            Button closeBtn = btnGO.AddComponent<Button>();
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.4f, 0.05f);
            btnRT.anchorMax = new Vector2(0.6f, 0.15f);
            btnRT.sizeDelta = Vector2.zero;

            GameObject btnTxtGO = new GameObject("BtnText");
            btnTxtGO.transform.SetParent(btnGO.transform, false);
            Text btnTxt = btnTxtGO.AddComponent<Text>();
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize = 20;
            btnTxt.color = Color.white;
            btnTxt.text = "Đóng (ESC)";
            btnTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform btRT = btnTxtGO.GetComponent<RectTransform>();
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.sizeDelta = Vector2.zero;

            var panelUI = parent.AddComponent<SimplePanelUI>();
            panelUI.panelRoot = panelRoot;
            panelUI.titleText = titleTxt;
            panelUI.bodyText = bodyTxt;
            panelUI.closeButton = closeBtn;
            panelRoot.SetActive(false);
        }

        // ================= 2. WORLD (ARCHITECTURE & ENVIRONMENT) =================
        private static void BuildWorldContent(GameObject parent)
        {
            // Architecture (Hollow Exterior Shells)
            GameObject goArch = CreateChild(parent, "Architecture");
            BuildArchitectureHollow(goArch);

            // Complete Multi-Floor Interiors
            GameObject goInt = CreateChild(parent, "Interiors");
            BuildMultiFloorInteriors(goInt);

            // Ground & Plazas
            GameObject goGround = CreateChild(parent, "Ground");
            BuildGround(goGround);

            // Vegetation
            GameObject goVeg = CreateChild(parent, "Vegetation");
            BuildVegetation(goVeg);

            // Parking
            GameObject goPark = CreateChild(parent, "Parking");
            BuildParking(goPark);

            // Infrastructure
            GameObject goInfra = CreateChild(parent, "Infrastructure");
            BuildInfrastructure(goInfra);

            // Props
            GameObject goProps = CreateChild(parent, "Props");
            BuildProps(goProps);

            // Background City
            GameObject goCity = CreateChild(parent, "BackgroundCity");
            BuildBackgroundCity(goCity);
        }

        private static void BuildArchitectureHollow(GameObject parent)
        {
            // --- BLDG_G_Main (X: 24, Y: 0, Z: 10) ---
            GameObject bldgG = CreateChild(parent, "BLDG_G_Main");
            bldgG.transform.localPosition = new Vector3(24.0f, 0.0f, 10.0f);

            float wG = 16.0f, hG = 42.0f, dG = 44.0f;
            float tW = 0.35f; // Wall thickness

            // Hollow Facade Walls (Upper floors from Y = 4.0 to 42.0)
            float hUpper = hG - 4.0f;
            float yCenterUpper = 4.0f + hUpper * 0.5f;

            // West Facade (overlooking courtyard) with window bands
            GameObject wallGW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGW.name = "BLDG_G_Facade_West";
            wallGW.transform.SetParent(bldgG.transform, false);
            wallGW.transform.localPosition = new Vector3(-wG * 0.5f + tW * 0.5f, yCenterUpper, 0);
            wallGW.transform.localScale = new Vector3(tW, hUpper, dG);
            wallGW.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // West Facade Glass horizontal bands
            for (int f = 2; f <= 9; f++)
            {
                float y = (f - 1) * 4.0f + 1.8f;
                GameObject glassBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glassBand.name = $"GlassBand_F{f}";
                glassBand.transform.SetParent(bldgG.transform, false);
                glassBand.transform.localPosition = new Vector3(-wG * 0.5f - 0.02f, y, 0);
                glassBand.transform.localScale = new Vector3(0.08f, 1.6f, dG * 0.85f);
                glassBand.GetComponent<MeshRenderer>().sharedMaterial = m_G_Glass;
            }

            // East Facade (rear)
            GameObject wallGE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGE.name = "BLDG_G_Facade_East";
            wallGE.transform.SetParent(bldgG.transform, false);
            wallGE.transform.localPosition = new Vector3(wG * 0.5f - tW * 0.5f, yCenterUpper, 0);
            wallGE.transform.localScale = new Vector3(tW, hUpper, dG);
            wallGE.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // North Facade (connecting to Building I)
            GameObject wallGN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGN.name = "BLDG_G_Facade_North";
            wallGN.transform.SetParent(bldgG.transform, false);
            wallGN.transform.localPosition = new Vector3(0, yCenterUpper, dG * 0.5f - tW * 0.5f);
            wallGN.transform.localScale = new Vector3(wG, hUpper, tW);
            wallGN.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // South Facade
            GameObject wallGS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGS.name = "BLDG_G_Facade_South";
            wallGS.transform.SetParent(bldgG.transform, false);
            wallGS.transform.localPosition = new Vector3(0, yCenterUpper, -dG * 0.5f + tW * 0.5f);
            wallGS.transform.localScale = new Vector3(wG, hUpper, tW);
            wallGS.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // East Pillar Ground Level
            GameObject gPillarRear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gPillarRear.name = "BLDG_G_Pillar_East";
            gPillarRear.transform.SetParent(bldgG.transform, false);
            gPillarRear.transform.localPosition = new Vector3(wG * 0.5f - 0.5f, 2.0f, 0);
            gPillarRear.transform.localScale = new Vector3(1.0f, 4.0f, dG);
            gPillarRear.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Roof Parapet
            GameObject gRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gRoof.name = "BLDG_G_Roof_Parapet";
            gRoof.transform.SetParent(bldgG.transform, false);
            gRoof.transform.localPosition = new Vector3(0, hG + 0.6f, 0);
            gRoof.transform.localScale = new Vector3(wG + 0.6f, 1.2f, dG + 0.6f);
            gRoof.GetComponent<MeshRenderer>().sharedMaterial = m_G_Roof;

            // Rooftop Water Tanks
            for (int t = 0; t < 3; t++)
            {
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "BLDG_G_Rooftop_WaterTank_" + t;
                tank.transform.SetParent(bldgG.transform, false);
                tank.transform.localPosition = new Vector3(-2.0f + t * 2.5f, hG + 2.4f, 12.0f);
                tank.transform.localScale = new Vector3(1.8f, 1.6f, 1.8f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = (t == 1) ? m_WaterTankBlack : m_WaterTankStainless;
            }

            // --- BLDG_I_Main (X: 18, Y: 0, Z: 48) ---
            GameObject bldgI = CreateChild(parent, "BLDG_I_Main");
            bldgI.transform.localPosition = new Vector3(18.0f, 0.0f, 48.0f);

            float wI = 46.0f, hI = 52.0f, dI = 16.0f;
            GameObject coreI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coreI.name = "BLDG_I_Core_Structure";
            coreI.transform.SetParent(bldgI.transform, false);
            coreI.transform.localPosition = new Vector3(0, hI * 0.5f, 0);
            coreI.transform.localScale = new Vector3(wI, hI, dI);
            coreI.GetComponent<MeshRenderer>().sharedMaterial = m_I_Wall;

            GameObject tumI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tumI.name = "BLDG_I_Elevator_Penthouse";
            tumI.transform.SetParent(bldgI.transform, false);
            tumI.transform.localPosition = new Vector3(0, hI + 2.0f, 0);
            tumI.transform.localScale = new Vector3(8.0f, 4.0f, 6.0f);
            tumI.GetComponent<MeshRenderer>().sharedMaterial = m_I_Wall;

            // --- BLDG_C_Main (X: 0, Y: 0, Z: 18) ---
            GameObject bldgC = CreateChild(parent, "BLDG_C_Main");
            bldgC.transform.localPosition = new Vector3(0.0f, 0.0f, 18.0f);

            float wC = 14.0f, hC = 20.0f, dC = 28.0f;
            GameObject coreC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coreC.name = "BLDG_C_Core_Structure";
            coreC.transform.SetParent(bldgC.transform, false);
            coreC.transform.localPosition = new Vector3(0, hC * 0.5f, 0);
            coreC.transform.localScale = new Vector3(wC, hC, dC);
            coreC.GetComponent<MeshRenderer>().sharedMaterial = m_C_Wall;

            for (int f = 0; f < 4; f++)
            {
                GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fin.name = "BLDG_C_Mint_Fin_" + f;
                fin.transform.SetParent(bldgC.transform, false);
                fin.transform.localPosition = new Vector3(wC * 0.5f + 0.15f, hC * 0.5f, -dC * 0.35f + f * 5.0f);
                fin.transform.localScale = new Vector3(0.3f, hC, 0.45f);
                fin.GetComponent<MeshRenderer>().sharedMaterial = m_C_Mint;
            }

            // --- BLDG_LowPodium (X: 18, Y: 0, Z: -6) ---
            GameObject podium = CreateChild(parent, "BLDG_LowPodium");
            podium.transform.localPosition = new Vector3(18.0f, 0.0f, -6.0f);

            float wP = 26.0f, hP = 9.8f, dP = 12.0f;
            GameObject upperP = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperP.name = "Podium_Upper_Structure";
            upperP.transform.SetParent(podium.transform, false);
            upperP.transform.localPosition = new Vector3(0, 6.8f, 0);
            upperP.transform.localScale = new Vector3(wP, 6.0f, dP);
            upperP.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject roofP = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofP.name = "Podium_Roof";
            roofP.transform.SetParent(podium.transform, false);
            roofP.transform.localPosition = new Vector3(0, hP + 0.1f, 0);
            roofP.transform.localScale = new Vector3(wP + 0.4f, 0.2f, dP + 0.4f);
            roofP.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

            GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "Podium_EastWall";
            eastWall.transform.SetParent(podium.transform, false);
            eastWall.transform.localPosition = new Vector3(wP * 0.5f - 0.2f, 1.9f, 0);
            eastWall.transform.localScale = new Vector3(0.4f, 3.8f, dP);
            eastWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject tLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tLeft.name = "Turquoise_Left";
            tLeft.transform.SetParent(podium.transform, false);
            tLeft.transform.localPosition = new Vector3(-wP * 0.5f + 1.5f, hP * 0.5f, -dP * 0.5f - 0.15f);
            tLeft.transform.localScale = new Vector3(3.2f, hP + 0.2f, 0.5f);
            tLeft.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Turquoise;

            GameObject tRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tRight.name = "Turquoise_Right";
            tRight.transform.SetParent(podium.transform, false);
            tRight.transform.localPosition = new Vector3(wP * 0.5f - 1.5f, hP * 0.5f, -dP * 0.5f - 0.15f);
            tRight.transform.localScale = new Vector3(3.2f, hP + 0.2f, 0.5f);
            tRight.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Turquoise;
        }

        // ================= MULTI-FLOOR INTERIORS =================
        private static void BuildMultiFloorInteriors(GameObject parent)
        {
            GameObject bldgG_Int = CreateChild(parent, "Building_G_Interior");
            bldgG_Int.transform.localPosition = new Vector3(24.0f, 0.0f, 10.0f); // Centered with BLDG_G_Main

            // 1. Ground Floor (Floor 01)
            BuildFloor01_Ground(bldgG_Int);

            // 2. Main Continuous Stair Core (Connecting Floor 1 -> Floor 2 -> Floor 3 -> Floor 4)
            BuildContinuousStairCore(bldgG_Int);

            // 3. Floor 02 (Playable Standard Classroom G-201, Medium G-202, Background G-203/204)
            BuildFloor02(bldgG_Int);

            // 4. Floor 03 (Playable Large Lecture Hall G-301, Seminar G-302, Background G-303/304)
            BuildFloor03(bldgG_Int);

            // 5. Floor 04 (Playable Advanced Computer/IT Lab G-401, Hardware G-402, Background)
            BuildFloor04(bldgG_Int);

            // 6. Upper Floor Slabs & Roof Cap
            BuildUpperFloorsStructural(bldgG_Int);

            // 7. Building C & I Representative Academic Interiors
            BuildBuildingCInterior(parent);
            BuildBuildingIInterior(parent);
        }

        // --- FLOOR 01 (GROUND FLOOR) ---
        private static void BuildFloor01_Ground(GameObject parent)
        {
            GameObject f1 = CreateChild(parent, "Floor_01");

            // Spatial Trigger
            GameObject trg = CreateChild(f1, "TRG_Floor_01");
            trg.transform.localPosition = new Vector3(0, 2.0f, -4.0f);
            BoxCollider col = trg.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(15.0f, 3.8f, 36.0f);
            var rt = trg.AddComponent<RoomTrigger>();
            rt.buildingName = "Tòa G";
            rt.floorNumber = 1;
            rt.roomId = "G_Floor01";
            rt.roomDisplayName = "Sảnh Tầng 1";

            // Lobby (Zone A)
            GameObject lobby = CreateChild(f1, "INT_Lobby_G");
            GameObject floorA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorA.name = "Floor_Lobby";
            floorA.transform.SetParent(lobby.transform, false);
            floorA.transform.localPosition = new Vector3(-3.5f, -0.05f, -1.0f);
            floorA.transform.localScale = new Vector3(8.0f, 0.10f, 12.0f);
            floorA.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            floorA.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            // Ceiling slats
            for (int i = 0; i < 18; i++)
            {
                float z = -6.5f + i * 0.6f;
                GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slat.name = "CeilingSlat_" + i;
                slat.transform.SetParent(lobby.transform, false);
                slat.transform.localPosition = new Vector3(-3.5f, 3.65f, z);
                slat.transform.localScale = new Vector3(7.8f, 0.12f, 0.08f);
                slat.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
            }

            // Waiting Lounge (Zone B)
            GameObject lounge = CreateChild(f1, "INT_Lounge_G");
            GameObject floorB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorB.name = "Floor_Lounge";
            floorB.transform.SetParent(lounge.transform, false);
            floorB.transform.localPosition = new Vector3(3.5f, -0.05f, -1.0f);
            floorB.transform.localScale = new Vector3(7.0f, 0.10f, 12.0f);
            floorB.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            floorB.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            GameObject media = GameObject.CreatePrimitive(PrimitiveType.Cube);
            media.name = "Media_LED_Display";
            media.transform.SetParent(lounge.transform, false);
            media.transform.localPosition = new Vector3(6.8f, 2.0f, -1.0f);
            media.transform.localScale = new Vector3(0.10f, 2.4f, 4.5f);
            media.GetComponent<MeshRenderer>().sharedMaterial = m_LEDDisplay;

            for (int s = 0; s < 2; s++)
            {
                GameObject sofa = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sofa.name = "Sofa_" + s;
                sofa.transform.SetParent(lounge.transform, false);
                sofa.transform.localPosition = new Vector3(2.5f, 0.40f, -3.0f + s * 4.0f);
                sofa.transform.localScale = new Vector3(1.2f, 0.75f, 2.4f);
                sofa.GetComponent<MeshRenderer>().sharedMaterial = m_CyanSofa;
            }

            // Reception Desks
            for (int d = 0; d < 2; d++)
            {
                GameObject rDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rDesk.name = "ReceptionDesk_" + d;
                rDesk.transform.SetParent(lobby.transform, false);
                rDesk.transform.localPosition = new Vector3(-2.0f + d * 3.8f, 0.55f, 0.5f);
                rDesk.transform.localScale = new Vector3(2.6f, 1.1f, 0.90f);
                rDesk.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Turquoise;
            }

            // Ground Training Lab (Zone C)
            GameObject lab = CreateChild(f1, "INT_ComputerLab_G");
            GameObject floorC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorC.name = "Floor_Lab_BlackMarble";
            floorC.transform.SetParent(lab.transform, false);
            floorC.transform.localPosition = new Vector3(0, -0.05f, -12.5f);
            floorC.transform.localScale = new Vector3(14.0f, 0.10f, 11.0f);
            floorC.GetComponent<MeshRenderer>().sharedMaterial = m_BlackMarble;
            floorC.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            // Workstation with INT_Computer_01
            GameObject deskPC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deskPC.name = "INT_Computer_01";
            deskPC.layer = LayerMask.NameToLayer("Interactable");
            deskPC.transform.SetParent(lab.transform, false);
            deskPC.transform.localPosition = new Vector3(-3.2f, 0.38f, -10.0f);
            deskPC.transform.localScale = new Vector3(2.4f, 0.76f, 0.90f);
            deskPC.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
            var compWid = deskPC.AddComponent<WorldObjectID>();
            compWid.objectId = "G_COMPUTER_001";
            compWid.displayName = "Máy trạm IUH CNTT #01";
            deskPC.AddComponent<ComputerController>();

            GameObject mon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mon.name = "Monitor";
            mon.transform.SetParent(deskPC.transform, false);
            mon.transform.localPosition = new Vector3(0, 0.70f, -0.15f);
            mon.transform.localScale = new Vector3(0.75f, 0.48f, 0.08f);
            mon.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

            // Stairway Entry Signage in Lobby directing to Stair Core
            GameObject stairSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairSign.name = "Sign_StairwayEntrance";
            stairSign.transform.SetParent(f1.transform, false);
            stairSign.transform.localPosition = new Vector3(0, 3.2f, 7.5f);
            stairSign.transform.localScale = new Vector3(3.2f, 0.45f, 0.10f);
            stairSign.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;
        }

        // --- CONTINUOUS STAIR CORE (DOG-LEG, 4 FLOORS) ---
        private static void BuildContinuousStairCore(GameObject parent)
        {
            GameObject stairCore = CreateChild(parent, "StairCore_North");
            // Located at north end of Building G (local Z = 14 to 20, X = -3 to +3)
            stairCore.transform.localPosition = new Vector3(0, 0, 14.5f);

            // Wall Enclosures for Stairwell Shaft
            GameObject stairWallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairWallW.name = "StairShaft_Wall_West";
            stairWallW.transform.SetParent(stairCore.transform, false);
            stairWallW.transform.localPosition = new Vector3(-3.6f, 8.0f, 0);
            stairWallW.transform.localScale = new Vector3(0.25f, 16.0f, 7.0f);
            stairWallW.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject stairWallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairWallE.name = "StairShaft_Wall_East";
            stairWallE.transform.SetParent(stairCore.transform, false);
            stairWallE.transform.localPosition = new Vector3(3.6f, 8.0f, 0);
            stairWallE.transform.localScale = new Vector3(0.25f, 16.0f, 7.0f);
            stairWallE.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject stairWallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairWallN.name = "StairShaft_Wall_North";
            stairWallN.transform.SetParent(stairCore.transform, false);
            stairWallN.transform.localPosition = new Vector3(0, 8.0f, 3.5f);
            stairWallN.transform.localScale = new Vector3(7.4f, 16.0f, 0.25f);
            stairWallN.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Generate Flights for Floors 1->2, 2->3, 3->4
            for (int f = 1; f <= 3; f++)
            {
                float baseHeight = (f - 1) * 4.0f;
                BuildStairLevel(stairCore, f, baseHeight);
            }

            // Top Floor restricted door at Floor 4 landing going up
            GameObject topRestrictedDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topRestrictedDoor.name = "Door_Restricted_Roof";
            topRestrictedDoor.transform.SetParent(stairCore.transform, false);
            topRestrictedDoor.transform.localPosition = new Vector3(-1.6f, 13.5f, 0f);
            topRestrictedDoor.transform.localScale = new Vector3(1.6f, 2.4f, 0.12f);
            topRestrictedDoor.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject topSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topSign.name = "Restricted_Sign";
            topSign.transform.SetParent(topRestrictedDoor.transform, false);
            topSign.transform.localPosition = new Vector3(0, 0.4f, -0.08f);
            topSign.transform.localScale = new Vector3(1.2f, 0.35f, 0.05f);
            topSign.GetComponent<MeshRenderer>().sharedMaterial = m_SignNotice;
        }

        private static void BuildStairLevel(GameObject parent, int floorNum, float baseY)
        {
            GameObject levelGO = CreateChild(parent, $"Stair_Floor_{floorNum}_to_{floorNum + 1}");

            float flightWidth = 1.6f;
            float stepRise = 2.0f / 12f; // 0.166m
            float stepTread = 3.0f / 12f; // 0.25m

            // --- FLIGHT 1: Climbs from Z = -2.5 to Z = 0.5 (Y: baseY to baseY + 2.0) on West side (X = -1.6) ---
            GameObject flight1 = CreateChild(levelGO, "Flight_Up_Part1");
            flight1.transform.localPosition = new Vector3(-1.6f, baseY, -1.0f);

            for (int s = 0; s < 12; s++)
            {
                GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Step_{s}";
                step.transform.SetParent(flight1.transform, false);
                step.transform.localPosition = new Vector3(0, s * stepRise + stepRise * 0.5f, -1.5f + s * stepTread + stepTread * 0.5f);
                step.transform.localScale = new Vector3(flightWidth, stepRise, stepTread);
                step.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
                Object.DestroyImmediate(step.GetComponent<Collider>()); // Avoid jagged step collision
            }

            // Smooth Ramp Collider for Flight 1
            GameObject ramp1 = new GameObject("RampCollider_Flight1");
            ramp1.transform.SetParent(flight1.transform, false);
            ramp1.transform.localPosition = new Vector3(0, 1.0f, 0);
            float angle1 = Mathf.Atan2(2.0f, 3.0f) * Mathf.Rad2Deg;
            ramp1.transform.localRotation = Quaternion.Euler(-angle1, 0, 0);
            BoxCollider boxRamp1 = ramp1.AddComponent<BoxCollider>();
            boxRamp1.size = new Vector3(flightWidth, 0.08f, Mathf.Sqrt(2.0f * 2.0f + 3.0f * 3.0f));

            // Handrail West
            GameObject rail1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail1.name = "Handrail_West";
            rail1.transform.SetParent(flight1.transform, false);
            rail1.transform.localPosition = new Vector3(-flightWidth * 0.5f + 0.05f, 1.95f, 0);
            rail1.transform.localRotation = Quaternion.Euler(-angle1, 0, 0);
            rail1.transform.localScale = new Vector3(0.06f, 0.06f, 3.6f);
            rail1.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            Object.DestroyImmediate(rail1.GetComponent<Collider>());

            // --- MID LANDING: at Y = baseY + 2.0 (Z = 1.8f to 3.2f, X = -3.2 to +3.2) ---
            GameObject midLanding = GameObject.CreatePrimitive(PrimitiveType.Cube);
            midLanding.name = "MidLanding_Platform";
            midLanding.transform.SetParent(levelGO.transform, false);
            midLanding.transform.localPosition = new Vector3(0, baseY + 2.0f - 0.08f, 2.4f);
            midLanding.transform.localScale = new Vector3(6.8f, 0.16f, 1.8f);
            midLanding.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            midLanding.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            // Landing Window overlooking North
            GameObject landingWin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landingWin.name = "Landing_NorthWindow";
            landingWin.transform.SetParent(levelGO.transform, false);
            landingWin.transform.localPosition = new Vector3(0, baseY + 3.2f, 3.32f);
            landingWin.transform.localScale = new Vector3(3.6f, 1.8f, 0.08f);
            landingWin.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            // --- FLIGHT 2: Turns 180°, Climbs from Z = 1.5 to Z = -1.5 (Y: baseY + 2.0 to baseY + 4.0) on East side (X = 1.6) ---
            GameObject flight2 = CreateChild(levelGO, "Flight_Up_Part2");
            flight2.transform.localPosition = new Vector3(1.6f, baseY + 2.0f, 0.0f);

            for (int s = 0; s < 12; s++)
            {
                GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Step_{s}";
                step.transform.SetParent(flight2.transform, false);
                step.transform.localPosition = new Vector3(0, s * stepRise + stepRise * 0.5f, 1.5f - s * stepTread - stepTread * 0.5f);
                step.transform.localScale = new Vector3(flightWidth, stepRise, stepTread);
                step.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
                Object.DestroyImmediate(step.GetComponent<Collider>());
            }

            // Smooth Ramp Collider for Flight 2
            GameObject ramp2 = new GameObject("RampCollider_Flight2");
            ramp2.transform.SetParent(flight2.transform, false);
            ramp2.transform.localPosition = new Vector3(0, 1.0f, 0);
            ramp2.transform.localRotation = Quaternion.Euler(angle1, 0, 0);
            BoxCollider boxRamp2 = ramp2.AddComponent<BoxCollider>();
            boxRamp2.size = new Vector3(flightWidth, 0.08f, Mathf.Sqrt(2.0f * 2.0f + 3.0f * 3.0f));

            // Handrail East
            GameObject rail2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail2.name = "Handrail_East";
            rail2.transform.SetParent(flight2.transform, false);
            rail2.transform.localPosition = new Vector3(flightWidth * 0.5f - 0.05f, 1.95f, 0);
            rail2.transform.localRotation = Quaternion.Euler(angle1, 0, 0);
            rail2.transform.localScale = new Vector3(0.06f, 0.06f, 3.6f);
            rail2.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            Object.DestroyImmediate(rail2.GetComponent<Collider>());

            // --- FLOOR LANDING (Connecting to Corridor at baseY + 4.0) ---
            GameObject floorLanding = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorLanding.name = $"FloorLanding_Level_{floorNum + 1}";
            floorLanding.transform.SetParent(levelGO.transform, false);
            floorLanding.transform.localPosition = new Vector3(0, baseY + 4.0f - 0.08f, -2.4f);
            floorLanding.transform.localScale = new Vector3(6.8f, 0.16f, 1.8f);
            floorLanding.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            floorLanding.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            // Directional Floor Number Sign on landing wall
            GameObject signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = $"Sign_Floor_{floorNum + 1}";
            signBoard.transform.SetParent(levelGO.transform, false);
            signBoard.transform.localPosition = new Vector3(0, baseY + 5.5f, -3.25f);
            signBoard.transform.localScale = new Vector3(2.4f, 0.65f, 0.08f);
            signBoard.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;
        }

        // --- FLOOR 02 (STANDARD CLASSROOMS) ---
        private static void BuildFloor02(GameObject parent)
        {
            GameObject f2 = CreateChild(parent, "Floor_02");
            float yFloor = 4.0f;

            // Floor Slab
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "FloorSlab_F2";
            slab.transform.SetParent(f2.transform, false);
            slab.transform.localPosition = new Vector3(0, yFloor - 0.125f, 0);
            slab.transform.localScale = new Vector3(15.2f, 0.25f, 43.0f);
            slab.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            slab.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            // Ceiling with Recessed Lights
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_F2";
            ceiling.transform.SetParent(f2.transform, false);
            ceiling.transform.localPosition = new Vector3(0, yFloor + 3.75f, 0);
            ceiling.transform.localScale = new Vector3(15.2f, 0.15f, 43.0f);
            ceiling.GetComponent<MeshRenderer>().sharedMaterial = m_CeilingGrid;

            // Corridor (Width 2.4m, runs from Z = 12 down to Z = -18)
            GameObject corridor = CreateChild(f2, "Corridor_F2");
            BuildCorridorSpine(corridor, yFloor, 2);

            // Playable Classroom G-201 (West side, Z: 2 to 11)
            BuildClassroom_G201(f2, yFloor);

            // Medium Classroom G-202 (East side, Z: 2 to 11)
            BuildClassroom_G202(f2, yFloor);

            // Background Classrooms G-203 & G-204 (Z: -8 to -17)
            BuildBackgroundClassroom(f2, yFloor, "G-203", new Vector3(-3.8f, yFloor, -12.5f), "Phòng Học G-203");
            BuildBackgroundClassroom(f2, yFloor, "G-204", new Vector3(3.8f, yFloor, -12.5f), "Phòng Học G-204");
        }

        // --- FLOOR 03 (LARGE LECTURE & SEMINAR) ---
        private static void BuildFloor03(GameObject parent)
        {
            GameObject f3 = CreateChild(parent, "Floor_03");
            float yFloor = 8.0f;

            // Floor Slab
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "FloorSlab_F3";
            slab.transform.SetParent(f3.transform, false);
            slab.transform.localPosition = new Vector3(0, yFloor - 0.125f, 0);
            slab.transform.localScale = new Vector3(15.2f, 0.25f, 43.0f);
            slab.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            slab.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            // Ceiling
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_F3";
            ceiling.transform.SetParent(f3.transform, false);
            ceiling.transform.localPosition = new Vector3(0, yFloor + 3.75f, 0);
            ceiling.transform.localScale = new Vector3(15.2f, 0.15f, 43.0f);
            ceiling.GetComponent<MeshRenderer>().sharedMaterial = m_CeilingGrid;

            // Corridor
            GameObject corridor = CreateChild(f3, "Corridor_F3");
            BuildCorridorSpine(corridor, yFloor, 3);

            // Playable Large Lecture Hall G-301
            BuildClassroom_G301(f3, yFloor);

            // Seminar Room G-302
            BuildClassroom_G302(f3, yFloor);

            // Background Classrooms
            BuildBackgroundClassroom(f3, yFloor, "G-303", new Vector3(-3.8f, yFloor, -12.5f), "Giảng Đường G-303");
            BuildBackgroundClassroom(f3, yFloor, "G-304", new Vector3(3.8f, yFloor, -12.5f), "Phòng Hội Thảo G-304");
        }

        // --- FLOOR 04 (ADVANCED COMPUTER & NETWORK LAB) ---
        private static void BuildFloor04(GameObject parent)
        {
            GameObject f4 = CreateChild(parent, "Floor_04");
            float yFloor = 12.0f;

            // Floor Slab
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "FloorSlab_F4";
            slab.transform.SetParent(f4.transform, false);
            slab.transform.localPosition = new Vector3(0, yFloor - 0.125f, 0);
            slab.transform.localScale = new Vector3(15.2f, 0.25f, 43.0f);
            slab.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            slab.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            // Ceiling
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_F4";
            ceiling.transform.SetParent(f4.transform, false);
            ceiling.transform.localPosition = new Vector3(0, yFloor + 3.75f, 0);
            ceiling.transform.localScale = new Vector3(15.2f, 0.15f, 43.0f);
            ceiling.GetComponent<MeshRenderer>().sharedMaterial = m_CeilingGrid;

            // Corridor
            GameObject corridor = CreateChild(f4, "Corridor_F4");
            BuildCorridorSpine(corridor, yFloor, 4);

            // Playable Advanced Computer Lab G-401
            BuildClassroom_G401(f4, yFloor);

            // Hardware Lab G-402
            BuildClassroom_G402(f4, yFloor);

            // Background Labs
            BuildBackgroundClassroom(f4, yFloor, "G-403", new Vector3(-3.8f, yFloor, -12.5f), "Lab IoT G-403");
            BuildBackgroundClassroom(f4, yFloor, "G-404", new Vector3(3.8f, yFloor, -12.5f), "Lab An Ninh Mạng G-404");
        }

        // --- CORRIDOR HELPER ---
        private static void BuildCorridorSpine(GameObject corridorGO, float yFloor, int floorNum)
        {
            // Corridor Floor Trigger
            GameObject trg = CreateChild(corridorGO, $"TRG_Floor_{floorNum:D2}");
            trg.transform.localPosition = new Vector3(0, yFloor + 1.8f, 0);
            BoxCollider col = trg.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2.6f, 3.6f, 32.0f);
            var rt = trg.AddComponent<RoomTrigger>();
            rt.buildingName = "Tòa G";
            rt.floorNumber = floorNum;
            rt.roomId = $"G_Corridor_F{floorNum}";
            rt.roomDisplayName = $"Hành lang Tầng {floorNum}";

            // Ceiling Lights along corridor
            for (int l = 0; l < 6; l++)
            {
                float z = 10.0f - l * 4.8f;
                GameObject lightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lightPanel.name = $"LED_Panel_{l}";
                lightPanel.transform.SetParent(corridorGO.transform, false);
                lightPanel.transform.localPosition = new Vector3(0, yFloor + 3.68f, z);
                lightPanel.transform.localScale = new Vector3(0.60f, 0.05f, 1.2f);
                lightPanel.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;
                Object.DestroyImmediate(lightPanel.GetComponent<Collider>());
            }

            // Directional Header Signboard
            GameObject headerSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headerSign.name = "Header_Signboard";
            headerSign.transform.SetParent(corridorGO.transform, false);
            headerSign.transform.localPosition = new Vector3(0, yFloor + 3.2f, 11.5f);
            headerSign.transform.localScale = new Vector3(2.4f, 0.45f, 0.08f);
            headerSign.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            // Fire Safety Cabinet
            GameObject fireCab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fireCab.name = "FireCabinet";
            fireCab.transform.SetParent(corridorGO.transform, false);
            fireCab.transform.localPosition = new Vector3(-1.25f, yFloor + 1.2f, 0);
            fireCab.transform.localScale = new Vector3(0.12f, 1.1f, 0.75f);
            fireCab.GetComponent<MeshRenderer>().sharedMaterial = m_FireCabinet;
        }

        // --- PLAYABLE CLASSROOM G-201 (STANDARD) ---
        private static void BuildClassroom_G201(GameObject parent, float yFloor)
        {
            GameObject room = CreateChild(parent, "ROOM_G_201");
            room.transform.localPosition = new Vector3(-4.4f, yFloor, 6.5f);

            // Spatial Trigger
            GameObject trg = CreateChild(room, "TRG_G_201");
            trg.transform.localPosition = new Vector3(0, 1.8f, 0);
            BoxCollider col = trg.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(6.5f, 3.6f, 8.5f);
            var rt = trg.AddComponent<RoomTrigger>();
            rt.buildingName = "Tòa G";
            rt.floorNumber = 2;
            rt.roomId = "G-201";
            rt.roomDisplayName = "Phòng học G-201";

            // Partition Wall East (facing corridor) with Doorway
            GameObject wallE_South = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE_South.name = "Wall_East_S";
            wallE_South.transform.SetParent(room.transform, false);
            wallE_South.transform.localPosition = new Vector3(3.2f, 1.8f, -2.5f);
            wallE_South.transform.localScale = new Vector3(0.18f, 3.6f, 4.0f);
            wallE_South.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject wallE_North = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE_North.name = "Wall_East_N";
            wallE_North.transform.SetParent(room.transform, false);
            wallE_North.transform.localPosition = new Vector3(3.2f, 1.8f, 3.0f);
            wallE_North.transform.localScale = new Vector3(0.18f, 3.6f, 3.0f);
            wallE_North.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject wallE_Header = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE_Header.name = "Wall_East_Header";
            wallE_Header.transform.SetParent(room.transform, false);
            wallE_Header.transform.localPosition = new Vector3(3.2f, 3.1f, 0.25f);
            wallE_Header.transform.localScale = new Vector3(0.18f, 1.0f, 1.5f);
            wallE_Header.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Partition Wall North & South
            GameObject wallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallN.name = "Wall_North_Dividing";
            wallN.transform.SetParent(room.transform, false);
            wallN.transform.localPosition = new Vector3(0, 1.8f, 4.5f);
            wallN.transform.localScale = new Vector3(6.5f, 3.6f, 0.18f);
            wallN.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject wallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallS.name = "Wall_South_Dividing";
            wallS.transform.SetParent(room.transform, false);
            wallS.transform.localPosition = new Vector3(0, 1.8f, -4.5f);
            wallS.transform.localScale = new Vector3(6.5f, 3.6f, 0.18f);
            wallS.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Classroom Door with DoorController
            GameObject doorGO = CreateChild(room, "Door_G_201");
            doorGO.transform.localPosition = new Vector3(3.2f, 0, 0.25f);
            doorGO.layer = LayerMask.NameToLayer("Interactable");

            GameObject doorLeaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorLeaf.name = "DoorLeaf";
            doorLeaf.transform.SetParent(doorGO.transform, false);
            doorLeaf.transform.localPosition = new Vector3(0, 1.25f, 0);
            doorLeaf.transform.localScale = new Vector3(0.08f, 2.5f, 1.35f);
            doorLeaf.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            BoxCollider dCol = doorGO.AddComponent<BoxCollider>();
            dCol.center = new Vector3(0, 1.25f, 0);
            dCol.size = new Vector3(0.8f, 2.5f, 1.6f);

            var dWid = doorGO.AddComponent<WorldObjectID>();
            dWid.objectId = "DOOR_G_201";
            dWid.displayName = "Cửa Phòng Học G-201";

            var dCtrl = doorGO.AddComponent<DoorController>();
            dCtrl.doorName = "Phòng G-201";
            dCtrl.doorLeafLeft = doorLeaf.transform;
            dCtrl.autoOpen = false;
            dCtrl.openOffset = new Vector3(0, 0, 1.3f);

            // Room Number Plate
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "RoomPlate_G201";
            plate.transform.SetParent(room.transform, false);
            plate.transform.localPosition = new Vector3(3.1f, 1.9f, 1.2f);
            plate.transform.localScale = new Vector3(0.04f, 0.25f, 0.45f);
            plate.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            // FRONT TEACHING ZONE (Facing North at Z = 4.3f)
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Teaching_Board";
            board.transform.SetParent(room.transform, false);
            board.transform.localPosition = new Vector3(0, 1.9f, 4.38f);
            board.transform.localScale = new Vector3(4.5f, 1.35f, 0.06f);
            board.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite; // Whiteboard

            // Marker Tray
            GameObject tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "Marker_Tray";
            tray.transform.SetParent(board.transform, false);
            tray.transform.localPosition = new Vector3(0, -0.52f, 0.05f);
            tray.transform.localScale = new Vector3(0.95f, 0.04f, 0.12f);
            tray.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            // Objective Anchor
            GameObject objBoard = CreateChild(room, "OBJ_G_201_Board");
            objBoard.transform.localPosition = new Vector3(0, 1.2f, 3.8f);

            // Teacher Desk
            GameObject tDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tDesk.name = "Teacher_Desk";
            tDesk.transform.SetParent(room.transform, false);
            tDesk.transform.localPosition = new Vector3(1.6f, 0.38f, 3.2f);
            tDesk.transform.localScale = new Vector3(1.4f, 0.76f, 0.70f);
            tDesk.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            GameObject tChair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tChair.name = "Teacher_Chair";
            tChair.transform.SetParent(room.transform, false);
            tChair.transform.localPosition = new Vector3(1.6f, 0.24f, 3.75f);
            tChair.transform.localScale = new Vector3(0.50f, 0.48f, 0.50f);
            tChair.GetComponent<MeshRenderer>().sharedMaterial = m_NavyChair;

            // Teacher Laptop
            GameObject tPC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tPC.name = "Teacher_Laptop";
            tPC.transform.SetParent(tDesk.transform, false);
            tPC.transform.localPosition = new Vector3(0, 0.44f, 0);
            tPC.transform.localScale = new Vector3(0.38f, 0.22f, 0.28f);
            tPC.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

            // Ceiling Projector
            GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            proj.name = "Ceiling_Projector";
            proj.transform.SetParent(room.transform, false);
            proj.transform.localPosition = new Vector3(0, 3.1f, 1.2f);
            proj.transform.localScale = new Vector3(0.40f, 0.18f, 0.35f);
            proj.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            Object.DestroyImmediate(proj.GetComponent<Collider>());

            // STUDENT SEATING ROWS (4 Rows of Desks, maintaining walking aisle)
            GameObject studentZone = CreateChild(room, "StudentArea");
            System.Random rnd = new System.Random(201);

            for (int r = 0; r < 4; r++)
            {
                float zDesk = 1.6f - r * 1.4f;

                // Left Desk & Right Desk separated by center aisle
                for (int side = -1; side <= 1; side += 2)
                {
                    float xDesk = side * 1.5f;

                    GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    desk.name = $"Desk_R{r}_S{side}";
                    desk.transform.SetParent(studentZone.transform, false);
                    desk.transform.localPosition = new Vector3(xDesk, 0.37f, zDesk);
                    desk.transform.localScale = new Vector3(1.30f, 0.74f, 0.50f);
                    desk.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

                    // 2 Chairs per desk with natural rotation jitter
                    for (int c = -1; c <= 1; c += 2)
                    {
                        float xChair = xDesk + c * 0.35f;
                        float jitterRot = (float)(rnd.NextDouble() * 3.0 - 1.5);

                        GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        chair.name = $"Chair_R{r}_S{side}_C{c}";
                        chair.transform.SetParent(studentZone.transform, false);
                        chair.transform.localPosition = new Vector3(xChair, 0.24f, zDesk - 0.45f);
                        chair.transform.localRotation = Quaternion.Euler(0, jitterRot, 0);
                        chair.transform.localScale = new Vector3(0.42f, 0.48f, 0.42f);
                        chair.GetComponent<MeshRenderer>().sharedMaterial = m_NavyChair;
                        Object.DestroyImmediate(chair.GetComponent<Collider>()); // Optimize physics

                        // Seat Anchor
                        GameObject seatAnchor = CreateChild(chair, "SeatAnchor");
                        seatAnchor.transform.localPosition = new Vector3(0, 0.25f, 0);
                    }
                }
            }

            // Lighting Fixtures inside classroom
            for (int i = 0; i < 4; i++)
            {
                float xL = (i % 2 == 0) ? -1.5f : 1.5f;
                float zL = (i < 2) ? 1.8f : -1.8f;
                GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fixture.name = $"LightFixture_{i}";
                fixture.transform.SetParent(room.transform, false);
                fixture.transform.localPosition = new Vector3(xL, 3.65f, zL);
                fixture.transform.localScale = new Vector3(0.40f, 0.06f, 1.4f);
                fixture.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;
                Object.DestroyImmediate(fixture.GetComponent<Collider>());
            }
        }

        // --- MEDIUM CLASSROOM G-202 ---
        private static void BuildClassroom_G202(GameObject parent, float yFloor)
        {
            GameObject room = CreateChild(parent, "ROOM_G_202");
            room.transform.localPosition = new Vector3(4.4f, yFloor, 6.5f);

            // Dividing Walls
            GameObject wallW_South = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW_South.name = "Wall_West_S";
            wallW_South.transform.SetParent(room.transform, false);
            wallW_South.transform.localPosition = new Vector3(-3.2f, 1.8f, -2.5f);
            wallW_South.transform.localScale = new Vector3(0.18f, 3.6f, 4.0f);
            wallW_South.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject wallW_North = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW_North.name = "Wall_West_N";
            wallW_North.transform.SetParent(room.transform, false);
            wallW_North.transform.localPosition = new Vector3(-3.2f, 1.8f, 3.0f);
            wallW_North.transform.localScale = new Vector3(0.18f, 3.6f, 3.0f);
            wallW_North.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Room Plate
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "RoomPlate_G202";
            plate.transform.SetParent(room.transform, false);
            plate.transform.localPosition = new Vector3(-3.1f, 1.9f, 1.2f);
            plate.transform.localScale = new Vector3(0.04f, 0.25f, 0.45f);
            plate.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            // Whiteboard & Desks
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Teaching_Board";
            board.transform.SetParent(room.transform, false);
            board.transform.localPosition = new Vector3(0, 1.9f, 4.38f);
            board.transform.localScale = new Vector3(4.5f, 1.35f, 0.06f);
            board.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            for (int r = 0; r < 3; r++)
            {
                float z = 1.6f - r * 1.6f;
                GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                desk.name = $"Desk_{r}";
                desk.transform.SetParent(room.transform, false);
                desk.transform.localPosition = new Vector3(0, 0.37f, z);
                desk.transform.localScale = new Vector3(3.2f, 0.74f, 0.50f);
                desk.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
            }
        }

        // --- PLAYABLE LARGE LECTURE HALL G-301 ---
        private static void BuildClassroom_G301(GameObject parent, float yFloor)
        {
            GameObject room = CreateChild(parent, "ROOM_G_301");
            room.transform.localPosition = new Vector3(-4.4f, yFloor, 6.5f);

            // Spatial Trigger
            GameObject trg = CreateChild(room, "TRG_G_301");
            trg.transform.localPosition = new Vector3(0, 1.8f, 0);
            BoxCollider col = trg.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(6.5f, 3.6f, 8.5f);
            var rt = trg.AddComponent<RoomTrigger>();
            rt.buildingName = "Tòa G";
            rt.floorNumber = 3;
            rt.roomId = "G-301";
            rt.roomDisplayName = "Giảng đường Lớn G-301";

            // Room Plate
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "RoomPlate_G301";
            plate.transform.SetParent(room.transform, false);
            plate.transform.localPosition = new Vector3(3.1f, 1.9f, 1.2f);
            plate.transform.localScale = new Vector3(0.04f, 0.25f, 0.45f);
            plate.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            // Front Large Screen & Lectern
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Large_Projector_Screen";
            screen.transform.SetParent(room.transform, false);
            screen.transform.localPosition = new Vector3(0, 2.1f, 4.38f);
            screen.transform.localScale = new Vector3(5.2f, 2.0f, 0.08f);
            screen.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject lectern = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lectern.name = "Professor_Lectern";
            lectern.transform.SetParent(room.transform, false);
            lectern.transform.localPosition = new Vector3(1.2f, 0.60f, 3.2f);
            lectern.transform.localScale = new Vector3(0.85f, 1.2f, 0.65f);
            lectern.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            CreateAnchor(room, "OBJ_G_301_Lectern", new Vector3(1.2f, 1.2f, 3.2f));

            // Desks
            for (int r = 0; r < 5; r++)
            {
                float z = 1.8f - r * 1.3f;
                for (int s = -1; s <= 1; s += 2)
                {
                    GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    desk.name = $"Desk_R{r}_S{s}";
                    desk.transform.SetParent(room.transform, false);
                    desk.transform.localPosition = new Vector3(s * 1.5f, 0.37f, z);
                    desk.transform.localScale = new Vector3(1.35f, 0.74f, 0.45f);
                    desk.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
                }
            }
        }

        // --- SEMINAR ROOM G-302 ---
        private static void BuildClassroom_G302(GameObject parent, float yFloor)
        {
            GameObject room = CreateChild(parent, "ROOM_G_302");
            room.transform.localPosition = new Vector3(4.4f, yFloor, 6.5f);

            // Conference U-Desk
            GameObject uDeskCenter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uDeskCenter.name = "Conference_Desk_Main";
            uDeskCenter.transform.SetParent(room.transform, false);
            uDeskCenter.transform.localPosition = new Vector3(0, 0.38f, 1.5f);
            uDeskCenter.transform.localScale = new Vector3(3.2f, 0.76f, 0.80f);
            uDeskCenter.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
        }

        // --- PLAYABLE ADVANCED IT LAB G-401 ---
        private static void BuildClassroom_G401(GameObject parent, float yFloor)
        {
            GameObject room = CreateChild(parent, "ROOM_G_401");
            room.transform.localPosition = new Vector3(-4.4f, yFloor, 6.5f);

            // Spatial Trigger
            GameObject trg = CreateChild(room, "TRG_G_401");
            trg.transform.localPosition = new Vector3(0, 1.8f, 0);
            BoxCollider col = trg.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(6.5f, 3.6f, 8.5f);
            var rt = trg.AddComponent<RoomTrigger>();
            rt.buildingName = "Tòa G";
            rt.floorNumber = 4;
            rt.roomId = "G-401";
            rt.roomDisplayName = "Lab Máy Tính Chuyên Ngành G-401";

            // Room Plate
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "RoomPlate_G401";
            plate.transform.SetParent(room.transform, false);
            plate.transform.localPosition = new Vector3(3.1f, 1.9f, 1.2f);
            plate.transform.localScale = new Vector3(0.04f, 0.25f, 0.45f);
            plate.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            // Server Rack Cabinet in Corner
            GameObject serverRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            serverRack.name = "Lab_ServerRack_Cabinet";
            serverRack.transform.SetParent(room.transform, false);
            serverRack.transform.localPosition = new Vector3(-2.6f, 1.2f, 3.6f);
            serverRack.transform.localScale = new Vector3(0.85f, 2.4f, 0.90f);
            serverRack.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            // Blinking LED status bar on Server
            GameObject rackLED = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rackLED.name = "LED_Status";
            rackLED.transform.SetParent(serverRack.transform, false);
            rackLED.transform.localPosition = new Vector3(0.52f, 0.2f, 0);
            rackLED.transform.localScale = new Vector3(0.05f, 1.2f, 0.60f);
            rackLED.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

            CreateAnchor(room, "OBJ_G_401_ServerRack", new Vector3(-2.6f, 1.2f, 3.6f));

            // Smartboard Display
            GameObject smartboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            smartboard.name = "Smartboard_Display";
            smartboard.transform.SetParent(room.transform, false);
            smartboard.transform.localPosition = new Vector3(0.8f, 1.9f, 4.38f);
            smartboard.transform.localScale = new Vector3(3.6f, 1.6f, 0.08f);
            smartboard.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

            // Rows of Dual-Monitor Workstations
            for (int r = 0; r < 3; r++)
            {
                float z = 1.4f - r * 1.8f;
                for (int s = -1; s <= 1; s += 2)
                {
                    float x = s * 1.5f;

                    GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    desk.name = $"Workstation_R{r}_S{s}";
                    desk.transform.SetParent(room.transform, false);
                    desk.transform.localPosition = new Vector3(x, 0.38f, z);
                    desk.transform.localScale = new Vector3(1.35f, 0.76f, 0.75f);
                    desk.GetComponent<MeshRenderer>().sharedMaterial = m_BlackMarble;

                    // Dual Monitors
                    for (int m = -1; m <= 1; m += 2)
                    {
                        GameObject mon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        mon.name = $"Mon_{m}";
                        mon.transform.SetParent(desk.transform, false);
                        mon.transform.localPosition = new Vector3(m * 0.30f, 0.65f, 0.15f);
                        mon.transform.localScale = new Vector3(0.55f, 0.38f, 0.06f);
                        mon.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;
                    }
                }
            }
        }

        // --- HARDWARE LAB G-402 ---
        private static void BuildClassroom_G402(GameObject parent, float yFloor)
        {
            GameObject room = CreateChild(parent, "ROOM_G_402");
            room.transform.localPosition = new Vector3(4.4f, yFloor, 6.5f);

            // Hardware Work Benches
            for (int r = 0; r < 2; r++)
            {
                GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bench.name = $"Electronics_Bench_{r}";
                bench.transform.SetParent(room.transform, false);
                bench.transform.localPosition = new Vector3(0, 0.40f, 1.0f - r * 2.5f);
                bench.transform.localScale = new Vector3(3.4f, 0.80f, 1.0f);
                bench.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }
        }

        // --- BACKGROUND CLASSROOM TEMPLATE ---
        private static void BuildBackgroundClassroom(GameObject parent, float yFloor, string roomId, Vector3 localPos, string displayName)
        {
            GameObject room = CreateChild(parent, "ROOM_" + roomId);
            room.transform.localPosition = localPos;

            // Room Door with Locked DoorController
            GameObject doorGO = CreateChild(room, "Door_" + roomId);
            doorGO.transform.localPosition = new Vector3(localPos.x < 0 ? 3.2f : -3.2f, 0, 0);

            GameObject doorLeaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorLeaf.name = "DoorLeaf";
            doorLeaf.transform.SetParent(doorGO.transform, false);
            doorLeaf.transform.localPosition = new Vector3(0, 1.25f, 0);
            doorLeaf.transform.localScale = new Vector3(0.08f, 2.5f, 1.35f);
            doorLeaf.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            var dCtrl = doorGO.AddComponent<DoorController>();
            dCtrl.doorName = displayName;
            dCtrl.doorLeafLeft = doorLeaf.transform;
            dCtrl.isLocked = true;
            dCtrl.autoOpen = false;

            // Room Plate
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "RoomPlate_" + roomId;
            plate.transform.SetParent(room.transform, false);
            plate.transform.localPosition = new Vector3(localPos.x < 0 ? 3.1f : -3.1f, 1.9f, 0.85f);
            plate.transform.localScale = new Vector3(0.04f, 0.25f, 0.45f);
            plate.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;
        }

        // --- UPPER FLOORS STRUCTURAL ---
        private static void BuildUpperFloorsStructural(GameObject parent)
        {
            // Slabs for Floors 5 to 10
            for (int f = 5; f <= 10; f++)
            {
                float y = (f - 1) * 4.0f;
                GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = $"FloorSlab_F{f:D2}";
                slab.transform.SetParent(parent.transform, false);
                slab.transform.localPosition = new Vector3(0, y - 0.125f, 0);
                slab.transform.localScale = new Vector3(15.2f, 0.25f, 43.0f);
                slab.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
                Object.DestroyImmediate(slab.GetComponent<Collider>());
            }
        }

        // --- BUILDING C & I REPRESENTATIVE INTERIORS ---
        private static void BuildBuildingCInterior(GameObject parent)
        {
            GameObject bldgC_Int = CreateChild(parent, "Building_C_Interior");
            bldgC_Int.transform.localPosition = new Vector3(0.0f, 0.0f, 18.0f);

            // Ground Floor Lobby & Stair Flight
            GameObject slab1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab1.name = "FloorSlab_C_F1";
            slab1.transform.SetParent(bldgC_Int.transform, false);
            slab1.transform.localPosition = new Vector3(0, -0.05f, 0);
            slab1.transform.localScale = new Vector3(13.4f, 0.10f, 27.0f);
            slab1.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Floor 2 Slab
            GameObject slab2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab2.name = "FloorSlab_C_F2";
            slab2.transform.SetParent(bldgC_Int.transform, false);
            slab2.transform.localPosition = new Vector3(0, 3.95f, 0);
            slab2.transform.localScale = new Vector3(13.4f, 0.15f, 27.0f);
            slab2.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Representative Classroom C-201
            GameObject roomC = CreateChild(bldgC_Int, "ROOM_C_201");
            roomC.transform.localPosition = new Vector3(-2.8f, 4.0f, 4.0f);

            GameObject boardC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardC.name = "Teaching_Board";
            boardC.transform.SetParent(roomC.transform, false);
            boardC.transform.localPosition = new Vector3(0, 1.9f, 4.0f);
            boardC.transform.localScale = new Vector3(4.0f, 1.3f, 0.06f);
            boardC.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            var trgC = roomC.AddComponent<BoxCollider>();
            trgC.isTrigger = true;
            trgC.size = new Vector3(5.5f, 3.6f, 8.0f);
            var rtC = roomC.AddComponent<RoomTrigger>();
            rtC.buildingName = "Tòa C";
            rtC.floorNumber = 2;
            rtC.roomId = "C-201";
            rtC.roomDisplayName = "Phòng học C-201";
        }

        private static void BuildBuildingIInterior(GameObject parent)
        {
            GameObject bldgI_Int = CreateChild(parent, "Building_I_Interior");
            bldgI_Int.transform.localPosition = new Vector3(18.0f, 0.0f, 48.0f);

            // Ground Floor Corridor & Slabs
            GameObject slab1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab1.name = "FloorSlab_I_F1";
            slab1.transform.SetParent(bldgI_Int.transform, false);
            slab1.transform.localPosition = new Vector3(0, -0.05f, 0);
            slab1.transform.localScale = new Vector3(44.0f, 0.10f, 15.0f);
            slab1.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Floor 2 Slab
            GameObject slab2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab2.name = "FloorSlab_I_F2";
            slab2.transform.SetParent(bldgI_Int.transform, false);
            slab2.transform.localPosition = new Vector3(0, 3.95f, 0);
            slab2.transform.localScale = new Vector3(44.0f, 0.15f, 15.0f);
            slab2.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Representative Classroom I-201
            GameObject roomI = CreateChild(bldgI_Int, "ROOM_I_201");
            roomI.transform.localPosition = new Vector3(-8.0f, 4.0f, 0);

            GameObject boardI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardI.name = "Teaching_Board";
            boardI.transform.SetParent(roomI.transform, false);
            boardI.transform.localPosition = new Vector3(0, 1.9f, 4.0f);
            boardI.transform.localScale = new Vector3(4.0f, 1.3f, 0.06f);
            boardI.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            var trgI = roomI.AddComponent<BoxCollider>();
            trgI.isTrigger = true;
            trgI.size = new Vector3(6.0f, 3.6f, 8.0f);
            var rtI = roomI.AddComponent<RoomTrigger>();
            rtI.buildingName = "Tòa I";
            rtI.floorNumber = 2;
            rtI.roomId = "I-201";
            rtI.roomDisplayName = "Phòng học I-201";
        }

        // ================= 3. GAMEPLAY & DOORS =================
        private static void BuildGameplayContent(GameObject parent, GameObject worldParent)
        {
            GameObject goDoors = CreateChild(parent, "Doors");

            // BLDG_G_Door_Main (World position: X: 16.0, Y: 0, Z: 8.0)
            GameObject doorMain = CreateChild(goDoors, "BLDG_G_Door_Main");
            doorMain.transform.localPosition = new Vector3(16.0f, 0f, 8.0f);
            doorMain.layer = LayerMask.NameToLayer("Interactable");

            // Hollow Frame
            GameObject frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameTop.name = "DoorFrame_Top";
            frameTop.transform.SetParent(doorMain.transform, false);
            frameTop.transform.localPosition = new Vector3(0, 2.7f, 0);
            frameTop.transform.localScale = new Vector3(0.20f, 0.20f, 4.0f);
            frameTop.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            GameObject frameLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameLeft.name = "DoorFrame_Left";
            frameLeft.transform.SetParent(doorMain.transform, false);
            frameLeft.transform.localPosition = new Vector3(0, 1.35f, -1.95f);
            frameLeft.transform.localScale = new Vector3(0.20f, 2.7f, 0.10f);
            frameLeft.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            GameObject frameRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameRight.name = "DoorFrame_Right";
            frameRight.transform.SetParent(doorMain.transform, false);
            frameRight.transform.localPosition = new Vector3(0, 1.35f, 1.95f);
            frameRight.transform.localScale = new Vector3(0.20f, 2.7f, 0.10f);
            frameRight.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Sliding Leaves
            GameObject leafLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leafLeft.name = "DoorLeaf_Left";
            leafLeft.transform.SetParent(doorMain.transform, false);
            leafLeft.transform.localPosition = new Vector3(0, 1.35f, -0.95f);
            leafLeft.transform.localScale = new Vector3(0.10f, 2.6f, 1.9f);
            leafLeft.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            GameObject leafRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leafRight.name = "DoorLeaf_Right";
            leafRight.transform.SetParent(doorMain.transform, false);
            leafRight.transform.localPosition = new Vector3(0, 1.35f, 0.95f);
            leafRight.transform.localScale = new Vector3(0.10f, 2.6f, 1.9f);
            leafRight.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            // Canopy Signboard
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = "BLDG_G_Entrance_Canopy";
            canopy.transform.SetParent(doorMain.transform, false);
            canopy.transform.localPosition = new Vector3(-2.2f, 3.6f, 0);
            canopy.transform.localScale = new Vector3(4.8f, 0.25f, 6.5f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            GameObject fascia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fascia.name = "Canopy_Signboard";
            fascia.transform.SetParent(canopy.transform, false);
            fascia.transform.localPosition = new Vector3(-0.52f, -0.15f, 0);
            fascia.transform.localScale = new Vector3(0.08f, 0.65f, 6.2f);
            fascia.GetComponent<MeshRenderer>().sharedMaterial = m_G_Sign;

            // RFID reader
            GameObject rfid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rfid.name = "Access_Control_Reader";
            rfid.transform.SetParent(doorMain.transform, false);
            rfid.transform.localPosition = new Vector3(-0.10f, 1.25f, 2.2f);
            rfid.transform.localScale = new Vector3(0.06f, 0.28f, 0.16f);
            rfid.GetComponent<MeshRenderer>().sharedMaterial = m_AccessControl;

            var doorWid = doorMain.AddComponent<WorldObjectID>();
            doorWid.objectId = "G_MAIN_DOOR_001";
            doorWid.displayName = "Cửa Kính Tự Động Nhà G";

            var doorCtrl = doorMain.AddComponent<DoorController>();
            doorCtrl.doorName = "Cửa Chính Nhà G";
            doorCtrl.doorLeafLeft = leafLeft.transform;
            doorCtrl.doorLeafRight = leafRight.transform;
            doorCtrl.autoOpen = false;
            doorCtrl.openOffset = new Vector3(0, 0, 1.8f);

            // Triggers Group
            GameObject goTriggers = CreateChild(parent, "Triggers");
            GameObject trgEntrance = CreateChild(goTriggers, "TRG_EnterBuildingG");
            trgEntrance.transform.localPosition = new Vector3(16.0f, 1.5f, 8.0f);
            BoxCollider boxEnt = trgEntrance.AddComponent<BoxCollider>();
            boxEnt.isTrigger = true;
            boxEnt.size = new Vector3(3.5f, 3.0f, 4.5f);
            var gTrgEnt = trgEntrance.AddComponent<GameTrigger>();
            gTrgEnt.triggerId = "TRG_ENTRANCE_G_001";
            gTrgEnt.displayName = "Khu Vực Sảnh Nhà G";

            // Objective Anchors
            GameObject goObj = CreateChild(parent, "ObjectiveAnchors");
            CreateAnchor(goObj, "OBJ_G_Reception", new Vector3(22.0f, 0, 10.5f));
            CreateAnchor(goObj, "OBJ_G_Lab_PC01", new Vector3(20.8f, 0, 0.0f));
            CreateAnchor(goObj, "OBJ_Courtyard_Tree", new Vector3(6.0f, 0, -4.0f));
            CreateAnchor(goObj, "OBJ_Parking_01", new Vector3(9.0f, 0, 10.0f));

            // Quest Anchors
            GameObject goQuest = CreateChild(parent, "QuestAnchors");
            CreateAnchor(goQuest, "QUEST_G_Entrance", new Vector3(14.0f, 0, 8.0f));
            CreateAnchor(goQuest, "QUEST_G_Reception", new Vector3(22.0f, 0, 10.5f));
            CreateAnchor(goQuest, "QUEST_G_InfoBoard", new Vector3(14.0f, 0, -1.0f));
            CreateAnchor(goQuest, "QUEST_G_ComputerLab", new Vector3(24.0f, 0, 0.0f));
            CreateAnchor(goQuest, "QUEST_G_Floor02_Classroom", new Vector3(19.6f, 4.0f, 16.5f));
            CreateAnchor(goQuest, "QUEST_G_Floor04_ITLab", new Vector3(19.6f, 12.0f, 16.5f));
        }

        private static GameObject CreateAnchor(GameObject parent, string name, Vector3 pos)
        {
            GameObject anchor = CreateChild(parent, name);
            anchor.transform.localPosition = pos;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "DebugMarker";
            marker.transform.SetParent(anchor.transform, false);
            marker.transform.localPosition = new Vector3(0, 1.2f, 0);
            marker.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            marker.GetComponent<MeshRenderer>().sharedMaterial = name.StartsWith("QUEST") ? m_SignWayfinding : m_SignNotice;
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "MarkerStem";
            stem.transform.SetParent(anchor.transform, false);
            stem.transform.localPosition = new Vector3(0, 0.6f, 0);
            stem.transform.localScale = new Vector3(0.04f, 0.6f, 0.04f);
            stem.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            Object.DestroyImmediate(stem.GetComponent<Collider>());

            return anchor;
        }

        // ================= 4. PLAYER & SPAWNS =================
        private static void BuildPlayerContent(GameObject parent, GameObject systemsParent)
        {
            GameObject goSpawns = CreateChild(parent, "PlayerSpawnPoints");
            var sm = systemsParent.transform.Find("GameManager")?.GetComponent<PlayerSpawnManager>();
            if (sm == null)
            {
                sm = systemsParent.transform.Find("GameManager")?.gameObject.AddComponent<PlayerSpawnManager>();
            }

            sm.spawnPoints.Clear();

            AddSpawn(goSpawns, sm, "SPAWN_MainCampus", new Vector3(10.0f, 0.1f, -8.0f), Quaternion.Euler(0, 35f, 0));
            AddSpawn(goSpawns, sm, "SPAWN_BuildingG_Exterior", new Vector3(11.5f, 0.1f, 8.0f), Quaternion.Euler(0, 90f, 0));
            AddSpawn(goSpawns, sm, "SPAWN_BuildingG_Lobby", new Vector3(18.0f, 0.1f, 6.0f), Quaternion.Euler(0, 45f, 0));
            AddSpawn(goSpawns, sm, "SPAWN_ComputerLab", new Vector3(24.0f, 0.1f, 2.0f), Quaternion.Euler(0, 180f, 0));

            // Multi-Floor Spawns
            AddSpawn(goSpawns, sm, "SPAWN_BuildingG_Floor02", new Vector3(24.0f, 4.1f, 10.0f), Quaternion.Euler(0, 180f, 0));
            AddSpawn(goSpawns, sm, "SPAWN_BuildingG_Floor03", new Vector3(24.0f, 8.1f, 10.0f), Quaternion.Euler(0, 180f, 0));
            AddSpawn(goSpawns, sm, "SPAWN_BuildingG_Floor04", new Vector3(24.0f, 12.1f, 10.0f), Quaternion.Euler(0, 180f, 0));

            // Player GameObject
            GameObject player = CreateChild(parent, "Player");
            player.tag = "Player";
            player.transform.localPosition = new Vector3(10.0f, 0.1f, -8.0f);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.75f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.875f, 0);

            player.AddComponent<PlayerState>();
            player.AddComponent<PlayerInput>();
            PlayerMovement pm = player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerLocationTracker>();
            sm.playerMovement = pm;

            // Visual Humanoid Model
            GameObject visual = CreateChild(player, "VisualModel");
            visual.transform.localPosition = Vector3.zero;

            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(visual.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.15f, 0);
            torso.transform.localScale = new Vector3(0.45f, 0.60f, 0.25f);
            torso.GetComponent<MeshRenderer>().sharedMaterial = m_StudentBluePolo;
            Object.DestroyImmediate(torso.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visual.transform, false);
            head.transform.localPosition = new Vector3(0, 1.62f, 0);
            head.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            head.GetComponent<MeshRenderer>().sharedMaterial = m_StudentSkin;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(head.transform, false);
            hair.transform.localPosition = new Vector3(0, 0.06f, -0.02f);
            hair.transform.localScale = new Vector3(1.05f, 0.55f, 1.05f);
            hair.GetComponent<MeshRenderer>().sharedMaterial = m_StudentHair;
            Object.DestroyImmediate(hair.GetComponent<Collider>());

            // Camera Target & PlayerCamera
            GameObject camTarget = CreateChild(player, "CameraTarget");
            camTarget.transform.localPosition = new Vector3(0, 1.55f, 0);

            GameObject camGO = CreateChild(parent, "PlayerCamera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 450f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.68f, 0.82f, 0.96f);
            camGO.AddComponent<AudioListener>();

            var tpc = camGO.AddComponent<IUHThirdPersonCamera>();
            tpc.SetTarget(camTarget.transform);

            pm.cameraTransform = camGO.transform;

            PlayerInteraction pi = player.AddComponent<PlayerInteraction>();
            pi.raycastOrigin = camGO.transform;
        }

        private static void AddSpawn(GameObject parent, PlayerSpawnManager sm, string id, Vector3 pos, Quaternion rot)
        {
            GameObject sp = CreateChild(parent, id);
            sp.transform.localPosition = pos;
            sp.transform.localRotation = rot;

            PlayerSpawnManager.SpawnPointEntry entry = new PlayerSpawnManager.SpawnPointEntry
            {
                spawnId = id,
                spawnTransform = sp.transform
            };
            sm.spawnPoints.Add(entry);
        }

        // ================= 5. NPC ROUTES & RUNTIME =================
        private static void BuildNPCContent(GameObject parent)
        {
            GameObject goRoutes = CreateChild(parent, "NPC_Routes");

            // Route 1: Courtyard to Lobby
            GameObject r1 = CreateChild(goRoutes, "NPC_Route_G_Courtyard");
            NPCRoute route1 = r1.AddComponent<NPCRoute>();
            route1.routeId = "ROUTE_Courtyard_Lobby";
            route1.mode = RouteMode.PingPong;
            route1.waypoints.Clear();

            Vector3[] wpPositions = {
                new Vector3(8.0f, 0f, -4.0f),
                new Vector3(10.0f, 0f, 2.0f),
                new Vector3(14.0f, 0f, 8.0f),
                new Vector3(18.0f, 0f, 7.0f)
            };

            for (int i = 0; i < wpPositions.Length; i++)
            {
                GameObject wp = CreateChild(r1, "WP_0" + (i + 1));
                wp.transform.localPosition = wpPositions[i];
                Waypoint wComp = wp.AddComponent<Waypoint>();
                wComp.waitTime = 2.5f;
                route1.waypoints.Add(wComp);
            }

            // NPC Runtime
            GameObject goRuntime = CreateChild(parent, "NPC_Runtime");
            GameObject npc1 = CreateChild(goRuntime, "NPC_Student_01");
            npc1.transform.localPosition = new Vector3(10.0f, 0f, 2.0f);
            npc1.transform.localRotation = Quaternion.Euler(0, 45f, 0);

            var id = npc1.AddComponent<NPCIdentity>();
            id.npcId = "NPC_STUDENT_001";
            id.displayName = "Nguyễn Văn An";
            id.role = "Sinh viên CNTT";

            var move = npc1.AddComponent<NPCMovement>();
            move.assignedRoute = route1;
            move.moveSpeed = 1.8f;

            // Visual NPC 1
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(npc1.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.15f, 0);
            torso.transform.localScale = new Vector3(0.42f, 0.60f, 0.24f);
            torso.GetComponent<MeshRenderer>().sharedMaterial = m_StudentWhiteShirt;
            Object.DestroyImmediate(torso.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npc1.transform, false);
            head.transform.localPosition = new Vector3(0, 1.62f, 0);
            head.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            head.GetComponent<MeshRenderer>().sharedMaterial = m_StudentSkin;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            GameObject pants = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pants.name = "Pants";
            pants.transform.SetParent(npc1.transform, false);
            pants.transform.localPosition = new Vector3(0, 0.45f, 0);
            pants.transform.localScale = new Vector3(0.38f, 0.75f, 0.22f);
            pants.GetComponent<MeshRenderer>().sharedMaterial = m_StudentDarkPants;
            Object.DestroyImmediate(pants.GetComponent<Collider>());

            // NPC 2: Inside Classroom G-201
            GameObject npc2 = CreateChild(goRuntime, "NPC_Student_02");
            npc2.transform.localPosition = new Vector3(19.6f, 4.0f, 16.5f); // Inside G-201
            npc2.transform.localRotation = Quaternion.Euler(0, 0f, 0);

            var id2 = npc2.AddComponent<NPCIdentity>();
            id2.npcId = "NPC_STUDENT_002";
            id2.displayName = "Lê Thị Mai";
            id2.role = "Lớp trưởng K19 CNTT";

            GameObject torso2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso2.name = "Torso";
            torso2.transform.SetParent(npc2.transform, false);
            torso2.transform.localPosition = new Vector3(0, 1.15f, 0);
            torso2.transform.localScale = new Vector3(0.42f, 0.60f, 0.24f);
            torso2.GetComponent<MeshRenderer>().sharedMaterial = m_StudentBluePolo;
            Object.DestroyImmediate(torso2.GetComponent<Collider>());

            GameObject head2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head2.name = "Head";
            head2.transform.SetParent(npc2.transform, false);
            head2.transform.localPosition = new Vector3(0, 1.62f, 0);
            head2.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            head2.GetComponent<MeshRenderer>().sharedMaterial = m_StudentSkin;
            Object.DestroyImmediate(head2.GetComponent<Collider>());
        }

        // ================= 6. LIGHTING & AUDIO =================
        private static void BuildLightingAndAudio(GameObject goLighting, GameObject goAudio)
        {
            GameObject sunGO = CreateChild(goLighting, "Directional_Sun");
            Light sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.96f, 0.88f);
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sunGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // Ground Floor Lights
            GameObject lightLobby = CreateChild(goLighting, "PointLight_Lobby");
            lightLobby.transform.localPosition = new Vector3(20.0f, 3.4f, 6.0f);
            Light p1 = lightLobby.AddComponent<Light>();
            p1.type = LightType.Point;
            p1.range = 14f;
            p1.intensity = 2.2f;
            p1.color = new Color(1f, 0.95f, 0.85f);

            // Floor 2 Classroom Lights
            GameObject lightF2 = CreateChild(goLighting, "PointLight_Floor02_G201");
            lightF2.transform.localPosition = new Vector3(19.6f, 7.2f, 16.5f);
            Light pF2 = lightF2.AddComponent<Light>();
            pF2.type = LightType.Point;
            pF2.range = 12f;
            pF2.intensity = 2.0f;
            pF2.color = new Color(0.98f, 0.98f, 0.95f);

            // Floor 4 Lab Lights
            GameObject lightF4 = CreateChild(goLighting, "PointLight_Floor04_G401");
            lightF4.transform.localPosition = new Vector3(19.6f, 15.2f, 16.5f);
            Light pF4 = lightF4.AddComponent<Light>();
            pF4.type = LightType.Point;
            pF4.range = 14f;
            pF4.intensity = 2.4f;
            pF4.color = new Color(0.92f, 0.96f, 1.0f);

            // Audio Zones
            GameObject azCourt = CreateChild(goAudio, "AUDIO_CourtyardZone");
            azCourt.transform.localPosition = new Vector3(12f, 1f, 0f);
            BoxCollider acBox = azCourt.AddComponent<BoxCollider>();
            acBox.isTrigger = true;
            acBox.size = new Vector3(30f, 6f, 40f);
            var az1 = azCourt.AddComponent<AudioZone>();
            az1.zoneId = "AUDIO_Courtyard";

            GameObject azLobby = CreateChild(goAudio, "AUDIO_LobbyZone");
            azLobby.transform.localPosition = new Vector3(24f, 1.5f, 4f);
            BoxCollider alBox = azLobby.AddComponent<BoxCollider>();
            alBox.isTrigger = true;
            alBox.size = new Vector3(18f, 5f, 22f);
            var az2 = azLobby.AddComponent<AudioZone>();
            az2.zoneId = "AUDIO_Lobby";
        }

        // ================= 7. DEBUG =================
        private static void BuildDebugContent(GameObject parent)
        {
            GameObject gizmos = CreateChild(parent, "Debug_GizmoVisualizer");
        }

        // --- GROUND & URBAN EXTERIORS (PRESERVED 100%) ---
        private static void BuildGround(GameObject parent)
        {
            GameObject plaza = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plaza.name = "Ground_ConcretePlaza";
            plaza.transform.SetParent(parent.transform, false);
            plaza.transform.localPosition = new Vector3(18.0f, -0.1f, 16.0f);
            plaza.transform.localScale = new Vector3(72.0f, 0.2f, 82.0f);
            plaza.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;
            plaza.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Concrete;

            GameObject roadFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadFront.name = "Road_FrontLane";
            roadFront.transform.SetParent(parent.transform, false);
            roadFront.transform.localPosition = new Vector3(18.0f, 0.02f, -14.0f);
            roadFront.transform.localScale = new Vector3(68.0f, 0.04f, 6.8f);
            roadFront.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;
            roadFront.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Asphalt;

            GameObject roadCourt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadCourt.name = "Road_CourtyardAsphalt";
            roadCourt.transform.SetParent(parent.transform, false);
            roadCourt.transform.localPosition = new Vector3(18.0f, 0.02f, 14.0f);
            roadCourt.transform.localScale = new Vector3(22.0f, 0.04f, 32.0f);
            roadCourt.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;
            roadCourt.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Asphalt;

            // Zebra
            GameObject zebra = CreateChild(parent, "PedestrianZebraCrossing");
            for (int zc = 0; zc < 6; zc++)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = "ZebraBar_" + zc;
                bar.transform.SetParent(zebra.transform, false);
                bar.transform.localPosition = new Vector3(11.0f + zc * 1.0f, 0.045f, -14.0f);
                bar.transform.localScale = new Vector3(0.55f, 0.01f, 4.8f);
                bar.GetComponent<MeshRenderer>().sharedMaterial = m_ParkingLine;
            }

            // Curbs
            for (int c = -1; c <= 1; c += 2)
            {
                GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curb.name = "Curb_Roadside_" + c;
                curb.transform.SetParent(parent.transform, false);
                curb.transform.localPosition = new Vector3(18.0f, 0.10f, -14.0f + c * 3.5f);
                curb.transform.localScale = new Vector3(68.0f, 0.18f, 0.35f);
                curb.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;
            }
        }

        private static void BuildVegetation(GameObject parent)
        {
            Vector3[] treePositions = {
                new Vector3(6.0f, 0, -4.0f),
                new Vector3(14.0f, 0, 26.0f),
                new Vector3(32.0f, 0, -6.0f),
                new Vector3(2.0f, 0, 8.0f)
            };

            for (int i = 0; i < treePositions.Length; i++)
            {
                GameObject tree = CreateChild(parent, "TropicalShadeTree_" + i);
                tree.transform.localPosition = treePositions[i];

                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk";
                trunk.transform.SetParent(tree.transform, false);
                trunk.transform.localPosition = new Vector3(0, 4.5f, 0);
                trunk.transform.localScale = new Vector3(0.9f, 4.5f, 0.9f);
                trunk.GetComponent<MeshRenderer>().sharedMaterial = m_TreeBark;

                for (int c = 0; c < 3; c++)
                {
                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crown.name = "CrownLayer_" + c;
                    crown.transform.SetParent(tree.transform, false);
                    crown.transform.localPosition = new Vector3(0, 6.5f + c * 1.8f, 0);
                    crown.transform.localScale = new Vector3(6.2f - c * 1.2f, 3.2f, 6.2f - c * 1.2f);
                    crown.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
                }
            }
        }

        private static void BuildParking(GameObject parent)
        {
            float[] shelterXs = { 8.0f, 11.2f };
            for (int s = 0; s < 2; s++)
            {
                GameObject shelter = CreateChild(parent, "Motorbike_Shelter_" + s);
                shelter.transform.localPosition = new Vector3(shelterXs[s], 0, 12.0f);

                for (int p = -2; p <= 2; p++)
                {
                    GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    post.name = "Post_" + p;
                    post.transform.SetParent(shelter.transform, false);
                    post.transform.localPosition = new Vector3(0, 1.5f, p * 3.8f);
                    post.transform.localScale = new Vector3(0.12f, 1.5f, 0.12f);
                    post.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
                }

                GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roof.name = "Corrugated_Roof";
                roof.transform.SetParent(shelter.transform, false);
                roof.transform.localPosition = new Vector3(0, 3.05f, 0);
                roof.transform.localRotation = Quaternion.Euler(4.0f, 0, 0);
                roof.transform.localScale = new Vector3(2.8f, 0.10f, 18.0f);
                roof.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "Stall_Line";
                line.transform.SetParent(shelter.transform, false);
                line.transform.localPosition = new Vector3(1.35f, 0.045f, 0);
                line.transform.localScale = new Vector3(0.12f, 0.01f, 18.0f);
                line.GetComponent<MeshRenderer>().sharedMaterial = m_ParkingLine;
            }

            // Motorbikes
            GameObject bikesGroup = CreateChild(parent, "Motorbike_Rows");
            System.Random rnd = new System.Random(42);
            for (int r = 0; r < 2; r++)
            {
                float xBase = shelterXs[r];
                for (int b = 0; b < 24; b++)
                {
                    float z = 3.5f + b * 0.72f;
                    float jitterRot = (float)(rnd.NextDouble() * 14.0 - 7.0);
                    float lean = (float)(rnd.NextDouble() * 5.0 - 2.5);
                    int colIdx = rnd.Next(m_BikeMats.Length);

                    GameObject bike = CreateChild(bikesGroup, $"Bike_R{r}_B{b}");
                    bike.transform.localPosition = new Vector3(xBase, 0, z);
                    bike.transform.localRotation = Quaternion.Euler(0, 90.0f + jitterRot, lean);

                    GameObject bBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bBody.name = "Body";
                    bBody.transform.SetParent(bike.transform, false);
                    bBody.transform.localPosition = new Vector3(0, 0.52f, 0);
                    bBody.transform.localScale = new Vector3(0.38f, 0.42f, 1.25f);
                    bBody.GetComponent<MeshRenderer>().sharedMaterial = m_BikeMats[colIdx];

                    GameObject bSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bSeat.name = "Seat";
                    bSeat.transform.SetParent(bike.transform, false);
                    bSeat.transform.localPosition = new Vector3(0, 0.76f, -0.15f);
                    bSeat.transform.localScale = new Vector3(0.32f, 0.10f, 0.70f);
                    bSeat.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    for (int w = 0; w < 2; w++)
                    {
                        float wz = (w == 0) ? 0.60f : -0.60f;
                        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        wheel.name = (w == 0) ? "Wheel_F" : "Wheel_R";
                        wheel.transform.SetParent(bike.transform, false);
                        wheel.transform.localPosition = new Vector3(0, 0.28f, wz);
                        wheel.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                        wheel.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);
                        wheel.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
                    }

                    if (rnd.NextDouble() < 0.42)
                    {
                        int hCol = rnd.Next(m_HelmetMats.Length);
                        GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        helmet.name = "Helmet";
                        helmet.transform.SetParent(bike.transform, false);
                        helmet.transform.localPosition = new Vector3(0.04f, 0.90f, 0.05f);
                        helmet.transform.localScale = new Vector3(0.24f, 0.22f, 0.26f);
                        helmet.GetComponent<MeshRenderer>().sharedMaterial = m_HelmetMats[hCol];
                    }
                }
            }
        }

        private static void BuildInfrastructure(GameObject parent)
        {
            for (int f = 0; f < 6; f++)
            {
                float y = 5.5f + f * 5.2f;
                GameObject ac = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ac.name = "AC_Condenser_F" + f;
                ac.transform.SetParent(parent.transform, false);
                ac.transform.localPosition = new Vector3(15.4f, y, 10.0f);
                ac.transform.localScale = new Vector3(0.6f, 0.9f, 1.4f);
                ac.GetComponent<MeshRenderer>().sharedMaterial = m_ACLouvers;
            }

            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "Rainwater_Downspout_G";
            pipe.transform.SetParent(parent.transform, false);
            pipe.transform.localPosition = new Vector3(15.8f, 21.0f, -8.0f);
            pipe.transform.localScale = new Vector3(0.18f, 21.0f, 0.18f);
            pipe.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
        }

        private static void BuildProps(GameObject parent)
        {
            for (int b = 0; b < 3; b++)
            {
                GameObject bench = CreateChild(parent, "PROP_Bench_" + b);
                bench.transform.localPosition = new Vector3(14.5f, 0, -6.0f + b * 5.0f);

                GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seat.name = "Seat";
                seat.transform.SetParent(bench.transform, false);
                seat.transform.localPosition = new Vector3(0, 0.45f, 0);
                seat.transform.localScale = new Vector3(0.65f, 0.12f, 2.2f);
                seat.GetComponent<MeshRenderer>().sharedMaterial = m_BenchGranite;

                for (int l = -1; l <= 1; l += 2)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = "Leg";
                    leg.transform.SetParent(bench.transform, false);
                    leg.transform.localPosition = new Vector3(0, 0.20f, l * 0.8f);
                    leg.transform.localScale = new Vector3(0.55f, 0.40f, 0.25f);
                    leg.GetComponent<MeshRenderer>().sharedMaterial = m_BenchGranite;
                }
            }

            // Campus Notice Board
            GameObject infoBoard = CreateChild(parent, "INT_InfoBoard_G_01");
            infoBoard.transform.localPosition = new Vector3(15.2f, 0, -1.0f);
            infoBoard.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            infoBoard.layer = LayerMask.NameToLayer("Interactable");

            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Board_Face";
            panel.transform.SetParent(infoBoard.transform, false);
            panel.transform.localPosition = new Vector3(0, 1.8f, 0);
            panel.transform.localScale = new Vector3(2.8f, 1.6f, 0.12f);
            panel.GetComponent<MeshRenderer>().sharedMaterial = m_SignNotice;

            for (int p = -1; p <= 1; p += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Support_Post";
                post.transform.SetParent(infoBoard.transform, false);
                post.transform.localPosition = new Vector3(p * 1.2f, 1.0f, 0);
                post.transform.localScale = new Vector3(0.08f, 1.0f, 0.08f);
                post.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }

            var widBoard = infoBoard.AddComponent<WorldObjectID>();
            widBoard.objectId = "G_INFOBOARD_001";
            widBoard.displayName = "Bảng Tin Khuôn Viên Nhà G";
            infoBoard.AddComponent<InfoBoardController>();
        }

        private static void BuildBackgroundCity(GameObject parent)
        {
            Vector3[] housePositions = {
                new Vector3(-24.0f, 0, 18.0f),
                new Vector3(-24.0f, 0, -4.0f),
                new Vector3(44.0f, 0, 24.0f),
                new Vector3(44.0f, 0, 2.0f)
            };

            for (int i = 0; i < housePositions.Length; i++)
            {
                GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
                house.name = "City_Shophouse_" + i;
                house.transform.SetParent(parent.transform, false);
                float h = 16.0f + (i % 2) * 6.0f;
                house.transform.localPosition = housePositions[i] + Vector3.up * (h * 0.5f);
                house.transform.localScale = new Vector3(10.0f, h, 14.0f);
                house.GetComponent<MeshRenderer>().sharedMaterial = m_UrbanHouse;

                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "Rooftop_WaterTank";
                tank.transform.SetParent(house.transform, false);
                tank.transform.localPosition = new Vector3(0, 0.52f, 0);
                tank.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = m_WaterTankStainless;
            }
        }
    }
}
