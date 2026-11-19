using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CombatOutcomeTests
{
    [Test]
    public void RepeatedCheckInAwards35GoldOnlyOnce()
    {
        const string id = "TEST_SINGLE_CHECKIN";
        var intKeys = new[] { "IUH_CHECKIN_" + id, "IUH_Guardian_Gold", "IUH_Guardian_SwordLv",
            "IUH_Guardian_ShieldLv", "IUH_Guardian_MaxHpLv" };
        var stringKeys = new[] { "IUH_CHECKIN_DATE_" + id, "IUH_Guardian_Items" };
        var restore = new System.Collections.Generic.List<Action>();
        foreach (var key in intKeys)
        {
            bool exists = PlayerPrefs.HasKey(key);
            int value = PlayerPrefs.GetInt(key);
            restore.Add(() => { if (exists) PlayerPrefs.SetInt(key, value); else PlayerPrefs.DeleteKey(key); });
        }
        foreach (var key in stringKeys)
        {
            bool exists = PlayerPrefs.HasKey(key);
            string value = PlayerPrefs.GetString(key);
            restore.Add(() => { if (exists) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); });
        }

        var inventoryType = Type.GetType("IUHCampus.Guardian.IUHGuardianInventory, Assembly-CSharp", true);
        var activitiesType = Type.GetType("IUHCampus.DailyActivities.IUHCampusDailyActivitiesManager, Assembly-CSharp", true);
        var recordType = Type.GetType("IUHCampus.DailyActivities.CheckInRecord, Assembly-CSharp", true);
        var inventoryObject = new GameObject("CheckInInventoryTest");
        var activitiesObject = new GameObject("CheckInActivitiesTest");
        var instance = inventoryType.GetProperty("Instance");
        var previousInventory = instance.GetValue(null);
        try
        {
            inventoryObject.SetActive(false);
            activitiesObject.SetActive(false);
            var inventory = inventoryObject.AddComponent(inventoryType);
            instance.GetSetMethod(true).Invoke(null, new object[] { inventory });
            var activities = activitiesObject.AddComponent(activitiesType);
            var record = Activator.CreateInstance(recordType);
            recordType.GetField("spotId").SetValue(record, id);
            recordType.GetField("spotName").SetValue(record, "Test");
            recordType.GetField("badgeName").SetValue(record, "Test Badge");
            var field = activitiesType.GetField("m_CheckInSpots", BindingFlags.Instance | BindingFlags.NonPublic);
            ((System.Collections.IList)field.GetValue(activities)).Add(record);
            var gold = inventoryType.GetProperty("Gold");
            int before = (int)gold.GetValue(inventory);
            var trigger = activitiesType.GetMethod("TriggerCheckInSpot");
            Assert.IsTrue((bool)trigger.Invoke(activities, new object[] { id, Vector3.zero }));
            Assert.IsFalse((bool)trigger.Invoke(activities, new object[] { id, Vector3.zero }));
            Assert.AreEqual(before + 35, gold.GetValue(inventory));
        }
        finally
        {
            instance.GetSetMethod(true).Invoke(null, new[] { previousInventory });
            UnityEngine.Object.DestroyImmediate(activitiesObject);
            UnityEngine.Object.DestroyImmediate(inventoryObject);
            foreach (var action in restore) action();
            PlayerPrefs.Save();
        }
    }

    [Test]
    public void ClosingVictoryKeepsMobileControlsHiddenOnStandalone()
    {
        if (!Application.isEditor &&
            Application.platform != RuntimePlatform.WindowsPlayer &&
            Application.platform != RuntimePlatform.OSXPlayer &&
            Application.platform != RuntimePlatform.LinuxPlayer)
            Assert.Ignore("Requires desktop Play Mode.");
        if (Application.isMobilePlatform) Assert.Ignore("Mobile controls are expected on mobile.");

        var controlsType = Type.GetType("IUHCampus.UI.IUHMobileControlsUI, Assembly-CSharp", true);
        var managerType = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var controls = new GameObject("DesktopMobileControlsTest");
        var battle = new GameObject("DesktopVictoryTest");
        try
        {
            controls.SetActive(false);
            var mobile = controls.AddComponent(controlsType);
            controlsType.GetField("m_ForceShowOnPC", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mobile, false);
            controls.SetActive(true);
            Assert.IsFalse(controls.activeSelf);
            var manager = battle.AddComponent(managerType);
            managerType.GetMethod("OnCloseVictoryClicked", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
            Assert.IsFalse(controls.activeSelf);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(battle);
            UnityEngine.Object.DestroyImmediate(controls);
        }
    }

    sealed class Recorder
    {
        public int Count;
        public bool Won;

        public void Record(object encounter, bool won)
        {
            Count++;
            Won = won;
        }
    }

    [Test]
    public void FleeReportsLossExactlyOnce()
    {
        var type = Type.GetType("IUHCampus.Guardian.Match3.Match3CombatManager, Assembly-CSharp", true);
        var go = new GameObject("FleeOutcomeTest");
        var manager = go.AddComponent(type);
        var recorder = new Recorder();
        var finished = type.GetEvent("CombatFinished");
        var handler = Delegate.CreateDelegate(finished.EventHandlerType, recorder, typeof(Recorder).GetMethod("Record"));
        finished.AddEventHandler(manager, handler);
        try
        {
            type.GetMethod("StartNewCombat").Invoke(manager, null);
            type.GetMethod("Flee").Invoke(manager, null);
            type.GetMethod("Flee").Invoke(manager, null);
            Assert.AreEqual(1, recorder.Count);
            Assert.IsFalse(recorder.Won);
        }
        finally
        {
            finished.RemoveEventHandler(manager, handler);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
