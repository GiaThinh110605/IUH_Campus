using System;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian
{
    [Serializable]
    public class InventoryItem
    {
        public string id;
        public string name;
        public int count;
        public string description;
        public bool isConsumable;

        public InventoryItem(string id, string name, int count, string description, bool isConsumable)
        {
            this.id = id;
            this.name = name;
            this.count = count;
            this.description = description;
            this.isConsumable = isConsumable;
        }
    }

    public class IUHGuardianInventory : MonoBehaviour
    {
        public static IUHGuardianInventory Instance { get; private set; }
        private const string PrefsPrefix = "IUH_Guardian_";
        private const string GoldKey = PrefsPrefix + "Gold";
        private const string SwordKey = PrefsPrefix + "SwordLv";
        private const string ShieldKey = PrefsPrefix + "ShieldLv";
        private const string MaxHpKey = PrefsPrefix + "MaxHpLv";
        private const string ItemsKey = PrefsPrefix + "Items";
        private const string MilestoneKeyPrefix = PrefsPrefix + "Milestone_";

        [Serializable]
        private class InventorySave
        {
            public int version = 1;
            public List<InventoryItem> items;
        }
        private bool m_IsBatchingReward;

        [Header("Currencies")]
        [SerializeField] private int m_Gold = 100;

        [Header("Upgrade Levels (1-5)")]
        [SerializeField] private int m_SwordLevel = 1;
        [SerializeField] private int m_ShieldLevel = 1;
        [SerializeField] private int m_MaxHpLevel = 1;

        [Header("Inventory Items")]
        [SerializeField] private List<InventoryItem> m_Items = new List<InventoryItem>();

        public event Action OnDataChanged;

        public int Gold => m_Gold;
        public int SwordLevel => m_SwordLevel;
        public int ShieldLevel => m_ShieldLevel;
        public int MaxHpLevel => m_MaxHpLevel;
        public IReadOnlyList<InventoryItem> Items => m_Items;

        private static readonly int[] s_SwordDamages = { 6, 8, 11, 14, 18 };
        private static readonly int[] s_SwordCosts = { 80, 150, 250, 400, 0 };

        private static readonly int[] s_ShieldValues = { 5, 8, 11, 15, 20 };
        private static readonly int[] s_ShieldCosts = { 80, 150, 250, 400, 0 };

        private static readonly int[] s_MaxHps = { 100, 125, 155, 190, 240 };
        private static readonly int[] s_MaxHpCosts = { 100, 180, 300, 500, 0 };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadData();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public int GetSwordDamage() => s_SwordDamages[Mathf.Clamp(m_SwordLevel - 1, 0, s_SwordDamages.Length - 1)];
        public int GetNextSwordDamage() => m_SwordLevel < 5 ? s_SwordDamages[m_SwordLevel] : GetSwordDamage();
        public int GetSwordUpgradeCost() => s_SwordCosts[Mathf.Clamp(m_SwordLevel - 1, 0, s_SwordCosts.Length - 1)];

        public int GetShieldValue() => s_ShieldValues[Mathf.Clamp(m_ShieldLevel - 1, 0, s_ShieldValues.Length - 1)];
        public int GetNextShieldValue() => m_ShieldLevel < 5 ? s_ShieldValues[m_ShieldLevel] : GetShieldValue();
        public int GetShieldUpgradeCost() => s_ShieldCosts[Mathf.Clamp(m_ShieldLevel - 1, 0, s_ShieldCosts.Length - 1)];

        public int GetMaxHp() => s_MaxHps[Mathf.Clamp(m_MaxHpLevel - 1, 0, s_MaxHps.Length - 1)];
        public int GetNextMaxHp() => m_MaxHpLevel < 5 ? s_MaxHps[m_MaxHpLevel] : GetMaxHp();
        public int GetMaxHpUpgradeCost() => s_MaxHpCosts[Mathf.Clamp(m_MaxHpLevel - 1, 0, s_MaxHpCosts.Length - 1)];

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            m_Gold += amount;
            SaveData();
            NotifyDataChanged();
        }

        public bool HasClaimedMilestone(string milestoneId)
        {
            return !string.IsNullOrEmpty(milestoneId) &&
                PlayerPrefs.GetInt(MilestoneKeyPrefix + milestoneId, 0) == 1;
        }

        public bool TryClaimMilestoneReward(string milestoneId, int gold, IReadOnlyList<string> items)
        {
            if (string.IsNullOrEmpty(milestoneId) || gold < 0 || HasClaimedMilestone(milestoneId))
                return false;

            m_IsBatchingReward = true;
            try
            {
                AddGold(gold);
                if (items != null)
                    foreach (string item in items) AddItem(item);
                PlayerPrefs.SetInt(MilestoneKeyPrefix + milestoneId, 1);
            }
            finally { m_IsBatchingReward = false; }

            SaveData();
            OnDataChanged?.Invoke();
            return true;
        }

        public bool TrySpendGold(int amount)
        {
            if (m_Gold < amount) return false;
            m_Gold -= amount;
            SaveData();
            OnDataChanged?.Invoke();
            return true;
        }

        public bool UpgradeSword()
        {
            if (m_SwordLevel >= 5) return false;
            int cost = GetSwordUpgradeCost();
            if (TrySpendGold(cost))
            {
                m_SwordLevel++;
                SaveData();
                OnDataChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeShield()
        {
            if (m_ShieldLevel >= 5) return false;
            int cost = GetShieldUpgradeCost();
            if (TrySpendGold(cost))
            {
                m_ShieldLevel++;
                SaveData();
                OnDataChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeMaxHp()
        {
            if (m_MaxHpLevel >= 5) return false;
            int cost = GetMaxHpUpgradeCost();
            if (TrySpendGold(cost))
            {
                m_MaxHpLevel++;
                SaveData();
                OnDataChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void RestoreFullHp()
        {
            if (IUHPlayerOverheadHUD.Instance != null)
            {
                int maxHp = GetMaxHp();
                IUHPlayerOverheadHUD.Instance.SetHp(maxHp, maxHp);
            }
            OnDataChanged?.Invoke();
        }

        public void AddItem(string itemName, int count = 1)
        {
            if (string.IsNullOrEmpty(itemName)) return;

            string cleanName = itemName.Replace("✨ ", "").Trim();
            var existing = m_Items.Find(i => i.name.Equals(cleanName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.count += count;
            }
            else
            {
                bool isConsumable = cleanName.Contains("Bình") || cleanName.Contains("Dược") || cleanName.Contains("Potion");
                string desc = GetItemDescription(cleanName);
                m_Items.Add(new InventoryItem(Guid.NewGuid().ToString(), cleanName, count, desc, isConsumable));
            }

            SaveData();
            NotifyDataChanged();
        }

        public bool UseItem(string itemName)
        {
            var item = m_Items.Find(i => i.name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
            if (item != null && item.count > 0 && item.isConsumable)
            {
                item.count--;
                if (item.count <= 0) m_Items.Remove(item);

                // Heal player overhead HUD if active
                if (IUHPlayerOverheadHUD.Instance != null)
                {
                    int maxHp = GetMaxHp();
                    IUHPlayerOverheadHUD.Instance.SetHp(maxHp, maxHp);
                }

                SaveData();
                OnDataChanged?.Invoke();
                return true;
            }
            return false;
        }

        private string GetItemDescription(string name)
        {
            if (name.Contains("Bình")) return "Dược liệu công nghệ sinh học IUH. Dùng để hồi phục đầy máu!";
            if (name.Contains("Khiên")) return "Trang bị phòng vệ xung lực. Tăng giáp khởi điểm khi vào trận.";
            if (name.Contains("Kiếm")) return "Vũ khí ánh sáng quang học. Tăng cường uy lực ô gạch kiếm.";
            if (name.Contains("Huy Hiệu")) return "Huy hiệu danh dự cao quý trao tặng cho Người Bảo Vệ IUH!";
            if (name.Contains("Đĩa")) return "Dữ liệu âm thanh lễ hội chứa tần số thanh tẩy sóng âm.";
            if (name.Contains("Pin") || name.Contains("Năng Lượng")) return "Lõi pin siêu tụ điện nano, nạp nhanh nộ kỹ năng.";
            return "Vật phẩm dữ liệu quý giá thu thập từ khuôn viên campus.";
        }

        private void SaveData()
        {
            if (m_IsBatchingReward) return;
            WritePlayerPrefs();
            PlayerPrefs.Save();
        }

        private void NotifyDataChanged()
        {
            if (!m_IsBatchingReward) OnDataChanged?.Invoke();
        }

        private void WritePlayerPrefs()
        {
            PlayerPrefs.SetInt(GoldKey, m_Gold);
            PlayerPrefs.SetInt(SwordKey, m_SwordLevel);
            PlayerPrefs.SetInt(ShieldKey, m_ShieldLevel);
            PlayerPrefs.SetInt(MaxHpKey, m_MaxHpLevel);
            PlayerPrefs.SetString(ItemsKey, JsonUtility.ToJson(new InventorySave { items = m_Items }));
        }

        private void LoadData()
        {
            m_Gold = PlayerPrefs.GetInt(GoldKey, 100);
            m_SwordLevel = PlayerPrefs.GetInt(SwordKey, 1);
            m_ShieldLevel = PlayerPrefs.GetInt(ShieldKey, 1);
            m_MaxHpLevel = PlayerPrefs.GetInt(MaxHpKey, 1);

            bool hasSavedItems = PlayerPrefs.HasKey(ItemsKey);
            if (hasSavedItems && TryLoadItems()) return;

            if (m_Items.Count == 0)
            {
                m_Items.Add(new InventoryItem("init_potion", "Bình Dược Liệu Nano (+50 HP)", 1, "Dược phẩm khẩn cấp cấp phát cho sinh viên.", true));
                m_Items.Add(new InventoryItem("init_crystal", "Mảnh Tinh Thể Dữ Liệu Lam", 1, "Mảnh tinh thể nghiên cứu ban đầu.", false));
            }
            if (!hasSavedItems) SaveData();
        }

        private bool TryLoadItems()
        {
            try
            {
                var saved = JsonUtility.FromJson<InventorySave>(PlayerPrefs.GetString(ItemsKey));
                if (saved == null || saved.items == null)
                    throw new ArgumentException("Inventory items list is missing.");

                m_Items = saved.items.FindAll(item => item != null &&
                    !string.IsNullOrWhiteSpace(item.name) && item.count > 0);
                return true;
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("[IUH Guardian] Saved inventory items are invalid; keeping starter inventory.");
                return false;
            }
        }
    }
}

