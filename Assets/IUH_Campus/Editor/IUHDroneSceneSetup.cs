#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using IUHCampus.Guardian;
using StarterAssets;

namespace IUHCampus.Editor
{
    public static class IUHDroneSceneSetup
    {
        [MenuItem("IUH Campus/Setup Guardian Drone System")]
        public static void SetupDroneInScene()
        {
            var existing = GameObject.Find("IUH_GUARDIAN_DRONE");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            Material darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Sign_Dark.mat");
            if (darkMat == null) darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Starter Assets/Sample/Environment/Art/Materials/GreyBlue_Mat.mat");

            Material chromeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Int_Chrome.mat");
            Material holoMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Waypoint_Holo.mat");
            if (holoMat == null) holoMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/SharedCampusConcept/Materials/Festival LED.mat");

            Material blueMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_BlueFacade.mat");
            if (blueMat == null) blueMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/SharedCampusConcept/Materials/IUH blue.mat");

            GameObject droneRoot = new GameObject("IUH_GUARDIAN_DRONE");
            Undo.RegisterCreatedObjectUndo(droneRoot, "Create Guardian Drone");

            var student = GameObject.Find("Campus Student");
            Vector3 spawnPos = student != null ? student.transform.position + new Vector3(1.5f, 2.2f, -1.2f) : new Vector3(0f, 3f, 0f);
            droneRoot.transform.position = spawnPos;

            var droneController = droneRoot.AddComponent<IUHGuardianDroneController>();

            // Fuselage
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Drone_Fuselage";
            body.transform.SetParent(droneRoot.transform, false);
            body.transform.localScale = new Vector3(0.5f, 0.18f, 0.7f);
            if (darkMat != null) body.GetComponent<Renderer>().sharedMaterial = darkMat;

            // Canopy
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Drone_Canopy";
            canopy.transform.SetParent(droneRoot.transform, false);
            canopy.transform.localPosition = new Vector3(0f, 0.09f, 0.05f);
            canopy.transform.localScale = new Vector3(0.38f, 0.16f, 0.48f);
            if (blueMat != null) canopy.GetComponent<Renderer>().sharedMaterial = blueMat;

            // Gimbal
            GameObject gimbal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gimbal.name = "Camera_Gimbal";
            gimbal.transform.SetParent(droneRoot.transform, false);
            gimbal.transform.localPosition = new Vector3(0f, -0.04f, 0.38f);
            gimbal.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
            if (chromeMat != null) gimbal.GetComponent<Renderer>().sharedMaterial = chromeMat;

            // Lens
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lens.name = "Camera_Lens";
            lens.transform.SetParent(gimbal.transform, false);
            lens.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lens.transform.localScale = new Vector3(0.6f, 0.15f, 0.6f);
            if (holoMat != null) lens.GetComponent<Renderer>().sharedMaterial = holoMat;

            // FPV Cam
            GameObject fpvCamObj = new GameObject("Drone_FPV_Camera");
            fpvCamObj.transform.SetParent(gimbal.transform, false);
            fpvCamObj.transform.localPosition = new Vector3(0f, 0f, 0.12f);
            Camera fpvCam = fpvCamObj.AddComponent<Camera>();
            fpvCam.nearClipPlane = 0.05f;
            fpvCam.fieldOfView = 75f;
            fpvCam.enabled = false;

            // TPP Cam
            GameObject tppCamObj = new GameObject("Drone_TPP_Camera");
            tppCamObj.transform.SetParent(droneRoot.transform, false);
            tppCamObj.transform.localPosition = new Vector3(0f, 1.4f, -3.5f);
            tppCamObj.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            Camera tppCam = tppCamObj.AddComponent<Camera>();
            tppCam.nearClipPlane = 0.1f;
            tppCam.fieldOfView = 65f;
            tppCam.enabled = false;

            // 4 Rotor Arms & Propellers
            List<IUHRotorSpinner> rotorSpinners = new List<IUHRotorSpinner>();
            Vector3[] armOffsets = new Vector3[]
            {
                new Vector3( 0.45f, 0.04f,  0.45f),
                new Vector3(-0.45f, 0.04f,  0.45f),
                new Vector3( 0.45f, 0.04f, -0.45f),
                new Vector3(-0.45f, 0.04f, -0.45f)
            };

            for (int i = 0; i < 4; i++)
            {
                Vector3 armPos = armOffsets[i];
                GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arm.name = $"RotorArm_{i}";
                arm.transform.SetParent(droneRoot.transform, false);
                arm.transform.localPosition = armPos * 0.5f;
                arm.transform.localRotation = Quaternion.LookRotation(armPos) * Quaternion.Euler(90f, 0f, 0f);
                arm.transform.localScale = new Vector3(0.04f, armPos.magnitude * 0.5f, 0.04f);
                if (darkMat != null) arm.GetComponent<Renderer>().sharedMaterial = darkMat;

                GameObject motor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                motor.name = $"MotorPod_{i}";
                motor.transform.SetParent(droneRoot.transform, false);
                motor.transform.localPosition = armPos;
                motor.transform.localScale = new Vector3(0.12f, 0.06f, 0.12f);
                if (chromeMat != null) motor.GetComponent<Renderer>().sharedMaterial = chromeMat;

                GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prop.name = $"Propeller_{i}";
                prop.transform.SetParent(motor.transform, false);
                prop.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                prop.transform.localScale = new Vector3(3.8f, 0.15f, 0.35f);
                if (blueMat != null) prop.GetComponent<Renderer>().sharedMaterial = blueMat;

                var spinner = prop.AddComponent<IUHRotorSpinner>();
                rotorSpinners.Add(spinner);

                GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                led.name = $"LED_Nav_{i}";
                led.transform.SetParent(motor.transform, false);
                led.transform.localPosition = new Vector3(0f, -0.6f, 0f);
                led.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                if (holoMat != null) led.GetComponent<Renderer>().sharedMaterial = holoMat;
            }

            // Glowing Hologram Neon Cargo Box
            GameObject neonCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonCube.name = "Neon_Hologram_Box";
            neonCube.transform.SetParent(droneRoot.transform, false);
            neonCube.transform.localPosition = new Vector3(0f, -0.32f, 0f);
            neonCube.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            Renderer neonRenderer = neonCube.GetComponent<Renderer>();
            if (holoMat != null) neonRenderer.sharedMaterial = holoMat;

            // Spotlight
            GameObject spotObj = new GameObject("Inspection_Spotlight");
            spotObj.transform.SetParent(droneRoot.transform, false);
            spotObj.transform.localPosition = new Vector3(0f, -0.1f, 0.25f);
            spotObj.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            Light spotLight = spotObj.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.color = new Color(0.85f, 0.95f, 1f);
            spotLight.intensity = 2.5f;
            spotLight.range = 45f;
            spotLight.spotAngle = 60f;

            // Setup Drone HUD Canvas
            IUHDroneHUD hud = CreateDroneHUDCanvas(droneRoot);

            // Serialized properties
            SerializedObject so = new SerializedObject(droneController);
            so.FindProperty("m_FPVCamera").objectReferenceValue = fpvCam;
            so.FindProperty("m_TPPCamera").objectReferenceValue = tppCam;
            so.FindProperty("m_Spotlight").objectReferenceValue = spotLight;
            so.FindProperty("m_NeonCargoBox").objectReferenceValue = neonCube;
            so.FindProperty("m_NeonBoxRenderer").objectReferenceValue = neonRenderer;
            so.FindProperty("m_DroneHUD").objectReferenceValue = hud;

            SerializedProperty rotorsProp = so.FindProperty("m_Rotors");
            rotorsProp.ClearArray();
            for (int i = 0; i < rotorSpinners.Count; i++)
            {
                rotorsProp.InsertArrayElementAtIndex(i);
                rotorsProp.GetArrayElementAtIndex(i).objectReferenceValue = rotorSpinners[i];
            }

            if (student != null)
            {
                so.FindProperty("m_PlayerTransform").objectReferenceValue = student.transform;
                var tpc = student.GetComponent<ThirdPersonController>();
                if (tpc != null) so.FindProperty("m_ThirdPersonController").objectReferenceValue = tpc;
                var inp = student.GetComponent<StarterAssetsInputs>();
                if (inp != null) so.FindProperty("m_StarterInputs").objectReferenceValue = inp;
            }
            so.ApplyModifiedProperties();

            Debug.Log("<color=cyan>[IUH Drone]</color> Guardian Drone setup completed successfully!");
            Selection.activeGameObject = droneRoot;
        }

