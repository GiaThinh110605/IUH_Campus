using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.DailyActivities
{
    public class IUHMysteryGift : MonoBehaviour
    {
        [Header("Mystery Gift Configuration")]
        [SerializeField] private float m_InteractRadius = 2.8f;
        [SerializeField] private KeyCode m_InteractKey = KeyCode.E;

        [Header("Visual Components")]
        [SerializeField] private Transform m_LidTransform;
        [SerializeField] private GameObject m_PromptBillboard;
        [SerializeField] private Light m_AuraLight;
        [SerializeField] private ParticleSystem m_GlowParticles;

        private Transform m_PlayerTransform;
        private bool m_IsOpened = false;
        private bool m_IsOpening = false;
        private float m_OpenProgress = 0f;

        private void Start()
        {
            FindPlayer();

            if (IUHCampusDailyActivitiesManager.Instance != null && IUHCampusDailyActivitiesManager.Instance.MysteryGiftOpened)
            {
                SetOpenedVisuals(instant: true);
            }
            else
            {
                if (m_PromptBillboard != null) m_PromptBillboard.SetActive(false);
            }
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (m_IsOpening)
            {
                m_OpenProgress += Time.deltaTime * 3f;
                if (m_LidTransform != null)
                {
                    m_LidTransform.localRotation = Quaternion.Euler(Mathf.Lerp(0f, -70f, m_OpenProgress), 0f, 0f);
                }
                if (m_OpenProgress >= 1f)
                {
                    m_IsOpening = false;
                }
                return;
            }

            if (m_IsOpened) return;

            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
            bool inRange = dist <= m_InteractRadius;

            if (m_PromptBillboard != null && m_PromptBillboard.activeSelf != inRange)
            {
                m_PromptBillboard.SetActive(inRange);
            }

            if (inRange && Input.GetKeyDown(m_InteractKey))
            {
                Open();
            }
        }

        public void Open()
        {
            if (m_IsOpened) return;
            m_IsOpened = true;
            m_IsOpening = true;
            m_OpenProgress = 0f;

            if (m_PromptBillboard != null) m_PromptBillboard.SetActive(false);

            if (m_AuraLight != null)
            {
                m_AuraLight.color = new Color(1f, 0.4f, 0.9f);
                m_AuraLight.intensity = 2.5f;
            }

            if (m_GlowParticles != null)
            {
                m_GlowParticles.Play();
            }

            if (IUHCampusDailyActivitiesManager.Instance != null)
            {
                IUHCampusDailyActivitiesManager.Instance.OpenMysteryGift(transform.position, (rewardMsg) =>
                {
                    // Callback if needed
                });
            }
        }

        private void SetOpenedVisuals(bool instant)
        {
            m_IsOpened = true;
            if (m_LidTransform != null) m_LidTransform.localRotation = Quaternion.Euler(-70f, 0f, 0f);
            if (m_PromptBillboard != null) m_PromptBillboard.SetActive(false);
            if (m_AuraLight != null) m_AuraLight.intensity = 0.5f;
        }
    }
}

