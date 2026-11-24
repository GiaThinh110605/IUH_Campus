#if UNITY_EDITOR
using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class PlaytestGroundFireBudgetTests
{
    [TestCase(false, 24)]
    [TestCase(true, 12)]
    public void FourSystemMonsterRespectsTotalBudgetAndEnvelope(bool mobile, int budget)
    {
        var prefab = Resources.Load<GameObject>("Guardian/VFX/CampusMonsterGroundFire");
        Assert.That(prefab, Is.Not.Null);
        var instance = UnityEngine.Object.Instantiate(prefab);
        try
        {
            var type = Type.GetType("IUHCampus.Guardian.Story.IUHCampusMonsterGroundFire, Assembly-CSharp", true);
            type.GetMethod("ConfigureForPlatform").Invoke(instance.GetComponent(type), new object[] { mobile });
            var systems = instance.GetComponentsInChildren<ParticleSystem>(true);
            Assert.That(systems.Length, Is.EqualTo(4));
            Assert.That(systems.Sum(p => p.main.maxParticles), Is.EqualTo(budget));
            foreach (var ps in systems)
            {
                Assert.That(ps.emission.rateOverTime.constant, Is.GreaterThan(0));
                Assert.That(ps.main.startColor.color.a, Is.GreaterThan(.35f));
                Assert.That(ps.collision.enabled, Is.False);
                Assert.That(ps.subEmitters.enabled, Is.False);
                float upperEnvelope = .2f + ps.main.startSize.constant / 2f
                    + ps.main.startLifetime.constant * ps.velocityOverLifetime.y.constant;
                Assert.That(upperEnvelope, Is.LessThanOrEqualTo(1f));
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }
}
#endif
