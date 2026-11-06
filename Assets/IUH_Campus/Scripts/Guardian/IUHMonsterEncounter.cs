using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian.Match3;
using IUHCampus.Minigames.GrabCampus;
using StarterAssets;

namespace IUHCampus.Guardian
{
    public class IUHMonsterEncounter : MonoBehaviour
    {
        [Header("Encounter Settings")]
        [SerializeField] private string m_MonsterName = "DATA CORRUPTION";
        [SerializeField] private int m_MonsterHp = 800;
        [SerializeField] private float m_RevealRadius = 16.0f;
        [SerializeField] private float m_TriggerRadius = 10.0f;
        [SerializeField] private KeyCode m_InteractKey = KeyCode.E;

        [Header("Combat Balance")]
        [SerializeField] private int m_BossMinDmg = 65;
        [SerializeField] private int m_BossMaxDmg = 90;
        [SerializeField] private float m_CombatDuration = 90f;

        [Header("Rewards on Victory")]
        [SerializeField] private int m_RewardGold = 100;
        [SerializeField] private string[] m_RewardItems = new string[] { "Bình Dược Liệu Nano (+50 HP)", "Tinh Thể Dữ Liệu" };

        [Header("Overhead UI References")]
        [SerializeField] private GameObject m_MonsterOverheadCanvas;
        [SerializeField] private Image m_BossHpFill;
        [SerializeField] private Text m_BossHpText;
        [SerializeField] private Text m_BossIntentText;
        [SerializeField] private GameObject m_ActionPromptObject;
        [SerializeField] private GameObject m_AuraEffect;

        [Header("Manager")]
        [SerializeField] private Match3CombatManager m_CombatManager;

        private Transform m_PlayerTransform;
        private ThirdPersonController m_ThirdPersonController;
        private StarterAssetsInputs m_StarterInputs;
        private IUHGrabMotorbike m_MotorbikeController;
        private bool m_MotorbikeWasEnabled = true;
        private bool m_IsEncounterActive;
        private bool m_IsDefeated;
        private float m_TriggerCooldown = 0f;

        public string MonsterName => m_MonsterName;
        public int MonsterHp => m_MonsterHp;
        public int BossMinDmg => m_BossMinDmg;
        public int BossMaxDmg => m_BossMaxDmg;
        public float CombatDuration => m_CombatDuration;
        public int RewardGold => m_RewardGold;
        public string[] RewardItems => m_RewardItems;
        public Transform PlayerTransform => m_PlayerTransform;
        public Image BossHpFill => m_BossHpFill;
        public Text BossHpText => m_BossHpText;
        public Text BossIntentText => m_BossIntentText;

        public string GetRewardsFormatted()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<color=#FDE047><b>💰 +{m_RewardGold} Vàng & Bạc IUH</b></color>");
            if (m_RewardItems != null)
            {
                foreach (var item in m_RewardItems)
                {
                    sb.AppendLine($"<color=#67E8F9>✨ {item}</color>");
                }
            }
            return sb.ToString().TrimEnd();
        }

        private void Start()
        {
            FindPlayer();

            if (m_CombatManager == null)
            {
                m_CombatManager = FindAnyObjectByType<Match3CombatManager>(FindObjectsInactive.Include);
            }

            if (m_MonsterOverheadCanvas != null)
            {
                m_MonsterOverheadCanvas.SetActive(false);
            }

            if (m_ActionPromptObject != null)
            {
                m_ActionPromptObject.SetActive(false);
            }
        }

        private void FindPlayer()
        {
            // The bike keeps reading input even while the student rides it, so it must be paused in combat too.
            if (m_MotorbikeController == null)
            {
                m_MotorbikeController = FindAnyObjectByType<IUHGrabMotorbike>(FindObjectsInactive.Include);
            }

            var student = GameObject.Find("Campus Student");
            if (student != null)
            {
                m_PlayerTransform = student.transform;
                m_ThirdPersonController = student.GetComponent<ThirdPersonController>();
                m_StarterInputs = student.GetComponent<StarterAssetsInputs>();
                return;
            }

            var playerByTag = GameObject.FindGameObjectWithTag("Player");
            if (playerByTag != null)
            {
                m_PlayerTransform = playerByTag.transform;
                m_ThirdPersonController = playerByTag.GetComponent<ThirdPersonController>();
                m_StarterInputs = playerByTag.GetComponent<StarterAssetsInputs>();
                return;
            }

            m_MotorbikeController = FindAnyObjectByType<IUHGrabMotorbike>(FindObjectsInactive.Include);
            if (m_MotorbikeController != null)
            {
                m_PlayerTransform = m_MotorbikeController.transform;
            }
            else
            {
                var mainCam = Camera.main;
                if (mainCam != null) m_PlayerTransform = mainCam.transform;
            }
        }

