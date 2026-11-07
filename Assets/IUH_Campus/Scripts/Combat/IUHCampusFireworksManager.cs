using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IUHCampus.Guardian;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Combat
{
    /// <summary>
    /// Hệ Thống Pháo Hoa Lễ Hội & Chiến Thắng IUH (IUH Campus Fireworks Celebration System):
    /// - Cho phép bắn pháo hoa thủ công bằng phím [K] (từ bệ giỏ khinh khí cầu hoặc khi bay/trên mặt đất).
    /// - Tự động kích hoạt đại tiệc pháo hoa ăn mừng (Grand Victory Celebration) khi tiêu diệt hết quái thú trên không.
    /// - Hiệu ứng đa tầng: Vệt đạn xé toạc màn đêm (Rocket Trail), quầng nổ đa sắc (Crimson, Gold, Cyan, Purple),
    ///   mưa sao băng lấp lánh (Sparkling Starlets), và ánh sáng phản chiếu (Volumetric Point Light) lên khuôn viên IUH.
    /// </summary>
    public class IUHCampusFireworksManager : MonoBehaviour
    {
        public static IUHCampusFireworksManager Instance { get; private set; }

        [Header("Celebration Palette")]
        public static readonly Color COLOR_CRIMSON = new Color(0.98f, 0.12f, 0.22f, 1f); // Đỏ thắm IUH
        public static readonly Color COLOR_GOLD    = new Color(1.00f, 0.85f, 0.15f, 1f); // Vàng ánh kim
        public static readonly Color COLOR_CYAN    = new Color(0.08f, 0.95f, 1.00f, 1f); // Xanh điện quang
        public static readonly Color COLOR_PURPLE  = new Color(0.85f, 0.20f, 1.00f, 1f); // Tím dạ quang
        public static readonly Color COLOR_WHITE   = new Color(1.00f, 1.00f, 1.00f, 1f);

        [Header("Settings")]
        [SerializeField] private float m_ManualCooldown = 0.35f;
        [SerializeField] private float m_ApexHeightMin = 36f;
        [SerializeField] private float m_ApexHeightMax = 54f;

        private Transform m_PlayerTransform;
        private Transform m_BalloonTransform;
        private float m_NextFireTime = 0f;
        private bool m_VictoryTriggered = false;
        private Material m_ParticleSharedMat;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            CreateSharedMaterial();
        }

        private void Start()
        {
            FindReferences();
        }

        private void CreateSharedMaterial()
        {
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                                 ?? Shader.Find("Particles/Standard Unlit")
                                 ?? Shader.Find("Mobile/Particles/Additive")
                                 ?? Shader.Find("Standard");

            m_ParticleSharedMat = new Material(particleShader);
            m_ParticleSharedMat.name = "M_IUH_FireworkParticle_Mat";
            if (m_ParticleSharedMat.HasProperty("_Surface")) m_ParticleSharedMat.SetFloat("_Surface", 1f); // Transparent
            if (m_ParticleSharedMat.HasProperty("_Blend")) m_ParticleSharedMat.SetFloat("_Blend", 1f); // Additive
        }

        private void FindReferences()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student != null) m_PlayerTransform = student.transform;

            var balloon = GameObject.Find("IUH_HOT_AIR_BALLOON");
            if (balloon != null) m_BalloonTransform = balloon.transform;
        }

        private void Update()
        {
            if (m_PlayerTransform == null || m_BalloonTransform == null)
            {
                FindReferences();
            }

            HandleInput();
            CheckAerialPredatorsDefeated();
        }

        private void HandleInput()
        {
            bool fireRequested = false;

            // Bấm phím [K] hoặc [B] để bắn pháo hoa
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.kKey.wasPressedThisFrame || Keyboard.current.bKey.wasPressedThisFrame)
                {
                    fireRequested = true;
                }
            }
