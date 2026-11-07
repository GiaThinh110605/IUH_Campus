using UnityEngine;
using StarterAssets;

namespace IUHCampus.Guardian
{
    public class IUHTechKioskInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float m_InteractRadius = 3.5f;
        [SerializeField] private KeyCode m_InteractKey = KeyCode.E;
        [SerializeField] private GameObject m_PromptObject;

        private Transform m_PlayerTransform;

        private void Start()
        {
            FindPlayer();
            if (m_PromptObject != null) m_PromptObject.SetActive(false);
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

            float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
            bool inRange = dist <= m_InteractRadius;

            if (m_PromptObject != null && m_PromptObject.activeSelf != inRange)
            {
                m_PromptObject.SetActive(inRange);
            }

            if (inRange && Input.GetKeyDown(m_InteractKey))
            {
                if (IUHUpgradeShopUI.Instance != null)
                {
                    IUHUpgradeShopUI.Instance.OpenShop(true);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_InteractRadius);
        }
    }
}

