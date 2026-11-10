using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using IUHCampus;
using IUHCampus.Guardian;

namespace IUHCampus.Guardian
{
    /// <summary>
    /// Master controller for the IUH Guardian Drone.
    /// Handles 6-DOF flight physics, smooth banking, FPV/TPP camera switching,
    /// vision modes, companion follow mode, photo reconnaissance,
    /// Airdrop supply crates [G], Data EMP bomb strikes [B], and Cinematic Flycam [C].
    /// </summary>
    public class IUHGuardianDroneController : MonoBehaviour
    {
        public static IUHGuardianDroneController Instance { get; private set; }

        [Header("Flight Dynamics")]
        [SerializeField] private float m_FlySpeed = 14f;
        [SerializeField] private float m_BoostMultiplier = 1.8f;
        [SerializeField] private float m_AscendSpeed = 8f;
        [SerializeField] private float m_RotationSpeed = 120f;
        [SerializeField] private float m_MaxTiltAngle = 22f;
        [SerializeField] private float m_TiltSmoothSpeed = 6f;

        [Header("Cameras")]
        [SerializeField] private Camera m_FPVCamera;
        [SerializeField] private Camera m_TPPCamera;
        [SerializeField] private bool m_UseFPV = false;

        [Header("Lighting & Visuals")]
        [SerializeField] private Light m_Spotlight;
        [SerializeField] private GameObject m_NeonCargoBox;
        [SerializeField] private List<IUHRotorSpinner> m_Rotors = new List<IUHRotorSpinner>();
        [SerializeField] private Renderer m_NeonBoxRenderer;

        [Header("Audio")]
        [SerializeField] private AudioSource m_RotorAudioSource;

        [Header("HUD")]
        [SerializeField] private IUHDroneHUD m_DroneHUD;

        [Header("Companion Mode (When not piloted)")]
        [SerializeField] private Transform m_PlayerTransform;
        [SerializeField] private Vector3 m_CompanionOffset = new Vector3(1.4f, 2.2f, -1.2f);
        [SerializeField] private float m_CompanionFollowSpeed = 5f;

        [Header("Player Components")]
        [SerializeField] private ThirdPersonController m_ThirdPersonController;
        [SerializeField] private StarterAssetsInputs m_StarterInputs;

        [Header("State")]
        [SerializeField] private bool m_IsControlled = false;
        [SerializeField] private float m_Battery = 100f;
        [SerializeField] private IUHDroneHUD.DroneVisionMode m_CurrentVisionMode = IUHDroneHUD.DroneVisionMode.Normal;
        [SerializeField] private bool m_IsCinematicActive = false;

        private float m_CurrentYaw = 0f;
        private float m_CurrentPitch = 0f;
        private float m_CurrentRoll = 0f;
        private Vector3 m_Velocity = Vector3.zero;
        private Camera m_PlayerMainCamera;
        private Coroutine m_CinematicRoutine;
        private int m_CinematicVariant = 0; // 0 = Orbit 360, 1 = Dronie

        public bool IsControlled => m_IsControlled;
        public float Battery => m_Battery;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            m_CurrentYaw = transform.eulerAngles.y;
        }

        private void Start()
        {
            FindPlayerAndCameras();
            SetupAudio();

            if (m_DroneHUD != null)
            {
                m_DroneHUD.Initialize(this);
            }

            SetControlled(false);
        }

        private void FindPlayerAndCameras()
        {
            if (m_PlayerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player == null)
                {
                    var student = GameObject.Find("Campus Student");
                    if (student != null) player = student;
                }
                if (player != null)
                {
                    m_PlayerTransform = player.transform;
                    m_ThirdPersonController = player.GetComponent<ThirdPersonController>();
                    m_StarterInputs = player.GetComponent<StarterAssetsInputs>();
                }
            }

