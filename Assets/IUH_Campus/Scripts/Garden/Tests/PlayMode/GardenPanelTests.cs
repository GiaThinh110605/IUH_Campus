using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IUHCampus.Garden
{
    public class GardenPanelTests
    {
        static readonly Type PanelType = Type.GetType("IUHCampus.Garden.IUHGardenPanelUI, Assembly-CSharp", true);

        [Test]
        public void NearestBedUsesHorizontalDistanceToBedEdge()
        {
            var find = PanelType.GetMethod("FindNearestBed");
            var beds = new[]
            {
                new Bounds(new Vector3(0, .4f, 0), new Vector3(6f, .3f, 1f)),
                new Bounds(new Vector3(0, .4f, 4), new Vector3(6f, .3f, 1f))
            };
            Assert.AreEqual(0, find.Invoke(null, new object[] { new Vector3(4.5f, 0, 0), beds, 1.8f }));
            Assert.AreEqual(1, find.Invoke(null, new object[] { new Vector3(2.5f, 0, 3), beds, 1.8f }));
            Assert.AreEqual(-1, find.Invoke(null, new object[] { new Vector3(5.1f, 0, 0), beds, 1.8f }));
            Assert.AreEqual(0, find.Invoke(null, new object[] { new Vector3(0, 5f, 1.2f), beds, 1.8f }), "height ignored");
        }

        [Test]
        public void WateringIsBlockedDuringCombat()
        {
            var combatType = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
            var panel = new GameObject("GardenPanelTest");
            var fight = new GameObject("GardenFightTest");
            try
            {
                panel.SetActive(false);
                var view = panel.AddComponent(PanelType);
                var manager = fight.AddComponent(combatType);
                combatType.GetField("m_IsEngaged", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(manager, true);
                Assert.IsFalse((bool)PanelType.GetMethod("CanOpenNow").Invoke(view, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(panel);
                UnityEngine.Object.DestroyImmediate(fight);
            }
        }
    }
}
