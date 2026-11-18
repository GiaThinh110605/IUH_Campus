using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian.Match3;
using StarterAssets;

namespace IUHCampus.Guardian
{
    public class IUHMonsterEncounter : MonoBehaviour
    {
        private const string AlreadyClaimedMessage = "Đã nhận thưởng khu vực này";
        private const string RewardUnavailableMessage = "Chưa nhận được thưởng";
        private bool m_RewardWarningLogged;
        [Header("Encounter Settings")]
        [SerializeField] private string m_MonsterName = "DATA CORRUPTION";
        [SerializeField] private string m_MilestoneId = "";
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
        private bool m_IsEncounterActive;
        private bool m_IsDefeated;
        private bool m_RequireLeaveRange;
        private float m_TriggerCooldown = 0f;
        private float m_ProximityTimer = 0f;
        private const float PROXIMITY_TRIGGER_SECONDS = 5.0f;

        public string MonsterName => m_MonsterName;
        public string MilestoneId => m_MilestoneId;
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

        public bool ClaimVictoryReward()
        {
            var inventory = IUHGuardianInventory.Instance;
            if (inventory == null || string.IsNullOrEmpty(m_MilestoneId))
            {
                if (!m_RewardWarningLogged)
                {
                    m_RewardWarningLogged = true;
                    Debug.LogWarning("[IUH Guardian] Victory reward unavailable: inventory or milestone is missing.", this);
                }
                return false;
            }
            return inventory.TryClaimMilestoneReward(m_MilestoneId, m_RewardGold, m_RewardItems);
        }

        public string GetRewardsFormatted(bool claimed, bool alreadyClaimed)
        {
            if (!claimed) return alreadyClaimed ? AlreadyClaimedMessage : RewardUnavailableMessage;
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

        public void ResetForAftershock()
        {
            m_IsDefeated = false;
            m_IsEncounterActive = false;
            m_TriggerCooldown = 0f;
            m_RequireLeaveRange = true;
            if (m_MonsterOverheadCanvas != null)
                m_MonsterOverheadCanvas.SetActive(false);
            if (m_ActionPromptObject != null)
                m_ActionPromptObject.SetActive(false);
        }

        private void Start()
        {
            FindPlayer();

            m_TriggerCooldown = 3.0f;
            m_RequireLeaveRange = true;

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
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null)
            {
                m_PlayerTransform = student.transform;
                m_ThirdPersonController = student.GetComponent<ThirdPersonController>();
                m_StarterInputs = student.GetComponent<StarterAssetsInputs>();
                return;
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

                if (IUHCampus.Core.IUHInputHelper.IsEscapeKeyDown())
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

            if (Match3CombatManager.Instance != null && Match3CombatManager.Instance.BlocksExplorationInput)
            {
                if (Vector3.Distance(transform.position, m_PlayerTransform.position) <= m_TriggerRadius) m_RequireLeaveRange = true;
                return;
            }
            var preview = m_PlayerTransform.GetComponent<IUHCampus.World.IUHConceptPreviewController>();
            if (preview != null && preview.IsExplorationPaused) return;
            if (IUHCampus.Core.IUHInputHelper.IsEscapeKeyDown()) return;
            float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
            bool inRevealRange = dist <= m_RevealRadius;
            bool inCombatRange = dist <= m_TriggerRadius;
            if (!inCombatRange) m_RequireLeaveRange = false;

            bool inProximity = inCombatRange || inRevealRange;
            if (m_MonsterOverheadCanvas != null && m_MonsterOverheadCanvas.activeSelf != inProximity)
            {
                m_MonsterOverheadCanvas.SetActive(inProximity);
            }

            if (m_ActionPromptObject != null && m_ActionPromptObject.activeSelf != inProximity)
            {
                m_ActionPromptObject.SetActive(inProximity);
            }

            // Cơ chế tiếp cận: Nếu người chơi đứng sát quái vật (~m_TriggerRadius) khoảng 5 giây sẽ tự động vào trận
            if (inCombatRange)
            {
                m_ProximityTimer += Time.deltaTime;
                float remaining = Mathf.Max(0f, PROXIMITY_TRIGGER_SECONDS - m_ProximityTimer);
                if (m_BossIntentText != null)
                {
                    m_BossIntentText.text = $"⚡ Vào trận sau {remaining:F1}s (hoặc nhấn [E])";
                }

                if (m_ProximityTimer >= PROXIMITY_TRIGGER_SECONDS && m_TriggerCooldown <= 0f)
                {
                    m_ProximityTimer = 0f;
                    TriggerCombat();
                    return;
                }
            }
            else
            {
                m_ProximityTimer = 0f;
                if (m_BossIntentText != null)
                {
                    m_BossIntentText.text = $"⚔️ Ý định: Tấn công {m_BossMinDmg}-{m_BossMaxDmg} DMG";
                }
            }

            // Người chơi có thể bấm phím [E] hoặc nút Tương Tác bất cứ lúc nào để vào trận ngay không cần chờ 5s
            bool interactRequested = Input.GetKeyDown(m_InteractKey) || IUHCampus.Core.IUHInputHelper.IsInteractKeyDown();
            if (inProximity && interactRequested && m_TriggerCooldown <= 0f)
            {
                m_ProximityTimer = 0f;
                TriggerCombat();
            }
        }

        public void TriggerCombat()
        {
            if (m_IsDefeated || m_IsEncounterActive) return;
            if (Match3CombatManager.Instance != null && Match3CombatManager.Instance.BlocksExplorationInput) return;

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
                m_ThirdPersonController.ResetMotion();
                m_ThirdPersonController.enabled = enabled;
            }

            if (m_StarterInputs != null)
            {
                m_StarterInputs.cursorLocked = enabled;
                m_StarterInputs.cursorInputForLook = enabled;
                m_StarterInputs.move = Vector2.zero;
                m_StarterInputs.look = Vector2.zero;
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
            m_RequireLeaveRange = true;

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

        public void ApplyEmpStun(float duration)
        {
            if (m_IsDefeated) return;
            m_TriggerCooldown = Mathf.Max(m_TriggerCooldown, duration);

            // Reduce monster HP by 25%
            int damage = Mathf.Max(1, m_MonsterHp / 4);
            m_MonsterHp = Mathf.Max(10, m_MonsterHp - damage);

            if (m_BossHpText != null)
            {
                m_BossHpText.text = $"{m_MonsterHp} HP (STUNNED!)";
            }
            if (m_BossHpFill != null)
            {
                m_BossHpFill.fillAmount = (float)m_MonsterHp / 100f;
            }

            IUHCameraShake.Shake(0.3f, 0.4f);
            IUHCombatAudio.PlayShield();
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
