using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IUHCampus.Core.Tests
{
    public sealed class MobileControlsLookupTests
    {
        [Test]
        public void TenUpdatesWithoutPlayerKeepTheSameRetryDeadline()
        {
            const BindingFlags members = BindingFlags.Instance | BindingFlags.NonPublic;
            var type = Type.GetType("IUHCampus.UI.IUHMobileControlsUI, Assembly-CSharp", true);
            var update = type.GetMethod("Update", members);
            var deadline = type.GetField("m_NextPlayerSearch", members);
            var root = new GameObject("Mobile controls lookup test");
            root.SetActive(false);
            try
            {
                var controls = root.AddComponent(type);
                float now = Time.unscaledTime;
                update.Invoke(controls, null);
                Assert.That(deadline, Is.Not.Null);
                float firstDeadline = (float)deadline.GetValue(controls);
                Assert.That(firstDeadline, Is.EqualTo(now + 0.5f).Within(0.001f));
                for (int i = 1; i < 10; i++)
                {
                    update.Invoke(controls, null);
                    Assert.That(deadline.GetValue(controls), Is.EqualTo(firstDeadline));
                }

                deadline.SetValue(controls, now - 1f);
                update.Invoke(controls, null);
                Assert.That(deadline.GetValue(controls), Is.EqualTo(firstDeadline));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
