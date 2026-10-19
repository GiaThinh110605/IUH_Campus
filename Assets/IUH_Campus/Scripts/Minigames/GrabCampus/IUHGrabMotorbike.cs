using UnityEngine;
using IUHCampus.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Arcade Motorbike Controller for Grab Campus ("Chuyến xe tâm linh").
    /// Full support for New Input System & Legacy Input.
    /// Seamlessly interfaces with Cinemachine to follow the motorbike and 3D rider figure.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class IUHGrabMotorbike : MonoBehaviour
    {
        [Header("Movement & Performance")]
        [SerializeField] private float m_MaxForwardSpeed = 16.0f; // ~58 km/h
        [SerializeField] private float m_TurboSpeed = 22.0f;      // ~79 km/h
        [SerializeField] private float m_ReverseSpeed = 4.5f;
        [SerializeField] private float m_Acceleration = 14.0f;
        [SerializeField] private float m_BrakingDeceleration = 22.0f;
        [SerializeField] private float m_CoastDeceleration = 7.0f;
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
        [SerializeField] private Transform m_CameraTarget;

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

        public bool IsMounted => m_IsMounted;
        public float CurrentSpeedKmH => Mathf.Abs(m_CurrentSpeed) * 3.6f;
        public float SpeedRatio => Mathf.Clamp01(Mathf.Abs(m_CurrentSpeed) / m_MaxForwardSpeed);
        public Transform CameraTarget => m_CameraTarget != null ? m_CameraTarget : transform;
        public float CurrentBankAngle => m_CurrentBankAngle;

        private void Awake()
        {
            m_CharController = GetComponent<CharacterController>();
            if (m_CharController == null)
            {
                m_CharController = gameObject.AddComponent<CharacterController>();
            }
            m_CharController.height = 1.3f;
            m_CharController.radius = 0.42f;
            m_CharController.center = new Vector3(0f, 0.72f, 0f);
            m_CharController.stepOffset = 0.45f;
            m_CharController.slopeLimit = 55f;

            SetupAudio();
            SetupCameraTarget();

            if (m_RiderAvatar != null)
            {
                m_RiderAvatar.SetActive(true);
            }
        }

        private void SetupAudio()
        {
            m_EngineAudio = gameObject.AddComponent<AudioSource>();
            m_EngineAudio.clip = IUHSoundSynthesizer.GetEngineSound();
            m_EngineAudio.loop = true;
            m_EngineAudio.playOnAwake = false;
            m_EngineAudio.volume = 0.65f;
            m_EngineAudio.spatialBlend = 0.5f;

            m_SfxAudio = gameObject.AddComponent<AudioSource>();
            m_SfxAudio.playOnAwake = false;
            m_SfxAudio.spatialBlend = 0.5f;
        }

        private void SetupCameraTarget()
        {
            if (m_CameraTarget == null)
            {
                var target = transform.Find("CameraTarget");
                if (target == null)
                {
                    var go = new GameObject("CameraTarget");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(0f, 1.35f, -0.2f);
                    m_CameraTarget = go.transform;
                }
                else
                {
                    m_CameraTarget = target;
                }
            }
        }

        private void Start()
        {
            FindPlayerReference();

            if (m_RiderAvatar == null)
            {
                var r = transform.Find("Rider_Figure");
                if (r != null) m_RiderAvatar = r.gameObject;
            }

            if (m_RiderAvatar != null)
            {
                m_RiderAvatar.SetActive(m_IsMounted);
            }
        }

        private void FindPlayerReference()
        {
            if (m_PlayerOriginal == null)
            {
                m_PlayerOriginal = GameObject.FindGameObjectWithTag("Player");
                if (m_PlayerOriginal == null)
                {
                    var all = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var go in all)
                    {
                        if (go.name == "PlayerArmature" && !go.hideFlags.HasFlag(HideFlags.NotEditable))
                        {
                            m_PlayerOriginal = go;
                            break;
                        }
                    }
                }
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
        }

        private void HandleMountInputs()
        {
            bool mountKey = IUHInputHelper.IsVehicleKeyDown();
            bool hornKey = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                hornKey = Keyboard.current.hKey.wasPressedThisFrame;
            }
#endif

            if (!m_IsMounted)
            {
                FindPlayerReference();
                if (m_PlayerOriginal != null && m_PlayerOriginal.activeInHierarchy)
                {
                    float dist = Vector3.Distance(transform.position, m_PlayerOriginal.transform.position);
                    if (dist < 3.2f && mountKey)
                    {
                        MountVehicle(m_PlayerOriginal);
                    }
                }
            }
            else
            {
                if (mountKey && Mathf.Abs(m_CurrentSpeed) < 1.5f)
                {
                    DismountVehicle();
                }

                if (hornKey && m_SfxAudio != null)
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

            if (m_RiderAvatar == null)
            {
                var r = transform.Find("Rider_Figure");
                if (r != null) m_RiderAvatar = r.gameObject;
            }

            if (m_RiderAvatar != null)
            {
                m_RiderAvatar.SetActive(true);
            }

            if (m_Headlight != null)
            {
                m_Headlight.enabled = true;
            }

            // Switch Cinemachine to follow this motorbike
            SwitchCinemachineTarget(true);

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

            if (m_EngineAudio != null && m_EngineAudio.isPlaying)
            {
                m_EngineAudio.Stop();
            }

            if (m_RiderAvatar != null)
            {
                m_RiderAvatar.SetActive(false);
            }

            FindPlayerReference();

            if (m_PlayerOriginal != null)
            {
                Vector3 dismountPos = transform.position - transform.right * 1.5f + Vector3.up * 0.1f;
                m_PlayerOriginal.transform.position = dismountPos;
                m_PlayerOriginal.transform.rotation = transform.rotation;
                m_PlayerOriginal.SetActive(true);
            }

            // Switch Cinemachine back to follow player
            SwitchCinemachineTarget(false);

            Debug.Log("[IUH Grab Motorbike] Người chơi đã xuống xe máy!");
        }

        private void SwitchCinemachineTarget(bool followBike)
        {
            var allCams = UnityEngine.Object.FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var vcam in allCams)
            {
                if (vcam == null) continue;
                var tpFollow = vcam.GetComponent<Unity.Cinemachine.CinemachineThirdPersonFollow>();

                if (followBike)
                {
                    Transform target = CameraTarget;
                    if (target == null) target = transform.Find("CameraTarget");
                    if (target == null) target = transform;
                    vcam.Follow = target;
                    if (vcam.LookAt != null) vcam.LookAt = target;

                    if (tpFollow != null)
                    {
                        tpFollow.CameraDistance = 5.4f;
                        tpFollow.ShoulderOffset = new Vector3(0f, 0.45f, 0f);
                        tpFollow.CameraSide = 0.5f;
                        tpFollow.VerticalArmLength = 0.5f;
                    }
                }
                else
                {
                    FindPlayerReference();
                    if (m_PlayerOriginal != null)
                    {
                        var root = m_PlayerOriginal.transform.Find("PlayerCameraRoot");
                        var t = root != null ? root : m_PlayerOriginal.transform;
                        vcam.Follow = t;
                        if (vcam.LookAt != null) vcam.LookAt = t;

                        if (tpFollow != null)
                        {
                            tpFollow.CameraDistance = 4.0f;
                            tpFollow.ShoulderOffset = new Vector3(1f, 0f, 0f);
                            tpFollow.CameraSide = 0.6f;
                            tpFollow.VerticalArmLength = 0f;
                        }
                    }
                }
            }
        }

        private void ReadDrivingInputs()
        {
            Vector2 move = IUHInputHelper.GetMovementAxis();
            m_ThrottleInput = move.y;
            m_SteerInput = move.x;
            m_IsBraking = IUHInputHelper.IsBrakeHeld();
            m_IsTurbo = IUHInputHelper.IsTurboHeld();
        }

        private void UpdatePhysicsMovement()
        {
            if (m_GhostSlowdownTimer > 0f)
            {
                m_GhostSlowdownTimer -= Time.deltaTime;
                m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, 0f, 15f * Time.deltaTime);
            }

            float customMult = (IUHCustomizationManager.Instance != null) ? IUHCustomizationManager.Instance.ActiveConfig.speedMultiplier : 1.0f;
            float targetMaxSpeed = (m_IsTurbo ? m_TurboSpeed : m_MaxForwardSpeed) * customMult;

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
                m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, 0f, m_CoastDeceleration * Time.deltaTime);
            }

            // Steering
            if (Mathf.Abs(m_CurrentSpeed) > 0.1f)
            {
                float dirMultiplier = Mathf.Sign(m_CurrentSpeed);
                float speedFactor = Mathf.Clamp01(Mathf.Abs(m_CurrentSpeed) / 12f);
                float effectiveTurnRate = Mathf.Lerp(m_TurnSpeed, m_TurnSpeed * 0.65f, speedFactor);

                float yaw = m_SteerInput * effectiveTurnRate * dirMultiplier * Time.deltaTime;
                transform.Rotate(0f, yaw, 0f);
            }

            Vector3 move = transform.forward * m_CurrentSpeed;
            move.y = -9.81f; // Gravity to stick to roads

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

            if (m_FrontSteerFork != null)
            {
                float targetForkAngle = m_SteerInput * 28f;
                m_FrontSteerFork.localRotation = Quaternion.Euler(0f, targetForkAngle, 0f);
            }

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

        public void OnGhostImpact(int damage)
        {
            m_GhostSlowdownTimer = 1.4f;
            if (m_SfxAudio != null)
            {
                m_SfxAudio.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 1.0f);
            }

            var mgr = IUHGrabCampusManager.Instance;
            if (mgr != null && mgr.IsMissionActive)
            {
                mgr.ApplyCargoDamage(damage, "Va Chạm Hồn Ma Tâm Linh!");
            }
        }
    }
}
