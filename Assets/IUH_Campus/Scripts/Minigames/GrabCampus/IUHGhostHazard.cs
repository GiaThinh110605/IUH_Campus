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

        private void Update()
        {
            // Sinusoidal floating bob
            float hoverY = Mathf.Sin(Time.time * m_HoverSpeed) * m_HoverHeight;

            // Patrol movement
            m_PatrolTimer += Time.deltaTime * m_PatrolSpeed;
            float patrolOffset = Mathf.Sin(m_PatrolTimer) * m_PatrolDistance;

            transform.position = m_BasePosition + (m_PatrolAxis.normalized * patrolOffset) + new Vector3(0f, hoverY, 0f);

            // Subtle rotation / swaying
            transform.Rotate(0f, 25f * Time.deltaTime, 0f);

            // Pulse light intensity
            if (m_GhostLight != null)
            {
                m_GhostLight.intensity = 2.0f + Mathf.Sin(Time.time * 4f) * 0.8f;
            }

            // Proximity spook sound check
            if (m_PlayerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
                if (dist < m_ScreamDistance && !m_HasScreamed)
                {
                    m_HasScreamed = true;
                    m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetGhostWhooshSound(), 0.9f);
                }
                else if (dist > m_ScreamDistance * 1.6f)
                {
                    m_HasScreamed = false;
                }
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
