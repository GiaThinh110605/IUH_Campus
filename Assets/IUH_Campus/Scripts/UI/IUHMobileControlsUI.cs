using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IUHCampus.Core;
using IUHCampus.Combat;
using IUHCampus.Guardian;
using IUHCampus.DailyActivities;
using StarterAssets;

namespace IUHCampus.UI
{
    /// <summary>
    /// Điều khiển toàn diện giao diện HUD phong cách Colorful Gaming UI – Modern Mobile Adventure Game:
    /// - Canvas Scaler chuẩn màn hình ngang 1920x1080 (Match Height/Width = 0.5).
    /// - HUD_Top:
    ///   + HUD_PlayerStatus: Avatar bo tròn, Level Badge "Lv.1", Health Bar xanh neon "100 / 100".
    ///   + HUD_TopRight: Cụm nút icon bo tròn viền cyan phát sáng (Kho đồ, Nâng cấp, Cài đặt).
    /// - HUD_Movement (Góc trái dưới):
    ///   + VirtualJoystick bo tròn viền cyan, 4 mũi tên điều hướng, inner handle cyan phát sáng.
    /// - HUD_Actions (Góc phải dưới):
    ///   + Interact_Button (Tương tác [E] - Cam Neon #FF9F24, to nhất 140px, icon bàn tay chỉ).
    ///   + Run_Button (Chạy - Primary Blue #16B9F5, icon người chạy).
    ///   + Jump_Button (Nhảy - Green #39E879, icon người nhảy).
    ///   + Firework_Button (Pháo hoa - Purple #A855F7, icon pháo hoa).
    ///   + Vehicle_Button (Xe / Bay - Cyan #25E5FF, icon phi cơ / xe).
    /// - TouchLookZone: Vùng vuốt xoay camera 360 độ mượt mà, Inverted Y ngửa mặt lên bầu trời.
    /// - Bảo toàn 100% logic di chuyển, combat, minigames và tương tác.
    /// </summary>
    public class IUHMobileControlsUI : MonoBehaviour
    {
        public static IUHMobileControlsUI Instance { get; private set; }
        public static bool IsTouchLookActive { get; private set; }

        [Header("Orientation Config")]
        [SerializeField] private bool m_ForceLandscape = true;

        [Header("Movement Joystick")]
        [SerializeField] private RectTransform m_JoystickBase;
        [SerializeField] private RectTransform m_JoystickHandle;
        [SerializeField] private float m_JoystickRange = 75f;

        [Header("Touch Look Zone")]
        [SerializeField] private RectTransform m_TouchLookZone;
        [SerializeField] private float m_LookSensitivity = 0.35f;

        [Header("Action Buttons (Bottom Right)")]
        [SerializeField] private Button m_BtnInteract;
        [SerializeField] private Button m_BtnSprint;
        [SerializeField] private Button m_BtnJump;
        [SerializeField] private Button m_BtnFireworks;
        [SerializeField] private Button m_BtnVehicle;

        [Header("Optional Combat Buttons")]
        [SerializeField] private Button m_BtnFire;
        [SerializeField] private Button m_BtnAim;
        [SerializeField] private Button m_BtnMissile;

        [Header("Top Right Menu Buttons")]
        [SerializeField] private Button m_BtnInventory;
        [SerializeField] private Button m_BtnUpgrade;
        [SerializeField] private Button m_BtnSettings;
        private Button m_BtnAR;

        [Header("Utility Key Buttons [H] & [B]")]
        [SerializeField] private Button m_BtnKeyH;
        [SerializeField] private Button m_BtnKeyB;

        [Header("Drone Mobile Flight Controls")]
        private Button m_BtnDroneLaunch;
        private GameObject m_DroneControlsRoot;
        private Button m_BtnDroneAscend;
        private Button m_BtnDroneDescend;
        private Button m_BtnDroneSupply;
        private Button m_BtnDroneExit;
        private bool m_LastDroneActiveState = false;

        [Header("Player Status Widgets (Top Left)")]
        [SerializeField] private Image m_HealthBarFill;
        [SerializeField] private Text m_HealthText;
        [SerializeField] private Text m_LevelText;
        [SerializeField] private Image m_AvatarImage;

        [Header("Platform Visibility")]
        [SerializeField] private bool m_ForceShowOnPC = false;

