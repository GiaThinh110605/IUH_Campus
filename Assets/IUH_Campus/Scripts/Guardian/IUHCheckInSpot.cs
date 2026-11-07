using System;
using UnityEngine;

namespace IUHCampus.Guardian
{
    public class IUHCheckInSpot : MonoBehaviour
    {
        [Header("Spot Details")]
        [SerializeField] private string m_SpotId = "SPOT_FOUNTAIN";
        [SerializeField] private string m_SpotName = "Đài Phun Nước Trung Tâm IUH";
        [SerializeField] private int m_RewardGold = 30;
        [SerializeField] private string m_SouvenirBadge = "Huy Hiệu Check-in: Đài Phun Nước IUH";
        [SerializeField] private float m_Radius = 2.8f;
        [SerializeField] private float m_DwellTimeRequired = 1.2f;

        [Header("Visuals")]
        [SerializeField] private GameObject m_HoloRing;
        [SerializeField] private GameObject m_BillboardPrompt;

        private Transform m_PlayerTransform;
        private float m_CurrentDwell = 0f;
        private bool m_HasCompleted = false;

        private void Start()
        {
            FindPlayer();
            m_HasCompleted = PlayerPrefs.GetInt("IUH_CHECKIN_" + m_SpotId, 0) == 1;
            if (m_BillboardPrompt != null) m_BillboardPrompt.SetActive(false);
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            float dist = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                          new Vector3(m_PlayerTransform.position.x, 0f, m_PlayerTransform.position.z));
            bool isInside = dist <= m_Radius;

            if (m_BillboardPrompt != null && m_BillboardPrompt.activeSelf != isInside)
            {
                m_BillboardPrompt.SetActive(isInside);
            }

            if (isInside)
            {
                m_CurrentDwell += Time.deltaTime;
                if (m_CurrentDwell >= m_DwellTimeRequired)
                {
                    TriggerCheckIn();
                    m_CurrentDwell = -5.0f; // delay before next re-trigger
                }
            }
            else
            {
                m_CurrentDwell = 0f;
            }
        }

        private void TriggerCheckIn()
        {
            // Trigger shutter flash effect on HUD
            IUHCampus.DailyActivities.IUHCampusActivitiesHUD.Instance?.TriggerCameraFlash();

            // Connect to Daily Activities Manager & Album
            if (IUHCampus.DailyActivities.IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampus.DailyActivities.IUHCampusDailyActivitiesManager.Instance.TriggerCheckInSpot(m_SpotId, transform.position);
                m_HasCompleted = true;
                return;
            }

            // Audio & VFX
            IUHCombatAudio.PlayShield(); // Camera shutter click vibe
            IUHCombatAudio.PlayCoin();

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.8f, $"📸 CHECK-IN: {m_SpotName}!", new Color(0.3f, 0.95f, 1f), 1.2f);

            if (!m_HasCompleted)
            {
                m_HasCompleted = true;
                PlayerPrefs.SetInt("IUH_CHECKIN_" + m_SpotId, 1);
                PlayerPrefs.Save();

                if (IUHGuardianInventory.Instance != null)
                {
                    IUHGuardianInventory.Instance.AddGold(m_RewardGold);
                    IUHGuardianInventory.Instance.AddItem(m_SouvenirBadge);
                }

                IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.1f, $"✨ +{m_RewardGold} Vàng & {m_SouvenirBadge}", new Color(1f, 0.9f, 0.2f), 1.1f);
            }
        }
    }
}

