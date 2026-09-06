using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus
{
    /// <summary>
    /// Smooth Third-Person Orbit Camera with SphereCast collision detection against walls, ceilings, and furniture.
    /// Supports pitch/yaw mouse orbiting, collision avoidance, and seamless first-person toggle.
    /// </summary>
    public class IUHThirdPersonCamera : MonoBehaviour
    {
        [Header("Target & Distances")]
        [SerializeField] private Transform m_Target;
        [SerializeField] private Vector3 m_TargetOffset = new Vector3(0.0f, 1.55f, 0.0f);
        [SerializeField] private float m_DefaultDistance = 2.5f;
        [SerializeField] private float m_MinDistance = 0.4f;
        [SerializeField] private float m_MaxDistance = 3.5f;

        [Header("Mouse Orbit Settings")]
        [SerializeField] private float m_MouseSensitivity = 2.0f;
        [SerializeField] private float m_MinPitch = -28.0f;
        [SerializeField] private float m_MaxPitch = 68.0f;
        [SerializeField] private float m_RotationSmoothTime = 0.06f;

        [Header("Collision Avoidance")]
        [SerializeField] private float m_CollisionRadius = 0.18f;
        [SerializeField] private LayerMask m_CollisionLayers = ~0;
        [SerializeField] private float m_CollisionBuffer = 0.12f;
        [SerializeField] private float m_DistanceSmoothTime = 0.15f;

        [Header("View Mode")]
        [SerializeField] private bool m_IsFirstPerson = false;
        [SerializeField] private bool m_ShowHUD = true;

        private float m_Yaw = 0f;
        private float m_Pitch = 12f;
        private float m_CurrentDistance;
        private float m_TargetDistance;
        private float m_DistanceSmoothVelocity;
        private Vector3 m_CurrentRotation;
        private Vector3 m_RotationSmoothVelocity;

        public bool IsFirstPerson => m_IsFirstPerson;

        private void Awake()
        {
            m_CurrentDistance = m_DefaultDistance;
            m_TargetDistance = m_DefaultDistance;
            Vector3 euler = transform.eulerAngles;
            m_Yaw = euler.y;
            m_Pitch = euler.x;
        }

        public void SetTarget(Transform target)
        {
            m_Target = target;
        }

        private void LateUpdate()
        {
            if (m_Target == null) return;

            HandleViewModeToggle();
            HandleMouseLook();
            HandleCameraPositionAndCollision();
        }

        private void HandleViewModeToggle()
        {
            if (IsKeyPressed(KeyCode.V) || IsKeyPressed(KeyCode.C))
            {
                m_IsFirstPerson = !m_IsFirstPerson;
            }
            if (IsKeyPressed(KeyCode.H))
            {
                m_ShowHUD = !m_ShowHUD;
            }
        }

        private void HandleMouseLook()
        {
            Vector2 mouseDelta = GetMouseDelta();
            m_Yaw += mouseDelta.x * m_MouseSensitivity;
            m_Pitch -= mouseDelta.y * m_MouseSensitivity;
            m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

            Vector3 targetRot = new Vector3(m_Pitch, m_Yaw, 0.0f);
            m_CurrentRotation = Vector3.SmoothDamp(m_CurrentRotation, targetRot, ref m_RotationSmoothVelocity, m_RotationSmoothTime);
            transform.rotation = Quaternion.Euler(m_CurrentRotation);
        }

        private void HandleCameraPositionAndCollision()
        {
            Vector3 targetCenter = m_Target.position + m_TargetOffset;

            if (m_IsFirstPerson)
            {
                // First person eye placement
                transform.position = targetCenter;
                return;
            }

            // Direction from target center pointing backwards towards camera
            Vector3 backDir = -transform.forward;
            float desiredDist = m_DefaultDistance;

            // SphereCast from TargetCenter backwards to detect walls, ceilings, and furniture
            RaycastHit hit;
            if (Physics.SphereCast(targetCenter, m_CollisionRadius, backDir, out hit, m_DefaultDistance, m_CollisionLayers, QueryTriggerInteraction.Ignore))
            {
                // Pull camera forward in front of obstacle
                desiredDist = Mathf.Clamp(hit.distance - m_CollisionBuffer, m_MinDistance, m_DefaultDistance);
            }

            m_TargetDistance = desiredDist;
            m_CurrentDistance = Mathf.SmoothDamp(m_CurrentDistance, m_TargetDistance, ref m_DistanceSmoothVelocity, m_DistanceSmoothTime);

            transform.position = targetCenter + backDir * m_CurrentDistance;
        }

        #region Input Helpers
        private Vector2 GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                return mouse.delta.ReadValue() * 0.12f;
            }
#endif
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            return new Vector2(mx, my);
        }

        private bool IsKeyPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (key == KeyCode.V) return kb.vKey.wasPressedThisFrame;
                if (key == KeyCode.C) return kb.cKey.wasPressedThisFrame;
                if (key == KeyCode.H) return kb.hKey.wasPressedThisFrame;
            }
#endif
            return Input.GetKeyDown(key);
        }
        #endregion

        private void OnGUI()
        {
            if (!m_ShowHUD) return;

            // Simple clean Campus HUD
            GUI.Box(new Rect(15, 15, 340, 85), "");
            GUI.color = new Color(0.12f, 0.45f, 0.90f, 1f);
            GUI.Label(new Rect(25, 20, 320, 24), "★ IUH CAMPUS - THAM QUAN NỘI THẤT");
            GUI.color = Color.white;
            GUI.Label(new Rect(25, 42, 320, 22), "<b>Chế độ: " + (m_IsFirstPerson ? "Góc nhìn thứ 1 (FPP)" : "Góc nhìn thứ 3 (TPP)") + "</b>");
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(25, 64, 320, 22), "[W,A,S,D] Đi lại | [Shift] Chạy | [V] Đổi góc nhìn");

            // Small bottom control bar
            GUI.Box(new Rect(15, Screen.height - 40, 420, 28), "");
            GUI.color = Color.yellow;
            GUI.Label(new Rect(25, Screen.height - 36, 400, 22), "Chuột: Quay camera 360° | Space: Nhảy | ESC: Thoát chuột | H: Ẩn HUD");
        }
    }
}
