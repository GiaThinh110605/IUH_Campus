using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;

namespace IUHCampus.Combat
{
    /// <summary>
    /// Timed Cyber Plasma Grenade that bounces physically and detonates after exactly 4 seconds
    /// with multi-layered explosive VFX, shockwave, camera shake, sound, and heavy AoE damage.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class IUHTimedGrenade : MonoBehaviour
    {
        [Header("Grenade Stats")]
        [SerializeField] private float m_FuseTime = 4.0f;
        [SerializeField] private float m_BlastRadius = 8.5f;
        [SerializeField] private int m_Damage = 180;
        [SerializeField] private float m_ExplosionForce = 1400f;

        private Rigidbody m_Rb;
        private SphereCollider m_Collider;
        private float m_TimeRemaining;
        private bool m_HasDetonated = false;

        // Visual components
        private Light m_BeaconLight;
        private GameObject m_TimerCanvasGo;
        private Text m_TimerText;
        private Renderer m_CoreRenderer;
        private Material m_CoreMat;

        // Beep frequency control
        private float m_NextBeepTime = 0f;
        private AudioSource m_AudioSource;

        private void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            m_Collider = GetComponent<SphereCollider>();

            // Setup audio source for procedural high-tech beeping
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 1.0f;
            m_AudioSource.minDistance = 3f;
            m_AudioSource.maxDistance = 30f;
            m_AudioSource.playOnAwake = false;
        }

        public void Initialize(Vector3 throwVelocity, float fuseTime = 4.0f, int damage = 180, float radius = 8.5f)
        {
            m_FuseTime = fuseTime;
            m_TimeRemaining = fuseTime;
            m_Damage = damage;
            m_BlastRadius = radius;

            // Ensure components are acquired
            if (m_Rb == null) m_Rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            if (m_Collider == null) m_Collider = GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>();

            // Physics setup
            m_Rb.mass = 1.2f;
            m_Rb.linearDamping = 0.2f;
            m_Rb.angularDamping = 0.8f;
            m_Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            m_Rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Bouncy Physic Material
            var pMat = new PhysicsMaterial("GrenadePhysicsMat")
            {
                bounciness = 0.55f,
                bounceCombine = PhysicsMaterialCombine.Maximum,
                dynamicFriction = 0.45f,
                staticFriction = 0.55f,
                frictionCombine = PhysicsMaterialCombine.Average
            };
            m_Collider.material = pMat;
            m_Collider.radius = 0.22f;

            // Apply throw impulse + spin
            m_Rb.linearVelocity = throwVelocity;
            m_Rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 14f;

            BuildVisuals();
        }

