using System;
using System.Collections.Generic;
using UnityEngine;
using IUHCampus.Guardian;
using StarterAssets;

namespace IUHCampus.DailyActivities
{
    public enum MascotQuestState
    {
        NotStarted,
        InProgress,
        Completed
    }

    [Serializable]
    public class CheckInRecord
    {
        public string spotId;
        public string spotName;
        public string badgeName;
        public string description;
        public string photoPath; // or placeholder color/icon
        public bool isUnlocked;
        public string unlockDate;
    }

    public class IUHCampusDailyActivitiesManager : MonoBehaviour
    {
        public static IUHCampusDailyActivitiesManager Instance { get; private set; }

        [Header("Coin Hunt Config")]
        [SerializeField] private int m_TotalCoins = 8;
        [SerializeField] private int m_CoinRewardGold = 15;
        [SerializeField] private int m_CoinAllClearBonusGold = 100;

        [Header("Mystery Gift Config")]
        [SerializeField] private int m_MysteryGoldReward = 200;
        [SerializeField] private string m_MysteryCostumeReward = "Áo Khoác Cyber Guardian IUH Neon";
        [SerializeField] private string m_MysteryBadgeReward = "Huy Hiệu Nhà Khai Phá Bí Ẩn IUH";

        [Header("Mascot Quest Config")]
        [SerializeField] private int m_MascotQuestItemsTarget = 3;
        [SerializeField] private int m_MascotGoldReward = 150;
        [SerializeField] private string m_MascotBadgeReward = "Huy Hiệu Bạn Thân Linh Vật IUH";

        [Header("Check-in Records")]
        [SerializeField] private List<CheckInRecord> m_CheckInSpots = new List<CheckInRecord>();

        [Header("Runtime State")]
        [SerializeField] private int m_CoinsCollectedCount = 0;
        [SerializeField] private bool m_MysteryGiftOpened = false;
        [SerializeField] private MascotQuestState m_MascotState = MascotQuestState.NotStarted;
        [SerializeField] private int m_MascotItemsCollected = 0;
        [SerializeField] private bool m_LuckyStarCollected = false;

        public event Action OnActivitiesStateChanged;
        public event Action<string, string> OnActivityBannerNotification; // title, message

        public int TotalCoins => m_TotalCoins;
        public int CoinsCollectedCount => m_CoinsCollectedCount;
        public bool IsCoinHuntCompleted => m_CoinsCollectedCount >= m_TotalCoins;
        public bool MysteryGiftOpened => m_MysteryGiftOpened;
        public MascotQuestState MascotState => m_MascotState;
        public int MascotItemsCollected => m_MascotItemsCollected;
        public int MascotItemsTarget => m_MascotQuestItemsTarget;
        public bool LuckyStarCollected => m_LuckyStarCollected;
        public IReadOnlyList<CheckInRecord> CheckInSpots => m_CheckInSpots;