        private static IUHDroneHUD CreateDroneHUDCanvas(GameObject droneRoot)
        {
            var oldHUD = GameObject.Find("IUH_DRONE_HUD_CANVAS");
            if (oldHUD != null) Undo.DestroyObjectImmediate(oldHUD);

            GameObject hudObj = new GameObject("IUH_DRONE_HUD_CANVAS");
            Canvas canvas = hudObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            CanvasScaler scaler = hudObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            hudObj.AddComponent<GraphicRaycaster>();
            IUHDroneHUD droneHUD = hudObj.AddComponent<IUHDroneHUD>();

            GameObject tintObj = new GameObject("VisionTint");
            tintObj.transform.SetParent(hudObj.transform, false);
            Image tintImg = tintObj.AddComponent<Image>();
            tintImg.color = new Color(0, 0, 0, 0);
            RectTransform tintRT = tintImg.rectTransform;
            tintRT.anchorMin = Vector2.zero;
            tintRT.anchorMax = Vector2.one;
            tintRT.sizeDelta = Vector2.zero;

            GameObject flashObj = new GameObject("FlashOverlay");
            flashObj.transform.SetParent(hudObj.transform, false);
            Image flashImg = flashObj.AddComponent<Image>();
            flashImg.color = new Color(1, 1, 1, 0);
            flashImg.raycastTarget = false;
            RectTransform flashRT = flashImg.rectTransform;
            flashRT.anchorMin = Vector2.zero;
            flashRT.anchorMax = Vector2.one;
            flashRT.sizeDelta = Vector2.zero;

            // Top Telemetry Bar
            GameObject topBar = new GameObject("TopTelemetryBar");
            topBar.transform.SetParent(hudObj.transform, false);
            Image topBarBg = topBar.AddComponent<Image>();
            topBarBg.color = new Color(0.04f, 0.08f, 0.15f, 0.75f);
            RectTransform topBarRT = topBarBg.rectTransform;
            topBarRT.anchorMin = new Vector2(0f, 1f);
            topBarRT.anchorMax = new Vector2(1f, 1f);
            topBarRT.pivot = new Vector2(0.5f, 1f);
            topBarRT.anchoredPosition = Vector2.zero;
            topBarRT.sizeDelta = new Vector2(0, 60);

            Text altText = CreateText(topBar.transform, "AltText", "ALT: 0.0 m", 22, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(40, 0), new Vector2(200, 50));
            altText.color = Color.cyan;

            Text spdText = CreateText(topBar.transform, "SpdText", "SPD: 0.0 km/h", 22, TextAnchor.MiddleLeft, new Vector2(0.25f, 0.5f), new Vector2(0, 0), new Vector2(200, 50));
            spdText.color = Color.white;

            Text batText = CreateText(topBar.transform, "BatText", "BAT: 100%", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 50));
            batText.color = Color.cyan;