        private Vector2 m_InputVector = Vector2.zero;
        private bool m_IsDraggingJoystick = false;
        private int m_JoystickPointerId = -999;
        private float m_LastJoystickTapTime = -1f;
        private bool m_IsSprinting = false;
        private StarterAssetsInputs m_StarterInputs;
        private ThirdPersonController m_ThirdPersonController;
        private float m_NextPlayerSearch;

        // Touch Look internal state
        private int m_LookPointerId = -999;
        private Vector2 m_PreviousLookTouchPos = Vector2.zero;
        private Vector2 m_AccumulatedLookDelta = Vector2.zero;
        private bool m_IsFiringHeld = false;
        private float m_NextContinuousFireTime = 0f;
        private Text m_InteractBtnText;

        public void Setup(RectTransform joystickBase, RectTransform joystickHandle, Button btnVehicle, Button btnInteract, Button btnAction)
        {
            m_JoystickBase = joystickBase;
            m_JoystickHandle = joystickHandle;
            m_BtnVehicle = btnVehicle;
            m_BtnInteract = btnInteract;
            if (m_BtnFire == null && btnAction != null) m_BtnFire = btnAction;
            SetupButtonListeners();
            SetupJoystickEvents();
        }

        private void Awake()
        {
            Instance = this;

            if (m_ForceLandscape)
            {
                ApplyLandscapeOrientation();
            }

            AutoWireReferences();
            CheckPlatformVisibility();
        }

        private void Start()
        {
            if (m_ForceLandscape)
            {
                ApplyLandscapeOrientation();
            }

            AutoWireReferences();
            CheckPlatformVisibility();
            SetupButtonListeners();
            SetupJoystickEvents();
            SetupTouchLookEvents();
        }

        public void ApplyLandscapeOrientation()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        public void AutoWireReferences()
        {
            WireJoystickAndLookZone();
            WireActionButtons();
            WireDroneFlightButtons();
            WireTopRightButtons();
            CleanupLegacyUtilityButtons();
            WirePlayerStatusWidgets();
        }

        private Button FindButton(params string[] paths)
        {
            foreach (var path in paths)
            {
                var btn = transform.Find(path)?.GetComponent<Button>();
                if (btn != null) return btn;
            }
            return null;
        }

        private void WireJoystickAndLookZone()
        {
            if (m_JoystickBase == null)
            {
                m_JoystickBase = transform.Find("HUD_Movement/VirtualJoystick/JoystickBase")?.GetComponent<RectTransform>()
                    ?? transform.Find("JoystickBase")?.GetComponent<RectTransform>();
            }
            if (m_JoystickHandle == null && m_JoystickBase != null)
            {
                m_JoystickHandle = m_JoystickBase.Find("JoystickHandle")?.GetComponent<RectTransform>();
            }

            if (m_TouchLookZone == null)
            {
                m_TouchLookZone = transform.Find("TouchLookZone")?.GetComponent<RectTransform>();
            }
            if (m_TouchLookZone != null)
            {
                // Chừa 120px phía trên góc phải để không nuốt touch của cụm nút HUD_TopRight (Camera AR, Kho đồ, Nâng cấp, Cài đặt)
                m_TouchLookZone.offsetMax = new Vector2(0f, -120f);
            }
        }

        private void WireActionButtons()
        {
            if (m_BtnFire == null) m_BtnFire = FindButton("HUD_Actions/Fire_Button", "HUD_Actions/BtnFire", "BtnFire");
            if (m_BtnInteract == null) m_BtnInteract = FindButton("HUD_Actions/Interact_Button", "BtnInteract");
            if (m_BtnInteract != null) m_InteractBtnText = m_BtnInteract.GetComponentInChildren<Text>();
            if (m_BtnSprint == null) m_BtnSprint = FindButton("HUD_Actions/Run_Button", "BtnSprint");
            if (m_BtnJump == null) m_BtnJump = FindButton("HUD_Actions/Jump_Button", "BtnJump");
            if (m_BtnFireworks == null) m_BtnFireworks = FindButton("HUD_Actions/Firework_Button", "BtnFireworks");
            if (m_BtnVehicle == null) m_BtnVehicle = FindButton("HUD_Actions/Vehicle_Button", "BtnVehicle");
            if (m_BtnAim == null) m_BtnAim = FindButton("HUD_Actions/BtnAim", "BtnAim");
            if (m_BtnMissile == null) m_BtnMissile = FindButton("HUD_Actions/BtnMissile", "BtnMissile");
        }

