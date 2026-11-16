using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CombatReentryTests
{
    [Test]
    public void EngagedFightIgnoresRepeatedStart()
    {
        var type = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var go = new GameObject("CombatReentryTest");
        var manager = go.AddComponent(type);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        try
        {
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            var rules = type.GetField("m_Rules", flags).GetValue(manager);
            type.GetField("m_RemainingTime", flags).SetValue(manager, 37f);
            LogAssert.Expect(LogType.Warning, "[IUH Guardian] StartNewCombat ignored: a fight is already in progress.");
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            Assert.AreSame(rules, type.GetField("m_Rules", flags).GetValue(manager));
            Assert.AreEqual(37f, type.GetField("m_RemainingTime", flags).GetValue(manager));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
