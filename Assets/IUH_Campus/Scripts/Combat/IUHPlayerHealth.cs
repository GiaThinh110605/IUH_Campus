using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHPlayerHealth : MonoBehaviour
    {
        public static IUHPlayerHealth Instance { get; private set; }

        [Header("Stats")]
        [SerializeField] private int m_MaxHp = 100;
        [SerializeField] private int m_CurrentHp = 100;
        [SerializeField] private int m_MaxShield = 50;
        [SerializeField] private int m_CurrentShield = 50;
        [SerializeField] private float m_ShieldRegenDelay = 5.0f;
        [SerializeField] private float m_ShieldRegenRate = 12.0f;

        [Header("UI References")]
        [SerializeField] private Image m_HpFill;
        [SerializeField] private Text m_HpText;
        [SerializeField] private Image m_ShieldFill;
        [SerializeField] private Text m_ShieldText;
        [SerializeField] private CanvasGroup m_DamageFlashOverlay;
        [SerializeField] private GameObject m_DeathScreenRoot;
        [SerializeField] private Text m_DeathCountdownText;

        [Header("Respawn")]
        [SerializeField] private Vector3 m_RespawnPoint = new Vector3(0f, 0.15f, -38f);
        [SerializeField] private float m_RespawnTime = 4.0f;

        private bool m_IsDead = false;
        private float m_LastDamageTime = 0f;
        private float m_InvulnerableTime = 0f;
        private Animator m_Animator;
        private StarterAssets.ThirdPersonController m_ThirdPersonController;
        private Coroutine m_DamageFlashCoroutine;

        public int CurrentHp => m_CurrentHp;
        public int MaxHp => m_MaxHp;
        public int CurrentShield => m_CurrentShield;
        public int MaxShield => m_MaxShield;
        public bool IsDead => m_IsDead;

        public event Action<int, int, int, int> OnHealthChanged;
        public event Action OnPlayerDied;
        public event Action OnPlayerRespawned;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);

            m_Animator = GetComponent<Animator>();
            m_ThirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        }

        private void Start()
        {
            m_CurrentHp = m_MaxHp;
            m_CurrentShield = m_MaxShield;
            m_IsDead = false;

            if (m_DeathScreenRoot != null) m_DeathScreenRoot.SetActive(false);
            if (m_DamageFlashOverlay != null)
            {
                m_DamageFlashOverlay.alpha = 0f;
                m_DamageFlashOverlay.blocksRaycasts = false;
            }

            UpdateUI();
        }

        private void Update()
        {
            if (m_IsDead) return;

            if (m_InvulnerableTime > 0f) m_InvulnerableTime -= Time.deltaTime;

            // Shield regeneration when not taking damage for a while
            if (Time.time - m_LastDamageTime >= m_ShieldRegenDelay && m_CurrentShield < m_MaxShield)
            {
                float add = m_ShieldRegenRate * Time.deltaTime;
                m_CurrentShield = Mathf.Min(m_MaxShield, m_CurrentShield + Mathf.RoundToInt(add));
                UpdateUI();
            }
        }

        public void TakeDamage(int damage, Vector3 attackerPos)
        {
            if (m_IsDead) return;
            if (m_InvulnerableTime > 0f) return;

            // Invulnerable while playing Match-3 puzzle minigame
            if (IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null && IUHCampus.Guardian.Match3.Match3CombatManager.Instance.IsCombatActive)
            {
                return;
            }

            m_InvulnerableTime = 0.35f; // i-frames
            m_LastDamageTime = Time.time;

            int remainingDamage = damage;

            // Absorb with shield first
            if (m_CurrentShield > 0)
            {
                if (m_CurrentShield >= remainingDamage)
                {
                    m_CurrentShield -= remainingDamage;
                    remainingDamage = 0;
                    IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.1f, $"🛡️ GIÁP ĐỠ: -{damage}", new Color(0.2f, 0.85f, 1f), 1.2f);
                }
                else
                {
                    remainingDamage -= m_CurrentShield;
                    m_CurrentShield = 0;
                    IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.1f, "⚡ VỠ GIÁP!", new Color(1f, 0.4f, 0.2f), 1.3f);
                }
            }

            // Remainder goes to real HP
            if (remainingDamage > 0)
            {
                m_CurrentHp = Mathf.Max(0, m_CurrentHp - remainingDamage);
                IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.3f, $"💔 MẤT MÁU: -{remainingDamage} HP!", new Color(1f, 0.25f, 0.25f), 1.4f);

                // Check Emergency Drone Supply when HP drops low (<= 35%)
                if (IUHEmergencyDroneSupplyManager.Instance != null)
                {
                    IUHEmergencyDroneSupplyManager.Instance.OnPlayerHealthChanged(m_CurrentHp, m_MaxHp);
                }
            }

            // Hurt reaction
            if (m_Animator != null)
            {
                try { m_Animator.CrossFadeInFixedTime("PlayerHurt", 0.05f); } catch {}
            }

            // Flash Screen Blood Red
            TriggerDamageFlash();

            // Audio & Camera Shake
            IUHCombatAudio.PlayPunchHit();
            IUHCameraShake.Shake(0.18f, 0.1f);

            // Knockback away from attacker
            Vector3 pushDir = (transform.position - attackerPos);
            pushDir.y = 0;
            if (pushDir.sqrMagnitude > 0.01f)
            {
                var cc = GetComponent<CharacterController>();
                if (cc != null && cc.enabled)
                {
                    cc.Move(pushDir.normalized * 0.75f);
                }
            }

            UpdateUI();

            // Check Death / Drone Emergency Revive
            if (m_CurrentHp <= 0)
            {
                if (IUHEmergencyDroneSupplyManager.Instance != null && IUHEmergencyDroneSupplyManager.Instance.TryEmergencyRevive())
                {
                    m_CurrentHp = m_MaxHp;
                    m_CurrentShield = m_MaxShield;
                    m_InvulnerableTime = 3.0f;
                    UpdateUI();
                    return;
                }

                Die();
            }
        }

        public void Heal(int amount)
        {
            if (m_IsDead) return;
            m_CurrentHp = Mathf.Min(m_MaxHp, m_CurrentHp + amount);
            m_CurrentShield = m_MaxShield;
            IUHCombatAudio.PlayHeal();
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.2f, $"💚 HỒI MÁU: +{amount} HP & ĐẦY GIÁP!", new Color(0.2f, 1f, 0.4f), 1.5f);
            UpdateUI();
        }

        private void TriggerDamageFlash()
        {
            if (m_DamageFlashOverlay == null) return;
            if (m_DamageFlashCoroutine != null) StopCoroutine(m_DamageFlashCoroutine);
            m_DamageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        private IEnumerator DamageFlashRoutine()
        {
            m_DamageFlashOverlay.alpha = 0.55f;
            float elapsed = 0f;
            float duration = 0.35f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                m_DamageFlashOverlay.alpha = Mathf.Lerp(0.55f, 0f, elapsed / duration);
                yield return null;
            }
            m_DamageFlashOverlay.alpha = 0f;
        }

        private void Die()
        {
            if (m_IsDead) return;
            m_IsDead = true;

            // Disable player control
            if (m_ThirdPersonController != null) m_ThirdPersonController.enabled = false;

            // Play Death Animation
            if (m_Animator != null)
            {
                try { m_Animator.CrossFadeInFixedTime("PlayerDeath", 0.1f); } catch {}
            }

            // Audio
            IUHCombatAudio.PlayBossRoar();

            // Death Screen & Floating text
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.5f, "☠️ BẠN ĐÃ BỊ HẠ GỤC!", new Color(1f, 0.1f, 0.1f), 3.0f);

            OnPlayerDied?.Invoke();

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            if (m_DeathScreenRoot != null) m_DeathScreenRoot.SetActive(true);

            float remaining = m_RespawnTime;
            while (remaining > 0f)
            {
                if (m_DeathCountdownText != null)
                {
                    m_DeathCountdownText.text = $"HỒI SINH TRONG: {Mathf.CeilToInt(remaining)}s";
                }
                yield return new WaitForSeconds(1.0f);
                remaining -= 1.0f;
            }

            Respawn();
        }

        public void Respawn()
        {
            // Teleport to safe spawn area
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = m_RespawnPoint;

            if (cc != null) cc.enabled = true;

            // Restore full stats
            m_CurrentHp = m_MaxHp;
            m_CurrentShield = m_MaxShield;
            m_IsDead = false;

            // Re-enable controller
            if (m_ThirdPersonController != null) m_ThirdPersonController.enabled = true;

            // Return to Idle
            if (m_Animator != null)
            {
                try { m_Animator.CrossFadeInFixedTime("Idle Walk Run Blend", 0.2f); } catch {}
            }

            // Hide death screen
            if (m_DeathScreenRoot != null) m_DeathScreenRoot.SetActive(false);

            IUHCombatAudio.PlayHeal();
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.4f, "✨ ĐÃ HỒI SINH TẠI CỔNG TRƯỜNG! FULL MÁU!", new Color(0.2f, 1f, 0.6f), 2.5f);

            UpdateUI();
            OnPlayerRespawned?.Invoke();
        }

        public void UpdateUI()
        {
            if (m_HpFill != null) m_HpFill.fillAmount = (float)m_CurrentHp / m_MaxHp;
            if (m_HpText != null) m_HpText.text = $"{m_CurrentHp}/{m_MaxHp}";

            if (m_ShieldFill != null) m_ShieldFill.fillAmount = (float)m_CurrentShield / m_MaxShield;
            if (m_ShieldText != null) m_ShieldText.text = $"{m_CurrentShield}/{m_MaxShield}";

            if (IUHPlayerOverheadHUD.Instance != null)
            {
                IUHPlayerOverheadHUD.Instance.SetHp(m_CurrentHp, m_MaxHp);
                IUHPlayerOverheadHUD.Instance.SetShield(m_CurrentShield);
            }

            OnHealthChanged?.Invoke(m_CurrentHp, m_MaxHp, m_CurrentShield, m_MaxShield);
        }
    }
}