            Text cmpText = CreateText(topBar.transform, "CompassText", "N 0°", 22, TextAnchor.MiddleRight, new Vector2(0.75f, 0.5f), Vector2.zero, new Vector2(180, 50));
            cmpText.color = Color.yellow;

            Text visionText = CreateText(topBar.transform, "VisionText", "MẮT THẦN: [BÌNH THƯỜNG]", 20, TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(-40, 0), new Vector2(280, 50));
            visionText.color = Color.white;

            // Cinematic Status Banner (Center top)
            Text cineText = CreateText(hudObj.transform, "CinematicStatusText", "🎬 FLYCAM 360° ĐANG QUAY: 50%", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(700, 60));
            cineText.color = Color.yellow;
            cineText.fontStyle = FontStyle.Bold;
            cineText.gameObject.SetActive(false);

            // Center Crosshair
            GameObject crosshair = new GameObject("CenterCrosshair");
            crosshair.transform.SetParent(hudObj.transform, false);
            RectTransform chRT = crosshair.AddComponent<RectTransform>();
            chRT.anchorMin = new Vector2(0.5f, 0.5f);
            chRT.anchorMax = new Vector2(0.5f, 0.5f);
            chRT.anchoredPosition = Vector2.zero;
            chRT.sizeDelta = new Vector2(90, 90);
            Image chImg = crosshair.AddComponent<Image>();
            chImg.color = new Color(0.2f, 0.9f, 1f, 0.5f);

            Text targetText = CreateText(hudObj.transform, "TargetInfoText", "QUÉT TRINH SÁT: [KHÔNG CÓ MỤC TIÊU]", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(600, 40));

            // Controls Guide at bottom
            GameObject botBar = new GameObject("BottomGuideBar");
            botBar.transform.SetParent(hudObj.transform, false);
            Image botBarBg = botBar.AddComponent<Image>();
            botBarBg.color = new Color(0.04f, 0.08f, 0.15f, 0.85f);
            RectTransform botBarRT = botBarBg.rectTransform;
            botBarRT.anchorMin = new Vector2(0f, 0f);
            botBarRT.anchorMax = new Vector2(1f, 0f);
            botBarRT.pivot = new Vector2(0.5f, 0f);
            botBarRT.anchoredPosition = Vector2.zero;
            botBarRT.sizeDelta = new Vector2(0, 55);

            Text guideText = CreateText(botBar.transform, "GuideText",
                "[W/A/S/D] Bay  |  [Space/Ctrl] Cao độ  |  [G] Tiếp tế Nano  |  [B] Bom EMP  |  [Z/X] Flycam  |  [F] Chụp ảnh  |  [U] Về Sinh viên",
                18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500, 50));
            guideText.color = new Color(0.85f, 0.95f, 1f);

