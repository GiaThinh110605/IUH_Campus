using UnityEngine;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Haunted Ghost Obstacle for Mini-Game 2.1 "Grab Campus: Chuyến xe tâm linh".
    /// Features floating sinusoidal hovering, spectral glow, proximity sound cues,
    /// and impact penalties against the Grab delivery cargo.
    /// </summary>
    public class IUHGhostHazard : MonoBehaviour
    {
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

        private Vector3 m_BasePosition;
        private AudioSource m_AudioSource;
        private bool m_HasScreamed = false;
        private float m_PatrolTimer = 0f;
        private Transform m_PlayerTransform;

        private void Awake()
        {
            m_BasePosition = transform.position;
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 0.85f;
            m_AudioSource.playOnAwake = false;
            m_AudioSource.maxDistance = 25f;
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
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) m_PlayerTransform = player.transform;
        }

        [Header("Pursuit AI")]
        [SerializeField] private float m_ChaseSpeed = 6.2f;
        [SerializeField] private float m_TriggerChaseDist = 26.0f;
        [SerializeField] private float m_LoseChaseDist = 34.0f;
        private bool m_IsChasing = false;

        private void Update()
        {
            // Sinusoidal floating bob
            float hoverY = Mathf.Sin(Time.time * m_HoverSpeed) * m_HoverHeight;

            // Find active target (motorbike if mounted, otherwise player)
            Transform target = null;
            var bike = Object.FindFirstObjectByType<IUHGrabMotorbike>();
            if (bike != null && bike.IsMounted)
            {
                target = bike.transform;
            }
            else
            {
                if (m_PlayerTransform == null || !m_PlayerTransform.gameObject.activeInHierarchy)
                {
                    var p = GameObject.Find("PlayerArmature");
                    if (p != null) m_PlayerTransform = p.transform;
                }
                if (m_PlayerTransform != null && m_PlayerTransform.gameObject.activeInHierarchy)
                {
                    target = m_PlayerTransform;
                }
            }

            float distToTarget = target != null ? Vector3.Distance(transform.position, target.position) : 999f;

            // State Transition: Patrol vs Chasing
            if (distToTarget <= m_TriggerChaseDist && target != null)
            {
                m_IsChasing = true;
            }
            else if (distToTarget >= m_LoseChaseDist)
            {
                m_IsChasing = false;
            }

            if (m_IsChasing && target != null)
            {
                // Active Pursuit Chase AI
                Vector3 targetPos = target.position + Vector3.up * 1.0f;
                Vector3 toTarget = (targetPos - transform.position);
                Vector3 dir = toTarget.normalized;

                // Move directly toward the player/motorbike
                transform.position = Vector3.MoveTowards(transform.position, targetPos + Vector3.up * hoverY, m_ChaseSpeed * Time.deltaTime);

                // Face directly at target
                Vector3 lookDir = toTarget;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 6f);
                }

                // Audio scream
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
            else
            {
                // Patrol movement around base position
                m_PatrolTimer += Time.deltaTime * m_PatrolSpeed;
                float patrolOffset = Mathf.Sin(m_PatrolTimer) * m_PatrolDistance;

                Vector3 targetPatrolPos = m_BasePosition + (m_PatrolAxis.normalized * patrolOffset) + new Vector3(0f, hoverY, 0f);
                transform.position = Vector3.Lerp(transform.position, targetPatrolPos, Time.deltaTime * 3f);

                transform.Rotate(0f, 25f * Time.deltaTime, 0f);
            }

            // Pulse spectral aura light
            if (m_GhostLight != null)
            {
                m_GhostLight.intensity = m_IsChasing ? (3.5f + Mathf.PingPong(Time.time * 6f, 1.5f)) : (2.2f + Mathf.Sin(Time.time * 3f) * 0.8f);
            }
        }

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
            // Check if player or motorcycle
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
            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetDamageSound(), 1.0f);
            m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostWhooshSound(), 1.0f);

            // Temporarily flash light red/purple
            if (m_GhostLight != null)
            {
                m_GhostLight.color = Color.magenta;
                m_GhostLight.intensity = 5.0f;
            }
        }
    }
}
