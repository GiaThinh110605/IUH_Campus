using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EncounterRangeTests
{
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    [UnityTest]
    public IEnumerator FarEncounterLeavesInteractForOtherSystems()
    {
        return CheckRange(8, false);
    }

    [UnityTest]
    public IEnumerator NearEncounterStartsCombat()
    {
        return CheckRange(3, true);
    }

    static IEnumerator CheckRange(float distance, bool shouldEngage)
    {
        var managerType = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var encounterType = Type.GetType("IUHCampus.Guardian.IUHMonsterEncounter, Assembly-CSharp", true);
        var inputType = Type.GetType("IUHCampus.Core.IUHInputHelper, Assembly-CSharp", true);
        var flag = inputType.GetField("VirtualInteractPressed");
        var old = flag.GetValue(null);
        var player = new GameObject("Campus Student");
        var enemy = new GameObject("Range encounter");
        var fight = new GameObject("Range fight");
        var oldLock = Cursor.lockState;
        bool oldCursor = Cursor.visible;
        try
        {
            player.transform.position = Vector3.right * distance;
            var manager = fight.AddComponent(managerType);
            var encounter = enemy.AddComponent(encounterType);
            yield return null;
            encounterType.GetField("m_TriggerCooldown", Fields).SetValue(encounter, 0f);
            flag.SetValue(null, true);
            yield return null;
            Assert.AreEqual(shouldEngage, managerType.GetProperty("BlocksExplorationInput").GetValue(manager));
            Assert.AreEqual(!shouldEngage, flag.GetValue(null));
        }
        finally
        {
            flag.SetValue(null, old);
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(fight);
            UnityEngine.Object.DestroyImmediate(player);
            Cursor.lockState = oldLock;
            Cursor.visible = oldCursor;
        }
    }
}
