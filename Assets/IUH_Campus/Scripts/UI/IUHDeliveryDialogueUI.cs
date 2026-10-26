using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// Delivery Hand-off Dialog Modal (3. Giao hàng chân thật cho Sinh Viên).
    /// - Shows destination NPC location: "Giao hàng - Ký túc xá B"
    /// - Order description and student greeting
    /// - Rewards: Cash + EXP + 5-Star Rating
    /// - Prominent green [ Giao hàng (E) ] button.
    /// Only when confirmed does the mission officially complete!
    /// </summary>
    public class IUHDeliveryDialogueUI : MonoBehaviour
    {
        private static IUHDeliveryDialogueUI s_Instance;
        public static IUHDeliveryDialogueUI Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = UnityEngine.Object.FindAnyObjectByType<IUHDeliveryDialogueUI>(FindObjectsInactive.Include);
                }
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        [Header("UI References")]
        [SerializeField] private GameObject m_DialogRoot;
        [SerializeField] private Text m_TxtLocationTitle;
        [SerializeField] private Text m_TxtMessage;
        [SerializeField] private Text m_TxtRewardCoins;
        [SerializeField] private Text m_TxtRewardExp;
        [SerializeField] private Button m_BtnDeliver;
        [SerializeField] private Button m_BtnClose;

        private Action m_OnDeliverConfirmed;
        private bool m_IsOpen = false;

        public bool IsOpen => m_IsOpen;

        public void Setup(GameObject dialogRoot, Text txtLocation, Text txtMessage, Text txtRewards, Button btnDeliver, Button btnClose = null)
        {
            m_DialogRoot = dialogRoot;
            m_TxtLocationTitle = txtLocation;
            m_TxtMessage = txtMessage;
            m_TxtRewardCoins = txtRewards;
            m_BtnDeliver = btnDeliver;
            m_BtnClose = btnClose;

            WireButtons();
        }

        private void Awake()
        {
            if (s_Instance == null) s_Instance = this;
            else if (s_Instance != this) { Destroy(gameObject); return; }

            AutoWireReferences();
            WireButtons();
        }

        private void WireButtons()
        {
            if (m_BtnDeliver != null)
            {
                m_BtnDeliver.onClick.RemoveAllListeners();
                m_BtnDeliver.onClick.AddListener(OnDeliverClicked);
            }

            if (m_BtnClose != null)
            {
                m_BtnClose.onClick.RemoveAllListeners();
                m_BtnClose.onClick.AddListener(CloseDialog);
            }
        }

        private void AutoWireReferences()
        {
            if (m_DialogRoot == null) m_DialogRoot = gameObject;
            if (m_TxtLocationTitle == null) m_TxtLocationTitle = transform.Find("DialogCard/TxtTitle")?.GetComponent<Text>();
            if (m_TxtMessage == null) m_TxtMessage = transform.Find("DialogCard/TxtMessage")?.GetComponent<Text>();
            if (m_TxtRewardCoins == null) m_TxtRewardCoins = transform.Find("DialogCard/TxtRewards")?.GetComponent<Text>();
            if (m_BtnDeliver == null) m_BtnDeliver = transform.Find("DialogCard/BtnDeliver")?.GetComponent<Button>();
            if (m_BtnClose == null) m_BtnClose = transform.Find("DialogCard/BtnClose")?.GetComponent<Button>();
        }

        private void Start()
        {
            CloseDialog();
        }

        private void Update()
        {
            if (m_IsOpen)
            {
                if (IUHInputHelper.IsInteractKeyDown() || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnDeliverClicked();
                }
                else if (IUHInputHelper.IsEscapeKeyDown())
                {
                    CloseDialog();
                }
            }
        }

        public void ShowDeliveryDialog(string locationName, string message, int cashReward, int expReward, Action onDeliverConfirmed)
        {
            AutoWireReferences();
            WireButtons();

            m_OnDeliverConfirmed = onDeliverConfirmed;
            m_IsOpen = true;

            gameObject.SetActive(true);
            if (m_DialogRoot != null) m_DialogRoot.SetActive(true);
            transform.SetAsLastSibling();

            if (m_TxtLocationTitle != null)
            {
                m_TxtLocationTitle.text = $"📦 BÀN GIAO HÀNG TẬN TAY — {locationName}";
            }

            if (m_TxtMessage != null)
            {
                m_TxtMessage.text = message;
            }

            if (m_TxtRewardCoins != null)
            {
                m_TxtRewardCoins.text = $"+{cashReward:N0} VNĐ  |  +{expReward} XP  |  ⭐⭐⭐⭐⭐ (5.0)";
            }

            if (m_TxtRewardExp != null)
            {
                m_TxtRewardExp.text = $"+{expReward} XP";
            }

            // Style button label
            if (m_BtnDeliver != null)
            {
                var btnTxt = m_BtnDeliver.GetComponentInChildren<Text>();
                if (btnTxt != null)
                {
                    btnTxt.text = "HOÀN TẤT BÀN GIAO & NHẬN THƯỞNG [E]";
                }
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var audio = GetComponent<AudioSource>();
            if (audio == null) audio = gameObject.AddComponent<AudioSource>();
            if (audio != null)
            {
                audio.PlayOneShot(IUHSoundSynthesizer.GetCoinClinkSound(), 1.0f);
            }
        }

        public void CloseDialog()
        {
            m_IsOpen = false;
            if (m_DialogRoot != null) m_DialogRoot.SetActive(false);
            gameObject.SetActive(false);
            m_OnDeliverConfirmed = null;

            // Restore cursor mode if portal is closed
            var portal = IUHMiniGamePortalUI.Instance;
            if (portal == null || !portal.IsOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDeliverClicked()
        {
            var callback = m_OnDeliverConfirmed;
            CloseDialog();
            callback?.Invoke();
        }
    }
}
