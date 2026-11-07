using System;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    [RequireComponent(typeof(IUHRealtimeMonsterTarget))]
    public class IUHMonsterAI : MonoBehaviour
    {
        [Header("Movement & Aggro")]
        [SerializeField] private float m_MoveSpeed = 3.2f;
        [SerializeField] private float m_AggroRadius = 18.0f;
        [SerializeField] private float m_AttackDistance = 2.2f;
        [SerializeField] private float m_AttackCooldown = 2.0f;

        [Header("Components")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private CharacterController m_CharacterController;

        private IUHRealtimeMonsterTarget m_TargetStats;
        private Transform m_PlayerTransform;
        private float m_NextAttackTime = 0f;
        private Vector3 m_SpawnPos;
        private string m_CurrentAnimState = "";

        private void Start()
        {
            m_SpawnPos = transform.position;
            m_TargetStats = GetComponent<IUHRealtimeMonsterTarget>();
            if (m_Animator == null) m_Animator = GetComponentInChildren<Animator>();
            if (m_CharacterController == null) m_CharacterController = GetComponent<CharacterController>();

            if (m_Animator != null)
            {
                m_Animator.applyRootMotion = false;
            }

            FindPlayer();
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void PlayAnim(string stateName, float crossFade = 0.15f)
        {
            if (m_Animator == null) return;
            if (m_CurrentAnimState == stateName) return;

            var info = m_Animator.GetCurrentAnimatorStateInfo(0);
            // Don't interrupt Attack until it's mostly finished
            if ((info.IsName("Attack01") || info.IsName("Attack02") || info.IsName("GetHit")) && info.normalizedTime < 0.8f && stateName != "Death")
            {
                return;
            }

            m_CurrentAnimState = stateName;
            try
            {
                m_Animator.CrossFadeInFixedTime(stateName, crossFade);
            }
            catch
            {
                try { m_Animator.Play(stateName); } catch {}
            }
        }

        private void Update()
        {
            if (m_TargetStats != null && (m_TargetStats.IsDead || m_TargetStats.IsStunned))
            {
                return;
            }

            // If Match-3 puzzle minigame is active, stop attacking and play Idle
            if (IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null && IUHCampus.Guardian.Match3.Match3CombatManager.Instance.IsCombatActive)
            {
                PlayAnim("Idle", 0.2f);
                return;
            }

            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            Vector3 toPlayer = m_PlayerTransform.position - transform.position;
            toPlayer.y = 0;
            float dist = toPlayer.magnitude;

            if (dist <= m_AggroRadius)
            {
                // Look towards player
                if (toPlayer.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);
                }

                if (dist > m_AttackDistance)
                {
                    // Run towards player with full running animation
                    Vector3 moveDelta = toPlayer.normalized * (m_MoveSpeed * Time.deltaTime);
                    moveDelta.y = -9.81f * Time.deltaTime; // gravity

                    if (m_CharacterController != null && m_CharacterController.enabled)
                    {
                        m_CharacterController.Move(moveDelta);
                    }
                    else
                    {
                        transform.position += new Vector3(moveDelta.x, 0, moveDelta.z);
                    }

                    PlayAnim("Run", 0.15f);
                }
                else
                {
                    // Attack range
                    if (Time.time >= m_NextAttackTime)
                    {
                        m_NextAttackTime = Time.time + m_AttackCooldown;
                        PlayAnim(UnityEngine.Random.value < 0.5f ? "Attack01" : "Attack02", 0.1f);
                        StartCoroutine(PerformMonsterAttackDamage());
                    }
                    else
                    {
                        PlayAnim("Idle", 0.2f);
                    }
                }
            }
            else
            {
                // Idle when not aggroed
                PlayAnim("Idle", 0.2f);
            }
        }

        private System.Collections.IEnumerator PerformMonsterAttackDamage()
        {
            // Wait for weapon swing apex (around 0.35s)
            yield return new WaitForSeconds(0.35f);

            if (m_TargetStats != null && m_TargetStats.IsDead) yield break;
            if (m_PlayerTransform == null) yield break;

            float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
            if (dist <= m_AttackDistance + 1.2f) // In melee weapon reach
            {
                var playerHealth = m_PlayerTransform.GetComponent<IUHPlayerHealth>();
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    int dmg = UnityEngine.Random.Range(18, 28);
                    playerHealth.TakeDamage(dmg, transform.position);
                    IUHCombatAudio.PlaySword();
                }
            }
        }
    }
}

