using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IUHCampus.Core;
using IUHCampus.Combat;
using IUHCampus.Guardian;
using StarterAssets;

namespace IUHCampus.UI
{
    /// <summary>
    /// Điều khiển toàn diện giao diện cảm ứng trên điện thoại ở chế độ màn hình ngang (Landscape Cyberpunk RPG HUD):
    /// - Tự động khóa màn hình xoay ngang (Landscape Left / Right).
    /// - Cần xoay Joystick ảo góc trái điều khiển di chuyển nhân vật (W, A, S, D).
    /// - Vùng cảm ứng đa điểm góc phải màn hình vuốt xoay camera 360 độ & ngửa lên bầu trời (Touch Look mượt mà, Inverted Y).
    /// - Cụm nút hành động công thái học phong cách Cyberpunk Glassmorphism:
    ///   + 🔫 [FIRE] Nút BẮN súng Plasma liên thanh to nhất, nổi bật nhất (giữ để xả đạn liên tục vào quái trên trời/dưới đất).
    ///   + 🎯 [AIM] Nút NGẮM ADS camera zoom và khóa mục tiêu quái vật bay trên trời.
    ///   + ⚔️ [E] Nút TƯƠNG TÁC / VÀO TRẬN MATCH-3 RPG / Dị tượng khuôn viên IUH.
    ///   + 🚀 [Q] Nút TÊN LỬA CAO XẠ phòng không bắn hạ quái bay / khinh khí cầu.
    ///   + 🦘 [Space] Nút NHẢY vượt địa hình.
    ///   + ⚡ [Shift] Nút CHẠY NHANH / TĂNG TỐC (Sprint / Turbo).
    ///   + 🛵 [V] Nút LÊN/XUỐNG XE & KHINH KHÍ CẦU.
    ///   + 🎆 [K] Nút BẮN PHÁO HOA lễ hội rực sáng khuôn viên trường.
    /// </summary>
    public class IUHMobileControlsUI : MonoBehaviour
    {
        public static IUHMobileControlsUI Instance { get; private set; }
        public static bool IsTouchLookActive { get; private set; }

        [Header("Orientation Config")]
        [SerializeField] private bool m_ForceLandscape = true;

        [Header("Joystick References")]
        [SerializeField] private RectTransform m_JoystickBase;
        [SerializeField] private RectTransform m_JoystickHandle;
        [SerializeField] private float m_JoystickRange = 70f;

        [Header("Touch Look Zone")]
        [SerializeField] private RectTransform m_TouchLookZone;
        [SerializeField] private float m_LookSensitivity = 0.35f;

        [Header("Combat Action Buttons")]
        [SerializeField] private Button m_BtnFire;
        [SerializeField] private Button m_BtnAim;
        [SerializeField] private Button m_BtnMissile;

        [Header("Campus Exploration Buttons")]
        [SerializeField] private Button m_BtnInteract;
        [SerializeField] private Button m_BtnJump;
        [SerializeField] private Button m_BtnSprint;
        [SerializeField] private Button m_BtnVehicle;
        [SerializeField] private Button m_BtnFireworks;

        [Header("Platform Visibility")]
        [SerializeField] private bool m_ForceShowOnPC = false;

        private Vector2 m_InputVector = Vector2.zero;
        private bool m_IsDraggingJoystick = false;
        private int m_JoystickPointerId = -999;
        private bool m_IsSprinting = false;
        private StarterAssetsInputs m_StarterInputs;
        private ThirdPersonController m_ThirdPersonController;

        // Touch Look internal state
        private int m_LookPointerId = -999;
        private Vector2 m_PreviousLookTouchPos = Vector2.zero;
        private Vector2 m_AccumulatedLookDelta = Vector2.zero;
        private bool m_IsFiringHeld = false;
        private float m_NextContinuousFireTime = 0f;

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

        // Procedural Sprite cache
        private static Sprite s_CircleSprite;
        private static Sprite s_GlowRingSprite;

        public static Sprite GetCircleSprite()
        {
            if (s_CircleSprite != null) return s_CircleSprite;
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float radius = center - 2f;
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01((radius - d) / 1.5f);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            s_CircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return s_CircleSprite;
        }

