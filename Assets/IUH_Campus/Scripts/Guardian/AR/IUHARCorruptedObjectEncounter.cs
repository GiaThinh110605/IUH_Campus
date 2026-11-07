using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian.Match3;
using IUHCampus.Minigames.GreenCampus;

namespace IUHCampus.Guardian.AR
{
    public class IUHARCorruptedObjectEncounter : MonoBehaviour
    {
        public static IUHARCorruptedObjectEncounter Instance { get; private set; }

        [Header("Prefabs & References")]
        [SerializeField] private GameObject m_CorruptedBackpack;
        [SerializeField] private GameObject m_MonsterObject;
        [SerializeField] private ParticleSystem m_PurpleSmokeParticles;
        [SerializeField] private Light m_PurpleAuraLight;
        [SerializeField] private GameObject m_ReticleObject;
        [SerializeField] private GameObject m_TapPromptCanvas;
        [SerializeField] private Match3CombatManager m_CombatManager;

        [Header("Monster Config")]
        [SerializeField] private string m_MonsterName = "Tiểu Quỷ Dữ Liệu (AR)";
        [SerializeField] private int m_MonsterHp = 60;
        [SerializeField] private float m_CombatDuration = 45f;
        [SerializeField] private int m_RewardGold = 100;
        [SerializeField] private string m_RewardItem = "Bình Dược Liệu Nano (+50 HP)";

        [Header("State")]
        private bool m_IsPlaced = false;
        private bool m_IsMonsterActive = false;
        private bool m_InCombat = false;
        private Camera m_ARCamera;

        public bool IsPlaced => m_IsPlaced;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_ARCamera = Camera.main;

            if (m_CorruptedBackpack != null) m_CorruptedBackpack.SetActive(false);
            if (m_MonsterObject != null) m_MonsterObject.SetActive(false);
            if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(false);
            if (m_PurpleAuraLight != null) m_PurpleAuraLight.enabled = false;
        }

        private void Start()
        {
            if (m_ARCamera == null) m_ARCamera = Camera.main;
        }

        private void Update()
        {
            if (!m_IsPlaced) return;

            if (m_IsMonsterActive && !m_InCombat)
            {
                // Touch tap on iPhone or Left Mouse Click on Editor
                bool isTapped = false;
                Vector2 tapPos = Vector2.zero;

                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    isTapped = true;
                    tapPos = Input.GetTouch(0).position;
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    isTapped = true;
                    tapPos = Input.mousePosition;
                }

                if (isTapped && m_ARCamera != null)
                {
                    Ray ray = m_ARCamera.ScreenPointToRay(tapPos);
                    if (Physics.Raycast(ray, out RaycastHit hit, 20f))
                    {
                        if (hit.transform.IsChildOf(m_MonsterObject.transform) ||
                            hit.transform.IsChildOf(m_CorruptedBackpack.transform) ||
                            hit.transform.IsChildOf(transform))
                        {
                            StartCombatEncounter();
                        }
                    }
                }
            }
        }

        public void PlaceCorruptedObject(Vector3 worldPos, Quaternion rotation)
        {
            if (m_IsPlaced) return;
            m_IsPlaced = true;

            transform.position = worldPos;
            transform.rotation = rotation;

            if (m_ReticleObject != null) m_ReticleObject.SetActive(false);

            StartCoroutine(SpawnSequenceRoutine());
        }

        private IEnumerator SpawnSequenceRoutine()
        {
            // 1. Show Corrupted Backpack
            if (m_CorruptedBackpack != null)
            {
                m_CorruptedBackpack.SetActive(true);
                m_CorruptedBackpack.transform.localScale = Vector3.zero;
            }

            // Animate scale up
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.5f;
                if (m_CorruptedBackpack != null)
                {
                    m_CorruptedBackpack.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, t);
                }
                yield return null;
            }

            // 2. Erupt purple smoke & dark aura light
            if (m_PurpleSmokeParticles != null) m_PurpleSmokeParticles.Play();
            if (m_PurpleAuraLight != null) m_PurpleAuraLight.enabled = true;

            IUHCombatAudio.PlayShield();
            IUHCameraShake.Shake(0.15f, 0.25f);
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 0.8f, "⚠️ DỮ LIỆU BỊ NHIỄM ĐỘC!", new Color(0.85f, 0.2f, 1f), 1.2f);

            yield return new WaitForSeconds(1.2f);

            // 3. Monster emerges from purple smoke
            if (m_MonsterObject != null)
            {
                m_MonsterObject.SetActive(true);
                m_MonsterObject.transform.localPosition = new Vector3(0f, -0.5f, 0.5f);
                m_MonsterObject.transform.localScale = Vector3.zero;
            }

            IUHCombatAudio.PlayBossRoar();
            IUHCameraShake.Shake(0.25f, 0.4f);

            elapsed = 0f;
            while (elapsed < 0.8f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.8f;
                if (m_MonsterObject != null)
                {
                    m_MonsterObject.transform.localPosition = Vector3.Lerp(new Vector3(0f, -0.5f, 0.5f), new Vector3(0f, 0f, 0.6f), t);
                    m_MonsterObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.35f, t);
                }
                yield return null;
            }

            m_IsMonsterActive = true;
            if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(true);

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.6f, "⚡ CHẠM ĐỂ CHIẾN ĐẤU!", new Color(0.3f, 0.95f, 1f), 1.3f);
        }

        public void StartCombatEncounter()
        {
            if (m_InCombat) return;
            m_InCombat = true;

            if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(false);

            IUHCombatAudio.PlaySword();
            IUHCameraShake.Shake(0.2f, 0.3f);

            if (m_CombatManager != null)
            {
                m_CombatManager.StartNewCombat();
            }
        }

        public void OnCombatWon()
        {
            m_InCombat = false;
            m_IsMonsterActive = false;

            if (m_MonsterObject != null) m_MonsterObject.SetActive(false);
            if (m_PurpleSmokeParticles != null) m_PurpleSmokeParticles.Stop();
            if (m_PurpleAuraLight != null) m_PurpleAuraLight.color = new Color(0.2f, 1f, 0.4f);

            IUHCombatAudio.PlayCoin();
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.5f, "✨ ĐÃ THANH TẨY THÀNH CÔNG!", new Color(0.3f, 1f, 0.5f), 1.4f);
        }
    }
}

