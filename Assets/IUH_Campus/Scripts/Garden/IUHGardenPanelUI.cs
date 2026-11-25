using System.Collections.Generic;
using IUHCampus.Core;
using IUHCampus.Guardian;
using IUHCampus.Guardian.Match3;
using IUHCampus.Guardian.Story;
using IUHCampus.World;
using UnityEngine;

namespace IUHCampus.Garden
{
    // "[E] Tưới hoa" next to a campus flowerbed: waters it, pays gold, shows a water splash.
    // (Class name kept so the existing component on GARDEN stays bound.)
    [DefaultExecutionOrder(-100)]
    public sealed class IUHGardenPanelUI : MonoBehaviour
    {
        public const float InteractDistance = 1.8f;
        static readonly Color GoldColor = new Color(1f, .84f, .2f);
        IUHGardenManager manager;
        IUHStoryDialogueUI story;
        Transform player;
        TextMesh label;
        ParticleSystem splash;
        float refreshAt;

        public int NearestSpot { get; private set; } = -1;

        void Start()
        {
            manager = GetComponent<IUHGardenManager>();
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            player = student != null ? student.transform : null;
            story = FindFirstObjectByType<IUHStoryDialogueUI>();
            label = BuildLabel(transform);
            splash = BuildSplash(transform);
        }

        public static int FindNearestBed(Vector3 playerPos, IReadOnlyList<Bounds> beds, float maxDist)
        {
            float best = maxDist * maxDist;
            int index = -1;
            for (int i = 0; i < beds.Count; i++)
            {
                var flat = beds[i];
                flat.center = new Vector3(flat.center.x, playerPos.y, flat.center.z);
                flat.size = new Vector3(flat.size.x, 0f, flat.size.z);
                var distance = flat.SqrDistance(playerPos);
                if (distance > best || (index >= 0 && distance == best)) continue;
                index = i;
                best = distance;
            }
            return index;
        }

        public bool CanOpenNow()
        {
            var combat = Match3CombatManager.Instance;
            var shop = IUHUpgradeShopUI.Instance;
            return Time.timeScale > 0 && (combat == null || !combat.BlocksExplorationInput) &&
                (story == null || !story.IsBusy) && (shop == null || !shop.IsOpen) &&
                !IUHConceptPreviewController.IsAnyUIOpen();
        }

        void Update()
        {
            if (player == null || manager == null || label == null) return;
            int candidate = FindNearestBed(player.position, manager.Areas, InteractDistance);
            bool allowed = candidate >= 0 && CanOpenNow();
            if (allowed && IUHInputHelper.IsGardenInteractKeyDown()) Water(candidate);
            if (candidate == NearestSpot && Time.unscaledTime < refreshAt) { FaceCamera(); return; }
            NearestSpot = candidate;
            refreshAt = Time.unscaledTime + .2f;
            label.gameObject.SetActive(allowed);
            if (!allowed) return;
            int wait = manager.GetSpot(candidate).SecondsUntilReady(manager.Clock());
            label.text = wait == 0 ? "[E] Tưới hoa" : $"Tưới lại sau {wait / 60}:{wait % 60:00}";
            label.transform.position = EdgePoint(manager.GetArea(candidate), player.position) + Vector3.up * 1.4f;
            FaceCamera();
        }

        public bool Water(int index)
        {
            if (!manager.TryWater(index, out int gold)) return false;
            var point = EdgePoint(manager.GetArea(index), player != null ? player.position : manager.GetArea(index).center);
            if (splash != null)
            {
                splash.transform.position = point + Vector3.up * .2f;
                splash.Play(true);
            }
            IUHCombatFloatingText.Spawn(point + Vector3.up * 1.8f, $"+{gold} Vàng", GoldColor);
            refreshAt = 0f;
            return true;
        }

        static Vector3 EdgePoint(Bounds area, Vector3 from)
        {
            var p = area.ClosestPoint(new Vector3(from.x, area.max.y, from.z));
            p.y = area.max.y;
            return p;
        }

        void FaceCamera()
        {
            var cam = Camera.main;
            if (cam != null && label.gameObject.activeSelf)
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - cam.transform.position);
        }

        static TextMesh BuildLabel(Transform parent)
        {
            var go = new GameObject("Water Prompt");
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = font;
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            text.fontSize = 64;
            text.characterSize = .045f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            go.SetActive(false);
            return text;
        }

        static ParticleSystem BuildSplash(Transform parent)
        {
            var go = new GameObject("Water Splash");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = .6f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(.5f, .9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(.05f, .11f);
            main.startColor = new Color(.55f, .82f, 1f, .85f);
            main.gravityModifier = 1.2f;
            main.maxParticles = Application.isMobilePlatform ? 18 : 36;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(Application.isMobilePlatform ? 14 : 28)) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 28f;
            shape.radius = .15f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null) renderer.sharedMaterial = new Material(shader) { name = "Water Splash (runtime)" };
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return ps;
        }
    }
}
