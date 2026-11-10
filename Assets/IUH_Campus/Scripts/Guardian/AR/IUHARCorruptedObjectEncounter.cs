using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Guardian.Match3;
using IUHCampus.Minigames.GreenCampus;
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

        public bool IsPlaced => m_IsPlaced;
        public bool IsMonsterActive => m_IsMonsterActive;
        public bool IsPurified => m_IsPurified;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_ARCamera = Camera.main;

            InitializeObjectSlots();
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
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
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

            bool tapRequested = false;
            Vector2 tapPos = Vector2.zero;

            // 1. Cảm ứng chạm màn hình điện thoại (Touch)
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            {
                tapRequested = true;
                tapPos = Input.GetTouch(0).position;
            }
            // 2. Click chuột trái trên máy tính
            else if (Input.GetMouseButtonDown(0))
            {
                tapRequested = true;
                tapPos = Input.mousePosition;
            }

            // 3. Phím tắt [E] hoặc [Space] hoặc nút ảo Interact trên Mobile Controls UI
            bool keyRequested = IUHInputHelper.IsInteractKeyDown();

            if (keyRequested)
            {
                float dist = m_PlayerTransform != null 
                    ? Vector3.Distance(m_PlayerTransform.position, transform.position)
                    : (m_ARCamera != null ? Vector3.Distance(m_ARCamera.transform.position, transform.position) : 0f);

                if (dist <= m_InteractionDistance || dist < 0.1f)
                {
                    StartCombatEncounter();
                    return;
                }
            }

            if (tapRequested && m_ARCamera != null)
            {
                Ray ray = m_ARCamera.ScreenPointToRay(tapPos);
                if (Physics.Raycast(ray, out RaycastHit hit, 35f))
                {
                    if (hit.transform.IsChildOf(m_MonsterObject.transform) ||
                        (m_ActiveCorruptedObject != null && hit.transform.IsChildOf(m_ActiveCorruptedObject.transform)) ||
                        hit.transform.IsChildOf(transform))
                    {
                        StartCombatEncounter();
                    }
                }
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
            // 1. Lựa chọn và kích hoạt loại vật thể nhiễm độc
            SelectAndActivateCorruptedObject();

            // Hiệu ứng phình to vật thể (Pop-in animation)
            if (m_ActiveCorruptedObject != null)
            {
                m_ActiveCorruptedObject.transform.localScale = Vector3.zero;
                float elapsed = 0f;
                while (elapsed < 0.45f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / 0.45f;
                    m_ActiveCorruptedObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.25f, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
            }

            // 2. Bùng nổ khói tím & tia chớp pixel glitch
            if (m_PurpleSmokeParticles != null) m_PurpleSmokeParticles.Play();
            if (m_GlitchPixelParticles != null) m_GlitchPixelParticles.Play();
            if (m_PurpleAuraLight != null) m_PurpleAuraLight.enabled = true;

            if (m_AutoPlaceOnStart)
            {
                // Trong khuôn viên trường 3D: vật thể nằm bốc khói tím cảnh báo,
                // chờ sinh viên tiến lại gần thì quái vật mới thức tỉnh!
                while (true)
                {
                    if (m_PlayerTransform == null) FindReferences();
                    float dist = m_PlayerTransform != null 
                        ? Vector3.Distance(m_PlayerTransform.position, transform.position) 
                        : (m_ARCamera != null ? Vector3.Distance(m_ARCamera.transform.position, transform.position) : 999f);

                    if (dist <= m_AwakenDistance) break;
                    yield return new WaitForSeconds(0.2f);
                }
            }

            IUHCombatAudio.PlayShield();
            IUHCameraShake.Shake(0.18f, 0.25f);
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 0.9f, "⚠️ DỊ TƯỢNG XUẤT HIỆN: DỮ LIỆU BỊ NHIỄM ĐỘC!", new Color(0.85f, 0.2f, 1f), 1.5f);

            yield return new WaitForSeconds(0.7f);

            // 3. Quái vật Cyber Demon bước ra từ tâm lốc xoáy khói tím
            if (m_MonsterObject != null)
            {
                m_MonsterObject.SetActive(true);
                m_MonsterObject.transform.localPosition = new Vector3(0f, -0.6f, 0.45f);
                m_MonsterObject.transform.localScale = Vector3.zero;
            }

            IUHCombatAudio.PlayBossRoar();
            IUHCameraShake.Shake(0.28f, 0.45f);

            float spawnDuration = 0.85f;
            float time = 0f;
            while (time < spawnDuration)
            {
                time += Time.deltaTime;
                float t = time / spawnDuration;
                float easeOut = Mathf.Sin(t * Mathf.PI * 0.5f);

                if (m_MonsterObject != null)
                {
                    // Bước trồi lên mặt đất và mở rộng hình thể
                    m_MonsterObject.transform.localPosition = Vector3.Lerp(new Vector3(0f, -0.6f, 0.45f), new Vector3(0f, 0f, 0.65f), easeOut);
                    m_MonsterObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.4f, easeOut);
                }
                yield return null;
            }

            m_IsMonsterActive = true;
            if (m_TapPromptCanvas != null) m_TapPromptCanvas.SetActive(true);

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.8f, "⚡ CHẠM ĐỂ CHIẾN ĐẤU [MATCH-3 RPG]!", new Color(0.15f, 0.95f, 1f), 1.5f);
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
            // 1. Quái vật tan biến trong bụi sáng vàng kim
            if (m_MonsterObject != null)
            {
                float fadeDuration = 0.6f;
                float elapsed = 0f;
                Vector3 initScale = m_MonsterObject.transform.localScale;

                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / fadeDuration;
                    m_MonsterObject.transform.localScale = Vector3.Lerp(initScale, Vector3.zero, t);
                    m_MonsterObject.transform.position += Vector3.up * 0.8f * Time.deltaTime;
                    yield return null;
                }
                m_MonsterObject.SetActive(false);
            }

            // 2. Chuyển khói tím sang hào quang Thanh Tẩy Xanh Ngọc
            if (m_PurpleSmokeParticles != null) m_PurpleSmokeParticles.Stop();
            if (m_GlitchPixelParticles != null) m_GlitchPixelParticles.Stop();

            if (m_PurpleAuraLight != null)
            {
                m_PurpleAuraLight.color = new Color(0.2f, 1f, 0.5f);
                m_PurpleAuraLight.intensity = 5.0f;
            }

            IUHCombatAudio.PlayHeal();
            IUHCombatAudio.PlayCoin();

            // 3. Trao thưởng vào túi đồ sinh viên IUH
            if (IUHGuardianInventory.Instance != null)
            {
                IUHGuardianInventory.Instance.AddGold(m_RewardGold);
                IUHGuardianInventory.Instance.AddItem(m_RewardItem);
            }

            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 1.6f, $"✨ ĐÃ THANH TẨY! +{m_RewardGold} VÀNG & {m_RewardItem}!", new Color(0.3f, 1f, 0.5f), 1.8f);

            yield return new WaitForSeconds(3.0f);

            // Giảm dần ánh sáng xanh ngọc
            if (m_PurpleAuraLight != null)
            {
                float t = 0f;
                while (t < 1.5f)
                {
                    t += Time.deltaTime;
                    m_PurpleAuraLight.intensity = Mathf.Lerp(5f, 0.8f, t / 1.5f);
                    yield return null;
                }
            }
        }
    }
}