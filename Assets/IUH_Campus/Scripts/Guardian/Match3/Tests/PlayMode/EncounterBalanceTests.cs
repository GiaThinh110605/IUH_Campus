using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EncounterBalanceTests
{
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    [TestCase(65, 90, 46, 63)]
    [TestCase(8, 12, 6, 8)]
    [TestCase(12, 8, 8, 8)]
    public void EncounterStatsAreScaled(int min, int max, int expectedMin, int expectedMax)
    {
        var managerType = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var encounterType = Type.GetType("IUHCampus.Guardian.IUHMonsterEncounter, Assembly-CSharp", true);
        var fight = new GameObject("Balance fight");
        var enemy = new GameObject("Balance encounter");
        try
        {
            var manager = fight.AddComponent(managerType);
            var encounter = enemy.AddComponent(encounterType);
            encounterType.GetField("m_MonsterHp", Fields).SetValue(encounter, 1000);
            encounterType.GetField("m_BossMinDmg", Fields).SetValue(encounter, min);
            encounterType.GetField("m_BossMaxDmg", Fields).SetValue(encounter, max);
            encounterType.GetField("m_CombatDuration", Fields).SetValue(encounter, 90f);
            managerType.GetMethod("SetActiveEncounter").Invoke(manager, new object[] { encounter });
            var config = managerType.GetField("m_Config", Fields).GetValue(manager);
            Assert.AreEqual(600, config.GetType().GetField("BossMaxHp").GetValue(config));
            Assert.AreEqual(expectedMin, config.GetType().GetField("BossMinDmg").GetValue(config));
            Assert.AreEqual(expectedMax, config.GetType().GetField("BossMaxDmg").GetValue(config));
            Assert.AreEqual(120f, managerType.GetField("m_CombatDuration", Fields).GetValue(manager));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(fight);
        }
    }
}