        private void WireDroneFlightButtons()
        {
            if (m_BtnDroneLaunch == null)
            {
                m_BtnDroneLaunch = FindButton("HUD_Actions/Drone_Button") ?? m_BtnVehicle;
            }
            if (m_DroneControlsRoot == null)
            {
                m_DroneControlsRoot = transform.Find("HUD_DroneFlightControls")?.gameObject;
            }
            if (m_DroneControlsRoot == null) return;

            m_BtnDroneAscend = m_DroneControlsRoot.transform.Find("Btn_DroneAscend")?.GetComponent<Button>();
            m_BtnDroneDescend = m_DroneControlsRoot.transform.Find("Btn_DroneDescend")?.GetComponent<Button>();
            m_BtnDroneSupply = m_DroneControlsRoot.transform.Find("Btn_DroneSupply")?.GetComponent<Button>();
            m_BtnDroneExit = m_DroneControlsRoot.transform.Find("Btn_DroneExit")?.GetComponent<Button>();
        }

        private void WireTopRightButtons()
        {
            if (m_BtnInventory == null)
            {
                m_BtnInventory = transform.Find("HUD_Top/HUD_TopRight/Inventory_Button")?.GetComponent<Button>()
                    ?? transform.Find("Inventory_Button")?.GetComponent<Button>();
            }
            if (m_BtnUpgrade == null)
            {
                m_BtnUpgrade = transform.Find("HUD_Top/HUD_TopRight/Upgrade_Button")?.GetComponent<Button>()
                    ?? transform.Find("Upgrade_Button")?.GetComponent<Button>();
            }
            if (m_BtnSettings == null)
            {
                m_BtnSettings = transform.Find("HUD_Top/HUD_TopRight/Settings_Button")?.GetComponent<Button>()
                    ?? transform.Find("Settings_Button")?.GetComponent<Button>();
            }

            WireARButton();

            if (IUHCampus.Guardian.AR.IUHLiveARController.Instance == null)
            {
                new GameObject("IUH_LIVE_AR_CONTROLLER", typeof(IUHCampus.Guardian.AR.IUHLiveARController));
            }
        }

        private void WireARButton()
        {
            if (m_BtnAR == null)
            {
                m_BtnAR = transform.Find("HUD_Top/HUD_TopRight/AR_Button")?.GetComponent<Button>()
                    ?? transform.Find("AR_Button")?.GetComponent<Button>();
            }
            if (m_BtnAR != null) return;

            var tr = transform.Find("HUD_Top/HUD_TopRight");
            if (tr == null) return;

            // Mở rộng HUD_TopRight để chứa đủ 4 nút (Cài đặt, Nâng cấp, Kho đồ, Camera AR)
            var trRt = tr.GetComponent<RectTransform>();
            if (trRt != null) trRt.sizeDelta = new Vector2(380f, 90f);

            if (m_BtnSettings == null) return;

            var arGo = Instantiate(m_BtnSettings.gameObject, tr, false);
            arGo.name = "AR_Button";
            arGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(-270f, 0f);
            var bgImg = arGo.GetComponent<Image>();
            if (bgImg != null) bgImg.raycastTarget = true;

            var txt = arGo.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = "Camera AR";
                txt.raycastTarget = false;
            }

            var icon = arGo.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null) icon.raycastTarget = false;

