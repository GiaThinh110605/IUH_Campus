using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.Editor
{
    public static class IUHRightClusterInteriorBuilder
    {
        private const string MatPath = "Assets/IUH_Campus/Materials/";
        private const string PrefabPath = "Assets/IUH_Campus/Prefabs/Architecture/";
        private const string ScreenshotDir = "Assets/IUH_Campus/Screenshots/";
        private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity-ide\brain\90eb314c-5d87-4a56-9144-aab594d4439c\";

        // Interior Materials
        private static Material m_BlackMarble;
        private static Material m_LightTile;
        private static Material m_LightWood;
        private static Material m_SageGreen;
        private static Material m_CyanSofa;
        private static Material m_LEDDisplay;
        private static Material m_ScreenPC;
        private static Material m_Poster1;
        private static Material m_Poster2;
        private static Material m_GlassFrosted;
        private static Material m_GlassClear;
        private static Material m_PerforatedMetal;
        private static Material m_MatteWhite;
        private static Material m_DarkMullion;
        private static Material m_Chrome;
        private static Material m_NavyChair;
        private static Material m_CeilingGrid;
        private static Material m_FoliageTree;
        private static Material m_AvatarSkin;
        private static Material m_AvatarJacket;

        [MenuItem("IUH Campus/Build & Capture Interior Views")]
        public static void BuildAndCaptureInterior()
        {
            Debug.Log("[IUH Interior] Starting Full Interior Construction & Capture...");

            // 1. Generate Textures and Materials
            IUHInteriorAssetGenerator.GenerateAllInteriorAssets();
            LoadInteriorMaterials();

            // 2. Open Right Cluster Scene if not already active
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.path != "Assets/IUH_Campus/Scenes/IUH_RightCluster.unity")
            {
                activeScene = EditorSceneManager.OpenScene("Assets/IUH_Campus/Scenes/IUH_RightCluster.unity", OpenSceneMode.Single);
            }

            // 3. Remove old interior if present
            GameObject existingInterior = GameObject.Find("IUH_RightCluster_Interior");
            if (existingInterior != null)
            {
                GameObject.DestroyImmediate(existingInterior);
            }

            // 4. Construct Complete Interior Complex
            GameObject interiorRoot = BuildInteriorHierarchy();

            // 5. Save Prefab
            if (!Directory.Exists(PrefabPath)) Directory.CreateDirectory(PrefabPath);
            string prefabFilePath = PrefabPath + "IUH_RightCluster_Interior.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(interiorRoot, prefabFilePath, InteractionMode.AutomatedAction);
            Debug.Log("[IUH Interior] Saved prefab to " + prefabFilePath);

            // 6. Disable unbaked ReflectionProbes in the scene to prevent URP ReflectionProbeManager NullReferenceException
            foreach (var probe in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                probe.enabled = false;
            }

            // 7. Capture Screenshots from all 4 Interior Cameras
            CaptureAllInteriorCameras(interiorRoot);

            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.Refresh();
            Debug.Log("[IUH Interior] Interior Construction & Captures Completed Successfully!");
        }

        private static void LoadInteriorMaterials()
        {
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
            m_PerforatedMetal = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_PerforatedMetal.mat");
            m_MatteWhite = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_MatteWhite.mat");
            m_DarkMullion = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_DarkMullion.mat");
            m_Chrome = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_Chrome.mat");
            m_NavyChair = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_NavyChair.mat");
            m_CeilingGrid = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_CeilingGrid.mat");
            m_FoliageTree = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Foliage_Tree.mat");
            m_AvatarSkin = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Avatar_Skin.mat");
            m_AvatarJacket = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Avatar_Jacket.mat");
        }

        public static GameObject BuildInteriorHierarchy()
        {
            // Dedicated clean interior coordinate space at X: 100, Y: 0, Z: 0
            GameObject root = new GameObject("IUH_RightCluster_Interior");
            root.transform.position = new Vector3(100.0f, 0.0f, 0.0f);
            root.transform.rotation = Quaternion.identity;

            // Structure Envelope
            GameObject structure = new GameObject("Interior_Structure");
            structure.transform.SetParent(root.transform, false);

            // Zone A: Front Desk & Reception Lobby
            GameObject zoneA = new GameObject("Zone_A_FrontDesk_Lobby");
            zoneA.transform.SetParent(structure.transform, false);
            BuildZoneA(zoneA);

            // Zone B: Waiting Lounge & Media Screen
            GameObject zoneB = new GameObject("Zone_B_WaitingLounge_Media");
            zoneB.transform.SetParent(structure.transform, false);
            BuildZoneB(zoneB);

            // Zone C: Training & Computer Lab
            GameObject zoneC = new GameObject("Zone_C_Training_ComputerLab");
            zoneC.transform.SetParent(structure.transform, false);
            BuildZoneC(zoneC);

            // Circulation & Corridor Spine
            GameObject corridor = new GameObject("Zone_Circulation_Corridor");
            corridor.transform.SetParent(structure.transform, false);
            BuildCorridor(corridor);

            // Exterior Entrance Plaza & Sliding Doors
            GameObject entrance = new GameObject("Zone_Exterior_Entrance");
            entrance.transform.SetParent(structure.transform, false);
            BuildExteriorEntrancePlaza(entrance);

            // Lighting & Reflection Probes
            GameObject lighting = new GameObject("Interior_Lighting");
            lighting.transform.SetParent(root.transform, false);
            BuildInteriorLighting(lighting);

            // Cameras
            GameObject cameras = new GameObject("Interior_Cameras");
            cameras.transform.SetParent(root.transform, false);
            BuildInteriorCameras(cameras);

            // Player Exploration System (Character Controller, Spawns, Camera System)
            GameObject playerSystem = new GameObject("PlayerSystem");
            playerSystem.transform.SetParent(root.transform, false);
            BuildPlayerSystem(playerSystem);

            return root;
        }

        // ================= ZONE A: FRONT DESK & RECEPTION =================
        // Positioned around X: [-12, -2], Z: [2, 12], Y: [0, 3.6]
        private static void BuildZoneA(GameObject parent)
        {
            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_LightTile_A";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = new Vector3(-7.0f, -0.05f, 7.0f);
            floor.transform.localScale = new Vector3(10.0f, 0.1f, 10.0f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Ceiling (Slatted Timber Baffle Ceiling)
            GameObject ceiling = new GameObject("Ceiling_Baffle_A");
            ceiling.transform.SetParent(parent.transform, false);
            for (int i = 0; i < 20; i++)
            {
                float z = 2.5f + i * 0.48f;
                GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slat.name = "CeilingSlat_" + i;
                slat.transform.SetParent(ceiling.transform, false);
                slat.transform.localPosition = new Vector3(-7.0f, 3.55f, z);
                slat.transform.localScale = new Vector3(9.8f, 0.12f, 0.08f);
                slat.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
            }

            // Suspended Linear LED Strip Fixture & Soft Ambient Light
            GameObject lightBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightBar.name = "LinearLight_Center";
            lightBar.transform.SetParent(ceiling.transform, false);
            lightBar.transform.localPosition = new Vector3(-7.0f, 3.35f, 7.0f);
            lightBar.transform.localScale = new Vector3(0.2f, 0.08f, 7.5f);
            lightBar.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            Light ptA = lightBar.AddComponent<Light>();
            ptA.type = LightType.Point;
            ptA.color = new Color(1.0f, 0.98f, 0.94f);
            ptA.range = 12.0f;
            ptA.intensity = 1.4f;

            // Walls:
            // Rear North Wall (behind desk): Z = 12
            GameObject rearWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearWall.name = "Wall_North_A";
            rearWall.transform.SetParent(parent.transform, false);
            rearWall.transform.localPosition = new Vector3(-7.0f, 1.75f, 12.05f);
            rearWall.transform.localScale = new Vector3(10.0f, 3.5f, 0.1f);
            rearWall.GetComponent<MeshRenderer>().sharedMaterial = m_SageGreen;

            // West Wall (Left side with sofa): X = -12
            GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWall.name = "Wall_West_A";
            westWall.transform.SetParent(parent.transform, false);
            westWall.transform.localPosition = new Vector3(-12.05f, 1.75f, 7.0f);
            westWall.transform.localScale = new Vector3(0.1f, 3.5f, 10.0f);
            westWall.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Timber feature slats on West wall
            for (int s = 0; s < 12; s++)
            {
                float z = 3.5f + s * 0.4f;
                GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slat.name = "WoodSlat_West_" + s;
                slat.transform.SetParent(westWall.transform, false);
                slat.transform.localPosition = new Vector3(0.08f, 0.0f, (z - 7.0f) / 10.0f);
                slat.transform.localScale = new Vector3(0.08f, 1.0f, 0.025f);
                slat.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
            }

            // East Partition towards Lounge: X = -2 (Partial Wall & Glass Vision Panel)
            GameObject glassEast = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glassEast.name = "GlassPartition_East_A";
            glassEast.transform.SetParent(parent.transform, false);
            glassEast.transform.localPosition = new Vector3(-2.0f, 1.75f, 7.0f);
            glassEast.transform.localScale = new Vector3(0.08f, 3.5f, 7.0f);
            glassEast.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            // ── Curved Front Desk Reception Counter ──
            // Positioned at X: -5.5, Z: 8.5
            GameObject desk = new GameObject("Curved_FrontDesk");
            desk.transform.SetParent(parent.transform, false);
            desk.transform.localPosition = new Vector3(-5.5f, 0.0f, 8.5f);

            // Curved Main Body (5 angled faceted segments forming smooth curve)
            int deskSegments = 6;
            float arcRadius = 2.8f;
            float startAngle = 100.0f;
            float endAngle = 190.0f;

            for (int i = 0; i < deskSegments; i++)
            {
                float a0 = Mathf.Lerp(startAngle, endAngle, (float)i / deskSegments) * Mathf.Deg2Rad;
                float a1 = Mathf.Lerp(startAngle, endAngle, (float)(i + 1) / deskSegments) * Mathf.Deg2Rad;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * arcRadius, 0.55f, Mathf.Sin(a0) * arcRadius);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * arcRadius, 0.55f, Mathf.Sin(a1) * arcRadius);
                Vector3 mid = (p0 + p1) * 0.5f;
                float segLen = Vector3.Distance(p0, p1);
                float ang = Mathf.Atan2(p1.z - p0.z, p1.x - p0.x) * Mathf.Rad2Deg;

                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "DeskSeg_" + i;
                seg.transform.SetParent(desk.transform, false);
                seg.transform.localPosition = mid;
                seg.transform.localRotation = Quaternion.Euler(0, -ang, 0);
                seg.transform.localScale = new Vector3(segLen + 0.02f, 1.1f, 0.55f);
                seg.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

                // Glass sneeze guard on top of desk
                GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                guard.name = "GlassShield_" + i;
                guard.transform.SetParent(desk.transform, false);
                guard.transform.localPosition = mid + new Vector3(0, 0.72f, 0);
                guard.transform.localRotation = Quaternion.Euler(0, -ang, 0);
                guard.transform.localScale = new Vector3(segLen, 0.32f, 0.02f);
                guard.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;
            }

            // Desktop Counter Surface (Top Ledger)
            GameObject counterTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterTop.name = "CounterTop";
            counterTop.transform.SetParent(desk.transform, false);
            counterTop.transform.localPosition = new Vector3(-1.8f, 1.12f, 1.8f);
            counterTop.transform.localRotation = Quaternion.Euler(0, -35f, 0);
            counterTop.transform.localScale = new Vector3(3.6f, 0.05f, 0.75f);
            counterTop.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // 2 Desktop PC Stations on reception desk
            for (int pc = 0; pc < 2; pc++)
            {
                float px = -1.2f - pc * 1.3f;
                float pz = 1.4f + pc * 0.8f;
                BuildCompactPCStation(desk, new Vector3(px, 1.15f, pz), Quaternion.Euler(0, 145f, 0));
            }

            // Receptionist Desk Nameplate
            GameObject nameplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nameplate.name = "Desk_Nameplate";
            nameplate.transform.SetParent(desk.transform, false);
            nameplate.transform.localPosition = new Vector3(-0.6f, 1.16f, 1.1f);
            nameplate.transform.localRotation = Quaternion.Euler(0, 145f, 0);
            nameplate.transform.localScale = new Vector3(0.35f, 0.08f, 0.06f);
            nameplate.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            // Receptionist Chair
            BuildOfficeChair(desk, new Vector3(-2.0f, 0.0f, 2.5f), Quaternion.Euler(0, -35f, 0));

            // ── Tall Storage Credenza / Cabinetry Behind Desk ──
            GameObject storageWall = new GameObject("StorageWall_Credenza");
            storageWall.transform.SetParent(parent.transform, false);
            storageWall.transform.localPosition = new Vector3(-6.5f, 1.4f, 11.6f);

            GameObject cabBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabBody.name = "Cabinet_Body";
            cabBody.transform.SetParent(storageWall.transform, false);
            cabBody.transform.localScale = new Vector3(5.2f, 2.8f, 0.6f);
            cabBody.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            // Cabinet Shelves & Folders
            for (int b = 0; b < 4; b++)
            {
                float bx = -1.8f + b * 1.2f;
                for (int shelf = 0; shelf < 3; shelf++)
                {
                    float sy = -0.6f + shelf * 0.8f;
                    GameObject binder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    binder.name = $"FolderBinder_{b}_{shelf}";
                    binder.transform.SetParent(storageWall.transform, false);
                    binder.transform.localPosition = new Vector3(bx, sy, -0.22f);
                    binder.transform.localScale = new Vector3(0.55f, 0.35f, 0.22f);
                    binder.GetComponent<MeshRenderer>().sharedMaterial = (shelf % 2 == 0) ? m_CyanSofa : m_SageGreen;
                }
            }

            // ── Left Cyan Lounge Sofa ──
            GameObject sofa = new GameObject("Reception_CyanSofa");
            sofa.transform.SetParent(parent.transform, false);
            sofa.transform.localPosition = new Vector3(-11.2f, 0.0f, 6.5f);
            sofa.transform.localRotation = Quaternion.Euler(0, 90f, 0);

            // Sofa Base & Seat
            GameObject sofaSeat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sofaSeat.name = "Sofa_Seat";
            sofaSeat.transform.SetParent(sofa.transform, false);
            sofaSeat.transform.localPosition = new Vector3(0, 0.45f, 0);
            sofaSeat.transform.localScale = new Vector3(3.4f, 0.42f, 0.95f);
            sofaSeat.GetComponent<MeshRenderer>().sharedMaterial = m_CyanSofa;

            // Sofa Backrest
            GameObject sofaBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sofaBack.name = "Sofa_Backrest";
            sofaBack.transform.SetParent(sofa.transform, false);
            sofaBack.transform.localPosition = new Vector3(0, 0.82f, -0.38f);
            sofaBack.transform.localScale = new Vector3(3.4f, 0.55f, 0.22f);
            sofaBack.GetComponent<MeshRenderer>().sharedMaterial = m_CyanSofa;

            // Sofa Plinth
            GameObject sofaPlinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sofaPlinth.name = "Sofa_Plinth";
            sofaPlinth.transform.SetParent(sofa.transform, false);
            sofaPlinth.transform.localPosition = new Vector3(0, 0.12f, 0);
            sofaPlinth.transform.localScale = new Vector3(3.45f, 0.22f, 0.98f);
            sofaPlinth.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            // Coffee Table
            GameObject coffeeTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coffeeTable.name = "CoffeeTable";
            coffeeTable.transform.SetParent(parent.transform, false);
            coffeeTable.transform.localPosition = new Vector3(-9.6f, 0.22f, 6.5f);
            coffeeTable.transform.localScale = new Vector3(0.7f, 0.42f, 1.6f);
            coffeeTable.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            // Standing Poster Rollup Banner
            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            banner.name = "RollupBanner_Standee";
            banner.transform.SetParent(parent.transform, false);
            banner.transform.localPosition = new Vector3(-3.2f, 1.05f, 3.2f);
            banner.transform.localRotation = Quaternion.Euler(0, -30f, 0);
            banner.transform.localScale = new Vector3(0.85f, 2.0f, 0.05f);
            banner.GetComponent<MeshRenderer>().sharedMaterial = m_Poster1;
        }

        // ================= ZONE B: WAITING LOUNGE & MEDIA SCREEN =================
        // Positioned around X: [2, 14], Z: [2, 12], Y: [0, 3.6]
        private static void BuildZoneB(GameObject parent)
        {
            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_LightTile_B";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = new Vector3(8.0f, -0.05f, 7.0f);
            floor.transform.localScale = new Vector3(12.0f, 0.1f, 10.0f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Ceiling (Modern White Metal Slats)
            GameObject ceiling = new GameObject("Ceiling_Lounge_B");
            ceiling.transform.SetParent(parent.transform, false);
            for (int i = 0; i < 24; i++)
            {
                float x = 2.4f + i * 0.48f;
                GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slat.name = "CeilingSlat_B_" + i;
                slat.transform.SetParent(ceiling.transform, false);
                slat.transform.localPosition = new Vector3(x, 3.55f, 7.0f);
                slat.transform.localScale = new Vector3(0.08f, 0.12f, 9.8f);
                slat.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;
            }

            // Central Ambient Downlight in Lounge
            GameObject spot = new GameObject("Downlight_Lounge_Center");
            spot.transform.SetParent(ceiling.transform, false);
            spot.transform.localPosition = new Vector3(8.0f, 3.4f, 7.0f);
            Light lB = spot.AddComponent<Light>();
            lB.type = LightType.Point;
            lB.color = new Color(1.0f, 0.98f, 0.95f);
            lB.range = 14.0f;
            lB.intensity = 1.4f;

            // North Focal Wall (Behind LED Display): Z = 12
            GameObject focalWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            focalWall.name = "FocalWall_North_B";
            focalWall.transform.SetParent(parent.transform, false);
            focalWall.transform.localPosition = new Vector3(8.0f, 1.75f, 12.05f);
            focalWall.transform.localScale = new Vector3(12.0f, 3.5f, 0.1f);
            focalWall.GetComponent<MeshRenderer>().sharedMaterial = m_SageGreen;

            // Alternating Timber Slats Cladding on North Wall
            for (int s = 0; s < 18; s++)
            {
                float x = 2.6f + s * 0.6f;
                if (x > 5.2f && x < 10.8f) continue; // Leave central recess for LED wall
                GameObject slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slat.name = "WallWoodSlat_" + s;
                slat.transform.SetParent(parent.transform, false);
                slat.transform.localPosition = new Vector3(x, 1.75f, 11.98f);
                slat.transform.localScale = new Vector3(0.12f, 3.4f, 0.08f);
                slat.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;
            }

            // East Outer Wall: X = 14
            GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "Wall_East_B";
            eastWall.transform.SetParent(parent.transform, false);
            eastWall.transform.localPosition = new Vector3(14.05f, 1.75f, 7.0f);
            eastWall.transform.localScale = new Vector3(0.1f, 3.5f, 10.0f);
            eastWall.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // ── Large LED Display Video Wall ──
            GameObject ledDisplay = new GameObject("Large_LED_Display");
            ledDisplay.transform.SetParent(parent.transform, false);
            ledDisplay.transform.localPosition = new Vector3(8.0f, 2.1f, 11.92f);

            // Screen Frame Bezel
            GameObject bezel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bezel.name = "LED_Bezel";
            bezel.transform.SetParent(ledDisplay.transform, false);
            bezel.transform.localScale = new Vector3(4.9f, 2.78f, 0.08f);
            bezel.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Active Emissive Screen Surface (16:9 aspect)
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "LED_Screen_Active";
            screen.transform.SetParent(ledDisplay.transform, false);
            screen.transform.localPosition = new Vector3(0, 0, -0.05f);
            screen.transform.localScale = new Vector3(4.8f, 2.7f, 0.02f);
            screen.GetComponent<MeshRenderer>().sharedMaterial = m_LEDDisplay;

            // Glow Light from LED Screen onto Floor
            GameObject screenLightGO = new GameObject("LED_Ambient_Glow");
            screenLightGO.transform.SetParent(ledDisplay.transform, false);
            screenLightGO.transform.localPosition = new Vector3(0, -0.2f, -0.8f);
            Light scrLight = screenLightGO.AddComponent<Light>();
            scrLight.type = LightType.Spot;
            scrLight.color = new Color(0.2f, 0.75f, 0.95f);
            scrLight.range = 8.0f;
            scrLight.spotAngle = 110f;
            scrLight.intensity = 2.0f;

            // ── 4 Clusters of Perforated Metal Waiting Benches ──
            // 2 clusters on West side, 2 clusters on East side, facing North towards the screen
            Vector3[] benchPositions = new Vector3[]
            {
                new Vector3(4.8f, 0.0f, 6.0f),  // Front-Left Cluster
                new Vector3(11.2f, 0.0f, 6.0f), // Front-Right Cluster
                new Vector3(4.8f, 0.0f, 3.6f),  // Rear-Left Cluster
                new Vector3(11.2f, 0.0f, 3.6f)  // Rear-Right Cluster
            };

            for (int i = 0; i < benchPositions.Length; i++)
            {
                BuildAirportWaitingBench(parent, benchPositions[i], Quaternion.identity);
            }

            // Decorative Architectural Planters
            BuildIndoorPlanter(parent, new Vector3(2.8f, 0.0f, 11.2f));
            BuildIndoorPlanter(parent, new Vector3(13.2f, 0.0f, 11.2f));
        }

        // ================= ZONE C: TRAINING & COMPUTER LAB =================
        // Positioned around X: [-12, 4], Z: [-16, -2], Y: [0, 3.6]
        private static void BuildZoneC(GameObject parent)
        {
            // Floor (Glossy Black Marble Tile!)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_BlackMarble_C";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = new Vector3(-4.0f, -0.05f, -9.0f);
            floor.transform.localScale = new Vector3(16.0f, 0.1f, 14.0f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = m_BlackMarble;

            // Ceiling (600x600 Acoustic Grid Tile with Bright LED Panels)
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_AcousticGrid_C";
            ceiling.transform.SetParent(parent.transform, false);
            ceiling.transform.localPosition = new Vector3(-4.0f, 3.55f, -9.0f);
            ceiling.transform.localScale = new Vector3(16.0f, 0.1f, 14.0f);
            ceiling.GetComponent<MeshRenderer>().sharedMaterial = m_CeilingGrid;

            // Recessed 600x600 LED Daylight Panels (5500K bright educational illumination)
            for (int px = 0; px < 2; px++)
            {
                float x = -7.0f + px * 6.0f;
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = $"LED_Panel_{px}";
                panel.transform.SetParent(ceiling.transform, false);
                panel.transform.localPosition = new Vector3(x, -0.06f, 0);
                panel.transform.localScale = new Vector3(2.4f, 0.04f, 8.0f);
                panel.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

                Light pl = panel.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(0.96f, 0.98f, 1.0f);
                pl.range = 14.0f;
                pl.intensity = 1.5f;
            }

            // South Front Wall (Teaching Front / Whiteboard): Z = -16
            GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWall.name = "Wall_South_C";
            southWall.transform.SetParent(parent.transform, false);
            southWall.transform.localPosition = new Vector3(-4.0f, 1.75f, -16.05f);
            southWall.transform.localScale = new Vector3(16.0f, 3.5f, 0.1f);
            southWall.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Front Teaching Whiteboard / Projector Screen
            GameObject whiteBoard = new GameObject("Whiteboard_Teaching");
            whiteBoard.transform.SetParent(parent.transform, false);
            whiteBoard.transform.localPosition = new Vector3(-4.0f, 1.85f, -15.95f);

            GameObject boardFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardFrame.name = "Board_Frame";
            boardFrame.transform.SetParent(whiteBoard.transform, false);
            boardFrame.transform.localPosition = Vector3.zero;
            boardFrame.transform.localScale = new Vector3(6.4f, 2.2f, 0.04f);
            boardFrame.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            GameObject boardSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardSurface.name = "Board_Surface";
            boardSurface.transform.SetParent(whiteBoard.transform, false);
            boardSurface.transform.localPosition = new Vector3(0, 0, 0.02f);
            boardSurface.transform.localScale = new Vector3(6.2f, 2.0f, 0.02f);
            boardSurface.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            GameObject tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "Marker_Tray";
            tray.transform.SetParent(whiteBoard.transform, false);
            tray.transform.localPosition = new Vector3(0, -1.05f, 0.05f);
            tray.transform.localScale = new Vector3(6.0f, 0.05f, 0.10f);
            tray.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            // West Wall: X = -12
            GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWall.name = "Wall_West_C";
            westWall.transform.SetParent(parent.transform, false);
            westWall.transform.localPosition = new Vector3(-12.05f, 1.75f, -9.0f);
            westWall.transform.localScale = new Vector3(0.1f, 3.5f, 14.0f);
            westWall.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Academic Posters on West Wall (parented directly to room with crisp scale)
            GameObject poster1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            poster1.name = "Poster_NetworkArch";
            poster1.transform.SetParent(parent.transform, false);
            poster1.transform.localPosition = new Vector3(-11.96f, 1.85f, -11.5f);
            poster1.transform.localScale = new Vector3(0.04f, 1.6f, 1.1f);
            poster1.GetComponent<MeshRenderer>().sharedMaterial = m_Poster1;

            GameObject poster2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            poster2.name = "Poster_AICenter";
            poster2.transform.SetParent(parent.transform, false);
            poster2.transform.localPosition = new Vector3(-11.96f, 1.85f, -6.5f);
            poster2.transform.localScale = new Vector3(0.04f, 1.6f, 1.1f);
            poster2.GetComponent<MeshRenderer>().sharedMaterial = m_Poster2;

            // Wall-mounted Oscillating Electric Fans near ceiling
            for (int f = 0; f < 3; f++)
            {
                float fz = -14.0f + f * 5.0f;
                BuildWallFan(westWall, new Vector3(0.2f, 1.1f, (fz - (-9.0f)) / 14.0f));
            }

            // East Wall: X = 4
            GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "Wall_East_C";
            eastWall.transform.SetParent(parent.transform, false);
            eastWall.transform.localPosition = new Vector3(4.05f, 1.75f, -9.0f);
            eastWall.transform.localScale = new Vector3(0.1f, 3.5f, 14.0f);
            eastWall.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // ── Rear Glass Wall & Entrance (Z = -2, looking into corridor) ──
            GameObject rearGlassWall = new GameObject("Rear_Glass_Curtain_C");
            rearGlassWall.transform.SetParent(parent.transform, false);
            rearGlassWall.transform.localPosition = new Vector3(-4.0f, 1.75f, -2.0f);

            // Mullion Structure
            int glassPanels = 8;
            float totalW = 16.0f;
            float panelW = totalW / glassPanels;

            for (int g = 0; g < glassPanels; g++)
            {
                float gx = -totalW * 0.5f + (g + 0.5f) * panelW;

                // Clear Glass Panel
                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glass.name = "GlassPane_" + g;
                glass.transform.SetParent(rearGlassWall.transform, false);
                glass.transform.localPosition = new Vector3(gx, 0, 0);
                glass.transform.localScale = new Vector3(panelW - 0.06f, 3.48f, 0.04f);
                glass.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

                // Frosted Horizontal Decal Stripe at eye level (1.0m to 1.8m)
                GameObject decal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                decal.name = "FrostedDecal_" + g;
                decal.transform.SetParent(rearGlassWall.transform, false);
                decal.transform.localPosition = new Vector3(gx, -0.3f, 0.005f);
                decal.transform.localScale = new Vector3(panelW - 0.06f, 0.8f, 0.045f);
                decal.GetComponent<MeshRenderer>().sharedMaterial = m_GlassFrosted;

                // Vertical Dark Mullion Frame
                GameObject mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mullion.name = "Mullion_" + g;
                mullion.transform.SetParent(rearGlassWall.transform, false);
                mullion.transform.localPosition = new Vector3(gx - panelW * 0.5f, 0, 0);
                mullion.transform.localScale = new Vector3(0.08f, 3.5f, 0.1f);
                mullion.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            }

            // Double Glass Doors in center of rear wall (gx = 0)
            GameObject doorFrame = new GameObject("DoorFrame_Central");
            doorFrame.transform.SetParent(rearGlassWall.transform, false);

            // Left Post
            GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postL.name = "Post_L";
            postL.transform.SetParent(doorFrame.transform, false);
            postL.transform.localPosition = new Vector3(-1.05f, 0, 0);
            postL.transform.localScale = new Vector3(0.08f, 3.5f, 0.12f);
            postL.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Right Post
            GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postR.name = "Post_R";
            postR.transform.SetParent(doorFrame.transform, false);
            postR.transform.localPosition = new Vector3(1.05f, 0, 0);
            postR.transform.localScale = new Vector3(0.08f, 3.5f, 0.12f);
            postR.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Top Lintel
            GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "Lintel";
            lintel.transform.SetParent(doorFrame.transform, false);
            lintel.transform.localPosition = new Vector3(0, 1.7f, 0);
            lintel.transform.localScale = new Vector3(2.2f, 0.12f, 0.12f);
            lintel.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Left Leaf sliding group
            GameObject leafL = new GameObject("DoorLeaf_Left");
            leafL.transform.SetParent(doorFrame.transform, false);
            leafL.transform.localPosition = new Vector3(-0.52f, 0, 0);

            GameObject dGlassL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dGlassL.name = "DoorGlass_L";
            dGlassL.transform.SetParent(leafL.transform, false);
            dGlassL.transform.localPosition = Vector3.zero;
            dGlassL.transform.localScale = new Vector3(0.95f, 3.3f, 0.04f);
            dGlassL.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            GameObject dDecalL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dDecalL.name = "DoorDecal_L";
            dDecalL.transform.SetParent(leafL.transform, false);
            dDecalL.transform.localPosition = new Vector3(0, -0.3f, 0.005f);
            dDecalL.transform.localScale = new Vector3(0.95f, 0.8f, 0.045f);
            dDecalL.GetComponent<MeshRenderer>().sharedMaterial = m_GlassFrosted;

            GameObject handleL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handleL.name = "DoorHandle_L";
            handleL.transform.SetParent(leafL.transform, false);
            handleL.transform.localPosition = new Vector3(0.38f, -0.4f, 0.08f);
            handleL.transform.localScale = new Vector3(0.04f, 0.9f, 0.06f);
            handleL.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            // Right Leaf sliding group
            GameObject leafR = new GameObject("DoorLeaf_Right");
            leafR.transform.SetParent(doorFrame.transform, false);
            leafR.transform.localPosition = new Vector3(0.52f, 0, 0);

            GameObject dGlassR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dGlassR.name = "DoorGlass_R";
            dGlassR.transform.SetParent(leafR.transform, false);
            dGlassR.transform.localPosition = Vector3.zero;
            dGlassR.transform.localScale = new Vector3(0.95f, 3.3f, 0.04f);
            dGlassR.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            GameObject dDecalR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dDecalR.name = "DoorDecal_R";
            dDecalR.transform.SetParent(leafR.transform, false);
            dDecalR.transform.localPosition = new Vector3(0, -0.3f, 0.005f);
            dDecalR.transform.localScale = new Vector3(0.95f, 0.8f, 0.045f);
            dDecalR.GetComponent<MeshRenderer>().sharedMaterial = m_GlassFrosted;

            GameObject handleR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handleR.name = "DoorHandle_R";
            handleR.transform.SetParent(leafR.transform, false);
            handleR.transform.localPosition = new Vector3(-0.38f, -0.4f, 0.08f);
            handleR.transform.localScale = new Vector3(0.04f, 0.9f, 0.06f);
            handleR.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            // Trigger & Automatic Door Component
            BoxCollider doorTrigger = doorFrame.AddComponent<BoxCollider>();
            doorTrigger.isTrigger = true;
            doorTrigger.center = Vector3.zero;
            doorTrigger.size = new Vector3(3.2f, 3.5f, 3.5f);
            IUHAutomaticDoor autoDoor = doorFrame.AddComponent<IUHAutomaticDoor>();
            autoDoor.BindDoors(leafL.transform, leafR.transform, 1.0f);

            // ── Two Main Center Double Rows of Desks (8 workstations) ──
            // Row 1 (Left center): X = -6.5
            BuildDoubleDeskRow(parent, new Vector3(-6.5f, 0.0f, -9.0f), 4);

            // Row 2 (Right center): X = -1.5
            BuildDoubleDeskRow(parent, new Vector3(-1.5f, 0.0f, -9.0f), 4);

            // ── Peripheral Side Desk Row (4 workstations along West wall) ──
            BuildSingleDeskRow(parent, new Vector3(-11.2f, 0.0f, -9.0f), 4);
        }

        // ================= ZONE CIRCULATION / CORRIDOR =================
        // Positioned around X: [-14, 14], Z: [-2, 2], Y: [0, 3.6]
        private static void BuildCorridor(GameObject parent)
        {
            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_Corridor";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = new Vector3(0.0f, -0.05f, 0.0f);
            floor.transform.localScale = new Vector3(28.0f, 0.1f, 4.0f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Ceiling
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling_Corridor";
            ceiling.transform.SetParent(parent.transform, false);
            ceiling.transform.localPosition = new Vector3(0.0f, 3.55f, 0.0f);
            ceiling.transform.localScale = new Vector3(28.0f, 0.1f, 4.0f);
            ceiling.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Recessed Downlights along corridor
            for (int i = 0; i < 2; i++)
            {
                float x = -6.0f + i * 12.0f;
                GameObject lightGO = new GameObject("Corridor_Light_" + i);
                lightGO.transform.SetParent(ceiling.transform, false);
                lightGO.transform.localPosition = new Vector3(x, -0.1f, 0.0f);
                Light l = lightGO.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1.0f, 0.98f, 0.95f);
                l.range = 10.0f;
                l.intensity = 1.2f;
            }

            // Directional & Room Nameplate Signage Above Doors
            BuildSignboard(parent, new Vector3(-5.5f, 2.7f, 1.95f), "PHÒNG TIẾP ĐÓN & TƯ VẤN HỌC THUẬT");
            BuildSignboard(parent, new Vector3(8.0f, 2.7f, 1.95f), "SẢNH CHỜ TRUYỀN THÔNG & DỮ LIỆU");
            BuildSignboard(parent, new Vector3(-4.0f, 2.7f, -1.95f), "PHÒNG THỰC HÀNH CÔNG NGHỆ CHUYÊN SÂU");
            BuildSignboard(parent, new Vector3(9.0f, 2.7f, -1.95f), "LỐI RA KHUÔN VIÊN & CỔNG CHÍNH IUH");
        }

        // ================= ZONE EXTERIOR ENTRANCE & PLAZA =================
        // Positioned around X: [4, 14], Z: [-14, -2], Y: [0, 3.6]
        private static void BuildExteriorEntrancePlaza(GameObject parent)
        {
            // 1. Exterior Plaza Ground Pavement
            GameObject plazaFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plazaFloor.name = "Floor_Exterior_Plaza";
            plazaFloor.transform.SetParent(parent.transform, false);
            plazaFloor.transform.localPosition = new Vector3(9.0f, -0.05f, -8.0f);
            plazaFloor.transform.localScale = new Vector3(14.0f, 0.1f, 12.0f);
            plazaFloor.GetComponent<MeshRenderer>().sharedMaterial = m_LightTile;

            // Outer curb pavers
            GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = "Plaza_Curb";
            curb.transform.SetParent(parent.transform, false);
            curb.transform.localPosition = new Vector3(9.0f, 0.05f, -14.1f);
            curb.transform.localScale = new Vector3(14.4f, 0.15f, 0.3f);
            curb.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // 2. Entrance Facade Wall (Z = -2.0)
            // Left Wall (X = 4 to 7)
            GameObject wallL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallL.name = "Entrance_Wall_Left";
            wallL.transform.SetParent(parent.transform, false);
            wallL.transform.localPosition = new Vector3(5.5f, 1.75f, -2.0f);
            wallL.transform.localScale = new Vector3(3.0f, 3.5f, 0.2f);
            wallL.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Right Wall (X = 11 to 14)
            GameObject wallR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallR.name = "Entrance_Wall_Right";
            wallR.transform.SetParent(parent.transform, false);
            wallR.transform.localPosition = new Vector3(12.5f, 1.75f, -2.0f);
            wallR.transform.localScale = new Vector3(3.0f, 3.5f, 0.2f);
            wallR.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Transom Glass above entrance opening (Y = 2.6 to 3.5)
            GameObject transom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            transom.name = "Entrance_Transom_Glass";
            transom.transform.SetParent(parent.transform, false);
            transom.transform.localPosition = new Vector3(9.0f, 3.05f, -2.0f);
            transom.transform.localScale = new Vector3(4.0f, 0.9f, 0.06f);
            transom.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            // Mullion Frame around opening
            for (int p = -1; p <= 1; p += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "Entrance_Post_" + p;
                post.transform.SetParent(parent.transform, false);
                post.transform.localPosition = new Vector3(9.0f + p * 2.0f, 1.75f, -2.0f);
                post.transform.localScale = new Vector3(0.12f, 3.5f, 0.22f);
                post.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            }

            // 3. Modern Cantilever Canopy & IUH Signboard
            GameObject canopy = new GameObject("Entrance_Canopy");
            canopy.transform.SetParent(parent.transform, false);
            canopy.transform.localPosition = new Vector3(9.0f, 3.55f, -4.5f);

            // Canopy Roof Slab
            GameObject canopySlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopySlab.name = "Canopy_RoofSlab";
            canopySlab.transform.SetParent(canopy.transform, false);
            canopySlab.transform.localPosition = Vector3.zero;
            canopySlab.transform.localScale = new Vector3(6.4f, 0.22f, 5.0f);
            canopySlab.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Front Fascia Signboard Banner
            GameObject fascia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fascia.name = "Canopy_FrontFascia";
            fascia.transform.SetParent(canopy.transform, false);
            fascia.transform.localPosition = new Vector3(0, 0, -2.55f);
            fascia.transform.localScale = new Vector3(6.4f, 0.55f, 0.1f);
            fascia.GetComponent<MeshRenderer>().sharedMaterial = (m_AvatarJacket != null) ? m_AvatarJacket : m_NavyChair;

            // White illuminated lettering banner
            GameObject fasciaSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fasciaSign.name = "Canopy_SignPlate";
            fasciaSign.transform.SetParent(canopy.transform, false);
            fasciaSign.transform.localPosition = new Vector3(0, 0.04f, -2.61f);
            fasciaSign.transform.localScale = new Vector3(5.2f, 0.32f, 0.04f);
            fasciaSign.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Accent Line along canopy
            GameObject accentLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            accentLine.name = "Canopy_AccentLine";
            accentLine.transform.SetParent(canopy.transform, false);
            accentLine.transform.localPosition = new Vector3(0, -0.24f, -2.58f);
            accentLine.transform.localScale = new Vector3(6.4f, 0.06f, 0.05f);
            accentLine.GetComponent<MeshRenderer>().sharedMaterial = m_LEDDisplay;

            // Canopy Downlights
            for (int l = -1; l <= 1; l += 2)
            {
                GameObject cLight = new GameObject("Canopy_Light_" + l);
                cLight.transform.SetParent(canopy.transform, false);
                cLight.transform.localPosition = new Vector3(l * 1.8f, -0.15f, 0);
                Light cl = cLight.AddComponent<Light>();
                cl.type = LightType.Point;
                cl.color = new Color(1.0f, 0.98f, 0.92f);
                cl.range = 8.0f;
                cl.intensity = 1.6f;
            }

            // 4. Automatic Sliding Glass Doors at Main Entrance
            GameObject entranceDoorFrame = new GameObject("Entrance_SlidingDoors");
            entranceDoorFrame.transform.SetParent(parent.transform, false);
            entranceDoorFrame.transform.localPosition = new Vector3(9.0f, 1.3f, -2.0f);

            // Left sliding leaf
            GameObject eLeafL = new GameObject("DoorLeaf_L");
            eLeafL.transform.SetParent(entranceDoorFrame.transform, false);
            eLeafL.transform.localPosition = new Vector3(-0.95f, 0, 0);

            GameObject eGlassL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eGlassL.name = "Glass_L";
            eGlassL.transform.SetParent(eLeafL.transform, false);
            eGlassL.transform.localPosition = Vector3.zero;
            eGlassL.transform.localScale = new Vector3(1.9f, 2.6f, 0.05f);
            eGlassL.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            GameObject eDecalL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eDecalL.name = "Decal_L";
            eDecalL.transform.SetParent(eLeafL.transform, false);
            eDecalL.transform.localPosition = new Vector3(0, -0.2f, 0.005f);
            eDecalL.transform.localScale = new Vector3(1.9f, 0.8f, 0.055f);
            eDecalL.GetComponent<MeshRenderer>().sharedMaterial = m_GlassFrosted;

            GameObject eHandleL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eHandleL.name = "Handle_L";
            eHandleL.transform.SetParent(eLeafL.transform, false);
            eHandleL.transform.localPosition = new Vector3(0.75f, -0.2f, 0.08f);
            eHandleL.transform.localScale = new Vector3(0.04f, 1.1f, 0.06f);
            eHandleL.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            // Right sliding leaf
            GameObject eLeafR = new GameObject("DoorLeaf_R");
            eLeafR.transform.SetParent(entranceDoorFrame.transform, false);
            eLeafR.transform.localPosition = new Vector3(0.95f, 0, 0);

            GameObject eGlassR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eGlassR.name = "Glass_R";
            eGlassR.transform.SetParent(eLeafR.transform, false);
            eGlassR.transform.localPosition = Vector3.zero;
            eGlassR.transform.localScale = new Vector3(1.9f, 2.6f, 0.05f);
            eGlassR.GetComponent<MeshRenderer>().sharedMaterial = m_GlassClear;

            GameObject eDecalR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eDecalR.name = "Decal_R";
            eDecalR.transform.SetParent(eLeafR.transform, false);
            eDecalR.transform.localPosition = new Vector3(0, -0.2f, 0.005f);
            eDecalR.transform.localScale = new Vector3(1.9f, 0.8f, 0.055f);
            eDecalR.GetComponent<MeshRenderer>().sharedMaterial = m_GlassFrosted;

            GameObject eHandleR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eHandleR.name = "Handle_R";
            eHandleR.transform.SetParent(eLeafR.transform, false);
            eHandleR.transform.localPosition = new Vector3(-0.75f, -0.2f, 0.08f);
            eHandleR.transform.localScale = new Vector3(0.04f, 1.1f, 0.06f);
            eHandleR.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            // Automatic Door Trigger
            BoxCollider entranceTrigger = entranceDoorFrame.AddComponent<BoxCollider>();
            entranceTrigger.isTrigger = true;
            entranceTrigger.center = Vector3.zero;
            entranceTrigger.size = new Vector3(4.5f, 3.0f, 4.5f);
            IUHAutomaticDoor eAutoDoor = entranceDoorFrame.AddComponent<IUHAutomaticDoor>();
            eAutoDoor.BindDoors(eLeafL.transform, eLeafR.transform, 1.4f);

            // 5. Planters flanking entrance
            BuildIndoorPlanter(parent, new Vector3(6.2f, 0, -4.5f));
            BuildIndoorPlanter(parent, new Vector3(11.8f, 0, -4.5f));
        }

        // ================= PLAYER SYSTEM & SPAWN POINTS =================
        public static void BuildPlayerSystem(GameObject parent)
        {
            // 1. Spawn Point Exterior (local pos: 9.0, 0.05, -8.0; facing North)
            GameObject spawnExt = new GameObject("PlayerSpawn_Exterior");
            spawnExt.transform.SetParent(parent.transform, false);
            spawnExt.transform.localPosition = new Vector3(9.0f, 0.05f, -8.0f);
            spawnExt.transform.localRotation = Quaternion.Euler(0, 0, 0);

            // 2. Spawn Point Interior (local pos: -7.0, 0.05, 6.0; facing West at Reception)
            GameObject spawnInt = new GameObject("PlayerSpawn_Interior");
            spawnInt.transform.SetParent(parent.transform, false);
            spawnInt.transform.localPosition = new Vector3(-7.0f, 0.05f, 6.0f);
            spawnInt.transform.localRotation = Quaternion.Euler(0, -90.0f, 0);

            // 3. Player Character
            GameObject player = new GameObject("Player");
            player.transform.SetParent(parent.transform, false);
            player.transform.localPosition = spawnExt.transform.localPosition;
            player.transform.localRotation = spawnExt.transform.localRotation;
            player.tag = "Player";
            player.layer = 2; // Ignore Raycast layer for camera collision

            // Character Controller
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.75f;
            cc.radius = 0.32f;
            cc.center = new Vector3(0, 0.875f, 0);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 45f;
            cc.minMoveDistance = 0.001f;

            // Camera Target
            GameObject camTarget = new GameObject("CameraTarget");
            camTarget.transform.SetParent(player.transform, false);
            camTarget.transform.localPosition = new Vector3(0, 1.55f, 0);

            // Ground Check
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform, false);
            groundCheck.transform.localPosition = new Vector3(0, 0.05f, 0);

            // Humanoid Student Model
            GameObject model = new GameObject("Model");
            model.transform.SetParent(player.transform, false);
            model.layer = 2;

            // Torso (Navy IUH Student Polo / Jacket)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(model.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.05f, 0);
            torso.transform.localScale = new Vector3(0.42f, 0.55f, 0.26f);
            torso.layer = 2;
            torso.GetComponent<MeshRenderer>().sharedMaterial = (m_AvatarJacket != null) ? m_AvatarJacket : m_NavyChair;
            UnityEngine.Object.DestroyImmediate(torso.GetComponent<Collider>());

            // Collar / Shirt Trim
            GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            collar.name = "Collar";
            collar.transform.SetParent(torso.transform, false);
            collar.transform.localPosition = new Vector3(0, 0.46f, 0.08f);
            collar.transform.localScale = new Vector3(0.24f, 0.12f, 0.14f);
            collar.layer = 2;
            collar.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;
            UnityEngine.Object.DestroyImmediate(collar.GetComponent<Collider>());

            // Student Backpack
            GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backpack.name = "Backpack";
            backpack.transform.SetParent(torso.transform, false);
            backpack.transform.localPosition = new Vector3(0, 0.05f, -0.22f);
            backpack.transform.localScale = new Vector3(0.34f, 0.42f, 0.18f);
            backpack.layer = 2;
            backpack.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            UnityEngine.Object.DestroyImmediate(backpack.GetComponent<Collider>());

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(model.transform, false);
            head.transform.localPosition = new Vector3(0, 1.56f, 0);
            head.transform.localScale = new Vector3(0.22f, 0.25f, 0.22f);
            head.layer = 2;
            head.GetComponent<MeshRenderer>().sharedMaterial = (m_AvatarSkin != null) ? m_AvatarSkin : m_LightWood;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

            // Hair
            GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hair.name = "Hair";
            hair.transform.SetParent(head.transform, false);
            hair.transform.localPosition = new Vector3(0, 0.46f, -0.02f);
            hair.transform.localScale = new Vector3(1.05f, 0.35f, 1.05f);
            hair.layer = 2;
            hair.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            UnityEngine.Object.DestroyImmediate(hair.GetComponent<Collider>());

            // Left Arm Pivot
            GameObject armLPivot = new GameObject("Arm_L_Pivot");
            armLPivot.transform.SetParent(model.transform, false);
            armLPivot.transform.localPosition = new Vector3(-0.27f, 1.25f, 0);
            armLPivot.layer = 2;

            GameObject armLMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armLMesh.name = "Arm_L_Mesh";
            armLMesh.transform.SetParent(armLPivot.transform, false);
            armLMesh.transform.localPosition = new Vector3(0, -0.28f, 0);
            armLMesh.transform.localScale = new Vector3(0.12f, 0.56f, 0.12f);
            armLMesh.layer = 2;
            armLMesh.GetComponent<MeshRenderer>().sharedMaterial = (m_AvatarJacket != null) ? m_AvatarJacket : m_NavyChair;
            UnityEngine.Object.DestroyImmediate(armLMesh.GetComponent<Collider>());

            GameObject handL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handL.name = "Hand_L";
            handL.transform.SetParent(armLPivot.transform, false);
            handL.transform.localPosition = new Vector3(0, -0.60f, 0);
            handL.transform.localScale = new Vector3(0.09f, 0.10f, 0.09f);
            handL.layer = 2;
            handL.GetComponent<MeshRenderer>().sharedMaterial = (m_AvatarSkin != null) ? m_AvatarSkin : m_LightWood;
            UnityEngine.Object.DestroyImmediate(handL.GetComponent<Collider>());

            // Right Arm Pivot
            GameObject armRPivot = new GameObject("Arm_R_Pivot");
            armRPivot.transform.SetParent(model.transform, false);
            armRPivot.transform.localPosition = new Vector3(0.27f, 1.25f, 0);
            armRPivot.layer = 2;

            GameObject armRMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armRMesh.name = "Arm_R_Mesh";
            armRMesh.transform.SetParent(armRPivot.transform, false);
            armRMesh.transform.localPosition = new Vector3(0, -0.28f, 0);
            armRMesh.transform.localScale = new Vector3(0.12f, 0.56f, 0.12f);
            armRMesh.layer = 2;
            armRMesh.GetComponent<MeshRenderer>().sharedMaterial = (m_AvatarJacket != null) ? m_AvatarJacket : m_NavyChair;
            UnityEngine.Object.DestroyImmediate(armRMesh.GetComponent<Collider>());

            GameObject handR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handR.name = "Hand_R";
            handR.transform.SetParent(armRPivot.transform, false);
            handR.transform.localPosition = new Vector3(0, -0.60f, 0);
            handR.transform.localScale = new Vector3(0.09f, 0.10f, 0.09f);
            handR.layer = 2;
            handR.GetComponent<MeshRenderer>().sharedMaterial = (m_AvatarSkin != null) ? m_AvatarSkin : m_LightWood;
            UnityEngine.Object.DestroyImmediate(handR.GetComponent<Collider>());

            // Left Leg Pivot
            GameObject legLPivot = new GameObject("Leg_L_Pivot");
            legLPivot.transform.SetParent(model.transform, false);
            legLPivot.transform.localPosition = new Vector3(-0.12f, 0.72f, 0);
            legLPivot.layer = 2;

            GameObject legLMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legLMesh.name = "Leg_L_Mesh";
            legLMesh.transform.SetParent(legLPivot.transform, false);
            legLMesh.transform.localPosition = new Vector3(0, -0.32f, 0);
            legLMesh.transform.localScale = new Vector3(0.15f, 0.66f, 0.16f);
            legLMesh.layer = 2;
            legLMesh.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            UnityEngine.Object.DestroyImmediate(legLMesh.GetComponent<Collider>());

            GameObject shoeL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shoeL.name = "Shoe_L";
            shoeL.transform.SetParent(legLPivot.transform, false);
            shoeL.transform.localPosition = new Vector3(0, -0.67f, 0.04f);
            shoeL.transform.localScale = new Vector3(0.15f, 0.09f, 0.26f);
            shoeL.layer = 2;
            shoeL.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;
            UnityEngine.Object.DestroyImmediate(shoeL.GetComponent<Collider>());

            // Right Leg Pivot
            GameObject legRPivot = new GameObject("Leg_R_Pivot");
            legRPivot.transform.SetParent(model.transform, false);
            legRPivot.transform.localPosition = new Vector3(0.12f, 0.72f, 0);
            legRPivot.layer = 2;

            GameObject legRMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legRMesh.name = "Leg_R_Mesh";
            legRMesh.transform.SetParent(legRPivot.transform, false);
            legRMesh.transform.localPosition = new Vector3(0, -0.32f, 0);
            legRMesh.transform.localScale = new Vector3(0.15f, 0.66f, 0.16f);
            legRMesh.layer = 2;
            legRMesh.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            UnityEngine.Object.DestroyImmediate(legRMesh.GetComponent<Collider>());

            GameObject shoeR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shoeR.name = "Shoe_R";
            shoeR.transform.SetParent(legRPivot.transform, false);
            shoeR.transform.localPosition = new Vector3(0, -0.67f, 0.04f);
            shoeR.transform.localScale = new Vector3(0.15f, 0.09f, 0.26f);
            shoeR.layer = 2;
            shoeR.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;
            UnityEngine.Object.DestroyImmediate(shoeR.GetComponent<Collider>());

            // Bind Animator
            IUHCharacterAnimator animator = model.AddComponent<IUHCharacterAnimator>();
            animator.BindLimbs(torso.transform, armLPivot.transform, armRPivot.transform, legLPivot.transform, legRPivot.transform);

            // Bind Controller
            player.AddComponent<IUHPlayerController>();

            // Setup Camera System
            SetupMainCamera(player.transform);
        }

        private static void SetupMainCamera(Transform playerTransform)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camGO = new GameObject("MainCamera");
                camGO.tag = "MainCamera";
                mainCam = camGO.AddComponent<Camera>();
            }

            IUHThirdPersonCamera tpCam = mainCam.GetComponent<IUHThirdPersonCamera>();
            if (tpCam == null) tpCam = mainCam.gameObject.AddComponent<IUHThirdPersonCamera>();
            tpCam.SetTarget(playerTransform);

            // Position initially behind player at exterior spawn
            mainCam.transform.position = playerTransform.position + new Vector3(0, 1.8f, -2.6f);
            mainCam.transform.rotation = Quaternion.Euler(12.0f, playerTransform.eulerAngles.y, 0);
        }

        // ================= PROCEDURAL FURNITURE & PROPS =================

        // Double-sided Computer Desk Row (with central low white divider screen)
        private static void BuildDoubleDeskRow(GameObject parent, Vector3 centerPos, int totalStations)
        {
            GameObject rowGO = new GameObject("DoubleDeskRow");
            rowGO.transform.SetParent(parent.transform, false);
            rowGO.transform.localPosition = centerPos;

            int stationsPerSide = totalStations / 2;
            float stationLen = 1.4f;
            float deskDepth = 0.7f;
            float totalLen = stationsPerSide * stationLen;

            // Table Top North Side
            GameObject topN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topN.name = "TableTop_North";
            topN.transform.SetParent(rowGO.transform, false);
            topN.transform.localPosition = new Vector3(0, 0.73f, deskDepth * 0.5f);
            topN.transform.localScale = new Vector3(totalLen, 0.05f, deskDepth);
            topN.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            // Table Top South Side
            GameObject topS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topS.name = "TableTop_South";
            topS.transform.SetParent(rowGO.transform, false);
            topS.transform.localPosition = new Vector3(0, 0.73f, -deskDepth * 0.5f);
            topS.transform.localScale = new Vector3(totalLen, 0.05f, deskDepth);
            topS.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            // Central White Matte Divider Screen (Low privacy screen ~0.35m high)
            GameObject divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = "White_Divider_Screen";
            divider.transform.SetParent(rowGO.transform, false);
            divider.transform.localPosition = new Vector3(0, 0.95f, 0);
            divider.transform.localScale = new Vector3(totalLen + 0.05f, 0.38f, 0.04f);
            divider.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Dark Metal Desk Frame & Legs
            for (int leg = 0; leg <= stationsPerSide; leg++)
            {
                float lx = -totalLen * 0.5f + leg * stationLen;
                GameObject legBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                legBar.name = "DeskLeg_" + leg;
                legBar.transform.SetParent(rowGO.transform, false);
                legBar.transform.localPosition = new Vector3(lx, 0.36f, 0);
                legBar.transform.localScale = new Vector3(0.06f, 0.72f, deskDepth * 2.0f);
                legBar.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            }

            // Workstations & Chairs on North & South
            for (int s = 0; s < stationsPerSide; s++)
            {
                float sx = -totalLen * 0.5f + (s + 0.5f) * stationLen;

                // North Station (Looking South towards divider)
                BuildPCWorkstation(rowGO, new Vector3(sx, 0.75f, deskDepth * 0.5f), Quaternion.Euler(0, 180f, 0));
                BuildOfficeChair(rowGO, new Vector3(sx, 0.0f, deskDepth + 0.45f), Quaternion.Euler(0, 180f, 0));

                // South Station (Looking North towards divider)
                BuildPCWorkstation(rowGO, new Vector3(sx, 0.75f, -deskDepth * 0.5f), Quaternion.identity);
                BuildOfficeChair(rowGO, new Vector3(sx, 0.0f, -deskDepth - 0.45f), Quaternion.identity);
            }
        }

        // Single Peripheral Desk Row along Wall
        private static void BuildSingleDeskRow(GameObject parent, Vector3 centerPos, int stationCount)
        {
            GameObject rowGO = new GameObject("SingleDeskRow_Wall");
            rowGO.transform.SetParent(parent.transform, false);
            rowGO.transform.localPosition = centerPos;

            float stationLen = 1.4f;
            float deskDepth = 0.65f;
            float totalLen = stationCount * stationLen;

            // Table Top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "TableTop_Wall";
            top.transform.SetParent(rowGO.transform, false);
            top.transform.localPosition = new Vector3(0, 0.73f, 0);
            top.transform.localScale = new Vector3(totalLen, 0.05f, deskDepth);
            top.GetComponent<MeshRenderer>().sharedMaterial = m_LightWood;

            // Legs
            for (int leg = 0; leg <= stationCount; leg++)
            {
                float lx = -totalLen * 0.5f + leg * stationLen;
                GameObject legBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                legBar.name = "DeskLeg_Wall_" + leg;
                legBar.transform.SetParent(rowGO.transform, false);
                legBar.transform.localPosition = new Vector3(lx, 0.36f, 0);
                legBar.transform.localScale = new Vector3(0.06f, 0.72f, deskDepth);
                legBar.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            }

            // Workstations & Chairs (Looking West towards wall)
            for (int s = 0; s < stationCount; s++)
            {
                float sx = -totalLen * 0.5f + (s + 0.5f) * stationLen;
                BuildPCWorkstation(rowGO, new Vector3(sx, 0.75f, 0), Quaternion.Euler(0, -90f, 0));
                BuildOfficeChair(rowGO, new Vector3(sx, 0.0f, 0.65f), Quaternion.Euler(0, -90f, 0));
            }
        }

        // Complete PC Workstation (Case, Screen, Keyboard, Mouse)
        private static void BuildPCWorkstation(GameObject parent, Vector3 localPos, Quaternion localRot)
        {
            GameObject pc = new GameObject("Workstation");
            pc.transform.SetParent(parent.transform, false);
            pc.transform.localPosition = localPos;
            pc.transform.localRotation = localRot;

            // 1. 24" Widescreen Monitor with glowing code editor screen
            GameObject monitor = new GameObject("Monitor_24in");
            monitor.transform.SetParent(pc.transform, false);
            monitor.transform.localPosition = new Vector3(0, 0.28f, 0.18f);

            // Bezel
            GameObject bez = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bez.name = "Bezel";
            bez.transform.SetParent(monitor.transform, false);
            bez.transform.localScale = new Vector3(0.56f, 0.34f, 0.03f);
            bez.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Emissive Screen
            GameObject scr = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scr.name = "Screen";
            scr.transform.SetParent(monitor.transform, false);
            scr.transform.localPosition = new Vector3(0, 0, -0.016f);
            scr.transform.localScale = new Vector3(0.53f, 0.31f, 0.01f);
            scr.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

            // Stand & Base
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = "Stand";
            stand.transform.SetParent(monitor.transform, false);
            stand.transform.localPosition = new Vector3(0, -0.2f, 0.02f);
            stand.transform.localScale = new Vector3(0.05f, 0.22f, 0.04f);
            stand.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            GameObject basePl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basePl.name = "BasePlate";
            basePl.transform.SetParent(monitor.transform, false);
            basePl.transform.localPosition = new Vector3(0, -0.27f, 0);
            basePl.transform.localScale = new Vector3(0.22f, 0.02f, 0.18f);
            basePl.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // 2. Tower PC Case
            GameObject pcCase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pcCase.name = "PC_TowerCase";
            pcCase.transform.SetParent(pc.transform, false);
            pcCase.transform.localPosition = new Vector3(0.42f, 0.22f, 0.15f);
            pcCase.transform.localScale = new Vector3(0.18f, 0.44f, 0.42f);
            pcCase.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // 3. Keyboard
            GameObject kb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kb.name = "Keyboard";
            kb.transform.SetParent(pc.transform, false);
            kb.transform.localPosition = new Vector3(-0.04f, 0.012f, -0.08f);
            kb.transform.localScale = new Vector3(0.44f, 0.018f, 0.14f);
            kb.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // 4. Mouse & Mousepad
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Mousepad";
            pad.transform.SetParent(pc.transform, false);
            pad.transform.localPosition = new Vector3(0.25f, 0.005f, -0.08f);
            pad.transform.localScale = new Vector3(0.20f, 0.005f, 0.22f);
            pad.GetComponent<MeshRenderer>().sharedMaterial = m_NavyChair;

            GameObject mouse = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouse.name = "Mouse";
            mouse.transform.SetParent(pc.transform, false);
            mouse.transform.localPosition = new Vector3(0.25f, 0.015f, -0.08f);
            mouse.transform.localScale = new Vector3(0.06f, 0.022f, 0.10f);
            mouse.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
        }

        // Compact PC Station for Reception Desk
        private static void BuildCompactPCStation(GameObject parent, Vector3 localPos, Quaternion localRot)
        {
            GameObject pc = new GameObject("Reception_PC");
            pc.transform.SetParent(parent.transform, false);
            pc.transform.localPosition = localPos;
            pc.transform.localRotation = localRot;

            // Monitor
            GameObject bez = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bez.name = "Monitor";
            bez.transform.SetParent(pc.transform, false);
            bez.transform.localPosition = new Vector3(0, 0.24f, 0);
            bez.transform.localScale = new Vector3(0.52f, 0.32f, 0.03f);
            bez.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            GameObject scr = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scr.name = "Screen";
            scr.transform.SetParent(pc.transform, false);
            scr.transform.localPosition = new Vector3(0, 0.24f, -0.016f);
            scr.transform.localScale = new Vector3(0.49f, 0.29f, 0.01f);
            scr.GetComponent<MeshRenderer>().sharedMaterial = m_ScreenPC;

            // Keyboard & Mouse
            GameObject kb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kb.name = "Keyboard";
            kb.transform.SetParent(pc.transform, false);
            kb.transform.localPosition = new Vector3(0, 0.012f, -0.22f);
            kb.transform.localScale = new Vector3(0.42f, 0.018f, 0.13f);
            kb.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
        }

        // Ergonomic Swivel Office Chair
        private static void BuildOfficeChair(GameObject parent, Vector3 localPos, Quaternion localRot)
        {
            GameObject chair = new GameObject("OfficeChair");
            chair.transform.SetParent(parent.transform, false);
            chair.transform.localPosition = localPos;
            chair.transform.localRotation = localRot;

            // Seat Cushion
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "SeatCushion";
            seat.transform.SetParent(chair.transform, false);
            seat.transform.localPosition = new Vector3(0, 0.48f, 0);
            seat.transform.localScale = new Vector3(0.48f, 0.08f, 0.46f);
            seat.GetComponent<MeshRenderer>().sharedMaterial = m_NavyChair;

            // Ergonomic Curved Backrest
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Backrest";
            back.transform.SetParent(chair.transform, false);
            back.transform.localPosition = new Vector3(0, 0.78f, 0.22f);
            back.transform.localScale = new Vector3(0.44f, 0.52f, 0.06f);
            back.GetComponent<MeshRenderer>().sharedMaterial = m_NavyChair;

            // Armrests
            for (int a = -1; a <= 1; a += 2)
            {
                GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arm.name = "Armrest_" + a;
                arm.transform.SetParent(chair.transform, false);
                arm.transform.localPosition = new Vector3(a * 0.25f, 0.62f, 0.05f);
                arm.transform.localScale = new Vector3(0.04f, 0.22f, 0.28f);
                arm.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
            }

            // Central Piston & 5-Star Base
            GameObject piston = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            piston.name = "Piston";
            piston.transform.SetParent(chair.transform, false);
            piston.transform.localPosition = new Vector3(0, 0.24f, 0);
            piston.transform.localScale = new Vector3(0.06f, 0.22f, 0.06f);
            piston.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            GameObject starBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            starBase.name = "StarBase";
            starBase.transform.SetParent(chair.transform, false);
            starBase.transform.localPosition = new Vector3(0, 0.06f, 0);
            starBase.transform.localScale = new Vector3(0.55f, 0.03f, 0.55f);
            starBase.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;
        }

        // Airport / Station 3-Seater Waiting Bench
        private static void BuildAirportWaitingBench(GameObject parent, Vector3 localPos, Quaternion localRot)
        {
            GameObject bench = new GameObject("PerforatedMetal_Bench");
            bench.transform.SetParent(parent.transform, false);
            bench.transform.localPosition = localPos;
            bench.transform.localRotation = localRot;

            float seatWidth = 0.55f;
            float spacing = 0.62f;

            // Main Chrome Crossbeam Beam
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Chrome_Beam";
            beam.transform.SetParent(bench.transform, false);
            beam.transform.localPosition = new Vector3(0, 0.38f, 0);
            beam.transform.localScale = new Vector3(1.95f, 0.06f, 0.06f);
            beam.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;

            // 2 Chrome Legs with curved feet
            for (int l = -1; l <= 1; l += 2)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg_" + l;
                leg.transform.SetParent(bench.transform, false);
                leg.transform.localPosition = new Vector3(l * 0.78f, 0.19f, 0);
                leg.transform.localScale = new Vector3(0.05f, 0.38f, 0.52f);
                leg.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;
            }

            // 3 Perforated Metal Seats
            for (int s = -1; s <= 1; s++)
            {
                GameObject seatGO = new GameObject("Seat_" + s);
                seatGO.transform.SetParent(bench.transform, false);
                seatGO.transform.localPosition = new Vector3(s * spacing, 0, 0);

                // Curved Seat Pan
                GameObject pan = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pan.name = "Pan";
                pan.transform.SetParent(seatGO.transform, false);
                pan.transform.localPosition = new Vector3(0, 0.44f, 0.05f);
                pan.transform.localScale = new Vector3(seatWidth, 0.04f, 0.48f);
                pan.GetComponent<MeshRenderer>().sharedMaterial = m_PerforatedMetal;

                // Backrest
                GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                back.name = "Back";
                back.transform.SetParent(seatGO.transform, false);
                back.transform.localPosition = new Vector3(0, 0.68f, -0.18f);
                back.transform.localScale = new Vector3(seatWidth, 0.46f, 0.04f);
                back.GetComponent<MeshRenderer>().sharedMaterial = m_PerforatedMetal;

                // Chrome Trim Edges
                GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trim.name = "Trim";
                trim.transform.SetParent(seatGO.transform, false);
                trim.transform.localPosition = new Vector3(0, 0.44f, -0.19f);
                trim.transform.localScale = new Vector3(seatWidth + 0.02f, 0.06f, 0.06f);
                trim.GetComponent<MeshRenderer>().sharedMaterial = m_Chrome;
            }
        }

        // Indoor Architectural Planter
        private static void BuildIndoorPlanter(GameObject parent, Vector3 localPos)
        {
            GameObject planter = new GameObject("IndoorPlanter");
            planter.transform.SetParent(parent.transform, false);
            planter.transform.localPosition = localPos;

            // White ceramic square box
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pot.name = "Pot";
            pot.transform.SetParent(planter.transform, false);
            pot.transform.localPosition = new Vector3(0, 0.35f, 0);
            pot.transform.localScale = new Vector3(0.65f, 0.7f, 0.65f);
            pot.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;

            // Soil
            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soil.name = "Soil";
            soil.transform.SetParent(planter.transform, false);
            soil.transform.localPosition = new Vector3(0, 0.68f, 0);
            soil.transform.localScale = new Vector3(0.60f, 0.06f, 0.60f);
            soil.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Lush green plant cluster
            for (int p = 0; p < 4; p++)
            {
                float ang = p * 90.0f;
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.name = "FoliageLeaf_" + p;
                leaf.transform.SetParent(planter.transform, false);
                leaf.transform.localPosition = new Vector3(Mathf.Cos(ang * Mathf.Deg2Rad) * 0.15f, 1.15f + (p % 2) * 0.2f, Mathf.Sin(ang * Mathf.Deg2Rad) * 0.15f);
                leaf.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
                leaf.GetComponent<MeshRenderer>().sharedMaterial = m_FoliageTree;
            }
        }

        // Wall Fan
        private static void BuildWallFan(GameObject wall, Vector3 localPos)
        {
            GameObject fan = new GameObject("WallFan");
            fan.transform.SetParent(wall.transform, false);
            fan.transform.localPosition = localPos;

            // Base mount
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = "Mount";
            b.transform.SetParent(fan.transform, false);
            b.transform.localScale = new Vector3(0.06f, 0.18f, 0.12f);
            b.GetComponent<MeshRenderer>().sharedMaterial = m_DarkMullion;

            // Fan Cage & Blade
            GameObject cage = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cage.name = "Cage";
            cage.transform.SetParent(fan.transform, false);
            cage.transform.localPosition = new Vector3(0.12f, -0.05f, 0);
            cage.transform.localRotation = Quaternion.Euler(15f, 0, 90f);
            cage.transform.localScale = new Vector3(0.45f, 0.06f, 0.45f);
            cage.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;
        }

        // Signboard
        private static void BuildSignboard(GameObject parent, Vector3 localPos, string text)
        {
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Signboard_" + text;
            sign.transform.SetParent(parent.transform, false);
            sign.transform.localPosition = localPos;
            sign.transform.localScale = new Vector3(2.8f, 0.35f, 0.05f);
            sign.GetComponent<MeshRenderer>().sharedMaterial = m_MatteWhite;
        }

        // ================= LIGHTING SETUP =================
        private static void BuildInteriorLighting(GameObject parent)
        {
            // Soft global fill light for the interior complex
            GameObject fillLightGO = new GameObject("Interior_Global_FillLight");
            fillLightGO.transform.SetParent(parent.transform, false);
            fillLightGO.transform.localPosition = new Vector3(0.0f, 3.4f, 0.0f);
            Light fillLight = fillLightGO.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.color = new Color(0.98f, 0.98f, 1.0f);
            fillLight.range = 30.0f;
            fillLight.intensity = 0.8f;
        }

        // ================= CAMERAS SETUP =================
        private static void BuildInteriorCameras(GameObject parent)
        {
            // Camera 1: Front Desk Area (Zone A)
            // Positioned to capture both cyan sofa on the left and curved desk on the right
            GameObject camA = new GameObject("Camera_Int_FrontDesk");
            camA.transform.SetParent(parent.transform, false);
            camA.transform.localPosition = new Vector3(-3.4f, 1.55f, 3.2f);
            camA.transform.localRotation = Quaternion.Euler(2.0f, -42.0f, 0.0f);
            Camera cA = camA.AddComponent<Camera>();
            cA.fieldOfView = 72.0f;
            cA.farClipPlane = 100.0f;

            // Camera 2: Waiting Lounge (Zone B)
            // Looking at the grand LED display wall and all 4 waiting bench clusters
            GameObject camB = new GameObject("Camera_Int_WaitingLounge");
            camB.transform.SetParent(parent.transform, false);
            camB.transform.localPosition = new Vector3(8.0f, 1.65f, 2.2f);
            camB.transform.localRotation = Quaternion.Euler(4.0f, 0.0f, 0.0f);
            Camera cB = camB.AddComponent<Camera>();
            cB.fieldOfView = 74.0f;
            cB.farClipPlane = 100.0f;

            // Camera 3: Training & Computer Lab (Zone C)
            // Dramatic deep aisle perspective showing glossy black marble, 12 workstations, white dividers, and transparent glass doors
            GameObject camC = new GameObject("Camera_Int_TrainingLab");
            camC.transform.SetParent(parent.transform, false);
            camC.transform.localPosition = new Vector3(-4.0f, 1.55f, -14.8f);
            camC.transform.localRotation = Quaternion.Euler(4.0f, 0.0f, 0.0f);
            Camera cC = camC.AddComponent<Camera>();
            cC.fieldOfView = 66.0f;
            cC.farClipPlane = 100.0f;

            // Camera 4: Connecting Corridor
            // Looking down circulation hallway showing doors, wayfinding, and view through glass walls
            GameObject camD = new GameObject("Camera_Int_Corridor");
            camD.transform.SetParent(parent.transform, false);
            camD.transform.localPosition = new Vector3(4.5f, 1.6f, 0.0f);
            camD.transform.localRotation = Quaternion.Euler(1.0f, -90.0f, 0.0f);
            Camera cD = camD.AddComponent<Camera>();
            cD.fieldOfView = 68.0f;
            cD.farClipPlane = 100.0f;
        }

        // ================= SCREENSHOT CAPTURE AUTOMATION =================
        private static void CaptureAllInteriorCameras(GameObject interiorRoot)
        {
            if (!Directory.Exists(ScreenshotDir)) Directory.CreateDirectory(ScreenshotDir);
            if (!Directory.Exists(ArtifactDir)) Directory.CreateDirectory(ArtifactDir);

            Camera[] cams = interiorRoot.GetComponentsInChildren<Camera>(true);
            foreach (var c in cams) c.enabled = false;

            foreach (var cam in cams)
            {
                string shotName = "";
                if (cam.gameObject.name == "Camera_Int_FrontDesk") shotName = "interior_front_desk.png";
                else if (cam.gameObject.name == "Camera_Int_WaitingLounge") shotName = "interior_waiting_lounge.png";
                else if (cam.gameObject.name == "Camera_Int_TrainingLab") shotName = "interior_training_lab.png";
                else if (cam.gameObject.name == "Camera_Int_Corridor") shotName = "interior_connecting_corridor.png";

                if (!string.IsNullOrEmpty(shotName))
                {
                    cam.enabled = true;
                    CaptureCameraToFile(cam, 1920, 1080, shotName);
                    cam.enabled = false;
                    Debug.Log("[IUH Interior] Captured: " + shotName);
                }
            }
        }

        private static void CaptureCameraToFile(Camera cam, int width, int height, string fileName)
        {
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            RenderTexture prevRT = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            cam.targetTexture = prevRT;
            RenderTexture.active = prevActive;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);

            // Save to project screenshots folder
            string projectPath = Path.Combine(ScreenshotDir, fileName);
            File.WriteAllBytes(projectPath, bytes);

            // Save to assistant artifact directory
            string artifactPath = Path.Combine(ArtifactDir, fileName);
            try
            {
                File.WriteAllBytes(artifactPath, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[IUH Interior] Could not copy to artifact dir: " + ex.Message);
            }
        }
    }
}
