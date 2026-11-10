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
        [SerializeField] private GameObject m_TabSkinsContent;

        [Header("Tabs")]
        [SerializeField] private Button m_BtnTabUpgrades;
        [SerializeField] private Button m_BtnTabInventory;
        [SerializeField] private Button m_BtnTabSkins;
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

        [Header("Skins Container")]
        [SerializeField] private Transform m_SkinsContainer;

        private ThirdPersonController m_PlayerController;
        private StarterAssetsInputs m_PlayerInputs;
        private bool m_IsOpen;
        private int m_CurrentTab = 0; // 0 = Upgrades, 1 = Inventory, 2 = Skins

        public bool IsOpen => m_IsOpen;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            if (m_BtnTabUpgrades != null) m_BtnTabUpgrades.onClick.AddListener(() => SwitchTab(0));
            if (m_BtnTabInventory != null) m_BtnTabInventory.onClick.AddListener(() => SwitchTab(1));
            if (m_BtnTabSkins != null) m_BtnTabSkins.onClick.AddListener(() => SwitchTab(2));
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
            SetPlayerControl(true);
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
            m_CurrentTab = tabIndex;
            if (m_TabUpgradesContent != null) m_TabUpgradesContent.SetActive(tabIndex == 0);
            if (m_TabInventoryContent != null) m_TabInventoryContent.SetActive(tabIndex == 1);
            if (m_TabSkinsContent != null) m_TabSkinsContent.SetActive(tabIndex == 2);

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
            if (m_BtnTabSkins != null)
            {
                var img = m_BtnTabSkins.GetComponent<Image>();
                if (img != null) img.color = tabIndex == 2 ? activeColor : idleColor;
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

            // Skins & Trails
            RefreshSkinsTab(inv);
        }

        private void RefreshSkinsTab(IUHGuardianInventory inv)
        {
            if (m_SkinsContainer == null) return;
            var customizer = IUHPlayerCustomizer.Instance;
            if (customizer == null) return;

            for (int i = m_SkinsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_SkinsContainer.GetChild(i).gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Helper to create category header
            Action<string> AddHeader = (title) =>
            {
                GameObject hGo = new GameObject("Header", typeof(RectTransform), typeof(Text));
                hGo.transform.SetParent(m_SkinsContainer, false);
                var hRt = hGo.GetComponent<RectTransform>();
                hRt.sizeDelta = new Vector2(490f, 28f);
                var hTxt = hGo.GetComponent<Text>();
                hTxt.text = title;
                hTxt.font = font;
                hTxt.fontSize = 13;
                hTxt.fontStyle = FontStyle.Bold;
                hTxt.color = new Color(0.35f, 0.9f, 1f);
                hTxt.alignment = TextAnchor.MiddleLeft;
            };

            // Helper to create skin/trail row
            Action<string, string, bool, bool, int, Action> AddRow = (name, desc, isOwned, isEquipped, cost, onAction) =>
            {
                GameObject row = new GameObject("CustomRow", typeof(RectTransform), typeof(Image));
                row.transform.SetParent(m_SkinsContainer, false);
                var rowRt = row.GetComponent<RectTransform>();
                rowRt.sizeDelta = new Vector2(490f, 44f);
                row.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.2f, 0.9f);

                // Name & Desc
                GameObject nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
                nameGo.transform.SetParent(row.transform, false);
                var nameRt = nameGo.GetComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0.03f, 0.5f);
                nameRt.anchorMax = new Vector2(0.03f, 0.5f);
                nameRt.pivot = new Vector2(0f, 0.5f);
                nameRt.sizeDelta = new Vector2(330f, 36f);
                var nameTxt = nameGo.GetComponent<Text>();
                nameTxt.text = $"<b>{name}</b>\n<size=11><color=#94A3B8>{desc}</color></size>";
                nameTxt.font = font;
                nameTxt.fontSize = 13;
                nameTxt.color = Color.white;
                nameTxt.alignment = TextAnchor.MiddleLeft;

                // Action Button
                GameObject btnGo = new GameObject("BtnAction", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(row.transform, false);
                var btnRt = btnGo.GetComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0.85f, 0.5f);
                btnRt.anchorMax = new Vector2(0.85f, 0.5f);
                btnRt.sizeDelta = new Vector2(100f, 32f);

                GameObject bTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                bTxtGo.transform.SetParent(btnGo.transform, false);
                var bTxtRt = bTxtGo.GetComponent<RectTransform>();
                bTxtRt.anchorMin = Vector2.zero;
                bTxtRt.anchorMax = Vector2.one;
                bTxtRt.sizeDelta = Vector2.zero;
                var bTxt = bTxtGo.GetComponent<Text>();
                bTxt.font = font;
                bTxt.fontSize = 11;
                bTxt.fontStyle = FontStyle.Bold;
                bTxt.alignment = TextAnchor.MiddleCenter;
                bTxt.color = Color.white;

                var btn = btnGo.GetComponent<Button>();
                var btnImg = btnGo.GetComponent<Image>();

                if (isEquipped)
                {
                    bTxt.text = "ĐANG DÙNG";
                    btnImg.color = new Color(0.3f, 0.35f, 0.45f);
                    btn.interactable = false;
                }
                else if (isOwned)
                {
                    bTxt.text = "TRANG BỊ";
                    btnImg.color = new Color(0.2f, 0.6f, 0.9f);
                    btn.interactable = true;
                    btn.onClick.AddListener(() =>
                    {
                        onAction?.Invoke();
                        IUHCombatAudio.PlayShield();
                        RefreshAllUI();
                    });
                }
                else
                {
                    bTxt.text = $"MUA ({cost}g)";
                    btnImg.color = new Color(0.16f, 0.65f, 0.38f);
                    btn.interactable = inv.Gold >= cost;
                    btn.onClick.AddListener(() =>
                    {
                        if (inv.TrySpendGold(cost))
                        {
                            onAction?.Invoke();
                            IUHCombatAudio.PlayCoin();
                            IUHCameraShake.Shake(0.12f, 0.2f);
                            RefreshAllUI();
                        }
                    });
                }
            };

            // Section 1: Skins
            AddHeader("TRANG PHỤC HỌC ĐƯỜNG & CHIẾN BINH");

            AddRow("Người Bảo Vệ Xanh IUH", "Áo khoác xanh rêu truyền thống của đội bảo vệ", true,
                customizer.CurrentSkin == PlayerSkinType.Default, 0, () => customizer.ApplySkin(PlayerSkinType.Default));

            AddRow("Đồng Phục Sinh Viên IUH", "Áo thun polo xanh IUH & quần jeans trẻ trung",
                customizer.IsSkinOwned(PlayerSkinType.IUH_Student), customizer.CurrentSkin == PlayerSkinType.IUH_Student, 80,
                () => { customizer.UnlockSkin(PlayerSkinType.IUH_Student); customizer.ApplySkin(PlayerSkinType.IUH_Student); });

            AddRow("Cử Nhân Tốt Nghiệp IUH", "Áo thụng cử nhân đỏ đô viền vàng & nón cử nhân",
                customizer.IsSkinOwned(PlayerSkinType.Graduate), customizer.CurrentSkin == PlayerSkinType.Graduate, 150,
                () => { customizer.UnlockSkin(PlayerSkinType.Graduate); customizer.ApplySkin(PlayerSkinType.Graduate); });

            AddRow("Giáp Cơ Động Cyber Bảo Vệ", "Giáp công nghệ cao phát sáng hào quang neon cyan",
                customizer.IsSkinOwned(PlayerSkinType.Cyber), customizer.CurrentSkin == PlayerSkinType.Cyber, 250,
                () => { customizer.UnlockSkin(PlayerSkinType.Cyber); customizer.ApplySkin(PlayerSkinType.Cyber); });

            // Section 2: Trails
            AddHeader("⚡ HIỆU ỨNG VỆT CHÂN KHI DI CHUYỂN");

            AddRow("Không Dùng Vệt Chân", "Tắt hiệu ứng bước chân", true,
                customizer.CurrentTrail == PlayerTrailType.None, 0, () => customizer.ApplyTrail(PlayerTrailType.None));

            AddRow("Tia Điện Cyber Neon", "Vệt sáng cyan sấm sét lướt theo từng bước chân",
                customizer.IsTrailOwned(PlayerTrailType.CyberSparks), customizer.CurrentTrail == PlayerTrailType.CyberSparks, 60,
                () => { customizer.UnlockTrail(PlayerTrailType.CyberSparks); customizer.ApplyTrail(PlayerTrailType.CyberSparks); });

            AddRow("Cánh Hoa Đào Rơi", "Cánh hoa hồng phấn bay bổng thơ mộng khi chạy",
                customizer.IsTrailOwned(PlayerTrailType.CherryPetals), customizer.CurrentTrail == PlayerTrailType.CherryPetals, 80,
                () => { customizer.UnlockTrail(PlayerTrailType.CherryPetals); customizer.ApplyTrail(PlayerTrailType.CherryPetals); });

            AddRow("Ánh Sao Hoàng Kim", "Ánh sao vàng hoàng gia lấp lánh rực rỡ",
                customizer.IsTrailOwned(PlayerTrailType.GoldenStars), customizer.CurrentTrail == PlayerTrailType.GoldenStars, 100,
                () => { customizer.UnlockTrail(PlayerTrailType.GoldenStars); customizer.ApplyTrail(PlayerTrailType.GoldenStars); });
        }

        private void RefreshInventoryTab(IUHGuardianInventory inv)
        {
            if (m_ItemsContainer == null) return;

            // Clear old items
            for (int i = m_ItemsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(m_ItemsContainer.GetChild(i).gameObject);
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
                rowRt.sizeDelta = new Vector2(490f, 44f);
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
                nameTxt.text = $"✨ <b>{item.name}</b> <color=#FDE047>x{item.count}</color>\n<size=11><color=#94A3B8>{item.description}</color></size>";
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
