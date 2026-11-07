using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Combat
{
    /// <summary>
    /// Cho phép nhân vật sinh viên IUH kích hoạt bộ cánh phản lực (Cyber Jetpack / Aero Wings),
    /// bay lơ lửng và cơ động 6 hướng (WASD + Space/Ctrl) trên bầu trời trường IUH để không chiến với quái vật.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ThirdPersonController))]
    public class IUHAerialFlightController : MonoBehaviour
    {
        public static IUHAerialFlightController Instance { get; private set; }

        [Header("Flight Dynamics")]
        [SerializeField] private float m_FlySpeed = 13.0f;
        [SerializeField] private float m_SprintFlySpeed = 24.0f;
        [SerializeField] private float m_VerticalSpeed = 9.5f;
        [SerializeField] private float m_Acceleration = 12.0f;
        [SerializeField] private float m_MaxAltitude = 55.0f;
        [SerializeField] private float m_RotationSmoothTime = 0.12f;

        [Header("Energy & Stamina")]
        [SerializeField] private float m_MaxStamina = 100f;
        [SerializeField] private float m_StaminaDrainPerSec = 2.5f;
        [SerializeField] private float m_SprintDrainMultiplier = 2.0f;
        [SerializeField] private float m_StaminaRegenPerSec = 15.0f;

        [Header("Visual Parts & Wings")]
        [SerializeField] private GameObject m_WingsContainer;
        [SerializeField] private ParticleSystem m_LeftThrusterVFX;
        [SerializeField] private ParticleSystem m_RightThrusterVFX;
        [SerializeField] private TrailRenderer[] m_WingTrails;

        [Header("Audio")]
        [SerializeField] private AudioSource m_FlightAudioSource;

        private CharacterController m_Controller;
        private ThirdPersonController m_TPC;
        private Camera m_MainCamera;
        private StarterAssetsInputs m_Inputs;

        private bool m_IsFlying = false;
        private float m_CurrentStamina;
        private Vector3 m_CurrentVelocity = Vector3.zero;
        private float m_RotationVelocity;
        private float m_LastSpacePressTime = -1f;
        private const float DOUBLE_TAP_WINDOW = 0.35f;

        public bool IsFlying => m_IsFlying;
        public float StaminaNormalized => Mathf.Clamp01(m_CurrentStamina / m_MaxStamina);
        public float CurrentAltitude => transform.position.y;

        public event Action<bool> OnFlightStateChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_Controller = GetComponent<CharacterController>();
            m_TPC = GetComponent<ThirdPersonController>();
            m_Inputs = GetComponent<StarterAssetsInputs>();
            m_CurrentStamina = m_MaxStamina;

            SetupAudioSource();
        }

        private void Start()
        {
            m_MainCamera = Camera.main;
            SetWingsActive(false);
        }

        private void SetupAudioSource()
        {
            if (m_FlightAudioSource == null)
            {
                m_FlightAudioSource = gameObject.AddComponent<AudioSource>();
                m_FlightAudioSource.playOnAwake = false;
                m_FlightAudioSource.loop = true;
                m_FlightAudioSource.spatialBlend = 0.25f;
                m_FlightAudioSource.volume = 0.45f;
            }
        }

        public void SetupWingsReferences(GameObject wings, ParticleSystem leftVFX = null, ParticleSystem rightVFX = null)
        {
            m_WingsContainer = wings;
            m_LeftThrusterVFX = leftVFX;
            m_RightThrusterVFX = rightVFX;
            if (m_WingsContainer != null)
            {
                m_WingTrails = m_WingsContainer.GetComponentsInChildren<TrailRenderer>(true);
            }
            SetWingsActive(m_IsFlying);
        }

        private void Update()
        {
            HandleToggleInput();

            if (m_IsFlying)
            {
                ProcessFlightMotion();
                ProcessStamina();
            }
            else
            {
                // Hồi phục stamina khi ở mặt đất hoặc trên sàn khinh khí cầu
                if (m_CurrentStamina < m_MaxStamina)
                {
                    m_CurrentStamina = Mathf.Min(m_MaxStamina, m_CurrentStamina + m_StaminaRegenPerSec * Time.deltaTime);
                }
            }
        }

        private void HandleToggleInput()
        {
            bool toggleRequested = false;

            // 1. Phím [J] bật/tắt cánh bay
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
            {
                toggleRequested = true;
            }
#endif
            try
            {
                if (!toggleRequested && Input.GetKeyDown(KeyCode.J)) toggleRequested = true;
            }
            catch {}

            if (toggleRequested)
            {
                ToggleFlight();
                return;
            }

            // 2. Nhấn đúp Space (Double-tap Space)
            bool spacePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                spacePressed = true;
            }
#endif
            try
            {
                if (!spacePressed && Input.GetKeyDown(KeyCode.Space)) spacePressed = true;
            }
            catch {}

            if (spacePressed)
            {
                float timeSinceLast = Time.time - m_LastSpacePressTime;
                if (timeSinceLast <= DOUBLE_TAP_WINDOW && timeSinceLast > 0.02f)
                {
                    ToggleFlight();
                    m_LastSpacePressTime = -1f;
                }
                else
                {
                    m_LastSpacePressTime = Time.time;
                }
            }
        }

        public void ToggleFlight()
        {
            if (m_IsFlying)
            {
                StopFlight();
            }
            else
            {
                if (m_CurrentStamina > 10f)
                {
                    StartFlight();
                }
            }
        }

        public void StartFlight()
        {
            if (m_IsFlying) return;
            m_IsFlying = true;

            // Rời khỏi giỏ khinh khí cầu nếu đang ở trên khinh khí cầu
            if (IUHHotAirBalloonController.Instance != null && IUHHotAirBalloonController.Instance.IsPlayerOnBoard)
            {
                IUHHotAirBalloonController.Instance.DisembarkPlayer();
            }

            // Khóa trọng lực và di chuyển mặt đất của TPC, nhưng vẫn giữ CameraRotation hoạt động tự nhiên
            if (m_TPC != null)
            {
                m_TPC.LockMovementAndGravity = true;
            }

            // Đẩy nhân vật bay lên nhẹ
            m_CurrentVelocity = Vector3.up * 3.0f;
            if (m_Controller != null)
            {
                m_Controller.Move(Vector3.up * 1.5f);
            }

            SetWingsActive(true);
            PlayFlightSound(true);

            OnFlightStateChanged?.Invoke(true);
        }

        public void StopFlight()
        {
            if (!m_IsFlying) return;
            m_IsFlying = false;

            if (m_TPC != null)
            {
                m_TPC.LockMovementAndGravity = false;
                m_TPC.ResetMotion();
            }

            SetWingsActive(false);
            PlayFlightSound(false);

            OnFlightStateChanged?.Invoke(false);
        }

        private void ProcessFlightMotion()
        {
            if (m_MainCamera == null) m_MainCamera = Camera.main;

            // 1. Thu nhận input di chuyển WASD / Phím mũi tên (Đa hệ thống)
            Vector2 inputDir = m_Inputs != null ? m_Inputs.move : Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (inputDir.sqrMagnitude < 0.01f && Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) inputDir.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) inputDir.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) inputDir.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) inputDir.x -= 1f;
                inputDir = Vector2.ClampMagnitude(inputDir, 1f);
            }
