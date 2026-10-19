using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;
using IUHCampus.UI;

namespace IUHCampus.Editor
{
    /// <summary>
    /// Automated Builder for Mini-Game 2.1 "Grab Campus: Chuyến xe tâm linh"
    /// and the In-Game Mini-Game Selection Portal Menu.
    /// Creates the Grab motorbike, delivery box, waypoints, ghost obstacles,
    /// dynamic HUD, and the full interactive selection portal UI in one click.
    /// </summary>
    public static class IUHGrabCampusBuilder
    {
        private const string MatDir = "Assets/IUH_Campus/Materials/";

        [MenuItem("IUH Campus/Setup Grab Campus Minigame & Mini-Game Menu (One-Click)")]
        public static void BuildGrabCampusAndMenu()
        {
            Debug.Log("[IUH Grab Campus Builder] Bắt đầu thiết lập Mini-Game 2.1 và Menu Chọn Game...");

            // 1. Ensure Materials
            var grabGreenMat = GetOrCreateMaterial("M_IUH_Grab_Green", new Color(0.0f, 0.69f, 0.31f), 0.1f, 0.85f);
            var grabBoxMat = GetOrCreateMaterial("M_IUH_Grab_Box", new Color(0.02f, 0.55f, 0.25f), 0.05f, 0.6f);
            var ghostMat = GetOrCreateMaterial("M_IUH_Ghost_Spooky", new Color(0.2f, 0.9f, 0.85f, 0.7f), 0f, 0.9f, true);
            var waypointMat = GetOrCreateMaterial("M_IUH_Waypoint_Holo", new Color(0.0f, 0.85f, 1.0f, 0.65f), 0f, 0.9f, true);
            var dropoffMat = GetOrCreateMaterial("M_IUH_Waypoint_Dropoff", new Color(0.1f, 1.0f, 0.4f, 0.65f), 0f, 0.9f, true);

            // 2. Setup or Find World Root
            var activeScene = EditorSceneManager.GetActiveScene();
            var gameplayRoot = GameObject.Find("GAMEPLAY_GRAB_CAMPUS");
            if (gameplayRoot != null) UnityEngine.Object.DestroyImmediate(gameplayRoot);
            gameplayRoot = new GameObject("GAMEPLAY_GRAB_CAMPUS");

            // 3. Build Grab Motorbike
            var motorbikeGO = BuildGrabMotorbike(gameplayRoot.transform, grabGreenMat, grabBoxMat);

            // 4. Build Ghosts
            var ghosts = BuildGhostHazards(gameplayRoot.transform, ghostMat);

            // 5. Build Waypoints
            var (pickupWp, dropoffWp) = BuildDeliveryWaypoints(gameplayRoot.transform, waypointMat, dropoffMat);

            // 6. Build Navigation Arrow
            var navArrow = BuildNavigationArrow(gameplayRoot.transform);

            // 7. Build UI Canvas
            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO != null) UnityEngine.Object.DestroyImmediate(canvasGO);
            canvasGO = BuildCanvasWithUI(gameplayRoot.transform);

            // 8. Wire Up Manager
            var grabMgr = gameplayRoot.AddComponent<IUHGrabCampusManager>();
            var portalUI = canvasGO.GetComponent<IUHMiniGamePortalUI>();

            // Assign fields via reflection or serialized object
            SerializedObject so = new SerializedObject(grabMgr);
            so.FindProperty("m_Motorbike").objectReferenceValue = motorbikeGO.GetComponent<IUHGrabMotorbike>();
            so.FindProperty("m_PickupWaypoint").objectReferenceValue = pickupWp;
            so.FindProperty("m_DropoffWaypoint").objectReferenceValue = dropoffWp;
            so.FindProperty("m_NavArrow").objectReferenceValue = navArrow.transform;

            var ghostListProp = so.FindProperty("m_GhostHazards");
            ghostListProp.ClearArray();
            for (int i = 0; i < ghosts.Count; i++)
            {
                ghostListProp.InsertArrayElementAtIndex(i);
                ghostListProp.GetArrayElementAtIndex(i).objectReferenceValue = ghosts[i];
            }

            // Wire HUD
            var hudPanel = canvasGO.transform.Find("DeliveryHUD")?.gameObject;
            so.FindProperty("m_MissionHudPanel").objectReferenceValue = hudPanel;
            if (hudPanel != null)
            {
                so.FindProperty("m_TxtMissionTitle").objectReferenceValue = hudPanel.transform.Find("TopCard/TxtTitle")?.GetComponent<Text>();
                so.FindProperty("m_TxtObjective").objectReferenceValue = hudPanel.transform.Find("TopCard/TxtObjective")?.GetComponent<Text>();
                so.FindProperty("m_TxtTimer").objectReferenceValue = hudPanel.transform.Find("TopCard/TxtTimer")?.GetComponent<Text>();
                so.FindProperty("m_TxtCargoIntegrity").objectReferenceValue = hudPanel.transform.Find("BottomCard/TxtCargo")?.GetComponent<Text>();
                so.FindProperty("m_TxtDistance").objectReferenceValue = hudPanel.transform.Find("BottomCard/TxtDistance")?.GetComponent<Text>();
                so.FindProperty("m_TxtSpeed").objectReferenceValue = hudPanel.transform.Find("BottomCard/TxtSpeed")?.GetComponent<Text>();
                so.FindProperty("m_ImgCargoBar").objectReferenceValue = hudPanel.transform.Find("BottomCard/CargoBarBg/CargoBarFill")?.GetComponent<Image>();
            }

            // Wire Results Dialogs
            var victoryDialog = canvasGO.transform.Find("VictoryDialog")?.gameObject;
            so.FindProperty("m_VictoryDialog").objectReferenceValue = victoryDialog;
            if (victoryDialog != null)
            {
                so.FindProperty("m_TxtVictoryReward").objectReferenceValue = victoryDialog.transform.Find("DialogCard/TxtReward")?.GetComponent<Text>();
                so.FindProperty("m_TxtVictoryRating").objectReferenceValue = victoryDialog.transform.Find("DialogCard/TxtRating")?.GetComponent<Text>();
            }

            var defeatDialog = canvasGO.transform.Find("DefeatDialog")?.gameObject;
            so.FindProperty("m_DefeatDialog").objectReferenceValue = defeatDialog;
            if (defeatDialog != null)
            {
                so.FindProperty("m_TxtDefeatReason").objectReferenceValue = defeatDialog.transform.Find("DialogCard/TxtReason")?.GetComponent<Text>();
            }