        private string m_TodayKey;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSpots();
                LoadDailyProgress();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSpots()
        {
            if (m_CheckInSpots == null || m_CheckInSpots.Count == 0)
            {
                m_CheckInSpots = new List<CheckInRecord>
                {
                    new CheckInRecord {
                        spotId = "SPOT_FOUNTAIN",
                        spotName = "Đài Phun Nước Trung Tâm",
                        badgeName = "Huy Hiệu: Giọt Nước Tri Thức",
                        description = "Đài phun nước biểu tượng giữa quảng trường chính IUH, nơi sinh viên tụ họp và thư giãn sau giờ học.",
                        isUnlocked = false
                    },
                    new CheckInRecord {
                        spotId = "SPOT_GATE",
                        spotName = "Cổng Chào IUH Nguyễn Văn Bảo",
                        badgeName = "Huy Hiệu: Khởi Đầu Tương Lai",
                        description = "Cổng chính uy nghiêm số 12 Nguyễn Văn Bảo - niềm tự hào chào đón hàng chục nghìn tân sinh viên mỗi năm.",
                        isUnlocked = false
                    },
                    new CheckInRecord {
                        spotId = "SPOT_FESTIVAL",
                        spotName = "Quảng Trường Lễ Hội & Âm Nhạc",
                        badgeName = "Huy Hiệu: Trái Tim Sôi Động",
                        description = "Sân khấu nhạc sống rực rỡ sắc màu, trung tâm của các ngày hội văn hóa, CLB sinh viên và sự kiện IUH.",
                        isUnlocked = false
                    },
                    new CheckInRecord {
                        spotId = "SPOT_BUILDING_E",
                        spotName = "Tòa Nhà Kỹ Thuật E",
                        badgeName = "Huy Hiệu: Kỹ Sư Công Nghệ",
                        description = "Khu phức hợp phòng thí nghiệm hiện đại, chế tạo robot, IoT và nghiên cứu kỹ thuật hàng đầu.",
                        isUnlocked = false
                    },
                    new CheckInRecord {
                        spotId = "SPOT_GARDEN",
                        spotName = "Vườn Cây Tri Thức IUH",
                        badgeName = "Huy Hiệu: Khoảng Lặng Bình Yên",
                        description = "Không gian xanh mát rợp bóng cây, nơi lý tưởng để đọc sách, trao đổi đồ án và tận hưởng thiên nhiên.",
                        isUnlocked = false
                    }
                };
            }
        }

        private void LoadDailyProgress()
        {
            m_TodayKey = DateTime.UtcNow.ToString("yyyyMMdd");
            string savedDate = PlayerPrefs.GetString("IUH_ACTIVITY_DATE", "");

            if (savedDate != m_TodayKey)
            {
                // New day! Reset daily events
                PlayerPrefs.SetString("IUH_ACTIVITY_DATE", m_TodayKey);
                m_CoinsCollectedCount = 0;
                m_MysteryGiftOpened = false;
                m_MascotState = MascotQuestState.NotStarted;
                m_MascotItemsCollected = 0;
                m_LuckyStarCollected = false;

                PlayerPrefs.SetInt($"IUH_COINS_COLLECTED_{m_TodayKey}", 0);
                PlayerPrefs.SetInt($"IUH_MYSTERY_OPENED_{m_TodayKey}", 0);
                PlayerPrefs.SetInt($"IUH_MASCOT_STATE_{m_TodayKey}", 0);
                PlayerPrefs.SetInt($"IUH_MASCOT_ITEMS_{m_TodayKey}", 0);
                PlayerPrefs.SetInt($"IUH_LUCKY_STAR_{m_TodayKey}", 0);
                PlayerPrefs.Save();
            }
            else
            {
                m_CoinsCollectedCount = PlayerPrefs.GetInt($"IUH_COINS_COLLECTED_{m_TodayKey}", 0);
                m_MysteryGiftOpened = PlayerPrefs.GetInt($"IUH_MYSTERY_OPENED_{m_TodayKey}", 0) == 1;
                m_MascotState = (MascotQuestState)PlayerPrefs.GetInt($"IUH_MASCOT_STATE_{m_TodayKey}", 0);
                m_MascotItemsCollected = PlayerPrefs.GetInt($"IUH_MASCOT_ITEMS_{m_TodayKey}", 0);
                m_LuckyStarCollected = PlayerPrefs.GetInt($"IUH_LUCKY_STAR_{m_TodayKey}", 0) == 1;
            }

            // Load persistent check-in album state
            foreach (var spot in m_CheckInSpots)
            {
                spot.isUnlocked = PlayerPrefs.GetInt("IUH_CHECKIN_" + spot.spotId, 0) == 1;
                spot.unlockDate = PlayerPrefs.GetString("IUH_CHECKIN_DATE_" + spot.spotId, "Chưa khám phá");
            }

            OnActivitiesStateChanged?.Invoke();
        }

        // ================= 1. DAILY COIN HUNT ================= //
        public bool IsCoinCollected(int coinIndex)
        {
            return PlayerPrefs.GetInt($"IUH_COIN_IDX_{m_TodayKey}_{coinIndex}", 0) == 1;
        }

