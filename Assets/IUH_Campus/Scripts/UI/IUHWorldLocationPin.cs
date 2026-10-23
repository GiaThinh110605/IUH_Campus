using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.UI
{
    /// <summary>
    /// 3D World-Space Floating Location Pin & Holographic Area Marker.
    /// Hovering gently above key campus zones (Cổng trường, Căn tin, Thư viện, Ký túc xá, Sân trường, Sân lễ hội).
    /// Features:
    /// - Camera billboarding (always faces camera).
    /// - Floating sinusoidal bobbing and gentle pulse.
    /// - Categorized color theme and glowing icon badge.
    /// - Proximity interaction: shows '[E] Tương tác' when player is within range.
    /// </summary>
    public class IUHWorldLocationPin : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField] private string m_ZoneTitle = "Cổng trường";
        [SerializeField] private string m_ZoneSubtitle = "Bắt đầu hành trình";
        [SerializeField] private string m_IconEmoji = "🏛️";
        [SerializeField] private Color m_ThemeColor = new Color(0.0f, 0.8f, 1.0f);
        [SerializeField] private int m_MiniGameIndex = -1; // -1 = Landmark, 0-5 = Mini-games 1-6

        [Header("Animation & Behavior")]
        [SerializeField] private float m_HoverHeight = 3.6f;
        [SerializeField] private float m_BobAmplitude = 0.22f;
        [SerializeField] private float m_BobFrequency = 2.0f;
        [SerializeField] private float m_InteractionRadius = 5.0f;

        private Transform m_CameraTransform;
        private Transform m_PlayerTransform;
        private Vector3 m_BasePosition;
        private bool m_IsPlayerNearby = false;

        // UI references
        private Canvas m_Canvas;
        private Text m_TxtTitle;
        private Text m_TxtSubtitle;
        private Text m_TxtIcon;
        private Image m_IconBg;
        private GameObject m_PromptBox;

        public string ZoneTitle => m_ZoneTitle;
        public int MiniGameIndex => m_MiniGameIndex;

        public void Setup(string title, string subtitle, string iconEmoji, Color themeColor, int gameIndex, float height = 3.8f)
        {
            m_ZoneTitle = title;
            m_ZoneSubtitle = subtitle;
            m_IconEmoji = iconEmoji;
            m_ThemeColor = themeColor;
            m_MiniGameIndex = gameIndex;
            m_HoverHeight = height;

            m_BasePosition = transform.position;
            BuildVisuals();
        }

        private void Awake()
        {
            m_BasePosition = transform.position;
            BuildVisuals();
        }

        private void Start()
        {
            FindReferences();
        }

        private void FindReferences()
        {
            if (Camera.main != null) m_CameraTransform = Camera.main.transform;

            var player = GameObject.Find("PlayerArmature");
            if (player != null) m_PlayerTransform = player.transform;
            else
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) m_PlayerTransform = p.transform;
            }
        }

        private void BuildVisuals()
        {
            // Clear children if already built
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            var canvasGO = new GameObject("PinCanvas");
            canvasGO.transform.SetParent(transform, false);
            canvasGO.transform.localPosition = Vector3.zero;

            m_Canvas = canvasGO.AddComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.WorldSpace;

            var rect = canvasGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 130f);
            rect.localScale = Vector3.one * 0.015f;

            // Root Card Box
            var cardGO = new GameObject("CardBox");
            cardGO.transform.SetParent(canvasGO.transform, false);
            var cardRect = cardGO.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(250f, 90f);
            cardRect.anchoredPosition = new Vector2(0f, 25f);

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);
            var cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = m_ThemeColor;
            cardOutline.effectDistance = new Vector2(2.5f, 2.5f);

            // Icon Circular Badge (Top-Center)
            var iconGO = new GameObject("IconBadge");
            iconGO.transform.SetParent(cardGO.transform, false);
            var iRect = iconGO.AddComponent<RectTransform>();
            iRect.sizeDelta = new Vector2(48f, 48f);
            iRect.anchoredPosition = new Vector2(0f, 45f);

            m_IconBg = iconGO.AddComponent<Image>();
            m_IconBg.color = m_ThemeColor;
            var iconOutline = iconGO.AddComponent<Outline>();
            iconOutline.effectColor = Color.white * 0.9f;
            iconOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var iconTxtGO = new GameObject("IconTxt");
            iconTxtGO.transform.SetParent(iconGO.transform, false);
            var itRect = iconTxtGO.AddComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero;
            itRect.anchorMax = Vector2.one;
            m_TxtIcon = iconTxtGO.AddComponent<Text>();
            m_TxtIcon.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtIcon.text = m_IconEmoji;
            m_TxtIcon.fontSize = 24;
            m_TxtIcon.alignment = TextAnchor.MiddleCenter;

            // Title Text
            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(cardGO.transform, false);
            var tRect = titleGO.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.04f, 0.40f);
            tRect.anchorMax = new Vector2(0.96f, 0.92f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            m_TxtTitle = titleGO.AddComponent<Text>();
            m_TxtTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtTitle.text = m_ZoneTitle;
            m_TxtTitle.fontSize = 20;
            m_TxtTitle.fontStyle = FontStyle.Bold;
            m_TxtTitle.alignment = TextAnchor.MiddleCenter;
            m_TxtTitle.color = Color.white;

            // Subtitle Text
            var subGO = new GameObject("TxtSubtitle");
            subGO.transform.SetParent(cardGO.transform, false);
            var sRect = subGO.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.04f, 0.05f);
            sRect.anchorMax = new Vector2(0.96f, 0.45f);
            sRect.offsetMin = Vector2.zero;
            sRect.offsetMax = Vector2.zero;
            m_TxtSubtitle = subGO.AddComponent<Text>();
            m_TxtSubtitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtSubtitle.text = m_ZoneSubtitle;
            m_TxtSubtitle.fontSize = 13;
            m_TxtSubtitle.alignment = TextAnchor.MiddleCenter;
            m_TxtSubtitle.color = new Color(0.8f, 0.9f, 1.0f);

            // Interaction Prompt Badge '[E] NHẬN NHIỆM VỤ' (Below card)
            m_PromptBox = new GameObject("PromptBox");
            m_PromptBox.transform.SetParent(canvasGO.transform, false);
            var pRect = m_PromptBox.AddComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(190f, 32f);
            pRect.anchoredPosition = new Vector2(0f, -32f);

            var pImg = m_PromptBox.AddComponent<Image>();
            pImg.color = new Color(0.0f, 0.72f, 0.35f, 0.95f);
            var pOutline = m_PromptBox.AddComponent<Outline>();
            pOutline.effectColor = Color.white;
            pOutline.effectDistance = new Vector2(1.5f, 1.5f);

            var pTxtGO = new GameObject("TxtPrompt");
            pTxtGO.transform.SetParent(m_PromptBox.transform, false);
            var ptRect = pTxtGO.AddComponent<RectTransform>();
            ptRect.anchorMin = Vector2.zero;
            ptRect.anchorMax = Vector2.one;
            var pt = pTxtGO.AddComponent<Text>();
            pt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pt.text = "⚡ BẤM [ E ] ĐỂ CHƠI";
            pt.fontSize = 14;
            pt.fontStyle = FontStyle.Bold;
            pt.alignment = TextAnchor.MiddleCenter;
            pt.color = Color.white;

            m_PromptBox.SetActive(false);
        }

        private void Update()
        {
            if (m_CameraTransform == null || m_PlayerTransform == null)
            {
                FindReferences();
                if (m_CameraTransform == null) return;
            }

            // 1. Sinusoidal bobbing
            float bob = Mathf.Sin(Time.time * m_BobFrequency) * m_BobAmplitude;
            transform.position = m_BasePosition + new Vector3(0f, m_HoverHeight + bob, 0f);

            // 2. Billboard to face camera
            transform.rotation = Quaternion.LookRotation(transform.position - m_CameraTransform.position);

            // 3. Proximity check with player
            if (m_PlayerTransform != null)
            {
                float dist = Vector3.Distance(m_PlayerTransform.position, m_BasePosition);
                bool nearby = dist <= m_InteractionRadius;

                if (nearby != m_IsPlayerNearby)
                {
                    m_IsPlayerNearby = nearby;
                    if (m_PromptBox != null) m_PromptBox.SetActive(nearby);
                }

                // Handle E interaction when nearby
                if (m_IsPlayerNearby && IUHInputHelper.IsInteractKeyDown())
                {
                    OnPinInteracted();
                }
            }
        }

        private void OnPinInteracted()
        {
            if (IUHGameFlowManager.Instance == null) return;

            if (m_MiniGameIndex >= 0 && m_MiniGameIndex <= 5)
            {
                IUHGameFlowManager.Instance.StartGameSetup(m_MiniGameIndex);
            }
            else
            {
                IUHGameFlowManager.Instance.GoToMiniGameSelection();
            }
        }
    }
}
