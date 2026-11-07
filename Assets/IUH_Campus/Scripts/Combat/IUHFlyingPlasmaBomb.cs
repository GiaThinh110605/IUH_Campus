using System;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHFlyingPlasmaBomb : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 22f;
        [SerializeField] private int m_Damage = 22;
        [SerializeField] private float m_ExplosionRadius = 2.8f;
        [SerializeField] private float m_Lifetime = 5f;

        private Vector3 m_Direction;
        private Vector3 m_TargetGroundPos;
        private bool m_HasExploded = false;

        public void Initialize(Vector3 targetPos, int damage)
        {
            m_Damage = damage;
            m_TargetGroundPos = targetPos;
            m_Direction = (targetPos - transform.position).normalized;
            transform.forward = m_Direction;

            Destroy(gameObject, m_Lifetime);
        }

        private void Update()
        {
            if (m_HasExploded) return;

            Vector3 step = m_Direction * (m_Speed * Time.deltaTime);
            Vector3 nextPos = transform.position + step;

            // Raycast ahead to detect collision with ground or student
            RaycastHit hit;
            if (Physics.Raycast(transform.position, m_Direction, out hit, step.magnitude * 1.5f, ~LayerMask.GetMask("Ignore Raycast", "TransparentFX"), QueryTriggerInteraction.Ignore))
            {
                Explode(hit.point);
                return;
            }

            transform.position = nextPos;

            // If passed target height (near ground)
            if (transform.position.y <= m_TargetGroundPos.y + 0.2f)
            {
                Explode(transform.position);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_HasExploded) return;
            if (other.name.Contains("SKY_DRONE") || other.GetComponent<IUHFlyingMonsterAI>() != null) return;

            Explode(transform.position);
        }

        private void Explode(Vector3 point)
        {
            if (m_HasExploded) return;
            m_HasExploded = true;

            // Spawn Ground Blast Explosion VFX
            SpawnExplosionVFX(point);

            // Audio
            IUHCombatAudio.PlayPunchHit();

            // Damage player if inside radius
            Collider[] hits = Physics.OverlapSphere(point, m_ExplosionRadius);
            foreach (var col in hits)
            {
                var health = col.GetComponentInParent<IUHPlayerHealth>();
                if (health != null && !health.IsDead)
                {
                    health.TakeDamage(m_Damage, point);
                    IUHCombatFloatingText.Spawn(health.transform.position + Vector3.up * 2.2f, $"💥 OANH TẠC: -{m_Damage} HP!", new Color(1f, 0.3f, 0.1f), 1.5f);
                    break;
                }
            }

            Destroy(gameObject);
        }

        private void SpawnExplosionVFX(Vector3 point)
        {
            GameObject blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blast.name = "Air_Bomb_Explosion";
            blast.transform.position = point + Vector3.up * 0.2f;
            blast.transform.localScale = Vector3.one * (m_ExplosionRadius * 1.5f);

            var rend = blast.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.25f, 0.05f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.35f, 0.05f) * 4f);
            rend.sharedMaterial = mat;

            Destroy(blast.GetComponent<Collider>());
            Destroy(blast, 0.25f);
        }
    }
}