        public void CollectCoin(int coinIndex, Vector3 worldPos)
        {
            if (IsCoinCollected(coinIndex)) return;

            PlayerPrefs.SetInt($"IUH_COIN_IDX_{m_TodayKey}_{coinIndex}", 1);
            m_CoinsCollectedCount++;
            PlayerPrefs.SetInt($"IUH_COINS_COLLECTED_{m_TodayKey}", m_CoinsCollectedCount);
            PlayerPrefs.Save();

            // Reward single coin
            IUHCombatAudio.PlayCoin();
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(m_CoinRewardGold);
            }

            IUHCombatFloatingText.Spawn(worldPos + Vector3.up * 1.5f, $"+{m_CoinRewardGold} Vàng (Xu {m_CoinsCollectedCount}/{m_TotalCoins})", new Color(1f, 0.9f, 0.2f), 1.2f);

            // Check all clear
            if (m_CoinsCollectedCount >= m_TotalCoins)
            {
                IUHCombatAudio.PlayCombo(4);
                if (IUHGuardianInventory.Instance != null)
                {
                    IUHGuardianInventory.Instance.AddGold(m_CoinAllClearBonusGold);
                    IUHGuardianInventory.Instance.AddItem("Huy Hiệu: Thợ Săn Xu IUH Hạng Nhất");
                }
                OnActivityBannerNotification?.Invoke("🪙 SĂN XU HOÀN TẤT!", $"Bạn đã tìm đủ {m_TotalCoins}/{m_TotalCoins} đồng xu hôm nay!\nNhận thưởng: +{m_CoinAllClearBonusGold} Vàng & Huy Hiệu Thợ Săn Xu.");
            }

            OnActivitiesStateChanged?.Invoke();
        }

        // ================= 2. MYSTERY GIFT ================= //
        public void OpenMysteryGift(Vector3 worldPos, Action<string> onRewardDetermined)
        {
            if (m_MysteryGiftOpened) return;

            m_MysteryGiftOpened = true;
            PlayerPrefs.SetInt($"IUH_MYSTERY_OPENED_{m_TodayKey}", 1);
            PlayerPrefs.Save();

            IUHCombatAudio.PlaySkillBurst();
            IUHCombatAudio.PlayCoin();

            // Roll prize: 50% big gold, 25% costume, 25% rare badge
            int roll = UnityEngine.Random.Range(0, 100);
            string rewardText;

            if (roll < 50)
            {
                int goldAmount = UnityEngine.Random.Range(180, 260);
                if (IUHGuardianInventory.Instance != null) IUHGuardianInventory.Instance.AddGold(goldAmount);
                rewardText = $"+{goldAmount} Vàng Khổng Lồ!";
            }
            else if (roll < 75)
            {
                if (IUHGuardianInventory.Instance != null)
                {
                    IUHGuardianInventory.Instance.AddGold(100);
                    IUHGuardianInventory.Instance.AddItem(m_MysteryCostumeReward);
                }
                rewardText = $"Trang phục hiếm [{m_MysteryCostumeReward}] + 100 Vàng!";
            }
            else
            {
                if (IUHGuardianInventory.Instance != null)
                {
                    IUHGuardianInventory.Instance.AddGold(100);
                    IUHGuardianInventory.Instance.AddItem(m_MysteryBadgeReward);
                }
                rewardText = $"Huy Hiệu Quý [{m_MysteryBadgeReward}] + 100 Vàng!";
            }

            IUHCombatFloatingText.Spawn(worldPos + Vector3.up * 2f, $"🎁 HỘP QUÀ BÍ ẨN: {rewardText}", new Color(1f, 0.4f, 0.9f), 1.5f);
            onRewardDetermined?.Invoke(rewardText);
            OnActivityBannerNotification?.Invoke("🎁 MỞ HỘP QUÀ BÍ ẨN!", $"Bạn đã tìm thấy Hộp Quà Giấu Trong Trường!\nPhần thưởng: {rewardText}");

            OnActivitiesStateChanged?.Invoke();
        }

