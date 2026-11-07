using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

namespace IUHCampus.Combat
{
    /// <summary>
    /// Cho phép nhân vật sinh viên IUH kích hoạt bộ cánh phản lực (Cyber Jetpack / Aero Wings),
    /// bay lơ lửng 360 độ trên bầu trời trường IUH để không chiến với các quái vật trên không.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ThirdPersonController))]
    public class IUHAerialFlightController : MonoBehaviour
    {
        public static IUHAerialFlightController Instance { get; private set; }

        [Header("Flight Dynamics")]
        [SerializeField] private float m_FlySpeed = 12.0f;
        [SerializeField] private float m_SprintFlySpeed = 22.0f;
        [SerializeField] private float m_VerticalSpeed = 9.0f;
        [SerializeField] private float m_Acceleration = 10.0f;
        [SerializeField] private float m_MaxAltitude = 55.0f;
        [SerializeField] private float m_MinGroundClearance = 0.8f;

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
            // Kiểm tra phím tắt kích hoạt bay
            HandleToggleInput();

            if (m_IsFlying)
            {
                ProcessFlightMotion();
                ProcessStamina();
            }
            else
            {
                // Hồi phục stamina khi ở mặt đất
                if (m_CurrentStamina < m_MaxStamina)
                {
                    m_CurrentStamina = Mathf.Min(m_MaxStamina, m_CurrentStamina + m_StaminaRegenPerSec * Time.deltaTime);
                }
            }
        }

        private void HandleToggleInput()
        {
            // Kích hoạt bằng phím [J] (Jetpack toggle) hoặc Space x 2
            if (Input.GetKeyDown(KeyCode.J))
            {
                ToggleFlight();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                float timeSinceLast = Time.time - m_LastSpacePressTime;
                if (timeSinceLast <= DOUBLE_TAP_WINDOW)
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

            // Vô hiệu hóa điều khiển trọng lực mặt đất của TPC để flight controller kiểm soát
            if (m_TPC != null)
            {
                m_TPC.enabled = false;
            }

            // Nhấc bổng nhân vật nhẹ lên khỏi mặt đất nếu đang đứng sát sàn
            if (m_Controller != null)
            {
                m_Controller.Move(Vector3.up * 2.0f);
            }

            SetWingsActive(true);
            PlayFlightSound(true);

            OnFlightStateChanged?.Invoke(true);
        }

        public void StopFlight()
        {
            if (!m_IsFlying) return;
            m_IsFlying = false;

            // Trả lại quyền điều khiển di chuyển cho ThirdPersonController
            if (m_TPC != null)
            {
                m_TPC.enabled = true;
                m_TPC.ResetMotion();
            }

            SetWingsActive(false);
            PlayFlightSound(false);

            OnFlightStateChanged?.Invoke(false);
        }

        private void ProcessFlightMotion()
        {
            if (m_MainCamera == null) m_MainCamera = Camera.main;

            // Đọc hướng di chuyển từ phím WASD / Input
            Vector2 inputDir = Vector2.zero;
            if (m_Inputs != null) inputDir = m_Inputs.move;
            if (inputDir.sqrMagnitude < 0.01f)
            {
                inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }

            bool isSprinting = (m_Inputs != null && m_Inputs.sprint) || Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isSprinting ? m_SprintFlySpeed : m_FlySpeed;

            // Tính vector chuyển động ngang và dọc theo hướng Camera
            Vector3 camForward = m_MainCamera != null ? m_MainCamera.transform.forward : transform.forward;
            Vector3 camRight = m_MainCamera != null ? m_MainCamera.transform.right : transform.right;

            // Giữ vector ngang
            Vector3 forwardPlanar = new Vector3(camForward.x, 0f, camForward.z).normalized;
            Vector3 rightPlanar = new Vector3(camRight.x, 0f, camRight.z).normalized;

            Vector3 horizontalMove = (forwardPlanar * inputDir.y + rightPlanar * inputDir.x).normalized * targetSpeed;

            // Tính vận tốc lên/xuống (Space = bay lên, Ctrl/C = bay xuống)
            float verticalMove = 0f;
            if (Input.GetKey(KeyCode.Space))
            {
                verticalMove += m_VerticalSpeed;
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
            {
                verticalMove -= m_VerticalSpeed;
            }

            // Giữ độ cao lơ lửng nếu không nhấn lên/xuống (hover nhẹ)
            float hoverBob = Mathf.Sin(Time.time * 3.5f) * 0.15f;
            Vector3 targetVelocity = new Vector3(horizontalMove.x, verticalMove + hoverBob, horizontalMove.z);

            m_CurrentVelocity = Vector3.Lerp(m_CurrentVelocity, targetVelocity, Time.deltaTime * m_Acceleration);

            // Kiểm tra trần bay tối đa
            if (transform.position.y > m_MaxAltitude && m_CurrentVelocity.y > 0f)
            {
                m_CurrentVelocity.y = 0f;
            }

            // Di chuyển nhân vật
            m_Controller.Move(m_CurrentVelocity * Time.deltaTime);

            // Xoay nhân vật theo hướng camera hoặc hướng di chuyển
            if (horizontalMove.sqrMagnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(horizontalMove.x, horizontalMove.z) * Mathf.Rad2Deg;
                float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * 12f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else if (m_MainCamera != null)
            {
                // Khi đứng yên xoay dần theo camera
                float camYaw = m_MainCamera.transform.eulerAngles.y;
                float angle = Mathf.LerpAngle(transform.eulerAngles.y, camYaw, Time.deltaTime * 8f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // Nếu chạm sát mặt đất trong lúc đang hạ cánh -> tự động tiếp đất
            if (m_Controller.isGrounded && verticalMove < -0.1f)
            {
                StopFlight();
            }
        }

        private void ProcessStamina()
        {
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float drain = m_StaminaDrainPerSec * (isSprinting ? m_SprintDrainMultiplier : 1.0f);
            m_CurrentStamina -= drain * Time.deltaTime;

            if (m_CurrentStamina <= 0f)
            {
                m_CurrentStamina = 0f;
                // Khi hết pin cánh phản lực, bắt đầu rơi từ từ xuống đất
                StopFlight();
            }
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

        /// <summary>Nạp đầy 100% Stamina ngay tức thì (gọi khi vào giỏ khinh khí cầu).</summary>
        public void InstantRecharge()
        {
            m_CurrentStamina = m_MaxStamina;
        }
    }
}