            // Right side Action Buttons
            CreateButton(hudObj.transform, "Btn_Capture", "📸 CHỤP ẢNH [F]", new Vector2(1f, 0.5f), new Vector2(-120, 150), new Vector2(200, 50), new Color(0.15f, 0.6f, 0.95f), () => droneHUD.OnClickCapturePhoto());
            CreateButton(hudObj.transform, "Btn_Airdrop", "📦 TIẾP TẾ [G]", new Vector2(1f, 0.5f), new Vector2(-120, 90), new Vector2(200, 50), new Color(0.1f, 0.75f, 0.4f), () => droneHUD.OnClickDropSupply());
            CreateButton(hudObj.transform, "Btn_EMP", "💥 BOM EMP [B]", new Vector2(1f, 0.5f), new Vector2(-120, 30), new Vector2(200, 50), new Color(0.8f, 0.2f, 0.85f), () => droneHUD.OnClickDropEmp());
            CreateButton(hudObj.transform, "Btn_Cinematic", "🎬 FLYCAM [C]", new Vector2(1f, 0.5f), new Vector2(-120, -30), new Vector2(200, 50), new Color(0.9f, 0.6f, 0.1f), () => droneHUD.OnClickToggleCinematic());
            CreateButton(hudObj.transform, "Btn_Vision", "👁️ MẮT THẦN [T]", new Vector2(1f, 0.5f), new Vector2(-120, -90), new Vector2(200, 50), new Color(0.55f, 0.2f, 0.75f), () => droneHUD.OnClickToggleVision());
            CreateButton(hudObj.transform, "Btn_Camera", "🔄 GÓC NHÌN [V]", new Vector2(1f, 0.5f), new Vector2(-120, -150), new Vector2(200, 50), new Color(0.2f, 0.5f, 0.65f), () => droneHUD.OnClickToggleCameraView());
            CreateButton(hudObj.transform, "Btn_Exit", "🔙 THU HỒI [U]", new Vector2(1f, 0.5f), new Vector2(-120, -210), new Vector2(200, 50), new Color(0.8f, 0.25f, 0.2f), () => droneHUD.OnClickReturnToPlayer());

            // Recon Report Popup Panel
            GameObject reportPanel = new GameObject("ReconReportPanel");
            reportPanel.transform.SetParent(hudObj.transform, false);
            Image reportBg = reportPanel.AddComponent<Image>();
            reportBg.color = new Color(0.06f, 0.12f, 0.22f, 0.95f);
            RectTransform reportRT = reportBg.rectTransform;
            reportRT.anchorMin = new Vector2(0.5f, 0.5f);
            reportRT.anchorMax = new Vector2(0.5f, 0.5f);
            reportRT.anchoredPosition = new Vector2(0, 80);
            reportRT.sizeDelta = new Vector2(560, 220);

            Text reportTitle = CreateText(reportPanel.transform, "ReportTitle", "📸 BÁO CÁO TRINH SÁT DRONE", 24, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(500, 45));
            reportTitle.color = Color.yellow;
            reportTitle.fontStyle = FontStyle.Bold;

            Text reportDesc = CreateText(reportPanel.transform, "ReportDesc", "Nội dung báo cáo...", 19, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(500, 110));
            reportDesc.color = Color.white;

            // Wire up SerializedObject for HUD
            SerializedObject so = new SerializedObject(droneHUD);
            so.FindProperty("m_DroneOSDCanvas").objectReferenceValue = canvas;
            so.FindProperty("m_AltitudeText").objectReferenceValue = altText;
            so.FindProperty("m_SpeedText").objectReferenceValue = spdText;
            so.FindProperty("m_BatteryText").objectReferenceValue = batText;
            so.FindProperty("m_CompassText").objectReferenceValue = cmpText;
            so.FindProperty("m_TargetInfoText").objectReferenceValue = targetText;
            so.FindProperty("m_VisionModeText").objectReferenceValue = visionText;
            so.FindProperty("m_CinematicStatusText").objectReferenceValue = cineText;
            so.FindProperty("m_VisionTintImage").objectReferenceValue = tintImg;
            so.FindProperty("m_FlashImage").objectReferenceValue = flashImg;
            so.FindProperty("m_ReportPopupPanel").objectReferenceValue = reportPanel;
            so.FindProperty("m_ReportPopupTitle").objectReferenceValue = reportTitle;
            so.FindProperty("m_ReportPopupDesc").objectReferenceValue = reportDesc;
            so.FindProperty("m_CrosshairReticle").objectReferenceValue = crosshair;
            so.FindProperty("m_StudentLaunchDroneButton").objectReferenceValue = null;
            so.ApplyModifiedProperties();

            return droneHUD;
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform rt = txt.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return txt;
        }

        private static GameObject CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            RectTransform rt = img.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = CreateText(btnObj.transform, "Label", label, 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;

            return btnObj;
        }
    }
}
#endif
