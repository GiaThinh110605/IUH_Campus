using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Guardian.Match3;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Guardian.AR
{
    public enum CorruptedObjectType
    {
        Backpack,
        EngineeringLaptop,
        LibraryTextbook,
        Random
    }

    /// <summary>
    /// Điều khiển hiện tượng 'Đồ Bị Nhiễm & Quái Vật Thức Tỉnh Trên Mặt Phẳng AR':
    /// - Quản lý 3 loại vật thể sinh viên (Balo, Laptop đồ án, Sách giáo trình).
    /// - Hiệu ứng khói tím u ám (Volumetric Smoke) + tia chớp pixel glitch + hào quang cảnh báo.
    /// - Diễn hoạt quái vật Cyber Demon bước ra từ lốc xoáy khói và đứng vững trên mặt phẳng AR.
    /// - Nhận diện tương tác cảm ứng / Click chuột / phím [E] để kích hoạt màn đấu Match-3 thanh tẩy.
    /// - Trao thưởng Vàng & Dược phẩm nano khi tiêu diệt quái vật thành công.
    /// </summary>
    public class IUHARCorruptedObjectEncounter : MonoBehaviour
    {
        public static IUHARCorruptedObjectEncounter Instance { get; private set; }

        [Header("Object Variety")]
        [SerializeField] private CorruptedObjectType m_ConfiguredType = CorruptedObjectType.Random;
        [SerializeField] private GameObject m_CorruptedBackpack;
        [SerializeField] private GameObject m_CorruptedLaptop;
        [SerializeField] private GameObject m_CorruptedTextbook;

        [Header("Encounter References")]
        [SerializeField] private GameObject m_MonsterObject;
        [SerializeField] private ParticleSystem m_PurpleSmokeParticles;
        [SerializeField] private ParticleSystem m_GlitchPixelParticles;
        [SerializeField] private Light m_PurpleAuraLight;
        [SerializeField] private GameObject m_ReticleObject;
        [SerializeField] private GameObject m_TapPromptCanvas;
        [SerializeField] private Match3CombatManager m_CombatManager;

        [Header("Monster & Rewards Config")]
        [SerializeField] private bool m_AutoPlaceOnStart = false;
        [SerializeField] private float m_AwakenDistance = 8.5f;
        [SerializeField] private float m_InteractionDistance = 4.8f;
        [SerializeField] private string m_MonsterName = "Tiểu Quỷ Dữ Liệu (AR Cyber Demon)";
        [SerializeField] private int m_MonsterHp = 800;
        [SerializeField] private int m_RewardGold = 100;
        [SerializeField] private string m_RewardItem = "Bình Dược Liệu Nano (+50 HP)";

        [Header("State")]
        private bool m_IsPlaced = false;
        private bool m_IsMonsterActive = false;
        private bool m_InCombat = false;
        private bool m_IsPurified = false;
        private GameObject m_ActiveCorruptedObject;
        private Camera m_ARCamera;
        private Transform m_PlayerTransform;
        private Color m_AftershockLightColor;
        private float m_AftershockLightIntensity;
        private float m_ProximityTimer = 0f;
        private const float PROXIMITY_TRIGGER_SECONDS = 5.0f;

        public bool IsPlaced => m_IsPlaced;
        public bool IsMonsterActive => m_IsMonsterActive;
        public bool IsPurified => m_IsPurified;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_ARCamera = Camera.main;

            if (m_PurpleAuraLight != null)
            {
                m_AftershockLightColor = m_PurpleAuraLight.color;
                m_AftershockLightIntensity = m_PurpleAuraLight.intensity;
            }

            InitializeObjectSlots();
        }

        public void ResetForAftershock()
        {
            StopAllCoroutines();
            m_InCombat = false;
            m_ProximityTimer = 0f;
            m_IsPurified = false;
            m_IsMonsterActive = false;
            m_IsPlaced = false;
            if (m_MonsterObject != null)
                m_MonsterObject.SetActive(false);
            if (m_PurpleAuraLight != null)
            {
                m_PurpleAuraLight.color = m_AftershockLightColor;
                m_PurpleAuraLight.intensity = m_AftershockLightIntensity;
            }
            if (m_AutoPlaceOnStart)
                PlaceCorruptedObject(transform.position, transform.rotation);
        }

        private void Start()
        {
            FindReferences();

            if (m_AutoPlaceOnStart && !m_IsPlaced)
            {
                PlaceCorruptedObject(transform.position, transform.rotation);
            }
        }

        private void InitializeObjectSlots()
        {
            if (m_CorruptedBackpack != null) m_CorruptedBackpack.SetActive(false);
            if (m_CorruptedLaptop != null) m_CorruptedLaptop.SetActive(false);
            if (m_CorruptedTextbook != null) m_CorruptedTextbook.SetActive(false);
            if (m_MonsterObject != null) m_MonsterObject.SetActive(false);
            if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(false);
            if (m_PurpleAuraLight != null) m_PurpleAuraLight.enabled = false;
        }

        private void FindReferences()
        {
            if (m_ARCamera == null) m_ARCamera = Camera.main;
            var student = GameObject.Find("Campus Student") ?? GameObject.Find("PlayerArmature") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;

            if (m_CombatManager == null)
            {
                m_CombatManager = Match3CombatManager.Instance ?? FindAnyObjectByType<Match3CombatManager>();
            }
        }

        private void Update()
        {
            // Nhịp thở nhấp nháy của hào quang ánh sáng tím cảnh báo
            if (m_PurpleAuraLight != null && m_PurpleAuraLight.enabled && !m_IsPurified)
            {
                m_PurpleAuraLight.intensity = 3.2f + Mathf.Sin(Time.time * 5.0f) * 1.5f;
            }

            if (!m_IsPlaced || !m_IsMonsterActive || m_InCombat || m_IsPurified) return;

            HandleEngagementInput();
        }

        private void HandleEngagementInput()
        {
            if (m_ARCamera == null) m_ARCamera = Camera.main;
            if (m_PlayerTransform == null) FindReferences();

            float playerDist = GetPlayerDistance();
            if (UpdateProximityTrigger(playerDist)) return;

            if (playerDist <= m_InteractionDistance && IUHInputHelper.IsInteractKeyDown())
            {
                StartCombatEncounter();
                return;
            }

            if (TryGetPointerTap(out Vector2 tapPos) && m_ARCamera != null)
            {
                TryRaycastEncounterTap(tapPos);
            }
        }

        private float GetPlayerDistance()
        {
            if (m_PlayerTransform != null)
                return Vector3.Distance(m_PlayerTransform.position, transform.position);
            if (m_ARCamera != null)
                return Vector3.Distance(m_ARCamera.transform.position, transform.position);
            return 999f;
        }

        private bool UpdateProximityTrigger(float playerDist)
        {
            if (playerDist > m_InteractionDistance)
            {
                m_ProximityTimer = 0f;
                return false;
            }

            m_ProximityTimer += Time.deltaTime;
            if (m_ProximityTimer >= PROXIMITY_TRIGGER_SECONDS)
            {
                m_ProximityTimer = 0f;
                StartCombatEncounter();
                return true;
            }
            return false;
        }

        private static bool TryGetPointerTap(out Vector2 tapPos)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            {
                tapPos = Input.GetTouch(0).position;
                return true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                tapPos = Input.mousePosition;
                return true;
            }
            tapPos = Vector2.zero;
            return false;
        }

        private void TryRaycastEncounterTap(Vector2 tapPos)
        {
            Ray ray = m_ARCamera.ScreenPointToRay(tapPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, 35f)) return;

            bool hitMonster = m_MonsterObject != null && hit.transform.IsChildOf(m_MonsterObject.transform);
            bool hitProp = m_ActiveCorruptedObject != null && hit.transform.IsChildOf(m_ActiveCorruptedObject.transform);
            if (hitMonster || hitProp || hit.transform.IsChildOf(transform))
            {
                StartCombatEncounter();
            }
        }

        /// <summary>
        /// Đặt vật thể nhiễm độc lên mặt phẳng AR được phát hiện và bắt đầu kịch bản thức tỉnh quái vật
        /// </summary>
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
            SelectAndActivateCorruptedObject();
            yield return AnimateObjectPopIn(m_ActiveCorruptedObject);

            if (m_PurpleSmokeParticles != null) m_PurpleSmokeParticles.Play();
            if (m_GlitchPixelParticles != null) m_GlitchPixelParticles.Play();
            if (m_PurpleAuraLight != null) m_PurpleAuraLight.enabled = true;

            if (m_AutoPlaceOnStart) yield return WaitUntilPlayerAwakens();

            IUHCombatAudio.PlayShield();
            IUHCameraShake.Shake(0.18f, 0.25f);
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 0.9f, "⚠️ DỊ TƯỢNG XUẤT HIỆN: DỮ LIỆU BỊ NHIỄM ĐỘC!", new Color(0.85f, 0.2f, 1f), 1.5f);
            yield return new WaitForSeconds(0.7f);

            yield return AnimateMonsterEmerge();

            m_IsMonsterActive = true;
            if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(true);
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.8f, "⚡ CHẠM ĐỂ CHIẾN ĐẤU [MATCH-3 RPG]!", new Color(0.15f, 0.95f, 1f), 1.5f);
        }

        private static IEnumerator AnimateObjectPopIn(GameObject target)
        {
            if (target == null) yield break;
            target.transform.localScale = Vector3.zero;
            const float duration = 0.45f;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                float t = elapsed / duration;
                target.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.25f, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }

        private IEnumerator WaitUntilPlayerAwakens()
        {
            while (true)
            {
                if (m_PlayerTransform == null) FindReferences();
                if (GetPlayerDistance() <= m_AwakenDistance) yield break;
                yield return new WaitForSeconds(0.2f);
            }
        }

        private IEnumerator AnimateMonsterEmerge()
        {
            if (m_MonsterObject != null)
            {
                m_MonsterObject.SetActive(true);
                m_MonsterObject.transform.localPosition = new Vector3(0f, -0.6f, 0.45f);
                m_MonsterObject.transform.localScale = Vector3.zero;
            }

            IUHCombatAudio.PlayBossRoar();
            IUHCameraShake.Shake(0.28f, 0.45f);

            const float spawnDuration = 0.85f;
            for (float time = 0f; time < spawnDuration; time += Time.deltaTime)
            {
                float easeOut = Mathf.Sin((time / spawnDuration) * Mathf.PI * 0.5f);
                if (m_MonsterObject != null)
                {
                    m_MonsterObject.transform.localPosition = Vector3.Lerp(new Vector3(0f, -0.6f, 0.45f), new Vector3(0f, 0f, 0.65f), easeOut);
                    m_MonsterObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.4f, easeOut);
                }
                yield return null;
            }
        }

        private void SelectAndActivateCorruptedObject()
        {
            CorruptedObjectType chosen = m_ConfiguredType;
            if (chosen == CorruptedObjectType.Random)
            {
                int rand = UnityEngine.Random.Range(0, 3);
                chosen = (CorruptedObjectType)rand;
            }

            switch (chosen)
            {
                case CorruptedObjectType.EngineeringLaptop:
                    if (m_CorruptedLaptop != null) m_ActiveCorruptedObject = m_CorruptedLaptop;
                    else m_ActiveCorruptedObject = m_CorruptedBackpack;
                    break;

                case CorruptedObjectType.LibraryTextbook:
                    if (m_CorruptedTextbook != null) m_ActiveCorruptedObject = m_CorruptedTextbook;
                    else m_ActiveCorruptedObject = m_CorruptedBackpack;
                    break;

                case CorruptedObjectType.Backpack:
                default:
                    m_ActiveCorruptedObject = m_CorruptedBackpack;
                    break;
            }

            if (m_ActiveCorruptedObject != null)
            {
                m_ActiveCorruptedObject.SetActive(true);
            }
        }

        public void StartCombatEncounter()
        {
            if (m_InCombat || m_IsPurified) return;
            // Another fight (or this monster's own IUHMonsterEncounter) already owns the board.
            if (Match3CombatManager.Instance != null && Match3CombatManager.Instance.BlocksExplorationInput) return;
            m_InCombat = true;

            if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(false);

            IUHCombatAudio.PlaySword();
            IUHCameraShake.Shake(0.22f, 0.35f);

            if (m_CombatManager == null)
            {
                m_CombatManager = Match3CombatManager.Instance ?? FindAnyObjectByType<Match3CombatManager>();
            }

            if (m_CombatManager != null)
            {
                var encounter = GetComponent<IUHMonsterEncounter>();
                if (encounter != null)
                {
                    m_CombatManager.SetActiveEncounter(encounter);
                }

                // Đăng ký nhận sự kiện khi kết thúc trận đấu Match-3
                m_CombatManager.CombatFinished += OnMatch3CombatFinished;
                m_CombatManager.StartNewCombat();
            }
        }

        private void OnMatch3CombatFinished(IUHMonsterEncounter encounter, bool victory)
        {
            // Only react to this monster's own fight; keep listening otherwise.
            if (encounter != GetComponent<IUHMonsterEncounter>()) return;

            if (m_CombatManager != null)
            {
                m_CombatManager.CombatFinished -= OnMatch3CombatFinished;
            }

            if (victory)
            {
                OnCombatWon();
            }
            else
            {
                // Thất bại: cho phép thử lại
                m_InCombat = false;
                if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(true);
            }
        }

        /// <summary>
        /// Kích hoạt khi người chơi chiến thắng màn đấu Match-3:
        /// Quái vật tan biến thành bụi vàng, khói độc chuyển thành hào quang hồi phục,
        /// và trao thưởng 100 Vàng + Bình Dược Liệu Nano vào kho đồ sinh viên.
        /// </summary>
        public void OnCombatWon()
        {
            m_InCombat = false;
            m_IsMonsterActive = false;
            m_IsPurified = true;

            StartCoroutine(PurificationSequenceRoutine());
        }

        private IEnumerator PurificationSequenceRoutine()
        {
            yield return FadeOutMonsterRoutine();

            if (m_PurpleSmokeParticles != null) m_PurpleSmokeParticles.Stop();
            if (m_GlitchPixelParticles != null) m_GlitchPixelParticles.Stop();

            if (m_PurpleAuraLight != null)
            {
                m_PurpleAuraLight.color = new Color(0.2f, 1f, 0.5f);
                m_PurpleAuraLight.intensity = 5.0f;
            }

            IUHCombatAudio.PlayHeal();
            IUHCombatAudio.PlayCoin();

            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(m_RewardGold);
                IUHGuardianInventory.Instance.AddItem(m_RewardItem);
            }

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.6f, $"✨ ĐÃ THANH TẨY! +{m_RewardGold} VÀNG & {m_RewardItem}!", new Color(0.3f, 1f, 0.5f), 1.8f);
            yield return new WaitForSeconds(3.0f);
            yield return FadeAuraLightRoutine(5f, 0.8f, 1.5f);
        }

        private IEnumerator FadeOutMonsterRoutine()
        {
            if (m_MonsterObject == null) yield break;
            const float fadeDuration = 0.6f;
            Vector3 initScale = m_MonsterObject.transform.localScale;
            for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.deltaTime)
            {
                m_MonsterObject.transform.localScale = Vector3.Lerp(initScale, Vector3.zero, elapsed / fadeDuration);
                m_MonsterObject.transform.position += Vector3.up * 0.8f * Time.deltaTime;
                yield return null;
            }
            m_MonsterObject.SetActive(false);
        }

        private IEnumerator FadeAuraLightRoutine(float fromIntensity, float toIntensity, float duration)
        {
            if (m_PurpleAuraLight == null) yield break;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                m_PurpleAuraLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, t / duration);
                yield return null;
            }
        }
    }
}
