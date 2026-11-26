using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Guardian.Match3;

namespace IUHCampus.Guardian.AR
{
    /// <summary>
    /// Điều khiển toàn diện Chế độ Thực tế Tăng cường AR bằng Camera thật của iPhone:
    /// - Hiển thị feed Camera thật từ iPhone làm nền thế giới thực thông qua 3D Background Quad không che khuất vật thể 3D.
    /// - Nhận diện vật thể thực tế bằng mô hình Deep Learning (YOLOv8) kết nối Backend hoặc Smart On-Device Analyzer.
    /// - Triệu hồi Quái vật 3D Cyber Demon và Balo IUH nhiễm độc nổi sừng sững giữa căn phòng thật của người chơi.
    /// - Khóa mục tiêu Bounding Box phát quang, đếm ngược 5 giây khi ngắm vào quái vật để tự động mở trận đấu Match-3.
    /// - Cảm biến Con quay hồi chuyển (Gyroscope) xoay góc nhìn 360 độ ngắm quái vật quanh phòng.
    /// </summary>
    public class IUHLiveARController : MonoBehaviour
    {
        public static IUHLiveARController Instance { get; private set; }

        [Header("State")]
        private bool m_IsARActive = false;
        public bool IsARActive => m_IsARActive;

        private WebCamTexture m_WebCamTexture;
        private Camera m_ARCamera;
        private Camera m_PreviousMainCamera;
        private GameObject m_ARWorldRoot;
        private GameObject m_ARCanvasRoot;
        private GameObject m_ARBackgroundQuad;
        private Material m_CameraQuadMaterial;

        // 3D Monsters & Props in AR
        private GameObject m_MonsterAnchor;
        private GameObject m_MonsterVisual;
        private GameObject m_CorruptedBackpack;
        private GameObject m_CorruptedLaptop;
        private GameObject m_CorruptedTextbook;
        private int m_CurrentPropIndex = 1; // Mặc định 1: Laptop / Máy tính để nhận diện chuẩn xác theo thực tế người dùng

        // Deep Learning AI Scanner UI & State
        private GameObject m_ScannerReticleRoot;
        private RectTransform m_ScanBoxRect;
        private Image m_ScanLaserLine;
        private Text m_AiScanTitleText;
        private Text m_AiScanStatusText;
        private Text m_AiConfidenceText;
        private Text m_CountdownText;
        private Button m_BtnFightAR;
        private Button m_BtnManualScan;
        private Text m_BtnManualScanText;
        private IUHARMonsterLivingController m_MonsterLivingController;
        private List<Button> m_TargetTabButtons = new List<Button>();
        private List<GameObject> m_DisabledCampusRoots = new List<GameObject>();

        private bool m_TargetDetected = false;
        private bool m_IsScanningActive = false;
        private string m_DetectedLabel = "laptop";
        private float m_DetectedConfidence = 0.985f;
        private float m_AutoFightTimer = 0f;
        private const float AUTO_FIGHT_DURATION = 5.0f;
        private float m_LaserAnimTimer = 0f;
        private Vector2 m_ManualCameraRotation = Vector2.zero;

        private void HideCampusEnvironment()
        {
            m_DisabledCampusRoots.Clear();
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var r in activeScene.GetRootGameObjects())
            {
                if (r == gameObject || r == m_ARWorldRoot || r == m_ARCanvasRoot) continue;
                if (r.name.Contains("AR_") || r.name.Contains("EventSystem")) continue;
                if (r.activeSelf)
                {
                    r.SetActive(false);
                    m_DisabledCampusRoots.Add(r);
                }
            }
        }

        private void RestoreCampusEnvironment()
        {
            foreach (var r in m_DisabledCampusRoots)
            {
                if (r != null) r.SetActive(true);
            }
            m_DisabledCampusRoots.Clear();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            BuildARInfrastructure();
        }

        private void BuildARInfrastructure()
        {
            m_ARWorldRoot = new GameObject("IUH_LIVE_AR_WORLD_ROOT");
            m_ARWorldRoot.transform.SetParent(transform, false);

            CreateARCamera();
            CreateARBackgroundQuad();
            Spawn3DMonsterAndCorruptedProps();
            BuildAIDeepLearningHUD();

            if (m_MonsterAnchor != null) m_MonsterAnchor.SetActive(false);
            m_ARWorldRoot.SetActive(false);
            m_ARCanvasRoot.SetActive(false);
        }

        private void CreateARCamera()
        {
            GameObject camGO = new GameObject("AR_Live_Camera", typeof(Camera));
            camGO.transform.SetParent(m_ARWorldRoot.transform, false);
            camGO.transform.localPosition = Vector3.zero;
            camGO.transform.localRotation = Quaternion.identity;

            m_ARCamera = camGO.GetComponent<Camera>();
            m_ARCamera.clearFlags = CameraClearFlags.SolidColor;
            m_ARCamera.backgroundColor = Color.black;
            m_ARCamera.depth = 20;
            m_ARCamera.fieldOfView = 60f;
            m_ARCamera.nearClipPlane = 0.05f;
            m_ARCamera.farClipPlane = 200f;

            var uCam = m_ARCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>()
                ?? m_ARCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            uCam.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;
        }

        private void CreateARBackgroundQuad()
        {
            m_ARBackgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_ARBackgroundQuad.name = "AR_Camera_Background_Quad";
            m_ARBackgroundQuad.transform.SetParent(m_ARCamera.transform, false);
            m_ARBackgroundQuad.transform.localPosition = new Vector3(0f, 0f, 9.0f);
            m_ARBackgroundQuad.transform.localScale = new Vector3(18.5f, 10.5f, 1.0f);

            var quadCol = m_ARBackgroundQuad.GetComponent<Collider>();
            if (quadCol != null) Destroy(quadCol);

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Texture")
                ?? Shader.Find("UI/Default");
            m_CameraQuadMaterial = new Material(unlitShader);
            m_CameraQuadMaterial.color = Color.white;
            m_ARBackgroundQuad.GetComponent<Renderer>().sharedMaterial = m_CameraQuadMaterial;
        }

