using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class UpgradeStatsTests
{
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly Type InventoryType = Type.GetType("IUHCampus.Guardian.IUHGuardianInventory, Assembly-CSharp", true);
    static readonly Type ConfigType = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatConfig, IUHCampus.Guardian.Match3.Core", true);
    GameObject inventoryObject;
    Component inventory;

    [SetUp]
    public void CreateInventory()
    {
        inventoryObject = new GameObject("UpgradeInventoryTest");
        inventoryObject.SetActive(false);
        inventory = inventoryObject.AddComponent(InventoryType);
    }

    [TearDown]
    public void DestroyInventory()
    {
        UnityEngine.Object.DestroyImmediate(inventoryObject);
    }

    static void SetLevels(Component target, int sword, int shield, int hp)
    {
        InventoryType.GetField("m_SwordLevel", Fields).SetValue(target, sword);
        InventoryType.GetField("m_ShieldLevel", Fields).SetValue(target, shield);
        InventoryType.GetField("m_MaxHpLevel", Fields).SetValue(target, hp);
    }

    void AssertInventoryStats(int sword, int shield, int hp)
    {
        Assert.AreEqual(sword, InventoryType.GetMethod("GetSwordDamage").Invoke(inventory, null));
        Assert.AreEqual(shield, InventoryType.GetMethod("GetShieldValue").Invoke(inventory, null));
        Assert.AreEqual(hp, InventoryType.GetMethod("GetMaxHp").Invoke(inventory, null));
    }

    [Test]
    public void InventoryTablesMatchSpec()
    {
        var sword = new[] { 22, 25, 29, 34, 40 };
        var shield = new[] { 15, 18, 21, 25, 30 };
        var hp = new[] { 1000, 1100, 1220, 1360, 1500 };
        for (int i = 0; i < sword.Length; i++)
        {
            SetLevels(inventory, i + 1, i + 1, i + 1);
            AssertInventoryStats(sword[i], shield[i], hp[i]);
        }
    }

    [Test]
    public void Level1EqualsMatch3ConfigDefaults()
    {
        SetLevels(inventory, 1, 1, 1);
        var config = Activator.CreateInstance(ConfigType);
        AssertInventoryStats((int)ConfigType.GetField("DmgPerSword").GetValue(config),
            (int)ConfigType.GetField("ShieldPerTile").GetValue(config),
            (int)ConfigType.GetField("PlayerMaxHp").GetValue(config));
    }

    [Test]
    public void OutOfRangeLevelIsClamped()
    {
        SetLevels(inventory, 0, 0, 0);
        AssertInventoryStats(22, 15, 1000);
        SetLevels(inventory, 9, 9, 9);
        AssertInventoryStats(40, 30, 1500);
    }
}
