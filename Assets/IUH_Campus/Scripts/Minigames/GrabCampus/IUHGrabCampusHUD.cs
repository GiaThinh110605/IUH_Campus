using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Dedicated Gameplay & Survival HUD for Grab Campus ("Chuyến xe tâm linh").
    /// Matches the concept design specification:
    /// - Panel 1: Order Acceptance Card (Thẻ nhận đơn) with [Nhận đơn (E)]
    /// - Panel 2: In-Game Delivery Status (Pickup/Dropoff, timer, cargo integrity, speed)
    /// - Panel 4: Survival Items Hotbar (Đèn pin [F], Pin [1], Bùa [2], Pháo sáng [3], Gậy [Q], Sơ cứu [4])
    /// - Floating Notifications & Status Banner
    /// </summary>
    public class IUHGrabCampusHUD : MonoBehaviour
    {
        private static IUHGrabCampusHUD s_Instance;
        public static IUHGrabCampusHUD Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = UnityEngine.Object.FindAnyObjectByType<IUHGrabCampusHUD>(FindObjectsInactive.Include);
                }
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        [Header("Order Acceptance Board")]
        [SerializeField] private GameObject m_OrderBoardPanel;
        [SerializeField] private Text m_TxtOrderBoardTitle;
        [SerializeField] private Text m_TxtOrderBoardPickup;
        [SerializeField] private Text m_TxtOrderBoardDropoff;
        [SerializeField] private Text m_TxtOrderBoardTime;
        [SerializeField] private Text m_TxtOrderBoardReward;
        [SerializeField] private Text m_TxtOrderBoardDanger;
        [SerializeField] private Button m_BtnAcceptOrder;

        [Header("Survival Item Hotbar")]
        [SerializeField] private GameObject m_HotbarPanel;
        [SerializeField] private Text m_TxtSlotFlashlight;
        [SerializeField] private Text m_TxtSlotBattery;
        [SerializeField] private Text m_TxtSlotTalisman;
        [SerializeField] private Text m_TxtSlotFlare;
        [SerializeField] private Text m_TxtSlotBat;
        [SerializeField] private Text m_TxtSlotFirstAid;
        [SerializeField] private Button m_BtnSlotFlashlight;
        [SerializeField] private Button m_BtnSlotBattery;
        [SerializeField] private Button m_BtnSlotTalisman;
        [SerializeField] private Button m_BtnSlotFlare;
        [SerializeField] private Button m_BtnSlotBat;
        [SerializeField] private Button m_BtnSlotFirstAid;

        [Header("Notification Banner")]
        [SerializeField] private GameObject m_NotificationBanner;
        [SerializeField] private Text m_TxtNotification;
        private float m_NotificationTimer = 0f;

        private void Awake()
        {
            if (s_Instance == null) s_Instance = this;
            else if (s_Instance != this) { Destroy(gameObject); return; }

            EnsureHotbarBuilt();
            EnsureOrderBoardBuilt();
            EnsureNotificationBannerBuilt();
            WireHotbarButtons();
        }

        private void Start()
        {
            if (m_OrderBoardPanel != null) m_OrderBoardPanel.SetActive(false);
            if (m_NotificationBanner != null) m_NotificationBanner.SetActive(false);
            if (m_HotbarPanel != null) m_HotbarPanel.SetActive(false);
        }

        private void Update()
        {
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr == null) return;

            // Update Notification Timer
            if (m_NotificationTimer > 0f)
            {
                m_NotificationTimer -= Time.deltaTime;
                if (m_NotificationTimer <= 0f && m_NotificationBanner != null)
                {
                    m_NotificationBanner.SetActive(false);
                }
            }

            // Order Board Input [E]
            if (m_OrderBoardPanel != null && m_OrderBoardPanel.activeSelf)
            {
                if (IUHInputHelper.IsInteractKeyDown() || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnAcceptOrderClicked();
                }
                return;
            }

            // Hotbar Visibility & Data Update
            if (mgr.IsMissionActive)
            {
                if (m_HotbarPanel != null && !m_HotbarPanel.activeSelf)
                {
                    m_HotbarPanel.SetActive(true);
                }
                UpdateHotbarData();
            }
            else
            {
                if (m_HotbarPanel != null && m_HotbarPanel.activeSelf)
                {
                    m_HotbarPanel.SetActive(false);
                }
            }
        }

        public void ShowOrderBoard(IUHGrabCampusManager.DeliveryOrder order)
        {
            EnsureOrderBoardBuilt();
            if (m_OrderBoardPanel == null) return;

            if (order == null)
            {
                var mgr = IUHGrabCampusManager.Instance;
                if (mgr != null) order = mgr.CurrentOrder;
            }
            if (order == null)
            {
                order = new IUHGrabCampusManager.DeliveryOrder
                {
                    title = "🧋 Đơn #01: Trà Sữa Trân Châu ToCoToCo ➔ KTX",
                    pickupName = "Tiệm Trà Sữa ToCoToCo IUH",
                    dropoffName = "Ký Túc Xá KTX (Cổng 4)",
                    timeLimitSeconds = 90.0f,
                    cashReward = 85000,
                    expReward = 150,
                    difficultyStars = 1
                };
            }

            m_OrderBoardPanel.SetActive(true);
            m_OrderBoardPanel.transform.SetAsLastSibling();

            if (m_TxtOrderBoardTitle != null)
            {
                m_TxtOrderBoardTitle.text = $"<size=26><color=#FEF08A><b>🛵 ĐƠN HÀNG GIAO TỐC HÀNH</b></color></size>\n<size=18><color=#38BDF8><b>{order.title}</b></color></size>";
            }

            if (m_TxtOrderBoardPickup != null)
            {
                m_TxtOrderBoardPickup.text = $"📦 <b>Điểm nhận hàng:</b> {order.pickupName}";
            }

            if (m_TxtOrderBoardDropoff != null)
            {
                m_TxtOrderBoardDropoff.text = $"🏁 <b>Điểm giao hàng:</b> {order.dropoffName}";
            }

            if (m_TxtOrderBoardTime != null)
            {
                m_TxtOrderBoardTime.text = $"⏱ <b>Thời gian:</b> {order.timeLimitSeconds:F0} giây";
            }

            if (m_TxtOrderBoardReward != null)
            {
                m_TxtOrderBoardReward.text = $"💰 <b>Tiền thưởng:</b> +{order.cashReward:N0} VNĐ   ⭐ <b>+{order.expReward} EXP</b>";
            }

            if (m_TxtOrderBoardDanger != null)
            {
                string stars = new string('⭐', Mathf.Clamp(order.difficultyStars, 1, 5));
                m_TxtOrderBoardDanger.text = $"⚠️ <b>Mức độ nguy hiểm:</b> <color=#F87171>{stars}</color> {(order.difficultyStars >= 3 ? "(Ma truy đuổi dữ dội)" : "(Bóng ma rình rập)")}";
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void HideOrderBoard()
        {
            if (m_OrderBoardPanel != null) m_OrderBoardPanel.SetActive(false);
        }

        public void OnAcceptOrderClicked()
        {
            HideOrderBoard();
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr != null)
            {
                mgr.ConfirmOrderAccepted();
            }
        }

        public void ShowNotification(string message)
        {
            if (m_NotificationBanner == null) EnsureNotificationBannerBuilt();
            if (m_NotificationBanner == null) return;

            m_NotificationBanner.SetActive(true);
            m_NotificationBanner.transform.SetAsLastSibling();
            if (m_TxtNotification != null)
            {
                m_TxtNotification.text = message;
            }
            m_NotificationTimer = 3.2f;
        }

        private void UpdateHotbarData()
        {
            var inv = IUHGrabInventoryManager.Instance;
            if (inv == null) return;

            // Flashlight
            if (m_TxtSlotFlashlight != null)
            {
                string state = inv.isFlashlightOn ? "<color=#FACC15>ON</color>" : "<color=#94A3B8>OFF</color>";
                m_TxtSlotFlashlight.text = $"🔦 <b>[F] ĐÈN PIN</b>\n{state} ({inv.flashlightBattery:F0}%)";
            }

            // Battery
            if (m_TxtSlotBattery != null)
            {
                m_TxtSlotBattery.text = $"🔋 <b>[1] PIN</b>\nx{inv.batteries}";
            }

            // Talisman
            if (m_TxtSlotTalisman != null)
            {
                string state = inv.IsTalismanActive ? "<color=#FACC15>KHIÊN ON</color>" : $"x{inv.talismans}";
                m_TxtSlotTalisman.text = $"✨ <b>[2] BÙA</b>\n{state}";
            }

            // Flare
            if (m_TxtSlotFlare != null)
            {
                m_TxtSlotFlare.text = $"💥 <b>[3] PHÁO</b>\nx{inv.flares}";
            }

            // Bat
            if (m_TxtSlotBat != null)
            {
                m_TxtSlotBat.text = $"🏏 <b>[Q] GẬY</b>\n<color=#A78BFA>Sẵn sàng</color>";
            }

            // First Aid
            if (m_TxtSlotFirstAid != null)
            {
                m_TxtSlotFirstAid.text = $"🩹 <b>[4] SƠ CỨU</b>\nx{inv.firstAidKits}";
            }
        }

        private void WireHotbarButtons()
        {
            if (m_BtnSlotFlashlight != null)
            {
                m_BtnSlotFlashlight.onClick.RemoveAllListeners();
                m_BtnSlotFlashlight.onClick.AddListener(() => IUHGrabInventoryManager.Instance?.ToggleFlashlight());
            }

            if (m_BtnSlotBattery != null)
            {
                m_BtnSlotBattery.onClick.RemoveAllListeners();
                m_BtnSlotBattery.onClick.AddListener(() => IUHGrabInventoryManager.Instance?.UseBattery());
            }

            if (m_BtnSlotTalisman != null)
            {
                m_BtnSlotTalisman.onClick.RemoveAllListeners();
                m_BtnSlotTalisman.onClick.AddListener(() => IUHGrabInventoryManager.Instance?.UseTalisman());
            }

            if (m_BtnSlotFlare != null)
            {
                m_BtnSlotFlare.onClick.RemoveAllListeners();
                m_BtnSlotFlare.onClick.AddListener(() => IUHGrabInventoryManager.Instance?.UseFlare());
            }

            if (m_BtnSlotBat != null)
            {
                m_BtnSlotBat.onClick.RemoveAllListeners();
                m_BtnSlotBat.onClick.AddListener(() => IUHGrabInventoryManager.Instance?.UseDefenseBat());
            }

            if (m_BtnSlotFirstAid != null)
            {
                m_BtnSlotFirstAid.onClick.RemoveAllListeners();
                m_BtnSlotFirstAid.onClick.AddListener(() => IUHGrabInventoryManager.Instance?.UseFirstAidKit());
            }
        }

        // ================= PROCEDURAL UI BUILDERS =================

        private void EnsureOrderBoardBuilt()
        {
            if (m_OrderBoardPanel != null) return;

            var canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;

            var boardGO = new GameObject("Grab_OrderBoardModal");
            boardGO.transform.SetParent(parent, false);
            var rect = boardGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(580f, 440f);

            var bgImg = boardGO.AddComponent<Image>();
            bgImg.color = new Color(0.03f, 0.07f, 0.15f, 0.96f);
            var outline = boardGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.85f, 0.45f, 1.0f);
            outline.effectDistance = new Vector2(3f, 3f);

            // Title
            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(boardGO.transform, false);
            var tRect = titleGO.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.05f, 0.72f);
            tRect.anchorMax = new Vector2(0.95f, 0.96f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            m_TxtOrderBoardTitle = titleGO.AddComponent<Text>();
            m_TxtOrderBoardTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtOrderBoardTitle.fontSize = 20;
            m_TxtOrderBoardTitle.alignment = TextAnchor.MiddleCenter;
            m_TxtOrderBoardTitle.color = Color.yellow;
            m_TxtOrderBoardTitle.verticalOverflow = VerticalWrapMode.Overflow;
            m_TxtOrderBoardTitle.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Details Container
            var detailsGO = new GameObject("Details");
            detailsGO.transform.SetParent(boardGO.transform, false);
            var dRect = detailsGO.AddComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0.08f, 0.24f);
            dRect.anchorMax = new Vector2(0.92f, 0.70f);
            dRect.offsetMin = Vector2.zero;
            dRect.offsetMax = Vector2.zero;
            var vlg = detailsGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            m_TxtOrderBoardPickup = CreateDetailText(detailsGO.transform, "TxtPickup");
            m_TxtOrderBoardDropoff = CreateDetailText(detailsGO.transform, "TxtDropoff");
            m_TxtOrderBoardTime = CreateDetailText(detailsGO.transform, "TxtTime");
            m_TxtOrderBoardReward = CreateDetailText(detailsGO.transform, "TxtReward");
            m_TxtOrderBoardDanger = CreateDetailText(detailsGO.transform, "TxtDanger");

            // Accept Button [E]
            var btnGO = new GameObject("BtnAcceptOrder");
            btnGO.transform.SetParent(boardGO.transform, false);
            var bRect = btnGO.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.12f, 0.05f);
            bRect.anchorMax = new Vector2(0.88f, 0.20f);
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            var bImg = btnGO.AddComponent<Image>();
            bImg.color = new Color(0.0f, 0.75f, 0.35f, 1.0f);
            m_BtnAcceptOrder = btnGO.AddComponent<Button>();
            m_BtnAcceptOrder.onClick.AddListener(OnAcceptOrderClicked);

            var bOutline = btnGO.AddComponent<Outline>();
            bOutline.effectColor = Color.white;
            bOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var bTxtGO = new GameObject("Txt");
            bTxtGO.transform.SetParent(btnGO.transform, false);
            var btRect = bTxtGO.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero;
            btRect.offsetMax = Vector2.zero;
            var bTxt = bTxtGO.AddComponent<Text>();
            bTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bTxt.fontSize = 18;
            bTxt.text = "<b>🛵 NHẬN ĐƠN NGAY  [ Phím E ]</b>";
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;

            m_OrderBoardPanel = boardGO;
        }

        private Text CreateDetailText(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(480f, 32f);
            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 15;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = Color.white;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            return txt;
        }

        private void EnsureHotbarBuilt()
        {
            if (m_HotbarPanel != null) return;

            var canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;

            var barGO = new GameObject("Grab_SurvivalHotbar");
            barGO.transform.SetParent(parent, false);
            var rect = barGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.sizeDelta = new Vector2(680f, 62f);

            var bgImg = barGO.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.05f, 0.12f, 0.90f);
            var outline = barGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.7f, 1.0f, 0.75f);
            outline.effectDistance = new Vector2(2f, 2f);

            var hlg = barGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(8, 8, 6, 6);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // 6 Item Slots
            (m_BtnSlotFlashlight, m_TxtSlotFlashlight) = CreateHotbarSlot(barGO.transform, "Slot_Flashlight", new Color(0.0f, 0.65f, 0.85f));
            (m_BtnSlotBattery, m_TxtSlotBattery) = CreateHotbarSlot(barGO.transform, "Slot_Battery", new Color(0.0f, 0.75f, 0.45f));
            (m_BtnSlotTalisman, m_TxtSlotTalisman) = CreateHotbarSlot(barGO.transform, "Slot_Talisman", new Color(0.85f, 0.70f, 0.05f));
            (m_BtnSlotFlare, m_TxtSlotFlare) = CreateHotbarSlot(barGO.transform, "Slot_Flare", new Color(0.88f, 0.25f, 0.15f));
            (m_BtnSlotBat, m_TxtSlotBat) = CreateHotbarSlot(barGO.transform, "Slot_Bat", new Color(0.65f, 0.35f, 0.95f));
            (m_BtnSlotFirstAid, m_TxtSlotFirstAid) = CreateHotbarSlot(barGO.transform, "Slot_FirstAid", new Color(0.92f, 0.20f, 0.35f));

            m_HotbarPanel = barGO;
            WireHotbarButtons();
        }

        private (Button, Text) CreateHotbarSlot(Transform parent, string name, Color borderCol)
        {
            var slotGO = new GameObject(name);
            slotGO.transform.SetParent(parent, false);

            var img = slotGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.16f, 0.95f);

            var outline = slotGO.AddComponent<Outline>();
            outline.effectColor = borderCol;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            var btn = slotGO.AddComponent<Button>();

            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(slotGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 12;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;

            return (btn, txt);
        }

        private void EnsureNotificationBannerBuilt()
        {
            if (m_NotificationBanner != null) return;

            var canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;

            var bannerGO = new GameObject("Grab_NotificationBanner");
            bannerGO.transform.SetParent(parent, false);
            var rect = bannerGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -80f);
            rect.sizeDelta = new Vector2(620f, 48f);

            var img = bannerGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.1f, 0.22f, 0.95f);
            var outline = bannerGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.9f, 0.5f, 1.0f);
            outline.effectDistance = new Vector2(2f, 2f);

            var txtGO = new GameObject("TxtNotification");
            txtGO.transform.SetParent(bannerGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            m_TxtNotification = txtGO.AddComponent<Text>();
            m_TxtNotification.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtNotification.fontSize = 16;
            m_TxtNotification.fontStyle = FontStyle.Bold;
            m_TxtNotification.alignment = TextAnchor.MiddleCenter;
            m_TxtNotification.color = Color.white;

            m_NotificationBanner = bannerGO;
        }
    }
}
