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

        private static Material CreateUnlitMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Texture");
            var mat = new Material(shader);
            mat.color = color;
            return mat;
        }

        private void BuildCombatUI()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf")
                ?? Font.CreateDynamicFontFromOSFont("Helvetica", 16)
                ?? Font.CreateDynamicFontFromOSFont("Arial", 16);

            CreateCombatCanvas();
            BuildBossHealthBar(font);
            BuildPlayerStatusBars(font);
            BuildHudOverlays(font);
            BuildEndGameModals(font);

            m_CombatCanvasRoot.SetActive(false);
        }

        private void CreateCombatCanvas()
        {
            m_CombatCanvasRoot = new GameObject("IUH_AR_CYBER_SLASH_CANVAS", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_CombatCanvasRoot.transform.SetParent(transform, false);

            var canvas = m_CombatCanvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;

            var scaler = m_CombatCanvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildBossHealthBar(Font font)
        {
            GameObject bossBarGO = CreatePanel(m_CombatCanvasRoot.transform, "Boss_HP_Container",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -18f), new Vector2(620f, 54f), new Color(0.05f, 0.08f, 0.16f, 0.92f));

            m_BossHpFill = CreateFilledBar(bossBarGO.transform, "Fill",
                new Vector2(0.02f, 0.15f), new Vector2(0.98f, 0.85f), new Color(0.9f, 0.2f, 0.35f, 0.95f));
            m_BossHpText = CreateOutlinedText(bossBarGO.transform, "Text", font,
                "👹 TIỂU QUỶ DỮ LIỆU: 1000 / 1000", 15, Color.white, new Color(0.25f, 0f, 0.08f, 0.9f));
        }

        private void BuildPlayerStatusBars(Font font)
        {
            GameObject playerStatusGO = CreatePanel(m_CombatCanvasRoot.transform, "Player_Status_Container",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(25f, -85f), new Vector2(270f, 75f), new Color(0.04f, 0.07f, 0.14f, 0.92f));

            m_PlayerHpFill = CreateFilledBar(playerStatusGO.transform, "HP_Fill",
                new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.92f), new Color(0.2f, 0.95f, 0.45f, 0.95f));
            m_PlayerHpText = CreateOutlinedText(m_PlayerHpFill.transform, "HP_Text", font,
                "🛡️ SINH VIÊN: 100 / 100", 12, Color.white, new Color(0.02f, 0.15f, 0.05f, 0.9f));

            m_UltimateEnergyFill = CreateFilledBar(playerStatusGO.transform, "Ult_Fill",
                new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.48f), new Color(1f, 0.85f, 0.1f, 0.95f));
            m_UltimateEnergyText = CreateOutlinedText(m_UltimateEnergyFill.transform, "Ult_Text", font,
                "⚡ NĂNG LƯỢNG TỐI THƯỢNG: 0%", 11, Color.white, new Color(0.2f, 0.15f, 0f, 0.9f));
        }

        private void BuildHudOverlays(Font font)
        {
            CreateFeedbackButton(m_CombatCanvasRoot.transform, "Btn_Exit_Combat",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(25f, -22f), new Vector2(170f, 50f), new Color(0.85f, 0.15f, 0.2f, 0.92f),
                font, "✕ THOÁT", 15, ExitCombatToCampus);

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

            BuildUltimateCutLineUI(font);
        }

        private void BuildUltimateCutLineUI(Font font)
        {
            m_UltimateCutLineUI = CreatePanel(m_CombatCanvasRoot.transform, "Ultimate_Cut_Line",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700f, 6f), new Color(1f, 0.85f, 0.1f, 0.95f));
            m_UltimateCutLineUI.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, -25f);

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
        }

        private void BuildEndGameModals(Font font)
        {
            BuildVictoryModal(font);
            BuildDefeatModal(font);
        }

        private void BuildVictoryModal(Font font)
        {
            m_VictoryModal = CreatePanel(m_CombatCanvasRoot.transform, "Modal_Victory",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(640f, 360f), new Color(0.04f, 0.08f, 0.16f, 0.96f));

            var vt = CreateOutlinedText(m_VictoryModal.transform, "Title", font,
                "🏆 CHIẾN THẮNG TUYỆT ĐỐI!\nQUÁI VẬT ĐÃ BỊ CHÉM ĐÔI & THANH TẨY", 20,
                new Color(0.3f, 1f, 0.6f), new Color(0f, 0.2f, 0.1f, 0.9f));
            var vtRt = vt.GetComponent<RectTransform>();
            vtRt.anchorMin = new Vector2(0f, 0.68f); vtRt.anchorMax = Vector2.one; vtRt.sizeDelta = Vector2.zero;

            GameObject vDesc = new GameObject("Desc", typeof(RectTransform), typeof(Text));
            vDesc.transform.SetParent(m_VictoryModal.transform, false);
            var vdRt = vDesc.GetComponent<RectTransform>();
            vdRt.anchorMin = new Vector2(0.05f, 0.42f); vdRt.anchorMax = new Vector2(0.95f, 0.68f); vdRt.sizeDelta = Vector2.zero;
            var vd = vDesc.GetComponent<Text>();
            vd.font = font;
            vd.text = "✨ Đòn chém tối thượng đã giải thoát vật thể khỏi virus dữ liệu!\n🎁 Nhận thưởng: +300 IUH Coin & +80 Điểm Kinh Nghiệm";
            vd.fontSize = 14; vd.alignment = TextAnchor.MiddleCenter; vd.color = new Color(1f, 0.9f, 0.6f);

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

            CreateFeedbackButton(m_VictoryModal.transform, "Btn_ScanNext",
                new Vector2(0.05f, 0.08f), new Vector2(0.52f, 0.26f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.12f, 0.65f, 0.95f, 0.98f),
                font, "🔍 QUÉT VẬT THỂ TIẾP", 15, ProceedToNextScan);

            CreateFeedbackButton(m_VictoryModal.transform, "Btn_ExitCampus",
                new Vector2(0.55f, 0.08f), new Vector2(0.95f, 0.26f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.25f, 0.32f, 0.42f, 0.95f),
                font, "🚪 VỀ SÂN TRƯỜNG", 15, ExitCombatToCampus);

            m_VictoryModal.SetActive(false);
        }

        private void BuildDefeatModal(Font font)
        {
            m_DefeatModal = CreatePanel(m_CombatCanvasRoot.transform, "Modal_Defeat",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(580f, 290f), new Color(0.14f, 0.04f, 0.06f, 0.96f));

            var dt = CreateOutlinedText(m_DefeatModal.transform, "Title", font,
                "⚠️ THẤT BẠI!\nBỊ BOM ĐỘC GAI NHỌN HẠ GỤC", 20,
                new Color(1f, 0.3f, 0.35f), new Color(0.2f, 0f, 0f, 0.9f));
            var dtRt = dt.GetComponent<RectTransform>();
            dtRt.anchorMin = new Vector2(0f, 0.65f); dtRt.anchorMax = Vector2.one; dtRt.sizeDelta = Vector2.zero;

            CreateFeedbackButton(m_DefeatModal.transform, "Btn_Exit",
                new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0f),
                Vector2.zero, new Vector2(250f, 52f), new Color(0.85f, 0.2f, 0.25f, 0.95f),
                font, "RÚT LUI", 15, ExitCombatToCampus);

            m_DefeatModal.SetActive(false);
        }

        private static GameObject CreatePanel(
            Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Image CreateFilledBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.sizeDelta = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.color = color;
            return img;
        }

        private static Text CreateOutlinedText(
            Transform parent, string name, Font font, string text, int fontSize, Color color, Color outlineColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            var txt = go.GetComponent<Text>();
            txt.font = font; txt.text = text; txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold; txt.alignment = TextAnchor.MiddleCenter; txt.color = color;
            go.GetComponent<Outline>().effectColor = outlineColor;
            return txt;
        }

        private static void CreateFeedbackButton(
            Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, Font font, string label, int fontSize,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(IUHCampus.UI.IUHButtonFeedback));
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
            btnGO.GetComponent<Image>().color = bgColor;

            GameObject txtGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGO.transform.SetParent(btnGO.transform, false);
            var tRt = txtGO.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one; tRt.sizeDelta = Vector2.zero;
            var txt = txtGO.GetComponent<Text>();
            txt.font = font; txt.text = label; txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold; txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white; txt.raycastTarget = false;

            btnGO.GetComponent<Button>().onClick.AddListener(onClick);
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
            while (m_IsCombatActive && m_BossCurrentHp > 0 && m_PlayerCurrentHp > 0)
            {
                if (!m_IsUltimatePhase)
                {
                    yield return SpawnShapeWaveRoutine();
                    if (!m_IsCombatActive || m_BossCurrentHp <= 0f) break;

                    yield return new WaitForSeconds(UnityEngine.Random.Range(2.2f, 3.2f));
                    if (!m_IsCombatActive || m_BossCurrentHp <= 0f) break;

                    if (m_UltimateEnergy >= 100f || m_BossCurrentHp <= 250f)
                    {
                        yield return TriggerUltimateSlashPhase();
                    }
                }
                yield return null;
            }

            if (m_BossCurrentHp <= 0f) ClearAllActiveShapes();
        }

        private IEnumerator SpawnShapeWaveRoutine()
        {
            if (m_MonsterVisual != null)
            {
                var livingAnim = m_MonsterVisual.GetComponent<IUHARMonsterLivingController>();
                if (livingAnim != null) livingAnim.TriggerAttackRoar();
            }

            int shapeCount = UnityEngine.Random.Range(2, 4);
            for (int i = 0; i < shapeCount; i++)
            {
                if (!m_IsCombatActive || m_IsUltimatePhase || m_BossCurrentHp <= 0f) yield break;
                SpawnFlyingShape(PickRandomShapeType());
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 0.45f));
            }
        }

        private static ShapeType PickRandomShapeType()
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.22f) return ShapeType.SpikeBomb;
            if (roll < 0.45f) return ShapeType.EnergyStar;
            if (roll < 0.65f) return ShapeType.DataPyramid;
            return ShapeType.DataCube;
        }

        private void SpawnFlyingShape(ShapeType type)
        {
            if (m_ARCamera == null || m_MonsterAnchor == null || !m_IsCombatActive || m_BossCurrentHp <= 0f) return;

            GameObject shapeObj = CreateShapeVisual(type, out Color shapeColor);
            shapeObj.GetComponent<Renderer>().sharedMaterial = CreateUnlitMaterial(shapeColor);
            shapeObj.transform.position = m_MonsterAnchor.transform.position +
                new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), 0.2f, UnityEngine.Random.Range(-0.2f, 0.2f));

            float upForce = UnityEngine.Random.Range(2.2f, 3.4f);
            float sideForce = UnityEngine.Random.Range(-0.85f, 0.85f);
            float forwardForce = UnityEngine.Random.Range(-0.4f, 0.2f);
            Vector3 initVel = (Vector3.up * upForce) + (m_ARCamera.transform.right * sideForce) + (m_ARCamera.transform.forward * forwardForce);

            m_ActiveShapes.Add(new FlyingCyberShape
            {
                gameObject = shapeObj,
                shapeType = type,
                velocity = initVel,
                rotationAxis = UnityEngine.Random.onUnitSphere,
                rotationSpeed = UnityEngine.Random.Range(120f, 360f),
                isSliced = false,
                lifetime = 0f
            });
            IUHCombatAudio.PlayPunchWhoosh();
        }

        private static GameObject CreateShapeVisual(ShapeType type, out Color shapeColor)
        {
            GameObject shapeObj;
            if (type == ShapeType.DataCube)
            {
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shapeObj.name = "Cyber_Cube";
                shapeObj.transform.localScale = Vector3.one * 0.26f;
                shapeColor = new Color(0.15f, 0.9f, 1f, 0.95f);
            }
            else if (type == ShapeType.EnergyStar)
            {
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shapeObj.name = "Energy_Star";
                shapeObj.transform.localScale = new Vector3(0.28f, 0.05f, 0.28f);
                shapeColor = new Color(1f, 0.9f, 0.1f, 0.95f);
            }
            else if (type == ShapeType.DataPyramid)
            {
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                shapeObj.name = "Data_Pyramid";
                shapeObj.transform.localScale = new Vector3(0.22f, 0.35f, 0.22f);
                shapeColor = new Color(0.85f, 0.2f, 1f, 0.95f);
            }
            else
            {
                shapeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shapeObj.name = "Spike_Bomb";
                shapeObj.transform.localScale = Vector3.one * 0.32f;
                shapeColor = new Color(0.95f, 0.12f, 0.18f, 0.95f);
                AttachBombSpikes(shapeObj.transform);
            }

            var col = shapeObj.GetComponent<Collider>();
            if (col != null) Destroy(col);
            return shapeObj;
        }

        private static void AttachBombSpikes(Transform parent)
        {
            for (int g = 0; g < 4; g++)
            {
                var spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spike.transform.SetParent(parent, false);
                spike.transform.localScale = new Vector3(0.18f, 1.25f, 0.18f);
                spike.transform.localRotation = Quaternion.Euler(g * 45f, g * 45f, 0f);
                var spCol = spike.GetComponent<Collider>();
                if (spCol != null) Destroy(spCol);
            }
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
            ReadPointerState(out bool pointerDown, out bool pointerHeld, out Vector3 pointerPos);

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

            if (!m_IsPointerDown || m_ARCamera == null) return;

            Vector3 worldPos = m_ARCamera.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, 0.45f));
            if (m_BladeFollower != null) m_BladeFollower.transform.position = worldPos;

            if (Vector3.Distance(pointerPos, m_LastPointerScreenPos) <= 16f) return;

            CheckSliceShapes(m_LastPointerScreenPos, pointerPos);
            if (m_IsUltimatePhase) CheckUltimateSlash(m_LastPointerScreenPos, pointerPos);
            m_LastPointerScreenPos = pointerPos;
        }

        private static void ReadPointerState(out bool pointerDown, out bool pointerHeld, out Vector3 pointerPos)
        {
            pointerDown = false;
            pointerHeld = false;
            pointerPos = Vector3.zero;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                pointerPos = t.position;
                pointerDown = t.phase == TouchPhase.Began;
                pointerHeld = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            }
            else if (Input.GetMouseButton(0))
            {
                pointerPos = Input.mousePosition;
                pointerDown = Input.GetMouseButtonDown(0);
                pointerHeld = true;
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

            if (s.shapeType == ShapeType.SpikeBomb)
            {
                HandleSpikeBombHit(s, index, hitPos);
                return;
            }

            IUHCombatAudio.PlaySword();
            m_ComboCount++;
            ShowComboUI();

            Color themeColor = ApplyValidSliceReward(s.shapeType, hitPos);

            SpawnNeonSliceArc(hitPos, cutDir, themeColor);
            SpawnCyberSparks(hitPos, cutDir, themeColor, 32);
            SpawnShockwaveRing(hitPos, themeColor, 1.25f);
            TriggerScreenPunch(0.06f, 0.018f);

            m_UltimateEnergy = Mathf.Min(100f, m_UltimateEnergy + 15f);
            UpdateHPDisplays();

            SpawnSlicedHalves(s, cutDir, themeColor);
            Destroy(s.gameObject);
            m_ActiveShapes.RemoveAt(index);
        }

        private void HandleSpikeBombHit(FlyingCyberShape s, int index, Vector3 hitPos)
        {
            Color bombColor = new Color(1f, 0.18f, 0.12f, 1f);
            DamagePlayer(25f);
            m_ComboCount = 0;
            if (m_ComboText != null) m_ComboText.gameObject.SetActive(false);

            IUHCombatAudio.PlayBossHit();
            IUHCombatFloatingText.Spawn(hitPos, "💥 BOM ĐỘC PHÁT NỔ! -25 HP", bombColor, 1.5f);
            SpawnShockwaveRing(hitPos, bombColor, 1.8f);
            SpawnCyberSparks(hitPos, Vector3.up, bombColor, 45);
            TriggerScreenPunch(0.12f, 0.035f);

            Destroy(s.gameObject);
            m_ActiveShapes.RemoveAt(index);
        }

        private Color ApplyValidSliceReward(ShapeType shapeType, Vector3 hitPos)
        {
            if (shapeType == ShapeType.EnergyStar)
            {
                HealPlayer(20f);
                IUHCombatAudio.PlayHeal();
                IUHCombatFloatingText.Spawn(hitPos, "💚 +20 HP HỒI PHỤC!", new Color(0.3f, 1f, 0.5f), 1.3f);
                return new Color(1f, 0.92f, 0.15f, 1f);
            }
            if (shapeType == ShapeType.DataPyramid)
            {
                Color color = new Color(0.85f, 0.2f, 1f, 1f);
                DamageBoss(80f);
                IUHCombatAudio.PlaySkillBurst();
                IUHCombatFloatingText.Spawn(hitPos, "⚡ SÓNG XUNG KÍCH -80 HP!", color, 1.5f);
                return color;
            }

            Color cubeColor = new Color(0.15f, 0.95f, 1f, 1f);
            float dmg = 50f + m_ComboCount * 6f;
            DamageBoss(dmg);
            IUHCombatFloatingText.Spawn(hitPos, $"⚔️ CHÉM BỔ ĐÔI -{dmg:F0}!", cubeColor, 1.2f);
            return cubeColor;
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
                IUHCampus.UI.IUHMobileControlsUI.Instance.CheckPlatformVisibility();
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
