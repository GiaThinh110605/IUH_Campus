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
    /// Master Automated Builder for Mini-Game 2.1 "Grab Campus: Chuyến xe tâm linh"
    /// and the Unified Campus Adventure Menu Systems.
    /// Implements all 6 Core Screens from the design reference:
    /// 1. Màn hình chính: Bắt đầu, Cài đặt, Trang phục & Xe, Thoát.
    /// 2. Gameplay HUD: Góc nhìn thứ ba, Bản đồ nhỏ (Minimap), Thanh trạng thái (Profile Lv.3), Điểm giao hàng & Thời gian.
    /// 3. Giao hàng: Hộp thoại hoàn tất đơn hàng, hiển thị địa điểm và nút nhận thưởng.
    /// 4. Ma đuổi: Cảnh báo "⚠️ MA ĐANG ĐUỔI THEO!", thanh truy đuổi đỏ, hiệu ứng kinh dị sương mù & vignette.
    /// 5. Giao diện Mobile: Virtual touch joystick bên trái, các nút hành động (Xe máy, Tương tác, Tăng tốc) bên phải.
    /// 6. Màn hình chiến thắng: Điểm kinh nghiệm, tiền thưởng, sao đánh giá và manh mối nhận được.
    /// 7. Giao diện tùy biến Trang phục & Xe máy (XANH SM, Grab, be, Sinh Viên IUH).
    /// 8. Menu Mini-game [Phím M]: Phông chữ to, đẹp, dễ nhìn, bỏ hoàn toàn các chữ tên người.
    /// </summary>
    public static class IUHGrabCampusBuilder
    {
        private const string MatDir = "Assets/IUH_Campus/Materials/";

        [MenuItem("IUH Campus/Setup Grab Campus Minigame & All UI Screens (One-Click)")]
        public static void BuildGrabCampusAndMenu()
        {
            Debug.Log("[IUH Grab Campus Builder] Bắt đầu thiết lập toàn diện Mini-Game 2.1 và Hệ Thống Giao Diện...");

            // 1. Ensure Materials
            var grabGreenMat = GetOrCreateMaterial("M_IUH_Grab_Green", new Color(0.0f, 0.69f, 0.31f), 0.1f, 0.85f);
            var grabBoxMat = GetOrCreateMaterial("M_IUH_Grab_Box", new Color(0.02f, 0.55f, 0.25f), 0.05f, 0.6f);
            var ghostMat = GetOrCreateMaterial("M_IUH_Ghost_Spooky", new Color(0.25f, 0.95f, 1.0f, 0.75f), 0f, 0.9f, true, new Color(0.25f, 0.95f, 1.0f) * 3.5f);
            var waypointMat = GetOrCreateMaterial("M_IUH_Waypoint_Holo", new Color(0.0f, 0.85f, 1.0f, 0.65f), 0f, 0.9f, true, new Color(0.0f, 0.85f, 1.0f) * 3.5f);
            var dropoffMat = GetOrCreateMaterial("M_IUH_Waypoint_Dropoff", new Color(0.1f, 1.0f, 0.4f, 0.65f), 0f, 0.9f, true, new Color(0.1f, 1.0f, 0.4f) * 3.5f);

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

            // 5b. Build 3D Night Food Shops (Boba Tea, Fish Balls, Broken Rice)
            BuildNightStreetShops(gameplayRoot.transform);

            // 5c. Build Delivery Drop-off Receiving Stations (KTX, Block V, Block E)
            BuildDeliveryDropoffStations(gameplayRoot.transform);

            // 6. Build Navigation Arrow
            var navArrow = BuildNavigationArrow(gameplayRoot.transform);

            // 7. Add Customization Manager to root
            var customMgr = gameplayRoot.AddComponent<IUHCustomizationManager>();

            // 8. Build UI Canvas
            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO != null) UnityEngine.Object.DestroyImmediate(canvasGO);
            canvasGO = BuildCanvasWithUI(gameplayRoot.transform);

            // 9. Wire Up Grab Manager
            var grabMgr = gameplayRoot.AddComponent<IUHGrabCampusManager>();
            var portalUI = canvasGO.GetComponent<IUHMiniGamePortalUI>();

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
                so.FindProperty("m_TxtMissionTitle").objectReferenceValue = hudPanel.transform.Find("TopRightBanner/TxtTitle")?.GetComponent<Text>();
                so.FindProperty("m_TxtObjective").objectReferenceValue = hudPanel.transform.Find("TopRightBanner/TxtObjective")?.GetComponent<Text>();
                so.FindProperty("m_TxtTimer").objectReferenceValue = hudPanel.transform.Find("TopRightBanner/TxtTimer")?.GetComponent<Text>();
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
                so.FindProperty("m_TxtVictoryClue").objectReferenceValue = victoryDialog.transform.Find("DialogCard/TxtClue")?.GetComponent<Text>();
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
            Debug.Log("[IUH Grab Campus Builder] HOÀN TẤT XUẤT SẮC! Đã khởi tạo trọn bộ 6 giao diện chính chuẩn thiết kế.");
        }

        private static GameObject BuildGrabMotorbike(Transform parent, Material grabGreenMat, Material grabBoxMat)
        {
            var bikeGO = new GameObject("IUH_Grab_Motorbike");
            bikeGO.transform.SetParent(parent);
            bikeGO.transform.position = new Vector3(398.0f, 0.60f, -18.0f);
            bikeGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var cc = bikeGO.AddComponent<CharacterController>();
            cc.height = 1.25f;
            cc.radius = 0.38f;
            cc.center = new Vector3(0f, 0.68f, 0f);
            cc.stepOffset = 0.45f;
            cc.slopeLimit = 55f;

            var darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Motorbike_Dark.mat");
            var chromeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Int_Chrome.mat");
            var amberMat = GetOrCreateMaterial("M_IUH_Amber_TurnLight", new Color(1.0f, 0.65f, 0.05f), 0.1f, 0.8f);
            var redTailMat = GetOrCreateMaterial("M_IUH_Red_Taillight", new Color(1.0f, 0.1f, 0.1f), 0.1f, 0.9f);
            var glassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Glass_Reflective.mat");

            // 1. Visual Root
            var visualRoot = new GameObject("Visual_Root");
            visualRoot.transform.SetParent(bikeGO.transform, false);

            // Center Chassis Frame
            var chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "Chassis_Frame";
            chassis.transform.SetParent(visualRoot.transform, false);
            chassis.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            chassis.transform.localScale = new Vector3(0.24f, 0.32f, 1.25f);
            chassis.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(chassis.GetComponent<Collider>());

            // Main Body Fairing
            var bodyFairing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyFairing.name = "Body_Fairing";
            bodyFairing.transform.SetParent(visualRoot.transform, false);
            bodyFairing.transform.localPosition = new Vector3(0f, 0.58f, 0.08f);
            bodyFairing.transform.localScale = new Vector3(0.38f, 0.42f, 0.95f);
            bodyFairing.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(bodyFairing.GetComponent<Collider>());

            // Step-through footboard
            var footboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            footboard.name = "Footboard";
            footboard.transform.SetParent(visualRoot.transform, false);
            footboard.transform.localPosition = new Vector3(0f, 0.28f, 0.15f);
            footboard.transform.localScale = new Vector3(0.48f, 0.08f, 0.45f);
            footboard.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(footboard.GetComponent<Collider>());

            // Chrome Engine Block
            var engine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            engine.name = "Engine_Block";
            engine.transform.SetParent(visualRoot.transform, false);
            engine.transform.localPosition = new Vector3(0f, 0.32f, -0.22f);
            engine.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            engine.transform.localScale = new Vector3(0.28f, 0.15f, 0.28f);
            engine.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;
            UnityEngine.Object.DestroyImmediate(engine.GetComponent<Collider>());

            // Exhaust Pipe
            var exhaust = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            exhaust.name = "Exhaust_Pipe";
            exhaust.transform.SetParent(visualRoot.transform, false);
            exhaust.transform.localPosition = new Vector3(0.23f, 0.26f, -0.42f);
            exhaust.transform.localRotation = Quaternion.Euler(98f, 0f, 0f);
            exhaust.transform.localScale = new Vector3(0.09f, 0.35f, 0.09f);
            exhaust.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;
            UnityEngine.Object.DestroyImmediate(exhaust.GetComponent<Collider>());

            // Contoured Leather Dual-Seat
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Contoured_Seat";
            seat.transform.SetParent(visualRoot.transform, false);
            seat.transform.localPosition = new Vector3(0f, 0.72f, -0.15f);
            seat.transform.localScale = new Vector3(0.34f, 0.14f, 0.85f);
            seat.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(seat.GetComponent<Collider>());

            // Rear Delivery Box Rack
            var rearRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearRack.name = "Rear_Cargo_Rack";
            rearRack.transform.SetParent(visualRoot.transform, false);
            rearRack.transform.localPosition = new Vector3(0f, 0.70f, -0.72f);
            rearRack.transform.localScale = new Vector3(0.36f, 0.05f, 0.38f);
            rearRack.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;
            UnityEngine.Object.DestroyImmediate(rearRack.GetComponent<Collider>());

            // Grab Thermal Delivery Box
            var deliveryBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deliveryBox.name = "Delivery_Box";
            deliveryBox.transform.SetParent(chassis.transform, false);
            deliveryBox.transform.localPosition = new Vector3(0f, 1.25f, -0.68f);
            deliveryBox.transform.localScale = new Vector3(2.1f, 1.6f, 1.6f);
            deliveryBox.GetComponent<MeshRenderer>().sharedMaterial = grabBoxMat;
            UnityEngine.Object.DestroyImmediate(deliveryBox.GetComponent<Collider>());

            // Reflective Strip
            var boxReflect = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxReflect.name = "Reflective_Strip";
            boxReflect.transform.SetParent(deliveryBox.transform, false);
            boxReflect.transform.localPosition = new Vector3(0f, 0f, -0.52f);
            boxReflect.transform.localScale = new Vector3(0.92f, 0.18f, 0.04f);
            boxReflect.GetComponent<MeshRenderer>().sharedMaterial = amberMat;
            UnityEngine.Object.DestroyImmediate(boxReflect.GetComponent<Collider>());

            // Red Taillight
            var taillight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            taillight.name = "Rear_Taillight";
            taillight.transform.SetParent(visualRoot.transform, false);
            taillight.transform.localPosition = new Vector3(0f, 0.58f, -0.85f);
            taillight.transform.localScale = new Vector3(0.24f, 0.08f, 0.05f);
            taillight.GetComponent<MeshRenderer>().sharedMaterial = redTailMat;
            UnityEngine.Object.DestroyImmediate(taillight.GetComponent<Collider>());

            // 2. Steer Fork
            var steerForkGO = new GameObject("Steer_Fork");
            steerForkGO.transform.SetParent(visualRoot.transform, false);
            steerForkGO.transform.localPosition = new Vector3(0f, 0.42f, 0.62f);

            var forkL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            forkL.name = "Fork_Shock_L";
            forkL.transform.SetParent(steerForkGO.transform, false);
            forkL.transform.localPosition = new Vector3(-0.14f, -0.1f, 0.02f);
            forkL.transform.localScale = new Vector3(0.045f, 0.32f, 0.045f);
            forkL.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;
            UnityEngine.Object.DestroyImmediate(forkL.GetComponent<Collider>());

            var forkR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            forkR.name = "Fork_Shock_R";
            forkR.transform.SetParent(steerForkGO.transform, false);
            forkR.transform.localPosition = new Vector3(0.14f, -0.1f, 0.02f);
            forkR.transform.localScale = new Vector3(0.045f, 0.32f, 0.045f);
            forkR.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;
            UnityEngine.Object.DestroyImmediate(forkR.GetComponent<Collider>());

            // Front Mask
            var frontMask = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontMask.name = "Front_Mask";
            frontMask.transform.SetParent(steerForkGO.transform, false);
            frontMask.transform.localPosition = new Vector3(0f, 0.34f, 0.08f);
            frontMask.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            frontMask.transform.localScale = new Vector3(0.36f, 0.38f, 0.22f);
            frontMask.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(frontMask.GetComponent<Collider>());

            // Curved Mudguard
            var frontFender = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontFender.name = "Front_Fender";
            frontFender.transform.SetParent(steerForkGO.transform, false);
            frontFender.transform.localPosition = new Vector3(0f, 0.05f, 0.05f);
            frontFender.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            frontFender.transform.localScale = new Vector3(0.24f, 0.07f, 0.42f);
            frontFender.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(frontFender.GetComponent<Collider>());

            // Headlight Crystal Lens
            var headlightLens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            headlightLens.name = "Headlight_Lens";
            headlightLens.transform.SetParent(frontMask.transform, false);
            headlightLens.transform.localPosition = new Vector3(0f, 0.05f, 0.48f);
            headlightLens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            headlightLens.transform.localScale = new Vector3(0.55f, 0.12f, 0.45f);
            headlightLens.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
            UnityEngine.Object.DestroyImmediate(headlightLens.GetComponent<Collider>());

            // Amber Turn Signals
            var signalL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            signalL.name = "Signal_L";
            signalL.transform.SetParent(frontMask.transform, false);
            signalL.transform.localPosition = new Vector3(-0.48f, 0.05f, 0.25f);
            signalL.transform.localScale = new Vector3(0.18f, 0.14f, 0.14f);
            signalL.GetComponent<MeshRenderer>().sharedMaterial = amberMat;
            UnityEngine.Object.DestroyImmediate(signalL.GetComponent<Collider>());

            var signalR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            signalR.name = "Signal_R";
            signalR.transform.SetParent(frontMask.transform, false);
            signalR.transform.localPosition = new Vector3(0.48f, 0.05f, 0.25f);
            signalR.transform.localScale = new Vector3(0.18f, 0.14f, 0.14f);
            signalR.GetComponent<MeshRenderer>().sharedMaterial = amberMat;
            UnityEngine.Object.DestroyImmediate(signalR.GetComponent<Collider>());

            // Handlebars
            var handlebar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handlebar.name = "Handlebar_Bar";
            handlebar.transform.SetParent(steerForkGO.transform, false);
            handlebar.transform.localPosition = new Vector3(0f, 0.52f, 0.02f);
            handlebar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            handlebar.transform.localScale = new Vector3(0.045f, 0.40f, 0.045f);
            handlebar.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;
            UnityEngine.Object.DestroyImmediate(handlebar.GetComponent<Collider>());

            // Left Grip
            var gripL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gripL.name = "Grip_L";
            gripL.transform.SetParent(handlebar.transform, false);
            gripL.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            gripL.transform.localScale = new Vector3(1.4f, 0.16f, 1.4f);
            gripL.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(gripL.GetComponent<Collider>());

            // Right Grip
            var gripR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gripR.name = "Grip_R";
            gripR.transform.SetParent(handlebar.transform, false);
            gripR.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            gripR.transform.localScale = new Vector3(1.4f, 0.16f, 1.4f);
            gripR.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(gripR.GetComponent<Collider>());

            // Dual Rearview Mirrors
            var mirrorL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mirrorL.name = "Mirror_L";
            mirrorL.transform.SetParent(steerForkGO.transform, false);
            mirrorL.transform.localPosition = new Vector3(-0.35f, 0.72f, 0.04f);
            mirrorL.transform.localScale = new Vector3(0.12f, 0.08f, 0.02f);
            mirrorL.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(mirrorL.GetComponent<Collider>());

            var mirrorR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mirrorR.name = "Mirror_R";
            mirrorR.transform.SetParent(steerForkGO.transform, false);
            mirrorR.transform.localPosition = new Vector3(0.35f, 0.72f, 0.04f);
            mirrorR.transform.localScale = new Vector3(0.12f, 0.08f, 0.02f);
            mirrorR.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(mirrorR.GetComponent<Collider>());

            // Digital Dashboard
            var dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dash.name = "Digital_Dashboard";
            dash.transform.SetParent(steerForkGO.transform, false);
            dash.transform.localPosition = new Vector3(0f, 0.49f, -0.05f);
            dash.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
            dash.transform.localScale = new Vector3(0.18f, 0.04f, 0.12f);
            dash.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(dash.GetComponent<Collider>());

            // 3. Spoke Wheels with Chrome Rims
            var wheelFront = BuildDetailedWheel("Wheel_Front", steerForkGO.transform, new Vector3(0f, -0.28f, 0.04f), darkMat, chromeMat);
            var wheelRear = BuildDetailedWheel("Wheel_Rear", visualRoot.transform, new Vector3(0f, 0.14f, -0.58f), darkMat, chromeMat);

            // 4. Headlight Spotlight
            var lightGO = new GameObject("Headlight_Spotlight");
            lightGO.transform.SetParent(steerForkGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0.35f, 0.25f);
            lightGO.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
            var spot = lightGO.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 45f;
            spot.spotAngle = 65f;
            spot.color = new Color(1.0f, 0.98f, 0.88f);
            spot.intensity = 2.4f;
            spot.enabled = true;

            // 5. Rider Figure (Attached to visualRoot so rider leans dynamically with the motorbike!)
            var riderGO = BuildRiderFigure(visualRoot.transform, grabGreenMat, grabBoxMat);

            // 6. Camera Target (Height 1.20, slightly forward for clear view over driver's shoulder)
            var camTarget = new GameObject("CameraTarget");
            camTarget.transform.SetParent(bikeGO.transform, false);
            camTarget.transform.localPosition = new Vector3(0f, 1.20f, 0.20f);

            // 7. Attach Component
            var motorbikeScript = bikeGO.AddComponent<IUHGrabMotorbike>();
            SerializedObject so = new SerializedObject(motorbikeScript);
            so.FindProperty("m_VisualRoot").objectReferenceValue = visualRoot.transform;
            so.FindProperty("m_FrontSteerFork").objectReferenceValue = steerForkGO.transform;
            so.FindProperty("m_WheelFront").objectReferenceValue = wheelFront.transform;
            so.FindProperty("m_WheelRear").objectReferenceValue = wheelRear.transform;
            so.FindProperty("m_RiderAvatar").objectReferenceValue = riderGO;
            so.FindProperty("m_Headlight").objectReferenceValue = spot;
            so.FindProperty("m_CameraTarget").objectReferenceValue = camTarget.transform;
            so.ApplyModifiedProperties();

            return bikeGO;
        }

        private static GameObject BuildDetailedWheel(string name, Transform parent, Vector3 localPos, Material tireMat, Material rimMat)
        {
            var wheelRoot = new GameObject(name);
            wheelRoot.transform.SetParent(parent, false);
            wheelRoot.transform.localPosition = localPos;

            var tire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tire.name = "Tire";
            tire.transform.SetParent(wheelRoot.transform, false);
            tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            tire.transform.localScale = new Vector3(0.58f, 0.085f, 0.58f);
            tire.GetComponent<MeshRenderer>().sharedMaterial = tireMat;
            UnityEngine.Object.DestroyImmediate(tire.GetComponent<Collider>());

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Chrome_Rim";
            rim.transform.SetParent(tire.transform, false);
            rim.transform.localPosition = Vector3.zero;
            rim.transform.localScale = new Vector3(0.70f, 1.05f, 0.70f);
            rim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;
            UnityEngine.Object.DestroyImmediate(rim.GetComponent<Collider>());

            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "Center_Hub";
            hub.transform.SetParent(tire.transform, false);
            hub.transform.localPosition = Vector3.zero;
            hub.transform.localScale = new Vector3(0.24f, 1.25f, 0.24f);
            hub.GetComponent<MeshRenderer>().sharedMaterial = rimMat;
            UnityEngine.Object.DestroyImmediate(hub.GetComponent<Collider>());

            return wheelRoot;
        }

        private static GameObject BuildRiderFigure(Transform parent, Material grabGreenMat, Material grabBoxMat)
        {
            var darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Motorbike_Dark.mat");

            var riderGO = new GameObject("Rider_Figure");
            riderGO.transform.SetParent(parent, false);
            riderGO.transform.localPosition = Vector3.zero;
            riderGO.transform.localRotation = Quaternion.identity;

            // 1. Torso / Uniform Jacket (Angled forward slightly in natural riding posture)
            var torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Rider_Jacket";
            torso.transform.SetParent(riderGO.transform, false);
            torso.transform.localPosition = new Vector3(0f, 0.96f, -0.12f);
            torso.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            torso.transform.localScale = new Vector3(0.38f, 0.46f, 0.26f);
            torso.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(torso.GetComponent<Collider>());

            // Jacket Brand Stripe
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Jacket_Stripe";
            stripe.transform.SetParent(torso.transform, false);
            stripe.transform.localPosition = new Vector3(0f, 0f, 0.52f);
            stripe.transform.localScale = new Vector3(0.95f, 0.22f, 0.04f);
            stripe.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(stripe.GetComponent<Collider>());

            // 2. Neck
            var neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Rider_Neck";
            neck.transform.SetParent(riderGO.transform, false);
            neck.transform.localPosition = new Vector3(0f, 1.20f, -0.06f);
            neck.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            neck.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
            neck.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(neck.GetComponent<Collider>());

            // 3. Helmet & Visor
            var helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helmet.name = "Rider_Helmet";
            helmet.transform.SetParent(riderGO.transform, false);
            helmet.transform.localPosition = new Vector3(0f, 1.34f, -0.02f);
            helmet.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            helmet.transform.localScale = new Vector3(0.34f, 0.36f, 0.36f);
            helmet.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(helmet.GetComponent<Collider>());

            var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Helmet_Visor";
            visor.transform.SetParent(helmet.transform, false);
            visor.transform.localPosition = new Vector3(0f, -0.02f, 0.36f);
            visor.transform.localScale = new Vector3(0.72f, 0.32f, 0.28f);
            visor.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(visor.GetComponent<Collider>());

            // 4. Courier Backpack
            var backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backpack.name = "Rider_Backpack";
            backpack.transform.SetParent(torso.transform, false);
            backpack.transform.localPosition = new Vector3(0f, 0.05f, -0.72f);
            backpack.transform.localRotation = Quaternion.identity;
            backpack.transform.localScale = new Vector3(0.92f, 0.88f, 0.65f);
            backpack.GetComponent<MeshRenderer>().sharedMaterial = grabBoxMat;
            UnityEngine.Object.DestroyImmediate(backpack.GetComponent<Collider>());

            // 5. Arms & Hands holding Handlebars
            var leftArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftArm.name = "Rider_Arm_L";
            leftArm.transform.SetParent(riderGO.transform, false);
            leftArm.transform.localPosition = new Vector3(-0.25f, 0.88f, 0.18f);
            leftArm.transform.localRotation = Quaternion.Euler(46f, -14f, 8f);
            leftArm.transform.localScale = new Vector3(0.09f, 0.24f, 0.09f);
            leftArm.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(leftArm.GetComponent<Collider>());

            var leftGlove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftGlove.name = "Rider_Glove_L";
            leftGlove.transform.SetParent(riderGO.transform, false);
            leftGlove.transform.localPosition = new Vector3(-0.30f, 0.70f, 0.40f);
            leftGlove.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            leftGlove.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(leftGlove.GetComponent<Collider>());

            var rightArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightArm.name = "Rider_Arm_R";
            rightArm.transform.SetParent(riderGO.transform, false);
            rightArm.transform.localPosition = new Vector3(0.25f, 0.88f, 0.18f);
            rightArm.transform.localRotation = Quaternion.Euler(46f, 14f, -8f);
            rightArm.transform.localScale = new Vector3(0.09f, 0.24f, 0.09f);
            rightArm.GetComponent<MeshRenderer>().sharedMaterial = grabGreenMat;
            UnityEngine.Object.DestroyImmediate(rightArm.GetComponent<Collider>());

            var rightGlove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightGlove.name = "Rider_Glove_R";
            rightGlove.transform.SetParent(riderGO.transform, false);
            rightGlove.transform.localPosition = new Vector3(0.30f, 0.70f, 0.40f);
            rightGlove.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            rightGlove.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(rightGlove.GetComponent<Collider>());

            // 6. Seated Legs & Boots
            var leftThigh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftThigh.name = "Rider_Leg_L";
            leftThigh.transform.SetParent(riderGO.transform, false);
            leftThigh.transform.localPosition = new Vector3(-0.16f, 0.54f, 0.02f);
            leftThigh.transform.localRotation = Quaternion.Euler(62f, -12f, 0f);
            leftThigh.transform.localScale = new Vector3(0.12f, 0.22f, 0.12f);
            leftThigh.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(leftThigh.GetComponent<Collider>());

            var leftCalf = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftCalf.name = "Rider_Calf_L";
            leftCalf.transform.SetParent(riderGO.transform, false);
            leftCalf.transform.localPosition = new Vector3(-0.18f, 0.32f, 0.18f);
            leftCalf.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            leftCalf.transform.localScale = new Vector3(0.11f, 0.18f, 0.11f);
            leftCalf.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(leftCalf.GetComponent<Collider>());

            var leftBoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftBoot.name = "Rider_Boot_L";
            leftBoot.transform.SetParent(riderGO.transform, false);
            leftBoot.transform.localPosition = new Vector3(-0.18f, 0.20f, 0.22f);
            leftBoot.transform.localScale = new Vector3(0.12f, 0.08f, 0.20f);
            leftBoot.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(leftBoot.GetComponent<Collider>());

            var rightThigh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightThigh.name = "Rider_Leg_R";
            rightThigh.transform.SetParent(riderGO.transform, false);
            rightThigh.transform.localPosition = new Vector3(0.16f, 0.54f, 0.02f);
            rightThigh.transform.localRotation = Quaternion.Euler(62f, 12f, 0f);
            rightThigh.transform.localScale = new Vector3(0.12f, 0.22f, 0.12f);
            rightThigh.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(rightThigh.GetComponent<Collider>());

            var rightCalf = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightCalf.name = "Rider_Calf_R";
            rightCalf.transform.SetParent(riderGO.transform, false);
            rightCalf.transform.localPosition = new Vector3(0.18f, 0.32f, 0.18f);
            rightCalf.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            rightCalf.transform.localScale = new Vector3(0.11f, 0.18f, 0.11f);
            rightCalf.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(rightCalf.GetComponent<Collider>());

            var rightBoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightBoot.name = "Rider_Boot_R";
            rightBoot.transform.SetParent(riderGO.transform, false);
            rightBoot.transform.localPosition = new Vector3(0.18f, 0.20f, 0.22f);
            rightBoot.transform.localScale = new Vector3(0.12f, 0.08f, 0.20f);
            rightBoot.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            UnityEngine.Object.DestroyImmediate(rightBoot.GetComponent<Collider>());

            return riderGO;
        }

        private static List<GameObject> BuildGhostHazards(Transform parent, Material ghostMat)
        {
            var ghostsRoot = new GameObject("GHOST_HAZARDS");
            ghostsRoot.transform.SetParent(parent, false);

            var ghostList = new List<GameObject>();

            var eyeMat = GetOrCreateMaterial("M_IUH_Ghost_Eyes", Color.red, 0f, 1.0f, false, Color.red * 5.0f);
            var hairMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Motorbike_Dark.mat");

            // Strategic positions along the delivery route from Gate 1 to Canteen to KTX
            Vector3[] ghostPositions = new Vector3[]
            {
                new Vector3(398.0f, 1.6f, -32.0f), // Ghost 1: Right in front of Gate 1 start on boulevard!
                new Vector3(380.0f, 1.6f, -48.0f), // Ghost 2: Near Canteen corner
                new Vector3(365.0f, 1.6f, -62.0f), // Ghost 3: Boulevard near Block E
                new Vector3(345.0f, 1.6f, -74.0f), // Ghost 4: Boulevard near Library turn
                new Vector3(328.0f, 1.6f, -82.0f), // Ghost 5: Haunting KTX dropoff gate
                new Vector3(402.0f, 1.6f, -22.0f)  // Ghost 6: Gate 1 patrol
            };

            for (int i = 0; i < ghostPositions.Length; i++)
            {
                var ghostGO = new GameObject($"Ghost_Hazard_0{i + 1}");
                ghostGO.transform.SetParent(ghostsRoot.transform);
                ghostGO.transform.position = ghostPositions[i];

                // 1. Ghost Body / Flowing Shrouded Robe
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "GhostBody";
                body.transform.SetParent(ghostGO.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                body.transform.localScale = new Vector3(0.65f, 0.95f, 0.65f);
                body.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;
                UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

                var skirt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                skirt.name = "GhostSkirt";
                skirt.transform.SetParent(ghostGO.transform, false);
                skirt.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                skirt.transform.localScale = new Vector3(0.85f, 0.35f, 0.85f);
                skirt.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;
                UnityEngine.Object.DestroyImmediate(skirt.GetComponent<Collider>());

                // 2. Head
                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "GhostHead";
                head.transform.SetParent(ghostGO.transform, false);
                head.transform.localPosition = new Vector3(0f, 1.55f, 0f);
                head.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
                head.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;
                UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

                // 3. Streaming Black Hair (Vietnamese horror classic)
                var hairTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hairTop.name = "GhostHairTop";
                hairTop.transform.SetParent(head.transform, false);
                hairTop.transform.localPosition = new Vector3(0f, 0.15f, -0.05f);
                hairTop.transform.localScale = new Vector3(1.08f, 0.65f, 1.15f);
                hairTop.GetComponent<MeshRenderer>().sharedMaterial = hairMat;
                UnityEngine.Object.DestroyImmediate(hairTop.GetComponent<Collider>());

                var hairStrandL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                hairStrandL.name = "HairStrandL";
                hairStrandL.transform.SetParent(head.transform, false);
                hairStrandL.transform.localPosition = new Vector3(-0.45f, -0.45f, 0.15f);
                hairStrandL.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                hairStrandL.transform.localScale = new Vector3(0.22f, 0.65f, 0.22f);
                hairStrandL.GetComponent<MeshRenderer>().sharedMaterial = hairMat;
                UnityEngine.Object.DestroyImmediate(hairStrandL.GetComponent<Collider>());

                var hairStrandR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                hairStrandR.name = "HairStrandR";
                hairStrandR.transform.SetParent(head.transform, false);
                hairStrandR.transform.localPosition = new Vector3(0.45f, -0.45f, 0.15f);
                hairStrandR.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                hairStrandR.transform.localScale = new Vector3(0.22f, 0.65f, 0.22f);
                hairStrandR.GetComponent<MeshRenderer>().sharedMaterial = hairMat;
                UnityEngine.Object.DestroyImmediate(hairStrandR.GetComponent<Collider>());

                // 4. Glowing Red Demonic Eyes
                var eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeL.name = "Eye_L";
                eyeL.transform.SetParent(ghostGO.transform, false);
                eyeL.transform.localPosition = new Vector3(-0.14f, 1.58f, 0.24f);
                eyeL.transform.localScale = new Vector3(0.10f, 0.10f, 0.10f);
                eyeL.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;
                UnityEngine.Object.DestroyImmediate(eyeL.GetComponent<Collider>());

                var eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeR.name = "Eye_R";
                eyeR.transform.SetParent(ghostGO.transform, false);
                eyeR.transform.localPosition = new Vector3(0.14f, 1.58f, 0.24f);
                eyeR.transform.localScale = new Vector3(0.10f, 0.10f, 0.10f);
                eyeR.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;
                UnityEngine.Object.DestroyImmediate(eyeR.GetComponent<Collider>());

                // 5. Screaming Dark Mouth
                var mouth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mouth.name = "GhostMouth";
                mouth.transform.SetParent(ghostGO.transform, false);
                mouth.transform.localPosition = new Vector3(0f, 1.42f, 0.25f);
                mouth.transform.localScale = new Vector3(0.12f, 0.16f, 0.08f);
                mouth.GetComponent<MeshRenderer>().sharedMaterial = hairMat;
                UnityEngine.Object.DestroyImmediate(mouth.GetComponent<Collider>());

                // 6. Outstretched Reaching Claws / Arms
                var armL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                armL.name = "GhostArmL";
                armL.transform.SetParent(ghostGO.transform, false);
                armL.transform.localPosition = new Vector3(-0.48f, 1.15f, 0.45f);
                armL.transform.localRotation = Quaternion.Euler(75f, -22f, 0f);
                armL.transform.localScale = new Vector3(0.09f, 0.45f, 0.09f);
                armL.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;
                UnityEngine.Object.DestroyImmediate(armL.GetComponent<Collider>());

                var armR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                armR.name = "GhostArmR";
                armR.transform.SetParent(ghostGO.transform, false);
                armR.transform.localPosition = new Vector3(0.48f, 1.15f, 0.45f);
                armR.transform.localRotation = Quaternion.Euler(75f, 22f, 0f);
                armR.transform.localScale = new Vector3(0.09f, 0.45f, 0.09f);
                armR.GetComponent<MeshRenderer>().sharedMaterial = ghostMat;
                UnityEngine.Object.DestroyImmediate(armR.GetComponent<Collider>());

                // 7. Aura Point Light
                var auraLightGO = new GameObject("AuraLight");
                auraLightGO.transform.SetParent(ghostGO.transform, false);
                auraLightGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                var pl = auraLightGO.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(0.25f, 0.95f, 1.0f);
                pl.range = 14f;
                pl.intensity = 3.5f;

                // 8. Trigger Collider
                var col = ghostGO.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 1.8f;
                col.center = new Vector3(0f, 1.0f, 0f);

                var hazardScript = ghostGO.AddComponent<IUHGhostHazard>();
                ghostGO.SetActive(false);
                ghostList.Add(ghostGO);
            }

            return ghostList;
        }

        private static (IUHDeliveryWaypoint, IUHDeliveryWaypoint) BuildDeliveryWaypoints(Transform parent, Material pickupMat, Material dropoffMat)
        {
            var waypointsRoot = new GameObject("DELIVERY_WAYPOINTS");
            waypointsRoot.transform.SetParent(parent, false);

            var pickupGO = BuildHolographicPillar("PickupWaypoint", waypointsRoot.transform, new Vector3(382.0f, 0.48f, -34.5f), pickupMat, new Color(0f, 0.85f, 1f), "📦 ĐIỂM LẤY ĐỒ\n[ TIỆM TRÀ SỮA TOCOTOCO ]");
            var pickupWp = pickupGO.AddComponent<IUHDeliveryWaypoint>();
            pickupWp.Setup(IUHDeliveryWaypoint.WaypointType.Pickup, new Color(0f, 0.85f, 1f), "📦 ĐIỂM LẤY ĐỒ\n[ TIỆM TRÀ SỮA TOCOTOCO ]");

            var dropoffGO = BuildHolographicPillar("DropoffWaypoint", waypointsRoot.transform, new Vector3(336.0f, 0.48f, -78.0f), dropoffMat, new Color(0.1f, 1f, 0.4f), "🏁 MỤC TIÊU GIAO HÀNG\n[ KÝ TÚC XÁ KTX ]");
            var dropoffWp = dropoffGO.AddComponent<IUHDeliveryWaypoint>();
            dropoffWp.Setup(IUHDeliveryWaypoint.WaypointType.Dropoff, new Color(0.1f, 1f, 0.4f), "🏁 MỤC TIÊU GIAO HÀNG\n[ KÝ TÚC XÁ KTX ]");

            pickupGO.SetActive(false);
            dropoffGO.SetActive(false);

            return (pickupWp, dropoffWp);
        }

        private static GameObject BuildHolographicPillar(string name, Transform parent, Vector3 pos, Material mat, Color lightCol, string labelText)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            // 1. Giant Holographic Sky Beam (70m tall, shoots straight up into the night sky!)
            var skyBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            skyBeam.name = "SkyLightBeam";
            skyBeam.transform.SetParent(go.transform, false);
            skyBeam.transform.localPosition = new Vector3(0f, 35.0f, 0f);
            skyBeam.transform.localScale = new Vector3(2.4f, 35.0f, 2.4f);
            skyBeam.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var colBeam = skyBeam.GetComponent<Collider>();
            if (colBeam != null) UnityEngine.Object.DestroyImmediate(colBeam);

            // 2. Base Concentric Rings
            var ringOuter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringOuter.name = "RingOuter";
            ringOuter.transform.SetParent(go.transform, false);
            ringOuter.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            ringOuter.transform.localScale = new Vector3(7.0f, 0.04f, 7.0f);
            ringOuter.GetComponent<MeshRenderer>().sharedMaterial = mat;
            UnityEngine.Object.DestroyImmediate(ringOuter.GetComponent<Collider>());

            var ringInner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringInner.name = "RingInner";
            ringInner.transform.SetParent(go.transform, false);
            ringInner.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            ringInner.transform.localScale = new Vector3(4.2f, 0.05f, 4.2f);
            ringInner.GetComponent<MeshRenderer>().sharedMaterial = mat;
            UnityEngine.Object.DestroyImmediate(ringInner.GetComponent<Collider>());

            // 3. Floating 3D Diamond / Pin Marker
            var diamond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            diamond.name = "DiamondMarker";
            diamond.transform.SetParent(go.transform, false);
            diamond.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            diamond.transform.localRotation = Quaternion.Euler(45f, 45f, 45f);
            diamond.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            diamond.GetComponent<MeshRenderer>().sharedMaterial = mat;
            UnityEngine.Object.DestroyImmediate(diamond.GetComponent<Collider>());

            // 4. In-World Billboard Canvas with Text
            var bbGO = new GameObject("BillboardCanvas");
            bbGO.transform.SetParent(go.transform, false);
            bbGO.transform.localPosition = new Vector3(0f, 6.8f, 0f);
            var bbCanvas = bbGO.AddComponent<Canvas>();
            bbCanvas.renderMode = RenderMode.WorldSpace;
            var bbRect = bbGO.GetComponent<RectTransform>();
            bbRect.sizeDelta = new Vector2(400f, 100f);
            bbRect.localScale = new Vector3(0.015f, 0.015f, 0.015f);

            var bbTxtGO = new GameObject("Txt");
            bbTxtGO.transform.SetParent(bbGO.transform, false);
            var bbtRect = bbTxtGO.AddComponent<RectTransform>();
            bbtRect.anchorMin = Vector2.zero;
            bbtRect.anchorMax = Vector2.one;
            bbtRect.offsetMin = Vector2.zero;
            bbtRect.offsetMax = Vector2.zero;
            var bbt = bbTxtGO.AddComponent<Text>();
            bbt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bbt.text = labelText;
            bbt.fontSize = 28;
            bbt.alignment = TextAnchor.MiddleCenter;
            bbt.color = Color.white;
            bbt.fontStyle = FontStyle.Bold;

            var bbtOutline = bbTxtGO.AddComponent<Outline>();
            bbtOutline.effectColor = Color.black;
            bbtOutline.effectDistance = new Vector2(2f, 2f);

            // 5. Point Light Beacon
            var lightGO = new GameObject("BeaconLight");
            lightGO.transform.SetParent(go.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 3.0f, 0f);
            var pl = lightGO.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = lightCol;
            pl.range = 28f;
            pl.intensity = 6.0f;

            // 6. Trigger Sphere Collider
            var trigCol = go.AddComponent<SphereCollider>();
            trigCol.isTrigger = true;
            trigCol.radius = 3.6f;
            trigCol.center = new Vector3(0f, 1.0f, 0f);

            return go;
        }

        private static GameObject BuildNavigationArrow(Transform parent)
        {
            var arrowGO = new GameObject("IUH_Nav_Arrow");
            arrowGO.transform.SetParent(parent);

            var cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = "ArrowMesh";
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

        private static void BuildNightStreetShops(Transform parent)
        {
            var shopsRoot = new GameObject("NIGHT_FOOD_SHOPS");
            shopsRoot.transform.SetParent(parent, false);

            var woodMat = GetOrCreateMaterial("M_IUH_Shop_Wood", new Color(0.32f, 0.18f, 0.10f), 0.05f, 0.4f);
            var warmLightMat = GetOrCreateMaterial("M_IUH_Shop_WarmLight", new Color(1.0f, 0.88f, 0.50f), 0f, 0.9f, false, new Color(1.0f, 0.85f, 0.45f) * 3.5f);
            var cyanAwningMat = GetOrCreateMaterial("M_IUH_Shop_CyanAwning", new Color(0.0f, 0.75f, 0.85f), 0.1f, 0.6f);
            var redLanternMat = GetOrCreateMaterial("M_IUH_Shop_RedLantern", new Color(1.0f, 0.20f, 0.12f), 0f, 0.8f, false, new Color(1.0f, 0.20f, 0.12f) * 3.5f);
            var yellowAwningMat = GetOrCreateMaterial("M_IUH_Shop_YellowAwning", new Color(0.95f, 0.75f, 0.05f), 0.1f, 0.6f);
            var metalMat = GetOrCreateMaterial("M_IUH_Shop_Metal", new Color(0.72f, 0.75f, 0.80f), 0.85f, 0.8f);
            var darkRoofMat = GetOrCreateMaterial("M_IUH_Shop_DarkRoof", new Color(0.15f, 0.18f, 0.22f), 0.1f, 0.3f);

            // 1. Tiệm Trà Sữa ToCoToCo IUH
            BuildBobaTeaShop(shopsRoot.transform, new Vector3(382.0f, 0.48f, -36.0f), woodMat, cyanAwningMat, warmLightMat);

            // 2. Xe Cá Viên Chiên 79 & Ăn Vặt Đêm Sinh Viên
            BuildFishBallCart(shopsRoot.transform, new Vector3(368.0f, 0.48f, -54.0f), metalMat, yellowAwningMat, redLanternMat, warmLightMat);

            // 3. Quán Cơm Tấm Đêm Sài Gòn
            BuildBrokenRiceStall(shopsRoot.transform, new Vector3(395.0f, 0.48f, -22.0f), woodMat, darkRoofMat, warmLightMat);
        }

        private static void BuildBobaTeaShop(Transform parent, Vector3 pos, Material woodMat, Material cyanAwningMat, Material warmLightMat)
        {
            var shopGO = new GameObject("Shop_01_BobaTea_ToCoToCo");
            shopGO.transform.SetParent(parent, false);
            shopGO.transform.position = pos;
            shopGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // Counter
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter";
            counter.transform.SetParent(shopGO.transform, false);
            counter.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            counter.transform.localScale = new Vector3(3.2f, 1.1f, 1.2f);
            counter.GetComponent<MeshRenderer>().sharedMaterial = woodMat;

            // Counter Top Trim
            var cTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cTop.name = "CounterTop";
            cTop.transform.SetParent(shopGO.transform, false);
            cTop.transform.localPosition = new Vector3(0f, 1.12f, 0f);
            cTop.transform.localScale = new Vector3(3.4f, 0.08f, 1.4f);
            cTop.GetComponent<MeshRenderer>().sharedMaterial = cyanAwningMat;
            UnityEngine.Object.DestroyImmediate(cTop.GetComponent<Collider>());

            // 4 Corner Pillars
            Vector3[] pOffsets = new Vector3[] {
                new Vector3(-1.5f, 1.6f, -0.55f),
                new Vector3(1.5f, 1.6f, -0.55f),
                new Vector3(-1.5f, 1.6f, 0.55f),
                new Vector3(1.5f, 1.6f, 0.55f)
            };
            foreach (var po in pOffsets)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "Pillar";
                pillar.transform.SetParent(shopGO.transform, false);
                pillar.transform.localPosition = po;
                pillar.transform.localScale = new Vector3(0.09f, 1.0f, 0.09f);
                pillar.GetComponent<MeshRenderer>().sharedMaterial = woodMat;
                UnityEngine.Object.DestroyImmediate(pillar.GetComponent<Collider>());
            }

            // Awning Roof
            var awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            awning.name = "AwningRoof";
            awning.transform.SetParent(shopGO.transform, false);
            awning.transform.localPosition = new Vector3(0f, 2.65f, 0.1f);
            awning.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            awning.transform.localScale = new Vector3(3.6f, 0.12f, 1.8f);
            awning.GetComponent<MeshRenderer>().sharedMaterial = cyanAwningMat;
            UnityEngine.Object.DestroyImmediate(awning.GetComponent<Collider>());

            // Hanging Fairy Lanterns (Chong đèn đêm)
            for (int i = -1; i <= 1; i++)
            {
                var lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lantern.name = $"FairyLantern_{i}";
                lantern.transform.SetParent(shopGO.transform, false);
                lantern.transform.localPosition = new Vector3(i * 1.1f, 2.35f, 0.7f);
                lantern.transform.localScale = new Vector3(0.24f, 0.28f, 0.24f);
                lantern.GetComponent<MeshRenderer>().sharedMaterial = warmLightMat;
                UnityEngine.Object.DestroyImmediate(lantern.GetComponent<Collider>());
            }

            // High-intensity Point Light illuminating the street and motorbike in front!
            var lightGO = new GameObject("Shop_Illumination_Light");
            lightGO.transform.SetParent(shopGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 2.2f, 1.2f);
            var pl = lightGO.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1.0f, 0.88f, 0.60f);
            pl.range = 22f;
            pl.intensity = 5.2f;

            // 3D Illuminated Billboard Signboard
            BuildShopSignboard(shopGO.transform, new Vector3(0f, 3.1f, 0.1f), "🧋 TIỆM TRÀ SỮA TOCOTOCO IUH", "Trà sữa trân châu hoàng gia • Trà đào cam sả đêm", new Color(0f, 0.75f, 0.85f));

            // Boba Cups on counter
            for (int j = -1; j <= 1; j++)
            {
                var cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cup.name = $"BobaCup_{j}";
                cup.transform.SetParent(shopGO.transform, false);
                cup.transform.localPosition = new Vector3(j * 0.7f, 1.25f, 0.2f);
                cup.transform.localScale = new Vector3(0.14f, 0.15f, 0.14f);
                cup.GetComponent<MeshRenderer>().sharedMaterial = warmLightMat;
                UnityEngine.Object.DestroyImmediate(cup.GetComponent<Collider>());
            }
        }

        private static void BuildFishBallCart(Transform parent, Vector3 pos, Material metalMat, Material yellowAwningMat, Material redLanternMat, Material warmLightMat)
        {
            var cartGO = new GameObject("Shop_02_FishBall_Cart_79");
            cartGO.transform.SetParent(parent, false);
            cartGO.transform.position = pos;
            cartGO.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            // Stainless Counter Cart
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "CartBody";
            counter.transform.SetParent(cartGO.transform, false);
            counter.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            counter.transform.localScale = new Vector3(2.6f, 1.05f, 1.2f);
            counter.GetComponent<MeshRenderer>().sharedMaterial = metalMat;

            // 2 Spoked Cart Wheels
            for (int w = -1; w <= 1; w += 2)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"CartWheel_{w}";
                wheel.transform.SetParent(cartGO.transform, false);
                wheel.transform.localPosition = new Vector3(w * 1.1f, 0.35f, -0.62f);
                wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                wheel.transform.localScale = new Vector3(0.55f, 0.06f, 0.55f);
                wheel.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
                UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());
            }

            // Cart Awning Canopy
            var canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = "Canopy";
            canopy.transform.SetParent(cartGO.transform, false);
            canopy.transform.localPosition = new Vector3(0f, 2.45f, 0.05f);
            canopy.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            canopy.transform.localScale = new Vector3(2.9f, 0.12f, 1.6f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = yellowAwningMat;
            UnityEngine.Object.DestroyImmediate(canopy.GetComponent<Collider>());

            // 4 Corner Metal Rods
            Vector3[] cOffsets = new Vector3[] {
                new Vector3(-1.25f, 1.55f, -0.55f),
                new Vector3(1.25f, 1.55f, -0.55f),
                new Vector3(-1.25f, 1.55f, 0.55f),
                new Vector3(1.25f, 1.55f, 0.55f)
            };
            foreach (var co in cOffsets)
            {
                var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rod.name = "Rod";
                rod.transform.SetParent(cartGO.transform, false);
                rod.transform.localPosition = co;
                rod.transform.localScale = new Vector3(0.06f, 0.95f, 0.06f);
                rod.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
                UnityEngine.Object.DestroyImmediate(rod.GetComponent<Collider>());
            }

            // Food Display Glass Showcase
            var showcase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            showcase.name = "Showcase";
            showcase.transform.SetParent(cartGO.transform, false);
            showcase.transform.localPosition = new Vector3(0.4f, 1.25f, 0.1f);
            showcase.transform.localScale = new Vector3(1.4f, 0.42f, 0.8f);
            showcase.GetComponent<MeshRenderer>().sharedMaterial = warmLightMat;
            UnityEngine.Object.DestroyImmediate(showcase.GetComponent<Collider>());

            // Steaming Frying Pot
            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "FryerPot";
            pot.transform.SetParent(cartGO.transform, false);
            pot.transform.localPosition = new Vector3(-0.75f, 1.15f, 0.1f);
            pot.transform.localScale = new Vector3(0.45f, 0.18f, 0.45f);
            pot.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
            UnityEngine.Object.DestroyImmediate(pot.GetComponent<Collider>());

            // 2 Vietnamese Round Red Lanterns
            for (int l = -1; l <= 1; l += 2)
            {
                var lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lantern.name = $"RedLantern_{l}";
                lantern.transform.SetParent(cartGO.transform, false);
                lantern.transform.localPosition = new Vector3(l * 1.1f, 2.15f, 0.65f);
                lantern.transform.localScale = new Vector3(0.32f, 0.36f, 0.32f);
                lantern.GetComponent<MeshRenderer>().sharedMaterial = redLanternMat;
                UnityEngine.Object.DestroyImmediate(lantern.GetComponent<Collider>());
            }

            // Warm Amber Point Light
            var lightGO = new GameObject("Cart_Illumination_Light");
            lightGO.transform.SetParent(cartGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 2.1f, 1.0f);
            var pl = lightGO.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1.0f, 0.68f, 0.22f);
            pl.range = 20f;
            pl.intensity = 5.5f;

            // Signboard
            BuildShopSignboard(cartGO.transform, new Vector3(0f, 2.95f, 0f), "🍢 XE CÁ VIÊN CHIÊN 79", "Cá viên • Bò viên • Xúc xích chiên giòn • Ăn vặt đêm IUH", new Color(0.95f, 0.75f, 0.05f));
        }

        private static void BuildBrokenRiceStall(Transform parent, Vector3 pos, Material woodMat, Material darkRoofMat, Material warmLightMat)
        {
            var stallGO = new GameObject("Shop_03_BrokenRice_Canteen");
            stallGO.transform.SetParent(parent, false);
            stallGO.transform.position = pos;
            stallGO.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            // Booth Counter
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter";
            counter.transform.SetParent(stallGO.transform, false);
            counter.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            counter.transform.localScale = new Vector3(3.2f, 1.1f, 1.4f);
            counter.GetComponent<MeshRenderer>().sharedMaterial = woodMat;

            // Pitched Tile Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "TileRoof";
            roof.transform.SetParent(stallGO.transform, false);
            roof.transform.localPosition = new Vector3(0f, 2.65f, 0.1f);
            roof.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            roof.transform.localScale = new Vector3(3.6f, 0.14f, 2.0f);
            roof.GetComponent<MeshRenderer>().sharedMaterial = darkRoofMat;
            UnityEngine.Object.DestroyImmediate(roof.GetComponent<Collider>());

            // Dining Table & Benches
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "DiningTable";
            table.transform.SetParent(stallGO.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.42f, 2.5f);
            table.transform.localScale = new Vector3(2.2f, 0.84f, 0.9f);
            table.GetComponent<MeshRenderer>().sharedMaterial = woodMat;

            for (int b = -1; b <= 1; b += 2)
            {
                var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bench.name = $"Bench_{b}";
                bench.transform.SetParent(stallGO.transform, false);
                bench.transform.localPosition = new Vector3(0f, 0.24f, 2.5f + b * 0.75f);
                bench.transform.localScale = new Vector3(2.2f, 0.48f, 0.35f);
                bench.GetComponent<MeshRenderer>().sharedMaterial = woodMat;
            }

            // 3 Hanging Bamboo Lanterns
            for (int k = -1; k <= 1; k++)
            {
                var lantern = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lantern.name = $"BambooLantern_{k}";
                lantern.transform.SetParent(stallGO.transform, false);
                lantern.transform.localPosition = new Vector3(k * 1.1f, 2.25f, 0.8f);
                lantern.transform.localScale = new Vector3(0.22f, 0.25f, 0.22f);
                lantern.GetComponent<MeshRenderer>().sharedMaterial = warmLightMat;
                UnityEngine.Object.DestroyImmediate(lantern.GetComponent<Collider>());
            }

            // Warm Orange Point Light
            var lightGO = new GameObject("Stall_Illumination_Light");
            lightGO.transform.SetParent(stallGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 2.2f, 1.4f);
            var pl = lightGO.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1.0f, 0.78f, 0.38f);
            pl.range = 22f;
            pl.intensity = 5.2f;

            // Signboard
            BuildShopSignboard(stallGO.transform, new Vector3(0f, 3.1f, 0.1f), "🍲 CƠM TẤM ĐÊM SÀI GÒN - IUH", "Cơm sườn bì chả • Trứng ốp la • Canh rong biển nóng", new Color(0.1f, 0.75f, 0.45f));
        }

        private static void BuildDeliveryDropoffStations(Transform parent)
        {
            var stationsRoot = new GameObject("DELIVERY_DROPOFF_STATIONS");
            stationsRoot.transform.SetParent(parent, false);

            var darkWood = GetOrCreateMaterial("M_IUH_Station_Wood", new Color(0.20f, 0.14f, 0.10f), 0.05f, 0.4f);
            var greenLightMat = GetOrCreateMaterial("M_IUH_Station_GreenGlow", new Color(0.1f, 1.0f, 0.4f), 0f, 0.9f, false, new Color(0.1f, 1.0f, 0.4f) * 3.5f);
            var warmLampMat = GetOrCreateMaterial("M_IUH_Station_WarmLamp", new Color(1.0f, 0.90f, 0.55f), 0f, 0.9f, false, new Color(1.0f, 0.90f, 0.55f) * 3.5f);

            // 1. Station KTX Block D (Cổng 4)
            BuildDropoffStation(stationsRoot.transform, "Station_01_KTX_Dorm", new Vector3(336.0f, 0.48f, -78.0f), "🏁 MỤC TIÊU GIAO HÀNG\n[ 🏢 KÝ TÚC XÁ KTX (CỔNG 4) ]", darkWood, greenLightMat, warmLampMat, true);

            // 2. Station Giảng Đường Tòa V
            BuildDropoffStation(stationsRoot.transform, "Station_02_Block_V", new Vector3(360.0f, 0.48f, -90.0f), "🏁 MỤC TIÊU GIAO HÀNG\n[ 🏛️ GIẢNG ĐƯỜNG TÒA V ]", darkWood, greenLightMat, warmLampMat, false);

            // 3. Station Sảnh Nghiên Cứu Tòa Nhà E
            BuildDropoffStation(stationsRoot.transform, "Station_03_Block_E", new Vector3(368.0f, 0.48f, -70.0f), "🏁 MỤC TIÊU GIAO HÀNG\n[ 🔬 SẢNH TÒA NHÀ E ]", darkWood, greenLightMat, warmLampMat, false);
        }

        private static void BuildDropoffStation(Transform parent, string name, Vector3 pos, string label, Material deskMat, Material greenGlowMat, Material lampMat, bool addStudentFigure)
        {
            var stationGO = new GameObject(name);
            stationGO.transform.SetParent(parent, false);
            stationGO.transform.position = pos;

            // Receiving Desk
            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "ReceivingDesk";
            desk.transform.SetParent(stationGO.transform, false);
            desk.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            desk.transform.localScale = new Vector3(2.2f, 0.9f, 1.0f);
            desk.GetComponent<MeshRenderer>().sharedMaterial = deskMat;

            // Parcel Boxes on Desk
            var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "ParcelCrate";
            crate.transform.SetParent(stationGO.transform, false);
            crate.transform.localPosition = new Vector3(-0.6f, 1.1f, 0f);
            crate.transform.localScale = new Vector3(0.55f, 0.4f, 0.55f);
            crate.GetComponent<MeshRenderer>().sharedMaterial = deskMat;
            UnityEngine.Object.DestroyImmediate(crate.GetComponent<Collider>());

            // Desk Study Lamp
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "DeskLamp";
            lamp.transform.SetParent(stationGO.transform, false);
            lamp.transform.localPosition = new Vector3(0.6f, 1.2f, 0f);
            lamp.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            lamp.GetComponent<MeshRenderer>().sharedMaterial = lampMat;
            UnityEngine.Object.DestroyImmediate(lamp.GetComponent<Collider>());

            // Green Point Light Beacon
            var lightGO = new GameObject("Station_GreenBeacon");
            lightGO.transform.SetParent(stationGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            var pl = lightGO.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(0.1f, 1.0f, 0.4f);
            pl.range = 24f;
            pl.intensity = 4.8f;

            // 3D Billboard Canvas
            var canvasGO = new GameObject("BillboardCanvas");
            canvasGO.transform.SetParent(stationGO.transform, false);
            canvasGO.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(480f, 110f);
            rect.localScale = new Vector3(0.009f, 0.009f, 0.009f);

            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(canvasGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = $"<color=#4ADE80><b>{label}</b></color>";
            txt.fontSize = 24;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;
            var outline = txtGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, 2f);

            // Student waiting figure
            if (addStudentFigure)
            {
                var student = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                student.name = "WaitingStudent";
                student.transform.SetParent(stationGO.transform, false);
                student.transform.localPosition = new Vector3(1.2f, 0.95f, 0f);
                student.transform.localScale = new Vector3(0.38f, 0.95f, 0.38f);
                student.GetComponent<MeshRenderer>().sharedMaterial = deskMat;
                UnityEngine.Object.DestroyImmediate(student.GetComponent<Collider>());

                // Glowing smartphone screen in hands
                var phone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                phone.name = "GlowingPhone";
                phone.transform.SetParent(student.transform, false);
                phone.transform.localPosition = new Vector3(0f, 0.15f, 0.35f);
                phone.transform.localScale = new Vector3(0.25f, 0.38f, 0.08f);
                phone.GetComponent<MeshRenderer>().sharedMaterial = greenGlowMat;
                UnityEngine.Object.DestroyImmediate(phone.GetComponent<Collider>());
            }
        }

        private static void BuildShopSignboard(Transform parent, Vector3 localPos, string title, string sub, Color borderCol)
        {
            var sbGO = new GameObject("Signboard");
            sbGO.transform.SetParent(parent, false);
            sbGO.transform.localPosition = localPos;

            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "Backing";
            bg.transform.SetParent(sbGO.transform, false);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale = new Vector3(3.2f, 0.75f, 0.08f);
            bg.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("M_IUH_Sign_Dark", new Color(0.04f, 0.08f, 0.16f), 0.1f, 0.5f);
            UnityEngine.Object.DestroyImmediate(bg.GetComponent<Collider>());

            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(sbGO.transform, false);
            canvasGO.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(480f, 110f);
            rect.localScale = new Vector3(0.0065f, 0.0065f, 0.0065f);

            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(canvasGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = $"<size=26><b>{title}</b></size>\n<size=16><color=#FEF08A>{sub}</color></size>";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;

            var outline = txtGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, 2f);
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
            scaler.matchWidthOrHeight = 1.0f;
            scaler.dynamicPixelsPerUnit = 2.0f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem exists
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSys = new GameObject("EventSystem");
                eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 1. Gameplay HUD (Profile Card, Destination Banner, Minimap, Action buttons)
            BuildDeliveryHUD(canvasGO.transform);

            // 2. Mobile Controls (Virtual Joystick & Action buttons - hidden on PC!)
            BuildMobileControls(canvasGO.transform);

            // 3. Horror Pursuit Overlay (Image 1 Panel 4)
            BuildHorrorPursuitOverlay(canvasGO.transform);

            // 4. Floating Portal Button [🎮 MENU TRÒ CHƠI (M / ESC)] (Top-Center, in front of HUD)
            BuildFloatingPortalButton(canvasGO.transform);

            // 5. Delivery Hand-off Modal (Image 1 Panel 3)
            BuildDeliveryDialogueModal(canvasGO.transform);

            // 6. Customization Wardrobe & Garage Modal (Image 2)
            BuildCustomizationPanel(canvasGO.transform);

            // 7. Results Dialogs (Victory & Defeat)
            BuildResultDialogs(canvasGO.transform);

            // 8. Mini-Game Selection Portal Menu [Phím M]
            BuildPortalMenuPanel(canvasGO.transform);

            // 9. Màn hình chính (Main Menu - Image 1 Panel 1)
            BuildMainMenuPanel(canvasGO.transform);

            canvasGO.AddComponent<IUHMiniGamePortalUI>();

            return canvasGO;
        }

        private static void BuildMainMenuPanel(Transform canvasTransform)
        {
            var menuGO = new GameObject("MainMenuPanel");
            menuGO.transform.SetParent(canvasTransform, false);

            var rect = menuGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bgImg = menuGO.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.05f, 0.12f, 0.94f);

            // Card Center
            var cardGO = new GameObject("TitleCard");
            cardGO.transform.SetParent(menuGO.transform, false);
            var cardRect = cardGO.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(560f, 620f);

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = new Color(0.04f, 0.08f, 0.16f, 0.96f);
            var cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.0f, 0.85f, 0.45f, 0.9f);
            cardOutline.effectDistance = new Vector2(2.5f, 2.5f);

            // Title
            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(cardGO.transform, false);
            var tRect = titleGO.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.05f, 0.72f);
            tRect.anchorMax = new Vector2(0.95f, 0.96f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var tTxt = titleGO.AddComponent<Text>();
            tTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tTxt.text = "<size=38><color=#00E676><b>🛵 Grab Campus</b></color></size>\n<size=22><color=#67E8F9><b>CHUYẾN XE TÂM LINH</b></color></size>";
            tTxt.alignment = TextAnchor.MiddleCenter;
            tTxt.color = Color.white;

            // Subtitle
            var subGO = new GameObject("TxtSubtitle");
            subGO.transform.SetParent(cardGO.transform, false);
            var subRect = subGO.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.05f, 0.60f);
            subRect.anchorMax = new Vector2(0.95f, 0.72f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;
            var subTxt = subGO.AddComponent<Text>();
            subTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subTxt.text = "<size=16><color=#94A3B8>Khám phá khuôn viên trường và bắt đầu hành trình của bạn!</color></size>";
            subTxt.alignment = TextAnchor.MiddleCenter;
            subTxt.color = Color.white;

            // Buttons Container
            var btnBox = new GameObject("ButtonsBox");
            btnBox.transform.SetParent(cardGO.transform, false);
            var bbRect = btnBox.AddComponent<RectTransform>();
            bbRect.anchorMin = new Vector2(0.1f, 0.06f);
            bbRect.anchorMax = new Vector2(0.9f, 0.58f);
            bbRect.offsetMin = Vector2.zero;
            bbRect.offsetMax = Vector2.zero;

            var vlg = btnBox.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            var btnStart = CreateMenuButton(btnBox.transform, "BtnStart", "▶️  BẮT ĐẦU CHƠI", new Color(0.0f, 0.68f, 0.32f), 20);
            var btnSettings = CreateMenuButton(btnBox.transform, "BtnSettings", "⚙️  CÀI ĐẶT ÂM LƯỢNG", new Color(0.12f, 0.22f, 0.38f), 18);
            var btnCustom = CreateMenuButton(btnBox.transform, "BtnCustomization", "👕  TRANG PHỤC & XE MÁY", new Color(0.02f, 0.52f, 0.78f), 18);
            var btnQuit = CreateMenuButton(btnBox.transform, "BtnQuit", "🚪  THOÁT", new Color(0.65f, 0.15f, 0.15f), 18);

            // Settings Sub-Dialog
            var setDialog = new GameObject("SettingsDialog");
            setDialog.transform.SetParent(menuGO.transform, false);
            var sRect = setDialog.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);
            sRect.sizeDelta = new Vector2(400f, 260f);
            var sImg = setDialog.AddComponent<Image>();
            sImg.color = new Color(0.05f, 0.1f, 0.2f, 0.98f);
            var sOutline = setDialog.AddComponent<Outline>();
            sOutline.effectColor = Color.cyan;
            sOutline.effectDistance = new Vector2(2f, 2f);

            var sTitle = new GameObject("Txt").AddComponent<Text>();
            sTitle.transform.SetParent(setDialog.transform, false);
            sTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sTitle.text = "<size=22><b>⚙️ CÀI ĐẶT ÂM LƯỢNG</b></size>";
            sTitle.alignment = TextAnchor.MiddleCenter;
            sTitle.color = Color.white;
            var stRect = sTitle.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0.1f, 0.65f);
            stRect.anchorMax = new Vector2(0.9f, 0.95f);
            stRect.offsetMin = Vector2.zero;
            stRect.offsetMax = Vector2.zero;

            var sliderGO = new GameObject("VolumeSlider");
            sliderGO.transform.SetParent(setDialog.transform, false);
            var slRect = sliderGO.AddComponent<RectTransform>();
            slRect.anchorMin = new Vector2(0.15f, 0.42f);
            slRect.anchorMax = new Vector2(0.85f, 0.58f);
            slRect.offsetMin = Vector2.zero;
            slRect.offsetMax = Vector2.zero;
            var slImg = sliderGO.AddComponent<Image>();
            slImg.color = new Color(0.2f, 0.3f, 0.4f);
            var slider = sliderGO.AddComponent<Slider>();

            var btnCloseSet = CreateMenuButton(setDialog.transform, "BtnClose", "Đóng", new Color(0.1f, 0.5f, 0.8f), 16);
            var bcsRect = btnCloseSet.GetComponent<RectTransform>();
            bcsRect.anchorMin = new Vector2(0.3f, 0.1f);
            bcsRect.anchorMax = new Vector2(0.7f, 0.28f);
            bcsRect.offsetMin = Vector2.zero;
            bcsRect.offsetMax = Vector2.zero;

            setDialog.SetActive(false);

            // Attach Main Menu UI Script
            var mmScript = menuGO.AddComponent<IUHMainMenuUI>();
            mmScript.Setup(menuGO, btnStart.GetComponent<Button>(), btnSettings.GetComponent<Button>(), btnCustom.GetComponent<Button>(), btnQuit.GetComponent<Button>(), setDialog, slider, btnCloseSet.GetComponent<Button>());
            menuGO.SetActive(false);
        }

        private static GameObject CreateMenuButton(Transform parent, string name, string text, Color color, int fontSize)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredHeight = 52f;

            var img = btnGO.AddComponent<Image>();
            img.color = color;

            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            var btn = btnGO.AddComponent<Button>();

            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(btnGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;

            return btnGO;
        }

        private static void BuildFloatingPortalButton(Transform canvasTransform)
        {
            var btnGO = new GameObject("FloatingPortalBtn");
            btnGO.transform.SetParent(canvasTransform, false);

            var rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -15f);
            rect.sizeDelta = new Vector2(380f, 50f);

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.18f, 0.96f);

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
            txt.text = "🏛️ MENU MINI-GAME  <color=#FDE047><b>[ M / ESC ]</b></color>";
            txt.fontSize = 18;
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
            rect.sizeDelta = new Vector2(1500f, 840f);

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
            hRect.anchoredPosition = new Vector2(0f, -12f);
            hRect.sizeDelta = new Vector2(0f, 90f);
            var hTxt = headerGO.AddComponent<Text>();
            hTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hTxt.text = "<size=30><color=#FEF08A><b>🏛️ CAMPUS ADVENTURE — CHỌN MINI-GAME</b></color></size>\n<size=17><color=#7DD3FC>⚡ Bấm phím số <b>[ 1 ]</b> đến <b>[ 6 ]</b> hoặc CLICK CHUỘT để chơi • Bấm <b>[ M ]</b> hoặc <b>[ ESC ]</b> để đóng</color></size>";
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = Color.white;

            // Wardrobe & Garage Customization Button [U]
            var btnCustPortal = CreateMenuButton(panelGO.transform, "BtnCustomizationPortal", "👕 CHỌN ĐỒNG PHỤC & XE MÁY (GRAB • BE • XANH SM) [ Phím U ]", new Color(0.0f, 0.65f, 0.45f), 16);
            var bpRect = btnCustPortal.GetComponent<RectTransform>();
            bpRect.anchorMin = new Vector2(0.5f, 1f);
            bpRect.anchorMax = new Vector2(0.5f, 1f);
            bpRect.pivot = new Vector2(0.5f, 1f);
            bpRect.anchoredPosition = new Vector2(0f, -86f);
            bpRect.sizeDelta = new Vector2(620f, 42f);

            // Close Button [X]
            var closeBtnGO = new GameObject("CloseBtn");
            closeBtnGO.transform.SetParent(panelGO.transform, false);
            var cRect = closeBtnGO.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(1f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(1f, 1f);
            cRect.anchoredPosition = new Vector2(-15f, -15f);
            cRect.sizeDelta = new Vector2(48f, 48f);
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
            cTxt.fontSize = 28;
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
            caRect.offsetMin = new Vector2(0f, 70f);
            caRect.offsetMax = new Vector2(0f, -135f);

            var grid = cardsArea.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(460f, 315f);
            grid.spacing = new Vector2(20f, 16f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // 6 Mini-Game Cards (NO author names, large fonts)
            BuildMiniGameCard(cardsArea.transform, 1, "🛵 2.1. Grab Campus: Chuyến Xe Tâm Linh", "Lấy trà sữa/cá viên tại quán chong đèn đêm, né ma và giao tới KTX. Giao thành công nhận điểm thưởng, thất bại phải chơi lại!", "Bãi Xe Cổng 1 (Nguyễn Văn Bảo)", "⚡ BẤM [ 1 ] ĐỂ CHỌN TRANG PHỤC & CHƠI", true, 0);
            BuildMiniGameCard(cardsArea.transform, 2, "👻 2.2. Trốn Tìm Với Ma Ở Ký Túc Xá", "Khám phá phòng tối KTX, né tránh ma tuần tra AI NavMesh 4 trạng thái, tìm chìa khóa & nhật ký.", "Sảnh Ký Túc Xá Block D", "📍 BẤM [ 2 ] ĐỂ ĐẾN KTX", false, 1);
            BuildMiniGameCard(cardsArea.transform, 3, "🔍 2.3. AI Object Hunt: Tìm Đồ Vật", "Sử dụng camera quét nhận diện sách cổ/bình chữa cháy qua Deep Learning YOLO & AR Hologram.", "Thư Viện Trung Tâm Block A", "📍 BẤM [ 3 ] ĐỂ ĐẾN THƯ VIỆN", false, 2);
            BuildMiniGameCard(cardsArea.transform, 4, "📸 2.4. Thợ Săn Khoảnh Khắc", "Khám phá khuôn viên, leo thang nóc nhà Block F chụp ảnh check-in và thu thập huy hiệu.", "Cầu Thang Lên Nóc Block F", "📍 BẤM [ 4 ] ĐỂ ĐẾN NÓC NHÀ", false, 3);
            BuildMiniGameCard(cardsArea.transform, 5, "🌱 2.5. Green Campus: Cứu Lấy Trường", "Thu gom và phân loại rác tái chế bằng AI, tích lũy điểm xanh đẩy lên Cloud Adafruit IO.", "Trạm Phân Loại Rác Sân Trường", "📍 BẤM [ 5 ] ĐỂ ĐẾN TRẠM RÁC", false, 4);
            BuildMiniGameCard(cardsArea.transform, 6, "🎪 2.6. Campus Festival: Tổ Chức Lễ Hội", "Quản lý ngân sách, trang trí sân khấu đại hội âm nhạc và kết nối đồng bộ Docker backend.", "Sân Khấu Đại Hội Trung Tâm", "📍 BẤM [ 6 ] ĐỂ ĐẾN SÂN KHẤU", false, 5);

            // Footer Instruction Bar
            var footerGO = new GameObject("Footer");
            footerGO.transform.SetParent(panelGO.transform, false);
            var fRect = footerGO.AddComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0f, 0f);
            fRect.anchorMax = new Vector2(1f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.anchoredPosition = new Vector2(0f, 10f);
            fRect.sizeDelta = new Vector2(0f, 54f);
            var fImg = footerGO.AddComponent<Image>();
            fImg.color = new Color(0.06f, 0.12f, 0.22f, 0.95f);

            var fTxtGO = new GameObject("Txt");
            fTxtGO.transform.SetParent(footerGO.transform, false);
            var fTxtRect = fTxtGO.AddComponent<RectTransform>();
            fTxtRect.anchorMin = new Vector2(0.02f, 0f);
            fTxtRect.anchorMax = new Vector2(0.60f, 1f);
            fTxtRect.offsetMin = Vector2.zero;
            fTxtRect.offsetMax = Vector2.zero;
            var fTxt = fTxtGO.AddComponent<Text>();
            fTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            fTxt.text = "💡 <color=#FDE047><b>MẸO NHANH:</b></color> Bấm phím <b>[1]</b> đến <b>[6]</b> để chọn trò chơi • Bấm <b>[M]</b> hoặc <b>[ESC]</b> để đóng/mở menu";
            fTxt.fontSize = 16;
            fTxt.alignment = TextAnchor.MiddleLeft;
            fTxt.color = Color.white;
            fTxt.fontStyle = FontStyle.Bold;

            // Free Roam Button
            var btnFR = CreateMenuButton(footerGO.transform, "BtnFreeRoam", "🚶 KHÁM PHÁ TỰ DO", new Color(0.18f, 0.32f, 0.50f), 15);
            var frRect = btnFR.GetComponent<RectTransform>();
            frRect.anchorMin = new Vector2(0.62f, 0.12f);
            frRect.anchorMax = new Vector2(0.79f, 0.88f);
            frRect.offsetMin = Vector2.zero;
            frRect.offsetMax = Vector2.zero;

            // Resume Button
            var btnRes = CreateMenuButton(footerGO.transform, "BtnResume", "▶️ TIẾP TỤC CHƠI [ ESC ]", new Color(0.0f, 0.65f, 0.32f), 15);
            var resRect = btnRes.GetComponent<RectTransform>();
            resRect.anchorMin = new Vector2(0.81f, 0.12f);
            resRect.anchorMax = new Vector2(0.98f, 0.88f);
            resRect.offsetMin = Vector2.zero;
            resRect.offsetMax = Vector2.zero;
        }

        private static void BuildMiniGameCard(Transform parent, int hotkeyNum, string title, string desc, string location, string btnLabel, bool isGrabGame, int cardIndex)
        {
            var cardGO = new GameObject($"Card_{cardIndex}_Game{hotkeyNum}");
            cardGO.transform.SetParent(parent, false);

            var img = cardGO.AddComponent<Image>();
            img.color = isGrabGame ? new Color(0.04f, 0.18f, 0.12f, 0.96f) : new Color(0.07f, 0.13f, 0.24f, 0.96f);

            var outline = cardGO.AddComponent<Outline>();
            outline.effectColor = isGrabGame ? new Color(0.0f, 0.95f, 0.5f, 1.0f) : new Color(0.25f, 0.55f, 0.95f, 0.9f);
            outline.effectDistance = new Vector2(2.5f, 2.5f);

            var cardBtn = cardGO.AddComponent<Button>();

            var vlg = cardGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(18, 18, 14, 14);
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // 1. Top Row: Hotkey Badge [PHÍM X]
            var topRow = new GameObject("TopRow");
            topRow.transform.SetParent(cardGO.transform, false);
            var topHlg = topRow.AddComponent<HorizontalLayoutGroup>();
            topHlg.childControlWidth = false;
            topHlg.childControlHeight = true;
            topRow.AddComponent<LayoutElement>().preferredHeight = 34f;

            var keyBadge = new GameObject("KeyBadge");
            keyBadge.transform.SetParent(topRow.transform, false);
            var kbImg = keyBadge.AddComponent<Image>();
            kbImg.color = isGrabGame ? new Color(1.0f, 0.85f, 0.15f) : new Color(0.2f, 0.85f, 1.0f);
            var kbLe = keyBadge.AddComponent<LayoutElement>();
            kbLe.preferredWidth = 140f;
            var kbTxtGO = new GameObject("Txt");
            kbTxtGO.transform.SetParent(keyBadge.transform, false);
            var kbTxtRect = kbTxtGO.AddComponent<RectTransform>();
            kbTxtRect.anchorMin = Vector2.zero;
            kbTxtRect.anchorMax = Vector2.one;
            var kbTxt = kbTxtGO.AddComponent<Text>();
            kbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            kbTxt.text = $"[ PHÍM {hotkeyNum} ]";
            kbTxt.fontSize = 18;
            kbTxt.alignment = TextAnchor.MiddleCenter;
            kbTxt.color = Color.black;
            kbTxt.fontStyle = FontStyle.Bold;

            // 2. Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(cardGO.transform, false);
            var titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.text = $"<b>{title}</b>";
            titleTxt.fontSize = 20;
            titleTxt.color = isGrabGame ? new Color(0.5f, 1.0f, 0.7f) : Color.white;
            titleTxt.fontStyle = FontStyle.Bold;
            titleGO.AddComponent<LayoutElement>().preferredHeight = 38f;

            // 3. Description
            var descGO = new GameObject("Desc");
            descGO.transform.SetParent(cardGO.transform, false);
            var descTxt = descGO.AddComponent<Text>();
            descTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descTxt.text = desc;
            descTxt.fontSize = 15;
            descTxt.lineSpacing = 1.2f;
            descTxt.color = new Color(0.92f, 0.95f, 0.98f);
            descGO.AddComponent<LayoutElement>().preferredHeight = 80f;

            // 4. Location Tag
            var locGO = new GameObject("Location");
            locGO.transform.SetParent(cardGO.transform, false);
            var locTxt = locGO.AddComponent<Text>();
            locTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            locTxt.text = $"📍 <b>Vị trí:</b> <color=#38BDF8>{location}</color>";
            locTxt.fontSize = 14;
            locTxt.color = new Color(0.75f, 0.85f, 0.95f);
            locGO.AddComponent<LayoutElement>().preferredHeight = 26f;

            // 5. Action Button
            var btnBarGO = new GameObject("BtnBar");
            btnBarGO.transform.SetParent(cardGO.transform, false);
            var hlg = btnBarGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            btnBarGO.AddComponent<LayoutElement>().preferredHeight = 46f;

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
            aTxt.fontSize = 17;
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

            // 1. Top-Left: Player Profile Status Card (Image 1 Panel 2)
            var profileCard = new GameObject("PlayerProfileCard");
            profileCard.transform.SetParent(hudGO.transform, false);
            var pRect = profileCard.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0f, 1f);
            pRect.anchorMax = new Vector2(0f, 1f);
            pRect.pivot = new Vector2(0f, 1f);
            pRect.anchoredPosition = new Vector2(25f, -20f);
            pRect.sizeDelta = new Vector2(320f, 76f);

            var pImg = profileCard.AddComponent<Image>();
            pImg.color = new Color(0.04f, 0.08f, 0.16f, 0.92f);
            var pOutline = profileCard.AddComponent<Outline>();
            pOutline.effectColor = new Color(0.0f, 0.75f, 1.0f, 0.8f);
            pOutline.effectDistance = new Vector2(2f, 2f);

            // Avatar Circle
            var avatarGO = new GameObject("AvatarCircle");
            avatarGO.transform.SetParent(profileCard.transform, false);
            var avRect = avatarGO.AddComponent<RectTransform>();
            avRect.anchorMin = new Vector2(0.05f, 0.15f);
            avRect.anchorMax = new Vector2(0.24f, 0.85f);
            avRect.offsetMin = Vector2.zero;
            avRect.offsetMax = Vector2.zero;
            var avImg = avatarGO.AddComponent<Image>();
            avImg.color = new Color(0.12f, 0.55f, 0.85f);
            var avTxt = new GameObject("Txt").AddComponent<Text>();
            avTxt.transform.SetParent(avatarGO.transform, false);
            avTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            avTxt.text = "🎓";
            avTxt.fontSize = 24;
            avTxt.alignment = TextAnchor.MiddleCenter;
            var avtRect = avTxt.GetComponent<RectTransform>();
            avtRect.anchorMin = Vector2.zero;
            avtRect.anchorMax = Vector2.one;

            // Info (Lv. 3, Money, XP)
            var pInfoGO = new GameObject("Info");
            pInfoGO.transform.SetParent(profileCard.transform, false);
            var piRect = pInfoGO.AddComponent<RectTransform>();
            piRect.anchorMin = new Vector2(0.28f, 0.08f);
            piRect.anchorMax = new Vector2(0.96f, 0.92f);
            piRect.offsetMin = Vector2.zero;
            piRect.offsetMax = Vector2.zero;
            var piTxt = pInfoGO.AddComponent<Text>();
            piTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            piTxt.text = "<size=17><color=#FDE047><b>Lv. 3</b></color>  <color=#34D399><b>$ 120</b></color></size>\n<size=13><color=#93C5FD>Kinh nghiệm: 120/300 XP</color></size>";
            piTxt.color = Color.white;
            piTxt.alignment = TextAnchor.MiddleLeft;

            // Costume / Brand Customization Button on HUD [U]
            var btnCustHUD = CreateMenuButton(hudGO.transform, "BtnCustomizationHUD", "👕 ĐỔI TRANG PHỤC [ U ]", new Color(0.0f, 0.70f, 0.35f), 15);
            var bhRect = btnCustHUD.GetComponent<RectTransform>();
            bhRect.anchorMin = new Vector2(0f, 1f);
            bhRect.anchorMax = new Vector2(0f, 1f);
            bhRect.pivot = new Vector2(0f, 1f);
            bhRect.anchoredPosition = new Vector2(25f, -104f);
            bhRect.sizeDelta = new Vector2(220f, 38f);

            // 2. Top-Right: Destination Target Banner (Image 1 Panel 2)
            var topBanner = new GameObject("TopRightBanner");
            topBanner.transform.SetParent(hudGO.transform, false);
            var tbRect = topBanner.AddComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(1f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(1f, 1f);
            tbRect.anchoredPosition = new Vector2(-25f, -20f);
            tbRect.sizeDelta = new Vector2(460f, 76f);

            var tbImg = topBanner.AddComponent<Image>();
            tbImg.color = new Color(0.04f, 0.1f, 0.18f, 0.94f);
            var tbOutline = topBanner.AddComponent<Outline>();
            tbOutline.effectColor = new Color(0f, 0.85f, 0.45f, 0.9f);
            tbOutline.effectDistance = new Vector2(2f, 2f);

            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(topBanner.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.04f, 0.52f);
            titleRect.anchorMax = new Vector2(0.68f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.text = "🛵 Grab Campus";
            titleTxt.fontSize = 18;
            titleTxt.color = Color.white;
            titleTxt.fontStyle = FontStyle.Bold;

            var objGO = new GameObject("TxtObjective");
            objGO.transform.SetParent(topBanner.transform, false);
            var objRect = objGO.AddComponent<RectTransform>();
            objRect.anchorMin = new Vector2(0.04f, 0.08f);
            objRect.anchorMax = new Vector2(0.68f, 0.5f);
            objRect.offsetMin = Vector2.zero;
            objRect.offsetMax = Vector2.zero;
            var objTxt = objGO.AddComponent<Text>();
            objTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            objTxt.text = "📍 Giao cho KTX B • 320m";
            objTxt.fontSize = 15;
            objTxt.color = new Color(0.7f, 0.95f, 0.85f);

            var timerGO = new GameObject("TxtTimer");
            timerGO.transform.SetParent(topBanner.transform, false);
            var timerRect = timerGO.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.70f, 0.1f);
            timerRect.anchorMax = new Vector2(0.97f, 0.9f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            var timerTxt = timerGO.AddComponent<Text>();
            timerTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timerTxt.text = "⏱ 03:20";
            timerTxt.fontSize = 26;
            timerTxt.alignment = TextAnchor.MiddleCenter;
            timerTxt.color = Color.yellow;
            timerTxt.fontStyle = FontStyle.Bold;

            // 3. Bottom-Left: Radar Minimap (Image 1 Panel 2)
            BuildMinimapRadar(hudGO.transform);

            // 4. Bottom-Center: Cargo Integrity & Speed Card
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

            var barBgGO = new GameObject("CargoBarBg");
            barBgGO.transform.SetParent(bottomCard.transform, false);
            var barBgRect = barBgGO.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.04f, 0.15f);
            barBgRect.anchorMax = new Vector2(0.48f, 0.45f);
            barBgRect.offsetMin = Vector2.zero;
            barBgRect.offsetMax = Vector2.zero;
            var barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

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

        private static void BuildMinimapRadar(Transform hudTransform)
        {
            var radarGO = new GameObject("MinimapRadar");
            radarGO.transform.SetParent(hudTransform, false);
            var rect = radarGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(25f, 25f);
            rect.sizeDelta = new Vector2(190f, 190f);

            var diskImg = radarGO.AddComponent<Image>();
            diskImg.color = new Color(0.03f, 0.08f, 0.14f, 0.92f);
            var diskOutline = radarGO.AddComponent<Outline>();
            diskOutline.effectColor = new Color(0f, 0.85f, 1.0f, 0.9f);
            diskOutline.effectDistance = new Vector2(2f, 2f);

            // Crosshairs Grid
            var gridGO = new GameObject("Crosshairs");
            gridGO.transform.SetParent(radarGO.transform, false);
            var gRect = gridGO.AddComponent<RectTransform>();
            gRect.anchorMin = Vector2.zero;
            gRect.anchorMax = Vector2.one;
            gRect.offsetMin = Vector2.zero;
            gRect.offsetMax = Vector2.zero;
            var gTxt = gridGO.AddComponent<Text>();
            gTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            gTxt.text = "<size=28><color=#38BDF822>✛</color></size>";
            gTxt.alignment = TextAnchor.MiddleCenter;

            // Player Icon (Yellow Rotating Arrow)
            var pIconGO = new GameObject("PlayerArrow");
            pIconGO.transform.SetParent(radarGO.transform, false);
            var pRect = pIconGO.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(22f, 22f);
            var pTxt = pIconGO.AddComponent<Text>();
            pTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pTxt.text = "<color=#FACC15>▲</color>";
            pTxt.fontSize = 18;
            pTxt.alignment = TextAnchor.MiddleCenter;

            // Objective Blip (Destination Flag - Green 🏁)
            var objBlipGO = new GameObject("ObjectiveBlip");
            objBlipGO.transform.SetParent(radarGO.transform, false);
            var obRect = objBlipGO.AddComponent<RectTransform>();
            obRect.sizeDelta = new Vector2(18f, 18f);
            var obTxt = objBlipGO.AddComponent<Text>();
            obTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            obTxt.text = "<color=#22C55E>🏁</color>";
            obTxt.fontSize = 18;
            obTxt.alignment = TextAnchor.MiddleCenter;
            objBlipGO.SetActive(false);

            // Pickup Blip (Shop Icon - Cyan/Gold 🧋)
            var pBlipGO = new GameObject("PickupBlip");
            pBlipGO.transform.SetParent(radarGO.transform, false);
            var pbRect = pBlipGO.AddComponent<RectTransform>();
            pbRect.sizeDelta = new Vector2(18f, 18f);
            var pbTxt = pBlipGO.AddComponent<Text>();
            pbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pbTxt.text = "<color=#38BDF8>🧋</color>";
            pbTxt.fontSize = 18;
            pbTxt.alignment = TextAnchor.MiddleCenter;
            pBlipGO.SetActive(false);

            // Route Info Badge below radar
            var routeBadgeGO = new GameObject("RouteInfoBadge");
            routeBadgeGO.transform.SetParent(radarGO.transform, false);
            var rbRect = routeBadgeGO.AddComponent<RectTransform>();
            rbRect.anchorMin = new Vector2(0.5f, 0f);
            rbRect.anchorMax = new Vector2(0.5f, 0f);
            rbRect.pivot = new Vector2(0.5f, 1f);
            rbRect.anchoredPosition = new Vector2(0f, -8f);
            rbRect.sizeDelta = new Vector2(240f, 44f);
            var rbImg = routeBadgeGO.AddComponent<Image>();
            rbImg.color = new Color(0.04f, 0.08f, 0.16f, 0.95f);
            var rbOutline = routeBadgeGO.AddComponent<Outline>();
            rbOutline.effectColor = new Color(0.0f, 0.75f, 1.0f, 0.8f);
            rbOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var routeTxtGO = new GameObject("TxtRouteInfo");
            routeTxtGO.transform.SetParent(routeBadgeGO.transform, false);
            var rtRect = routeTxtGO.AddComponent<RectTransform>();
            rtRect.anchorMin = Vector2.zero;
            rtRect.anchorMax = Vector2.one;
            rtRect.offsetMin = new Vector2(6f, 2f);
            rtRect.offsetMax = new Vector2(-6f, -2f);
            var rtTxt = routeTxtGO.AddComponent<Text>();
            rtTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rtTxt.text = "🧋 Lấy hàng ➔ 🏁 Giao hàng";
            rtTxt.fontSize = 12;
            rtTxt.alignment = TextAnchor.MiddleCenter;
            rtTxt.color = Color.white;
            rtTxt.fontStyle = FontStyle.Bold;

            var minimapScript = radarGO.AddComponent<IUHMinimapUI>();
            minimapScript.Setup(rect, pRect, obRect, rect);
        }

        private static void BuildMobileControls(Transform canvasTransform)
        {
            var mobileRoot = new GameObject("MobileControls");
            mobileRoot.transform.SetParent(canvasTransform, false);
            var mrRect = mobileRoot.AddComponent<RectTransform>();
            mrRect.anchorMin = Vector2.zero;
            mrRect.anchorMax = Vector2.one;
            mrRect.offsetMin = Vector2.zero;
            mrRect.offsetMax = Vector2.zero;

            // Virtual Joystick on Bottom-Left
            var joyBase = new GameObject("JoystickBase");
            joyBase.transform.SetParent(mobileRoot.transform, false);
            var jbRect = joyBase.AddComponent<RectTransform>();
            jbRect.anchorMin = new Vector2(0f, 0f);
            jbRect.anchorMax = new Vector2(0f, 0f);
            jbRect.pivot = new Vector2(0.5f, 0.5f);
            jbRect.anchoredPosition = new Vector2(310f, 110f);
            jbRect.sizeDelta = new Vector2(130f, 130f);

            var jbImg = joyBase.AddComponent<Image>();
            jbImg.color = new Color(0.06f, 0.12f, 0.22f, 0.85f);
            var jbOutline = joyBase.AddComponent<Outline>();
            jbOutline.effectColor = new Color(0f, 0.85f, 1f, 0.8f);
            jbOutline.effectDistance = new Vector2(2f, 2f);

            var joyHandle = new GameObject("JoystickHandle");
            joyHandle.transform.SetParent(joyBase.transform, false);
            var jhRect = joyHandle.AddComponent<RectTransform>();
            jhRect.sizeDelta = new Vector2(55f, 55f);
            var jhImg = joyHandle.AddComponent<Image>();
            jhImg.color = new Color(0.2f, 0.85f, 1f, 0.95f);

            // Right Action Buttons
            // 1. Vehicle Mount [🛵]
            var btnVeh = CreateRoundActionButton(mobileRoot.transform, "BtnVehicle", "🛵", new Vector2(-205f, 115f), 68f, new Color(0.0f, 0.72f, 0.35f));
            // 2. Interaction [💬]
            var btnInt = CreateRoundActionButton(mobileRoot.transform, "BtnInteract", "💬", new Vector2(-120f, 185f), 68f, new Color(0.12f, 0.55f, 0.95f));
            // 3. Action / Turbo [⚡]
            var btnAct = CreateRoundActionButton(mobileRoot.transform, "BtnAction", "⚡", new Vector2(-105f, 95f), 78f, new Color(1.0f, 0.45f, 0.1f));

            var mcScript = mobileRoot.AddComponent<IUHMobileControlsUI>();
            mcScript.Setup(jbRect, jhRect, btnVeh.GetComponent<Button>(), btnInt.GetComponent<Button>(), btnAct.GetComponent<Button>());
            mobileRoot.SetActive(Application.isMobilePlatform);
        }

        private static GameObject CreateRoundActionButton(Transform parent, string name, string icon, Vector2 pos, float size, Color col)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(size, size);

            var img = btnGO.AddComponent<Image>();
            img.color = col;
            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2f, 2f);

            var btn = btnGO.AddComponent<Button>();

            var txtGO = new GameObject("Icon");
            txtGO.transform.SetParent(btnGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = icon;
            txt.fontSize = (int)(size * 0.45f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btnGO;
        }

        private static void BuildHorrorPursuitOverlay(Transform canvasTransform)
        {
            var horrorGO = new GameObject("HorrorPursuitOverlay");
            horrorGO.transform.SetParent(canvasTransform, false);
            var rect = horrorGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Full screen vignette
            var vignetteGO = new GameObject("Vignette");
            vignetteGO.transform.SetParent(horrorGO.transform, false);
            var vRect = vignetteGO.AddComponent<RectTransform>();
            vRect.anchorMin = Vector2.zero;
            vRect.anchorMax = Vector2.one;
            vRect.offsetMin = Vector2.zero;
            vRect.offsetMax = Vector2.zero;
            var vImg = vignetteGO.AddComponent<Image>();
            vImg.color = new Color(0.8f, 0f, 0f, 0f);

            // Danger Banner at Top (Image 1 Panel 4)
            var bannerGO = new GameObject("DangerAlertBanner");
            bannerGO.transform.SetParent(horrorGO.transform, false);
            var bRect = bannerGO.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 1f);
            bRect.anchorMax = new Vector2(0.5f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.anchoredPosition = new Vector2(0f, -95f);
            bRect.sizeDelta = new Vector2(460f, 54f);

            var bImg = bannerGO.AddComponent<Image>();
            bImg.color = new Color(0.72f, 0.08f, 0.08f, 0.95f);
            var bOutline = bannerGO.AddComponent<Outline>();
            bOutline.effectColor = Color.yellow;
            bOutline.effectDistance = new Vector2(2f, 2f);

            var txtGO = new GameObject("TxtWarning");
            txtGO.transform.SetParent(bannerGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var bTxt = txtGO.AddComponent<Text>();
            bTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bTxt.text = "⚠️ MA ĐANG ĐUỔI THEO! 👻";
            bTxt.fontSize = 20;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;
            bTxt.fontStyle = FontStyle.Bold;

            // Pursuit Threat Fill Bar
            var fillBg = new GameObject("PursuitBarBg");
            fillBg.transform.SetParent(bannerGO.transform, false);
            var fbRect = fillBg.AddComponent<RectTransform>();
            fbRect.anchorMin = new Vector2(0.05f, 0.06f);
            fbRect.anchorMax = new Vector2(0.95f, 0.22f);
            fbRect.offsetMin = Vector2.zero;
            fbRect.offsetMax = Vector2.zero;
            var fbImg = fillBg.AddComponent<Image>();
            fbImg.color = new Color(0.2f, 0.05f, 0.05f, 0.9f);

            var fill = new GameObject("PursuitBarFill");
            fill.transform.SetParent(fillBg.transform, false);
            var fRect = fill.AddComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;
            var fImg = fill.AddComponent<Image>();
            fImg.color = Color.red;
            fImg.type = Image.Type.Filled;
            fImg.fillMethod = Image.FillMethod.Horizontal;
            fImg.fillAmount = 0.5f;

            var hpScript = horrorGO.AddComponent<IUHHorrorPursuitUI>();
            hpScript.Setup(bannerGO, bTxt, fImg, vImg);
            bannerGO.SetActive(false);
        }

        private static void BuildDeliveryDialogueModal(Transform canvasTransform)
        {
            var dialogGO = new GameObject("DeliveryDialogueModal");
            dialogGO.transform.SetParent(canvasTransform, false);
            var rect = dialogGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var cardGO = new GameObject("DialogCard");
            cardGO.transform.SetParent(dialogGO.transform, false);
            var cRect = cardGO.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(520f, 380f);

            var cImg = cardGO.AddComponent<Image>();
            cImg.color = new Color(0.04f, 0.08f, 0.16f, 0.98f);
            var cOutline = cardGO.AddComponent<Outline>();
            cOutline.effectColor = new Color(0f, 0.85f, 0.45f, 0.95f);
            cOutline.effectDistance = new Vector2(2.5f, 2.5f);

            // Title
            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(cardGO.transform, false);
            var tRect = titleGO.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.06f, 0.75f);
            tRect.anchorMax = new Vector2(0.94f, 0.94f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var tTxt = titleGO.AddComponent<Text>();
            tTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tTxt.text = "<size=24><color=#34D399><b>📦 Giao Hàng — Ký Túc Xá B</b></color></size>";
            tTxt.alignment = TextAnchor.MiddleCenter;
            tTxt.color = Color.white;

            // Message
            var msgGO = new GameObject("TxtMessage");
            msgGO.transform.SetParent(cardGO.transform, false);
            var mRect = msgGO.AddComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.08f, 0.42f);
            mRect.anchorMax = new Vector2(0.92f, 0.72f);
            mRect.offsetMin = Vector2.zero;
            mRect.offsetMax = Vector2.zero;
            var mTxt = msgGO.AddComponent<Text>();
            mTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mTxt.text = "Bạn có đơn hàng giao cho bạn sinh viên này. Hãy giao đúng địa điểm và thời gian nhé!";
            mTxt.fontSize = 16;
            mTxt.lineSpacing = 1.25f;
            mTxt.alignment = TextAnchor.MiddleCenter;
            mTxt.color = new Color(0.9f, 0.95f, 0.98f);

            // Rewards
            var rwdGO = new GameObject("TxtRewards");
            rwdGO.transform.SetParent(cardGO.transform, false);
            var rRect = rwdGO.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.08f, 0.24f);
            rRect.anchorMax = new Vector2(0.92f, 0.40f);
            rRect.offsetMin = Vector2.zero;
            rRect.offsetMax = Vector2.zero;
            var rTxt = rwdGO.AddComponent<Text>();
            rTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rTxt.text = "<size=17><color=#FDE047><b>Phần thưởng:</b></color>  <color=#34D399>💰 +75,000 VNĐ</color>  •  <color=#60A5FA>⭐ +100 XP</color></size>";
            rTxt.alignment = TextAnchor.MiddleCenter;

            // Deliver Button
            var btnDeliver = CreateMenuButton(cardGO.transform, "BtnDeliver", "GIAO HÀNG  [ E ]", new Color(0.0f, 0.68f, 0.32f), 18);
            var bdRect = btnDeliver.GetComponent<RectTransform>();
            bdRect.anchorMin = new Vector2(0.2f, 0.06f);
            bdRect.anchorMax = new Vector2(0.8f, 0.22f);
            bdRect.offsetMin = Vector2.zero;
            bdRect.offsetMax = Vector2.zero;

            var dScript = dialogGO.AddComponent<IUHDeliveryDialogueUI>();
            dScript.Setup(dialogGO, tTxt, mTxt, rTxt, btnDeliver.GetComponent<Button>());
            dialogGO.SetActive(false);
        }

        private static void BuildCustomizationPanel(Transform canvasTransform)
        {
            var customGO = new GameObject("CustomizationPanel");
            customGO.transform.SetParent(canvasTransform, false);
            var rect = customGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1440f, 820f);

            var img = customGO.AddComponent<Image>();
            img.color = new Color(0.03f, 0.06f, 0.12f, 0.98f);
            var outline = customGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.85f, 1.0f, 0.9f);
            outline.effectDistance = new Vector2(3f, 3f);

            // Header
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(customGO.transform, false);
            var hRect = headerGO.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.anchoredPosition = new Vector2(0f, -12f);
            hRect.sizeDelta = new Vector2(0f, 90f);
            var hTxt = headerGO.AddComponent<Text>();
            hTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hTxt.text = "<size=30><color=#FACC15><b>🏛️ CAMPUS ADVENTURE — TRANG PHỤC & XE MÁY</b></color></size>\n<size=16><color=#7DD3FC>Chọn phong cách trang phục hãng công nghệ và kiểu xe yêu thích của bạn</color></size>";
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = Color.white;

            // Back Button
            var backBtn = CreateMenuButton(customGO.transform, "BtnBack", "✕  QUAY LẠI", new Color(0.7f, 0.2f, 0.2f), 16);
            var bbRect = backBtn.GetComponent<RectTransform>();
            bbRect.anchorMin = new Vector2(1f, 1f);
            bbRect.anchorMax = new Vector2(1f, 1f);
            bbRect.pivot = new Vector2(1f, 1f);
            bbRect.anchoredPosition = new Vector2(-20f, -20f);
            bbRect.sizeDelta = new Vector2(160f, 48f);

            // Cards Grid (4 Brands: XANH SM, Grab, Be, Sinh Viên IUH)
            var cardsBox = new GameObject("CardsBox");
            cardsBox.transform.SetParent(customGO.transform, false);
            var cbRect = cardsBox.AddComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.03f, 0.12f);
            cbRect.anchorMax = new Vector2(0.97f, 0.86f);
            cbRect.offsetMin = Vector2.zero;
            cbRect.offsetMax = Vector2.zero;

            var hlg = cardsBox.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var (cardXanhSM, btnXanhSM, txtXanhSM) = BuildBrandCard(cardsBox.transform, "XANH SM", "Lịch sự - Thân thiện - Xanh", "Trang phục xe điện Xanh SM ngọc bích thời thượng.", "⚡ +5% Tốc độ\n🛡️ +5% Sức bền\n🌟 +10% Nhận diện", new Color(0.0f, 0.72f, 0.85f));
            var (cardGrab, btnGrab, txtGrab) = BuildBrandCard(cardsBox.transform, "Grab", "Nhanh chóng - An toàn - Tiện lợi", "Màu xanh lá đặc trưng Grab siêu tốc né ma cực đỉnh.", "⚡ +10% Tốc độ\n👻 +5% Né ma\n📦 Bền kiện hàng", new Color(0.0f, 0.69f, 0.31f));
            var (cardBe, btnBe, txtBe) = BuildBrandCard(cardsBox.transform, "be", "Đồng hành - Linh hoạt - Dễ dàng", "Sắc vàng be năng động mang lại nhiều tiền thưởng.", "💰 +15% Tiền thưởng\n🛡️ +10% Sức bền\n🌟 Tích điểm nhanh", new Color(1.0f, 0.76f, 0.03f));
            var (cardDefault, btnDefault, txtDefault) = BuildBrandCard(cardsBox.transform, "Sinh Viên IUH", "Năng động - Sáng tạo - Trí tuệ", "Đồng phục sinh viên truyền thống IUH & xe thể thao.", "🎓 Chuẩn sinh viên IUH\n⚡ Cân bằng toàn diện\n🛡️ Bền bỉ", new Color(0.12f, 0.35f, 0.80f));

            // Footer Action Bar
            var footerGO = new GameObject("Footer");
            footerGO.transform.SetParent(customGO.transform, false);
            var fRect = footerGO.AddComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0f, 0f);
            fRect.anchorMax = new Vector2(1f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.anchoredPosition = new Vector2(0f, 10f);
            fRect.sizeDelta = new Vector2(0f, 65f);
            var fImg = footerGO.AddComponent<Image>();
            fImg.color = new Color(0.05f, 0.10f, 0.18f, 0.96f);

            var tipTxtGO = new GameObject("TxtTip");
            tipTxtGO.transform.SetParent(footerGO.transform, false);
            var ttRect = tipTxtGO.AddComponent<RectTransform>();
            ttRect.anchorMin = new Vector2(0.03f, 0f);
            ttRect.anchorMax = new Vector2(0.46f, 1f);
            ttRect.offsetMin = Vector2.zero;
            ttRect.offsetMax = Vector2.zero;
            var ttTxt = tipTxtGO.AddComponent<Text>();
            ttTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ttTxt.text = "💡 <color=#FDE047><b>MẸO:</b></color> Bấm phím <b>[ 1 ]</b> đến <b>[ 4 ]</b> để chọn nhanh • Bấm <b>[ U ]</b> bất cứ lúc nào trong game để đổi lại";
            ttTxt.fontSize = 15;
            ttTxt.alignment = TextAnchor.MiddleLeft;
            ttTxt.color = Color.white;
            ttTxt.fontStyle = FontStyle.Bold;

            var btnPortal = CreateMenuButton(footerGO.transform, "BtnOpenPortal", "🏛️ MENU CHỌN TRÒ CHƠI [ M ]", new Color(0.12f, 0.28f, 0.50f), 15);
            var bpRect = btnPortal.GetComponent<RectTransform>();
            bpRect.anchorMin = new Vector2(0.48f, 0.12f);
            bpRect.anchorMax = new Vector2(0.68f, 0.88f);
            bpRect.offsetMin = Vector2.zero;
            bpRect.offsetMax = Vector2.zero;

            var btnStart = CreateMenuButton(footerGO.transform, "BtnStartGame", "▶️ BẮT ĐẦU CHƠI VỚI TRANG PHỤC NÀY", new Color(0.0f, 0.72f, 0.35f), 16);
            var bsRect = btnStart.GetComponent<RectTransform>();
            bsRect.anchorMin = new Vector2(0.70f, 0.12f);
            bsRect.anchorMax = new Vector2(0.98f, 0.88f);
            bsRect.offsetMin = Vector2.zero;
            bsRect.offsetMax = Vector2.zero;

            var cScript = customGO.AddComponent<IUHCustomizationUI>();
            cScript.Setup(customGO, backBtn.GetComponent<Button>(), btnStart.GetComponent<Button>(), btnPortal.GetComponent<Button>(), btnXanhSM, btnGrab, btnBe, btnDefault, txtXanhSM, txtGrab, txtBe, txtDefault);
            customGO.SetActive(true); // Open upon entering game so player can choose costume before playing!
        }

        private static (GameObject, Button, Text) BuildBrandCard(Transform parent, string title, string tagline, string desc, string stats, Color brandCol)
        {
            var cardGO = new GameObject($"Card_{title}");
            cardGO.transform.SetParent(parent, false);

            var img = cardGO.AddComponent<Image>();
            img.color = new Color(0.05f, 0.10f, 0.18f, 0.95f);
            var outline = cardGO.AddComponent<Outline>();
            outline.effectColor = brandCol;
            outline.effectDistance = new Vector2(2f, 2f);

            var vlg = cardGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 18, 18);
            vlg.spacing = 10f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Brand Banner
            var bannerGO = new GameObject("Banner");
            bannerGO.transform.SetParent(cardGO.transform, false);
            var bImg = bannerGO.AddComponent<Image>();
            bImg.color = brandCol;
            bannerGO.AddComponent<LayoutElement>().preferredHeight = 50f;
            var bTxt = new GameObject("Txt").AddComponent<Text>();
            bTxt.transform.SetParent(bannerGO.transform, false);
            bTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bTxt.text = $"<b>{title}</b>";
            bTxt.fontSize = 22;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;
            var btRect = bTxt.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;

            // Tagline
            var tagGO = new GameObject("Tagline");
            tagGO.transform.SetParent(cardGO.transform, false);
            var tagTxt = tagGO.AddComponent<Text>();
            tagTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tagTxt.text = $"<i>{tagline}</i>";
            tagTxt.fontSize = 14;
            tagTxt.alignment = TextAnchor.MiddleCenter;
            tagTxt.color = new Color(0.85f, 0.9f, 0.98f);
            tagGO.AddComponent<LayoutElement>().preferredHeight = 24f;

            // Description
            var descGO = new GameObject("Desc");
            descGO.transform.SetParent(cardGO.transform, false);
            var descTxt = descGO.AddComponent<Text>();
            descTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descTxt.text = desc;
            descTxt.fontSize = 14;
            descTxt.lineSpacing = 1.2f;
            descTxt.color = Color.white;
            descGO.AddComponent<LayoutElement>().preferredHeight = 65f;

            // Bonus Stats Box
            var statsGO = new GameObject("StatsBox");
            statsGO.transform.SetParent(cardGO.transform, false);
            var sImg = statsGO.AddComponent<Image>();
            sImg.color = new Color(0.02f, 0.05f, 0.10f, 0.85f);
            statsGO.AddComponent<LayoutElement>().preferredHeight = 85f;
            var sTxt = new GameObject("Txt").AddComponent<Text>();
            sTxt.transform.SetParent(statsGO.transform, false);
            sTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sTxt.text = stats;
            sTxt.fontSize = 14;
            sTxt.lineSpacing = 1.25f;
            sTxt.alignment = TextAnchor.MiddleCenter;
            sTxt.color = new Color(0.35f, 0.95f, 0.65f);
            var stRect = sTxt.GetComponent<RectTransform>();
            stRect.anchorMin = Vector2.zero;
            stRect.anchorMax = Vector2.one;

            // Select Button
            var btnGO = CreateMenuButton(cardGO.transform, "BtnSelect", "○  Chọn", brandCol, 16);
            var btn = btnGO.GetComponent<Button>();
            var txt = btnGO.transform.Find("Txt")?.GetComponent<Text>();

            return (cardGO, btn, txt);
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
            vcRect.anchorMin = new Vector2(0.28f, 0.25f);
            vcRect.anchorMax = new Vector2(0.72f, 0.75f);
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
            vtRect.anchorMin = new Vector2(0.05f, 0.78f);
            vtRect.anchorMax = new Vector2(0.95f, 0.96f);
            vtRect.offsetMin = Vector2.zero;
            vtRect.offsetMax = Vector2.zero;
            var vt = vTitle.AddComponent<Text>();
            vt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vt.text = "🎉 GIAO HÀNG THÀNH CÔNG!";
            vt.fontSize = 26;
            vt.alignment = TextAnchor.MiddleCenter;
            vt.color = Color.white;
            vt.fontStyle = FontStyle.Bold;

            var vRating = new GameObject("TxtRating");
            vRating.transform.SetParent(vCard.transform, false);
            var vrRect = vRating.AddComponent<RectTransform>();
            vrRect.anchorMin = new Vector2(0.05f, 0.62f);
            vrRect.anchorMax = new Vector2(0.95f, 0.78f);
            vrRect.offsetMin = Vector2.zero;
            vrRect.offsetMax = Vector2.zero;
            var vr = vRating.AddComponent<Text>();
            vr.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vr.text = "⭐⭐⭐ (3/3 Sao)";
            vr.fontSize = 22;
            vr.alignment = TextAnchor.MiddleCenter;
            vr.color = Color.yellow;

            var vReward = new GameObject("TxtReward");
            vReward.transform.SetParent(vCard.transform, false);
            var vrwRect = vReward.AddComponent<RectTransform>();
            vrwRect.anchorMin = new Vector2(0.05f, 0.40f);
            vrwRect.anchorMax = new Vector2(0.95f, 0.62f);
            vrwRect.offsetMin = Vector2.zero;
            vrwRect.offsetMax = Vector2.zero;
            var vrw = vReward.AddComponent<Text>();
            vrw.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vrw.text = "💰 +75,000 VNĐ  •  ⭐ +150 EXP\n📦 Thưởng nguyên vẹn (100%): +15,000 VNĐ";
            vrw.fontSize = 17;
            vrw.alignment = TextAnchor.MiddleCenter;
            vrw.color = new Color(0.85f, 0.95f, 0.9f);

            var vClue = new GameObject("TxtClue");
            vClue.transform.SetParent(vCard.transform, false);
            var vclRect = vClue.AddComponent<RectTransform>();
            vclRect.anchorMin = new Vector2(0.05f, 0.22f);
            vclRect.anchorMax = new Vector2(0.95f, 0.38f);
            vclRect.offsetMin = Vector2.zero;
            vclRect.offsetMax = Vector2.zero;
            var vcl = vClue.AddComponent<Text>();
            vcl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vcl.text = "📜 Manh mối nhận được: [Mảnh nhật ký tại Ký Túc Xá Block D]";
            vcl.fontSize = 15;
            vcl.alignment = TextAnchor.MiddleCenter;
            vcl.color = new Color(1.0f, 0.85f, 0.35f);

            var btnCont = CreateMenuButton(vCard.transform, "BtnContinue", "TIẾP TỤC  [ M ]", new Color(0.0f, 0.68f, 0.32f), 18);
            var bcRect = btnCont.GetComponent<RectTransform>();
            bcRect.anchorMin = new Vector2(0.25f, 0.05f);
            bcRect.anchorMax = new Vector2(0.75f, 0.18f);
            bcRect.offsetMin = Vector2.zero;
            bcRect.offsetMax = Vector2.zero;

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
            dcRect.anchorMin = new Vector2(0.30f, 0.3f);
            dcRect.anchorMax = new Vector2(0.70f, 0.7f);
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

            var dBtn = CreateMenuButton(dCard.transform, "BtnRetry", "THỬ LẠI NGAY", new Color(0.85f, 0.25f, 0.25f), 18);
            var dbRect = dBtn.GetComponent<RectTransform>();
            dbRect.anchorMin = new Vector2(0.3f, 0.08f);
            dbRect.anchorMax = new Vector2(0.7f, 0.24f);
            dbRect.offsetMin = Vector2.zero;
            dbRect.offsetMax = Vector2.zero;

            defGO.SetActive(false);
        }

        private static Material GetOrCreateMaterial(string matName, Color color, float metallic = 0.1f, float smoothness = 0.5f, bool transparent = false, Color? emissionColor = null)
        {
            if (!Directory.Exists(MatDir)) Directory.CreateDirectory(MatDir);
            string path = MatDir + matName + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);

            if (transparent)
            {
                mat.SetFloat("_Surface", 1);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }

            if (emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor.Value);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}

