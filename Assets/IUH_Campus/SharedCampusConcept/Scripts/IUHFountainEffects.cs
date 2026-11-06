using UnityEngine;
using UnityEngine.Rendering;
namespace IUHCampus.World
{
    // Built once per scene instance. No particle collisions, per-frame allocations or external textures.
    public sealed class IUHFountainEffects : MonoBehaviour
    {
        public Material waterParticles, waterRipples;
        [Range(0, 1)] public float waterVolume = .12f;
        private AudioClip waterSound;
        private void Awake()
        {
            if (transform.Find("Animated Fountain FX") != null) return;
            var root = new GameObject("Animated Fountain FX").transform;
            root.SetParent(transform, false);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6;
                var radial = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                var jet = Make(root, "Jet " + i, radial * 4.6f + Vector3.up * .6f, waterParticles, 42, 1.07f, .032f, 1f);
                jet.transform.localRotation = Quaternion.LookRotation(-radial * 2.34f + Vector3.up * 5.24f);
                var main = jet.main; main.startSpeed = 5.74f;
                var renderer = jet.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Stretch; renderer.lengthScale = 1.5f; renderer.velocityScale = .025f;
                var impact = radial * 2.1f + Vector3.up * .58f;
                var splash = Make(root, "Splash " + i, impact, waterParticles, 15, .45f, .035f, .6f);
                splash.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                main = splash.main; main.startSpeed = new ParticleSystem.MinMaxCurve(.65f, 1.3f);
                var shape = splash.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 45; shape.radius = .1f;
                var ripple = Make(root, "Ripple " + i, impact + Vector3.up * .008f, waterRipples, 3, 1.1f, .6f, 0);
                ripple.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                var size = ripple.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .12f, 1, 1.5f));
                jet.Play(); splash.Play(); ripple.Play();
            }
            StartWaterSound(root);
        }
        private void StartWaterSound(Transform root)
        {
            // A quiet, locally generated water bed: no third-party audio or network dependency.
            const int rate = 22050, count = rate * 4;
            var samples = new float[count]; var random = new System.Random(713);
            float fast = 0, slow = 0;
            for (int i = 0; i < count; i++)
            {
                float noise = (float)random.NextDouble() * 2 - 1;
                fast += (noise - fast) * .3f; slow += (noise - slow) * .025f;
                float t = i / (float)rate;
                float envelope = .65f + .2f * Mathf.Sin(t * Mathf.PI * 4) + .1f * Mathf.Sin(t * Mathf.PI * 9);
                float seam = Mathf.Min(1, Mathf.Min(i, count - 1 - i) / (rate * .025f));
                samples[i] = (fast - slow) * envelope * seam;
            }
            waterSound = AudioClip.Create("Fountain water bed", count, 1, rate, false);
            waterSound.SetData(samples, 0);
            var source = root.gameObject.AddComponent<AudioSource>();
            source.clip = waterSound; source.loop = true; source.volume = waterVolume;
            source.spatialBlend = 1; source.minDistance = 3; source.maxDistance = 22;
            source.rolloffMode = AudioRolloffMode.Linear; source.dopplerLevel = 0; source.Play();
        }
        private void OnDestroy() { if (waterSound != null) Destroy(waterSound); }

        private static ParticleSystem Make(Transform parent, string name, Vector3 position, Material material, float rate, float lifetime, float size, float gravity)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.prewarm = true; main.duration = 2;
            main.startLifetime = lifetime; main.startSpeed = 0; main.startSize = size;
            main.startColor = new Color(.7f, .94f, 1f, .65f); main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 80;
            main.cullingMode = ParticleSystemCullingMode.PauseAndCatchup;
            var emission = ps.emission; emission.rateOverTime = rate;
            var shape = ps.shape; shape.enabled = false;
            var color = ps.colorOverLifetime; color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(.8f, .12f), new GradientAlphaKey(.55f, .7f), new GradientAlphaKey(0, 1) });
            color.color = gradient;
            var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            return ps;
        }
    }
}
