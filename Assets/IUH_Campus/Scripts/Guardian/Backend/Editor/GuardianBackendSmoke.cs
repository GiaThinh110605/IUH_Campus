using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IUHCampus.Guardian.Backend.Editor
{
    public static class GuardianBackendSmoke
    {
        private static bool running;
        [Serializable] private sealed class Report
        {
            public string timestampUtc, platform, baseUrl, error;
            public bool dirtyBefore, dirtyAfter, passed;
            public HealthResult health;
            public PredictResult predict;
            public EventPostResult first, second;
            public string[] eventSchemas;
        }

        [MenuItem("IUH Campus/Guardian Backend/Smoke Test")]
        private static async void Run()
        {
            if (running) return;
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.isDirty || EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogWarning("[IUH Guardian] Smoke skipped: scene must be clean and Editor stopped."); return; }
            running = true;
            var config = ScriptableObject.CreateInstance<GuardianBackendConfig>();
            var report = new Report { timestampUtc = DateTime.UtcNow.ToString("o"), platform = Application.unityVersion + " " + Application.platform,
                dirtyBefore = scene.isDirty, baseUrl = config.BaseUrl };
            string root = Directory.GetParent(Application.dataPath).FullName;
            try
            {
                var client = new GuardianApiClient(config);
                report.health = await client.GetHealthAsync();
                report.predict = await client.PredictAsync(File.ReadAllBytes(Path.Combine(root,
                    "Server/tests/fixtures/laptop-stocksnap.jpg")), "laptop");
                var value = GuardianEventFactory.ScanConfirmed("B5_smoke", "library_intro", "laptop", .70f);
                report.first = await client.PostEventAsync(value);
                report.second = await client.PostEventAsync(value);
                report.eventSchemas = new[] { value.Json, GuardianEventFactory.CombatWon("B5_smoke", "library_intro").Json,
                    GuardianEventFactory.CombatLost("B5_smoke", "library_intro").Json,
                    GuardianEventFactory.UpgradePurchased("B5_smoke", "library_intro", "defense", 2).Json };
                report.passed = report.health.Status == GuardianRequestStatus.Success && report.health.DbOk && report.health.ModelLoaded &&
                    report.predict.Status == GuardianRequestStatus.Success && report.first.Outcome == GuardianEventOutcome.Recorded &&
                    report.second.Outcome == GuardianEventOutcome.Duplicate;
            }
            catch (Exception error) { report.error = error.GetType().Name; }
            finally
            {
                report.dirtyAfter = scene.isDirty;
                report.passed &= !report.dirtyAfter;
                string directory = Path.Combine(root, "Docs/Evidence/CampusGuardian/B5");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "smoke.json"), JsonUtility.ToJson(report, true));
                UnityEngine.Object.DestroyImmediate(config); running = false;
                if (report.passed) Debug.Log("[IUH Guardian] Backend smoke: health OK; predict OK; event Recorded -> Duplicate; scene clean. Evidence: Docs/Evidence/CampusGuardian/B5/smoke.json");
                else Debug.LogError("[IUH Guardian] Backend smoke failed; see B5/smoke.json.");
            }
        }
    }
}
