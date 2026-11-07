using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHCampusZoneDirector : MonoBehaviour
    {
        public static IUHCampusZoneDirector Instance { get; private set; }

        [Header("Sky Drone Predators")]
        [SerializeField] private GameObject m_SkyDroneNorth;    // SKY_DRONE_PREDATOR_1 (North Challenge Arena)
        [SerializeField] private GameObject m_SkyDroneEast;     // SKY_DRONE_PREDATOR_2 (Building E)
        [SerializeField] private GameObject m_SkyDroneWest;     // SKY_DRONE_PREDATOR_3 (Canteen/Garden)

        [Header("Specific Danger Combat Zones")]
        // Zone 1: Tòa Nhà Kỹ Thuật E
        [SerializeField] private Vector3 m_BuildingECenter = new Vector3(18f, 0.1f, -15f);
        [SerializeField] private float m_BuildingERadius = 12.0f;

        // Zone 2: Khu Căn Tin Sinh Viên
        [SerializeField] private Vector3 m_CanteenCenter = new Vector3(-22f, 0.1f, -18f);
        [SerializeField] private float m_CanteenRadius = 12.0f;

        // Zone 3: Sân Đấu Thử Thách Phía Sau Tòa Nhà (North Combat Arena)
        [SerializeField] private Vector3 m_NorthArenaCenter = new Vector3(0f, 0.1f, 16f);
        [SerializeField] private float m_NorthArenaRadius = 14.0f;

        [Header("Zone State")]
        private bool m_InBuildingE = false;
        private bool m_InCanteen = false;
        private bool m_InNorthArena = false;

        private Transform m_PlayerTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            FindPlayerAndDrones();

            // All drones MUST start completely inactive across the entire campus
            if (m_SkyDroneNorth != null) m_SkyDroneNorth.SetActive(false);
            if (m_SkyDroneEast != null) m_SkyDroneEast.SetActive(false);
            if (m_SkyDroneWest != null) m_SkyDroneWest.SetActive(false);
        }

        public void FindPlayerAndDrones()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;

            if (m_SkyDroneNorth == null) m_SkyDroneNorth = GameObject.Find("SKY_DRONE_PREDATOR_1");
            if (m_SkyDroneEast == null) m_SkyDroneEast = GameObject.Find("SKY_DRONE_PREDATOR_2");
            if (m_SkyDroneWest == null) m_SkyDroneWest = GameObject.Find("SKY_DRONE_PREDATOR_3");
        }

        private void Update()
        {
            // If Match-3 minigame is active, pause combat drones
            if (IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null && IUHCampus.Guardian.Match3.Match3CombatManager.Instance.IsCombatActive)
            {
                return;
            }

            if (m_PlayerTransform == null)
            {
                FindPlayerAndDrones();
                if (m_PlayerTransform == null) return;
            }

            // Optional Testing Shortcut [K]: Instantly summon a Sky Monster right in front of you
            if (Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.K))
            {
                SummonSkyMonsterDirectlyOverhead();
            }

            // Only activate drones when player is INSIDE a specific designated zone
            UpdateCombatZone(m_BuildingECenter, m_BuildingERadius, m_SkyDroneEast, "TÒA NHÀ KỸ THUẬT E", ref m_InBuildingE, "BUILDING_E_AMBUSH");
            UpdateCombatZone(m_CanteenCenter, m_CanteenRadius, m_SkyDroneWest, "KHU CĂN TIN SINH VIÊN", ref m_InCanteen, "CANTEEN_AMBUSH");
            UpdateCombatZone(m_NorthArenaCenter, m_NorthArenaRadius, m_SkyDroneNorth, "SÂN ĐẤU THỬ THÁCH PHÍA BẮC", ref m_InNorthArena, null);
        }

        private void UpdateCombatZone(Vector3 center, float radius, GameObject drone, string zoneName, ref bool wasInside, string optionalGroundAmbush)
        {
            if (m_PlayerTransform == null) return;

            float dist = Vector2.Distance(new Vector2(m_PlayerTransform.position.x, m_PlayerTransform.position.z),
                                          new Vector2(center.x, center.z));
            bool isInside = (dist <= radius);

            if (isInside && !wasInside)
            {
                wasInside = true;
                if (drone != null)
                {
                    var hp = drone.GetComponent<IUHRealtimeMonsterTarget>();
                    if (hp != null && hp.IsDead)
                    {
                        hp.ResetHealth();
                    }

                    drone.transform.position = center + Vector3.up * 3.6f;
                    drone.SetActive(true);

                    var ai = drone.GetComponent<IUHFlyingMonsterAI>();
                    if (ai != null) ai.SetZoneAnchor(center);

                    IUHCombatAudio.PlayBossRoar();
                    IUHCombatFloatingText.Spawn(center + Vector3.up * 2.5f,
                        $"⚠️ BƯỚC VÀO VÙNG CHIẾN SỰ: {zoneName}!\nPHI CƠ MÃ ĐỘC XUẤT HIỆN!", new Color(1f, 0.25f, 0.2f), 3.0f);
                }

                if (!string.IsNullOrEmpty(optionalGroundAmbush))
                {
                    var ambush = GameObject.Find(optionalGroundAmbush);
                    if (ambush != null)
                    {
                        var spawner = ambush.GetComponent<IUHBuildingMonsterSpawner>();
                        if (spawner != null) spawner.TriggerAmbush();
                    }
                }
            }
            else if (!isInside && wasInside)
            {
                wasInside = false;
                if (drone != null && drone.activeSelf)
                {
                    drone.SetActive(false);
                    IUHCombatFloatingText.Spawn(m_PlayerTransform.position + Vector3.up * 2.0f,
                        $"🌿 ĐÃ RỜI KHỎI {zoneName}\nQUÁI VẬT TRÊN KHÔNG ĐÃ RÚT LUI!", new Color(0.2f, 0.95f, 0.4f), 2.5f);
                }
            }
        }

        public void SummonSkyMonsterDirectlyOverhead()
        {
            if (m_PlayerTransform == null) FindPlayerAndDrones();
            if (m_PlayerTransform == null) return;

            GameObject targetDrone = m_SkyDroneNorth ?? m_SkyDroneEast ?? m_SkyDroneWest;
            if (targetDrone != null)
            {
                targetDrone.SetActive(false);
                var hp = targetDrone.GetComponent<IUHRealtimeMonsterTarget>();
                if (hp != null)
                {
                    hp.ResetHealth();
                }

                targetDrone.transform.position = m_PlayerTransform.position + m_PlayerTransform.forward * 7.0f + Vector3.up * 3.6f;
                targetDrone.SetActive(true);

                var ai = targetDrone.GetComponent<IUHFlyingMonsterAI>();
                if (ai != null) ai.SetZoneAnchor(m_PlayerTransform.position);

                IUHCombatAudio.PlaySkillBurst();
                IUHCombatAudio.PlayBossRoar();
                IUHCombatFloatingText.Spawn(m_PlayerTransform.position + Vector3.up * 2.5f,
                    "🚨 [PHÍM K] ĐÃ TRIỆU HỒI PHI CƠ MÃ ĐỘC TRÊN ĐẦU BẠN!\nHÃY BẮN [Chuột Trái / F] HOẶC PHÓNG TÊN LỬA [Q]!", new Color(1f, 0.2f, 0.2f), 3.0f);
            }
        }
    }
}
