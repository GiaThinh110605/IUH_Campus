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
            // 1. Joystick
            if (m_JoystickBase == null)
            {
                m_JoystickBase = transform.Find("HUD_Movement/VirtualJoystick/JoystickBase")?.GetComponent<RectTransform>()
                    ?? transform.Find("JoystickBase")?.GetComponent<RectTransform>();
            }
            if (m_JoystickHandle == null && m_JoystickBase != null)
            {
                m_JoystickHandle = m_JoystickBase.Find("JoystickHandle")?.GetComponent<RectTransform>();
            }

            // 2. Touch Look Zone
            if (m_TouchLookZone == null)
            {
                m_TouchLookZone = transform.Find("TouchLookZone")?.GetComponent<RectTransform>();
            }

            // 3. Action Buttons
            if (m_BtnFire == null)
            {
                m_BtnFire = transform.Find("HUD_Actions/Fire_Button")?.GetComponent<Button>()
                    ?? transform.Find("HUD_Actions/BtnFire")?.GetComponent<Button>()
                    ?? transform.Find("BtnFire")?.GetComponent<Button>();
            }
            if (m_BtnInteract == null)
            {
                m_BtnInteract = transform.Find("HUD_Actions/Interact_Button")?.GetComponent<Button>()
                    ?? transform.Find("BtnInteract")?.GetComponent<Button>();
            }
            if (m_BtnInteract != null)
            {
                m_InteractBtnText = m_BtnInteract.GetComponentInChildren<Text>();
            }
            if (m_BtnSprint == null)
            {
                m_BtnSprint = transform.Find("HUD_Actions/Run_Button")?.GetComponent<Button>()
                    ?? transform.Find("BtnSprint")?.GetComponent<Button>();
            }
            if (m_BtnJump == null)
            {
                m_BtnJump = transform.Find("HUD_Actions/Jump_Button")?.GetComponent<Button>()
                    ?? transform.Find("BtnJump")?.GetComponent<Button>();
            }
            if (m_BtnFireworks == null)
            {
                m_BtnFireworks = transform.Find("HUD_Actions/Firework_Button")?.GetComponent<Button>()
                    ?? transform.Find("BtnFireworks")?.GetComponent<Button>();
            }
            if (m_BtnVehicle == null)
            {
                m_BtnVehicle = transform.Find("HUD_Actions/Vehicle_Button")?.GetComponent<Button>()
                    ?? transform.Find("BtnVehicle")?.GetComponent<Button>();
            }
            if (m_BtnDroneLaunch == null)
            {
                m_BtnDroneLaunch = transform.Find("HUD_Actions/Drone_Button")?.GetComponent<Button>()
                    ?? m_BtnVehicle;
            }

            if (m_DroneControlsRoot == null)
            {
                m_DroneControlsRoot = transform.Find("HUD_DroneFlightControls")?.gameObject;
            }
            if (m_DroneControlsRoot != null)
            {
                m_BtnDroneAscend = m_DroneControlsRoot.transform.Find("Btn_DroneAscend")?.GetComponent<Button>();
                m_BtnDroneDescend = m_DroneControlsRoot.transform.Find("Btn_DroneDescend")?.GetComponent<Button>();
                m_BtnDroneSupply = m_DroneControlsRoot.transform.Find("Btn_DroneSupply")?.GetComponent<Button>();
                m_BtnDroneExit = m_DroneControlsRoot.transform.Find("Btn_DroneExit")?.GetComponent<Button>();
            }

            // Combat buttons fallback
            if (m_BtnAim == null) m_BtnAim = transform.Find("HUD_Actions/BtnAim")?.GetComponent<Button>() ?? transform.Find("BtnAim")?.GetComponent<Button>();
            if (m_BtnMissile == null) m_BtnMissile = transform.Find("HUD_Actions/BtnMissile")?.GetComponent<Button>() ?? transform.Find("BtnMissile")?.GetComponent<Button>();

            // 4. Top Right Buttons
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

            // Utility Key Buttons [H] & [B]
            if (m_BtnKeyH == null)
            {
                m_BtnKeyH = transform.Find("HUD_UtilityKeys/BtnKeyH")?.GetComponent<Button>()
                    ?? transform.Find("BtnKeyH")?.GetComponent<Button>();
            }
            if (m_BtnKeyB == null)
            {
                m_BtnKeyB = transform.Find("HUD_UtilityKeys/BtnKeyB")?.GetComponent<Button>()
                    ?? transform.Find("BtnKeyB")?.GetComponent<Button>();
            }

            // 5. Player Status Widgets
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
            if (m_StarterInputs == null || m_ThirdPersonController == null)
            {
                var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
                if (student != null)
                {
                    m_StarterInputs = student.GetComponent<StarterAssetsInputs>();
                    m_ThirdPersonController = student.GetComponent<ThirdPersonController>();
                }
            }

            bool isDroneActive = IUHCampus.Guardian.IUHGuardianDroneController.Instance != null && IUHCampus.Guardian.IUHGuardianDroneController.Instance.IsControlled;

            // 1. Đồng bộ di chuyển từ Joystick vào Player controller & Tự động Chạy (Auto-Sprint)
            if (m_StarterInputs != null)
            {
                if (isDroneActive)
                {
                    // Khi đang lái drone, nhân vật người chơi đứng yên tại chỗ, KHÔNG chạy theo drone
                    m_StarterInputs.MoveInput(Vector2.zero);
                    m_StarterInputs.SprintInput(false);
                    m_StarterInputs.JumpInput(false);
                }
                else if (m_IsDraggingJoystick && m_InputVector.sqrMagnitude > 0.001f)
                {
                    m_StarterInputs.MoveInput(m_InputVector);

                    // Khi người chơi kéo cần joystick ra xa (> 65% bán kính), nhân vật tự động Chạy nhanh (Sprint)
                    // Kéo nhẹ (< 65%) thì đi bộ bình thường
                    bool autoSprint = m_InputVector.magnitude >= 0.65f || m_IsSprinting;
                    m_StarterInputs.SprintInput(autoSprint);
                }
                else if (!m_IsDraggingJoystick && IUHInputHelper.VirtualMoveAxis.sqrMagnitude < 0.001f && Application.isMobilePlatform)
                {
                    m_StarterInputs.MoveInput(Vector2.zero);
                    m_StarterInputs.SprintInput(false);
                }
            }

            // 2. Đồng bộ xoay camera (Touch Look) mượt mà không khựng, không drift
            if (IsTouchLookActive && m_AccumulatedLookDelta.sqrMagnitude > 0.0001f)
            {
                if (IUHCampus.Guardian.IUHGuardianDroneController.Instance != null && IUHCampus.Guardian.IUHGuardianDroneController.Instance.IsControlled)
                {
                    IUHCampus.Core.IUHInputHelper.VirtualDroneLook = new Vector2(m_AccumulatedLookDelta.x, -m_AccumulatedLookDelta.y) * 0.15f;
                    m_AccumulatedLookDelta = Vector2.zero;
                }
                else if (m_StarterInputs != null)
                {
                    m_StarterInputs.LookInput(m_AccumulatedLookDelta);
                    m_AccumulatedLookDelta = Vector2.zero; // Tiêu thụ ngay delta frame này
                }
            }
            else if (!IsTouchLookActive && Application.isMobilePlatform && m_StarterInputs != null)
            {
                m_StarterInputs.LookInput(Vector2.zero);
            }

            // 3. Xử lý giữ nút Bắn liên thanh trên Mobile (chỉ bắn khi bấm/giữ nút Bắn Súng)
            if (m_IsFiringHeld)
            {
                IUHInputHelper.VirtualFireHeld = true;
                if (Time.time >= m_NextContinuousFireTime)
                {
                    m_NextContinuousFireTime = Time.time + 0.16f;
                    if (IUHCampusCombatManager.Instance != null)
                    {
                        IUHCampusCombatManager.Instance.TryFireGun();
                    }
                }
            }

            // 4. Cập nhật nhãn động cho nút Tương tác [E] (Khinh khí cầu, Xe, NPC)
            UpdateInteractContextLabel();

            // 5. Đồng bộ thanh máu và Level người chơi
            UpdatePlayerStatusUI();

            // 6. Tự động ẩn TouchLookZone khi có Modal mở (Album, Balo/Shop, Hướng dẫn) để không chặn cảm ứng bấm nút đóng
            bool isAnyModalOpen = (IUHCampusActivitiesHUD.Instance != null && (IUHCampusActivitiesHUD.Instance.IsAlbumOpen || IUHCampusActivitiesHUD.Instance.IsMascotDialogOpen))
                || (IUHCombatHUD.Instance != null && IUHCombatHUD.Instance.IsGuideOpen)
                || (IUHUpgradeShopUI.Instance != null && IUHUpgradeShopUI.Instance.IsOpen);

            if (m_TouchLookZone != null && m_TouchLookZone.gameObject.activeSelf == isAnyModalOpen)
            {
                m_TouchLookZone.gameObject.SetActive(!isAnyModalOpen);
                if (isAnyModalOpen)
                {
                    m_LookPointerId = -999;
                    IsTouchLookActive = false;
                    m_AccumulatedLookDelta = Vector2.zero;
                }
            }

            // 7. Đồng bộ giao diện điều khiển Drone khi đang bay
            bool isDroneActive = IUHCampus.Guardian.IUHGuardianDroneController.Instance != null && IUHCampus.Guardian.IUHGuardianDroneController.Instance.IsControlled;
            if (isDroneActive != m_LastDroneActiveState)
            {
                m_LastDroneActiveState = isDroneActive;
                UpdateDroneFlightModeUI(isDroneActive);
            }

            // Phím F8 giả lập bật tắt UI mobile trên Editor/PC
            if (Input.GetKeyDown(KeyCode.F8))
            {
                m_ForceShowOnPC = !m_ForceShowOnPC;
                CheckPlatformVisibility();
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
            // ===== 1. NÚT TƯƠNG TÁC [E] (Khinh khí cầu / Trận đấu / Dị tượng / Trò chuyện) ===== //
            if (m_BtnInteract != null)
            {
                m_BtnInteract.onClick.RemoveAllListeners();
                m_BtnInteract.onClick.AddListener(() => {
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
            }

            // ===== 2. NÚT CHẠY NHANH [Shift / Turbo] ===== //
            if (m_BtnSprint != null)
            {
                var trigger = m_BtnSprint.gameObject.GetComponent<EventTrigger>() ?? m_BtnSprint.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();

                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener((e) => {
                    m_IsSprinting = true;
                    IUHInputHelper.VirtualTurboHeld = true;
                    if (m_StarterInputs != null) m_StarterInputs.SprintInput(true);
                });
                trigger.triggers.Add(down);

                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener((e) => {
                    m_IsSprinting = false;
                    IUHInputHelper.VirtualTurboHeld = false;
                    if (m_StarterInputs != null) m_StarterInputs.SprintInput(false);
                });
                trigger.triggers.Add(up);
            }

            // ===== 3. NÚT NHẢY [Space] ===== //
            if (m_BtnJump != null)
            {
                m_BtnJump.onClick.RemoveAllListeners();
                m_BtnJump.onClick.AddListener(() => {
                    if (m_StarterInputs != null)
                    {
                        m_StarterInputs.JumpInput(true);
                    }
                });
            }

            // ===== 4. NÚT BẮN PHÁO HOA LỄ HỘI [K/B] ===== //
            if (m_BtnFireworks != null)
            {
                m_BtnFireworks.onClick.RemoveAllListeners();
                m_BtnFireworks.onClick.AddListener(() => {
                    if (IUHCampusFireworksManager.Instance != null)
                    {
                        IUHCampusFireworksManager.Instance.TriggerPlayerFirework();
                    }
                });
            }

            // ===== 5. NÚT LÊN/XUỐNG XE & KHINH KHÍ CẦU [V] ===== //
            if (m_BtnVehicle != null)
            {
                m_BtnVehicle.onClick.RemoveAllListeners();
                m_BtnVehicle.onClick.AddListener(() => {
                    IUHInputHelper.VirtualVehiclePressed = true;
                });
            }

            // ===== 6. TOP RIGHT: KHO ĐỒ (Inventory / Album) ===== //
            if (m_BtnInventory != null)
            {
                m_BtnInventory.onClick.RemoveAllListeners();
                m_BtnInventory.onClick.AddListener(() => {
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
            }

            // ===== 7. TOP RIGHT: NÂNG CẤP (Upgrade Shop) ===== //
            if (m_BtnUpgrade != null)
            {
                m_BtnUpgrade.onClick.RemoveAllListeners();
                m_BtnUpgrade.onClick.AddListener(() => {
                    IUHCombatAudio.PlayCoin();
                    if (IUHUpgradeShopUI.Instance != null)
                    {
                        IUHUpgradeShopUI.Instance.ToggleShop();
                    }
                });
            }

            // ===== 8. TOP RIGHT: CÀI ĐẶT (Settings / Guide) ===== //
            if (m_BtnSettings != null)
            {
                m_BtnSettings.onClick.RemoveAllListeners();
                m_BtnSettings.onClick.AddListener(() => {
                    IUHCombatAudio.PlayShield();
                    if (IUHCombatHUD.Instance != null)
                    {
                        IUHCombatHUD.Instance.ToggleGuide();
                    }
                });
            }

            // ===== 9. COMBAT: NÚT BẮN (nếu có) ===== //
            if (m_BtnFire != null)
            {
                var trigger = m_BtnFire.gameObject.GetComponent<EventTrigger>() ?? m_BtnFire.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();

                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener((e) => {
                    m_IsFiringHeld = true;
                    m_NextContinuousFireTime = 0f;
                    IUHInputHelper.VirtualFirePressed = true;
                    IUHInputHelper.VirtualFireHeld = true;
                    if (IUHCampusCombatManager.Instance != null)
                    {
                        IUHCampusCombatManager.Instance.TryFireGun();
                    }
                });
                trigger.triggers.Add(down);

                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener((e) => {
                    m_IsFiringHeld = false;
                    IUHInputHelper.VirtualFireHeld = false;
                });
                trigger.triggers.Add(up);
            }

            // ===== 10. COMBAT: NÚT NGẮM (nếu có) ===== //
            if (m_BtnAim != null)
            {
                m_BtnAim.onClick.RemoveAllListeners();
                m_BtnAim.onClick.AddListener(() => {
                    IUHInputHelper.VirtualAimToggled = !IUHInputHelper.VirtualAimToggled;
                    IUHCombatAudio.PlayLaserShoot();
                });
            }

            // ===== 11. COMBAT: NÚT TÊN LỬA (nếu có) ===== //
            if (m_BtnMissile != null)
            {
                m_BtnMissile.onClick.RemoveAllListeners();
                m_BtnMissile.onClick.AddListener(() => {
                    IUHInputHelper.VirtualMissilePressed = true;
                    if (IUHCampusCombatManager.Instance != null)
                    {
                        IUHCampusCombatManager.Instance.UseAntiAirMissile();
                    }
                });
            }

            // ===== 12. PHÍM NHỎ [H] (HƯỚNG DẪN / BẢNG TRỢ GIÚP / CÒI XE) ===== //
            if (m_BtnKeyH != null)
            {
                m_BtnKeyH.onClick.RemoveAllListeners();
                m_BtnKeyH.onClick.AddListener(() => {
                    IUHInputHelper.VirtualHelpPressed = true;
                    IUHCombatAudio.PlayShield();
                    var hud = IUHCombatHUD.Instance ?? FindAnyObjectByType<IUHCombatHUD>(FindObjectsInactive.Include);
                    if (hud != null)
                    {
                        hud.ToggleGuide();
                    }
                });
            }

            // ===== 13. PHÍM NHỎ [B] (BALO / CỬA HÀNG / THẢ BOM) ===== //
            if (m_BtnKeyB != null)
            {
                m_BtnKeyB.onClick.RemoveAllListeners();
                m_BtnKeyB.onClick.AddListener(() => {
                    IUHInputHelper.VirtualBackpackPressed = true;
                    IUHCombatAudio.PlayCoin();
                    var shop = IUHUpgradeShopUI.Instance ?? FindAnyObjectByType<IUHUpgradeShopUI>(FindObjectsInactive.Include);
                    if (shop != null)
                    {
                        shop.ToggleShop();
                    }
                });
            }

            // ===== 14. DRONE FLIGHT CONTROLS ===== //
            if (m_BtnDroneLaunch != null)
            {
                m_BtnDroneLaunch.onClick.RemoveAllListeners();
                m_BtnDroneLaunch.onClick.AddListener(() => {
                    IUHCombatAudio.PlayLaserShoot();
                    if (IUHCampus.Guardian.IUHGuardianDroneController.Instance != null)
                    {
                        IUHCampus.Guardian.IUHGuardianDroneController.Instance.SetControlled(true);
                    }
                });
            }

            if (m_BtnDroneExit != null)
            {
                m_BtnDroneExit.onClick.RemoveAllListeners();
                m_BtnDroneExit.onClick.AddListener(() => {
                    IUHCombatAudio.PlayShield();
                    if (IUHCampus.Guardian.IUHGuardianDroneController.Instance != null)
                    {
                        IUHCampus.Guardian.IUHGuardianDroneController.Instance.SetControlled(false);
                    }
                });
            }

            if (m_BtnDroneSupply != null)
            {
                m_BtnDroneSupply.onClick.RemoveAllListeners();
                m_BtnDroneSupply.onClick.AddListener(() => {
                    IUHCombatAudio.PlayCoin();
                    if (IUHCampus.Guardian.IUHGuardianDroneController.Instance != null)
                    {
                        IUHCampus.Guardian.IUHGuardianDroneController.Instance.DropSupplyAirdrop();
                    }
                });
            }

            if (m_BtnDroneAscend != null)
            {
                var trigger = m_BtnDroneAscend.gameObject.GetComponent<EventTrigger>() ?? m_BtnDroneAscend.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();
                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener((e) => { IUHInputHelper.VirtualDroneAltitude = 1f; });
                trigger.triggers.Add(down);
                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener((e) => { IUHInputHelper.VirtualDroneAltitude = 0f; });
                trigger.triggers.Add(up);
            }

            if (m_BtnDroneDescend != null)
            {
                var trigger = m_BtnDroneDescend.gameObject.GetComponent<EventTrigger>() ?? m_BtnDroneDescend.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();
                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener((e) => { IUHInputHelper.VirtualDroneAltitude = -1f; });
                trigger.triggers.Add(down);
                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener((e) => { IUHInputHelper.VirtualDroneAltitude = 0f; });
                trigger.triggers.Add(up);
            }
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
            down.callback.AddListener((data) => {
                var ped = (PointerEventData)data;
                if (m_LookPointerId == -999)
                {
                    m_LookPointerId = ped.pointerId;
                    m_PreviousLookTouchPos = ped.position;
                    IsTouchLookActive = true;
                    m_AccumulatedLookDelta = Vector2.zero;
                }
            });
            trigger.triggers.Add(down);

            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener((data) => {
                var ped = (PointerEventData)data;
                if (ped.pointerId == m_LookPointerId)
                {
                    Vector2 currentPos = ped.position;
                    Vector2 rawDelta = currentPos - m_PreviousLookTouchPos;
                    m_PreviousLookTouchPos = currentPos;

                    // INVERT Y: Vuốt ngón tay lên trên (rawDelta.y > 0) -> camera ngửa mặt lên trời (pitch âm)!
                    Vector2 frameLookDelta = new Vector2(rawDelta.x, -rawDelta.y) * m_LookSensitivity;
                    m_AccumulatedLookDelta += frameLookDelta;
                }
            });
            trigger.triggers.Add(drag);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((data) => {
                var ped = (PointerEventData)data;
                if (ped.pointerId == m_LookPointerId)
                {
                    m_LookPointerId = -999;
                    IsTouchLookActive = false;
                    m_AccumulatedLookDelta = Vector2.zero;
                    if (m_StarterInputs != null)
                    {
                        m_StarterInputs.LookInput(Vector2.zero);
                    }
                }
            });
            trigger.triggers.Add(up);
        }
    }
}
