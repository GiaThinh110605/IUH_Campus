using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus
{
    /// <summary>
    /// Interactive Fly & Tour Camera Controller for IUH Campus VR Simulation.
    /// Supports both New Input System and Legacy Input Manager.
    /// Features smooth free-look, WASD fly/walk, sprint, and quick POI teleportation.
    /// </summary>
    public class IUHInteractiveTourController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float m_NormalSpeed = 16.0f;
        [SerializeField] private float m_SprintMultiplier = 2.8f;
        [SerializeField] private float m_SlowMultiplier = 0.35f;
        [SerializeField] private float m_LookSensitivity = 2.2f;
        [SerializeField] private float m_SmoothTransitionTime = 1.2f;

        [System.Serializable]
        public struct PointOfInterest
        {
            public string Name;
            public string Description;
            public Vector3 Position;
            public Vector3 RotationEuler;

            public PointOfInterest(string name, string desc, Vector3 pos, Vector3 rot)
            {
                Name = name;
                Description = desc;
                Position = pos;
                RotationEuler = rot;
            }
        }

        [Header("Tour Points of Interest (POIs)")]
        public PointOfInterest[] m_POIs;

        private float m_Yaw;
        private float m_Pitch;
        private bool m_IsTransitioning = false;
        private Vector3 m_TransitionStartPos;
        private Quaternion m_TransitionStartRot;
        private Vector3 m_TransitionTargetPos;
        private Quaternion m_TransitionTargetRot;
        private float m_TransitionElapsed = 0f;
        private int m_CurrentPOIIndex = 0;
        private bool m_ShowHUD = true;

        private void Awake()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            m_Pitch = euler.x;
            m_Yaw = euler.y;

            // Initialize default iconic POIs based on IUH Campus Master Layout
            if (m_POIs == null || m_POIs.Length == 0)
            {
                m_POIs = new PointOfInterest[]
                {
                    new PointOfInterest(
                        "1. Toàn cảnh Trên Cao (Aerial Campus)",
                        "Góc nhìn tổng thể toàn bộ khuôn viên IUH: Cổng chính, Sân trung tâm, Nhà Hiệu bộ, Cụm Nhà G-I-C.",
                        new Vector3(0.0f, 65.0f, -80.0f),
                        new Vector3(38.0f, 0.0f, 0.0f)
                    ),
                    new PointOfInterest(
                        "2. Cổng Chính & Tượng Đài Bác Hồ",
                        "Lối vào chính từ đường Nguyễn Văn Bảo, cổng chào hoa văn IUH và tượng đài trung tâm.",
                        new Vector3(0.0f, 2.5f, -78.0f),
                        new Vector3(8.0f, 0.0f, 0.0f)
                    ),
                    new PointOfInterest(
                        "3. Cụm Nhà G (KTX Nữ 9 Tầng) & Sân Bãi Xe",
                        "Mặt đứng lưới ban công 9x9, biển hiệu IUH đỉnh mái, bãi đỗ ~150 xe máy và cây bóng mát.",
                        new Vector3(26.0f, 2.0f, -14.0f),
                        new Vector3(14.0f, 12.0f, 0.0f)
                    ),
                    new PointOfInterest(
                        "4. Nhà C (Khối Giảng Đường 5 Tầng)",
                        "Mặt đứng điểm nhấn mint green, lõi thang kính góc, kết nối trực tiếp với Nhà G.",
                        new Vector3(12.0f, 3.0f, 5.0f),
                        new Vector3(12.0f, -40.0f, 0.0f)
                    ),
                    new PointOfInterest(
                        "5. Nhà I (KTX Nam 13 Tầng Cao Nhất Cụm Phải)",
                        "Khối tháp 13 tầng cao 45m với mái dốc đỏ cam đặc trưng và hệ thống bồn nước kỹ thuật.",
                        new Vector3(26.0f, 15.0f, -22.0f),
                        new Vector3(24.0f, 0.0f, 0.0f)
                    ),
                    new PointOfInterest(
                        "6. Nhà Hiệu Bộ (Nhà A) & Sân Thể Thao",
                        "Trung tâm hành chính IUH, sân cầu lông/bóng chuyền giữa sân và hàng cây cổ thụ bóng mát.",
                        new Vector3(0.0f, 3.0f, 22.0f),
                        new Vector3(10.0f, 0.0f, 0.0f)
                    )
                };
            }
        }

        private void Update()
        {
            HandleQuickHotkeys();

            if (m_IsTransitioning)
            {
                m_TransitionElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(m_TransitionElapsed / m_SmoothTransitionTime);
                t = Mathf.SmoothStep(0f, 1f, t);

                transform.position = Vector3.Lerp(m_TransitionStartPos, m_TransitionTargetPos, t);
                transform.rotation = Quaternion.Slerp(m_TransitionStartRot, m_TransitionTargetRot, t);

                if (m_TransitionElapsed >= m_SmoothTransitionTime)
                {
                    m_IsTransitioning = false;
                    Vector3 euler = transform.rotation.eulerAngles;
                    m_Pitch = euler.x;
                    m_Yaw = euler.y;
                }
                return;
            }

            HandleFreeLook();
            HandleMovement();
        }

        private void HandleQuickHotkeys()
        {
            // Toggle HUD with H
            if (IsKeyPressed(KeyCode.H))
            {
                m_ShowHUD = !m_ShowHUD;
            }

            // POI selection keys 1 - 6
            for (int i = 0; i < Mathf.Min(6, m_POIs.Length); i++)
            {
                KeyCode code = KeyCode.Alpha1 + i;
                if (IsKeyPressed(code))
                {
                    TeleportToPOI(i);
                    break;
                }
            }
        }

        private void HandleFreeLook()
        {
            bool isRotating = IsRightMouseDown();

            if (isRotating)
            {
                Vector2 mouseDelta = GetMouseDelta();
                m_Yaw += mouseDelta.x * m_LookSensitivity * 0.15f;
                m_Pitch -= mouseDelta.y * m_LookSensitivity * 0.15f;
                m_Pitch = Mathf.Clamp(m_Pitch, -85.0f, 85.0f);

                transform.rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0.0f);
            }
        }

        private void HandleMovement()
        {
            Vector3 moveInput = Vector3.zero;

            if (IsKeyHeld(KeyCode.W) || IsKeyHeld(KeyCode.UpArrow)) moveInput += transform.forward;
            if (IsKeyHeld(KeyCode.S) || IsKeyHeld(KeyCode.DownArrow)) moveInput -= transform.forward;
            if (IsKeyHeld(KeyCode.A) || IsKeyHeld(KeyCode.LeftArrow)) moveInput -= transform.right;
            if (IsKeyHeld(KeyCode.D) || IsKeyHeld(KeyCode.RightArrow)) moveInput += transform.right;
            if (IsKeyHeld(KeyCode.E) || IsKeyHeld(KeyCode.Space)) moveInput += Vector3.up;
            if (IsKeyHeld(KeyCode.Q) || IsKeyHeld(KeyCode.LeftControl)) moveInput -= Vector3.up;

            if (moveInput.sqrMagnitude > 0.001f)
            {
                float speed = m_NormalSpeed;
                if (IsKeyHeld(KeyCode.LeftShift)) speed *= m_SprintMultiplier;
                if (IsKeyHeld(KeyCode.LeftAlt)) speed *= m_SlowMultiplier;

                transform.position += moveInput.normalized * speed * Time.deltaTime;
                // Minimum height clamp above ground level
                if (transform.position.y < 1.0f)
                {
                    Vector3 p = transform.position;
                    p.y = 1.0f;
                    transform.position = p;
                }
            }
        }

        public void TeleportToPOI(int index)
        {
            if (index < 0 || index >= m_POIs.Length) return;
            m_CurrentPOIIndex = index;

            m_TransitionStartPos = transform.position;
            m_TransitionStartRot = transform.rotation;
            m_TransitionTargetPos = m_POIs[index].Position;
            m_TransitionTargetRot = Quaternion.Euler(m_POIs[index].RotationEuler);
            m_TransitionElapsed = 0f;
            m_IsTransitioning = true;
        }

        #region Input Helper (Supports New & Legacy Input)
        private bool IsKeyPressed(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return false;
            switch (code)
            {
                case KeyCode.Alpha1: return Keyboard.current.digit1Key.wasPressedThisFrame;
                case KeyCode.Alpha2: return Keyboard.current.digit2Key.wasPressedThisFrame;
                case KeyCode.Alpha3: return Keyboard.current.digit3Key.wasPressedThisFrame;
                case KeyCode.Alpha4: return Keyboard.current.digit4Key.wasPressedThisFrame;
                case KeyCode.Alpha5: return Keyboard.current.digit5Key.wasPressedThisFrame;
                case KeyCode.Alpha6: return Keyboard.current.digit6Key.wasPressedThisFrame;
                case KeyCode.H: return Keyboard.current.hKey.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(code);
#endif
        }

        private bool IsKeyHeld(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return false;
            switch (code)
            {
                case KeyCode.W: return Keyboard.current.wKey.isPressed;
                case KeyCode.S: return Keyboard.current.sKey.isPressed;
                case KeyCode.A: return Keyboard.current.aKey.isPressed;
                case KeyCode.D: return Keyboard.current.dKey.isPressed;
                case KeyCode.E: return Keyboard.current.eKey.isPressed;
                case KeyCode.Q: return Keyboard.current.qKey.isPressed;
                case KeyCode.Space: return Keyboard.current.spaceKey.isPressed;
                case KeyCode.LeftShift: return Keyboard.current.leftShiftKey.isPressed;
                case KeyCode.LeftControl: return Keyboard.current.leftCtrlKey.isPressed;
                case KeyCode.LeftAlt: return Keyboard.current.leftAltKey.isPressed;
                case KeyCode.UpArrow: return Keyboard.current.upArrowKey.isPressed;
                case KeyCode.DownArrow: return Keyboard.current.downArrowKey.isPressed;
                case KeyCode.LeftArrow: return Keyboard.current.leftArrowKey.isPressed;
                case KeyCode.RightArrow: return Keyboard.current.rightArrowKey.isPressed;
            }
            return false;
#else
            return Input.GetKey(code);
#endif
        }

        private bool IsRightMouseDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
            return Input.GetMouseButton(1);
#endif
        }

        private Vector2 GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
