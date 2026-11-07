using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHRealtimeMonsterTarget : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private string m_MonsterName = "Quái Vật Dữ Liệu";
        [SerializeField] private int m_MaxHp = 150;
        [SerializeField] private int m_CurrentHp = 150;
        [SerializeField] private int m_RewardGold = 60;
        [SerializeField] private string m_RewardItem = "Lõi Dữ Liệu Tinh Khiết";

        [Header("Overhead UI")]
        [SerializeField] private GameObject m_HpBarRoot;
        [SerializeField] private Image m_HpFillImage;
        [SerializeField] private Text m_HpText;

        [Header("Visuals & Audio")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private Renderer[] m_Renderers;
        [SerializeField] private GameObject m_StunVisualObject;

        private bool m_IsDead = false;
        private bool m_IsStunned = false;
        private float m_StunDuration = 0f;
        private Color[] m_OriginalColors;
        private Material[] m_ClonedMats;

        public string MonsterName => m_MonsterName;
        public int CurrentHp => m_CurrentHp;
        public int MaxHp => m_MaxHp;
        public bool IsDead => m_IsDead;
        public bool IsStunned => m_IsStunned;

        public event Action<int, int> OnHpChanged;
        public event Action OnDefeated;

        private void Start()
        {
            m_CurrentHp = m_MaxHp;
            if (m_Animator == null) m_Animator = GetComponentInChildren<Animator>();
            CacheRenderers();
            UpdateHpUI();

            if (m_StunVisualObject != null)
                m_StunVisualObject.SetActive(false);
        }

        private void CacheRenderers()
        {
            if (m_Renderers == null || m_Renderers.Length == 0)
                m_Renderers = GetComponentsInChildren<Renderer>();

            if (m_Renderers != null && m_Renderers.Length > 0)
            {
                m_ClonedMats = new Material[m_Renderers.Length];
                m_OriginalColors = new Color[m_Renderers.Length];

                for (int i = 0; i < m_Renderers.Length; i++)
                {
                    if (m_Renderers[i] != null && m_Renderers[i].material != null)
                    {
                        m_ClonedMats[i] = m_Renderers[i].material;
                        m_OriginalColors[i] = m_ClonedMats[i].color;
                    }
                }
            }
        }

        private void Update()
        {
            if (m_IsStunned)
            {
                m_StunDuration -= Time.deltaTime;
                if (m_StunDuration <= 0f)
                {
                    m_IsStunned = false;
                    if (m_StunVisualObject != null) m_StunVisualObject.SetActive(false);
                }
            }

            // Always face HP bar to camera
            if (m_HpBarRoot != null && Camera.main != null)
            {
                m_HpBarRoot.transform.rotation = Camera.main.transform.rotation;
            }
        }

        public void TakeDamage(int damage, bool isCrit = false)
        {
            if (m_IsDead) return;

            m_CurrentHp = Mathf.Max(0, m_CurrentHp - damage);

            // Play GetHit animation if available
            if (m_Animator != null && !m_IsDead)
            {
                try { m_Animator.Play("GetHit", -1, 0f); } catch {}
            }

            // Audio & VFX
            IUHCombatAudio.PlayBossHit();
            StartCoroutine(FlashRedRoutine());

            // Floating text
            Color textColor = isCrit ? new Color(1f, 0.3f, 0.1f) : new Color(1f, 0.9f, 0.2f);
            string text = isCrit ? $"-{damage} CRIT!" : $"-{damage}";
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2f + UnityEngine.Random.insideUnitSphere * 0.4f, text, textColor, 1.2f);

            UpdateHpUI();
            OnHpChanged?.Invoke(m_CurrentHp, m_MaxHp);

            if (m_CurrentHp <= 0)
            {
                Die();
            }
        }

        public void ApplyStun(float duration)
        {
            if (m_IsDead) return;

            m_IsStunned = true;
            m_StunDuration = Mathf.Max(m_StunDuration, duration);

            if (m_StunVisualObject != null) m_StunVisualObject.SetActive(true);
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.3f, "⚡ LÀM CHOÁNG (STUN)!", new Color(0.2f, 0.9f, 1f), 1.4f);
        }

        private IEnumerator FlashRedRoutine()
        {
            if (m_ClonedMats != null)
            {
                for (int i = 0; i < m_ClonedMats.Length; i++)
                {
                    if (m_ClonedMats[i] != null) m_ClonedMats[i].color = Color.red;
                }
            }

            yield return new WaitForSeconds(0.12f);

            if (m_ClonedMats != null && m_OriginalColors != null)
            {
                for (int i = 0; i < m_ClonedMats.Length; i++)
                {
                    if (m_ClonedMats[i] != null && i < m_OriginalColors.Length)
                        m_ClonedMats[i].color = m_OriginalColors[i];
                }
            }
        }

        private void UpdateHpUI()
        {
            if (m_HpFillImage != null)
            {
                m_HpFillImage.fillAmount = (float)m_CurrentHp / m_MaxHp;
            }

            if (m_HpText != null)
            {
                m_HpText.text = $"{m_CurrentHp}/{m_MaxHp}";
            }
        }

        private void Die()
        {
            if (m_IsDead) return;
            m_IsDead = true;

            // Disable colliders so player can walk freely
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var c in colliders) c.enabled = false;

            // Play Death Animation
            if (m_Animator != null)
            {
                try { m_Animator.Play("Death", -1, 0f); } catch {}
            }

            IUHCombatAudio.PlayBossRoar();
            IUHCombatAudio.PlayCoin();

            // Spawn Physical Dropped Gold Coins
            SpawnLootCoins(transform.position + Vector3.up * 1f, 4);

            // Reward player
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(m_RewardGold);
                if (!string.IsNullOrEmpty(m_RewardItem))
                {
                    IUHGuardianInventory.Instance.AddItem(m_RewardItem);
                }
            }

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.5f, $"💀 TIÊU DIỆT {m_MonsterName}!\n+{m_RewardGold} Vàng", new Color(1f, 0.85f, 0.2f), 2.5f);

            OnDefeated?.Invoke();

            StartCoroutine(DeathRoutine());
        }

        private void SpawnLootCoins(Vector3 center, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coin.name = "Dropped_Gold_Coin";
                coin.transform.position = center + UnityEngine.Random.insideUnitSphere * 0.3f;
                coin.transform.localScale = new Vector3(0.45f, 0.06f, 0.45f);
                coin.transform.rotation = Quaternion.Euler(90f, UnityEngine.Random.Range(0, 360), 0);

                var rend = coin.GetComponent<Renderer>();
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = new Color(1f, 0.85f, 0.1f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.75f, 0.1f) * 2.5f);
                rend.sharedMaterial = mat;

                var rb = coin.AddComponent<Rigidbody>();
                rb.linearVelocity = new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(3.5f, 5.5f), UnityEngine.Random.Range(-2.5f, 2.5f));
                rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 10f;

                var pickup = coin.AddComponent<IUHLootCoinPickup>();
                pickup.Initialize(15);
            }
        }

        public void ResetHealth()
        {
            m_IsDead = false;
            m_IsStunned = false;
            m_CurrentHp = m_MaxHp;
            transform.localScale = Vector3.one;

            if (m_HpBarRoot != null) m_HpBarRoot.SetActive(true);
            UpdateHpUI();

            var col = GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        private IEnumerator DeathRoutine()
        {
            if (m_HpBarRoot != null) m_HpBarRoot.SetActive(false);

            if (m_Animator != null)
            {
                // Wait for death animation to finish, then sink or dissolve
                yield return new WaitForSeconds(3.0f);
            }

            float elapsed = 0f;
            Vector3 origScale = transform.localScale;

            while (elapsed < 0.8f)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(origScale, Vector3.zero, elapsed / 0.8f);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
