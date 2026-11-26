using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

namespace IUHCampus.Guardian
{
    public class IUHUpgradeShopUI : MonoBehaviour
    {
        public static IUHUpgradeShopUI Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject m_ShopPanel;
        [SerializeField] private GameObject m_TabUpgradesContent;
        [SerializeField] private GameObject m_TabInventoryContent;

        [Header("Tabs")]
        [SerializeField] private Button m_BtnTabUpgrades;
        [SerializeField] private Button m_BtnTabInventory;
        [SerializeField] private Button m_BtnClose;

        [Header("Currency")]
        [SerializeField] private Text m_TxtGold;

        [Header("Upgrade Row 1 - Sword")]
        [SerializeField] private Text m_TxtSwordCurrent;
        [SerializeField] private Text m_TxtSwordNext;
        [SerializeField] private Button m_BtnUpgradeSword;
        [SerializeField] private Text m_TxtBtnSword;

        [Header("Upgrade Row 2 - Shield")]
        [SerializeField] private Text m_TxtShieldCurrent;
        [SerializeField] private Text m_TxtShieldNext;
        [SerializeField] private Button m_BtnUpgradeShield;
        [SerializeField] private Text m_TxtBtnShield;

        [Header("Upgrade Row 3 - Max HP")]
        [SerializeField] private Text m_TxtHpCurrent;
        [SerializeField] private Text m_TxtHpNext;
        [SerializeField] private Button m_BtnUpgradeHp;
        [SerializeField] private Text m_TxtBtnHp;

        [Header("Inventory Items Container")]
        [SerializeField] private Transform m_ItemsContainer;
        [SerializeField] private Text m_TxtEmptyInventory;

        [SerializeField] private ScrollRect m_ItemsScrollRect;

        private const int MaxUpgradeLevel = 5;
        private const float UpgradeShakeIntensity = 0.15f;
        private const float UpgradeShakeDuration = 0.2f;
        private static readonly Color ActiveTabColor = new Color(0.2f, 0.65f, 1f);
        private static readonly Color IdleTabColor = new Color(0.12f, 0.18f, 0.28f);

        private ThirdPersonController m_PlayerController;
        private StarterAssetsInputs m_PlayerInputs;
        private bool m_IsOpen;
        private int m_CurrentTab;

        public bool IsOpen => m_IsOpen;
        private static bool IsCombatBlocked =>
            IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null &&
            IUHCampus.Guardian.Match3.Match3CombatManager.Instance.BlocksExplorationInput;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            if (m_BtnTabUpgrades != null) m_BtnTabUpgrades.onClick.AddListener(() => SwitchTab(0));
            if (m_BtnTabInventory != null) m_BtnTabInventory.onClick.AddListener(() => SwitchTab(1));
            if (m_BtnClose != null) m_BtnClose.onClick.AddListener(CloseShop);

            if (m_BtnUpgradeSword != null) m_BtnUpgradeSword.onClick.AddListener(OnUpgradeSwordClicked);
            if (m_BtnUpgradeShield != null) m_BtnUpgradeShield.onClick.AddListener(OnUpgradeShieldClicked);
            if (m_BtnUpgradeHp != null) m_BtnUpgradeHp.onClick.AddListener(OnUpgradeHpClicked);

            if (m_ShopPanel != null) m_ShopPanel.SetActive(false);
        }

