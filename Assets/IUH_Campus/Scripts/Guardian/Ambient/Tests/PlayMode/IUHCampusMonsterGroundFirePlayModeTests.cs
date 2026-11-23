#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class IUHCampusMonsterGroundFirePlayModeTests
{
    GameObject encounter;
    [TearDown] public void Cleanup() { if (encounter != null) Object.DestroyImmediate(encounter); }

    GameObject CreateFire()
    {
        encounter = new GameObject("Test Encounter");
        var prefab = Resources.Load<GameObject>("Guardian/VFX/CampusMonsterGroundFire");
        Assert.That(prefab, Is.Not.Null);
        var fire = Object.Instantiate(prefab, encounter.transform);
        fire.transform.localPosition = Vector3.up * .2f;
        return fire;
    }

    [Test]
    public void MobileHalvesCapacityEmissionAndPreservesLowParticleEnvelope()
    {
        var fire = CreateFire();
        var type = System.Type.GetType("IUHCampus.Guardian.Story.IUHCampusMonsterGroundFire, Assembly-CSharp");
        Assert.That(type, Is.Not.Null);
        var component = fire.GetComponent(type);
        var configure = type.GetMethod("ConfigureForPlatform");
        var systems = fire.GetComponentsInChildren<ParticleSystem>(true);
        Assert.That(systems, Is.Not.Empty);
        configure.Invoke(component, new object[] { false });
        int desktopCapacity = systems.Sum(ps => ps.main.maxParticles);
        float desktopRate = systems.Sum(ps => ps.emission.rateOverTime.constant);
        configure.Invoke(component, new object[] { true });
        Assert.That(systems.Sum(ps => ps.main.maxParticles), Is.LessThanOrEqualTo(desktopCapacity / 2));
        Assert.That(systems.Sum(ps => ps.emission.rateOverTime.constant), Is.LessThanOrEqualTo(desktopRate / 2f));
        foreach (var ps in systems)
        {
            Assert.That(ps.emission.burstCount, Is.Zero);
            Assert.That(ps.collision.enabled, Is.False);
            Assert.That(ps.subEmitters.enabled, Is.False);
            ps.Simulate(8f, false, true);
            var sample = new ParticleSystem.Particle[ps.main.maxParticles];
            int count = ps.GetParticles(sample);
            Assert.That(count, Is.GreaterThan(0));
            foreach (var p in sample.Take(count))
            {
                Vector3 world = ps.transform.TransformPoint(p.position);
                Assert.That(world.y + .175f, Is.LessThanOrEqualTo(1f));
                Assert.That(world.y - .175f, Is.GreaterThanOrEqualTo(0f));
                Assert.That(new Vector2(world.x, world.z).magnitude + .175f, Is.LessThanOrEqualTo(.65f));
            }
        }
    }

    [UnityTest]
    public IEnumerator ParentDisableClearsFireAndArenaStripRetainsParticlesWithoutErrors()
    {
        var fire = CreateFire();
        var systems = fire.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in systems) ps.Simulate(2f, false, true);
        encounter.SetActive(false);
        foreach (var ps in systems) Assert.That(ps.particleCount, Is.Zero);
        encounter.SetActive(true);
        yield return null;
        foreach (var ps in systems) Assert.That(ps.isPlaying, Is.True);
        var type = System.Type.GetType("IUHCampus.Guardian.Match3.Match3BattleStage, Assembly-CSharp");
        Assert.That(type, Is.Not.Null);
        type.GetMethod("StripToVisuals", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { encounter });
        yield return null;
        Assert.That(encounter.GetComponentsInChildren<ParticleSystem>(true).Length, Is.EqualTo(systems.Length));
        LogAssert.NoUnexpectedReceived();
    }
}

#endif
