using System;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHPlasmaBullet : MonoBehaviour
    {
        [Header("Flight Dynamics")]
        [SerializeField] private float m_Speed = 58f;
        [SerializeField] private float m_Lifetime = 2.5f;
        [SerializeField] private int m_Damage = 32;
        [SerializeField] private bool m_IsCrit = false;

        [Header("Components")]
        [SerializeField] private TrailRenderer m_Trail;
        [SerializeField] private Light m_Light;

        private Vector3 m_Direction;
        private Vector3 m_LastPos;
        private bool m_HasHit = false;
        private Transform m_HomingTarget;
        private float m_HomingTurnSpeed = 18f;

        public void Initialize(Vector3 direction, int damage, bool isCrit)
        {
            m_Direction = direction.normalized;
            m_Damage = damage;
            m_IsCrit = isCrit;
            m_LastPos = transform.position;
            transform.forward = m_Direction;

            Destroy(gameObject, m_Lifetime);
        }

        public void SetHomingTarget(Transform target, float turnSpeed = 18f)
        {
            m_HomingTarget = target;
            m_HomingTurnSpeed = turnSpeed;
        }

        private void Update()
        {
            if (m_HasHit) return;

            // Smart Anti-Air Homing Guidance
            if (m_HomingTarget != null && m_HomingTarget.gameObject.activeInHierarchy)
            {
                Vector3 toTarget = (m_HomingTarget.position - transform.position).normalized;
                m_Direction = Vector3.RotateTowards(m_Direction, toTarget, m_HomingTurnSpeed * Mathf.Deg2Rad * Time.deltaTime * 60f, 0f);
                transform.forward = m_Direction;
            }

            Vector3 nextPos = transform.position + m_Direction * (m_Speed * Time.deltaTime);

            // Continuous raycast from last pos to next pos so bullets NEVER tunnel through thin objects
            Vector3 travelDir = nextPos - m_LastPos;
            float travelDist = travelDir.magnitude;

            if (travelDist > 0.001f)
            {
                RaycastHit hit;
                int layerMask = ~LayerMask.GetMask("Ignore Raycast", "TransparentFX");

                if (Physics.Raycast(m_LastPos, travelDir.normalized, out hit, travelDist, layerMask, QueryTriggerInteraction.Ignore))
                {
                    OnBulletImpact(hit.collider, hit.point, hit.normal);
                    return;
                }
            }

            transform.position = nextPos;
            m_LastPos = nextPos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_HasHit) return;
            if (other.name.Contains("Student") || other.CompareTag("Player")) return;

            OnBulletImpact(other, transform.position, -m_Direction);
        }

        private void OnBulletImpact(Collider col, Vector3 point, Vector3 normal)
        {
            if (m_HasHit) return;
            m_HasHit = true;

            // Check if we hit a Monster
            var monster = col.GetComponentInParent<IUHRealtimeMonsterTarget>();
            if (monster != null && !monster.IsDead)
            {
                monster.TakeDamage(m_Damage, m_IsCrit);
            }

            // Spawn Impact Spark VFX
            SpawnImpactVFX(point, normal);

            // Audio
            IUHCombatAudio.PlayBossHit();

            Destroy(gameObject);
        }

        private void SpawnImpactVFX(Vector3 point, Vector3 normal)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "Plasma_Impact_Sparks";
            spark.transform.position = point + normal * 0.05f;
            spark.transform.localScale = Vector3.one * 0.35f;

            var rend = spark.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = m_IsCrit ? new Color(1f, 0.4f, 0.1f) : new Color(0.2f, 0.95f, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", mat.color * 4f);
            rend.sharedMaterial = mat;
            Destroy(spark.GetComponent<Collider>());

            Destroy(spark, 0.25f);
        }
    }
}

