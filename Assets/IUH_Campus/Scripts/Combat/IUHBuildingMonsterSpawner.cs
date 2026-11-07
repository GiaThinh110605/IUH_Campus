using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    public class IUHBuildingMonsterSpawner : MonoBehaviour
    {
        [Header("Building Info")]
        [SerializeField] private string m_BuildingName = "TÒA NHÀ KỸ THUẬT E";
        [SerializeField] private string m_AlertMessage = "Vệ Binh Nhiễm Mã Độc Xuất Hiện!";

        [Header("Spawn Configuration")]
        [SerializeField] private GameObject[] m_MonsterInstancesToActivate;
        [SerializeField] private GameObject m_SkyDroneToActivate;
        [SerializeField] private float m_TriggerRadius = 5.0f;
        [SerializeField] private bool m_HasTriggered = false;

        private Transform m_PlayerTransform;

        private void Start()
        {
            FindPlayer();

            // Deactivate ambush monsters until player enters
            if (!m_HasTriggered)
            {
                if (m_MonsterInstancesToActivate != null)
                {
                    foreach (var m in m_MonsterInstancesToActivate)
                    {
                        if (m != null) m.SetActive(false);
                    }
                }

                if (m_SkyDroneToActivate != null)
                {
                    m_SkyDroneToActivate.SetActive(false);
                }
            }
        }

        private void FindPlayer()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;
            else if (Camera.main != null) m_PlayerTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (m_HasTriggered) return;

            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                          new Vector3(m_PlayerTransform.position.x, 0, m_PlayerTransform.position.z));

            if (dist <= m_TriggerRadius)
            {
                TriggerAmbush();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_HasTriggered) return;
            if (other.name.Contains("Student") || other.CompareTag("Player"))
            {
                TriggerAmbush();
            }
        }

        public void TriggerAmbush()
        {
            if (m_HasTriggered) return;
            m_HasTriggered = true;

            IUHCombatAudio.PlayBossRoar();
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 2.5f, $"⚠️ BÁO ĐỘNG {m_BuildingName}!\n{m_AlertMessage}", new Color(1f, 0.2f, 0.3f), 3.5f);

            // Activate monsters with portal effect
            if (m_MonsterInstancesToActivate != null)
            {
                foreach (var m in m_MonsterInstancesToActivate)
                {
                    if (m != null)
                    {
                        m.SetActive(true);
                        StartCoroutine(SpawnGlitchPortal(m.transform.position));
                    }
                }
            }

            // Activate sky drone predator overhead if assigned
            if (m_SkyDroneToActivate != null)
            {
                m_SkyDroneToActivate.SetActive(true);
                StartCoroutine(SpawnSkyWarpPortal(m_SkyDroneToActivate.transform.position));
            }
        }

        private IEnumerator SpawnSkyWarpPortal(Vector3 pos)
        {
            GameObject warp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            warp.name = "Sky_Warp_Portal_VFX";
            warp.transform.position = pos;
            warp.transform.localScale = Vector3.one * 1.0f;

            var rend = warp.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.2f, 0.1f, 0.9f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.25f, 0.1f) * 6f);
            rend.sharedMaterial = mat;
            Destroy(warp.GetComponent<Collider>());

            float elapsed = 0f;
            float dur = 0.8f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / dur;
                float s = Mathf.Lerp(1f, 8f, p);
                warp.transform.localScale = Vector3.one * s;
                mat.color = new Color(1f, 0.2f, 0.1f, (1f - p) * 0.9f);
                yield return null;
            }

            Destroy(warp);
            Destroy(mat);
        }

        private IEnumerator SpawnGlitchPortal(Vector3 pos)
        {
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            portal.name = "Glitch_Portal_VFX";
            portal.transform.position = pos + Vector3.up * 0.1f;
            portal.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);

            var rend = portal.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.8f, 0.1f, 0.9f, 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.9f, 0.1f, 1f) * 4f);
            rend.sharedMaterial = mat;
            Destroy(portal.GetComponent<Collider>());

            float elapsed = 0f;
            float dur = 0.6f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / dur;
                float s = Mathf.Lerp(0.5f, 3.5f, p);
                portal.transform.localScale = new Vector3(s, 0.02f, s);
                mat.color = new Color(0.8f, 0.1f, 0.9f, (1f - p) * 0.8f);
                yield return null;
            }

            Destroy(portal);
            Destroy(mat);
        }
    }
}

