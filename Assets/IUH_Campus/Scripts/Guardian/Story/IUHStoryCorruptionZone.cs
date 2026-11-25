using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryCorruptionZone : MonoBehaviour
    {
        const int PcParticleBudget = 140;
        const int MobileParticleBudget = 50;
        const int PcClusters = 12;
        const int MobileClusters = 6;
        const int NearMonsterClusters = 3;
        const float Padding = 6f;
        const float PurifySeconds = 2f;
        const float GoldenAngle = 2.39996323f;
        const float Coverage = .75f;
        const string SmokeResource = "Guardian/VFX/CorruptionSmoke";
        static readonly Color SmokeTint = new Color(.30f, .21f, .40f, .55f);
        readonly List<ParticleSystem> systems = new List<ParticleSystem>();
        readonly List<Dictionary<uint, byte>> initialAlpha = new List<Dictionary<uint, byte>>();
        readonly ParticleSystem.Particle[] particles = new ParticleSystem.Particle[PcParticleBudget];
        bool purifying;
        public float Radius { get; private set; }

        public static IUHStoryCorruptionZone Create(IReadOnlyList<Vector3> positions, Material fog)
        {
            int count = positions == null ? 0 : positions.Count;
            Vector3 center = Vector3.zero;
            for (int i = 0; i < count; i++) center += positions[i];
            if (count > 0) center /= count;
            float radius = 0;
            for (int i = 0; i < count; i++) radius = Mathf.Max(radius, Vector3.Distance(center, positions[i]));
            var root = new GameObject("Story Corruption Zone");
            root.transform.position = center;
            var zone = root.AddComponent<IUHStoryCorruptionZone>();
            zone.Radius = radius + Padding;
            zone.BuildSmoke(positions, count);
            return zone;
        }

        void BuildSmoke(IReadOnlyList<Vector3> positions, int count)
        {
            var prefab = Resources.Load<GameObject>(SmokeResource);
            if (prefab == null) return;
            int fieldCount = Application.isMobilePlatform ? MobileClusters : PcClusters;
            int instances = fieldCount + Mathf.Min(count, NearMonsterClusters);
            int budget = Application.isMobilePlatform ? MobileParticleBudget : PcParticleBudget;
            int systemCount = prefab.GetComponentsInChildren<ParticleSystem>(true).Length;
            if (systemCount == 0 || systemCount * instances > budget) return;
            int slot = 0;
            for (int i = 0; i < instances; i++)
            {
                float angle = i * GoldenAngle;
                float distance = Radius * Coverage * Mathf.Sqrt((i + .5f) / fieldCount);
                Vector3 position = i < fieldCount
                    ? transform.position + new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance)
                    : positions[i - fieldCount];
                var instance = Instantiate(prefab, position, prefab.transform.rotation, transform);
                foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>(true))
                {
                    int allowance = budget / (systemCount * instances) + (slot++ < budget % (systemCount * instances) ? 1 : 0);
                    ConfigureBudget(ps, allowance);
                    systems.Add(ps);
                    ps.Play(false);
                }
            }
        }

        static void ConfigureBudget(ParticleSystem ps, int allowance)
        {
            ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            float ratio = Mathf.Min(1f, (float)allowance / Mathf.Max(1, main.maxParticles));
            main.maxParticles = allowance;
            main.startColor = SmokeTint;
            var emission = ps.emission;
            emission.rateOverTimeMultiplier *= ratio;
            emission.rateOverDistanceMultiplier *= ratio;
            for (int i = 0; i < emission.burstCount; i++)
            {
                var burst = emission.GetBurst(i);
                var curve = burst.count;
                curve.curveMultiplier *= Mathf.Min(1f, allowance / Mathf.Max(1f, curve.constantMax));
                burst.count = curve;
                emission.SetBurst(i, burst);
            }
            var collision = ps.collision;
            collision.enabled = false;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        void Update()
        {
            if (purifying) return;
            foreach (var ps in systems)
                if (ps != null && !ps.IsAlive(false)) ps.Play(false);
        }

        public void Purify()
        {
            if (purifying) return;
            purifying = true;
            IUHCombatFloatingText.Spawn(transform.position + Vector3.up * 3,
                "KHU VỰC ĐÃ ĐƯỢC THANH TẨY", Color.cyan, 1.4f);
            foreach (var ps in systems)
            {
                var alphas = new Dictionary<uint, byte>();
                if (ps != null)
                {
                    ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                    int count = ps.GetParticles(particles);
                    for (int i = 0; i < count; i++) alphas[particles[i].randomSeed] = particles[i].startColor.a;
                }
                initialAlpha.Add(alphas);
            }
            StartCoroutine(Fade());
        }

        IEnumerator Fade()
        {
            double start = Time.realtimeSinceStartupAsDouble;
            while (Time.realtimeSinceStartupAsDouble - start < PurifySeconds)
            {
                float opacity = Mathf.Clamp01(1f - (float)(Time.realtimeSinceStartupAsDouble - start) / PurifySeconds);
                for (int s = 0; s < systems.Count; s++)
                {
                    var ps = systems[s];
                    if (ps == null) continue;
                    int count = ps.GetParticles(particles);
                    for (int i = 0; i < count; i++)
                    {
                        var color = particles[i].startColor;
                        if (initialAlpha[s].TryGetValue(particles[i].randomSeed, out byte alpha))
                            color.a = (byte)Mathf.RoundToInt(alpha * opacity);
                        particles[i].startColor = color;
                    }
                    ps.SetParticles(particles, count);
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
