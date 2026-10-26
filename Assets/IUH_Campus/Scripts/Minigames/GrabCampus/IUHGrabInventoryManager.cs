using System;
using UnityEngine;
using IUHCampus.Core;

namespace IUHCampus.Minigames.GrabCampus
{
    /// <summary>
    /// Inventory & Survival Action Manager for Grab Campus ("Chuyến xe tâm linh").
    /// Manages 7 Survival Items according to the concept design:
    /// 1. Đèn pin [F]: Chiếu sáng, làm chậm ma 60%, tiêu hao pin.
    /// 2. Pin dự phòng [1]: Nạp 100% pin cho đèn pin.
    /// 3. Bùa hộ mệnh [2]: Khiên hào quang vàng kim 8s, chặn đòn ma & đẩy lùi ma.
    /// 4. Pháo sáng [3]: Ném pháo sáng bốc đỏ 8s, làm ma bị mù & hoảng sợ bỏ chạy.
    /// 5. Gậy tự vệ [Q/Chuột trái]: Đánh cận chiến làm choáng ma 4.5s và đẩy lùi 8m.
    /// 6. Bộ sơ cứu [4]: Hồi +35% độ bền kiện hàng / máu tài xế.
    /// 7. Chìa khóa: Mở cổng phụ hoặc rương tiếp tế.
    /// </summary>
    public class IUHGrabInventoryManager : MonoBehaviour
    {
        private static IUHGrabInventoryManager s_Instance;
        public static IUHGrabInventoryManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = UnityEngine.Object.FindAnyObjectByType<IUHGrabInventoryManager>(FindObjectsInactive.Include);
                }
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        [Header("Survival Items Inventory")]
        public bool hasFlashlight = true;
        public float flashlightBattery = 100f; // 0 to 100%
        public bool isFlashlightOn = false;
        public int batteries = 1;
        public int talismans = 1;
        public int flares = 1;
        public int defenseBats = 1;
        public int firstAidKits = 1;
        public int keys = 0;

        [Header("Flashlight Spotlight")]
        [SerializeField] private Light m_FlashlightLight;
        [SerializeField] private float m_FlashlightRange = 35f;
        [SerializeField] private float m_FlashlightSpotAngle = 55f;
        [SerializeField] private float m_BatteryDrainRate = 2.0f; // % per second

        [Header("Cooldowns & Timers")]
        private float m_BatCooldownTimer = 0f;
        private const float BAT_COOLDOWN = 1.2f;
        private float m_TalismanActiveTimer = 0f;
        private GameObject m_TalismanAuraObj;

        private AudioSource m_AudioSource;

        public bool IsTalismanActive => m_TalismanActiveTimer > 0f;

        private void Awake()
        {
            if (s_Instance == null) s_Instance = this;
            else if (s_Instance != this) { Destroy(gameObject); return; }

            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;

            EnsureFlashlightSetup();
            EnsureTalismanAuraSetup();
        }

        private void Start()
        {
            ResetInventoryForNewGame();
        }

        public void ResetInventoryForNewGame()
        {
            hasFlashlight = true;
            flashlightBattery = 100f;
            isFlashlightOn = false;
            batteries = 2;
            talismans = 1;
            flares = 2;
            defenseBats = 1;
            firstAidKits = 1;
            keys = 0;

            m_TalismanActiveTimer = 0f;
            if (m_TalismanAuraObj != null) m_TalismanAuraObj.SetActive(false);
            if (m_FlashlightLight != null) m_FlashlightLight.enabled = false;
        }

