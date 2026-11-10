using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHCampusCombatManager : MonoBehaviour
    {
        public static IUHCampusCombatManager Instance { get; private set; }

        [Header("Gun Configuration")]
        [SerializeField] private Transform m_GunMuzzle;
        [SerializeField] private LineRenderer m_LaserTracer;
        [SerializeField] private Light m_MuzzleLight;
        [SerializeField] private float m_FireRate = 0.22f; // shots per sec
        [SerializeField] private int m_BaseDamage = 28;
        [SerializeField] private float m_CritChance = 0.25f;
        [SerializeField] private float m_MaxRange = 90f;

        [Header("Skill 1: EMP Shockwave")]
        [SerializeField] private float m_Skill1Cooldown = 8.0f;
        [SerializeField] private float m_Skill1Radius = 8.0f;
        [SerializeField] private int m_Skill1Damage = 55;

        [Header("Skill 2: Laser Barrage")]
        [SerializeField] private float m_Skill2Cooldown = 12.0f;
        [SerializeField] private int m_Skill2MissilesCount = 6;
        [SerializeField] private int m_Skill2DamagePerShot = 32;

        [Header("Skill 3: Ultimate Drone Strike")]
        [SerializeField] private float m_Skill3Cooldown = 25.0f;
        [SerializeField] private float m_Skill3Radius = 14.0f;
        [SerializeField] private int m_Skill3Damage = 200;

        [Header("Anti-Air Aim & Missile Configuration")]
        [SerializeField] private float m_AimFov = 38f;
        [SerializeField] private float m_NormalFov = 60f;
        [SerializeField] private float m_AimTransitionSpeed = 12f;
        [SerializeField] private float m_AntiAirMissileCooldown = 4.0f;
        [SerializeField] private int m_AntiAirMissileDamage = 95;

        [Header("Timed Plasma Grenade Configuration")]
        [SerializeField] private float m_GrenadeFuseTime = 4.0f;
        [SerializeField] private float m_GrenadeCooldown = 3.5f;
        [SerializeField] private int m_GrenadeDamage = 180;
        [SerializeField] private float m_GrenadeRadius = 8.5f;

        [Header("Runtime Cooldowns")]
        private float m_NextFireTime = 0f;
        private float m_Skill1Timer = 0f;
        private float m_Skill2Timer = 0f;
        private float m_Skill3Timer = 0f;
        private float m_AntiAirMissileTimer = 0f;
        private float m_GrenadeTimer = 0f;

        public float Skill1Cooldown => m_Skill1Cooldown;
        public float Skill2Cooldown => m_Skill2Cooldown;
        public float Skill3Cooldown => m_Skill3Cooldown;
        public float AntiAirMissileCooldown => m_AntiAirMissileCooldown;

        public float Skill1Remaining => Mathf.Max(0f, m_Skill1Timer);
        public float Skill2Remaining => Mathf.Max(0f, m_Skill2Timer);
        public float Skill3Remaining => Mathf.Max(0f, m_Skill3Timer);
        public float AntiAirMissileRemaining => Mathf.Max(0f, m_AntiAirMissileTimer);

        public bool IsSkill1Ready => m_Skill1Timer <= 0f;
        public bool IsSkill2Ready => m_Skill2Timer <= 0f;
        public bool IsSkill3Ready => m_Skill3Timer <= 0f;
        public bool IsAntiAirMissileReady => m_AntiAirMissileTimer <= 0f;

        public bool IsAiming => m_IsAiming;
        public IUHRealtimeMonsterTarget LockedAirTarget => m_LockedAirTarget;

        public event Action OnSkillStateChanged;

        private Transform m_PlayerTransform;
        private Camera m_MainCamera;
        private IUHGuardianDroneController m_DroneController;
        private Animator m_PlayerAnimator;
        private GameObject m_RifleVisualObject;
        private bool m_IsMeleeOnly = false;
        private int m_PunchComboIndex = 0;
        private float m_NextPunchTime = 0f;

        private bool m_IsAiming = false;
        private IUHRealtimeMonsterTarget m_LockedAirTarget = null;
        private CinemachineCamera m_CinemachineCam;
        private CinemachineThirdPersonFollow m_CinemachineFollow;

        public bool IsMeleeOnly => m_IsMeleeOnly;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            m_MainCamera = Camera.main;
            FindPlayer();

            var droneGo = GameObject.Find("IUH_GUARDIAN_DRONE");
            if (droneGo != null) m_DroneController = droneGo.GetComponent<IUHGuardianDroneController>();

            if (m_MuzzleLight != null) m_MuzzleLight.enabled = false;
            if (m_LaserTracer != null) m_LaserTracer.enabled = false;

            m_RifleVisualObject = GameObject.Find("IUH_CYBER_PLASMA_RIFLE");

            var vcamGo = GameObject.Find("PlayerFollowCamera");
            if (vcamGo != null)
            {
                m_CinemachineCam = vcamGo.GetComponent<CinemachineCamera>();
                m_CinemachineFollow = vcamGo.GetComponent<CinemachineThirdPersonFollow>();
                if (m_CinemachineCam != null) m_NormalFov = m_CinemachineCam.Lens.FieldOfView;
            }
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null)
            {
                m_PlayerTransform = student.transform;
                m_PlayerAnimator = student.GetComponent<Animator>();
            }
        }

        private void Update()
        {
            // If Match-3 puzzle minigame is active, pause combat input & skills
            if (IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null && IUHCampus.Guardian.Match3.Match3CombatManager.Instance.IsCombatActive)
            {
                m_IsAiming = false;
                UpdateAimCamera();
                return;
            }

            // Decrement skill & missile cooldowns
            if (m_Skill1Timer > 0f) m_Skill1Timer -= Time.deltaTime;
            if (m_Skill2Timer > 0f) m_Skill2Timer -= Time.deltaTime;
            if (m_Skill3Timer > 0f) m_Skill3Timer -= Time.deltaTime;
            if (m_AntiAirMissileTimer > 0f) m_AntiAirMissileTimer -= Time.deltaTime;
            if (m_GrenadeTimer > 0f) m_GrenadeTimer -= Time.deltaTime;

            // Handle Aiming State (Holding Right Mouse Button on PC or Virtual Aim on Mobile)
            bool isMobileAim = IUHCampus.Core.IUHInputHelper.VirtualAimHeld || IUHCampus.Core.IUHInputHelper.VirtualAimToggled;
            m_IsAiming = ((Cursor.lockState == CursorLockMode.Locked) && Input.GetMouseButton(1)) || isMobileAim;
            UpdateAimCamera();
            UpdateAirLockOn();

            // Handle Input if not blocked by UI modals
            bool canCombat = (Cursor.lockState == CursorLockMode.Locked) || Application.isMobilePlatform || isMobileAim || IUHCampus.Core.IUHInputHelper.VirtualFireHeld;
            if (canCombat)
            {
                // Switch Weapon Mode: [X] or [Tab] or [0]
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Alpha0))
                {
                    ToggleMeleeMode();
                }

                // Quick Melee Punch: [V]
                if (Input.GetKeyDown(KeyCode.V))
                {
                    PerformMeleePunch();
                }

                // Dedicated Anti-Air Missile: [Q] or Mobile Virtual Missile
                if (Input.GetKeyDown(KeyCode.Q) || IUHCampus.Core.IUHInputHelper.VirtualMissilePressed)
                {
                    IUHCampus.Core.IUHInputHelper.VirtualMissilePressed = false;
                    UseAntiAirMissile();
                }

                // Timed Plasma Grenade (Explodes after 4s): [G]
                if (Input.GetKeyDown(KeyCode.G))
                {
                    ThrowTimedGrenade();
                }

                // Call Drone Emergency Supply: [T]
                if (Input.GetKeyDown(KeyCode.T))
                {
                    if (IUHEmergencyDroneSupplyManager.Instance != null)
                    {
                        IUHEmergencyDroneSupplyManager.Instance.CallManualSupply();
                    }
                }

                // Primary Attack: Left Click or [F] or Mobile Virtual Fire
                if (m_IsMeleeOnly)
                {
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F) || IUHCampus.Core.IUHInputHelper.VirtualFirePressed)
                    {
                        IUHCampus.Core.IUHInputHelper.VirtualFirePressed = false;
                        PerformMeleePunch();
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.F) || IUHCampus.Core.IUHInputHelper.VirtualFireHeld || IUHCampus.Core.IUHInputHelper.VirtualFirePressed)
                    {
                        IUHCampus.Core.IUHInputHelper.VirtualFirePressed = false;
                        TryFireGun();
                    }
                }

                // Skills
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                {
                    UseSkill1();
                }
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    UseSkill2();
                }
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                {
                    UseSkill3();
                }
            }
            else
            {
                m_IsAiming = false;
            }
        }

        // ================= MELEE COMBAT (ĐÁNH TAY KHÔNG) ================= //
        public void ToggleMeleeMode()
        {
            m_IsMeleeOnly = !m_IsMeleeOnly;
            if (m_RifleVisualObject == null) m_RifleVisualObject = GameObject.Find("IUH_CYBER_PLASMA_RIFLE");
            if (m_RifleVisualObject != null) m_RifleVisualObject.SetActive(!m_IsMeleeOnly);

            IUHCombatAudio.PlayPunchWhoosh();
            string msg = m_IsMeleeOnly ? "👊 ĐÃ CHUYỂN SANG: ĐÁNH TAY KHÔNG!\n[Chuột Trái / F / V] Tung Cú Đấm" : "🔫 ĐÃ RÚT: SÚNG CYBER PLASMA!\n[Chuột Trái / F] Bắn Đạn Thật | [V] Đấm Cận Chiến";
            Color col = m_IsMeleeOnly ? new Color(1f, 0.75f, 0.2f) : new Color(0.2f, 0.95f, 1f);
            if (m_PlayerTransform != null)
            {
                IUHCombatFloatingText.Spawn(m_PlayerTransform.position + Vector3.up * 2.2f, msg, col, 1.8f);
            }
        }

        public void PerformMeleePunch()
        {
            if (Time.time < m_NextPunchTime) return;
            m_NextPunchTime = Time.time + 0.35f;

            if (m_PlayerTransform == null) FindPlayer();
            if (m_PlayerTransform == null) return;

            if (m_PlayerAnimator == null) m_PlayerAnimator = m_PlayerTransform.GetComponent<Animator>();
            if (m_PlayerAnimator != null)
            {
                string animName = (m_PunchComboIndex % 2 == 0) ? "MeleePunch01" : "MeleePunch02";
                m_PlayerAnimator.CrossFadeInFixedTime(animName, 0.05f);
                m_PunchComboIndex++;
            }

            // Punch Whoosh Sound
            IUHCombatAudio.PlayPunchWhoosh();

            // Spawn Punch Arc Slash VFX in front of student
            SpawnPunchArcVFX(m_PlayerTransform.position + Vector3.up * 1.1f, m_PlayerTransform.forward);

            // Hit detection: cone / sphere in front of player
            Vector3 center = m_PlayerTransform.position + Vector3.up * 1.0f + m_PlayerTransform.forward * 1.3f;
            Collider[] hits = Physics.OverlapSphere(center, 1.8f);
            bool hasHitMonster = false;

            foreach (var col in hits)
            {
                if (col.transform == m_PlayerTransform || col.transform.IsChildOf(m_PlayerTransform)) continue;

                var monster = col.GetComponentInParent<IUHRealtimeMonsterTarget>();
                if (monster != null && !monster.IsDead)
                {
                    hasHitMonster = true;
                    int baseDmg = UnityEngine.Random.Range(42, 58);
                    bool isCrit = UnityEngine.Random.value < 0.35f;
                    int finalDmg = isCrit ? Mathf.RoundToInt(baseDmg * 1.8f) : baseDmg;

                    monster.TakeDamage(finalDmg, isCrit);

                    // Knockback monster slightly
                    var cc = monster.GetComponent<CharacterController>();
                    if (cc != null && cc.enabled)
                    {
                        cc.Move(m_PlayerTransform.forward * 0.9f);
                    }

                    // Impact hit VFX
                    SpawnPunchImpactVFX(col.ClosestPoint(center));
                }
            }

            if (hasHitMonster)
            {
                IUHCombatAudio.PlayPunchHit();
                IUHCameraShake.Shake(0.12f, 0.08f);
            }
        }

        private void SpawnPunchArcVFX(Vector3 origin, Vector3 forward)
        {
            GameObject arcGo = new GameObject("Punch_Arc_VFX", typeof(LineRenderer));
            arcGo.transform.position = origin;
            arcGo.transform.forward = forward;

            var lr = arcGo.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth = 0.18f;
            lr.endWidth = 0.02f;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.85f, 0.2f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.75f, 0.1f) * 3f);
            lr.sharedMaterial = mat;

            int segments = 12;
            lr.positionCount = segments;
            float radius = 1.2f;
            float angleSpan = 110f;
            float startAngle = -angleSpan / 2f;

            for (int i = 0; i < segments; i++)
            {
                float a = (startAngle + (angleSpan / (segments - 1)) * i) * Mathf.Deg2Rad;
                Vector3 pt = new Vector3(Mathf.Sin(a) * radius, 0f, Mathf.Cos(a) * radius);
                lr.SetPosition(i, pt);
            }

            Destroy(arcGo, 0.15f);
        }

        private void SpawnPunchImpactVFX(Vector3 hitPoint)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "Punch_Impact_Spark";
            spark.transform.position = hitPoint;
            spark.transform.localScale = Vector3.one * 0.45f;

            var rend = spark.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.6f, 0.1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 4f);
            rend.sharedMaterial = mat;

            Destroy(spark.GetComponent<Collider>());
            Destroy(spark, 0.2f);
        }

        // ================= 1. PRIMARY WEAPON FIRE ================= //
        public void TryFireGun()
        {
            if (Time.time < m_NextFireTime) return;
            m_NextFireTime = Time.time + m_FireRate;

            if (m_MainCamera == null) m_MainCamera = Camera.main;
            if (m_MainCamera == null) return;

            // Aim ray through screen center
            Ray ray = m_MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 aimTarget = ray.origin + ray.direction * m_MaxRange;
            RaycastHit hit;

            bool isCrit = UnityEngine.Random.value <= m_CritChance;
            int damage = m_BaseDamage + UnityEngine.Random.Range(-4, 6);
            if (isCrit) damage = Mathf.RoundToInt(damage * 1.8f);

            if (m_LockedAirTarget != null && !m_LockedAirTarget.IsDead)
            {
                aimTarget = m_LockedAirTarget.transform.position;
            }
            else if (Physics.Raycast(ray, out hit, m_MaxRange))
            {
                aimTarget = hit.point;
            }

            Vector3 muzzlePos = m_GunMuzzle != null ? m_GunMuzzle.position : (m_PlayerTransform != null ? m_PlayerTransform.position + Vector3.up * 1.2f : ray.origin);
            Vector3 shootDir = (aimTarget - muzzlePos).normalized;
            if (shootDir.sqrMagnitude < 0.001f) shootDir = ray.direction;

            // Spawn Real Physical Plasma Bullet
            GameObject bullet = CreatePhysicalBullet(muzzlePos, shootDir, damage, isCrit);

            // If an aerial enemy is locked, guide plasma bullet directly into it!
            if (m_LockedAirTarget != null && !m_LockedAirTarget.IsDead)
            {
                var bulletComp = bullet.GetComponent<IUHPlasmaBullet>();
                if (bulletComp != null)
                {
                    bulletComp.SetHomingTarget(m_LockedAirTarget.transform, 24f);
                }
            }

            // Muzzle flash light
            if (m_MuzzleLight != null)
            {
                StartCoroutine(MuzzleFlashRoutine());
            }

            // Sound
            IUHCombatAudio.PlayEnergy();
        }

        // ================= AIMING & ANTI-AIR TARGETING SYSTEM ================= //
        private void UpdateAimCamera()
        {
            if (m_CinemachineCam != null)
            {
                var lens = m_CinemachineCam.Lens;
                float targetFov = m_IsAiming ? m_AimFov : m_NormalFov;
                lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFov, Time.deltaTime * m_AimTransitionSpeed);
                m_CinemachineCam.Lens = lens;
            }
            else if (m_MainCamera != null)
            {
                float targetFov = m_IsAiming ? m_AimFov : m_NormalFov;
                m_MainCamera.fieldOfView = Mathf.Lerp(m_MainCamera.fieldOfView, targetFov, Time.deltaTime * m_AimTransitionSpeed);
            }

            if (m_CinemachineFollow != null)
            {
                Vector3 targetOffset = m_IsAiming ? new Vector3(0.65f, 0.45f, 0f) : new Vector3(0.5f, 0.45f, 0f);
                float targetDist = m_IsAiming ? 2.2f : 3.6f;
                m_CinemachineFollow.ShoulderOffset = Vector3.Lerp(m_CinemachineFollow.ShoulderOffset, targetOffset, Time.deltaTime * m_AimTransitionSpeed);
                m_CinemachineFollow.CameraDistance = Mathf.Lerp(m_CinemachineFollow.CameraDistance, targetDist, Time.deltaTime * m_AimTransitionSpeed);
            }

            // While aiming, smoothly rotate player character to face crosshair aim direction
            if (m_IsAiming && m_PlayerTransform != null && m_MainCamera != null)
            {
                Vector3 camFwd = m_MainCamera.transform.forward;
                camFwd.y = 0;
                if (camFwd.sqrMagnitude > 0.001f)
                {
                    m_PlayerTransform.rotation = Quaternion.Slerp(m_PlayerTransform.rotation, Quaternion.LookRotation(camFwd.normalized), Time.deltaTime * 18f);
                }

                // Magnetic Aim-Assist when aiming at locked flying monster
                if (m_LockedAirTarget != null && !m_LockedAirTarget.IsDead)
                {
                    var tpc = m_PlayerTransform.GetComponent<StarterAssets.ThirdPersonController>();
                    if (tpc != null)
                    {
                        Vector3 toTarget = (m_LockedAirTarget.transform.position - m_MainCamera.transform.position).normalized;
                        Vector3 fullCamFwd = m_MainCamera.transform.forward;
                        float yawDiff = Vector3.SignedAngle(new Vector3(fullCamFwd.x, 0, fullCamFwd.z), new Vector3(toTarget.x, 0, toTarget.z), Vector3.up);
                        float camPitch = -Mathf.Asin(Mathf.Clamp(fullCamFwd.y, -0.99f, 0.99f)) * Mathf.Rad2Deg;
                        float targetPitch = -Mathf.Asin(Mathf.Clamp(toTarget.y, -0.99f, 0.99f)) * Mathf.Rad2Deg;
                        float pitchDiff = Mathf.DeltaAngle(camPitch, targetPitch);
                        tpc.AddCameraRotation(yawDiff * Time.deltaTime * 4.5f, pitchDiff * Time.deltaTime * 4.5f);
                    }
                }
            }
        }

        private void UpdateAirLockOn()
        {
            m_LockedAirTarget = null;
            if (m_MainCamera == null) m_MainCamera = Camera.main;
            if (m_MainCamera == null) return;

            Ray ray = m_MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var allMonsters = FindObjectsByType<IUHRealtimeMonsterTarget>(FindObjectsSortMode.None);

            float bestScore = float.MaxValue;
            float maxAngle = m_IsAiming ? 30f : 20f;

            foreach (var m in allMonsters)
            {
                if (m.IsDead || !m.gameObject.activeInHierarchy) continue;

                // Is it an aerial target? (has IUHFlyingMonsterAI or height > 3.5m)
                bool isAir = m.GetComponent<IUHFlyingMonsterAI>() != null || (m.transform.position.y > 3.5f);
                if (!isAir) continue;

                Vector3 toTarget = (m.transform.position - ray.origin);
                float dist = toTarget.magnitude;
                if (dist > m_MaxRange) continue;

                float angle = Vector3.Angle(ray.direction, toTarget.normalized);
                if (angle <= maxAngle)
                {
                    float score = angle * 2f + dist * 0.1f;
                    if (score < bestScore)
                    {
                        bestScore = score;
                        m_LockedAirTarget = m;
                    }
                }
            }
        }

        public void UseAntiAirMissile()
        {
            if (!IsAntiAirMissileReady)
            {
                IUHCombatFloatingText.Spawn(m_PlayerTransform != null ? m_PlayerTransform.position + Vector3.up * 2f : transform.position, $"⏳ TÊN LỬA ĐANG NẠP LẠI ({m_AntiAirMissileTimer:0.0}s)!", new Color(1f, 0.4f, 0.2f), 0.8f);
                return;
            }

            m_AntiAirMissileTimer = m_AntiAirMissileCooldown;

            if (m_MainCamera == null) m_MainCamera = Camera.main;
            Vector3 muzzlePos = m_GunMuzzle != null ? m_GunMuzzle.position : (m_PlayerTransform != null ? m_PlayerTransform.position + Vector3.up * 1.4f : transform.position);

            Ray ray = m_MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 launchDir = (m_LockedAirTarget != null) ? (m_LockedAirTarget.transform.position - muzzlePos).normalized : ray.direction;

            // Create Anti-Air Rocket
            GameObject rocket = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rocket.name = "AntiAir_Homing_Rocket";
            rocket.transform.position = muzzlePos;
            rocket.transform.localScale = new Vector3(0.25f, 0.55f, 0.25f);
            rocket.transform.forward = launchDir;

            var rend = rocket.GetComponent<Renderer>();
            Material rMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            rMat.color = new Color(1f, 0.35f, 0.1f);
            rMat.EnableKeyword("_EMISSION");
            rMat.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.1f) * 4f);
            rend.sharedMaterial = rMat;

            var col = rocket.GetComponent<CapsuleCollider>();
            if (col != null) col.isTrigger = true;

            var trail = rocket.AddComponent<TrailRenderer>();
            trail.time = 0.4f;
            trail.startWidth = 0.25f;
            trail.endWidth = 0.04f;
            trail.sharedMaterial = rMat;

            var rocketComp = rocket.AddComponent<IUHAntiAirRocket>();
            rocketComp.Initialize(m_LockedAirTarget != null ? m_LockedAirTarget.transform : null, launchDir, m_AntiAirMissileDamage);

            // Audio & Feedback
            IUHCombatAudio.PlaySkillBurst();
            IUHCameraShake.Shake(0.18f, 0.12f);

            string hint = (m_LockedAirTarget != null) 
                ? $"🚀 PHÓNG TÊN LỬA TẦM NHIỆT KHÓA MỤC TIÊU: {m_LockedAirTarget.name}!" 
                : "🚀 PHÓNG TÊN LỬA PHÒNG KHÔNG FLAK!";
            IUHCombatFloatingText.Spawn(muzzlePos + Vector3.up * 1.2f, hint, new Color(1f, 0.6f, 0.1f), 1.8f);
        }

        public void ThrowTimedGrenade()
        {
            if (m_GrenadeTimer > 0f)
            {
                IUHCombatFloatingText.Spawn(m_PlayerTransform != null ? m_PlayerTransform.position + Vector3.up * 2f : transform.position,
                    $"⏳ ĐANG CHẾ TẠO BOM ({m_GrenadeTimer:0.0}s)!", new Color(1f, 0.5f, 0.2f), 0.8f);
                return;
            }

            m_GrenadeTimer = m_GrenadeCooldown;

            Vector3 spawnPos;
            Vector3 throwDir;
            if (m_MainCamera != null)
            {
                Vector3 camFwd = m_MainCamera.transform.forward;
                spawnPos = m_MainCamera.transform.position + camFwd * 1.2f + m_MainCamera.transform.right * 0.35f;
                throwDir = (camFwd + Vector3.up * 0.16f).normalized;
            }
            else
            {
                spawnPos = m_PlayerTransform != null ? m_PlayerTransform.position + Vector3.up * 1.5f + m_PlayerTransform.forward * 0.8f : transform.position;
                throwDir = (m_PlayerTransform != null ? m_PlayerTransform.forward : transform.forward) + Vector3.up * 0.2f;
            }

            // Create Grenade GameObject
            GameObject grenadeGo = new GameObject("IUH_Timed_Plasma_Grenade");
            grenadeGo.transform.position = spawnPos;

            var grenade = grenadeGo.AddComponent<IUHTimedGrenade>();
            Vector3 throwVelocity = throwDir * 18.5f;
            grenade.Initialize(throwVelocity, m_GrenadeFuseTime, m_GrenadeDamage, m_GrenadeRadius);

            // Audio & Screen Feedback
            IUHCombatAudio.PlayPunchHit();
            IUHCombatFloatingText.Spawn(spawnPos + Vector3.up * 0.4f, "💣 ĐÃ NÉM BOM HẸN GIỜ (NỔ SAU 4S)!", new Color(1f, 0.85f, 0.1f), 1.6f);
        }

        private IEnumerator MuzzleFlashRoutine()
        {
            m_MuzzleLight.enabled = true;
            yield return new WaitForSeconds(0.06f);
            m_MuzzleLight.enabled = false;
        }

        private GameObject CreatePhysicalBullet(Vector3 pos, Vector3 dir, int damage, bool isCrit)
        {
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Plasma_Bullet";
            bullet.transform.position = pos;
            bullet.transform.localScale = new Vector3(0.18f, 0.18f, 0.45f);
            bullet.transform.forward = dir;

            var rend = bullet.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            Color bulletColor = isCrit ? new Color(1f, 0.4f, 0.1f) : new Color(0.1f, 0.95f, 1f);
            mat.color = bulletColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", bulletColor * 5f);
            rend.sharedMaterial = mat;

            var col = bullet.GetComponent<SphereCollider>();
            if (col != null) { col.isTrigger = true; col.radius = 0.5f; }

            var light = bullet.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = bulletColor;
            light.intensity = 2.5f;
            light.range = 3.5f;

            var trail = bullet.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.15f;
            trail.endWidth = 0.02f;
            trail.sharedMaterial = mat;

            var bComp = bullet.AddComponent<IUHPlasmaBullet>();
            bComp.Initialize(dir, damage, isCrit);
            return bullet;
        }

        // ================= 2. SKILL 1: EMP SHOCKWAVE ================= //
        public void UseSkill1()
        {
            if (!IsSkill1Ready) return;
            m_Skill1Timer = m_Skill1Cooldown;

            if (m_PlayerTransform == null) FindPlayer();
            Vector3 origin = m_PlayerTransform != null ? m_PlayerTransform.position : transform.position;

            IUHCombatAudio.PlaySkillBurst();
            IUHCombatFloatingText.Spawn(origin + Vector3.up * 2f, "⚡ XUNG ĐIỆN PHÁ GIÁP EMP!", new Color(0.2f, 0.95f, 1f), 1.5f);

            // Find all monsters in radius
            var monsters = FindObjectsByType<IUHRealtimeMonsterTarget>(FindObjectsSortMode.None);
            foreach (var m in monsters)
            {
                if (m.IsDead) continue;
                float dist = Vector3.Distance(origin, m.transform.position);
                if (dist <= m_Skill1Radius)
                {
                    m.TakeDamage(m_Skill1Damage, isCrit: true);
                    m.ApplyStun(3.5f);
                }
            }

            // Spawn expanding ring VFX
            StartCoroutine(SpawnEmpRingRoutine(origin));
            OnSkillStateChanged?.Invoke();
        }

        private IEnumerator SpawnEmpRingRoutine(Vector3 center)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "EMP_Shockwave_Ring";
            ring.transform.position = center + Vector3.up * 0.1f;
            ring.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);

            var rend = ring.GetComponent<Renderer>();
            Material ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ringMat.color = new Color(0.1f, 0.9f, 1f, 0.6f);
            ringMat.EnableKeyword("_EMISSION");
            ringMat.SetColor("_EmissionColor", new Color(0.2f, 1f, 1f) * 3f);
            rend.sharedMaterial = ringMat;
            Destroy(ring.GetComponent<Collider>());

            float elapsed = 0f;
            float duration = 0.45f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float scale = Mathf.Lerp(1f, m_Skill1Radius * 2f, progress);
                ring.transform.localScale = new Vector3(scale, 0.02f, scale);
                ringMat.color = new Color(0.1f, 0.9f, 1f, (1f - progress) * 0.6f);
                yield return null;
            }

            Destroy(ring);
            Destroy(ringMat);
        }

        // ================= 3. SKILL 2: CYBER LASER BARRAGE ================= //
        public void UseSkill2()
        {
            if (!IsSkill2Ready) return;
            m_Skill2Timer = m_Skill2Cooldown;

            IUHCombatAudio.PlaySword();
            IUHCombatFloatingText.Spawn(m_PlayerTransform != null ? m_PlayerTransform.position + Vector3.up * 2f : transform.position, "💥 BÃO LAZER TRUY ĐUỔI!", new Color(1f, 0.4f, 0.9f), 1.5f);

            StartCoroutine(LaserBarrageRoutine());
            OnSkillStateChanged?.Invoke();
        }

        private IEnumerator LaserBarrageRoutine()
        {
            var monsters = new List<IUHRealtimeMonsterTarget>(FindObjectsByType<IUHRealtimeMonsterTarget>(FindObjectsSortMode.None));
            monsters.RemoveAll(m => m.IsDead || !m.gameObject.activeInHierarchy);

            // Prioritize aerial flying monsters so barrage swarms up into the sky!
            monsters.Sort((a, b) =>
            {
                bool aAir = a.GetComponent<IUHFlyingMonsterAI>() != null || a.transform.position.y > 3.5f;
                bool bAir = b.GetComponent<IUHFlyingMonsterAI>() != null || b.transform.position.y > 3.5f;
                if (aAir && !bAir) return -1;
                if (!aAir && bAir) return 1;
                return 0;
            });

            Vector3 startPos = m_GunMuzzle != null ? m_GunMuzzle.position : (m_PlayerTransform != null ? m_PlayerTransform.position + Vector3.up * 1.5f : transform.position);

            for (int i = 0; i < m_Skill2MissilesCount; i++)
            {
                IUHCombatAudio.PlayEnergy();

                // Target closest monster (or aerial priority) or forward direction
                IUHRealtimeMonsterTarget target = null;
                float closestDist = float.MaxValue;

                foreach (var m in monsters)
                {
                    if (m.IsDead || !m.gameObject.activeInHierarchy) continue;
                    float dist = Vector3.Distance(startPos, m.transform.position);
                    bool isAir = m.GetComponent<IUHFlyingMonsterAI>() != null || m.transform.position.y > 3.5f;
                    float allowedDist = isAir ? 65f : 35f;

                    if (dist < closestDist && dist <= allowedDist)
                    {
                        closestDist = dist;
                        target = m;
                    }
                }

                Vector3 targetPos = target != null ? target.transform.position + Vector3.up * 1.2f : startPos + (m_MainCamera != null ? m_MainCamera.transform.forward * 20f : Vector3.forward * 20f);
                targetPos += UnityEngine.Random.insideUnitSphere * 0.5f;

                StartCoroutine(ShowLaserTracer(startPos, targetPos));

                if (target != null)
                {
                    target.TakeDamage(m_Skill2DamagePerShot, isCrit: UnityEngine.Random.value < 0.35f);
                }

                yield return new WaitForSeconds(0.08f);
            }
        }

        private IEnumerator ShowLaserTracer(Vector3 from, Vector3 to)
        {
            GameObject lineGo = new GameObject("LaserTracer", typeof(LineRenderer));
            LineRenderer lr = lineGo.GetComponent<LineRenderer>();
            lr.startWidth = 0.08f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            lr.material.color = new Color(0.2f, 1f, 0.4f);
            lr.material.EnableKeyword("_EMISSION");
            lr.material.SetColor("_EmissionColor", new Color(0.2f, 1f, 0.4f) * 3f);
            lr.positionCount = 2;
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);

            yield return new WaitForSeconds(0.06f);
            Destroy(lineGo);
        }

        // ================= 4. SKILL 3: ULTIMATE DRONE STRIKE ================= //
        public void UseSkill3()
        {
            if (!IsSkill3Ready) return;
            m_Skill3Timer = m_Skill3Cooldown;

            if (m_MainCamera == null) m_MainCamera = Camera.main;

            // Target ground in front of player
            Ray ray = m_MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetGround = ray.origin + ray.direction * 15f;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 40f))
            {
                targetGround = hit.point;
            }

            IUHCombatAudio.PlayCombo(6);
            IUHCombatFloatingText.Spawn(targetGround + Vector3.up * 2f, "🚀 CHIÊU CUỐI: KHÔNG KÍCH QUỸ ĐẠO!", new Color(1f, 0.85f, 0.1f), 2f);

            StartCoroutine(DroneStrikeRoutine(targetGround));
            OnSkillStateChanged?.Invoke();
        }

        private IEnumerator DroneStrikeRoutine(Vector3 targetGround)
        {
            // Red targeting laser beacon
            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "Targeting_Laser_Beacon";
            beacon.transform.position = targetGround + Vector3.up * 10f;
            beacon.transform.localScale = new Vector3(0.25f, 10f, 0.25f);
            var bRend = beacon.GetComponent<Renderer>();
            Material bMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bMat.color = new Color(1f, 0.1f, 0.2f, 0.8f);
            bMat.EnableKeyword("_EMISSION");
            bMat.SetColor("_EmissionColor", Color.red * 4f);
            bRend.sharedMaterial = bMat;
            Destroy(beacon.GetComponent<Collider>());

            // Siren chime
            IUHCombatAudio.PlayBossHit();

            yield return new WaitForSeconds(0.9f);

            // EXPLOSION!
            IUHCombatAudio.PlaySkillBurst();
            IUHCombatAudio.PlayBossRoar();

            Destroy(beacon);
            Destroy(bMat);

            // Mega shockwave explosion sphere
            GameObject boom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boom.name = "Ultimate_Nuke_Sphere";
            boom.transform.position = targetGround + Vector3.up * 1f;
            boom.transform.localScale = Vector3.one * 2f;
            var boomRend = boom.GetComponent<Renderer>();
            Material boomMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            boomMat.color = new Color(1f, 0.9f, 0.2f, 0.9f);
            boomMat.EnableKeyword("_EMISSION");
            boomMat.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.1f) * 6f);
            boomRend.sharedMaterial = boomMat;
            Destroy(boom.GetComponent<Collider>());

            // Damage all monsters in radius
            var monsters = FindObjectsByType<IUHRealtimeMonsterTarget>(FindObjectsSortMode.None);
            foreach (var m in monsters)
            {
                if (m.IsDead) continue;
                float d = Vector3.Distance(targetGround, m.transform.position);
                if (d <= m_Skill3Radius)
                {
                    m.TakeDamage(m_Skill3Damage, isCrit: true);
                    m.ApplyStun(4.0f);
                }
            }

            // Animate blast expansion & screen shake
            float elapsed = 0f;
            float dur = 0.6f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / dur;
                float s = Mathf.Lerp(2f, m_Skill3Radius * 2f, p);
                boom.transform.localScale = Vector3.one * s;
                boomMat.color = new Color(1f, 0.8f, 0.2f, (1f - p));
                yield return null;
            }

            Destroy(boom);
            Destroy(boomMat);
        }
    }
}