            if (m_PlayerMainCamera == null)
            {
                var mainCamObj = GameObject.FindWithTag("MainCamera");
                if (mainCamObj != null)
                {
                    m_PlayerMainCamera = mainCamObj.GetComponent<Camera>();
                }
            }
        }

        private void SetupAudio()
        {
            if (m_RotorAudioSource == null)
            {
                m_RotorAudioSource = GetComponent<AudioSource>();
                if (m_RotorAudioSource == null)
                {
                    m_RotorAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (m_RotorAudioSource != null)
            {
                m_RotorAudioSource.loop = true;
                m_RotorAudioSource.spatialBlend = 0.85f;
                m_RotorAudioSource.minDistance = 3f;
                m_RotorAudioSource.maxDistance = 60f;
                m_RotorAudioSource.playOnAwake = false;

                if (m_RotorAudioSource.clip == null)
                {
                    m_RotorAudioSource.clip = CreateDroneSynthClip();
                }
                m_RotorAudioSource.Play();
            }
        }

        private AudioClip CreateDroneSynthClip()
        {
            int sampleRate = 22050;
            float duration = 1.0f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float wave = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.4f
                           + Mathf.Sin(2f * Mathf.PI * 360f * t) * 0.3f
                           + Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.2f
                           + (Random.value * 2f - 1f) * 0.08f;
                samples[i] = wave * 0.4f;
            }

            var clip = AudioClip.Create("DroneWhirrSynth", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Update()
        {
            // Toggle Drone Control [U]
            if (Input.GetKeyDown(KeyCode.U))
            {
                SetControlled(!m_IsControlled);
            }

            if (m_IsControlled)
            {
                if (!m_IsCinematicActive)
                {
                    HandleFlightInput();
                }
                UpdateCameraViews();
                HandleFeatureKeys();
                UpdateTelemetry();
            }
            else
            {
                UpdateCompanionFollow();
            }

            AnimateNeonBox();
        }

        public void ToggleDroneControl()
        {
            SetControlled(!m_IsControlled);
        }

        public void SetControlled(bool controlled)
        {
            m_IsControlled = controlled;

            FindPlayerAndCameras();

            if (m_IsControlled)
            {
                if (m_ThirdPersonController != null)
                {
                    m_ThirdPersonController.enabled = false;
                }
                if (m_StarterInputs != null)
                {
                    m_StarterInputs.move = Vector2.zero;
                    m_StarterInputs.look = Vector2.zero;
                    m_StarterInputs.sprint = false;
                    m_StarterInputs.jump = false;
                    m_StarterInputs.cursorLocked = false;
                    m_StarterInputs.cursorInputForLook = false;
                }
                var anim = m_ThirdPersonController != null ? m_ThirdPersonController.GetComponent<Animator>() : null;
                if (anim != null)
                {
                    anim.SetFloat("Speed", 0f);
                    anim.SetFloat("MotionSpeed", 0f);
                }
                if (m_PlayerMainCamera != null)
                {
                    m_PlayerMainCamera.enabled = false;
                }

                if (m_PlayerTransform != null)
                {
                    transform.position = m_PlayerTransform.position + Vector3.up * 3.5f + m_PlayerTransform.forward * 2f;
                    m_CurrentYaw = m_PlayerTransform.eulerAngles.y;
                    m_CurrentPitch = 0f;
                }

                UpdateActiveCamera();
                if (m_DroneHUD != null) m_DroneHUD.SetDroneHUDActive(true);
            }
            else
            {
                StopCinematicIfActive();

                // Return to student with unlocked movement
                if (m_ThirdPersonController != null)
                {
                    m_ThirdPersonController.enabled = true;
                }
                if (m_StarterInputs != null)
                {
                    m_StarterInputs.cursorLocked = true;
                    m_StarterInputs.cursorInputForLook = true;
                }
                if (m_PlayerMainCamera != null)
                {
                    m_PlayerMainCamera.enabled = true;
                }

                if (m_FPVCamera != null) m_FPVCamera.enabled = false;
                if (m_TPPCamera != null) m_TPPCamera.enabled = false;

                if (m_DroneHUD != null) m_DroneHUD.SetDroneHUDActive(false);
            }

            foreach (var r in m_Rotors)
            {
                if (r != null) r.SetSpinActive(true);
            }
        }

        private void HandleFlightInput()
        {
            float h = Input.GetAxis("Horizontal"); // A/D
            float v = Input.GetAxis("Vertical");   // W/S
            float up = 0f;
            if (Input.GetKey(KeyCode.Space)) up += 1f;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) up -= 1f;

            // Support Mobile Joystick
            if (IUHCampus.Core.IUHInputHelper.VirtualMoveAxis.sqrMagnitude > 0.01f)
            {
                h = IUHCampus.Core.IUHInputHelper.VirtualMoveAxis.x;
                v = IUHCampus.Core.IUHInputHelper.VirtualMoveAxis.y;
            }

            // Support Mobile Altitude buttons (Lên / Xuống)
            if (Mathf.Abs(IUHCampus.Core.IUHInputHelper.VirtualDroneAltitude) > 0.01f)
            {
                up = IUHCampus.Core.IUHInputHelper.VirtualDroneAltitude;
            }

            bool isBoosting = Input.GetKey(KeyCode.LeftShift) || IUHCampus.Core.IUHInputHelper.VirtualDroneBoost;
            float speed = m_FlySpeed * (isBoosting ? m_BoostMultiplier : 1.0f);

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Support Mobile Touch Look swipe
            if (IUHCampus.Core.IUHInputHelper.VirtualDroneLook.sqrMagnitude > 0.001f)
            {
                mouseX += IUHCampus.Core.IUHInputHelper.VirtualDroneLook.x;
                mouseY += IUHCampus.Core.IUHInputHelper.VirtualDroneLook.y;
                IUHCampus.Core.IUHInputHelper.VirtualDroneLook = Vector2.zero;
            }

            if (Input.GetKey(KeyCode.Q)) mouseX -= 1f;
            if (Input.GetKey(KeyCode.E)) mouseX += 1f;

            m_CurrentYaw += mouseX * m_RotationSpeed * Time.deltaTime;
            m_CurrentPitch = Mathf.Clamp(m_CurrentPitch - mouseY * m_RotationSpeed * 0.7f * Time.deltaTime, -60f, 60f);

            Quaternion yawRotation = Quaternion.Euler(0f, m_CurrentYaw, 0f);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;

            Vector3 moveDir = (forward * v + right * h).normalized;
            Vector3 targetVel = moveDir * speed + Vector3.up * (up * m_AscendSpeed);

            m_Velocity = Vector3.Lerp(m_Velocity, targetVel, Time.deltaTime * 6f);
            transform.position += m_Velocity * Time.deltaTime;

            if (transform.position.y < 0.6f)
            {
                Vector3 pos = transform.position;
                pos.y = 0.6f;
                transform.position = pos;
            }

            float targetRoll = -h * m_MaxTiltAngle;
            float targetPitch = (v * m_MaxTiltAngle * 0.6f) + m_CurrentPitch;
            m_CurrentRoll = Mathf.Lerp(m_CurrentRoll, targetRoll, Time.deltaTime * m_TiltSmoothSpeed);

            transform.rotation = Quaternion.Euler(targetPitch, m_CurrentYaw, m_CurrentRoll);

            float throttle = Mathf.Clamp01(m_Velocity.magnitude / (m_FlySpeed * m_BoostMultiplier));
            foreach (var r in m_Rotors)
            {
                if (r != null) r.SetThrottle(throttle);
            }

            if (m_RotorAudioSource != null)
            {
                m_RotorAudioSource.pitch = Mathf.Lerp(0.85f, 1.45f, throttle);
                m_RotorAudioSource.volume = Mathf.Lerp(0.4f, 0.85f, throttle);
            }

            m_Battery = Mathf.Max(0f, m_Battery - Time.deltaTime * (isBoosting ? 0.4f : 0.15f));
        }

        private void UpdateCompanionFollow()
        {
            if (m_PlayerTransform == null)
            {
                FindPlayerAndCameras();
                if (m_PlayerTransform == null) return;
            }

            m_Battery = Mathf.Min(100f, m_Battery + Time.deltaTime * 1.5f);

            Vector3 targetPos = m_PlayerTransform.position
                              + m_PlayerTransform.right * m_CompanionOffset.x
                              + Vector3.up * m_CompanionOffset.y
                              + m_PlayerTransform.forward * m_CompanionOffset.z;

            float bob = Mathf.Sin(Time.time * 2f) * 0.15f;
            targetPos.y += bob;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * m_CompanionFollowSpeed);

            Vector3 lookTarget = m_PlayerTransform.position + m_PlayerTransform.forward * 8f + Vector3.up * 1.5f;
            Quaternion lookRot = Quaternion.LookRotation((lookTarget - transform.position).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

            foreach (var r in m_Rotors)
            {
                if (r != null) r.SetThrottle(0.1f);
            }
            if (m_RotorAudioSource != null)
            {
                m_RotorAudioSource.pitch = 0.85f;
                m_RotorAudioSource.volume = 0.25f;
            }
        }

        private void HandleFeatureKeys()
        {
            if (Input.GetKeyDown(KeyCode.V)) ToggleCameraView();
            if (Input.GetKeyDown(KeyCode.T)) ToggleVisionMode();
            if (Input.GetKeyDown(KeyCode.F)) CapturePhotoRecon();
            if (Input.GetKeyDown(KeyCode.G)) DropSupplyAirdrop();
            if (Input.GetKeyDown(KeyCode.B)) DropEmpBomb();
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X)) ToggleCinematicMode();
        }

        public void ToggleCameraView()
        {
            m_UseFPV = !m_UseFPV;
            UpdateActiveCamera();
        }

        private void UpdateActiveCamera()
        {
            if (m_FPVCamera != null) m_FPVCamera.enabled = m_IsControlled && m_UseFPV;
            if (m_TPPCamera != null) m_TPPCamera.enabled = m_IsControlled && !m_UseFPV;
        }

        private void UpdateCameraViews()
        {
            if (!m_UseFPV && m_TPPCamera != null)
            {
                Vector3 behind = transform.position - transform.forward * 3.5f + Vector3.up * 1.4f;
                m_TPPCamera.transform.position = Vector3.Lerp(m_TPPCamera.transform.position, behind, Time.deltaTime * 12f);
                m_TPPCamera.transform.LookAt(transform.position + transform.forward * 2f);
            }
        }

        public void ToggleVisionMode()
        {
            m_CurrentVisionMode = (IUHDroneHUD.DroneVisionMode)(((int)m_CurrentVisionMode + 1) % 3);
            if (m_DroneHUD != null)
            {
                m_DroneHUD.SetVisionModeUI(m_CurrentVisionMode);
            }
        }

        // ==========================================
        // FEATURE 1: AIRDROP SUPPLY CRATE [G]
        // ==========================================
        public void DropSupplyAirdrop()
        {
            StartCoroutine(AirdropSupplyRoutine());
        }

        private IEnumerator AirdropSupplyRoutine()
        {
            PlayDropSFX();

            // Spawn glowing crate under drone
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Airdrop_Supply_Box";
            crate.transform.position = transform.position + Vector3.down * 0.8f;
            crate.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.1f, 0.9f, 0.4f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.1f, 1.2f, 0.4f));
            crate.GetComponent<Renderer>().material = mat;

            // Parachute umbrella top
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Chute_Canopy";
            canopy.transform.SetParent(crate.transform, false);
            canopy.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            canopy.transform.localScale = new Vector3(1.8f, 0.5f, 1.8f);
            var chuteMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            chuteMat.color = new Color(0.8f, 1f, 0.8f);
            canopy.GetComponent<Renderer>().material = chuteMat;

            if (m_DroneHUD != null)
            {
                m_DroneHUD.ShowActionNotification("📦 ĐANG THẢ HÀNG CỨU TRỢ", "Thùng tiếp tế Nano đang bung dù hạ cánh...");
            }

            // Descend smoothly
            float fallSpeed = 3.6f;
            while (crate != null && crate.transform.position.y > 0.4f)
            {
                crate.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
                crate.transform.Rotate(Vector3.up, 40f * Time.deltaTime, Space.Self);
                yield return null;
            }

            if (crate != null)
            {
                crate.transform.position = new Vector3(crate.transform.position.x, 0.35f, crate.transform.position.z);

                // Heal player to 100%
                if (IUHGuardianInventory.Instance != null)
                {
                    IUHGuardianInventory.Instance.RestoreFullHp();
                    IUHGuardianInventory.Instance.AddItem("Bình Dược Liệu Nano (+50 HP)", 1);
                }

                IUHCombatAudio.PlayHeal();
                PlayHealingChimeSFX();

                if (m_DroneHUD != null)
                {
                    m_DroneHUD.ShowActionNotification("💚 TIẾP TẾ HOÀN TẤT!", "Sinh viên đã được HỒI PHỤC 100% MÁU & nhận +1 Bình Dược Liệu!");
                }

                // Disappear after a few seconds
                yield return new WaitForSeconds(3.0f);
                Destroy(crate);
            }
        }

        // ==========================================
        // FEATURE 2: DATA EMP BOMB STRIKE [B]
        // ==========================================
        public void DropEmpBomb()
        {
            StartCoroutine(EmpBombRoutine());
        }

        private IEnumerator EmpBombRoutine()
        {
            PlayDropSFX();

            GameObject bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bomb.name = "Data_EMP_Bomb";
            bomb.transform.position = transform.position + Vector3.down * 0.8f;
            bomb.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.7f, 0.1f, 1.0f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1.2f, 0.2f, 2.0f));
            bomb.GetComponent<Renderer>().material = mat;

            // Electric Light on bomb
            var bombLight = bomb.AddComponent<Light>();
            bombLight.color = new Color(0.8f, 0.2f, 1f);
            bombLight.range = 8f;
            bombLight.intensity = 2f;

            if (m_DroneHUD != null)
            {
                m_DroneHUD.ShowActionNotification("💥 THẢ BOM XUNG ĐIỆN EMP!", "Bom điện từ đang lao xuống mục tiêu...");
            }

            float velocityY = -5.0f;
            while (bomb != null && bomb.transform.position.y > 0.4f)
            {
                velocityY += -18f * Time.deltaTime;
                bomb.transform.position += Vector3.up * velocityY * Time.deltaTime;
                yield return null;
            }

            if (bomb != null)
            {
                Vector3 blastPos = bomb.transform.position;
                Destroy(bomb);

                // EMP Shockwave explosion
                PlayThunderSFX();
                IUHCameraShake.Shake(0.35f, 0.45f);

                // Shockwave ring
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "EMP_Shockwave_Ring";
                ring.transform.position = blastPos + Vector3.up * 0.1f;
                ring.transform.localScale = new Vector3(1f, 0.05f, 1f);
                Destroy(ring.GetComponent<Collider>());
                var ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                ringMat.color = new Color(0.8f, 0.3f, 1f);
                ringMat.EnableKeyword("_EMISSION");
                ringMat.SetColor("_EmissionColor", new Color(1.5f, 0.5f, 2.5f));
                ring.GetComponent<Renderer>().material = ringMat;

                // Stun and damage monsters in 14m radius
                var monsters = Object.FindObjectsByType<IUHMonsterEncounter>(FindObjectsSortMode.None);
                int hitCount = 0;
                foreach (var m in monsters)
                {
                    if (m != null && m.gameObject.activeInHierarchy)
                    {
                        float d = Vector3.Distance(blastPos, m.transform.position);
                        if (d <= 14f)
                        {
                            hitCount++;
                            // Stun monster cooldown
                            m.ApplyEmpStun(5.0f);
                        }
                    }
                }

                if (m_DroneHUD != null)
                {
                    m_DroneHUD.ShowActionNotification("⚡ NỔ SÓNG ĐIỆN TỪ EMP!", $"Đã làm CHOÁNG {hitCount} quái vật trong 5 GIÂY & phá hủy lá chắn dữ liệu!");
                }

                // Expand ring
                float ringElapsed = 0f;
                while (ringElapsed < 0.6f && ring != null)
                {
                    ringElapsed += Time.deltaTime;
                    float scale = Mathf.Lerp(1f, 22f, ringElapsed / 0.6f);
                    ring.transform.localScale = new Vector3(scale, 0.05f, scale);
                    yield return null;
                }
                if (ring != null) Destroy(ring);
            }
        }

        // ==========================================
        // FEATURE 6: CINEMATIC FLYCAM (ORBIT & DRONIE) [C]
        // ==========================================
        public void ToggleCinematicMode()
        {
            if (m_IsCinematicActive)
            {
                StopCinematicIfActive();
                if (m_DroneHUD != null) m_DroneHUD.SetCinematicProgress("", false);
                return;
            }

            m_CinematicVariant = (m_CinematicVariant + 1) % 2;
            if (m_CinematicVariant == 0)
            {
                m_CinematicRoutine = StartCoroutine(CinematicOrbitRoutine());
            }
            else
            {
                m_CinematicRoutine = StartCoroutine(CinematicDronieRoutine());
            }
        }

        private void StopCinematicIfActive()
        {
            if (m_CinematicRoutine != null)
            {
                StopCoroutine(m_CinematicRoutine);
                m_CinematicRoutine = null;
            }
            m_IsCinematicActive = false;
        }

        private IEnumerator CinematicOrbitRoutine()
        {
            if (m_PlayerTransform == null) yield break;

            m_IsCinematicActive = true;
            float duration = 6.0f;
            float elapsed = 0f;
            float radius = 6.0f;
            float height = 2.4f;
            Vector3 center = m_PlayerTransform.position;

            while (elapsed < duration && m_IsCinematicActive)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float angle = progress * Mathf.PI * 2f;

                Vector3 targetPos = center + new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 8f);
                transform.LookAt(center + Vector3.up * 1.2f);

                if (m_DroneHUD != null)
                {
                    m_DroneHUD.SetCinematicProgress($"🎬 FLYCAM 360° ĐANG QUAY: {(progress * 100f):F0}%", true);
                }
                yield return null;
            }

            CapturePhotoRecon();
            if (m_DroneHUD != null)
            {
                m_DroneHUD.SetCinematicProgress("", false);
                m_DroneHUD.ShowActionNotification("🎬 THƯỚC PHIM 360° HOÀN TẤT!", "Đã ghi nhận toàn cảnh sinh viên IUH góc quay điện ảnh 360 độ!");
            }
            m_IsCinematicActive = false;
        }

        private IEnumerator CinematicDronieRoutine()
        {
            if (m_PlayerTransform == null) yield break;

            m_IsCinematicActive = true;
            float duration = 5.0f;
            float elapsed = 0f;
            Vector3 startPos = m_PlayerTransform.position + m_PlayerTransform.forward * 2.2f + Vector3.up * 1.5f;
            Vector3 targetPos = m_PlayerTransform.position - m_PlayerTransform.forward * 22f + Vector3.up * 26f;

            transform.position = startPos;

            while (elapsed < duration && m_IsCinematicActive)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float smoothT = Mathf.SmoothStep(0f, 1f, progress);

                transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                transform.LookAt(m_PlayerTransform.position + Vector3.up * 1.2f);

                if (m_DroneHUD != null)
                {
                    m_DroneHUD.SetCinematicProgress($"📸 DRONIE BAY XA: {(progress * 100f):F0}%", true);
                }
                yield return null;
            }

            CapturePhotoRecon();
            if (m_DroneHUD != null)
            {
                m_DroneHUD.SetCinematicProgress("", false);
                m_DroneHUD.ShowActionNotification("📸 SIÊU ẢNH DRONIE CAO CẤP!", "Đã chụp siêu ảnh từ độ cao 26m khoe trọn đại cảnh trường IUH!");
            }
            m_IsCinematicActive = false;
        }

        // ==========================================
        // PHOTO RECON
        // ==========================================
        public void CapturePhotoRecon()
        {
            PlayShutterSFX();

            if (m_DroneHUD != null)
            {
                m_DroneHUD.TriggerShutterFlash();
            }

            Camera activeCam = m_UseFPV ? m_FPVCamera : m_TPPCamera;
            if (activeCam == null) activeCam = Camera.main;

            string targetName = "Toà nhà Giảng đường IUH";
            int goldReward = 50;
            int dataCoreReward = 1;

            if (activeCam != null)
            {
                Ray ray = new Ray(activeCam.transform.position, activeCam.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 150f))
                {
                    string hitName = hit.collider.gameObject.name;
                    if (hitName.Contains("MONSTER") || hitName.Contains("Demon") || hitName.Contains("Orc") || hitName.Contains("Backpack"))
                    {
                        targetName = $"Phát hiện Quái Vật Dữ Liệu [{hitName}]";
                        goldReward = 100;
                        dataCoreReward = 2;
                    }
                    else if (hitName.Contains("CHEST") || hitName.Contains("Chest"))
                    {
                        targetName = $"Rương Báu Ẩn [{hitName}]";
                        goldReward = 80;
                        dataCoreReward = 2;
                    }
                    else if (hitName.Contains("CANTEEN") || hitName.Contains("Căn Tin"))
                    {
                        targetName = "Khu Vực Căn Tin Sinh Viên IUH";
                        goldReward = 40;
                    }
                    else if (hitName.Contains("FESTIVAL") || hitName.Contains("Festival"))
                    {
                        targetName = "Sân Khấu Lễ Hội Âm Nhạc IUH";
                        goldReward = 60;
                    }
                    else if (transform.position.y > 18f)
                    {
                        targetName = $"Nóc Toà Nhà Trung Tâm IUH (Độ cao {transform.position.y:F0}m)";
                        goldReward = 75;
                        dataCoreReward = 1;
                    }
                }
            }

            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(goldReward);
                IUHGuardianInventory.Instance.AddItem("Lõi Dữ Liệu", dataCoreReward);
            }

            if (m_DroneHUD != null)
            {
                m_DroneHUD.ShowReconReport(targetName, goldReward, dataCoreReward);
            }
        }

        private void PlayDropSFX()
        {
            if (m_RotorAudioSource == null) return;
            int sampleRate = 22050;
            float duration = 0.25f;
            int total = (int)(sampleRate * duration);
            float[] samples = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(600f, 200f, (float)i / total);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - (float)i / total) * 0.7f;
            }
            var clip = AudioClip.Create("DropSFX", total, 1, sampleRate, false);
            clip.SetData(samples, 0);
            m_RotorAudioSource.PlayOneShot(clip, 0.8f);
        }

        private void PlayHealingChimeSFX()
        {
            if (m_RotorAudioSource == null) return;
            int sampleRate = 22050;
            float duration = 0.5f;
            int total = (int)(sampleRate * duration);
            float[] samples = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                samples[i] = (Mathf.Sin(2f * Mathf.PI * 523f * t) + Mathf.Sin(2f * Mathf.PI * 659f * t) + Mathf.Sin(2f * Mathf.PI * 784f * t)) * 0.25f * (1f - (float)i / total);
            }
            var clip = AudioClip.Create("HealChimeSFX", total, 1, sampleRate, false);
            clip.SetData(samples, 0);
            m_RotorAudioSource.PlayOneShot(clip, 0.85f);
        }

        private void PlayThunderSFX()
        {
            if (m_RotorAudioSource == null) return;
            int sampleRate = 22050;
            float duration = 0.45f;
            int total = (int)(sampleRate * duration);
            float[] samples = new float[total];
            for (int i = 0; i < total; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (float)i / total;
                samples[i] = (Random.value * 2f - 1f) * env * 0.9f + Mathf.Sin(2f * Mathf.PI * 80f * t) * env * 0.5f;
            }
            var clip = AudioClip.Create("ThunderSFX", total, 1, sampleRate, false);
            clip.SetData(samples, 0);
            m_RotorAudioSource.PlayOneShot(clip, 1.0f);
        }

        private void PlayShutterSFX()
        {
            if (m_RotorAudioSource == null) return;
            int sampleRate = 22050;
            float duration = 0.12f;
            int samplesCount = (int)(sampleRate * duration);
            float[] samples = new float[samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (float)i / samplesCount;
                samples[i] = Mathf.Sin(2f * Mathf.PI * 1200f * t) * env * 0.7f + (Random.value * 2f - 1f) * env * 0.4f;
            }
            var clip = AudioClip.Create("ShutterSFX", samplesCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            m_RotorAudioSource.PlayOneShot(clip, 0.9f);
        }

        private void UpdateTelemetry()
        {
            if (m_DroneHUD == null) return;

            float altitude = transform.position.y;
            float speedKmh = m_Velocity.magnitude * 3.6f;
            float heading = (transform.eulerAngles.y + 360f) % 360f;

            string targetName = "";
            Camera activeCam = m_UseFPV ? m_FPVCamera : m_TPPCamera;
            if (activeCam != null)
            {
                Ray ray = new Ray(activeCam.transform.position, activeCam.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 80f))
                {
                    targetName = hit.collider.gameObject.name;
                }
            }

            m_DroneHUD.UpdateTelemetry(altitude, speedKmh, m_Battery, heading, targetName);
        }

        private void AnimateNeonBox()
        {
            if (m_NeonCargoBox != null)
            {
                m_NeonCargoBox.transform.Rotate(Vector3.up, 45f * Time.deltaTime, Space.Self);
            }

            if (m_NeonBoxRenderer != null && m_NeonBoxRenderer.material != null)
            {
                float pulse = 0.7f + Mathf.PingPong(Time.time * 1.5f, 0.6f);
                Color glow = new Color(0.1f * pulse, 0.8f * pulse, 1.0f * pulse);
                m_NeonBoxRenderer.material.SetColor("_EmissionColor", glow);
            }
        }
    }
}
