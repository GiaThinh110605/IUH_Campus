using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus;

namespace IUHCampus.Editor
{
    public static class IUHGameReadyMasterBuilder
    {
        private const string ScenePath = "Assets/IUH_Campus/Scenes/IUH_RightCluster.unity";
        private const string MatPath = "Assets/IUH_Campus/Materials/";
        private const string TexPath = "Assets/IUH_Campus/Textures/";
        private const string PrefabPath = "Assets/IUH_Campus/Prefabs/";

        // Material Cache
        private static Material m_WallWeathered;
        private static Material m_ConcreteGround;
        private static Material m_ConcreteCurb;
        private static Material m_AsphaltRoad;
        private static Material m_DrainGrate;
        private static Material m_ManholeCover;
        private static Material m_SignWayfinding;
        private static Material m_SignNotice;
        private static Material m_ACLouvers;
        private static Material m_WindowBlinds;
        private static Material m_TreeBark;
        private static Material m_MatureFoliage;
        private static Material m_BenchGranite;
        private static Material m_FireExtinguisher;
        private static Material m_AccessControl;
        private static Material m_UrbanHouse;
        private static Material m_G_Wall, m_G_Glass, m_G_Railings, m_G_Roof, m_G_Sign;
        private static Material m_I_Wall, m_I_Glass, m_I_Roof, m_I_Railings;
        private static Material m_C_Wall, m_C_GreenGlass, m_C_Roof, m_C_Mint;
        private static Material m_Podium_Turquoise, m_Podium_Roof;
        private static Material m_DarkMetal, m_PVCPipe, m_ParkingLine, m_PlanterSoil, m_FireCabinet;
        private static Material m_WaterTankBlack, m_WaterTankStainless;
        private static Material m_BlackMarble, m_LightTile, m_LightWood, m_SageGreen, m_CyanSofa;
        private static Material m_LEDDisplay, m_ScreenPC, m_Poster1, m_Poster2, m_GlassFrosted, m_GlassClear;
        private static Material m_Chrome, m_NavyChair, m_CeilingGrid, m_MatteWhite, m_DarkMullion;
        private static Material[] m_BikeMats;
        private static Material[] m_HelmetMats;
        private static Material m_StudentWhiteShirt, m_StudentBluePolo, m_StudentJeans, m_StudentDarkPants, m_StudentSkin, m_StudentHair;

        [MenuItem("IUH Campus/Build Game-Ready Campus Scene & Architecture")]
        public static void BuildGameReadyScene()
        {
            Debug.Log("[IUH Game-Ready Master Builder] Starting Full Generation...");

            // 1. Generate Textures & Materials
            IUHTextureGenerator.GenerateAllTextures();
            IUHMaterialGenerator.GenerateAllMaterials();

            // 2. Setup Tags & Layers
            SetupTagsAndLayers();

            // 3. Generate Standardized Prefabs
            IUHCampusPrefabGenerator.GenerateAllStandardPrefabs();

            // 4. Load Materials
            LoadAllMaterials();

            // 5. Open or Create Scene
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            // 6. Clear Old Roots
            string[] oldRoots = { "IUH_RightCluster", "IUH_RightCluster_Interior", "Campus_RightCluster", "RightBuildingsCluster" };
            foreach (string rootName in oldRoots)
            {
                GameObject oldGo = GameObject.Find(rootName);
                if (oldGo != null) GameObject.DestroyImmediate(oldGo);
            }

            // 7. Construct Unified Game-Ready Hierarchy
            GameObject campusRoot = BuildCampusHierarchy();

            // 8. Disable unbaked reflection probes
            foreach (var probe in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                probe.enabled = false;
            }

            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.Refresh();
            Debug.Log("[IUH Game-Ready Master Builder] Game-Ready Campus Scene Built Successfully!");
        }