        // ================= 3. MASCOT CHALLENGE ================= //
        public void AcceptMascotQuest()
        {
            if (m_MascotState != MascotQuestState.NotStarted) return;

            m_MascotState = MascotQuestState.InProgress;
            m_MascotItemsCollected = 0;
            PlayerPrefs.SetInt($"IUH_MASCOT_STATE_{m_TodayKey}", (int)m_MascotState);
            PlayerPrefs.SetInt($"IUH_MASCOT_ITEMS_{m_TodayKey}", 0);
            PlayerPrefs.Save();

            IUHCombatAudio.PlayShield();
            OnActivityBannerNotification?.Invoke("🤖 THỬ THÁCH LINH VẬT BẮT ĐẦU!", $"Hãy tìm và thu thập {m_MascotQuestItemsTarget} Vi Mạch Dữ Liệu đang rơi quanh trường!");
            OnActivitiesStateChanged?.Invoke();
        }

        public void CollectMascotQuestItem(int itemIndex, Vector3 worldPos)
        {
            if (m_MascotState != MascotQuestState.InProgress) return;

            m_MascotItemsCollected++;
            PlayerPrefs.SetInt($"IUH_MASCOT_ITEMS_{m_TodayKey}", m_MascotItemsCollected);
            PlayerPrefs.Save();

            IUHCombatAudio.PlayCoin();
            IUHCombatFloatingText.Spawn(worldPos + Vector3.up * 1.5f, $"📦 Vi Mạch Dữ Liệu ({m_MascotItemsCollected}/{m_MascotQuestItemsTarget})", new Color(0.2f, 0.9f, 1f), 1.2f);

            if (m_MascotItemsCollected >= m_MascotQuestItemsTarget)
            {
                IUHCombatAudio.PlayHeal();
                OnActivityBannerNotification?.Invoke("🤖 ĐÃ THU THẬP ĐỦ VI MẠCH!", "Hãy quay lại gặp Linh Vật IUH để hoàn thành thử thách và nhận quà!");
            }

            OnActivitiesStateChanged?.Invoke();
        }

        public void CompleteMascotQuest(Action onCompleted)
        {
            if (m_MascotState != MascotQuestState.InProgress || m_MascotItemsCollected < m_MascotQuestItemsTarget) return;

            m_MascotState = MascotQuestState.Completed;
            PlayerPrefs.SetInt($"IUH_MASCOT_STATE_{m_TodayKey}", (int)m_MascotState);
            PlayerPrefs.Save();

            IUHCombatAudio.PlayCombo(5);
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(m_MascotGoldReward);
                IUHGuardianInventory.Instance.AddItem(m_MascotBadgeReward);
            }

