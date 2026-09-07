using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    public static class IUHRightClusterBuilder
    {
        private const string ScenePath = "Assets/IUH_Campus/Scenes/IUH_RightCluster.unity";
        private const string PrefabArchPath = "Assets/IUH_Campus/Prefabs/Architecture/";
        private const string PrefabEnvPath = "Assets/IUH_Campus/Prefabs/Environment/";
        private const string MatPath = "Assets/IUH_Campus/Materials/";

        // Building G Materials
        private static Material m_G_Wall;
        private static Material m_G_WindowGlass;
        private static Material m_G_Railings;
        private static Material m_G_Roof;
        private static Material m_G_RoofEdge;
        private static Material m_G_Sign;

        // Building I Materials
        private static Material m_I_Wall;
        private static Material m_I_Roof;
        private static Material m_I_Glass;
        private static Material m_I_Railings;

        // Building C Materials
        private static Material m_C_Wall;
        private static Material m_C_AccentMint;
        private static Material m_C_GreenGlass;
        private static Material m_C_Roof;

        // Low Podium Materials
        private static Material m_Podium_Roof;
        private static Material m_Podium_Glazing;
        private static Material m_Podium_Turquoise;

        // Common & Ground Surfaces
        private static Material m_ConcreteGround;
        private static Material m_ConcreteCurb;
        private static Material m_AsphaltRoad;
        private static Material m_ParkingLine;
        private static Material m_BgBuilding;
        private static Material m_MatureFoliage;
        private static Material m_FoliageTree;
        private static Material m_DarkMetal;
        private static Material m_LightMetal;

        // Drainage & Utility
        private static Material m_DrainGrate;
        private static Material m_ManholeCover;
        private static Material m_PVCPipe;
        private static Material m_FireCabinet;

        // Facade Variation & MEP
        private static Material m_ACLouvers;
        private static Material m_ACUnit;
        private static Material m_WindowBlinds;
        private static Material m_WindowDark;

        // Signs & Boards
        private static Material m_SignDirectional;
        private static Material m_SignNoticeBoard;

        // Vegetation & Soil
        private static Material m_TreeBark;
        private static Material m_PlanterSoil;

        // Campus Props
        private static Material m_BenchGranite;
        private static Material m_BenchWood;

        // Motorbike & Helmets
        private static Material[] m_MotorbikeMats;
        private static Material[] m_HelmetMats;

        // Student NPCs
        private static Material m_StudentWhiteShirt;
        private static Material m_StudentBluePolo;
        private static Material m_StudentJeans;
        private static Material m_StudentDarkPants;
        private static Material m_StudentSkin;
        private static Material m_StudentHair;

        // Rooftop & Laundry
        private static Material m_WaterTankBlack;
        private static Material m_WaterTankStainless;
        private static Material[] m_LaundryMats;

        [MenuItem("IUH Campus/Build Right Buildings Cluster (G-I-C)")]
        public static void BuildRightClusterScene()
        {
            IUHTextureGenerator.GenerateAllTextures();
            IUHMaterialGenerator.GenerateAllMaterials();
            LoadMaterials();
            EnsureDirectories();

            // 1. Create or open dedicated Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Build the Complete Right Cluster Hierarchy
            GameObject clusterGO = CreateRightClusterHierarchy();

            // 3. Setup Cameras & Lighting
            SetupLightingAndCameras(clusterGO);

            // 4. Save Prefab
            string prefabPath = PrefabArchPath + "IUH_RightCluster.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(clusterGO, prefabPath, InteractionMode.AutomatedAction);

            // 5. Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[IUH] Successfully built IUH Right Buildings Cluster Scene and Prefab: " + ScenePath);
        }

        public static GameObject CreateRightClusterPrefab()
        {
            LoadMaterials();
            EnsureDirectories();
            GameObject clusterGO = CreateRightClusterHierarchy();
            string prefabPath = PrefabArchPath + "IUH_RightCluster.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(clusterGO, prefabPath);
            GameObject.DestroyImmediate(clusterGO);
            return prefab;
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(PrefabArchPath)) Directory.CreateDirectory(PrefabArchPath);
            if (!Directory.Exists(PrefabEnvPath)) Directory.CreateDirectory(PrefabEnvPath);
            if (!Directory.Exists("Assets/IUH_Campus/Scenes/")) Directory.CreateDirectory("Assets/IUH_Campus/Scenes/");
        }

        private static void LoadMaterials()
        {
            m_G_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Wall.mat");
            m_G_WindowGlass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_WindowGlass.mat");
            m_G_Railings = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Railings.mat");
            m_G_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Roof.mat");
            m_G_RoofEdge = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_RoofEdge.mat");
            m_G_Sign = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_Sign.mat");

            m_I_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Wall.mat");
            m_I_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Roof.mat");
            m_I_Glass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Glass.mat");
            m_I_Railings = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_I_Railings.mat");

            m_C_Wall = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_Wall.mat");
            m_C_AccentMint = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_AccentMint.mat");
            m_C_GreenGlass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_GreenGlass.mat");
            m_C_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_C_Roof.mat");

            m_Podium_Roof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Podium_Roof.mat");
            m_Podium_Glazing = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Podium_Glazing.mat");
            m_Podium_Turquoise = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Podium_Turquoise.mat");

            m_ConcreteGround = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Concrete_Ground.mat");
            m_ConcreteCurb = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Concrete_Curb.mat");
            m_AsphaltRoad = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Asphalt_Road.mat");
            m_ParkingLine = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Parking_Line.mat");
            m_BgBuilding = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Background_Building.mat");
            m_MatureFoliage = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_MatureFoliage.mat");
            m_FoliageTree = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Foliage_Tree.mat");
            m_DarkMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_DarkMetal.mat");
            m_LightMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_LightMetal.mat");

            m_DrainGrate = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Drain_Grate.mat");
            m_ManholeCover = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Manhole_Cover.mat");
            m_PVCPipe = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_PVC_Pipe.mat");
            m_FireCabinet = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_FireCabinet.mat");

            m_ACLouvers = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_AC_Louvers.mat");
            m_ACUnit = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_AC_Unit.mat");
            m_WindowBlinds = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Window_Blinds.mat");
            m_WindowDark = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Window_Dark.mat");

            m_SignDirectional = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Sign_Directional.mat");
            m_SignNoticeBoard = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Sign_NoticeBoard.mat");

            m_TreeBark = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Tree_Bark.mat");
            m_PlanterSoil = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Planter_Soil.mat");

            m_BenchGranite = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Bench_Granite.mat");
            m_BenchWood = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Bench_Wood.mat");

            m_MotorbikeMats = new Material[]
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

            m_WaterTankBlack = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_WaterTank_Black.mat");
            m_WaterTankStainless = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_WaterTank_Stainless.mat");

            m_LaundryMats = new Material[]
            {
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Cloth_Red.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Cloth_Blue.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Cloth_Yellow.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Cloth_White.mat")
            };
        }

        // ================= ROOT HIERARCHY BUILDER =================
        public static GameObject CreateRightClusterHierarchy()
        {
            GameObject root = new GameObject("IUH_RightCluster");

            // 1. Environment (Ground, Roads, Curbs, Drainage, Background)
            GameObject goEnv = new GameObject("Environment");
            goEnv.transform.SetParent(root.transform);
            BuildEnvironment(goEnv);

            // 2. Buildings (G, I, C, Podium, Connections, Footings, Facades, MEP)
            GameObject goBuildings = new GameObject("Buildings");
            goBuildings.transform.SetParent(root.transform);

            // BLDG_G: Hero Building (Nha G - KTX Nu, 9 floors)
            BuildBuildingG(goBuildings);

            // BLDG_I: Major Background Building (Nha I - KTX Nam, 13 floors)
            BuildBuildingI(goBuildings);

            // BLDG_C: Secondary Mid-Rise (Nha C, 5 floors)
            BuildBuildingC(goBuildings);

            // LOW_PODIUM_FRONT_G: Podium/Auxiliary in front of G
            BuildLowPodium(goBuildings);

            // Covered Walkways / Connections
            BuildCoveredConnections(goBuildings);

            // 3. Vegetation (Tropical Trees, Palms, Shrubs, Tree Pits)
            GameObject goVeg = new GameObject("Vegetation");
            goVeg.transform.SetParent(root.transform);
            BuildVegetation(goVeg);

            // 4. Props (Motorbikes, Canopies, Signs, Benches, Bins, Lighting)
            GameObject goProps = new GameObject("Props");
            goProps.transform.SetParent(root.transform);
            BuildProps(goProps);

            // 5. Campus Life / Student NPCs
            GameObject goLife = new GameObject("CampusLife");
            goLife.transform.SetParent(root.transform);
            BuildCampusLife(goLife);

            // 6. Lighting
            GameObject goLight = new GameObject("Lighting");
            goLight.transform.SetParent(root.transform);
            BuildLighting(goLight);

            return root;
        }

        // ================= 1. ENVIRONMENT (GROUND, ROADS, CURBS, DRAINAGE) =================
        private static void BuildEnvironment(GameObject parent)
        {
            // Base Substrate Concrete Ground (Sân khuôn viên tổng thể)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground_ConcretePlaza";
            ground.transform.SetParent(parent.transform);
            ground.transform.localPosition = new Vector3(18.0f, -0.1f, 16.0f);
            ground.transform.localScale = new Vector3(72.0f, 0.2f, 82.0f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            // InternalRoads (Đường nội bộ & Lòng sân trong chữ U)
            GameObject roads = new GameObject("InternalRoads");
            roads.transform.SetParent(parent.transform);

            // Đường trước mặt cụm chữ U (Front Lane)
            GameObject roadFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadFront.name = "Road_FrontLane";
            roadFront.transform.SetParent(roads.transform);
            roadFront.transform.localPosition = new Vector3(18.0f, 0.02f, -14.0f);
            roadFront.transform.localScale = new Vector3(66.0f, 0.04f, 6.5f);
            roadFront.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;

            // Vạch qua đường cho người đi bộ (Pedestrian Zebra Crossing)
            GameObject zebra = new GameObject("PedestrianZebraCrossing");
            zebra.transform.SetParent(roads.transform);
            for (int zc = 0; zc < 6; zc++)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = "ZebraBar_" + zc;
                bar.transform.SetParent(zebra.transform);
                bar.transform.localPosition = new Vector3(11.0f + zc * 1.0f, 0.045f, -14.0f);
                bar.transform.localScale = new Vector3(0.55f, 0.01f, 4.5f);
                bar.GetComponent<MeshRenderer>().sharedMaterial = m_ParkingLine;
            }

            // Lòng sân trong chữ U trải nhựa asphalt cho bãi đỗ xe máy
            GameObject roadCourt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadCourt.name = "Road_CourtyardAsphalt";
            roadCourt.transform.SetParent(roads.transform);
            roadCourt.transform.localPosition = new Vector3(18.0f, 0.02f, 14.0f);
            roadCourt.transform.localScale = new Vector3(18.0f, 0.04f, 32.0f);
            roadCourt.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;

            // Concrete Sidewalks & Curbs (Bó vỉa bê tông phân cách giao thông)
            GameObject curbs = new GameObject("CurbsAndSidewalks");
            curbs.transform.SetParent(parent.transform);

            // Bó vỉa mép bắc đường mặt tiền (Z = -10.75m)
            GameObject curbFrontNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curbFrontNorth.name = "Curb_Front_North";
            curbFrontNorth.transform.SetParent(curbs.transform);
            curbFrontNorth.transform.localPosition = new Vector3(18.0f, 0.08f, -10.75f);
            curbFrontNorth.transform.localScale = new Vector3(66.0f, 0.16f, 0.22f);
            curbFrontNorth.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // Bó vỉa mép nam đường mặt tiền (Z = -17.25m)
            GameObject curbFrontSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curbFrontSouth.name = "Curb_Front_South";
            curbFrontSouth.transform.SetParent(curbs.transform);
            curbFrontSouth.transform.localPosition = new Vector3(18.0f, 0.08f, -17.25f);
            curbFrontSouth.transform.localScale = new Vector3(66.0f, 0.16f, 0.22f);
            curbFrontSouth.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // Bó vỉa bo viền khu sân trong chữ U
            GameObject curbCourtW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curbCourtW.name = "Curb_Courtyard_West";
            curbCourtW.transform.SetParent(curbs.transform);
            curbCourtW.transform.localPosition = new Vector3(8.9f, 0.08f, 14.0f);
            curbCourtW.transform.localScale = new Vector3(0.22f, 0.16f, 32.0f);
            curbCourtW.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            GameObject curbCourtE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curbCourtE.name = "Curb_Courtyard_East";
            curbCourtE.transform.SetParent(curbs.transform);
            curbCourtE.transform.localPosition = new Vector3(27.1f, 0.08f, 14.0f);
            curbCourtE.transform.localScale = new Vector3(0.22f, 0.16f, 32.0f);
            curbCourtE.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // Drainage System (Hệ thống rãnh thoát nước có nắp gang & hố ga)
            GameObject drainage = new GameObject("DrainageSystem");
            drainage.transform.SetParent(parent.transform);

            // Rãnh thoát nước dọc bó vỉa đường mặt tiền
            for (int dg = 0; dg < 8; dg++)
            {
                float dgX = -10.0f + dg * 7.5f;
                GameObject grate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grate.name = "DrainGrate_Road_" + dg;
                grate.transform.SetParent(drainage.transform);
                grate.transform.localPosition = new Vector3(dgX, 0.042f, -11.1f);
                grate.transform.localScale = new Vector3(1.4f, 0.01f, 0.45f);
                grate.GetComponent<MeshRenderer>().sharedMaterial = m_DrainGrate;
            }

            // Nắp cống tròn (Manholes)
            Vector3[] manholePos = {
                new Vector3(3.0f, 0.045f, -14.0f),
                new Vector3(22.0f, 0.045f, -14.0f),
                new Vector3(36.0f, 0.045f, -14.0f),
                new Vector3(18.0f, 0.045f, 2.0f),
                new Vector3(18.0f, 0.045f, 28.0f)
            };
            for (int mh = 0; mh < manholePos.Length; mh++)
            {
                GameObject manhole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                manhole.name = "Manhole_Cover_" + mh;
                manhole.transform.SetParent(drainage.transform);
                manhole.transform.localPosition = manholePos[mh];
                manhole.transform.localScale = new Vector3(0.85f, 0.015f, 0.85f);
                manhole.GetComponent<MeshRenderer>().sharedMaterial = m_ManholeCover;
            }

            // ParkingArea (Vạch sơn vàng phân ô bãi đỗ xe máy)
            GameObject parkingArea = new GameObject("ParkingArea");
            parkingArea.transform.SetParent(parent.transform);

            for (int r = 0; r < 4; r++)
            {
                float z = 3.5f + r * 3.6f;
                GameObject pLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pLine.name = "ParkingLine_Row_" + r;
                pLine.transform.SetParent(parkingArea.transform);
                pLine.transform.localPosition = new Vector3(18.0f, 0.045f, z);
                pLine.transform.localScale = new Vector3(16.0f, 0.01f, 0.14f);
                pLine.GetComponent<MeshRenderer>().sharedMaterial = m_ParkingLine;
            }

            // BackgroundCity (Nhà phố đô thị TP.HCM bao bọc cụm chữ U)
            GameObject bgCity = new GameObject("BackgroundCity");
            bgCity.transform.SetParent(parent.transform);

            // Phố phụ cận phía Đông (ngoài cánh phải Nhà G)
            float[] cityHeights = { 9.5f, 14.5f, 8.5f, 12.0f, 16.5f, 10.5f, 13.5f };
            for (int i = 0; i < 7; i++)
            {
                float z = -10.0f + i * 9.5f;
                float h = cityHeights[i];
                GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
                house.name = "UrbanBlock_East_" + i;
                house.transform.SetParent(bgCity.transform);
                house.transform.localPosition = new Vector3(46.0f, h * 0.5f, z);
                house.transform.localScale = new Vector3(12.0f, h, 8.5f);
                house.GetComponent<MeshRenderer>().sharedMaterial = m_BgBuilding;

                // Mái tôn & Bồn nước mái nhà dân
                GameObject hRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hRoof.name = "Roof";
                hRoof.transform.SetParent(house.transform);
                hRoof.transform.localPosition = new Vector3(0, 0.53f, 0);
                hRoof.transform.localScale = new Vector3(1.04f, 0.12f, 1.04f);
                hRoof.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? m_I_Roof : m_Podium_Roof;

                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "RoofWaterTank";
                tank.transform.SetParent(house.transform);
                tank.transform.localPosition = new Vector3(0.2f, 0.62f, 0.2f);
                tank.transform.localScale = new Vector3(0.18f, 0.15f, 0.18f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = m_WaterTankStainless;
            }

            // Phố phụ cận phía Bắc (phía sau Nhà I)
            for (int j = 0; j < 6; j++)
            {
                float x = -2.0f + j * 9.0f;
                float h = 10.5f + (j % 3) * 3.5f;
                GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
                house.name = "UrbanBlock_North_" + j;
                house.transform.SetParent(bgCity.transform);
                house.transform.localPosition = new Vector3(x, h * 0.5f, 52.0f);
                house.transform.localScale = new Vector3(8.5f, h, 10.0f);
                house.GetComponent<MeshRenderer>().sharedMaterial = m_BgBuilding;

                GameObject hRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hRoof.name = "Roof";
                hRoof.transform.SetParent(house.transform);
                hRoof.transform.localPosition = new Vector3(0, 0.53f, 0);
                hRoof.transform.localScale = new Vector3(1.04f, 0.12f, 1.04f);
                hRoof.GetComponent<MeshRenderer>().sharedMaterial = m_I_Roof;
            }
        }

        // ================= 2. BUILDINGS (G, I, C, PODIUM, FOOTINGS & MEP) =================

        // ── BLDG_G: KTX Nữ (9 Floors, Hero Building) ──
        private static void BuildBuildingG(GameObject parent)
        {
            GameObject bldgG = new GameObject("Building_G");
            bldgG.transform.SetParent(parent.transform);
            bldgG.transform.localPosition = new Vector3(30.0f, 0.0f, 12.0f);
            bldgG.transform.localRotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);

            float width = 49.0f;
            float depth = 16.0f;
            float wallH = 31.0f;
            int floorCount = 9;
            float floorH = wallH / floorCount; // ~3.44m

            // 1. G_Structure: Core body
            GameObject gStruct = new GameObject("G_Structure");
            gStruct.transform.SetParent(bldgG.transform, false);

            GameObject coreBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coreBox.name = "Core_Slab";
            coreBox.transform.SetParent(gStruct.transform, false);
            coreBox.transform.localPosition = new Vector3(0, wallH * 0.5f, 0);
            coreBox.transform.localScale = new Vector3(width, wallH, depth);
            coreBox.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Solid Base Skirting / Plinth (0.4m foot trim to ground building firmly)
            GameObject baseSkirting = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseSkirting.name = "Perimeter_Base_Skirting";
            baseSkirting.transform.SetParent(gStruct.transform, false);
            baseSkirting.transform.localPosition = new Vector3(0, 0.22f, 0);
            baseSkirting.transform.localScale = new Vector3(width + 0.35f, 0.44f, depth + 0.35f);
            baseSkirting.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // Ground service base band
            GameObject basePlinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basePlinth.name = "Ground_Service_Band";
            basePlinth.transform.SetParent(gStruct.transform, false);
            basePlinth.transform.localPosition = new Vector3(0, 1.6f, 0);
            basePlinth.transform.localScale = new Vector3(width + 0.2f, 3.2f, depth + 0.2f);
            basePlinth.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // 2. G_FacadeModules & Balconies
            GameObject gFacades = new GameObject("G_FacadeModules");
            gFacades.transform.SetParent(bldgG.transform, false);

            GameObject gBalconies = new GameObject("G_Balconies");
            gBalconies.transform.SetParent(bldgG.transform, false);

            int bayCount = 9;
            float bayW = (width - 4.0f) / bayCount; // ~4.77m
            float startX = -width * 0.5f + 2.0f + bayW * 0.5f;
            float frontZ = -depth * 0.5f;
            System.Random rnd = new System.Random(42);

            for (int f = 1; f < floorCount; f++) // Floors 2 to 9
            {
                float floorY = f * floorH;

                // Horizontal lintel slab edge between floors
                GameObject slabEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slabEdge.name = "SlabEdge_F" + f;
                slabEdge.transform.SetParent(gFacades.transform, false);
                slabEdge.transform.localPosition = new Vector3(0, floorY, frontZ - 0.25f);
                slabEdge.transform.localScale = new Vector3(width + 0.2f, 0.45f, 0.6f);
                slabEdge.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

                for (int b = 0; b < bayCount; b++)
                {
                    float bayX = startX + b * bayW;

                    // Recessed Balcony Alcove Box
                    GameObject balconyAlcove = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    balconyAlcove.name = $"Balcony_F{f}_B{b}";
                    balconyAlcove.transform.SetParent(gBalconies.transform, false);
                    balconyAlcove.transform.localPosition = new Vector3(bayX, floorY + floorH * 0.48f, frontZ + 0.45f);
                    balconyAlcove.transform.localScale = new Vector3(bayW - 0.45f, floorH - 0.55f, 1.2f);
                    balconyAlcove.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

                    // Window glass with controlled variation (blinds, dark interior, normal)
                    GameObject glassPane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    glassPane.name = $"Glass_F{f}_B{b}";
                    glassPane.transform.SetParent(balconyAlcove.transform, false);
                    glassPane.transform.localPosition = new Vector3(0, 0.05f, 0.45f);
                    glassPane.transform.localScale = new Vector3(0.85f, 0.72f, 0.1f);

                    double winVar = rnd.NextDouble();
                    if (winVar < 0.15)
                        glassPane.GetComponent<MeshRenderer>().sharedMaterial = m_WindowBlinds;
                    else if (winVar < 0.25)
                        glassPane.GetComponent<MeshRenderer>().sharedMaterial = m_WindowDark;
                    else
                        glassPane.GetComponent<MeshRenderer>().sharedMaterial = m_G_WindowGlass;

                    // Balcony metal railing
                    GameObject railing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    railing.name = $"Railing_F{f}_B{b}";
                    railing.transform.SetParent(gBalconies.transform, false);
                    railing.transform.localPosition = new Vector3(bayX, floorY + 0.55f, frontZ - 0.15f);
                    railing.transform.localScale = new Vector3(bayW - 0.48f, 1.1f, 0.08f);
                    railing.GetComponent<MeshRenderer>().sharedMaterial = m_G_Railings;

                    GameObject railTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    railTop.name = "TopBar";
                    railTop.transform.SetParent(railing.transform, false);
                    railTop.transform.localPosition = new Vector3(0, 0.5f, 0);
                    railTop.transform.localScale = new Vector3(1.02f, 0.08f, 1.4f);
                    railTop.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

                    // Occupancy Detail: Clothes & personal laundry on ~20% of residential balconies
                    if (rnd.NextDouble() < 0.22)
                    {
                        GameObject clothes = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        clothes.name = "LaundryCloth";
                        clothes.transform.SetParent(balconyAlcove.transform, false);
                        float clothX = ((float)rnd.NextDouble() - 0.5f) * 0.5f;
                        clothes.transform.localPosition = new Vector3(clothX, -0.1f, -0.1f);
                        clothes.transform.localScale = new Vector3(0.35f, 0.55f, 0.04f);
                        clothes.GetComponent<MeshRenderer>().sharedMaterial = m_LaundryMats[rnd.Next(m_LaundryMats.Length)];
                    }
                }
            }

            // Vertical Structural Columns Grid
            for (int b = 0; b <= bayCount; b++)
            {
                float colX = -width * 0.5f + 2.0f + b * bayW - bayW * 0.5f;
                if (b == 0) colX = -width * 0.5f + 0.35f;
                if (b == bayCount) colX = width * 0.5f - 0.35f;

                GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cube);
                col.name = "Column_" + b;
                col.transform.SetParent(gFacades.transform, false);
                col.transform.localPosition = new Vector3(colX, wallH * 0.5f, frontZ - 0.28f);
                col.transform.localScale = new Vector3(0.55f, wallH, 0.65f);
                col.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;
            }

            // MEP System: AC outdoor units on brackets + Drain lines
            GameObject gMEP = new GameObject("G_MEP_Systems");
            gMEP.transform.SetParent(bldgG.transform, false);

            for (int f = 2; f <= 8; f++)
            {
                float y = f * floorH - 0.4f;
                for (int b = 1; b < bayCount; b += 2)
                {
                    float x = startX + b * bayW + 1.2f;

                    GameObject acUnit = new GameObject($"AC_Unit_F{f}_B{b}");
                    acUnit.transform.SetParent(gMEP.transform, false);
                    acUnit.transform.localPosition = new Vector3(x, y, frontZ - 0.35f);

                    // Body
                    GameObject acBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    acBody.name = "Casing";
                    acBody.transform.SetParent(acUnit.transform, false);
                    acBody.transform.localScale = new Vector3(0.85f, 0.65f, 0.45f);
                    acBody.GetComponent<MeshRenderer>().sharedMaterial = m_ACLouvers;

                    // Metal Bracket
                    GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bracket.name = "Bracket";
                    bracket.transform.SetParent(acUnit.transform, false);
                    bracket.transform.localPosition = new Vector3(0, -0.38f, 0.12f);
                    bracket.transform.localScale = new Vector3(0.75f, 0.08f, 0.35f);
                    bracket.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    // Condensate Drain Tube
                    GameObject drainTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    drainTube.name = "DrainTube";
                    drainTube.transform.SetParent(acUnit.transform, false);
                    drainTube.transform.localPosition = new Vector3(0.35f, -0.7f, 0.12f);
                    drainTube.transform.localScale = new Vector3(0.04f, 0.65f, 0.04f);
                    drainTube.GetComponent<MeshRenderer>().sharedMaterial = m_PVCPipe;
                }
            }

            // Vertical Rainwater Downspouts (Ống xả nước mưa góc tòa nhà)
            float[] downspoutXs = { -width * 0.5f + 0.45f, width * 0.5f - 0.45f };
            for (int ds = 0; ds < downspoutXs.Length; ds++)
            {
                GameObject downspout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                downspout.name = "Rainwater_Downspout_" + ds;
                downspout.transform.SetParent(gMEP.transform, false);
                downspout.transform.localPosition = new Vector3(downspoutXs[ds], wallH * 0.5f, frontZ - 0.28f);
                downspout.transform.localScale = new Vector3(0.14f, wallH * 0.5f, 0.14f);
                downspout.GetComponent<MeshRenderer>().sharedMaterial = m_PVCPipe;

                // Ground Drain Grate at downspout base
                GameObject grate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grate.name = "DownspoutGrate_" + ds;
                grate.transform.SetParent(gMEP.transform, false);
                grate.transform.localPosition = new Vector3(downspoutXs[ds], 0.04f, frontZ - 0.45f);
                grate.transform.localScale = new Vector3(0.65f, 0.02f, 0.65f);
                grate.GetComponent<MeshRenderer>().sharedMaterial = m_DrainGrate;
            }

            // Exterior Fire Hose Cabinet (Tủ PCCC)
            GameObject fireCab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fireCab.name = "FireCabinet_Exterior";
            fireCab.transform.SetParent(gMEP.transform, false);
            fireCab.transform.localPosition = new Vector3(-width * 0.5f + 3.0f, 1.4f, frontZ - 0.25f);
            fireCab.transform.localScale = new Vector3(0.85f, 1.15f, 0.35f);
            fireCab.GetComponent<MeshRenderer>().sharedMaterial = m_FireCabinet;

            // 3. G_Roof: Shallow pitched roof + Parapet + Rooftop objects
            GameObject gRoof = new GameObject("G_Roof");
            gRoof.transform.SetParent(bldgG.transform, false);

            GameObject parapet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            parapet.name = "Parapet_Crown";
            parapet.transform.SetParent(gRoof.transform, false);
            parapet.transform.localPosition = new Vector3(0, wallH + 0.7f, 0);
            parapet.transform.localScale = new Vector3(width + 0.6f, 1.4f, depth + 0.6f);
            parapet.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject mainRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainRoof.name = "Shallow_Pitched_Roof";
            mainRoof.transform.SetParent(gRoof.transform, false);
            mainRoof.transform.localPosition = new Vector3(0, wallH + 1.8f, 0);
            mainRoof.transform.localScale = new Vector3(width + 1.2f, 1.2f, depth + 1.0f);
            mainRoof.GetComponent<MeshRenderer>().sharedMaterial = m_G_Roof;

            // Roof orange-red edge trims
            GameObject roofEdgeFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofEdgeFront.name = "Roof_Edge_Trim_Front";
            roofEdgeFront.transform.SetParent(gRoof.transform, false);
            roofEdgeFront.transform.localPosition = new Vector3(0, wallH + 2.3f, -depth * 0.5f - 0.55f);
            roofEdgeFront.transform.localScale = new Vector3(width + 1.6f, 0.35f, 0.45f);
            roofEdgeFront.GetComponent<MeshRenderer>().sharedMaterial = m_G_RoofEdge;

            GameObject roofEdgeRear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofEdgeRear.name = "Roof_Edge_Trim_Rear";
            roofEdgeRear.transform.SetParent(gRoof.transform, false);
            roofEdgeRear.transform.localPosition = new Vector3(0, wallH + 2.3f, depth * 0.5f + 0.55f);
            roofEdgeRear.transform.localScale = new Vector3(width + 1.6f, 0.35f, 0.45f);
            roofEdgeRear.GetComponent<MeshRenderer>().sharedMaterial = m_G_RoofEdge;

            // Elevator Penthouse
            GameObject penthouse = GameObject.CreatePrimitive(PrimitiveType.Cube);
            penthouse.name = "Stairwell_Elevator_Penthouse";
            penthouse.transform.SetParent(gRoof.transform, false);
            penthouse.transform.localPosition = new Vector3(-8.0f, wallH + 3.0f, 0);
            penthouse.transform.localScale = new Vector3(8.0f, 3.2f, 7.0f);
            penthouse.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Water Tanks (Black & Stainless Steel)
            for (int t = 0; t < 3; t++)
            {
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "WaterTank_Black_" + t;
                tank.transform.SetParent(gRoof.transform, false);
                tank.transform.localPosition = new Vector3(6.0f + t * 2.8f, wallH + 3.2f, 3.0f);
                tank.transform.localScale = new Vector3(1.8f, 1.4f, 1.8f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = m_WaterTankBlack;
            }

            for (int s = 0; s < 2; s++)
            {
                GameObject sTank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                sTank.name = "WaterTank_Stainless_" + s;
                sTank.transform.SetParent(gRoof.transform, false);
                sTank.transform.localPosition = new Vector3(7.0f + s * 3.0f, wallH + 3.2f, -3.0f);
                sTank.transform.localScale = new Vector3(1.6f, 1.3f, 1.6f);
                sTank.GetComponent<MeshRenderer>().sharedMaterial = m_WaterTankStainless;
            }

            // Mechanical Service Box
            GameObject mechBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mechBox.name = "Mechanical_Service_Box";
            mechBox.transform.SetParent(gRoof.transform, false);
            mechBox.transform.localPosition = new Vector3(-16.0f, wallH + 2.8f, 1.0f);
            mechBox.transform.localScale = new Vector3(5.5f, 2.0f, 4.0f);
            mechBox.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

            // 4. G_Signage: Blue Billboard "TRUONG DAI HOC CONG NGHIEP TP. HO CHI MINH"
            GameObject gSignage = new GameObject("G_Signage");
            gSignage.transform.SetParent(bldgG.transform, false);

            GameObject billboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            billboard.name = "IUH_Billboard_Sign";
            billboard.transform.SetParent(gSignage.transform, false);
            billboard.transform.localPosition = new Vector3(0, wallH + 0.3f, frontZ - 0.45f);
            billboard.transform.localScale = new Vector3(32.0f, 2.4f, 0.25f);
            billboard.GetComponent<MeshRenderer>().sharedMaterial = m_G_Sign;

            GameObject signBorder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBorder.name = "Billboard_Frame";
            signBorder.transform.SetParent(billboard.transform, false);
            signBorder.transform.localPosition = new Vector3(0, 0, -0.05f);
            signBorder.transform.localScale = new Vector3(1.02f, 1.08f, 0.4f);
            signBorder.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

            // Compound Collider
            BoxCollider colG = bldgG.AddComponent<BoxCollider>();
            colG.center = new Vector3(0, wallH * 0.5f, 0);
            colG.size = new Vector3(width, wallH, depth);
        }

        // ── BLDG_I: KTX Nam (13 Floors, Major Background Building) ──
        private static void BuildBuildingI(GameObject parent)
        {
            GameObject bldgI = new GameObject("Building_I");
            bldgI.transform.SetParent(parent.transform);
            bldgI.transform.localPosition = new Vector3(17.0f, 0.0f, 38.0f);
            bldgI.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            float width = 38.0f;
            float depth = 16.0f;
            float wallH = 42.0f;
            int floorCount = 13;
            float floorH = wallH / floorCount; // ~3.23m

            // 1. I_Structure
            GameObject iStruct = new GameObject("I_Structure");
            iStruct.transform.SetParent(bldgI.transform, false);

            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "Core_Slab";
            slab.transform.SetParent(iStruct.transform, false);
            slab.transform.localPosition = new Vector3(0, wallH * 0.5f, 0);
            slab.transform.localScale = new Vector3(width, wallH, depth);
            slab.GetComponent<MeshRenderer>().sharedMaterial = m_I_Wall;

            // Perimeter Base Skirting
            GameObject baseSkirting = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseSkirting.name = "Perimeter_Base_Skirting";
            baseSkirting.transform.SetParent(iStruct.transform, false);
            baseSkirting.transform.localPosition = new Vector3(0, 0.22f, 0);
            baseSkirting.transform.localScale = new Vector3(width + 0.35f, 0.44f, depth + 0.35f);
            baseSkirting.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // 2. I_FacadeModules: Windows with blinds & AC variations
            GameObject iFacades = new GameObject("I_FacadeModules");
            iFacades.transform.SetParent(bldgI.transform, false);

            int dormBays = 9;
            float bayW = (width - 4.0f) / dormBays;
            float startX = -width * 0.5f + 2.0f + bayW * 0.5f;
            System.Random rnd = new System.Random(77);

            for (int f = 1; f <= floorCount; f++)
            {
                float y = (f - 0.5f) * floorH;

                GameObject belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                belt.name = "FloorBelt_F" + f;
                belt.transform.SetParent(iFacades.transform, false);
                belt.transform.localPosition = new Vector3(0, f * floorH, -depth * 0.5f - 0.1f);
                belt.transform.localScale = new Vector3(width + 0.2f, 0.35f, 0.4f);
                belt.GetComponent<MeshRenderer>().sharedMaterial = m_I_Wall;

                for (int b = 0; b < dormBays; b++)
                {
                    float x = startX + b * bayW;

                    GameObject winFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    winFront.name = $"DormWin_F{f}_B{b}";
                    winFront.transform.SetParent(iFacades.transform, false);
                    winFront.transform.localPosition = new Vector3(x, y, -depth * 0.5f - 0.08f);
                    winFront.transform.localScale = new Vector3(bayW * 0.62f, floorH * 0.58f, 0.18f);

                    double winVar = rnd.NextDouble();
                    if (winVar < 0.14)
                        winFront.GetComponent<MeshRenderer>().sharedMaterial = m_WindowBlinds;
                    else if (winVar < 0.24)
                        winFront.GetComponent<MeshRenderer>().sharedMaterial = m_WindowDark;
                    else
                        winFront.GetComponent<MeshRenderer>().sharedMaterial = m_I_Glass;

                    GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rail.name = $"Railing_F{f}_B{b}";
                    rail.transform.SetParent(iFacades.transform, false);
                    rail.transform.localPosition = new Vector3(x, y - floorH * 0.15f, -depth * 0.5f - 0.16f);
                    rail.transform.localScale = new Vector3(bayW * 0.65f, 0.45f, 0.06f);
                    rail.GetComponent<MeshRenderer>().sharedMaterial = m_I_Railings;
                }
            }

            // MEP & AC units on Building I
            GameObject iMEP = new GameObject("I_MEP_Systems");
            iMEP.transform.SetParent(bldgI.transform, false);

            for (int f = 2; f <= 12; f += 2)
            {
                float y = (f - 0.5f) * floorH - 0.6f;
                for (int b = 1; b < dormBays; b += 3)
                {
                    float x = startX + b * bayW;
                    GameObject ac = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ac.name = $"AC_I_F{f}_B{b}";
                    ac.transform.SetParent(iMEP.transform, false);
                    ac.transform.localPosition = new Vector3(x, y, -depth * 0.5f - 0.28f);
                    ac.transform.localScale = new Vector3(0.8f, 0.6f, 0.4f);
                    ac.GetComponent<MeshRenderer>().sharedMaterial = m_ACLouvers;
                }
            }

            // Rainwater downspouts on Building I corners
            for (int ds = 0; ds < 2; ds++)
            {
                float dx = (ds == 0) ? -width * 0.5f + 0.35f : width * 0.5f - 0.35f;
                GameObject downspout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                downspout.name = "Downspout_I_" + ds;
                downspout.transform.SetParent(iMEP.transform, false);
                downspout.transform.localPosition = new Vector3(dx, wallH * 0.5f, -depth * 0.5f - 0.22f);
                downspout.transform.localScale = new Vector3(0.14f, wallH * 0.5f, 0.14f);
                downspout.GetComponent<MeshRenderer>().sharedMaterial = m_PVCPipe;
            }

            // 3. I_Roof: Long shallow pitched roof with dominant orange-red corrugated metal
            GameObject iRoof = new GameObject("I_Roof");
            iRoof.transform.SetParent(bldgI.transform, false);

            GameObject pitchedRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pitchedRoof.name = "Pitched_OrangeRed_Roof";
            pitchedRoof.transform.SetParent(iRoof.transform, false);
            pitchedRoof.transform.localPosition = new Vector3(0, wallH + 1.5f, 0);
            pitchedRoof.transform.localScale = new Vector3(width + 1.6f, 3.0f, depth + 1.2f);
            pitchedRoof.GetComponent<MeshRenderer>().sharedMaterial = m_I_Roof;

            // Rooftop water tank farm
            for (int t = 0; t < 4; t++)
            {
                GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "WaterTank_I_" + t;
                tank.transform.SetParent(iRoof.transform, false);
                tank.transform.localPosition = new Vector3(-12.0f + t * 3.5f, wallH + 3.8f, 0);
                tank.transform.localScale = new Vector3(2.0f, 1.6f, 2.0f);
                tank.GetComponent<MeshRenderer>().sharedMaterial = m_WaterTankBlack;
            }

            BoxCollider colI = bldgI.AddComponent<BoxCollider>();
            colI.center = new Vector3(0, wallH * 0.5f, 0);
            colI.size = new Vector3(width, wallH, depth);
        }

        // ── BLDG_C: Nhà C (5 Floors, Mid-Rise Institutional) ──
        private static void BuildBuildingC(GameObject parent)
        {
            GameObject bldgC = new GameObject("Building_C");
            bldgC.transform.SetParent(parent.transform);
            bldgC.transform.localPosition = new Vector3(6.0f, 0.0f, 18.0f);
            bldgC.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

            float width = 28.0f;
            float depth = 16.0f;
            float height = 18.0f;
            int floors = 5;
            float floorH = height / floors; // 3.6m

            // 1. C_Structure
            GameObject cStruct = new GameObject("C_Structure");
            cStruct.transform.SetParent(bldgC.transform, false);

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Core_Structure";
            core.transform.SetParent(cStruct.transform, false);
            core.transform.localPosition = new Vector3(0, height * 0.5f, 0);
            core.transform.localScale = new Vector3(width, height, depth);
            core.GetComponent<MeshRenderer>().sharedMaterial = m_C_Wall;

            // Perimeter Base Skirting
            GameObject baseSkirting = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseSkirting.name = "Perimeter_Base_Skirting";
            baseSkirting.transform.SetParent(cStruct.transform, false);
            baseSkirting.transform.localPosition = new Vector3(0, 0.22f, 0);
            baseSkirting.transform.localScale = new Vector3(width + 0.35f, 0.44f, depth + 0.35f);
            baseSkirting.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // Mint Green Horizontal Accent Spandrel Bands
            for (int f = 1; f < floors; f++)
            {
                float y = f * floorH;
                GameObject mintBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mintBand.name = "MintBand_F" + f;
                mintBand.transform.SetParent(cStruct.transform, false);
                mintBand.transform.localPosition = new Vector3(0, y, -depth * 0.5f - 0.12f);
                mintBand.transform.localScale = new Vector3(width + 0.2f, 0.8f, 0.35f);
                mintBand.GetComponent<MeshRenderer>().sharedMaterial = m_C_AccentMint;
            }

            // 2. C_Glass: Vertical stairwell glass strip & horizontal classroom ribbons
            GameObject cGlass = new GameObject("C_Glass");
            cGlass.transform.SetParent(bldgC.transform, false);

            GameObject stairGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairGlass.name = "Vertical_Stairwell_Glass_Strip";
            stairGlass.transform.SetParent(cGlass.transform, false);
            stairGlass.transform.localPosition = new Vector3(width * 0.5f - 2.5f, height * 0.5f, -depth * 0.5f - 0.2f);
            stairGlass.transform.localScale = new Vector3(4.0f, height - 1.0f, 0.3f);
            stairGlass.GetComponent<MeshRenderer>().sharedMaterial = m_C_GreenGlass;

            for (int f = 1; f <= floors; f++)
            {
                float y = (f - 0.5f) * floorH;
                GameObject winStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winStrip.name = "WinStrip_F" + f;
                winStrip.transform.SetParent(cGlass.transform, false);
                winStrip.transform.localPosition = new Vector3(-3.0f, y, -depth * 0.5f - 0.08f);
                winStrip.transform.localScale = new Vector3(width - 11.0f, 1.8f, 0.18f);
                winStrip.GetComponent<MeshRenderer>().sharedMaterial = m_C_GreenGlass;
            }

            // Downspouts on Building C
            for (int ds = 0; ds < 2; ds++)
            {
                float dx = (ds == 0) ? -width * 0.5f + 0.35f : width * 0.5f - 0.35f;
                GameObject downspout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                downspout.name = "Downspout_C_" + ds;
                downspout.transform.SetParent(bldgC.transform, false);
                downspout.transform.localPosition = new Vector3(dx, height * 0.5f, -depth * 0.5f - 0.22f);
                downspout.transform.localScale = new Vector3(0.12f, height * 0.5f, 0.12f);
                downspout.GetComponent<MeshRenderer>().sharedMaterial = m_PVCPipe;
            }

            // 3. C_Roof: Flat/low-slope green roof
            GameObject cRoof = new GameObject("C_Roof");
            cRoof.transform.SetParent(bldgC.transform, false);

            GameObject roofCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofCap.name = "Flat_Green_Roof";
            roofCap.transform.SetParent(cRoof.transform, false);
            roofCap.transform.localPosition = new Vector3(0, height + 0.35f, 0);
            roofCap.transform.localScale = new Vector3(width + 0.6f, 0.7f, depth + 0.6f);
            roofCap.GetComponent<MeshRenderer>().sharedMaterial = m_C_Roof;

            GameObject parapet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            parapet.name = "Parapet";
            parapet.transform.SetParent(cRoof.transform, false);
            parapet.transform.localPosition = new Vector3(0, height + 1.1f, 0);
            parapet.transform.localScale = new Vector3(width + 0.8f, 0.9f, depth + 0.8f);
            parapet.GetComponent<MeshRenderer>().sharedMaterial = m_C_Wall;

            BoxCollider colC = bldgC.AddComponent<BoxCollider>();
            colC.center = new Vector3(0, height * 0.5f, 0);
            colC.size = new Vector3(width, height, depth);
        }

        // ── LOW_PODIUM_FRONT_G: Khối Đế / Phụ Trợ Trước Cụm G ──
        private static void BuildLowPodium(GameObject parent)
        {
            GameObject podium = new GameObject("Low_Podium");
            podium.transform.SetParent(parent.transform);
            podium.transform.localPosition = new Vector3(18.0f, 0.0f, -6.0f);
            podium.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            float width = 26.0f;
            float depth = 12.0f;
            float height = 9.8f;

            // 1. Podium_Structure
            GameObject podStruct = new GameObject("Podium_Structure");
            podStruct.transform.SetParent(podium.transform, false);

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Core_Podium_Body";
            core.transform.SetParent(podStruct.transform, false);
            core.transform.localPosition = new Vector3(0, height * 0.5f, 0);
            core.transform.localScale = new Vector3(width, height, depth);
            core.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            // Base Skirting
            GameObject baseSkirting = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseSkirting.name = "Perimeter_Base_Skirting";
            baseSkirting.transform.SetParent(podStruct.transform, false);
            baseSkirting.transform.localPosition = new Vector3(0, 0.22f, 0);
            baseSkirting.transform.localScale = new Vector3(width + 0.35f, 0.44f, depth + 0.35f);
            baseSkirting.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

            // Turquoise Corner Cladding Accents
            GameObject cornerLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cornerLeft.name = "Turquoise_Corner_Left";
            cornerLeft.transform.SetParent(podStruct.transform, false);
            cornerLeft.transform.localPosition = new Vector3(-width * 0.5f + 1.5f, height * 0.5f, -depth * 0.5f - 0.15f);
            cornerLeft.transform.localScale = new Vector3(3.2f, height + 0.2f, 0.5f);
            cornerLeft.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Turquoise;

            GameObject cornerRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cornerRight.name = "Turquoise_Corner_Right";
            cornerRight.transform.SetParent(podStruct.transform, false);
            cornerRight.transform.localPosition = new Vector3(width * 0.5f - 1.5f, height * 0.5f, -depth * 0.5f - 0.15f);
            cornerRight.transform.localScale = new Vector3(3.2f, height + 0.2f, 0.5f);
            cornerRight.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Turquoise;

            // 2. Podium_Glass
            GameObject podGlass = new GameObject("Podium_Glass");
            podGlass.transform.SetParent(podium.transform, false);

            float podFloorH = height / 3.0f;
            for (int f = 1; f <= 3; f++)
            {
                float y = (f - 0.5f) * podFloorH;
                GameObject glassRibbon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glassRibbon.name = "Horizontal_Glazing_F" + f;
                glassRibbon.transform.SetParent(podGlass.transform, false);
                glassRibbon.transform.localPosition = new Vector3(0, y, -depth * 0.5f - 0.1f);
                glassRibbon.transform.localScale = new Vector3(width - 6.0f, 1.6f, 0.25f);
                glassRibbon.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Glazing;
            }

            // 3. Podium_Roof
            GameObject podRoof = new GameObject("Podium_Roof");
            podRoof.transform.SetParent(podium.transform, false);

            GameObject roofCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofCap.name = "PaleBlueGray_Roof";
            roofCap.transform.SetParent(podRoof.transform, false);
            roofCap.transform.localPosition = new Vector3(0, height + 0.4f, 0);
            roofCap.transform.localScale = new Vector3(width + 0.8f, 0.8f, depth + 0.8f);
            roofCap.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

            BoxCollider colPod = podium.AddComponent<BoxCollider>();
            colPod.center = new Vector3(0, height * 0.5f, 0);
            colPod.size = new Vector3(width, height, depth);
        }

        // ── Covered Connections Between Buildings ──
        private static void BuildCoveredConnections(GameObject parent)
        {
            GameObject conn = new GameObject("Covered_Connections");
            conn.transform.SetParent(parent.transform, false);

            // Skybridge between C and G
            GameObject bridgeCG = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridgeCG.name = "Bridge_C_to_G";
            bridgeCG.transform.SetParent(conn.transform, false);
            bridgeCG.transform.localPosition = new Vector3(18.0f, 7.5f, 26.0f);
            bridgeCG.transform.localScale = new Vector3(8.5f, 3.8f, 3.8f);
            bridgeCG.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject bridgeWin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridgeWin.name = "Bridge_Windows";
            bridgeWin.transform.SetParent(bridgeCG.transform, false);
            bridgeWin.transform.localPosition = new Vector3(0, 0, -0.52f);
            bridgeWin.transform.localScale = new Vector3(0.95f, 0.65f, 0.1f);
            bridgeWin.GetComponent<MeshRenderer>().sharedMaterial = m_G_WindowGlass;

            // Ground Walkway Canopy
            GameObject groundCanopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundCanopy.name = "Ground_Walkway_Canopy";
            groundCanopy.transform.SetParent(conn.transform, false);
            groundCanopy.transform.localPosition = new Vector3(14.0f, 3.0f, 0.0f);
            groundCanopy.transform.localScale = new Vector3(4.0f, 0.2f, 6.0f);
            groundCanopy.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

            for (int p = 0; p < 4; p++)
            {
                float px = (p % 2 == 0) ? -1.8f : 1.8f;
                float pz = (p < 2) ? -2.6f : 2.6f;
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Post_" + p;
                post.transform.SetParent(groundCanopy.transform, false);
                post.transform.localPosition = new Vector3(px, -7.5f, pz);
                post.transform.localScale = new Vector3(0.08f, 7.5f, 0.08f);
                post.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }
        }

        // ================= 3. VEGETATION (TREES, TREE PITS, PALMS, SHRUBS) =================
        private static void BuildVegetation(GameObject parent)
        {
            // Large Canopy Tropical Shade Trees with Concrete Tree Pits (Bồn cây bê tông)
            GameObject largeTrees = new GameObject("LargeTropicalTrees");
            largeTrees.transform.SetParent(parent.transform);

            Vector3[] treePositions = {
                new Vector3(6.0f, 0, -4.0f),    // Bên hông lối đi sân trong cạnh C/Podium
                new Vector3(14.0f, 0, 26.0f),   // Góc sân trong phía sau gần Nhà I
                new Vector3(32.0f, 0, -6.0f),   // Phía trước cánh phải Nhà G
                new Vector3(2.0f, 0, 8.0f)      // Phía trước cánh trái Nhà C
            };
            float[] treeHeights = { 13.5f, 15.0f, 13.0f, 12.5f };
            float[] canopyRadii = { 5.2f, 6.0f, 5.0f, 4.8f };

            for (int i = 0; i < treePositions.Length; i++)
            {
                GameObject tree = new GameObject("LargeTropicalTree_" + i);
                tree.transform.SetParent(largeTrees.transform, false);
                tree.transform.localPosition = treePositions[i];

                // Raised Concrete Tree Pit Curb (Bồn cây bê tông bo viền)
                GameObject pitCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pitCurb.name = "TreePit_Curb";
                pitCurb.transform.SetParent(tree.transform, false);
                pitCurb.transform.localPosition = new Vector3(0, 0.12f, 0);
                pitCurb.transform.localScale = new Vector3(3.2f, 0.24f, 3.2f);
                pitCurb.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

                // Dark Soil inside pit
                GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
                soil.name = "Soil";
                soil.transform.SetParent(pitCurb.transform, false);
                soil.transform.localPosition = new Vector3(0, 0.35f, 0);
                soil.transform.localScale = new Vector3(0.85f, 0.4f, 0.85f);
                soil.GetComponent<MeshRenderer>().sharedMaterial = m_PlanterSoil;

                // Natural Curved Trunk with Bark Texture
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk";
                trunk.transform.SetParent(tree.transform, false);
                trunk.transform.localPosition = new Vector3(0, treeHeights[i] * 0.35f, 0);
                trunk.transform.localScale = new Vector3(1.2f, treeHeights[i] * 0.35f, 1.2f);
                trunk.GetComponent<MeshRenderer>().sharedMaterial = m_TreeBark;

                // Massive Broadleaf Canopy (multi-tiered organic clusters)
                int clusters = 4;
                for (int c = 0; c < clusters; c++)
                {
                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crown.name = "Canopy_Cluster_" + c;
                    crown.transform.SetParent(tree.transform, false);
                    float crownY = treeHeights[i] * (0.60f + c * 0.12f);
                    float r = canopyRadii[i] * (1.0f - c * 0.15f);
                    float offsetX = (c % 2 == 1) ? 1.0f : -0.8f;
                    float offsetZ = (c >= 2) ? 0.8f : -0.8f;
                    crown.transform.localPosition = new Vector3(offsetX, crownY, offsetZ);
                    crown.transform.localScale = new Vector3(r * 2.0f, r * 1.2f, r * 2.0f);
                    crown.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
                }
            }

            // Palms (Cau cảnh sân trường dọc lối đi)
            GameObject palms = new GameObject("CampusPalms");
            palms.transform.SetParent(parent.transform);

            Vector3[] palmPositions = {
                new Vector3(8.0f, 0, -9.5f),
                new Vector3(16.0f, 0, -9.5f),
                new Vector3(24.0f, 0, -9.5f),
                new Vector3(32.0f, 0, -9.5f),
                new Vector3(3.0f, 0, -2.0f)
            };

            for (int p = 0; p < palmPositions.Length; p++)
            {
                GameObject palm = new GameObject("CampusPalm_" + p);
                palm.transform.SetParent(palms.transform, false);
                palm.transform.localPosition = palmPositions[p];

                // Planter box for each palm
                GameObject palmPit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                palmPit.name = "PalmPit";
                palmPit.transform.SetParent(palm.transform, false);
                palmPit.transform.localPosition = new Vector3(0, 0.12f, 0);
                palmPit.transform.localScale = new Vector3(1.6f, 0.24f, 1.6f);
                palmPit.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteCurb;

                float h = 7.5f + (p % 3) * 0.6f;
                GameObject pTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pTrunk.name = "Trunk";
                pTrunk.transform.SetParent(palm.transform, false);
                pTrunk.transform.localPosition = new Vector3(0, h * 0.5f, 0);
                pTrunk.transform.localScale = new Vector3(0.42f, h * 0.5f, 0.42f);
                pTrunk.GetComponent<MeshRenderer>().sharedMaterial = m_TreeBark;

                GameObject fronds = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fronds.name = "PalmCrown";
                fronds.transform.SetParent(palm.transform, false);
                fronds.transform.localPosition = new Vector3(0, h + 0.5f, 0);
                fronds.transform.localScale = new Vector3(5.2f, 1.4f, 5.2f);
                fronds.GetComponent<MeshRenderer>().sharedMaterial = m_FoliageTree;
            }

            // Trimmed Ornamental Shrubs (Hàng cây bụi trang trí)
            GameObject shrubs = new GameObject("TrimmedShrubs");
            shrubs.transform.SetParent(parent.transform);

            Vector3[] shrubPositions = {
                new Vector3(10.0f, 0, -8.5f),
                new Vector3(20.0f, 0, -8.5f),
                new Vector3(28.0f, 0, -8.5f),
                new Vector3(4.0f, 0, 10.0f),
                new Vector3(4.0f, 0, 20.0f),
                new Vector3(31.0f, 0, 2.0f)
            };

            for (int s = 0; s < shrubPositions.Length; s++)
            {
                GameObject shrub = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shrub.name = "TrimmedHedge_" + s;
                shrub.transform.SetParent(shrubs.transform, false);
                shrub.transform.localPosition = new Vector3(shrubPositions[s].x, 0.55f, shrubPositions[s].z);
                shrub.transform.localScale = new Vector3(2.4f, 1.1f, 0.85f);
                shrub.GetComponent<MeshRenderer>().sharedMaterial = m_FoliageTree;
            }
        }

        // ================= 4. PROPS (MOTORBIKES, CANOPIES, BENCHES, SIGNS) =================
        private static void BuildProps(GameObject parent)
        {
            // Motorcycles in courtyard parking: ~120 bikes in realistic dense rows with helmet details
            GameObject goBikes = new GameObject("Motorcycles");
            goBikes.transform.SetParent(parent.transform);

            System.Random rnd = new System.Random(101);
            int totalBikes = 120;
            int bikesPerRow = 30;
            int rows = 4;
            float rowStartX = 11.2f;
            float bikeSpacing = 0.44f;

            for (int r = 0; r < rows; r++)
            {
                float z = 3.5f + r * 3.6f;
                for (int b = 0; b < bikesPerRow && (r * bikesPerRow + b) < totalBikes; b++)
                {
                    // Irregular natural placement offset
                    float x = rowStartX + b * bikeSpacing + (float)(rnd.NextDouble() * 0.08f - 0.04f);
                    float rotY = 90.0f + (float)(rnd.NextDouble() * 14.0f - 7.0f); // ±7° natural variation
                    float sideStandTilt = (float)(rnd.NextDouble() * 4.0f + 2.0f); // Slight lean on kickstand

                    GameObject bike = new GameObject($"Bike_R{r}_{b}");
                    bike.transform.SetParent(goBikes.transform, false);
                    bike.transform.localPosition = new Vector3(x, 0, z);
                    bike.transform.localRotation = Quaternion.Euler(0, rotY, sideStandTilt);

                    // Body
                    GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    body.name = "Body";
                    body.transform.SetParent(bike.transform, false);
                    body.transform.localPosition = new Vector3(0, 0.52f, 0);
                    body.transform.localScale = new Vector3(1.72f, 0.44f, 0.38f);
                    body.GetComponent<MeshRenderer>().sharedMaterial = m_MotorbikeMats[rnd.Next(m_MotorbikeMats.Length)];

                    // Seat
                    GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    seat.name = "Seat";
                    seat.transform.SetParent(bike.transform, false);
                    seat.transform.localPosition = new Vector3(-0.2f, 0.78f, 0);
                    seat.transform.localScale = new Vector3(0.9f, 0.14f, 0.34f);
                    seat.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    // Handlebars
                    GameObject handlebar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    handlebar.name = "Handlebar";
                    handlebar.transform.SetParent(bike.transform, false);
                    handlebar.transform.localPosition = new Vector3(0.48f, 0.88f, 0);
                    handlebar.transform.localScale = new Vector3(0.12f, 0.06f, 0.62f);
                    handlebar.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

                    // Wheels
                    for (int w = 0; w < 2; w++)
                    {
                        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        wheel.name = "Wheel_" + w;
                        wheel.transform.SetParent(bike.transform, false);
                        wheel.transform.localPosition = new Vector3((w == 0 ? -0.62f : 0.62f), 0.30f, 0);
                        wheel.transform.localRotation = Quaternion.Euler(90, 0, 0);
                        wheel.transform.localScale = new Vector3(0.56f, 0.08f, 0.56f);
                        wheel.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
                    }

                    // Helmet on seat or handlebar for ~35% of motorbikes
                    if (rnd.NextDouble() < 0.35)
                    {
                        GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        helmet.name = "Helmet";
                        helmet.transform.SetParent(bike.transform, false);
                        bool onHandlebar = rnd.NextDouble() < 0.45;
                        if (onHandlebar)
                            helmet.transform.localPosition = new Vector3(0.48f, 0.95f, 0.22f);
                        else
                            helmet.transform.localPosition = new Vector3(-0.15f, 0.92f, 0);
                        helmet.transform.localScale = new Vector3(0.24f, 0.22f, 0.24f);
                        helmet.GetComponent<MeshRenderer>().sharedMaterial = m_HelmetMats[rnd.Next(m_HelmetMats.Length)];
                    }
                }
            }

            // Motorbike Shelter Canopy (Mái che nhà xe khung thép mái tôn lượn sóng)
            GameObject bikeShelter = new GameObject("MotorbikeParkingShelter");
            bikeShelter.transform.SetParent(parent.transform, false);

            for (int sr = 0; sr < 2; sr++)
            {
                float sz = 5.3f + sr * 7.2f;
                GameObject shelterRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelterRoof.name = "ShelterRoof_" + sr;
                shelterRoof.transform.SetParent(bikeShelter.transform, false);
                shelterRoof.transform.localPosition = new Vector3(18.0f, 2.7f, sz);
                shelterRoof.transform.localScale = new Vector3(16.5f, 0.12f, 5.5f);
                shelterRoof.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

                // Support Columns
                for (int sc = 0; sc < 5; sc++)
                {
                    float scX = 10.5f + sc * 3.75f;
                    GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    col.name = $"ShelterPost_{sr}_{sc}";
                    col.transform.SetParent(bikeShelter.transform, false);
                    col.transform.localPosition = new Vector3(scX, 1.35f, sz);
                    col.transform.localScale = new Vector3(0.12f, 1.35f, 0.12f);
                    col.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
                }
            }

            // Campus Stone Benches (Ghế đá trường học đặt dưới tán cây bóng mát)
            GameObject benches = new GameObject("CampusBenches");
            benches.transform.SetParent(parent.transform, false);

            Vector3[] benchPositions = {
                new Vector3(10.5f, 0, -4.8f),
                new Vector3(13.8f, 0, -4.8f),
                new Vector3(16.5f, 0, 25.2f),
                new Vector3(13.5f, 0, 25.2f),
                new Vector3(4.2f, 0, 4.0f),
                new Vector3(4.2f, 0, 7.5f)
            };
            for (int bi = 0; bi < benchPositions.Length; bi++)
            {
                GameObject bench = new GameObject("CampusBench_" + bi);
                bench.transform.SetParent(benches.transform, false);
                bench.transform.localPosition = benchPositions[bi];

                // Seat slab
                GameObject bSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bSeat.name = "Seat";
                bSeat.transform.SetParent(bench.transform, false);
                bSeat.transform.localPosition = new Vector3(0, 0.45f, 0);
                bSeat.transform.localScale = new Vector3(1.5f, 0.08f, 0.45f);
                bSeat.GetComponent<MeshRenderer>().sharedMaterial = m_BenchGranite;

                // Backrest
                GameObject bBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bBack.name = "Backrest";
                bBack.transform.SetParent(bench.transform, false);
                bBack.transform.localPosition = new Vector3(0, 0.75f, -0.2f);
                bBack.transform.localScale = new Vector3(1.5f, 0.42f, 0.06f);
                bBack.GetComponent<MeshRenderer>().sharedMaterial = m_BenchGranite;

                // Legs
                for (int bl = 0; bl < 2; bl++)
                {
                    GameObject bLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bLeg.name = "Leg_" + bl;
                    bLeg.transform.SetParent(bench.transform, false);
                    bLeg.transform.localPosition = new Vector3((bl == 0 ? -0.55f : 0.55f), 0.22f, 0);
                    bLeg.transform.localScale = new Vector3(0.14f, 0.44f, 0.38f);
                    bLeg.GetComponent<MeshRenderer>().sharedMaterial = m_BenchGranite;
                }
            }

            // Dual Waste / Recycling Bins (Thùng rác phân loại 2 ngăn)
            GameObject bins = new GameObject("CampusTrashBins");
            bins.transform.SetParent(parent.transform, false);

            Vector3[] binPos = {
                new Vector3(9.2f, 0, -8.0f),
                new Vector3(25.0f, 0, -8.0f),
                new Vector3(5.2f, 0, 16.0f)
            };
            for (int b = 0; b < binPos.Length; b++)
            {
                GameObject binUnit = new GameObject("DualTrashBin_" + b);
                binUnit.transform.SetParent(bins.transform, false);
                binUnit.transform.localPosition = binPos[b];

                // Green bin (Organic)
                GameObject binGreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
                binGreen.name = "Bin_Organic";
                binGreen.transform.SetParent(binUnit.transform, false);
                binGreen.transform.localPosition = new Vector3(-0.25f, 0.45f, 0);
                binGreen.transform.localScale = new Vector3(0.42f, 0.9f, 0.42f);
                binGreen.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;

                // Blue/Orange bin (Inorganic)
                GameObject binBlue = GameObject.CreatePrimitive(PrimitiveType.Cube);
                binBlue.name = "Bin_Inorganic";
                binBlue.transform.SetParent(binUnit.transform, false);
                binBlue.transform.localPosition = new Vector3(0.25f, 0.45f, 0);
                binBlue.transform.localScale = new Vector3(0.42f, 0.9f, 0.42f);
                binBlue.GetComponent<MeshRenderer>().sharedMaterial = m_MotorbikeMats[2];
            }

            // Wayfinding & Notice Signage (Biển chỉ dẫn & Bảng tin thông báo Đoàn Hội)
            GameObject signs = new GameObject("CampusSigns");
            signs.transform.SetParent(parent.transform, false);

            // Directional Signpost
            GameObject dirPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dirPost.name = "Directional_Post";
            dirPost.transform.SetParent(signs.transform, false);
            dirPost.transform.localPosition = new Vector3(8.5f, 1.35f, -9.0f);
            dirPost.transform.localScale = new Vector3(0.12f, 1.35f, 0.12f);
            dirPost.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

            GameObject dirBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dirBoard.name = "Directional_Board";
            dirBoard.transform.SetParent(dirPost.transform, false);
            dirBoard.transform.localPosition = new Vector3(0, 0.55f, 0);
            dirBoard.transform.localScale = new Vector3(8.0f, 0.55f, 0.4f);
            dirBoard.GetComponent<MeshRenderer>().sharedMaterial = m_SignDirectional;

            // Campus Bulletin Notice Board
            GameObject noticeBoard = new GameObject("Notice_Bulletin_Board");
            noticeBoard.transform.SetParent(signs.transform, false);
            noticeBoard.transform.localPosition = new Vector3(25.5f, 0, -9.0f);

            GameObject nbPostL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nbPostL.name = "Post_L";
            nbPostL.transform.SetParent(noticeBoard.transform, false);
            nbPostL.transform.localPosition = new Vector3(-1.1f, 1.25f, 0);
            nbPostL.transform.localScale = new Vector3(0.08f, 1.25f, 0.08f);
            nbPostL.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject nbPostR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nbPostR.name = "Post_R";
            nbPostR.transform.SetParent(noticeBoard.transform, false);
            nbPostR.transform.localPosition = new Vector3(1.1f, 1.25f, 0);
            nbPostR.transform.localScale = new Vector3(0.08f, 1.25f, 0.08f);
            nbPostR.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject nbBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nbBoard.name = "Board";
            nbBoard.transform.SetParent(noticeBoard.transform, false);
            nbBoard.transform.localPosition = new Vector3(0, 1.6f, 0);
            nbBoard.transform.localScale = new Vector3(2.4f, 1.2f, 0.08f);
            nbBoard.GetComponent<MeshRenderer>().sharedMaterial = m_SignNoticeBoard;

            // Campus Outdoor Lighting Lampposts (Cột đèn chiếu sáng khuôn viên)
            GameObject lampposts = new GameObject("Lampposts");
            lampposts.transform.SetParent(parent.transform, false);

            Vector3[] lampPos = {
                new Vector3(7.5f, 0, -12.5f),
                new Vector3(22.0f, 0, -12.5f),
                new Vector3(36.0f, 0, -12.5f),
                new Vector3(7.5f, 0, 15.0f),
                new Vector3(28.5f, 0, 15.0f)
            };
            for (int l = 0; l < lampPos.Length; l++)
            {
                GameObject lamp = new GameObject("Lamppost_" + l);
                lamp.transform.SetParent(lampposts.transform, false);
                lamp.transform.localPosition = lampPos[l];

                // Pole
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Pole";
                pole.transform.SetParent(lamp.transform, false);
                pole.transform.localPosition = new Vector3(0, 2.25f, 0);
                pole.transform.localScale = new Vector3(0.12f, 2.25f, 0.12f);
                pole.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                // Lamp Luminaire Head
                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
                head.name = "LuminaireHead";
                head.transform.SetParent(lamp.transform, false);
                head.transform.localPosition = new Vector3(0.2f, 4.45f, 0);
                head.transform.localScale = new Vector3(0.65f, 0.12f, 0.35f);
                head.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;
            }

            // Security Guard Booth
            GameObject booth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            booth.name = "Security_Guard_Booth";
            booth.transform.SetParent(parent.transform, false);
            booth.transform.localPosition = new Vector3(4.5f, 1.5f, -12.0f);
            booth.transform.localScale = new Vector3(2.8f, 3.0f, 2.8f);
            booth.GetComponent<MeshRenderer>().sharedMaterial = m_G_Wall;

            GameObject boothRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boothRoof.name = "Booth_Roof";
            boothRoof.transform.SetParent(booth.transform, false);
            boothRoof.transform.localPosition = new Vector3(0, 0.55f, 0);
            boothRoof.transform.localScale = new Vector3(1.2f, 0.15f, 1.2f);
            boothRoof.GetComponent<MeshRenderer>().sharedMaterial = m_Podium_Roof;

            // Power transformer box
            GameObject transformer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            transformer.name = "Electrical_Transformer_Box";
            transformer.transform.SetParent(parent.transform, false);
            transformer.transform.localPosition = new Vector3(40.0f, 1.25f, 28.0f);
            transformer.transform.localScale = new Vector3(2.5f, 2.5f, 2.0f);
            transformer.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;
        }

        private struct NPCDef
        {
            public Vector3 pos;
            public float rotY;
            public bool isSitting;
            public bool isBlueShirt;
        }

        // ================= 5. CAMPUS LIFE / STUDENT NPCS =================
        private static void BuildCampusLife(GameObject parent)
        {
            // 20 Student NPCs placed in natural groupings (studying on benches, chatting, walking)
            // Keeping clear circulation path (>2.5m) for player exploration
            List<NPCDef> npcList = new List<NPCDef>
            {
                // Group 1: Chatting near entrance plaza
                new NPCDef { pos = new Vector3(10.5f, 0, -6.5f), rotY = 45.0f, isSitting = false, isBlueShirt = false },
                new NPCDef { pos = new Vector3(11.4f, 0, -5.8f), rotY = -135.0f, isSitting = false, isBlueShirt = true },

                // Group 2: Sitting on stone bench 1
                new NPCDef { pos = new Vector3(10.2f, 0.45f, -4.8f), rotY = 0.0f, isSitting = true, isBlueShirt = false },
                new NPCDef { pos = new Vector3(10.8f, 0.45f, -4.8f), rotY = 0.0f, isSitting = true, isBlueShirt = true },

                // Group 3: Sitting on stone bench 2
                new NPCDef { pos = new Vector3(13.8f, 0.45f, -4.8f), rotY = 0.0f, isSitting = true, isBlueShirt = false },

                // Group 4: Walking along front sidewalk
                new NPCDef { pos = new Vector3(18.5f, 0, -8.2f), rotY = 90.0f, isSitting = false, isBlueShirt = false },
                new NPCDef { pos = new Vector3(22.0f, 0, -8.0f), rotY = 85.0f, isSitting = false, isBlueShirt = true },
                new NPCDef { pos = new Vector3(28.0f, 0, -8.4f), rotY = -90.0f, isSitting = false, isBlueShirt = false },

                // Group 5: Chatting by notice board
                new NPCDef { pos = new Vector3(24.5f, 0, -7.8f), rotY = 30.0f, isSitting = false, isBlueShirt = false },
                new NPCDef { pos = new Vector3(25.3f, 0, -7.5f), rotY = -150.0f, isSitting = false, isBlueShirt = true },

                // Group 6: Courtyard walking path
                new NPCDef { pos = new Vector3(8.2f, 0, 4.0f), rotY = 0.0f, isSitting = false, isBlueShirt = true },
                new NPCDef { pos = new Vector3(8.2f, 0, 12.0f), rotY = 0.0f, isSitting = false, isBlueShirt = false },
                new NPCDef { pos = new Vector3(8.5f, 0, 20.0f), rotY = 180.0f, isSitting = false, isBlueShirt = false },

                // Group 7: Near motorbike parking row
                new NPCDef { pos = new Vector3(12.0f, 0, 1.8f), rotY = -45.0f, isSitting = false, isBlueShirt = true },
                new NPCDef { pos = new Vector3(15.5f, 0, 1.6f), rotY = 40.0f, isSitting = false, isBlueShirt = false },

                // Group 8: Rear courtyard bench
                new NPCDef { pos = new Vector3(16.5f, 0.45f, 25.2f), rotY = 180.0f, isSitting = true, isBlueShirt = false },
                new NPCDef { pos = new Vector3(13.5f, 0.45f, 25.2f), rotY = 180.0f, isSitting = true, isBlueShirt = true },

                // Group 9: Near security booth
                new NPCDef { pos = new Vector3(6.5f, 0, -11.5f), rotY = -90.0f, isSitting = false, isBlueShirt = true },
                new NPCDef { pos = new Vector3(5.5f, 0, -9.8f), rotY = 10.0f, isSitting = false, isBlueShirt = false }
            };

            for (int n = 0; n < npcList.Count; n++)
            {
                var def = npcList[n];
                GameObject student = new GameObject("Student_NPC_" + n);
                student.transform.SetParent(parent.transform, false);
                student.transform.localPosition = def.pos;
                student.transform.localRotation = Quaternion.Euler(0, def.rotY, 0);

                Material shirtMat = def.isBlueShirt ? m_StudentBluePolo : m_StudentWhiteShirt;

                if (!def.isSitting)
                {
                    // Standing human figure (~1.72m tall)
                    // Torso & Shirt
                    GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    torso.name = "Torso";
                    torso.transform.SetParent(student.transform, false);
                    torso.transform.localPosition = new Vector3(0, 1.15f, 0);
                    torso.transform.localScale = new Vector3(0.42f, 0.58f, 0.24f);
                    torso.GetComponent<MeshRenderer>().sharedMaterial = shirtMat;

                    // Head & Hair
                    GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    head.name = "Head";
                    head.transform.SetParent(student.transform, false);
                    head.transform.localPosition = new Vector3(0, 1.58f, 0);
                    head.transform.localScale = new Vector3(0.22f, 0.24f, 0.22f);
                    head.GetComponent<MeshRenderer>().sharedMaterial = m_StudentSkin;

                    GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    hair.name = "Hair";
                    hair.transform.SetParent(student.transform, false);
                    hair.transform.localPosition = new Vector3(0, 1.64f, -0.02f);
                    hair.transform.localScale = new Vector3(0.24f, 0.16f, 0.24f);
                    hair.GetComponent<MeshRenderer>().sharedMaterial = m_StudentHair;

                    // Backpack
                    GameObject pack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pack.name = "Backpack";
                    pack.transform.SetParent(student.transform, false);
                    pack.transform.localPosition = new Vector3(0, 1.18f, -0.18f);
                    pack.transform.localScale = new Vector3(0.32f, 0.44f, 0.16f);
                    pack.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    // Legs
                    for (int l = 0; l < 2; l++)
                    {
                        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        leg.name = "Leg_" + l;
                        leg.transform.SetParent(student.transform, false);
                        leg.transform.localPosition = new Vector3((l == 0 ? -0.11f : 0.11f), 0.44f, 0);
                        leg.transform.localScale = new Vector3(0.16f, 0.86f, 0.18f);
                        leg.GetComponent<MeshRenderer>().sharedMaterial = (n % 2 == 0) ? m_StudentJeans : m_StudentDarkPants;
                    }
                }
                else
                {
                    // Sitting human figure
                    // Torso
                    GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    torso.name = "Torso";
                    torso.transform.SetParent(student.transform, false);
                    torso.transform.localPosition = new Vector3(0, 0.52f, -0.08f);
                    torso.transform.localScale = new Vector3(0.40f, 0.55f, 0.24f);
                    torso.GetComponent<MeshRenderer>().sharedMaterial = shirtMat;

                    // Head & Hair
                    GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    head.name = "Head";
                    head.transform.SetParent(student.transform, false);
                    head.transform.localPosition = new Vector3(0, 0.94f, -0.06f);
                    head.transform.localScale = new Vector3(0.22f, 0.24f, 0.22f);
                    head.GetComponent<MeshRenderer>().sharedMaterial = m_StudentSkin;

                    GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    hair.name = "Hair";
                    hair.transform.SetParent(student.transform, false);
                    hair.transform.localPosition = new Vector3(0, 1.0f, -0.08f);
                    hair.transform.localScale = new Vector3(0.24f, 0.16f, 0.24f);
                    hair.GetComponent<MeshRenderer>().sharedMaterial = m_StudentHair;

                    // Lap & Thighs
                    GameObject lap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    lap.name = "Lap";
                    lap.transform.SetParent(student.transform, false);
                    lap.transform.localPosition = new Vector3(0, 0.12f, 0.16f);
                    lap.transform.localScale = new Vector3(0.38f, 0.18f, 0.44f);
                    lap.GetComponent<MeshRenderer>().sharedMaterial = m_StudentJeans;

                    // Lower legs
                    for (int l = 0; l < 2; l++)
                    {
                        GameObject lowerLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        lowerLeg.name = "LowerLeg_" + l;
                        lowerLeg.transform.SetParent(student.transform, false);
                        lowerLeg.transform.localPosition = new Vector3((l == 0 ? -0.11f : 0.11f), -0.18f, 0.35f);
                        lowerLeg.transform.localScale = new Vector3(0.14f, 0.44f, 0.16f);
                        lowerLeg.GetComponent<MeshRenderer>().sharedMaterial = m_StudentJeans;
                    }
                }
            }
        }

        // ================= 6. LIGHTING & CAMERAS =================
        private static void BuildLighting(GameObject parent)
        {
            // Directional Daylight: Warm tropical sunlight (~5800K, soft shadows)
            GameObject sunGO = new GameObject("DirectionalLight");
            sunGO.transform.SetParent(parent.transform);
            sunGO.transform.localPosition = new Vector3(0, 50.0f, 0);
            sunGO.transform.localRotation = Quaternion.Euler(46.0f, -32.0f, 0.0f);

            Light sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1.0f, 0.97f, 0.93f); // Warm natural daylight
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.72f;

            // Ambient bounce & Sky
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.82f, 0.94f);
            RenderSettings.ambientEquatorColor = new Color(0.85f, 0.86f, 0.88f);
            RenderSettings.ambientGroundColor = new Color(0.45f, 0.42f, 0.38f);
        }

        private static void SetupLightingAndCameras(GameObject clusterGO)
        {
            // Setup Cameras container
            GameObject camRoot = new GameObject("Cameras");
            camRoot.transform.SetParent(clusterGO.transform);

            Color tropicalSkyColor = new Color(0.68f, 0.82f, 0.96f);

            // 1. Camera_Aerial: Balanced campus flycam oblique view showing U-shape, courtyard, and background city
            GameObject camAerial = new GameObject("Camera_Aerial");
            camAerial.transform.SetParent(camRoot.transform);
            camAerial.transform.position = new Vector3(-4.0f, 42.0f, -28.0f);
            camAerial.transform.rotation = Quaternion.Euler(38.0f, 32.0f, 0.0f);
            Camera cAer = camAerial.AddComponent<Camera>();
            cAer.fieldOfView = 62.0f;
            cAer.farClipPlane = 450.0f;
            cAer.clearFlags = CameraClearFlags.SolidColor;
            cAer.backgroundColor = tropicalSkyColor;

            // 2. Camera_Courtyard: Courtyard environmental density view showing motorbikes, trees, benches, students
            GameObject camCourt = new GameObject("Camera_Courtyard");
            camCourt.transform.SetParent(camRoot.transform);
            camCourt.transform.position = new Vector3(7.0f, 3.2f, 0.5f);
            camCourt.transform.rotation = Quaternion.Euler(12.0f, 42.0f, 0.0f);
            Camera cCrt = camCourt.AddComponent<Camera>();
            cCrt.fieldOfView = 68.0f;
            cCrt.farClipPlane = 300.0f;
            cCrt.clearFlags = CameraClearFlags.SolidColor;
            cCrt.backgroundColor = tropicalSkyColor;

            // 3. Camera_Entrance: Grounding, curbs, drainage & entrance walkway canopy view
            GameObject camEnt = new GameObject("Camera_Entrance");
            camEnt.transform.SetParent(camRoot.transform);
            camEnt.transform.position = new Vector3(14.5f, 1.75f, -10.5f);
            camEnt.transform.rotation = Quaternion.Euler(5.0f, 18.0f, 0.0f);
            Camera cEnt = camEnt.AddComponent<Camera>();
            cEnt.fieldOfView = 65.0f;
            cEnt.farClipPlane = 250.0f;
            cEnt.clearFlags = CameraClearFlags.SolidColor;
            cEnt.backgroundColor = tropicalSkyColor;

            // 4. Camera_PlayerHeight: Human eye-level 1.70m perspective looking across courtyard towards Building G
            GameObject camEye = new GameObject("Camera_PlayerHeight");
            camEye.transform.SetParent(camRoot.transform);
            camEye.transform.position = new Vector3(9.2f, 1.70f, -6.0f);
            camEye.transform.rotation = Quaternion.Euler(3.0f, 38.0f, 0.0f);
            Camera cEye = camEye.AddComponent<Camera>();
            cEye.fieldOfView = 68.0f;
            cEye.farClipPlane = 250.0f;
            cEye.clearFlags = CameraClearFlags.SolidColor;
            cEye.backgroundColor = tropicalSkyColor;
        }
    }
}