#endif
            try
            {
                if (inputDir.sqrMagnitude < 0.01f)
                {
                    float h = Input.GetAxisRaw("Horizontal");
                    float v = Input.GetAxisRaw("Vertical");
                    if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
                        inputDir = Vector2.ClampMagnitude(new Vector2(h, v), 1f);
                }
            }
            catch {}

            // 2. Sprint tăng tốc
            bool isSprinting = (m_Inputs != null && m_Inputs.sprint);
#if ENABLE_INPUT_SYSTEM
            if (!isSprinting && Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            {
                isSprinting = true;
            }
#endif
            try
            {
                if (!isSprinting && Input.GetKey(KeyCode.LeftShift)) isSprinting = true;
            }
            catch {}

            float targetSpeed = isSprinting ? m_SprintFlySpeed : m_FlySpeed;

            // 3. Tính toán hướng di chuyển ngang phẳng theo góc nhìn của Camera
            Vector3 camForward = m_MainCamera != null ? m_MainCamera.transform.forward : transform.forward;
            Vector3 camRight = m_MainCamera != null ? m_MainCamera.transform.right : transform.right;

            Vector3 planarForward = new Vector3(camForward.x, 0f, camForward.z).normalized;
            Vector3 planarRight = new Vector3(camRight.x, 0f, camRight.z).normalized;

            Vector3 horizontalMove = Vector3.zero;
            if (inputDir.sqrMagnitude > 0.01f)
            {
                horizontalMove = (planarForward * inputDir.y + planarRight * inputDir.x).normalized * targetSpeed;
            }

            // 4. Bay lên (Space) / Bay xuống (Ctrl hoặc C)
            float verticalMove = 0f;
            bool ascendKey = false;
            bool descendKey = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.isPressed) ascendKey = true;
                if (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.cKey.isPressed) descendKey = true;
            }