#else
            return new Vector2(Input.GetAxis("Mouse X") * 10f, Input.GetAxis("Mouse Y") * 10f);
#endif
        }
        #endregion

        #region HUD UI (OnGUI)
        private void OnGUI()
        {
            if (!m_ShowHUD)
            {
                GUI.color = Color.white;
                GUI.Label(new Rect(15, Screen.height - 35, 300, 25), "Nhấn [H] để hiển thị bảng điều khiển Tour");
                return;
            }

            // Top-Left Banner
            GUI.Box(new Rect(15, 15, 390, 82), "");
            GUI.color = new Color(0.1f, 0.4f, 0.85f, 1f);
            GUI.Label(new Rect(25, 20, 370, 24), "★ ĐẠI HỌC CÔNG NGHIỆP TP. HỒ CHÍ MINH (IUH)");
            GUI.color = Color.white;
            GUI.Label(new Rect(25, 42, 370, 22), "<b>Khuôn viên Cơ sở Chính - 3D Virtual Tour</b>");
            GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
            GUI.Label(new Rect(25, 64, 370, 22), "Tham chiếu trực quan: https://vr.iuh.edu.vn/");

            // Current Location Panel
            GUI.color = Color.white;
            GUI.Box(new Rect(15, 105, 390, 80), "");
            if (m_CurrentPOIIndex >= 0 && m_CurrentPOIIndex < m_POIs.Length)
            {
                GUI.color = new Color(1f, 0.85f, 0.2f, 1f);
                GUI.Label(new Rect(25, 110, 370, 22), "📍 " + m_POIs[m_CurrentPOIIndex].Name);
                GUI.color = Color.white;
                GUI.Label(new Rect(25, 132, 370, 48), m_POIs[m_CurrentPOIIndex].Description);
            }

            // Quick Viewpoint Buttons Panel
            GUI.Box(new Rect(15, 192, 390, 160), "");
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            GUI.Label(new Rect(25, 198, 370, 20), "<b>ĐIỂM THAM QUAN NHANH (Phím số 1 - 6):</b>");

            for (int i = 0; i < Mathf.Min(6, m_POIs.Length); i++)
            {
                float y = 224 + i * 21;
                bool isCurrent = (i == m_CurrentPOIIndex);
                string prefix = isCurrent ? "▶ " : "  ";
                GUI.color = isCurrent ? new Color(0.3f, 1f, 0.5f, 1f) : Color.white;
                if (GUI.Button(new Rect(25, y, 370, 20), prefix + m_POIs[i].Name))
                {
                    TeleportToPOI(i);
                }
            }

            // Controls Guide (Bottom Left)
            GUI.color = Color.white;
            GUI.Box(new Rect(15, Screen.height - 130, 390, 115), "");
            GUI.color = new Color(1f, 0.9f, 0.5f, 1f);
            GUI.Label(new Rect(25, Screen.height - 125, 370, 20), "<b>HƯỚNG DẪN ĐIỀU KHIỂN:</b>");
            GUI.color = Color.white;
            GUI.Label(new Rect(25, Screen.height - 105, 370, 20), "• [W / A / S / D]: Bay / Đi bộ di chuyển tự do");
            GUI.Label(new Rect(25, Screen.height - 87, 370, 20), "• [Q / E] hoặc [Space / Ctrl]: Lên / Xuống độ cao");
            GUI.Label(new Rect(25, Screen.height - 69, 370, 20), "• [Giữ Chuột Phải]: Xoay góc nhìn tự do 360°");
            GUI.Label(new Rect(25, Screen.height - 51, 370, 20), "• [Shift]: Bay nhanh | [Phím 1-6]: Chuyển vị trí | [H]: Ẩn HUD");
        }
        #endregion
    }
}
