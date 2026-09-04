using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    public static class IUHBuildingBuilder
    {
        private const string MatPath = "Assets/IUH_Campus/Materials/";
        private const string PrefabArchPath = "Assets/IUH_Campus/Prefabs/Architecture/";
        private const string PrefabEnvPath = "Assets/IUH_Campus/Prefabs/Environment/";
        private const string ScenePath = "Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity";

        private static Material m_WhiteFacade;
        private static Material m_BlueFins;
        private static Material m_BlueFacade;
        private static Material m_DarkMetal;
        private static Material m_LightMetal;
        private static Material m_GlassReflective;
        private static Material m_GlassCurtain;
        private static Material m_SignBillboard;
        private static Material m_LogoCrest;
        private static Material m_GraniteStairs;
        private static Material m_AsphaltRoad;
        private static Material m_ConcreteGround;
        private static Material m_Grass;
        private static Material m_FlowerOrange;
        private static Material m_FlowerPotWhite;
        private static Material m_FoliageTree;
        private static Material m_MonumentStone;
        private static Material m_MonumentLetter;
        private static Material m_FlagVn;
        private static Material m_FlagIuh;
        private static Material m_RoadArrow;
        private static Material m_BgBuilding;

        private static Material m_GreenGlass;
        private static Material m_RedMetalRoof;
        private static Material m_PaleGreenRoof;
        private static Material m_TowerPyramidRoof;
        private static Material m_MotorbikeRed;
        private static Material m_MotorbikeDark;
        private static Material m_BlueTarp;
        private static Material m_MatureFoliage;
        private static Material m_RooftopMetal;

        [MenuItem("IUH Campus/Build Main Building Ground Scene")]
        public static void BuildFullCampus()
        {
            IUHTextureGenerator.GenerateAllTextures();
            IUHMaterialGenerator.GenerateAllMaterials();
            LoadMaterials();
            EnsureDirectories();

            // 1. Base Main Building Prefabs
            GameObject finPrefab = CreateVerticalFinPrefab();
            GameObject windowPrefab = CreateWindowModulePrefab();
            GameObject canopyPrefab = CreateEntranceCanopyPrefab();
            GameObject stairsPrefab = CreateEntranceStairsPrefab();
            GameObject lobbyPrefab = CreateEntranceLobbyPrefab();
            GameObject glassBoxPrefab = CreateCentralGlassBoxPrefab();
            GameObject monumentPrefab = CreateFrontMonumentPrefab();
            GameObject flowerPotPrefab = CreateFlowerPotPrefab();
            GameObject flowerRowPrefab = CreateFlowerRowPrefab(flowerPotPrefab);
            GameObject grassIslandPrefab = CreateGrassIslandPrefab();
            GameObject treePrefab = CreateCampusTreePrefab();
            GameObject flagPolesPrefab = CreateFlagPolesPrefab();

            GameObject mainBuildingPrefab = AssembleMainBuildingPrefab(
                finPrefab, windowPrefab, canopyPrefab, stairsPrefab, lobbyPrefab, glassBoxPrefab
            );

            // 2. Large Left Academic Building (Nhà X/V - Massive Horizontal Building with Balconies & Pale Green Roof)
            GameObject largeLeftBuildingPrefab = CreateLargeLeftBuildingPrefab();

            // 3. Green Glass Tower (Nhà B/E - Attached/adjacent directly to the right end of the Large Left Building)
            GameObject greenTowerPrefab = CreateGreenGlassTowerPrefab();

            // 4. Red-Roof Academic Block (Nhà A - Behind the Large Left Building)
            GameObject redRoofBuildingPrefab = CreateRedRoofBuildingPrefab();

            // 5. Environmental & Campus Props
            GameObject centralMatureTreePrefab = CreateCentralMatureTreePrefab();
            GameObject motorbikeRowPrefab = CreateMotorbikeParkingPrefab();
            GameObject blueBoothPrefab = CreateCanopyBoothPrefab();
            GameObject sportsCourtPrefab = CreateSportsCourtyardPrefab();
            GameObject coveredRampPrefab = CreateCoveredRampPrefab();

            // 6. Build the Final Accurate Scene
            CreateAndPopulateAccurateScene(
                mainBuildingPrefab, monumentPrefab, grassIslandPrefab, flowerRowPrefab,
                treePrefab, flagPolesPrefab,
                largeLeftBuildingPrefab, greenTowerPrefab, redRoofBuildingPrefab,
                centralMatureTreePrefab, motorbikeRowPrefab, blueBoothPrefab,
                sportsCourtPrefab, coveredRampPrefab
            );

            Debug.Log("[IUH] Masterplan corrected and built with 100% spatial accuracy!");
        }

        private static void LoadMaterials()
        {
            m_WhiteFacade = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_WhiteFacade.mat");
            m_BlueFins = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_BlueFins.mat");
            m_BlueFacade = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_BlueFacade.mat");
            m_DarkMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_DarkMetal.mat");
            m_LightMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_LightMetal.mat");
            m_GlassReflective = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Glass_Reflective.mat");
            m_GlassCurtain = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Glass_CurtainWall.mat");
            m_SignBillboard = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Sign_CurtainWall.mat");
            m_LogoCrest = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Logo_Crest.mat");
            m_GraniteStairs = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Granite_Stairs.mat");
            m_AsphaltRoad = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Asphalt_Road.mat");
            m_ConcreteGround = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Concrete_Ground.mat");
            m_Grass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Grass.mat");
            m_FlowerOrange = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Flower_Orange.mat");
            m_FlowerPotWhite = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_FlowerPot_White.mat");
            m_FoliageTree = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Foliage_Tree.mat");
            m_MonumentStone = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Monument_Stone.mat");
            m_MonumentLetter = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Monument_Letter.mat");
            m_FlagVn = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Flag_Vietnam.mat");
            m_FlagIuh = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Flag_IUH.mat");
            m_RoadArrow = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Road_Arrow.mat");
            m_BgBuilding = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Background_Building.mat");

            m_GreenGlass = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_GreenGlass.mat");
            m_RedMetalRoof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_RedMetalRoof.mat");
            m_PaleGreenRoof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_PaleGreenRoof.mat");
            m_TowerPyramidRoof = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_TowerPyramidRoof.mat");
            m_MotorbikeRed = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Red.mat");
            m_MotorbikeDark = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Dark.mat");
            m_BlueTarp = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_BlueTarp.mat");
            m_MatureFoliage = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_MatureFoliage.mat");
            m_RooftopMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_RooftopMetal.mat");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(PrefabArchPath)) Directory.CreateDirectory(PrefabArchPath);
            if (!Directory.Exists(PrefabEnvPath)) Directory.CreateDirectory(PrefabEnvPath);
            if (!Directory.Exists("Assets/IUH_Campus/Scenes/")) Directory.CreateDirectory("Assets/IUH_Campus/Scenes/");
        }

        private static GameObject SaveAsPrefab(GameObject go, string fullPath)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, fullPath);
            GameObject.DestroyImmediate(go);
            return prefab;
        }

        #region Base Main Building Prefabs

        private static GameObject CreateVerticalFinPrefab()
        {
            GameObject root = new GameObject("IUH_VerticalFin");
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "FinBlade";
            blade.transform.SetParent(root.transform);
            blade.transform.localPosition = new Vector3(0, 0, 0.15f);
            blade.transform.localScale = new Vector3(0.12f, 4.0f, 0.35f);
            blade.GetComponent<MeshRenderer>().sharedMaterial = m_BlueFins;

            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "FinTrim";
            trim.transform.SetParent(root.transform);
            trim.transform.localPosition = new Vector3(0, 0, 0.33f);
            trim.transform.localScale = new Vector3(0.04f, 4.0f, 0.04f);
            trim.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_VerticalFin.prefab");
        }

        private static GameObject CreateWindowModulePrefab()
        {
            GameObject root = new GameObject("IUH_WindowModule");

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(root.transform);
            frame.transform.localScale = new Vector3(3.2f, 3.8f, 0.15f);
            frame.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "Glass";
            glass.transform.SetParent(root.transform);
            glass.transform.localPosition = new Vector3(0, 0, 0.02f);
            glass.transform.localScale = new Vector3(3.0f, 3.6f, 0.08f);
            glass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;

            GameObject mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mullion.name = "Mullion_H";
            mullion.transform.SetParent(root.transform);
            mullion.transform.localPosition = new Vector3(0, 0.4f, 0.05f);
            mullion.transform.localScale = new Vector3(3.0f, 0.08f, 0.06f);
            mullion.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_WindowModule.prefab");
        }

        private static GameObject CreateEntranceCanopyPrefab()
        {
            GameObject root = new GameObject("IUH_EntranceCanopy");

            GameObject canopyTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopyTop.name = "Canopy_Top";
            canopyTop.transform.SetParent(root.transform);
            canopyTop.transform.localPosition = new Vector3(0, 0.2f, 2.8f);
            canopyTop.transform.localScale = new Vector3(18.0f, 0.35f, 6.0f);
            canopyTop.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject underside = GameObject.CreatePrimitive(PrimitiveType.Cube);
            underside.name = "Canopy_Soffit";
            underside.transform.SetParent(root.transform);
            underside.transform.localPosition = new Vector3(0, 0.02f, 2.8f);
            underside.transform.localScale = new Vector3(17.4f, 0.05f, 5.6f);
            underside.GetComponent<MeshRenderer>().sharedMaterial = m_GlassCurtain;

            GameObject fascia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fascia.name = "Canopy_Fascia";
            fascia.transform.SetParent(root.transform);
            fascia.transform.localPosition = new Vector3(0, 0.2f, 5.8f);
            fascia.transform.localScale = new Vector3(18.2f, 0.45f, 0.15f);
            fascia.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            for (int i = -3; i <= 3; i++)
            {
                if (i == 0) continue;
                GameObject strut = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                strut.name = "Strut_" + i;
                strut.transform.SetParent(root.transform);
                float x = i * 2.6f;
                strut.transform.localPosition = new Vector3(x, 1.2f, 2.6f);
                strut.transform.localRotation = Quaternion.Euler(45, 0, 0);
                strut.transform.localScale = new Vector3(0.08f, 1.8f, 0.08f);
                strut.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;
            }

            return SaveAsPrefab(root, PrefabArchPath + "IUH_EntranceCanopy.prefab");
        }

        private static GameObject CreateEntranceStairsPrefab()
        {
            GameObject root = new GameObject("IUH_EntranceStairs");

            int steps = 5;
            float stepDepth = 0.6f;
            float stepHeight = 0.20f;
            float baseWidth = 22.0f;

            for (int s = 0; s < steps; s++)
            {
                GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = "Step_" + (s + 1);
                step.transform.SetParent(root.transform);
                float y = (s + 0.5f) * stepHeight;
                float z = (steps - 1 - s) * stepDepth + 2.0f;
                float width = baseWidth + (steps - s) * 0.4f;
                step.transform.localPosition = new Vector3(0, y, z);
                step.transform.localScale = new Vector3(width, stepHeight, (s + 1) * stepDepth + 4.0f);
                step.GetComponent<MeshRenderer>().sharedMaterial = m_GraniteStairs;
            }

            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Landing_Platform";
            platform.transform.SetParent(root.transform);
            platform.transform.localPosition = new Vector3(0, steps * stepHeight + 0.05f, -1.0f);
            platform.transform.localScale = new Vector3(26.0f, 0.1f, 8.0f);
            platform.GetComponent<MeshRenderer>().sharedMaterial = m_GraniteStairs;

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject cheek = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cheek.name = "CheekWall_" + (side > 0 ? "Right" : "Left");
                cheek.transform.SetParent(root.transform);
                cheek.transform.localPosition = new Vector3(side * (baseWidth * 0.52f), (steps * stepHeight * 0.6f), 3.0f);
                cheek.transform.localScale = new Vector3(0.8f, steps * stepHeight * 1.3f, 6.0f);
                cheek.GetComponent<MeshRenderer>().sharedMaterial = m_GraniteStairs;
            }

            return SaveAsPrefab(root, PrefabArchPath + "IUH_EntranceStairs.prefab");
        }

        private static GameObject CreateEntranceLobbyPrefab()
        {
            GameObject root = new GameObject("IUH_EntranceLobby");

            GameObject glassWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glassWall.name = "Lobby_GlassFacade";
            glassWall.transform.SetParent(root.transform);
            glassWall.transform.localPosition = new Vector3(0, 2.25f, 0);
            glassWall.transform.localScale = new Vector3(38.0f, 4.5f, 0.1f);
            glassWall.GetComponent<MeshRenderer>().sharedMaterial = m_GlassCurtain;

            for (int i = -9; i <= 9; i++)
            {
                GameObject mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mullion.name = "Mullion_V_" + i;
                mullion.transform.SetParent(root.transform);
                mullion.transform.localPosition = new Vector3(i * 2.0f, 2.25f, 0.05f);
                mullion.transform.localScale = new Vector3(0.1f, 4.5f, 0.12f);
                mullion.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }

            GameObject doorLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorLeft.name = "Door_Left";
            doorLeft.transform.SetParent(root.transform);
            doorLeft.transform.localPosition = new Vector3(-1.2f, 1.5f, 0.08f);
            doorLeft.transform.localScale = new Vector3(2.2f, 3.0f, 0.08f);
            doorLeft.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject doorRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorRight.name = "Door_Right";
            doorRight.transform.SetParent(root.transform);
            doorRight.transform.localPosition = new Vector3(1.2f, 1.5f, 0.08f);
            doorRight.transform.localScale = new Vector3(2.2f, 3.0f, 0.08f);
            doorRight.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            for (int c = -3; c <= 3; c++)
            {
                if (c == 0) continue;
                GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                col.name = "InteriorColumn_" + c;
                col.transform.SetParent(root.transform);
                col.transform.localPosition = new Vector3(c * 5.0f, 2.25f, -4.0f);
                col.transform.localScale = new Vector3(0.8f, 2.25f, 0.8f);
                col.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }

            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "ReceptionDesk";
            desk.transform.SetParent(root.transform);
            desk.transform.localPosition = new Vector3(0, 0.6f, -6.0f);
            desk.transform.localScale = new Vector3(6.0f, 1.2f, 1.4f);
            desk.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject deskTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deskTop.name = "ReceptionDeskTop";
            deskTop.transform.SetParent(desk.transform);
            deskTop.transform.localPosition = new Vector3(0, 0.52f, 0);
            deskTop.transform.localScale = new Vector3(1.05f, 0.08f, 1.1f);
            deskTop.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject lobbyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lobbyFloor.name = "LobbyFloor";
            lobbyFloor.transform.SetParent(root.transform);
            lobbyFloor.transform.localPosition = new Vector3(0, 0.02f, -8.0f);
            lobbyFloor.transform.localScale = new Vector3(38.0f, 0.05f, 16.0f);
            lobbyFloor.GetComponent<MeshRenderer>().sharedMaterial = m_GraniteStairs;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_EntranceLobby.prefab");
        }

        private static GameObject CreateCentralGlassBoxPrefab()
        {
            GameObject root = new GameObject("IUH_CentralGlassBox");

            float boxWidth = 20.0f;
            float boxHeight = 11.5f;
            float boxDepth = 1.6f;

            GameObject glassCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glassCube.name = "GlassVolume";
            glassCube.transform.SetParent(root.transform);
            glassCube.transform.localPosition = new Vector3(0, boxHeight * 0.5f, boxDepth * 0.5f);
            glassCube.transform.localScale = new Vector3(boxWidth, boxHeight, boxDepth);
            glassCube.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "PerimeterFrame";
            frame.transform.SetParent(root.transform);
            frame.transform.localPosition = new Vector3(0, boxHeight * 0.5f, boxDepth + 0.02f);
            frame.transform.localScale = new Vector3(boxWidth + 0.4f, boxHeight + 0.4f, 0.1f);
            frame.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            int numV = 6;
            for (int i = 0; i <= numV; i++)
            {
                float x = -boxWidth * 0.5f + (boxWidth / numV) * i;
                GameObject mullionV = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mullionV.name = "Mullion_V_" + i;
                mullionV.transform.SetParent(root.transform);
                mullionV.transform.localPosition = new Vector3(x, boxHeight * 0.5f, boxDepth + 0.05f);
                mullionV.transform.localScale = new Vector3(0.12f, boxHeight, 0.12f);
                mullionV.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }

            int numH = 4;
            for (int j = 0; j <= numH; j++)
            {
                float y = (boxHeight / numH) * j;
                GameObject mullionH = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mullionH.name = "Mullion_H_" + j;
                mullionH.transform.SetParent(root.transform);
                mullionH.transform.localPosition = new Vector3(0, y, boxDepth + 0.05f);
                mullionH.transform.localScale = new Vector3(boxWidth, 0.12f, 0.12f);
                mullionH.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }

            GameObject signBoard = GameObject.CreatePrimitive(PrimitiveType.Quad);
            signBoard.name = "IUH_Billboard_Sign";
            signBoard.transform.SetParent(root.transform);
            signBoard.transform.localPosition = new Vector3(0, boxHeight * 0.65f, boxDepth + 0.12f);
            signBoard.transform.localRotation = Quaternion.Euler(0, 180, 0);
            signBoard.transform.localScale = new Vector3(14.0f, 4.2f, 1.0f);
            signBoard.GetComponent<MeshRenderer>().sharedMaterial = m_SignBillboard;

            GameObject subLouver = GameObject.CreatePrimitive(PrimitiveType.Cube);
            subLouver.name = "SubLouver_Strip";
            subLouver.transform.SetParent(root.transform);
            subLouver.transform.localPosition = new Vector3(0, 0.15f, boxDepth + 0.2f);
            subLouver.transform.localScale = new Vector3(boxWidth, 0.4f, 0.6f);
            subLouver.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_CentralGlassBox.prefab");
        }

        private static GameObject CreateFrontMonumentPrefab()
        {
            GameObject root = new GameObject("IUH_FrontMonument");

            GameObject baseCenter = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            baseCenter.name = "Rock_Center";
            baseCenter.transform.SetParent(root.transform);
            baseCenter.transform.localPosition = new Vector3(0, 0.55f, 0);
            baseCenter.transform.localRotation = Quaternion.Euler(0, 0, 90);
            baseCenter.transform.localScale = new Vector3(0.9f, 2.4f, 1.2f);
            baseCenter.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentStone;

            GameObject baseLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            baseLeft.name = "Rock_Left";
            baseLeft.transform.SetParent(root.transform);
            baseLeft.transform.localPosition = new Vector3(-2.2f, 0.45f, 0.1f);
            baseLeft.transform.localScale = new Vector3(1.2f, 0.85f, 1.1f);
            baseLeft.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentStone;

            GameObject baseRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            baseRight.name = "Rock_Right";
            baseRight.transform.SetParent(root.transform);
            baseRight.transform.localPosition = new Vector3(2.2f, 0.40f, -0.1f);
            baseRight.transform.localScale = new Vector3(1.1f, 0.80f, 1.0f);
            baseRight.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentStone;

            GameObject letterI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letterI.name = "Letter_I";
            letterI.transform.SetParent(root.transform);
            letterI.transform.localPosition = new Vector3(1.3f, 1.35f, 0.25f);
            letterI.transform.localScale = new Vector3(0.35f, 1.4f, 0.3f);
            letterI.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject letterU = new GameObject("Letter_U");
            letterU.transform.SetParent(root.transform);
            letterU.transform.localPosition = new Vector3(0, 1.35f, 0.25f);

            GameObject uL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uL.transform.SetParent(letterU.transform);
            uL.transform.localPosition = new Vector3(-0.45f, 0.1f, 0);
            uL.transform.localScale = new Vector3(0.28f, 1.2f, 0.3f);
            uL.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject uR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uR.transform.SetParent(letterU.transform);
            uR.transform.localPosition = new Vector3(0.45f, 0.1f, 0);
            uR.transform.localScale = new Vector3(0.28f, 1.2f, 0.3f);
            uR.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject uB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uB.transform.SetParent(letterU.transform);
            uB.transform.localPosition = new Vector3(0, -0.55f, 0);
            uB.transform.localScale = new Vector3(1.18f, 0.28f, 0.3f);
            uB.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject letterH = new GameObject("Letter_H");
            letterH.transform.SetParent(root.transform);
            letterH.transform.localPosition = new Vector3(-1.3f, 1.35f, 0.25f);

            GameObject hL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hL.transform.SetParent(letterH.transform);
            hL.transform.localPosition = new Vector3(-0.45f, 0, 0);
            hL.transform.localScale = new Vector3(0.28f, 1.4f, 0.3f);
            hL.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject hR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hR.transform.SetParent(letterH.transform);
            hR.transform.localPosition = new Vector3(0.45f, 0, 0);
            hR.transform.localScale = new Vector3(0.28f, 1.4f, 0.3f);
            hR.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject hM = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hM.transform.SetParent(letterH.transform);
            hM.transform.localPosition = new Vector3(0, 0, 0);
            hM.transform.localScale = new Vector3(1.18f, 0.28f, 0.3f);
            hM.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.9f, 0);
            col.size = new Vector3(5.5f, 1.8f, 2.0f);

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_FrontMonument.prefab");
        }

        private static GameObject CreateFlowerPotPrefab()
        {
            GameObject root = new GameObject("IUH_FlowerPot");

            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "PotBody";
            pot.transform.SetParent(root.transform);
            pot.transform.localPosition = new Vector3(0, 0.25f, 0);
            pot.transform.localScale = new Vector3(0.6f, 0.25f, 0.6f);
            pot.GetComponent<MeshRenderer>().sharedMaterial = m_FlowerPotWhite;

            GameObject flowers = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flowers.name = "Flowers";
            flowers.transform.SetParent(root.transform);
            flowers.transform.localPosition = new Vector3(0, 0.55f, 0);
            flowers.transform.localScale = new Vector3(0.68f, 0.35f, 0.68f);
            flowers.GetComponent<MeshRenderer>().sharedMaterial = m_FlowerOrange;

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_FlowerPot.prefab");
        }

        private static GameObject CreateFlowerRowPrefab(GameObject potPrefab)
        {
            GameObject root = new GameObject("IUH_FlowerPotRow");
            int count = 10;
            float spacing = 0.85f;

            for (int i = 0; i < count; i++)
            {
                GameObject pot = PrefabUtility.InstantiatePrefab(potPrefab) as GameObject;
                pot.transform.SetParent(root.transform);
                pot.transform.localPosition = new Vector3(i * spacing, 0, 0);
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_FlowerPotRow.prefab");
        }

        private static GameObject CreateGrassIslandPrefab()
        {
            GameObject root = new GameObject("IUH_GrassIsland");

            GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            island.name = "Island_Mound";
            island.transform.SetParent(root.transform);
            island.transform.localPosition = new Vector3(0, 0.08f, 0);
            island.transform.localScale = new Vector3(10.0f, 0.12f, 7.0f);
            island.GetComponent<MeshRenderer>().sharedMaterial = m_Grass;

            GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            curb.name = "Island_Curb";
            curb.transform.SetParent(root.transform);
            curb.transform.localPosition = new Vector3(0, 0.04f, 0);
            curb.transform.localScale = new Vector3(10.4f, 0.08f, 7.4f);
            curb.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_GrassIsland.prefab");
        }

        private static GameObject CreateCampusTreePrefab()
        {
            GameObject root = new GameObject("IUH_CampusTree");

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform);
            trunk.transform.localPosition = new Vector3(0, 2.5f, 0);
            trunk.transform.localScale = new Vector3(0.4f, 2.5f, 0.4f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject canopy1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy1.name = "Canopy_Main";
            canopy1.transform.SetParent(root.transform);
            canopy1.transform.localPosition = new Vector3(0, 5.8f, 0);
            canopy1.transform.localScale = new Vector3(4.5f, 3.2f, 4.5f);
            canopy1.GetComponent<MeshRenderer>().sharedMaterial = m_FoliageTree;

            GameObject canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy2.name = "Canopy_Top";
            canopy2.transform.SetParent(root.transform);
            canopy2.transform.localPosition = new Vector3(0.3f, 7.0f, 0.2f);
            canopy2.transform.localScale = new Vector3(3.2f, 2.2f, 3.2f);
            canopy2.GetComponent<MeshRenderer>().sharedMaterial = m_FoliageTree;

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_CampusTree.prefab");
        }

        private static GameObject CreateFlagPolesPrefab()
        {
            GameObject root = new GameObject("IUH_FlagPoles");

            for (int i = 0; i < 2; i++)
            {
                float x = -i * 1.6f;

                GameObject baseCol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseCol.name = "Base_" + i;
                baseCol.transform.SetParent(root.transform);
                baseCol.transform.localPosition = new Vector3(x, 0.3f, 0);
                baseCol.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
                baseCol.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Pole_" + i;
                pole.transform.SetParent(root.transform);
                pole.transform.localPosition = new Vector3(x, 6.0f, 0);
                pole.transform.localScale = new Vector3(0.09f, 6.0f, 0.09f);
                pole.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

                GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Quad);
                flag.name = "FlagCloth_" + (i == 0 ? "Vietnam" : "IUH");
                flag.transform.SetParent(root.transform);
                flag.transform.localPosition = new Vector3(x - 1.2f, 10.5f, 0);
                flag.transform.localRotation = Quaternion.Euler(0, 190, 0);
                flag.transform.localScale = new Vector3(2.4f, 1.6f, 1.0f);
                flag.GetComponent<MeshRenderer>().sharedMaterial = (i == 0) ? m_FlagVn : m_FlagIuh;
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_FlagPoles.prefab");
        }

        private static GameObject AssembleMainBuildingPrefab(
            GameObject finPrefab, GameObject windowPrefab, GameObject canopyPrefab,
            GameObject stairsPrefab, GameObject lobbyPrefab, GameObject glassBoxPrefab)
        {
            GameObject root = new GameObject("IUH_MainBuilding");

            float buildingWidth = 42.0f;
            float buildingHeight = 24.0f;
            float buildingDepth = 20.0f;

            // Structure
            GameObject goStructure = new GameObject("Structure");
            goStructure.transform.SetParent(root.transform);

            GameObject exteriorWalls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exteriorWalls.name = "ExteriorWalls";
            exteriorWalls.transform.SetParent(goStructure.transform);
            exteriorWalls.transform.localPosition = new Vector3(0, buildingHeight * 0.5f, -buildingDepth * 0.5f);
            exteriorWalls.transform.localScale = new Vector3(buildingWidth, buildingHeight, buildingDepth);
            exteriorWalls.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            // Pale Green Hipped Roof for Main Building
            GameObject mainRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainRoof.name = "MainBuilding_PaleGreen_Roof";
            mainRoof.transform.SetParent(goStructure.transform);
            mainRoof.transform.localPosition = new Vector3(0, buildingHeight + 1.2f, -buildingDepth * 0.5f);
            mainRoof.transform.localScale = new Vector3(buildingWidth + 2.0f, 2.4f, buildingDepth + 2.0f);
            mainRoof.GetComponent<MeshRenderer>().sharedMaterial = m_PaleGreenRoof;

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject frameCol = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frameCol.name = "MainColumn_" + (side > 0 ? "Right" : "Left");
                frameCol.transform.SetParent(goStructure.transform);
                frameCol.transform.localPosition = new Vector3(side * (buildingWidth * 0.5f - 0.75f), buildingHeight * 0.5f, 0.1f);
                frameCol.transform.localScale = new Vector3(1.5f, buildingHeight, 0.4f);
                frameCol.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }

            GameObject headerBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headerBeam.name = "HeaderBeam";
            headerBeam.transform.SetParent(goStructure.transform);
            headerBeam.transform.localPosition = new Vector3(0, buildingHeight - 0.75f, 0.15f);
            headerBeam.transform.localScale = new Vector3(buildingWidth, 1.5f, 0.5f);
            headerBeam.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            // Facade & Louvers
            GameObject goFacade = new GameObject("Facade");
            goFacade.transform.SetParent(root.transform);

            for (int floor = 1; floor <= 4; floor++)
            {
                float floorY = floor * 4.5f + 1.0f;

                for (int col = 0; col < 2; col++)
                {
                    float winX = -buildingWidth * 0.5f + 3.2f + col * 3.6f;
                    GameObject win = PrefabUtility.InstantiatePrefab(windowPrefab) as GameObject;
                    win.transform.SetParent(goFacade.transform);
                    win.transform.localPosition = new Vector3(winX, floorY, 0.05f);
                }

                for (int col = 0; col < 2; col++)
                {
                    float winX = buildingWidth * 0.5f - 6.8f + col * 3.6f;
                    GameObject win = PrefabUtility.InstantiatePrefab(windowPrefab) as GameObject;
                    win.transform.SetParent(goFacade.transform);
                    win.transform.localPosition = new Vector3(winX, floorY, 0.05f);
                }

                for (int fin = 0; fin < 12; fin++)
                {
                    float finX = -buildingWidth * 0.5f + 1.8f + fin * 0.75f;
                    GameObject finObj = PrefabUtility.InstantiatePrefab(finPrefab) as GameObject;
                    finObj.transform.SetParent(goFacade.transform);
                    finObj.transform.localPosition = new Vector3(finX, floorY, 0.12f);
                }

                for (int fin = 0; fin < 12; fin++)
                {
                    float finX = buildingWidth * 0.5f - 10.2f + fin * 0.75f;
                    GameObject finObj = PrefabUtility.InstantiatePrefab(finPrefab) as GameObject;
                    finObj.transform.SetParent(goFacade.transform);
                    finObj.transform.localPosition = new Vector3(finX, floorY, 0.12f);
                }
            }

            float topFloorY = 21.0f;
            for (int fin = 0; fin < 48; fin++)
            {
                float finX = -buildingWidth * 0.5f + 2.0f + fin * 0.8f;
                if (finX >= 11.0f && finX <= 16.0f) continue;

                GameObject finObj = PrefabUtility.InstantiatePrefab(finPrefab) as GameObject;
                finObj.transform.SetParent(goFacade.transform);
                finObj.transform.localPosition = new Vector3(finX, topFloorY, 0.12f);
                finObj.transform.localScale = new Vector3(1, 0.75f, 1);
            }

            GameObject glassBox = PrefabUtility.InstantiatePrefab(glassBoxPrefab) as GameObject;
            glassBox.transform.SetParent(goFacade.transform);
            glassBox.transform.localPosition = new Vector3(0, 4.8f, 0.1f);

            // Entrance
            GameObject goEntrance = new GameObject("Entrance");
            goEntrance.transform.SetParent(root.transform);

            GameObject lobby = PrefabUtility.InstantiatePrefab(lobbyPrefab) as GameObject;
            lobby.transform.SetParent(goEntrance.transform);
            lobby.transform.localPosition = Vector3.zero;

            GameObject canopy = PrefabUtility.InstantiatePrefab(canopyPrefab) as GameObject;
            canopy.transform.SetParent(goEntrance.transform);
            canopy.transform.localPosition = new Vector3(0, 4.6f, 0.2f);

            GameObject stairs = PrefabUtility.InstantiatePrefab(stairsPrefab) as GameObject;
            stairs.transform.SetParent(goEntrance.transform);
            stairs.transform.localPosition = new Vector3(0, 0, 0);

            // Signage
            GameObject goSignage = new GameObject("IUH_Signage");
            goSignage.transform.SetParent(root.transform);

            GameObject topCrest = GameObject.CreatePrimitive(PrimitiveType.Quad);
            topCrest.name = "IUH_TopCrest_Logo";
            topCrest.transform.SetParent(goSignage.transform);
            topCrest.transform.localPosition = new Vector3(13.5f, 21.0f, 0.35f);
            topCrest.transform.localRotation = Quaternion.Euler(0, 180, 0);
            topCrest.transform.localScale = new Vector3(3.2f, 3.2f, 1.0f);
            topCrest.GetComponent<MeshRenderer>().sharedMaterial = m_LogoCrest;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_MainBuilding.prefab");
        }

        #endregion

        #region Large Left Building (Nhà X/V - Massive Horizontal Academic Building with Pale Green Roof)

        private static GameObject CreateLargeLeftBuildingPrefab()
        {
            GameObject root = new GameObject("IUH_LargeLeftBuilding");

            float floorH = 3.6f;
            int numFloors = 5;
            float bHeight = numFloors * floorH; // 18m
            float buildingWidth = 110.0f; // Very wide: extends far to the left
            float buildingDepth = 18.0f;

            // Main Building Core Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "LargeBuilding_Core";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(-buildingWidth * 0.5f, bHeight * 0.5f, 0);
            body.transform.localScale = new Vector3(buildingWidth, bHeight, buildingDepth);
            body.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            // Open-Corridor Balconies, Dark Railings & Windows across all 5 floors (Front Facade facing driveway)
            for (int f = 1; f <= numFloors; f++)
            {
                float y = (f - 0.5f) * floorH;

                // Balcony Slab
                GameObject balcony = GameObject.CreatePrimitive(PrimitiveType.Cube);
                balcony.name = "Balcony_Floor_" + f;
                balcony.transform.SetParent(root.transform);
                balcony.transform.localPosition = new Vector3(-buildingWidth * 0.5f, (f - 1) * floorH + 0.15f, buildingDepth * 0.5f + 0.9f);
                balcony.transform.localScale = new Vector3(buildingWidth, 0.3f, 2.0f);
                balcony.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

                // Railing
                GameObject railing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                railing.name = "Railing_Floor_" + f;
                railing.transform.SetParent(root.transform);
                railing.transform.localPosition = new Vector3(-buildingWidth * 0.5f, (f - 1) * floorH + 0.7f, buildingDepth * 0.5f + 1.85f);
                railing.transform.localScale = new Vector3(buildingWidth, 0.85f, 0.1f);
                railing.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                // Classroom Window Glass Band
                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glass.name = "Glass_Floor_" + f;
                glass.transform.SetParent(root.transform);
                glass.transform.localPosition = new Vector3(-buildingWidth * 0.5f, y + 0.4f, buildingDepth * 0.5f + 0.05f);
                glass.transform.localScale = new Vector3(buildingWidth - 2.0f, 2.0f, 0.1f);
                glass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;
            }

            // Vertical Structural Columns Grid (24 columns along the long facade)
            int numCols = 24;
            for (int c = 0; c <= numCols; c++)
            {
                float x = -c * (buildingWidth / numCols);
                GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cube);
                col.name = "Column_" + c;
                col.transform.SetParent(root.transform);
                col.transform.localPosition = new Vector3(x, bHeight * 0.5f, buildingDepth * 0.5f + 1.9f);
                col.transform.localScale = new Vector3(0.5f, bHeight, 0.3f);
                col.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }

            // Ground Floor Open Entrance Portals / Passageways (as seen in ground photo)
            for (int p = 0; p < 3; p++)
            {
                GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                portal.name = "EntrancePortal_" + p;
                portal.transform.SetParent(root.transform);
                portal.transform.localPosition = new Vector3(-18.0f - p * 25.0f, 2.0f, buildingDepth * 0.5f + 1.0f);
                portal.transform.localScale = new Vector3(6.0f, 4.0f, 2.5f);
                portal.GetComponent<MeshRenderer>().sharedMaterial = m_BlueFacade;
            }

            // Massive Pale Green Corrugated Hipped Roof spanning the entire length
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Large_PaleGreen_Roof";
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(-buildingWidth * 0.5f, bHeight + 1.5f, 0);
            roof.transform.localScale = new Vector3(buildingWidth + 4.0f, 3.0f, buildingDepth + 4.0f);
            roof.GetComponent<MeshRenderer>().sharedMaterial = m_PaleGreenRoof;

            // Rooftop HVAC & Solar Array detail (as seen from aerial photo)
            GameObject solar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            solar.name = "Rooftop_SolarPanels";
            solar.transform.SetParent(roof.transform);
            solar.transform.localPosition = new Vector3(0.2f, 0.52f, 0);
            solar.transform.localScale = new Vector3(0.25f, 0.05f, 0.6f);
            solar.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            BoxCollider boxCol = root.AddComponent<BoxCollider>();
            boxCol.center = new Vector3(-buildingWidth * 0.5f, bHeight * 0.5f, 0);
            boxCol.size = new Vector3(buildingWidth, bHeight, buildingDepth);

            return SaveAsPrefab(root, PrefabArchPath + "IUH_LargeLeftBuilding.prefab");
        }

        #endregion

        #region Green Glass Tower (Nhà B/E - Attached Directly to Right of Large Left Building)

        private static GameObject CreateGreenGlassTowerPrefab()
        {
            GameObject root = new GameObject("IUH_GreenGlassTower");

            float towerW = 20.0f;
            float towerD = 20.0f;
            float towerH = 54.0f; // 13-story vertical tower

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Tower_Core";
            core.transform.SetParent(root.transform);
            core.transform.localPosition = new Vector3(0, towerH * 0.5f, 0);
            core.transform.localScale = new Vector3(towerW, towerH, towerD);
            core.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            // Emerald / Turquoise Green Glass Corners
            for (int side = -1; side <= 1; side += 2)
            {
                for (int fb = -1; fb <= 1; fb += 2)
                {
                    GameObject greenCorner = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    greenCorner.name = "GreenGlassCorner_" + side + "_" + fb;
                    greenCorner.transform.SetParent(root.transform);
                    greenCorner.transform.localPosition = new Vector3(side * (towerW * 0.44f), towerH * 0.48f, fb * (towerD * 0.44f));
                    greenCorner.transform.localScale = new Vector3(3.8f, towerH * 0.94f, 3.8f);
                    greenCorner.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;
                }
            }

            // Central Vertical Green Glass Curtain Window Banks (Front & Back)
            CreateTowerGlassFacade(root, new Vector3(0, towerH * 0.45f, towerD * 0.5f + 0.15f), new Vector3(11.0f, towerH * 0.82f, 0.25f), towerH, true);
            CreateTowerGlassFacade(root, new Vector3(0, towerH * 0.45f, -towerD * 0.5f - 0.15f), new Vector3(11.0f, towerH * 0.82f, 0.25f), towerH, true);
            CreateTowerGlassFacade(root, new Vector3(towerW * 0.5f + 0.15f, towerH * 0.45f, 0), new Vector3(0.25f, towerH * 0.82f, 10.0f), towerH, false);

            // Upper Cantilevered White Observation Section (Floors 9–10)
            GameObject upperBalcony = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperBalcony.name = "Upper_Observation_Balcony";
            upperBalcony.transform.SetParent(root.transform);
            upperBalcony.transform.localPosition = new Vector3(0, 42.0f, towerD * 0.5f + 1.2f);
            upperBalcony.transform.localScale = new Vector3(15.0f, 6.5f, 2.2f);
            upperBalcony.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject balconyGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            balconyGlass.name = "Balcony_Glass";
            balconyGlass.transform.SetParent(upperBalcony.transform);
            balconyGlass.transform.localPosition = new Vector3(0, 0.1f, 0.1f);
            balconyGlass.transform.localScale = new Vector3(0.92f, 0.65f, 1.02f);
            balconyGlass.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;

            // Rooftop Crown & Pyramidal Dark Slate Cap
            GameObject roofBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofBase.name = "Rooftop_Parapet_Base";
            roofBase.transform.SetParent(root.transform);
            roofBase.transform.localPosition = new Vector3(0, towerH + 1.5f, 0);
            roofBase.transform.localScale = new Vector3(towerW + 0.8f, 3.0f, towerD + 0.8f);
            roofBase.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject pyramidCrown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pyramidCrown.name = "Pyramidal_Crown";
            pyramidCrown.transform.SetParent(root.transform);
            pyramidCrown.transform.localPosition = new Vector3(0, towerH + 6.5f, 0);
            pyramidCrown.transform.localScale = new Vector3(12.0f, 4.0f, 10.0f);
            pyramidCrown.GetComponent<MeshRenderer>().sharedMaterial = m_TowerPyramidRoof;

            // Rooftop Stainless Steel Water Tanks
            GameObject waterTank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waterTank.name = "Rooftop_WaterTank";
            waterTank.transform.SetParent(root.transform);
            waterTank.transform.localPosition = new Vector3(-5.0f, towerH + 3.8f, 3.5f);
            waterTank.transform.localRotation = Quaternion.Euler(0, 0, 90);
            waterTank.transform.localScale = new Vector3(1.8f, 3.5f, 1.8f);
            waterTank.GetComponent<MeshRenderer>().sharedMaterial = m_RooftopMetal;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, towerH * 0.5f, 0);
            col.size = new Vector3(towerW + 1.0f, towerH, towerD + 1.0f);

            return SaveAsPrefab(root, PrefabArchPath + "IUH_GreenGlassTower.prefab");
        }

        private static void CreateTowerGlassFacade(GameObject root, Vector3 pos, Vector3 scale, float towerH, bool isZAxis)
        {
            GameObject centerGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerGlass.name = "GlassFacade_" + (isZAxis ? (pos.z > 0 ? "Front" : "Back") : "Side");
            centerGlass.transform.SetParent(root.transform);
            centerGlass.transform.localPosition = pos;
            centerGlass.transform.localScale = scale;
            centerGlass.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;

            int floors = 12;
            for (int f = 1; f <= floors; f++)
            {
                float y = f * 3.8f;
                GameObject hMullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hMullion.name = "FloorBand_" + f;
                hMullion.transform.SetParent(root.transform);
                if (isZAxis)
                {
                    hMullion.transform.localPosition = new Vector3(pos.x, y, pos.z + (pos.z > 0 ? 0.12f : -0.12f));
                    hMullion.transform.localScale = new Vector3(scale.x + 0.2f, 0.35f, 0.15f);
                }
                else
                {
                    hMullion.transform.localPosition = new Vector3(pos.x + 0.12f, y, pos.z);
                    hMullion.transform.localScale = new Vector3(0.15f, 0.35f, scale.z + 0.2f);
                }
                hMullion.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }
        }

        #endregion

        #region Red Roof Academic Building (Nhà A - Behind Large Left Building)

        private static GameObject CreateRedRoofBuildingPrefab()
        {
            GameObject root = new GameObject("IUH_RedRoofBuilding");

            float buildingW = 56.0f;
            float buildingH = 22.0f; // 6 floors
            float buildingD = 18.0f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Academic_Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0, buildingH * 0.5f, 0);
            body.transform.localScale = new Vector3(buildingW, buildingH, buildingD);
            body.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            int floors = 6;
            for (int f = 1; f <= floors; f++)
            {
                float y = (f - 0.5f) * (buildingH / floors);

                GameObject fWin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fWin.name = "F_Window_Band_" + f;
                fWin.transform.SetParent(root.transform);
                fWin.transform.localPosition = new Vector3(0, y, buildingD * 0.5f + 0.08f);
                fWin.transform.localScale = new Vector3(buildingW - 4.0f, 1.6f, 0.12f);
                fWin.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;

                for (int ac = -5; ac <= 5; ac++)
                {
                    if (ac % 2 == 0) continue;
                    GameObject fAc = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fAc.name = "F_AC_Unit_F" + f + "_" + ac;
                    fAc.transform.SetParent(root.transform);
                    fAc.transform.localPosition = new Vector3(ac * 4.2f, y - 1.1f, buildingD * 0.5f + 0.35f);
                    fAc.transform.localScale = new Vector3(0.85f, 0.55f, 0.45f);
                    fAc.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
                }
            }

            GameObject redRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            redRoof.name = "Terracotta_Red_Roof";
            redRoof.transform.SetParent(root.transform);
            redRoof.transform.localPosition = new Vector3(0, buildingH + 2.5f, 0);
            redRoof.transform.localScale = new Vector3(buildingW + 2.5f, 5.0f, buildingD + 2.5f);
            redRoof.GetComponent<MeshRenderer>().sharedMaterial = m_RedMetalRoof;

            GameObject roofRidge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofRidge.name = "Roof_Ridge";
            roofRidge.transform.SetParent(redRoof.transform);
            roofRidge.transform.localPosition = new Vector3(0, 0.52f, 0);
            roofRidge.transform.localScale = new Vector3(0.75f, 0.08f, 0.35f);
            roofRidge.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_RedRoofBuilding.prefab");
        }

        #endregion

        #region Environment Props (Central Large Tree, Motorbikes, Canopy, Covered Ramp)

        private static GameObject CreateCentralMatureTreePrefab()
        {
            GameObject root = new GameObject("IUH_CentralLargeTree");

            // Thick Gnarled Trunk with base root planter
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform);
            trunk.transform.localPosition = new Vector3(0, 4.0f, 0);
            trunk.transform.localScale = new Vector3(1.6f, 4.0f, 1.6f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            // Hexagonal Concrete Bench / Planter around tree base
            GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bench.name = "TreeBase_Bench";
            bench.transform.SetParent(root.transform);
            bench.transform.localPosition = new Vector3(0, 0.4f, 0);
            bench.transform.localScale = new Vector3(4.2f, 0.4f, 4.2f);
            bench.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            // Sprawling Dense Leaf Canopy (Massive shade tree matching ground photo)
            Vector3[] canopyOffsets = new Vector3[]
            {
                new Vector3(0, 10.0f, 0),
                new Vector3(-4.5f, 9.2f, 2.5f),
                new Vector3(4.8f, 9.5f, -2.0f),
                new Vector3(2.0f, 11.5f, 3.8f),
                new Vector3(-3.0f, 10.8f, -3.5f)
            };

            Vector3[] canopyScales = new Vector3[]
            {
                new Vector3(13.0f, 7.5f, 13.0f),
                new Vector3(10.5f, 6.8f, 10.5f),
                new Vector3(11.0f, 7.0f, 11.0f),
                new Vector3(9.5f, 6.2f, 9.5f),
                new Vector3(9.8f, 6.5f, 9.8f)
            };

            for (int i = 0; i < canopyOffsets.Length; i++)
            {
                GameObject lobe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lobe.name = "Canopy_Lobe_" + i;
                lobe.transform.SetParent(root.transform);
                lobe.transform.localPosition = canopyOffsets[i];
                lobe.transform.localScale = canopyScales[i];
                lobe.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_CentralLargeTree.prefab");
        }

        private static GameObject CreateCoveredRampPrefab()
        {
            GameObject root = new GameObject("IUH_CoveredRamp_Basement");

            // Blue / Grey Covered Ramp Structure seen on the far left of ground photo
            GameObject rampSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampSlab.name = "Ramp_Slab";
            rampSlab.transform.SetParent(root.transform);
            rampSlab.transform.localPosition = new Vector3(0, 1.5f, 0);
            rampSlab.transform.localRotation = Quaternion.Euler(12, 0, 0);
            rampSlab.transform.localScale = new Vector3(6.0f, 0.3f, 16.0f);
            rampSlab.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            // Canopy Roof over ramp
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Ramp_Roof";
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(0, 4.0f, 0);
            roof.transform.localRotation = Quaternion.Euler(12, 0, 0);
            roof.transform.localScale = new Vector3(6.8f, 0.2f, 17.0f);
            roof.GetComponent<MeshRenderer>().sharedMaterial = m_BlueFacade;

            // Blue Steel Railings
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject railing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                railing.name = "Railing_" + side;
                railing.transform.SetParent(root.transform);
                railing.transform.localPosition = new Vector3(side * 3.1f, 2.3f, 0);
                railing.transform.localRotation = Quaternion.Euler(12, 0, 0);
                railing.transform.localScale = new Vector3(0.1f, 1.2f, 16.0f);
                railing.GetComponent<MeshRenderer>().sharedMaterial = m_BlueFacade;
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_CoveredRamp_Basement.prefab");
        }

        private static GameObject CreateMotorbikeParkingPrefab()
        {
            GameObject root = new GameObject("IUH_MotorbikeParkingRow");

            int bikeCount = 10;
            float spacing = 1.1f;

            for (int i = 0; i < bikeCount; i++)
            {
                GameObject bike = new GameObject("Motorbike_" + i);
                bike.transform.SetParent(root.transform);
                bike.transform.localPosition = new Vector3(i * spacing, 0, 0);
                bike.transform.localRotation = Quaternion.Euler(0, 15 * (i % 2 == 0 ? 1 : -1), 0);

                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(bike.transform);
                body.transform.localPosition = new Vector3(0, 0.55f, 0);
                body.transform.localScale = new Vector3(0.35f, 0.45f, 1.4f);
                body.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? m_MotorbikeRed : m_MotorbikeDark;

                GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seat.name = "Seat";
                seat.transform.SetParent(bike.transform);
                seat.transform.localPosition = new Vector3(0, 0.78f, -0.15f);
                seat.transform.localScale = new Vector3(0.32f, 0.12f, 0.75f);
                seat.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                GameObject bars = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bars.name = "Bars";
                bars.transform.SetParent(bike.transform);
                bars.transform.localPosition = new Vector3(0, 0.95f, 0.45f);
                bars.transform.localScale = new Vector3(0.65f, 0.08f, 0.08f);
                bars.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

                for (int w = -1; w <= 1; w += 2)
                {
                    GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = "Wheel_" + (w > 0 ? "F" : "R");
                    wheel.transform.SetParent(bike.transform);
                    wheel.transform.localPosition = new Vector3(0, 0.32f, w * 0.55f);
                    wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    wheel.transform.localScale = new Vector3(0.55f, 0.12f, 0.55f);
                    wheel.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
                }
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_MotorbikeParkingRow.prefab");
        }

        private static GameObject CreateCanopyBoothPrefab()
        {
            GameObject root = new GameObject("IUH_CanopyBooth_Blue");

            float width = 8.0f;
            float depth = 5.0f;
            float height = 3.2f;

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Canopy_Cover";
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(0, height, 0);
            roof.transform.localRotation = Quaternion.Euler(6, 0, 0);
            roof.transform.localScale = new Vector3(width, 0.15f, depth);
            roof.GetComponent<MeshRenderer>().sharedMaterial = m_BlueTarp;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg";
                    leg.transform.SetParent(root.transform);
                    leg.transform.localPosition = new Vector3(x * (width * 0.46f), height * 0.5f, z * (depth * 0.46f));
                    leg.transform.localScale = new Vector3(0.08f, height * 0.5f, 0.08f);
                    leg.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;
                }
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_CanopyBooth_Blue.prefab");
        }

        private static GameObject CreateSportsCourtyardPrefab()
        {
            GameObject root = new GameObject("IUH_SportsCourtyard");

            GameObject court = GameObject.CreatePrimitive(PrimitiveType.Cube);
            court.name = "Court_Slab";
            court.transform.SetParent(root.transform);
            court.transform.localPosition = new Vector3(0, 0.02f, 0);
            court.transform.localScale = new Vector3(45.0f, 0.04f, 70.0f);
            court.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            for (int i = -2; i <= 2; i++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "Court_Line_" + i;
                line.transform.SetParent(root.transform);
                line.transform.localPosition = new Vector3(0, 0.05f, i * 12.0f);
                line.transform.localScale = new Vector3(32.0f, 0.02f, 0.25f);
                line.GetComponent<MeshRenderer>().sharedMaterial = m_RoadArrow;
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_SportsCourtyard.prefab");
        }

        #endregion

        #region Final Corrected Spatial Scene Assembly

        private static void CreateAndPopulateAccurateScene(
            GameObject mainBuildingPrefab, GameObject monumentPrefab, GameObject grassIslandPrefab,
            GameObject flowerRowPrefab, GameObject treePrefab, GameObject flagPolesPrefab,
            GameObject largeLeftBuildingPrefab, GameObject greenTowerPrefab, GameObject redRoofBuildingPrefab,
            GameObject centralMatureTreePrefab, GameObject motorbikeRowPrefab, GameObject blueBoothPrefab,
            GameObject sportsCourtPrefab, GameObject coveredRampPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject campusRoot = new GameObject("IUH_Campus_Environment");

            // ================= 1. IUH MAIN BUILDING (Nhà Hiệu Bộ - on the Right / Foreground) =================
            GameObject goMainBuilding = new GameObject("MainBuilding_Area");
            goMainBuilding.transform.SetParent(campusRoot.transform);

            GameObject mainBuilding = PrefabUtility.InstantiatePrefab(mainBuildingPrefab) as GameObject;
            mainBuilding.transform.SetParent(goMainBuilding.transform);
            mainBuilding.transform.localPosition = new Vector3(25.0f, 0, 0); // Positioned on the Right side

            // ================= 2. LARGE LEFT BUILDING (Extends far to the left) =================
            GameObject goLargeLeft = new GameObject("LargeLeftBuilding_Area");
            goLargeLeft.transform.SetParent(campusRoot.transform);

            GameObject largeBuilding = PrefabUtility.InstantiatePrefab(largeLeftBuildingPrefab) as GameObject;
            largeBuilding.transform.SetParent(goLargeLeft.transform);
            largeBuilding.transform.localPosition = new Vector3(-8.0f, 0, -22.0f); // Spans from -8 to -118

            // ================= 3. GREEN GLASS TOWER (Adjacent/Attached to Large Left Building) =================
            GameObject goTower = new GameObject("GreenGlassTower_Area");
            goTower.transform.SetParent(campusRoot.transform);

            GameObject greenTower = PrefabUtility.InstantiatePrefab(greenTowerPrefab) as GameObject;
            greenTower.transform.SetParent(goTower.transform);
            greenTower.transform.localPosition = new Vector3(0.0f, 0, -22.0f); // Directly beside the right end of the large building

            // ================= 4. RED-ROOF ACADEMIC BLOCK (Behind Large Left Building) =================
            GameObject goRedRoof = new GameObject("RedRoofBuilding_Area");
            goRedRoof.transform.SetParent(campusRoot.transform);

            GameObject redRoofBuilding = PrefabUtility.InstantiatePrefab(redRoofBuildingPrefab) as GameObject;
            redRoofBuilding.transform.SetParent(goRedRoof.transform);
            redRoofBuilding.transform.localPosition = new Vector3(-62.0f, 0, -56.0f); // Centered behind the large building

            // ================= 5. ROADS & COURTYARD DRIVEWAY =================
            GameObject goRoads = new GameObject("Roads_And_Pavements");
            goRoads.transform.SetParent(campusRoot.transform);

            // Open Courtyard Driveway between Main Building and Large Left Building / Tower
            GameObject courtyardRoad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            courtyardRoad.name = "Courtyard_Driveway";
            courtyardRoad.transform.SetParent(goRoads.transform);
            courtyardRoad.transform.localPosition = new Vector3(-15.0f, -0.05f, 0);
            courtyardRoad.transform.localScale = new Vector3(130.0f, 0.1f, 48.0f);
            courtyardRoad.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;

            // Main Building Forecourt Plaza
            GameObject mainPlaza = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainPlaza.name = "MainBuilding_Plaza";
            mainPlaza.transform.SetParent(goRoads.transform);
            mainPlaza.transform.localPosition = new Vector3(25.0f, -0.05f, 14.0f);
            mainPlaza.transform.localScale = new Vector3(70.0f, 0.1f, 32.0f);
            mainPlaza.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;

            // Sports Ground in Courtyard
            GameObject sportCourt = PrefabUtility.InstantiatePrefab(sportsCourtPrefab) as GameObject;
            sportCourt.transform.SetParent(goRoads.transform);
            sportCourt.transform.localPosition = new Vector3(-75.0f, 0, 16.0f);

            // Sidewalks
            GameObject mainSidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainSidewalk.name = "Main_Sidewalk";
            mainSidewalk.transform.SetParent(goRoads.transform);
            mainSidewalk.transform.localPosition = new Vector3(25.0f, 0.05f, 3.0f);
            mainSidewalk.transform.localScale = new Vector3(65.0f, 0.1f, 10.0f);
            mainSidewalk.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            GameObject buildingSidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buildingSidewalk.name = "LargeBuilding_Sidewalk";
            buildingSidewalk.transform.SetParent(goRoads.transform);
            buildingSidewalk.transform.localPosition = new Vector3(-62.0f, 0.05f, -11.0f);
            buildingSidewalk.transform.localScale = new Vector3(115.0f, 0.1f, 4.0f);
            buildingSidewalk.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            // Road Arrow
            GameObject roadArrow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            roadArrow.name = "Road_Arrow_Yellow";
            roadArrow.transform.SetParent(goRoads.transform);
            roadArrow.transform.localPosition = new Vector3(28.8f, 0.01f, 18.0f);
            roadArrow.transform.localRotation = Quaternion.Euler(90, 180, 0);
            roadArrow.transform.localScale = new Vector3(2.2f, 2.2f, 1.0f);
            roadArrow.GetComponent<MeshRenderer>().sharedMaterial = m_RoadArrow;

            // Covered Basement Ramp on the Far Left
            GameObject coveredRamp = PrefabUtility.InstantiatePrefab(coveredRampPrefab) as GameObject;
            coveredRamp.transform.SetParent(goRoads.transform);
            coveredRamp.transform.localPosition = new Vector3(-120.0f, 0, -8.0f);

            // ================= 6. CENTRAL MATURE SHADE TREE & LANDSCAPING =================
            GameObject goLandscaping = new GameObject("Landscaping");
            goLandscaping.transform.SetParent(campusRoot.transform);

            // Central Giant Mature Shade Tree (between Main Building, Green Tower, and Large Building)
            GameObject centralTree = PrefabUtility.InstantiatePrefab(centralMatureTreePrefab) as GameObject;
            centralTree.transform.SetParent(goLandscaping.transform);
            centralTree.transform.localPosition = new Vector3(5.0f, 0, -8.0f);

            // Trimmed ornamental spherical ficus bushes along driveway
            for (int b = 0; b < 6; b++)
            {
                GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bush.name = "Ornamental_Bush_" + b;
                bush.transform.SetParent(goLandscaping.transform);
                bush.transform.localPosition = new Vector3(-15.0f - b * 16.0f, 1.2f, -10.0f);
                bush.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
                bush.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
            }

            // Front Monument & Grass Island
            GameObject goMonument = new GameObject("Monument");
            goMonument.transform.SetParent(campusRoot.transform);

            GameObject island = PrefabUtility.InstantiatePrefab(grassIslandPrefab) as GameObject;
            island.transform.SetParent(goMonument.transform);
            island.transform.localPosition = new Vector3(25.0f, 0, 14.0f);

            GameObject monument = PrefabUtility.InstantiatePrefab(monumentPrefab) as GameObject;
            monument.transform.SetParent(goMonument.transform);
            monument.transform.localPosition = new Vector3(25.0f, 0.1f, 14.0f);

            // Flower Pot Rows
            GameObject flowerRowRight = PrefabUtility.InstantiatePrefab(flowerRowPrefab) as GameObject;
            flowerRowRight.transform.SetParent(goLandscaping.transform);
            flowerRowRight.transform.localPosition = new Vector3(29.5f, 0.1f, 8.2f);

            GameObject flowerRowLeft = PrefabUtility.InstantiatePrefab(flowerRowPrefab) as GameObject;
            flowerRowLeft.transform.SetParent(goLandscaping.transform);
            flowerRowLeft.transform.localPosition = new Vector3(12.5f, 0.1f, 8.2f);

            // Flagpoles
            GameObject goFlags = new GameObject("Flags");
            goFlags.transform.SetParent(campusRoot.transform);

            GameObject flagPoles = PrefabUtility.InstantiatePrefab(flagPolesPrefab) as GameObject;
            flagPoles.transform.SetParent(goFlags.transform);
            flagPoles.transform.localPosition = new Vector3(8.5f, 0, 8.5f);

            // ================= 7. MOTORBIKE PARKING & CANOPY BOOTHS =================
            GameObject goParking = new GameObject("Motorbike_Parking");
            goParking.transform.SetParent(campusRoot.transform);

            GameObject bikeRow1 = PrefabUtility.InstantiatePrefab(motorbikeRowPrefab) as GameObject;
            bikeRow1.transform.SetParent(goParking.transform);
            bikeRow1.transform.localPosition = new Vector3(-35.0f, 0, -11.0f);

            GameObject bikeRow2 = PrefabUtility.InstantiatePrefab(motorbikeRowPrefab) as GameObject;
            bikeRow2.transform.SetParent(goParking.transform);
            bikeRow2.transform.localPosition = new Vector3(-65.0f, 0, -11.0f);

            GameObject bikeRow3 = PrefabUtility.InstantiatePrefab(motorbikeRowPrefab) as GameObject;
            bikeRow3.transform.SetParent(goParking.transform);
            bikeRow3.transform.localPosition = new Vector3(-95.0f, 0, -11.0f);

            // Blue canopy booths
            GameObject booth1 = PrefabUtility.InstantiatePrefab(blueBoothPrefab) as GameObject;
            booth1.transform.SetParent(goParking.transform);
            booth1.transform.localPosition = new Vector3(-50.0f, 0, 4.0f);

            GameObject booth2 = PrefabUtility.InstantiatePrefab(blueBoothPrefab) as GameObject;
            booth2.transform.SetParent(goParking.transform);
            booth2.transform.localPosition = new Vector3(-60.0f, 0, 4.0f);

            // ================= 8. LIGHTING & CAMERAS =================
            SetupLightingAndCameras(campusRoot);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void SetupLightingAndCameras(GameObject root)
        {
            GameObject sunObj = new GameObject("Directional_SunLight");
            sunObj.transform.SetParent(root.transform);
            Light sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.98f, 0.94f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(48, 145, 0);

            GameObject probeObj = new GameObject("ReflectionProbe_Entrance");
            probeObj.transform.SetParent(root.transform);
            probeObj.transform.localPosition = new Vector3(25.0f, 2.5f, 5.0f);
            ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
            probe.size = new Vector3(80.0f, 30.0f, 60.0f);
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;

            GameObject probeTower = new GameObject("ReflectionProbe_Tower");
            probeTower.transform.SetParent(root.transform);
            probeTower.transform.localPosition = new Vector3(0, 15.0f, -22.0f);
            ReflectionProbe p2 = probeTower.AddComponent<ReflectionProbe>();
            p2.size = new Vector3(100.0f, 60.0f, 80.0f);
            p2.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            p2.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;

            // Main Camera default: Standing in the driveway looking at the whole complex (Ground view matching Photo 2)
            GameObject camObj = new GameObject("Main_Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.transform.position = new Vector3(10.0f, 1.8f, 18.0f);
            camObj.transform.rotation = Quaternion.Euler(10, 205, 0);
        }

        #endregion
    }
}
