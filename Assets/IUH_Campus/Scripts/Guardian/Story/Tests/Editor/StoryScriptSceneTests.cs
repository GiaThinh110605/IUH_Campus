using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IUHCampus.Guardian.Story.Tests
{
    public class StoryScriptSceneTests
    {
        const string ScenePath = "Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity";
        IUHStoryScript Script => AssetDatabase.LoadAssetAtPath<IUHStoryScript>("Assets/IUH_Campus/Data/Story/StoryScript_DemGlitch.asset");
        [Test] public void StoryScript_AllNamesExistInScene()
        {
            Assert.NotNull(Script);
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var names = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.name).ToArray();
                foreach (var name in Script.checkIns.Select(c => c.checkInObjectName)
                    .Concat(Script.chapters.SelectMany(c => c.encounterObjectNames))
                    .Concat(Script.aftershockZones.SelectMany(c => c.encounterObjectNames)))
                    Assert.AreEqual(1, names.Count(n => n == name), name);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
        [Test] public void StoryScript_ChapterIdsUniqueAndEncounterNamesUnique()
        {
            Assert.NotNull(Script);
            var ids = Script.chapters.Select(c => c.id).ToArray();
            var encounters = Script.chapters.SelectMany(c => c.encounterObjectNames).ToArray();
            Assert.AreEqual(ids.Length, ids.Distinct().Count());
            Assert.AreEqual(encounters.Length, encounters.Distinct().Count());
            foreach (var id in ids.Concat(encounters)) Assert.IsFalse(string.IsNullOrEmpty(id) || id.Contains(',') || id.Contains('|'));
        }
        [Test] public void StoryScript_MaterialsAssigned()
        {
            Assert.NotNull(Script);
            Assert.NotNull(Script.fogMaterial);
            Assert.NotNull(Script.beaconMaterial);
        }
    }
}