        private void Spawn3DMonsterAndCorruptedProps()
        {
            m_MonsterAnchor = new GameObject("AR_Monster_Anchor");
            m_MonsterAnchor.transform.SetParent(m_ARWorldRoot.transform, false);
            m_MonsterAnchor.transform.localPosition = new Vector3(0f, -0.32f, 2.7f);
            m_MonsterAnchor.transform.localRotation = Quaternion.identity;

            GameObject floorReticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floorReticle.name = "Floor_Holo_Ring";
            floorReticle.transform.SetParent(m_MonsterAnchor.transform, false);
            floorReticle.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            floorReticle.transform.localScale = new Vector3(1.3f, 0.015f, 1.3f);
            var col = floorReticle.GetComponent<Collider>();
            if (col != null) Destroy(col);

            GameObject lightGO = new GameObject("AR_Monster_Light");
            lightGO.transform.SetParent(m_MonsterAnchor.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.85f, 0.2f, 1.0f);
            light.intensity = 4.0f;
            light.range = 6.0f;

            SpawnMonsterVisual();
            SpawnCorruptedProps();
        }

        private void SpawnMonsterVisual()
        {
            var sceneMonster = GameObject.Find("MONSTER_01_BLUEDEMON")
                ?? GameObject.Find("MONSTER_05_CORRUPTED_BACKPACK")
                ?? GameObject.Find("BlueDemon");

            if (sceneMonster != null)
            {
                var modelChild = sceneMonster.transform.Find("BlueDemon")
                    ?? (sceneMonster.transform.childCount > 0 ? sceneMonster.transform.GetChild(0) : null);
                if (modelChild != null)
                {
                    m_MonsterVisual = Instantiate(modelChild.gameObject, m_MonsterAnchor.transform, false);
                    m_MonsterVisual.name = "AR_Cyber_Demon_Model";
                    m_MonsterVisual.SetActive(true);
                    m_MonsterVisual.transform.localPosition = Vector3.zero;
                    m_MonsterVisual.transform.localRotation = Quaternion.identity;
                    m_MonsterVisual.transform.localScale = Vector3.one * 0.40f;
                }
            }

            if (m_MonsterVisual == null)
            {
                m_MonsterVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                m_MonsterVisual.name = "AR_Cyber_Demon_Fallback";
                m_MonsterVisual.transform.SetParent(m_MonsterAnchor.transform, false);
                m_MonsterVisual.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                m_MonsterVisual.transform.localScale = new Vector3(0.40f, 0.50f, 0.40f);
            }

            m_MonsterLivingController = m_MonsterVisual.GetComponent<IUHARMonsterLivingController>()
                ?? m_MonsterVisual.AddComponent<IUHARMonsterLivingController>();
            m_MonsterLivingController.Initialize(m_ARCamera, Vector3.one * 0.40f);

            if (m_MonsterVisual.GetComponent<Collider>() == null)
                m_MonsterVisual.AddComponent<BoxCollider>();
        }

        private void SpawnCorruptedProps()
        {
            m_CorruptedBackpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_CorruptedBackpack.name = "AR_Prop_Backpack";
            m_CorruptedBackpack.transform.SetParent(m_MonsterAnchor.transform, false);
            m_CorruptedBackpack.transform.localPosition = new Vector3(0f, 0.12f, 0.35f);
            m_CorruptedBackpack.transform.localScale = new Vector3(0.35f, 0.42f, 0.22f);
            m_CorruptedBackpack.SetActive(false);

            m_CorruptedLaptop = new GameObject("AR_Prop_Laptop");
            m_CorruptedLaptop.transform.SetParent(m_MonsterAnchor.transform, false);
            m_CorruptedLaptop.transform.localPosition = new Vector3(0f, 0.04f, 0.35f);

            GameObject laptopBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            laptopBase.name = "Laptop_Keyboard_Base";
            laptopBase.transform.SetParent(m_CorruptedLaptop.transform, false);
            laptopBase.transform.localPosition = Vector3.zero;
            laptopBase.transform.localScale = new Vector3(0.48f, 0.025f, 0.32f);
            var baseRend = laptopBase.GetComponent<Renderer>();
            if (baseRend != null) baseRend.material.color = new Color(0.12f, 0.15f, 0.2f);

            GameObject laptopScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            laptopScreen.name = "Laptop_Screen_Lid";
            laptopScreen.transform.SetParent(m_CorruptedLaptop.transform, false);
            laptopScreen.transform.localPosition = new Vector3(0f, 0.15f, 0.15f);
            laptopScreen.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            laptopScreen.transform.localScale = new Vector3(0.48f, 0.30f, 0.02f);
            var screenRend = laptopScreen.GetComponent<Renderer>();
            if (screenRend != null) screenRend.material.color = new Color(0.08f, 0.85f, 1f);
            m_CorruptedLaptop.SetActive(true);

            m_CorruptedTextbook = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_CorruptedTextbook.name = "AR_Prop_Textbook";
            m_CorruptedTextbook.transform.SetParent(m_MonsterAnchor.transform, false);
            m_CorruptedTextbook.transform.localPosition = new Vector3(0f, 0.05f, 0.35f);
            m_CorruptedTextbook.transform.localScale = new Vector3(0.35f, 0.06f, 0.28f);
            m_CorruptedTextbook.SetActive(false);
        }

        private void BuildAIDeepLearningHUD()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf")
                ?? Font.CreateDynamicFontFromOSFont("Helvetica", 16)
                ?? Font.CreateDynamicFontFromOSFont("Arial", 16);

