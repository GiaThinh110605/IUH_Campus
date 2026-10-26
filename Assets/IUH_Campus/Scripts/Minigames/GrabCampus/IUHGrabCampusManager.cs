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
            OrderSelection,
            PickingUp,
            Delivering,
            Victory,
            Defeat
        }

        private static IUHGrabCampusManager s_Instance;
        public static IUHGrabCampusManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = UnityEngine.Object.FindAnyObjectByType<IUHGrabCampusManager>(FindObjectsInactive.Include);
                }
                return s_Instance;
            }
            private set => s_Instance = value;
        }
        public int AttacksSuffered { get; private set; } = 0;

        [Header("Orders Configuration")]
        [SerializeField] private List<DeliveryOrder> m_Orders = new List<DeliveryOrder>();
        [SerializeField] private int m_SelectedOrderIndex = 0;

        [Header("Vehicle & Player")]
        [SerializeField] private IUHGrabMotorbike m_Motorbike;
        [SerializeField] private Transform m_PlayerTransform;

        public IUHGrabMotorbike Motorbike => m_Motorbike;
        public Transform ActiveTargetTransform
        {
            get
            {
                if (m_Motorbike != null && m_Motorbike.IsMounted) return m_Motorbike.transform;
                if (m_PlayerTransform != null && m_PlayerTransform.gameObject.activeInHierarchy) return m_PlayerTransform;
                return m_Motorbike != null ? m_Motorbike.transform : null;
            }
        }

        /// <summary>
        /// Snaps a position cleanly to the campus road mesh, ignoring ghosts, vehicles, and items.
        /// </summary>
        public static Vector3 GetGroundPosition(Vector3 desiredPos)
        {
            RaycastHit[] hits = Physics.RaycastAll(new Vector3(desiredPos.x, 25f, desiredPos.z), Vector3.down, 50f, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => b.point.y.CompareTo(a.point.y));

            foreach (var hit in hits)
            {
                string cName = hit.collider.gameObject.name.ToLower();
                if (cName.Contains("ghost") || cName.Contains("motorbike") || cName.Contains("player") ||
                    cName.Contains("rider") || cName.Contains("waypoint") || cName.Contains("stall") ||
                    cName.Contains("customer") || cName.Contains("hazard") || cName.Contains("item"))
                {
                    continue;
                }

                // Campus road/paving elevation is between 0.2m and 1.8m
                if (hit.point.y < 2.0f && hit.point.y > -1.0f)
                {
                    return new Vector3(desiredPos.x, hit.point.y + 0.12f, desiredPos.z);
                }
            }
            return new Vector3(desiredPos.x, 0.49f + 0.12f, desiredPos.z);
        }

        [Header("Waypoints")]
        [SerializeField] private IUHDeliveryWaypoint m_PickupWaypoint;
        [SerializeField] private IUHDeliveryWaypoint m_DropoffWaypoint;
        [SerializeField] private Transform m_NavArrow;

        [Header("Ghost Hazards")]
        [SerializeField] private List<GameObject> m_GhostHazards = new List<GameObject>();

        [Header("Lighting & Ambiance")]
        [SerializeField] private Light m_MainDirectionalLight;
        private Color m_OriginalLightColor = new Color(1.0f, 0.97f, 0.92f);
        private float m_OriginalLightIntensity = 1.35f;
        private Color m_SpookyNightColor = new Color(0.60f, 0.78f, 1.0f);
        private float m_SpookyLightIntensity = 0.85f;
        private struct CachedLightInfo { public Light light; public Color color; public float intensity; }
        private List<CachedLightInfo> m_CachedDirLights = new List<CachedLightInfo>();
        private CameraClearFlags m_OriginalClearFlags = CameraClearFlags.Skybox;
        private Color m_OriginalCamBgColor = new Color(0.5f, 0.7f, 0.9f);

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
        private float m_GhostSpawnTimer = 7.0f;

        // Player Career Stats (Saved)
        public int TotalEarnings { get; private set; } = 0;
        public int CompletedMissions { get; private set; } = 0;
        public float DriverRating { get; private set; } = 5.0f;

        public bool IsMissionActive => m_CurrentPhase == MissionPhase.PickingUp || m_CurrentPhase == MissionPhase.Delivering;
        public MissionPhase CurrentPhase => m_CurrentPhase;
        public int CargoIntegrity => m_CargoIntegrity;
        public DeliveryOrder CurrentOrder
        {
            get
            {
                InitializeDefaultOrdersIfEmpty();
                if (m_Orders != null && m_SelectedOrderIndex >= 0 && m_SelectedOrderIndex < m_Orders.Count)
                    return m_Orders[m_SelectedOrderIndex];
                if (m_Orders != null && m_Orders.Count > 0)
                    return m_Orders[0];
                return null;
            }
        }

        private void Awake()
        {
            if (s_Instance == null) s_Instance = this;
            else if (s_Instance != this) { Destroy(gameObject); return; }

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
            if (m_Orders != null && m_Orders.Count >= 3 && m_Orders[0] != null && m_Orders[0].id == "ORDER_01_TOCOTOCO_KTX")
            {
                foreach (var o in m_Orders) if (o != null) o.hasGhosts = true;
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
                    pickupPosition = new Vector3(360.0f, 0.49f, -38.0f),
                    dropoffName = "Ký Túc Xá KTX (Cổng 4)",
                    dropoffPosition = new Vector3(290.0f, 0.45f, -50.0f),
                    timeLimitSeconds = 240.0f,
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
                    pickupPosition = new Vector3(409.0f, 0.49f, -40.0f),
                    dropoffName = "Giảng Đường Tòa V",
                    dropoffPosition = new Vector3(310.0f, 0.49f, -70.0f),
                    timeLimitSeconds = 240.0f,
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
                    pickupPosition = new Vector3(380.0f, 0.49f, -8.0f),
                    dropoffName = "Sảnh Nghiên Cứu Tòa Nhà E",
                    dropoffPosition = new Vector3(330.0f, 0.49f, -60.0f),
                    timeLimitSeconds = 240.0f,
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
            UpdateRandomGhostEncounters();
            CheckGhostTacticalDecisions();
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

        public void ShowNotification(string message)
        {
            if (IUHGrabCampusHUD.Instance != null)
            {
                IUHGrabCampusHUD.Instance.ShowNotification(message);
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

            m_CurrentPhase = MissionPhase.OrderSelection;
            AttacksSuffered = 0;
            m_TimeRemaining = order.timeLimitSeconds;
            m_CargoIntegrity = 100;

            // Reset survival items & inventory
            IUHGrabInventoryManager.Instance?.ResetInventoryForNewGame();
            var items = FindObjectsByType<IUHGrabSurvivalItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var itm in items) itm.ResetItem();

            var ghosts = FindObjectsByType<IUHGhostHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var g in ghosts) g.ResetGhost();

            // Apply player loadout (Xanh SM, Grab, be, Sinh vien IUH)
            if (IUHCustomizationManager.Instance != null)
            {
                IUHCustomizationManager.Instance.ApplyTheme(IUHCampus.Core.PlayerLoadout.Current.theme);
            }

            // Mount vehicle automatically or teleport bike near player
            EnsurePlayerOnMotorbike();

            // Keep in-game HUD and night off until order is accepted
            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);
            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(false);
            HideWaypoints();
            SetGhostsActive(false);
            ApplyNightAmbiance(false);

            // Show Order Board (Infographic Panel 1)
            if (IUHGrabCampusHUD.Instance != null)
            {
                IUHGrabCampusHUD.Instance.ShowOrderBoard(order);
            }
            else
            {
                ConfirmOrderAccepted();
            }

            Debug.Log($"[IUH Grab Campus] Mở đơn hàng: {order.title}");
        }

        public void ConfirmOrderAccepted()
        {
            var order = CurrentOrder;
            if (order == null) return;

            // Guarantee player is mounted and motorbike is unparented and safely positioned on the road
            EnsurePlayerOnMotorbike();

            m_CurrentPhase = MissionPhase.PickingUp;
            m_TimeRemaining = order.timeLimitSeconds;
            m_CargoIntegrity = 100;

            // Spooky Night Mode
            ApplyNightAmbiance(order.hasGhosts);
            // Dynamic Spawning: All static ghosts start inactive and spawn organically along the travel path!
            SetGhostsActive(false);
            m_GhostSpawnTimer = 7.0f; // Safe zone of 7s before first random ghost appears

            // Setup Physical 3D Objectives (Stall with bubble tea cups & Student Customer NPC)
            IUHGrabDeliveryObjectives.Instance.SetupForOrder(order);

            // Spawn/Show BOTH Waypoints so delivery target is visible immediately from start!
            ShowPickupWaypoint(order.pickupPosition, $"📦 ĐIỂM LẤY ĐỒ\n[ {order.pickupName} ]");
            ShowDropoffWaypoint(order.dropoffPosition, $"🏁 MỤC TIÊU GIAO HÀNG\n[ {order.dropoffName} ]");

            // Enable HUD and close Order Board modal
            if (IUHGrabCampusHUD.Instance != null) IUHGrabCampusHUD.Instance.HideOrderBoard();
            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(true);
            if (m_TxtMissionTitle != null) m_TxtMissionTitle.text = order.title;
            if (m_TxtObjective != null) m_TxtObjective.text = $"[1/2] Lái xe đến lấy đồ tại: {order.pickupName}";

            // Clean UI: Hide 6-game menu panel and hub banners during active delivery
            if (IUHCampus.UI.IUHMiniGamePortalUI.Instance != null)
            {
                IUHCampus.UI.IUHMiniGamePortalUI.Instance.SetMenuOpen(false);
            }
            var portalPanel = GameObject.Find("PortalMenuPanel");
            if (portalPanel != null) portalPanel.SetActive(false);
            var hubBanners = GameObject.Find("HubTopBanner");
            if (hubBanners != null) hubBanners.SetActive(false);

            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);

            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(true);

            if (m_AudioSource == null) m_AudioSource = GetComponent<AudioSource>();
            if (m_AudioSource == null) m_AudioSource = gameObject.AddComponent<AudioSource>();
            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);

            ShowNotification($"🛵 ĐÃ NHẬN ĐƠN! Đến lấy hàng tại: {order.pickupName}!");
            Debug.Log($"[IUH Grab Campus] Đã nhận đơn, bắt đầu di chuyển: {order.title}");
        }

        private void EnsurePlayerOnMotorbike()
        {
            if (m_Motorbike == null)
            {
                m_Motorbike = FindFirstObjectByType<IUHGrabMotorbike>();
                if (m_Motorbike == null) return;
            }

            if (m_Motorbike.GetComponent<IUHGrabCargoVisual>() == null)
            {
                m_Motorbike.gameObject.AddComponent<IUHGrabCargoVisual>();
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

            // Safe road spawn on wide, open Eastern Boulevard (410, 0.55, -18) facing south down the avenue
            Vector3 roadSpawn = GetGroundPosition(new Vector3(410.0f, 0.55f, -18.0f));
            Quaternion roadRot = Quaternion.Euler(0f, 180f, 0f);

            if (m_PlayerTransform != null)
            {
                var pcc = m_PlayerTransform.GetComponent<CharacterController>();
                if (pcc != null) pcc.enabled = false;
                m_PlayerTransform.position = roadSpawn;
                m_PlayerTransform.rotation = roadRot;
                if (pcc != null) pcc.enabled = true;
            }

            // Keep GAMEPLAY_GRAB_CAMPUS root stationary at origin with zero rotation
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            m_Motorbike.transform.SetParent(null, true);

            var cc = m_Motorbike.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            m_Motorbike.transform.position = roadSpawn;
            m_Motorbike.transform.rotation = roadRot;
            if (cc != null) cc.enabled = true;

            // Strip any rogue colliders attached to child parts so CharacterController never gets trapped
            IUHGrabCargoVisual.StripAllColliders(m_Motorbike.transform);

            if (m_PlayerTransform != null)
            {
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
                    // Trigger physical stall collection
                    IUHGrabDeliveryObjectives.Instance.CollectCargoAtStall();
                }
                else if (waypoint.Type == IUHDeliveryWaypoint.WaypointType.Dropoff)
                {
                    if (m_TxtObjective != null)
                    {
                        m_TxtObjective.text = $"⚠️ CHƯA LẤY ĐỒ! Hãy ghé {CurrentOrder.pickupName} lấy thức ăn trước!";
                    }
                    if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 0.6f);
                    ShowNotification($"⚠️ CHƯA LẤY ĐỒ! Hãy ghé {CurrentOrder.pickupName} trước!");
                }
            }
            else if (m_CurrentPhase == MissionPhase.Delivering && waypoint.Type == IUHDeliveryWaypoint.WaypointType.Dropoff)
            {
                // Trigger physical customer handoff
                IUHGrabDeliveryObjectives.Instance.DeliverCargoToCustomer();
            }
        }

        public void OnCargoCollectedFromStall()
        {
            if (m_CurrentPhase != MissionPhase.PickingUp) return;

            m_CurrentPhase = MissionPhase.Delivering;
            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);

            if (m_PickupWaypoint != null) m_PickupWaypoint.gameObject.SetActive(false);
            ShowDropoffWaypoint(CurrentOrder.dropoffPosition, $"🏁 MỤC TIÊU GIAO HÀNG\n[ {CurrentOrder.dropoffName} ]");

            if (m_TxtObjective != null)
            {
                m_TxtObjective.text = $"[2/2] ĐÃ LẤY ĐỒ! Vượt ma giao gấp tới: {CurrentOrder.dropoffName}!";
            }

            ShowNotification($"📦 ĐÃ LẤY 2 LY TRÀ SỮA TỪ TIỆM TOCOTOCO! Giao gấp đến Bạn Sinh Viên tại {CurrentOrder.dropoffName}!");
            Debug.Log($"[IUH Grab Campus] Đã lấy đồ thành công từ {CurrentOrder.pickupName}! Đang giao tới {CurrentOrder.dropoffName}...");
        }

        public void ApplyGhostDefeatedReward(int cashBonus, int integrityHeal)
        {
            TotalEarnings += cashBonus;
            m_CargoIntegrity = Mathf.Clamp(m_CargoIntegrity + integrityHeal, 0, 100);
            IUHGrabCargoVisual.Instance?.UpdateCargoIntegrity(m_CargoIntegrity);
            if (m_ImgCargoBar != null) m_ImgCargoBar.fillAmount = m_CargoIntegrity / 100f;
            if (m_TxtCargoIntegrity != null) m_TxtCargoIntegrity.text = $"📦 Kiện hàng: {m_CargoIntegrity}%";
            Debug.Log($"[IUH Grab Campus] Đã đánh đuổi ma thành công! Thưởng trừ tà: +{cashBonus:N0} VNĐ, Kiện hàng hồi phục: {m_CargoIntegrity}%");
        }

        private float m_TacticalAlertCooldown = 0f;

        private void CheckGhostTacticalDecisions()
        {
            if (!IsMissionActive || m_Motorbike == null) return;

            if (m_TacticalAlertCooldown > 0f)
            {
                m_TacticalAlertCooldown -= Time.deltaTime;
            }

            Vector3 bikePos = m_Motorbike.transform.position;
            float bikeSpeed = m_Motorbike.CurrentSpeedKmH;

            var ghosts = FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
            IUHGhostHazard nearestChasingGhost = null;
            float nearestDist = float.MaxValue;

            foreach (var g in ghosts)
            {
                if (g == null || !g.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(bikePos, g.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    if (g.currentState == IUHGhostHazard.GhostState.Chasing)
                    {
                        nearestChasingGhost = g;
                    }
                }
            }

            // 1. Ghost approaching closely (< 24m) -> Show Tactical Choice Banner
            if (nearestChasingGhost != null && nearestDist < 24.0f && m_TacticalAlertCooldown <= 0f)
            {
                m_TacticalAlertCooldown = 4.5f;
                ShowNotification("⚠️ MA ĐANG ĐUỔI THEO! [GIỮ SHIFT]: BỨT TỐC BỎ CHẠY | [BẤM Q]: VUNG GẬY ĐÁNH ĐUỔI | [F]: BẬT ĐÈN PIN");
            }

            // 2. Tactical Choice: Fast Escape (Chạy Nhanh)
            // If the driver is driving with turbo or speed > 36 km/h and pulls away > 25m:
            if (nearestChasingGhost != null && bikeSpeed > 36.0f && nearestDist > 25.0f)
            {
                nearestChasingGhost.OnScaredByFlare(bikePos); // Causes ghost to give up and flee/return
                ShowNotification("💨 ĐÃ BỨT TỐC CẮT ĐUÔI HỒN MA THÀNH CÔNG!");
            }
        }

        public void OnCargoDeliveredToCustomer()
        {
            if (m_CurrentPhase != MissionPhase.Delivering) return;

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

        public void ApplyCargoDamage(int damage, string reason)
        {
            if (!IsMissionActive) return;

            AttacksSuffered++;
            m_CargoIntegrity = Mathf.Clamp(m_CargoIntegrity - damage, 0, 100);
            IUHGrabCargoVisual.Instance?.UpdateCargoIntegrity(m_CargoIntegrity);

            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 1.0f);

            ShowNotification($"⚠️ Va chạm ma quỷ (-{damage}%): {reason}! Kiện hàng còn: {m_CargoIntegrity}%");
            Debug.LogWarning($"[IUH Grab Campus] Kiện hàng bị tổn hại (-{damage}%): {reason}. Còn lại: {m_CargoIntegrity}%");

            if (m_CargoIntegrity <= 0)
            {
                TriggerDefeat("Kiện hàng đã bị vỡ nát hoàn toàn do ma tấn công!");
            }
        }

        public void ApplyCargoHeal(int healAmount)
        {
            if (!IsMissionActive) return;

            m_CargoIntegrity = Mathf.Clamp(m_CargoIntegrity + healAmount, 0, 100);
            IUHGrabCargoVisual.Instance?.UpdateCargoIntegrity(m_CargoIntegrity);
            ShowNotification($"🩹 Kiện hàng được gia cố: +{healAmount}%! Còn lại: {m_CargoIntegrity}%");
        }

        private void TriggerVictory()
        {
            m_CurrentPhase = MissionPhase.Victory;
            HideWaypoints();
            SetGhostsActive(false);
            ApplyNightAmbiance(false);

            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(false);
            if (IUHGrabCampusHUD.Instance != null) IUHGrabCampusHUD.Instance.HideOrderBoard();

            // Calculate Reward & Rating
            int stars = 3;
            if (m_CargoIntegrity < 60 || m_TimeRemaining < 10f || AttacksSuffered >= 2) stars = 2;
            if (m_CargoIntegrity < 30 || m_TimeRemaining < 5f || AttacksSuffered >= 4) stars = 1;

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
                    m_TxtVictoryReward.text = $"💰 <b>+{totalPay:N0} VNĐ</b>   ⭐ <b>+{CurrentOrder.expReward} EXP</b>\n📦 Thưởng nguyên vẹn ({m_CargoIntegrity}%): +{cargoBonus:N0} VNĐ\n👻 Bị ma tấn công: {AttacksSuffered} lần";
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

            if (IUHCampus.Core.IUHGameFlowManager.Instance != null)
            {
                IUHCampus.Core.IUHGameFlowManager.Instance.ShowResult(true, "🎉 NHIỆM VỤ HOÀN THÀNH!", $"Số đơn: {CompletedMissions}/3 • Tiền: +{totalPay:N0}đ • Bị tấn công: {AttacksSuffered} lần");
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
            if (IUHGrabCampusHUD.Instance != null) IUHGrabCampusHUD.Instance.HideOrderBoard();

            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDefeatSound(), 1.0f);

            if (m_DefeatDialog != null)
            {
                m_DefeatDialog.SetActive(true);
                if (m_TxtDefeatReason != null)
                {
                    m_TxtDefeatReason.text = $"Lý do: {reason}\nThời gian: {m_TimeRemaining:F0}s | Bị ma tấn công: {AttacksSuffered} lần | Kiện hàng: {m_CargoIntegrity}%";
                }
            }

            if (IUHCampus.Core.IUHGameFlowManager.Instance != null)
            {
                IUHCampus.Core.IUHGameFlowManager.Instance.ShowResult(false, "💀 NHIỆM VỤ THẤT BẠI!", $"Lý do: {reason}\nBị ma tấn công: {AttacksSuffered} lần | Kiện hàng: {m_CargoIntegrity}%");
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

            IUHGrabDeliveryObjectives.Instance?.HideObjectives();

            if (m_Motorbike != null && m_Motorbike.IsMounted)
            {
                m_Motorbike.DismountVehicle();
            }

            if (m_MissionHudPanel != null) m_MissionHudPanel.SetActive(false);
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);
            if (m_NavArrow != null) m_NavArrow.gameObject.SetActive(false);
            if (IUHGrabCampusHUD.Instance != null) IUHGrabCampusHUD.Instance.HideOrderBoard();
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
            if (m_GhostHazards != null)
            {
                foreach (var g in m_GhostHazards)
                {
                    if (g != null) g.SetActive(active);
                }
            }

            var allSceneGhosts = FindObjectsByType<IUHGhostHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var sg in allSceneGhosts)
            {
                if (sg != null) sg.gameObject.SetActive(active);
            }
        }

        private void UpdateRandomGhostEncounters()
        {
            var order = CurrentOrder;
            if (order == null || !order.hasGhosts || m_Motorbike == null) return;

            // 1. Recycle any ghosts that have fallen far behind (>35m) or player has outrun
            var allGhosts = FindObjectsByType<IUHGhostHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int activeGhostCount = 0;
            foreach (var g in allGhosts)
            {
                if (g == null) continue;
                if (g.gameObject.activeInHierarchy)
                {
                    activeGhostCount++;
                    float dist = Vector3.Distance(g.transform.position, m_Motorbike.transform.position);
                    // If motorbike speeds past ghost (>35m away), ghost fades out and returns to pool
                    if (dist > 35.0f)
                    {
                        g.gameObject.SetActive(false);
                        activeGhostCount--;
                    }
                }
            }

            // 2. Countdown spawn interval
            m_GhostSpawnTimer -= Time.deltaTime;
            if (m_GhostSpawnTimer > 0f) return;

            // Balanced encounter pacing: max 2 active ghosts at any given moment
            if (activeGhostCount >= 2)
            {
                m_GhostSpawnTimer = UnityEngine.Random.Range(4.0f, 7.0f);
                return;
            }

            // Check distance to destination: safe arrival zone within 18m of pickup/dropoff
            Vector3 currentDest = (m_CurrentPhase == MissionPhase.PickingUp) ? order.pickupPosition : order.dropoffPosition;
            if (Vector3.Distance(m_Motorbike.transform.position, currentDest) < 18.0f)
            {
                m_GhostSpawnTimer = 5.0f;
                return;
            }

            // Find an inactive ghost from the pool
            IUHGhostHazard ghostToSpawn = null;
            foreach (var g in allGhosts)
            {
                if (g != null && !g.gameObject.activeInHierarchy)
                {
                    ghostToSpawn = g;
                    break;
                }
            }

            if (ghostToSpawn == null)
            {
                m_GhostSpawnTimer = 6.0f;
                return;
            }

            // Calculate forward spawn position 22 - 32 meters ahead on the road
            float distAhead = UnityEngine.Random.Range(22.0f, 32.0f);
            float lateralOffset = UnityEngine.Random.Range(-3.2f, 3.2f);
            Vector3 spawnCandidate = m_Motorbike.transform.position + (m_Motorbike.transform.forward * distAhead) + (m_Motorbike.transform.right * lateralOffset);
            Vector3 spawnGround = GetGroundPosition(spawnCandidate);

            // Roll random ghost behavior:
            // 35% Observer (Spectral apparition by roadside that vanishes when approached)
            // 35% PathBlocker (Floating across the street, requires steering or honk/flare)
            // 30% Chaser (Emerges from darkness to pursue the bike, can be outpaced with speed)
            float roll = UnityEngine.Random.value;
            IUHGhostHazard.GhostBehaviorType beh;
            string encounterMsg;
            if (roll < 0.35f)
            {
                beh = IUHGhostHazard.GhostBehaviorType.Observer;
                encounterMsg = "👻 Một bóng trắng thoắt ẩn thoắt hiện bên lề đường!";
            }
            else if (roll < 0.70f)
            {
                beh = IUHGhostHazard.GhostBehaviorType.PathBlocker;
                encounterMsg = "⚠️ Hồn ma đang lơ lửng chắn ngang lộ trình! Hãy lách xe qua!";
            }
            else
            {
                beh = IUHGhostHazard.GhostBehaviorType.Chaser;
                encounterMsg = "⚡ MA ĐANG ĐUỔI THEO! Nhấn [Shift] vít ga phóng nhanh!";
            }

            ghostToSpawn.SpawnAt(spawnGround, beh);

            if (m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostWhooshSound(), 0.85f);
            }

            ShowNotification(encounterMsg);
            Debug.Log($"[IUH Grab Campus] Hồn ma ngẫu nhiên xuất hiện tại {spawnGround} dạng {beh}!");

            // Schedule next random spawn (between 11 and 17 seconds)
            m_GhostSpawnTimer = UnityEngine.Random.Range(11.0f, 17.0f);
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
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(0.42f, 0.52f, 0.70f);
                RenderSettings.ambientEquatorColor = new Color(0.30f, 0.38f, 0.50f);
                RenderSettings.ambientGroundColor = new Color(0.22f, 0.26f, 0.35f);
                RenderSettings.ambientIntensity = 1.15f;
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogColor = new Color(0.12f, 0.18f, 0.30f);
                RenderSettings.fogDensity = 0.0035f;
            }
            else
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(0.72f, 0.80f, 0.92f);
                RenderSettings.ambientEquatorColor = new Color(0.58f, 0.64f, 0.72f);
                RenderSettings.ambientGroundColor = new Color(0.38f, 0.42f, 0.46f);
                RenderSettings.ambientIntensity = 1.30f;
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
