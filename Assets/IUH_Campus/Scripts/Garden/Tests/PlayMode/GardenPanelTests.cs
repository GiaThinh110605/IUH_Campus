using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IUHCampus.Garden
{
    public class GardenPanelTests
    {
        [Test]
        public void NearestBedIsSelected()
        {
            var type = Type.GetType("IUHCampus.Garden.IUHGardenPanelUI, Assembly-CSharp", true);
            var find = type.GetMethod("FindNearestBed");
            var beds = new[]
            {
                new Bounds(Vector3.zero, new Vector3(6f, .3f, 1f)),
                new Bounds(Vector3.forward * 4f, new Vector3(6f, .3f, 1f))
            };
            Assert.AreEqual(0, find.Invoke(null, new object[] { new Vector3(4.5f, 0, 0), beds, 2f }));
            Assert.AreEqual(1, find.Invoke(null, new object[] { new Vector3(2.5f, 0, 3), beds, 2f }));
            Assert.AreEqual(-1, find.Invoke(null, new object[] { Vector3.right * 5.1f, beds, 2f }));
        }

        [Test]
        public void PanelDoesNotOpenDuringCombat()
        {
            var type = Type.GetType("IUHCampus.Garden.IUHGardenPanelUI, Assembly-CSharp", true);
            var combatType = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
            var panel = new GameObject("GardenPanelTest");
            var fight = new GameObject("GardenFightTest");
            try
            {
                panel.SetActive(false);
                var view = panel.AddComponent(type);
                var manager = fight.AddComponent(combatType);
                combatType.GetField("m_IsEngaged", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(manager, true);
                Assert.IsFalse((bool)type.GetMethod("CanOpenNow").Invoke(view, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fight);
                UnityEngine.Object.DestroyImmediate(panel);
            }
        }
    }
}
