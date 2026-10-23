using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using StarterAssets;

namespace IUHCampus.Minigames.GreenCampus.Editor
{
    public static class GreenCampusTests
    {
        [Serializable]
        public class Report
        {
            public int passed;
            public int failed;
            public List<string> details = new List<string>();
            public int Total => passed + failed;
            public void Check(string name, bool condition)
            { if (condition) passed++; else failed++; details.Add((condition ? "PASS " : "FAIL ") + name); }
        }

        [MenuItem("IUH Campus/Tests/Run Green Campus Tests")]
        public static void RunMenu()
        {
            var report = Run();
            Debug.Log($"[Green Campus Tests] {report.passed}/{report.Total}\n" + string.Join("\n", report.details));
        }

        public static Report Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run in Edit Mode.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save open scenes before running isolated tests.");
            var scenes = EditorSceneManager.GetSceneManagerSetup();
            var cursor = Cursor.lockState; bool visible = Cursor.visible;
            var previousDirectory = GreenCampusStorage.DirectoryOverride;
            bool previousFailure = GreenCampusStorage.SimulateWriteFailure;
            var testDirectory = Path.Combine(Application.temporaryCachePath, "IUH_Green_Tests_" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(testDirectory);
                GreenCampusStorage.DirectoryOverride = testDirectory;
                GreenCampusStorage.SimulateWriteFailure = false;
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                return Core(testDirectory);
            }
            finally
            {
                GreenCampusStorage.DirectoryOverride = previousDirectory;
                GreenCampusStorage.SimulateWriteFailure = previousFailure;
                EditorSceneManager.RestoreSceneManagerSetup(scenes);
                Cursor.lockState = cursor; Cursor.visible = visible;
                // This path is generated above beneath temporaryCachePath, never a player save.
                if (Path.GetFullPath(testDirectory).StartsWith(Path.GetFullPath(Application.temporaryCachePath) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                { try { Directory.Delete(testDirectory, true); } catch (IOException) { } }
            }
        }

        private static Report Core(string testDirectory)
        {
            var r = new Report();
            var definitions = new List<GreenItemDefinition>();
            for (int zone = 0; zone < 3; zone++) for (int category = 0; category < 3; category++)
                definitions.Add(new GreenItemDefinition("item_" + zone + "_" + category, "zone_" + zone, (WasteCategory)category));
            var data = new GreenCampusProgress().NewRound();
            r.Check("New round has 240 seconds", data.hasRound && data.secondsRemaining == 240);
            bool correct;
            r.Check("Correct sorting is accepted", data.TrySort(definitions[0].id, WasteCategory.Recyclable, definitions, out correct) && correct);
            r.Check("First item awards 20 points", data.Score(definitions) == 20);
            r.Check("Duplicate sorting does not award points", !data.TrySort(definitions[0].id, WasteCategory.Recyclable, definitions, out correct) && data.Score(definitions) == 20);
            r.Check("Unknown item rejected", !data.TrySort("invalid", WasteCategory.Organic, definitions, out correct));
            data.TrySort(definitions[1].id, WasteCategory.Organic, definitions, out correct);
            data.TrySort(definitions[2].id, WasteCategory.Residual, definitions, out correct);
            r.Check("Clearing one zone adds 30 bonus", data.Score(definitions) == 90 && data.CompletedZones(definitions) == 1);
            foreach (var d in definitions.Skip(3)) data.TrySort(d.id, d.category, definitions, out correct);
            r.Check("Nine items and three zones award 270", data.Score(definitions) == 270 && data.IsComplete(definitions));
            r.Check("Completed round recorded once", data.completedRounds == 1 && data.bestScore == 270);
            data.TrySort(definitions[8].id, definitions[8].category, definitions, out correct);
            r.Check("Replay of final sort cannot duplicate completion", data.completedRounds == 1);
            var next = data.NewRound();
            r.Check("Restart preserves best result but resets active round", next.bestScore == 270 && next.sortedIds.Count == 0 && next.completedRounds == 1);
            next.TrySort(definitions[0].id, WasteCategory.Organic, definitions, out correct);
            r.Check("Wrong sorting penalizes without completing item", !correct && next.wrongAttempts == 1 && !next.IsSorted(definitions[0].id) && next.Score(definitions) == 0);
            next.TrySort(definitions[0].id, WasteCategory.Recyclable, definitions, out correct);
            r.Check("Wrong-attempt penalty persists", next.Score(definitions) == 15);
            next.secondsRemaining = 0;
            r.Check("No sorting after timeout", !next.TrySort(definitions[1].id, WasteCategory.Organic, definitions, out correct));
            var dirty = new GreenCampusProgress { sortedIds = new List<string> { definitions[0].id, definitions[0].id, null, "unknown" }, secondsRemaining = float.NaN, bestScore = 99999 };
            dirty.Normalize(definitions);
            r.Check("Corrupt IDs and timer normalized", dirty.sortedIds.Count == 1 && dirty.secondsRemaining == 240 && dirty.bestScore == 270);
            r.Check("Save writes to isolated storage", GreenCampusStorage.TrySave(data));
            r.Check("Save reload restores completed round", GreenCampusStorage.Load().sortedIds.Count == 9 && GreenCampusStorage.Load().bestScore == 270);
            GreenCampusStorage.SimulateWriteFailure = true;
            r.Check("Failed write preserves previous save", !GreenCampusStorage.TrySave(new GreenCampusProgress()) && GreenCampusStorage.Load().sortedIds.Count == 9);
            GreenCampusStorage.SimulateWriteFailure = false;

            GreenCampusStorage.DirectoryOverride = Path.Combine(testDirectory, "manager");
            var player = new GameObject("GreenTestPlayer"); player.tag = "Player"; player.layer = LayerMask.NameToLayer("Player");
            var controller = player.AddComponent<ThirdPersonController>();
            var root = new GameObject("GreenTestRoot");
            var manager = root.AddComponent<IUHGreenCampusManager>();
            manager.spawnPoint = Vector3.zero;
            var world = new GameObject("World"); world.transform.SetParent(root.transform); manager.gameWorld = world;
            manager.items = definitions.Select((d, i) =>
            {
                var go = new GameObject(d.id); go.transform.SetParent(world.transform); go.transform.position = new Vector3(1 + i * 0.08f, 0.15f, 0);
                var item = go.AddComponent<IUHGreenLitter>(); item.itemId = d.id; item.zoneId = d.zoneId; item.category = d.category; item.displayName = d.id;
                return item;
            }).ToArray();
            manager.bins = Enumerable.Range(0, 3).Select(i =>
            { var go = new GameObject("Bin" + i); go.transform.SetParent(world.transform); var bin = go.AddComponent<IUHGreenBin>(); bin.category = (WasteCategory)i; return bin; }).ToArray();
            manager.zones = Enumerable.Range(0, 3).Select(i => new GreenZoneView { id = "zone_" + i, cleanVisual = new GameObject("Clean" + i), dirtyVisual = new GameObject("Dirty" + i) }).ToArray();
            manager.ui = IUHGreenCampusUI.Build(root.transform);
            typeof(IUHGreenCampusManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, null);
            try
            {
                manager.OpenBriefing();
                r.Check("Briefing owns controller and cursor", manager.HasActiveModal && !controller.enabled && Cursor.visible);
                r.Check("Manager begins a playable round", manager.BeginRound(true) && manager.State == IUHGreenCampusManager.GameState.Playing && controller.enabled);
                var first = manager.items[0];
                first.transform.position = Vector3.right * 20;
                r.Check("Remote pickup rejected", !manager.TryPickup(first));
                first.ReturnHome(); Physics.SyncTransforms();
                r.Check("Nearby pickup succeeds", manager.TryPickup(first) && manager.CarriedItem == first);
                GreenCampusStorage.SimulateWriteFailure = true;
                r.Check("Failed sorting save cannot reward or consume carried item", !manager.TrySort(first.category) && manager.CurrentScore == 0 && manager.CarriedItem == first);
                GreenCampusStorage.SimulateWriteFailure = false;
                r.Check("Successful sorting commits 20 points", manager.TrySort(first.category) && manager.CurrentScore == 20 && manager.CarriedItem == null);
                r.Check("Sorted object cannot be picked again", !manager.TryPickup(first));
                var second = manager.items[1]; Physics.SyncTransforms(); manager.TryPickup(second);
                r.Check("Wrong bin retains carried object", !manager.TrySort(WasteCategory.Residual) && manager.CarriedItem == second && manager.Progress.wrongAttempts == 1);
                manager.CancelMission();
                r.Check("Cancel drops held object without losing saved sorting", manager.CarriedItem == null && second.transform.parent == world.transform && GreenCampusStorage.Load().sortedIds.Count == 1);
                manager.BeginRound();
                manager.AdvanceTime(1000);
                r.Check("Timer expiry opens defeat and blocks pickup", manager.State == IUHGreenCampusManager.GameState.Defeat && !manager.TryPickup(second));
                r.Check("Restart resets timer and items", manager.BeginRound(true) && manager.Progress.secondsRemaining == 240 && manager.SortedCount == 0);
                foreach (var item in manager.items)
                { Physics.SyncTransforms(); if (!manager.TryPickup(item) || !manager.TrySort(item.category)) throw new Exception("Manager sorting fixture failed for " + item.itemId); }
                r.Check("Production manager reaches victory with 270 points", manager.State == IUHGreenCampusManager.GameState.Victory && manager.CurrentScore == 270 && manager.CleanZoneCount == 3);
                r.Check("All restored green corners are visible", manager.zones.All(z => z.cleanVisual.activeSelf && !z.dirtyVisual.activeSelf));
                var portalGo = new GameObject("GreenTestPortal"); var portal = portalGo.AddComponent<IUHCampus.UI.IUHMiniGamePortalUI>();
                typeof(IUHCampus.UI.IUHMiniGamePortalUI).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(portal, null);
                portal.SetMenuOpen(true); portal.SetMenuOpen(false);
                r.Check("Portal resume respects Green result modal", !controller.enabled && Cursor.visible && Cursor.lockState == CursorLockMode.None);
                manager.CancelMission();
                r.Check("Closing result restores gameplay", controller.enabled && !manager.HasActiveSessionOrUI);
                UnityEngine.Object.DestroyImmediate(portalGo);
            }
            finally
            {
                GreenCampusStorage.SimulateWriteFailure = false;
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(player);
            }
            return r;
        }
    }
}
