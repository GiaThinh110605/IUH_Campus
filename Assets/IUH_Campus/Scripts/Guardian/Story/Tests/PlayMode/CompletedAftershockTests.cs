using System;
using System.Collections;
using System.Reflection;
using IUHCampus.Guardian.Story;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CompletedAftershockTests
{
    [UnityTest]
    public IEnumerator CompletedSaveKeepsMonstersLockedAndToysOn()
    {
        const string key = "IUH_Story_Progress";
        bool hadSave = PlayerPrefs.HasKey(key);
        string saved = PlayerPrefs.GetString(key);
        var roots = new System.Collections.Generic.List<GameObject>();
        var script = ScriptableObject.CreateInstance<IUHStoryScript>();
        script.checkIns = new[] { new IUHStoryScript.CheckInStep() };
        script.chapters = new[] { new IUHStoryScript.Chapter { id = "test", encounterObjectNames = new[] { "TestEncounter" } } };
        try
        {
            var encounter = new GameObject("TestEncounter");
            roots.Add(encounter);
            encounter.AddComponent(Type.GetType("IUHCampus.Guardian.IUHMonsterEncounter, Assembly-CSharp", true));
            var spawner = new GameObject("TestSpawner");
            roots.Add(spawner);
            spawner.AddComponent(Type.GetType("IUHCampus.Combat.IUHBuildingMonsterSpawner, Assembly-CSharp", true));
            var toys = new GameObject("TestCombatToys");
            roots.Add(toys);
            var combat = (Behaviour)toys.AddComponent(Type.GetType("IUHCampus.Combat.IUHCampusCombatManager, Assembly-CSharp", true));
            var balloon = new GameObject("IUH_HOT_AIR_BALLOON");
            roots.Add(balloon);
            var directorObject = new GameObject("TestStoryDirector");
            roots.Add(directorObject);
            directorObject.SetActive(false);
            var type = Type.GetType("IUHCampus.Guardian.Story.IUHStoryDirector, Assembly-CSharp", true);
            var director = directorObject.AddComponent(type);
            type.GetField("m_StoryScript", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(director, script);
            PlayerPrefs.SetString(key, "v1|Completed|0|1|TestEncounter");
            directorObject.SetActive(true);
            yield return null;
            Assert.IsFalse(encounter.activeSelf);
            Assert.IsFalse(spawner.activeSelf);
            Assert.IsTrue(combat.enabled);
            Assert.IsTrue(balloon.activeSelf);
        }
        finally
        {
            foreach (var go in roots) UnityEngine.Object.DestroyImmediate(go);
            var uiType = Type.GetType("IUHCampus.Guardian.Story.IUHStoryDialogueUI, Assembly-CSharp", true);
            foreach (var ui in UnityEngine.Object.FindObjectsByType(uiType, FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(((Component)ui).gameObject);
            var beaconType = Type.GetType("IUHCampus.Guardian.Story.IUHStoryBeacon, Assembly-CSharp", true);
            foreach (var beacon in UnityEngine.Object.FindObjectsByType(beaconType, FindObjectsInactive.Include, FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(((Component)beacon).gameObject);
            UnityEngine.Object.DestroyImmediate(script);
            if (hadSave) PlayerPrefs.SetString(key, saved);
            else PlayerPrefs.DeleteKey(key);
        }
    }
}
