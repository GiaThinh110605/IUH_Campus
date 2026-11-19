using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CombatOutcomeTests
{
    [Test]
    public void ClosingVictoryKeepsMobileControlsHiddenOnStandalone()
    {
        if (!Application.isEditor &&
            Application.platform != RuntimePlatform.WindowsPlayer &&
            Application.platform != RuntimePlatform.OSXPlayer &&
            Application.platform != RuntimePlatform.LinuxPlayer)
            Assert.Ignore("Requires desktop Play Mode.");
        if (Application.isMobilePlatform) Assert.Ignore("Mobile controls are expected on mobile.");

        var controlsType = Type.GetType("IUHCampus.UI.IUHMobileControlsUI, Assembly-CSharp", true);
        var managerType = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var controls = new GameObject("DesktopMobileControlsTest");
        var battle = new GameObject("DesktopVictoryTest");
        try
        {
            controls.SetActive(false);
            var mobile = controls.AddComponent(controlsType);
            controlsType.GetField("m_ForceShowOnPC", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mobile, false);
            controls.SetActive(true);
            Assert.IsFalse(controls.activeSelf);
            var manager = battle.AddComponent(managerType);
            managerType.GetMethod("OnCloseVictoryClicked", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
            Assert.IsFalse(controls.activeSelf);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(battle);
            UnityEngine.Object.DestroyImmediate(controls);
        }
    }

    sealed class Recorder
    {
        public int Count;
        public bool Won;

        public void Record(object encounter, bool won)
        {
            Count++;
            Won = won;
        }
    }

    [Test]
    public void FleeReportsLossExactlyOnce()
    {
        var type = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var go = new GameObject("FleeOutcomeTest");
        var manager = go.AddComponent(type);
        var recorder = new Recorder();
        var finished = type.GetEvent("CombatFinished");
        var handler = Delegate.CreateDelegate(finished.EventHandlerType, recorder, typeof(Recorder).GetMethod("Record"));
        finished.AddEventHandler(manager, handler);
        try
        {
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            type.GetMethod("Flee").Invoke(manager, null);
            type.GetMethod("Flee").Invoke(manager, null);
            Assert.AreEqual(1, recorder.Count);
            Assert.IsFalse(recorder.Won);
        }
        finally
        {
            finished.RemoveEventHandler(manager, handler);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
