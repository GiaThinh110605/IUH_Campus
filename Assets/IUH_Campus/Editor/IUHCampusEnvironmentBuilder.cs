using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.UI;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.Editor
{
    /// <summary>
    /// Master Environment Artist & Campus Hub World Builder.
    /// Implements all 7 Key Campus Hub Zones matching the target design reference:
    /// - Khu vực A: Cổng trường (Gate 1 - Main Entrance, Welcome Banner, Security Booth)
    /// - Khu vực B: Sân trường & Quảng trường trung tâm (Grand Central Fountain, Plaza, Benches, Trees, Bulletin Board)
    /// - Khu vực C: Bãi xe & Căn tin (Canteen, Dining tables, Parasols, Night Food Shops, Parking bays)
    /// - Khu vực D: Ký túc xá (KTX Block D, Dorm entrance, Night lanterns, Horror hide-and-seek setup)
    /// - Khu vực E: Thư viện & Phòng học (Library Block A entrance, Object Hunt shelf, Photo tripod spot)
    /// - Khu vực F: Trạm phân loại rác thông minh & Công viên xanh (Green Campus 3-bin IoT station)
    /// - Khu vực G: Sân khấu lễ hội (Campus Festival 2026 concert stage, Truss, Sound, LED lighting)
    /// Plus 6 3D World-Space Floating Location Pins and Campus Hub Master HUD integration.
    /// </summary>
    public static class IUHCampusEnvironmentBuilder
    {
        private const string MatDir = "Assets/IUH_Campus/Materials/";

[MenuItem("IUH Campus/Redesign Shared Campus Hub (Full 7 Zones & HUD)")]
        public static void EnsureFullCampusRedesign()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "IUH_Campus_Main")
            {
                var scenePath = "Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity";
                if (System.IO.File.Exists(scenePath))
                {
                    activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }

            Debug.Log("[IUH Campus Redesign] Bắt đầu nâng cấp toàn diện khuôn viên IUH Campus Hub theo chuẩn thiết kế...");

            // 1. Setup Master Environment Root (Prevents duplicate objects)
            var hubRoot = GameObject.Find("IUH_SHARED_CAMPUS_HUB");
            if (hubRoot != null) UnityEngine.Object.DestroyImmediate(hubRoot);
            hubRoot = new GameObject("IUH_SHARED_CAMPUS_HUB");

            // 2. Setup Lighting (Stylized Realistic / Android Optimized)
            SetupDaytimeLighting(hubRoot.transform);

            // 3. Build 7 Distinct Zones
            BuildZoneA_MainGate(hubRoot.transform);
            BuildZoneB_CentralPlaza_Fountain(hubRoot.transform);
            BuildZoneC_Canteen_Parking(hubRoot.transform);
            BuildZoneD_KTX_Dormitory(hubRoot.transform);
            BuildZoneE_Library(hubRoot.transform);
            BuildZoneF_GreenCampus(hubRoot.transform);
            BuildZoneG_FestivalStage(hubRoot.transform);

            // 4. Setup 6 3D World-Space Floating Location Pins
            Build6WorldLocationPins(hubRoot.transform);

            // 5. Ensure Master Campus Hub HUD on Canvas
            SetupMasterCampusHubHUD();

            // 6. Ensure IUHGameFlowManager is active & configured
            IUHGameFlowRefactorSetup.EnsureGameFlowSetup();

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[IUH Campus Redesign] HOÀN TẤT XUẤT SẮC! Khuôn viên chung 7 khu vực, Landmark đài phun nước, 3D Location Pins và Master HUD đã sẵn sàng.");
        }

        private static Material GetMat(string matName)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MatDir + matName + ".mat");
            if (mat != null) return mat;

            // Fallback material
            var newMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            newMat.name = matName;
            AssetDatabase.CreateAsset(newMat, MatDir + matName + ".mat");
            return newMat;
        }

        // ================= 1. LIGHTING SETUP =================
        private static void SetupDaytimeLighting(Transform parent)
        {
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light dirLight = null;
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    dirLight = l;
                    break;
                }
            }

            if (dirLight == null)
            {
                var go = new GameObject("MainDirectionalLight");
                go.transform.SetParent(parent, false);
                dirLight = go.AddComponent<Light>();
                dirLight.type = LightType.Directional;
            }

            // Warm golden sunlight
            dirLight.color = new Color(1.0f, 0.96f, 0.88f);
            dirLight.intensity = 1.15f;
            dirLight.shadows = LightShadows.Soft;
            dirLight.shadowNormalBias = 0.4f;
            dirLight.shadowBias = 0.05f;
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ambient sky lighting (natural warm gradient)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.45f, 0.62f, 0.88f);
            RenderSettings.ambientEquatorColor = new Color(0.60f, 0.68f, 0.75f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.42f, 0.35f);

            // Fog: light atmospheric haze
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.0025f;
            RenderSettings.fogColor = new Color(0.65f, 0.78f, 0.92f);
        }

        // ================= 2. KHU VỰC A: CỔNG TRƯỜNG =================
        private static void BuildZoneA_MainGate(Transform parent)
        {
            var zoneGO = new GameObject("Zone_A_MainGate");
            zoneGO.transform.SetParent(parent);
            zoneGO.transform.position = new Vector3(0.0f, 0.05f, -72.0f);

            var stoneMat = GetMat("M_IUH_Gate_Stone");
            var goldMat = GetMat("M_IUH_Gate_GoldText");
            var bannerMat = GetMat("M_IUH_Gate_WelcomeBanner");
            var darkMat = GetMat("M_IUH_DarkMetal");

            // Left & Right Grand Pillars
            var leftPillar = CreateCube(zoneGO.transform, "LeftPillar", new Vector3(-6.5f, 2.8f, 0f), new Vector3(1.4f, 5.6f, 1.4f), stoneMat);
            var rightPillar = CreateCube(zoneGO.transform, "RightPillar", new Vector3(6.5f, 2.8f, 0f), new Vector3(1.4f, 5.6f, 1.4f), stoneMat);

            // Arch Crossbeam
            var archBeam = CreateCube(zoneGO.transform, "ArchCrossbeam", new Vector3(0f, 5.4f, 0f), new Vector3(14.4f, 0.9f, 1.2f), stoneMat);

            // Gold Lettering Sign Board
            var signBoard = CreateCube(zoneGO.transform, "GateSignBoard", new Vector3(0f, 5.4f, 0.65f), new Vector3(11.5f, 0.65f, 0.1f), goldMat);

            // Red Welcome Banner
            var banner = CreateCube(zoneGO.transform, "WelcomeBanner", new Vector3(0f, 4.4f, 0f), new Vector3(11.0f, 0.8f, 0.05f), bannerMat);

            // Security Guard Booth
            var booth = CreateCube(zoneGO.transform, "SecurityBooth", new Vector3(9.2f, 1.6f, 2.0f), new Vector3(2.6f, 3.2f, 2.6f), stoneMat);
            var boothRoof = CreateCube(booth.transform, "Roof", new Vector3(0f, 0.55f, 0f), new Vector3(1.2f, 0.15f, 1.2f), darkMat);

            // Flower Planter Rows
            var leftFlowers = CreateCube(zoneGO.transform, "LeftFlowers", new Vector3(-8.5f, 0.35f, 1.5f), new Vector3(3.2f, 0.7f, 1.0f), GetMat("M_IUH_Flower_Orange"));
            var rightFlowers = CreateCube(zoneGO.transform, "RightFlowers", new Vector3(8.5f, 0.35f, 1.5f), new Vector3(3.2f, 0.7f, 1.0f), GetMat("M_IUH_Flower_Orange"));
        }

        // ================= 3. KHU VỰC B: SÂN TRƯỜNG & ĐÀI PHUN NƯỚC TRUNG TÂM =================
        private static void BuildZoneB_CentralPlaza_Fountain(Transform parent)
        {
            var zoneGO = new GameObject("Zone_B_CentralPlaza_Fountain");
            zoneGO.transform.SetParent(parent);
            zoneGO.transform.position = new Vector3(0.0f, 0.05f, 15.0f);

            var stoneMat = GetMat("M_IUH_Main_Stone");
            var concreteMat = GetMat("M_IUH_Main_Concrete");
            var waterMat = GetMat("M_IUH_Waypoint_Holo");
            var benchMat = GetMat("M_IUH_Bench_Granite");
            var foliageMat = GetMat("M_IUH_Foliage_Tree");
            var woodMat = GetMat("M_IUH_Tree_Bark");

            // 1. Plaza Cobblestone Base Ring
            var plazaBase = CreateCylinder(zoneGO.transform, "PlazaRing", new Vector3(0f, 0.02f, 0f), new Vector3(26.0f, 0.04f, 26.0f), concreteMat);

            // 2. Outer Fountain Basin Wall
            var fountainBasin = CreateCylinder(zoneGO.transform, "FountainBasin", new Vector3(0f, 0.35f, 0f), new Vector3(12.0f, 0.65f, 12.0f), stoneMat);

            // 3. Water Surface (Gleaming Blue)
            var water = CreateCylinder(zoneGO.transform, "WaterSurface", new Vector3(0f, 0.55f, 0f), new Vector3(11.2f, 0.1f, 11.2f), waterMat);

            // 4. Center Multi-Tiered Pedestal & Monument
            var tier1 = CreateCylinder(zoneGO.transform, "CenterTier1", new Vector3(0f, 0.9f, 0f), new Vector3(5.2f, 0.8f, 5.2f), stoneMat);
            var tier2 = CreateCylinder(zoneGO.transform, "CenterTier2", new Vector3(0f, 1.6f, 0f), new Vector3(3.2f, 0.9f, 3.2f), stoneMat);

            // Center Spire / IUH Gear Emblem Monument
            var spire = CreateCube(zoneGO.transform, "IUH_MonumentSpire", new Vector3(0f, 2.8f, 0f), new Vector3(1.2f, 2.2f, 1.2f), GetMat("M_IUH_Monument_Letter"));
            var crest = CreateCylinder(spire.transform, "GearCrest", new Vector3(0f, 0.6f, 0f), new Vector3(1.8f, 0.2f, 1.8f), GetMat("M_IUH_Logo_Crest"));

            // Gentle fountain point light
            var fLight = zoneGO.AddComponent<Light>();
            fLight.type = LightType.Point;
            fLight.color = new Color(0.2f, 0.85f, 1.0f);
            fLight.range = 14.0f;
            fLight.intensity = 1.8f;

            // 5. Encircling Granite Benches (8 directions)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                float dist = 9.5f;
                Vector3 bPos = new Vector3(Mathf.Sin(angle) * dist, 0.3f, Mathf.Cos(angle) * dist);
                Quaternion bRot = Quaternion.Euler(0f, i * 45f + 90f, 0f);

                var bench = CreateCube(zoneGO.transform, $"Bench_{i + 1}", bPos, new Vector3(1.8f, 0.45f, 0.65f), benchMat);
                bench.transform.rotation = bRot;
            }

            // 6. Plaza Greenery Trees (4 diagonal corners)
            Vector3[] treeOffsets = new Vector3[] {
                new Vector3(-11f, 0f, 11f), new Vector3(11f, 0f, 11f),
                new Vector3(-11f, 0f, -11f), new Vector3(11f, 0f, -11f)
            };
            foreach (var tPos in treeOffsets)
            {
                var tree = new GameObject("PlazaTree");
                tree.transform.SetParent(zoneGO.transform, false);
                tree.transform.localPosition = tPos;

                var trunk = CreateCylinder(tree.transform, "Trunk", new Vector3(0f, 1.8f, 0f), new Vector3(0.5f, 3.6f, 0.5f), woodMat);
                var canopy = CreateSphere(tree.transform, "Canopy", new Vector3(0f, 4.2f, 0f), new Vector3(4.2f, 3.5f, 4.2f), foliageMat);
            }

            // 7. Campus Bulletin Board (Interactive Mission Noticeboard)
            var boardGO = new GameObject("CampusMissionBoard");
            boardGO.transform.SetParent(zoneGO.transform, false);
            boardGO.transform.localPosition = new Vector3(-8.5f, 0f, 0f);
            boardGO.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            var postL = CreateCube(boardGO.transform, "PostL", new Vector3(-1.4f, 1.3f, 0f), new Vector3(0.18f, 2.6f, 0.18f), woodMat);
            var postR = CreateCube(boardGO.transform, "PostR", new Vector3(1.4f, 1.3f, 0f), new Vector3(0.18f, 2.6f, 0.18f), woodMat);
            var boardPanel = CreateCube(boardGO.transform, "Panel", new Vector3(0f, 1.8f, 0f), new Vector3(2.8f, 1.4f, 0.1f), GetMat("M_IUH_Sign_NoticeBoard"));
        }

        // ================= 4. KHU VỰC C: BÃI XE & CĂN TIN =================
        private static void BuildZoneC_Canteen_Parking(Transform parent)
        {
            var zoneGO = new GameObject("Zone_C_Canteen_Parking");
            zoneGO.transform.SetParent(parent);
            zoneGO.transform.position = new Vector3(20.0f, 0.05f, -20.0f);

            var yellowAwning = GetMat("M_IUH_Shop_YellowAwning");
            var cyanAwning = GetMat("M_IUH_Shop_CyanAwning");
            var woodMat = GetMat("M_IUH_Shop_Wood");
            var parkingLineMat = GetMat("M_IUH_Parking_Line");

            // Canteen Terrace Counter
            var counter = CreateCube(zoneGO.transform, "OrderCounter", new Vector3(-4.5f, 1.2f, 0f), new Vector3(7.0f, 2.4f, 2.5f), woodMat);
            var awning = CreateCube(counter.transform, "Awning", new Vector3(0f, 0.65f, 0.35f), new Vector3(1.08f, 0.2f, 1.35f), yellowAwning);

            // Outdoor Dining Tables & Parasols (3 sets)
            for (int i = 0; i < 3; i++)
            {
                var tableSet = new GameObject($"DiningTable_{i + 1}");
                tableSet.transform.SetParent(zoneGO.transform, false);
                tableSet.transform.localPosition = new Vector3(-8.5f + i * 3.5f, 0f, 4.5f);

                var table = CreateCylinder(tableSet.transform, "Table", new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.85f, 1.4f), woodMat);
                var parasol = CreateCylinder(tableSet.transform, "Parasol", new Vector3(0f, 2.2f, 0f), new Vector3(2.6f, 0.3f, 2.6f), cyanAwning);
            }

            // Motorbike Parking Area Bays
            var parkBay = new GameObject("ParkingBays");
            parkBay.transform.SetParent(zoneGO.transform, false);
            parkBay.transform.localPosition = new Vector3(5.5f, 0.02f, 1.5f);

            for (int i = 0; i < 6; i++)
            {
                var line = CreateCube(parkBay.transform, $"Line_{i}", new Vector3(i * 1.5f, 0f, 0f), new Vector3(0.12f, 0.02f, 3.2f), parkingLineMat);
            }
        }

        // ================= 5. KHU VỰC D: KÝ TÚC XÁ BLOCK D =================
        private static void BuildZoneD_KTX_Dormitory(Transform parent)
        {
            var zoneGO = new GameObject("Zone_D_KTX_Dormitory");
            zoneGO.transform.SetParent(parent);
            zoneGO.transform.position = new Vector3(-35.0f, 0.05f, 25.0f);

            var stoneMat = GetMat("M_IUH_Gate_Stone");
            var darkMat = GetMat("M_IUH_DarkMetal");

            // KTX Entrance Foyer Portal
            var portalL = CreateCube(zoneGO.transform, "FoyerL", new Vector3(-3.5f, 2.0f, 0f), new Vector3(0.8f, 4.0f, 0.8f), stoneMat);
            var portalR = CreateCube(zoneGO.transform, "FoyerR", new Vector3(3.5f, 2.0f, 0f), new Vector3(0.8f, 4.0f, 0.8f), stoneMat);
            var canopy = CreateCube(zoneGO.transform, "Canopy", new Vector3(0f, 3.8f, 1.2f), new Vector3(8.2f, 0.4f, 2.8f), darkMat);

            // Night Lantern (Warm Light)
            var lantern = new GameObject("Lantern");
            lantern.transform.SetParent(zoneGO.transform, false);
            lantern.transform.localPosition = new Vector3(0f, 3.2f, 0.5f);
            var light = lantern.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.65f, 0.3f);
            light.range = 8.0f;
            light.intensity = 1.4f;
        }

        // ================= 6. KHU VỰC E: THƯ VIỆN TRUNG TÂM =================
        private static void BuildZoneE_Library(Transform parent)
        {
            var zoneGO = new GameObject("Zone_E_Library");
            zoneGO.transform.SetParent(parent);
            zoneGO.transform.position = new Vector3(24.0f, 0.05f, 30.0f);

            var glassMat = GetMat("M_IUH_Main_Glass");
            var concreteMat = GetMat("M_IUH_Academic_Concrete");

            // Library Entrance Canopy & Columns
            var colL = CreateCylinder(zoneGO.transform, "ColL", new Vector3(-4.0f, 2.4f, 0f), new Vector3(0.7f, 4.8f, 0.7f), concreteMat);
            var colR = CreateCylinder(zoneGO.transform, "ColR", new Vector3(4.0f, 2.4f, 0f), new Vector3(0.7f, 4.8f, 0.7f), concreteMat);
            var architrave = CreateCube(zoneGO.transform, "Architrave", new Vector3(0f, 4.8f, 0f), new Vector3(9.5f, 0.8f, 1.4f), concreteMat);

            // Library Sign Board
            var sign = CreateCube(zoneGO.transform, "SignBoard", new Vector3(0f, 4.8f, 0.75f), new Vector3(7.2f, 0.5f, 0.1f), GetMat("M_IUH_Academic_TopSign"));

            // Photo Tripod Stand Marker (For Mini-game 2.4)
            var tripod = new GameObject("PhotoTripodMarker");
            tripod.transform.SetParent(zoneGO.transform, false);
            tripod.transform.localPosition = new Vector3(0f, 0f, 4.5f);
            var leg1 = CreateCylinder(tripod.transform, "Leg1", new Vector3(0f, 0.65f, 0f), new Vector3(0.08f, 1.3f, 0.08f), GetMat("M_IUH_DarkMetal"));
            var cameraBox = CreateCube(tripod.transform, "CameraBody", new Vector3(0f, 1.35f, 0f), new Vector3(0.35f, 0.25f, 0.28f), GetMat("M_IUH_LightMetal"));
        }

        // ================= 7. KHU VỰC F: TRẠM PHÂN LOẠI RÁC XANH =================
        private static void BuildZoneF_GreenCampus(Transform parent)
        {
            var zoneGO = new GameObject("Zone_F_GreenCampus");
            zoneGO.transform.SetParent(parent);
            zoneGO.transform.position = new Vector3(-12.0f, 0.05f, -35.0f);

            var woodMat = GetMat("M_IUH_Station_Wood");
            var greenGlow = GetMat("M_IUH_Station_GreenGlow");

            // Smart Station Enclosure Base
            var stationBase = CreateCube(zoneGO.transform, "StationBase", new Vector3(0f, 0.15f, 0f), new Vector3(4.2f, 0.3f, 1.6f), woodMat);

            // 3 Color Bins (Organic - Green, Recyclable - Blue, Hazardous - Yellow)
            var binGreen = CreateCube(zoneGO.transform, "BinOrganic_Green", new Vector3(-1.2f, 0.75f, 0f), new Vector3(0.85f, 1.1f, 0.85f), GetMat("M_IUH_Station_GreenGlow"));
            var binBlue = CreateCube(zoneGO.transform, "BinRecycle_Blue", new Vector3(0f, 0.75f, 0f), new Vector3(0.85f, 1.1f, 0.85f), GetMat("M_IUH_Waypoint_Holo"));
            var binYellow = CreateCube(zoneGO.transform, "BinHazard_Yellow", new Vector3(1.2f, 0.75f, 0f), new Vector3(0.85f, 1.1f, 0.85f), GetMat("M_IUH_Shop_YellowAwning"));

            // IoT Sensor Solar Mast & Adafruit Display
            var mast = CreateCylinder(zoneGO.transform, "IoTMast", new Vector3(2.4f, 1.6f, 0f), new Vector3(0.12f, 3.2f, 0.12f), GetMat("M_IUH_DarkMetal"));
            var solarPanel = CreateCube(mast.transform, "SolarPanel", new Vector3(0f, 1.55f, 0f), new Vector3(0.8f, 0.05f, 0.6f), GetMat("M_IUH_DarkMetal"));
            solarPanel.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);

            var ledDisplay = CreateCube(mast.transform, "AdafruitDisplay", new Vector3(-0.4f, 0.6f, 0f), new Vector3(0.7f, 0.4f, 0.08f), GetMat("M_IUH_Station_GreenGlow"));
        }

        // ================= 8. KHU VỰC G: SÂN KHẤU LỄ HỘI =================
        private static void BuildZoneG_FestivalStage(Transform parent)
        {
            var zoneGO = new GameObject("Zone_G_FestivalStage");
            zoneGO.transform.SetParent(parent);
            zoneGO.transform.position = new Vector3(0.0f, 0.05f, -10.0f);

            var darkMat = GetMat("M_IUH_DarkMetal");
            var woodMat = GetMat("M_IUH_Shop_Wood");
            var pinkMat = GetMat("M_IUH_Cloth_Red");

            // Main Stage Platform (Elevated)
            var stage = CreateCube(zoneGO.transform, "StageDeck", new Vector3(0f, 0.6f, 0f), new Vector3(12.0f, 1.2f, 7.5f), woodMat);

            // Stage Backdrop Wall
            var backdrop = CreateCube(zoneGO.transform, "Backdrop", new Vector3(0f, 3.2f, -3.4f), new Vector3(11.2f, 4.4f, 0.25f), darkMat);

            // Concert Truss Rig (Left & Right towers + top crossbeam)
            var trussL = CreateCube(zoneGO.transform, "TrussL", new Vector3(-5.6f, 3.2f, 0f), new Vector3(0.5f, 6.4f, 0.5f), darkMat);
            var trussR = CreateCube(zoneGO.transform, "TrussR", new Vector3(5.6f, 3.2f, 0f), new Vector3(0.5f, 6.4f, 0.5f), darkMat);
            var trussTop = CreateCube(zoneGO.transform, "TrussTop", new Vector3(0f, 6.2f, 0f), new Vector3(11.8f, 0.5f, 0.5f), darkMat);

            // Left & Right Audio Speaker Stacks
            var speakerL = CreateCube(zoneGO.transform, "SpeakerL", new Vector3(-4.8f, 2.0f, 2.2f), new Vector3(0.9f, 2.2f, 0.7f), darkMat);
            var speakerR = CreateCube(zoneGO.transform, "SpeakerR", new Vector3(4.8f, 2.0f, 2.2f), new Vector3(0.9f, 2.2f, 0.7f), darkMat);

            // Stage Spotlights (Pink & Cyan)
            var spotL = new GameObject("SpotlightL");
            spotL.transform.SetParent(zoneGO.transform, false);
            spotL.transform.localPosition = new Vector3(-3.5f, 5.8f, 0f);
            var sl1 = spotL.AddComponent<Light>();
            sl1.type = LightType.Spot;
            sl1.color = new Color(0.95f, 0.2f, 0.6f);
            sl1.range = 14.0f;
            sl1.spotAngle = 45f;
            spotL.transform.rotation = Quaternion.Euler(45f, 25f, 0f);

            var spotR = new GameObject("SpotlightR");
            spotR.transform.SetParent(zoneGO.transform, false);
            spotR.transform.localPosition = new Vector3(3.5f, 5.8f, 0f);
            var sl2 = spotR.AddComponent<Light>();
            sl2.type = LightType.Spot;
            sl2.color = new Color(0.1f, 0.85f, 1.0f);
            sl2.range = 14.0f;
            sl2.spotAngle = 45f;
            spotR.transform.rotation = Quaternion.Euler(45f, -25f, 0f);
        }

        // ================= 9. 6 3D WORLD-SPACE FLOATING LOCATION PINS =================
        private static void Build6WorldLocationPins(Transform parent)
        {
            var pinsRoot = new GameObject("WorldLocationPins");
            pinsRoot.transform.SetParent(parent);

            // 1. CỔNG TRƯỜNG
            CreatePin(pinsRoot.transform, "Pin_A_MainGate", new Vector3(0f, 0.05f, -70f), "CỔNG TRƯỜNG", "Bắt đầu hành trình", "🏛️", new Color(0.0f, 0.75f, 1.0f), -1, 4.5f);

            // 2. SÂN TRƯỜNG & ĐÀI PHUN NƯỚC
            CreatePin(pinsRoot.transform, "Pin_B_CentralPlaza", new Vector3(0f, 0.05f, 15f), "SÂN TRƯỜNG", "Quảng trường & Đài phun nước", "⛲", new Color(0.1f, 0.85f, 0.45f), -1, 4.8f);

            // 3. CĂN TIN & BÃI XE (Mini-game 1: Grab Campus)
            CreatePin(pinsRoot.transform, "Pin_C_Canteen_Grab", new Vector3(20f, 0.05f, -20f), "CĂN TIN & BÃI XE", "Nhận đơn Grab Campus 2.1", "🛵", new Color(1.0f, 0.65f, 0.1f), 0, 4.2f);

            // 4. KÝ TÚC XÁ BLOCK D (Mini-game 2: Trốn ma)
            CreatePin(pinsRoot.transform, "Pin_D_KTX_Horror", new Vector3(-35f, 0.05f, 25f), "KÝ TÚC XÁ", "Trốn tìm với ma 2.2", "👻", new Color(0.7f, 0.35f, 0.95f), 1, 4.5f);

            // 5. THƯ VIỆN TRUNG TÂM (Mini-game 3: AI Object & 4: Chụp ảnh)
            CreatePin(pinsRoot.transform, "Pin_E_Library", new Vector3(24f, 0.05f, 30f), "THƯ VIỆN TRUNG TÂM", "AI Object 2.3 & Chụp ảnh 2.4", "📚", new Color(0.2f, 0.85f, 1.0f), 2, 4.8f);

            // 6. SÂN LỄ HỘI (Mini-game 6: Campus Festival)
            CreatePin(pinsRoot.transform, "Pin_G_Festival", new Vector3(0f, 0.05f, -10f), "SÂN LỄ HỘI", "Campus Festival 2026", "🎪", new Color(0.95f, 0.3f, 0.65f), 5, 4.6f);
        }

        private static void CreatePin(Transform parent, string name, Vector3 pos, string title, string sub, string emoji, Color col, int gameIdx, float height)
        {
            var pinGO = new GameObject(name);
            pinGO.transform.SetParent(parent);
            pinGO.transform.position = pos;

            var pin = pinGO.AddComponent<IUHWorldLocationPin>();
            pin.Setup(title, sub, emoji, col, gameIdx, height);
        }

        // ================= 10. CAMPUS HUB MASTER HUD SETUP =================
        private static void SetupMasterCampusHubHUD()
        {
            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO == null) return;

            var hubHUD = canvasGO.GetComponent<IUHCampusHubHUD>();
            if (hubHUD == null)
            {
                hubHUD = canvasGO.AddComponent<IUHCampusHubHUD>();
            }
        }

        // ================= PRIMITIVE HELPERS =================
        private static GameObject CreateCube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateSphere(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }
    }
}
