using System;
using System.Collections;
using UnityEngine;
using IUHCampus.Core;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHEmergencySupplyCrate : MonoBehaviour
    {
        [Header("Landing")]
        [SerializeField] private float m_FallSpeed = 5.5f;
        [SerializeField] private float m_GroundY = 0.35f;
        [SerializeField] private float m_PickupRadius = 2.4f;
        [SerializeField] private int m_HealHp = 100;

        private bool m_HasLanded = false;
        private bool m_HasPickedUp = false;
        private Transform m_PlayerTransform;
        private Light m_BeaconLight;
        private GameObject m_Parachute;

        public void Initialize(Vector3 spawnPos, Transform playerTransform)
        {
            transform.position = spawnPos;
            m_PlayerTransform = playerTransform;
            m_GroundY = (playerTransform != null) ? playerTransform.position.y + 0.35f : 0.35f;

            BuildVisuals();
            Destroy(gameObject, 45f); // Auto clean up if not picked up
        }

        private void BuildVisuals()
        {
            var rend = GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = CreateEmissiveMat(new Color(0.1f, 0.95f, 0.4f), new Color(0.1f, 0.95f, 0.4f) * 3f);

            m_BeaconLight = gameObject.AddComponent<Light>();
            m_BeaconLight.type = LightType.Point;
            m_BeaconLight.color = new Color(0.2f, 1f, 0.5f);
            m_BeaconLight.intensity = 3.5f;
            m_BeaconLight.range = 6.0f;

            m_Parachute = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_Parachute.name = "Supply_Parachute";
            m_Parachute.transform.SetParent(transform, false);
            m_Parachute.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            m_Parachute.transform.localScale = new Vector3(2.0f, 0.55f, 2.0f);
            Destroy(m_Parachute.GetComponent<Collider>());
            m_Parachute.GetComponent<Renderer>().sharedMaterial =
                CreateEmissiveMat(new Color(0.85f, 1f, 0.85f, 0.95f), new Color(0.2f, 0.9f, 0.4f) * 1.5f);

            var crossMat = CreateEmissiveMat(Color.white, Color.white * 2f);
            CreateCrossBar("Cross_H", new Vector3(0.55f, 0.05f, 0.2f), crossMat);
            CreateCrossBar("Cross_V", new Vector3(0.2f, 0.05f, 0.55f), crossMat);
        }

        private static Material CreateEmissiveMat(Color color, Color emission)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            return mat;
        }

        private void CreateCrossBar(string barName, Vector3 scale, Material mat)
        {
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = barName;
            bar.transform.SetParent(transform, false);
            bar.transform.localPosition = new Vector3(0f, 0.51f, 0f);
            bar.transform.localScale = scale;
            Destroy(bar.GetComponent<Collider>());
            bar.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private void Update()
        {
            if (m_HasPickedUp) return;

            // 1. Descend until ground is reached
            if (!m_HasLanded)
            {
                transform.position += Vector3.down * (m_FallSpeed * Time.deltaTime);
                transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.Self);

                if (transform.position.y <= m_GroundY)
                {
                    m_HasLanded = true;
                    transform.position = new Vector3(transform.position.x, m_GroundY, transform.position.z);
                    if (m_Parachute != null)
                    {
                        StartCoroutine(FoldParachuteRoutine());
                    }
                    SpawnLandingPulse(transform.position);
                }
            }
            else
            {
                // Hover bobbing & beacon pulse on ground
                float bob = Mathf.Sin(Time.time * 3f) * 0.05f;
                transform.position = new Vector3(transform.position.x, m_GroundY + bob, transform.position.z);
                transform.Rotate(Vector3.up, 25f * Time.deltaTime, Space.Self);
                if (m_BeaconLight != null)
                {
                    m_BeaconLight.intensity = 2.5f + Mathf.Sin(Time.time * 5f) * 1.5f;
                }
            }

            // 2. Proximity pickup check
            if (m_PlayerTransform == null)
            {
                m_PlayerTransform = IUHPlayerLocator.Player;
            }

            if (m_PlayerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, m_PlayerTransform.position);
                if (dist <= m_PickupRadius)
                {
                    PickupSupply();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_HasPickedUp) return;
            if (other.name.Contains("Student") || other.CompareTag("Player"))
            {
                PickupSupply();
            }
        }

        private void PickupSupply()
        {
            if (m_HasPickedUp) return;
            m_HasPickedUp = true;

            // Heal Player & Shield
            if (IUHPlayerHealth.Instance != null)
            {
                IUHPlayerHealth.Instance.Heal(m_HealHp);
            }

            IUHCombatAudio.PlayHeal();
            IUHCombatAudio.PlayShield();

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.8f, "💚 ĐÃ NHẬN HÒM TIẾP TẾ!\nPHỤC HỒI 100% MÁU & LÁ CHẮN!", new Color(0.1f, 1f, 0.4f), 2.5f);

            // Healing Burst VFX
            SpawnHealBurstVFX(transform.position);

            Destroy(gameObject, 0.05f);
        }

        private IEnumerator FoldParachuteRoutine()
        {
            if (m_Parachute == null) yield break;
            float elapsed = 0f;
            float dur = 0.5f;
            Vector3 startScale = m_Parachute.transform.localScale;
            while (elapsed < dur && m_Parachute != null)
            {
                elapsed += Time.deltaTime;
                m_Parachute.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / dur);
                yield return null;
            }
            if (m_Parachute != null) Destroy(m_Parachute);
        }

        private void SpawnLandingPulse(Vector3 pos)
        {
            GameObject pulse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pulse.name = "Supply_Landing_Pulse";
            pulse.transform.position = pos + Vector3.up * 0.05f;
            pulse.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
            Destroy(pulse.GetComponent<Collider>());

            var rend = pulse.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.2f, 1f, 0.5f, 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.2f, 1f, 0.5f) * 3f);
            rend.sharedMaterial = mat;

            StartCoroutine(AnimateRing(pulse, mat, 4.5f, 0.4f));
        }

        private void SpawnHealBurstVFX(Vector3 pos)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Heal_Burst_Sphere";
            sphere.transform.position = pos + Vector3.up * 1f;
            sphere.transform.localScale = Vector3.one * 1f;
            Destroy(sphere.GetComponent<Collider>());

            var rend = sphere.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.1f, 1f, 0.4f, 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.2f, 1.2f, 0.5f) * 4f);
            rend.sharedMaterial = mat;

            StartCoroutine(AnimateBurst(sphere, mat, 3.5f, 0.35f));
        }

        private IEnumerator AnimateRing(GameObject ring, Material mat, float maxScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && ring != null)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;
                float s = Mathf.Lerp(0.5f, maxScale, p);
                ring.transform.localScale = new Vector3(s, 0.02f, s);
                mat.color = new Color(0.2f, 1f, 0.5f, (1f - p) * 0.8f);
                yield return null;
            }
            if (ring != null) Destroy(ring);
            if (mat != null) Destroy(mat);
        }

        private IEnumerator AnimateBurst(GameObject sphere, Material mat, float maxScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && sphere != null)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;
                float s = Mathf.Lerp(1f, maxScale, p);
                sphere.transform.localScale = Vector3.one * s;
                mat.color = new Color(0.1f, 1f, 0.4f, (1f - p) * 0.7f);
                yield return null;
            }
            if (sphere != null) Destroy(sphere);
            if (mat != null) Destroy(mat);
        }
    }
}

