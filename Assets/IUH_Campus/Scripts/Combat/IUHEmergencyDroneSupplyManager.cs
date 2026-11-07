using System;
using System.Collections;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHEmergencyDroneSupplyManager : MonoBehaviour
    {
        public static IUHEmergencyDroneSupplyManager Instance { get; private set; }

        [Header("Supply Configuration")]
        [SerializeField] private float m_AutoSupplyThreshold = 0.35f; // <= 35% HP
        [SerializeField] private float m_SupplyCooldown = 30.0f;
        [SerializeField] private float m_ReviveCooldown = 50.0f;

        private float m_SupplyCooldownTimer = 0f;
        private float m_ReviveCooldownTimer = 0f;
        private Transform m_PlayerTransform;
        private GameObject m_GuardianDrone;

        public float SupplyCooldownRemaining => Mathf.Max(0f, m_SupplyCooldownTimer);
        public bool IsSupplyReady => m_SupplyCooldownTimer <= 0f;
        public bool IsReviveReady => m_ReviveCooldownTimer <= 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            FindPlayerAndDrone();
        }

        private void FindPlayerAndDrone()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;

            m_GuardianDrone = GameObject.Find("IUH_GUARDIAN_DRONE");
        }

        private void Update()
        {
            if (m_SupplyCooldownTimer > 0f) m_SupplyCooldownTimer -= Time.deltaTime;
            if (m_ReviveCooldownTimer > 0f) m_ReviveCooldownTimer -= Time.deltaTime;

            // Manual Call for Drone Supply: [T] or [G]
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    CallManualSupply();
                }
            }
        }

        public void CallManualSupply()
        {
            if (!IsSupplyReady)
            {
                if (m_PlayerTransform != null)
                {
                    IUHCombatFloatingText.Spawn(m_PlayerTransform.position + Vector3.up * 2f, 
                        $"⏳ DRONE TIẾP TẾ ĐANG NẠP LẠI ({m_SupplyCooldownTimer:0.0}s)!", new Color(1f, 0.4f, 0.2f), 1.0f);
                }
                return;
            }

            TriggerDroneDrop("📦 SINH VIÊN GỌI TIẾP TẾ KHẨN CẤP!");
        }

        public void OnPlayerHealthChanged(int currentHp, int maxHp)
        {
            if (currentHp <= 0) return;

            float ratio = (float)currentHp / Mathf.Max(1, maxHp);
            if (ratio <= m_AutoSupplyThreshold && IsSupplyReady)
            {
                TriggerDroneDrop("🚨 CẢNH BÁO MÁU YẾU! DRONE TỰ ĐỘNG THẢ HÒM CỨU THƯƠNG!");
            }
        }

        public bool TryEmergencyRevive()
        {
            if (!IsReviveReady) return false;

            m_ReviveCooldownTimer = m_ReviveCooldown;
            m_SupplyCooldownTimer = m_SupplyCooldown;

            if (m_PlayerTransform == null) FindPlayerAndDrone();
            Vector3 pos = m_PlayerTransform != null ? m_PlayerTransform.position : Vector3.zero;

            // Immediate revive burst
            IUHCombatAudio.PlaySkillBurst();
            IUHCombatAudio.PlayHeal();
            IUHCameraShake.Shake(0.35f, 0.25f);

            IUHCombatFloatingText.Spawn(pos + Vector3.up * 2.5f, 
                "🚁 [DRONE IUH] KÍCH HOẠT HỒI SINH TIẾP TẾ KHẨN CẤP!", new Color(0.1f, 1f, 0.4f), 3.5f);

            // Spawn immediate supply crate right at feet
            SpawnCrate(pos + Vector3.up * 4.5f);

            return true;
        }

        public void TriggerDroneDrop(string notificationTitle)
        {
            m_SupplyCooldownTimer = m_SupplyCooldown;

            if (m_PlayerTransform == null) FindPlayerAndDrone();
            if (m_PlayerTransform == null) return;

            Vector3 playerPos = m_PlayerTransform.position;
            Vector3 dropOrigin = playerPos + m_PlayerTransform.forward * 2.5f + Vector3.up * 14f;

            // Audio & Drone Announcement
            IUHCombatAudio.PlaySkillBurst();
            IUHCombatFloatingText.Spawn(playerPos + Vector3.up * 2.2f, notificationTitle, new Color(0.1f, 1f, 0.4f), 2.2f);

            // Spawn Crate
            SpawnCrate(dropOrigin);

            // Animate Guardian Drone flying overhead if exists
            if (m_GuardianDrone != null)
            {
                StartCoroutine(DroneFlyoverRoutine(playerPos));
            }
        }

        private void SpawnCrate(Vector3 origin)
        {
            GameObject crateGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crateGo.name = "Emergency_Supply_Crate";
            crateGo.transform.position = origin;
            crateGo.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            var col = crateGo.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = true;

            var crateComp = crateGo.AddComponent<IUHEmergencySupplyCrate>();
            crateComp.Initialize(origin, m_PlayerTransform);
        }

        private IEnumerator DroneFlyoverRoutine(Vector3 playerPos)
        {
            Vector3 startPos = playerPos - Vector3.forward * 18f + Vector3.up * 8f;
            Vector3 endPos = playerPos + Vector3.forward * 20f + Vector3.up * 8f;

            float elapsed = 0f;
            float duration = 2.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;
                if (m_GuardianDrone != null)
                {
                    m_GuardianDrone.transform.position = Vector3.Lerp(startPos, endPos, p);
                    m_GuardianDrone.transform.LookAt(endPos);
                }
                yield return null;
            }
        }
    }
}

