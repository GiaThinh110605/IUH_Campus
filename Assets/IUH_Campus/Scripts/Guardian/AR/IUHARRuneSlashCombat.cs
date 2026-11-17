using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.Guardian.AR
{
    /// <summary>
    /// Chế độ Chiến đấu AR: 🧊 Fruit Ninja Cyber Shapes (Chém Bổ Khối Lõi Dữ Liệu)
    /// - Quái vật 3D trên bàn tung các khối đa giác dữ liệu bay lên không trung theo hình parabol:
    ///   + Khối vuông dữ liệu (Cyber Cube): Vuốt chém bổ đôi, gây sát thương cho boss và nạp năng lượng Ultimate.
    ///   + Ngôi sao năng lượng (Energy Star): Chém trúng hồi máu cho sinh viên (+20 HP).
    ///   + Kim tự tháp dữ liệu (Data Pyramid): Chém trúng tạo sóng xung kích nổ liên hoàn.
    ///   + Bom độc gai nhọn (Spike Bomb): NÉ RA! Nếu chém trúng sẽ bị nổ mất máu (-25 HP).
    /// - Khi tích đầy 100% năng lượng Ultimate / Quái vật cạn máu:
    ///   + Kích hoạt Slow-motion "ĐÒN CHÉM TỐI THƯỢNG (ULTIMATE CYBER SLASH)": Vuốt một đường thần tốc chém đôi quái vật!
    /// </summary>
    public class IUHARRuneSlashCombat : MonoBehaviour
    {
        public static IUHARRuneSlashCombat Instance { get; private set; }

        [Header("State")]
        private bool m_IsCombatActive = false;
        public bool IsCombatActive => m_IsCombatActive;

        private Camera m_ARCamera;
        private GameObject m_MonsterAnchor;
        private Transform m_MonsterVisual;

        // Health & Energy
        private float m_BossMaxHp = 1000f;
        private float m_BossCurrentHp = 1000f;
        private float m_PlayerMaxHp = 100f;
        private float m_PlayerCurrentHp = 100f;
        private float m_UltimateEnergy = 0f; // 0 đến 100%
        private int m_ComboCount = 0;

        // Blade Trail
        private GameObject m_BladeFollower;
        private TrailRenderer m_BladeTrail;
        private Vector3 m_LastPointerScreenPos;
        private bool m_IsPointerDown = false;

        // Cyber Flying Shapes
        public enum ShapeType { DataCube, EnergyStar, DataPyramid, SpikeBomb }

        private class FlyingCyberShape
        {
            public GameObject gameObject;
            public ShapeType shapeType;
            public Vector3 velocity;
            public Vector3 rotationAxis;
            public float rotationSpeed;
            public bool isSliced;
            public float lifetime;
        }

        private List<FlyingCyberShape> m_ActiveShapes = new List<FlyingCyberShape>();
        private List<GameObject> m_SlicedHalves = new List<GameObject>();

        // Ultimate Slash Phase (Slow-motion chém đôi boss)
        private bool m_IsUltimatePhase = false;
        private GameObject m_UltimateCutLineUI;
        private Text m_UltimateAlertText;

        // Combat HUD UI
        private GameObject m_CombatCanvasRoot;
        private Image m_BossHpFill;
        private Text m_BossHpText;
        private Image m_PlayerHpFill;
        private Text m_PlayerHpText;
        private Image m_UltimateEnergyFill;
        private Text m_UltimateEnergyText;
        private Text m_InstructionBanner;
        private Text m_ComboText;
        private GameObject m_VictoryModal;
        private Text m_VictoryCountdownText;
        private GameObject m_DefeatModal;

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

            BuildBladeTrailObject();
            BuildCombatUI();
        }

        private void BuildBladeTrailObject()
        {
            m_BladeFollower = new GameObject("AR_Blade_Follower");
            m_BladeFollower.transform.SetParent(transform, false);

            m_BladeTrail = m_BladeFollower.AddComponent<TrailRenderer>();
            m_BladeTrail.time = 0.26f;
            m_BladeTrail.startWidth = 0.14f;
            m_BladeTrail.endWidth = 0.005f;

            Shader trailShader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Texture");
            var trailMat = new Material(trailShader);
            m_BladeTrail.sharedMaterial = trailMat;

            // Gradient màu Cyan phát sáng sang Vàng Kim
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.2f, 1f, 0.9f), 0.0f), new GradientColorKey(new Color(1f, 0.85f, 0.15f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            m_BladeTrail.colorGradient = gradient;
            m_BladeTrail.emitting = false;
        }

        private void BuildCombatUI()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf")
                ?? Font.CreateDynamicFontFromOSFont("Helvetica", 16)
                ?? Font.CreateDynamicFontFromOSFont("Arial", 16);

            m_CombatCanvasRoot = new GameObject("IUH_AR_CYBER_SLASH_CANVAS", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_CombatCanvasRoot.transform.SetParent(transform, false);

            var canvas = m_CombatCanvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;

            var scaler = m_CombatCanvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // 1. Boss HP Container (Top-Center)
            GameObject bossBarGO = new GameObject("Boss_HP_Container", typeof(RectTransform), typeof(Image));
            bossBarGO.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var bbRt = bossBarGO.GetComponent<RectTransform>();
            bbRt.anchorMin = new Vector2(0.5f, 1f); bbRt.anchorMax = new Vector2(0.5f, 1f);
            bbRt.pivot = new Vector2(0.5f, 1f);
            bbRt.anchoredPosition = new Vector2(0f, -18f);
            bbRt.sizeDelta = new Vector2(620f, 54f);
            bossBarGO.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.16f, 0.92f);

            GameObject bossFillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            bossFillGO.transform.SetParent(bossBarGO.transform, false);
            var bfRt = bossFillGO.GetComponent<RectTransform>();
            bfRt.anchorMin = new Vector2(0.02f, 0.15f); bfRt.anchorMax = new Vector2(0.98f, 0.85f);
            bfRt.sizeDelta = Vector2.zero;
            m_BossHpFill = bossFillGO.GetComponent<Image>();
            m_BossHpFill.type = Image.Type.Filled;
            m_BossHpFill.fillMethod = Image.FillMethod.Horizontal;
            m_BossHpFill.color = new Color(0.9f, 0.2f, 0.35f, 0.95f);

            GameObject bossTxtGO = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            bossTxtGO.transform.SetParent(bossBarGO.transform, false);
            var btRt = bossTxtGO.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero; btRt.anchorMax = Vector2.one; btRt.sizeDelta = Vector2.zero;
            m_BossHpText = bossTxtGO.GetComponent<Text>();
            m_BossHpText.font = font;
            m_BossHpText.text = "👹 TIỂU QUỶ DỮ LIỆU: 1000 / 1000";
            m_BossHpText.fontSize = 15;
            m_BossHpText.fontStyle = FontStyle.Bold;
            m_BossHpText.alignment = TextAnchor.MiddleCenter;
            m_BossHpText.color = Color.white;
            bossTxtGO.GetComponent<Outline>().effectColor = new Color(0.25f, 0f, 0.08f, 0.9f);

            // 2. Player Status & Ultimate Energy (Top-Left)
            GameObject playerStatusGO = new GameObject("Player_Status_Container", typeof(RectTransform), typeof(Image));
            playerStatusGO.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var psRt = playerStatusGO.GetComponent<RectTransform>();
            psRt.anchorMin = new Vector2(0f, 1f); psRt.anchorMax = new Vector2(0f, 1f);
            psRt.pivot = new Vector2(0f, 1f);
            psRt.anchoredPosition = new Vector2(25f, -85f);
            psRt.sizeDelta = new Vector2(270f, 75f);
            playerStatusGO.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.14f, 0.92f);

            // HP Bar
            GameObject pFillGO = new GameObject("HP_Fill", typeof(RectTransform), typeof(Image));
            pFillGO.transform.SetParent(playerStatusGO.transform, false);
            var pfRt = pFillGO.GetComponent<RectTransform>();
            pfRt.anchorMin = new Vector2(0.04f, 0.52f); pfRt.anchorMax = new Vector2(0.96f, 0.92f);
            pfRt.sizeDelta = Vector2.zero;
            m_PlayerHpFill = pFillGO.GetComponent<Image>();
            m_PlayerHpFill.type = Image.Type.Filled;
            m_PlayerHpFill.fillMethod = Image.FillMethod.Horizontal;
            m_PlayerHpFill.color = new Color(0.2f, 0.95f, 0.45f, 0.95f);

            GameObject pTxtGO = new GameObject("HP_Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            pTxtGO.transform.SetParent(pFillGO.transform, false);
            var ptRt = pTxtGO.GetComponent<RectTransform>();
            ptRt.anchorMin = Vector2.zero; ptRt.anchorMax = Vector2.one; ptRt.sizeDelta = Vector2.zero;
            m_PlayerHpText = pTxtGO.GetComponent<Text>();
            m_PlayerHpText.font = font;
            m_PlayerHpText.text = "🛡️ SINH VIÊN: 100 / 100";
            m_PlayerHpText.fontSize = 12;
            m_PlayerHpText.fontStyle = FontStyle.Bold;
            m_PlayerHpText.alignment = TextAnchor.MiddleCenter;
            m_PlayerHpText.color = Color.white;
            pTxtGO.GetComponent<Outline>().effectColor = new Color(0.02f, 0.15f, 0.05f, 0.9f);

            // Ultimate Energy Bar
            GameObject ultFillGO = new GameObject("Ult_Fill", typeof(RectTransform), typeof(Image));
            ultFillGO.transform.SetParent(playerStatusGO.transform, false);
            var ufRt = ultFillGO.GetComponent<RectTransform>();
            ufRt.anchorMin = new Vector2(0.04f, 0.08f); ufRt.anchorMax = new Vector2(0.96f, 0.48f);
            ufRt.sizeDelta = Vector2.zero;
            m_UltimateEnergyFill = ultFillGO.GetComponent<Image>();
            m_UltimateEnergyFill.type = Image.Type.Filled;
            m_UltimateEnergyFill.fillMethod = Image.FillMethod.Horizontal;
            m_UltimateEnergyFill.color = new Color(1f, 0.85f, 0.1f, 0.95f);

            GameObject ultTxtGO = new GameObject("Ult_Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            ultTxtGO.transform.SetParent(ultFillGO.transform, false);
            var utRt = ultTxtGO.GetComponent<RectTransform>();
            utRt.anchorMin = Vector2.zero; utRt.anchorMax = Vector2.one; utRt.sizeDelta = Vector2.zero;
            m_UltimateEnergyText = ultTxtGO.GetComponent<Text>();
            m_UltimateEnergyText.font = font;
            m_UltimateEnergyText.text = "⚡ NĂNG LƯỢNG TỐI THƯỢNG: 0%";
            m_UltimateEnergyText.fontSize = 11;
            m_UltimateEnergyText.fontStyle = FontStyle.Bold;
            m_UltimateEnergyText.alignment = TextAnchor.MiddleCenter;
            m_UltimateEnergyText.color = Color.white;
            ultTxtGO.GetComponent<Outline>().effectColor = new Color(0.2f, 0.15f, 0f, 0.9f);

            // 3. Nút Thoát Trận [✕ THOÁT] (Top-Left)
            GameObject exitBtnGO = new GameObject("Btn_Exit_Combat", typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback));
            exitBtnGO.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var exRt = exitBtnGO.GetComponent<RectTransform>();
            exRt.anchorMin = new Vector2(0f, 1f); exRt.anchorMax = new Vector2(0f, 1f);
            exRt.pivot = new Vector2(0f, 1f);
            exRt.anchoredPosition = new Vector2(25f, -22f);
            exRt.sizeDelta = new Vector2(170f, 50f);
            exitBtnGO.GetComponent<Image>().color = new Color(0.85f, 0.15f, 0.2f, 0.92f);

            GameObject exTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
            exTxt.transform.SetParent(exitBtnGO.transform, false);
            var etxRt = exTxt.GetComponent<RectTransform>();
            etxRt.anchorMin = Vector2.zero; etxRt.anchorMax = Vector2.one; etxRt.sizeDelta = Vector2.zero;
            var t = exTxt.GetComponent<Text>();
            t.font = font; t.text = "✕ THOÁT"; t.fontSize = 15; t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
            t.raycastTarget = false;

            exitBtnGO.GetComponent<Button>().onClick.AddListener(ExitCombatToCampus);

            // 4. Instruction Banner (Top-Center)
            GameObject bannerGO = new GameObject("Instruction_Banner", typeof(RectTransform), typeof(Text), typeof(Outline));
            bannerGO.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var bnRt = bannerGO.GetComponent<RectTransform>();
            bnRt.anchorMin = new Vector2(0.5f, 1f); bnRt.anchorMax = new Vector2(0.5f, 1f);
            bnRt.pivot = new Vector2(0.5f, 1f);
            bnRt.anchoredPosition = new Vector2(0f, -80f);
            bnRt.sizeDelta = new Vector2(760f, 35f);
            m_InstructionBanner = bannerGO.GetComponent<Text>();
            m_InstructionBanner.font = font;
            m_InstructionBanner.text = "🧊 VUỐT CHÉM BỔ ĐÔI CÁC KHỐI DỮ LIỆU! ⚠️ NÉ BOM ĐỎ!";
            m_InstructionBanner.fontSize = 15;
            m_InstructionBanner.fontStyle = FontStyle.Bold;
            m_InstructionBanner.alignment = TextAnchor.MiddleCenter;
            m_InstructionBanner.color = new Color(1f, 0.92f, 0.3f);
            bannerGO.GetComponent<Outline>().effectColor = new Color(0.1f, 0.05f, 0f, 0.9f);

            // 5. Combo Counter (Left-Center)
            GameObject comboGO = new GameObject("Combo_Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            comboGO.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var cbRt = comboGO.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(0f, 0.5f); cbRt.anchorMax = new Vector2(0f, 0.5f);
            cbRt.pivot = new Vector2(0f, 0.5f);
            cbRt.anchoredPosition = new Vector2(30f, 50f);
            cbRt.sizeDelta = new Vector2(250f, 60f);
            m_ComboText = comboGO.GetComponent<Text>();
            m_ComboText.font = font;
            m_ComboText.text = "⚡ COMBO x0";
            m_ComboText.fontSize = 24;
            m_ComboText.fontStyle = FontStyle.Bold;
            m_ComboText.color = new Color(0.3f, 0.95f, 1f);
            comboGO.GetComponent<Outline>().effectColor = new Color(0f, 0.15f, 0.3f, 0.9f);
            m_ComboText.gameObject.SetActive(false);

            // 6. Ultimate Cut Line UI
            m_UltimateCutLineUI = new GameObject("Ultimate_Cut_Line", typeof(RectTransform), typeof(Image));
            m_UltimateCutLineUI.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var clRt = m_UltimateCutLineUI.GetComponent<RectTransform>();
            clRt.anchorMin = new Vector2(0.5f, 0.5f); clRt.anchorMax = new Vector2(0.5f, 0.5f);
            clRt.sizeDelta = new Vector2(700f, 6f);
            clRt.localRotation = Quaternion.Euler(0f, 0f, -25f);
            m_UltimateCutLineUI.GetComponent<Image>().color = new Color(1f, 0.85f, 0.1f, 0.95f);

            GameObject uAlert = new GameObject("AlertText", typeof(RectTransform), typeof(Text), typeof(Outline));
            uAlert.transform.SetParent(m_UltimateCutLineUI.transform, false);
            var uaRt = uAlert.GetComponent<RectTransform>();
            uaRt.anchorMin = new Vector2(0.5f, 1f); uaRt.anchorMax = new Vector2(0.5f, 1f);
            uaRt.pivot = new Vector2(0.5f, 0f);
            uaRt.anchoredPosition = new Vector2(0f, 15f);
            uaRt.sizeDelta = new Vector2(650f, 40f);
            m_UltimateAlertText = uAlert.GetComponent<Text>();
            m_UltimateAlertText.font = font;
            m_UltimateAlertText.text = "⚡ VUỐT THEO ĐƯỜNG VÀNG ĐỂ CHÉM ĐÔI QUÁI VẬT! ⚡";
            m_UltimateAlertText.fontSize = 18;
            m_UltimateAlertText.fontStyle = FontStyle.Bold;
            m_UltimateAlertText.alignment = TextAnchor.MiddleCenter;
            m_UltimateAlertText.color = new Color(1f, 0.95f, 0.3f);
            uAlert.GetComponent<Outline>().effectColor = new Color(0.3f, 0.15f, 0f, 0.9f);

            m_UltimateCutLineUI.SetActive(false);

            // 7. Modals: Victory & Defeat
            BuildEndGameModals(font);

            m_CombatCanvasRoot.SetActive(false);
        }

        private void BuildEndGameModals(Font font)
        {
            // Victory Modal
            m_VictoryModal = new GameObject("Modal_Victory", typeof(RectTransform), typeof(Image));
            m_VictoryModal.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var vRt = m_VictoryModal.GetComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.5f, 0.5f); vRt.anchorMax = new Vector2(0.5f, 0.5f);
            vRt.sizeDelta = new Vector2(640f, 360f);
            m_VictoryModal.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.16f, 0.96f);

            GameObject vTitle = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
            vTitle.transform.SetParent(m_VictoryModal.transform, false);
            var vtRt = vTitle.GetComponent<RectTransform>();
            vtRt.anchorMin = new Vector2(0f, 0.68f); vtRt.anchorMax = Vector2.one; vtRt.sizeDelta = Vector2.zero;
            var vt = vTitle.GetComponent<Text>();
            vt.font = font; vt.text = "🏆 CHIẾN THẮNG TUYỆT ĐỐI!\nQUÁI VẬT ĐÃ BỊ CHÉM ĐÔI & THANH TẨY";
            vt.fontSize = 20; vt.fontStyle = FontStyle.Bold; vt.alignment = TextAnchor.MiddleCenter;
            vt.color = new Color(0.3f, 1f, 0.6f);
            vTitle.GetComponent<Outline>().effectColor = new Color(0f, 0.2f, 0.1f, 0.9f);

            GameObject vDesc = new GameObject("Desc", typeof(RectTransform), typeof(Text));
            vDesc.transform.SetParent(m_VictoryModal.transform, false);
            var vdRt = vDesc.GetComponent<RectTransform>();
            vdRt.anchorMin = new Vector2(0.05f, 0.42f); vdRt.anchorMax = new Vector2(0.95f, 0.68f); vdRt.sizeDelta = Vector2.zero;
            var vd = vDesc.GetComponent<Text>();
            vd.font = font; vd.text = "✨ Đòn chém tối thượng đã giải thoát vật thể khỏi virus dữ liệu!\n🎁 Nhận thưởng: +300 IUH Coin & +80 Điểm Kinh Nghiệm";
            vd.fontSize = 14; vd.alignment = TextAnchor.MiddleCenter; vd.color = new Color(1f, 0.9f, 0.6f);

            // Dòng đếm ngược tự động chuyển sang vật thể tiếp theo
            GameObject vCountGO = new GameObject("CountdownText", typeof(RectTransform), typeof(Text));
            vCountGO.transform.SetParent(m_VictoryModal.transform, false);
            var vcRt = vCountGO.GetComponent<RectTransform>();
            vcRt.anchorMin = new Vector2(0.05f, 0.28f); vcRt.anchorMax = new Vector2(0.95f, 0.42f); vcRt.sizeDelta = Vector2.zero;
            m_VictoryCountdownText = vCountGO.GetComponent<Text>();
            m_VictoryCountdownText.font = font;
            m_VictoryCountdownText.text = "⏱️ Tự động chuyển quét vật thể tiếp theo sau 5s...";
            m_VictoryCountdownText.fontSize = 13;
            m_VictoryCountdownText.fontStyle = FontStyle.Italic;
            m_VictoryCountdownText.alignment = TextAnchor.MiddleCenter;
            m_VictoryCountdownText.color = new Color(0.35f, 0.9f, 1f);

            // Nút 1: [ 🔍 QUÉT VẬT THỂ TIẾP THEO (AR) ]
            GameObject vBtnScan = new GameObject("Btn_ScanNext", typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback));
            vBtnScan.transform.SetParent(m_VictoryModal.transform, false);
            var vbsRt = vBtnScan.GetComponent<RectTransform>();
            vbsRt.anchorMin = new Vector2(0.05f, 0.08f); vbsRt.anchorMax = new Vector2(0.52f, 0.26f);
            vbsRt.sizeDelta = Vector2.zero;
            vBtnScan.GetComponent<Image>().color = new Color(0.12f, 0.65f, 0.95f, 0.98f);

            GameObject vbsTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
            vbsTxt.transform.SetParent(vBtnScan.transform, false);
            var vbstRt = vbsTxt.GetComponent<RectTransform>();
            vbstRt.anchorMin = Vector2.zero; vbstRt.anchorMax = Vector2.one; vbstRt.sizeDelta = Vector2.zero;
            var vbst = vbsTxt.GetComponent<Text>();
            vbst.font = font; vbst.text = "🔍 QUÉT VẬT THỂ TIẾP"; vbst.fontSize = 15; vbst.fontStyle = FontStyle.Bold;
            vbst.alignment = TextAnchor.MiddleCenter; vbst.color = Color.white; vbst.raycastTarget = false;

            vBtnScan.GetComponent<Button>().onClick.AddListener(ProceedToNextScan);

            // Nút 2: [ 🚪 VỀ SÂN TRƯỜNG ]
            GameObject vBtnExit = new GameObject("Btn_ExitCampus", typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback));
            vBtnExit.transform.SetParent(m_VictoryModal.transform, false);
            var vbeRt = vBtnExit.GetComponent<RectTransform>();
            vbeRt.anchorMin = new Vector2(0.55f, 0.08f); vbeRt.anchorMax = new Vector2(0.95f, 0.26f);
            vbeRt.sizeDelta = Vector2.zero;
            vBtnExit.GetComponent<Image>().color = new Color(0.25f, 0.32f, 0.42f, 0.95f);

            GameObject vbeTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
            vbeTxt.transform.SetParent(vBtnExit.transform, false);
            var vbetRt = vbeTxt.GetComponent<RectTransform>();
            vbetRt.anchorMin = Vector2.zero; vbetRt.anchorMax = Vector2.one; vbetRt.sizeDelta = Vector2.zero;
            var vbet = vbeTxt.GetComponent<Text>();
            vbet.font = font; vbet.text = "🚪 VỀ SÂN TRƯỜNG"; vbet.fontSize = 15; vbet.fontStyle = FontStyle.Bold;
            vbet.alignment = TextAnchor.MiddleCenter; vbet.color = Color.white; vbet.raycastTarget = false;

            vBtnExit.GetComponent<Button>().onClick.AddListener(ExitCombatToCampus);
            m_VictoryModal.SetActive(false);

            // Defeat Modal
            m_DefeatModal = new GameObject("Modal_Defeat", typeof(RectTransform), typeof(Image));
            m_DefeatModal.transform.SetParent(m_CombatCanvasRoot.transform, false);
            var dRt = m_DefeatModal.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0.5f); dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(580f, 290f);
            m_DefeatModal.GetComponent<Image>().color = new Color(0.14f, 0.04f, 0.06f, 0.96f);

            GameObject dTitle = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
            dTitle.transform.SetParent(m_DefeatModal.transform, false);
            var dtRt = dTitle.GetComponent<RectTransform>();
            dtRt.anchorMin = new Vector2(0f, 0.65f); dtRt.anchorMax = Vector2.one; dtRt.sizeDelta = Vector2.zero;
            var dt = dTitle.GetComponent<Text>();
            dt.font = font; dt.text = "⚠️ THẤT BẠI!\nBỊ BOM ĐỘC GAI NHỌN HẠ GỤC";
            dt.fontSize = 20; dt.fontStyle = FontStyle.Bold; dt.alignment = TextAnchor.MiddleCenter;
            dt.color = new Color(1f, 0.3f, 0.35f);
            dTitle.GetComponent<Outline>().effectColor = new Color(0.2f, 0f, 0f, 0.9f);

            GameObject dBtn = new GameObject("Btn_Exit", typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback));
            dBtn.transform.SetParent(m_DefeatModal.transform, false);
            var dbRt = dBtn.GetComponent<RectTransform>();
            dbRt.anchorMin = new Vector2(0.5f, 0.15f); dbRt.anchorMax = new Vector2(0.5f, 0.15f);
            dbRt.pivot = new Vector2(0.5f, 0f);
            dbRt.sizeDelta = new Vector2(250f, 52f);
            dBtn.GetComponent<Image>().color = new Color(0.85f, 0.2f, 0.25f, 0.95f);

            GameObject dbTxt = new GameObject("Text", typeof(RectTransform), typeof(Text));
            dbTxt.transform.SetParent(dBtn.transform, false);
            var dbtRt = dbTxt.GetComponent<RectTransform>();
            dbtRt.anchorMin = Vector2.zero; dbtRt.anchorMax = Vector2.one; dbtRt.sizeDelta = Vector2.zero;
            var dbt = dbTxt.GetComponent<Text>();
            dbt.font = font; dbt.text = "RÚT LUI"; dbt.fontSize = 15; dbt.fontStyle = FontStyle.Bold;
            dbt.alignment = TextAnchor.MiddleCenter; dbt.color = Color.white; dbt.raycastTarget = false;

            dBtn.GetComponent<Button>().onClick.AddListener(ExitCombatToCampus);
            m_DefeatModal.SetActive(false);
        }

        public void StartCombat(GameObject monsterAnchor, Camera arCamera)
        {
            m_MonsterAnchor = monsterAnchor;
            m_ARCamera = arCamera;
            if (m_MonsterAnchor != null)
            {
                m_MonsterVisual = m_MonsterAnchor.transform.Find("AR_Cyber_Demon_Model")
                    ?? m_MonsterAnchor.transform.Find("AR_Cyber_Demon_Fallback")
                    ?? m_MonsterAnchor.transform;
            }

            m_BossMaxHp = 1000f;
            m_BossCurrentHp = m_BossMaxHp;
            m_PlayerMaxHp = 100f;
            m_PlayerCurrentHp = m_PlayerMaxHp;
            m_UltimateEnergy = 0f;
            m_ComboCount = 0;
            m_IsUltimatePhase = false;

            ClearAllActiveShapes();
            UpdateHPDisplays();

            if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
            if (m_DefeatModal != null) m_DefeatModal.SetActive(false);
            if (m_UltimateCutLineUI != null) m_UltimateCutLineUI.SetActive(false);

            m_CombatCanvasRoot.SetActive(true);
            m_IsCombatActive = true;

            IUHCombatAudio.PlayBossRoar();
            IUHCombatFloatingText.Spawn(m_ARCamera.transform.position + m_ARCamera.transform.forward * 2f, "🧊 CHIẾN ĐẤU CHÉM KHỐI DỮ LIỆU BẮT ĐẦU!", new Color(0.2f, 0.95f, 1f), 2.5f);

            StartCoroutine(ShapeTossingRoutine());
        }

        private IEnumerator ShapeTossingRoutine()
        {
            int wave = 0;

            while (m_IsCombatActive && m_BossCurrentHp > 0 && m_PlayerCurrentHp > 0)
            {
                if (!m_IsUltimatePhase)
                {
                    wave++;
                    int shapeCount = UnityEngine.Random.Range(2, 4);

                    // Quái vật gầm rú và lao tới tung đòn cử động sống động
                    if (m_MonsterVisual != null)
                    {
                        var livingAnim = m_MonsterVisual.GetComponent<IUHARMonsterLivingController>();
                        if (livingAnim != null) livingAnim.TriggerAttackRoar();
                    }

                    // Tung đợt khối dữ liệu bay lên
                    for (int i = 0; i < shapeCount; i++)
                    {
                        if (!m_IsCombatActive || m_IsUltimatePhase || m_BossCurrentHp <= 0f) break;

                        // Tỉ lệ xuất hiện các loại hình khối
                        float roll = UnityEngine.Random.value;
                        ShapeType chosenType = ShapeType.DataCube;
                        if (roll < 0.22f) chosenType = ShapeType.SpikeBomb; // 22% bom độc
                        else if (roll < 0.45f) chosenType = ShapeType.EnergyStar; // 23% ngôi sao hồi máu
                        else if (roll < 0.65f) chosenType = ShapeType.DataPyramid; // 20% kim tự tháp
                        else chosenType = ShapeType.DataCube; // 35% khối vuông

                        SpawnFlyingShape(chosenType);
                        yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 0.45f));
                    }

                    if (!m_IsCombatActive || m_BossCurrentHp <= 0f) break;

                    yield return new WaitForSeconds(UnityEngine.Random.Range(2.2f, 3.2f));

                    if (!m_IsCombatActive || m_BossCurrentHp <= 0f) break;

                    // Nếu năng lượng Ultimate đạt 100% hoặc Boss còn dưới 25% máu -> Kích hoạt Ultimate Slash
                    if (m_UltimateEnergy >= 100f || m_BossCurrentHp <= 250f)
                    {
                        yield return TriggerUltimateSlashPhase();
                    }
                }

                yield return null;
            }

            // Dừng hoàn toàn và xóa sạch các khối khi quái vật hết máu
            if (m_BossCurrentHp <= 0f)
            {
                ClearAllActiveShapes();
            }
        }

        private void SpawnFlyingShape(ShapeType type)
        {
            if (m_ARCamera == null || m_MonsterAnchor == null || !m_IsCombatActive || m_BossCurrentHp <= 0f) return;

            Vector3 spawnOrigin = m_MonsterAnchor.transform.position + new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), 0.2f, UnityEngine.Random.Range(-0.2f, 0.2f));

            GameObject shapeObj = null;
            Color shapeColor = Color.cyan;

            if (type == ShapeType.DataCube)
            {
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shapeObj.name = "Cyber_Cube";
                shapeObj.transform.localScale = Vector3.one * 0.26f;
                shapeColor = new Color(0.15f, 0.9f, 1f, 0.95f);
            }
            else if (type == ShapeType.EnergyStar)
            {
                // Ngôi sao năng lượng (Capsule hoặc Cylinder xoay ngang)
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shapeObj.name = "Energy_Star";
                shapeObj.transform.localScale = new Vector3(0.28f, 0.05f, 0.28f);
                shapeColor = new Color(1f, 0.9f, 0.1f, 0.95f); // Vàng kim rực rỡ
            }
            else if (type == ShapeType.DataPyramid)
            {
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                shapeObj.name = "Data_Pyramid";
                shapeObj.transform.localScale = new Vector3(0.22f, 0.35f, 0.22f);
                shapeColor = new Color(0.85f, 0.2f, 1f, 0.95f); // Tím neon
            }
            else // SpikeBomb
            {
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shapeObj.name = "Spike_Bomb";
                shapeObj.transform.localScale = Vector3.one * 0.32f;
                shapeColor = new Color(0.95f, 0.12f, 0.18f, 0.95f); // Đỏ cảnh báo

                // Gắn thêm các gai nhọn nhỏ quanh bom
                for (int g = 0; g < 4; g++)
                {
                    var spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    spike.transform.SetParent(shapeObj.transform, false);
                    spike.transform.localScale = new Vector3(0.18f, 1.25f, 0.18f);
                    spike.transform.localRotation = Quaternion.Euler(g * 45f, g * 45f, 0f);
                    var spCol = spike.GetComponent<Collider>();
                    if (spCol != null) Destroy(spCol);
                }
            }

            var col = shapeObj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Texture");
            var mat = new Material(sh);
            mat.color = shapeColor;
            shapeObj.GetComponent<Renderer>().sharedMaterial = mat;

            shapeObj.transform.position = spawnOrigin;

            // Vận tốc phóng bay parabol lên không trung trên bàn
            float upForce = UnityEngine.Random.Range(2.2f, 3.4f);
            float sideForce = UnityEngine.Random.Range(-0.85f, 0.85f);
            float forwardForce = UnityEngine.Random.Range(-0.4f, 0.2f);
            Vector3 initVel = (Vector3.up * upForce) + (m_ARCamera.transform.right * sideForce) + (m_ARCamera.transform.forward * forwardForce);

            var shape = new FlyingCyberShape
            {
                gameObject = shapeObj,
                shapeType = type,
                velocity = initVel,
                rotationAxis = UnityEngine.Random.onUnitSphere,
                rotationSpeed = UnityEngine.Random.Range(120f, 360f),
                isSliced = false,
                lifetime = 0f
            };

            m_ActiveShapes.Add(shape);
            IUHCombatAudio.PlayPunchWhoosh();
        }

        private void Update()
        {
            if (!m_IsCombatActive) return;

            // 1. Nhận diện thao tác vuốt chém của ngón tay
            HandleBladeSwipeInput();

            // 2. Mô phỏng vật lý parabol trọng lực cho các khối bay
            UpdateFlyingShapes();

            // 3. Cập nhật các mảnh vỡ sau khi bị chém
            UpdateSlicedHalves();
        }

        private void HandleBladeSwipeInput()
        {
            bool pointerDown = false;
            bool pointerHeld = false;
            Vector3 pointerPos = Vector3.zero;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                pointerPos = t.position;
                if (t.phase == TouchPhase.Began) pointerDown = true;
                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) pointerHeld = true;
            }
            else if (Input.GetMouseButton(0))
            {
                pointerPos = Input.mousePosition;
                if (Input.GetMouseButtonDown(0)) pointerDown = true;
                pointerHeld = true;
            }

            if (pointerDown)
            {
                m_IsPointerDown = true;
                m_LastPointerScreenPos = pointerPos;
                if (m_BladeTrail != null) m_BladeTrail.emitting = true;
            }
            else if (!pointerHeld)
            {
                m_IsPointerDown = false;
                if (m_BladeTrail != null) m_BladeTrail.emitting = false;
            }

            if (m_IsPointerDown && m_ARCamera != null)
            {
                // Vị trí tia chém trong không gian AR trước camera
                Vector3 worldPos = m_ARCamera.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, 0.45f));
                if (m_BladeFollower != null) m_BladeFollower.transform.position = worldPos;

                float swipeDist = Vector3.Distance(pointerPos, m_LastPointerScreenPos);
                if (swipeDist > 16f)
                {
                    // Kiểm tra chém trúng khối dữ liệu bay
                    CheckSliceShapes(m_LastPointerScreenPos, pointerPos);

                    // Kiểm tra chém đòn tối thượng (Ultimate Slash)
                    if (m_IsUltimatePhase)
                    {
                        CheckUltimateSlash(m_LastPointerScreenPos, pointerPos);
                    }

                    m_LastPointerScreenPos = pointerPos;
                }
            }
        }

        private void CheckSliceShapes(Vector3 startScreen, Vector3 endScreen)
        {
            if (m_ARCamera == null) return;

            Vector3 startWorld = m_ARCamera.ScreenToWorldPoint(new Vector3(startScreen.x, startScreen.y, 0.6f));
            Vector3 endWorld = m_ARCamera.ScreenToWorldPoint(new Vector3(endScreen.x, endScreen.y, 0.6f));
            Vector3 cutDir = (endWorld - startWorld).normalized;
            if (cutDir.sqrMagnitude < 0.01f) cutDir = m_ARCamera.transform.right;

            for (int i = m_ActiveShapes.Count - 1; i >= 0; i--)
            {
                var s = m_ActiveShapes[i];
                if (s == null || s.gameObject == null || s.isSliced) continue;

                Vector3 screenPoint = m_ARCamera.WorldToScreenPoint(s.gameObject.transform.position);
                if (screenPoint.z < 0.1f) continue; // Phía sau camera

                float dist = DistancePointToSegment(screenPoint, startScreen, endScreen);
                if (dist < 80f) // Bán kính chém trúng (pixels)
                {
                    SliceShape(s, i, cutDir);
                }
            }
        }

        private void SliceShape(FlyingCyberShape s, int index, Vector3 cutDir)
        {
            s.isSliced = true;
            Vector3 hitPos = s.gameObject.transform.position;

#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif

            Color themeColor;

            if (s.shapeType == ShapeType.SpikeBomb)
            {
                themeColor = new Color(1f, 0.18f, 0.12f, 1f);
                // CHÉM TRÚNG BOM ĐỘC GAI NHỌN!
                DamagePlayer(25f);
                m_ComboCount = 0;
                if (m_ComboText != null) m_ComboText.gameObject.SetActive(false);

                IUHCombatAudio.PlayBossHit();
                IUHCombatFloatingText.Spawn(hitPos, "💥 BOM ĐỘC PHÁT NỔ! -25 HP", themeColor, 1.5f);

                // Hiệu ứng nổ bom độc hoành tráng
                SpawnShockwaveRing(hitPos, themeColor, 1.8f);
                SpawnCyberSparks(hitPos, Vector3.up, themeColor, 45);
                TriggerScreenPunch(0.12f, 0.035f);

                Destroy(s.gameObject);
                m_ActiveShapes.RemoveAt(index);
                return;
            }

            // CHÉM TRÚNG KHỐI HỢP LỆ!
            IUHCombatAudio.PlaySword();
            m_ComboCount++;
            ShowComboUI();

            if (s.shapeType == ShapeType.EnergyStar)
            {
                themeColor = new Color(1f, 0.92f, 0.15f, 1f);
                // HỒI MÁU CHO NGƯỜI CHƠI
                HealPlayer(20f);
                IUHCombatAudio.PlayHeal();
                IUHCombatFloatingText.Spawn(hitPos, "💚 +20 HP HỒI PHỤC!", new Color(0.3f, 1f, 0.5f), 1.3f);
            }
            else if (s.shapeType == ShapeType.DataPyramid)
            {
                themeColor = new Color(0.85f, 0.2f, 1f, 1f);
                // SÓNG XUNG KÍCH SÁT THƯƠNG LAN
                DamageBoss(80f);
                IUHCombatAudio.PlaySkillBurst();
                IUHCombatFloatingText.Spawn(hitPos, "⚡ SÓNG XUNG KÍCH -80 HP!", themeColor, 1.5f);
            }
            else // DataCube
            {
                themeColor = new Color(0.15f, 0.95f, 1f, 1f);
                // SÁT THƯƠNG CƠ BẢN
                float dmg = 50f + m_ComboCount * 6f;
                DamageBoss(dmg);
                IUHCombatFloatingText.Spawn(hitPos, $"⚔️ CHÉM BỔ ĐÔI -{dmg:F0}!", themeColor, 1.2f);
            }

            // === BỘ HIỆU ỨNG CHÉM CỰC MẮT (CYBER CUT VFX SUITE) === //
            // 1. Vệt sáng đường chém sắc lẹm (Glowing Neon Slice Plane)
            SpawnNeonSliceArc(hitPos, cutDir, themeColor);

            // 2. Bùng nổ cụm tia lửa điện & mảnh năng lượng (Cyber Sparks Eruption)
            SpawnCyberSparks(hitPos, cutDir, themeColor, 32);

            // 3. Vòng sóng xung kích công nghệ giãn nở (Shockwave Hologram Ring)
            SpawnShockwaveRing(hitPos, themeColor, 1.25f);

            // 4. Giật rung màn hình tạo cảm giác lực chém (Micro Camera Punch)
            TriggerScreenPunch(0.06f, 0.018f);

            // Tích nạp năng lượng Ultimate
            m_UltimateEnergy = Mathf.Min(100f, m_UltimateEnergy + 15f);
            UpdateHPDisplays();

            // TẠO 2 NỬA VĂNG SANG 2 BÊN THEO HƯỚNG VẾT CHÉM (DYNAMIC DISSOLVE SLICES)
            SpawnSlicedHalves(s, cutDir, themeColor);

            Destroy(s.gameObject);
            m_ActiveShapes.RemoveAt(index);
        }

        private void SpawnNeonSliceArc(Vector3 hitPos, Vector3 cutDir, Color color)
        {
            GameObject arcGO = new GameObject("VFX_Slice_Arc");
            arcGO.transform.position = hitPos;
            if (cutDir.sqrMagnitude > 0.001f)
            {
                arcGO.transform.rotation = Quaternion.LookRotation(cutDir, m_ARCamera != null ? m_ARCamera.transform.forward : Vector3.up);
            }

            GameObject bladeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bladeMesh.transform.SetParent(arcGO.transform, false);
            bladeMesh.transform.localScale = new Vector3(0.015f, 0.09f, 0.85f);
            var col = bladeMesh.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Texture");
            var mat = new Material(sh);
            mat.color = color * 1.5f;
            bladeMesh.GetComponent<Renderer>().sharedMaterial = mat;

            StartCoroutine(AnimateSliceArcRoutine(arcGO, bladeMesh, mat, color));
        }

        private IEnumerator AnimateSliceArcRoutine(GameObject root, GameObject bladeMesh, Material mat, Color baseColor)
        {
            float duration = 0.18f;
            float elapsed = 0f;
            Vector3 initScale = bladeMesh.transform.localScale;

            while (elapsed < duration && bladeMesh != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bladeMesh.transform.localScale = new Vector3(
                    Mathf.Lerp(initScale.x * 1.5f, 0f, t),
                    Mathf.Lerp(initScale.y * 1.4f, 0f, t),
                    Mathf.Lerp(initScale.z * 0.4f, initScale.z * 1.6f, t)
                );

                Color c = baseColor;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                mat.color = c;

                yield return null;
            }

            if (root != null) Destroy(root);
        }

        private void SpawnCyberSparks(Vector3 hitPos, Vector3 cutDir, Color color, int count)
        {
            GameObject sparksContainer = new GameObject("VFX_Cyber_Sparks");
            sparksContainer.transform.position = hitPos;

            Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Texture");
            var sparkMat = new Material(sh);
            sparkMat.color = color * 1.6f;

            for (int i = 0; i < count; i++)
            {
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spark.transform.SetParent(sparksContainer.transform, false);
                spark.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.018f, 0.042f);
                var col = spark.GetComponent<Collider>();
                if (col != null) Destroy(col);
                spark.GetComponent<Renderer>().sharedMaterial = sparkMat;

                Vector3 randomDir = (UnityEngine.Random.onUnitSphere + cutDir * 0.8f).normalized;
                float speed = UnityEngine.Random.Range(2.4f, 5.5f);
                StartCoroutine(AnimateSparkParticle(spark, randomDir * speed, sparkMat, color));
            }

            Destroy(sparksContainer, 0.45f);
        }

        private IEnumerator AnimateSparkParticle(GameObject spark, Vector3 velocity, Material mat, Color baseColor)
        {
            float lifetime = UnityEngine.Random.Range(0.24f, 0.38f);
            float elapsed = 0f;
            Vector3 initScale = spark.transform.localScale;

            while (elapsed < lifetime && spark != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                spark.transform.position += velocity * Time.deltaTime;
                velocity += Vector3.down * 4.5f * Time.deltaTime;
                spark.transform.localScale = Vector3.Lerp(initScale, Vector3.zero, t);

                yield return null;
            }
        }

        private void SpawnShockwaveRing(Vector3 hitPos, Color color, float maxRadius)
        {
            GameObject ringGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringGO.name = "VFX_Shockwave_Ring";
            ringGO.transform.position = hitPos;
            var col = ringGO.GetComponent<Collider>();
            if (col != null) Destroy(col);

            if (m_ARCamera != null)
            {
                ringGO.transform.rotation = Quaternion.LookRotation(m_ARCamera.transform.forward, m_ARCamera.transform.up) * Quaternion.Euler(90f, 0f, 0f);
            }

            Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Texture");
            var mat = new Material(sh);
            mat.color = color;
            ringGO.GetComponent<Renderer>().sharedMaterial = mat;

            StartCoroutine(AnimateShockwaveRoutine(ringGO, mat, color, maxRadius));
        }

        private IEnumerator AnimateShockwaveRoutine(GameObject ring, Material mat, Color baseColor, float maxRadius)
        {
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration && ring != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = Mathf.Sin(t * Mathf.PI * 0.5f);

                float currentRadius = Mathf.Lerp(0.12f, maxRadius, ease);
                ring.transform.localScale = new Vector3(currentRadius, 0.005f, currentRadius);

                Color c = baseColor;
                c.a = Mathf.Lerp(0.85f, 0f, t);
                mat.color = c;

                yield return null;
            }

            if (ring != null) Destroy(ring);
        }

        private void TriggerScreenPunch(float duration, float magnitude)
        {
            StartCoroutine(ScreenPunchRoutine(duration, magnitude));
        }

        private IEnumerator ScreenPunchRoutine(float duration, float magnitude)
        {
            if (m_ARCamera == null) yield break;

            Vector3 originalLocalPos = m_ARCamera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = 1f - (elapsed / duration);
                Vector3 offset = (UnityEngine.Random.insideUnitSphere * magnitude) * percent;
                m_ARCamera.transform.localPosition = originalLocalPos + offset;
                yield return null;
            }

            m_ARCamera.transform.localPosition = originalLocalPos;
        }

        private void SpawnSlicedHalves(FlyingCyberShape s, Vector3 cutDir, Color color)
        {
            if (s.gameObject == null) return;

            Vector3 pos = s.gameObject.transform.position;
            Vector3 scale = s.gameObject.transform.localScale;
            Material mat = s.gameObject.GetComponent<Renderer>().sharedMaterial;

            Vector3 pushNormal = m_ARCamera != null 
                ? Vector3.Cross(cutDir, m_ARCamera.transform.forward).normalized 
                : Vector3.right;
            if (pushNormal.sqrMagnitude < 0.01f) pushNormal = Vector3.right;

            for (int i = 0; i < 2; i++)
            {
                PrimitiveType pType = (s.shapeType == ShapeType.DataPyramid) ? PrimitiveType.Capsule : (s.shapeType == ShapeType.EnergyStar ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                GameObject half = GameObject.CreatePrimitive(pType);
                half.name = "Cyber_Half_Piece";
                Vector3 halfOffset = (i == 0 ? -pushNormal : pushNormal) * 0.09f;
                half.transform.position = pos + halfOffset;
                half.transform.localScale = new Vector3(scale.x * 0.5f, scale.y * 0.85f, scale.z * 0.5f);
                half.GetComponent<Renderer>().sharedMaterial = mat;

                var col = half.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var rb = half.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.mass = 0.4f;

                Vector3 pushVelocity = (i == 0 ? -pushNormal : pushNormal) * UnityEngine.Random.Range(2.2f, 3.6f) + Vector3.up * 1.5f;
                rb.linearVelocity = pushVelocity;
                rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 24f;

                m_SlicedHalves.Add(half);
                StartCoroutine(DissolveHalfPiece(half, 1.2f));
            }
        }

        private IEnumerator DissolveHalfPiece(GameObject half, float lifetime)
        {
            if (half == null) yield break;
            float elapsed = 0f;
            Vector3 initScale = half.transform.localScale;

            while (elapsed < lifetime && half != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;
                half.transform.localScale = Vector3.Lerp(initScale, Vector3.zero, t * t);
                yield return null;
            }

            if (half != null) Destroy(half);
        }

        private void UpdateFlyingShapes()
        {
            if (m_BossCurrentHp <= 0f)
            {
                ClearAllActiveShapes();
                return;
            }

            float dt = Time.deltaTime;
            Vector3 gravity = Vector3.down * 3.8f; // Trọng lực ảo nhẹ nhàng để dễ chém

            for (int i = m_ActiveShapes.Count - 1; i >= 0; i--)
            {
                var s = m_ActiveShapes[i];
                if (s == null || s.gameObject == null)
                {
                    m_ActiveShapes.RemoveAt(i);
                    continue;
                }

                s.lifetime += dt;
                s.velocity += gravity * dt;
                s.gameObject.transform.position += s.velocity * dt;
                s.gameObject.transform.Rotate(s.rotationAxis, s.rotationSpeed * dt);

                // Nếu khối rơi xuống quá sâu dưới mặt sàn bàn hoặc tồn tại quá 3.5s -> Tự hủy
                if (s.lifetime > 3.8f || (m_MonsterAnchor != null && s.gameObject.transform.position.y < m_MonsterAnchor.transform.position.y - 0.6f))
                {
                    Destroy(s.gameObject);
                    m_ActiveShapes.RemoveAt(i);
                }
            }
        }

        private void UpdateSlicedHalves()
        {
            m_SlicedHalves.RemoveAll(h => h == null);
        }

        // ======================= ULTIMATE CYBER SLASH (SLOW-MOTION) ======================= //

        private IEnumerator TriggerUltimateSlashPhase()
        {
            m_IsUltimatePhase = true;
            ClearAllActiveShapes();

            // Hiệu ứng Slow-motion kịch tính
            Time.timeScale = 0.35f;

            m_InstructionBanner.text = "⚡ ĐÃ TÍCH ĐỦ NĂNG LƯỢNG TỐI THƯỢNG! CHÉM ĐÔI QUÁI VẬT!";
            m_InstructionBanner.color = new Color(1f, 0.95f, 0.2f);

            if (m_UltimateCutLineUI != null)
            {
                m_UltimateCutLineUI.SetActive(true);
            }

            IUHCombatAudio.PlayEnergy();
            IUHCombatAudio.PlayBossRoar();

            float timer = 0f;
            while (m_IsUltimatePhase && timer < 3.5f)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Hết giờ hoặc đã chém xong -> khôi phục thời gian
            Time.timeScale = 1.0f;
            if (m_UltimateCutLineUI != null) m_UltimateCutLineUI.SetActive(false);
            m_IsUltimatePhase = false;
        }

        private void CheckUltimateSlash(Vector3 startScreen, Vector3 endScreen)
        {
            // Kiểm tra đường vuốt chéo dứt khoát trên màn hình
            float swipeLength = Vector3.Distance(startScreen, endScreen);
            if (swipeLength > 200f) // Đường vuốt dài dứt khoát
            {
                ExecuteUltimateSlashSuccess();
            }
        }

        private void ExecuteUltimateSlashSuccess()
        {
            m_IsUltimatePhase = false;
            Time.timeScale = 1.0f;
            if (m_UltimateCutLineUI != null) m_UltimateCutLineUI.SetActive(false);

            m_BossCurrentHp = 0f;
            UpdateHPDisplays();

            IUHCombatAudio.PlaySkillBurst();
            IUHCombatAudio.PlayBossHit();
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
            OnMonsterDefeated();
        }

        private void OnMonsterDefeated()
        {
            // 1. Dừng ngay lập tức việc ném các khối dữ liệu và tắt trạng thái chiến đấu
            m_IsCombatActive = false;
            m_IsUltimatePhase = false;
            Time.timeScale = 1.0f;

            StopAllCoroutines();
            ClearAllActiveShapes();
            CloseCombatUI();

            // 2. Hiệu ứng tiểu quỷ tan rã nhanh và hiển thị ngay lượt quét tiếp theo
            if (m_MonsterVisual != null && m_MonsterAnchor != null)
            {
                StartCoroutine(DisintegrateAndSwitchToNextScanRoutine());
            }
            else
            {
                ProceedToNextScan();
            }
        }

        private IEnumerator DisintegrateAndSwitchToNextScanRoutine()
        {
            // Quái vật thu nhỏ tan biến nhanh trong 0.35s
            if (m_MonsterAnchor != null)
            {
                float t = 0f;
                Vector3 origScale = m_MonsterAnchor.transform.localScale;

                while (t < 1f)
                {
                    t += Time.deltaTime * 3.0f;
                    m_MonsterAnchor.transform.localScale = Vector3.Lerp(origScale, Vector3.zero, t);
                    yield return null;
                }

                m_MonsterAnchor.SetActive(false);
            }

            // Xóa sạch triệt để mọi khối bay còn sót lại
            ClearAllActiveShapes();
            CloseCombatUI();

            IUHCombatAudio.PlaySkillBurst();

            // HIỂN THỊ NGAY LƯỢT QUÉT TIẾP THEO
            ProceedToNextScan();
        }

        // ======================= DAMAGE & RECOVERY ======================= //

        private void DamageBoss(float amount)
        {
            if (!m_IsCombatActive || m_BossCurrentHp <= 0f) return;

            m_BossCurrentHp = Mathf.Max(0f, m_BossCurrentHp - amount);
            UpdateHPDisplays();

            // Quái vật giật lùi co rúm người trúng đòn
            if (m_MonsterVisual != null)
            {
                var livingAnim = m_MonsterVisual.GetComponent<IUHARMonsterLivingController>();
                if (livingAnim != null) livingAnim.TriggerHitReaction();
            }

            // KHI QUÁI VẬT HẾT MÁU -> DỪNG NGAY VIỆC THẢ ĐỒ VẬT VÀ XÓA SẠCH MỌI KHỐI ĐANG BAY
            if (m_BossCurrentHp <= 0f)
            {
                OnMonsterDefeated();
            }
        }

        private void DamagePlayer(float amount)
        {
            m_PlayerCurrentHp = Mathf.Max(0f, m_PlayerCurrentHp - amount);
            UpdateHPDisplays();

            if (m_PlayerCurrentHp <= 0f)
            {
                LoseCombat();
            }
        }

        private void HealPlayer(float amount)
        {
            m_PlayerCurrentHp = Mathf.Min(m_PlayerMaxHp, m_PlayerCurrentHp + amount);
            UpdateHPDisplays();
        }

        private void ShowComboUI()
        {
            if (m_ComboText != null)
            {
                m_ComboText.gameObject.SetActive(true);
                string title = m_ComboCount >= 10 ? "🔥 UNSTOPPABLE" : (m_ComboCount >= 5 ? "⚡ CRITICAL CUT" : "⚔️ COMBO");
                m_ComboText.text = $"{title} x{m_ComboCount}!";
                StopCoroutine("AnimateComboBounceRoutine");
                StartCoroutine(AnimateComboBounceRoutine());
            }
        }

        private IEnumerator AnimateComboBounceRoutine()
        {
            if (m_ComboText == null) yield break;
            float t = 0f;
            Vector3 targetScale = Vector3.one * 1.0f;
            m_ComboText.transform.localScale = Vector3.one * 1.55f;

            while (t < 0.22f)
            {
                t += Time.deltaTime;
                float progress = t / 0.22f;
                m_ComboText.transform.localScale = Vector3.Lerp(Vector3.one * 1.55f, targetScale, progress);
                yield return null;
            }
            m_ComboText.transform.localScale = targetScale;
        }

        private void UpdateHPDisplays()
        {
            if (m_BossHpFill != null) m_BossHpFill.fillAmount = m_BossCurrentHp / m_BossMaxHp;
            if (m_BossHpText != null) m_BossHpText.text = $"👹 TIỂU QUỶ DỮ LIỆU: {m_BossCurrentHp:F0} / {m_BossMaxHp:F0}";

            if (m_PlayerHpFill != null) m_PlayerHpFill.fillAmount = m_PlayerCurrentHp / m_PlayerMaxHp;
            if (m_PlayerHpText != null) m_PlayerHpText.text = $"🛡️ SINH VIÊN: {m_PlayerCurrentHp:F0} / {m_PlayerMaxHp:F0}";

            if (m_UltimateEnergyFill != null) m_UltimateEnergyFill.fillAmount = m_UltimateEnergy / 100f;
            if (m_UltimateEnergyText != null) m_UltimateEnergyText.text = $"⚡ NĂNG LƯỢNG TỐI THƯỢNG: {m_UltimateEnergy:F0}%";
        }

        private void WinCombat()
        {
            m_IsCombatActive = false;
            m_IsUltimatePhase = false;
            Time.timeScale = 1.0f;
            StopAllCoroutines();
            ClearAllActiveShapes();
            if (m_BladeTrail != null) m_BladeTrail.emitting = false;

            IUHCombatAudio.PlayCoin();

            if (m_MonsterAnchor != null)
            {
                m_MonsterAnchor.SetActive(false);
            }

            if (m_VictoryModal != null)
            {
                m_VictoryModal.SetActive(true);
                StartCoroutine(AutoTransitionToNextScanRoutine(5f));
            }
        }

        private IEnumerator AutoTransitionToNextScanRoutine(float countdown)
        {
            float remaining = countdown;
            while (remaining > 0f)
            {
                if (m_VictoryCountdownText != null)
                {
                    m_VictoryCountdownText.text = $"⏱️ Tự động chuyển sang quét vật thể tiếp theo sau {Mathf.CeilToInt(remaining)}s...\n(Hoặc chọn [Quét tiếp] / [Về sân trường])";
                }
                yield return new WaitForSeconds(1.0f);
                remaining -= 1.0f;
            }

            // Hết đếm ngược -> tự động chuyển sang quét vật thể kế tiếp
            if (m_VictoryModal != null && m_VictoryModal.activeSelf)
            {
                ProceedToNextScan();
            }
        }

        public void ProceedToNextScan()
        {
            StopAllCoroutines();
            CloseCombatUI();

            if (IUHLiveARController.Instance != null)
            {
                IUHLiveARController.Instance.PrepareNextObjectScan();
            }
            else
            {
                ExitCombatToCampus();
            }
        }

        private void LoseCombat()
        {
            m_IsCombatActive = false;
            Time.timeScale = 1.0f;
            StopAllCoroutines();
            ClearAllActiveShapes();

            if (m_DefeatModal != null) m_DefeatModal.SetActive(true);
        }

        public void CloseCombatUI()
        {
            m_IsCombatActive = false;
            m_IsUltimatePhase = false;
            Time.timeScale = 1.0f;
            StopAllCoroutines();
            ClearAllActiveShapes();

            if (m_CombatCanvasRoot != null) m_CombatCanvasRoot.SetActive(false);
            if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
            if (m_DefeatModal != null) m_DefeatModal.SetActive(false);
            if (m_BladeTrail != null) m_BladeTrail.emitting = false;
        }

        public void ExitCombatToCampus()
        {
            CloseCombatUI();

            if (IUHLiveARController.Instance != null)
            {
                IUHLiveARController.Instance.CloseARMode();
            }

            if (IUHCampus.UI.IUHMobileControlsUI.Instance != null)
            {
                IUHCampus.UI.IUHMobileControlsUI.Instance.gameObject.SetActive(true);
            }

            var mainCam = Camera.main ?? GameObject.Find("MainCamera")?.GetComponent<Camera>();
            if (mainCam != null) mainCam.gameObject.SetActive(true);

            IUHCombatAudio.PlayShield();
        }

        private void ClearAllActiveShapes()
        {
            foreach (var s in m_ActiveShapes)
            {
                if (s != null && s.gameObject != null) Destroy(s.gameObject);
            }
            m_ActiveShapes.Clear();

            foreach (var h in m_SlicedHalves)
            {
                if (h != null) Destroy(h);
            }
            m_SlicedHalves.Clear();

            // Quét dọn triệt để mọi khối hoặc mảnh chém còn sót lại trong cảnh
            var allGos = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            for (int i = 0; i < allGos.Length; i++)
            {
                var go = allGos[i];
                if (go == null) continue;
                string gName = go.name;
                if (gName.StartsWith("Cyber_") || gName.StartsWith("Energy_") || 
                    gName.StartsWith("Data_") || gName.StartsWith("Spike_"))
                {
                    Destroy(go);
                }
            }
        }

        private float DistancePointToSegment(Vector2 point, Vector2 segA, Vector2 segB)
        {
            float l2 = (segB - segA).sqrMagnitude;
            if (l2 < 0.0001f) return Vector2.Distance(point, segA);
            float t = Mathf.Clamp01(Vector2.Dot(point - segA, segB - segA) / l2);
            Vector2 projection = segA + t * (segB - segA);
            return Vector2.Distance(point, projection);
        }
    }
}
