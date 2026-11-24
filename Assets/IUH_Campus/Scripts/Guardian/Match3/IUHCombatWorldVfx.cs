using UnityEngine;
using UnityEngine.Rendering;

namespace IUHCampus.Guardian.Match3
{
    public sealed class IUHCombatWorldVfx : MonoBehaviour
    {
        public const int MonsterHit = 0;
        public const int PlayerHit = 1;
        public const int Skill = 2;
        public const int Defeat = 3;
        public const int PerKindCapacity = 3;
        public const int Capacity = 4 * PerKindCapacity;
        public const float MaximumLifetime = 5f;
        private const float HitScaleFactor = .38f;
        private const float SkillScaleFactor = .65f;
        private const float MinimumScale = .15f;
        private const float MaximumScale = 3f;
        private const float ScaleEpsilon = .000001f;
        private static readonly string[] Paths = { "Guardian/Combat/HitMonster", "Guardian/Combat/HitPlayer", "Guardian/Combat/Skill", "Guardian/Combat/Defeat" };
        private sealed class Slot
        {
            public GameObject Root;
            public ParticleSystem[] Systems;
            public Transform[] Transforms;
            public Vector3 Scale;
            public int Kind;
            public float Started;
        }
        private readonly Slot[] slots = new Slot[Capacity];
        private bool initialized;
        public int InstanceCount { get; private set; }
        public int ActiveCount
        {
            get { int count = 0; foreach (var s in slots) if (s != null && s.Root.activeSelf) count++; return count; }
        }
        public bool EffectsEnabled = true;

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            var holder = new GameObject("InactiveVfxPreparation");
            holder.transform.SetParent(transform, false);
            holder.SetActive(false);
            for (int kind = 0; kind < Paths.Length; kind++)
            {
                var prefab = Resources.Load<GameObject>(Paths[kind]);
                if (prefab == null) continue;
                for (int i = 0; i < PerKindCapacity; i++) CreateSlot(kind * PerKindCapacity + i, kind, prefab, holder.transform);
            }
            Destroy(holder);
        }

        private void CreateSlot(int index, int kind, GameObject prefab, Transform holder)
        {
            var root = Instantiate(prefab, holder);
            root.SetActive(false);
            // CFX helpers can self-destroy, shake the camera and create animated lights.
            // Pool lifetime and rendering are owned solely by this component.
            foreach (var effect in root.GetComponentsInChildren<CartoonFX.CFXR_Effect>(true))
            {
                effect.cameraShake = null;
                effect.animatedLights = new CartoonFX.CFXR_Effect.AnimatedLight[0];
                effect.clearBehavior = CartoonFX.CFXR_Effect.ClearBehavior.None;
            }
            foreach (var helper in root.GetComponentsInChildren<MonoBehaviour>(true)) helper.enabled = false;
            foreach (var light in root.GetComponentsInChildren<Light>(true)) light.enabled = false;
            var systems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in systems) PrepareSystem(ps);
            foreach (var renderer in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            root.transform.SetParent(transform, false);
            slots[index] = new Slot { Root = root, Systems = systems, Transforms = root.GetComponentsInChildren<Transform>(true), Scale = root.transform.localScale, Kind = kind };
            InstanceCount++;
        }