        private void EnsureFlashlightSetup()
        {
            if (m_FlashlightLight == null)
            {
                var lightGo = new GameObject("Grab_Flashlight_Spotlight");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 1.1f, 0.4f);
                m_FlashlightLight = lightGo.AddComponent<Light>();
                m_FlashlightLight.type = LightType.Spot;
                m_FlashlightLight.spotAngle = m_FlashlightSpotAngle;
                m_FlashlightLight.range = m_FlashlightRange;
                m_FlashlightLight.intensity = 4.2f;
                m_FlashlightLight.color = new Color(0.95f, 0.98f, 1.0f);
                m_FlashlightLight.shadows = LightShadows.Hard;
                m_FlashlightLight.enabled = false;
            }
        }

        private void EnsureTalismanAuraSetup()
        {
            if (m_TalismanAuraObj == null)
            {
                m_TalismanAuraObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_TalismanAuraObj.name = "TalismanSacredAura";
                m_TalismanAuraObj.transform.SetParent(transform, false);
                m_TalismanAuraObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                m_TalismanAuraObj.transform.localScale = Vector3.one * 5.2f;

                var col = m_TalismanAuraObj.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var r = m_TalismanAuraObj.GetComponent<Renderer>();
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.85f, 0.1f, 0.28f);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.2f) * 2.5f);
                r.material = mat;

                var light = m_TalismanAuraObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.85f, 0.1f);
                light.range = 8.0f;
                light.intensity = 3.5f;

                m_TalismanAuraObj.SetActive(false);
            }
        }

        private void Update()
        {
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr == null || !mgr.IsMissionActive) return;

            UpdateParentFollow();
            HandleHotkeys();
            UpdateFlashlightDrain();
            UpdateTalismanAura();

            if (m_BatCooldownTimer > 0f)
            {
                m_BatCooldownTimer -= Time.deltaTime;
            }
        }

        public Transform GetActiveTargetTransform()
        {
            var mgr = IUHGrabCampusManager.Instance;
            if (mgr != null && mgr.ActiveTargetTransform != null) return mgr.ActiveTargetTransform;
            var bike = FindFirstObjectByType<IUHGrabMotorbike>();
            if (bike != null) return bike.transform;
            return transform;
        }

        private void UpdateParentFollow()
        {
            Transform target = GetActiveTargetTransform();
            if (target == null || target == transform) return;

            // Follow bike/player directly with flashlight and talisman WITHOUT moving or rotating the root GAMEPLAY_GRAB_CAMPUS!
            if (m_FlashlightLight != null)
            {
                m_FlashlightLight.transform.position = target.position + target.forward * 0.8f + Vector3.up * 0.9f;
                m_FlashlightLight.transform.rotation = target.rotation;
            }

            if (m_TalismanAuraObj != null && m_TalismanAuraObj.activeSelf)
            {
                m_TalismanAuraObj.transform.position = target.position + Vector3.up * 0.8f;
            }
        }

        private void HandleHotkeys()
        {
            // Flashlight [F]
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFlashlight();
            }

            // Battery [1]
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                UseBattery();
            }

            // Talisman [2]
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                UseTalisman();
            }

            // Flare [3]
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                UseFlare();
            }

            // Defense Bat [Q] or Left Mouse
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0))
            {
                UseDefenseBat();
            }

            // First Aid Kit [4]
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                UseFirstAidKit();
            }
        }

        public void AddItem(IUHGrabSurvivalItem.ItemType type, int count, string itemName = null)
        {
            switch (type)
            {
                case IUHGrabSurvivalItem.ItemType.Flashlight:
                    hasFlashlight = true;
                    flashlightBattery = 100f;
                    break;
                case IUHGrabSurvivalItem.ItemType.Battery:
                    batteries += count;
                    break;
                case IUHGrabSurvivalItem.ItemType.Talisman:
                    talismans += count;
                    break;
                case IUHGrabSurvivalItem.ItemType.Flare:
                    flares += count;
                    break;
                case IUHGrabSurvivalItem.ItemType.DefenseBat:
                    defenseBats += count;
                    break;
                case IUHGrabSurvivalItem.ItemType.FirstAidKit:
                    firstAidKits += count;
                    break;
                case IUHGrabSurvivalItem.ItemType.Key:
                    keys += count;
                    break;
            }
        }

        public void ToggleFlashlight()
        {
            if (!hasFlashlight)
            {
                IUHGrabCampusManager.Instance?.ShowNotification("❌ Chưa có Đèn Pin! Hãy tìm nhặt trên đường.");
                return;
            }

            if (flashlightBattery <= 0f)
            {
                IUHGrabCampusManager.Instance?.ShowNotification("⚠️ Đèn pin hết pin! Nhấn [1] để dùng Pin dự phòng.");
                if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetFlashlightClickSound(), 0.8f);
                return;
            }

            isFlashlightOn = !isFlashlightOn;
            if (m_FlashlightLight != null) m_FlashlightLight.enabled = isFlashlightOn;

            if (m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetFlashlightClickSound(), 1.0f);
            }

            string status = isFlashlightOn ? "ĐÃ BẬT" : "ĐÃ TẮT";
            IUHGrabCampusManager.Instance?.ShowNotification($"🔦 Đèn pin: {status} ({flashlightBattery:F0}%)");
        }

        private void UpdateFlashlightDrain()
        {
            if (!isFlashlightOn) return;

            flashlightBattery -= m_BatteryDrainRate * Time.deltaTime;
            if (flashlightBattery <= 0f)
            {
                flashlightBattery = 0f;
                isFlashlightOn = false;
                if (m_FlashlightLight != null) m_FlashlightLight.enabled = false;
                IUHGrabCampusManager.Instance?.ShowNotification("⚠️ Đèn pin đã cạn kiệt năng lượng! Hãy nạp pin [1].");
                return;
            }

            Transform target = GetActiveTargetTransform();
            Vector3 targetPos = target.position;
            Vector3 targetFwd = target.forward;

            // Raycast / cone illumination check on active ghosts
            var ghosts = FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
            foreach (var ghost in ghosts)
            {
                if (ghost == null || !ghost.gameObject.activeInHierarchy) continue;

                Vector3 dirToGhost = ghost.transform.position - targetPos;
                float dist = dirToGhost.magnitude;

                if (dist <= m_FlashlightRange)
                {
                    float angle = Vector3.Angle(targetFwd, dirToGhost.normalized);
                    if (angle <= m_FlashlightSpotAngle * 0.5f)
                    {
                        ghost.OnIlluminatedByFlashlight(Time.deltaTime);
                    }
                }
            }
        }

        public void UseBattery()
        {
            if (batteries <= 0)
            {
                IUHGrabCampusManager.Instance?.ShowNotification("❌ Đã hết Pin dự phòng!");
                return;
            }

            batteries--;
            flashlightBattery = 100f;
            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);
            IUHGrabCampusManager.Instance?.ShowNotification("🔋 Đã thay pin mới! Năng lượng đèn pin: 100%");
        }

        public void UseTalisman()
        {
            if (talismans <= 0)
            {
                IUHGrabCampusManager.Instance?.ShowNotification("❌ Đã hết Bùa hộ mệnh!");
                return;
            }

            talismans--;
            m_TalismanActiveTimer = 8.0f;
            if (m_TalismanAuraObj != null) m_TalismanAuraObj.SetActive(true);

            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetTalismanChantSound(), 1.0f);
            IUHGrabCampusManager.Instance?.ShowNotification("✨ BÙA HỘ MỆNH KÍCH HOẠT! Lá chắn tâm linh bảo vệ trong 8 giây!");

            // Ward nearby ghosts
            WardNearbyGhosts(12f);
        }

        private void UpdateTalismanAura()
        {
            if (m_TalismanActiveTimer <= 0f) return;

            m_TalismanActiveTimer -= Time.deltaTime;
            if (m_TalismanAuraObj != null)
            {
                m_TalismanAuraObj.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
                float pulse = 4.8f + Mathf.Sin(Time.time * 8f) * 0.4f;
                m_TalismanAuraObj.transform.localScale = Vector3.one * pulse;
            }

            WardNearbyGhosts(8f);

            if (m_TalismanActiveTimer <= 0f)
            {
                if (m_TalismanAuraObj != null) m_TalismanAuraObj.SetActive(false);
                IUHGrabCampusManager.Instance?.ShowNotification("Lá chắn Bùa hộ mệnh đã tan biến.");
            }
        }

        private void WardNearbyGhosts(float radius)
        {
            Transform target = GetActiveTargetTransform();
            var ghosts = FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
            foreach (var g in ghosts)
            {
                if (g == null || !g.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(target.position, g.transform.position);
                if (d <= radius)
                {
                    g.OnWardedByTalisman(target.position);
                }
            }
        }

        public void UseFlare()
        {
            if (flares <= 0)
            {
                IUHGrabCampusManager.Instance?.ShowNotification("❌ Đã hết Pháo sáng!");
                return;
            }

            flares--;

            Transform target = GetActiveTargetTransform();
            // Spawn flare in front of active target
            Vector3 spawnPos = target.position + target.forward * 2.5f + Vector3.up * 0.5f;
            var flareGo = new GameObject("ActiveRoadsideFlare");
            flareGo.transform.position = spawnPos;

            var light = flareGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.2f, 0.05f); // Crimson red
            light.range = 22.0f;
            light.intensity = 5.5f;

            var audio = flareGo.AddComponent<AudioSource>();
            audio.clip = IUHSoundSynthesizer.GetFlareHissSound();
            audio.loop = true;
            audio.spatialBlend = 0.8f;
            audio.Play();

            // Flare logic helper
            var fl = flareGo.AddComponent<GrabFlareInstance>();
            fl.Init(8.0f);

            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetFlareHissSound(), 1.0f);
            IUHGrabCampusManager.Instance?.ShowNotification("💥 ĐÃ NÉM PHÁO SÁNG! Vùng sáng đỏ xua đuổi ma trong 8 giây!");
        }

        public void UseDefenseBat()
        {
            if (m_BatCooldownTimer > 0f) return;
            m_BatCooldownTimer = BAT_COOLDOWN;

            if (m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetBatSwingSound(), 1.0f);
            }

            Transform target = GetActiveTargetTransform();
            Vector3 targetPos = target.position;
            Vector3 targetFwd = target.forward;

            // Check if any ghost is within melee combat range (8.0m sweep)
            var ghosts = FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
            bool hitAny = false;

            foreach (var g in ghosts)
            {
                if (g == null || !g.gameObject.activeInHierarchy) continue;
                Vector3 toGhost = g.transform.position - targetPos;
                float d = toGhost.magnitude;

                if (d <= 8.0f)
                {
                    float angle = Vector3.Angle(targetFwd, toGhost.normalized);
                    if (angle <= 120f || d <= 4.0f) // Broad combat sweep arc
                    {
                        hitAny = true;
                        g.OnBatHit(targetPos);
                    }
                }
            }

            if (hitAny)
            {
                if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetBatHitSound(), 1.0f);
                IUHGrabCampusManager.Instance?.ApplyGhostDefeatedReward(10000, 10);
                IUHGrabCampusManager.Instance?.ShowNotification("⚔️ ĐÃ ĐÁNH ĐUỔI HỒN MA! (+10,000 VNĐ Thưởng Trừ Tà)");
            }
            else
            {
                IUHGrabCampusManager.Instance?.ShowNotification("🏏 Vung gậy trừ tà tự vệ [Q]!");
            }
        }

        public void UseFirstAidKit()
        {
            if (firstAidKits <= 0)
            {
                IUHGrabCampusManager.Instance?.ShowNotification("❌ Đã hết Bộ sơ cứu!");
                return;
            }

            var mgr = IUHGrabCampusManager.Instance;
            if (mgr == null) return;

            firstAidKits--;
            mgr.ApplyCargoHeal(35);

            if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetHealSound(), 1.0f);
            IUHGrabCampusManager.Instance?.ShowNotification("🩹 Đã dùng Bộ sơ cứu! Hồi phục +35% độ bền kiện hàng!");
        }
    }

    /// <summary>
    /// Temporary flare object burning on the road.
    /// </summary>
    public class GrabFlareInstance : MonoBehaviour
    {
        private float m_LifeTime = 8.0f;
        private Light m_Light;

        public void Init(float lifetime)
        {
            m_LifeTime = lifetime;
            m_Light = GetComponent<Light>();
        }

        private void Update()
        {
            m_LifeTime -= Time.deltaTime;
            if (m_Light != null)
            {
                m_Light.intensity = 4.5f + Mathf.Sin(Time.time * 25f) * 1.5f;
            }

            // Scare nearby ghosts
            var ghosts = UnityEngine.Object.FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
            foreach (var g in ghosts)
            {
                if (g == null || !g.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(transform.position, g.transform.position);
                if (d <= 18.0f)
                {
                    g.OnScaredByFlare(transform.position);
                }
            }

            if (m_LifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
