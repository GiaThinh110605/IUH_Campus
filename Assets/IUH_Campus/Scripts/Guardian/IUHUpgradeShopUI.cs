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

            Color activeColor = new Color(0.2f, 0.65f, 1f);
            Color idleColor = new Color(0.12f, 0.18f, 0.28f);

            if (m_BtnTabUpgrades != null)
            {
                var img = m_BtnTabUpgrades.GetComponent<Image>();
                if (img != null) img.color = tabIndex == 0 ? activeColor : idleColor;
            }
            if (m_BtnTabInventory != null)
            {
                var img = m_BtnTabInventory.GetComponent<Image>();
                if (img != null) img.color = tabIndex == 1 ? activeColor : idleColor;
            }

            RefreshAllUI();
        }

        private void OnUpgradeSwordClicked()
        {
            if (IUHGuardianInventory.Instance != null && IUHGuardianInventory.Instance.UpgradeSword())
            {
                IUHCombatAudio.PlayCoin();
                IUHCameraShake.Shake(0.15f, 0.2f);
            }
        }

        private void OnUpgradeShieldClicked()
        {
            if (IUHGuardianInventory.Instance != null && IUHGuardianInventory.Instance.UpgradeShield())
            {
                IUHCombatAudio.PlayCoin();
                IUHCameraShake.Shake(0.15f, 0.2f);
            }
        }

        private void OnUpgradeHpClicked()
        {
            if (IUHGuardianInventory.Instance != null && IUHGuardianInventory.Instance.UpgradeMaxHp())
            {
                IUHCombatAudio.PlayCoin();
                IUHCameraShake.Shake(0.15f, 0.2f);
            }
        }

        public void RefreshAllUI()
        {
            var inv = IUHGuardianInventory.Instance;
            if (inv == null) return;

            // Currency
            if (m_TxtGold != null) m_TxtGold.text = $"<b>{inv.Gold} Vàng</b>";

            // Sword Upgrade
            int swLvl = inv.SwordLevel;
            int swDmg = inv.GetSwordDamage();
            int swCost = inv.GetSwordUpgradeCost();
            if (m_TxtSwordCurrent != null) m_TxtSwordCurrent.text = $"Cấp {swLvl}: <b>{swDmg} DMG / ô kiếm</b>";
            if (swLvl < 5)
            {
                if (m_TxtSwordNext != null) m_TxtSwordNext.text = $"Tiếp: <b>{inv.GetNextSwordDamage()} DMG</b>";
                if (m_TxtBtnSword != null) m_TxtBtnSword.text = $"NÂNG CẤP ({swCost} Vàng)";
                if (m_BtnUpgradeSword != null) m_BtnUpgradeSword.interactable = inv.Gold >= swCost;
            }
            else
            {
                if (m_TxtSwordNext != null) m_TxtSwordNext.text = "<color=#4ADE80><b>ĐẠT TỐI ĐA (MAX)</b></color>";
                if (m_TxtBtnSword != null) m_TxtBtnSword.text = "TỐI ĐA";
                if (m_BtnUpgradeSword != null) m_BtnUpgradeSword.interactable = false;
            }

            // Shield Upgrade
            int shLvl = inv.ShieldLevel;
            int shVal = inv.GetShieldValue();
            int shCost = inv.GetShieldUpgradeCost();
            if (m_TxtShieldCurrent != null) m_TxtShieldCurrent.text = $"Cấp {shLvl}: <b>{shVal} Giáp / ô khiên</b>";
            if (shLvl < 5)
            {
                if (m_TxtShieldNext != null) m_TxtShieldNext.text = $"Tiếp: <b>{inv.GetNextShieldValue()} Giáp</b>";
                if (m_TxtBtnShield != null) m_TxtBtnShield.text = $"NÂNG CẤP ({shCost} Vàng)";
                if (m_BtnUpgradeShield != null) m_BtnUpgradeShield.interactable = inv.Gold >= shCost;
            }
            else
            {
                if (m_TxtShieldNext != null) m_TxtShieldNext.text = "<color=#4ADE80><b>ĐẠT TỐI ĐA (MAX)</b></color>";
                if (m_TxtBtnShield != null) m_TxtBtnShield.text = "TỐI ĐA";
                if (m_BtnUpgradeShield != null) m_BtnUpgradeShield.interactable = false;
            }

            // HP Upgrade
            int hpLvl = inv.MaxHpLevel;
            int hpVal = inv.GetMaxHp();
            int hpCost = inv.GetMaxHpUpgradeCost();
            if (m_TxtHpCurrent != null) m_TxtHpCurrent.text = $"Cấp {hpLvl}: <b>{hpVal} Máu Tối Đa</b>";
            if (hpLvl < 5)
            {
                if (m_TxtHpNext != null) m_TxtHpNext.text = $"Tiếp: <b>{inv.GetNextMaxHp()} Máu</b>";
                if (m_TxtBtnHp != null) m_TxtBtnHp.text = $"NÂNG CẤP ({hpCost} Vàng)";
                if (m_BtnUpgradeHp != null) m_BtnUpgradeHp.interactable = inv.Gold >= hpCost;
            }
            else
            {
                if (m_TxtHpNext != null) m_TxtHpNext.text = "<color=#4ADE80><b>ĐẠT TỐI ĐA (MAX)</b></color>";
                if (m_TxtBtnHp != null) m_TxtBtnHp.text = "TỐI ĐA";
                if (m_BtnUpgradeHp != null) m_BtnUpgradeHp.interactable = false;
            }

            // Inventory Items
            RefreshInventoryTab(inv);

            if (m_CurrentTab == 1) ResetScrollToTop(m_ItemsScrollRect);
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

            // Clear old items
            for (int i = m_ItemsContainer.childCount - 1; i >= 0; i--)
            {
                var child = m_ItemsContainer.GetChild(i);
                child.SetParent(null, false);
                Destroy(child.gameObject);
            }

            var items = inv.Items;
            if (m_TxtEmptyInventory != null)
            {
                m_TxtEmptyInventory.gameObject.SetActive(items.Count == 0);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            foreach (var item in items)
            {
                GameObject row = new GameObject("ItemRow", typeof(RectTransform), typeof(Image));
                row.transform.SetParent(m_ItemsContainer, false);
                var rowRt = row.GetComponent<RectTransform>();
                rowRt.sizeDelta = new Vector2(760f, 48f);
                row.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.2f, 0.9f);

                // Name & Count
                GameObject nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
                nameGo.transform.SetParent(row.transform, false);
                var nameRt = nameGo.GetComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0.03f, 0.5f);
                nameRt.anchorMax = new Vector2(0.03f, 0.5f);
                nameRt.pivot = new Vector2(0f, 0.5f);
                nameRt.sizeDelta = new Vector2(330f, 36f);
                var nameTxt = nameGo.GetComponent<Text>();
                nameTxt.text = $"<b>{item.name}</b> <color=#FDE047>x{item.count}</color>\n<size=11><color=#94A3B8>{item.description}</color></size>";
                nameTxt.font = font;
                nameTxt.fontSize = 13;
                nameTxt.color = Color.white;
                nameTxt.alignment = TextAnchor.MiddleLeft;

                // Use Button if consumable
                if (item.isConsumable)
                {
                    GameObject btnGo = new GameObject("BtnUse", typeof(RectTransform), typeof(Image), typeof(Button));
                    btnGo.transform.SetParent(row.transform, false);
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

                    string curItemName = item.name;
                    btnGo.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (inv.UseItem(curItemName))
                        {
                            IUHCombatAudio.PlayHeal();
                            IUHCameraShake.Shake(0.15f, 0.2f);
                        }
                    });
                }
            }
        }
    }
}