        private void Update()
        {
            if (m_IsDefeated) return;

            if (m_TriggerCooldown > 0f)
            {
                m_TriggerCooldown -= Time.deltaTime;
            }

            if (m_IsEncounterActive)
            {
                // The combat HUD shows the boss bar during the duel.
                if (m_MonsterOverheadCanvas != null && m_MonsterOverheadCanvas.activeSelf)
                {
                    m_MonsterOverheadCanvas.SetActive(false);
                }
                if (m_ActionPromptObject != null && m_ActionPromptObject.activeSelf)
                {
                    m_ActionPromptObject.SetActive(false);
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (m_CombatManager != null) m_CombatManager.Flee();
                    else OnCombatDefeat();
                }
                return;
            }

            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
            bool inRevealRange = dist <= m_RevealRadius;
            bool inCombatRange = dist <= m_TriggerRadius;

            if (m_MonsterOverheadCanvas != null && m_MonsterOverheadCanvas.activeSelf != inCombatRange)
            {
                m_MonsterOverheadCanvas.SetActive(inCombatRange);
            }

            if (m_ActionPromptObject != null && m_ActionPromptObject.activeSelf != (inRevealRange && !inCombatRange))
            {
                m_ActionPromptObject.SetActive(inRevealRange && !inCombatRange);
            }

            if (inCombatRange && m_TriggerCooldown <= 0f)
            {
                TriggerCombat();
            }
            else if (inRevealRange && Input.GetKeyDown(m_InteractKey))
            {
                TriggerCombat();
            }
        }

        public void TriggerCombat()
        {
            if (m_IsDefeated || m_IsEncounterActive) return;

            m_IsEncounterActive = true;
            if (m_ActionPromptObject != null) m_ActionPromptObject.SetActive(false);
            if (m_MonsterOverheadCanvas != null) m_MonsterOverheadCanvas.SetActive(false);

            if (m_PlayerTransform == null) FindPlayer();
            SetPlayerControl(false);

            if (m_CombatManager == null)
            {
                m_CombatManager = FindAnyObjectByType<Match3CombatManager>(FindObjectsInactive.Include);
            }

            if (m_CombatManager != null)
            {
                m_CombatManager.gameObject.SetActive(true);
                m_CombatManager.SetActiveEncounter(this);
                m_CombatManager.StartNewCombat();
            }
        }

        private void SetPlayerControl(bool enabled)
        {
            if (m_ThirdPersonController != null)
            {
                m_ThirdPersonController.enabled = enabled;
            }

            if (m_StarterInputs != null)
            {
                m_StarterInputs.cursorLocked = enabled;
                m_StarterInputs.cursorInputForLook = enabled;
                m_StarterInputs.move = Vector2.zero;
                m_StarterInputs.look = Vector2.zero;
            }

            if (m_MotorbikeController != null)
            {
                if (!enabled)
                {
                    m_MotorbikeWasEnabled = m_MotorbikeController.enabled;
                    m_MotorbikeController.enabled = false;
                }
                else
                {
                    m_MotorbikeController.enabled = m_MotorbikeWasEnabled;
                }
            }

            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;
        }

        public void OnCombatVictory()
        {
            m_IsDefeated = true;
            m_IsEncounterActive = false;

            SetPlayerControl(true);

            if (m_CombatManager != null)
            {
                m_CombatManager.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        public void OnCombatDefeat()
        {
            m_IsEncounterActive = false;
            m_TriggerCooldown = 3.5f;

            SetPlayerControl(true);

            if (m_CombatManager != null)
            {
                m_CombatManager.gameObject.SetActive(false);
            }

            if (m_MonsterOverheadCanvas != null)
            {
                m_MonsterOverheadCanvas.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_RevealRadius);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, m_TriggerRadius);
        }
    }
}
