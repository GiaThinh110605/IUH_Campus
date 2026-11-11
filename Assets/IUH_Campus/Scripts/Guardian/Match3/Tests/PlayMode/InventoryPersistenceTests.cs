using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class InventoryPersistenceTests
{
    private const string Prefix = "IUH_Guardian_";
    private const string ItemsKey = Prefix + "Items";
    private const string Milestone = "test_g2_persistence";
    private const string MilestoneKey = Prefix + "Milestone_" + Milestone;
    private const string ItemName = "Potion G2";
    private readonly Dictionary<string, int?> savedInts = new Dictionary<string, int?>();
    private string savedItems;
    private bool hadItems;
    private Type inventoryType;
    private FieldInfo instanceField;
    private object originalInstance;
    private Component inventory;
    private GameObject host;

    [SetUp]
    public void SetUp()
    {
        foreach (string key in new[] { Prefix + "Gold", Prefix + "SwordLv", Prefix + "ShieldLv",
            Prefix + "MaxHpLv", MilestoneKey })
        {
            savedInts[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            PlayerPrefs.DeleteKey(key);
        }
        hadItems = PlayerPrefs.HasKey(ItemsKey);
        savedItems = PlayerPrefs.GetString(ItemsKey);
        PlayerPrefs.DeleteKey(ItemsKey);
        inventoryType = Type.GetType("IUHCampus.Guardian.IUHGuardianInventory, Assembly-CSharp", true);
        instanceField = inventoryType.GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        originalInstance = instanceField.GetValue(null);
    }

    [TearDown]
    public void TearDown()
    {
        if (host != null) UnityEngine.Object.DestroyImmediate(host);
        if (instanceField != null) instanceField.SetValue(null, originalInstance);
        foreach (var pair in savedInts)
        {
            if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
            else PlayerPrefs.DeleteKey(pair.Key);
        }
        if (hadItems) PlayerPrefs.SetString(ItemsKey, savedItems);
        else PlayerPrefs.DeleteKey(ItemsKey);
        PlayerPrefs.Save();
        savedInts.Clear();
    }

    private void Restart()
    {
        if (host != null) UnityEngine.Object.DestroyImmediate(host);
        instanceField.SetValue(null, null);
        host = new GameObject("G2InventoryPersistenceTest");
        inventory = host.AddComponent(inventoryType);
    }

    private IList Items => (IList)inventoryType.GetProperty("Items").GetValue(inventory);
    private object Call(string method, params object[] args) => inventoryType.GetMethod(method).Invoke(inventory, args);
    private int Count(string name)
    {
        foreach (object item in Items)
            if ((string)item.GetType().GetField("name").GetValue(item) == name)
                return (int)item.GetType().GetField("count").GetValue(item);
        return 0;
    }

    [Test]
    public void AddedItemSurvivesRestartWithExactCount()
    {
        Restart();
        Call("AddItem", ItemName, 3);
        Restart();
        Assert.AreEqual(3, Count(ItemName));
    }

    [Test]
    public void ConsumedLastItemDoesNotGrantStarterItemsAgain()
    {
        PlayerPrefs.SetString(ItemsKey, "{\"version\":1,\"items\":[]}");
        Restart();
        Assert.AreEqual(0, Items.Count);
        Call("AddItem", ItemName, 1);
        Assert.IsTrue((bool)Call("UseItem", ItemName));
        Restart();
        Assert.AreEqual(0, Items.Count);
    }

    [Test]
    public void MissingItemsKeyGrantsExactlyTwoStartersAndSavesImmediately()
    {
        Restart();
        Assert.AreEqual(2, Items.Count);
        Assert.AreEqual(1, Count("Bình Dược Liệu Nano (+50 HP)"));
        Assert.AreEqual(1, Count("Mảnh Tinh Thể Dữ Liệu Lam"));
        Assert.IsTrue(PlayerPrefs.HasKey(ItemsKey));
        StringAssert.Contains("\"version\":1", PlayerPrefs.GetString(ItemsKey));
    }

    [Test]
    public void CorruptJsonWarnsOnceAndKeepsStartersWithoutException()
    {
        PlayerPrefs.SetString(ItemsKey, "{broken json");
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("\\[IUH Guardian\\].*"));
        Assert.DoesNotThrow(Restart);
        Assert.AreEqual(2, Items.Count);
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void MilestoneGoldFlagAndItemsSurviveRestartTogether()
    {
        Restart();
        Assert.IsTrue((bool)Call("TryClaimMilestoneReward", Milestone, 25, new[] { ItemName, ItemName }));
        Restart();
        Assert.AreEqual(125, inventoryType.GetProperty("Gold").GetValue(inventory));
        Assert.IsTrue((bool)Call("HasClaimedMilestone", Milestone));
        Assert.AreEqual(2, Count(ItemName));
        Assert.IsFalse((bool)Call("TryClaimMilestoneReward", Milestone, 25, new[] { ItemName }));
        Assert.AreEqual(2, Count(ItemName));
    }

    [Test]
    public void InvalidEntriesAreSkippedWhileValidEntryKeepsItsIdentity()
    {
        PlayerPrefs.SetString(ItemsKey, "{\"version\":1,\"items\":[null,{\"name\":\"\",\"count\":2},{\"name\":\"bad\",\"count\":0},{\"name\":\"negative\",\"count\":-1},{\"id\":\"saved-id\",\"name\":\"Potion G2\",\"count\":4,\"isConsumable\":true}]}");
        Restart();
        Assert.AreEqual(1, Items.Count);
        Assert.AreEqual(4, Count(ItemName));
        Assert.AreEqual("saved-id", Items[0].GetType().GetField("id").GetValue(Items[0]));
        Assert.IsTrue((bool)Call("UseItem", ItemName));
        Restart();
        Assert.AreEqual(3, Count(ItemName));
    }
}
