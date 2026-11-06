using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Guardian.Match3
{
    /// <summary>
    /// Drives one Match-3 duel: board input → <see cref="Match3CombatRules"/> → HUD, popups and
    /// the 3D <see cref="Match3BattleStage"/>. Balance lives in the rules and the encounter.
    /// </summary>
    public class Match3CombatManager : MonoBehaviour
    {
        public static Match3CombatManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Match3BoardView m_BoardView;
        [SerializeField] private Match3BattleStage m_Stage;

        [Header("Balance (boss values are overridden by the encounter)")]
        [SerializeField] private Match3CombatConfig m_Config = new Match3CombatConfig();
        [SerializeField] private float m_CombatDuration = 90f;
        [SerializeField] private string m_PlayerName = "SINH VIÊN IUH";

        [Header("HUD - Player (left)")]
        [SerializeField] private Text m_PlayerNameText;
        [SerializeField] private Image m_PlayerHpFill;
        [SerializeField] private Text m_PlayerHpText;
        [SerializeField] private Image m_PlayerShieldFill;
        [SerializeField] private Text m_PlayerShieldText;
        [SerializeField] private Image m_PlayerEnergyFill;
        [SerializeField] private Text m_PlayerEnergyText;

        [Header("HUD - Monster (right)")]
        [SerializeField] private Text m_BossNameText;
        [SerializeField] private Image m_BossHpFill;
        [SerializeField] private Text m_BossHpText;
        [SerializeField] private Text m_BossIntentText;

        [Header("HUD - Center")]
        [SerializeField] private Text m_ComboText;
        [SerializeField] private RectTransform m_PlayerPopupAnchor;
        [SerializeField] private RectTransform m_BossPopupAnchor;
        [SerializeField] private Button m_SkillButton;
        [SerializeField] private Text m_SkillButtonText;
        [SerializeField] private Text m_TimerText;
        [SerializeField] private Image m_TimerFill;

        [Header("Modals")]
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

        private static readonly Color DamageColor = new Color(1f, 0.62f, 0.2f);
        private static readonly Color HurtColor = new Color(1f, 0.3f, 0.35f);
        private static readonly Color ShieldColor = new Color(0.45f, 0.8f, 1f);
        private static readonly Color HealColor = new Color(0.4f, 0.95f, 0.55f);
        private static readonly Color EnergyColor = new Color(1f, 0.85f, 0.25f);
        private static readonly Color PurifyColor = new Color(0.8f, 0.6f, 1f);

        private IUHMonsterEncounter m_CurrentEncounter;
        private Match3CombatRules m_Rules;
        private float m_RemainingTime;
        private bool m_IsCombatActive;
        private bool m_IsResolving;
        private bool m_UsingStage;
        private float m_ComboTextTimer;

        private float m_ShownPlayerHp;
        private float m_ShownBossHp;

        public bool IsCombatActive => m_IsCombatActive;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            if (m_SkillButton != null) m_SkillButton.onClick.AddListener(OnSkillButtonClicked);
            if (m_BtnRetryVictory != null) m_BtnRetryVictory.onClick.AddListener(StartNewCombat);
            if (m_BtnRetryDefeat != null) m_BtnRetryDefeat.onClick.AddListener(StartNewCombat);
            if (m_BtnCloseVictory != null) m_BtnCloseVictory.onClick.AddListener(OnCloseVictoryClicked);
            if (m_BtnFleeDefeat != null) m_BtnFleeDefeat.onClick.AddListener(Flee);
            if (m_Stage == null) m_Stage = GetComponent<Match3BattleStage>();
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

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (m_BoardView != null)
            {
                m_BoardView.OnStepResolved -= HandleCascadeStep;
                m_BoardView.OnTurnCompleted -= HandlePlayerTurnCompleted;
            }
        }

        public void SetActiveEncounter(IUHMonsterEncounter encounter)
        {
            m_CurrentEncounter = encounter;
            if (encounter == null) return;

            m_Config.BossMaxHp = encounter.MonsterHp;
            m_Config.BossMinDmg = encounter.BossMinDmg;
            m_Config.BossMaxDmg = Mathf.Max(encounter.BossMinDmg, encounter.BossMaxDmg);
            m_CombatDuration = encounter.CombatDuration;
        }

        public void StartNewCombat()
        {
            StopAllCoroutines();
            gameObject.SetActive(true);

            m_Rules = new Match3CombatRules(m_Config);
            m_ShownPlayerHp = m_Rules.PlayerHp;
            m_ShownBossHp = m_Rules.BossHp;
            m_RemainingTime = m_CombatDuration;
            m_IsCombatActive = true;
            m_IsResolving = false;
            m_ComboTextTimer = 0f;

            if (m_VictoryModal != null) m_VictoryModal.SetActive(false);
            if (m_DefeatModal != null) m_DefeatModal.SetActive(false);
            if (m_ComboText != null) m_ComboText.gameObject.SetActive(false);
            ClearPopups(m_PlayerPopupAnchor);
            ClearPopups(m_BossPopupAnchor);
            if (m_PlayerNameText != null) m_PlayerNameText.text = m_PlayerName;
            if (m_BossNameText != null) m_BossNameText.text = m_CurrentEncounter != null ? m_CurrentEncounter.MonsterName : "DATA MONSTER";

            EnterStage();

            var model = new Match3BoardModel();
            model.Initialize();
            if (m_BoardView != null)
            {
                m_BoardView.gameObject.SetActive(true);
                m_BoardView.InputLocked = false;
                m_BoardView.Initialize(model);
            }

            UpdateAllUI();
            UpdateTimerDisplay();
        }

        /// <summary>Leave the fight without winning (Esc, or the defeat modal's button).</summary>
        public void Flee()
        {
            StopAllCoroutines();
            m_IsCombatActive = false;
            ExitStage(true);
            if (m_BoardView != null) m_BoardView.gameObject.SetActive(false);
            if (m_CurrentEncounter != null) m_CurrentEncounter.OnCombatDefeat();
            else gameObject.SetActive(false);
        }

        private void Update()
        {
            if (m_Rules == null) return;

            AnimateBars();

            if (m_ComboTextTimer > 0f)
            {
                m_ComboTextTimer -= Time.deltaTime;
                if (m_ComboTextTimer <= 0f && m_ComboText != null) m_ComboText.gameObject.SetActive(false);
            }

            if (!m_IsCombatActive || m_IsResolving) return;

            m_RemainingTime -= Time.deltaTime;
            if (m_RemainingTime <= 0f)
            {
                m_RemainingTime = 0f;
                m_Rules.TimeOut();
                StartCoroutine(DefeatRoutine("⏱️ HẾT THỜI GIAN!\nData Monster đã quá tải và chiếm quyền hệ thống."));
            }
            UpdateTimerDisplay();
        }

        private void HandleCascadeStep(CascadeStep step)
        {
            if (!m_IsCombatActive || m_Rules == null) return;

            StepResult r = m_Rules.ApplyStep(step);

            if (r.Damage > 0)
            {
                Popup(m_BossPopupAnchor, $"-{r.Damage}", DamageColor, r.Multiplier > 1.01f ? 52 : 42);
                if (m_Stage != null && m_UsingStage) { m_Stage.PlayPlayerAttack(); m_Stage.PlayMonsterHit(); }
            }
            if (r.IntentReduced > 0) Popup(m_BossPopupAnchor, $"Thanh tẩy -{r.IntentReduced}", PurifyColor, 30, 60f);
            if (r.ShieldGained > 0) Popup(m_PlayerPopupAnchor, $"+{r.ShieldGained} Giáp", ShieldColor, 32);
            if (r.Healed > 0) Popup(m_PlayerPopupAnchor, $"+{r.Healed} HP", HealColor, 34, 40f);
            if (r.EnergyGained > 0) Popup(m_PlayerPopupAnchor, $"+{r.EnergyGained} Nộ", EnergyColor, 28, 80f);

            if (step.ComboIndex >= 1 && m_ComboText != null)
            {
                m_ComboText.gameObject.SetActive(true);
                m_ComboText.text = $"COMBO x{step.ComboIndex + 1}  (×{r.Multiplier:0.##})";
                m_ComboTextTimer = 1.2f;
                StartCoroutine(Punch(m_ComboText.rectTransform));
            }

            UpdateAllUI();
        }

        private void HandlePlayerTurnCompleted()
        {
            if (!m_IsCombatActive || m_Rules == null) return;
            StartCoroutine(BossTurnRoutine());
        }

        private IEnumerator BossTurnRoutine()
        {
            if (m_Rules.Outcome == CombatOutcome.Victory)
            {
                yield return VictoryRoutine();
                yield break;
            }

            m_IsResolving = true;
            LockBoard(true);

            yield return new WaitForSeconds(0.35f);

            bool charged = m_Rules.IsBossCharging;
            if (m_Stage != null && m_UsingStage) { m_Stage.PlayMonsterAttack(charged); m_Stage.PlayPlayerHit(); }
            yield return new WaitForSeconds(0.14f);

            BossTurnResult turn = m_Rules.ResolveBossTurn();
            if (turn.Blocked > 0) Popup(m_PlayerPopupAnchor, $"Chặn {turn.Blocked}", ShieldColor, 30, 50f);
            if (turn.Damage > 0) Popup(m_PlayerPopupAnchor, $"-{turn.Damage}", HurtColor, charged ? 54 : 44);
            UpdateAllUI();

            if (m_Rules.Outcome == CombatOutcome.Defeat)
            {
                yield return DefeatRoutine("Data Monster đã áp đảo lá chắn của bạn!\nHãy thử lại để thanh tẩy khu vực.");
                yield break;
            }

            yield return new WaitForSeconds(0.3f);
            m_IsResolving = false;
            LockBoard(false);
            UpdateAllUI();
        }

        private void OnSkillButtonClicked()
        {
            if (!m_IsCombatActive || m_Rules == null || m_IsResolving) return;
            if (m_BoardView != null && m_BoardView.IsBusy) return;

            int dmg = m_Rules.CastSkill();
            if (dmg <= 0) return;

            Popup(m_BossPopupAnchor, $"⚡ -{dmg}", EnergyColor, 60);
            if (m_Stage != null && m_UsingStage) { m_Stage.PlayPlayerAttack(); m_Stage.PlayMonsterHit(); }
            UpdateAllUI();

            // The skill is a free action: the boss only acts after a swap.
            if (m_Rules.Outcome == CombatOutcome.Victory) StartCoroutine(VictoryRoutine());
        }

        private IEnumerator VictoryRoutine()
        {
            m_IsCombatActive = false;
            m_IsResolving = true;
            LockBoard(true);
            UpdateAllUI();

            if (m_Stage != null && m_UsingStage) yield return m_Stage.PlayMonsterDefeat();
            else yield return new WaitForSeconds(0.4f);

            if (m_CurrentEncounter != null)
            {
                if (m_VictoryTitleText != null) m_VictoryTitleText.text = $"🏆 ĐÃ ĐÁNH BẠI {m_CurrentEncounter.MonsterName.ToUpper()}!";
                if (m_VictoryDescText != null) m_VictoryDescText.text = $"Còn {m_Rules.PlayerHp}/{m_Config.PlayerMaxHp} HP. Khu vực đã được thanh tẩy!";
                if (m_VictoryRewardsText != null) m_VictoryRewardsText.text = m_CurrentEncounter.GetRewardsFormatted();
            }
            if (m_VictoryModal != null) m_VictoryModal.SetActive(true);
        }

        private IEnumerator DefeatRoutine(string reason)
        {
            m_IsCombatActive = false;
            m_IsResolving = true;
            LockBoard(true);
            UpdateAllUI();
            yield return new WaitForSeconds(0.4f);

            if (m_DefeatDescText != null && !string.IsNullOrEmpty(reason)) m_DefeatDescText.text = reason;
            if (m_DefeatModal != null) m_DefeatModal.SetActive(true);
        }

        private void OnCloseVictoryClicked()
        {
            ExitStage(false);
            if (m_BoardView != null) m_BoardView.gameObject.SetActive(false);
            if (m_CurrentEncounter != null) m_CurrentEncounter.OnCombatVictory();
            else gameObject.SetActive(false);
        }

        private void EnterStage()
        {
            m_UsingStage = false;
            if (m_Stage == null || m_CurrentEncounter == null) return;

            Transform player = m_CurrentEncounter.PlayerTransform;
            // The encounter falls back to the camera when no student exists; never stage the camera itself.
            if (player == null || player.GetComponent<Camera>() != null) return;

            m_Stage.Enter(player, m_CurrentEncounter.transform);
            m_UsingStage = m_Stage.IsActive;
        }

        private void ExitStage(bool restorePlayerPosition)
        {
            if (m_Stage != null && m_UsingStage) m_Stage.Exit(restorePlayerPosition);
            m_UsingStage = false;
        }

        private void LockBoard(bool locked)
        {
            if (m_BoardView != null) m_BoardView.InputLocked = locked;
        }

        private void AnimateBars()
        {
            float k = 1f - Mathf.Exp(-8f * Time.deltaTime);
            m_ShownPlayerHp = Mathf.Lerp(m_ShownPlayerHp, m_Rules.PlayerHp, k);
            m_ShownBossHp = Mathf.Lerp(m_ShownBossHp, m_Rules.BossHp, k);

            if (m_PlayerHpFill != null) m_PlayerHpFill.fillAmount = m_ShownPlayerHp / Mathf.Max(1, m_Config.PlayerMaxHp);
            if (m_BossHpFill != null) m_BossHpFill.fillAmount = m_ShownBossHp / Mathf.Max(1, m_Config.BossMaxHp);
        }

        private void UpdateTimerDisplay()
        {
            if (m_TimerText != null)
            {
                int totalSec = Mathf.CeilToInt(m_RemainingTime);
                m_TimerText.text = $"⏱️ {totalSec / 60:00}:{totalSec % 60:00}";
                m_TimerText.color = m_RemainingTime <= 10f ? HurtColor : m_RemainingTime <= 20f ? EnergyColor : Color.white;
            }

            if (m_TimerFill != null)
            {
                float ratio = m_CombatDuration > 0 ? Mathf.Clamp01(m_RemainingTime / m_CombatDuration) : 0f;
                m_TimerFill.fillAmount = ratio;
                m_TimerFill.color = ratio <= 0.2f ? HurtColor : ratio <= 0.45f ? EnergyColor : new Color(0.2f, 0.75f, 1f);
            }
        }

        private void UpdateAllUI()
        {
            if (m_Rules == null) return;
            var c = m_Config;

            if (m_PlayerHpText != null) m_PlayerHpText.text = $"HP {m_Rules.PlayerHp}";
            if (m_PlayerShieldFill != null) m_PlayerShieldFill.fillAmount = Mathf.Clamp01((float)m_Rules.PlayerShield / Mathf.Max(1, m_Rules.BossIntent));
            if (m_PlayerShieldText != null) m_PlayerShieldText.text = m_Rules.PlayerShield > 0 ? $"🛡 {m_Rules.PlayerShield}" : "";
            if (m_PlayerEnergyFill != null) m_PlayerEnergyFill.fillAmount = (float)m_Rules.PlayerEnergy / Mathf.Max(1, c.PlayerMaxEnergy);
            if (m_PlayerEnergyText != null) m_PlayerEnergyText.text = $"⚡ {m_Rules.PlayerEnergy}/{c.PlayerMaxEnergy}";

            if (m_BossHpText != null) m_BossHpText.text = $"HP {m_Rules.BossHp}";
            if (m_BossIntentText != null)
            {
                if (m_Rules.IsOver)
                {
                    m_BossIntentText.text = "";
                }
                else if (m_Rules.IsBossCharging)
                {
                    m_BossIntentText.text = $"⚠ ĐANG TÍCH TỤ: {m_Rules.BossIntent} DMG";
                    m_BossIntentText.color = HurtColor;
                }
                else
                {
                    m_BossIntentText.text = $"Ý định: tấn công {m_Rules.BossIntent} DMG";
                    m_BossIntentText.color = EnergyColor;
                }
            }

            if (m_SkillButton != null)
            {
                bool canCast = m_IsCombatActive && !m_IsResolving && m_Rules.CanCastSkill && (m_BoardView == null || !m_BoardView.IsBusy);
                m_SkillButton.interactable = canCast;
                if (m_SkillButtonText != null)
                {
                    m_SkillButtonText.text = m_Rules.CanCastSkill ? $"⚡ TUNG SKILL ({c.SkillDamage} DMG)" : $"Nộ {m_Rules.PlayerEnergy}/{c.PlayerMaxEnergy}";
                }
            }
        }

        private void Popup(RectTransform anchor, string text, Color color, int size, float yOffset = 0f)
        {
            if (anchor == null) return;
            Font font = m_ComboText != null ? m_ComboText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var go = new GameObject("Popup", typeof(RectTransform), typeof(Text), typeof(Outline));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(anchor, false);
            rt.sizeDelta = new Vector2(420f, 80f);
            rt.anchoredPosition = new Vector2(Random.Range(-40f, 40f), yOffset);

            var t = go.GetComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            go.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

            StartCoroutine(FloatAndFade(rt, t));
        }

        private static void ClearPopups(RectTransform anchor)
        {
            if (anchor == null) return;
            for (int i = anchor.childCount - 1; i >= 0; i--) Destroy(anchor.GetChild(i).gameObject);
        }

        private static IEnumerator FloatAndFade(RectTransform rt, Text t)
        {
            const float duration = 0.9f;
            Vector2 start = rt.anchoredPosition;
            Color c = t.color;
            for (float time = 0f; time < duration && rt != null; time += Time.deltaTime)
            {
                float k = time / duration;
                rt.anchoredPosition = start + new Vector2(0f, 90f * k);
                rt.localScale = Vector3.one * (k < 0.15f ? Mathf.Lerp(0.6f, 1.15f, k / 0.15f) : 1f);
                c.a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
                t.color = c;
                yield return null;
            }
            if (rt != null) Destroy(rt.gameObject);
        }

        private static IEnumerator Punch(RectTransform rt)
        {
            for (float time = 0f; time < 0.2f; time += Time.deltaTime)
            {
                rt.localScale = Vector3.one * Mathf.Lerp(1.35f, 1f, time / 0.2f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }
    }
}
