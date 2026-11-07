using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;

namespace IUHCampus.DailyActivities
{
    public class IUHCampusActivitiesHUD : MonoBehaviour
    {
        public static IUHCampusActivitiesHUD Instance { get; private set; }

        [Header("Top-Right Widget")]
        [SerializeField] private GameObject m_WidgetPanel;
        [SerializeField] private Button m_ToggleWidgetButton;
        [SerializeField] private Text m_CoinStatusText;
        [SerializeField] private Text m_MysteryGiftText;
        [SerializeField] private Text m_MascotStatusText;
        [SerializeField] private Text m_CheckInStatusText;
        [SerializeField] private Text m_LuckyStarText;
        [SerializeField] private Button m_OpenAlbumButton;

        [Header("Mascot Dialogue Dialog")]
        [SerializeField] private GameObject m_MascotDialogRoot;
        [SerializeField] private Text m_MascotDialogTitle;
        [SerializeField] private Text m_MascotDialogMessage;
        [SerializeField] private Button m_MascotActionButton;
        [SerializeField] private Text m_MascotActionText;
        [SerializeField] private Button m_MascotCloseButton;

        [Header("Memorial Album Modal")]
        [SerializeField] private GameObject m_AlbumModalRoot;
        [SerializeField] private Button m_CloseAlbumButton;
        [SerializeField] private Transform m_AlbumCardsContainer;

        [Header("Shutter Flash FX")]
        [SerializeField] private CanvasGroup m_FlashCanvasGroup;

        [Header("Celebration Banner Toast")]
        [SerializeField] private GameObject m_BannerToastRoot;
        [SerializeField] private Text m_BannerTitle;
        [SerializeField] private Text m_BannerMessage;

        private Action m_CurrentMascotAction;
        private Coroutine m_BannerCoroutine;
        private Coroutine m_FlashCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (m_MascotDialogRoot != null) m_MascotDialogRoot.SetActive(false);
            if (m_AlbumModalRoot != null) m_AlbumModalRoot.SetActive(false);
            if (m_BannerToastRoot != null) m_BannerToastRoot.SetActive(false);
            if (m_FlashCanvasGroup != null)
            {
                m_FlashCanvasGroup.alpha = 0f;
                m_FlashCanvasGroup.blocksRaycasts = false;
            }

            if (m_OpenAlbumButton != null) m_OpenAlbumButton.onClick.AddListener(OpenAlbum);
            if (m_CloseAlbumButton != null) m_CloseAlbumButton.onClick.AddListener(CloseAlbum);
            if (m_MascotCloseButton != null) m_MascotCloseButton.onClick.AddListener(CloseMascotDialog);
            if (m_MascotActionButton != null) m_MascotActionButton.onClick.AddListener(OnMascotActionClicked);

            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.OnActivitiesStateChanged += RefreshHUD;
                IUHCampusDailyActivitiesManager.Instance.OnActivityBannerNotification += ShowBanner;
            }

