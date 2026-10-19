using UnityEngine;
using IUHCampus.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Arcade Motorbike Controller for Grab Campus ("Chuyến xe tâm linh").
    /// Provides rock-solid, ultra-smooth arcade vehicle handling, bike leaning/banking,
    /// dynamic engine pitch audio, horn, headlight, and seamless mount/dismount flow.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class IUHGrabMotorbike : MonoBehaviour
    {
        [Header("Movement & Performance")]
        [SerializeField] private float m_MaxForwardSpeed = 16.0f; // ~58 km/h
        [SerializeField] private float m_TurboSpeed = 22.0f;      // ~79 km/h
        [SerializeField] private float m_ReverseSpeed = 4.5f;
        [SerializeField] private float m_Acceleration = 12.0f;
        [SerializeField] private float m_BrakingDeceleration = 20.0f;
        [SerializeField] private float m_CoastDeceleration = 6.0f;
        [SerializeField] private float m_TurnSpeed = 85.0f;
        [SerializeField] private float m_MaxBankAngle = 22.0f;
        [SerializeField] private float m_BankSpeed = 8.0f;

        [Header("Visual References")]
        [SerializeField] private Transform m_VisualRoot;
        [SerializeField] private Transform m_FrontSteerFork;
        [SerializeField] private Transform m_WheelFront;
        [SerializeField] private Transform m_WheelRear;
        [SerializeField] private GameObject m_RiderAvatar;
        [SerializeField] private Light m_Headlight;
        [SerializeField] private MeshRenderer m_TailLightRenderer;

        [Header("Camera")]
        [SerializeField] private Camera m_VehicleCamera;
        [SerializeField] private Vector3 m_CameraOffset = new Vector3(0f, 2.2f, -4.6f);
        [SerializeField] private float m_CameraSmooth = 12.0f;

        [Header("Mount & State")]
        [SerializeField] private bool m_IsMounted = false;
        [SerializeField] private KeyCode m_MountKey = KeyCode.E;
        [SerializeField] private KeyCode m_HornKey = KeyCode.H;
        [SerializeField] private KeyCode m_TurboKey = KeyCode.LeftShift;

        // Runtime state
        private CharacterController m_CharController;
        private AudioSource m_EngineAudio;
        private AudioSource m_SfxAudio;
        private float m_CurrentSpeed = 0f;
        private float m_CurrentBankAngle = 0f;
        private float m_SteerInput = 0f;
        private float m_ThrottleInput = 0f;
        private bool m_IsBraking = false;
        private bool m_IsTurbo = false;
        private float m_GhostSlowdownTimer = 0f;

        private GameObject m_PlayerOriginal;
        private Camera m_PreviousMainCamera;

        public bool IsMounted => m_IsMounted;
        public float CurrentSpeedKmH => Mathf.Abs(m_CurrentSpeed) * 3.6f;
        public float SpeedRatio => Mathf.Clamp01(Mathf.Abs(m_CurrentSpeed) / m_MaxForwardSpeed);

        private void Awake()
        {
            m_CharController = GetComponent<CharacterController>();
            if (m_CharController == null)
            {
                m_CharController = gameObject.AddComponent<CharacterController>();
                m_CharController.height = 1.3f;
                m_CharController.radius = 0.45f;
                m_CharController.center = new Vector3(0f, 0.65f, 0f);
            }

            SetupAudio();
            SetupCamera();

            if (m_RiderAvatar != null)
            {
                m_RiderAvatar.SetActive(m_IsMounted);
            }
        }

        private void SetupAudio()
        {
            m_EngineAudio = gameObject.AddComponent<AudioSource>();
            m_EngineAudio.clip = IUHSoundSynthesizer.GetEngineSound();
            m_EngineAudio.loop = true;
            m_EngineAudio.playOnAwake = false;
            m_EngineAudio.volume = 0.65f;
            m_EngineAudio.spatialBlend = 0.6f;

            m_SfxAudio = gameObject.AddComponent<AudioSource>();
            m_SfxAudio.playOnAwake = false;
            m_SfxAudio.spatialBlend = 0.5f;
        }

        private void SetupCamera()
        {
            if (m_VehicleCamera == null)
            {
                var camGo = new GameObject("GrabBike_Camera");
                camGo.transform.SetParent(null);
                m_VehicleCamera = camGo.AddComponent<Camera>();
                m_VehicleCamera.fieldOfView = 65f;
                m_VehicleCamera.nearClipPlane = 0.1f;
                camGo.AddComponent<AudioListener>();
                m_VehicleCamera.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            m_PlayerOriginal = GameObject.FindGameObjectWithTag("Player");
            if (m_PlayerOriginal == null)
            {
                m_PlayerOriginal = GameObject.Find("PlayerArmature");
            }
        }

        private void Update()
        {
            HandleMountInputs();

            if (!m_IsMounted)
            {
                ApplyGravityWhenIdle();
                return;
            }

            ReadDrivingInputs();
            UpdatePhysicsMovement();
            UpdateVisualsAndLeaning();
            UpdateAudioPitch();
            UpdateVehicleCamera();
        }

        private void HandleMountInputs()
        {
            // If near player, allow mounting
            if (!m_IsMounted && m_PlayerOriginal != null)
            {
                float dist = Vector3.Distance(transform.position, m_PlayerOriginal.transform.position);
                if (dist < 2.8f)
                {
                    if (Input.GetKeyDown(m_MountKey))
                    {
                        MountVehicle(m_PlayerOriginal);
                    }
                }
            }
            else if (m_IsMounted)
            {
                // Dismount only when vehicle is nearly stopped
                if (Input.GetKeyDown(m_MountKey) && Mathf.Abs(m_CurrentSpeed) < 1.0f)
                {
                    // Don't dismount if delivery mission is strictly on-bike unless user explicitly presses
                    DismountVehicle();
                }

                // Horn
                if (Input.GetKeyDown(m_HornKey))
                {
                    m_SfxAudio.PlayOneShot(IUHSoundSynthesizer.GetHornSound(), 1.0f);
                }
            }
        }

        public void MountVehicle(GameObject player)
        {
            m_PlayerOriginal = player;
            m_IsMounted = true;

            if (m_PlayerOriginal != null)
            {
                m_PlayerOriginal.SetActive(false);
            }

            if (m_RiderAvatar != null)
            {
                m_RiderAvatar.SetActive(true);
            }

            if (m_Headlight != null)
            {
                m_Headlight.enabled = true;
            }

            // Switch to vehicle camera
            SwitchToVehicleCamera(true);

            // Start engine sound
            if (m_EngineAudio != null && !m_EngineAudio.isPlaying)
            {
                m_EngineAudio.Play();
            }

            Debug.Log("[IUH Grab Motorbike] Người chơi đã lên xe máy Grab!");
        }

        public void DismountVehicle()
        {
            m_IsMounted = false;

            if (m_RiderAvatar != null)
            {
                m_RiderAvatar.SetActive(false);
            }

            if (m_EngineAudio != null && m_EngineAudio.isPlaying)
            {
                m_EngineAudio.Stop();
            }

            // Switch camera back
            SwitchToVehicleCamera(false);

            if (m_PlayerOriginal != null)
            {
                // Position player slightly to the left of the bike
                Vector3 dismountPos = transform.position - transform.right * 1.2f + Vector3.up * 0.1f;
                m_PlayerOriginal.transform.position = dismountPos;
                m_PlayerOriginal.transform.rotation = transform.rotation;
                m_PlayerOriginal.SetActive(true);
            }

            Debug.Log("[IUH Grab Motorbike] Người chơi đã xuống xe máy!");
        }

        private void SwitchToVehicleCamera(bool useVehicleCam)
        {
            if (useVehicleCam)
            {
                var mainCam = Camera.main;
                if (mainCam != null && mainCam != m_VehicleCamera)
                {
                    m_PreviousMainCamera = mainCam;
                    m_PreviousMainCamera.gameObject.SetActive(false);
                }

                if (m_VehicleCamera != null)
                {
                    m_VehicleCamera.gameObject.SetActive(true);
                    m_VehicleCamera.tag = "MainCamera";
                    // Position immediately behind
                    m_VehicleCamera.transform.position = transform.position + transform.TransformDirection(m_CameraOffset);
                    m_VehicleCamera.transform.LookAt(transform.position + Vector3.up * 1.2f);
                }
            }
            else
            {
                if (m_VehicleCamera != null)
                {
                    m_VehicleCamera.gameObject.SetActive(false);
                    m_VehicleCamera.tag = "Untagged";
                }

                if (m_PreviousMainCamera != null)
                {
                    m_PreviousMainCamera.gameObject.SetActive(true);
                    m_PreviousMainCamera.tag = "MainCamera";
                }
            }
        }

        private void ReadDrivingInputs()
        {
            // Vertical axis (W/S, Up/Down)
            m_ThrottleInput = Input.GetAxisRaw("Vertical");

            // Horizontal axis (A/D, Left/Right)
            m_SteerInput = Input.GetAxisRaw("Horizontal");

            // Braking (Space)
            m_IsBraking = Input.GetKey(KeyCode.Space);

            // Turbo (Shift)
            m_IsTurbo = Input.GetKey(m_TurboKey);
        }

        private void UpdatePhysicsMovement()
        {
            if (m_GhostSlowdownTimer > 0f)
            {
                m_GhostSlowdownTimer -= Time.deltaTime;
                m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, 0f, 15f * Time.deltaTime);
            }

            float targetMaxSpeed = m_IsTurbo ? m_TurboSpeed : m_MaxForwardSpeed;

            // Acceleration / Deceleration
            if (m_IsBraking)
            {
                m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, 0f, m_BrakingDeceleration * Time.deltaTime);
            }
            else if (m_ThrottleInput > 0f)
            {
                m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, targetMaxSpeed, m_Acceleration * Time.deltaTime);
            }
            else if (m_ThrottleInput < 0f)
            {
                m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, -m_ReverseSpeed, m_Acceleration * 0.7f * Time.deltaTime);
            }
            else
            {
                // Coast to stop
                m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, 0f, m_CoastDeceleration * Time.deltaTime);
            }

            // Steering: turn rate depends on speed and direction
            if (Mathf.Abs(m_CurrentSpeed) > 0.1f)
            {
                float dirMultiplier = Mathf.Sign(m_CurrentSpeed);
                // Reduce steering sharpness at ultra high speeds for stability
                float speedFactor = Mathf.Clamp01(Mathf.Abs(m_CurrentSpeed) / 12f);
                float effectiveTurnRate = Mathf.Lerp(m_TurnSpeed, m_TurnSpeed * 0.65f, speedFactor);

                float yaw = m_SteerInput * effectiveTurnRate * dirMultiplier * Time.deltaTime;
                transform.Rotate(0f, yaw, 0f);
            }

            // Forward movement vector
            Vector3 move = transform.forward * m_CurrentSpeed;

            // Simple gravity check to stick to ground
            move.y = -9.81f;

            m_CharController.Move(move * Time.deltaTime);
        }

        private void ApplyGravityWhenIdle()
        {
            if (m_CharController != null && m_CharController.enabled)
            {
                m_CharController.Move(new Vector3(0f, -9.81f * Time.deltaTime, 0f));
            }
        }

        private void UpdateVisualsAndLeaning()
        {
            // Bank/Lean body when turning
            float targetBank = 0f;
            if (Mathf.Abs(m_CurrentSpeed) > 1.0f)
            {
                targetBank = -m_SteerInput * m_MaxBankAngle * (m_CurrentSpeed / m_MaxForwardSpeed);
            }

            m_CurrentBankAngle = Mathf.Lerp(m_CurrentBankAngle, targetBank, Time.deltaTime * m_BankSpeed);

            if (m_VisualRoot != null)
            {
                m_VisualRoot.localRotation = Quaternion.Euler(0f, 0f, m_CurrentBankAngle);
            }

            // Steer front handlebars/fork
            if (m_FrontSteerFork != null)
            {
                float targetForkAngle = m_SteerInput * 28f;
                m_FrontSteerFork.localRotation = Quaternion.Euler(0f, targetForkAngle, 0f);
            }

            // Wheel spinning visual
            float wheelCircumference = 2f * Mathf.PI * 0.32f;
            float rotationAmount = (m_CurrentSpeed * Time.deltaTime / wheelCircumference) * 360f;

            if (m_WheelFront != null) m_WheelFront.Rotate(rotationAmount, 0f, 0f, Space.Self);
            if (m_WheelRear != null) m_WheelRear.Rotate(rotationAmount, 0f, 0f, Space.Self);
        }

        private void UpdateAudioPitch()
        {
            if (m_EngineAudio != null)
            {
                float speedFactor = Mathf.Clamp01(Mathf.Abs(m_CurrentSpeed) / m_MaxForwardSpeed);
                float throttleBonus = (m_ThrottleInput > 0f) ? 0.3f : 0f;
                float turboBonus = m_IsTurbo ? 0.4f : 0f;

                m_EngineAudio.pitch = Mathf.Lerp(0.85f, 2.3f, speedFactor + throttleBonus + turboBonus);
                m_EngineAudio.volume = Mathf.Lerp(0.45f, 0.9f, speedFactor + (m_ThrottleInput > 0 ? 0.3f : 0f));
            }
        }

        private void UpdateVehicleCamera()
        {
            if (m_VehicleCamera == null || !m_VehicleCamera.gameObject.activeSelf) return;

            // Target camera position behind bike
            Vector3 desiredPos = transform.position + transform.TransformDirection(m_CameraOffset);

            // Add dynamic FOV zoom at turbo speed
            float targetFov = m_IsTurbo ? 76f : 65f;
            m_VehicleCamera.fieldOfView = Mathf.Lerp(m_VehicleCamera.fieldOfView, targetFov, Time.deltaTime * 5f);

            // Smooth camera motion
            m_VehicleCamera.transform.position = Vector3.Lerp(m_VehicleCamera.transform.position, desiredPos, Time.deltaTime * m_CameraSmooth);

            // Look slightly ahead of the bike
            Vector3 lookTarget = transform.position + transform.forward * 4.0f + Vector3.up * 1.1f;
            m_VehicleCamera.transform.LookAt(lookTarget);
        }

        /// <summary>
        /// Called when colliding with a ghost apparition.
        /// </summary>
        public void OnGhostImpact(int damage)
        {
            m_GhostSlowdownTimer = 1.4f; // Shocked slowdown
            m_SfxAudio.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 1.0f);

            var mgr = IUHGrabCampusManager.Instance;
            if (mgr != null && mgr.IsMissionActive)
            {
                mgr.ApplyCargoDamage(damage, "Va Chạm Hồn Ma Tâm Linh!");
            }
        }

        private void OnDestroy()
        {
            if (m_VehicleCamera != null)
            {
                Destroy(m_VehicleCamera.gameObject);
            }
        }
    }
}