        private static void PrepareSystem(ParticleSystem ps)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.None;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var collision = ps.collision; collision.enabled = false;
            var lights = ps.lights; lights.enabled = false;
            if (!Application.isMobilePlatform) return;
            int mobileParticles = main.maxParticles / 2;
            main.maxParticles = Mathf.Max(1, mobileParticles);
            var emission = ps.emission;
            if (mobileParticles == 0) emission.enabled = false;
            emission.rateOverTimeMultiplier *= 0.5f;
            emission.rateOverDistanceMultiplier *= 0.5f;
            for (int i = 0; i < emission.burstCount; i++)
            {
                var burst = emission.GetBurst(i);
                var count = burst.count;
                if (count.mode == ParticleSystemCurveMode.Constant) count.constant *= 0.5f;
                else if (count.mode == ParticleSystemCurveMode.TwoConstants)
                { count.constantMin *= 0.5f; count.constantMax *= 0.5f; }
                if (count.mode == ParticleSystemCurveMode.Curve || count.mode == ParticleSystemCurveMode.TwoCurves) count.curveMultiplier *= 0.5f;
                burst.count = count;
                emission.SetBurst(i, burst);
            }
        }

        // kind: monster hit=0, player hit=1, skill/combo=2, monster defeat=3.
        public bool Emit(int kind, Vector3 position, float bodyHeight, int layer)
        {
            if (!EffectsEnabled || !isActiveAndEnabled || kind < 0 || kind > Defeat) return false;
            Initialize();
            foreach (var s in slots)
            {
                if (s == null || s.Kind != kind || s.Root.activeSelf) continue;
                foreach (var child in s.Transforms) child.gameObject.layer = layer;
                s.Root.transform.position = position;
                float size = Mathf.Clamp(bodyHeight * (kind >= Skill ? SkillScaleFactor : HitScaleFactor), MinimumScale, MaximumScale);
                Vector3 ownerScale = transform.lossyScale;
                Vector3 inverseOwnerScale = new Vector3(
                    1f / Mathf.Max(Mathf.Abs(ownerScale.x), ScaleEpsilon),
                    1f / Mathf.Max(Mathf.Abs(ownerScale.y), ScaleEpsilon),
                    1f / Mathf.Max(Mathf.Abs(ownerScale.z), ScaleEpsilon));
                s.Root.transform.localScale = Vector3.Scale(s.Scale * size, inverseOwnerScale);
                s.Started = Time.unscaledTime;
                s.Root.SetActive(true);
                foreach (var ps in s.Systems) ps.Play(false);
                return true;
            }
            return false;
        }

        private void Update()
        {
            foreach (var s in slots)
            {
                if (s == null || !s.Root.activeSelf) continue;
                bool alive = false;
                foreach (var ps in s.Systems) if (ps.IsAlive(false)) { alive = true; break; }
                if (!EffectsEnabled || !alive || Time.unscaledTime - s.Started > MaximumLifetime) Release(s);
            }
        }
        private static void Release(Slot s)
        {
            foreach (var ps in s.Systems) ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            s.Root.SetActive(false);
        }
        public void Clear()
        {
            foreach (var s in slots) if (s != null) Release(s);
        }
        private void OnDisable() => Clear();

        // Body bounds of an arena proxy, ignoring beacon/pillar/beam decorations and particle children.
        public static bool TryGetBodyBounds(GameObject proxy, out Bounds bounds, out int layer)
        {
            bounds = default;
            layer = proxy.layer;
            bool found = false;
            foreach (var skin in proxy.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!skin.enabled || !skin.gameObject.activeInHierarchy) continue;
                if (!found) { bounds = skin.bounds; layer = skin.gameObject.layer; found = true; }
                else bounds.Encapsulate(skin.bounds);
            }
            if (found) return true;
            foreach (var mesh in proxy.GetComponentsInChildren<MeshRenderer>())
            {
                if (!mesh.enabled || !mesh.gameObject.activeInHierarchy || IsEffectDecoration(mesh.transform, proxy.transform)) continue;
                if (!found) { bounds = mesh.bounds; layer = mesh.gameObject.layer; found = true; }
                else bounds.Encapsulate(mesh.bounds);
            }
            return found;
        }

        private static bool IsEffectDecoration(Transform node, Transform root)
        {
            while (node != null && node != root)
            {
                string name = node.name;
                if (name.IndexOf("Beacon", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Pillar", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Beam", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || node.GetComponent<ParticleSystem>() != null) return true;
                node = node.parent;
            }
            return false;
        }
    }
}