            so.ApplyModifiedProperties();

            // Wire Portal UI fields
            SerializedObject soPortal = new SerializedObject(portalUI);
            soPortal.FindProperty("m_MenuPanel").objectReferenceValue = canvasGO.transform.Find("PortalMenuPanel")?.gameObject;
            soPortal.FindProperty("m_FloatingButton").objectReferenceValue = canvasGO.transform.Find("FloatingPortalBtn")?.gameObject;
            var player = GameObject.Find("PlayerArmature");
            if (player != null)
            {
                soPortal.FindProperty("m_PlayerObject").objectReferenceValue = player;
                var moveScript = player.GetComponent("ThirdPersonController") as MonoBehaviour;
                if (moveScript != null) soPortal.FindProperty("m_PlayerMovementScript").objectReferenceValue = moveScript;
            }
            soPortal.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("[IUH Grab Campus Builder] HOÀN TẤT! Đã cài đặt hoàn chỉnh Mini-Game 2.1 và Menu Chọn Game [Phím M].");
        }

        private static GameObject BuildGrabMotorbike(Transform parent, Material grabGreenMat, Material grabBoxMat)
        {
            var bikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/IUH_Campus/Prefabs/Motorcycle.prefab");
            GameObject bikeGO;
            if (bikePrefab != null)
            {
                bikeGO = UnityEngine.Object.Instantiate(bikePrefab, parent);
                bikeGO.name = "IUH_Grab_Motorbike";
            }
            else
            {
                bikeGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bikeGO.name = "IUH_Grab_Motorbike";
                bikeGO.transform.SetParent(parent);
                bikeGO.transform.localScale = new Vector3(0.7f, 1.1f, 1.8f);
            }

            // Position at Gate 1 Parking Lot
            bikeGO.transform.position = new Vector3(398.0f, 0.55f, -18.0f);
            bikeGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // Apply Grab Green Material to Body and FrontCowl
            foreach (var r in bikeGO.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.name.Contains("Body") || r.name.Contains("FrontCowl") || r.name.Contains("Helmet"))
                {
                    r.sharedMaterial = grabGreenMat;
                }
            }

            // Remove static NavMeshObstacle or non-convex colliders that might interfere with movement
            var obstacle = bikeGO.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (obstacle != null) UnityEngine.Object.DestroyImmediate(obstacle);

            // Add Grab Delivery Box on the rear rack
            var deliveryBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deliveryBox.name = "Grab_Delivery_Box";
            deliveryBox.transform.SetParent(bikeGO.transform, false);
            deliveryBox.transform.localPosition = new Vector3(0f, 0.95f, -0.62f);
            deliveryBox.transform.localScale = new Vector3(0.55f, 0.48f, 0.45f);
            deliveryBox.GetComponent<MeshRenderer>().sharedMaterial = grabBoxMat;

            // Remove box collider from delivery box to avoid internal collision
            var boxCol = deliveryBox.GetComponent<Collider>();
            if (boxCol != null) UnityEngine.Object.DestroyImmediate(boxCol);

            // Add Headlight
            var lightGO = new GameObject("Bike_Headlight");
            lightGO.transform.SetParent(bikeGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0.75f, 0.85f);
            lightGO.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            var spotLight = lightGO.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.range = 35.0f;
            spotLight.spotAngle = 60.0f;
            spotLight.color = new Color(1.0f, 0.95f, 0.85f);
            spotLight.intensity = 3.5f;

            // Build Full 3D Human Rider Figure on the bike
            var riderGO = BuildRiderFigure(bikeGO.transform, grabGreenMat, grabBoxMat);

            // Add CharacterController & Controller Script
            var charCtrl = bikeGO.GetComponent<CharacterController>();
            if (charCtrl == null) charCtrl = bikeGO.AddComponent<CharacterController>();
            charCtrl.height = 1.3f;
            charCtrl.radius = 0.45f;
            charCtrl.center = new Vector3(0f, 0.65f, 0f);

            var bikeCtrl = bikeGO.AddComponent<IUHGrabMotorbike>();

            // Wire rider and camera target
            var soBike = new SerializedObject(bikeCtrl);
            soBike.FindProperty("m_RiderAvatar").objectReferenceValue = riderGO;
            var camTarget = bikeGO.transform.Find("CameraTarget");
            if (camTarget != null) soBike.FindProperty("m_CameraTarget").objectReferenceValue = camTarget;
            soBike.ApplyModifiedProperties();

