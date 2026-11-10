using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IUHCampus.Core;
using IUHCampus.Combat;
using StarterAssets;

namespace IUHCampus.UI
{
    /// <summary>
    /// Điều khiển toàn diện giao diện cảm ứng trên điện thoại ở chế độ màn hình ngang (Landscape Mode):
    /// - Tự động khóa màn hình xoay ngang (Landscape Left / Right).
    /// - Cần xoay Joystick ảo góc trái điều khiển di chuyển nhân vật (W, A, S, D).
    /// - Vùng cảm ứng nửa phải màn hình vuốt xoay camera 360 độ (Touch Look).
    /// - Cụm nút hành động góc dưới phải:
    ///   + [E] Tương tác dị tượng / Vào trận Match-3 RPG / Nói chuyện.
    ///   + [Space] Nhảy vượt địa hình.
    ///   + [Shift] Chạy nhanh / Tăng tốc (Sprint / Turbo).
    ///   + [V] Lên/xuống xe máy & khinh khí cầu.
    ///   + [K/B] Kỹ năng bắn pháo hoa lễ hội rực sáng khuôn viên IUH.
    /// </summary>
    public class IUHMobileControlsUI : MonoBehaviour
    {
        public static IUHMobileControlsUI Instance { get; private set; }

        [Header("Orientation Config")]
        [SerializeField] private bool m_ForceLandscape = true;

        [Header("Joystick References")]
        [SerializeField] private RectTransform m_JoystickBase;
        [SerializeField] private RectTransform m_JoystickHandle;
        [SerializeField] private float m_JoystickRange = 65f;

        [Header("Touch Look Zone")]
        [SerializeField] private RectTransform m_TouchLookZone;
        [SerializeField] private float m_LookSensitivity = 0.12f;

        [Header("Action Buttons")]
        [SerializeField] private Button m_BtnInteract;
        [SerializeField] private Button m_BtnJump;
        [SerializeField] private Button m_BtnSprint;
        [SerializeField] private Button m_BtnVehicle;
        [SerializeField] private Button m_BtnFireworks;

        [Header("Platform Visibility")]
        [SerializeField] private bool m_ForceShowOnPC = false;

        private Vector2 m_InputVector = Vector2.zero;
        private bool m_IsDraggingJoystick = false;
        private bool m_IsSprinting = false;
        private StarterAssetsInputs m_StarterInputs;

        public void Setup(RectTransform joystickBase, RectTransform joystickHandle, Button btnVehicle, Button btnInteract, Button btnAction)
        {
            m_JoystickBase = joystickBase;
            m_JoystickHandle = joystickHandle;
            m_BtnVehicle = btnVehicle;
            m_BtnInteract = btnInteract;
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

        private void AutoWireReferences()
        {
            if (m_JoystickBase == null) m_JoystickBase = transform.Find("JoystickBase")?.GetComponent<RectTransform>();
            if (m_JoystickHandle == null && m_JoystickBase != null) m_JoystickHandle = m_JoystickBase.Find("JoystickHandle")?.GetComponent<RectTransform>();
            if (m_TouchLookZone == null) m_TouchLookZone = transform.Find("TouchLookZone")?.GetComponent<RectTransform>();

            if (m_BtnInteract == null) m_BtnInteract = transform.Find("BtnInteract")?.GetComponent<Button>();
            if (m_BtnJump == null) m_BtnJump = transform.Find("BtnJump")?.GetComponent<Button>();
            if (m_BtnSprint == null) m_BtnSprint = transform.Find("BtnSprint")?.GetComponent<Button>();
            if (m_BtnVehicle == null) m_BtnVehicle = transform.Find("BtnVehicle")?.GetComponent<Button>();
            if (m_BtnFireworks == null) m_BtnFireworks = transform.Find("BtnFireworks")?.GetComponent<Button>();
        }

        public void CheckPlatformVisibility()
        {
            bool shouldShow = m_ForceShowOnPC || Application.isMobilePlatform;
            gameObject.SetActive(shouldShow);
        }

        private void Update()
        {
            if (m_StarterInputs == null)
            {
                var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
                if (student != null) m_StarterInputs = student.GetComponent<StarterAssetsInputs>();
            }

            // Đồng bộ di chuyển từ Joystick vào Player controller
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

            // Phím F8 giả lập bật tắt UI mobile trên PC
            if (Input.GetKeyDown(KeyCode.F8))
            {
                m_ForceShowOnPC = !m_ForceShowOnPC;
                CheckPlatformVisibility();
            }
        }

        private void SetupButtonListeners()
        {
            // 1. Nút Tương tác [E] (Vào Match-3 / Dị tượng / Lên khinh khí cầu)
            if (m_BtnInteract != null)
            {
                m_BtnInteract.onClick.RemoveAllListeners();
                m_BtnInteract.onClick.AddListener(() =>
                {
                    IUHInputHelper.VirtualInteractPressed = true;
                });
            }

            // 2. Nút Nhảy [Space]
            if (m_BtnJump != null)
            {
                m_BtnJump.onClick.RemoveAllListeners();
                m_BtnJump.onClick.AddListener(() =>
                {
                    if (m_StarterInputs != null)
                    {
                        m_StarterInputs.JumpInput(true);
                    }
                });
            }

            // 3. Nút Chạy nhanh / Tăng tốc [Shift]
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

            // 4. Nút Lên / Xuống Xe & Khinh khí cầu [V]
            if (m_BtnVehicle != null)
            {
                m_BtnVehicle.onClick.RemoveAllListeners();
                m_BtnVehicle.onClick.AddListener(() =>
                {
                    IUHInputHelper.VirtualVehiclePressed = true;
                });
            }

            // 5. Nút Bắn Pháo Hoa Lễ Hội [K/B]
            if (m_BtnFireworks != null)
            {
                m_BtnFireworks.onClick.RemoveAllListeners();
                m_BtnFireworks.onClick.AddListener(() =>
                {
                    if (IUHCampusFireworksManager.Instance != null)
                    {
                        IUHCampusFireworksManager.Instance.TriggerPlayerFirework();
                    }
                });
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
            OnJoystickDrag(eventData);
        }

        private void OnJoystickDrag(PointerEventData eventData)
        {
            if (!m_IsDraggingJoystick || m_JoystickBase == null) return;

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
            m_IsDraggingJoystick = false;
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

            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener((data) => {
                var ped = (PointerEventData)data;
                if (m_StarterInputs != null)
                {
                    Vector2 lookDelta = ped.delta * m_LookSensitivity;
                    m_StarterInputs.LookInput(lookDelta);
                }
            });
            trigger.triggers.Add(drag);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((data) => {
                if (m_StarterInputs != null)
                {
                    m_StarterInputs.LookInput(Vector2.zero);
                }
            });
            trigger.triggers.Add(up);
        }
    }
}
