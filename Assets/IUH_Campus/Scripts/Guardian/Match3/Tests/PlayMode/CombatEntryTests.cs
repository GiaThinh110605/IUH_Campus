using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
public class CombatEntryTests
{
    private static Type FindType(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        { var type = assembly.GetType(name); if (type != null) return type; }
        throw new TypeLoadException(name);
    }
    [Test] public void FreeCursorStopsPendingMovementOnFirstApply()
    {
        var go = new GameObject("PreviewMotionTest");
        try
        {
            var controllerType = FindType("StarterAssets.ThirdPersonController");
            var previewType = Type.GetType("IUHCampus.World.IUHConceptPreviewController, Assembly-CSharp", true);
            var controller = (Behaviour)go.AddComponent(controllerType);
            var inputsType = FindType("StarterAssets.StarterAssetsInputs");
            var inputs = go.GetComponent(inputsType) ?? go.AddComponent(inputsType);
            inputsType.GetField("move").SetValue(inputs, Vector2.up);
            controllerType.GetField("_speed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, 5f);
            var preview = go.AddComponent(previewType);
            previewType.GetField("movement", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(preview, controller);
            previewType.GetField("freeCursor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(preview, true);
            previewType.GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(preview, null);
            Assert.IsFalse(controller.enabled);
            Assert.AreEqual(Vector2.zero, inputsType.GetField("move").GetValue(inputs));
            Assert.AreEqual(0f, controllerType.GetField("_speed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller));
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }
    [Test] public void ConsumedEscapeDoesNotAlsoPauseGameFlow()
    {
        var go = new GameObject("EscapeOwnershipTest"); go.SetActive(false);
        var previewType = FindType("IUHCampus.World.IUHConceptPreviewController");
        var field = previewType.GetField("escapeHandledFrame", BindingFlags.Static | BindingFlags.NonPublic);
        var saved = field.GetValue(null);
        try
        {
            field.SetValue(null, Time.frameCount);
            var flowType = FindType("IUHCampus.Core.IUHGameFlowManager"); var flow = go.AddComponent(flowType);
            var before = flowType.GetProperty("CurrentState").GetValue(flow);
            flowType.GetMethod("HandleEscapeKey", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(flow, null);
            Assert.AreEqual(before, flowType.GetProperty("CurrentState").GetValue(flow));
        }
        finally { field.SetValue(null, saved); UnityEngine.Object.DestroyImmediate(go); }
    }
    [UnityTest] public IEnumerator IntroBlocksCombatAndFleeCancelsIt()
    {
        var type = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var go = new GameObject("CombatEntryTest");
        var manager = (MonoBehaviour)go.AddComponent(type);
        try
        {
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            yield return null;
            Assert.IsFalse((bool)type.GetProperty("IsCombatActive").GetValue(manager), "Combat must stay locked during Battle Start");
            var intro = go.transform.Find("BattleStartIntro");
            Assert.IsNotNull(intro);
            Assert.IsNotNull(intro.Find("BATTLE")); Assert.IsNotNull(intro.Find("START"));
            type.GetMethod("Flee").Invoke(manager, null);
            yield return null;
            Assert.IsNull(go.transform.Find("BattleStartIntro"));
            Assert.IsFalse((bool)type.GetProperty("IsCombatActive").GetValue(manager));
        }
        finally { UnityEngine.Object.Destroy(go); }
    }
    [UnityTest] public IEnumerator HudFadesBeforeUnlockAndFleeRestoresLayout()
    {
        var type = FindType("IUHCampus.Guardian.Match3.Match3CombatManager");
        var go = new GameObject("HudRevealTest");
        var hud = new GameObject("TopHUD", typeof(RectTransform));
        hud.transform.SetParent(go.transform, false);
        var rect = hud.GetComponent<RectTransform>();
        var home = new Vector2(10, -20); rect.anchoredPosition = home;
        var manager = (MonoBehaviour)go.AddComponent(type);
        try
        {
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            yield return null;
            while (go.transform.Find("BattleStartIntro") != null) yield return null;
            yield return new WaitForSecondsRealtime(.12f);
            var group = hud.GetComponent<CanvasGroup>();
            Assert.That(group.alpha, Is.GreaterThan(0).And.LessThan(1), "HUD should fade instead of popping in");
            Assert.IsFalse(group.interactable);
            Assert.IsFalse((bool)type.GetProperty("IsCombatActive").GetValue(manager));
            Assert.AreNotEqual(home, rect.anchoredPosition);
            type.GetMethod("Flee").Invoke(manager, null);
            Assert.AreEqual(home, rect.anchoredPosition, "Cancel must restore layout for the next fight");
        }
        finally { UnityEngine.Object.Destroy(go); }
    }
    [UnityTest] public IEnumerator IntroFinishesBeforeCombatUnlocks()
    {
        var type = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var go = new GameObject("CombatEntryTest"); var manager = go.AddComponent(type);
        try
        {
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            yield return new WaitForSecondsRealtime(2f);
            Assert.IsTrue((bool)type.GetProperty("IsCombatActive").GetValue(manager));
            Assert.IsNull(go.transform.Find("BattleStartIntro"));
        }
        finally { UnityEngine.Object.Destroy(go); }
    }
}
