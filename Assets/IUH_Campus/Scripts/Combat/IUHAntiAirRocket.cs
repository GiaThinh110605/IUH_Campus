using System;
using System.Collections;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHAntiAirRocket : MonoBehaviour
    {
        [Header("Flight Dynamics")]
        [SerializeField] private float m_Speed = 52f;
        [SerializeField] private float m_TurnSpeed = 26f;
        [SerializeField] private float m_ProximityRadius = 2.2f;
        [SerializeField] private float m_BlastRadius = 6.0f;
        [SerializeField] private int m_Damage = 95;

        private Transform m_Target;
        private Vector3 m_Direction;
        private bool m_HasDetonated = false;

        public void Initialize(Transform target, Vector3 initialDir, int damage)
        {
            m_Target = target;
            m_Direction = initialDir.normalized;
            m_Damage = damage;
            transform.forward = m_Direction;

            Destroy(gameObject, 4.5f);
        }

        private void Update()
        {
            if (m_HasDetonated) return;

            // Smooth Homing Trajectory towards Target
            if (m_Target != null && m_Target.gameObject.activeInHierarchy)
            {
                Vector3 toTarget = (m_Target.position - transform.position).normalized;
                m_Direction = Vector3.RotateTowards(m_Direction, toTarget, m_TurnSpeed * Mathf.Deg2Rad * Time.deltaTime * 60f, 0f);
                transform.forward = m_Direction;

                // Mid-air proximity fuse
                float dist = Vector3.Distance(transform.position, m_Target.position);
                if (dist <= m_ProximityRadius)
                {
                    Detonate(m_Target.position);
                    return;
                }
            }

            Vector3 step = m_Direction * (m_Speed * Time.deltaTime);
            Vector3 nextPos = transform.position + step;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, m_Direction, out hit, step.magnitude, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.name.Contains("Student") && !hit.collider.CompareTag("Player"))
                {
                    Detonate(hit.point);
                    return;
                }
            }

            transform.position = nextPos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_HasDetonated) return;
            if (other.name.Contains("Student") || other.CompareTag("Player")) return;
            Detonate(transform.position);
        }

        private void Detonate(Vector3 explosionPoint)
        {
            if (m_HasDetonated) return;
            m_HasDetonated = true;

            // Flak Explosion VFX
            SpawnFlakBurstVFX(explosionPoint);

            // Screen Shake
            IUHCameraShake.Shake(0.35f, 0.22f);

            // Audio cues
            IUHCombatAudio.PlaySkillBurst();
            IUHCombatAudio.PlayBossHit();

            // Deal radial damage & stun to all monsters in blast radius
            Collider[] hits = Physics.OverlapSphere(explosionPoint, m_BlastRadius);
            foreach (var col in hits)
            {
                var monster = col.GetComponentInParent<IUHRealtimeMonsterTarget>();
                if (monster != null && !monster.IsDead)
                {
                    monster.TakeDamage(m_Damage, isCrit: true);
                    monster.ApplyStun(2.5f);
                }
            }

            IUHCombatFloatingText.Spawn(explosionPoint + Vector3.up * 1.5f, "💥 NỔ PHÒNG KHÔNG FLAK BURST!", new Color(1f, 0.5f, 0.1f), 1.6f);

            Destroy(gameObject);
        }

        private void SpawnFlakBurstVFX(Vector3 point)
        {
            GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            burst.name = "Flak_Burst_VFX";
            burst.transform.position = point;
            burst.transform.localScale = Vector3.one * 1.2f;

            var rend = burst.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.45f, 0.05f, 0.95f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 6f);
            rend.sharedMaterial = mat;
            Destroy(burst.GetComponent<Collider>());

            // Spawn smoke/spark ring
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Flak_Ring_VFX";
            ring.transform.position = point;
            ring.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
            var rRend = ring.GetComponent<Renderer>();
            Material rMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            rMat.color = new Color(1f, 0.7f, 0.2f, 0.8f);
            rMat.EnableKeyword("_EMISSION");
            rMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.2f) * 4f);
            rRend.sharedMaterial = rMat;
            Destroy(ring.GetComponent<Collider>());

            // Animate burst expansion
            var runner = new GameObject("Burst_Coroutine_Runner").AddComponent<EmptyMonoBehaviour>();
            runner.StartCoroutine(AnimateBurst(burst, ring, mat, rMat, runner.gameObject));
        }

        private static IEnumerator AnimateBurst(GameObject sphere, GameObject ring, Material sMat, Material rMat, GameObject runner)
        {
            float elapsed = 0f;
            float dur = 0.45f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / dur;

                if (sphere != null)
                {
                    sphere.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 7.5f, p);
                    sMat.color = new Color(1f, 0.45f, 0.05f, (1f - p) * 0.95f);
                }

                if (ring != null)
                {
                    float ringScale = Mathf.Lerp(1f, 10f, p);
                    ring.transform.localScale = new Vector3(ringScale, 0.02f, ringScale);
                    rMat.color = new Color(1f, 0.7f, 0.2f, (1f - p) * 0.8f);
                }

                yield return null;
            }

            if (sphere != null) Destroy(sphere);
            if (ring != null) Destroy(ring);
            if (sMat != null) Destroy(sMat);
            if (rMat != null) Destroy(rMat);
            if (runner != null) Destroy(runner);
        }

        private class EmptyMonoBehaviour : MonoBehaviour { }
    }
}

