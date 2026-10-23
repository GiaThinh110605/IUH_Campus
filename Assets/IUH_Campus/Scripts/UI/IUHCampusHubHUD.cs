using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.UI
{
    /// <summary>
    /// Master Shared Campus Hub HUD (Mirrors the target design specification).
    /// Features:
    /// - Top-Left: "Campus Adventure — Khuôn viên chung" title & student description badge.
    /// - Top-Right: Profile card (Avatar, Lv.3, EXP bar, 5,620 Gold, 120 Gems).
    /// - Right Sidebar: "6 Mini-game" quick cards (Grab, KTX Ghost, AI Object, Photo, Green, Festival).
    /// - Bottom-Left: Minimap radar & "Nhiệm vụ hiện tại" objective card.
    /// - Bottom-Right: 3 Quick Action buttons: Túi đồ [U], Bản đồ [M], Cài đặt.
    /// - Center-Bottom: Floating interaction prompt [E].
    /// </summary>
    public class IUHCampusHubHUD : MonoBehaviour
    {
        public static IUHCampusHubHUD Instance { get; private set; }

        [Header("Root HUD Panel")]
        [SerializeField] private GameObject m_HubHudRoot;

        [Header("Top Left Info")]
        [SerializeField] private GameObject m_TopLeftBanner;
        [SerializeField] private Text m_TxtCampusTitle;
        [SerializeField] private Text m_TxtCampusDesc;

        [Header("Top Right Profile & Currency")]
        [SerializeField] private GameObject m_ProfileWidget;
        [SerializeField] private Text m_TxtPlayerLevel;
        [SerializeField] private Slider m_SliderExp;
        [SerializeField] private Text m_TxtGold;
        [SerializeField] private Text m_TxtGems;

        [Header("Right Sidebar (6 Mini-games)")]
        [SerializeField] private GameObject m_SidebarPanel;
        [SerializeField] private Button[] m_SidebarGameButtons;

        [Header("Bottom Left Objective")]
        [SerializeField] private GameObject m_ObjectivePanel;
        [SerializeField] private Text m_TxtActiveObjective;

        [Header("Bottom Right Action Buttons")]
        [SerializeField] private Button m_BtnInventory;
        [SerializeField] private Button m_BtnMap;
        [SerializeField] private Button m_BtnSettings;

        [Header("Interaction Prompt")]
        [SerializeField] private GameObject m_InteractionPrompt;
        [SerializeField] private Text m_TxtInteractionPrompt;

        private bool m_IsVisible = true;
        public bool IsVisible => m_IsVisible;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            EnsureHUDVisuals();
            WireButtons();
        }

        private void Start()
        {
            UpdatePlayerCurrency(5620, 120, 3, 0.72f);
            SetObjectiveText("Chưa có nhiệm vụ • Hãy khám phá khuôn viên hoặc chọn 1 trong 6 mini-game!");
        }

        public void SetHUDVisible(bool visible)
        {
            m_IsVisible = visible;
            if (m_HubHudRoot != null)
            {
                m_HubHudRoot.SetActive(visible);
            }
        }

        public void SetObjectiveText(string text)
        {
            if (m_TxtActiveObjective != null)
            {
                m_TxtActiveObjective.text = text;
            }
        }

        public void UpdatePlayerCurrency(int gold, int gems, int level = 3, float expProgress = 0.7f)
        {
            if (m_TxtGold != null) m_TxtGold.text = $"{gold:N0}";
            if (m_TxtGems != null) m_TxtGems.text = $"{gems}";
            if (m_TxtPlayerLevel != null) m_TxtPlayerLevel.text = $"Lv. {level}";
            if (m_SliderExp != null) m_SliderExp.value = expProgress;
        }

        public void ShowInteractionPrompt(bool show, string message = "BẤM [ E ] ĐỂ TƯƠNG TÁC")
        {
            if (m_InteractionPrompt != null)
            {
                m_InteractionPrompt.SetActive(show);
                if (show && m_TxtInteractionPrompt != null)
                {
                    m_TxtInteractionPrompt.text = message;
                }
            }
        }

        private void WireButtons()
        {
            // Inventory / Wardrobe
            if (m_BtnInventory != null)
            {
                m_BtnInventory.onClick.RemoveAllListeners();
                m_BtnInventory.onClick.AddListener(() => {
                    if (IUHGameFlowManager.Instance != null)
                        IUHGameFlowManager.Instance.StartGameSetup(0);
                });
            }

            // Map / Portal Menu
            if (m_BtnMap != null)
            {
                m_BtnMap.onClick.RemoveAllListeners();
                m_BtnMap.onClick.AddListener(() => {
                    if (IUHGameFlowManager.Instance != null)
                        IUHGameFlowManager.Instance.GoToMiniGameSelection();
                });
            }

            // Settings
            if (m_BtnSettings != null)
            {
                m_BtnSettings.onClick.RemoveAllListeners();
                m_BtnSettings.onClick.AddListener(() => {
                    var mainUI = IUHMainMenuUI.Instance;
                    if (mainUI != null)
                    {
                        var settingsDialog = mainUI.transform.Find("SettingsDialog");
                        if (settingsDialog != null) settingsDialog.gameObject.SetActive(true);
                    }
                });
            }

            // 6 Mini-Game Sidebar Buttons
            if (m_SidebarGameButtons != null)
            {
                for (int i = 0; i < m_SidebarGameButtons.Length; i++)
                {
                    int gameIdx = i;
                    if (m_SidebarGameButtons[i] != null)
                    {
                        m_SidebarGameButtons[i].onClick.RemoveAllListeners();
                        m_SidebarGameButtons[i].onClick.AddListener(() => {
                            if (IUHGameFlowManager.Instance != null)
                                IUHGameFlowManager.Instance.StartGameSetup(gameIdx);
                        });
                    }
                }
            }
        }

        private void EnsureHUDVisuals()
        {
            if (m_HubHudRoot != null) return;

            var canvas = GameObject.Find("IUH_Minigame_Canvas");
            if (canvas == null) return;

            var existing = canvas.transform.Find("CampusHubHUD");
            if (existing != null)
            {
                m_HubHudRoot = existing.gameObject;
                AutoWireComponents();
                return;
            }

            // 1. Root Container
            var rootGO = new GameObject("CampusHubHUD");
            rootGO.transform.SetParent(canvas.transform, false);
            var rRect = rootGO.AddComponent<RectTransform>();
            rRect.anchorMin = Vector2.zero;
            rRect.anchorMax = Vector2.one;
            rRect.offsetMin = Vector2.zero;
            rRect.offsetMax = Vector2.zero;
            m_HubHudRoot = rootGO;

            // 2. Top-Left Title Card
            BuildTopLeftBanner(rootGO.transform);

            // 3. Top-Right Profile Widget
            BuildTopRightProfile(rootGO.transform);

            // 4. Right Sidebar: 6 Mini-games
            BuildSidebar6MiniGames(rootGO.transform);

            // 5. Bottom-Left Active Objective
            BuildBottomLeftObjective(rootGO.transform);

            // 6. Bottom-Right 3 Quick Action Buttons
            BuildBottomRightActionButtons(rootGO.transform);

            // 7. Center-Bottom Interaction Prompt
            BuildInteractionPrompt(rootGO.transform);

            AutoWireComponents();
        }

        private void BuildTopLeftBanner(Transform parent)
        {
            var bannerGO = new GameObject("TopLeftBanner");
            bannerGO.transform.SetParent(parent, false);
            var rect = bannerGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(25f, -25f);
            rect.sizeDelta = new Vector2(340f, 130f);

            var img = bannerGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.16f, 0.82f);
            var outline = bannerGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.85f, 0.55f, 0.9f);
            outline.effectDistance = new Vector2(2f, 2f);

            var vlg = bannerGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Title
            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(bannerGO.transform, false);
            m_TxtCampusTitle = titleGO.AddComponent<Text>();
            m_TxtCampusTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtCampusTitle.text = "🎓 <size=19><b>Campus Adventure</b></size>\n<color=#34D399><b>KHUÔN VIÊN CHUNG</b></color>";
            m_TxtCampusTitle.fontSize = 15;
            m_TxtCampusTitle.color = Color.white;
            titleGO.AddComponent<LayoutElement>().preferredHeight = 44f;

            // Description
            var descGO = new GameObject("TxtDesc");
            descGO.transform.SetParent(bannerGO.transform, false);
            m_TxtCampusDesc = descGO.AddComponent<Text>();
            m_TxtCampusDesc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtCampusDesc.text = "Khuôn viên trường Đại học Công nghiệp TP.HCM (IUH) — Khám phá và tham gia 6 mini-game liên kết.";
            m_TxtCampusDesc.fontSize = 12;
            m_TxtCampusDesc.color = new Color(0.85f, 0.92f, 1.0f);
            m_TxtCampusDesc.lineSpacing = 1.25f;
            descGO.AddComponent<LayoutElement>().preferredHeight = 52f;

            m_TopLeftBanner = bannerGO;
        }

        private void BuildTopRightProfile(Transform parent)
        {
            var profGO = new GameObject("ProfileWidget");
            profGO.transform.SetParent(parent, false);
            var rect = profGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-25f, -25f);
            rect.sizeDelta = new Vector2(360f, 62f);

            var img = profGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);
            var outline = profGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.85f, 0.95f, 0.9f);
            outline.effectDistance = new Vector2(2f, 2f);

            var hlg = profGO.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 14, 8, 8);
            hlg.spacing = 12f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Avatar circle
            var avGO = new GameObject("Avatar");
            avGO.transform.SetParent(profGO.transform, false);
            var avImg = avGO.AddComponent<Image>();
            avImg.color = new Color(0.12f, 0.45f, 0.85f);
            var avLe = avGO.AddComponent<LayoutElement>();
            avLe.preferredWidth = 46f;
            avLe.preferredHeight = 46f;

            var avTxtGO = new GameObject("TxtAv");
            avTxtGO.transform.SetParent(avGO.transform, false);
            var avtRect = avTxtGO.AddComponent<RectTransform>();
            avtRect.anchorMin = Vector2.zero;
            avtRect.anchorMax = Vector2.one;
            var avt = avTxtGO.AddComponent<Text>();
            avt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            avt.text = "🧑‍🎓";
            avt.fontSize = 24;
            avt.alignment = TextAnchor.MiddleCenter;

            // Level & EXP Column
            var lvlCol = new GameObject("LevelCol");
            lvlCol.transform.SetParent(profGO.transform, false);
            var lcLe = lvlCol.AddComponent<LayoutElement>();
            lcLe.preferredWidth = 90f;
            var lcv = lvlCol.AddComponent<VerticalLayoutGroup>();
            lcv.spacing = 3f;
            lcv.childControlWidth = true;
            lcv.childControlHeight = false;

            var lvlTxtGO = new GameObject("TxtLvl");
            lvlTxtGO.transform.SetParent(lvlCol.transform, false);
            m_TxtPlayerLevel = lvlTxtGO.AddComponent<Text>();
            m_TxtPlayerLevel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtPlayerLevel.text = "Lv. 3";
            m_TxtPlayerLevel.fontSize = 14;
            m_TxtPlayerLevel.fontStyle = FontStyle.Bold;
            m_TxtPlayerLevel.color = new Color(0.4f, 0.95f, 1.0f);
            lvlTxtGO.AddComponent<LayoutElement>().preferredHeight = 18f;

            // Slider EXP
            var sldGO = new GameObject("ExpBar");
            sldGO.transform.SetParent(lvlCol.transform, false);
            m_SliderExp = sldGO.AddComponent<Slider>();
            m_SliderExp.minValue = 0f;
            m_SliderExp.maxValue = 1f;
            m_SliderExp.value = 0.72f;
            sldGO.AddComponent<LayoutElement>().preferredHeight = 12f;

            var sldBg = sldGO.AddComponent<Image>();
            sldBg.color = new Color(0.1f, 0.15f, 0.25f);
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(sldGO.transform, false);
            var fRect = fillGO.AddComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = new Vector2(0.72f, 1f);
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;
            var fImg = fillGO.AddComponent<Image>();
            fImg.color = new Color(0.0f, 0.85f, 0.45f);
            m_SliderExp.fillRect = fRect;

            // Currency Box (Gold & Gems)
            var curBox = new GameObject("CurrencyBox");
            curBox.transform.SetParent(profGO.transform, false);
            var cbLe = curBox.AddComponent<LayoutElement>();
            cbLe.preferredWidth = 170f;
            var cbH = curBox.AddComponent<HorizontalLayoutGroup>();
            cbH.spacing = 10f;
            cbH.childControlWidth = true;
            cbH.childControlHeight = true;

            // Gold
            var gGO = new GameObject("Gold");
            gGO.transform.SetParent(curBox.transform, false);
            m_TxtGold = gGO.AddComponent<Text>();
            m_TxtGold.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtGold.text = "🪙 5,620";
            m_TxtGold.fontSize = 15;
            m_TxtGold.fontStyle = FontStyle.Bold;
            m_TxtGold.alignment = TextAnchor.MiddleCenter;
            m_TxtGold.color = new Color(1.0f, 0.85f, 0.2f);

            // Gems
            var gemGO = new GameObject("Gems");
            gemGO.transform.SetParent(curBox.transform, false);
            m_TxtGems = gemGO.AddComponent<Text>();
            m_TxtGems.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtGems.text = "💎 120";
            m_TxtGems.fontSize = 15;
            m_TxtGems.fontStyle = FontStyle.Bold;
            m_TxtGems.alignment = TextAnchor.MiddleCenter;
            m_TxtGems.color = new Color(0.3f, 0.9f, 1.0f);

            m_ProfileWidget = profGO;
        }

        private void BuildSidebar6MiniGames(Transform parent)
        {
            var sideGO = new GameObject("Sidebar6MiniGames");
            sideGO.transform.SetParent(parent, false);
            var rect = sideGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            rect.sizeDelta = new Vector2(250f, 430f);

            var img = sideGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);
            var outline = sideGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.6f, 0.9f, 0.8f);
            outline.effectDistance = new Vector2(2f, 2f);

            var vlg = sideGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Sidebar Header
            var hdrGO = new GameObject("TxtHeader");
            hdrGO.transform.SetParent(sideGO.transform, false);
            var hTxt = hdrGO.AddComponent<Text>();
            hTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hTxt.text = "🎮 <b>6 MINI-GAME</b>";
            hTxt.fontSize = 15;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(0.9f, 0.95f, 1.0f);
            hdrGO.AddComponent<LayoutElement>().preferredHeight = 26f;

            m_SidebarGameButtons = new Button[6];

            string[] titles = new string[] {
                "1. Grab Campus\n<size=11><color=#94A3B8>Chuyến xe tâm linh</color></size>",
                "2. Trốn tìm với ma\n<size=11><color=#94A3B8>Ở ký túc xá Block D</color></size>",
                "3. AI Object Hunt\n<size=11><color=#94A3B8>Tìm đồ vật Thư viện</color></size>",
                "4. Thợ săn khoảnh khắc\n<size=11><color=#94A3B8>Chụp ảnh địa điểm</color></size>",
                "5. Green Campus\n<size=11><color=#94A3B8>Cứu lấy trường học</color></size>",
                "6. Campus Festival\n<size=11><color=#94A3B8>Tổ chức lễ hội</color></size>"
            };

            string[] icons = new string[] { "🛵", "👻", "🔍", "📸", "🌱", "🎪" };
            Color[] colors = new Color[] {
                new Color(0.0f, 0.65f, 0.95f),   // Blue
                new Color(0.65f, 0.35f, 0.95f),  // Purple
                new Color(0.95f, 0.65f, 0.10f),  // Orange
                new Color(0.10f, 0.85f, 0.95f),  // Cyan
                new Color(0.15f, 0.80f, 0.40f),  // Green
                new Color(0.95f, 0.30f, 0.65f)   // Pink
            };

            for (int i = 0; i < 6; i++)
            {
                var btnGO = new GameObject($"BtnGame_{i + 1}");
                btnGO.transform.SetParent(sideGO.transform, false);
                var bImg = btnGO.AddComponent<Image>();
                bImg.color = new Color(0.08f, 0.14f, 0.24f, 0.95f);
                var bOut = btnGO.AddComponent<Outline>();
                bOut.effectColor = colors[i];
                bOut.effectDistance = new Vector2(1.5f, 1.5f);
                var btn = btnGO.AddComponent<Button>();
                btnGO.AddComponent<LayoutElement>().preferredHeight = 52f;

                var h = btnGO.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(8, 8, 4, 4);
                h.spacing = 8f;
                h.childControlWidth = false;
                h.childControlHeight = true;

                // Emoji Icon Box
                var iconBox = new GameObject("Icon");
                iconBox.transform.SetParent(btnGO.transform, false);
                var ibLe = iconBox.AddComponent<LayoutElement>();
                ibLe.preferredWidth = 36f;
                var ibTxt = iconBox.AddComponent<Text>();
                ibTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                ibTxt.text = icons[i];
                ibTxt.fontSize = 20;
                ibTxt.alignment = TextAnchor.MiddleCenter;

                // Title Label
                var lblBox = new GameObject("Label");
                lblBox.transform.SetParent(btnGO.transform, false);
                var lbLe = lblBox.AddComponent<LayoutElement>();
                lbLe.preferredWidth = 175f;
                var lbTxt = lblBox.AddComponent<Text>();
                lbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lbTxt.text = titles[i];
                lbTxt.fontSize = 12;
                lbTxt.fontStyle = FontStyle.Bold;
                lbTxt.alignment = TextAnchor.MiddleLeft;
                lbTxt.color = Color.white;

                m_SidebarGameButtons[i] = btn;
            }

            m_SidebarPanel = sideGO;
        }

        private void BuildBottomLeftObjective(Transform parent)
        {
            var objGO = new GameObject("ObjectivePanel");
            objGO.transform.SetParent(parent, false);
            var rect = objGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(25f, 25f);
            rect.sizeDelta = new Vector2(360f, 65f);

            var img = objGO.AddComponent<Image>();
            img.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);
            var outline = objGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.0f, 0.85f, 0.65f, 0.9f);
            outline.effectDistance = new Vector2(2f, 2f);

            var vlg = objGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(14, 14, 8, 8);
            vlg.spacing = 4f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var tagGO = new GameObject("TxtTag");
            tagGO.transform.SetParent(objGO.transform, false);
            var tagTxt = tagGO.AddComponent<Text>();
            tagTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tagTxt.text = "🎯 <color=#FDE047><b>NHIỆM VỤ HIỆN TẠI</b></color>";
            tagTxt.fontSize = 13;
            tagTxt.fontStyle = FontStyle.Bold;
            tagTxt.color = Color.white;
            tagGO.AddComponent<LayoutElement>().preferredHeight = 18f;

            var objTxtGO = new GameObject("TxtObj");
            objTxtGO.transform.SetParent(objGO.transform, false);
            m_TxtActiveObjective = objTxtGO.AddComponent<Text>();
            m_TxtActiveObjective.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtActiveObjective.text = "Chưa có nhiệm vụ • Khám phá khuôn viên hoặc nhấn [M] để mở danh sách mini-game.";
            m_TxtActiveObjective.fontSize = 12;
            m_TxtActiveObjective.color = new Color(0.85f, 0.95f, 1.0f);
            objTxtGO.AddComponent<LayoutElement>().preferredHeight = 32f;

            m_ObjectivePanel = objGO;
        }

        private void BuildBottomRightActionButtons(Transform parent)
        {
            var rowGO = new GameObject("ActionButtonsRow");
            rowGO.transform.SetParent(parent, false);
            var rect = rowGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-280f, 25f);
            rect.sizeDelta = new Vector2(240f, 60f);

            var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // 1. Túi đồ (Inventory)
            m_BtnInventory = CreateRoundActionBtn(rowGO.transform, "BtnInventory", "🎒\nTúi đồ", new Color(0.15f, 0.55f, 0.90f));

            // 2. Bản đồ (Map)
            m_BtnMap = CreateRoundActionBtn(rowGO.transform, "BtnMap", "🗺️\nBản đồ", new Color(0.0f, 0.72f, 0.40f));

            // 3. Cài đặt (Settings)
            m_BtnSettings = CreateRoundActionBtn(rowGO.transform, "BtnSettings", "⚙️\nCài đặt", new Color(0.35f, 0.40f, 0.50f));
        }

        private Button CreateRoundActionBtn(Transform parent, string name, string label, Color color)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var img = btnGO.AddComponent<Image>();
            img.color = color;
            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = Color.white * 0.8f;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(btnGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = label;
            txt.fontSize = 11;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btnGO.AddComponent<Button>();
        }

        private void BuildInteractionPrompt(Transform parent)
        {
            var promptGO = new GameObject("CenterInteractionPrompt");
            promptGO.transform.SetParent(parent, false);
            var rect = promptGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 35f);
            rect.sizeDelta = new Vector2(340f, 48f);

            var img = promptGO.AddComponent<Image>();
            img.color = new Color(0.02f, 0.65f, 0.35f, 0.95f);
            var outline = promptGO.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2f, 2f);

            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(promptGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            m_TxtInteractionPrompt = txtGO.AddComponent<Text>();
            m_TxtInteractionPrompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtInteractionPrompt.text = "⚡ BẤM [ E ] ĐỂ NHẬN NHIỆM VỤ";
            m_TxtInteractionPrompt.fontSize = 16;
            m_TxtInteractionPrompt.fontStyle = FontStyle.Bold;
            m_TxtInteractionPrompt.alignment = TextAnchor.MiddleCenter;
            m_TxtInteractionPrompt.color = Color.white;

            m_InteractionPrompt = promptGO;
            m_InteractionPrompt.SetActive(false);
        }

        private void AutoWireComponents()
        {
            WireButtons();
        }

        private void Update()
        {
            // Toggle sidebar or quick HUD elements if in free roam
            if (IUHGameFlowManager.Instance != null && IUHGameFlowManager.Instance.CurrentState == IUHGameFlowManager.FlowState.Playing)
            {
                if (m_HubHudRoot != null && !m_HubHudRoot.activeSelf)
                {
                    m_HubHudRoot.SetActive(true);
                }
            }
            else
            {
                if (m_HubHudRoot != null && m_HubHudRoot.activeSelf)
                {
                    // Hide hub hud when modal menus or result dialogs are up
                    m_HubHudRoot.SetActive(false);
                }
            }
        }
    }
}
