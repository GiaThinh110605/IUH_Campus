using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    [RequireComponent(typeof(IUHRealtimeMonsterTarget))]
    public class IUHFlyingMonsterAI : MonoBehaviour
    {
        [Header("Flight Dynamics")]
        [SerializeField] private float m_FlightAltitude = 3.6f;
        [SerializeField] private float m_FlightSpeed = 5.2f;
        [SerializeField] private float m_PatrolRadius = 8f;
        [SerializeField] private float m_AggroRadius = 65f;
        [SerializeField] private float m_OptimalDistance = 7.5f;

        [Header("Combat & Weapons")]
        [SerializeField] private float m_AttackCooldown = 3.8f;
        [SerializeField] private int m_BombDamage = 22;
        [SerializeField] private Transform[] m_FireMuzzles;
        [SerializeField] private LineRenderer m_LaserBeam;

        [Header("Visual Parts")]
        [SerializeField] private Transform[] m_RotorBlades;
        [SerializeField] private float m_RotorSpinSpeed = 1200f;

        private IUHRealtimeMonsterTarget m_TargetStats;
        private Transform m_PlayerTransform;
        private Vector3 m_OriginPos;
        private Vector3 m_ZoneAnchor = Vector3.zero;
        private float m_NextAttackTime = 0f;
        private float m_PatrolAngle = 0f;
        private bool m_IsAttacking = false;

        public void SetZoneAnchor(Vector3 anchor)
        {
            m_ZoneAnchor = anchor;
            m_OriginPos = anchor;
            m_OriginPos.y = m_FlightAltitude;
        }

        private void Start()
        {
            m_OriginPos = transform.position;
            m_OriginPos.y = m_FlightAltitude;
            transform.position = m_OriginPos;

            m_TargetStats = GetComponent<IUHRealtimeMonsterTarget>();
            if (m_TargetStats != null)
            {
                m_TargetStats.OnDefeated += OnDestroyed;
            }

            FindPlayer();

            if (m_LaserBeam != null)
            {
                m_LaserBeam.positionCount = 2;
                m_LaserBeam.enabled = false;
            }

            m_NextAttackTime = Time.time + UnityEngine.Random.Range(1.5f, 3.0f);
        }

        private void OnEnable()
        {
            FindPlayer();
            m_IsAttacking = false;
            m_NextAttackTime = Time.time + UnityEngine.Random.Range(1.2f, 2.2f);

            if (m_TargetStats == null) m_TargetStats = GetComponent<IUHRealtimeMonsterTarget>();
            if (m_TargetStats != null)
            {
                m_TargetStats.OnDefeated -= OnDestroyed;
                m_TargetStats.OnDefeated += OnDestroyed;
            }

            if (m_LaserBeam != null)
            {
                m_LaserBeam.positionCount = 2;
                m_LaserBeam.enabled = false;
            }

            IUHCombatAudio.PlaySkillBurst();
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.5f, $"⚠️ {gameObject.name}\nĐÃ VÀO VỊ TRÍ CHIẾN ĐẤU!", new Color(1f, 0.25f, 0.2f), 2.5f);
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void Update()
        {
            // Spin Rotors
            if (m_RotorBlades != null)
            {
                for (int i = 0; i < m_RotorBlades.Length; i++)
                {
                    if (m_RotorBlades[i] != null)
                    {
                        m_RotorBlades[i].Rotate(Vector3.up, m_RotorSpinSpeed * Time.deltaTime, Space.Self);
                    }
                }
            }

            if (m_TargetStats != null && (m_TargetStats.IsDead || m_TargetStats.IsStunned))
            {
                if (m_LaserBeam != null) m_LaserBeam.enabled = false;
                return;
            }

            // If Match-3 puzzle minigame is active, stop attacking and patrol peacefully
            if (IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null && IUHCampus.Guardian.Match3.Match3CombatManager.Instance.IsCombatActive)
            {
                if (m_LaserBeam != null) m_LaserBeam.enabled = false;
                PatrolSky();
                return;
            }

            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            Vector3 toPlayer = m_PlayerTransform.position - transform.position;
            float horizontalDist = new Vector2(toPlayer.x, toPlayer.z).magnitude;

            if (horizontalDist <= m_AggroRadius)
            {
                // In Combat: circle around player at high altitude and engage
                EngagePlayer(toPlayer, horizontalDist);
            }
            else
            {
                // Patrol in circle around origin in the sky
                PatrolSky();
            }
        }

        private void PatrolSky()
        {
            if (m_LaserBeam != null) m_LaserBeam.enabled = false;

            m_PatrolAngle += (m_FlightSpeed / m_PatrolRadius) * Time.deltaTime;
            Vector3 targetPos = m_OriginPos + new Vector3(Mathf.Sin(m_PatrolAngle) * m_PatrolRadius, Mathf.Sin(Time.time * 1.5f) * 0.8f, Mathf.Cos(m_PatrolAngle) * m_PatrolRadius);

            Vector3 flyDir = (targetPos - transform.position);
            if (flyDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flyDir.normalized), Time.deltaTime * 3.5f);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, m_FlightSpeed * Time.deltaTime);
            }
        }

        private void EngagePlayer(Vector3 toPlayer, float horizontalDist)
        {
            // Aim nose, cannons and eye directly at player's chest without any connecting line
            Vector3 aimTarget = m_PlayerTransform.position + Vector3.up * 1.1f;
            Vector3 lookDir = (aimTarget - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);

            // Hover in the front hemisphere of the player so the player ALWAYS sees it directly on screen!
            Vector3 forwardOffset = m_PlayerTransform.forward * (m_OptimalDistance * 0.9f);
            Vector3 sideOffset = m_PlayerTransform.right * (Mathf.Sin(Time.time * 0.8f) * (m_OptimalDistance * 0.45f));
            float hoverHeight = m_FlightAltitude + Mathf.Sin(Time.time * 2.2f) * 0.35f;
            Vector3 desiredPos = m_PlayerTransform.position + forwardOffset + sideOffset + Vector3.up * hoverHeight;

            // Restrict movement to its designated zone area so it never leaves its zone
            if (m_ZoneAnchor != Vector3.zero)
            {
                Vector3 offsetFromAnchor = desiredPos - m_ZoneAnchor;
                offsetFromAnchor.y = 0f;
                if (offsetFromAnchor.magnitude > 14f)
                {
                    desiredPos = m_ZoneAnchor + offsetFromAnchor.normalized * 14f + Vector3.up * hoverHeight;
                }
            }

            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 2.4f);

            // Keep targeting line disabled - monster aims purely via rotation/facing
            if (m_LaserBeam != null) m_LaserBeam.enabled = false;

            // Attack sequence
            if (Time.time >= m_NextAttackTime && !m_IsAttacking)
            {
                m_NextAttackTime = Time.time + m_AttackCooldown;
                StartCoroutine(AirBombardmentRoutine());
            }
        }

        private IEnumerator AirBombardmentRoutine()
        {
            m_IsAttacking = true;

            // Warning Laser Flash & Sound
            IUHCombatAudio.PlaySkillBurst();
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.5f, "⚠️ KHÓA MỤC TIÊU MẶT ĐẤT!", new Color(1f, 0.2f, 0.2f), 1.2f);

            yield return new WaitForSeconds(0.6f);

            if (m_TargetStats != null && m_TargetStats.IsDead)
            {
                m_IsAttacking = false;
                yield break;
            }

            // Fire 2 volleys of Plasma Bombs downwards
            for (int volley = 0; volley < 2; volley++)
            {
                if (m_PlayerTransform == null) break;

                Vector3 targetLead = m_PlayerTransform.position + UnityEngine.Random.insideUnitSphere * 0.8f;
                targetLead.y = 0.1f;

                Vector3 spawnPos = transform.position + transform.forward * 0.5f + Vector3.down * 0.4f;
                if (m_FireMuzzles != null && m_FireMuzzles.Length > 0)
                {
                    var m = m_FireMuzzles[volley % m_FireMuzzles.Length];
                    if (m != null) spawnPos = m.position;
                }

                SpawnPlasmaBomb(spawnPos, targetLead);

                // Sound
                IUHCombatAudio.PlayPunchHit();

                // Air Recoil
                transform.position += Vector3.up * 0.25f;

                yield return new WaitForSeconds(0.25f);
            }

            m_IsAttacking = false;
        }

        private void SpawnPlasmaBomb(Vector3 origin, Vector3 targetGround)
        {
            GameObject bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bomb.name = "Sky_Plasma_Bomb";
            bomb.transform.position = origin;
            bomb.transform.localScale = Vector3.one * 0.55f;

            var rend = bomb.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.35f, 0.05f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.35f, 0.05f) * 3.5f);
            rend.sharedMaterial = mat;

            var col = bomb.GetComponent<SphereCollider>();
            if (col != null) col.isTrigger = true;

            var trail = bomb.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.25f;
            trail.endWidth = 0.03f;
            trail.sharedMaterial = mat;

            var bComp = bomb.AddComponent<IUHFlyingPlasmaBomb>();
            bComp.Initialize(targetGround, m_BombDamage);
        }

        private void OnDestroyed()
        {
            if (m_LaserBeam != null) m_LaserBeam.enabled = false;

            // Spawn Mid-Air Explosion
            GameObject airExplosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            airExplosion.name = "MidAir_Drone_Explosion";
            airExplosion.transform.position = transform.position;
            airExplosion.transform.localScale = Vector3.one * 3.5f;

            var rend = airExplosion.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.45f, 0.1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 4f);
            rend.sharedMaterial = mat;

            Destroy(airExplosion.GetComponent<Collider>());
            Destroy(airExplosion, 0.35f);

            // Drop 5 coins falling from the sky
            for (int i = 0; i < 5; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coin.name = "Falling_Sky_Gold_Coin";
                coin.transform.position = transform.position + UnityEngine.Random.insideUnitSphere * 0.8f;
                coin.transform.localScale = new Vector3(0.5f, 0.07f, 0.5f);
                coin.transform.rotation = Quaternion.Euler(90f, UnityEngine.Random.Range(0, 360), 0);

                var cRend = coin.GetComponent<Renderer>();
                var cMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                cMat.color = new Color(1f, 0.85f, 0.15f);
                cMat.EnableKeyword("_EMISSION");
                cMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.1f) * 2.5f);
                cRend.sharedMaterial = cMat;

                var rb = coin.AddComponent<Rigidbody>();
                rb.linearVelocity = new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(1f, 3f), UnityEngine.Random.Range(-3f, 3f));
                rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 12f;

                var pickup = coin.AddComponent<IUHLootCoinPickup>();
                pickup.Initialize(20);
            }
        }
    }
}