#endif
            try
            {
                if (!fireRequested && (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.B)))
                {
                    fireRequested = true;
                }
            }
            catch {}

            if (fireRequested && Time.time >= m_NextFireTime)
            {
                m_NextFireTime = Time.time + m_ManualCooldown;
                TriggerPlayerFirework();
            }
        }

        /// <summary>
        /// Kỹ năng bắn pháo hoa từ người chơi: Tự động phát hiện vị trí (khinh khí cầu, trên không hoặc mặt đất)
        /// và bắn chùm pháo hoa ngẫu nhiên phối màu rực rỡ lên bầu trời.
        /// </summary>
        public void TriggerPlayerFirework()
        {
            Vector3 origin;
            bool onBalloon = IUHHotAirBalloonController.Instance != null && IUHHotAirBalloonController.Instance.IsPlayerOnBoard;

            if (onBalloon && m_BalloonTransform != null)
            {
                // Bắn từ bệ giỏ khinh khí cầu ở độ cao 36m
                origin = m_BalloonTransform.position + Vector3.up * 1.5f + UnityEngine.Random.insideUnitSphere * 1.2f;
                origin.y = Mathf.Max(origin.y, m_BalloonTransform.position.y + 1.2f);
            }
            else if (m_PlayerTransform != null)
            {
                // Bắn từ vị trí nhân vật
                origin = m_PlayerTransform.position + Vector3.up * 1.8f;
            }
            else
            {
                origin = new Vector3(6f, 1f, -10f);
            }

            // Chọn màu ngẫu nhiên từ bộ 4 màu đặc trưng
            Color primary, secondary;
            PickRandomPalette(out primary, out secondary);

            // Điểm nổ cách vị trí bắn từ 18m đến 28m lên trên
            float targetHeight = origin.y + UnityEngine.Random.Range(20f, 32f);
            targetHeight = Mathf.Clamp(targetHeight, m_ApexHeightMin, m_ApexHeightMax + 10f);

            Vector3 apex = origin + new Vector3(
                UnityEngine.Random.Range(-12f, 12f),
                targetHeight - origin.y,
                UnityEngine.Random.Range(-12f, 12f)
            );

            StartCoroutine(FireworkSequenceRoutine(origin, apex, primary, secondary));
        }

        /// <summary>
        /// Tự động kiểm tra quái vật bay trên bầu trời, khi tiêu diệt hết sẽ kích hoạt đại tiệc pháo hoa mừng chiến thắng!
        /// </summary>
        private void CheckAerialPredatorsDefeated()
        {
            if (m_VictoryTriggered) return;

            var d1 = GameObject.Find("SKY_DRONE_PREDATOR_1");
            var d2 = GameObject.Find("SKY_DRONE_PREDATOR_2");
            var d3 = GameObject.Find("SKY_DRONE_PREDATOR_3");

            bool d1Dead = (d1 == null || !d1.activeInHierarchy);
            bool d2Dead = (d2 == null || !d2.activeInHierarchy);
            bool d3Dead = (d3 == null || !d3.activeInHierarchy);

            if (d1Dead && d2Dead && d3Dead)
            {
                m_VictoryTriggered = true;
                StartCoroutine(VictoryCelebrationRoutine());
            }
        }

        /// <summary>
        /// Kích hoạt đại lễ mừng chiến thắng: Phóng 18 quả pháo hoa liên hoàn từ nhiều bệ phóng quanh trường và khinh khí cầu.
        /// </summary>
        public void TriggerVictoryCelebration()
        {
            StartCoroutine(VictoryCelebrationRoutine());
        }

        private IEnumerator VictoryCelebrationRoutine()
        {
            Debug.Log("<color=#FFD54F>★ [IUH FIREWORKS] TOÀN BỘ QUÁI VẬT TRÊN KHÔNG ĐÃ BỊ TIÊU DIỆT! BẮT ĐẦU ĐẠI LỄ PHÁO HOA ĂN MỪNG CHIẾN THẮNG! ★</color>");

            Vector3[] launchBases = new Vector3[]
            {
                new Vector3(6.0f, 1f, -10.0f),  // Khinh khí cầu trung tâm
                new Vector3(-15.0f, 0.5f, -18.0f), // Góc trái sân trường
                new Vector3(25.0f, 0.5f, -18.0f),  // Góc phải sân trường
                new Vector3(6.0f, 0.5f, -28.0f),   // Cổng trường / Đài phun nước
                new Vector3(-6.0f, 0.5f, 5.0f)     // Trước sảnh tòa nhà E
            };

            int totalFireworks = 18;
            for (int i = 0; i < totalFireworks; i++)
            {
                Vector3 basePos = launchBases[i % launchBases.Length] + UnityEngine.Random.insideUnitSphere * 2.5f;
                basePos.y = Mathf.Max(0.5f, basePos.y);

                float apexY = UnityEngine.Random.Range(m_ApexHeightMin, m_ApexHeightMax);
                Vector3 apex = new Vector3(
                    basePos.x + UnityEngine.Random.Range(-15f, 15f),
                    apexY,
                    basePos.z + UnityEngine.Random.Range(-15f, 15f)
                );

                Color p, s;
                PickRandomPalette(out p, out s);

                StartCoroutine(FireworkSequenceRoutine(basePos, apex, p, s));

                // Khoảng cách nhịp nổ giữa các phát bắn
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.28f, 0.48f));
            }
        }

        private void PickRandomPalette(out Color primary, out Color secondary)
        {
            int r = UnityEngine.Random.Range(0, 4);
            switch (r)
            {
                case 0:
                    primary = COLOR_CRIMSON;
                    secondary = COLOR_GOLD;
                    break;
                case 1:
                    primary = COLOR_GOLD;
                    secondary = COLOR_CYAN;
                    break;
                case 2:
                    primary = COLOR_CYAN;
                    secondary = COLOR_PURPLE;
                    break;
                default:
                    primary = COLOR_PURPLE;
                    secondary = COLOR_CRIMSON;
                    break;
            }
        }

        /// <summary>
        /// Chuỗi diễn hoạt pháo hoa:
        /// 1. Rocket phóng vút với âm thanh rít và vệt đuôi sáng (Trail)
        /// 2. Kích nổ tại đỉnh: Bụi hoa lửa bung nở, chớp sáng môi trường (Flash Light) và tiếng nổ vang vọng
        /// </summary>
        private IEnumerator FireworkSequenceRoutine(Vector3 start, Vector3 apex, Color primaryColor, Color secondaryColor)
        {
            // Âm thanh vút phóng
            IUHCombatAudio.PlayFireworkWhistle();

            // 1. Tạo rocket bay vút
            GameObject rocket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rocket.name = "Firework_Rocket";
            rocket.transform.position = start;
            rocket.transform.localScale = Vector3.one * 0.35f;

            if (rocket.GetComponent<Collider>() != null) Destroy(rocket.GetComponent<Collider>());

            // Material phát sáng
            var rend = rocket.GetComponent<Renderer>();
            Material rocketMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Standard"));
            rocketMat.color = primaryColor;
            if (rocketMat.HasProperty("_EmissionColor"))
            {
                rocketMat.EnableKeyword("_EMISSION");
                rocketMat.SetColor("_EmissionColor", primaryColor * 4.0f);
            }
            rend.sharedMaterial = rocketMat;

            // Trail vệt đuôi lấp lánh
            var trail = rocket.AddComponent<TrailRenderer>();
            trail.time = 0.35f;
            trail.startWidth = 0.45f;
            trail.endWidth = 0.05f;
            trail.sharedMaterial = rocketMat;
            trail.colorGradient = MakeGradient(primaryColor, secondaryColor);

            // Bay lên đỉnh quỹ đạo
            float flightDuration = UnityEngine.Random.Range(0.48f, 0.65f);
            float elapsed = 0f;

            while (elapsed < flightDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flightDuration;

                // Đường bay gia tốc mượt mà có chút uốn lượn tự nhiên
                float easeT = Mathf.Sin(t * Mathf.PI * 0.5f);
                Vector3 currentPos = Vector3.Lerp(start, apex, easeT);
                currentPos.x += Mathf.Sin(t * 10f) * 0.25f;

                if (rocket != null) rocket.transform.position = currentPos;
                yield return null;
            }

            Vector3 burstPos = rocket != null ? rocket.transform.position : apex;
            if (rocket != null) Destroy(rocket);

            // 2. Kích nổ tại đỉnh (Burst Explosion)
            SpawnBurstExplosion(burstPos, primaryColor, secondaryColor);
        }

        private void SpawnBurstExplosion(Vector3 pos, Color pColor, Color sColor)
        {
            // Âm thanh nổ rền vang
            IUHCombatAudio.PlayFireworkBurst();

            GameObject burstRoot = new GameObject("Firework_Burst_FX");
            burstRoot.transform.position = pos;

            // A. Quầng sáng Flash chớp nháy (Central Flash)
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "Burst_Flash";
            flash.transform.SetParent(burstRoot.transform, false);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localScale = Vector3.one * 1.5f;
            if (flash.GetComponent<Collider>() != null) Destroy(flash.GetComponent<Collider>());

            var fRend = flash.GetComponent<Renderer>();
            var fMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Standard"));
            fMat.color = Color.white;
            if (fMat.HasProperty("_EmissionColor"))
            {
                fMat.EnableKeyword("_EMISSION");
                fMat.SetColor("_EmissionColor", Color.white * 6.0f);
            }
            fRend.sharedMaterial = fMat;

            // B. Đèn chiếu sáng môi trường thời gian thực (Volumetric Flash Light)
            var lightGo = new GameObject("Burst_PointLight");
            lightGo.transform.SetParent(burstRoot.transform, false);
            lightGo.transform.localPosition = Vector3.zero;

            var pointLight = lightGo.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = pColor;
            pointLight.range = 55f;
            pointLight.intensity = 6.5f;

            // C. Hệ thống hạt pháo hoa bung xòe hình cầu (Particle Explosion)
            var psGo = new GameObject("Burst_Particles");
            psGo.transform.SetParent(burstRoot.transform, false);
            psGo.transform.localPosition = Vector3.zero;

            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = UnityEngine.Random.Range(1.4f, 1.9f);
            main.startSpeed = UnityEngine.Random.Range(12f, 18f);
            main.startSize = UnityEngine.Random.Range(0.25f, 0.45f);
            main.startColor = new ParticleSystem.MinMaxGradient(pColor, sColor);
            main.gravityModifier = 0.35f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)UnityEngine.Random.Range(65, 95)) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = MakeParticleFadeGradient(pColor, sColor);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(0.7f, 0.8f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var psr = psGo.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = m_ParticleSharedMat != null ? m_ParticleSharedMat : fMat;

            ps.Play();

            // Coroutine làm mờ đèn và quầng sáng
            StartCoroutine(FadeOutBurstRoutine(burstRoot, flash, pointLight, 1.8f));
        }

        private IEnumerator FadeOutBurstRoutine(GameObject root, GameObject flash, Light light, float duration)
        {
            float elapsed = 0f;
            Vector3 initScale = flash != null ? flash.transform.localScale : Vector3.one;
            float initIntensity = light != null ? light.intensity : 5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (flash != null)
                {
                    // Flash nở nhanh trong 0.15s đầu rồi teo nhỏ biến mất
                    if (t < 0.2f)
                    {
                        flash.transform.localScale = Vector3.Lerp(initScale, initScale * 2.5f, t / 0.2f);
                    }
                    else
                    {
                        flash.transform.localScale = Vector3.Lerp(initScale * 2.5f, Vector3.zero, (t - 0.2f) / 0.8f);
                    }
                }

                if (light != null)
                {
                    light.intensity = Mathf.Lerp(initIntensity, 0f, t * t);
                }

                yield return null;
            }

            if (root != null) Destroy(root);
        }

        private Gradient MakeGradient(Color a, Color b)
        {
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            return grad;
        }

        private Gradient MakeParticleFadeGradient(Color p, Color s)
        {
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(p, 0f), new GradientColorKey(s, 0.6f), new GradientColorKey(COLOR_GOLD, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            return grad;
        }
    }
}