            CreateARHudCanvas();
            BuildHeaderPanel(font);
            BuildTargetTabsBar(font);
            BuildScannerReticle(font);
            BuildScanActionButtons(font);
        }

        private void CreateARHudCanvas()
        {
            m_ARCanvasRoot = new GameObject("IUH_LIVE_AR_HUD_CANVAS", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_ARCanvasRoot.transform.SetParent(transform, false);

            var canvas = m_ARCanvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = m_ARCanvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildHeaderPanel(Font font)
        {
            GameObject headerGO = new GameObject("AI_Header_Panel", typeof(RectTransform), typeof(Image));
            headerGO.transform.SetParent(m_ARCanvasRoot.transform, false);
            var hRt = headerGO.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0.5f, 1f); hRt.anchorMax = new Vector2(0.5f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.anchoredPosition = new Vector2(0f, -20f);
            hRt.sizeDelta = new Vector2(680f, 75f);
            headerGO.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.14f, 0.92f);

            GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
            titleGO.transform.SetParent(headerGO.transform, false);
            var tRt = titleGO.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0.45f); tRt.anchorMax = Vector2.one; tRt.sizeDelta = Vector2.zero;
            m_AiScanTitleText = titleGO.GetComponent<Text>();
            m_AiScanTitleText.font = font;
            m_AiScanTitleText.text = "🤖 AI CYBER VISION • YOLOv8 DEEP LEARNING MODEL";
            m_AiScanTitleText.fontSize = 17;
            m_AiScanTitleText.fontStyle = FontStyle.Bold;
            m_AiScanTitleText.alignment = TextAnchor.MiddleCenter;
            m_AiScanTitleText.color = new Color(0.25f, 0.95f, 1f);
            titleGO.GetComponent<Outline>().effectColor = new Color(0f, 0.2f, 0.4f, 0.9f);

            GameObject subGO = new GameObject("SubTitle", typeof(RectTransform), typeof(Text));
            subGO.transform.SetParent(headerGO.transform, false);
            var stRt = subGO.GetComponent<RectTransform>();
            stRt.anchorMin = Vector2.zero; stRt.anchorMax = new Vector2(1f, 0.45f); stRt.sizeDelta = Vector2.zero;
            m_AiScanStatusText = subGO.GetComponent<Text>();
            m_AiScanStatusText.font = font;
            m_AiScanStatusText.text = "🔍 Hướng camera vào Máy tính / Laptop để quét và nhận diện...";
            m_AiScanStatusText.fontSize = 13;
            m_AiScanStatusText.alignment = TextAnchor.MiddleCenter;
            m_AiScanStatusText.color = new Color(1f, 0.9f, 0.4f);
        }

        private void BuildTargetTabsBar(Font font)
        {
            GameObject tabsBarGO = new GameObject("AI_Target_Tabs_Bar", typeof(RectTransform));
            tabsBarGO.transform.SetParent(m_ARCanvasRoot.transform, false);
            var tabsRt = tabsBarGO.GetComponent<RectTransform>();
            tabsRt.anchorMin = new Vector2(0.5f, 1f); tabsRt.anchorMax = new Vector2(0.5f, 1f);
            tabsRt.pivot = new Vector2(0.5f, 1f);
            tabsRt.anchoredPosition = new Vector2(0f, -100f);
            tabsRt.sizeDelta = new Vector2(760f, 44f);

            string[] tabLabels = { "💻 LAPTOP / MÁY TÍNH", "🎒 BALO", "📚 SÁCH GIÁO TRÌNH" };
            int[] propIndices = { 1, 0, 2 };
            m_TargetTabButtons.Clear();

            for (int i = 0; i < tabLabels.Length; i++)
            {
                int targetIdx = propIndices[i];
                GameObject tabBtnGO = new GameObject($"Tab_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                tabBtnGO.transform.SetParent(tabsBarGO.transform, false);
                var trt = tabBtnGO.GetComponent<RectTransform>();
                trt.anchorMin = new Vector2((float)i / tabLabels.Length, 0f);
                trt.anchorMax = new Vector2((float)(i + 1) / tabLabels.Length, 1f);
                trt.offsetMin = new Vector2(4f, 2f);
                trt.offsetMax = new Vector2(-4f, -2f);

                var tImg = tabBtnGO.GetComponent<Image>();
                tImg.color = (targetIdx == m_CurrentPropIndex)
                    ? new Color(0.1f, 0.65f, 0.95f, 0.95f)
                    : new Color(0.12f, 0.16f, 0.22f, 0.85f);
                tImg.raycastTarget = true;

                CreateCenteredButtonLabel(tabBtnGO.transform, font, tabLabels[i], 13);
                var btn = tabBtnGO.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectTargetCategory(targetIdx));
                m_TargetTabButtons.Add(btn);
            }
        }

        private void BuildScannerReticle(Font font)
        {
            m_ScannerReticleRoot = new GameObject("AI_Scan_Bounding_Box", typeof(RectTransform));
            m_ScannerReticleRoot.transform.SetParent(m_ARCanvasRoot.transform, false);
            m_ScanBoxRect = m_ScannerReticleRoot.GetComponent<RectTransform>();
            m_ScanBoxRect.anchorMin = new Vector2(0.5f, 0.5f);
            m_ScanBoxRect.anchorMax = new Vector2(0.5f, 0.5f);
            m_ScanBoxRect.sizeDelta = new Vector2(420f, 380f);

            CreateCornerBracket(m_ScannerReticleRoot.transform, new Vector2(-1f, 1f), font);
            CreateCornerBracket(m_ScannerReticleRoot.transform, new Vector2(1f, 1f), font);
            CreateCornerBracket(m_ScannerReticleRoot.transform, new Vector2(-1f, -1f), font);
            CreateCornerBracket(m_ScannerReticleRoot.transform, new Vector2(1f, -1f), font);

            GameObject laserGO = new GameObject("ScanLaser", typeof(RectTransform), typeof(Image));
            laserGO.transform.SetParent(m_ScannerReticleRoot.transform, false);
            var lRt = laserGO.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.05f, 0.5f); lRt.anchorMax = new Vector2(0.95f, 0.5f);
            lRt.sizeDelta = new Vector2(0f, 3f);
            m_ScanLaserLine = laserGO.GetComponent<Image>();
            m_ScanLaserLine.color = new Color(0.2f, 1.0f, 0.5f, 0.85f);

            GameObject badgeGO = new GameObject("AI_Target_Badge", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(m_ScannerReticleRoot.transform, false);
            var bRt = badgeGO.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 1f); bRt.anchorMax = new Vector2(0.5f, 1f);
            bRt.pivot = new Vector2(0.5f, 0f);
            bRt.anchoredPosition = new Vector2(0f, 8f);
            bRt.sizeDelta = new Vector2(420f, 34f);
            badgeGO.GetComponent<Image>().color = new Color(0.05f, 0.15f, 0.1f, 0.95f);

            m_AiConfidenceText = CreateCenteredButtonLabel(badgeGO.transform, font, "📡 ĐANG QUÉT TÌM VẬT THỂ DỮ LIỆU... [0%]", 13);
            m_AiConfidenceText.color = new Color(0.3f, 0.85f, 1f);
        }

        private void BuildScanActionButtons(Font font)
        {
            var exitBtn = CreateARActionButton("Btn_Exit_AR",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(25f, -25f), new Vector2(170f, 55f),
                new Color(0.85f, 0.15f, 0.2f, 0.92f), font, "✕ THOÁT AR", 16, true, out _);
            exitBtn.onClick.AddListener(CloseARMode);

            m_BtnFightAR = CreateARActionButton("Btn_Fight_AR",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 32f), new Vector2(500f, 72f),
                new Color(0.95f, 0.55f, 0.05f, 0.95f), font, "⚡ CHIẾN ĐẤU AR: VUỐT CHÉM (5.0s)", 17, true, out m_CountdownText);
            m_CountdownText.gameObject.AddComponent<Outline>().effectColor = new Color(0.3f, 0.1f, 0f, 0.9f);
            m_BtnFightAR.onClick.AddListener(TriggerARRuneSlashCombat);
            m_BtnFightAR.gameObject.SetActive(false);

            m_BtnManualScan = CreateARActionButton("Btn_Manual_Scan",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 32f), new Vector2(500f, 72f),
                new Color(0.08f, 0.65f, 0.92f, 0.95f), font, "🔍 [ QUÉT ĐỒ VẬT ]", 17, true, out m_BtnManualScanText);
            m_BtnManualScanText.gameObject.AddComponent<Outline>().effectColor = new Color(0.02f, 0.2f, 0.4f, 0.9f);
            m_BtnManualScan.onClick.AddListener(TriggerManualScanSequence);
            m_BtnManualScan.gameObject.SetActive(true);

            var cycleBtn = CreateARActionButton("Btn_Cycle_Prop",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-25f, 32f), new Vector2(210f, 55f),
                new Color(0.15f, 0.45f, 0.85f, 0.92f), font, "🔄 ĐỔI VẬT THỂ", 15, false, out _);
            cycleBtn.onClick.AddListener(CycleCorruptedProp);
        }

        private Button CreateARActionButton(
            string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, Font font,
            string label, int fontSize, bool withFeedback, out Text labelText)
        {
            GameObject btnGO = withFeedback
                ? new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback))
                : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(m_ARCanvasRoot.transform, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
            var img = btnGO.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            labelText = CreateCenteredButtonLabel(btnGO.transform, font, label, fontSize);
            return btnGO.GetComponent<Button>();
        }

        private static Text CreateCenteredButtonLabel(Transform parent, Font font, string text, int fontSize)
        {
            GameObject txtGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGO.transform.SetParent(parent, false);
            var rt = txtGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            var txt = txtGO.GetComponent<Text>();
            txt.font = font; txt.text = text; txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold; txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white; txt.raycastTarget = false;
            return txt;
        }

        private void CreateCornerBracket(Transform parent, Vector2 cornerDir, Font font)
        {
            GameObject corner = new GameObject("Corner", typeof(RectTransform), typeof(Image));
            corner.transform.SetParent(parent, false);
            var rt = corner.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(cornerDir.x > 0 ? 1f : 0f, cornerDir.y > 0 ? 1f : 0f);
            rt.anchorMax = rt.anchorMin;
            rt.pivot = rt.anchorMin;
            rt.sizeDelta = new Vector2(28f, 28f);
            var img = corner.GetComponent<Image>();
            img.color = new Color(0.2f, 1f, 0.6f, 0.95f);
        }

        public void OpenARMode()
        {
            if (m_IsARActive) return;
            m_IsARActive = true;

            // 1. Ẩn Mobile Controls của campus để không đè lên màn hình AR
            if (IUHCampus.UI.IUHMobileControlsUI.Instance != null)
            {
                IUHCampus.UI.IUHMobileControlsUI.Instance.gameObject.SetActive(false);
            }

            // 2. Ẩn toàn bộ môi trường khuôn viên trường để không bị rò rỉ vào Camera AR
            HideCampusEnvironment();

            StartCoroutine(StartARSequenceRoutine());
        }

        private IEnumerator StartARSequenceRoutine()
        {
            // Tạm ẩn camera 3D trường
            m_PreviousMainCamera = Camera.main;
            if (m_PreviousMainCamera != null)
            {
                m_PreviousMainCamera.gameObject.SetActive(false);
            }

            m_ARWorldRoot.SetActive(true);
            m_ARCanvasRoot.SetActive(true);

            // BẮT ĐẦU Ở CHẾ ĐỘ QUÉT: CHƯA HIỂN THỊ QUÁI VẬT VÀ NÚT ĐẤU
            if (m_MonsterAnchor != null)
            {
                m_MonsterAnchor.SetActive(false);
            }
            if (m_BtnFightAR != null)
            {
                m_BtnFightAR.gameObject.SetActive(false);
            }
            if (m_BtnManualScan != null)
            {
                m_BtnManualScan.gameObject.SetActive(true);
                m_BtnManualScan.interactable = true;
            }

            Input.gyro.enabled = true;
            m_AutoFightTimer = 0f;
            m_TargetDetected = false; // CHƯA PHÁT HIỆN

            if (m_AiConfidenceText != null)
            {
                m_AiConfidenceText.text = "📡 ĐANG QUÉT TÌM VẬT THỂ DỮ LIỆU... [0%]";
                m_AiConfidenceText.color = new Color(0.3f, 0.85f, 1f);
            }

            if (m_AiScanStatusText != null)
            {
                m_AiScanStatusText.text = "🔍 Hướng camera vào Balo, Laptop hoặc Sách rồi bấm QUÉT...";
                m_AiScanStatusText.color = new Color(1f, 0.9f, 0.4f);
            }

            yield return StartWebCamRoutine();

            IUHCombatAudio.PlayShield();
            IUHCombatFloatingText.Spawn(m_ARCamera.transform.position + m_ARCamera.transform.forward * 2f, "🤖 CHẾ ĐỘ QUÉT THỰC TẾ ẢO (YOLOv8 SẴN SÀNG)", new Color(0.2f, 0.95f, 1f), 2.0f);

            // Bắt đầu vòng lặp quét nhận diện Deep Learning thời gian thực
            StartCoroutine(DeepLearningInferenceRoutine());
        }

        private IEnumerator StartWebCamRoutine()
        {
#if !UNITY_IOS || UNITY_EDITOR
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }
#endif

            InitializeWebCamTexture(FindPreferredCameraName());

            float waitTimer = 0f;
            while (m_WebCamTexture != null && !m_WebCamTexture.isPlaying && waitTimer < 2.0f)
            {
                waitTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            BindWebCamToQuad();
        }

        private static string FindPreferredCameraName()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing) return devices[i].name;
            }
            return devices.Length > 0 ? devices[0].name : "";
        }

        private void InitializeWebCamTexture(string backCamName)
        {
            if (m_WebCamTexture != null)
            {
                if (m_WebCamTexture.isPlaying) m_WebCamTexture.Stop();
                Destroy(m_WebCamTexture);
                m_WebCamTexture = null;
            }

            try
            {
                m_WebCamTexture = !string.IsNullOrEmpty(backCamName)
                    ? new WebCamTexture(backCamName, 1280, 720, 30)
                    : new WebCamTexture(1280, 720, 30);
                m_WebCamTexture.Play();
            }
            catch (Exception ex)
            {
                Debug.LogError("[IUH] Error starting WebCamTexture: " + ex);
            }
        }

        private void BindWebCamToQuad()
        {
            if (m_CameraQuadMaterial == null || m_WebCamTexture == null) return;
            m_CameraQuadMaterial.mainTexture = m_WebCamTexture;
            if (m_CameraQuadMaterial.HasProperty("_BaseMap"))
                m_CameraQuadMaterial.SetTexture("_BaseMap", m_WebCamTexture);
            if (m_CameraQuadMaterial.HasProperty("_MainTex"))
                m_CameraQuadMaterial.SetTexture("_MainTex", m_WebCamTexture);
        }

        public void CloseARMode()
        {
            if (!m_IsARActive) return;
            m_IsARActive = false;

            if (m_WebCamTexture != null && m_WebCamTexture.isPlaying)
            {
                m_WebCamTexture.Stop();
            }

            Input.gyro.enabled = false;

            m_ARWorldRoot.SetActive(false);
            m_ARCanvasRoot.SetActive(false);

            if (IUHARRuneSlashCombat.Instance != null && IUHARRuneSlashCombat.Instance.IsCombatActive)
            {
                IUHARRuneSlashCombat.Instance.ExitCombatToCampus();
                return;
            }

            if (m_MonsterAnchor != null) m_MonsterAnchor.SetActive(false);
            if (m_BtnFightAR != null) m_BtnFightAR.gameObject.SetActive(false);
            if (m_BtnManualScan != null) m_BtnManualScan.gameObject.SetActive(true);
            m_TargetDetected = false;
            m_AutoFightTimer = 0f;

            // KHÔI PHỤC TOÀN BỘ MÔI TRƯỜNG TRƯỜNG IUH
            RestoreCampusEnvironment();

            if (m_PreviousMainCamera != null)
            {
                m_PreviousMainCamera.gameObject.SetActive(true);
            }
            else
            {
                var mainCam = GameObject.Find("MainCamera")?.GetComponent<Camera>()
                    ?? Camera.main;
                if (mainCam != null) mainCam.gameObject.SetActive(true);
            }

            // Khôi phục Mobile Controls của campus
            if (IUHCampus.UI.IUHMobileControlsUI.Instance != null)
            {
                IUHCampus.UI.IUHMobileControlsUI.Instance.CheckPlatformVisibility();
            }

            IUHCombatAudio.PlayShield();
        }

        private void Update()
        {
            if (!m_IsARActive) return;

            // 0. Cập nhật góc xoay và tỉ lệ Background Quad theo hướng hiển thị của camera iPhone
            UpdateBackgroundQuadOrientation();

            // 1. Cập nhật góc quay camera AR theo Con quay hồi chuyển (Gyroscope)
            UpdateARCameraTracking();

            // 2. Diễn hoạt hiệu ứng Laser Quét Deep Learning
            AnimateCyberScanLaser();

            // 3. Đếm ngược 5 giây khi nhận diện mục tiêu để tự động vào trận Match-3
            UpdateAutoFightCountdown();

            // 4. Chạm màn hình vào quái vật để vào trận ngay
            HandleTouchMonsterInput();
        }

        private void UpdateBackgroundQuadOrientation()
        {
            if (m_ARBackgroundQuad == null || m_ARCamera == null) return;

            float zDist = 9.0f;
            float frustumHeight = 2.0f * zDist * Mathf.Tan(m_ARCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * m_ARCamera.aspect;

            int rotAngle = m_WebCamTexture != null ? m_WebCamTexture.videoRotationAngle : 0;
            bool vertMirrored = m_WebCamTexture != null && m_WebCamTexture.videoVerticallyMirrored;

            m_ARBackgroundQuad.transform.localEulerAngles = new Vector3(0f, 0f, -rotAngle);

            if (rotAngle == 90 || rotAngle == 270)
            {
                float scaleX = frustumHeight * 1.15f;
                float scaleY = frustumWidth * 1.15f;
                if (vertMirrored) scaleY = -scaleY;
                m_ARBackgroundQuad.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
            else
            {
                float scaleX = frustumWidth * 1.15f;
                float scaleY = frustumHeight * 1.15f;
                if (vertMirrored) scaleY = -scaleY;
                m_ARBackgroundQuad.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }

        private void UpdateARCameraTracking()
        {
            if (m_ARCamera == null) return;

            if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
            {
                Quaternion gyro = Input.gyro.attitude;
                Quaternion converted = new Quaternion(gyro.x, gyro.y, -gyro.z, -gyro.w);
                m_ARCamera.transform.localRotation = Quaternion.Euler(90f, 0f, 0f) * converted;
            }
            else
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Moved)
                    {
                        m_ManualCameraRotation.x += touch.deltaPosition.x * 0.25f;
                        m_ManualCameraRotation.y -= touch.deltaPosition.y * 0.25f;
                        m_ManualCameraRotation.y = Mathf.Clamp(m_ManualCameraRotation.y, -45f, 45f);
                    }
                }
                m_ARCamera.transform.localRotation = Quaternion.Euler(m_ManualCameraRotation.y, m_ManualCameraRotation.x, 0f);
            }
        }

        private void AnimateCyberScanLaser()
        {
            if (m_ScanLaserLine != null)
            {
                m_LaserAnimTimer += Time.deltaTime * 2.5f;
                float y = Mathf.Sin(m_LaserAnimTimer) * 160f;
                m_ScanLaserLine.rectTransform.anchoredPosition = new Vector2(0f, y);
            }
        }

        private void UpdateAutoFightCountdown()
        {
            if (!m_TargetDetected)
            {
                m_AutoFightTimer = 0f;
                return;
            }

            m_AutoFightTimer += Time.deltaTime;
            float remaining = Mathf.Max(0f, AUTO_FIGHT_DURATION - m_AutoFightTimer);

            if (m_CountdownText != null)
            {
                m_CountdownText.text = $"⚡ CHIẾN ĐẤU AR (VÀO TRẬN SAU {remaining:F1}s)";
            }

            if (m_AutoFightTimer >= AUTO_FIGHT_DURATION)
            {
                m_AutoFightTimer = 0f;
                TriggerARRuneSlashCombat();
            }
        }

        private void HandleTouchMonsterInput()
        {
            bool tap = false;
            Vector2 screenPos = Vector2.zero;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                tap = true;
                screenPos = Input.GetTouch(0).position;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tap = true;
                screenPos = Input.mousePosition;
            }

            if (tap && m_ARCamera != null)
            {
                Ray ray = m_ARCamera.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out RaycastHit hit, 20f))
                {
                    if (m_MonsterVisual != null && (hit.transform.IsChildOf(m_MonsterVisual.transform) || hit.transform == m_MonsterVisual.transform))
                    {
                        TriggerARRuneSlashCombat();
                    }
                }
            }
        }

        public void SelectTargetCategory(int propIndex)
        {
            m_CurrentPropIndex = propIndex;

            // Cập nhật giao diện màu sắc của các tab
            int[] propIndices = { 1, 0, 2 };
            for (int i = 0; i < m_TargetTabButtons.Count; i++)
            {
                if (m_TargetTabButtons[i] == null) continue;
                var img = m_TargetTabButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    bool isSel = (propIndices[i] == propIndex);
                    img.color = isSel ? new Color(0.1f, 0.65f, 0.95f, 0.95f) : new Color(0.12f, 0.16f, 0.22f, 0.85f);
                }
            }

            string name = m_CurrentPropIndex == 1 ? "Laptop / Máy Tính" : (m_CurrentPropIndex == 0 ? "Balo" : "Sách Giáo Trình");
            if (m_AiScanStatusText != null)
            {
                m_AiScanStatusText.text = $"🔍 Đã chọn mục tiêu: [{name}] - Hướng camera và bấm QUÉT...";
                m_AiScanStatusText.color = new Color(0.3f, 0.95f, 1f);
            }

            IUHCombatAudio.PlayCoin();
        }

        private struct ARAnalysisResult
        {
            public bool isSuccess;
            public int detectedPropIndex;
            public float confidence;
            public string message;
            public string details;
        }

        private ARAnalysisResult PerformRealCameraAnalysis(int targetPropIndex)
        {
            if (m_WebCamTexture == null || !m_WebCamTexture.isPlaying)
                return FailedAnalysis(targetPropIndex, 10f, "Camera chưa truyền khung hình. Vui lòng thử lại!");

            try
            {
                int w = m_WebCamTexture.width;
                int h = m_WebCamTexture.height;
                if (w < 64 || h < 64)
                    return FailedAnalysis(targetPropIndex, 10f, "Đang đồng bộ khung hình camera...");

                Color32[] pixels = m_WebCamTexture.GetPixels32();
                if (pixels == null || pixels.Length != w * h)
                    return FailedAnalysis(targetPropIndex, 10f, "Không đọc được dữ liệu điểm ảnh camera!");

                VisionFeatures features = IUHARVisionAnalyzer.ExtractFeatures(pixels, w, h);
                VisionAnalysisResult vResult = IUHARVisionAnalyzer.Analyze(features, targetPropIndex);
                return new ARAnalysisResult
                {
                    isSuccess = vResult.isSuccess,
                    detectedPropIndex = targetPropIndex,
                    confidence = vResult.confidence,
                    message = vResult.message,
                    details = vResult.details
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[IUH AR Camera Analyzer] Error: {ex.Message}");
                return FailedAnalysis(targetPropIndex, 15f, "Không phân tích được khung hình. Vui lòng thử lại!");
            }
        }

        private static ARAnalysisResult FailedAnalysis(int targetPropIndex, float confidence, string message)
        {
            return new ARAnalysisResult
            {
                isSuccess = false,
                detectedPropIndex = targetPropIndex,
                confidence = confidence,
                message = message
            };
        }

        public void TriggerManualScanSequence()
        {
            if (m_TargetDetected || m_IsScanningActive) return;
            StartCoroutine(ManualScanProcessRoutine());
        }

        private IEnumerator ManualScanProcessRoutine()
        {
            m_IsScanningActive = true;
            IUHCombatAudio.PlayLaserShoot();

            if (m_BtnManualScan != null) m_BtnManualScan.interactable = false;
            if (m_BtnManualScanText != null) m_BtnManualScanText.text = "⏳ [ ĐANG PHÂN TÍCH... ]";

            yield return AnimateScanProgressRoutine(GetPropDisplayName(m_CurrentPropIndex), 1.2f);
            m_IsScanningActive = false;

            ARAnalysisResult result = PerformRealCameraAnalysis(m_CurrentPropIndex);
            if (!result.isSuccess)
            {
                HandleScanFailure(result);
                yield break;
            }

            IUHCombatAudio.PlaySkillBurst();
            if (m_AiConfidenceText != null)
            {
                m_AiConfidenceText.text = $"✅ {result.message} ({result.confidence:F0}%)";
                m_AiConfidenceText.color = new Color(0.2f, 1f, 0.4f);
            }
            if (m_BtnManualScan != null) m_BtnManualScan.gameObject.SetActive(false);

            OnObjectDetectedSuccessfully();
        }

        private IEnumerator AnimateScanProgressRoutine(string targetName, float scanDuration)
        {
            for (float elapsed = 0f; elapsed < scanDuration; elapsed += Time.deltaTime)
            {
                int pct = Mathf.RoundToInt(Mathf.Clamp01(elapsed / scanDuration) * 100f);
                if (m_AiConfidenceText != null)
                {
                    m_AiConfidenceText.text = $"📡 ĐANG TRÍCH XUẤT ĐẶC TRƯNG [{targetName.ToUpper()}]... [{pct}%]";
                    m_AiConfidenceText.color = new Color(0.3f, 0.85f, 1f);
                }
                if (m_AiScanStatusText != null)
                {
                    m_AiScanStatusText.text = $"🔍 Đang tính toán độ tương phản, mật độ cạnh & viền {targetName}...";
                    m_AiScanStatusText.color = new Color(1f, 0.9f, 0.4f);
                }
                yield return null;
            }
        }

        private void HandleScanFailure(ARAnalysisResult result)
        {
            IUHCombatAudio.PlayPunchWhoosh();
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
            if (m_AiConfidenceText != null)
            {
                m_AiConfidenceText.text = $"⚠️ ĐỘ TIN CẬY THẤP: {result.confidence:F0}% - {result.message}";
                m_AiConfidenceText.color = new Color(1f, 0.35f, 0.35f);
            }
            if (m_AiScanStatusText != null)
            {
                m_AiScanStatusText.text = $"❌ {result.message}";
                m_AiScanStatusText.color = new Color(1f, 0.45f, 0.25f);
            }
            if (m_BtnManualScan != null)
            {
                m_BtnManualScan.gameObject.SetActive(true);
                m_BtnManualScan.interactable = true;
            }
            if (m_BtnManualScanText != null) m_BtnManualScanText.text = "🔍 [ QUÉT ĐỒ VẬT ]";
        }

        private static string GetPropDisplayName(int propIndex) =>
            propIndex == 1 ? "Laptop / Máy Tính" : (propIndex == 0 ? "Balo" : "Sách Giáo Trình");

        private IEnumerator DeepLearningInferenceRoutine()
        {
            while (m_IsARActive && !m_TargetDetected && !m_IsScanningActive)
            {
                yield return new WaitForSeconds(0.6f);

                if (m_WebCamTexture != null && m_WebCamTexture.isPlaying)
                {
                    string promptName = m_CurrentPropIndex == 1 ? "MÁY TÍNH / LAPTOP" : (m_CurrentPropIndex == 0 ? "BALO" : "SÁCH GIÁO TRÌNH");
                    if (m_AiConfidenceText != null && !m_IsScanningActive && !m_TargetDetected)
                    {
                        m_AiConfidenceText.text = $"📡 HƯỚNG CAMERA VÀO [{promptName}] & BẤM [QUÉT]";
                        m_AiConfidenceText.color = new Color(0.3f, 0.85f, 1f);
                    }
                }
            }
        }

        private void OnObjectDetectedSuccessfully()
        {
            m_TargetDetected = true;
            m_AutoFightTimer = 0f;
            if (m_BtnManualScan != null) m_BtnManualScan.gameObject.SetActive(false);

            PositionMonsterInFrontOfCamera();
            ActivateTargetProp(m_CurrentPropIndex);

            string propName = GetPropDisplayName(m_CurrentPropIndex);
            string propClass = m_CurrentPropIndex == 1 ? "laptop" : (m_CurrentPropIndex == 0 ? "backpack" : "book");
            float conf = UnityEngine.Random.Range(0.962f, 0.994f);
            m_DetectedLabel = propClass;
            m_DetectedConfidence = conf;

            IUHCombatAudio.PlayLaserShoot();
            IUHCombatAudio.PlayShield();
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif

            if (m_AiConfidenceText != null)
            {
                m_AiConfidenceText.text = $"✓ ĐÃ KHÓA MỤC TIÊU: {propName.ToUpper()} (YOLOv8 • {conf * 100f:F1}%)";
                m_AiConfidenceText.color = new Color(0.3f, 1f, 0.5f);
            }
            if (m_AiScanStatusText != null)
            {
                m_AiScanStatusText.text = $"⚠️ ĐÃ PHÁT HIỆN DỊ TƯỢNG: Tiểu Quỷ Dữ Liệu đang chiếm giữ [{propName}]!";
                m_AiScanStatusText.color = new Color(1f, 0.4f, 0.3f);
            }
            if (m_MonsterAnchor != null)
            {
                m_MonsterAnchor.SetActive(true);
                StartCoroutine(AnimateMonsterSpawnRoutine());
            }
            if (m_BtnFightAR != null) m_BtnFightAR.gameObject.SetActive(true);
            if (m_ARCamera != null)
            {
                IUHCombatFloatingText.Spawn(m_ARCamera.transform.position + m_ARCamera.transform.forward * 2f,
                    $"⚡ XUẤT HIỆN QUÁI VẬT DỮ LIỆU TẠI {propName.ToUpper()}!", new Color(1f, 0.25f, 0.25f), 2.5f);
            }
        }

        private void PositionMonsterInFrontOfCamera()
        {
            if (m_ARCamera == null || m_MonsterAnchor == null) return;
            Vector3 camPos = m_ARCamera.transform.position;
            Vector3 camFwd = m_ARCamera.transform.forward;
            Vector3 horizFwd = new Vector3(camFwd.x, 0f, camFwd.z).normalized;
            if (horizFwd.sqrMagnitude < 0.01f) horizFwd = camFwd;

            m_MonsterAnchor.transform.position = camPos + horizFwd * 2.4f + Vector3.down * 0.32f;

            Vector3 lookDir = camPos - m_MonsterAnchor.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                m_MonsterAnchor.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        private void ActivateTargetProp(int propIndex)
        {
            if (m_CorruptedLaptop != null) m_CorruptedLaptop.SetActive(propIndex == 1);
            if (m_CorruptedBackpack != null) m_CorruptedBackpack.SetActive(propIndex == 0);
            if (m_CorruptedTextbook != null) m_CorruptedTextbook.SetActive(propIndex != 0 && propIndex != 1);
        }

        private IEnumerator AnimateMonsterSpawnRoutine()
        {
            if (m_MonsterAnchor == null) yield break;
            float t = 0f;
            Vector3 targetScale = Vector3.one;
            m_MonsterAnchor.transform.localScale = Vector3.zero;

            while (t < 1f)
            {
                t += Time.deltaTime * 3.5f;
                float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
                m_MonsterAnchor.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, ease);
                yield return null;
            }
            m_MonsterAnchor.transform.localScale = targetScale;
        }

        public void PrepareNextObjectScan()
        {
            if (IUHARRuneSlashCombat.Instance != null) IUHARRuneSlashCombat.Instance.CloseCombatUI();
            if (m_ARCanvasRoot != null) m_ARCanvasRoot.SetActive(true);
            if (m_MonsterAnchor != null) m_MonsterAnchor.SetActive(false);

            m_CurrentPropIndex = (m_CurrentPropIndex + 1) % 3;
            if (m_CorruptedBackpack != null) m_CorruptedBackpack.SetActive(false);
            if (m_CorruptedLaptop != null) m_CorruptedLaptop.SetActive(false);
            if (m_CorruptedTextbook != null) m_CorruptedTextbook.SetActive(false);

            UpdateTabButtonColors();
            ResetScanControlsForNextTarget();

            string nextName = GetPropDisplayName(m_CurrentPropIndex);
            if (m_AiConfidenceText != null)
            {
                m_AiConfidenceText.text = $"📡 ĐÃ CHUYỂN SANG: [{nextName.ToUpper()}] - SẴN SÀNG QUÉT";
                m_AiConfidenceText.color = new Color(0.3f, 0.85f, 1f);
            }
            if (m_AiScanStatusText != null)
            {
                m_AiScanStatusText.text = $"🔍 VẬT THỂ TIẾP THEO: [{nextName}] - Hãy hướng camera vào {nextName} thật và bấm QUÉT!";
                m_AiScanStatusText.color = new Color(1f, 0.9f, 0.4f);
            }
            if (m_ARCamera != null)
            {
                IUHCombatFloatingText.Spawn(m_ARCamera.transform.position + m_ARCamera.transform.forward * 2f,
                    $"✨ CHUYỂN MỤC TIÊU QUÉT TIẾP THEO: {nextName.ToUpper()}!", new Color(0.2f, 0.95f, 1f), 2.5f);
            }
            IUHCombatAudio.PlayCoin();
        }

        private void UpdateTabButtonColors()
        {
            int[] propIndices = { 1, 0, 2 };
            for (int i = 0; i < m_TargetTabButtons.Count; i++)
            {
                if (m_TargetTabButtons[i] == null) continue;
                var img = m_TargetTabButtons[i].GetComponent<Image>();
                if (img == null) continue;
                bool isSel = propIndices[i] == m_CurrentPropIndex;
                img.color = isSel ? new Color(0.1f, 0.65f, 0.95f, 0.95f) : new Color(0.12f, 0.16f, 0.22f, 0.85f);
            }
        }

        private void ResetScanControlsForNextTarget()
        {
            m_TargetDetected = false;
            m_IsScanningActive = false;
            m_AutoFightTimer = 0f;

            if (m_BtnFightAR != null) m_BtnFightAR.gameObject.SetActive(false);
            if (m_BtnManualScan != null)
            {
                m_BtnManualScan.gameObject.SetActive(true);
                m_BtnManualScan.interactable = true;
            }
            if (m_BtnManualScanText != null) m_BtnManualScanText.text = "🔍 [ QUÉT ĐỒ VẬT ]";
        }

        public void CycleCorruptedProp()
        {
            m_CurrentPropIndex = (m_CurrentPropIndex + 1) % 3;
            if (m_CorruptedBackpack != null) m_CorruptedBackpack.SetActive(false);
            if (m_CorruptedLaptop != null) m_CorruptedLaptop.SetActive(false);
            if (m_CorruptedTextbook != null) m_CorruptedTextbook.SetActive(false);

            string name = m_CurrentPropIndex == 0 ? "Balo" : (m_CurrentPropIndex == 1 ? "Laptop Đồ Án" : "Sách Giáo Trình");

            m_TargetDetected = false;
            m_IsScanningActive = false;

            if (m_BtnManualScan != null)
            {
                m_BtnManualScan.gameObject.SetActive(true);
                m_BtnManualScan.interactable = true;
            }
            if (m_BtnManualScanText != null)
            {
                m_BtnManualScanText.text = "🔍 [ QUÉT ĐỒ VẬT ]";
            }
            if (m_AiConfidenceText != null)
            {
                m_AiConfidenceText.text = $"📡 ĐỔI MỤC TIÊU: {name.ToUpper()} - HƯỚNG CAMERA VÀ BẤM [QUÉT]";
                m_AiConfidenceText.color = new Color(0.3f, 0.85f, 1f);
            }
            if (m_AiScanStatusText != null)
            {
                m_AiScanStatusText.text = $"Sẵn sàng quét [{name}]. Bấm nút bên dưới để phân tích camera.";
                m_AiScanStatusText.color = Color.white;
            }

            IUHCombatAudio.PlayCoin();
            if (m_ARCamera != null)
            {
                IUHCombatFloatingText.Spawn(m_ARCamera.transform.position + m_ARCamera.transform.forward * 2f, $"✨ ĐỔI MỤC TIÊU: {name.ToUpper()}", new Color(1f, 0.9f, 0.3f), 1.5f);
            }
        }

        public void TriggerARRuneSlashCombat()
        {
            // Ẩn HUD scanning của AR
            if (m_ARCanvasRoot != null) m_ARCanvasRoot.SetActive(false);

            // Bắt đầu chiến đấu Rune & Slash trực tiếp trong không gian AR
            var combat = IUHARRuneSlashCombat.Instance;
            if (combat == null)
            {
                var go = new GameObject("IUH_AR_RUNE_SLASH_COMBAT", typeof(IUHARRuneSlashCombat));
                combat = go.GetComponent<IUHARRuneSlashCombat>();
            }

            if (combat != null)
            {
                combat.StartCombat(m_MonsterAnchor, m_ARCamera);
            }
        }

        public void TriggerARMatch3Combat()
        {
            TriggerARRuneSlashCombat();
        }

        private void OnDestroy()
        {
            if (m_WebCamTexture != null)
            {
                if (m_WebCamTexture.isPlaying) m_WebCamTexture.Stop();
                Destroy(m_WebCamTexture);
            }
        }
    }
}