        public static Sprite GetGlowRingSprite()
        {
            if (s_GlowRingSprite != null) return s_GlowRingSprite;
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float outerR = center - 2f;
            float innerR = outerR * 0.82f;
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = 0f;
                    if (d <= outerR)
                    {
                        if (d >= innerR)
                        {
                            // Viền neon sáng
                            alpha = Mathf.Clamp01((outerR - d) / 1.5f) * Mathf.Clamp01((d - innerR) / 1.5f + 0.35f);
                        }
                        else
                        {
                            // Lòng kính trong suốt
                            alpha = 0.28f * (1f - (innerR - d) / innerR * 0.3f);
                        }
                    }
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            s_GlowRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return s_GlowRingSprite;
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
            ApplyAestheticStyling();
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

        private void AutoWireReferences()
        {
            if (m_JoystickBase == null) m_JoystickBase = transform.Find("JoystickBase")?.GetComponent<RectTransform>();
            if (m_JoystickHandle == null && m_JoystickBase != null) m_JoystickHandle = m_JoystickBase.Find("JoystickHandle")?.GetComponent<RectTransform>();
            if (m_TouchLookZone == null) m_TouchLookZone = transform.Find("TouchLookZone")?.GetComponent<RectTransform>();

            if (m_BtnFire == null) m_BtnFire = transform.Find("BtnFire")?.GetComponent<Button>();
            if (m_BtnAim == null) m_BtnAim = transform.Find("BtnAim")?.GetComponent<Button>();
            if (m_BtnMissile == null) m_BtnMissile = transform.Find("BtnMissile")?.GetComponent<Button>();

            if (m_BtnInteract == null) m_BtnInteract = transform.Find("BtnInteract")?.GetComponent<Button>();
            if (m_BtnJump == null) m_BtnJump = transform.Find("BtnJump")?.GetComponent<Button>();
            if (m_BtnSprint == null) m_BtnSprint = transform.Find("BtnSprint")?.GetComponent<Button>();
            if (m_BtnVehicle == null) m_BtnVehicle = transform.Find("BtnVehicle")?.GetComponent<Button>();
            if (m_BtnFireworks == null) m_BtnFireworks = transform.Find("BtnFireworks")?.GetComponent<Button>();
        }

        /// <summary>
        /// Nâng cấp toàn diện hình ảnh các nút bấm thành phong cách Cyberpunk RPG bo tròn phát sáng
        /// </summary>
        public void ApplyAestheticStyling()
        {
            var roundSprite = GetCircleSprite();
            var glowRing = GetGlowRingSprite();

            if (m_JoystickBase != null)
            {
                var img = m_JoystickBase.GetComponent<Image>();
                if (img != null) { img.sprite = glowRing; img.color = new Color(0.04f, 0.75f, 1f, 0.85f); }
            }
            if (m_JoystickHandle != null)
            {
                var img = m_JoystickHandle.GetComponent<Image>();
                if (img != null) { img.sprite = roundSprite; img.color = new Color(0.2f, 0.95f, 1f, 0.95f); }
            }

            StyleButton(m_BtnFire, roundSprite, new Color(1f, 0.25f, 0.12f, 0.92f), new Color(1f, 0.8f, 0.3f, 1f));
            StyleButton(m_BtnAim, roundSprite, new Color(0.05f, 0.72f, 0.95f, 0.92f), new Color(0.6f, 0.95f, 1f, 1f));
            StyleButton(m_BtnMissile, roundSprite, new Color(1f, 0.65f, 0.05f, 0.92f), new Color(1f, 0.9f, 0.4f, 1f));
            StyleButton(m_BtnInteract, roundSprite, new Color(0.12f, 0.55f, 1f, 0.92f), new Color(0.5f, 0.85f, 1f, 1f));
            StyleButton(m_BtnJump, roundSprite, new Color(0.05f, 0.82f, 0.4f, 0.92f), new Color(0.6f, 1f, 0.8f, 1f));
            StyleButton(m_BtnSprint, roundSprite, new Color(0.85f, 0.12f, 0.95f, 0.92f), new Color(1f, 0.6f, 1f, 1f));
            StyleButton(m_BtnVehicle, roundSprite, new Color(0.45f, 0.2f, 0.95f, 0.92f), new Color(0.8f, 0.6f, 1f, 1f));
            StyleButton(m_BtnFireworks, roundSprite, new Color(1f, 0.15f, 0.55f, 0.92f), new Color(1f, 0.65f, 0.85f, 1f));
        }

        private void StyleButton(Button btn, Sprite sprite, Color normalColor, Color glowColor)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
                img.color = normalColor;
            }

            var outline = btn.GetComponent<Outline>() ?? btn.gameObject.AddComponent<Outline>();
            outline.effectColor = glowColor;
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;
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