            return bikeGO;
        }

        private static GameObject BuildRiderFigure(Transform bikeTransform, Material grabGreenMat, Material grabBoxMat)
        {
            var riderGO = new GameObject("Rider_Figure");
            riderGO.transform.SetParent(bikeTransform, false);
            riderGO.transform.localPosition = Vector3.zero;
            riderGO.transform.localRotation = Quaternion.identity;

            var darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Motorbike_Dark.mat");
            var pantsMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Student_DarkPants.mat");
            if (pantsMat == null) pantsMat = darkMat;

            // 1. Torso (wearing Grab delivery jacket)
            var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.name = "Rider_Torso";
            torso.transform.SetParent(riderGO.transform, false);
            torso.transform.localPosition = new Vector3(0f, 0.96f, -0.05f);
            torso.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            torso.transform.localScale = new Vector3(0.38f, 0.44f, 0.26f);
            torso.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(torso.GetComponent<Collider>());

            // 2. Head & Helmet with Visor
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Rider_Head";
            head.transform.SetParent(riderGO.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.32f, 0.08f);
            head.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            head.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

            var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Rider_Visor";
            visor.transform.SetParent(head.transform, false);
            visor.transform.localPosition = new Vector3(0f, 0.02f, 0.42f);
            visor.transform.localScale = new Vector3(0.72f, 0.28f, 0.22f);
            visor.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(visor.GetComponent<Collider>());

            // 3. Left Arm (reaching forward to left handlebar)
            var armL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            armL.name = "Rider_Arm_L";
            armL.transform.SetParent(riderGO.transform, false);
            armL.transform.localPosition = new Vector3(-0.24f, 1.08f, 0.16f);
            armL.transform.localRotation = Quaternion.Euler(48f, -12f, 0f);
            armL.transform.localScale = new Vector3(0.10f, 0.26f, 0.10f);
            armL.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(armL.GetComponent<Collider>());

            // 4. Right Arm (reaching forward to right handlebar)
            var armR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            armR.name = "Rider_Arm_R";
            armR.transform.SetParent(riderGO.transform, false);
            armR.transform.localPosition = new Vector3(0.24f, 1.08f, 0.16f);
            armR.transform.localRotation = Quaternion.Euler(48f, 12f, 0f);
            armR.transform.localScale = new Vector3(0.10f, 0.26f, 0.10f);
            armR.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(armR.GetComponent<Collider>());

            // 5. Left Leg
            var thighL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            thighL.name = "Rider_Thigh_L";
            thighL.transform.SetParent(riderGO.transform, false);
            thighL.transform.localPosition = new Vector3(-0.20f, 0.72f, 0.05f);
            thighL.transform.localRotation = Quaternion.Euler(65f, -15f, 0f);
            thighL.transform.localScale = new Vector3(0.12f, 0.24f, 0.12f);
            thighL.GetComponent<MeshRenderer>().sharedMaterial = pantsMat;
            UnityEngine.Object.DestroyImmediate(thighL.GetComponent<Collider>());

            var shinL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shinL.name = "Rider_Shin_L";
            shinL.transform.SetParent(riderGO.transform, false);
            shinL.transform.localPosition = new Vector3(-0.22f, 0.45f, 0.18f);
            shinL.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            shinL.transform.localScale = new Vector3(0.11f, 0.24f, 0.11f);
            shinL.GetComponent<MeshRenderer>().sharedMaterial = pantsMat;
            UnityEngine.Object.DestroyImmediate(shinL.GetComponent<Collider>());

            // 6. Right Leg
            var thighR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            thighR.name = "Rider_Thigh_R";
            thighR.transform.SetParent(riderGO.transform, false);
            thighR.transform.localPosition = new Vector3(0.20f, 0.72f, 0.05f);
            thighR.transform.localRotation = Quaternion.Euler(65f, 15f, 0f);
            thighR.transform.localScale = new Vector3(0.12f, 0.24f, 0.12f);
            thighR.GetComponent<MeshRenderer>().sharedMaterial = pantsMat;
            UnityEngine.Object.DestroyImmediate(thighR.GetComponent<Collider>());

            var shinR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shinR.name = "Rider_Shin_R";
            shinR.transform.SetParent(riderGO.transform, false);
            shinR.transform.localPosition = new Vector3(0.22f, 0.45f, 0.18f);
            shinR.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            shinR.transform.localScale = new Vector3(0.11f, 0.24f, 0.11f);
            shinR.GetComponent<MeshRenderer>().sharedMaterial = pantsMat;
            UnityEngine.Object.DestroyImmediate(shinR.GetComponent<Collider>());

            // 7. Thermal Backpack on rider's back
            var pack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pack.name = "Rider_Backpack";
            pack.transform.SetParent(riderGO.transform, false);
            pack.transform.localPosition = new Vector3(0f, 1.05f, -0.22f);
            pack.transform.localScale = new Vector3(0.38f, 0.42f, 0.22f);
            pack.GetComponent<MeshRenderer>().sharedMaterial = grabBoxMat;
            UnityEngine.Object.DestroyImmediate(pack.GetComponent<Collider>());

            // 8. CameraTarget for Cinemachine
            var camTarget = new GameObject("CameraTarget");
            camTarget.transform.SetParent(bikeTransform, false);
            camTarget.transform.localPosition = new Vector3(0f, 1.35f, -0.15f);

            return riderGO;
        }

        private static List<GameObject> BuildGhostHazards(Transform parent, Material ghostMat)
        {
            var ghostsRoot = new GameObject("GHOST_HAZARDS");
            ghostsRoot.transform.SetParent(parent, false);

            var ghostList = new List<GameObject>();

            // 4 Key Ghost Patrol positions along the road network
            Vector3[] ghostPositions = new Vector3[]
            {
                new Vector3(385.0f, 1.2f, -38.0f), // Courtyard path
                new Vector3(360.0f, 1.2f, -80.0f), // Towards Block V / KTX
                new Vector3(345.0f, 1.2f, -70.0f), // Near Gate 4 / KTX entrance
                new Vector3(400.0f, 1.2f, -24.0f)  // Near Library road
            };

            for (int i = 0; i < ghostPositions.Length; i++)
            {
                var ghostGO = new GameObject($"Ghost_Hazard_0{i + 1}");
                ghostGO.transform.SetParent(ghostsRoot.transform);
                ghostGO.transform.position = ghostPositions[i];

                // Visual apparition: capsule body + sphere head
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "GhostBody";
                body.transform.SetParent(ghostGO.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                body.transform.localScale = new Vector3(0.55f, 0.7f, 0.55f);
                body.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;

                // Glowing spectral eyes
                var eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeL.name = "Eye_L";
                eyeL.transform.SetParent(ghostGO.transform, false);
                eyeL.transform.localPosition = new Vector3(-0.12f, 1.0f, 0.22f);
                eyeL.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                var eyeMat = GetOrCreateMaterial("M_IUH_Ghost_Eyes", Color.red, 0f, 1.0f);
                eyeL.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;

                var eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeR.name = "Eye_R";
                eyeR.transform.SetParent(ghostGO.transform, false);
                eyeR.transform.localPosition = new Vector3(0.12f, 1.0f, 0.22f);
                eyeR.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                eyeR.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;

                // Trigger Sphere Collider
                var col = ghostGO.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 1.4f;
                col.center = new Vector3(0f, 0.8f, 0f);

                ghostGO.AddComponent<IUHGhostHazard>();
                ghostList.Add(ghostGO);
            }

            return ghostList;
        }

        private static (IUHDeliveryWaypoint, IUHDeliveryWaypoint) BuildDeliveryWaypoints(Transform parent, Material pickupMat, Material dropoffMat)
        {
            var wpRoot = new GameObject("DELIVERY_WAYPOINTS");
            wpRoot.transform.SetParent(parent, false);

            var pickupGO = CreateWaypointGO("Waypoint_Pickup", wpRoot.transform, pickupMat, IUHDeliveryWaypoint.WaypointType.Pickup);
            var dropoffGO = CreateWaypointGO("Waypoint_Dropoff", wpRoot.transform, dropoffMat, IUHDeliveryWaypoint.WaypointType.Dropoff);

            return (pickupGO.GetComponent<IUHDeliveryWaypoint>(), dropoffGO.GetComponent<IUHDeliveryWaypoint>());
        }

        private static GameObject CreateWaypointGO(string name, Transform parent, Material mat, IUHDeliveryWaypoint.WaypointType type)
        {
            var wpGO = new GameObject(name);
            wpGO.transform.SetParent(parent);

            // Vertical holographic cylinder
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "Beam";
            beam.transform.SetParent(wpGO.transform, false);
            beam.transform.localPosition = new Vector3(0f, 4.0f, 0f);
            beam.transform.localScale = new Vector3(2.5f, 4.0f, 2.5f);
            beam.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var beamCol = beam.GetComponent<Collider>();
            if (beamCol != null) UnityEngine.Object.DestroyImmediate(beamCol);

            // Ground pulsing ring (flat cylinder)
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "GroundRing";
            ring.transform.SetParent(wpGO.transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            ring.transform.localScale = new Vector3(3.5f, 0.02f, 3.5f);
            ring.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var ringCol = ring.GetComponent<Collider>();
            if (ringCol != null) UnityEngine.Object.DestroyImmediate(ringCol);

            // Floating icon (diamond / cube)
            var icon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            icon.name = "FloatingIcon";
            icon.transform.SetParent(wpGO.transform, false);
            icon.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            icon.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);
            icon.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            icon.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var iconCol = icon.GetComponent<Collider>();
            if (iconCol != null) UnityEngine.Object.DestroyImmediate(iconCol);

            // Point Light
            var lightGO = new GameObject("PointLight");
            lightGO.transform.SetParent(wpGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var pLight = lightGO.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.range = 8.0f;
            pLight.intensity = 3.0f;

            // Trigger Sphere
            var triggerCol = wpGO.AddComponent<SphereCollider>();
            triggerCol.isTrigger = true;
            triggerCol.radius = 3.5f;

            var wp = wpGO.AddComponent<IUHDeliveryWaypoint>();
            return wpGO;
        }

        private static GameObject BuildNavigationArrow(Transform parent)
        {
            var arrowGO = new GameObject("Nav_Compass_Arrow");
            arrowGO.transform.SetParent(parent);

            var cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = "ArrowShaft";
            cone.transform.SetParent(arrowGO.transform, false);
            cone.transform.localPosition = new Vector3(0f, 0f, 0.4f);
            cone.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cone.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
            cone.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("M_IUH_Nav_Arrow", Color.yellow, 0.1f, 0.9f);
            var col = cone.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            arrowGO.SetActive(false);
            return arrowGO;
        }

        private static GameObject BuildCanvasWithUI(Transform parent)
        {
            var canvasGO = new GameObject("IUH_Minigame_Canvas");
            canvasGO.transform.SetParent(parent);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 1.0f; // Match Height to prevent vertical truncation on ultra-wide screens
            scaler.dynamicPixelsPerUnit = 2.0f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem exists
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSys = new GameObject("EventSystem");
                eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 1. Floating Button [🎮 Mini-Games (M)]
            BuildFloatingPortalButton(canvasGO.transform);

            // 2. Full Modal Selection Portal Panel
            BuildPortalMenuPanel(canvasGO.transform);

            // 3. Delivery HUD Panel
            BuildDeliveryHUD(canvasGO.transform);

            // 4. Results Dialogs (Victory & Defeat)
            BuildResultDialogs(canvasGO.transform);

            canvasGO.AddComponent<IUHMiniGamePortalUI>();

            return canvasGO;
        }

        private static void BuildFloatingPortalButton(Transform canvasTransform)
        {
            var btnGO = new GameObject("FloatingPortalBtn");
            btnGO.transform.SetParent(canvasTransform, false);

            var rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-25f, -25f);
            rect.sizeDelta = new Vector2(300f, 62f);

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.06f, 0.10f, 0.20f, 0.96f);

            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.90f, 1.0f, 1.0f);
            outline.effectDistance = new Vector2(2.5f, 2.5f);

            var btn = btnGO.AddComponent<Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var txt = textGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = "🎮 MENU MINI-GAME  <color=#FACC15><b>[ M ]</b></color>";
            txt.fontSize = 20;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;
        }

        private static void BuildPortalMenuPanel(Transform canvasTransform)
        {
            var panelGO = new GameObject("PortalMenuPanel");
            panelGO.transform.SetParent(canvasTransform, false);

            var rect = panelGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1480f, 820f);

            var img = panelGO.AddComponent<Image>();
            img.color = new Color(0.03f, 0.06f, 0.12f, 0.98f);

            var outline = panelGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.75f, 1.0f, 0.9f);
            outline.effectDistance = new Vector2(3f, 3f);

            // Header Banner
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(panelGO.transform, false);
            var hRect = headerGO.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.anchoredPosition = new Vector2(0f, -10f);
            hRect.sizeDelta = new Vector2(0f, 85f);
            var hTxt = headerGO.AddComponent<Text>();
            hTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hTxt.text = "<size=28><color=#FEF08A><b>🏛️ CAMPUS ADVENTURE — CHỌN MINI-GAME</b></color></size>\n<size=15><color=#7DD3FC>⚡ Bấm phím số <b>[ 1 ]</b> đến <b>[ 6 ]</b> hoặc CLICK chuột để chọn trò chơi • Bấm <b>[ M ]</b> hoặc <b>[ ESC ]</b> để đóng</color></size>";
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = Color.white;

            // Close Button [X]
            var closeBtnGO = new GameObject("CloseBtn");
            closeBtnGO.transform.SetParent(panelGO.transform, false);
            var cRect = closeBtnGO.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(1f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(1f, 1f);
            cRect.anchoredPosition = new Vector2(-15f, -15f);
            cRect.sizeDelta = new Vector2(46f, 46f);
            var cImg = closeBtnGO.AddComponent<Image>();
            cImg.color = new Color(0.9f, 0.2f, 0.2f, 0.95f);
            var cBtn = closeBtnGO.AddComponent<Button>();
            var cOutline = closeBtnGO.AddComponent<Outline>();
            cOutline.effectColor = Color.white;
            cOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var cTxtGO = new GameObject("Txt");
            cTxtGO.transform.SetParent(closeBtnGO.transform, false);
            var cTxt = cTxtGO.AddComponent<Text>();
            cTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cTxt.text = "✕";
            cTxt.fontSize = 26;
            cTxt.alignment = TextAnchor.MiddleCenter;
            cTxt.color = Color.white;
            cTxt.fontStyle = FontStyle.Bold;
            var cTxtRect = cTxtGO.GetComponent<RectTransform>();
            cTxtRect.anchorMin = Vector2.zero;
            cTxtRect.anchorMax = Vector2.one;

            // Cards Grid Area
            var cardsArea = new GameObject("CardsArea");
            cardsArea.transform.SetParent(panelGO.transform, false);
            var caRect = cardsArea.AddComponent<RectTransform>();
            caRect.anchorMin = new Vector2(0.02f, 0f);
            caRect.anchorMax = new Vector2(0.98f, 1f);
            caRect.offsetMin = new Vector2(0f, 65f);
            caRect.offsetMax = new Vector2(0f, -100f);

            var grid = cardsArea.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(450f, 310f);
            grid.spacing = new Vector2(20f, 15f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // 6 Mini-Game Cards
            BuildMiniGameCard(cardsArea.transform, 1, "🛵 2.1. Grab Campus: Chuyến Xe Tâm Linh", "Bạn (Gia Thịnh)", "Lái xe máy Grab đêm, tránh bóng ma và chướng ngại vật, giao cơm/sách/linh kiện tới KTX đúng hạn.", "Bãi Xe Cổng 1 (Nguyễn Văn Bảo)", "⚡ BẤM [ 1 ] ĐỂ CHƠI NGAY", true, 0);
            BuildMiniGameCard(cardsArea.transform, 2, "👻 2.2. Trốn Tìm Với Ma Ở Ký Túc Xá", "Bạn (Gia Thịnh)", "Khám phá phòng tối KTX, né tránh ma tuần tra AI NavMesh 4 trạng thái, tìm chìa khóa & nhật ký.", "Sảnh Ký Túc Xá Block D", "📍 BẤM [ 2 ] ĐỂ ĐẾN KTX", false, 1);
            BuildMiniGameCard(cardsArea.transform, 3, "🔍 2.3. AI Object Hunt: Tìm Đồ Vật", "Bạn (Gia Thịnh)", "Sử dụng camera quét nhận diện sách cổ/bình chữa cháy qua Deep Learning YOLO & AR Hologram.", "Thư Viện Trung Tâm Block A", "📍 BẤM [ 3 ] ĐỂ ĐẾN THƯ VIỆN", false, 2);
            BuildMiniGameCard(cardsArea.transform, 4, "📸 2.4. Thợ Săn Khoảnh Khắc", "Minh Nhật", "Khám phá khuôn viên, leo thang nóc nhà Block F chụp ảnh check-in và thu thập huy hiệu.", "Cầu Thang Lên Nóc Block F", "📍 BẤM [ 4 ] ĐỂ ĐẾN NÓC NHÀ", false, 3);
            BuildMiniGameCard(cardsArea.transform, 5, "🌱 2.5. Green Campus: Cứu Lấy Trường", "Minh Nhật", "Thu gom và phân loại rác tái chế bằng AI, tích lũy điểm xanh đẩy lên Cloud Adafruit IO.", "Trạm Phân Loại Rác Sân Trường", "📍 BẤM [ 5 ] ĐỂ ĐẾN TRẠM RÁC", false, 4);
            BuildMiniGameCard(cardsArea.transform, 6, "🎪 2.6. Campus Festival: Tổ Chức Lễ Hội", "Minh Nhật", "Quản lý ngân sách, trang trí sân khấu đại hội âm nhạc và kết nối đồng bộ Docker backend.", "Sân Khấu Đại Hội Trung Tâm", "📍 BẤM [ 6 ] ĐỂ ĐẾN SÂN KHẤU", false, 5);

            // Footer Instruction Bar
            var footerGO = new GameObject("Footer");
            footerGO.transform.SetParent(panelGO.transform, false);
            var fRect = footerGO.AddComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0f, 0f);
            fRect.anchorMax = new Vector2(1f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.anchoredPosition = new Vector2(0f, 10f);
            fRect.sizeDelta = new Vector2(0f, 48f);
            var fImg = footerGO.AddComponent<Image>();
            fImg.color = new Color(0.06f, 0.12f, 0.22f, 0.95f);

            var fTxtGO = new GameObject("Txt");
            fTxtGO.transform.SetParent(footerGO.transform, false);
            var fTxtRect = fTxtGO.AddComponent<RectTransform>();
            fTxtRect.anchorMin = Vector2.zero;
            fTxtRect.anchorMax = Vector2.one;
            var fTxt = fTxtGO.AddComponent<Text>();
            fTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            fTxt.text = "💡 <color=#FDE047><b>MẸO NHANH:</b></color> Bạn có thể bấm phím số <b>[1]</b>, <b>[2]</b>, <b>[3]</b>, <b>[4]</b>, <b>[5]</b>, <b>[6]</b> bất cứ lúc nào để chọn game tức thì!";
            fTxt.fontSize = 15;
            fTxt.alignment = TextAnchor.MiddleCenter;
            fTxt.color = Color.white;
            fTxt.fontStyle = FontStyle.Bold;
        }

        private static void BuildMiniGameCard(Transform parent, int hotkeyNum, string title, string author, string desc, string location, string btnLabel, bool isGrabGame, int cardIndex)
        {
            var cardGO = new GameObject($"Card_{cardIndex}_Game{hotkeyNum}");
            cardGO.transform.SetParent(parent, false);

            var img = cardGO.AddComponent<Image>();
            img.color = isGrabGame ? new Color(0.04f, 0.18f, 0.12f, 0.96f) : new Color(0.07f, 0.13f, 0.24f, 0.96f);

            var outline = cardGO.AddComponent<Outline>();
            outline.effectColor = isGrabGame ? new Color(0.0f, 0.95f, 0.5f, 1.0f) : new Color(0.25f, 0.55f, 0.95f, 0.9f);
            outline.effectDistance = new Vector2(2.5f, 2.5f);

            var cardBtn = cardGO.AddComponent<Button>();

            // Card Content Vertical Layout
            var vlg = cardGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // 1. Top Row: Hotkey Badge [PHÍM X] + Author Pill
            var topRow = new GameObject("TopRow");
            topRow.transform.SetParent(cardGO.transform, false);
            var topHlg = topRow.AddComponent<HorizontalLayoutGroup>();
            topHlg.childControlWidth = false;
            topHlg.childControlHeight = true;
            topHlg.spacing = 10f;
            topRow.AddComponent<LayoutElement>().preferredHeight = 30f;

            // Hotkey Badge
            var keyBadge = new GameObject("KeyBadge");
            keyBadge.transform.SetParent(topRow.transform, false);
            var kbImg = keyBadge.AddComponent<Image>();
            kbImg.color = isGrabGame ? new Color(1.0f, 0.85f, 0.15f) : new Color(0.2f, 0.85f, 1.0f);
            var kbLe = keyBadge.AddComponent<LayoutElement>();
            kbLe.preferredWidth = 105f;
            var kbTxtGO = new GameObject("Txt");
            kbTxtGO.transform.SetParent(keyBadge.transform, false);
            var kbTxtRect = kbTxtGO.AddComponent<RectTransform>();
            kbTxtRect.anchorMin = Vector2.zero;
            kbTxtRect.anchorMax = Vector2.one;
            var kbTxt = kbTxtGO.AddComponent<Text>();
            kbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            kbTxt.text = $"[ PHÍM {hotkeyNum} ]";
            kbTxt.fontSize = 15;
            kbTxt.alignment = TextAnchor.MiddleCenter;
            kbTxt.color = Color.black;
            kbTxt.fontStyle = FontStyle.Bold;

            // Author Pill
            var authorPill = new GameObject("AuthorPill");
            authorPill.transform.SetParent(topRow.transform, false);
            var apImg = authorPill.AddComponent<Image>();
            apImg.color = isGrabGame ? new Color(0.06f, 0.45f, 0.25f, 0.9f) : new Color(0.15f, 0.35f, 0.65f, 0.9f);
            var apLe = authorPill.AddComponent<LayoutElement>();
            apLe.preferredWidth = 135f;
            var apTxtGO = new GameObject("Txt");
            apTxtGO.transform.SetParent(authorPill.transform, false);
            var apTxtRect = apTxtGO.AddComponent<RectTransform>();
            apTxtRect.anchorMin = Vector2.zero;
            apTxtRect.anchorMax = Vector2.one;
            var apTxt = apTxtGO.AddComponent<Text>();
            apTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            apTxt.text = author;
            apTxt.fontSize = 13;
            apTxt.alignment = TextAnchor.MiddleCenter;
            apTxt.color = Color.white;
            apTxt.fontStyle = FontStyle.Bold;

            // 2. Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(cardGO.transform, false);
            var titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.text = $"<b>{title}</b>";
            titleTxt.fontSize = 17;
            titleTxt.color = isGrabGame ? new Color(0.5f, 1.0f, 0.7f) : Color.white;
            titleTxt.fontStyle = FontStyle.Bold;
            titleGO.AddComponent<LayoutElement>().preferredHeight = 36f;

            // 3. Description
            var descGO = new GameObject("Desc");
            descGO.transform.SetParent(cardGO.transform, false);
            var descTxt = descGO.AddComponent<Text>();
            descTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descTxt.text = desc;
            descTxt.fontSize = 13;
            descTxt.lineSpacing = 1.2f;
            descTxt.color = new Color(0.92f, 0.95f, 0.98f);
            descGO.AddComponent<LayoutElement>().preferredHeight = 78f;

            // 4. Location Tag
            var locGO = new GameObject("Location");
            locGO.transform.SetParent(cardGO.transform, false);
            var locTxt = locGO.AddComponent<Text>();
            locTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            locTxt.text = $"📍 <b>Vị trí:</b> <color=#38BDF8>{location}</color>";
            locTxt.fontSize = 12;
            locTxt.color = new Color(0.75f, 0.85f, 0.95f);
            locGO.AddComponent<LayoutElement>().preferredHeight = 24f;

            // 5. Action Button
            var btnBarGO = new GameObject("BtnBar");
            btnBarGO.transform.SetParent(cardGO.transform, false);
            var hlg = btnBarGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            btnBarGO.AddComponent<LayoutElement>().preferredHeight = 44f;

            var actionBtnGO = new GameObject("ActionBtn");
            actionBtnGO.transform.SetParent(btnBarGO.transform, false);
            var aImg = actionBtnGO.AddComponent<Image>();
            aImg.color = isGrabGame ? new Color(0.0f, 0.68f, 0.32f, 1f) : new Color(0.14f, 0.48f, 0.88f, 1f);
            var aBtn = actionBtnGO.AddComponent<Button>();
            var aOutline = actionBtnGO.AddComponent<Outline>();
            aOutline.effectColor = isGrabGame ? new Color(0.4f, 1f, 0.7f) : new Color(0.6f, 0.85f, 1f);
            aOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var aTxtGO = new GameObject("Txt");
            aTxtGO.transform.SetParent(actionBtnGO.transform, false);
            var aTxtRect = aTxtGO.AddComponent<RectTransform>();
            aTxtRect.anchorMin = Vector2.zero;
            aTxtRect.anchorMax = Vector2.one;
            var aTxt = aTxtGO.AddComponent<Text>();
            aTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            aTxt.text = btnLabel;
            aTxt.fontSize = 15;
            aTxt.alignment = TextAnchor.MiddleCenter;
            aTxt.color = Color.white;
            aTxt.fontStyle = FontStyle.Bold;
        }

        private static void BuildDeliveryHUD(Transform canvasTransform)
        {
            var hudGO = new GameObject("DeliveryHUD");
            hudGO.transform.SetParent(canvasTransform, false);
            var rect = hudGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Top Mission Card (Centered at top)
            var topCard = new GameObject("TopCard");
            topCard.transform.SetParent(hudGO.transform, false);
            var topRect = topCard.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0.5f, 1f);
            topRect.anchorMax = new Vector2(0.5f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.anchoredPosition = new Vector2(0f, -20f);
            topRect.sizeDelta = new Vector2(740f, 95f);
            var topImg = topCard.AddComponent<Image>();
            topImg.color = new Color(0.04f, 0.1f, 0.18f, 0.94f);
            var topOutline = topCard.AddComponent<Outline>();
            topOutline.effectColor = new Color(0f, 0.85f, 0.45f, 0.9f);
            topOutline.effectDistance = new Vector2(2f, 2f);

            // Title
            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(topCard.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.03f, 0.52f);
            titleRect.anchorMax = new Vector2(0.68f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.text = "🛵 Grab Campus: Chuyến Xe Tâm Linh";
            titleTxt.fontSize = 18;
            titleTxt.color = Color.white;
            titleTxt.fontStyle = FontStyle.Bold;

            // Objective
            var objGO = new GameObject("TxtObjective");
            objGO.transform.SetParent(topCard.transform, false);
            var objRect = objGO.AddComponent<RectTransform>();
            objRect.anchorMin = new Vector2(0.03f, 0.08f);
            objRect.anchorMax = new Vector2(0.68f, 0.5f);
            objRect.offsetMin = Vector2.zero;
            objRect.offsetMax = Vector2.zero;
            var objTxt = objGO.AddComponent<Text>();
            objTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            objTxt.text = "[1/2] Lái xe đến điểm nhận hàng Căn Tin";
            objTxt.fontSize = 14;
            objTxt.color = new Color(0.7f, 0.95f, 0.85f);

            // Timer
            var timerGO = new GameObject("TxtTimer");
            timerGO.transform.SetParent(topCard.transform, false);
            var timerRect = timerGO.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.7f, 0.1f);
            timerRect.anchorMax = new Vector2(0.97f, 0.9f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            var timerTxt = timerGO.AddComponent<Text>();
            timerTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timerTxt.text = "⏱ 01:15";
            timerTxt.fontSize = 28;
            timerTxt.alignment = TextAnchor.MiddleCenter;
            timerTxt.color = Color.yellow;
            timerTxt.fontStyle = FontStyle.Bold;

            // Bottom Gauge Card (Centered at bottom)
            var bottomCard = new GameObject("BottomCard");
            bottomCard.transform.SetParent(hudGO.transform, false);
            var bRect = bottomCard.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0f);
            bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.anchoredPosition = new Vector2(0f, 25f);
            bRect.sizeDelta = new Vector2(640f, 85f);
            var bImg = bottomCard.AddComponent<Image>();
            bImg.color = new Color(0.04f, 0.08f, 0.15f, 0.94f);
            var bOutline = bottomCard.AddComponent<Outline>();
            bOutline.effectColor = new Color(0f, 0.75f, 1f, 0.8f);
            bOutline.effectDistance = new Vector2(2f, 2f);

            // Cargo Text
            var cargoTxtGO = new GameObject("TxtCargo");
            cargoTxtGO.transform.SetParent(bottomCard.transform, false);
            var ctRect = cargoTxtGO.AddComponent<RectTransform>();
            ctRect.anchorMin = new Vector2(0.04f, 0.52f);
            ctRect.anchorMax = new Vector2(0.48f, 0.95f);
            ctRect.offsetMin = Vector2.zero;
            ctRect.offsetMax = Vector2.zero;
            var ct = cargoTxtGO.AddComponent<Text>();
            ct.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ct.text = "📦 Kiện hàng: 100%";
            ct.fontSize = 16;
            ct.color = Color.green;
            ct.fontStyle = FontStyle.Bold;

            // Cargo Bar Background
            var barBgGO = new GameObject("CargoBarBg");
            barBgGO.transform.SetParent(bottomCard.transform, false);
            var barBgRect = barBgGO.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.04f, 0.15f);
            barBgRect.anchorMax = new Vector2(0.48f, 0.45f);
            barBgRect.offsetMin = Vector2.zero;
            barBgRect.offsetMax = Vector2.zero;
            var barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Cargo Bar Fill
            var barFillGO = new GameObject("CargoBarFill");
            barFillGO.transform.SetParent(barBgGO.transform, false);
            var barFillRect = barFillGO.AddComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = Vector2.one;
            barFillRect.offsetMin = Vector2.zero;
            barFillRect.offsetMax = Vector2.zero;
            var barFillImg = barFillGO.AddComponent<Image>();
            barFillImg.color = new Color(0f, 0.85f, 0.45f, 1f);
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillAmount = 1.0f;

            // Distance Text
            var distTxtGO = new GameObject("TxtDistance");
            distTxtGO.transform.SetParent(bottomCard.transform, false);
            var dtRect = distTxtGO.AddComponent<RectTransform>();
            dtRect.anchorMin = new Vector2(0.52f, 0.52f);
            dtRect.anchorMax = new Vector2(0.82f, 0.95f);
            dtRect.offsetMin = Vector2.zero;
            dtRect.offsetMax = Vector2.zero;
            var dt = distTxtGO.AddComponent<Text>();
            dt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dt.text = "📍 Đến KTX: 85m";
            dt.fontSize = 14;
            dt.color = Color.white;

            // Speed Text
            var spdTxtGO = new GameObject("TxtSpeed");
            spdTxtGO.transform.SetParent(bottomCard.transform, false);
            var spdRect = spdTxtGO.AddComponent<RectTransform>();
            spdRect.anchorMin = new Vector2(0.83f, 0.15f);
            spdRect.anchorMax = new Vector2(0.98f, 0.85f);
            spdRect.offsetMin = Vector2.zero;
            spdRect.offsetMax = Vector2.zero;
            var spd = spdTxtGO.AddComponent<Text>();
            spd.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            spd.text = "0\n<size=12>km/h</size>";
            spd.fontSize = 22;
            spd.alignment = TextAnchor.MiddleCenter;
            spd.color = new Color(0f, 0.85f, 1f);
            spd.fontStyle = FontStyle.Bold;

            hudGO.SetActive(false);
        }

        private static void BuildResultDialogs(Transform canvasTransform)
        {
            // 1. Victory Dialog
            var vicGO = new GameObject("VictoryDialog");
            vicGO.transform.SetParent(canvasTransform, false);
            var vRect = vicGO.AddComponent<RectTransform>();
            vRect.anchorMin = Vector2.zero;
            vRect.anchorMax = Vector2.one;
            vRect.offsetMin = Vector2.zero;
            vRect.offsetMax = Vector2.zero;

            var vCard = new GameObject("DialogCard");
            vCard.transform.SetParent(vicGO.transform, false);
            var vcRect = vCard.AddComponent<RectTransform>();
            vcRect.anchorMin = new Vector2(0.32f, 0.3f);
            vcRect.anchorMax = new Vector2(0.68f, 0.7f);
            vcRect.offsetMin = Vector2.zero;
            vcRect.offsetMax = Vector2.zero;
            var vcImg = vCard.AddComponent<Image>();
            vcImg.color = new Color(0.04f, 0.12f, 0.08f, 0.98f);
            var vcOutline = vCard.AddComponent<Outline>();
            vcOutline.effectColor = new Color(0f, 1f, 0.45f, 0.95f);
            vcOutline.effectDistance = new Vector2(3f, 3f);

            var vTitle = new GameObject("TxtTitle");
            vTitle.transform.SetParent(vCard.transform, false);
            var vtRect = vTitle.AddComponent<RectTransform>();
            vtRect.anchorMin = new Vector2(0.05f, 0.72f);
            vtRect.anchorMax = new Vector2(0.95f, 0.95f);
            vtRect.offsetMin = Vector2.zero;
            vtRect.offsetMax = Vector2.zero;
            var vt = vTitle.AddComponent<Text>();
            vt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vt.text = "🎉 GIAO HÀNG THÀNH CÔNG!";
            vt.fontSize = 24;
            vt.alignment = TextAnchor.MiddleCenter;
            vt.color = Color.white;
            vt.fontStyle = FontStyle.Bold;

            var vRating = new GameObject("TxtRating");
            vRating.transform.SetParent(vCard.transform, false);
            var vrRect = vRating.AddComponent<RectTransform>();
            vrRect.anchorMin = new Vector2(0.05f, 0.52f);
            vrRect.anchorMax = new Vector2(0.95f, 0.72f);
            vrRect.offsetMin = Vector2.zero;
            vrRect.offsetMax = Vector2.zero;
            var vr = vRating.AddComponent<Text>();
            vr.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vr.text = "⭐⭐⭐ (3/3 Sao)";
            vr.fontSize = 20;
            vr.alignment = TextAnchor.MiddleCenter;
            vr.color = Color.yellow;

            var vReward = new GameObject("TxtReward");
            vReward.transform.SetParent(vCard.transform, false);
            var vrwRect = vReward.AddComponent<RectTransform>();
            vrwRect.anchorMin = new Vector2(0.05f, 0.28f);
            vrwRect.anchorMax = new Vector2(0.95f, 0.52f);
            vrwRect.offsetMin = Vector2.zero;
            vrwRect.offsetMax = Vector2.zero;
            var vrw = vReward.AddComponent<Text>();
            vrw.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vrw.text = "+75,000 VNĐ  (+150 EXP)\nThưởng nguyên vẹn (100%): +15,000 VNĐ";
            vrw.fontSize = 15;
            vrw.alignment = TextAnchor.MiddleCenter;
            vrw.color = new Color(0.85f, 0.95f, 0.9f);

            var vBtn = new GameObject("BtnContinue");
            vBtn.transform.SetParent(vCard.transform, false);
            var vbRect = vBtn.AddComponent<RectTransform>();
            vbRect.anchorMin = new Vector2(0.3f, 0.08f);
            vbRect.anchorMax = new Vector2(0.7f, 0.24f);
            vbRect.offsetMin = Vector2.zero;
            vbRect.offsetMax = Vector2.zero;
            var vbImg = vBtn.AddComponent<Image>();
            vbImg.color = new Color(0f, 0.7f, 0.35f);
            var vbButton = vBtn.AddComponent<Button>();
            var vbTxt = new GameObject("Txt").AddComponent<Text>();
            vbTxt.transform.SetParent(vBtn.transform, false);
            vbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vbTxt.text = "Tiếp Tục [M]";
            vbTxt.fontSize = 16;
            vbTxt.alignment = TextAnchor.MiddleCenter;
            vbTxt.color = Color.white;
            vbTxt.fontStyle = FontStyle.Bold;

            vicGO.SetActive(false);

            // 2. Defeat Dialog
            var defGO = new GameObject("DefeatDialog");
            defGO.transform.SetParent(canvasTransform, false);
            var dRect = defGO.AddComponent<RectTransform>();
            dRect.anchorMin = Vector2.zero;
            dRect.anchorMax = Vector2.one;
            dRect.offsetMin = Vector2.zero;
            dRect.offsetMax = Vector2.zero;

            var dCard = new GameObject("DialogCard");
            dCard.transform.SetParent(defGO.transform, false);
            var dcRect = dCard.AddComponent<RectTransform>();
            dcRect.anchorMin = new Vector2(0.32f, 0.3f);
            dcRect.anchorMax = new Vector2(0.68f, 0.7f);
            dcRect.offsetMin = Vector2.zero;
            dcRect.offsetMax = Vector2.zero;
            var dcImg = dCard.AddComponent<Image>();
            dcImg.color = new Color(0.14f, 0.04f, 0.04f, 0.98f);
            var dcOutline = dCard.AddComponent<Outline>();
            dcOutline.effectColor = new Color(1f, 0.25f, 0.25f, 0.95f);
            dcOutline.effectDistance = new Vector2(3f, 3f);

            var dTitle = new GameObject("TxtTitle");
            dTitle.transform.SetParent(dCard.transform, false);
            var dtRect = dTitle.AddComponent<RectTransform>();
            dtRect.anchorMin = new Vector2(0.05f, 0.72f);
            dtRect.anchorMax = new Vector2(0.95f, 0.95f);
            dtRect.offsetMin = Vector2.zero;
            dtRect.offsetMax = Vector2.zero;
            var dt = dTitle.AddComponent<Text>();
            dt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dt.text = "⚠️ GIAO HÀNG THẤT BẠI!";
            dt.fontSize = 24;
            dt.alignment = TextAnchor.MiddleCenter;
            dt.color = Color.red;
            dt.fontStyle = FontStyle.Bold;

            var dReason = new GameObject("TxtReason");
            dReason.transform.SetParent(dCard.transform, false);
            var drRect = dReason.AddComponent<RectTransform>();
            drRect.anchorMin = new Vector2(0.05f, 0.35f);
            drRect.anchorMax = new Vector2(0.95f, 0.68f);
            drRect.offsetMin = Vector2.zero;
            drRect.offsetMax = Vector2.zero;
            var dr = dReason.AddComponent<Text>();
            dr.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dr.text = "Lý do: Kiện hàng đã bị vỡ nát hoàn toàn!";
            dr.fontSize = 16;
            dr.alignment = TextAnchor.MiddleCenter;
            dr.color = Color.white;

            var dBtn = new GameObject("BtnRetry");
            dBtn.transform.SetParent(dCard.transform, false);
            var dbRect = dBtn.AddComponent<RectTransform>();
            dbRect.anchorMin = new Vector2(0.3f, 0.08f);
            dbRect.anchorMax = new Vector2(0.7f, 0.24f);
            dbRect.offsetMin = Vector2.zero;
            dbRect.offsetMax = Vector2.zero;
            var dbImg = dBtn.AddComponent<Image>();
            dbImg.color = new Color(0.85f, 0.25f, 0.25f);
            var dbButton = dBtn.AddComponent<Button>();
            var dbTxt = new GameObject("Txt").AddComponent<Text>();
            dbTxt.transform.SetParent(dBtn.transform, false);
            dbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dbTxt.text = "Thử Lại Ngay";
            dbTxt.fontSize = 16;
            dbTxt.alignment = TextAnchor.MiddleCenter;
            dbTxt.color = Color.white;
            dbTxt.fontStyle = FontStyle.Bold;

            defGO.SetActive(false);
        }

        private static Material GetOrCreateMaterial(string matName, Color color, float metallic = 0.1f, float smoothness = 0.5f, bool transparent = false)
        {
            if (!Directory.Exists(MatDir)) Directory.CreateDirectory(MatDir);
            string path = MatDir + matName + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                mat.color = color;
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);

                if (transparent)
                {
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = 3000;
                }

                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }
    }
}