            m_BtnAR = arGo.GetComponent<Button>();
        }

        private void CleanupLegacyUtilityButtons()
        {
            // Xóa triệt để 2 nút thô hình chữ nhật cũ: "HƯỚNG DẪN [H]" và "KHO ĐỒ & NÂNG CẤP [B]"
            // nếu còn sót lại từ scene cũ, tránh đè lên cụm nút tròn icon neon hiện đại.
            string[] legacyUglyNames = { "Btn_OpenControlsGuide", "Btn_HUD_OpenShop", "BtnKeyH", "BtnKeyB", "HUD_UtilityKeys" };
            foreach (var legName in legacyUglyNames)
            {
                var legT = transform.Find(legName) ?? transform.Find("HUD_UtilityKeys/" + legName);
                if (legT != null)
                {
                    legT.gameObject.SetActive(false);
                    Destroy(legT.gameObject);
                }
            }
            if (m_BtnKeyH != null)
            {
                m_BtnKeyH.gameObject.SetActive(false);
                Destroy(m_BtnKeyH.gameObject);
                m_BtnKeyH = null;
            }
            if (m_BtnKeyB != null)
            {
                m_BtnKeyB.gameObject.SetActive(false);
                Destroy(m_BtnKeyB.gameObject);
                m_BtnKeyB = null;
            }
        }

        private void WirePlayerStatusWidgets()
        {
            if (m_HealthBarFill == null)
            {
                m_HealthBarFill = transform.Find("HUD_Top/HUD_PlayerStatus/Health_Bar/Fill")?.GetComponent<Image>();
            }
            if (m_HealthText == null)
            {
                m_HealthText = transform.Find("HUD_Top/HUD_PlayerStatus/Health_Bar/HealthText")?.GetComponent<Text>();
            }
            if (m_LevelText == null)
            {
                m_LevelText = transform.Find("HUD_Top/HUD_PlayerStatus/Level_Badge/Text")?.GetComponent<Text>();
            }
            if (m_AvatarImage == null)
            {
                m_AvatarImage = transform.Find("HUD_Top/HUD_PlayerStatus/Avatar_Frame/Avatar_Image")?.GetComponent<Image>();
            }
        }

        public void CheckPlatformVisibility()
        {
            bool shouldShow = m_ForceShowOnPC || Application.isMobilePlatform;
            gameObject.SetActive(shouldShow);
        }

        private void Update()
        {
            EnsurePlayerReferences();

            var droneCtrl = IUHCampus.Guardian.IUHGuardianDroneController.Instance;
            bool isDroneActive = droneCtrl != null && droneCtrl.IsControlled;

            EnsureExplorerMovementEnabled(isDroneActive);
            SyncMovementInput(isDroneActive);
            SyncTouchLookInput(isDroneActive);
            ProcessHeldFire();

            UpdateInteractContextLabel();
            UpdatePlayerStatusUI();
            SyncTouchLookZoneVisibility();
            SyncDroneFlightModeUI();

            // Phím F8 giả lập bật tắt UI mobile trên Editor/PC
            if (Input.GetKeyDown(KeyCode.F8))
            {
                m_ForceShowOnPC = !m_ForceShowOnPC;
                CheckPlatformVisibility();
            }
        }

        private void EnsurePlayerReferences()
        {
            if (m_StarterInputs != null && m_ThirdPersonController != null) return;
            if (Time.unscaledTime < m_NextPlayerSearch) return;

            m_NextPlayerSearch = Time.unscaledTime + IUHPlayerLocator.RetryInterval;
            var student = GameObject.Find("Campus Student") ?? GameObject.Find("PlayerArmature") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null)
            {
                m_StarterInputs = student.GetComponent<StarterAssetsInputs>();
                m_ThirdPersonController = student.GetComponent<ThirdPersonController>();
                return;
            }

            m_ThirdPersonController = FindAnyObjectByType<ThirdPersonController>();
            if (m_ThirdPersonController != null)
            {
                m_StarterInputs = m_ThirdPersonController.GetComponent<StarterAssetsInputs>();
            }
        }

        private void EnsureExplorerMovementEnabled(bool isDroneActive)
        {
            if (m_ThirdPersonController == null || isDroneActive || m_ThirdPersonController.enabled) return;
            if (!IUHCampus.World.IUHConceptPreviewController.IsAnyUIOpen())
            {
                m_ThirdPersonController.enabled = true;
            }
        }

        private void SyncMovementInput(bool isDroneActive)
        {
            if (m_StarterInputs == null) return;

            if (isDroneActive)
            {
                // Khi đang lái drone, nhân vật người chơi đứng yên tại chỗ, KHÔNG chạy theo drone
                m_StarterInputs.MoveInput(Vector2.zero);
                m_StarterInputs.SprintInput(false);
                m_StarterInputs.JumpInput(false);
                return;
            }

            if (m_IsDraggingJoystick && m_InputVector.sqrMagnitude > 0.001f)
            {
                m_StarterInputs.MoveInput(m_InputVector);
                // Kéo cần joystick > 65% bán kính -> tự động Chạy nhanh (Sprint)
                bool autoSprint = m_InputVector.magnitude >= 0.65f || m_IsSprinting;
                m_StarterInputs.SprintInput(autoSprint);
                return;
            }

            if (!m_IsDraggingJoystick)
            {
                m_InputVector = Vector2.zero;
                if (Application.isMobilePlatform && IUHInputHelper.VirtualMoveAxis.sqrMagnitude < 0.001f)
                {
                    m_StarterInputs.MoveInput(Vector2.zero);
                    m_StarterInputs.SprintInput(false);
                }
            }
        }

        private void SyncTouchLookInput(bool isDroneActive)
        {
            if (IsTouchLookActive && m_AccumulatedLookDelta.sqrMagnitude > 0.0001f)
            {
                if (isDroneActive)
                {
                    IUHCampus.Core.IUHInputHelper.VirtualDroneLook = new Vector2(m_AccumulatedLookDelta.x, -m_AccumulatedLookDelta.y) * 0.15f;
                    m_AccumulatedLookDelta = Vector2.zero;
                }
                else if (m_StarterInputs != null)
                {
                    m_StarterInputs.LookInput(m_AccumulatedLookDelta);
                    m_AccumulatedLookDelta = Vector2.zero;
                }
            }
            else if (!IsTouchLookActive && Application.isMobilePlatform && m_StarterInputs != null)
            {
                m_StarterInputs.LookInput(Vector2.zero);
            }
        }

        private void ProcessHeldFire()
        {
            if (!m_IsFiringHeld) return;

            IUHInputHelper.VirtualFireHeld = true;
            if (Time.time >= m_NextContinuousFireTime)
            {
                m_NextContinuousFireTime = Time.time + 0.16f;
                IUHCampusCombatManager.Instance?.TryFireGun();
            }
        }

        private void SyncTouchLookZoneVisibility()
        {
            bool isAnyModalOpen = (IUHCampusActivitiesHUD.Instance != null && (IUHCampusActivitiesHUD.Instance.IsAlbumOpen || IUHCampusActivitiesHUD.Instance.IsMascotDialogOpen))
                || (IUHCombatHUD.Instance != null && IUHCombatHUD.Instance.IsGuideOpen)
                || (IUHUpgradeShopUI.Instance != null && IUHUpgradeShopUI.Instance.IsOpen);

            if (m_TouchLookZone == null || m_TouchLookZone.gameObject.activeSelf != isAnyModalOpen) return;

            m_TouchLookZone.gameObject.SetActive(!isAnyModalOpen);
            if (isAnyModalOpen)
            {
                m_LookPointerId = -999;
                IsTouchLookActive = false;
                StarterAssets.StarterAssetsInputs.IsTouchLookActive = false;
                m_AccumulatedLookDelta = Vector2.zero;
            }
        }

        private void SyncDroneFlightModeUI()
        {
            bool isDroneActive = IUHCampus.Guardian.IUHGuardianDroneController.Instance != null
                && IUHCampus.Guardian.IUHGuardianDroneController.Instance.IsControlled;
            if (isDroneActive != m_LastDroneActiveState)
            {
                m_LastDroneActiveState = isDroneActive;
                UpdateDroneFlightModeUI(isDroneActive);
            }
        }

        private void UpdateDroneFlightModeUI(bool isDroneActive)
        {
            if (m_DroneControlsRoot != null)
            {
                m_DroneControlsRoot.SetActive(isDroneActive);
            }

            var hudActions = transform.Find("HUD_Actions")?.gameObject;
            if (hudActions != null)
            {
                hudActions.SetActive(!isDroneActive);
            }
        }

        private void UpdatePlayerStatusUI()
        {
            if (m_HealthBarFill == null && m_HealthText == null) return;

            int curHp = 100;
            int maxHp = 100;

            if (IUHPlayerHealth.Instance != null)
            {
                curHp = IUHPlayerHealth.Instance.CurrentHp;
                maxHp = Mathf.Max(1, IUHPlayerHealth.Instance.MaxHp);
            }

            if (m_HealthBarFill != null)
            {
                float targetFill = Mathf.Clamp01((float)curHp / maxHp);
                m_HealthBarFill.fillAmount = Mathf.Lerp(m_HealthBarFill.fillAmount, targetFill, Time.unscaledDeltaTime * 10f);
            }

            if (m_HealthText != null)
            {
                m_HealthText.text = $"{curHp} / {maxHp}";
            }

            if (m_LevelText != null && string.IsNullOrEmpty(m_LevelText.text))
            {
                m_LevelText.text = "Lv.1";
            }
        }

        private void UpdateInteractContextLabel()
        {
            if (m_InteractBtnText == null)
            {
                if (m_BtnInteract != null) m_InteractBtnText = m_BtnInteract.GetComponentInChildren<Text>();
                if (m_InteractBtnText == null) return;
            }

            if (IUHHotAirBalloonController.Instance != null && IUHHotAirBalloonController.Instance.IsPlayerNear)
            {
                var bState = IUHHotAirBalloonController.Instance.State;
                if (bState == BalloonFlightState.Grounded)
                {
                    m_InteractBtnText.text = "LÊN KHINH KHÍ CẦU\n[E]";
                }
                else
                {
                    m_InteractBtnText.text = "NHẢY XUỐNG\n[E]";
                }
            }
            else
            {
                m_InteractBtnText.text = "Tương tác\n[E]";
            }
        }

        public void TriggerDoubleTapJump()
        {
            // Nếu người chơi đang ở trên giỏ khinh khí cầu, nhấn đúp nhảy là lập tức bung cánh nhảy xuống
            if (IUHHotAirBalloonController.Instance != null && IUHHotAirBalloonController.Instance.IsPlayerInBasket)
            {
                IUHHotAirBalloonController.Instance.JumpDownFromBalloon();
                return;
            }

            if (m_StarterInputs != null)
            {
                m_StarterInputs.JumpInput(true);
                StartCoroutine(ResetJumpRoutine());
            }
        }

        private System.Collections.IEnumerator ResetJumpRoutine()
        {
            yield return new WaitForSeconds(0.12f);
            if (m_StarterInputs != null)
            {
                m_StarterInputs.JumpInput(false);
            }
        }

        public void SetupButtonListeners()
        {
            SetupGameplayActionListeners();
            SetupTopMenuListeners();
            SetupCombatButtonListeners();
            SetupLegacyUtilityListeners();
            SetupDroneFlightListeners();
        }

        private static void BindClick(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null) return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }

        private static void BindHoldTrigger(
            Button btn,
            UnityEngine.Events.UnityAction<BaseEventData> onDown,
            UnityEngine.Events.UnityAction<BaseEventData> onUp)
        {
            if (btn == null) return;
            var trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(onDown);
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(onUp);
            trigger.triggers.Add(up);
        }

        private void SetupGameplayActionListeners()
        {
            BindClick(m_BtnInteract, () =>
            {
                if (IUHHotAirBalloonController.Instance != null && IUHHotAirBalloonController.Instance.IsPlayerNear)
                {
                    IUHHotAirBalloonController.Instance.ToggleBoardOrLaunch();
                }
                else
                {
                    IUHInputHelper.VirtualInteractPressed = true;
                }
                IUHCombatAudio.PlaySword();
            });

            BindHoldTrigger(
                m_BtnSprint,
                _ =>
                {
                    m_IsSprinting = true;
                    IUHInputHelper.VirtualTurboHeld = true;
                    if (m_StarterInputs != null) m_StarterInputs.SprintInput(true);
                },
                _ =>
                {
                    m_IsSprinting = false;
                    IUHInputHelper.VirtualTurboHeld = false;
                    if (m_StarterInputs != null) m_StarterInputs.SprintInput(false);
                });

            BindClick(m_BtnJump, () => m_StarterInputs?.JumpInput(true));
            BindClick(m_BtnFireworks, () => IUHCampusFireworksManager.Instance?.TriggerPlayerFirework());
            BindClick(m_BtnVehicle, () => IUHInputHelper.VirtualVehiclePressed = true);
        }

        private void SetupTopMenuListeners()
        {
            BindClick(m_BtnInventory, () =>
            {
                IUHCombatAudio.PlayShield();
                if (IUHCampusActivitiesHUD.Instance != null)
                {
                    IUHCampusActivitiesHUD.Instance.ToggleAlbum();
                }
                else if (IUHUpgradeShopUI.Instance != null)
                {
                    IUHUpgradeShopUI.Instance.ToggleShop();
                }
            });

            BindClick(m_BtnUpgrade, () =>
            {
                IUHCombatAudio.PlayCoin();
                IUHUpgradeShopUI.Instance?.ToggleShop();
            });

            BindClick(m_BtnSettings, () =>
            {
                IUHCombatAudio.PlayShield();
                IUHCombatHUD.Instance?.ToggleGuide();
            });

            BindClick(m_BtnAR, () =>
            {
                Debug.Log("[IUH] Bấm nút Camera AR!");
                IUHCombatAudio.PlayShield();
                var arCtrl = IUHCampus.Guardian.AR.IUHLiveARController.Instance;
                if (arCtrl == null)
                {
                    var liveObj = new GameObject("IUH_LIVE_AR_CONTROLLER", typeof(IUHCampus.Guardian.AR.IUHLiveARController));
                    arCtrl = liveObj.GetComponent<IUHCampus.Guardian.AR.IUHLiveARController>();
                }
                arCtrl?.OpenARMode();
            });
        }

        private void SetupCombatButtonListeners()
        {
            BindHoldTrigger(
                m_BtnFire,
                _ =>
                {
                    m_IsFiringHeld = true;
                    m_NextContinuousFireTime = 0f;
                    IUHInputHelper.VirtualFirePressed = true;
                    IUHInputHelper.VirtualFireHeld = true;
                    IUHCampusCombatManager.Instance?.TryFireGun();
                },
                _ =>
                {
                    m_IsFiringHeld = false;
                    IUHInputHelper.VirtualFireHeld = false;
                });

            BindClick(m_BtnAim, () =>
            {
                IUHInputHelper.VirtualAimToggled = !IUHInputHelper.VirtualAimToggled;
                IUHCombatAudio.PlayLaserShoot();
            });

            BindClick(m_BtnMissile, () =>
            {
                IUHInputHelper.VirtualMissilePressed = true;
                IUHCampusCombatManager.Instance?.UseAntiAirMissile();
            });
        }

        private void SetupLegacyUtilityListeners()
        {
            BindClick(m_BtnKeyH, () =>
            {
                IUHInputHelper.VirtualHelpPressed = true;
                IUHCombatAudio.PlayShield();
                var hud = IUHCombatHUD.Instance ?? FindAnyObjectByType<IUHCombatHUD>(FindObjectsInactive.Include);
                hud?.ToggleGuide();
            });

            BindClick(m_BtnKeyB, () =>
            {
                IUHInputHelper.VirtualBackpackPressed = true;
                IUHCombatAudio.PlayCoin();
                var shop = IUHUpgradeShopUI.Instance ?? FindAnyObjectByType<IUHUpgradeShopUI>(FindObjectsInactive.Include);
                shop?.ToggleShop();
            });
        }

        private void SetupDroneFlightListeners()
        {
            BindClick(m_BtnDroneLaunch, () =>
            {
                IUHCombatAudio.PlayLaserShoot();
                IUHCampus.Guardian.IUHGuardianDroneController.Instance?.SetControlled(true);
            });

            BindClick(m_BtnDroneExit, () =>
            {
                IUHCombatAudio.PlayShield();
                IUHCampus.Guardian.IUHGuardianDroneController.Instance?.SetControlled(false);
            });

            BindClick(m_BtnDroneSupply, () =>
            {
                IUHCombatAudio.PlayCoin();
                IUHCampus.Guardian.IUHGuardianDroneController.Instance?.DropSupplyAirdrop();
            });

            BindHoldTrigger(
                m_BtnDroneAscend,
                _ => IUHInputHelper.VirtualDroneAltitude = 1f,
                _ => IUHInputHelper.VirtualDroneAltitude = 0f);

            BindHoldTrigger(
                m_BtnDroneDescend,
                _ => IUHInputHelper.VirtualDroneAltitude = -1f,
                _ => IUHInputHelper.VirtualDroneAltitude = 0f);
        }

        public void SetupJoystickEvents()
        {
            if (m_JoystickBase == null) return;

            var trigger = m_JoystickBase.gameObject.GetComponent<EventTrigger>() ?? m_JoystickBase.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((data) => OnJoystickDown((PointerEventData)data));
            trigger.triggers.Add(down);

            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener((data) => OnJoystickDrag((PointerEventData)data));
            trigger.triggers.Add(drag);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((data) => OnJoystickUp((PointerEventData)data));
            trigger.triggers.Add(up);
        }

        private void OnJoystickDown(PointerEventData eventData)
        {
            m_IsDraggingJoystick = true;
            m_JoystickPointerId = eventData.pointerId;

            // Cơ chế Nhấn đúp (Double Tap to Jump): Nhấn đúp 2 lần liên tiếp (< 0.38s) là Nhảy!
            float timeSinceLastTap = Time.time - m_LastJoystickTapTime;
            if (timeSinceLastTap > 0.05f && timeSinceLastTap < 0.38f)
            {
                TriggerDoubleTapJump();
            }
            m_LastJoystickTapTime = Time.time;

            OnJoystickDrag(eventData);
        }

        private void OnJoystickDrag(PointerEventData eventData)
        {
            if (!m_IsDraggingJoystick || m_JoystickBase == null) return;
            if (eventData.pointerId != m_JoystickPointerId) return;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_JoystickBase, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                localPoint = Vector2.ClampMagnitude(localPoint, m_JoystickRange);

                if (m_JoystickHandle != null)
                {
                    m_JoystickHandle.anchoredPosition = localPoint;
                }

                m_InputVector = localPoint / m_JoystickRange;
                IUHInputHelper.VirtualMoveAxis = new Vector2(m_InputVector.x, m_InputVector.y);

                // Auto-Sprint theo cự ly kéo Joystick
                if (m_StarterInputs != null)
                {
                    bool autoSprint = m_InputVector.magnitude >= 0.65f || m_IsSprinting;
                    m_StarterInputs.SprintInput(autoSprint);
                }
            }
        }

        private void OnJoystickUp(PointerEventData eventData)
        {
            if (eventData.pointerId != m_JoystickPointerId) return;

            m_IsDraggingJoystick = false;
            m_JoystickPointerId = -999;
            m_InputVector = Vector2.zero;
            IUHInputHelper.VirtualMoveAxis = Vector2.zero;

            if (m_JoystickHandle != null)
            {
                m_JoystickHandle.anchoredPosition = Vector2.zero;
            }

            if (m_StarterInputs != null)
            {
                m_StarterInputs.MoveInput(Vector2.zero);
                m_StarterInputs.SprintInput(false);
            }
        }

        public void SetupTouchLookEvents()
        {
            if (m_TouchLookZone == null) return;

            var trigger = m_TouchLookZone.gameObject.GetComponent<EventTrigger>() ?? m_TouchLookZone.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(data => OnTouchLookDown((PointerEventData)data));
            trigger.triggers.Add(down);

            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener(data => OnTouchLookDrag((PointerEventData)data));
            trigger.triggers.Add(drag);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(data => OnTouchLookUp((PointerEventData)data));
            trigger.triggers.Add(up);
        }

        private void OnTouchLookDown(PointerEventData ped)
        {
            if (m_LookPointerId != -999) return;
            m_LookPointerId = ped.pointerId;
            m_PreviousLookTouchPos = ped.position;
            IsTouchLookActive = true;
            StarterAssets.StarterAssetsInputs.IsTouchLookActive = true;
            m_AccumulatedLookDelta = Vector2.zero;
        }

        private void OnTouchLookDrag(PointerEventData ped)
        {
            if (ped.pointerId != m_LookPointerId) return;
            Vector2 currentPos = ped.position;
            Vector2 rawDelta = currentPos - m_PreviousLookTouchPos;
            m_PreviousLookTouchPos = currentPos;

            // INVERT Y: Vuốt ngón tay lên trên (rawDelta.y > 0) -> camera ngửa mặt lên trời (pitch âm)
            m_AccumulatedLookDelta += new Vector2(rawDelta.x, -rawDelta.y) * m_LookSensitivity;
        }

        private void OnTouchLookUp(PointerEventData ped)
        {
            if (ped.pointerId != m_LookPointerId) return;
            m_LookPointerId = -999;
            IsTouchLookActive = false;
            StarterAssets.StarterAssetsInputs.IsTouchLookActive = false;
            m_AccumulatedLookDelta = Vector2.zero;
            m_StarterInputs?.LookInput(Vector2.zero);
        }
    }
}