        private static void SetupTagsAndLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            string[] requiredTags = { "Player", "Interactable", "NPC", "MainCamera", "DoorAnchor", "Waypoint" };
            foreach (string tag in requiredTags)
            {
                bool exists = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) { exists = true; break; }
                }
                if (!exists)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                }
            }

            // Assign Layers 6-15
            string[] layerNames = {
                "Environment", "Ground", "Building", "Glass",
                "Player", "NPC", "Interactable", "Trigger",
                "Navigation", "Decoration"
            };
            for (int i = 0; i < layerNames.Length; i++)
            {
                int layerIdx = 6 + i;
                if (layerIdx < layersProp.arraySize)
                {
                    SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(layerIdx);
                    if (string.IsNullOrEmpty(layerProp.stringValue))
                    {
                        layerProp.stringValue = layerNames[i];
                    }
                }
            }

            tagManager.ApplyModifiedProperties();
        }

        private static void LoadAllMaterials()
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
            m_G_Railings = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Railings.mat");
            m_G_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Roof.mat");
            m_G_Sign = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Sign.mat");

            m_I_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Wall.mat");
            m_I_Glass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Glass.mat");
            m_I_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Roof.mat");
            m_I_Railings = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Railings.mat");

            m_C_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_Wall.mat");
            m_C_GreenGlass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_GreenGlass.mat");
            m_C_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_Roof.mat");
            m_C_Mint = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_AccentMint.mat");

            m_Podium_Turquoise = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Podium_Turquoise.mat");
            m_Podium_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Podium_Roof.mat");

            m_DarkMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");
            m_PVCPipe = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_PVC_Pipe.mat");
            m_ParkingLine = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Parking_Line.mat");
            m_PlanterSoil = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Planter_Soil.mat");
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
            m_Poster1 = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_Poster1.mat");
            m_Poster2 = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_Poster2.mat");
            m_GlassFrosted = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_GlassFrosted.mat");
            m_GlassClear = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_GlassClear.mat");
            m_Chrome = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_Chrome.mat");
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
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Yellow.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Orange.mat")
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

        public static GameObject BuildCampusHierarchy()
        {
            GameObject root = new GameObject("Campus_RightCluster");

            // 1. Architecture (Buildings G, I, C, Podium)
            GameObject goArch = new GameObject("Architecture");
            goArch.transform.SetParent(root.transform);
            BuildArchitecture(goArch);

            // 2. Interiors (Seamlessly integrated at Building G ground floor)
            GameObject goInt = new GameObject("Interiors");
            goInt.transform.SetParent(root.transform);
            BuildIntegratedInteriors(goInt);

            // 3. Ground (Plaza, Asphalt, Curbs, Zebra, Drainage)
            GameObject goGround = new GameObject("Ground");
            goGround.transform.SetParent(root.transform);
            BuildGround(goGround);

            // 4. Vegetation (Shade Trees, Palms, Shrubs)
            GameObject goVeg = new GameObject("Vegetation");
            goVeg.transform.SetParent(root.transform);
            BuildVegetation(goVeg);

            // 5. Parking (Motorcycle Rows, Shelters, Markings)
            GameObject goParking = new GameObject("Parking");
            goParking.transform.SetParent(root.transform);
            BuildParking(goParking);

            // 6. Infrastructure (MEP, AC, Downspouts, Electrical)
            GameObject goInfra = new GameObject("Infrastructure");
            goInfra.transform.SetParent(root.transform);
            BuildInfrastructure(goInfra);

            // 7. Props (Benches, Trash Bins, Wayfinding, Notice Board, Security)
            GameObject goProps = new GameObject("Props");
            goProps.transform.SetParent(root.transform);
            BuildProps(goProps);

            // 8. NPC (20 Student NPCs in Natural Groups)
            GameObject goNPC = new GameObject("NPC");
            goNPC.transform.SetParent(root.transform);
            BuildNPCs(goNPC);

            // 9. Gameplay (Anchors, Spawns, Routes, Triggers, Interactions)
            GameObject goGame = new GameObject("Gameplay");
            goGame.transform.SetParent(root.transform);
            BuildGameplay(goGame);

            // 10. Lighting (Sun + Interior Daylight)
            GameObject goLight = new GameObject("Lighting");
            goLight.transform.SetParent(root.transform);
            BuildLighting(goLight);

            // 11. Audio (Environmental Audio Zones)
            GameObject goAudio = new GameObject("Audio");
            goAudio.transform.SetParent(root.transform);
            BuildAudio(goAudio);

            // 12. Background Urban Context
            GameObject goUrban = new GameObject("Background_Urban");
            goUrban.transform.SetParent(root.transform);
            BuildBackgroundUrban(goUrban);

            // 13. Player Exploration Controller
            BuildPlayer(root);

            return root;
        }

        // ================= 1. ARCHITECTURE =================
        private static void BuildArchitecture(GameObject parent)
        {
            // --- BLDG_G: Hero Building (X: 24, Y: 21, Z: 10) ---
            GameObject bldgG = new GameObject("Building_G");
            bldgG.transform.SetParent(parent.transform);
            bldgG.transform.localPosition = new Vector3(24.0f, 0.0f, 10.0f);

            float wG = 16.0f;
            float hG = 42.0f;
            float dG = 44.0f;

            // Core Upper Shell (Floors 2 to 9, Y: 4.0 to 42.0)
            GameObject upperG = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperG.name = "G_Upper_Structure";
            upperG.transform.SetParent(bldgG.transform, false);
            upperG.transform.localPosition = new Vector3(0, 23.0f, 0);
            upperG.transform.localScale = new Vector3(wG, 38.0f, dG);
            upperG.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Ground Floor Pillars & Frame (leaves open portal for entrance!)
            GameObject gPillarRear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gPillarRear.name = "G_Pillar_East";
            gPillarRear.transform.SetParent(bldgG.transform, false);
            gPillarRear.transform.localPosition = new Vector3(wG * 0.5f - 0.5f, 2.0f, 0);
            gPillarRear.transform.localScale = new Vector3(1.0f, 4.0f, dG);
            gPillarRear.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Base Skirting
            GameObject gSkirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gSkirt.name = "G_Base_Skirting";
            gSkirt.transform.SetParent(bldgG.transform, false);
            gSkirt.transform.localPosition = new Vector3(0, 0.2f, 0);
            gSkirt.transform.localScale = new Vector3(wG + 0.4f, 0.4f, dG + 0.4f);
            gSkirt.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // Roof Penthouse & Parapet
            GameObject gRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gRoof.name = "G_Roof_Parapet";
            gRoof.transform.SetParent(bldgG.transform, false);
            gRoof.transform.localPosition = new Vector3(0, hG + 0.6f, 0);
            gRoof.transform.localScale = new Vector3(wG + 0.6f, 1.2f, dG + 0.6f);
            gRoof.GetComponent<MeshRenderer>().sharedMaterial = m_G_Roof;

            // Rooftop Water Tanks
            for (int t = 0; t < 3; t++)
            {
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "Rooftop_WaterTank_" + t;
                tank.transform.SetParent(bldgG.transform, false);
                tank.transform.localPosition = new Vector3(-2.0f + t * 2.5f, hG + 2.4f, 12.0f);
                tank.transform.localScale = new Vector3(1.8f, 1.6f, 1.8f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = (t == 1) ? m_WaterTankBlack : m_WaterTankStainless;
            }

            // --- BLDG_I: Background Academic Tower (X: 18, Y: 26, Z: 48) ---
            GameObject bldgI = new GameObject("Building_I");
            bldgI.transform.SetParent(parent.transform);
            bldgI.transform.localPosition = new Vector3(18.0f, 0.0f, 48.0f);

            float wI = 46.0f;
            float hI = 52.0f;
            float dI = 16.0f;

            GameObject coreI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coreI.name = "I_Core_Structure";
            coreI.transform.SetParent(bldgI.transform, false);
            coreI.transform.localPosition = new Vector3(0, hI * 0.5f, 0);
            coreI.transform.localScale = new Vector3(wI, hI, dI);
            coreI.GetComponent<MeshRenderer>().sharedMaterial = m_I_Wall;

            // Rooftop Tum Thang
            GameObject tumI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tumI.name = "I_Elevator_Penthouse";
            tumI.transform.SetParent(bldgI.transform, false);
            tumI.transform.localPosition = new Vector3(0, hI + 2.0f, 0);
            tumI.transform.localScale = new Vector3(8.0f, 4.0f, 6.0f);
            tumI.GetComponent<MeshRenderer>().sharedMaterial = m_I_Wall;

            // --- BLDG_C: Mid-Rise Academic Wing (X: 0, Y: 10, Z: 18) ---
            GameObject bldgC = new GameObject("Building_C");
            bldgC.transform.SetParent(parent.transform);
            bldgC.transform.localPosition = new Vector3(0.0f, 0.0f, 18.0f);

            float wC = 14.0f;
            float hC = 20.0f;
            float dC = 28.0f;

            GameObject coreC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coreC.name = "C_Core_Structure";
            coreC.transform.SetParent(bldgC.transform, false);
            coreC.transform.localPosition = new Vector3(0, hC * 0.5f, 0);
            coreC.transform.localScale = new Vector3(wC, hC, dC);
            coreC.GetComponent<MeshRenderer>().sharedMaterial = m_C_Wall;

            // Vertical Mint Accent Fins
            for (int f = 0; f < 4; f++)
            {
                GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fin.name = "C_Mint_Fin_" + f;
                fin.transform.SetParent(bldgC.transform, false);
                fin.transform.localPosition = new Vector3(wC * 0.5f + 0.15f, hC * 0.5f, -dC * 0.35f + f * 5.0f);
                fin.transform.localScale = new Vector3(0.3f, hC, 0.45f);
                fin.GetComponent<MeshRenderer>().sharedMaterial = m_C_Mint;
            }

            // --- LOW_PODIUM: Auxiliary / Entrance Block (X: 18, Y: 4.9, Z: -6) ---
            GameObject podium = new GameObject("Low_Podium");
            podium.transform.SetParent(parent.transform);
            podium.transform.localPosition = new Vector3(18.0f, 0.0f, -6.0f);

            float wP = 26.0f;
            float hP = 9.8f;
            float dP = 12.0f;

            // Upper Structure (Floors 2-3, Y: 3.8 to 9.8m) - preserves upper massing without blocking ground lab!
            GameObject upperP = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperP.name = "Podium_Upper_Structure";
            upperP.transform.SetParent(podium.transform, false);
            upperP.transform.localPosition = new Vector3(0, 6.8f, 0);
            upperP.transform.localScale = new Vector3(wP, 6.0f, dP);
            upperP.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Roof Parapet
            GameObject roofP = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofP.name = "Podium_Roof";
            roofP.transform.SetParent(podium.transform, false);
            roofP.transform.localPosition = new Vector3(0, hP + 0.1f, 0);
            roofP.transform.localScale = new Vector3(wP + 0.4f, 0.2f, dP + 0.4f);
            roofP.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

            // South Exterior Wall (Facade facing front street at Z = -12)
            GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWall.name = "Podium_Ground_SouthWall";
            southWall.transform.SetParent(podium.transform, false);
            southWall.transform.localPosition = new Vector3(0, 1.9f, -dP * 0.5f + 0.2f);
            southWall.transform.localScale = new Vector3(wP, 3.8f, 0.4f);
            southWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // West Exterior Wall (Facade facing Courtyard at X = 5)
            GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWall.name = "Podium_Ground_WestWall";
            westWall.transform.SetParent(podium.transform, false);
            westWall.transform.localPosition = new Vector3(-wP * 0.5f + 0.2f, 1.9f, 0);
            westWall.transform.localScale = new Vector3(0.4f, 3.8f, dP);
            westWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // East Exterior Wall (Facing perimeter at X = 31)
            GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "Podium_Ground_EastWall";
            eastWall.transform.SetParent(podium.transform, false);
            eastWall.transform.localPosition = new Vector3(wP * 0.5f - 0.2f, 1.9f, 0);
            eastWall.transform.localScale = new Vector3(0.4f, 3.8f, dP);
            eastWall.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Turquoise Corner Accents (Front Facade)
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

        // ================= 2. SEAMLESS INTEGRATED INTERIORS =================
        private static void BuildIntegratedInteriors(GameObject parent)
        {
            // Positioned inside Building G ground floor at X: 24, Y: 0, Z: 6
            // The grand entrance opens directly to the Courtyard at X: 16.0!
            GameObject intRoot = new GameObject("GroundFloor_Functional_Interiors");
            intRoot.transform.SetParent(parent.transform);
            intRoot.transform.localPosition = new Vector3(24.0f, 0.0f, 6.0f);

            // --- Zone A: Reception Lobby (X: -7 to 0, Z: -2 to 8, relative to intRoot) ---
            GameObject zoneA = new GameObject("Zone_A_FrontDesk_Lobby");
            zoneA.transform.SetParent(intRoot.transform, false);

            // Floor (Light Porcelain Tile)
            GameObject floorA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorA.name = "Floor_Lobby";
            floorA.transform.SetParent(zoneA.transform, false);
            floorA.transform.localPosition = new Vector3(-3.5f, -0.05f, 3.0f);
            floorA.transform.localScale = new Vector3(8.0f, 0.10f, 12.0f);
            floorA.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Slatted Timber Baffle Ceiling
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

            // Curved Reception Counter
            GameObject desk = new GameObject("Reception_Desk");
            desk.transform.SetParent(zoneA.transform, false);
            desk.transform.localPosition = new Vector3(-2.0f, 0, 4.5f);

            GameObject counterBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterBody.name = "CounterBody";
            counterBody.transform.SetParent(desk.transform, false);
            counterBody.transform.localPosition = new Vector3(0, 0.55f, 0);
            counterBody.transform.localScale = new Vector3(3.6f, 1.1f, 0.85f);
            counterBody.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            GameObject counterTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterTop.name = "CounterTop";
            counterTop.transform.SetParent(desk.transform, false);
            counterTop.transform.localPosition = new Vector3(0, 1.12f, 0);
            counterTop.transform.localScale = new Vector3(3.8f, 0.08f, 1.0f);
            counterTop.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            var interDesk = desk.AddComponent<IUHInteractable>();
            interDesk.interactionType = InteractionType.InspectionPoint;
            interDesk.promptText = "Bàn lễ tân & hướng dẫn sinh viên IUH";
            interDesk.interactionRange = 2.2f;

            // --- Zone B: Waiting Lounge (X: 0 to 7, Z: -2 to 8) ---
            GameObject zoneB = new GameObject("Zone_B_WaitingLounge_Media");
            zoneB.transform.SetParent(intRoot.transform, false);

            GameObject floorB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorB.name = "Floor_Lounge";
            floorB.transform.SetParent(zoneB.transform, false);
            floorB.transform.localPosition = new Vector3(3.5f, -0.05f, 3.0f);
            floorB.transform.localScale = new Vector3(7.0f, 0.10f, 12.0f);
            floorB.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Wall-mounted LED Media Screen
            GameObject media = GameObject.CreatePrimitive(PrimitiveType.Cube);
            media.name = "Media_LED_Display";
            media.transform.SetParent(zoneB.transform, false);
            media.transform.localPosition = new Vector3(6.8f, 2.0f, 3.0f);
            media.transform.localScale = new Vector3(0.10f, 2.4f, 4.5f);
            media.GetComponent<MeshRenderer>().sharedMaterial = m_LEDDisplay;

            // Lounge Sofas
            for (int s = 0; s < 2; s++)
            {
                GameObject sofa = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sofa.name = "Sofa_" + s;
                sofa.transform.SetParent(zoneB.transform, false);
                sofa.transform.localPosition = new Vector3(2.5f, 0.40f, 1.0f + s * 4.0f);
                sofa.transform.localScale = new Vector3(1.2f, 0.75f, 2.4f);
                sofa.GetComponent<MeshRenderer>().sharedMaterial = m_CyanSofa;
            }

            // --- Zone C: Training & Computer Lab (X: -6 to 6, Z: -14 to -3) ---
            GameObject zoneC = new GameObject("Zone_C_Training_ComputerLab");
            zoneC.transform.SetParent(intRoot.transform, false);

            // Floor (Glossy Black Marble)
            GameObject floorC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorC.name = "Floor_Lab_BlackMarble";
            floorC.transform.SetParent(zoneC.transform, false);
            floorC.transform.localPosition = new Vector3(0, -0.05f, -8.5f);
            floorC.transform.localScale = new Vector3(14.0f, 0.10f, 11.0f);
            floorC.GetComponent<MeshRenderer>().sharedMaterial = m_BlackMarble;

            // Ceiling with Acoustic Grid
            GameObject ceilC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceilC.name = "Ceiling_AcousticGrid";
            ceilC.transform.SetParent(zoneC.transform, false);
            ceilC.transform.localPosition = new Vector3(0, 3.65f, -8.5f);
            ceilC.transform.localScale = new Vector3(14.0f, 0.10f, 11.0f);
            ceilC.GetComponent<MeshRenderer>().sharedMaterial = m_CeilingGrid;

            // Lab Enclosure Walls (Back, Sides, and Corridor Partition)
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

            // North Partition with 4m wide entrance from Corridor
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

            // Whiteboard at front of Lab
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Teaching_Whiteboard";
            board.transform.SetParent(zoneC.transform, false);
            board.transform.localPosition = new Vector3(0, 1.9f, -13.85f);
            board.transform.localScale = new Vector3(6.5f, 2.0f, 0.08f);
            board.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            var interBoard = board.AddComponent<IUHInteractable>();
            interBoard.interactionType = InteractionType.InspectionPoint;
            interBoard.promptText = "Bảng giảng dạy chuyên đề CNTT - Đại học Công nghiệp TP.HCM";
            interBoard.interactionRange = 2.5f;

            // Teacher Podium & Workstation at front of Lab
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

            // Rows of Computer Workstations with Navy Task Chairs
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

                    // Monitor
                    GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    monitor.name = "Monitor";
                    monitor.transform.SetParent(deskPC.transform, false);
                    monitor.transform.localPosition = new Vector3(0, 0.70f, -0.15f);
                    monitor.transform.localScale = new Vector3(0.75f, 0.48f, 0.08f);
                    monitor.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

                    // Navy Task Chair with Chrome Base
                    GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    chair.name = "Navy_Task_Chair";
                    chair.transform.SetParent(deskPC.transform, false);
                    chair.transform.localPosition = new Vector3(0, 0.22f, 0.75f);
                    chair.transform.localScale = new Vector3(0.55f, 0.44f, 0.55f);
                    chair.GetComponent<MeshRenderer>().sharedMaterial = m_NavyChair;

                    var interPC = deskPC.AddComponent<IUHInteractable>();
                    interPC.interactionType = InteractionType.Computer;
                    interPC.promptText = "Nhấn E để đăng nhập máy trạm IUH";
                    interPC.interactionRange = 1.8f;
                }
            }

            // --- Corridor Circulation Spine (Connecting Lobby to Lab at Z = -3.0) ---
            GameObject corridor = new GameObject("Corridor_Circulation_Spine");
            corridor.transform.SetParent(intRoot.transform, false);

            // Overhead Suspended Wayfinding Sign
            GameObject corrSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corrSign.name = "Corridor_Wayfinding_Sign";
            corrSign.transform.SetParent(corridor.transform, false);
            corrSign.transform.localPosition = new Vector3(0, 2.85f, -2.85f);
            corrSign.transform.localScale = new Vector3(2.6f, 0.45f, 0.08f);
            corrSign.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            // Emergency Exit Lightbox above door
            GameObject exitSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitSign.name = "Emergency_Exit_Sign";
            exitSign.transform.SetParent(corridor.transform, false);
            exitSign.transform.localPosition = new Vector3(0, 3.25f, -2.90f);
            exitSign.transform.localScale = new Vector3(0.85f, 0.28f, 0.12f);
            exitSign.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            // Fire Hose Cabinet on side wall
            GameObject fireCab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fireCab.name = "Corridor_Fire_Cabinet";
            fireCab.transform.SetParent(corridor.transform, false);
            fireCab.transform.localPosition = new Vector3(-6.85f, 1.25f, -2.5f);
            fireCab.transform.localScale = new Vector3(0.20f, 1.3f, 0.90f);
            fireCab.GetComponent<MeshRenderer>().sharedMaterial = m_FireCabinet;

            // --- Grand Entrance Facing Courtyard (X = -8.0, opening directly to Courtyard at X = 16.0!) ---
            GameObject grandEnt = new GameObject("Courtyard_Public_Entrance");
            grandEnt.transform.SetParent(intRoot.transform, false);
            grandEnt.transform.localPosition = new Vector3(-8.0f, 0, 2.0f);

            // Cantilever Entrance Canopy
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = "Entrance_Canopy";
            canopy.transform.SetParent(grandEnt.transform, false);
            canopy.transform.localPosition = new Vector3(-2.2f, 3.6f, 0);
            canopy.transform.localScale = new Vector3(4.8f, 0.25f, 6.5f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Canopy Front Fascia Sign
            GameObject fascia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fascia.name = "Canopy_Signboard";
            fascia.transform.SetParent(canopy.transform, false);
            fascia.transform.localPosition = new Vector3(-0.52f, -0.15f, 0);
            fascia.transform.localScale = new Vector3(0.08f, 0.65f, 6.2f);
            fascia.GetComponent<MeshRenderer>().sharedMaterial = m_G_Sign;

            // Automatic Sliding Glass Doors
            GameObject doors = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doors.name = "Automatic_Sliding_Glass_Doors";
            doors.transform.SetParent(grandEnt.transform, false);
            doors.transform.localPosition = new Vector3(0, 1.35f, 0);
            doors.transform.localScale = new Vector3(0.12f, 2.6f, 3.8f);
            doors.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            var interDoor = doors.AddComponent<IUHInteractable>();
            interDoor.interactionType = InteractionType.Door;
            interDoor.promptText = "Cửa kính tự động - Lối vào chính Nhà G";
            interDoor.interactionRange = 2.5f;

            // Access Control Card Reader
            GameObject rfid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rfid.name = "Access_Control_Reader";
            rfid.transform.SetParent(grandEnt.transform, false);
            rfid.transform.localPosition = new Vector3(-0.10f, 1.25f, 2.2f);
            rfid.transform.localScale = new Vector3(0.06f, 0.28f, 0.16f);
            rfid.GetComponent<MeshRenderer>().sharedMaterial = m_AccessControl;

            // Fire Extinguisher near Entrance
            GameObject fireExt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fireExt.name = "Fire_Extinguisher_Wall";
            fireExt.transform.SetParent(grandEnt.transform, false);
            fireExt.transform.localPosition = new Vector3(0.15f, 1.10f, -2.2f);
            fireExt.transform.localScale = new Vector3(0.20f, 0.60f, 0.25f);
            fireExt.GetComponent<MeshRenderer>().sharedMaterial = m_FireExtinguisher;

            var interExt = fireExt.AddComponent<IUHInteractable>();
            interExt.interactionType = InteractionType.FireEquipment;
            interExt.promptText = "Bình chữa cháy khí CO2 - Vị trí cửa chính";
            interExt.interactionRange = 1.8f;
        }

        // ================= 3. GROUND =================
        private static void BuildGround(GameObject parent)
        {
            // Concrete Plaza Ground (Substrate)
            GameObject plaza = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plaza.name = "Ground_ConcretePlaza";
            plaza.transform.SetParent(parent.transform);
            plaza.transform.localPosition = new Vector3(18.0f, -0.1f, 16.0f);
            plaza.transform.localScale = new Vector3(72.0f, 0.2f, 82.0f);
            plaza.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            // Asphalt Roads & Courtyard Internal Lane
            GameObject roadFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadFront.name = "Road_FrontLane";
            roadFront.transform.SetParent(parent.transform);
            roadFront.transform.localPosition = new Vector3(18.0f, 0.02f, -14.0f);
            roadFront.transform.localScale = new Vector3(68.0f, 0.04f, 6.8f);
            roadFront.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;

            GameObject roadCourt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadCourt.name = "Road_CourtyardAsphalt";
            roadCourt.transform.SetParent(parent.transform);
            roadCourt.transform.localPosition = new Vector3(18.0f, 0.02f, 14.0f);
            roadCourt.transform.localScale = new Vector3(22.0f, 0.04f, 32.0f);
            roadCourt.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;

            // Pedestrian Zebra Crossing
            GameObject zebra = new GameObject("PedestrianZebraCrossing");
            zebra.transform.SetParent(parent.transform);
            for (int zc = 0; zc < 6; zc++)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = "ZebraBar_" + zc;
                bar.transform.SetParent(zebra.transform);
                bar.transform.localPosition = new Vector3(11.0f + zc * 1.0f, 0.045f, -14.0f);
                bar.transform.localScale = new Vector3(0.55f, 0.01f, 4.8f);
                bar.GetComponent<MeshRenderer>().sharedMaterial = m_ParkingLine;
            }

            // Concrete Curbs dividing roads from walkways
            for (int c = -1; c <= 1; c += 2)
            {
                GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curb.name = "Curb_Roadside_" + c;
                curb.transform.SetParent(parent.transform);
                curb.transform.localPosition = new Vector3(18.0f, 0.10f, -14.0f + c * 3.5f);
                curb.transform.localScale = new Vector3(68.0f, 0.18f, 0.35f);
                curb.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;
            }

            // Drainage Grates & Sewer Manholes
            for (int mg = 0; mg < 4; mg++)
            {
                GameObject grate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grate.name = "DrainGrate_Margin_" + mg;
                grate.transform.SetParent(parent.transform);
                grate.transform.localPosition = new Vector3(4.0f + mg * 9.0f, 0.045f, -17.2f);
                grate.transform.localScale = new Vector3(1.2f, 0.02f, 0.55f);
                grate.GetComponent<MeshRenderer>().sharedMaterial = m_DrainGrate;
            }

            for (int mh = 0; mh < 2; mh++)
            {
                GameObject manhole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                manhole.name = "Manhole_Cover_" + mh;
                manhole.transform.SetParent(parent.transform);
                manhole.transform.localPosition = new Vector3(10.0f + mh * 16.0f, 0.045f, 14.0f);
                manhole.transform.localScale = new Vector3(1.1f, 0.02f, 1.1f);
                manhole.GetComponent<MeshRenderer>().sharedMaterial = m_ManholeCover;
            }
        }

        // ================= 4. VEGETATION =================
        private static void BuildVegetation(GameObject parent)
        {
            Vector3[] treePositions = {
                new Vector3(6.0f, 0, -4.0f),    // Courtyard entry walkway
                new Vector3(14.0f, 0, 26.0f),   // Rear courtyard near Bldg I
                new Vector3(32.0f, 0, -6.0f),   // Front flank of Bldg G
                new Vector3(2.0f, 0, 8.0f)      // Sidewalk near Bldg C
            };

            for (int i = 0; i < treePositions.Length; i++)
            {
                GameObject tree = new GameObject("TropicalShadeTree_" + i);
                tree.transform.SetParent(parent.transform);
                tree.transform.localPosition = treePositions[i];

                // Concrete Tree Pit Curb
                GameObject pit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pit.name = "TreePit_Curb";
                pit.transform.SetParent(tree.transform, false);
                pit.transform.localPosition = new Vector3(0, 0.12f, 0);
                pit.transform.localScale = new Vector3(3.2f, 0.24f, 3.2f);
                pit.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

                // Soil
                GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
                soil.name = "Soil";
                soil.transform.SetParent(pit.transform, false);
                soil.transform.localPosition = new Vector3(0, 0.35f, 0);
                soil.transform.localScale = new Vector3(0.85f, 0.40f, 0.85f);
                soil.GetComponent<MeshRenderer>().sharedMaterial = m_PlanterSoil;

                // Trunk
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk";
                trunk.transform.SetParent(tree.transform, false);
                trunk.transform.localPosition = new Vector3(0, 4.5f, 0);
                trunk.transform.localScale = new Vector3(1.2f, 4.5f, 1.2f);
                trunk.GetComponent<MeshRenderer>().sharedMaterial = m_TreeBark;

                // 3 Organic Broadleaf Canopy Tiers
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

            // Flanking Royal Palms along Walkway
            for (int p = 0; p < 4; p++)
            {
                GameObject palm = new GameObject("RoyalPalm_" + p);
                palm.transform.SetParent(parent.transform);
                palm.transform.localPosition = new Vector3(8.0f + p * 8.0f, 0, -9.5f);

                GameObject pTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pTrunk.name = "PalmTrunk";
                pTrunk.transform.SetParent(palm.transform, false);
                pTrunk.transform.localPosition = new Vector3(0, 4.0f, 0);
                pTrunk.transform.localScale = new Vector3(0.45f, 4.0f, 0.45f);
                pTrunk.GetComponent<MeshRenderer>().sharedMaterial = m_TreeBark;

                GameObject fronds = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fronds.name = "Fronds";
                fronds.transform.SetParent(palm.transform, false);
                fronds.transform.localPosition = new Vector3(0, 8.2f, 0);
                fronds.transform.localScale = new Vector3(4.2f, 1.8f, 4.2f);
                fronds.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
            }
        }

        // ================= 5. PARKING =================
        private static void BuildParking(GameObject parent)
        {
            // Sheltered Motorcycle Parking Rows under Corrugated Metal Canopies
            GameObject shelters = new GameObject("Parking_Shelters");
            shelters.transform.SetParent(parent.transform);

            // Double Rows of Shelters inside Courtyard (Z = 4 to 20, X = 7.5 to 11)
            float[] shelterXs = { 7.5f, 10.5f };
            for (int s = 0; s < shelterXs.Length; s++)
            {
                GameObject shelter = new GameObject("Canopy_Shelter_" + s);
                shelter.transform.SetParent(shelters.transform);
                shelter.transform.localPosition = new Vector3(shelterXs[s], 0, 12.0f);

                // Steel Posts
                for (int p = -2; p <= 2; p++)
                {
                    GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    post.name = "Steel_Post_" + p;
                    post.transform.SetParent(shelter.transform, false);
                    post.transform.localPosition = new Vector3(0, 1.5f, p * 4.0f);
                    post.transform.localScale = new Vector3(0.12f, 1.5f, 0.12f);
                    post.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
                }

                // Corrugated Roof
                GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roof.name = "Corrugated_Roof";
                roof.transform.SetParent(shelter.transform, false);
                roof.transform.localPosition = new Vector3(0, 3.05f, 0);
                roof.transform.localRotation = Quaternion.Euler(4.0f, 0, 0);
                roof.transform.localScale = new Vector3(2.8f, 0.10f, 18.0f);
                roof.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

                // Yellow Parking Bay Line
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "Stall_Line";
                line.transform.SetParent(shelter.transform, false);
                line.transform.localPosition = new Vector3(1.35f, 0.045f, 0);
                line.transform.localScale = new Vector3(0.12f, 0.01f, 18.0f);
                line.GetComponent<MeshRenderer>().sharedMaterial = m_ParkingLine;
            }

            // Dense Motorcycle Rows (~100 motorbikes with organic jitter, colors & helmets)
            GameObject bikesGroup = new GameObject("Motorbike_Rows");
            bikesGroup.transform.SetParent(parent.transform);

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

                    GameObject bike = new GameObject($"Bike_R{r}_B{b}");
                    bike.transform.SetParent(bikesGroup.transform);
                    bike.transform.localPosition = new Vector3(xBase, 0, z);
                    bike.transform.localRotation = Quaternion.Euler(0, 90.0f + jitterRot, lean);

                    // Body
                    GameObject bBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bBody.name = "Body";
                    bBody.transform.SetParent(bike.transform, false);
                    bBody.transform.localPosition = new Vector3(0, 0.52f, 0);
                    bBody.transform.localScale = new Vector3(0.38f, 0.42f, 1.25f);
                    bBody.GetComponent<MeshRenderer>().sharedMaterial = m_BikeMats[colIdx];

                    // Seat
                    GameObject bSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bSeat.name = "Seat";
                    bSeat.transform.SetParent(bike.transform, false);
                    bSeat.transform.localPosition = new Vector3(0, 0.76f, -0.15f);
                    bSeat.transform.localScale = new Vector3(0.32f, 0.10f, 0.70f);
                    bSeat.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    // Wheels
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

                    // Helmet on ~40% of bikes
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

            // Add Carving NavMeshObstacle along the bike rows so NPCs navigate around them
            for (int r = 0; r < 2; r++)
            {
                GameObject nmoGO = new GameObject("NavObstacle_BikeRow_" + r);
                nmoGO.transform.SetParent(parent.transform);
                nmoGO.transform.localPosition = new Vector3(shelterXs[r], 0.6f, 12.0f);
                var nmo = nmoGO.AddComponent<UnityEngine.AI.NavMeshObstacle>();
                nmo.carving = true;
                nmo.size = new Vector3(1.8f, 1.5f, 18.0f);
            }
        }

        // ================= 6. INFRASTRUCTURE =================
        private static void BuildInfrastructure(GameObject parent)
        {
            // Outdoor AC Condenser Units on Building G Facade
            GameObject acGroup = new GameObject("AC_Units");
            acGroup.transform.SetParent(parent.transform);

            for (int f = 2; f <= 8; f++)
            {
                float y = f * 3.8f + 1.2f;
                for (int b = -2; b <= 2; b += 2)
                {
                    GameObject ac = new GameObject($"AC_G_F{f}_B{b}");
                    ac.transform.SetParent(acGroup.transform);
                    ac.transform.localPosition = new Vector3(15.65f, y, 10.0f + b * 5.0f);

                    GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    casing.name = "Casing";
                    casing.transform.SetParent(ac.transform, false);
                    casing.transform.localScale = new Vector3(0.45f, 0.65f, 0.85f);
                    casing.GetComponent<MeshRenderer>().sharedMaterial = m_ACLouvers;

                    GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bracket.name = "Bracket";
                    bracket.transform.SetParent(ac.transform, false);
                    bracket.transform.localPosition = new Vector3(0.10f, -0.38f, 0);
                    bracket.transform.localScale = new Vector3(0.35f, 0.08f, 0.75f);
                    bracket.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    GameObject drain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    drain.name = "DrainTube";
                    drain.transform.SetParent(ac.transform, false);
                    drain.transform.localPosition = new Vector3(0.12f, -0.7f, 0.35f);
                    drain.transform.localScale = new Vector3(0.04f, 0.65f, 0.04f);
                    drain.GetComponent<MeshRenderer>().sharedMaterial = m_PVCPipe;
                }
            }

            // Rainwater Downspout Pipes running down building corners
            Vector3[] downspoutPositions = {
                new Vector3(16.2f, 21.0f, -11.8f),
                new Vector3(31.8f, 21.0f, -11.8f),
                new Vector3(16.2f, 21.0f, 31.8f),
                new Vector3(31.8f, 21.0f, 31.8f)
            };

            for (int ds = 0; ds < downspoutPositions.Length; ds++)
            {
                GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pipe.name = "Rainwater_Downspout_" + ds;
                pipe.transform.SetParent(parent.transform);
                pipe.transform.localPosition = downspoutPositions[ds];
                pipe.transform.localScale = new Vector3(0.15f, 21.0f, 0.15f);
                pipe.GetComponent<MeshRenderer>().sharedMaterial = m_PVCPipe;
            }

            // Weatherproof Electrical Distribution Cabinet
            GameObject elecCabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            elecCabin.name = "Electrical_Service_Cabinet";
            elecCabin.transform.SetParent(parent.transform);
            elecCabin.transform.localPosition = new Vector3(16.25f, 1.25f, -8.0f);
            elecCabin.transform.localScale = new Vector3(0.45f, 1.60f, 1.10f);
            elecCabin.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            var interElec = elecCabin.AddComponent<IUHInteractable>();
            interElec.interactionType = InteractionType.InspectionPoint;
            interElec.promptText = "Tủ điện phân phối kỹ thuật - Khu vực Nhà G";
            interElec.interactionRange = 2.0f;
        }

        // ================= 7. PROPS =================
        private static void BuildProps(GameObject parent)
        {
            // Polished Granite Benches under shade trees and along walkways
            Vector3[] benchPositions = {
                new Vector3(8.5f, 0, -4.0f),
                new Vector3(10.5f, 0, -1.0f),
                new Vector3(11.0f, 0, 5.0f),
                new Vector3(11.0f, 0, 11.0f),
                new Vector3(14.0f, 0, 23.0f)
            };

            for (int b = 0; b < benchPositions.Length; b++)
            {
                GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bench.name = "GraniteBench_" + b;
                bench.transform.SetParent(parent.transform);
                bench.transform.localPosition = benchPositions[b] + new Vector3(0, 0.44f, 0);
                bench.transform.localScale = new Vector3(1.8f, 0.08f, 0.55f);
                bench.GetComponent<MeshRenderer>().sharedMaterial = m_BenchGranite;

                // Legs
                for (int l = -1; l <= 1; l += 2)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = "Leg_" + l;
                    leg.transform.SetParent(bench.transform, false);
                    leg.transform.localPosition = new Vector3(l * 0.65f, -0.24f, 0);
                    leg.transform.localScale = new Vector3(0.16f, 0.40f, 0.45f);
                    leg.GetComponent<MeshRenderer>().sharedMaterial = m_BenchGranite;
                }

                var inter = bench.AddComponent<IUHInteractable>();
                inter.interactionType = InteractionType.Chair;
                inter.promptText = "Nhấn E để ngồi ghế đá";
                inter.interactionRange = 2.0f;
            }

            // Dual Compartment Recycling Bins
            Vector3[] binPositions = {
                new Vector3(13.5f, 0, -6.5f),
                new Vector3(13.5f, 0, 3.0f),
                new Vector3(13.5f, 0, 17.0f)
            };

            for (int bi = 0; bi < binPositions.Length; bi++)
            {
                GameObject bin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bin.name = "DualRecycleBin_" + bi;
                bin.transform.SetParent(parent.transform);
                bin.transform.localPosition = binPositions[bi] + new Vector3(0, 0.52f, 0);
                bin.transform.localScale = new Vector3(0.95f, 0.95f, 0.45f);
                bin.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;
            }

            // Campus Directional Signposts with physically mounted frames
            GameObject signPost1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signPost1.name = "DirectionalSign_Courtyard";
            signPost1.transform.SetParent(parent.transform);
            signPost1.transform.localPosition = new Vector3(12.5f, 2.0f, -8.0f);
            signPost1.transform.localScale = new Vector3(1.6f, 0.85f, 0.08f);
            signPost1.GetComponent<MeshRenderer>().sharedMaterial = m_SignWayfinding;

            GameObject postPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postPole.name = "Pole";
            postPole.transform.SetParent(signPost1.transform, false);
            postPole.transform.localPosition = new Vector3(0, -1.25f, 0);
            postPole.transform.localScale = new Vector3(0.08f, 1.25f, 0.08f);
            postPole.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            var interSign = signPost1.AddComponent<IUHInteractable>();
            interSign.interactionType = InteractionType.InfoBoard;
            interSign.promptText = "Bảng chỉ dẫn: Nhà G - Nhà I - Nhà C - Bãi xe";
            interSign.interactionRange = 2.4f;

            // Notice Bulletin Board
            GameObject noticeBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            noticeBoard.name = "BulletinNoticeBoard";
            noticeBoard.transform.SetParent(parent.transform);
            noticeBoard.transform.localPosition = new Vector3(15.2f, 1.8f, -2.5f);
            noticeBoard.transform.localScale = new Vector3(0.08f, 1.4f, 2.4f);
            noticeBoard.GetComponent<MeshRenderer>().sharedMaterial = m_SignNotice;

            var interNotice = noticeBoard.AddComponent<IUHInteractable>();
            interNotice.interactionType = InteractionType.InfoBoard;
            interNotice.promptText = "Bảng thông báo hoạt động Đoàn Thanh Niên & Hội Sinh Viên IUH";
            interNotice.interactionRange = 2.4f;

            // Security Booth at Entrance
            GameObject booth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            booth.name = "Security_Guard_Booth";
            booth.transform.SetParent(parent.transform);
            booth.transform.localPosition = new Vector3(6.5f, 1.4f, -13.5f);
            booth.transform.localScale = new Vector3(2.4f, 2.8f, 2.4f);
            booth.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject boothRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boothRoof.name = "BoothRoof";
            boothRoof.transform.SetParent(booth.transform, false);
            boothRoof.transform.localPosition = new Vector3(0, 0.52f, 0);
            boothRoof.transform.localScale = new Vector3(1.15f, 0.10f, 1.15f);
            boothRoof.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            var interBooth = booth.AddComponent<IUHInteractable>();
            interBooth.interactionType = InteractionType.InspectionPoint;
            interBooth.promptText = "Chốt kiểm soát an ninh & thẻ sinh viên IUH";
            interBooth.interactionRange = 2.5f;
        }

        // ================= 8. CAMPUS LIFE (20 STUDENT NPCS) =================
        private static void BuildNPCs(GameObject parent)
        {
            // Group 1: Studying on Benches (4 NPCs)
            Vector3[] benchNpcPos = {
                new Vector3(8.5f, 0.45f, -4.0f),
                new Vector3(8.9f, 0.45f, -4.0f),
                new Vector3(10.5f, 0.45f, -1.0f),
                new Vector3(11.0f, 0.45f, 5.0f)
            };
            for (int i = 0; i < benchNpcPos.Length; i++)
            {
                CreateStudentNPC(parent, "NPC_Sitting_" + i, benchNpcPos[i], Quaternion.Euler(0, (i % 2 == 0) ? 90f : -90f, 0), isSitting: true, (i % 2 == 0));
            }

            // Group 2: Chatting in Circles (6 NPCs)
            Vector3[] chatPos = {
                new Vector3(10.0f, 0, 0.5f),
                new Vector3(10.6f, 0, 0.8f),
                new Vector3(10.2f, 0, 1.2f),
                new Vector3(12.5f, 0, 14.5f),
                new Vector3(13.2f, 0, 14.8f),
                new Vector3(12.8f, 0, 15.3f)
            };
            for (int i = 0; i < chatPos.Length; i++)
            {
                float rot = i * 60f;
                CreateStudentNPC(parent, "NPC_Chatting_" + i, chatPos[i], Quaternion.Euler(0, rot, 0), isSitting: false, (i % 2 == 1));
            }

            // Group 3: Parking Area checking bikes (4 NPCs)
            Vector3[] parkPos = {
                new Vector3(16.5f, 0, 6.0f),
                new Vector3(16.5f, 0, 11.0f),
                new Vector3(19.8f, 0, 8.5f),
                new Vector3(19.8f, 0, 15.0f)
            };
            for (int i = 0; i < parkPos.Length; i++)
            {
                CreateStudentNPC(parent, "NPC_Parking_" + i, parkPos[i], Quaternion.Euler(0, (i % 2 == 0) ? 45f : -45f, 0), isSitting: false, (i % 2 == 0));
            }

            // Group 4: Inside Lobby & Training Lab (6 NPCs)
            Vector3[] lobbyNpcPos = {
                new Vector3(22.0f, 0, 10.5f),  // Waiting in Lounge
                new Vector3(22.5f, 0, 9.8f),   // Lounge conversation
                new Vector3(22.2f, 0, 14.5f),  // Near reception desk
                new Vector3(20.8f, 0, -0.5f),  // Training Lab PC 1
                new Vector3(27.2f, 0, -0.5f),  // Training Lab PC 2
                new Vector3(24.0f, 0, -4.0f)   // Standing by whiteboard
            };
            for (int i = 0; i < lobbyNpcPos.Length; i++)
            {
                CreateStudentNPC(parent, "NPC_Interior_" + i, lobbyNpcPos[i], Quaternion.Euler(0, (i < 3) ? 180f : 0f, 0), isSitting: (i >= 3 && i <= 4), (i % 2 == 1));
            }
        }

        private static void CreateStudentNPC(GameObject parent, string name, Vector3 pos, Quaternion rot, bool isSitting, bool isYouthUnionPolo)
        {
            GameObject npc = new GameObject(name);
            npc.transform.SetParent(parent.transform);
            npc.transform.localPosition = pos;
            npc.transform.localRotation = rot;

            float yOffset = isSitting ? 0.35f : 0.0f;

            // Torso (White uniform shirt or Blue Youth Union polo)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(npc.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.05f - yOffset * 0.4f, 0);
            torso.transform.localScale = new Vector3(0.42f, 0.58f, 0.24f);
            torso.GetComponent<MeshRenderer>().sharedMaterial = isYouthUnionPolo ? m_StudentBluePolo : m_StudentWhiteShirt;

            // Backpack
            GameObject pack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pack.name = "Backpack";
            pack.transform.SetParent(torso.transform, false);
            pack.transform.localPosition = new Vector3(0, 0.05f, -0.65f);
            pack.transform.localScale = new Vector3(0.85f, 0.85f, 0.55f);
            pack.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npc.transform, false);
            head.transform.localPosition = new Vector3(0, 1.52f - yOffset * 0.4f, 0);
            head.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            head.GetComponent<MeshRenderer>().sharedMaterial = m_StudentSkin;

            // Hair
            GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(head.transform, false);
            hair.transform.localPosition = new Vector3(0, 0.25f, 0);
            hair.transform.localScale = new Vector3(1.05f, 0.65f, 1.05f);
            hair.GetComponent<MeshRenderer>().sharedMaterial = m_StudentHair;

            // Legs
            for (int l = -1; l <= 1; l += 2)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg_" + l;
                leg.transform.SetParent(npc.transform, false);
                if (isSitting)
                {
                    leg.transform.localPosition = new Vector3(l * 0.12f, 0.45f, 0.22f);
                    leg.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    leg.transform.localScale = new Vector3(0.16f, 0.55f, 0.16f);
                }
                else
                {
                    leg.transform.localPosition = new Vector3(l * 0.12f, 0.42f, 0);
                    leg.transform.localScale = new Vector3(0.16f, 0.76f, 0.18f);
                }
                leg.GetComponent<MeshRenderer>().sharedMaterial = (isYouthUnionPolo) ? m_StudentJeans : m_StudentDarkPants;
            }

            BoxCollider col = npc.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.88f - yOffset * 0.4f, 0);
            col.size = new Vector3(0.6f, 1.75f, 0.6f);

            var inter = npc.AddComponent<IUHInteractable>();
            inter.interactionType = InteractionType.NPC;
            inter.promptText = "Trò chuyện với sinh viên IUH";
            inter.interactionRange = 2.0f;
        }

        // ================= 9. GAMEPLAY FOUNDATION ANCHORS =================
        private static void BuildGameplay(GameObject parent)
        {
            // 1. Player Spawn Points
            GameObject spawns = new GameObject("PlayerSpawnPoints");
            spawns.transform.SetParent(parent.transform);

            GameObject spawnCourt = new GameObject("PlayerSpawn_Courtyard");
            spawnCourt.transform.SetParent(spawns.transform);
            spawnCourt.transform.localPosition = new Vector3(11.0f, 0.05f, -5.0f);
            spawnCourt.transform.localRotation = Quaternion.Euler(0, 35f, 0);

            GameObject spawnEnt = new GameObject("PlayerSpawn_Entrance_G");
            spawnEnt.transform.SetParent(spawns.transform);
            spawnEnt.transform.localPosition = new Vector3(13.5f, 0.05f, 6.0f);
            spawnEnt.transform.localRotation = Quaternion.Euler(0, 90f, 0);

            GameObject spawnLobby = new GameObject("PlayerSpawn_Lobby");
            spawnLobby.transform.SetParent(spawns.transform);
            spawnLobby.transform.localPosition = new Vector3(18.0f, 0.05f, 6.0f);
            spawnLobby.transform.localRotation = Quaternion.Euler(0, 90f, 0);

            // 2. NPC Routes & Waypoints
            GameObject routes = new GameObject("NPCRoutes");
            routes.transform.SetParent(parent.transform);

            // Route 1: Courtyard Walkway
            GameObject rCourt = new GameObject("NPC_Route_Courtyard");
            rCourt.transform.SetParent(routes.transform);
            Vector3[] wpCourt = {
                new Vector3(11.0f, 0, -8.0f),
                new Vector3(11.0f, 0, 0.0f),
                new Vector3(11.0f, 0, 10.0f),
                new Vector3(11.0f, 0, 20.0f)
            };
            for (int w = 0; w < wpCourt.Length; w++)
            {
                GameObject wp = new GameObject("WP_" + w);
                wp.transform.SetParent(rCourt.transform);
                wp.transform.localPosition = wpCourt[w];
                wp.tag = "Waypoint";
            }

            // Route 2: Entrance to Lobby
            GameObject rEnt = new GameObject("NPC_Route_Entrance_G");
            rEnt.transform.SetParent(routes.transform);
            Vector3[] wpEnt = {
                new Vector3(11.0f, 0, 6.0f),
                new Vector3(14.0f, 0, 6.0f),
                new Vector3(18.0f, 0, 6.0f),
                new Vector3(22.0f, 0, 6.0f)
            };
            for (int w = 0; w < wpEnt.Length; w++)
            {
                GameObject wp = new GameObject("WP_" + w);
                wp.transform.SetParent(rEnt.transform);
                wp.transform.localPosition = wpEnt[w];
                wp.tag = "Waypoint";
            }

            // Route 3: Parking Circulation
            GameObject rPark = new GameObject("NPC_Route_Parking");
            rPark.transform.SetParent(routes.transform);
            Vector3[] wpPark = {
                new Vector3(16.0f, 0, 2.0f),
                new Vector3(16.0f, 0, 10.0f),
                new Vector3(16.0f, 0, 18.0f),
                new Vector3(19.5f, 0, 18.0f)
            };
            for (int w = 0; w < wpPark.Length; w++)
            {
                GameObject wp = new GameObject("WP_" + w);
                wp.transform.SetParent(rPark.transform);
                wp.transform.localPosition = wpPark[w];
                wp.tag = "Waypoint";
            }

            // 3. Door Anchors
            GameObject doors = new GameObject("DoorAnchors");
            doors.transform.SetParent(parent.transform);

            GameObject dMain = new GameObject("DoorAnchor_Main_G");
            dMain.transform.SetParent(doors.transform);
            dMain.transform.localPosition = new Vector3(16.0f, 0, 6.0f);
            dMain.tag = "DoorAnchor";

            GameObject dLab = new GameObject("DoorAnchor_Lab_C");
            dLab.transform.SetParent(doors.transform);
            dLab.transform.localPosition = new Vector3(24.0f, 0, 3.0f);
            dLab.tag = "DoorAnchor";

            // 4. Trigger Volumes
            GameObject triggers = new GameObject("TriggerVolumes");
            triggers.transform.SetParent(parent.transform);

            GameObject tEnt = new GameObject("Trigger_Entrance_G");
            tEnt.transform.SetParent(triggers.transform);
            tEnt.transform.localPosition = new Vector3(16.0f, 1.5f, 6.0f);
            BoxCollider tc1 = tEnt.AddComponent<BoxCollider>();
            tc1.isTrigger = true;
            tc1.size = new Vector3(3.0f, 3.0f, 4.0f);

            GameObject tLab = new GameObject("Trigger_Lab_Room");
            tLab.transform.SetParent(triggers.transform);
            tLab.transform.localPosition = new Vector3(24.0f, 1.5f, -2.5f);
            BoxCollider tc2 = tLab.AddComponent<BoxCollider>();
            tc2.isTrigger = true;
            tc2.size = new Vector3(12.0f, 3.0f, 10.0f);

            // 5. Objective Points
            GameObject objs = new GameObject("ObjectivePoints");
            objs.transform.SetParent(parent.transform);

            GameObject o1 = new GameObject("Objective_Visit_Lobby");
            o1.transform.SetParent(objs.transform);
            o1.transform.localPosition = new Vector3(22.0f, 0, 8.0f);

            GameObject o2 = new GameObject("Objective_Explore_Courtyard");
            o2.transform.SetParent(objs.transform);
            o2.transform.localPosition = new Vector3(11.0f, 0, 10.0f);

            GameObject o3 = new GameObject("Objective_Check_Motorbike");
            o3.transform.SetParent(objs.transform);
            o3.transform.localPosition = new Vector3(18.0f, 0, 8.0f);
        }

        // ================= 10. LIGHTING =================
        private static void BuildLighting(GameObject parent)
        {
            // Tropical Directional Sun (5800K, soft realistic shadows)
            GameObject sunGO = new GameObject("DirectionalLight_Sun");
            sunGO.transform.SetParent(parent.transform);
            sunGO.transform.localRotation = Quaternion.Euler(46.0f, -38.0f, 0.0f);
            Light sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.96f, 0.88f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;

            // Interior Soft Daylight Panels
            GameObject intLight = new GameObject("Interior_Lobby_Light");
            intLight.transform.SetParent(parent.transform);
            intLight.transform.localPosition = new Vector3(22.0f, 3.2f, 6.0f);
            Light l1 = intLight.AddComponent<Light>();
            l1.type = LightType.Point;
            l1.color = new Color(0.98f, 0.98f, 1.0f);
            l1.range = 14.0f;
            l1.intensity = 1.4f;

            GameObject labLight = new GameObject("Interior_Lab_Light");
            labLight.transform.SetParent(parent.transform);
            labLight.transform.localPosition = new Vector3(24.0f, 3.2f, -2.5f);
            Light l2 = labLight.AddComponent<Light>();
            l2.type = LightType.Point;
            l2.color = new Color(0.96f, 0.98f, 1.0f);
            l2.range = 16.0f;
            l2.intensity = 1.5f;
        }

        // ================= 11. AUDIO =================
        private static void BuildAudio(GameObject parent)
        {
            // Exterior Courtyard Ambience Zone
            GameObject aCourt = new GameObject("Audio_Exterior_Courtyard");
            aCourt.transform.SetParent(parent.transform);
            aCourt.transform.localPosition = new Vector3(12.0f, 1.5f, 8.0f);
            AudioSource src1 = aCourt.AddComponent<AudioSource>();
            src1.spatialBlend = 0.5f;
            src1.volume = 0.35f;
            src1.loop = true;
            src1.playOnAwake = false;

            // Interior Lobby AC Hum Zone
            GameObject aLobby = new GameObject("Audio_Interior_Lobby");
            aLobby.transform.SetParent(parent.transform);
            aLobby.transform.localPosition = new Vector3(22.0f, 1.5f, 6.0f);
            AudioSource src2 = aLobby.AddComponent<AudioSource>();
            src2.spatialBlend = 0.8f;
            src2.volume = 0.25f;
            src2.loop = true;
            src2.playOnAwake = false;

            // Interior Lab Electronics Tone Zone
            GameObject aLab = new GameObject("Audio_Interior_ComputerLab");
            aLab.transform.SetParent(parent.transform);
            aLab.transform.localPosition = new Vector3(24.0f, 1.5f, -2.5f);
            AudioSource src3 = aLab.AddComponent<AudioSource>();
            src3.spatialBlend = 0.8f;
            src3.volume = 0.20f;
            src3.loop = true;
            src3.playOnAwake = false;
        }

        // ================= 12. BACKGROUND URBAN CONTEXT =================
        private static void BuildBackgroundUrban(GameObject parent)
        {
            // Low-poly HCMC residential shophouses around campus perimeter
            Vector3[] urbanPositions = {
                new Vector3(-28.0f, 0, 10.0f),
                new Vector3(-28.0f, 0, 30.0f),
                new Vector3(-28.0f, 0, -10.0f),
                new Vector3(52.0f, 0, 10.0f),
                new Vector3(52.0f, 0, 30.0f),
                new Vector3(52.0f, 0, -10.0f),
                new Vector3(18.0f, 0, 72.0f)
            };

            for (int u = 0; u < urbanPositions.Length; u++)
            {
                GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
                house.name = "Urban_Shophouse_" + u;
                house.transform.SetParent(parent.transform);
                float h = 14.0f + (u % 3) * 3.5f;
                house.transform.localPosition = urbanPositions[u] + new Vector3(0, h * 0.5f, 0);
                house.transform.localScale = new Vector3(14.0f, h, 14.0f);
                house.GetComponent<MeshRenderer>().sharedMaterial = m_UrbanHouse;

                // Rooftop water tank
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "Tank";
                tank.transform.SetParent(house.transform, false);
                tank.transform.localPosition = new Vector3(0, 0.55f, 0);
                tank.transform.localScale = new Vector3(0.25f, 0.15f, 0.25f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = m_WaterTankStainless;
            }
        }

        // ================= 13. PLAYER EXPLORATION SYSTEM =================
        private static void BuildPlayer(GameObject root)
        {
            GameObject playerSystem = new GameObject("PlayerSystem");
            playerSystem.transform.SetParent(root.transform);

            // Instantiate or construct player at courtyard spawn
            GameObject player = new GameObject("Player");
            player.transform.SetParent(playerSystem.transform);
            player.transform.localPosition = new Vector3(11.0f, 0.05f, -5.0f);
            player.transform.localRotation = Quaternion.Euler(0, 35.0f, 0);
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Ignore Raycast") != -1 ? LayerMask.NameToLayer("Ignore Raycast") : 2;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.75f;
            cc.radius = 0.32f;
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 45.0f;
            cc.center = new Vector3(0, 0.88f, 0);

            // Add runtime controller
            var controller = player.AddComponent<IUHPlayerController>();

            // Camera Target anchor
            GameObject camTarget = new GameObject("CameraTarget");
            camTarget.transform.SetParent(player.transform, false);
            camTarget.transform.localPosition = new Vector3(0, 1.55f, 0);

            // Player Model
            GameObject model = new GameObject("Model");
            model.transform.SetParent(player.transform, false);

            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(model.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.05f, 0);
            torso.transform.localScale = new Vector3(0.42f, 0.58f, 0.24f);
            torso.GetComponent<MeshRenderer>().sharedMaterial = m_StudentBluePolo;

            GameObject pack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pack.name = "Backpack";
            pack.transform.SetParent(torso.transform, false);
            pack.transform.localPosition = new Vector3(0, 0.05f, -0.65f);
            pack.transform.localScale = new Vector3(0.85f, 0.85f, 0.55f);
            pack.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(model.transform, false);
            head.transform.localPosition = new Vector3(0, 1.52f, 0);
            head.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            head.GetComponent<MeshRenderer>().sharedMaterial = m_StudentSkin;

            for (int l = -1; l <= 1; l += 2)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg_" + l;
                leg.transform.SetParent(model.transform, false);
                leg.transform.localPosition = new Vector3(l * 0.12f, 0.42f, 0);
                leg.transform.localScale = new Vector3(0.16f, 0.76f, 0.18f);
                leg.GetComponent<MeshRenderer>().sharedMaterial = m_StudentJeans;
            }

            // Ensure Main Camera exists and is set up
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camGO = GameObject.Find("MainCamera");
                if (camGO == null) camGO = new GameObject("MainCamera");
                mainCam = camGO.GetComponent<Camera>();
                if (mainCam == null) mainCam = camGO.AddComponent<Camera>();
                mainCam.tag = "MainCamera";
            }
            mainCam.transform.position = player.transform.position + new Vector3(0, 1.8f, -2.6f);
            mainCam.transform.rotation = Quaternion.Euler(12.0f, 35.0f, 0.0f);
        }
    }
}