            RefreshHUD();
        }

        private void OnDestroy()
        {
            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.OnActivitiesStateChanged -= RefreshHUD;
                IUHCampusDailyActivitiesManager.Instance.OnActivityBannerNotification -= ShowBanner;
            }
        }

        public void RefreshHUD()
        {
            var mgr = IUHCampusDailyActivitiesManager.Instance;
            if (mgr == null) return;

            // 1. Coin Hunt
            if (m_CoinStatusText != null)
            {
                if (mgr.IsCoinHuntCompleted)
                    m_CoinStatusText.text = $"🪙 Săn Xu: <color=#00FF88>{mgr.CoinsCollectedCount}/{mgr.TotalCoins} (XONG)</color>";
                else
                    m_CoinStatusText.text = $"🪙 Săn Xu: <color=#FFDD44>{mgr.CoinsCollectedCount}/{mgr.TotalCoins}</color>";
            }

            // 2. Mystery Gift
            if (m_MysteryGiftText != null)
            {
                if (mgr.MysteryGiftOpened)
                    m_MysteryGiftText.text = "🎁 Hộp Quà Bí Ẩn: <color=#00FF88>Đã Mở (Nhận Quà)</color>";
                else
                    m_MysteryGiftText.text = "🎁 Hộp Quà Bí Ẩn: <color=#FF88EE>Đang Giấu Trong Trường</color>";
            }

            // 3. Mascot Quest
            if (m_MascotStatusText != null)
            {
                if (mgr.MascotState == MascotQuestState.NotStarted)
                    m_MascotStatusText.text = "🤖 Linh Vật IUH: <color=#66DDFF>Có Thử Thách Mới</color>";
                else if (mgr.MascotState == MascotQuestState.InProgress)
                    m_MascotStatusText.text = $"🤖 Linh Vật IUH: <color=#FFDD44>Nhặt Vi Mạch ({mgr.MascotItemsCollected}/{mgr.MascotItemsTarget})</color>";
                else
                    m_MascotStatusText.text = "🤖 Linh Vật IUH: <color=#00FF88>Đã Hoàn Thành</color>";
            }

            // 4. Check-in
            if (m_CheckInStatusText != null)
            {
                int unlocked = 0;
                foreach (var s in mgr.CheckInSpots) if (s.isUnlocked) unlocked++;
                m_CheckInStatusText.text = $"📸 Check-in: <color=#00E5FF>{unlocked}/{mgr.CheckInSpots.Count} Địa Điểm</color>";
            }

            // 5. Lucky Star
            if (m_LuckyStarText != null)
            {
                if (mgr.LuckyStarCollected)
                    m_LuckyStarText.text = "⭐ Ngôi Sao May Mắn: <color=#00FF88>Đã Nhận Thưởng</color>";
                else
                    m_LuckyStarText.text = "⭐ Ngôi Sao May Mắn: <color=#FFCC00>Đang Tỏa Sáng</color>";
            }
        }

        public bool IsMascotDialogOpen => m_MascotDialogRoot != null && m_MascotDialogRoot.activeSelf;
        public bool IsAlbumOpen => m_AlbumModalRoot != null && m_AlbumModalRoot.activeSelf;

        private StarterAssets.StarterAssetsInputs m_PlayerInputs;

        private void SetUIMode(bool isOpen)
        {
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;

            if (m_PlayerInputs == null)
            {
                var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
                if (student != null) m_PlayerInputs = student.GetComponent<StarterAssets.StarterAssetsInputs>();
            }

            if (m_PlayerInputs != null)
            {
                m_PlayerInputs.cursorLocked = !isOpen;
                m_PlayerInputs.cursorInputForLook = !isOpen;
            }

            var studentGo = GameObject.Find("Campus Student");
            if (studentGo != null)
            {
                var previewCtrl = studentGo.GetComponent<IUHCampus.World.IUHConceptPreviewController>();
                if (previewCtrl != null) previewCtrl.Apply();

                var tpc = studentGo.GetComponent<StarterAssets.ThirdPersonController>();
                if (tpc != null) tpc.enabled = !isOpen;
            }
        }

        private void Update()
        {
            if (IsMascotDialogOpen)
            {
                // Accept shortcut: E or Enter or Space
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (m_MascotActionButton != null && m_MascotActionButton.gameObject.activeSelf)
                    {
                        OnMascotActionClicked();
                        return;
                    }
                }

                // Close shortcut: Escape or Q or Backspace
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Backspace))
                {
                    CloseMascotDialog();
                    return;
                }
            }

            if (IsAlbumOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.N))
                {
                    CloseAlbum();
                    return;
                }
            }
            else if (!IsMascotDialogOpen && Input.GetKeyDown(KeyCode.N))
            {
                OpenAlbum();
            }
        }

        // ================= MASCOT DIALOG ================= //
        public void OpenMascotDialog(string title, string message, string actionButtonText, Action onAction)
        {
            if (m_MascotDialogRoot == null) return;

            if (m_MascotDialogTitle != null) m_MascotDialogTitle.text = title;
            if (m_MascotDialogMessage != null) m_MascotDialogMessage.text = message;

            m_CurrentMascotAction = onAction;

            if (m_MascotActionButton != null)
            {
                m_MascotActionButton.gameObject.SetActive(onAction != null);
                if (m_MascotActionText != null) m_MascotActionText.text = $"{actionButtonText} [E]";
            }

            m_MascotDialogRoot.SetActive(true);
            SetUIMode(true);

            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                if (m_MascotActionButton != null && m_MascotActionButton.gameObject.activeSelf)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(m_MascotActionButton.gameObject);
                else if (m_MascotCloseButton != null)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(m_MascotCloseButton.gameObject);
            }
        }

        public void CloseMascotDialog()
        {
            if (m_MascotDialogRoot != null) m_MascotDialogRoot.SetActive(false);
            m_CurrentMascotAction = null;
            SetUIMode(false);
        }

        private void OnMascotActionClicked()
        {
            var act = m_CurrentMascotAction;
            CloseMascotDialog();
            act?.Invoke();
        }

        // ================= MEMORIAL ALBUM ================= //
        public void OpenAlbum()
        {
            if (m_AlbumModalRoot == null) return;
            PopulateAlbumCards();
            m_AlbumModalRoot.SetActive(true);
            SetUIMode(true);

            if (UnityEngine.EventSystems.EventSystem.current != null && m_CloseAlbumButton != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(m_CloseAlbumButton.gameObject);
            }
        }

        public void CloseAlbum()
        {
            if (m_AlbumModalRoot != null) m_AlbumModalRoot.SetActive(false);
            SetUIMode(false);
        }

        private void PopulateAlbumCards()
        {
            if (m_AlbumCardsContainer == null) return;
            var mgr = IUHCampusDailyActivitiesManager.Instance;
            if (mgr == null) return;

            // Clear old cards
            for (int i = m_AlbumCardsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_AlbumCardsContainer.GetChild(i).gameObject);
            }

            // Create cards for each check-in spot
            foreach (var spot in mgr.CheckInSpots)
            {
                GameObject cardGo = CreateCardUI(spot);
                cardGo.transform.SetParent(m_AlbumCardsContainer, false);
            }
        }

        private GameObject CreateCardUI(CheckInRecord spot)
        {
            GameObject card = new GameObject("Card_" + spot.spotId, typeof(RectTransform), typeof(Image));
            var cardImg = card.GetComponent<Image>();
            cardImg.color = spot.isUnlocked ? new Color(0.12f, 0.16f, 0.24f, 0.95f) : new Color(0.1f, 0.1f, 0.12f, 0.85f);

            var rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240f, 320f);

            // Outline / Frame
            var outline = card.AddComponent<Outline>();
            outline.effectColor = spot.isUnlocked ? new Color(0f, 0.85f, 1f, 0.8f) : new Color(0.4f, 0.4f, 0.4f, 0.4f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(card.transform, false);
            var titleTxt = titleGo.GetComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleTxt.text = spot.spotName;
            titleTxt.fontSize = 15;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = spot.isUnlocked ? new Color(1f, 0.9f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -12f);
            titleRt.sizeDelta = new Vector2(-16f, 34f);

            // Photo Placeholder Image Box
            GameObject photoBox = new GameObject("PhotoBox", typeof(RectTransform), typeof(Image));
            photoBox.transform.SetParent(card.transform, false);
            var photoImg = photoBox.GetComponent<Image>();
            photoImg.color = spot.isUnlocked ? new Color(0.15f, 0.35f, 0.5f, 1f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var photoRt = photoBox.GetComponent<RectTransform>();
            photoRt.anchorMin = new Vector2(0.5f, 0.5f);
            photoRt.anchorMax = new Vector2(0.5f, 0.5f);
            photoRt.sizeDelta = new Vector2(210f, 130f);
            photoRt.anchoredPosition = new Vector2(0f, 30f);

            // Photo Icon / Stamp
            GameObject stampGo = new GameObject("Stamp", typeof(RectTransform), typeof(Text));
            stampGo.transform.SetParent(photoBox.transform, false);
            var stampTxt = stampGo.GetComponent<Text>();
            stampTxt.font = titleTxt.font;
            stampTxt.text = spot.isUnlocked ? "📸 ĐÃ CHECK-IN" : "🔒 CHƯA KHÁM PHÁ";
            stampTxt.fontSize = 14;
            stampTxt.fontStyle = FontStyle.Bold;
            stampTxt.alignment = TextAnchor.MiddleCenter;
            stampTxt.color = spot.isUnlocked ? new Color(0.3f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f);
            var stampRt = stampGo.GetComponent<RectTransform>();
            stampRt.anchorMin = Vector2.zero;
            stampRt.anchorMax = Vector2.one;
            stampRt.sizeDelta = Vector2.zero;

            // Description
            GameObject descGo = new GameObject("Desc", typeof(RectTransform), typeof(Text));
            descGo.transform.SetParent(card.transform, false);
            var descTxt = descGo.GetComponent<Text>();
            descTxt.font = titleTxt.font;
            descTxt.text = spot.isUnlocked ? $"{spot.description}\n\n<i>{spot.badgeName}</i>" : "Hãy tìm đến địa điểm này trong khuôn viên để chụp ảnh và mở khóa kỷ niệm!";
            descTxt.fontSize = 11;
            descTxt.color = spot.isUnlocked ? new Color(0.9f, 0.95f, 1f, 0.9f) : new Color(0.5f, 0.5f, 0.5f, 0.8f);
            descTxt.alignment = TextAnchor.UpperLeft;
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 0.5f);
            descRt.pivot = new Vector2(0.5f, 0.5f);
            descRt.anchoredPosition = new Vector2(0f, -50f);
            descRt.sizeDelta = new Vector2(-20f, 75f);

            return card;
        }

        // ================= SHUTTER FLASH ================= //
        public void TriggerCameraFlash()
        {
            if (m_FlashCanvasGroup == null) return;
            if (m_FlashCoroutine != null) StopCoroutine(m_FlashCoroutine);
            m_FlashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            m_FlashCanvasGroup.alpha = 0.95f;
            float elapsed = 0f;
            float duration = 0.6f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                m_FlashCanvasGroup.alpha = Mathf.Lerp(0.95f, 0f, elapsed / duration);
                yield return null;
            }
            m_FlashCanvasGroup.alpha = 0f;
        }

        // ================= BANNER TOAST ================= //
        public void ShowBanner(string title, string message)
        {
            if (m_BannerToastRoot == null) return;

            if (m_BannerTitle != null) m_BannerTitle.text = title;
            if (m_BannerMessage != null) m_BannerMessage.text = message;

            if (m_BannerCoroutine != null) StopCoroutine(m_BannerCoroutine);
            m_BannerCoroutine = StartCoroutine(BannerRoutine());
        }

        private IEnumerator BannerRoutine()
        {
            m_BannerToastRoot.SetActive(true);
            yield return new WaitForSeconds(4.5f);
            m_BannerToastRoot.SetActive(false);
        }
    }
}

