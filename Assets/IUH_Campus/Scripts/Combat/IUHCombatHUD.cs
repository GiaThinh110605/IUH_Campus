using System;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Combat
{
    public class IUHCombatHUD : MonoBehaviour
    {
        public static IUHCombatHUD Instance { get; private set; }

        [Header("Crosshair & Anti-Air HUD")]
        [SerializeField] private GameObject m_CrosshairRoot;

        [Header("Controls Guide")]
        [SerializeField] private Button m_BtnOpenGuide;
        [SerializeField] private GameObject m_GuideModalRoot;
        [SerializeField] private Button m_BtnCloseGuide;

        public bool IsGuideOpen => m_GuideModalRoot != null && m_GuideModalRoot.activeSelf;

        private GameObject m_AntiAirRingGo;
        private Image m_AntiAirRingImage;
        private GameObject m_LockOnTextGo;
        private Text m_LockOnText;
        private Image m_CrosshairDotImage;

        // Drone Supply Widget
        private GameObject m_SupplyWidgetGo;
        private Text m_SupplyStatusText;
        private Image m_SupplyIcon;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (m_BtnOpenGuide != null) m_BtnOpenGuide.onClick.AddListener(OpenGuide);
            if (m_BtnCloseGuide != null) m_BtnCloseGuide.onClick.AddListener(CloseGuide);

            if (m_GuideModalRoot != null) m_GuideModalRoot.SetActive(false);

            SetupAntiAirUI();
            SetupDroneSupplyUI();
        }

        private void SetupDroneSupplyUI()
        {
            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;

            m_SupplyWidgetGo = new GameObject("Drone_Supply_Status_Widget", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            m_SupplyWidgetGo.transform.SetParent(canvas.transform, false);

            var rt = m_SupplyWidgetGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(25f, -112f);
            rt.sizeDelta = new Vector2(280f, 32f);

            var bgImg = m_SupplyWidgetGo.GetComponent<Image>();
            bgImg.color = new Color(0.08f, 0.12f, 0.18f, 0.85f);

            // Text
            var textGo = new GameObject("Supply_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(m_SupplyWidgetGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10f, 0f);
            textRt.offsetMax = new Vector2(-10f, 0f);

            m_SupplyStatusText = textGo.GetComponent<Text>();
            m_SupplyStatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            m_SupplyStatusText.fontSize = 12;
            m_SupplyStatusText.fontStyle = FontStyle.Bold;
            m_SupplyStatusText.alignment = TextAnchor.MiddleLeft;
            m_SupplyStatusText.color = new Color(0.2f, 1f, 0.5f);
        }

        private void SetupAntiAirUI()
        {
            if (m_CrosshairRoot == null) m_CrosshairRoot = GameObject.Find("Crosshair");
            if (m_CrosshairRoot != null)
            {
                m_CrosshairDotImage = m_CrosshairRoot.GetComponent<Image>();

                // Outer Anti-Air Targeting Ring
                m_AntiAirRingGo = new GameObject("AntiAir_Aim_Ring", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                m_AntiAirRingGo.transform.SetParent(m_CrosshairRoot.transform, false);
                var ringRt = m_AntiAirRingGo.GetComponent<RectTransform>();
                ringRt.sizeDelta = new Vector2(56f, 56f);
                ringRt.anchoredPosition = Vector2.zero;

                m_AntiAirRingImage = m_AntiAirRingGo.GetComponent<Image>();
                m_AntiAirRingImage.color = new Color(0.2f, 0.95f, 1f, 0.7f);
                m_AntiAirRingGo.SetActive(false);

                // Lock-On Status Text Label below reticle
                m_LockOnTextGo = new GameObject("LockOn_Target_Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                m_LockOnTextGo.transform.SetParent(m_CrosshairRoot.transform, false);
                var textRt = m_LockOnTextGo.GetComponent<RectTransform>();
                textRt.sizeDelta = new Vector2(380f, 44f);
                textRt.anchoredPosition = new Vector2(0f, -48f);

                m_LockOnText = m_LockOnTextGo.GetComponent<Text>();
                m_LockOnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                m_LockOnText.fontSize = 13;
                m_LockOnText.fontStyle = FontStyle.Bold;
                m_LockOnText.alignment = TextAnchor.MiddleCenter;
                m_LockOnText.color = new Color(1f, 0.85f, 0.2f);
                var outline = m_LockOnTextGo.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                outline.effectDistance = new Vector2(1f, -1f);

                m_LockOnTextGo.SetActive(false);
            }
        }

        private void Update()
        {
            // Toggle Controls Guide with [H] or close with [ESC]
            if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleGuide();
            }
            else if (IsGuideOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseGuide();
            }

            UpdateAntiAirHUD();
            UpdateDroneSupplyUI();
        }

        private void UpdateDroneSupplyUI()
        {
            if (m_SupplyStatusText == null) return;
            var mgr = IUHEmergencyDroneSupplyManager.Instance;
            var playerHp = IUHPlayerHealth.Instance;
            if (mgr == null) return;

            if (mgr.IsSupplyReady)
            {
                if (playerHp != null && playerHp.CurrentHp <= 35)
                {
                    m_SupplyStatusText.color = new Color(1f, 0.25f, 0.2f);
                    m_SupplyStatusText.text = "🚨 <color=#FF3344>MÁU YẾU!</color> [T] GỌI TIẾP TẾ NGAY!";
                }
                else
                {
                    m_SupplyStatusText.color = new Color(0.2f, 1f, 0.5f);
                    m_SupplyStatusText.text = "📦 TIẾP TẾ DRONE <color=#FFDD44>[T]</color>: <color=#00FF88>SẴN SÀNG</color>";
                }
            }
            else
            {
                m_SupplyStatusText.color = new Color(0.8f, 0.85f, 0.9f, 0.75f);
                m_SupplyStatusText.text = $"⏳ TIẾP TẾ DRONE [T]: NẠP LẠI ({mgr.SupplyCooldownRemaining:0.0}s)";
            }
        }

        private void UpdateAntiAirHUD()
        {
            var combatMgr = IUHCampusCombatManager.Instance;
            if (combatMgr == null || m_AntiAirRingGo == null) return;

            bool isAiming = combatMgr.IsAiming;
            var lockedTarget = combatMgr.LockedAirTarget;

            if (isAiming)
            {
                m_AntiAirRingGo.SetActive(true);
                m_AntiAirRingGo.transform.Rotate(Vector3.forward, 45f * Time.deltaTime);

                if (lockedTarget != null && !lockedTarget.IsDead)
                {
                    // Locked onto flying enemy!
                    float dist = Vector3.Distance(Camera.main.transform.position, lockedTarget.transform.position);
                    m_AntiAirRingImage.color = new Color(1f, 0.25f, 0.15f, 0.9f);
                    if (m_CrosshairDotImage != null) m_CrosshairDotImage.color = new Color(1f, 0.2f, 0.15f, 1f);

                    m_LockOnTextGo.SetActive(true);
                    m_LockOnText.color = new Color(1f, 0.35f, 0.15f);
                    m_LockOnText.text = $"[ 🎯 ĐÃ KHÓA: {lockedTarget.name} ({dist:0}M) ]\n<color=#FFDD44>[Q]</color> Phóng Tên Lửa Phòng Không | <color=#00E5FF>[L-Click]</color> Bắn Plasma";
                }
                else
                {
                    m_AntiAirRingImage.color = new Color(0.2f, 0.95f, 1f, 0.75f);
                    if (m_CrosshairDotImage != null) m_CrosshairDotImage.color = new Color(0.2f, 0.95f, 1f, 0.9f);

                    m_LockOnTextGo.SetActive(true);
                    m_LockOnText.color = new Color(0.7f, 0.95f, 1f, 0.8f);
                    m_LockOnText.text = "<color=#00E5FF>🔭 NGẮM CAO XẠ ADS</color> - Hướng vào Phi Cơ để Khóa Mục Tiêu";
                }
            }
            else
            {
                m_AntiAirRingGo.SetActive(false);
                m_LockOnTextGo.SetActive(false);
                if (m_CrosshairDotImage != null) m_CrosshairDotImage.color = new Color(0.2f, 0.95f, 1f, 0.75f);
            }
        }

        public void ToggleGuide()
        {
            if (IsGuideOpen) CloseGuide();
            else OpenGuide();
        }

        public void OpenGuide()
        {
            if (m_GuideModalRoot != null) m_GuideModalRoot.SetActive(true);
            SetUIMode(true);
        }

        public void CloseGuide()
        {
            if (m_GuideModalRoot != null) m_GuideModalRoot.SetActive(false);
            SetUIMode(false);
        }

        private void SetUIMode(bool isOpen)
        {
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;

            var student = GameObject.Find("Campus Student");
            if (student != null)
            {
                var tpc = student.GetComponent<StarterAssets.ThirdPersonController>();
                if (tpc != null) tpc.enabled = !isOpen;

                var previewCtrl = student.GetComponent<IUHCampus.World.IUHConceptPreviewController>();
                if (previewCtrl != null) previewCtrl.Apply();
            }
        }
    }
}

