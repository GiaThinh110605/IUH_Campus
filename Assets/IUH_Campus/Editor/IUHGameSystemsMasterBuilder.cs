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
    public static class IUHGameSystemsMasterBuilder
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

        [MenuItem("IUH Campus/Build Full Game-Ready World & Systems (IUH_World)")]
        public static void BuildWorldAndSystems()
        {
            Debug.Log("[IUH Game Systems Builder] Khởi tạo toàn bộ thế giới IUH_World...");

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

            // --- BUILD WORLD (LOCKED ARCHITECTURE & ENVIRONMENT) ---
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
            Debug.Log("[IUH Game Systems Builder] Thế giới IUH_World đã được tạo thành công!");
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

            // Interaction Prompt Root
            GameObject promptRoot = new GameObject("InteractionPrompt");
            promptRoot.transform.SetParent(canvasGO.transform, false);
            RectTransform pRT = promptRoot.AddComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.5f, 0.2f);
            pRT.anchorMax = new Vector2(0.5f, 0.2f);
            pRT.sizeDelta = new Vector2(400f, 60f);

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

            // Simple Modal Panel
            GameObject panelRoot = new GameObject("SimpleInteractionPanel");
            panelRoot.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.2f, 0.2f);
            panelRT.anchorMax = new Vector2(0.8f, 0.8f);
            panelRT.sizeDelta = Vector2.zero;

            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.18f, 0.92f);

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
            // Architecture
            GameObject goArch = CreateChild(parent, "Architecture");
            BuildArchitecture(goArch);

            // Interiors
            GameObject goInt = CreateChild(parent, "Interiors");
            BuildInteriors(goInt);

            // Ground
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

        private static void BuildArchitecture(GameObject parent)
        {
            // BLDG_G_Main (X: 24, Y: 0, Z: 10)
            GameObject bldgG = CreateChild(parent, "BLDG_G_Main");
            bldgG.transform.localPosition = new Vector3(24.0f, 0.0f, 10.0f);

            float wG = 16.0f, hG = 42.0f, dG = 44.0f;
            GameObject upperG = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperG.name = "BLDG_G_Upper_Structure";
            upperG.transform.SetParent(bldgG.transform, false);
            upperG.transform.localPosition = new Vector3(0, 23.0f, 0);
            upperG.transform.localScale = new Vector3(wG, 38.0f, dG);
            upperG.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject gPillarRear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gPillarRear.name = "BLDG_G_Pillar_East";
            gPillarRear.transform.SetParent(bldgG.transform, false);
            gPillarRear.transform.localPosition = new Vector3(wG * 0.5f - 0.5f, 2.0f, 0);
            gPillarRear.transform.localScale = new Vector3(1.0f, 4.0f, dG);
            gPillarRear.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject gRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gRoof.name = "BLDG_G_Roof_Parapet";
            gRoof.transform.SetParent(bldgG.transform, false);
            gRoof.transform.localPosition = new Vector3(0, hG + 0.6f, 0);
            gRoof.transform.localScale = new Vector3(wG + 0.6f, 1.2f, dG + 0.6f);
            gRoof.GetComponent<MeshRenderer>().sharedMaterial = m_G_Roof;

            for (int t = 0; t < 3; t++)
            {
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "BLDG_G_Rooftop_WaterTank_" + t;
                tank.transform.SetParent(bldgG.transform, false);
                tank.transform.localPosition = new Vector3(-2.0f + t * 2.5f, hG + 2.4f, 12.0f);
                tank.transform.localScale = new Vector3(1.8f, 1.6f, 1.8f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = (t == 1) ? m_WaterTankBlack : m_WaterTankStainless;
            }

            // BLDG_I_Main (X: 18, Y: 0, Z: 48)
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

            // BLDG_C_Main (X: 0, Y: 0, Z: 18)
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

            // BLDG_LowPodium (X: 18, Y: 0, Z: -6)
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

            GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWall.name = "Podium_SouthWall";
            southWall.transform.SetParent(podium.transform, false);
            southWall.transform.localPosition = new Vector3(0, 1.9f, -dP * 0.5f + 0.2f);
            southWall.transform.localScale = new Vector3(wP, 3.8f, 0.4f);
            southWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWall.name = "Podium_WestWall";
            westWall.transform.SetParent(podium.transform, false);
            westWall.transform.localPosition = new Vector3(-wP * 0.5f + 0.2f, 1.9f, 0);
            westWall.transform.localScale = new Vector3(0.4f, 3.8f, dP);
            westWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

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

        private static void BuildInteriors(GameObject parent)
        {
            GameObject intRoot = CreateChild(parent, "INT_GroundFloor_G");
            intRoot.transform.localPosition = new Vector3(24.0f, 0.0f, 6.0f);

            // Lobby (Zone A)
            GameObject zoneA = CreateChild(intRoot, "INT_Lobby_G");
            GameObject floorA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorA.name = "Floor_Lobby";
            floorA.transform.SetParent(zoneA.transform, false);
            floorA.transform.localPosition = new Vector3(-3.5f, -0.05f, 3.0f);
            floorA.transform.localScale = new Vector3(8.0f, 0.10f, 12.0f);
            floorA.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            floorA.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            for (int i = 0; i < 18; i++)
            {
                float z = -2.5f + i * 0.6f;
                GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slat.name = "CeilingSlat_" + i;
                slat.transform.SetParent(zoneA.transform, false);
                slat.transform.localPosition = new Vector3(-3.5f, 3.65f, z);
                slat.transform.localScale = new Vector3(7.8f, 0.12f, 0.08f);
                slat.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
            }

            // Waiting Lounge (Zone B)
            GameObject zoneB = CreateChild(intRoot, "INT_Lounge_G");
            GameObject floorB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorB.name = "Floor_Lounge";
            floorB.transform.SetParent(zoneB.transform, false);
            floorB.transform.localPosition = new Vector3(3.5f, -0.05f, 3.0f);
            floorB.transform.localScale = new Vector3(7.0f, 0.10f, 12.0f);
            floorB.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;
            floorB.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            GameObject media = GameObject.CreatePrimitive(PrimitiveType.Cube);
            media.name = "Media_LED_Display";
            media.transform.SetParent(zoneB.transform, false);
            media.transform.localPosition = new Vector3(6.8f, 2.0f, 3.0f);
            media.transform.localScale = new Vector3(0.10f, 2.4f, 4.5f);
            media.GetComponent<MeshRenderer>().sharedMaterial = m_LEDDisplay;

            for (int s = 0; s < 2; s++)
            {
                GameObject sofa = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sofa.name = "Sofa_" + s;
                sofa.transform.SetParent(zoneB.transform, false);
                sofa.transform.localPosition = new Vector3(2.5f, 0.40f, 1.0f + s * 4.0f);
                sofa.transform.localScale = new Vector3(1.2f, 0.75f, 2.4f);
                sofa.GetComponent<MeshRenderer>().sharedMaterial = m_CyanSofa;
            }

            // Training Lab (Zone C)
            GameObject zoneC = CreateChild(intRoot, "INT_ComputerLab_G");
            GameObject floorC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorC.name = "Floor_Lab_BlackMarble";
            floorC.transform.SetParent(zoneC.transform, false);
            floorC.transform.localPosition = new Vector3(0, -0.05f, -8.5f);
            floorC.transform.localScale = new Vector3(14.0f, 0.10f, 11.0f);
            floorC.GetComponent<MeshRenderer>().sharedMaterial = m_BlackMarble;
            floorC.AddComponent<SurfaceIdentifier>().surfaceType = SurfaceType.Tile;

            GameObject ceilC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceilC.name = "Ceiling_AcousticGrid";
            ceilC.transform.SetParent(zoneC.transform, false);
            ceilC.transform.localPosition = new Vector3(0, 3.65f, -8.5f);
            ceilC.transform.localScale = new Vector3(14.0f, 0.10f, 11.0f);
            ceilC.GetComponent<MeshRenderer>().sharedMaterial = m_CeilingGrid;

            // Enclosure walls
            GameObject labBackWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            labBackWall.name = "Lab_BackWall";
            labBackWall.transform.SetParent(zoneC.transform, false);
            labBackWall.transform.localPosition = new Vector3(0, 1.8f, -14.0f);
            labBackWall.transform.localScale = new Vector3(14.0f, 3.6f, 0.25f);
            labBackWall.GetComponent<MeshRenderer>().sharedMaterial = m_SageGreen;

            GameObject labWestWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            labWestWall.name = "Lab_WestWall";
            labWestWall.transform.SetParent(zoneC.transform, false);
            labWestWall.transform.localPosition = new Vector3(-7.0f, 1.8f, -8.5f);
            labWestWall.transform.localScale = new Vector3(0.25f, 3.6f, 11.0f);
            labWestWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject labEastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            labEastWall.name = "Lab_EastWall";
            labEastWall.transform.SetParent(zoneC.transform, false);
            labEastWall.transform.localPosition = new Vector3(7.0f, 1.8f, -8.5f);
            labEastWall.transform.localScale = new Vector3(0.25f, 3.6f, 11.0f);
            labEastWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject labNorthL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            labNorthL.name = "Lab_NorthPartition_L";
            labNorthL.transform.SetParent(zoneC.transform, false);
            labNorthL.transform.localPosition = new Vector3(-4.5f, 1.8f, -3.0f);
            labNorthL.transform.localScale = new Vector3(5.0f, 3.6f, 0.25f);
            labNorthL.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject labNorthR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            labNorthR.name = "Lab_NorthPartition_R";
            labNorthR.transform.SetParent(zoneC.transform, false);
            labNorthR.transform.localPosition = new Vector3(4.5f, 1.8f, -3.0f);
            labNorthR.transform.localScale = new Vector3(5.0f, 3.6f, 0.25f);
            labNorthR.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Whiteboard
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Teaching_Whiteboard";
            board.transform.SetParent(zoneC.transform, false);
            board.transform.localPosition = new Vector3(0, 1.9f, -13.85f);
            board.transform.localScale = new Vector3(6.5f, 2.0f, 0.08f);
            board.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;
            var wIdBoard = board.AddComponent<WorldObjectID>();
            wIdBoard.objectId = "G_WHITEBOARD_001";
            wIdBoard.displayName = "Bảng Giảng Dạy CNTT";

            // Teacher Desk
            GameObject teacherDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            teacherDesk.name = "Teacher_Desk";
            teacherDesk.transform.SetParent(zoneC.transform, false);
            teacherDesk.transform.localPosition = new Vector3(0, 0.45f, -11.8f);
            teacherDesk.transform.localScale = new Vector3(2.6f, 0.90f, 0.90f);
            teacherDesk.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            GameObject tMon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tMon.name = "Teacher_Monitor";
            tMon.transform.SetParent(teacherDesk.transform, false);
            tMon.transform.localPosition = new Vector3(0, 0.65f, 0.15f);
            tMon.transform.localScale = new Vector3(0.85f, 0.50f, 0.08f);
            tMon.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

            // Student PC Workstations
            for (int row = 0; row < 2; row++)
            {
                float z = -6.0f - row * 3.5f;
                for (int col = -1; col <= 1; col += 2)
                {
                    float x = col * 3.2f;
                    GameObject deskPC = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    deskPC.name = $"PC_Desk_R{row}_C{col}";
                    deskPC.transform.SetParent(zoneC.transform, false);
                    deskPC.transform.localPosition = new Vector3(x, 0.38f, z);
                    deskPC.transform.localScale = new Vector3(2.4f, 0.76f, 0.90f);
                    deskPC.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

                    GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    monitor.name = "Monitor";
                    monitor.transform.SetParent(deskPC.transform, false);
                    monitor.transform.localPosition = new Vector3(0, 0.70f, -0.15f);
                    monitor.transform.localScale = new Vector3(0.75f, 0.48f, 0.08f);
                    monitor.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

                    GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    chair.name = "Navy_Task_Chair";
                    chair.transform.SetParent(deskPC.transform, false);
                    chair.transform.localPosition = new Vector3(0, 0.22f, 0.75f);
                    chair.transform.localScale = new Vector3(0.55f, 0.44f, 0.55f);
                    chair.GetComponent<MeshRenderer>().sharedMaterial = m_NavyChair;

                    // Workstation 0 is the primary interactable computer
                    if (row == 0 && col == -1)
                    {
                        deskPC.name = "INT_Computer_01";
                        deskPC.layer = LayerMask.NameToLayer("Interactable");
                        var compWid = deskPC.AddComponent<WorldObjectID>();
                        compWid.objectId = "G_COMPUTER_001";
                        compWid.displayName = "Máy trạm IUH CNTT #01";
                        deskPC.AddComponent<ComputerController>();
                    }
                }
            }

            // Corridor Circulation Spine
            GameObject corridor = CreateChild(intRoot, "INT_Corridor_Spine");
            GameObject corrSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corrSign.name = "Corridor_Wayfinding_Sign";
            corrSign.transform.SetParent(corridor.transform, false);
            corrSign.transform.localPosition = new Vector3(0, 2.85f, -2.85f);
            corrSign.transform.localScale = new Vector3(2.6f, 0.45f, 0.08f);
            corrSign.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            GameObject exitSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitSign.name = "Emergency_Exit_Sign";
            exitSign.transform.SetParent(corridor.transform, false);
            exitSign.transform.localPosition = new Vector3(0, 3.25f, -2.90f);
            exitSign.transform.localScale = new Vector3(0.85f, 0.28f, 0.12f);
            exitSign.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            GameObject fireCab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fireCab.name = "Corridor_Fire_Cabinet";
            fireCab.transform.SetParent(corridor.transform, false);
            fireCab.transform.localPosition = new Vector3(-6.85f, 1.25f, -2.5f);
            fireCab.transform.localScale = new Vector3(0.20f, 1.3f, 0.90f);
            fireCab.GetComponent<MeshRenderer>().sharedMaterial = m_FireCabinet;
        }

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
                    crown.name = "Canopy_Tier_" + c;
                    crown.transform.SetParent(tree.transform, false);
                    float y = 9.0f + c * 1.8f;
                    float r = 5.2f - c * 0.8f;
                    float ox = (c == 1) ? 0.8f : -0.6f;
                    float oz = (c == 2) ? 0.6f : -0.6f;
                    crown.transform.localPosition = new Vector3(ox, y, oz);
                    crown.transform.localScale = new Vector3(r * 2.0f, r * 1.15f, r * 2.0f);
                    crown.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
                }

                BoxCollider col = tree.AddComponent<BoxCollider>();
                col.center = new Vector3(0, 2.5f, 0);
                col.size = new Vector3(1.4f, 5.0f, 1.4f);

                var nmo = tree.AddComponent<UnityEngine.AI.NavMeshObstacle>();
                nmo.carving = true;
                nmo.size = new Vector3(3.2f, 3.0f, 3.2f);
            }
        }

        private static void BuildParking(GameObject parent)
        {
            GameObject shelters = CreateChild(parent, "Parking_Shelters");
            float[] shelterXs = { 7.5f, 10.5f };
            for (int s = 0; s < shelterXs.Length; s++)
            {
                GameObject shelter = CreateChild(shelters, "Canopy_Shelter_" + s);
                shelter.transform.localPosition = new Vector3(shelterXs[s], 0, 12.0f);

                for (int p = -2; p <= 2; p++)
                {
                    GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    post.name = "Steel_Post_" + p;
                    post.transform.SetParent(shelter.transform, false);
                    post.transform.localPosition = new Vector3(0, 1.5f, p * 4.0f);
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
            // AC outdoor units
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

            // Downspouts
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "Rainwater_Downspout_G";
            pipe.transform.SetParent(parent.transform, false);
            pipe.transform.localPosition = new Vector3(15.8f, 21.0f, -8.0f);
            pipe.transform.localScale = new Vector3(0.18f, 21.0f, 0.18f);
            pipe.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
        }

        private static void BuildProps(GameObject parent)
        {
            // Granite benches
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

            // Campus Notice Board (The primary interactable InfoBoard!)
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

        // ================= 3. GAMEPLAY (DOORS, TRIGGERS, ANCHORS) =================
        private static void BuildGameplayContent(GameObject parent, GameObject worldParent)
        {
            // Doors Group
            GameObject goDoors = CreateChild(parent, "Doors");

            // BLDG_G_Door_Main (World position: X: 16.0, Y: 0, Z: 8.0)
            GameObject doorMain = CreateChild(goDoors, "BLDG_G_Door_Main");
            doorMain.transform.localPosition = new Vector3(16.0f, 0f, 8.0f);
            doorMain.layer = LayerMask.NameToLayer("Interactable");

            // Hollow Frame (Top Lintel, Left Post, Right Post)
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

            // Trigger box for auto open
            BoxCollider doorTrigger = doorMain.AddComponent<BoxCollider>();
            doorTrigger.isTrigger = true;
            doorTrigger.center = new Vector3(0, 1.5f, 0);
            doorTrigger.size = new Vector3(4.0f, 3.0f, 5.0f);

            var doorWid = doorMain.AddComponent<WorldObjectID>();
            doorWid.objectId = "G_MAIN_DOOR_001";
            doorWid.displayName = "Cửa Kính Tự Động Nhà G";

            var doorCtrl = doorMain.AddComponent<DoorController>();
            doorCtrl.doorName = "Cửa Chính Nhà G";
            doorCtrl.doorLeafLeft = leafLeft.transform;
            doorCtrl.doorLeafRight = leafRight.transform;
            doorCtrl.autoOpen = false; // Interact to open for MVP validation!
            doorCtrl.openOffset = new Vector3(0, 0, 1.8f);

            // Triggers Group
            GameObject goTriggers = CreateChild(parent, "Triggers");

            // TRG_EnterBuildingG
            GameObject trgEntrance = CreateChild(goTriggers, "TRG_EnterBuildingG");
            trgEntrance.transform.localPosition = new Vector3(16.0f, 1.5f, 8.0f);
            BoxCollider boxEnt = trgEntrance.AddComponent<BoxCollider>();
            boxEnt.isTrigger = true;
            boxEnt.size = new Vector3(3.5f, 3.0f, 4.5f);
            var gTrgEnt = trgEntrance.AddComponent<GameTrigger>();
            gTrgEnt.triggerId = "TRG_ENTRANCE_G_001";
            gTrgEnt.displayName = "Khu Vực Sảnh Nhà G";

            // TRG_EnterComputerLab
            GameObject trgLab = CreateChild(goTriggers, "TRG_EnterComputerLab");
            trgLab.transform.localPosition = new Vector3(24.0f, 1.5f, 3.0f);
            BoxCollider boxLab = trgLab.AddComponent<BoxCollider>();
            boxLab.isTrigger = true;
            boxLab.size = new Vector3(4.5f, 3.0f, 2.5f);
            var gTrgLab = trgLab.AddComponent<GameTrigger>();
            gTrgLab.triggerId = "TRG_LAB_G_001";
            gTrgLab.displayName = "Khu Vực Phòng Máy CNTT";

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
        }

        private static GameObject CreateAnchor(GameObject parent, string name, Vector3 pos)
        {
            GameObject anchor = CreateChild(parent, name);
            anchor.transform.localPosition = pos;

            // Add visible debug pin / diamond for developer visualization
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
            // Spawn Points
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

            // Player Interaction
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

            // Visual NPC
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
        }

        // ================= 6. LIGHTING & AUDIO =================
        private static void BuildLightingAndAudio(GameObject goLighting, GameObject goAudio)
        {
            // Directional Sun
            GameObject sunGO = CreateChild(goLighting, "Directional_Sun");
            Light sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.96f, 0.88f);
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sunGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // Interior downlights in Lobby and Lab
            GameObject lightLobby = CreateChild(goLighting, "PointLight_Lobby");
            lightLobby.transform.localPosition = new Vector3(20.0f, 3.4f, 6.0f);
            Light p1 = lightLobby.AddComponent<Light>();
            p1.type = LightType.Point;
            p1.range = 14f;
            p1.intensity = 2.2f;
            p1.color = new Color(1f, 0.95f, 0.85f);

            GameObject lightLab = CreateChild(goLighting, "PointLight_ComputerLab");
            lightLab.transform.localPosition = new Vector3(24.0f, 3.4f, -2.5f);
            Light p2 = lightLab.AddComponent<Light>();
            p2.type = LightType.Point;
            p2.range = 16f;
            p2.intensity = 2.8f;
            p2.color = new Color(0.9f, 0.95f, 1.0f);

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
    }
}
