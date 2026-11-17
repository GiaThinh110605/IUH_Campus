using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class AftershockResetTests
{
    [Test]
    public void NativeEncounterResetClearsDefeatWithoutActivatingObject()
    {
        var type = Type.GetType("IUHCampus.Guardian.IUHMonsterEncounter, Assembly-CSharp", true);
        var go = new GameObject("AftershockResetTest");
        go.SetActive(false);
        var encounter = go.AddComponent(type);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        try
        {
            type.GetField("m_IsDefeated", flags).SetValue(encounter, true);
            type.GetField("m_IsEncounterActive", flags).SetValue(encounter, true);
            var reset = type.GetMethod("ResetForAftershock");
            Assert.IsNotNull(reset);
            reset.Invoke(encounter, null);
            Assert.IsFalse((bool)type.GetField("m_IsDefeated", flags).GetValue(encounter));
            Assert.IsFalse((bool)type.GetField("m_IsEncounterActive", flags).GetValue(encounter));
            Assert.IsTrue((bool)type.GetField("m_RequireLeaveRange", flags).GetValue(encounter));
            Assert.AreEqual(0f, type.GetField("m_TriggerCooldown", flags).GetValue(encounter));
            Assert.IsFalse(go.activeSelf);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