            onCompleted?.Invoke();
            OnActivityBannerNotification?.Invoke("🎉 HOÀN THÀNH THỬ THÁCH LINH VẬT!", $"Linh Vật rất vui mừng và tặng bạn:\n+{m_MascotGoldReward} Vàng & {m_MascotBadgeReward}");
            OnActivitiesStateChanged?.Invoke();
        }

        // ================= 4. CHECK-IN CHALLENGE & ALBUM ================= //
        public bool TriggerCheckInSpot(string spotId, Vector3 worldPos)
        {
            var spot = m_CheckInSpots.Find(s => s.spotId == spotId);
            if (spot == null) return false;

            bool isFirstTime = !spot.isUnlocked;
            spot.isUnlocked = true;
            spot.unlockDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            PlayerPrefs.SetInt("IUH_CHECKIN_" + spot.spotId, 1);
            PlayerPrefs.SetString("IUH_CHECKIN_DATE_" + spot.spotId, spot.unlockDate);
            PlayerPrefs.Save();

            IUHCombatAudio.PlayShield();
            IUHCombatAudio.PlayCoin();

            int rewardGold = 35;
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(rewardGold);
                IUHGuardianInventory.Instance.AddItem(spot.badgeName);
            }

            IUHCombatFloatingText.Spawn(worldPos + Vector3.up * 1.8f, $"📸 CHECK-IN: {spot.spotName}!", new Color(0.3f, 0.95f, 1f), 1.3f);
            IUHCombatFloatingText.Spawn(worldPos + Vector3.up * 2.2f, $"+{rewardGold} Vàng & {spot.badgeName}", new Color(1f, 0.9f, 0.2f), 1.2f);

            OnActivityBannerNotification?.Invoke($"📸 CHECK-IN THÀNH CÔNG: {spot.spotName}!", $"Đã lưu vào Album Kỷ Niệm IUH!\nNhận thưởng: +{rewardGold} Vàng & {spot.badgeName}.");
            OnActivitiesStateChanged?.Invoke();

            return isFirstTime;
        }

        // ================= 5. LUCKY STAR ================= //
        public void CollectLuckyStar(Vector3 worldPos, Action<string> onRewardMsg)
        {
            if (m_LuckyStarCollected) return;

            m_LuckyStarCollected = true;
            PlayerPrefs.SetInt($"IUH_LUCKY_STAR_{m_TodayKey}", 1);
            PlayerPrefs.Save();

            IUHCombatAudio.PlayCombo(6);
            IUHCombatAudio.PlaySkillBurst();

            // Random Surprise Buff or Jackpot
            int roll = UnityEngine.Random.Range(0, 3);
            string rewardSummary;

            if (roll == 0)
            {
                int jackpotGold = UnityEngine.Random.Range(300, 500);
                if (IUHGuardianInventory.Instance != null) IUHGuardianInventory.Instance.AddGold(jackpotGold);
                rewardSummary = $"⭐ JACKPOT KHỦNG: +{jackpotGold} Vàng!";
            }
            else if (roll == 1)
            {
                // Speed boost buff for 60 seconds
                ApplySpeedBoost(1.6f, 60f);
                if (IUHGuardianInventory.Instance != null) IUHGuardianInventory.Instance.AddGold(150);
                rewardSummary = "⭐ BUFF TỐC ĐỘ SIÊU TỐC: +60% Tốc Chạy (60s) & +150 Vàng!";
            }
            else
            {
                if (IUHGuardianInventory.Instance != null)
                {
                    IUHGuardianInventory.Instance.AddGold(150);
                    IUHGuardianInventory.Instance.AddItem("Lõi Dữ Liệu Hoàng Kim Tối Thượng (+100 HP)");
                }
                rewardSummary = "⭐ BẢO VẬT TỐI THƯỢNG: [Lõi Dữ Liệu Hoàng Kim] & +150 Vàng!";
            }

            IUHCombatFloatingText.Spawn(worldPos + Vector3.up * 2f, rewardSummary, new Color(1f, 0.85f, 0.1f), 1.6f);
            onRewardMsg?.Invoke(rewardSummary);
            OnActivityBannerNotification?.Invoke("⭐ TÌM THẤY NGÔI SAO MAY MẮN!", $"Chúc mừng bạn đã khám phá Ngôi Sao May Mắn hôm nay!\n{rewardSummary}");

            OnActivitiesStateChanged?.Invoke();
        }

        private void ApplySpeedBoost(float multiplier, float duration)
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null)
            {
                var controller = student.GetComponent<ThirdPersonController>();
                if (controller != null)
                {
                    StartCoroutine(SpeedBoostRoutine(controller, multiplier, duration));
                }
            }
        }

        private System.Collections.IEnumerator SpeedBoostRoutine(ThirdPersonController controller, float multiplier, float duration)
        {
            float origMove = controller.MoveSpeed;
            float origSprint = controller.SprintSpeed;
            controller.MoveSpeed *= multiplier;
            controller.SprintSpeed *= multiplier;

            yield return new WaitForSeconds(duration);

            if (controller != null)
            {
                controller.MoveSpeed = origMove;
                controller.SprintSpeed = origSprint;
            }
        }
    }
}

