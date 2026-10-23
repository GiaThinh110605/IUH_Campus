using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.UI;

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
        private Color m_SpookyNightColor = new Color(0.12f, 0.18f, 0.40f);
        private float m_SpookyLightIntensity = 0.12f;
        private struct CachedLightInfo { public Light light; public Color color; public float intensity; }
        private List<CachedLightInfo> m_CachedDirLights = new List<CachedLightInfo>();
        private CameraClearFlags m_OriginalClearFlags = CameraClearFlags.Skybox;
        private Color m_OriginalCamBgColor = Color.black;

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
        [SerializeField] private Text m_TxtVictoryClue;
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
            if (m_Orders != null && m_Orders.Count >= 3 && m_Orders[0].id == "ORDER_01_TOCOTOCO_KTX")
            {
                foreach (var o in m_Orders) o.hasGhosts = true;
                ApplyReferenceLayoutOrders();
                return;
            }

            m_Orders = new List<DeliveryOrder>
            {
                new DeliveryOrder
                {
                    id = "ORDER_01_TOCOTOCO_KTX",
                    title = "🧋 Đơn #01: Trà Sữa Trân Châu ToCoToCo ➔ KTX",
                    description = "Nhận 2 ly trà sữa trân châu tại Tiệm Trà Sữa ToCoToCo và giao gấp đến Ký Túc Xá KTX (Cổng 4) trước khi đá tan, cẩn thận hồn ma lượn lờ!",
                    pickupName = "Tiệm Trà Sữa ToCoToCo IUH",
                    pickupPosition = new Vector3(382.0f, 0.48f, -34.5f),
                    dropoffName = "Ký Túc Xá KTX (Cổng 4)",
                    dropoffPosition = new Vector3(336.0f, 0.48f, -78.0f),
                    timeLimitSeconds = 90.0f,
                    cashReward = 85000,
                    expReward = 150,
                    difficultyStars = 1,
                    hasGhosts = true
                },
                new DeliveryOrder
                {
                    id = "ORDER_02_CAVIENCHIEN_BLOCK_V",
                    title = "🍢 Đơn #02: Cá Viên Chiên Nóng Giòn ➔ Tòa V",
                    description = "Lấy phần cá viên chiên thập cẩm và xúc xích nướng bơ tại Xe Cá Viên Chiên 79 giao cho sinh viên ôn thi đêm tại Giảng Đường Tòa V!",
                    pickupName = "Xe Cá Viên Chiên 79",
                    pickupPosition = new Vector3(368.0f, 0.48f, -52.0f),
                    dropoffName = "Giảng Đường Tòa V",
                    dropoffPosition = new Vector3(360.0f, 0.48f, -90.0f),
                    timeLimitSeconds = 80.0f,
                    cashReward = 95000,
                    expReward = 180,
                    difficultyStars = 2,
                    hasGhosts = true
                },
                new DeliveryOrder
                {
                    id = "ORDER_03_COMTAM_BLOCK_E",
                    title = "🍲 Đơn #03: Cơm Tấm Sườn Bì Chả ➔ Tòa E",
                    description = "Giao suất cơm sườn nướng thơm phức từ Quán Cơm Tấm Đêm Sài Gòn đến các bạn sinh viên tại Sảnh Nghiên Cứu Tòa Nhà E!",
                    pickupName = "Quán Cơm Tấm Đêm Sài Gòn",
                    pickupPosition = new Vector3(395.0f, 0.48f, -20.0f),
                    dropoffName = "Sảnh Nghiên Cứu Tòa Nhà E",
                    dropoffPosition = new Vector3(368.0f, 0.48f, -70.0f),
                    timeLimitSeconds = 70.0f,
                    cashReward = 120000,
                    expReward = 220,
                    difficultyStars = 3,
                    hasGhosts = true
                }
            };
            ApplyReferenceLayoutOrders();
        }

        private void ApplyReferenceLayoutOrders()
        {
            if (!IUHCampus.World.IUHReferenceLayout.IsActive) return;
            var pickups = IUHCampus.World.IUHReferenceLayout.Pickups;
            var dropoffs = IUHCampus.World.IUHReferenceLayout.Dropoffs;
            for (int i = 0; i < Mathf.Min(3, m_Orders.Count); i++)
            {
                m_Orders[i].pickupPosition = pickups[i];
                m_Orders[i].dropoffPosition = dropoffs[i];
            }
        }

        private void Update()
        {
            if (m_CurrentPhase == MissionPhase.Defeat)
            {
                if (IUHCampus.Core.IUHInputHelper.IsInteractKeyDown() || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R))
                {
                    RetryCurrentMission();
                    return;
                }
            }

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
            var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (greenMgr != null && greenMgr.HasActiveSessionOrUI) greenMgr.CancelMission(false);
            InitializeDefaultOrdersIfEmpty();
            FindSceneReferences();

            if (m_Orders == null || m_Orders.Count == 0) return;
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

            // Spawn/Show BOTH Waypoints so delivery target is visible immediately from start!
            ShowPickupWaypoint(order.pickupPosition, $"📦 ĐIỂM LẤY ĐỒ\n[ {order.pickupName} ]");
            ShowDropoffWaypoint(order.dropoffPosition, $"🏁 MỤC TIÊU GIAO HÀNG\n[ {order.dropoffName} ]");

            // Enable HUD
            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(true);
            if (m_TxtMissionTitle != null) m_TxtMissionTitle.text = order.title;
            if (m_TxtObjective != null) m_TxtObjective.text = $"[1/2] Lái xe đến lấy đồ tại: {order.pickupName}";

            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);

            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(true);

            if (m_AudioSource == null) m_AudioSource = GetComponent<AudioSource>();
            if (m_AudioSource == null) m_AudioSource = gameObject.AddComponent<AudioSource>();
            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);
            Debug.Log($"[IUH Grab Campus] Bắt đầu nhiệm vụ: {order.title}");
        }

        private void EnsurePlayerOnMotorbike()
        {
            if (m_Motorbike == null)
            {
                m_Motorbike = FindFirstObjectByType<IUHGrabMotorbike>();
                if (m_Motorbike == null) return;
            }

            if (m_PlayerTransform == null)
            {
                var p = GameObject.Find("PlayerArmature");
                if (p == null)
                {
                    var all = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var go in all)
                    {
                        if (go.name == "PlayerArmature" && !go.hideFlags.HasFlag(HideFlags.NotEditable))
                        {
                            p = go;
                            break;
                        }
                    }
                }
                if (p != null) m_PlayerTransform = p.transform;
            }

            if (m_PlayerTransform != null)
            {
                // Ensure player and motorbike start on the wide asphalt campus boulevard (Gate 1 road)
                Vector3 roadSpawn = IUHCampus.World.IUHReferenceLayout.IsActive
                    ? IUHCampus.World.IUHReferenceLayout.Entrance : new Vector3(398.0f, 0.55f, -18.0f);
                Quaternion roadRot = Quaternion.Euler(0f, 180f, 0f);

                var pcc = m_PlayerTransform.GetComponent<CharacterController>();
                if (pcc != null) pcc.enabled = false;
                m_PlayerTransform.position = roadSpawn;
                m_PlayerTransform.rotation = roadRot;
                if (pcc != null) pcc.enabled = true;

                var cc = m_Motorbike.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                m_Motorbike.transform.position = roadSpawn;
                m_Motorbike.transform.rotation = roadRot;
                if (cc != null) cc.enabled = true;

                m_Motorbike.MountVehicle(m_PlayerTransform.gameObject);
            }
        }

        public void OnWaypointReached(IUHDeliveryWaypoint waypoint)
        {
            if (!IsMissionActive) return;

            if (m_CurrentPhase == MissionPhase.PickingUp)
            {
                if (waypoint.Type == IUHDeliveryWaypoint.WaypointType.Pickup)
                {
                    // Picked up successfully!
                    m_CurrentPhase = MissionPhase.Delivering;
                    m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);

                    if (m_PickupWaypoint != null) m_PickupWaypoint.gameObject.SetActive(false);

                    if (m_TxtObjective != null)
                    {
                        m_TxtObjective.text = $"[2/2] ĐÃ LẤY ĐỒ! Vượt ma giao gấp tới: {CurrentOrder.dropoffName}!";
                    }

                    Debug.Log($"[IUH Grab Campus] Đã lấy đồ thành công từ {CurrentOrder.pickupName}! Đang giao tới {CurrentOrder.dropoffName}...");
                }
                else if (waypoint.Type == IUHDeliveryWaypoint.WaypointType.Dropoff)
                {
                    // Driver went to destination before picking up!
                    if (m_TxtObjective != null)
                    {
                        m_TxtObjective.text = $"⚠️ CHƯA LẤY ĐỒ! Hãy ghé {CurrentOrder.pickupName} lấy thức ăn trước!";
                    }
                    if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 0.6f);
                }
            }
            else if (m_CurrentPhase == MissionPhase.Delivering && waypoint.Type == IUHDeliveryWaypoint.WaypointType.Dropoff)
            {
                // Completed delivery route! Open Hand-off Dialog (Image 1 panel 3)
                var dialog = IUHDeliveryDialogueUI.Instance;
                if (dialog != null)
                {
                    dialog.ShowDeliveryDialog(
                        CurrentOrder.dropoffName,
                        $"Bạn có đơn hàng từ {CurrentOrder.pickupName} giao cho bạn sinh viên tại {CurrentOrder.dropoffName}. Cảm ơn bạn đã vượt qua màn đêm và hồn ma để giao đồ an toàn!",
                        CurrentOrder.cashReward,
                        CurrentOrder.expReward,
                        () => TriggerVictory()
                    );
                }
                else
                {
                    TriggerVictory();
                }
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
                    m_TxtVictoryReward.text = $"💰 +{totalPay:N0} VNĐ   ⭐ +{CurrentOrder.expReward} EXP\n📦 Thưởng nguyên vẹn ({m_CargoIntegrity}%): +{cargoBonus:N0} VNĐ";
                }
                if (m_TxtVictoryRating != null)
                {
                    m_TxtVictoryRating.text = new string('⭐', stars) + $" ({stars}/3 Sao)";
                }
                if (m_TxtVictoryClue != null)
                {
                    m_TxtVictoryClue.text = $"📜 Manh mối: [Mảnh nhật ký #{m_SelectedOrderIndex + 1} tại Ký Túc Xá Block D]";
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

            if (m_Motorbike != null && m_Motorbike.IsMounted)
            {
                m_Motorbike.DismountVehicle();
            }

            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);
            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(false);
        }

        private void ShowPickupWaypoint(Vector3 pos, string label = null)
        {
            if (m_PickupWaypoint != null)
            {
                m_PickupWaypoint.transform.position = pos;
                m_PickupWaypoint.Setup(IUHDeliveryWaypoint.WaypointType.Pickup, new Color(0f, 0.9f, 1f, 0.9f), label);
                m_PickupWaypoint.gameObject.SetActive(true);
            }
        }

        private void ShowDropoffWaypoint(Vector3 pos, string label = null)
        {
            if (m_DropoffWaypoint != null)
            {
                m_DropoffWaypoint.transform.position = pos;
                m_DropoffWaypoint.Setup(IUHDeliveryWaypoint.WaypointType.Dropoff, new Color(0.1f, 1f, 0.4f, 0.95f), label);
                m_DropoffWaypoint.gameObject.SetActive(true);
            }
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
            // 1. Cache all directional lights once
            if (m_CachedDirLights.Count == 0)
            {
                var dirLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (var l in dirLights)
                {
                    if (l != null && l.type == LightType.Directional)
                    {
                        m_CachedDirLights.Add(new CachedLightInfo { light = l, color = l.color, intensity = l.intensity });
                    }
                }
            }

            // 2. Set all directional lights
            foreach (var item in m_CachedDirLights)
            {
                if (item.light != null)
                {
                    if (night)
                    {
                        item.light.color = m_SpookyNightColor;
                        item.light.intensity = m_SpookyLightIntensity;
                    }
                    else
                    {
                        item.light.color = item.color;
                        item.light.intensity = item.intensity;
                    }
                }
            }

            // 3. Spooky Midnight Camera & Fog
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                if (night)
                {
                    m_OriginalClearFlags = mainCam.clearFlags;
                    m_OriginalCamBgColor = mainCam.backgroundColor;
                    mainCam.clearFlags = CameraClearFlags.SolidColor;
                    mainCam.backgroundColor = new Color(0.015f, 0.025f, 0.06f);
                }
                else
                {
                    mainCam.clearFlags = m_OriginalClearFlags;
                    mainCam.backgroundColor = m_OriginalCamBgColor;
                }
            }

            if (night)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.04f, 0.06f, 0.14f);
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = new Color(0.02f, 0.035f, 0.09f);
                RenderSettings.fogDensity = 0.022f;
            }
            else
            {
                RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.45f);
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
