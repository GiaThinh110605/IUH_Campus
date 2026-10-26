using UnityEngine;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Intelligent Haunted Ghost AI for Mini-Game 2.1 "Grab Campus: Chuyến xe tâm linh".
    /// Features:
    /// - Behaviors: Patrol, Observing (spectral figure), PathBlocker (ngáng đường), Chasing (truy đuổi).
    /// - Reactions:
    ///   * Flashlight: Slows down by 60%, covers eyes.
    ///   * Self-Defense Bat: Stunned for 4.5s and knocked back 8m.
    ///   * Roadside Flare: Scared away, flees for 6s.
    ///   * Talisman: Blocked by golden shield, warded off 10m with 0 cargo damage.
    /// - Target Loss: Can lose track of player (>34m) and return smoothly to patrol base.
    /// </summary>
    public class IUHGhostHazard : MonoBehaviour
    {
        public enum GhostBehaviorType
        {
            Patroller,     // Tuần tra bình thường
            Observer,      // Đứng nhìn từ xa rồi biến mất khi lại gần
            PathBlocker,   // Chặn ngã tư/ngõ hẹp, ép dùng đồ hoặc đi đường vòng
            Chaser         // Truy đuổi người chơi khi phát hiện
        }

        public enum GhostState
        {
            Patrolling,
            Observing,
            Chasing,
            Stunned,
            Fleeing,
            Returning
        }

        [Header("Ghost Behavior")]
        public GhostBehaviorType behavior = GhostBehaviorType.Chaser;
        public GhostState currentState = GhostState.Patrolling;

        [Header("Movement & Floating")]
        [SerializeField] private float m_HoverSpeed = 2.2f;
        [SerializeField] private float m_HoverHeight = 0.45f;
        [SerializeField] private float m_PatrolDistance = 6.0f;
        [SerializeField] private float m_PatrolSpeed = 2.5f;
        [SerializeField] private Vector3 m_PatrolAxis = Vector3.right;

        [Header("Spectral Visuals")]
        [SerializeField] private Color m_GhostColor = new Color(0.2f, 0.9f, 0.8f, 0.75f);
        [SerializeField] private Light m_GhostLight;

        [Header("Damage & Impact")]
        [SerializeField] private int m_CargoDamage = 15;
        [SerializeField] private float m_ScreamDistance = 8.0f;

        [Header("Pursuit AI Config")]
        [SerializeField] private float m_BaseChaseSpeed = 6.2f;
        [SerializeField] private float m_TriggerChaseDist = 26.0f;
        [SerializeField] private float m_LoseChaseDist = 34.0f;

        // Dynamic State Variables
        private Vector3 m_BasePosition;
        private AudioSource m_AudioSource;
        private bool m_HasScreamed = false;
        private float m_PatrolTimer = 0f;
        private Transform m_PlayerTransform;

        private float m_StunTimer = 0f;
        private float m_FleeTimer = 0f;
        private Vector3 m_FleeDirection = Vector3.back;
        private float m_FlashlightSlowTimer = 0f;
        private bool m_IsVanished = false;

        public bool IsChasing => currentState == GhostState.Chasing;
        public bool IsStunned => currentState == GhostState.Stunned;

        private void Awake()
        {
            m_BasePosition = transform.position;
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 0.85f;
            m_AudioSource.playOnAwake = false;
            m_AudioSource.maxDistance = 28f;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear;

            if (m_GhostLight == null)
            {
                m_GhostLight = GetComponentInChildren<Light>();
                if (m_GhostLight == null)
                {
                    var lightGo = new GameObject("GhostAuraLight");
                    lightGo.transform.SetParent(transform, false);
                    lightGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                    m_GhostLight = lightGo.AddComponent<Light>();
                    m_GhostLight.type = LightType.Point;
                    m_GhostLight.color = m_GhostColor;
                    m_GhostLight.range = 6.0f;
                    m_GhostLight.intensity = 2.5f;
                }
            }
        }

        private void Start()
        {
            FindPlayerTransform();
            if (behavior == GhostBehaviorType.Observer)
            {
                currentState = GhostState.Observing;
            }
        }

        private void FindPlayerTransform()
        {
            var player = GameObject.Find("PlayerArmature");
            if (player != null) m_PlayerTransform = player.transform;
            else
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) m_PlayerTransform = p.transform;
            }
        }

        private void Update()
        {
            if (m_IsVanished) return;

            float hoverY = Mathf.Sin(Time.time * m_HoverSpeed) * m_HoverHeight;

            // Handle temporary Stun state
            if (m_StunTimer > 0f)
            {
                m_StunTimer -= Time.deltaTime;
                transform.Rotate(0f, 140f * Time.deltaTime, 0f); // Dizzy spinning
                if (m_GhostLight != null) m_GhostLight.intensity = 1.0f + Mathf.PingPong(Time.time * 8f, 1.5f);
                if (m_StunTimer <= 0f)
                {
                    currentState = GhostState.Returning;
                }
                return;
            }

            // Handle Fleeing state (caused by Flare or Talisman)
            if (m_FleeTimer > 0f)
            {
                m_FleeTimer -= Time.deltaTime;
                transform.position += m_FleeDirection * (m_BaseChaseSpeed * 1.2f * Time.deltaTime);
                if (m_FleeTimer <= 0f)
                {
                    currentState = GhostState.Returning;
                }
                return;
            }

            // Handle Flashlight slow decay
            if (m_FlashlightSlowTimer > 0f)
            {
                m_FlashlightSlowTimer -= Time.deltaTime;
            }

            // Find active target (motorbike or player)
            Transform target = GetActiveTarget();
            float distToTarget = target != null ? Vector3.Distance(transform.position, target.position) : 999f;

            // State Machine transitions
            switch (currentState)
            {
                case GhostState.Observing:
                    UpdateObserving(distToTarget);
                    break;

                case GhostState.Patrolling:
                    UpdatePatrolling(hoverY, target, distToTarget);
                    break;

                case GhostState.Chasing:
                    UpdateChasing(hoverY, target, distToTarget);
                    break;

                case GhostState.Returning:
                    UpdateReturning(hoverY, distToTarget);
                    break;
            }

            // Pulse Aura Light
            if (m_GhostLight != null)
            {
                if (currentState == GhostState.Chasing)
                {
                    m_GhostLight.intensity = 3.6f + Mathf.PingPong(Time.time * 6f, 1.6f);
                    m_GhostLight.color = Color.Lerp(m_GhostColor, Color.red, 0.35f);
                }
                else if (m_FlashlightSlowTimer > 0f)
                {
                    m_GhostLight.intensity = 1.2f;
                    m_GhostLight.color = Color.yellow;
                }
                else
                {
                    m_GhostLight.intensity = 2.2f + Mathf.Sin(Time.time * 3f) * 0.7f;
                    m_GhostLight.color = m_GhostColor;
                }
            }
        }

        private Transform GetActiveTarget()
        {
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr != null && mgr.ActiveTargetTransform != null)
            {
                return mgr.ActiveTargetTransform;
            }

            if (m_PlayerTransform == null || !m_PlayerTransform.gameObject.activeInHierarchy)
            {
                FindPlayerTransform();
            }

            return (m_PlayerTransform != null && m_PlayerTransform.gameObject.activeInHierarchy) ? m_PlayerTransform : null;
        }

        private void UpdateObserving(float distToTarget)
        {
            // Face target
            Transform target = GetActiveTarget();
            if (target != null)
            {
                Vector3 toTarget = target.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), Time.deltaTime * 3f);
                }
            }

            // Vanish into thin air if player comes within 10 meters!
            if (distToTarget < 11.0f)
            {
                VanishWithSpookySound();
            }
        }

        private void VanishWithSpookySound()
        {
            m_IsVanished = true;
            if (m_AudioSource != null)
            {
                AudioSource.PlayClipAtPoint(IUHSoundSynthesizer.GetGhostWhooshSound(), transform.position, 1.0f);
            }
            IUHGrabCampusManager.Instance?.ShowNotification("👻 Bóng ma thoáng qua đã tan biến vào màn đêm!");
            gameObject.SetActive(false);
        }

        private void UpdatePatrolling(float hoverY, Transform target, float distToTarget)
        {
            // Check trigger pursuit
            if (behavior == GhostBehaviorType.Chaser || behavior == GhostBehaviorType.Patroller)
            {
                if (distToTarget <= m_TriggerChaseDist && target != null)
                {
                    currentState = GhostState.Chasing;
                    if (m_AudioSource != null)
                    {
                        m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostMoanSound(), 0.9f);
                    }
                    return;
                }
            }

            // Normal patrol movement
            m_PatrolTimer += Time.deltaTime * m_PatrolSpeed;
            float patrolOffset = Mathf.Sin(m_PatrolTimer) * m_PatrolDistance;

            Vector3 targetPatrolPos = m_BasePosition + (m_PatrolAxis.normalized * patrolOffset) + new Vector3(0f, hoverY, 0f);
            transform.position = Vector3.Lerp(transform.position, targetPatrolPos, Time.deltaTime * 3f);
            transform.Rotate(0f, 25f * Time.deltaTime, 0f);
        }

        private void UpdateChasing(float hoverY, Transform target, float distToTarget)
        {
            // Check if player managed to lose the ghost (Mất dấu)
            if (distToTarget >= m_LoseChaseDist || target == null)
            {
                currentState = GhostState.Returning;
                IUHGrabCampusManager.Instance?.ShowNotification("💨 Bạn đã cắt đuôi hồn ma thành công!");
                if (m_AudioSource != null)
                {
                    m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostMoanSound(), 0.6f);
                }
                gameObject.SetActive(false);
                return;
            }

            // Move towards player
            Vector3 targetPos = target.position + Vector3.up * 1.0f;
            Vector3 toTarget = (targetPos - transform.position);

            // Calculate current chase speed (reduced if blinded by flashlight)
            float speed = m_BaseChaseSpeed;
            if (m_FlashlightSlowTimer > 0f)
            {
                speed *= 0.40f; // 60% slow!
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPos + Vector3.up * hoverY, speed * Time.deltaTime);

            // Face target
            Vector3 lookDir = toTarget;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 7f);
            }

            // Audio scream proximity cue
            if (distToTarget < m_ScreamDistance && !m_HasScreamed)
            {
                m_HasScreamed = true;
                if (m_AudioSource != null)
                {
                    m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostWhooshSound(), 1.0f);
                }
            }
            else if (distToTarget > m_ScreamDistance * 1.5f)
            {
                m_HasScreamed = false;
            }
        }

        private void UpdateReturning(float hoverY, float distToTarget)
        {
            Vector3 targetBase = m_BasePosition + new Vector3(0f, hoverY, 0f);
            transform.position = Vector3.MoveTowards(transform.position, targetBase, (m_BaseChaseSpeed * 0.7f) * Time.deltaTime);

            Vector3 toBase = targetBase - transform.position;
            toBase.y = 0f;
            if (toBase.sqrMagnitude > 0.05f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toBase), Time.deltaTime * 5f);
            }

            if (Vector3.Distance(transform.position, targetBase) < 0.8f)
            {
                currentState = GhostState.Patrolling;
            }
            else if (distToTarget <= m_TriggerChaseDist * 0.75f && behavior == GhostBehaviorType.Chaser)
            {
                // Re-triggered if player comes back close
                currentState = GhostState.Chasing;
            }
        }

        // ================= SURVIVAL ITEM REACTIONS =================

        /// <summary>
        /// Called when player shines flashlight directly at ghost.
        /// </summary>
        public void OnIlluminatedByFlashlight(float dt)
        {
            m_FlashlightSlowTimer = 1.0f; // Keep slow active for 1s
        }

        /// <summary>
        /// Called when player hits ghost with self-defense bat.
        /// </summary>
        public void OnBatHit(Vector3 attackerPosition)
        {
            currentState = GhostState.Stunned;
            m_StunTimer = 4.5f;

            // Knockback 8m
            Vector3 knockbackDir = (transform.position - attackerPosition).normalized;
            knockbackDir.y = 0.2f;
            transform.position += knockbackDir * 8.0f;

            if (m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetBatHitSound(), 1.0f);
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostWhooshSound(), 0.9f);
            }
        }

        /// <summary>
        /// Called when roadside flare burns near ghost.
        /// </summary>
        public void OnScaredByFlare(Vector3 flarePos)
        {
            if (currentState == GhostState.Fleeing) return;

            currentState = GhostState.Fleeing;
            m_FleeTimer = 6.0f;
            Vector3 fleeDir = (transform.position - flarePos).normalized;
            fleeDir.y = 0f;
            m_FleeDirection = fleeDir;

            if (m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostMoanSound(), 1.0f);
            }
        }

        /// <summary>
        /// Called when Talisman aura wards off the ghost.
        /// </summary>
        public void OnWardedByTalisman(Vector3 talismanPos)
        {
            Vector3 wardDir = (transform.position - talismanPos).normalized;
            wardDir.y = 0f;
            transform.position += wardDir * 6.0f * Time.deltaTime;

            if (currentState == GhostState.Chasing)
            {
                currentState = GhostState.Returning;
            }
        }

        // ================= COLLISION & DAMAGE =================

        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.gameObject);
        }

        private void HandleCollision(GameObject otherGo)
        {
            if (m_IsVanished || m_StunTimer > 0f || m_FleeTimer > 0f) return;

            // Check if player has active Talisman shield
            var inv = IUHGrabInventoryManager.Instance;
            if (inv != null && inv.IsTalismanActive)
            {
                // Protected by sacred talisman!
                OnWardedByTalisman(otherGo.transform.position);
                IUHGrabCampusManager.Instance?.ShowNotification("🛡️ BÙA HỘ MỆNH ĐÃ CHẶN ĐÒN TẤN CÔNG CỦA MA!");
                AudioSource.PlayClipAtPoint(IUHSoundSynthesizer.GetTalismanChantSound(), transform.position, 1.0f);
                return;
            }

            // Normal damage
            var bike = otherGo.GetComponentInParent<IUHGrabMotorbike>();
            if (bike != null)
            {
                bike.OnGhostImpact(m_CargoDamage);
                PlaySpookyHitEffect();
                return;
            }

            if (otherGo.CompareTag("Player") || otherGo.name.Contains("Player"))
            {
                var mgr = IUHGrabCampusManager.Instance;
                if (mgr != null && mgr.IsMissionActive)
                {
                    mgr.ApplyCargoDamage(m_CargoDamage, "Bị Hồn Ma Tấn Công!");
                }
                PlaySpookyHitEffect();
            }
        }

        private void PlaySpookyHitEffect()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 1.0f);
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostWhooshSound(), 1.0f);
            }

            if (m_GhostLight != null)
            {
                m_GhostLight.color = Color.magenta;
                m_GhostLight.intensity = 5.0f;
            }

            // Temporarily retreat 4m after attacking so player has breathing room
            currentState = GhostState.Returning;
        }

        public void ResetGhost()
        {
            m_IsVanished = false;
            gameObject.SetActive(true);
            transform.position = m_BasePosition;
            currentState = (behavior == GhostBehaviorType.Observer) ? GhostState.Observing : GhostState.Patrolling;
            m_StunTimer = 0f;
            m_FleeTimer = 0f;
            m_FlashlightSlowTimer = 0f;
        }

        public void SpawnAt(Vector3 position, GhostBehaviorType newBehavior)
        {
            m_IsVanished = false;
            m_BasePosition = position;
            transform.position = position;
            behavior = newBehavior;
            currentState = (behavior == GhostBehaviorType.Observer) ? GhostState.Observing : 
                           (behavior == GhostBehaviorType.Chaser) ? GhostState.Chasing : GhostState.Patrolling;
            m_StunTimer = 0f;
            m_FleeTimer = 0f;
            m_FlashlightSlowTimer = 0f;
            m_HasScreamed = false;

            if (m_GhostLight != null)
            {
                m_GhostLight.color = m_GhostColor;
                m_GhostLight.intensity = 2.5f;
            }

            gameObject.SetActive(true);
        }
    }
}
