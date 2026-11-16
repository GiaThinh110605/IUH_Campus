using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CombatReentryTests
{
    [Test]
    public void RetryCanRestartAFinishedFight()
    {
        var type = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var go = new GameObject("CombatRetryTest");
        go.SetActive(false);
        var manager = go.AddComponent(type);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Type buttonType = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            buttonType = assembly.GetType("UnityEngine.UI.Button");
            if (buttonType != null) break;
        }
        Assert.IsNotNull(buttonType);
        var buttonObject = new GameObject("Retry", typeof(RectTransform));
        buttonObject.transform.SetParent(go.transform, false);
        var button = buttonObject.AddComponent(buttonType);
        type.GetField("m_BtnRetryDefeat", flags).SetValue(manager, button);
        go.SetActive(true);
        try
        {
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            var rules = type.GetField("m_Rules", flags).GetValue(manager);
            type.GetField("m_OutcomeReported", flags).SetValue(manager, true);
            type.GetField("m_IsCombatActive", flags).SetValue(manager, false);
            var clicked = (UnityEngine.Events.UnityEvent)buttonType.GetProperty("onClick").GetValue(button);
            clicked.Invoke();
            Assert.AreNotSame(rules, type.GetField("m_Rules", flags).GetValue(manager));
            Assert.IsFalse((bool)type.GetField("m_OutcomeReported", flags).GetValue(manager));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

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
