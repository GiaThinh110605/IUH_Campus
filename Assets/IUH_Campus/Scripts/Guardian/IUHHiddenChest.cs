using System;
using UnityEngine;
using StarterAssets;

namespace IUHCampus.Guardian
{
    public class IUHHiddenChest : MonoBehaviour
    {
        [Header("Chest Configuration")]
        [SerializeField] private string m_ChestId = "CHEST_CANTEEN";
        [SerializeField] private string m_ChestName = "Rương Kho Báu Căn Tin";
        [SerializeField] private int m_RewardGold = 50;
        [SerializeField] private string m_RewardItem = "Bình Dược Liệu Nano (+50 HP)";
        [SerializeField] private float m_InteractRadius = 3.0f;
        [SerializeField] private KeyCode m_InteractKey = KeyCode.E;

        [Header("Components")]
        [SerializeField] private Transform m_LidTransform;
        [SerializeField] private GameObject m_PromptObject;
        [SerializeField] private Light m_AuraLight;

        private bool m_IsOpened = false;
        private Transform m_PlayerTransform;
        private bool m_IsOpening = false;
        private float m_OpenProgress = 0f;

        private void Start()
        {
            FindPlayer();
            m_IsOpened = PlayerPrefs.GetInt("IUH_CHEST_" + m_ChestId, 0) == 1;

            if (m_IsOpened)
            {
                if (m_LidTransform != null) m_LidTransform.localRotation = Quaternion.Euler(-65f, 0f, 0f);
                if (m_AuraLight != null) m_AuraLight.enabled = false;
                if (m_PromptObject != null) m_PromptObject.SetActive(false);
            }
            else
            {
                if (m_PromptObject != null) m_PromptObject.SetActive(false);
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
                m_OpenProgress += Time.deltaTime * 3.5f;
                if (m_LidTransform != null)
                {
                    m_LidTransform.localRotation = Quaternion.Euler(Mathf.Lerp(0f, -65f, m_OpenProgress), 0f, 0f);
                }
                if (m_OpenProgress >= 1f) m_IsOpening = false;
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

            if (m_PromptObject != null && m_PromptObject.activeSelf != inRange)
            {
                m_PromptObject.SetActive(inRange);
            }

            if (inRange && Input.GetKeyDown(m_InteractKey))
            {
                OpenChest();
            }
        }

        public void OpenChest()
        {
            if (m_IsOpened) return;
            m_IsOpened = true;
            m_IsOpening = true;
            m_OpenProgress = 0f;

            PlayerPrefs.SetInt("IUH_CHEST_" + m_ChestId, 1);
            PlayerPrefs.Save();

            if (m_PromptObject != null) m_PromptObject.SetActive(false);
            if (m_AuraLight != null) m_AuraLight.color = new Color(0.2f, 1f, 0.4f);

            // Reward Gold & Item
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(m_RewardGold);
                if (!string.IsNullOrEmpty(m_RewardItem))
                {
                    IUHGuardianInventory.Instance.AddItem(m_RewardItem);
                }
            }

            // Audio & VFX
            IUHCombatAudio.PlayCoin();
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.5f, $"🎁 +{m_RewardGold} VÀNG!", new Color(1f, 0.9f, 0.2f), 1.25f);
            if (!string.IsNullOrEmpty(m_RewardItem))
            {
                IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.8f, $"✨ Nhận: {m_RewardItem}", new Color(0.3f, 0.9f, 1f), 1.0f);
            }
        }
    }
}

