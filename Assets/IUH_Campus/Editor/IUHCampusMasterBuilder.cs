using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    public static class IUHCampusMasterBuilder
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
        private static Material m_FlagYouth;
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

        private static Material m_AcadWhite;
        private static Material m_AcadGlass;
        private static Material m_AcadTurquoise;
        private static Material m_AcadDarkFrame;
        private static Material m_AcadConcrete;
        private static Material m_AcadTopSign;

        private static Material m_GateStone;
        private static Material m_GateBlueRailing;
        private static Material m_GateWelcomeBanner;
        private static Material m_GateBooth;
        private static Material m_GateBoothRoof;
        private static Material m_GateEventFlag;
        private static Material m_GateRoad;
        private static Material m_GateCurb;
        private static Material m_GateGoldText;

        [MenuItem("IUH Campus/Build Complete Master Campus")]
        public static void BuildMasterCampus()
        {
            IUHTextureGenerator.GenerateAllTextures();
            IUHMaterialGenerator.GenerateAllMaterials();
            LoadMaterials();
            EnsureDirectories();

            GameObject mainBuildingPrefab = CreateMainBuildingPrefab();
            GameObject greenTowerPrefab = CreateGreenGlassTowerPrefab();
            GameObject leftPaleWingPrefab = CreateLeftPaleWingPrefab();
            GameObject redRoofPrefab = CreateRedRoofBuildingPrefab();
            GameObject rightLargeAcademicPrefab = CreateRightLargeAcademicBuildingPrefab();
            GameObject backTowerPrefab = CreateBackCentralTowerPrefab();
            GameObject treeClusterPrefab = CreateCentralTreeClusterPrefab();
            GameObject sportsCourtyardPrefab = CreateSportsCourtyardPrefab();
            GameObject motorbikeRowPrefab = CreateMotorbikeParkingPrefab();
            GameObject blueBoothPrefab = CreateCanopyBoothPrefab();
            GameObject coveredRampPrefab = CreateCoveredRampPrefab();
            GameObject monumentPrefab = CreateFrontMonumentPrefab();
            GameObject flagPolesPrefab = CreateFlagPolesPrefab();
            GameObject mainEntrancePrefab = CreateMainEntrancePrefab();

            AssembleMasterScene(
                mainBuildingPrefab, greenTowerPrefab, leftPaleWingPrefab, redRoofPrefab,
                rightLargeAcademicPrefab, backTowerPrefab, treeClusterPrefab, sportsCourtyardPrefab,
                motorbikeRowPrefab, blueBoothPrefab, coveredRampPrefab, monumentPrefab, flagPolesPrefab,
                mainEntrancePrefab
            );

            Debug.Log("[IUH] Entire Master IUH Campus constructed with authoritative aerial layout and ground details!");
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
            m_FlagYouth = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Flag_YouthUnion.mat");
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

        #region Prefabs

        private static GameObject CreateMainBuildingPrefab()
        {
            GameObject root = new GameObject("IUH_MainBuilding");

            const float bW = 44.0f;
            const float bH = 22.0f;
            const float bD = 20.0f;
            const float floorH = bH / 5.0f;
            const float frontZ = bD * 0.5f;

            GameObject goStructure = new GameObject("Structure");
            goStructure.transform.SetParent(root.transform);

            GameObject mainFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainFrame.name = "MainFrame";
            mainFrame.transform.SetParent(goStructure.transform);
            mainFrame.transform.localPosition = new Vector3(0, bH * 0.5f, 0);
            mainFrame.transform.localScale = new Vector3(bW, bH, bD);
            mainFrame.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject goFloors = new GameObject("Floors");
            goFloors.transform.SetParent(goStructure.transform);
            for (int f = 1; f <= 5; f++)
            {
                float fy = f * floorH;
                GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = "FloorSlab_F" + f;
                slab.transform.SetParent(goFloors.transform);
                slab.transform.localPosition = new Vector3(0, fy - 0.15f, 0);
                slab.transform.localScale = new Vector3(bW + 0.1f, 0.3f, bD + 0.1f);
                slab.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;
            }

            GameObject goCols = new GameObject("Columns");
            goCols.transform.SetParent(goStructure.transform);
            for (int c = -3; c <= 3; c++)
            {
                float cx = c * 6.2f;
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "InteriorColumn_" + c;
                pillar.transform.SetParent(goCols.transform);
                pillar.transform.localPosition = new Vector3(cx, floorH * 1.05f, frontZ - 1.2f);
                pillar.transform.localScale = new Vector3(0.55f, floorH * 1.05f, 0.55f);
                pillar.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }

            GameObject roof = new GameObject("IUH_MainBuilding_Roof");
            roof.transform.SetParent(goStructure.transform);

            GameObject roofSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofSlab.name = "Roof_Slab_Overhang";
            roofSlab.transform.SetParent(roof.transform);
            roofSlab.transform.localPosition = new Vector3(0, bH + 0.45f, 0.6f);
            roofSlab.transform.localScale = new Vector3(bW + 1.8f, 0.9f, bD + 2.4f);
            roofSlab.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject roofFascia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofFascia.name = "Roof_Fascia_Front";
            roofFascia.transform.SetParent(roof.transform);
            roofFascia.transform.localPosition = new Vector3(0, bH + 0.45f, frontZ + 1.85f);
            roofFascia.transform.localScale = new Vector3(bW + 2.0f, 1.0f, 0.25f);
            roofFascia.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject roofSoffit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofSoffit.name = "Roof_Soffit";
            roofSoffit.transform.SetParent(roof.transform);
            roofSoffit.transform.localPosition = new Vector3(0, bH - 0.05f, frontZ + 0.9f);
            roofSoffit.transform.localScale = new Vector3(bW + 1.6f, 0.12f, 1.8f);
            roofSoffit.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject goFacade = new GameObject("Facade");
            goFacade.transform.SetParent(root.transform);

            GameObject goGlassPanels = new GameObject("GlassPanels");
            goGlassPanels.transform.SetParent(goFacade.transform);

            float glassH = floorH * 2.12f;
            GameObject lowerGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowerGlass.name = "Lower_GlassCurtainWall";
            lowerGlass.transform.SetParent(goGlassPanels.transform);
            lowerGlass.transform.localPosition = new Vector3(0, glassH * 0.5f, frontZ + 0.12f);
            lowerGlass.transform.localScale = new Vector3(bW - 0.2f, glassH, 0.18f);
            lowerGlass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassCurtain;

            for (int m = 0; m <= 22; m++)
            {
                float mx = -bW * 0.5f + 0.8f + m * ((bW - 1.6f) / 22.0f);
                GameObject mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mullion.name = "Mullion_Lower_" + m;
                mullion.transform.SetParent(goGlassPanels.transform);
                mullion.transform.localPosition = new Vector3(mx, glassH * 0.5f, frontZ + 0.24f);
                mullion.transform.localScale = new Vector3(0.09f, glassH, 0.14f);
                mullion.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }

            float[] transoms = new float[] { 2.4f, floorH, floorH + 2.0f, glassH };
            foreach (float ty in transoms)
            {
                GameObject transom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                transom.name = "Transom_Y_" + ty.ToString("F1");
                transom.transform.SetParent(goGlassPanels.transform);
                transom.transform.localPosition = new Vector3(0, ty, frontZ + 0.24f);
                transom.transform.localScale = new Vector3(bW, 0.12f, 0.14f);
                transom.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }

            GameObject mezzPlanter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mezzPlanter.name = "Mezzanine_IndoorPlanter";
            mezzPlanter.transform.SetParent(goGlassPanels.transform);
            mezzPlanter.transform.localPosition = new Vector3(0, floorH + 0.35f, frontZ - 0.45f);
            mezzPlanter.transform.localScale = new Vector3(bW - 2.0f, 0.6f, 0.8f);
            mezzPlanter.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;

            GameObject sillBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sillBand.name = "SillBand_F2_F3";
            sillBand.transform.SetParent(goFacade.transform);
            sillBand.transform.localPosition = new Vector3(0, glassH + 0.25f, frontZ + 0.22f);
            sillBand.transform.localScale = new Vector3(bW + 0.8f, 0.55f, 0.45f);
            sillBand.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject goWindows = new GameObject("Windows");
            goWindows.transform.SetParent(goFacade.transform);

            float[] upperFloorYs = new float[] { floorH * 2.5f, floorH * 3.5f, floorH * 4.5f };
            float[] flankWindowXs = new float[] { -18.5f, -14.2f, -9.8f, 9.8f, 14.2f, 18.5f };

            for (int fi = 0; fi < upperFloorYs.Length; fi++)
            {
                float wy = upperFloorYs[fi];
                foreach (float wx in flankWindowXs)
                {
                    GameObject winModule = new GameObject("IUH_MainBuilding_WindowModule_" + wx.ToString("F1") + "_F" + (fi + 3));
                    winModule.transform.SetParent(goWindows.transform);
                    winModule.transform.localPosition = new Vector3(wx, wy, frontZ + 0.08f);

                    GameObject winGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    winGlass.name = "GlassPane";
                    winGlass.transform.SetParent(winModule.transform);
                    winGlass.transform.localPosition = Vector3.zero;
                    winGlass.transform.localScale = new Vector3(2.4f, 2.8f, 0.1f);
                    winGlass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;

                    GameObject winFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    winFrame.name = "DarkFrame";
                    winFrame.transform.SetParent(winModule.transform);
                    winFrame.transform.localPosition = new Vector3(0, 0, 0.06f);
                    winFrame.transform.localScale = new Vector3(2.55f, 2.95f, 0.08f);
                    winFrame.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    GameObject sash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sash.name = "SashLine";
                    sash.transform.SetParent(winModule.transform);
                    sash.transform.localPosition = new Vector3(0, 0, 0.12f);
                    sash.transform.localScale = new Vector3(0.08f, 2.8f, 0.06f);
                    sash.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;
                }
            }

            float[] f5CenterXs = new float[] { -5.8f, -2.0f, 2.0f, 5.8f };
            foreach (float cx in f5CenterXs)
            {
                GameObject winF5 = new GameObject("IUH_MainBuilding_WindowModule_F5Center_" + cx.ToString("F1"));
                winF5.transform.SetParent(goWindows.transform);
                winF5.transform.localPosition = new Vector3(cx, floorH * 4.5f, frontZ + 0.08f);

                GameObject winGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winGlass.name = "GlassPane";
                winGlass.transform.SetParent(winF5.transform);
                winGlass.transform.localPosition = Vector3.zero;
                winGlass.transform.localScale = new Vector3(2.5f, 2.8f, 0.1f);
                winGlass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;

                GameObject winFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winFrame.name = "DarkFrame";
                winFrame.transform.SetParent(winF5.transform);
                winFrame.transform.localPosition = new Vector3(0, 0, 0.06f);
                winFrame.transform.localScale = new Vector3(2.65f, 2.95f, 0.08f);
                winFrame.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;
            }

            GameObject goFins = new GameObject("VerticalFins");
            goFins.transform.SetParent(goFacade.transform);

            float finStartY = glassH + 0.5f;
            float finH = bH - finStartY + 0.1f;
            const float finSpacing = 0.85f;
            const float centralBillboardW = 10.5f;
            int totalFinCols = Mathf.FloorToInt(bW / finSpacing);

            for (int fc = 0; fc < totalFinCols; fc++)
            {
                float fx = -bW * 0.5f + 0.5f + fc * finSpacing;

                bool isCenterGap = (fx > -centralBillboardW && fx < centralBillboardW);

                if (isCenterGap)
                {

                    float f5FinY = floorH * 4.5f;
                    GameObject f5Fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    f5Fin.name = "IUH_FacadeFin_F5_" + fc;
                    f5Fin.transform.SetParent(goFins.transform);
                    f5Fin.transform.localPosition = new Vector3(fx, f5FinY, frontZ + 0.32f);
                    f5Fin.transform.localRotation = Quaternion.Euler(12f, 0, 0);
                    f5Fin.transform.localScale = new Vector3(0.12f, floorH * 0.88f, 0.48f);
                    f5Fin.GetComponent<MeshRenderer>().sharedMaterial = m_BlueFins;
                }
                else
                {

                    GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fin.name = "IUH_FacadeFin_" + fc;
                    fin.transform.SetParent(goFins.transform);
                    fin.transform.localPosition = new Vector3(fx, finStartY + finH * 0.5f, frontZ + 0.32f);
                    fin.transform.localRotation = Quaternion.Euler(12f, 0, 0);
                    fin.transform.localScale = new Vector3(0.12f, finH, 0.48f);
                    fin.GetComponent<MeshRenderer>().sharedMaterial = m_BlueFins;
                }
            }

            GameObject goFrames = new GameObject("Frames");
            goFrames.transform.SetParent(goFacade.transform);

            float[] pierXs = new float[] { -bW * 0.5f + 0.2f, -centralBillboardW - 0.2f, centralBillboardW + 0.2f, bW * 0.5f - 0.2f };
            foreach (float px in pierXs)
            {
                GameObject pier = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pier.name = "StructuralPier_" + px.ToString("F1");
                pier.transform.SetParent(goFrames.transform);
                pier.transform.localPosition = new Vector3(px, finStartY + finH * 0.5f, frontZ + 0.35f);
                pier.transform.localScale = new Vector3(0.45f, finH + 0.2f, 0.55f);
                pier.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }

            GameObject goCentral = new GameObject("CentralFacade");
            goCentral.transform.SetParent(root.transform);

            float billboardY = glassH + floorH * 0.95f;
            float billboardH = floorH * 1.82f;
            float billboardW = centralBillboardW * 1.95f;

            GameObject centralGlass = new GameObject("IUH_MainBuilding_CentralGlassFacade");
            centralGlass.transform.SetParent(goCentral.transform);
            centralGlass.transform.localPosition = new Vector3(0, billboardY, frontZ + 0.32f);

            GameObject bbBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bbBox.name = "CentralGlass_Backing";
            bbBox.transform.SetParent(centralGlass.transform);
            bbBox.transform.localPosition = Vector3.zero;
            bbBox.transform.localScale = new Vector3(billboardW, billboardH, 0.35f);
            bbBox.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject bbGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bbGlass.name = "CentralGlass_ReflectiveLayer";
            bbGlass.transform.SetParent(centralGlass.transform);
            bbGlass.transform.localPosition = new Vector3(0, 0, 0.20f);
            bbGlass.transform.localScale = new Vector3(billboardW - 0.3f, billboardH - 0.3f, 0.08f);
            bbGlass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;

            GameObject bbBorder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bbBorder.name = "CentralGlass_MetalBorder";
            bbBorder.transform.SetParent(centralGlass.transform);
            bbBorder.transform.localPosition = new Vector3(0, 0, 0.22f);
            bbBorder.transform.localScale = new Vector3(billboardW + 0.2f, billboardH + 0.2f, 0.12f);
            bbBorder.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject signage = new GameObject("IUH_MainBuilding_Signage");
            signage.transform.SetParent(centralGlass.transform);
            signage.transform.localPosition = new Vector3(0, 0, 0.28f);

            GameObject signCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signCube.name = "Sign_Display_Cube";
            signCube.transform.SetParent(signage.transform);
            signCube.transform.localPosition = Vector3.zero;
            signCube.transform.localScale = new Vector3(billboardW * 0.94f, billboardH * 0.90f, 0.04f);
            signCube.GetComponent<MeshRenderer>().sharedMaterial = m_SignBillboard;

            GameObject crest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crest.name = "IUH_Crest_Logo";
            crest.transform.SetParent(goCentral.transform);
            crest.transform.localPosition = new Vector3(-bW * 0.5f + 5.2f, bH - 1.6f, frontZ + 0.38f);
            crest.transform.localScale = new Vector3(3.6f, 3.6f, 0.04f);
            crest.GetComponent<MeshRenderer>().sharedMaterial = m_LogoCrest;

            GameObject goEntrance = new GameObject("Entrance");
            goEntrance.transform.SetParent(root.transform);

            GameObject glassDoors = new GameObject("GlassDoors");
            glassDoors.transform.SetParent(goEntrance.transform);
            glassDoors.transform.localPosition = new Vector3(0, 1.5f, frontZ + 0.26f);

            GameObject doorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorFrame.name = "Door_Frame";
            doorFrame.transform.SetParent(glassDoors.transform);
            doorFrame.transform.localPosition = Vector3.zero;
            doorFrame.transform.localScale = new Vector3(7.5f, 3.0f, 0.15f);
            doorFrame.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject doorGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorGlass.name = "Door_Panes";
            doorGlass.transform.SetParent(glassDoors.transform);
            doorGlass.transform.localPosition = new Vector3(0, 0, 0.04f);
            doorGlass.transform.localScale = new Vector3(7.1f, 2.7f, 0.06f);
            doorGlass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassCurtain;

            for (int h = -1; h <= 1; h += 2)
            {
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "DoorHandle_" + (h > 0 ? "R" : "L");
                handle.transform.SetParent(glassDoors.transform);
                handle.transform.localPosition = new Vector3(h * 0.35f, -0.1f, 0.14f);
                handle.transform.localScale = new Vector3(0.04f, 0.5f, 0.04f);
                handle.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;
            }

            GameObject lobby = new GameObject("Lobby");
            lobby.transform.SetParent(goEntrance.transform);
            lobby.transform.localPosition = new Vector3(0, 0.2f, frontZ - 3.0f);

            GameObject lobbyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lobbyFloor.name = "Lobby_Floor";
            lobbyFloor.transform.SetParent(lobby.transform);
            lobbyFloor.transform.localPosition = Vector3.zero;
            lobbyFloor.transform.localScale = new Vector3(18.0f, 0.4f, 6.0f);
            lobbyFloor.GetComponent<MeshRenderer>().sharedMaterial = m_GraniteStairs;

            GameObject receptionDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            receptionDesk.name = "ReceptionDesk";
            receptionDesk.transform.SetParent(lobby.transform);
            receptionDesk.transform.localPosition = new Vector3(0, 0.6f, -1.5f);
            receptionDesk.transform.localScale = new Vector3(4.5f, 1.1f, 1.2f);
            receptionDesk.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            float canopyY = floorH * 1.05f;
            float canopyExt = 6.2f;

            GameObject canopy = new GameObject("IUH_MainBuilding_EntranceCanopy");
            canopy.transform.SetParent(goEntrance.transform);
            canopy.transform.localPosition = new Vector3(0, canopyY, frontZ);

            GameObject canopySlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopySlab.name = "Canopy_RoofSlab";
            canopySlab.transform.SetParent(canopy.transform);
            canopySlab.transform.localPosition = new Vector3(0, 0, canopyExt * 0.5f);
            canopySlab.transform.localScale = new Vector3(18.0f, 0.38f, canopyExt);
            canopySlab.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject canopyFrontFascia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopyFrontFascia.name = "Canopy_FrontFascia";
            canopyFrontFascia.transform.SetParent(canopy.transform);
            canopyFrontFascia.transform.localPosition = new Vector3(0, -0.22f, canopyExt + 0.05f);
            canopyFrontFascia.transform.localScale = new Vector3(18.2f, 0.65f, 0.25f);
            canopyFrontFascia.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            GameObject canopySoffit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopySoffit.name = "Canopy_GlassSoffit";
            canopySoffit.transform.SetParent(canopy.transform);
            canopySoffit.transform.localPosition = new Vector3(0, -0.15f, canopyExt * 0.5f);
            canopySoffit.transform.localScale = new Vector3(17.2f, 0.06f, canopyExt - 0.4f);
            canopySoffit.GetComponent<MeshRenderer>().sharedMaterial = m_GlassCurtain;

            for (int r = -3; r <= 3; r++)
            {
                if (r == 0) continue;
                float rx = r * 2.8f;
                GameObject stay = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stay.name = "CanopyStay_" + r;
                stay.transform.SetParent(canopy.transform);
                stay.transform.localPosition = new Vector3(rx, 1.85f, canopyExt * 0.45f);
                stay.transform.localRotation = Quaternion.Euler(46f, 0, 0);
                stay.transform.localScale = new Vector3(0.06f, 4.2f, 0.06f);
                stay.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;
            }

            GameObject stairs = new GameObject("IUH_MainBuilding_Stairs");
            stairs.transform.SetParent(goEntrance.transform);
            stairs.transform.localPosition = new Vector3(0, 0, frontZ);

            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "LandingPlatform";
            platform.transform.SetParent(stairs.transform);
            platform.transform.localPosition = new Vector3(0, 0.42f, 1.8f);
            platform.transform.localScale = new Vector3(19.0f, 0.84f, 3.6f);
            platform.GetComponent<MeshRenderer>().sharedMaterial = m_GraniteStairs;

            for (int s = 0; s < 5; s++)
            {
                float stepZ = 3.6f + s * 0.75f;
                float stepTopY = 0.84f - s * 0.168f;
                GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = "StairStep_" + s;
                step.transform.SetParent(stairs.transform);
                step.transform.localPosition = new Vector3(0, stepTopY * 0.5f, stepZ);
                step.transform.localScale = new Vector3(19.6f + s * 0.4f, stepTopY, 0.75f);
                step.GetComponent<MeshRenderer>().sharedMaterial = m_GraniteStairs;
            }

            GameObject goLandscaping = new GameObject("Landscaping");
            goLandscaping.transform.SetParent(root.transform);

            foreach (float side in new float[] { -12.5f, 12.5f })
            {
                GameObject bonsaiBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bonsaiBase.name = "BonsaiPlanter_" + (side > 0 ? "R" : "L");
                bonsaiBase.transform.SetParent(goLandscaping.transform);
                bonsaiBase.transform.localPosition = new Vector3(side, 0.48f, frontZ + 4.5f);
                bonsaiBase.transform.localScale = new Vector3(2.6f, 0.48f, 2.6f);
                bonsaiBase.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

                GameObject bonsaiTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bonsaiTrunk.name = "BonsaiTrunk";
                bonsaiTrunk.transform.SetParent(goLandscaping.transform);
                bonsaiTrunk.transform.localPosition = new Vector3(side, 2.4f, frontZ + 4.5f);
                bonsaiTrunk.transform.localScale = new Vector3(0.48f, 2.2f, 0.48f);
                bonsaiTrunk.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                GameObject bonsaiCanopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bonsaiCanopy.name = "BonsaiCanopy";
                bonsaiCanopy.transform.SetParent(goLandscaping.transform);
                bonsaiCanopy.transform.localPosition = new Vector3(side, 5.2f, frontZ + 4.5f);
                bonsaiCanopy.transform.localScale = new Vector3(5.6f, 4.2f, 5.6f);
                bonsaiCanopy.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
            }

            GameObject goSides = new GameObject("SideFacades");
            goSides.transform.SetParent(root.transform);

            foreach (float side in new float[] { -1f, 1f })
            {
                float sx = side * (bW * 0.5f + 0.05f);
                GameObject sideWing = new GameObject("SideFacade_" + (side > 0 ? "R" : "L"));
                sideWing.transform.SetParent(goSides.transform);
                sideWing.transform.localPosition = new Vector3(sx, 0, 0);

                for (int f = 1; f <= 5; f++)
                {
                    GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    band.name = "FloorBand_F" + f;
                    band.transform.SetParent(sideWing.transform);
                    band.transform.localPosition = new Vector3(0, f * floorH - 0.2f, 0);
                    band.transform.localScale = new Vector3(0.18f, 0.4f, bD);
                    band.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    if (f >= 2)
                    {
                        GameObject sideWin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        sideWin.name = "SideWindow_F" + f;
                        sideWin.transform.SetParent(sideWing.transform);
                        sideWin.transform.localPosition = new Vector3(0, (f - 0.5f) * floorH, 0);
                        sideWin.transform.localScale = new Vector3(0.15f, 2.2f, bD - 4.0f);
                        sideWin.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;
                    }
                }
            }

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, bH * 0.5f, 0);
            col.size = new Vector3(bW + 2.0f, bH + 1.0f, bD + 2.0f);

            return SaveAsPrefab(root, PrefabArchPath + "IUH_MainBuilding.prefab");
        }

        private static GameObject CreateGreenGlassTowerPrefab()
        {
            GameObject root = new GameObject("IUH_GreenGlassTower");

            float towerW = 22.0f;
            float towerD = 22.0f;
            float towerH = 56.0f;

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Tower_Core";
            core.transform.SetParent(root.transform);
            core.transform.localPosition = new Vector3(0, towerH * 0.5f, 0);
            core.transform.localScale = new Vector3(towerW, towerH, towerD);
            core.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            for (int side = -1; side <= 1; side += 2)
            {
                for (int fb = -1; fb <= 1; fb += 2)
                {
                    GameObject greenCorner = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    greenCorner.name = "GreenCorner_" + side + "_" + fb;
                    greenCorner.transform.SetParent(root.transform);
                    greenCorner.transform.localPosition = new Vector3(side * (towerW * 0.44f), towerH * 0.48f, fb * (towerD * 0.44f));
                    greenCorner.transform.localScale = new Vector3(4.0f, towerH * 0.94f, 4.0f);
                    greenCorner.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;
                }
            }

            for (int side = -1; side <= 1; side += 2)
            {

                GameObject glassV = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glassV.name = "CurtainGlass_Z_" + side;
                glassV.transform.SetParent(root.transform);
                glassV.transform.localPosition = new Vector3(0, towerH * 0.45f, side * (towerD * 0.5f + 0.15f));
                glassV.transform.localScale = new Vector3(12.0f, towerH * 0.82f, 0.25f);
                glassV.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;

                GameObject glassH = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glassH.name = "CurtainGlass_X_" + side;
                glassH.transform.SetParent(root.transform);
                glassH.transform.localPosition = new Vector3(side * (towerW * 0.5f + 0.15f), towerH * 0.45f, 0);
                glassH.transform.localScale = new Vector3(0.25f, towerH * 0.82f, 12.0f);
                glassH.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;
            }

            for (int f = 1; f <= 13; f++)
            {
                float y = f * 3.8f;
                GameObject fBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fBand.name = "FloorBand_" + f;
                fBand.transform.SetParent(root.transform);
                fBand.transform.localPosition = new Vector3(0, y, 0);
                fBand.transform.localScale = new Vector3(towerW + 0.35f, 0.4f, towerD + 0.35f);
                fBand.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }

            GameObject upperBalcony = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperBalcony.name = "Observation_Balcony";
            upperBalcony.transform.SetParent(root.transform);
            upperBalcony.transform.localPosition = new Vector3(0, 44.0f, towerD * 0.5f + 1.2f);
            upperBalcony.transform.localScale = new Vector3(16.0f, 7.0f, 2.4f);
            upperBalcony.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject roofBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofBase.name = "Rooftop_Base";
            roofBase.transform.SetParent(root.transform);
            roofBase.transform.localPosition = new Vector3(0, towerH + 1.5f, 0);
            roofBase.transform.localScale = new Vector3(towerW + 1.0f, 3.0f, towerD + 1.0f);
            roofBase.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.name = "Pyramidal_Crown";
            crown.transform.SetParent(root.transform);
            crown.transform.localPosition = new Vector3(0, towerH + 7.0f, 0);
            crown.transform.localScale = new Vector3(13.0f, 4.5f, 11.0f);
            crown.GetComponent<MeshRenderer>().sharedMaterial = m_TowerPyramidRoof;

            GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tank.name = "WaterTank";
            tank.transform.SetParent(root.transform);
            tank.transform.localPosition = new Vector3(-5.5f, towerH + 4.0f, 4.0f);
            tank.transform.localRotation = Quaternion.Euler(0, 0, 90);
            tank.transform.localScale = new Vector3(2.0f, 4.0f, 2.0f);
            tank.GetComponent<MeshRenderer>().sharedMaterial = m_RooftopMetal;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, towerH * 0.5f, 0);
            col.size = new Vector3(towerW + 1.0f, towerH, towerD + 1.0f);

            return SaveAsPrefab(root, PrefabArchPath + "IUH_GreenGlassTower.prefab");
        }

        private static GameObject CreateLeftPaleWingPrefab()
        {
            GameObject root = new GameObject("IUH_LeftPaleWing");

            float floorH = 3.6f;
            int numFloors = 5;
            float bHeight = numFloors * floorH;
            float wingLength = 115.0f;
            float wingDepth = 18.0f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Wing_Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0, bHeight * 0.5f, -wingLength * 0.5f);
            body.transform.localScale = new Vector3(wingDepth, bHeight, wingLength);
            body.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            for (int f = 1; f <= numFloors; f++)
            {
                float y = (f - 0.5f) * floorH;

                GameObject balcony = GameObject.CreatePrimitive(PrimitiveType.Cube);
                balcony.name = "Balcony_F" + f;
                balcony.transform.SetParent(root.transform);
                balcony.transform.localPosition = new Vector3(wingDepth * 0.5f + 0.9f, (f - 1) * floorH + 0.15f, -wingLength * 0.5f);
                balcony.transform.localScale = new Vector3(2.0f, 0.3f, wingLength);
                balcony.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

                GameObject railing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                railing.name = "Railing_F" + f;
                railing.transform.SetParent(root.transform);
                railing.transform.localPosition = new Vector3(wingDepth * 0.5f + 1.85f, (f - 1) * floorH + 0.7f, -wingLength * 0.5f);
                railing.transform.localScale = new Vector3(0.1f, 0.85f, wingLength);
                railing.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glass.name = "Glass_F" + f;
                glass.transform.SetParent(root.transform);
                glass.transform.localPosition = new Vector3(wingDepth * 0.5f + 0.05f, y + 0.4f, -wingLength * 0.5f);
                glass.transform.localScale = new Vector3(0.1f, 2.0f, wingLength - 2.0f);
                glass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassReflective;
            }

            int numCols = 24;
            for (int c = 0; c <= numCols; c++)
            {
                float z = -c * (wingLength / numCols);
                GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cube);
                col.name = "Column_" + c;
                col.transform.SetParent(root.transform);
                col.transform.localPosition = new Vector3(wingDepth * 0.5f + 1.9f, bHeight * 0.5f, z);
                col.transform.localScale = new Vector3(0.3f, bHeight, 0.5f);
                col.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;
            }

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "PaleGreen_Roof";
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(0, bHeight + 1.5f, -wingLength * 0.5f);
            roof.transform.localScale = new Vector3(wingDepth + 4.0f, 3.0f, wingLength + 4.0f);
            roof.GetComponent<MeshRenderer>().sharedMaterial = m_PaleGreenRoof;

            GameObject solar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            solar.name = "SolarPanels";
            solar.transform.SetParent(roof.transform);
            solar.transform.localPosition = new Vector3(0, 0.52f, 0.2f);
            solar.transform.localScale = new Vector3(0.6f, 0.05f, 0.25f);
            solar.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

            BoxCollider boxCol = root.AddComponent<BoxCollider>();
            boxCol.center = new Vector3(0, bHeight * 0.5f, -wingLength * 0.5f);
            boxCol.size = new Vector3(wingDepth, bHeight, wingLength);

            return SaveAsPrefab(root, PrefabArchPath + "IUH_LeftPaleWing.prefab");
        }

        private static GameObject CreateRedRoofBuildingPrefab()
        {
            GameObject root = new GameObject("IUH_RedRoofBuilding");

            float buildingW = 20.0f;
            float buildingH = 22.0f;
            float buildingL = 60.0f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Academic_Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0, buildingH * 0.5f, 0);
            body.transform.localScale = new Vector3(buildingW, buildingH, buildingL);
            body.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            for (int f = 1; f <= 6; f++)
            {
                float y = (f - 0.5f) * (buildingH / 6.0f);
                GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.name = "Win_F" + f;
                win.transform.SetParent(root.transform);
                win.transform.localPosition = new Vector3(buildingW * 0.5f + 0.08f, y, 0);
                win.transform.localScale = new Vector3(0.12f, 1.6f, buildingL - 4.0f);
                win.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;
            }

            GameObject redRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            redRoof.name = "Terracotta_Red_Roof";
            redRoof.transform.SetParent(root.transform);
            redRoof.transform.localPosition = new Vector3(0, buildingH + 2.5f, 0);
            redRoof.transform.localScale = new Vector3(buildingW + 2.5f, 5.0f, buildingL + 2.5f);
            redRoof.GetComponent<MeshRenderer>().sharedMaterial = m_RedMetalRoof;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_RedRoofBuilding.prefab");
        }

        private static GameObject CreateRightLargeAcademicBuildingPrefab()
        {
            return IUHRightClusterBuilder.CreateRightClusterPrefab();
        }

        private static GameObject CreateBackCentralTowerPrefab()
        {
            GameObject root = new GameObject("IUH_BackCentralTower");

            float towerW = 32.0f;
            float towerD = 24.0f;
            float towerH = 68.0f;

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Tower_Core";
            core.transform.SetParent(root.transform);
            core.transform.localPosition = new Vector3(0, towerH * 0.5f, 0);
            core.transform.localScale = new Vector3(towerW, towerH, towerD);
            core.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            for (int f = 1; f <= 15; f++)
            {
                float y = (f - 0.5f) * (towerH / 15.0f);
                GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.name = "Win_F" + f;
                win.transform.SetParent(root.transform);
                win.transform.localPosition = new Vector3(0, y, towerD * 0.5f + 0.12f);
                win.transform.localScale = new Vector3(towerW - 4.0f, 2.2f, 0.2f);
                win.GetComponent<MeshRenderer>().sharedMaterial = m_GreenGlass;
            }

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crown.name = "Crown";
            crown.transform.SetParent(root.transform);
            crown.transform.localPosition = new Vector3(0, towerH + 2.0f, 0);
            crown.transform.localScale = new Vector3(towerW + 1.2f, 4.0f, towerD + 1.2f);
            crown.GetComponent<MeshRenderer>().sharedMaterial = m_WhiteFacade;

            return SaveAsPrefab(root, PrefabArchPath + "IUH_BackCentralTower.prefab");
        }

        private static GameObject CreateCentralTreeClusterPrefab()
        {
            GameObject root = new GameObject("IUH_CentralTreeCluster");

            Vector3[] positions = new Vector3[]
            {
                new Vector3(-24.0f, 0, 2.0f),
                new Vector3(24.0f, 0, 2.0f),
                new Vector3(-30.0f, 0, -6.0f),
                new Vector3(30.0f, 0, -6.0f)
            };

            for (int t = 0; t < positions.Length; t++)
            {
                GameObject tree = new GameObject("MatureTree_" + t);
                tree.transform.SetParent(root.transform);
                tree.transform.localPosition = positions[t];

                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk";
                trunk.transform.SetParent(tree.transform);
                trunk.transform.localPosition = new Vector3(0, 4.5f, 0);
                trunk.transform.localScale = new Vector3(1.6f, 4.5f, 1.6f);
                trunk.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bench.name = "Bench";
                bench.transform.SetParent(tree.transform);
                bench.transform.localPosition = new Vector3(0, 0.35f, 0);
                bench.transform.localScale = new Vector3(4.2f, 0.35f, 4.2f);
                bench.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

                Vector3[] lobeOffsets = new Vector3[]
                {
                    new Vector3(0, 11.0f, 0),
                    new Vector3(-3.5f, 10.2f, 2.0f),
                    new Vector3(4.0f, 10.5f, -1.8f),
                    new Vector3(1.5f, 12.2f, 3.0f),
                    new Vector3(-2.2f, 11.5f, -2.8f)
                };

                for (int i = 0; i < lobeOffsets.Length; i++)
                {
                    GameObject lobe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    lobe.name = "Lobe_" + i;
                    lobe.transform.SetParent(tree.transform);
                    lobe.transform.localPosition = lobeOffsets[i];
                    lobe.transform.localScale = new Vector3(10.5f, 7.2f, 10.5f);
                    lobe.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
                }
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_CentralTreeCluster.prefab");
        }

        private static GameObject CreateSportsCourtyardPrefab()
        {
            GameObject root = new GameObject("IUH_SportsCourtyard");

            GameObject court = GameObject.CreatePrimitive(PrimitiveType.Cube);
            court.name = "Court_Slab";
            court.transform.SetParent(root.transform);
            court.transform.localPosition = new Vector3(0, 0.02f, 0);
            court.transform.localScale = new Vector3(48.0f, 0.04f, 75.0f);
            court.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            for (int i = -3; i <= 3; i++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "Line_" + i;
                line.transform.SetParent(root.transform);
                line.transform.localPosition = new Vector3(0, 0.05f, i * 11.0f);
                line.transform.localScale = new Vector3(36.0f, 0.02f, 0.25f);
                line.GetComponent<MeshRenderer>().sharedMaterial = m_RoadArrow;
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_SportsCourtyard.prefab");
        }

        private static GameObject CreateMotorbikeParkingPrefab()
        {
            GameObject root = new GameObject("IUH_MotorbikeParkingRow");
            int count = 10;
            for (int i = 0; i < count; i++)
            {
                GameObject bike = new GameObject("Bike_" + i);
                bike.transform.SetParent(root.transform);
                bike.transform.localPosition = new Vector3(0, 0, i * 1.1f);
                bike.transform.localRotation = Quaternion.Euler(0, 90 + 15 * (i % 2 == 0 ? 1 : -1), 0);

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
            float width = 6.0f;
            float depth = 10.0f;
            float height = 3.2f;

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Canopy_Cover";
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(0, height, 0);
            roof.transform.localRotation = Quaternion.Euler(0, 0, 6);
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

        private static GameObject CreateCoveredRampPrefab()
        {
            GameObject root = new GameObject("IUH_CoveredRamp_Basement");
            GameObject rampSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampSlab.name = "Ramp_Slab";
            rampSlab.transform.SetParent(root.transform);
            rampSlab.transform.localPosition = new Vector3(0, 1.5f, 0);
            rampSlab.transform.localRotation = Quaternion.Euler(0, 0, 12);
            rampSlab.transform.localScale = new Vector3(16.0f, 0.3f, 6.0f);
            rampSlab.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Ramp_Roof";
            roof.transform.SetParent(root.transform);
            roof.transform.localPosition = new Vector3(0, 4.0f, 0);
            roof.transform.localRotation = Quaternion.Euler(0, 0, 12);
            roof.transform.localScale = new Vector3(17.0f, 0.2f, 6.8f);
            roof.GetComponent<MeshRenderer>().sharedMaterial = m_BlueFacade;

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_CoveredRamp_Basement.prefab");
        }

        private static GameObject CreateFrontMonumentPrefab()
        {
            GameObject root = new GameObject("IUH_FrontMonument");

            GameObject islandCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            islandCurb.name = "Grass_Island_Curb";
            islandCurb.transform.SetParent(root.transform);
            islandCurb.transform.localPosition = new Vector3(0, 0.12f, 0);
            islandCurb.transform.localScale = new Vector3(16.0f, 0.35f, 10.0f);
            islandCurb.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            GameObject islandTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            islandTip.name = "Grass_Island_Tip";
            islandTip.transform.SetParent(root.transform);
            islandTip.transform.localPosition = new Vector3(-0.6f, 0.12f, -4.5f);
            islandTip.transform.localScale = new Vector3(7.5f, 0.35f, 4.8f);
            islandTip.transform.localRotation = Quaternion.Euler(0, -8f, 0);
            islandTip.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            GameObject grassSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grassSurface.name = "Grass_Surface";
            grassSurface.transform.SetParent(root.transform);
            grassSurface.transform.localPosition = new Vector3(0, 0.28f, 0);
            grassSurface.transform.localScale = new Vector3(15.0f, 0.15f, 9.2f);
            grassSurface.GetComponent<MeshRenderer>().sharedMaterial = m_Grass;

            GameObject monRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            monRock.name = "Monument_Rock";
            monRock.transform.SetParent(root.transform);
            monRock.transform.localPosition = new Vector3(0, 0.9f, 0.5f);
            monRock.transform.localScale = new Vector3(7.5f, 1.6f, 3.2f);
            monRock.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentStone;

            GameObject letterGroup = new GameObject("IUH_3D_Letters");
            letterGroup.transform.SetParent(root.transform);
            letterGroup.transform.localPosition = new Vector3(0, 1.9f, -1.1f);

            GameObject letterI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letterI.name = "Letter_I";
            letterI.transform.SetParent(letterGroup.transform);
            letterI.transform.localPosition = new Vector3(-2.4f, 0, 0);
            letterI.transform.localScale = new Vector3(0.55f, 2.2f, 0.45f);
            letterI.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject letterU = new GameObject("Letter_U");
            letterU.transform.SetParent(letterGroup.transform);
            letterU.transform.localPosition = new Vector3(0, 0, 0);

            GameObject uL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uL.transform.SetParent(letterU.transform);
            uL.transform.localPosition = new Vector3(-0.65f, 0, 0);
            uL.transform.localScale = new Vector3(0.48f, 2.2f, 0.45f);
            uL.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject uR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uR.transform.SetParent(letterU.transform);
            uR.transform.localPosition = new Vector3(0.65f, 0, 0);
            uR.transform.localScale = new Vector3(0.48f, 2.2f, 0.45f);
            uR.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject uB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uB.transform.SetParent(letterU.transform);
            uB.transform.localPosition = new Vector3(0, -0.88f, 0);
            uB.transform.localScale = new Vector3(1.78f, 0.48f, 0.45f);
            uB.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject letterH = new GameObject("Letter_H");
            letterH.transform.SetParent(letterGroup.transform);
            letterH.transform.localPosition = new Vector3(2.4f, 0, 0);

            GameObject hL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hL.transform.SetParent(letterH.transform);
            hL.transform.localPosition = new Vector3(-0.65f, 0, 0);
            hL.transform.localScale = new Vector3(0.48f, 2.2f, 0.45f);
            hL.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject hR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hR.transform.SetParent(letterH.transform);
            hR.transform.localPosition = new Vector3(0.65f, 0, 0);
            hR.transform.localScale = new Vector3(0.48f, 2.2f, 0.45f);
            hR.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            GameObject hC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hC.transform.SetParent(letterH.transform);
            hC.transform.localPosition = new Vector3(0, 0, 0);
            hC.transform.localScale = new Vector3(1.78f, 0.48f, 0.45f);
            hC.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

            for (int p = -5; p <= 5; p++)
            {
                float px = p * 1.45f;
                GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pot.name = "Pot_" + p;
                pot.transform.SetParent(root.transform);
                pot.transform.localPosition = new Vector3(px, 0.32f, -4.2f);
                pot.transform.localScale = new Vector3(0.65f, 0.35f, 0.65f);
                pot.GetComponent<MeshRenderer>().sharedMaterial = m_FlowerPotWhite;

                GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = "Flower_" + p;
                flower.transform.SetParent(root.transform);
                flower.transform.localPosition = new Vector3(px, 0.78f, -4.2f);
                flower.transform.localScale = new Vector3(0.75f, 0.55f, 0.75f);
                flower.GetComponent<MeshRenderer>().sharedMaterial = m_FlowerOrange;
            }

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.0f, 0);
            col.size = new Vector3(16.0f, 2.2f, 10.0f);

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_FrontMonument.prefab");
        }

        private static GameObject CreateFlagPolesPrefab()
        {
            GameObject root = new GameObject("IUH_FlagPoles");

            Material[] flagMats = new Material[] { m_FlagVn, m_FlagIuh, m_FlagYouth };
            string[] flagNames = new string[] { "Vietnam", "IUH", "YouthUnion" };

            for (int i = 0; i < 3; i++)
            {
                float x = i * 3.2f;

                GameObject poleBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
                poleBase.name = "PoleBase_" + i;
                poleBase.transform.SetParent(root.transform);
                poleBase.transform.localPosition = new Vector3(x, 0.4f, 0);
                poleBase.transform.localScale = new Vector3(0.75f, 0.8f, 0.75f);
                poleBase.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Pole_" + i;
                pole.transform.SetParent(root.transform);
                pole.transform.localPosition = new Vector3(x, 11.0f, 0);
                pole.transform.localScale = new Vector3(0.16f, 11.0f, 0.16f);
                pole.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

                GameObject finial = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                finial.name = "Finial_" + i;
                finial.transform.SetParent(root.transform);
                finial.transform.localPosition = new Vector3(x, 22.3f, 0);
                finial.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                finial.GetComponent<MeshRenderer>().sharedMaterial = m_MonumentLetter;

                GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flag.name = "FlagCloth_" + flagNames[i];
                flag.transform.SetParent(root.transform);
                flag.transform.localPosition = new Vector3(x + 1.85f, 20.0f, 0);
                flag.transform.localScale = new Vector3(3.6f, 2.4f, 0.08f);
                flag.GetComponent<MeshRenderer>().sharedMaterial = flagMats[i];
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_FlagPoles.prefab");
        }

        private static GameObject CreateMainEntrancePrefab()
        {
            GameObject root = new GameObject("IUH_MainEntrance");

            Material matStone   = m_GateStone   ?? m_ConcreteGround;
            Material matRail    = m_GateBlueRailing ?? m_DarkMetal;
            Material matBooth   = m_GateBooth   ?? m_WhiteFacade;
            Material matBoothRf = m_GateBoothRoof   ?? m_BlueFacade;
            Material matBanner  = m_GateWelcomeBanner ?? m_AsphaltRoad;
            Material matFlag    = m_GateEventFlag ?? m_FlagIuh;
            Material matRoad    = m_GateRoad    ?? m_AsphaltRoad;
            Material matCurb    = m_GateCurb    ?? m_ConcreteGround;
            Material matGold    = m_GateGoldText ?? m_MonumentLetter;

            GameObject goRoad = new GameObject("Entrance_Road");
            goRoad.transform.SetParent(root.transform);

            GameObject roadSurf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadSurf.name = "Road_Asphalt";
            roadSurf.transform.SetParent(goRoad.transform);
            roadSurf.transform.localPosition = new Vector3(0, -0.02f, 0);
            roadSurf.transform.localScale = new Vector3(14.0f, 0.06f, 30.0f);
            roadSurf.GetComponent<MeshRenderer>().sharedMaterial = matRoad;

            GameObject laneLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            laneLine.name = "LaneDivider";
            laneLine.transform.SetParent(goRoad.transform);
            laneLine.transform.localPosition = new Vector3(0, 0.02f, 0);
            laneLine.transform.localScale = new Vector3(0.22f, 0.04f, 28.0f);
            laneLine.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

            GameObject leftCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftCurb.name = "Curb_Left";
            leftCurb.transform.SetParent(goRoad.transform);
            leftCurb.transform.localPosition = new Vector3(-9.5f, 0.05f, 0);
            leftCurb.transform.localScale = new Vector3(5.0f, 0.10f, 30.0f);
            leftCurb.GetComponent<MeshRenderer>().sharedMaterial = matCurb;

            GameObject rightCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightCurb.name = "Curb_Right";
            rightCurb.transform.SetParent(goRoad.transform);
            rightCurb.transform.localPosition = new Vector3(9.5f, 0.05f, 0);
            rightCurb.transform.localScale = new Vector3(5.0f, 0.10f, 30.0f);
            rightCurb.GetComponent<MeshRenderer>().sharedMaterial = matCurb;

            GameObject goPillars = new GameObject("Gate_Pillars");
            goPillars.transform.SetParent(root.transform);

            float pillarX  = 8.2f;
            float pillarH  = 8.5f;

            foreach (float side in new float[] { -1f, 1f })
            {
                string sideTag = side < 0 ? "L" : "R";

                GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plinth.name = "Pillar_Plinth_" + sideTag;
                plinth.transform.SetParent(goPillars.transform);
                plinth.transform.localPosition = new Vector3(side * pillarX, 0.5f, -10.0f);
                plinth.transform.localScale = new Vector3(2.8f, 1.0f, 2.8f);
                plinth.GetComponent<MeshRenderer>().sharedMaterial = matStone;

                GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shaft.name = "Pillar_Shaft_" + sideTag;
                shaft.transform.SetParent(goPillars.transform);
                shaft.transform.localPosition = new Vector3(side * pillarX, pillarH * 0.5f + 1.0f, -10.0f);
                shaft.transform.localScale = new Vector3(2.2f, pillarH, 2.2f);
                shaft.GetComponent<MeshRenderer>().sharedMaterial = matStone;

                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cap.name = "Pillar_Capital_" + sideTag;
                cap.transform.SetParent(goPillars.transform);
                cap.transform.localPosition = new Vector3(side * pillarX, pillarH + 1.8f, -10.0f);
                cap.transform.localScale = new Vector3(2.7f, 0.55f, 2.7f);
                cap.GetComponent<MeshRenderer>().sharedMaterial = matStone;

                GameObject plaque = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plaque.name = "Pillar_Plaque_" + sideTag;
                plaque.transform.SetParent(goPillars.transform);
                plaque.transform.localPosition = new Vector3(side * pillarX, pillarH * 0.5f + 1.0f, -10.0f + 1.15f);
                plaque.transform.localScale = new Vector3(1.8f, 2.4f, 0.08f);
                plaque.GetComponent<MeshRenderer>().sharedMaterial = matGold;
            }

            GameObject goBanner = new GameObject("Overhead_Welcome_Banner");
            goBanner.transform.SetParent(root.transform);
            goBanner.transform.localPosition = new Vector3(0, pillarH + 1.45f, -10.0f);

            GameObject bannerBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bannerBeam.name = "Banner_Beam";
            bannerBeam.transform.SetParent(goBanner.transform);
            bannerBeam.transform.localPosition = Vector3.zero;
            bannerBeam.transform.localScale = new Vector3(pillarX * 2.0f - 2.2f, 0.28f, 0.28f);
            bannerBeam.GetComponent<MeshRenderer>().sharedMaterial = matRail;

            GameObject bannerCloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bannerCloth.name = "Banner_Cloth";
            bannerCloth.transform.SetParent(goBanner.transform);
            bannerCloth.transform.localPosition = new Vector3(0, -1.05f, 0.04f);
            bannerCloth.transform.localScale = new Vector3(pillarX * 2.0f - 2.6f, 1.9f, 0.08f);
            bannerCloth.GetComponent<MeshRenderer>().sharedMaterial = matBanner;

            GameObject bannerText = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bannerText.name = "Banner_GoldLettering";
            bannerText.transform.SetParent(goBanner.transform);
            bannerText.transform.localPosition = new Vector3(0, -0.35f, 0.09f);
            bannerText.transform.localScale = new Vector3(pillarX * 2.0f - 4.0f, 0.35f, 0.04f);
            bannerText.GetComponent<MeshRenderer>().sharedMaterial = matGold;

            GameObject goMonumentSlab = new GameObject("University_Monument_Slab");
            goMonumentSlab.transform.SetParent(root.transform);
            goMonumentSlab.transform.localPosition = new Vector3(-13.5f, 0, -6.0f);

            GameObject monBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monBase.name = "Monument_Base";
            monBase.transform.SetParent(goMonumentSlab.transform);
            monBase.transform.localPosition = new Vector3(0, 0.35f, 0);
            monBase.transform.localScale = new Vector3(8.5f, 0.70f, 2.0f);
            monBase.GetComponent<MeshRenderer>().sharedMaterial = matStone;

            GameObject monSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monSlab.name = "Monument_Slab";
            monSlab.transform.SetParent(goMonumentSlab.transform);
            monSlab.transform.localPosition = new Vector3(0, 2.25f, 0);
            monSlab.transform.localScale = new Vector3(8.0f, 3.8f, 0.65f);
            monSlab.GetComponent<MeshRenderer>().sharedMaterial = matStone;

            GameObject monCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monCap.name = "Monument_TopCap";
            monCap.transform.SetParent(goMonumentSlab.transform);
            monCap.transform.localPosition = new Vector3(0, 4.35f, 0);
            monCap.transform.localScale = new Vector3(8.4f, 0.45f, 0.80f);
            monCap.GetComponent<MeshRenderer>().sharedMaterial = matStone;

            GameObject monLetters = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monLetters.name = "Monument_IUH_Letters";
            monLetters.transform.SetParent(goMonumentSlab.transform);
            monLetters.transform.localPosition = new Vector3(0, 2.6f, 0.38f);
            monLetters.transform.localScale = new Vector3(5.8f, 1.2f, 0.08f);
            monLetters.GetComponent<MeshRenderer>().sharedMaterial = matGold;

            GameObject monSubtext = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monSubtext.name = "Monument_Subtext";
            monSubtext.transform.SetParent(goMonumentSlab.transform);
            monSubtext.transform.localPosition = new Vector3(0, 1.6f, 0.38f);
            monSubtext.transform.localScale = new Vector3(7.2f, 0.55f, 0.06f);
            monSubtext.GetComponent<MeshRenderer>().sharedMaterial = matGold;

            GameObject monGrass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monGrass.name = "Monument_GrassBed";
            monGrass.transform.SetParent(goMonumentSlab.transform);
            monGrass.transform.localPosition = new Vector3(0, 0.06f, 0);
            monGrass.transform.localScale = new Vector3(10.0f, 0.12f, 4.0f);
            monGrass.GetComponent<MeshRenderer>().sharedMaterial = m_Grass;

            GameObject goBooth = new GameObject("Security_Booth");
            goBooth.transform.SetParent(root.transform);
            goBooth.transform.localPosition = new Vector3(12.5f, 0, -8.0f);

            GameObject boothBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boothBody.name = "Booth_Body";
            boothBody.transform.SetParent(goBooth.transform);
            boothBody.transform.localPosition = new Vector3(0, 1.4f, 0);
            boothBody.transform.localScale = new Vector3(2.8f, 2.8f, 2.4f);
            boothBody.GetComponent<MeshRenderer>().sharedMaterial = matBooth;

            GameObject boothRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boothRoof.name = "Booth_Roof";
            boothRoof.transform.SetParent(goBooth.transform);
            boothRoof.transform.localPosition = new Vector3(0, 2.95f, 0);
            boothRoof.transform.localScale = new Vector3(3.2f, 0.30f, 2.8f);
            boothRoof.GetComponent<MeshRenderer>().sharedMaterial = matBoothRf;

            foreach (float wz in new float[] { 0f })
            {
                GameObject boothWin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boothWin.name = "Booth_Window_Front";
                boothWin.transform.SetParent(goBooth.transform);
                boothWin.transform.localPosition = new Vector3(0, 1.6f, 1.25f);
                boothWin.transform.localScale = new Vector3(2.0f, 1.5f, 0.08f);
                boothWin.GetComponent<MeshRenderer>().sharedMaterial = m_GlassCurtain;
            }

            GameObject boothDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boothDoor.name = "Booth_Door";
            boothDoor.transform.SetParent(goBooth.transform);
            boothDoor.transform.localPosition = new Vector3(0, 1.0f, -1.25f);
            boothDoor.transform.localScale = new Vector3(0.95f, 2.0f, 0.08f);
            boothDoor.GetComponent<MeshRenderer>().sharedMaterial = matBooth;

            GameObject goRailing = new GameObject("Right_Blue_Railing");
            goRailing.transform.SetParent(root.transform);
            goRailing.transform.localPosition = new Vector3(11.8f, 0, 0);

            GameObject topRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topRail.name = "Rail_Top";
            topRail.transform.SetParent(goRailing.transform);
            topRail.transform.localPosition = new Vector3(0, 1.05f, 0);
            topRail.transform.localScale = new Vector3(0.10f, 0.10f, 22.0f);
            topRail.GetComponent<MeshRenderer>().sharedMaterial = matRail;

            GameObject botRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            botRail.name = "Rail_Bottom";
            botRail.transform.SetParent(goRailing.transform);
            botRail.transform.localPosition = new Vector3(0, 0.35f, 0);
            botRail.transform.localScale = new Vector3(0.10f, 0.10f, 22.0f);
            botRail.GetComponent<MeshRenderer>().sharedMaterial = matRail;

            int numBalusters = 18;
            for (int b = 0; b < numBalusters; b++)
            {
                float bz = -10.0f + b * (22.0f / (numBalusters - 1));
                GameObject bal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bal.name = "Baluster_" + b;
                bal.transform.SetParent(goRailing.transform);
                bal.transform.localPosition = new Vector3(0, 0.55f, bz);
                bal.transform.localScale = new Vector3(0.08f, 1.0f, 0.08f);
                bal.GetComponent<MeshRenderer>().sharedMaterial = matRail;
            }

            GameObject goRailingL = new GameObject("Left_Blue_Railing");
            goRailingL.transform.SetParent(root.transform);
            goRailingL.transform.localPosition = new Vector3(-11.8f, 0, 0);

            GameObject topRailL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topRailL.name = "Rail_Top_L";
            topRailL.transform.SetParent(goRailingL.transform);
            topRailL.transform.localPosition = new Vector3(0, 1.05f, 0);
            topRailL.transform.localScale = new Vector3(0.10f, 0.10f, 22.0f);
            topRailL.GetComponent<MeshRenderer>().sharedMaterial = matRail;

            GameObject botRailL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            botRailL.name = "Rail_Bottom_L";
            botRailL.transform.SetParent(goRailingL.transform);
            botRailL.transform.localPosition = new Vector3(0, 0.35f, 0);
            botRailL.transform.localScale = new Vector3(0.10f, 0.10f, 22.0f);
            botRailL.GetComponent<MeshRenderer>().sharedMaterial = matRail;

            for (int b = 0; b < numBalusters; b++)
            {
                float bz = -10.0f + b * (22.0f / (numBalusters - 1));
                GameObject bal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bal.name = "Baluster_L_" + b;
                bal.transform.SetParent(goRailingL.transform);
                bal.transform.localPosition = new Vector3(0, 0.55f, bz);
                bal.transform.localScale = new Vector3(0.08f, 1.0f, 0.08f);
                bal.GetComponent<MeshRenderer>().sharedMaterial = matRail;
            }

            GameObject goFlags = new GameObject("Event_Flag_Line");
            goFlags.transform.SetParent(root.transform);

            Material[] flagCycleMats = new Material[]
            {
                m_GateEventFlag ?? m_FlagIuh,
                m_FlagVn        ?? m_FlagIuh,
                m_GateBlueRailing ?? m_DarkMetal,
                m_Grass,
            };

            int flagCount = 6;
            float flagSpacing = 4.0f;
            foreach (float side in new float[] { -1f, 1f })
            {
                for (int fi = 0; fi < flagCount; fi++)
                {
                    float fz = -12.0f + fi * flagSpacing;
                    float fx = side * 9.8f;

                    GameObject fPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    fPole.name = "FlagPole_" + (side < 0 ? "L" : "R") + "_" + fi;
                    fPole.transform.SetParent(goFlags.transform);
                    fPole.transform.localPosition = new Vector3(fx, 2.5f, fz);
                    fPole.transform.localScale = new Vector3(0.06f, 2.5f, 0.06f);
                    fPole.GetComponent<MeshRenderer>().sharedMaterial = m_LightMetal;

                    GameObject fFlag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fFlag.name = "Pennant_" + (side < 0 ? "L" : "R") + "_" + fi;
                    fFlag.transform.SetParent(goFlags.transform);
                    fFlag.transform.localPosition = new Vector3(fx + side * 0.6f, 4.6f, fz);
                    fFlag.transform.localScale = new Vector3(0.06f, 0.8f, 1.2f);
                    fFlag.GetComponent<MeshRenderer>().sharedMaterial = flagCycleMats[fi % flagCycleMats.Length];
                }
            }

            GameObject goEntranceTrees = new GameObject("Entrance_Trees");
            goEntranceTrees.transform.SetParent(root.transform);

            foreach (float side in new float[] { -1f, 1f })
            {
                for (int t = 0; t < 4; t++)
                {
                    float tz = -14.0f + t * 5.0f;
                    float tx = side * 13.0f;

                    GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    trunk.name = "Tree_Trunk_" + (side < 0 ? "L" : "R") + "_" + t;
                    trunk.transform.SetParent(goEntranceTrees.transform);
                    trunk.transform.localPosition = new Vector3(tx, 1.5f, tz);
                    trunk.transform.localScale = new Vector3(0.22f, 1.5f, 0.22f);
                    trunk.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMetal;

                    GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    canopy.name = "Tree_Canopy_" + (side < 0 ? "L" : "R") + "_" + t;
                    canopy.transform.SetParent(goEntranceTrees.transform);
                    canopy.transform.localPosition = new Vector3(tx, 4.0f, tz);
                    canopy.transform.localScale = new Vector3(2.8f, 3.2f, 2.8f);
                    canopy.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
                }
            }

            return SaveAsPrefab(root, PrefabEnvPath + "IUH_MainEntrance.prefab");
        }

        #endregion

        #region Master Scene Assembly

        private static void AssembleMasterScene(
            GameObject mainBuildingPrefab, GameObject greenTowerPrefab, GameObject leftPaleWingPrefab,
            GameObject redRoofPrefab, GameObject rightLargeAcademicPrefab, GameObject backTowerPrefab,
            GameObject treeClusterPrefab, GameObject sportsCourtyardPrefab, GameObject motorbikeRowPrefab,
            GameObject blueBoothPrefab, GameObject coveredRampPrefab, GameObject monumentPrefab,
            GameObject flagPolesPrefab, GameObject mainEntrancePrefab)

        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject campusRoot = new GameObject("IUH_Campus");

            GameObject goBuildings = new GameObject("Buildings");
            goBuildings.transform.SetParent(campusRoot.transform);

            GameObject mainBuilding = PrefabUtility.InstantiatePrefab(mainBuildingPrefab) as GameObject;
            mainBuilding.transform.SetParent(goBuildings.transform);
            mainBuilding.transform.localPosition = new Vector3(0, 0, 75.0f);
            mainBuilding.transform.localRotation = Quaternion.Euler(0, 180, 0);

            GameObject backTower = PrefabUtility.InstantiatePrefab(backTowerPrefab) as GameObject;
            backTower.transform.SetParent(goBuildings.transform);
            backTower.transform.localPosition = new Vector3(0, 0, 108.0f);
            backTower.transform.localRotation = Quaternion.Euler(0, 180, 0);

            GameObject greenComplex = new GameObject("IUH_GreenGlassComplex");
            greenComplex.transform.SetParent(goBuildings.transform);

            GameObject greenTower = PrefabUtility.InstantiatePrefab(greenTowerPrefab) as GameObject;
            greenTower.transform.SetParent(greenComplex.transform);
            greenTower.transform.localPosition = new Vector3(-24.0f, 0, 64.0f);
            greenTower.transform.localRotation = Quaternion.Euler(0, 90, 0);

            GameObject leftWing = PrefabUtility.InstantiatePrefab(leftPaleWingPrefab) as GameObject;
            leftWing.transform.SetParent(greenComplex.transform);
            leftWing.transform.localPosition = new Vector3(-27.0f, 0, 52.0f);

            GameObject redRoof = PrefabUtility.InstantiatePrefab(redRoofPrefab) as GameObject;
            redRoof.transform.SetParent(greenComplex.transform);
            redRoof.transform.localPosition = new Vector3(-50.0f, 0, 10.0f);
            redRoof.transform.localRotation = Quaternion.Euler(0, 90, 0);

            GameObject rightBuilding = PrefabUtility.InstantiatePrefab(rightLargeAcademicPrefab) as GameObject;
            rightBuilding.name = "IUH_RightCluster";
            rightBuilding.transform.SetParent(goBuildings.transform);
            rightBuilding.transform.localPosition = new Vector3(14.0f, 0, 4.0f);

            GameObject goCourtyard = new GameObject("CentralCourtyard");
            goCourtyard.transform.SetParent(campusRoot.transform);

            GameObject courtAsphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            courtAsphalt.name = "Courtyard_Asphalt_Ground";
            courtAsphalt.transform.SetParent(goCourtyard.transform);
            courtAsphalt.transform.localPosition = new Vector3(0, -0.05f, 0);
            courtAsphalt.transform.localScale = new Vector3(68.0f, 0.1f, 150.0f);
            courtAsphalt.GetComponent<MeshRenderer>().sharedMaterial = m_AsphaltRoad;

            GameObject sportCourt = PrefabUtility.InstantiatePrefab(sportsCourtyardPrefab) as GameObject;
            sportCourt.transform.SetParent(goCourtyard.transform);
            sportCourt.transform.localPosition = new Vector3(0, 0, -10.0f);

            GameObject leftSidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftSidewalk.name = "Left_Flank_Sidewalk";
            leftSidewalk.transform.SetParent(goCourtyard.transform);
            leftSidewalk.transform.localPosition = new Vector3(-18.0f, 0.05f, 0);
            leftSidewalk.transform.localScale = new Vector3(4.0f, 0.1f, 140.0f);
            leftSidewalk.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            GameObject rightSidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightSidewalk.name = "Right_Flank_Sidewalk";
            rightSidewalk.transform.SetParent(goCourtyard.transform);
            rightSidewalk.transform.localPosition = new Vector3(17.0f, 0.05f, 0);
            rightSidewalk.transform.localScale = new Vector3(4.0f, 0.1f, 140.0f);
            rightSidewalk.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            GameObject mainBuildingSidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainBuildingSidewalk.name = "MainBuilding_Sidewalk";
            mainBuildingSidewalk.transform.SetParent(goCourtyard.transform);
            mainBuildingSidewalk.transform.localPosition = new Vector3(0, 0.05f, 60.0f);
            mainBuildingSidewalk.transform.localScale = new Vector3(50.0f, 0.1f, 10.0f);
            mainBuildingSidewalk.GetComponent<MeshRenderer>().sharedMaterial = m_ConcreteGround;

            GameObject roadArrow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            roadArrow.name = "Road_Arrow_Yellow";
            roadArrow.transform.SetParent(goCourtyard.transform);
            roadArrow.transform.localPosition = new Vector3(0, 0.02f, 42.0f);
            roadArrow.transform.localRotation = Quaternion.Euler(90, 0, 0);
            roadArrow.transform.localScale = new Vector3(3.5f, 3.5f, 1.0f);
            roadArrow.GetComponent<MeshRenderer>().sharedMaterial = m_RoadArrow;

            GameObject goLandscaping = new GameObject("Landscaping");
            goLandscaping.transform.SetParent(campusRoot.transform);

            GameObject treeCluster = PrefabUtility.InstantiatePrefab(treeClusterPrefab) as GameObject;
            treeCluster.transform.SetParent(goLandscaping.transform);
            treeCluster.transform.localPosition = new Vector3(0, 0, 52.0f);

            for (int i = -3; i <= 3; i++)
            {

                GameObject bL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bL.name = "Bush_L_" + i;
                bL.transform.SetParent(goLandscaping.transform);
                bL.transform.localPosition = new Vector3(-16.0f, 1.2f, i * 18.0f);
                bL.transform.localScale = new Vector3(2.4f, 2.4f, 2.4f);
                bL.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;

                GameObject bR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bR.name = "Bush_R_" + i;
                bR.transform.SetParent(goLandscaping.transform);
                bR.transform.localPosition = new Vector3(15.0f, 1.2f, i * 18.0f);
                bR.transform.localScale = new Vector3(2.4f, 2.4f, 2.4f);
                bR.GetComponent<MeshRenderer>().sharedMaterial = m_MatureFoliage;
            }

            GameObject monument = PrefabUtility.InstantiatePrefab(monumentPrefab) as GameObject;
            monument.transform.SetParent(goLandscaping.transform);
            monument.transform.localPosition = new Vector3(-3.0f, 0.1f, 57.0f);
            monument.transform.localRotation = Quaternion.Euler(0, 0, 0);

            GameObject flagPoles = PrefabUtility.InstantiatePrefab(flagPolesPrefab) as GameObject;
            flagPoles.transform.SetParent(goLandscaping.transform);
            flagPoles.transform.localPosition = new Vector3(10.0f, 0, 58.0f);
            flagPoles.transform.localRotation = Quaternion.Euler(0, 0, 0);

            GameObject goParking = new GameObject("Parking");
            goParking.transform.SetParent(campusRoot.transform);

            for (int k = -2; k <= 2; k++)
            {
                GameObject bikeRowL = PrefabUtility.InstantiatePrefab(motorbikeRowPrefab) as GameObject;
                bikeRowL.transform.SetParent(goParking.transform);
                bikeRowL.transform.localPosition = new Vector3(-19.0f, 0, k * 22.0f);
            }

            for (int k = -2; k <= 2; k++)
            {
                GameObject bikeRowR = PrefabUtility.InstantiatePrefab(motorbikeRowPrefab) as GameObject;
                bikeRowR.transform.SetParent(goParking.transform);
                bikeRowR.transform.localPosition = new Vector3(18.0f, 0, k * 22.0f);
            }

            GameObject booth1 = PrefabUtility.InstantiatePrefab(blueBoothPrefab) as GameObject;
            booth1.transform.SetParent(goParking.transform);
            booth1.transform.localPosition = new Vector3(-15.0f, 0, 24.0f);

            GameObject booth2 = PrefabUtility.InstantiatePrefab(blueBoothPrefab) as GameObject;
            booth2.transform.SetParent(goParking.transform);
            booth2.transform.localPosition = new Vector3(15.0f, 0, -32.0f);

            GameObject coveredRamp = PrefabUtility.InstantiatePrefab(coveredRampPrefab) as GameObject;
            coveredRamp.transform.SetParent(goParking.transform);
            coveredRamp.transform.localPosition = new Vector3(-21.0f, 0, 38.0f);

            GameObject goCity = new GameObject("BackgroundCity");
            goCity.transform.SetParent(campusRoot.transform);

            GameObject goEntranceGate = new GameObject("IUH_MainEntrance_Zone");
            goEntranceGate.transform.SetParent(campusRoot.transform);

            GameObject mainEntrance = PrefabUtility.InstantiatePrefab(mainEntrancePrefab) as GameObject;
            mainEntrance.transform.SetParent(goEntranceGate.transform);

            mainEntrance.transform.localPosition = new Vector3(0, 0, -72.0f);
            mainEntrance.transform.localRotation = Quaternion.identity;

            for (int bx = -2; bx <= 2; bx++)
            {
                for (int bz = -2; bz <= 2; bz++)
                {
                    if (Mathf.Abs(bx) <= 1 && Mathf.Abs(bz) <= 1) continue;

                    GameObject cityBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cityBlock.name = "CityBlock_" + bx + "_" + bz;
                    cityBlock.transform.SetParent(goCity.transform);
                    float h = Random.Range(12.0f, 28.0f);
                    cityBlock.transform.localPosition = new Vector3(bx * 90.0f, h * 0.5f, bz * 95.0f);
                    cityBlock.transform.localScale = new Vector3(65.0f, h, 70.0f);
                    cityBlock.GetComponent<MeshRenderer>().sharedMaterial = m_BgBuilding;
                }
            }

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
            sunObj.transform.rotation = Quaternion.Euler(50, 160, 0);

            GameObject probeObj = new GameObject("ReflectionProbe_Courtyard");
            probeObj.transform.SetParent(root.transform);
            probeObj.transform.localPosition = new Vector3(0, 15.0f, 0);
            ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
            probe.size = new Vector3(160.0f, 60.0f, 200.0f);
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;

            GameObject camObj = new GameObject("Main_Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<IUHInteractiveTourController>();
            cam.fieldOfView = 65.0f;
            camObj.transform.position = new Vector3(0, 95.0f, -55.0f);
            camObj.transform.rotation = Quaternion.Euler(48, 0, 0);

            GameObject camGround = new GameObject("Camera_Courtyard_Ground");
            camGround.transform.SetParent(root.transform);
            Camera cGround = camGround.AddComponent<Camera>();
            cGround.enabled = false;
            camGround.transform.position = new Vector3(0, 2.5f, 22.0f);
            camGround.transform.rotation = Quaternion.Euler(4, 0, 0);

            GameObject camMon = new GameObject("Camera_Front_Monument_CloseUp");
            camMon.transform.SetParent(root.transform);
            Camera cMon = camMon.AddComponent<Camera>();
            cMon.enabled = false;
            camMon.transform.position = new Vector3(-2.5f, 1.85f, 44.0f);
            camMon.transform.rotation = Quaternion.Euler(6, 12, 0);

            GameObject camTower = new GameObject("Camera_LookAt_GreenTower");
            camTower.transform.SetParent(root.transform);
            Camera cTower = camTower.AddComponent<Camera>();
            cTower.enabled = false;
            camTower.transform.position = new Vector3(10.0f, 1.8f, -10.0f);
            camTower.transform.rotation = Quaternion.Euler(10, 300, 0);

            GameObject camRight = new GameObject("Camera_LookAt_RightBuilding");
            camRight.transform.SetParent(root.transform);
            Camera cRight = camRight.AddComponent<Camera>();
            cRight.enabled = false;
            camRight.transform.position = new Vector3(-10.0f, 1.8f, -10.0f);
            camRight.transform.rotation = Quaternion.Euler(10, 60, 0);
        }

        #endregion
    }
}