        private void BuildVisuals()
        {
            // 1. Grenade Mesh & Cyber Neon Core
            var coreGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreGo.name = "Grenade_Core_Mesh";
            coreGo.transform.SetParent(transform, false);
            coreGo.transform.localScale = Vector3.one * 0.44f;
            Destroy(coreGo.GetComponent<Collider>());

            m_CoreRenderer = coreGo.GetComponent<Renderer>();
            m_CoreMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            m_CoreMat.color = new Color(0.12f, 0.16f, 0.22f);
            m_CoreMat.EnableKeyword("_EMISSION");
            m_CoreMat.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.05f) * 2f);
            if (m_CoreMat.HasProperty("_Metallic")) m_CoreMat.SetFloat("_Metallic", 0.9f);
            if (m_CoreMat.HasProperty("_Smoothness")) m_CoreMat.SetFloat("_Smoothness", 0.85f);
            m_CoreRenderer.sharedMaterial = m_CoreMat;

            // Neon Belt Ring around grenade equator
            var ringGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringGo.name = "Grenade_Neon_Ring";
            ringGo.transform.SetParent(transform, false);
            ringGo.transform.localScale = new Vector3(0.48f, 0.04f, 0.48f);
            Destroy(ringGo.GetComponent<Collider>());

            var ringRend = ringGo.GetComponent<Renderer>();
            var ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ringMat.color = new Color(1f, 0.7f, 0.1f);
            ringMat.EnableKeyword("_EMISSION");
            ringMat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.1f) * 4f);
            ringRend.sharedMaterial = ringMat;

            // 2. Beacon PointLight
            var lightGo = new GameObject("Beacon_PointLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = Vector3.up * 0.2f;

            m_BeaconLight = lightGo.AddComponent<Light>();
            m_BeaconLight.type = LightType.Point;
            m_BeaconLight.range = 5f;
            m_BeaconLight.intensity = 3.5f;
            m_BeaconLight.color = new Color(1f, 0.65f, 0.1f);

            // 3. Floating 3D Countdown Display
            m_TimerCanvasGo = new GameObject("Grenade_Countdown_Canvas");
            m_TimerCanvasGo.transform.SetParent(transform, false);
            m_TimerCanvasGo.transform.localPosition = Vector3.up * 0.65f;

            var canvas = m_TimerCanvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = m_TimerCanvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 45);
            m_TimerCanvasGo.transform.localScale = Vector3.one * 0.012f;

            var textGo = new GameObject("Timer_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(m_TimerCanvasGo.transform, false);
            var tRt = textGo.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            m_TimerText = textGo.GetComponent<Text>();
            m_TimerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            m_TimerText.fontSize = 26;
            m_TimerText.fontStyle = FontStyle.Bold;
            m_TimerText.alignment = TextAnchor.MiddleCenter;
            m_TimerText.color = new Color(1f, 0.9f, 0.2f);

            var outline = textGo.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private void Update()
        {
            if (m_HasDetonated) return;

            m_TimeRemaining -= Time.deltaTime;

            // Keep timer facing camera
            if (m_TimerCanvasGo != null && Camera.main != null)
            {
                m_TimerCanvasGo.transform.rotation = Quaternion.LookRotation(m_TimerCanvasGo.transform.position - Camera.main.transform.position);
            }

            // Update countdown text & urgency color
            if (m_TimerText != null)
            {
                float displaySec = Mathf.Max(0f, m_TimeRemaining);
                if (m_TimeRemaining > 1.5f)
                {
                    m_TimerText.text = $"💣 [{displaySec:0.0}s]";
                    m_TimerText.color = new Color(1f, 0.85f, 0.2f);
                }
                else
                {
                    m_TimerText.text = $"⚠️ [{displaySec:0.0}s]!";
                    m_TimerText.color = Color.Lerp(Color.red, Color.yellow, Mathf.PingPong(Time.time * 12f, 1f));
                }
            }

            // Warning beep sound & light pulse
            UpdateBeeping();

            // Detonate at 4.0s (0s remaining)
            if (m_TimeRemaining <= 0f)
            {
                Detonate();
            }
        }

        private void UpdateBeeping()
        {
            if (Time.time >= m_NextBeepTime)
            {
                // Calculate beep interval: starts at 1.0s, accelerates down to 0.12s
                float interval = Mathf.Lerp(0.12f, 1.0f, Mathf.Clamp01(m_TimeRemaining / m_FuseTime));
                m_NextBeepTime = Time.time + interval;

                // Play synth beep audio
                PlayWarningBeep(1.0f + (1f - (m_TimeRemaining / m_FuseTime)) * 0.8f);

                // Flash beacon light & emission
                StartCoroutine(BeaconFlashRoutine(interval * 0.45f));
            }
        }

        private IEnumerator BeaconFlashRoutine(float duration)
        {
            if (m_BeaconLight != null)
            {
                m_BeaconLight.intensity = 8f;
                m_BeaconLight.color = m_TimeRemaining < 1.5f ? Color.red : new Color(1f, 0.65f, 0.1f);
            }
            if (m_CoreMat != null)
            {
                Color emission = (m_TimeRemaining < 1.5f ? Color.red : new Color(1f, 0.5f, 0.05f)) * 6f;
                m_CoreMat.SetColor("_EmissionColor", emission);
            }

            yield return new WaitForSeconds(duration);

            if (m_BeaconLight != null) m_BeaconLight.intensity = 2f;
            if (m_CoreMat != null)
            {
                m_CoreMat.SetColor("_EmissionColor", new Color(0.8f, 0.3f, 0.05f) * 1.5f);
            }
        }

        private void PlayWarningBeep(float pitch)
        {
            if (m_AudioSource == null) return;

            // Generate procedural crisp beep sound
            int sampleRate = 44100;
            float duration = 0.06f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];
            float freq = 880f * pitch;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / totalSamples;
                samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.45f;
            }

            var clip = AudioClip.Create("GrenadeBeep", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            m_AudioSource.PlayOneShot(clip, 0.7f);
        }

        private void Detonate()
        {
            if (m_HasDetonated) return;
            m_HasDetonated = true;

            Vector3 blastPoint = transform.position;

            // 1. Multi-Layered Cinematic Explosion VFX
            SpawnExplosionVFX(blastPoint);

            // 2. Camera Shake
            IUHCameraShake.Shake(0.55f, 0.38f);

            // 3. Audio Burst
            IUHCombatAudio.PlaySkillBurst();
            IUHCombatAudio.PlayBossHit();

            // 4. Area of Effect Damage & Stun
            Collider[] colliders = Physics.OverlapSphere(blastPoint, m_BlastRadius);
            HashSet<IUHRealtimeMonsterTarget> hitMonsters = new HashSet<IUHRealtimeMonsterTarget>();

            foreach (var col in colliders)
            {
                var monster = col.GetComponentInParent<IUHRealtimeMonsterTarget>();
                if (monster != null && !monster.IsDead && hitMonsters.Add(monster))
                {
                    monster.TakeDamage(m_Damage, isCrit: true);
                    monster.ApplyStun(3.5f);

                    // Physical knockback
                    var mRb = monster.GetComponent<Rigidbody>();
                    if (mRb != null)
                    {
                        mRb.AddExplosionForce(m_ExplosionForce, blastPoint, m_BlastRadius, 1.2f, ForceMode.Impulse);
                    }
                }

                // Physics knockback on generic rigidbodies
                var rb = col.attachedRigidbody;
                if (rb != null && rb != m_Rb)
                {
                    rb.AddExplosionForce(m_ExplosionForce, blastPoint, m_BlastRadius, 0.8f, ForceMode.Impulse);
                }
            }

            // Floating Combat Text
            IUHCombatFloatingText.Spawn(blastPoint + Vector3.up * 1.5f, $"OANH TẠC BOM HẸN GIỜ (4S)!\n-{m_Damage} SÁT THƯƠNG DIỆN RỘNG!", new Color(1f, 0.6f, 0.1f), 2.2f);

            // Destroy grenade GameObject
            Destroy(gameObject);
        }

        private void SpawnExplosionVFX(Vector3 point)
        {
            // Flash Light (Illuminates surroundings)
            GameObject flashLightGo = new GameObject("Explosion_Flash_Light");
            flashLightGo.transform.position = point + Vector3.up * 0.8f;
            var fLight = flashLightGo.AddComponent<Light>();
            fLight.type = LightType.Point;
            fLight.range = 25f;
            fLight.intensity = 16f;
            fLight.color = new Color(1f, 0.7f, 0.25f);
            Destroy(flashLightGo, 0.25f);

            // A. Expanding Core Fireball Sphere
            GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireball.name = "Fireball_Core_VFX";
            fireball.transform.position = point + Vector3.up * 0.5f;
            fireball.transform.localScale = Vector3.one * 0.8f;
            Destroy(fireball.GetComponent<Collider>());

            var rend = fireball.GetComponent<Renderer>();
            var fMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            fMat.color = new Color(1f, 0.35f, 0.05f, 0.95f);
            fMat.EnableKeyword("_EMISSION");
            fMat.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.1f) * 8f);
            rend.sharedMaterial = fMat;

            // B. Inner Electric Sunburst
            GameObject innerSun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            innerSun.name = "Fireball_Inner_Sun";
            innerSun.transform.position = point + Vector3.up * 0.5f;
            innerSun.transform.localScale = Vector3.one * 0.5f;
            Destroy(innerSun.GetComponent<Collider>());

            var sRend = innerSun.GetComponent<Renderer>();
            var sMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            sMat.color = Color.white;
            sMat.EnableKeyword("_EMISSION");
            sMat.SetColor("_EmissionColor", Color.white * 10f);
            sRend.sharedMaterial = sMat;

            // C. Ground Shockwave Ring
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.name = "Ground_Shockwave_Ring";
            shockwave.transform.position = point + Vector3.up * 0.1f;
            shockwave.transform.localScale = new Vector3(1f, 0.04f, 1f);
            Destroy(shockwave.GetComponent<Collider>());

            var swRend = shockwave.GetComponent<Renderer>();
            var swMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            swMat.color = new Color(1f, 0.8f, 0.2f, 0.85f);
            swMat.EnableKeyword("_EMISSION");
            swMat.SetColor("_EmissionColor", new Color(1f, 0.75f, 0.2f) * 6f);
            swRend.sharedMaterial = swMat;

            // D. Sparks / Shrapnel Flying Outward
            List<GameObject> sparks = new List<GameObject>();
            int sparkCount = 12;
            for (int i = 0; i < sparkCount; i++)
            {
                var spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = $"Spark_{i}";
                spark.transform.position = point + Vector3.up * 0.4f;
                spark.transform.localScale = Vector3.one * 0.22f;
                Destroy(spark.GetComponent<Collider>());

                var spRend = spark.GetComponent<Renderer>();
                var spMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                spMat.color = new Color(1f, 0.9f, 0.3f);
                spMat.EnableKeyword("_EMISSION");
                spMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.2f) * 10f);
                spRend.sharedMaterial = spMat;

                sparks.Add(spark);
            }

            // Launch Animation Coroutine
            var runner = new GameObject("Explosion_VFX_Animator").AddComponent<EmptyMonoRunner>();
            runner.StartCoroutine(AnimateExplosionVFX(fireball, innerSun, shockwave, sparks, fMat, sMat, swMat, runner.gameObject));
        }

        private static IEnumerator AnimateExplosionVFX(
            GameObject fireball,
            GameObject innerSun,
            GameObject shockwave,
            List<GameObject> sparks,
            Material fMat,
            Material sMat,
            Material swMat,
            GameObject runner)
        {
            float elapsed = 0f;
            float duration = 0.5f;

            // Random velocities for sparks
            Vector3[] sparkVels = new Vector3[sparks.Count];
            for (int i = 0; i < sparks.Count; i++)
            {
                sparkVels[i] = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(14f, 24f);
                if (sparkVels[i].y < 2f) sparkVels[i].y = Mathf.Abs(sparkVels[i].y) + 2f;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Expand fireball
                if (fireball != null)
                {
                    float scale = Mathf.Lerp(0.8f, 9.2f, Mathf.Sin(t * Mathf.PI * 0.5f));
                    fireball.transform.localScale = Vector3.one * scale;

                    float fade = 1f - t;
                    fMat.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.1f) * (8f * fade));
                }

                // Inner electric sun expands fast then fades
                if (innerSun != null)
                {
                    float sunScale = Mathf.Lerp(0.5f, 5.0f, Mathf.Clamp01(t * 2f));
                    innerSun.transform.localScale = Vector3.one * sunScale;
                    float sunFade = Mathf.Clamp01(1f - t * 2.5f);
                    sMat.SetColor("_EmissionColor", Color.white * (10f * sunFade));
                }

                // Expand ground shockwave ring
                if (shockwave != null)
                {
                    float swScale = Mathf.Lerp(1.2f, 15.5f, t);
                    shockwave.transform.localScale = new Vector3(swScale, 0.04f, swScale);
                    float swFade = 1f - t;
                    swMat.SetColor("_EmissionColor", new Color(1f, 0.75f, 0.2f) * (6f * swFade));
                }

                // Move sparks
                for (int i = 0; i < sparks.Count; i++)
                {
                    if (sparks[i] != null)
                    {
                        sparks[i].transform.position += (sparkVels[i] + Vector3.down * 9.8f * elapsed) * Time.deltaTime;
                        sparks[i].transform.localScale = Vector3.one * Mathf.Lerp(0.22f, 0.04f, t);
                    }
                }

                yield return null;
            }

            // Cleanup objects
            if (fireball != null) Destroy(fireball);
            if (innerSun != null) Destroy(innerSun);
            if (shockwave != null) Destroy(shockwave);
            foreach (var s in sparks) if (s != null) Destroy(s);
            if (runner != null) Destroy(runner);
        }
    }

    public class EmptyMonoRunner : MonoBehaviour { }
}
