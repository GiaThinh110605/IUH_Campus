using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CombatOutcomeTests
{
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