        private void Start()
        {
            FindPlayer();
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.OnDataChanged += RefreshAllUI;
            }
        }

        private void OnDestroy()
        {
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.OnDataChanged -= RefreshAllUI;
            }
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null)
            {
                m_PlayerController = student.GetComponent<ThirdPersonController>();
                m_PlayerInputs = student.GetComponent<StarterAssetsInputs>();
            }
        }

        private void Update()
        {
            if (IsCombatBlocked)
            {
                IUHCampus.Core.IUHInputHelper.VirtualBackpackPressed = false;
                if (m_IsOpen) CloseShop();
                return;
            }
            // Toggle with key 'B' or 'I' or mobile VirtualBackpackPressed
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.I) || IUHCampus.Core.IUHInputHelper.VirtualBackpackPressed)
            {
                IUHCampus.Core.IUHInputHelper.VirtualBackpackPressed = false;
                if (m_IsOpen) CloseShop();
                else OpenShop();
            }
            else if (m_IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }

        public void ToggleShop()
        {
            if (m_IsOpen) CloseShop();
            else OpenShop(true);
        }

        public void OpenShop(int tabIndex = 0)
        {
            if (IsCombatBlocked) return;
            m_IsOpen = true;
            if (m_ShopPanel != null) m_ShopPanel.SetActive(true);

            SetPlayerControl(false);
            SwitchTab(tabIndex);
            RefreshAllUI();
            IUHCombatAudio.PlayCoin();
        }

        public void OpenShop(bool openToUpgrades)
        {
            OpenShop(openToUpgrades ? 0 : 1);
        }

        public void CloseShop()
        {
            m_IsOpen = false;
            if (m_ShopPanel != null) m_ShopPanel.SetActive(false);
            SetPlayerControl(!IsCombatBlocked);
        }

        private void SetPlayerControl(bool enabled)
        {
            if (m_PlayerController == null || m_PlayerInputs == null) FindPlayer();

            if (m_PlayerController != null) m_PlayerController.enabled = enabled;
            if (m_PlayerInputs != null)
            {
                m_PlayerInputs.cursorLocked = enabled;
                m_PlayerInputs.cursorInputForLook = enabled;
                m_PlayerInputs.move = Vector2.zero;
                m_PlayerInputs.look = Vector2.zero;
            }

            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;
        }

        private void SwitchTab(int tabIndex)
        {
            tabIndex = tabIndex == 1 ? 1 : 0;
            m_CurrentTab = tabIndex;
            if (m_TabUpgradesContent != null) m_TabUpgradesContent.SetActive(tabIndex == 0);
            if (m_TabInventoryContent != null) m_TabInventoryContent.SetActive(tabIndex == 1);

            if (m_BtnTabUpgrades != null)
            {
                var img = m_BtnTabUpgrades.GetComponent<Image>();
                if (img != null) img.color = tabIndex == 0 ? ActiveTabColor : IdleTabColor;
            }
            if (m_BtnTabInventory != null)
            {
                var img = m_BtnTabInventory.GetComponent<Image>();
                if (img != null) img.color = tabIndex == 1 ? ActiveTabColor : IdleTabColor;
            }

            RefreshAllUI();
        }

        private void OnUpgradeSwordClicked()
        {
            if (IUHGuardianInventory.Instance != null && IUHGuardianInventory.Instance.UpgradeSword())
            {
                IUHCombatAudio.PlayCoin();
                IUHCameraShake.Shake(UpgradeShakeIntensity, UpgradeShakeDuration);
            }
        }

        private void OnUpgradeShieldClicked()
        {
            if (IUHGuardianInventory.Instance != null && IUHGuardianInventory.Instance.UpgradeShield())
            {
                IUHCombatAudio.PlayCoin();
                IUHCameraShake.Shake(UpgradeShakeIntensity, UpgradeShakeDuration);
            }
        }

        private void OnUpgradeHpClicked()
        {
            if (IUHGuardianInventory.Instance != null && IUHGuardianInventory.Instance.UpgradeMaxHp())
            {
                IUHCombatAudio.PlayCoin();
                IUHCameraShake.Shake(UpgradeShakeIntensity, UpgradeShakeDuration);
            }
        }

        public void RefreshAllUI()
        {
            var inv = IUHGuardianInventory.Instance;
            if (inv == null) return;

            if (m_TxtGold != null) m_TxtGold.text = $"<b>{inv.Gold} Vàng</b>";

            RefreshUpgradeRow(
                m_TxtSwordCurrent, m_TxtSwordNext, m_TxtBtnSword, m_BtnUpgradeSword,
                inv.SwordLevel, $"Cấp {inv.SwordLevel}: <b>{inv.GetSwordDamage()} DMG / ô kiếm</b>",
                $"Tiếp: <b>{inv.GetNextSwordDamage()} DMG</b>", inv.GetSwordUpgradeCost(), inv.Gold);

            RefreshUpgradeRow(
                m_TxtShieldCurrent, m_TxtShieldNext, m_TxtBtnShield, m_BtnUpgradeShield,
                inv.ShieldLevel, $"Cấp {inv.ShieldLevel}: <b>{inv.GetShieldValue()} Giáp / ô khiên</b>",
                $"Tiếp: <b>{inv.GetNextShieldValue()} Giáp</b>", inv.GetShieldUpgradeCost(), inv.Gold);

            RefreshUpgradeRow(
                m_TxtHpCurrent, m_TxtHpNext, m_TxtBtnHp, m_BtnUpgradeHp,
                inv.MaxHpLevel, $"Cấp {inv.MaxHpLevel}: <b>{inv.GetMaxHp()} Máu Tối Đa</b>",
                $"Tiếp: <b>{inv.GetNextMaxHp()} Máu</b>", inv.GetMaxHpUpgradeCost(), inv.Gold);

            RefreshInventoryTab(inv);

            if (m_CurrentTab == 1) ResetScrollToTop(m_ItemsScrollRect);
        }

        private static void RefreshUpgradeRow(
            Text currentTxt, Text nextTxt, Text btnTxt, Button btn,
            int level, string currentLabel, string nextLabel, int cost, int gold)
        {
            if (currentTxt != null) currentTxt.text = currentLabel;
            if (level < MaxUpgradeLevel)
            {
                if (nextTxt != null) nextTxt.text = nextLabel;
                if (btnTxt != null) btnTxt.text = $"NÂNG CẤP ({cost} Vàng)";
                if (btn != null) btn.interactable = gold >= cost;
            }
            else
            {
                if (nextTxt != null) nextTxt.text = "<color=#4ADE80><b>ĐẠT TỐI ĐA (MAX)</b></color>";
                if (btnTxt != null) btnTxt.text = "TỐI ĐA";
                if (btn != null) btn.interactable = false;
            }
        }

        private static void ResetScrollToTop(ScrollRect scroll)
        {
            if (scroll == null || !scroll.gameObject.activeInHierarchy) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
        }

        private void RefreshInventoryTab(IUHGuardianInventory inv)
        {
            if (m_ItemsContainer == null) return;

            ClearContainer(m_ItemsContainer);

            var items = inv.Items;
            if (m_TxtEmptyInventory != null)
            {
                m_TxtEmptyInventory.gameObject.SetActive(items.Count == 0);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            foreach (var item in items)
            {
                GameObject row = CreateInventoryRow(item.name, item.count, item.description, font);
                if (item.isConsumable)
                {
                    CreateUseButton(row.transform, font, inv, item.name);
                }
            }
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                child.SetParent(null, false);
                Destroy(child.gameObject);
            }
        }

        private GameObject CreateInventoryRow(string itemName, int count, string description, Font font)
        {
            GameObject row = new GameObject("ItemRow", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(m_ItemsContainer, false);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 48f);
            row.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.2f, 0.9f);

            GameObject nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
            nameGo.transform.SetParent(row.transform, false);
            var nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0.03f, 0.5f);
            nameRt.anchorMax = new Vector2(0.03f, 0.5f);
            nameRt.pivot = new Vector2(0f, 0.5f);
            nameRt.sizeDelta = new Vector2(330f, 36f);
            var nameTxt = nameGo.GetComponent<Text>();
            nameTxt.text = $"<b>{itemName}</b> <color=#FDE047>x{count}</color>\n<size=11><color=#94A3B8>{description}</color></size>";
            nameTxt.font = font;
            nameTxt.fontSize = 13;
            nameTxt.color = Color.white;
            nameTxt.alignment = TextAnchor.MiddleLeft;
            return row;
        }

        private static void CreateUseButton(Transform rowTransform, Font font, IUHGuardianInventory inv, string itemName)
        {
            GameObject btnGo = new GameObject("BtnUse", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(rowTransform, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.85f, 0.5f);
            btnRt.anchorMax = new Vector2(0.85f, 0.5f);
            btnRt.sizeDelta = new Vector2(100f, 32f);
            btnGo.GetComponent<Image>().color = new Color(0.16f, 0.65f, 0.38f);

            GameObject bTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            bTxtGo.transform.SetParent(btnGo.transform, false);
            var bTxtRt = bTxtGo.GetComponent<RectTransform>();
            bTxtRt.anchorMin = Vector2.zero;
            bTxtRt.anchorMax = Vector2.one;
            bTxtRt.sizeDelta = Vector2.zero;
            var bTxt = bTxtGo.GetComponent<Text>();
            bTxt.text = "SỬ DỤNG";
            bTxt.font = font;
            bTxt.fontSize = 12;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;

            btnGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (inv.UseItem(itemName))
                {
                    IUHCombatAudio.PlayHeal();
                    IUHCameraShake.Shake(UpgradeShakeIntensity, UpgradeShakeDuration);
                }
            });
        }
    }
}