            // 1. Đồng bộ di chuyển từ Joystick vào Player controller
            if (m_StarterInputs != null)
            {
                if (m_IsDraggingJoystick && m_InputVector.sqrMagnitude > 0.001f)
                {
                    m_StarterInputs.MoveInput(m_InputVector);
                }
                else if (!m_IsDraggingJoystick && IUHInputHelper.VirtualMoveAxis.sqrMagnitude < 0.001f && Application.isMobilePlatform)
                {
                    m_StarterInputs.MoveInput(Vector2.zero);
                }

                if (m_IsSprinting)
                {
                    m_StarterInputs.SprintInput(true);
                }
            }

            // 2. Đồng bộ xoay camera (Touch Look) mượt mà không khựng, không drift
            if (m_StarterInputs != null)
            {
                if (IsTouchLookActive && m_AccumulatedLookDelta.sqrMagnitude > 0.0001f)
                {
                    m_StarterInputs.LookInput(m_AccumulatedLookDelta);
                    m_AccumulatedLookDelta = Vector2.zero; // Tiêu thụ ngay delta frame này
                }
                else if (!IsTouchLookActive && Application.isMobilePlatform)
                {
                    m_StarterInputs.LookInput(Vector2.zero);
                }
            }

            // 3. Xử lý giữ nút Bắn liên thanh trên Mobile
            if (m_IsFiringHeld)
            {
                IUHInputHelper.VirtualFireHeld = true;
                if (Time.time >= m_NextContinuousFireTime)
                {
                    m_NextContinuousFireTime = Time.time + 0.18f;
                    if (IUHCampusCombatManager.Instance != null)
                    {
                        IUHCampusCombatManager.Instance.TryFireGun();
                    }
                }
            }

            // Phím F8 giả lập bật tắt UI mobile trên Editor/PC
            if (Input.GetKeyDown(KeyCode.F8))
            {
                m_ForceShowOnPC = !m_ForceShowOnPC;
                CheckPlatformVisibility();
            }
        }

        private void SetupButtonListeners()
        {
            // ===== 1. NÚT BẮN PLASMA CHÍNH (FIRE / ATTACK) ===== //
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

            // ===== 2. NÚT NGẮM ADS (AIM / SCOPE) ===== //
            if (m_BtnAim != null)
            {
                m_BtnAim.onClick.RemoveAllListeners();
                m_BtnAim.onClick.AddListener(() => {
                    IUHInputHelper.VirtualAimToggled = !IUHInputHelper.VirtualAimToggled;
                    IUHCombatAudio.PlayLaserShoot();
                    UpdateAimVisualState();
                });
            }

            // ===== 3. NÚT TÊN LỬA CAO XẠ [Q] (ANTI-AIR MISSILE) ===== //
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

            // ===== 4. NÚT TƯƠNG TÁC [E] (Vào Match-3 RPG / Dị tượng) ===== //
            if (m_BtnInteract != null)
            {
                m_BtnInteract.onClick.RemoveAllListeners();
                m_BtnInteract.onClick.AddListener(() => {
                    IUHInputHelper.VirtualInteractPressed = true;
                });
            }

            // ===== 5. NÚT NHẢY [Space] ===== //
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

            // ===== 6. NÚT CHẠY NHANH [Shift / Turbo] ===== //
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

            // ===== 7. NÚT LÊN/XUỐNG XE & KHINH KHÍ CẦU [V] ===== //
            if (m_BtnVehicle != null)
            {
                m_BtnVehicle.onClick.RemoveAllListeners();
                m_BtnVehicle.onClick.AddListener(() => {
                    IUHInputHelper.VirtualVehiclePressed = true;
                });
            }

            // ===== 8. NÚT BẮN PHÁO HOA LỄ HỘI [K/B] ===== //
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
        }

        private void UpdateAimVisualState()
        {
            if (m_BtnAim == null) return;
            var img = m_BtnAim.GetComponent<Image>();
            if (img != null)
            {
                img.color = IUHInputHelper.VirtualAimToggled
                    ? new Color(0.1f, 1f, 0.85f, 1f)
                    : new Color(0.05f, 0.72f, 0.95f, 0.92f);
            }
        }

        private void SetupJoystickEvents()
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
            }
        }

        private void SetupTouchLookEvents()
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
                    // Vuốt ngón tay sang phải (rawDelta.x > 0) -> camera quay sang phải (yaw dương)!
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
