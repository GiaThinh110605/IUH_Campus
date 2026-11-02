using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Guardian.Match3
{
    public class Match3CombatManager : MonoBehaviour
    {
        public static Match3CombatManager Instance { get; private set; }

        [Header("Board Reference")]
        [SerializeField] private Match3BoardView m_BoardView;

        [Header("Player Settings")]
        [SerializeField] private int m_PlayerMaxHp = 100;
        [SerializeField] private int m_PlayerMaxEnergy = 100;

        [Header("Boss Settings")]
        [SerializeField] private int m_BossMaxHp = 120;
        [SerializeField] private int m_BossMinDmg = 12;
        [SerializeField] private int m_BossMaxDmg = 18;

        [Header("Balance Multipliers")]
        [SerializeField] private int m_DmgPerSword = 6;
        [SerializeField] private int m_ShieldPerTile = 5;
        [SerializeField] private int m_EnergyPerTile = 6;
        [SerializeField] private int m_HealPerTile = 5;
        [SerializeField] private int m_SkillDamage = 40;

        [Header("UI - Player HUD")]
        [SerializeField] private Image m_PlayerHpFill;
        [SerializeField] private Text m_PlayerHpText;
        [SerializeField] private Image m_PlayerShieldFill;
        [SerializeField] private Text m_PlayerShieldText;
        [SerializeField] private Image m_PlayerEnergyFill;
        [SerializeField] private Text m_PlayerEnergyText;
        [SerializeField] private Button m_SkillButton;
        [SerializeField] private Text m_SkillButtonText;

        [Header("UI - Boss HUD")]
        [SerializeField] private Image m_BossHpFill;
        [SerializeField] private Text m_BossHpText;
        [SerializeField] private Text m_BossIntentText;

        [Header("Combat Timer")]
        [SerializeField] private float m_CombatDuration = 60f;
        [SerializeField] private Text m_TimerText;
        [SerializeField] private Image m_TimerFill;

        [Header("UI - Modals")]
        [SerializeField] private GameObject m_VictoryModal;
        [SerializeField] private Text m_VictoryTitleText;
        [SerializeField] private Text m_VictoryDescText;
        [SerializeField] private Text m_VictoryRewardsText;
        [SerializeField] private GameObject m_DefeatModal;
        [SerializeField] private Text m_DefeatDescText;
        [SerializeField] private Button m_BtnRetryVictory;
        [SerializeField] private Button m_BtnRetryDefeat;
        [SerializeField] private Button m_BtnCloseVictory;
        [SerializeField] private Button m_BtnFleeDefeat;

        private IUHMonsterEncounter m_CurrentEncounter;

        public void SetActiveEncounter(IUHMonsterEncounter encounter)
        {
            m_CurrentEncounter = encounter;
            if (encounter != null)
            {
                if (encounter.BossHpFill != null) m_BossHpFill = encounter.BossHpFill;
                if (encounter.BossHpText != null) m_BossHpText = encounter.BossHpText;
                if (encounter.BossIntentText != null) m_BossIntentText = encounter.BossIntentText;
                m_BossMaxHp = encounter.MonsterHp;
                m_BossMinDmg = encounter.BossMinDmg;
                m_BossMaxDmg = encounter.BossMaxDmg;
                m_CombatDuration = encounter.CombatDuration;
            }
        }

        private int m_CurrentPlayerHp;
        private int m_CurrentPlayerShield;
        private int m_CurrentPlayerEnergy;

        private int m_CurrentBossHp;
        private int m_CurrentBossIntentDmg;

        private int m_TurnAccumulatedDamage;
        private bool m_IsCombatActive;
        private float m_RemainingTime;

        public bool IsCombatActive => m_IsCombatActive;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            if (m_SkillButton != null) m_SkillButton.onClick.AddListener(OnSkillButtonClicked);
            if (m_BtnRetryVictory != null) m_BtnRetryVictory.onClick.AddListener(StartNewCombat);
            if (m_BtnRetryDefeat != null) m_BtnRetryDefeat.onClick.AddListener(StartNewCombat);
            if (m_BtnCloseVictory != null) m_BtnCloseVictory.onClick.AddListener(OnCloseVictoryClicked);
            if (m_BtnFleeDefeat != null) m_BtnFleeDefeat.onClick.AddListener(OnFleeDefeatClicked);
        }

        private void Start()
        {
            if (m_BoardView != null)
            {
                m_BoardView.OnStepResolved += HandleCascadeStep;
                m_BoardView.OnTurnCompleted += HandlePlayerTurnCompleted;
            }

            if (!m_IsCombatActive)
            {
                if (m_BoardView != null) m_BoardView.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!m_IsCombatActive) return;

            m_RemainingTime -= Time.deltaTime;
            if (m_RemainingTime <= 0f)
            {
                m_RemainingTime = 0f;
                UpdateTimerDisplay();
                TriggerDefeat("⏱️ HẾT THỜI GIAN CHIẾN ĐẤU!\nQuái vật dữ liệu đã quá tải và chiếm quyền kiểm soát hệ thống!");
            }
            else
            {
                UpdateTimerDisplay();
            }
        }

        public void StartNewCombat()
        {
            gameObject.SetActive(true);
            if (m_BoardView != null)
            {
                m_BoardView.gameObject.SetActive(true);
            }

            m_CurrentPlayerHp = m_PlayerMaxHp;
            m_CurrentPlayerShield = 0;
            m_CurrentPlayerEnergy = 0;

            m_CurrentBossHp = m_BossMaxHp;
            RollBossIntent();

            m_TurnAccumulatedDamage = 0;
            m_RemainingTime = m_CombatDuration;
            m_IsCombatActive = true;

            if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
            if (m_DefeatModal != null) m_DefeatModal.SetActive(false);

            Match3BoardModel model = new Match3BoardModel();
            model.Initialize();

            if (m_BoardView != null)
            {
                m_BoardView.gameObject.SetActive(true);
                m_BoardView.Initialize(model);
            }

            UpdateAllUI();
            UpdateTimerDisplay();
        }

        private void RollBossIntent()
        {
            m_CurrentBossIntentDmg = UnityEngine.Random.Range(m_BossMinDmg, m_BossMaxDmg + 1);
        }

        private void HandleCascadeStep(CascadeStep step)
        {
            if (!m_IsCombatActive) return;

            int stepDamage = step.SwordCount * m_DmgPerSword;
            int stepShield = step.ShieldCount * m_ShieldPerTile;
            int stepEnergy = step.EnergyCount * m_EnergyPerTile;
            int stepHeal = step.HealCount * m_HealPerTile;

            m_TurnAccumulatedDamage += stepDamage;
            m_CurrentPlayerShield += stepShield;
            m_CurrentPlayerEnergy = Mathf.Min(m_PlayerMaxEnergy, m_CurrentPlayerEnergy + stepEnergy);
            m_CurrentPlayerHp = Mathf.Min(m_PlayerMaxHp, m_CurrentPlayerHp + stepHeal);

            UpdateAllUI();
        }

        private void HandlePlayerTurnCompleted()
        {
            if (!m_IsCombatActive) return;

            StartCoroutine(ExecuteTurnResolutionRoutine());
        }

        private IEnumerator ExecuteTurnResolutionRoutine()
        {
            if (m_TurnAccumulatedDamage > 0)
            {
                m_CurrentBossHp = Mathf.Max(0, m_CurrentBossHp - m_TurnAccumulatedDamage);
                m_TurnAccumulatedDamage = 0;
                UpdateAllUI();
            }

            if (m_CurrentBossHp <= 0)
            {
                TriggerVictory();
                yield break;
            }

            yield return new WaitForSeconds(0.4f);

            int damageToTake = m_CurrentBossIntentDmg;
            if (m_CurrentPlayerShield > 0)
            {
                if (m_CurrentPlayerShield >= damageToTake)
                {
                    m_CurrentPlayerShield -= damageToTake;
                    damageToTake = 0;
                }
                else
                {
                    damageToTake -= m_CurrentPlayerShield;
                    m_CurrentPlayerShield = 0;
                }
            }

            m_CurrentPlayerShield = 0;
            m_CurrentPlayerHp = Mathf.Max(0, m_CurrentPlayerHp - damageToTake);
            RollBossIntent();
            UpdateAllUI();

            if (m_CurrentPlayerHp <= 0)
            {
                TriggerDefeat("Dữ liệu bị nguyền đã áp đảo lá chắn của bạn!\nHãy thử lại để thanh tẩy khu vực.");
            }
        }

        private void OnSkillButtonClicked()
        {
            if (!m_IsCombatActive || m_BoardView.IsBusy) return;
            if (m_CurrentPlayerEnergy < m_PlayerMaxEnergy) return;

            m_CurrentPlayerEnergy = 0;
            m_CurrentBossHp = Mathf.Max(0, m_CurrentBossHp - m_SkillDamage);
            UpdateAllUI();

            if (m_CurrentBossHp <= 0)
            {
                TriggerVictory();
            }
            else
            {
                StartCoroutine(ExecuteTurnResolutionRoutine());
            }
        }

        private void TriggerVictory()
        {
            m_IsCombatActive = false;
            if (m_CurrentEncounter != null)
            {
                if (m_VictoryTitleText != null)
                {
                    m_VictoryTitleText.text = $"🏆 CHIẾN THẮNG {m_CurrentEncounter.MonsterName.ToUpper()}!";
                }
                if (m_VictoryDescText != null)
                {
                    m_VictoryDescText.text = $"Bạn đã xuất sắc đánh bại {m_CurrentEncounter.MonsterName} và thanh tẩy thành công khu vực!";
                }
                if (m_VictoryRewardsText != null)
                {
                    m_VictoryRewardsText.text = m_CurrentEncounter.GetRewardsFormatted();
                }
            }
            if (m_VictoryModal != null) m_VictoryModal.SetActive(true);
        }

        private void TriggerDefeat(string reason = null)
        {
            m_IsCombatActive = false;
            if (m_DefeatDescText != null && !string.IsNullOrEmpty(reason))
            {
                m_DefeatDescText.text = reason;
            }
            if (m_DefeatModal != null) m_DefeatModal.SetActive(true);
        }

        private void UpdateTimerDisplay()
        {
            if (m_TimerText != null)
            {
                int totalSec = Mathf.CeilToInt(m_RemainingTime);
                int mins = totalSec / 60;
                int secs = totalSec % 60;
                m_TimerText.text = $"⏱️ THỜI GIAN: {mins:00}:{secs:00}";
                if (m_RemainingTime <= 10f)
                {
                    m_TimerText.color = new Color(1f, 0.28f, 0.28f);
                }
                else if (m_RemainingTime <= 20f)
                {
                    m_TimerText.color = new Color(1f, 0.85f, 0.25f);
                }
                else
                {
                    m_TimerText.color = Color.white;
                }
            }

            if (m_TimerFill != null)
            {
                float ratio = m_CombatDuration > 0 ? Mathf.Clamp01(m_RemainingTime / m_CombatDuration) : 0f;
                m_TimerFill.fillAmount = ratio;
                if (ratio <= 0.2f)
                    m_TimerFill.color = new Color(0.95f, 0.2f, 0.25f);
                else if (ratio <= 0.45f)
                    m_TimerFill.color = new Color(1f, 0.75f, 0.15f);
                else
                    m_TimerFill.color = new Color(0.2f, 0.75f, 1f);
            }
        }

        private void OnCloseVictoryClicked()
        {
            if (m_BoardView != null) m_BoardView.gameObject.SetActive(false);
            if (m_CurrentEncounter != null)
            {
                m_CurrentEncounter.OnCombatVictory();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnFleeDefeatClicked()
        {
            if (m_BoardView != null) m_BoardView.gameObject.SetActive(false);
            if (m_CurrentEncounter != null)
            {
                m_CurrentEncounter.OnCombatDefeat();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void UpdateAllUI()
        {
            if (m_PlayerHpFill != null) m_PlayerHpFill.fillAmount = (float)m_CurrentPlayerHp / m_PlayerMaxHp;
            if (m_PlayerHpText != null) m_PlayerHpText.text = $"{m_CurrentPlayerHp}/{m_PlayerMaxHp}";

            if (m_PlayerShieldFill != null) m_PlayerShieldFill.fillAmount = Mathf.Clamp01((float)m_CurrentPlayerShield / 50f);
            if (m_PlayerShieldText != null) m_PlayerShieldText.text = m_CurrentPlayerShield > 0 ? $"Giáp: {m_CurrentPlayerShield}" : "";

            if (m_PlayerEnergyFill != null) m_PlayerEnergyFill.fillAmount = (float)m_CurrentPlayerEnergy / m_PlayerMaxEnergy;
            if (m_PlayerEnergyText != null) m_PlayerEnergyText.text = $"{m_CurrentPlayerEnergy}/{m_PlayerMaxEnergy}";

            if (IUHPlayerOverheadHUD.Instance != null)
            {
                IUHPlayerOverheadHUD.Instance.SetHp(m_CurrentPlayerHp, m_PlayerMaxHp);
                IUHPlayerOverheadHUD.Instance.SetShield(m_CurrentPlayerShield);
                IUHPlayerOverheadHUD.Instance.SetEnergy(m_CurrentPlayerEnergy, m_PlayerMaxEnergy);
            }

            if (m_BossHpFill != null) m_BossHpFill.fillAmount = (float)m_CurrentBossHp / m_BossMaxHp;
            if (m_BossHpText != null) m_BossHpText.text = $"{m_CurrentBossHp}/{m_BossMaxHp}";

            if (m_BossIntentText != null) m_BossIntentText.text = $"Ý định: Tấn công {m_CurrentBossIntentDmg} DMG";

            if (m_SkillButton != null)
            {
                bool canCast = m_CurrentPlayerEnergy >= m_PlayerMaxEnergy && m_IsCombatActive && (m_BoardView != null && !m_BoardView.IsBusy);
                m_SkillButton.interactable = canCast;
                if (m_SkillButtonText != null)
                {
                    m_SkillButtonText.text = canCast ? "⚡ TUNG SKILL (40 DMG)" : $"Nộ: {m_CurrentPlayerEnergy}/100";
                }
            }
        }
    }
}

