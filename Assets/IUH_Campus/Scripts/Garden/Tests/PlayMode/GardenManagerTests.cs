using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IUHCampus.Garden
{
    public class GardenManagerTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static readonly DateTime Start = new DateTime(2026, 11, 19, 8, 0, 0, DateTimeKind.Utc);

        [TestCase(5, false, 5)]
        [TestCase(100, false, 120)]
        [TestCase(100, true, 125)]
        public void PlantAndHarvestRespectGold(int startGold, bool fertilize, int finalGold)
        {
            WithManager(startGold, (manager, inventory) =>
            {
                var type = manager.GetType();
                var bed = (GardenBed)type.GetMethod("GetBed").Invoke(manager, new object[] { 0 });
                if (startGold < 10)
                {
                    Assert.IsFalse(Act(manager, "TryPlant"));
                    Assert.AreEqual(GardenStage.Empty, bed.Stage);
                }
                else
                {
                    Assert.IsTrue(Act(manager, "TryPlant"));
                    Assert.IsTrue(Act(manager, "TryWater"));
                    if (fertilize) Assert.IsTrue(Act(manager, "TryFertilize"));
                    SetTime(manager, Start.AddMinutes(5));
                    type.GetMethod("Load").Invoke(manager, null);
                    Assert.AreEqual(GardenStage.Sprout, ((GardenBed)type.GetMethod("GetBed").Invoke(manager, new object[] { 0 })).Stage);
                    Assert.IsTrue(Act(manager, "TryWater"));
                    SetTime(manager, Start.AddMinutes(6));
                    type.GetMethod("Load").Invoke(manager, null);
                    Assert.IsTrue(Act(manager, "TryHarvest"));
                    Assert.IsFalse(Act(manager, "TryHarvest"));
                }

                Assert.AreEqual(finalGold, inventory.GetType().GetProperty("Gold").GetValue(inventory));
            });
        }

        static bool Act(Component manager, string method) =>
            (bool)manager.GetType().GetMethod(method).Invoke(manager, new object[] { 0 });

        static void SetTime(Component manager, DateTime time) =>
            manager.GetType().GetProperty("Clock").SetValue(manager, (Func<DateTime>)(() => time));

        static void WithManager(int gold, Action<Component, Component> check)
        {
            var inventoryType = Type.GetType("IUHCampus.Guardian.IUHGuardianInventory, Assembly-CSharp", true);
            var managerType = Type.GetType("IUHCampus.Garden.IUHGardenManager, Assembly-CSharp", true);
            var setter = inventoryType.GetProperty("Instance").GetSetMethod(true);
            var original = inventoryType.GetProperty("Instance").GetValue(null);
            var restore = new System.Collections.Generic.List<Action>();
            foreach (var key in new[] { "IUH_Guardian_Gold", "IUH_Guardian_SwordLv", "IUH_Guardian_ShieldLv", "IUH_Guardian_MaxHpLv" })
            {
                bool had = PlayerPrefs.HasKey(key);
                int value = PlayerPrefs.GetInt(key);
                restore.Add(() => { if (had) PlayerPrefs.SetInt(key, value); else PlayerPrefs.DeleteKey(key); });
            }
            foreach (var key in new[] { "IUH_Guardian_Items", "IUH_Garden_v1" })
            {
                bool had = PlayerPrefs.HasKey(key);
                string value = PlayerPrefs.GetString(key);
                restore.Add(() => { if (had) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); });
            }
            var stock = new GameObject("Test inventory");
            var garden = new GameObject("Test garden");
            try
            {
                stock.SetActive(false);
                garden.SetActive(false);
                var inventory = stock.AddComponent(inventoryType);
                inventoryType.GetField("m_Gold", Private).SetValue(inventory, gold);
                setter.Invoke(null, new object[] { inventory });
                PlayerPrefs.DeleteKey("IUH_Garden_v1");
                var manager = garden.AddComponent(managerType);
                SetTime(manager, Start);
                garden.SetActive(true);
                check(manager, inventory);
            }
            finally
            {
                setter.Invoke(null, new[] { original });
                UnityEngine.Object.DestroyImmediate(garden);
                UnityEngine.Object.DestroyImmediate(stock);
                foreach (var undo in restore) undo();
                PlayerPrefs.Save();
            }
        }
    }
}
