using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus
{
    /// <summary>
    /// Robust Third-Person & First-Person Character Controller for IUH Campus exploration.
    /// Handles camera-relative WASD movement, sprint, jump, gravity, and view mode toggling.
    /// Supports both New Input System and Legacy Input Manager.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class IUHPlayerController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float m_WalkSpeed = 2.4f;
        [SerializeField] private float m_RunSpeed = 5.0f;
        [SerializeField] private float m_RotationSmoothTime = 0.12f;
        [SerializeField] private float m_SpeedSmoothTime = 0.1f;

        [Header("Jump & Physics")]
        [SerializeField] private float m_JumpHeight = 0.8f;
        [SerializeField] private float m_Gravity = -15.0f;
        [SerializeField] private float m_GroundCheckRadius = 0.28f;
        [SerializeField] private LayerMask m_GroundLayers = ~0;

        [Header("References")]
        [SerializeField] private Transform m_CameraTransform;
        [SerializeField] private IUHCharacterAnimator m_Animator;

        private CharacterController m_Controller;
        private Vector3 m_Velocity;
        private float m_CurrentSpeed;
        private float m_SpeedSmoothVelocity;
        private float m_TargetRotation;
        private float m_RotationVelocity;
        private bool m_IsGrounded;
        private bool m_CursorLocked = true;

        public float CurrentSpeed => m_CurrentSpeed;
        public bool IsGrounded => m_IsGrounded;

        private void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            if (m_Animator == null) m_Animator = GetComponentInChildren<IUHCharacterAnimator>();
            if (m_CameraTransform == null && Camera.main != null) m_CameraTransform = Camera.main.transform;

            SetCursorState(true);
        }

        private void Update()
        {
            HandleCursorToggle();
            CheckGround();
            HandleMovement();
            HandleJumpAndGravity();
        }

        private void CheckGround()
        {
            Vector3 spherePos = transform.position + Vector3.up * (m_Controller.radius * 0.9f);
            m_IsGrounded = Physics.CheckSphere(spherePos, m_GroundCheckRadius, m_GroundLayers, QueryTriggerInteraction.Ignore);

            if (m_IsGrounded && m_Velocity.y < 0)
            {
                m_Velocity.y = -2.0f; // Small grounding downward bias
            }
        }

        private void HandleMovement()
        {
            Vector2 input = GetMovementInput();
            bool isRunning = IsKeyHeld(KeyCode.LeftShift) || IsKeyHeld(KeyCode.RightShift);
            float targetSpeed = (input.sqrMagnitude > 0.01f) ? (isRunning ? m_RunSpeed : m_WalkSpeed) : 0.0f;

            m_CurrentSpeed = Mathf.SmoothDamp(m_CurrentSpeed, targetSpeed, ref m_SpeedSmoothVelocity, m_SpeedSmoothTime);

            if (input.sqrMagnitude > 0.01f)
            {
                // Calculate camera-relative movement direction
                float inputAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
                float camAngle = (m_CameraTransform != null) ? m_CameraTransform.eulerAngles.y : 0.0f;
                m_TargetRotation = inputAngle + camAngle;

                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_TargetRotation, ref m_RotationVelocity, m_RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

                Vector3 moveDir = Quaternion.Euler(0.0f, m_TargetRotation, 0.0f) * Vector3.forward;
                m_Controller.Move(moveDir.normalized * (m_CurrentSpeed * Time.deltaTime));
            }

            // Update procedural animator
            if (m_Animator != null)
            {
                m_Animator.UpdateAnimation(m_CurrentSpeed, m_WalkSpeed, m_RunSpeed, m_IsGrounded);
            }
        }

        private void HandleJumpAndGravity()
        {
            if (m_IsGrounded && IsKeyPressed(KeyCode.Space))
            {
                m_Velocity.y = Mathf.Sqrt(m_JumpHeight * -2.0f * m_Gravity);
            }

            m_Velocity.y += m_Gravity * Time.deltaTime;
            m_Controller.Move(m_Velocity * Time.deltaTime);
        }

        private void HandleCursorToggle()
        {
            if (IsKeyPressed(KeyCode.Escape))
            {
                SetCursorState(!m_CursorLocked);
            }
            else if (!m_CursorLocked && IsMouseDown(0))
            {
                SetCursorState(true);
            }
        }

        public void SetCursorState(bool locked)
        {
            m_CursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            bool prevEnabled = m_Controller.enabled;
            m_Controller.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            m_Velocity = Vector3.zero;
            m_CurrentSpeed = 0f;
            m_Controller.enabled = prevEnabled;
        }

        #region Cross-Platform Input Helpers
        private Vector2 GetMovementInput()
        {
            Vector2 input = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
                return input.normalized;
            }
#endif
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            return new Vector2(h, v).normalized;
        }

        private bool IsKeyPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (key == KeyCode.Space) return kb.spaceKey.wasPressedThisFrame;
                if (key == KeyCode.Escape) return kb.escapeKey.wasPressedThisFrame;
                if (key == KeyCode.V) return kb.vKey.wasPressedThisFrame;
                if (key == KeyCode.C) return kb.cKey.wasPressedThisFrame;
            }
#endif
            return Input.GetKeyDown(key);
        }

        private bool IsKeyHeld(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (key == KeyCode.LeftShift) return kb.leftShiftKey.isPressed;
                if (key == KeyCode.RightShift) return kb.rightShiftKey.isPressed;
            }
#endif
            return Input.GetKey(key);
        }

        private bool IsMouseDown(int button)
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && button == 0) return mouse.leftButton.wasPressedThisFrame;
#endif
            return Input.GetMouseButtonDown(button);
        }
        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 spherePos = transform.position + Vector3.up * 0.3f;
            Gizmos.DrawWireSphere(spherePos, m_GroundCheckRadius);
        }
    }
}
