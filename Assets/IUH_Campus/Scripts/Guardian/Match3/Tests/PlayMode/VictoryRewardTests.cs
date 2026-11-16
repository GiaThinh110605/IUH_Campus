using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class VictoryRewardTests
{
    private const string GoldKey = "IUH_Guardian_Gold";
    private const string ItemsKey = "IUH_Guardian_Items";
    private string savedItems;
    private const string Milestone = "test_g1_victory";
    private const string MilestoneKey = "IUH_Guardian_Milestone_" + Milestone;
    private const string ItemName = "test_g1_item";
    private readonly Dictionary<string, int?> savedPrefs = new Dictionary<string, int?>();
    private Type inventoryType;
    private FieldInfo instanceField;
    private object originalInstance;
    private Component inventory;
    private GameObject host;
    private int notifications;

    [SetUp]
    public void SetUp()
    {
        notifications = 0;
        savedItems = PlayerPrefs.HasKey(ItemsKey) ? PlayerPrefs.GetString(ItemsKey) : null;
        PlayerPrefs.DeleteKey(ItemsKey);
        foreach (string key in new[] { GoldKey, MilestoneKey, "IUH_Guardian_SwordLv",
            "IUH_Guardian_ShieldLv", "IUH_Guardian_MaxHpLv" })
            savedPrefs[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
        PlayerPrefs.SetInt(GoldKey, 100);
        PlayerPrefs.DeleteKey(MilestoneKey);
        inventoryType = Type.GetType("IUHCampus.Guardian.IUHGuardianInventory, Assembly-CSharp", true);
        instanceField = inventoryType.GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        originalInstance = instanceField.GetValue(null);
        CreateInventory();
    }

    private void CreateInventory()
    {
        instanceField.SetValue(null, null);
        host = new GameObject("G1VictoryRewardTest");
        inventory = host.AddComponent(inventoryType);
        inventoryType.GetEvent("OnDataChanged").AddEventHandler(inventory, (Action)(() => notifications++));
    }

    [TearDown]
    public void TearDown()
    {
        if (host != null) UnityEngine.Object.DestroyImmediate(host);
        if (instanceField != null) instanceField.SetValue(null, originalInstance);
        foreach (var pair in savedPrefs)
        {
            if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
            else PlayerPrefs.DeleteKey(pair.Key);
        }
        if (savedItems != null) PlayerPrefs.SetString(ItemsKey, savedItems);
        else PlayerPrefs.DeleteKey(ItemsKey);
        PlayerPrefs.Save();
        savedPrefs.Clear();
    }

    private int Gold => (int)inventoryType.GetProperty("Gold").GetValue(inventory);

    private bool Claim(string milestone, int gold, string[] items = null)
    {
        var method = inventoryType.GetMethod("TryClaimMilestoneReward");
        Assert.IsNotNull(method, "Milestone reward API must exist.");
        return (bool)method.Invoke(inventory, new object[] { milestone, gold, items });
    }

    private bool HasClaimed(string milestone)
    {
        var method = inventoryType.GetMethod("HasClaimedMilestone");
        Assert.IsNotNull(method, "Milestone ledger API must exist.");
        return (bool)method.Invoke(inventory, new object[] { milestone });
    }

    private int ItemCount()
    {
        foreach (object item in (IEnumerable)inventoryType.GetProperty("Items").GetValue(inventory))
            if ((string)item.GetType().GetField("name").GetValue(item) == ItemName)
                return (int)item.GetType().GetField("count").GetValue(item);
        return 0;
    }

    [Test]
    public void FirstClaimAddsGoldAndItemsAndNotifiesOnce()
    {
        Assert.IsTrue(Claim(Milestone, 25, new[] { ItemName, ItemName }));
        Assert.AreEqual(125, Gold);
        Assert.AreEqual(125, PlayerPrefs.GetInt(GoldKey));
        Assert.AreEqual(1, PlayerPrefs.GetInt(MilestoneKey));
        Assert.IsTrue(HasClaimed(Milestone));
        Assert.AreEqual(2, ItemCount());
        Assert.AreEqual(1, notifications);
    }

    [Test]
    public void SecondClaimDoesNotChangeGoldItemsOrNotifications()
    {
        Assert.IsTrue(Claim(Milestone, 25, new[] { ItemName }));
        Assert.IsFalse(Claim(Milestone, 25, new[] { ItemName }));
        Assert.AreEqual(125, Gold);
        Assert.AreEqual(1, ItemCount());
        Assert.AreEqual(1, notifications);
    }

    [TestCase(null)]
    [TestCase("")]
    public void EmptyMilestoneDoesNotChangeSave(string milestone)
    {
        Assert.IsFalse(Claim(milestone, 25, new[] { ItemName }));
        Assert.IsFalse(HasClaimed(milestone));
        Assert.AreEqual(100, Gold);
        Assert.AreEqual(100, PlayerPrefs.GetInt(GoldKey));
        Assert.IsFalse(PlayerPrefs.HasKey(MilestoneKey));
        Assert.AreEqual(0, ItemCount());
        Assert.AreEqual(0, notifications);
    }

    [Test]
    public void NegativeGoldDoesNotClaimMilestone()
    {
        Assert.IsFalse(Claim(Milestone, -1, new[] { ItemName }));
        Assert.IsFalse(HasClaimed(Milestone));
        Assert.AreEqual(100, Gold);
        Assert.AreEqual(0, ItemCount());
        Assert.AreEqual(0, notifications);
    }

    [Test]
    public void NewInventoryRemembersGoldAndClaimedMilestone()
    {
        Assert.IsTrue(Claim(Milestone, 25));
        UnityEngine.Object.DestroyImmediate(host);
        CreateInventory();
        Assert.AreEqual(125, Gold);
        Assert.IsTrue(HasClaimed(Milestone));
        Assert.IsFalse(Claim(Milestone, 25));
        Assert.AreEqual(125, Gold);
    }

    [Test]
    public void ZeroGoldRewardStillClaimsAndNotifiesOnce()
    {
        Assert.IsTrue(Claim(Milestone, 0));
        Assert.IsTrue(HasClaimed(Milestone));
        Assert.AreEqual(100, Gold);
        Assert.AreEqual(1, notifications);
    }
}
