using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace IUHCampus.Garden
{
    public class GardenManagerTests
    {
        static readonly DateTime Start = new DateTime(2026, 11, 25, 8, 0, 0, DateTimeKind.Utc);
        static readonly Type ManagerType = Type.GetType("IUHCampus.Garden.IUHGardenManager, Assembly-CSharp", true);
        static readonly Type InventoryType = Type.GetType("IUHCampus.Guardian.IUHGuardianInventory, Assembly-CSharp", true);
        const string SaveKey = "IUH_Garden_v3";

        [Test]
        public void WateringPaysGoldOncePerCooldownAndPersists()
        {
            var restore = new List<Action>();
            foreach (var key in new[] { "IUH_Guardian_Gold", "IUH_Guardian_SwordLv", "IUH_Guardian_ShieldLv", "IUH_Guardian_MaxHpLv" })
            {
                bool had = PlayerPrefs.HasKey(key); int value = PlayerPrefs.GetInt(key);
                restore.Add(() => { if (had) PlayerPrefs.SetInt(key, value); else PlayerPrefs.DeleteKey(key); });
            }
            foreach (var key in new[] { "IUH_Guardian_Items", SaveKey })
            {
                bool had = PlayerPrefs.HasKey(key); string value = PlayerPrefs.GetString(key);
                restore.Add(() => { if (had) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); });
            }
            var instanceProp = InventoryType.GetProperty("Instance");
            var originalInventory = instanceProp.GetValue(null);
            var host = new GameObject("GardenManagerTest");
            var invHost = new GameObject("GardenInventoryTest");
            try
            {
                PlayerPrefs.DeleteKey(SaveKey);
                invHost.SetActive(false);
                var inventory = invHost.AddComponent(InventoryType);
                InventoryType.GetField("m_Gold", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(inventory, 100);
                instanceProp.GetSetMethod(true).Invoke(null, new object[] { inventory });
                int gold0 = 100;
                var manager = host.AddComponent(ManagerType);
                var now = Start;
                ManagerType.GetProperty("Clock").SetValue(manager, (Func<DateTime>)(() => now));
                var beds = new List<Bounds> { new Bounds(new Vector3(-20, .35f, -1), new Vector3(16, .1f, 12)), new Bounds(new Vector3(9, .2f, 10), new Vector3(3, .1f, 8)) };
                ManagerType.GetMethod("Configure").Invoke(manager, new object[] { beds });
                Assert.AreEqual(2, (int)ManagerType.GetProperty("SpotCount").GetValue(manager));
                Assert.IsTrue(Water(manager, 0));
                Assert.IsFalse(Water(manager, 0), "cooldown");
                Assert.IsTrue(Water(manager, 1), "other bed independent");
                Assert.AreEqual(gold0 + 2 * GardenRules.WaterGold, (int)InventoryType.GetProperty("Gold").GetValue(inventory));
                now = Start.AddSeconds(GardenRules.CooldownSeconds);
                Assert.IsTrue(Water(manager, 0));

                var reloaded = new GameObject("GardenManagerReload").AddComponent(ManagerType);
                try
                {
                    ManagerType.GetMethod("Configure").Invoke(reloaded, new object[] { beds });
                    var spot = (WateringSpot)ManagerType.GetMethod("GetSpot").Invoke(reloaded, new object[] { 0 });
                    Assert.AreEqual(now.Ticks, spot.LastWateredUtcTicks, "saved per bed id");
                }
                finally { UnityEngine.Object.DestroyImmediate(reloaded.gameObject); }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                instanceProp.GetSetMethod(true).Invoke(null, new[] { originalInventory });
                UnityEngine.Object.DestroyImmediate(invHost);
                foreach (var undo in restore) undo();
                PlayerPrefs.Save();
            }
        }

        [Test]
        public void FindAreasSplitsConnectedLawnPieces()
        {
            var go = new GameObject("LawnTest");
            try
            {
                var mesh = new Mesh();
                // two separate 2x2 quads facing up
                mesh.vertices = new[] { new Vector3(0,0,0), new Vector3(0,0,2), new Vector3(2,0,2), new Vector3(2,0,0),
                                        new Vector3(5,0,0), new Vector3(5,0,2), new Vector3(7,0,2), new Vector3(7,0,0) };
                mesh.triangles = new[] { 0,1,2, 0,2,3, 4,5,6, 4,6,7 };
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var list = (List<Bounds>)ManagerType.GetMethod("FindAreas").Invoke(null, new object[] { go.GetComponent<MeshFilter>() });
                Assert.AreEqual(2, list.Count);
                Assert.AreEqual(1f, list[0].center.x, .01f);
                Assert.AreEqual(6f, list[1].center.x, .01f);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        static bool Water(Component manager, int index)
        {
            var args = new object[] { index, 0 };
            return (bool)ManagerType.GetMethod("TryWater").Invoke(manager, args);
        }
    }
}
