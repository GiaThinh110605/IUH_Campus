using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Master Manager for Mini-Game 2.1 "Grab Campus: Chuyến xe tâm linh".
    /// Orchestrates delivery orders, night spooky ambiance, waypoint beacons,
    /// dynamic HUD, ghost hazards, scoring, and persistence.
    /// </summary>
    public class IUHGrabCampusManager : MonoBehaviour
    {
        [System.Serializable]
        public class DeliveryOrder
        {
            public string id;
            public string title;
            public string description;
            public string pickupName;
            public Vector3 pickupPosition;
            public string dropoffName;
            public Vector3 dropoffPosition;
            public float timeLimitSeconds;
            public int cashReward;
            public int expReward;
            public int difficultyStars; // 1 to 3
            public bool hasGhosts;
        }

        public enum MissionPhase
        {
            Inactive,
            PickingUp,
            Delivering,
            Victory,
            Defeat
        }

        public static IUHGrabCampusManager Instance { get; private set; }

        [Header("Orders Configuration")]
        [SerializeField] private List<DeliveryOrder> m_Orders = new List<DeliveryOrder>();
        [SerializeField] private int m_SelectedOrderIndex = 0;

        [Header("Vehicle & Player")]
        [SerializeField] private IUHGrabMotorbike m_Motorbike;
        [SerializeField] private Transform m_PlayerTransform;

        [Header("Waypoints")]
        [SerializeField] private IUHDeliveryWaypoint m_PickupWaypoint;
        [SerializeField] private IUHDeliveryWaypoint m_DropoffWaypoint;
        [SerializeField] private Transform m_NavArrow;

        [Header("Ghost Hazards")]
        [SerializeField] private List<GameObject> m_GhostHazards = new List<GameObject>();

        [Header("Lighting & Ambiance")]
        [SerializeField] private Light m_MainDirectionalLight;
        private Color m_OriginalLightColor = Color.white;
        private float m_OriginalLightIntensity = 1.0f;
        private Color m_SpookyNightColor = new Color(0.2f, 0.35f, 0.55f);
        private float m_SpookyLightIntensity = 0.35f;

        [Header("HUD Elements")]
        [SerializeField] private GameObject m_MissionHudPanel;
        [SerializeField] private Text m_TxtMissionTitle;
        [SerializeField] private Text m_TxtObjective;
        [SerializeField] private Text m_TxtTimer;
        [SerializeField] private Text m_TxtCargoIntegrity;
        [SerializeField] private Image m_ImgCargoBar;
        [SerializeField] private Text m_TxtDistance;
        [SerializeField] private Text m_TxtSpeed;

        [Header("Result Dialogs")]
        [SerializeField] private GameObject m_VictoryDialog;
        [SerializeField] private Text m_TxtVictoryReward;
        [SerializeField] private Text m_TxtVictoryRating;
        [SerializeField] private GameObject m_DefeatDialog;
        [SerializeField] private Text m_TxtDefeatReason;

        // Runtime Mission State
        private MissionPhase m_CurrentPhase = MissionPhase.Inactive;
        private float m_TimeRemaining = 0f;
        private int m_CargoIntegrity = 100;
        private AudioSource m_AudioSource;

        // Player Career Stats (Saved)
        public int TotalEarnings { get; private set; } = 0;
        public int CompletedMissions { get; private set; } = 0;
        public float DriverRating { get; private set; } = 5.0f;

        public bool IsMissionActive => m_CurrentPhase == MissionPhase.PickingUp || m_CurrentPhase == MissionPhase.Delivering;
        public MissionPhase CurrentPhase => m_CurrentPhase;
        public DeliveryOrder CurrentOrder => (m_Orders != null && m_SelectedOrderIndex >= 0 && m_SelectedOrderIndex < m_Orders.Count) ? m_Orders[m_SelectedOrderIndex] : null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            m_AudioSource = gameObject.AddComponent<AudioSource>();
            LoadCareerStats();
            InitializeDefaultOrdersIfEmpty();
            FindSceneReferences();
        }

        private void Start()
        {
            HideWaypoints();
            SetGhostsActive(false);

            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);
        }

        private void FindSceneReferences()
        {
            if (m_MainDirectionalLight == null)
            {
                var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (var l in lights)
                {
                    if (l.type == LightType.Directional)
                    {
                        m_MainDirectionalLight = l;
                        m_OriginalLightColor = l.color;
                        m_OriginalLightIntensity = l.intensity;
                        break;
                    }
                }
            }

            if (m_Motorbike == null)
            {
                m_Motorbike = FindFirstObjectByType<IUHGrabMotorbike>();
            }

            if (m_PlayerTransform == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) m_PlayerTransform = p.transform;
            }
        }

        private void InitializeDefaultOrdersIfEmpty()
        {
            if (m_Orders != null && m_Orders.Count > 0) return;

            m_Orders = new List<DeliveryOrder>
            {
                new DeliveryOrder
                {
                    id = "ORDER_01_CANTEEN_KTX",
                    title = "🛵 Đơn #01: Cơm Sườn Đêm Căn Tin",
                    description = "Nhận hộp cơm nóng tại Căn Tin Sân Trường và giao gấp tới Ký Túc Xá Cổng 4.",
                    pickupName = "Căn Tin Trung Tâm",
                    pickupPosition = new Vector3(375.0f, 0.5f, -50.0f),
                    dropoffName = "Ký Túc Xá Cổng 4 (Block X)",
                    dropoffPosition = new Vector3(330.0f, 0.5f, -78.0f),
                    timeLimitSeconds = 75.0f,
                    cashReward = 60000,
                    expReward = 100,
                    difficultyStars = 1,
                    hasGhosts = false
                },
                new DeliveryOrder
                {
                    id = "ORDER_02_LIBRARY_BLOCK_V",
                    title = "👻 Đơn #02: Giáo Trình Cổ Giảng Đường",
                    description = "Vận chuyển tập giáo trình quý từ Thư Viện Tòa A tới Phòng Nghiên Cứu Tòa V. Cẩn thận bóng ma lượn lờ!",
                    pickupName = "Thư Viện Tòa A",
                    pickupPosition = new Vector3(410.0f, 0.5f, -28.0f),
                    dropoffName = "Giảng Đường Tòa V",
                    dropoffPosition = new Vector3(367.0f, 0.5f, -95.0f),
                    timeLimitSeconds = 60.0f,
                    cashReward = 85000,
                    expReward = 150,
                    difficultyStars = 2,
                    hasGhosts = true
                },
                new DeliveryOrder
                {
                    id = "ORDER_03_GATE1_BLOCK_E",
                    title = "⚡ Đơn #03: Linh Kiện Khẩn Cấp Bão Đêm",
                    description = "Tiếp nhận thùng linh kiện tại Cổng 1 Nguyễn Văn Bảo, vượt màn đêm sương mù và né hồn ma về Tòa Nhà E.",
                    pickupName = "Cổng 1 Nguyễn Văn Bảo",
                    pickupPosition = new Vector3(403.8f, 0.5f, -15.0f),
                    dropoffName = "Sảnh Tòa Nhà E",
                    dropoffPosition = new Vector3(368.0f, 0.5f, -70.0f),
                    timeLimitSeconds = 50.0f,
                    cashReward = 120000,
                    expReward = 200,
                    difficultyStars = 3,
                    hasGhosts = true
                }
            };
        }

        private void Update()
        {
            if (!IsMissionActive) return;

            // Timer countdown
            m_TimeRemaining -= Time.deltaTime;
            if (m_TimeRemaining <= 0f)
            {
                m_TimeRemaining = 0f;
                TriggerDefeat("Hết thời gian giao hàng!");
                return;
            }

            UpdateHud();
            UpdateNavArrow();
        }

        private void UpdateHud()
        {
            if (m_MissionHudPanel == null) return;

            // Format Timer
            int minutes = Mathf.FloorToInt(m_TimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(m_TimeRemaining % 60f);
            if (m_TxtTimer != null)
            {
                m_TxtTimer.text = $"⏱ {minutes:00}:{seconds:00}";
                // Flash red when under 15 seconds
                if (m_TimeRemaining < 15f)
                {
                    m_TxtTimer.color = (Mathf.Sin(Time.time * 8f) > 0f) ? Color.red : Color.yellow;
                }
                else
                {
                    m_TxtTimer.color = Color.white;
                }
            }

            // Cargo Integrity
            if (m_TxtCargoIntegrity != null)
            {
                m_TxtCargoIntegrity.text = $"📦 Kiện hàng: {m_CargoIntegrity}%";
                if (m_CargoIntegrity > 70) m_TxtCargoIntegrity.color = Color.green;
                else if (m_CargoIntegrity > 40) m_TxtCargoIntegrity.color = Color.yellow;
                else m_TxtCargoIntegrity.color = Color.red;
            }

            if (m_ImgCargoBar != null)
            {
                m_ImgCargoBar.fillAmount = m_CargoIntegrity / 100.0f;
            }

            // Speed
            if (m_TxtSpeed != null && m_Motorbike != null)
            {
                m_TxtSpeed.text = $"{m_Motorbike.CurrentSpeedKmH:F0} km/h";
            }

            // Distance to active target
            Vector3 targetPos = (m_CurrentPhase == MissionPhase.PickingUp) ? CurrentOrder.pickupPosition : CurrentOrder.dropoffPosition;
            Vector3 sourcePos = (m_Motorbike != null && m_Motorbike.IsMounted) ? m_Motorbike.transform.position : (m_PlayerTransform != null ? m_PlayerTransform.position : Vector3.zero);
            float dist = Vector3.Distance(new Vector3(sourcePos.x, 0f, sourcePos.z), new Vector3(targetPos.x, 0f, targetPos.z));

            if (m_TxtDistance != null)
            {
                string targetLabel = (m_CurrentPhase == MissionPhase.PickingUp) ? CurrentOrder.pickupName : CurrentOrder.dropoffName;
                m_TxtDistance.text = $"📍 Đến {targetLabel}: {dist:F0}m";
            }
        }

        private void UpdateNavArrow()
        {
            if (m_NavArrow == null) return;

            Transform currentVehicle = (m_Motorbike != null && m_Motorbike.IsMounted) ? m_Motorbike.transform : m_PlayerTransform;
            if (currentVehicle == null) return;

            Vector3 targetPos = (m_CurrentPhase == MissionPhase.PickingUp) ? CurrentOrder.pickupPosition : CurrentOrder.dropoffPosition;
            Vector3 dir = (targetPos - currentVehicle.position);
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
            {
                m_NavArrow.position = currentVehicle.position + Vector3.up * 2.8f;
                m_NavArrow.rotation = Quaternion.LookRotation(dir);
            }
        }

        public void StartMission(int orderIndex)
        {
            if (orderIndex < 0 || orderIndex >= m_Orders.Count) orderIndex = 0;
            m_SelectedOrderIndex = orderIndex;
            var order = CurrentOrder;

            m_CurrentPhase = MissionPhase.PickingUp;
            m_TimeRemaining = order.timeLimitSeconds;
            m_CargoIntegrity = 100;

            // Spooky Night Mode
            ApplyNightAmbiance(order.hasGhosts);
            SetGhostsActive(order.hasGhosts);

            // Mount vehicle automatically or teleport bike near player
            EnsurePlayerOnMotorbike();

            // Spawn/Show Waypoints
            ShowPickupWaypoint(order.pickupPosition);

            // Enable HUD
            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(true);
            if (m_TxtMissionTitle != null) m_TxtMissionTitle.text = order.title;
            if (m_TxtObjective != null) m_TxtObjective.text = $"[1/2] Lái xe đến điểm nhận: {order.pickupName}";

            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);

            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(true);

            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);
            Debug.Log($"[IUH Grab Campus] Bắt đầu nhiệm vụ: {order.title}");
        }

        private void EnsurePlayerOnMotorbike()
        {
            if (m_Motorbike == null) return;

            if (!m_Motorbike.IsMounted)
            {
                // Teleport motorbike near player
                if (m_PlayerTransform != null)
                {
                    m_Motorbike.transform.position = m_PlayerTransform.position + m_PlayerTransform.forward * 1.5f + Vector3.up * 0.2f;
                    m_Motorbike.transform.rotation = m_PlayerTransform.rotation;
                }
                m_Motorbike.MountVehicle(m_PlayerTransform != null ? m_PlayerTransform.gameObject : null);
            }
        }

        public void OnWaypointReached(IUHDeliveryWaypoint waypoint)
        {
            if (!IsMissionActive) return;

            if (m_CurrentPhase == MissionPhase.PickingUp && waypoint.Type == IUHDeliveryWaypoint.WaypointType.Pickup)
            {
                // Picked up successfully!
                m_CurrentPhase = MissionPhase.Delivering;
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);

                if (m_PickupWaypoint != null) m_PickupWaypoint.gameObject.SetActive(false);
                ShowDropoffWaypoint(CurrentOrder.dropoffPosition);

                if (m_TxtObjective != null)
                {
                    m_TxtObjective.text = $"[2/2] Vượt chướng ngại vật giao tới: {CurrentOrder.dropoffName}!";
                }

                Debug.Log($"[IUH Grab Campus] Đã nhận hàng thành công! Đang giao tới {CurrentOrder.dropoffName}...");
            }
            else if (m_CurrentPhase == MissionPhase.Delivering && waypoint.Type == IUHDeliveryWaypoint.WaypointType.Dropoff)
            {
                // Completed delivery!
                TriggerVictory();
            }
        }

        public void ApplyCargoDamage(int damage, string reason)
        {
            if (!IsMissionActive) return;

            m_CargoIntegrity = Mathf.Clamp(m_CargoIntegrity - damage, 0, 100);
            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 1.0f);

            Debug.LogWarning($"[IUH Grab Campus] Kiện hàng bị tổn hại (-{damage}%): {reason}. Còn lại: {m_CargoIntegrity}%");

            if (m_CargoIntegrity <= 0)
            {
                TriggerDefeat("Kiện hàng đã bị vỡ nát hoàn toàn!");
            }
        }

        private void TriggerVictory()
        {
            m_CurrentPhase = MissionPhase.Victory;
            HideWaypoints();
            SetGhostsActive(false);
            ApplyNightAmbiance(false);

            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(false);

            // Calculate Reward & Rating
            int stars = 3;
            if (m_CargoIntegrity < 60 || m_TimeRemaining < 10f) stars = 2;
            if (m_CargoIntegrity < 30 || m_TimeRemaining < 5f) stars = 1;

            int cargoBonus = Mathf.RoundToInt(CurrentOrder.cashReward * (m_CargoIntegrity / 100.0f) * 0.2f);
            int totalPay = CurrentOrder.cashReward + cargoBonus;

            TotalEarnings += totalPay;
            CompletedMissions++;
            DriverRating = Mathf.Clamp(DriverRating + 0.1f, 1.0f, 5.0f);
            SaveCareerStats();

            // Play Fanfare
            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetVictoryFanfare(), 1.0f);
            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetCoinClinkSound(), 0.9f);

            // Show Victory Dialog
            if (m_VictoryDialog != null)
            {
                m_VictoryDialog.SetActive(true);
                if (m_TxtVictoryReward != null)
                {
                    m_TxtVictoryReward.text = $"+{totalPay:N0} VNĐ  (+{CurrentOrder.expReward} EXP)\nThưởng nguyên vẹn ({m_CargoIntegrity}%): +{cargoBonus:N0} VNĐ";
                }
                if (m_TxtVictoryRating != null)
                {
                    m_TxtVictoryRating.text = new string('⭐', stars) + $" ({stars}/3 Sao)";
                }
            }

            Debug.Log($"[IUH Grab Campus] THẮNG LỢI! Giao hàng hoàn tất: +{totalPay:N0} VNĐ, {stars} sao.");
        }

        private void TriggerDefeat(string reason)
        {
            m_CurrentPhase = MissionPhase.Defeat;
            HideWaypoints();
            SetGhostsActive(false);
            ApplyNightAmbiance(false);

            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(false);

            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDefeatSound(), 1.0f);

            if (m_DefeatDialog != null)
            {
                m_DefeatDialog.SetActive(true);
                if (m_TxtDefeatReason != null)
                {
                    m_TxtDefeatReason.text = $"Lý do: {reason}\nThời gian: {m_TimeRemaining:F0}s | Kiện hàng: {m_CargoIntegrity}%";
                }
            }

            Debug.LogWarning($"[IUH Grab Campus] THẤT BẠI: {reason}");
        }

        public void RetryCurrentMission()
        {
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            StartMission(m_SelectedOrderIndex);
        }

        public void CancelMission()
        {
            m_CurrentPhase = MissionPhase.Inactive;
            HideWaypoints();
            SetGhostsActive(false);
            ApplyNightAmbiance(false);

            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);
            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(false);
        }

        private void ShowPickupWaypoint(Vector3 pos)
        {
            if (m_PickupWaypoint != null)
            {
                m_PickupWaypoint.transform.position = pos;
                m_PickupWaypoint.Setup(IUHDeliveryWaypoint.WaypointType.Pickup, new Color(0f, 0.9f, 1f, 0.9f));
                m_PickupWaypoint.gameObject.SetActive(true);
            }
            if (m_DropoffWaypoint != null) m_DropoffWaypoint.gameObject.SetActive(false);
        }

        private void ShowDropoffWaypoint(Vector3 pos)
        {
            if (m_DropoffWaypoint != null)
            {
                m_DropoffWaypoint.transform.position = pos;
                m_DropoffWaypoint.Setup(IUHDeliveryWaypoint.WaypointType.Dropoff, new Color(0f, 1f, 0.35f, 0.9f));
                m_DropoffWaypoint.gameObject.SetActive(true);
            }
            if (m_PickupWaypoint != null) m_PickupWaypoint.gameObject.SetActive(false);
        }

        private void HideWaypoints()
        {
            if (m_PickupWaypoint != null) m_PickupWaypoint.gameObject.SetActive(false);
            if (m_DropoffWaypoint != null) m_DropoffWaypoint.gameObject.SetActive(false);
        }

        private void SetGhostsActive(bool active)
        {
            if (m_GhostHazards == null) return;
            foreach (var g in m_GhostHazards)
            {
                if (g != null) g.SetActive(active);
            }
        }

        private void ApplyNightAmbiance(bool night)
        {
            if (m_MainDirectionalLight == null) return;

            if (night)
            {
                m_MainDirectionalLight.color = m_SpookyNightColor;
                m_MainDirectionalLight.intensity = m_SpookyLightIntensity;
                RenderSettings.fog = true;
                RenderSettings.fogColor = new Color(0.12f, 0.18f, 0.3f);
                RenderSettings.fogDensity = 0.025f;
            }
            else
            {
                m_MainDirectionalLight.color = m_OriginalLightColor;
                m_MainDirectionalLight.intensity = m_OriginalLightIntensity;
                RenderSettings.fog = false;
            }
        }

        private void SaveCareerStats()
        {
            PlayerPrefs.SetInt("IUH_Grab_Earnings", TotalEarnings);
            PlayerPrefs.SetInt("IUH_Grab_Missions", CompletedMissions);
            PlayerPrefs.SetFloat("IUH_Grab_Rating", DriverRating);
            PlayerPrefs.Save();
        }

        private void LoadCareerStats()
        {
            TotalEarnings = PlayerPrefs.GetInt("IUH_Grab_Earnings", 125000);
            CompletedMissions = PlayerPrefs.GetInt("IUH_Grab_Missions", 2);
            DriverRating = PlayerPrefs.GetFloat("IUH_Grab_Rating", 4.9f);
        }
    }
}