#endif
            try
            {
                if (!ascendKey && Input.GetKey(KeyCode.Space)) ascendKey = true;
                if (!descendKey && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))) descendKey = true;
            }
            catch {}

            if (ascendKey) verticalMove += m_VerticalSpeed;
            if (descendKey) verticalMove -= m_VerticalSpeed;

            // Nhấp nhô lơ lửng nhẹ khi đứng yên trên không (hover bob)
            float hoverBob = (inputDir.sqrMagnitude < 0.01f && Mathf.Abs(verticalMove) < 0.01f) ? Mathf.Sin(Time.time * 3f) * 0.18f : 0f;

            Vector3 targetVelocity = new Vector3(horizontalMove.x, verticalMove + hoverBob, horizontalMove.z);
            m_CurrentVelocity = Vector3.Lerp(m_CurrentVelocity, targetVelocity, Time.deltaTime * m_Acceleration);

            // Giới hạn trần bay tối đa
            if (transform.position.y > m_MaxAltitude && m_CurrentVelocity.y > 0f)
            {
                m_CurrentVelocity.y = 0f;
            }

            // Áp dụng di chuyển lên CharacterController
            if (m_Controller != null && m_Controller.enabled)
            {
                m_Controller.Move(m_CurrentVelocity * Time.deltaTime);
            }

            // 5. Xoay thân nhân vật theo hướng bay (CHỈ XOAY KHI CÓ DI CHUYỂN, TUYỆT ĐỐI KHÔNG TỰ XOAY KHI ĐỨNG YÊN ĐỂ TRÁNH VÒNG LẶP XOAY MÁY ẢNH)
            if (horizontalMove.sqrMagnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(horizontalMove.x, horizontalMove.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref m_RotationVelocity, m_RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // 6. Tự động tiếp đất an toàn nếu chạm đất khi đang chủ động bay hạ cánh
            if (m_Controller != null && m_Controller.isGrounded && verticalMove < -0.1f)
            {
                StopFlight();
            }
        }

        private void ProcessStamina()
        {
            bool isSprinting = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) isSprinting = true;
#endif
            try { if (!isSprinting && Input.GetKey(KeyCode.LeftShift)) isSprinting = true; } catch {}

            float drain = m_StaminaDrainPerSec * (isSprinting ? m_SprintDrainMultiplier : 1.0f);
            m_CurrentStamina -= drain * Time.deltaTime;

            if (m_CurrentStamina <= 0f)
            {
                m_CurrentStamina = 0f;
                // Khi cạn năng lượng, tự động tắt cánh bay và đáp xuống đất
                StopFlight();
            }
        }

        public void InstantRecharge()
        {
            m_CurrentStamina = m_MaxStamina;
        }

        private void SetWingsActive(bool active)
        {
            if (m_WingsContainer != null)
            {
                m_WingsContainer.SetActive(active);
            }

            if (m_LeftThrusterVFX != null)
            {
                if (active) m_LeftThrusterVFX.Play(); else m_LeftThrusterVFX.Stop();
            }
            if (m_RightThrusterVFX != null)
            {
                if (active) m_RightThrusterVFX.Play(); else m_RightThrusterVFX.Stop();
            }
        }

        private void PlayFlightSound(bool play)
        {
            if (m_FlightAudioSource == null) return;
            if (play)
            {
                if (!m_FlightAudioSource.isPlaying) m_FlightAudioSource.Play();
            }
            else
            {
                if (m_FlightAudioSource.isPlaying) m_FlightAudioSource.Stop();
            }
        }
    }
}