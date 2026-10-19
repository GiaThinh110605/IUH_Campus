using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// Delivery Hand-off Dialog Modal (3. Giao hàng).
    /// Exactly mirrors Panel 3 of the design specification:
    /// - Shows destination NPC location: "Giao hàng - Ký túc xá B"
    /// - Order description and greeting
    /// - Rewards: Cash + EXP
    /// - Prominent green [ Giao hàng ] button (Click or Key 'E').
    /// </summary>
    public class IUHDeliveryDialogueUI : MonoBehaviour
    {
        public static IUHDeliveryDialogueUI Instance { get; private set; }

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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            AutoWireReferences();

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
        }

        private void Start()
        {
            CloseDialog();
        }

        private void Update()
        {
            if (m_IsOpen)
            {
                if (IUHInputHelper.IsInteractKeyDown())
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
            m_OnDeliverConfirmed = onDeliverConfirmed;
            m_IsOpen = true;

            if (m_DialogRoot != null) m_DialogRoot.SetActive(true);

            if (m_TxtLocationTitle != null) m_TxtLocationTitle.text = $"📦 Giao hàng — {locationName}";
            if (m_TxtMessage != null) m_TxtMessage.text = message;
            if (m_TxtRewardCoins != null) m_TxtRewardCoins.text = $"+{cashReward:N0} VNĐ";
            if (m_TxtRewardExp != null) m_TxtRewardExp.text = $"+{expReward} XP";

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseDialog()
        {
            m_IsOpen = false;
            if (m_DialogRoot != null) m_DialogRoot.SetActive(false);
            m_OnDeliverConfirmed = null;

            // Only lock cursor back if portal menu isn't open
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
