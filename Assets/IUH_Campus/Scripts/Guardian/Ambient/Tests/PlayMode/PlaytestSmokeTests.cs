#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class PlaytestSmokeTests
{
    GameObject root;
    static System.Type ZoneType => System.AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType("IUHCampus.Guardian.Story.IUHStoryCorruptionZone"))
        .First(t => t != null);
    Component Create(Vector3[] positions)
    {
        var zone = (Component)ZoneType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { positions, null });
        root = zone.gameObject;
        return zone;
    }
    [TearDown] public void Cleanup() { if (root) Object.DestroyImmediate(root); Time.timeScale = 1; }
    [Test] public void CenterRadiusNoSphereAndBudget()
    {
        var zone = Create(new[] { new Vector3(10, 2, 4), new Vector3(18, 2, 4) });
        Assert.That(root.transform.position, Is.EqualTo(new Vector3(14, 2, 4)));
        Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(ZoneType.GetProperty("Radius").GetValue(zone), Is.EqualTo(10f));
        Assert.That(root.GetComponentsInChildren<MeshRenderer>(true), Is.Empty);
        Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
        var systems = root.GetComponentsInChildren<ParticleSystem>(true);
        Assert.That(systems.Length, Is.GreaterThanOrEqualTo(Application.isMobilePlatform ? 6 : 10));
        Assert.That(systems.Sum(s => s.main.maxParticles), Is.LessThanOrEqualTo(Application.isMobilePlatform ? 30 : 80));
        Assert.That(systems.All(s => !s.collision.enabled), Is.True);
    }
    [TestCase(true)] [TestCase(false)] public void NullOrEmptyIsSafe(bool useNull)
    {
        var zone = Create(useNull ? null : new Vector3[0]);
        Assert.That(root.transform.position, Is.EqualTo(Vector3.zero));
        Assert.That(ZoneType.GetProperty("Radius").GetValue(zone), Is.EqualTo(6f));
    }
    [UnityTest] public IEnumerator PurifyStopsEmissionAndDestroysWhilePaused()
    {
        var zone = Create(new[] { Vector3.zero });
        Time.timeScale = 0;
        ZoneType.GetMethod("Purify").Invoke(zone, null);
        ZoneType.GetMethod("Purify").Invoke(zone, null);
        Assert.That(root.GetComponentsInChildren<ParticleSystem>().All(s => !s.isEmitting), Is.True);
        yield return new WaitForSecondsRealtime(2.3f);
        Assert.That(root == null, Is.True);
    }
    [UnityTest] public IEnumerator FadeUsesInitialByteAlphaRatherThanRepeatedRounding()
    {
        var zone = Create(new[] { Vector3.zero });
        var ps = root.GetComponentInChildren<ParticleSystem>();
        Assert.That(ps, Is.Not.Null);
        ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        var sample = new ParticleSystem.Particle
        {
            position = Vector3.zero, startLifetime = 10, remainingLifetime = 10,
            startSize = 1, startColor = new Color32(90, 65, 115, 179), randomSeed = 123456
        };
        ps.SetParticles(new[] { sample }, 1);
        Time.timeScale = 0;
        double start = Time.realtimeSinceStartupAsDouble;
        ZoneType.GetMethod("Purify").Invoke(zone, null);
        yield return new WaitForSecondsRealtime(.8f);
        var output = new ParticleSystem.Particle[80];
        Assert.That(ps.GetParticles(output), Is.EqualTo(1));
        float expected = 179 * Mathf.Clamp01(1f - (float)(Time.realtimeSinceStartupAsDouble - start) / 2f);
        Assert.That((float)output[0].startColor.a, Is.EqualTo(expected).Within(8f));
    }
}

#endif
