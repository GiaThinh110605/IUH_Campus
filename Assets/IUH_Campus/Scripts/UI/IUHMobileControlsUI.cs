using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IUHCampus.Core;

namespace IUHCampus.UI
{
    /// <summary>
    /// Virtual On-Screen Mobile Controls (Giao diện mobile) as shown in Design Image 1.
    /// Provides smooth touch/mouse Virtual Joystick on the bottom-left,
    /// and action buttons (Vehicle Mount, Interaction, Turbo/Action) on the bottom-right.
    /// Feeds directly into IUHInputHelper for seamless player & motorbike control.
    /// </summary>
    public class IUHMobileControlsUI : MonoBehaviour
    {
        [Header("Joystick References")]
        [SerializeField] private RectTransform m_JoystickBase;
        [SerializeField] private RectTransform m_JoystickHandle;
        [SerializeField] private float m_JoystickRange = 55f;

        [Header("Buttons")]
        [SerializeField] private Button m_BtnVehicle;
        [SerializeField] private Button m_BtnInteract;
        [SerializeField] private Button m_BtnAction;

        private Vector2 m_InputVector = Vector2.zero;
        private bool m_IsDraggingJoystick = false;

        public void Setup(RectTransform joystickBase, RectTransform joystickHandle, Button btnVehicle, Button btnInteract, Button btnAction)
        {
            m_JoystickBase = joystickBase;
            m_JoystickHandle = joystickHandle;
            m_BtnVehicle = btnVehicle;
            m_BtnInteract = btnInteract;
            m_BtnAction = btnAction;
            SetupButtonListeners();
            SetupJoystickEvents();
        }

        [Header("Platform Visibility")]
        [SerializeField] private bool m_ForceShowOnPC = false;

        private void Awake()
        {
            AutoWireReferences();
            CheckPlatformVisibility();
        }

        private void AutoWireReferences()
        {
            if (m_JoystickBase == null) m_JoystickBase = transform.Find("JoystickBase")?.GetComponent<RectTransform>();
            if (m_JoystickHandle == null && m_JoystickBase != null) m_JoystickHandle = m_JoystickBase.Find("JoystickHandle")?.GetComponent<RectTransform>();
            if (m_BtnVehicle == null) m_BtnVehicle = transform.Find("BtnVehicle")?.GetComponent<Button>();
            if (m_BtnInteract == null) m_BtnInteract = transform.Find("BtnInteract")?.GetComponent<Button>();
            if (m_BtnAction == null) m_BtnAction = transform.Find("BtnAction")?.GetComponent<Button>();
        }

        private void Start()
        {
            AutoWireReferences();
            CheckPlatformVisibility();
            SetupButtonListeners();
            SetupJoystickEvents();
        }

        public void CheckPlatformVisibility()
        {
            // On PC (Windows, Mac, Linux, Unity Editor), mobile touch controls should NOT be displayed!
            // Only show when running on genuine mobile devices (Android / iOS) or if specifically force-enabled.
            bool shouldShow = m_ForceShowOnPC || Application.isMobilePlatform;
            gameObject.SetActive(shouldShow);
        }

        private void Update()
        {
            // Allow developers to toggle mobile touch controls on PC using F8 for quick testing
            if (Input.GetKeyDown(KeyCode.F8))
            {
                m_ForceShowOnPC = !m_ForceShowOnPC;
                CheckPlatformVisibility();
                Debug.Log($"[IUH Mobile Controls] Chế độ giả lập Mobile trên PC: {(m_ForceShowOnPC ? "BẬT" : "TẮT")}");
            }
        }

        private void SetupButtonListeners()
        {
            if (m_BtnVehicle != null)
            {
                m_BtnVehicle.onClick.RemoveAllListeners();
                m_BtnVehicle.onClick.AddListener(() =>
                {
                    IUHInputHelper.VirtualVehiclePressed = true;
                });
            }

            if (m_BtnInteract != null)
            {
                m_BtnInteract.onClick.RemoveAllListeners();
                m_BtnInteract.onClick.AddListener(() =>
                {
                    IUHInputHelper.VirtualInteractPressed = true;
                });
            }

            // Turbo / Action Button holding
            if (m_BtnAction != null)
            {
                var trigger = m_BtnAction.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = m_BtnAction.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();

                // PointerDown
                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener((e) => { IUHInputHelper.VirtualTurboHeld = true; });
                trigger.triggers.Add(down);

                // PointerUp
                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener((e) => { IUHInputHelper.VirtualTurboHeld = false; });
                trigger.triggers.Add(up);
            }
        }

        private void SetupJoystickEvents()
        {
            if (m_JoystickBase == null) return;

            var trigger = m_JoystickBase.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = m_JoystickBase.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            // PointerDown
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((data) => OnJoystickDown((PointerEventData)data));
            trigger.triggers.Add(down);

            // Drag
            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener((data) => OnJoystickDrag((PointerEventData)data));
            trigger.triggers.Add(drag);

            // PointerUp
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
        }
    }
}
