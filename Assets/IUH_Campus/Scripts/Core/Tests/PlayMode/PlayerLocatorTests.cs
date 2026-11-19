using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IUHCampus.Core.Tests
{
    public sealed class PlayerLocatorTests
    {
        [Test]
        public void MissingPlayerSearchIsThrottledAndCacheCanBeReset()
        {
            var locator = Type.GetType("IUHCampus.Core.IUHPlayerLocator, Assembly-CSharp");
            Assert.That(locator, Is.Not.Null);
            var clock = locator.GetField("Clock");
            var originalClock = clock.GetValue(null);
            var playerProperty = locator.GetProperty("Player");
            var reset = locator.GetMethod("Reset");
            float now = 0f;
            GameObject player = null;
            GameObject fallback = null;
            try
            {
                clock.SetValue(null, (Func<float>)(() => now));
                reset.Invoke(null, null);
                Assert.That(playerProperty.GetValue(null), Is.Null);

                fallback = new GameObject("Fallback player") { tag = "Player" };
                player = new GameObject("Campus Student");
                now = 0.49f;
                Assert.That(playerProperty.GetValue(null), Is.Null);
                now = 0.5f;
                Assert.That(playerProperty.GetValue(null), Is.SameAs(player.transform));

                Object.DestroyImmediate(player);
                Assert.That(playerProperty.GetValue(null), Is.Null);
                now = 1f;
                Assert.That(playerProperty.GetValue(null), Is.SameAs(fallback.transform));

                player = new GameObject("Campus Student");
                Assert.That(playerProperty.GetValue(null), Is.SameAs(fallback.transform));
                reset.Invoke(null, null);
                Assert.That(playerProperty.GetValue(null), Is.SameAs(player.transform));
            }
            finally
            {
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(fallback);
                clock.SetValue(null, originalClock);
                reset.Invoke(null, null);
            }
        }
    }
}
