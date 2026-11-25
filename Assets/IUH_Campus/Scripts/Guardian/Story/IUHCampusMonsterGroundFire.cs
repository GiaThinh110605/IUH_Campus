using UnityEngine;
using UnityEngine.Rendering;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHCampusMonsterGroundFire : MonoBehaviour
    {
        const int DesktopParticleLimit = 24;
        const int MobileParticleLimit = 12;
        const float DesktopEmissionRate = 16f;
        const float MobileEmissionRate = 8f;
        const float ParticleLifetime = 1.2f;
        const float ParticleSize = .50f;
        const float GroundRadius = .48f;
        const float RiseSpeed = .20f;
        const float FadeInFraction = .15f;
        static readonly Color FireColor = new Color(.42f, .30f, .46f, .70f);
        static readonly Vector3 GroundShapeRotation = new Vector3(90f, 0f, 0f);
        ParticleSystem[] systems;

        void Awake()
        {
            systems = GetComponentsInChildren<ParticleSystem>(true);
            ConfigureForPlatform(Application.isMobilePlatform);
            foreach (var light in GetComponentsInChildren<Light>(true)) light.enabled = false;
            foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            AlignToFeet();
        }

        // The rigged monster visual stands ~1 m behind the encounter origin: centre the aura between its feet (XZ only).
        public bool AlignToFeet()
        {
            var owner = transform.parent;
            if (owner == null) return false;
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (var bone in owner.GetComponentsInChildren<Transform>(true))
            {
                if (bone.IsChildOf(transform) || bone.name.IndexOf("foot", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                sum += bone.position;
                count++;
            }
            if (count == 0) return false;
            var feet = sum / count;
            transform.position = new Vector3(feet.x, transform.position.y, feet.z);
            return true;
        }

        public void ConfigureForPlatform(bool mobile)
        {
            if (systems == null) systems = GetComponentsInChildren<ParticleSystem>(true);
            for (int index = 0; index < systems.Length; index++)
            {
                var ps = systems[index];
                if (ps.transform != transform)
                {
                    ps.transform.localPosition = Vector3.zero;
                    ps.transform.localRotation = Quaternion.identity;
                    ps.transform.localScale = Vector3.one;
                }
                int budget = mobile ? MobileParticleLimit : DesktopParticleLimit;
                int capacity = budget / systems.Length + (index < budget % systems.Length ? 1 : 0);
                float rate = (mobile ? MobileEmissionRate : DesktopEmissionRate) * capacity / budget;
                Configure(ps, capacity, rate);
            }
        }

        static void Configure(ParticleSystem ps, int capacity, float rate)
        {
            ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            ConfigureMain(ps, capacity);
            ConfigureEmission(ps, rate);
            ConfigureMotion(ps);
            ConfigureModules(ps);
            ConfigureColor(ps);
            ConfigureRenderer(ps);
        }

        static void ConfigureMain(ParticleSystem ps, int capacity)
        {
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.None;
            main.maxParticles = Mathf.Max(1, capacity);
            main.startLifetime = ParticleLifetime;
            main.startSpeed = 0f;
            main.startSize3D = false;
            main.startSize = ParticleSize;
            main.startRotation3D = false;
            main.startRotation = 0f;
            main.startColor = FireColor;
            main.gravityModifier = 0f;
            main.simulationSpeed = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }

        static void ConfigureEmission(ParticleSystem ps, float rate)
        {
            var emission = ps.emission;
            emission.enabled = rate > 0f;
            emission.SetBursts(new ParticleSystem.Burst[0]);
            emission.rateOverTime = rate;
            emission.rateOverDistance = 0f;
        }

        static void ConfigureMotion(ParticleSystem ps)
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = GroundRadius;
            shape.radiusThickness = 1f;
            shape.position = Vector3.zero;
            shape.rotation = GroundShapeRotation;
            shape.scale = Vector3.one;
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = 0f; velocity.y = RiseSpeed; velocity.z = 0f;
            velocity.orbitalX = 0f; velocity.orbitalY = 0f; velocity.orbitalZ = 0f;
            velocity.radial = 0f; velocity.speedModifier = 1f;
        }

        static void ConfigureModules(ParticleSystem ps)
        {
            var force = ps.forceOverLifetime; force.enabled = false;
            var limit = ps.limitVelocityOverLifetime; limit.enabled = false;
            var noise = ps.noise; noise.enabled = false;
            var collision = ps.collision; collision.enabled = false;
            var trigger = ps.trigger; trigger.enabled = false;
            var sub = ps.subEmitters; sub.enabled = false;
            var lights = ps.lights; lights.enabled = false;
            var trails = ps.trails; trails.enabled = false;
            var size = ps.sizeOverLifetime; size.enabled = false;
            var sizeSpeed = ps.sizeBySpeed; sizeSpeed.enabled = false;
            var rotation = ps.rotationOverLifetime; rotation.enabled = false;
            var rotationSpeed = ps.rotationBySpeed; rotationSpeed.enabled = false;
            var colorSpeed = ps.colorBySpeed; colorSpeed.enabled = false;
        }

        static void ConfigureColor(ParticleSystem ps)
        {
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, FadeInFraction), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
        }

        static void ConfigureRenderer(ParticleSystem ps)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        void OnEnable()
        {
            if (systems == null) return;
            foreach (var ps in systems) if (ps != null) ps.Play(false);
        }

        void OnDisable()
        {
            if (systems == null) return;
            foreach (var ps in systems)
                if (ps != null) ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}


