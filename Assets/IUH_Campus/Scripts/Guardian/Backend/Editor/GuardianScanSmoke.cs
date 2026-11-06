using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IUHCampus.Guardian.Backend.Editor
{
    public static class GuardianScanSmoke
    {
        private static bool running;
        [Serializable] private sealed class Report
        {
            public bool dirtyBefore, dirtyAfter, passed;
            public ScanSessionResult first, second;
            public ScanFrameResult frame;
            public string error;
        }
        [MenuItem("IUH Campus/Guardian Backend/B7 Scan Smoke")]
        private static async void Run()
        {
            if (running || EditorSceneManager.GetActiveScene().isDirty || EditorApplication.isPlayingOrWillChangePlaymode) return;
            running = true;
            var config = ScriptableObject.CreateInstance<GuardianBackendConfig>();
            var report = new Report { dirtyBefore = EditorSceneManager.GetActiveScene().isDirty };
            try
            {
                var client = new GuardianApiClient(config);
                report.first = await client.StartScanSessionAsync();
                if (report.first.Outcome == ScanSessionOutcome.Created || report.first.Outcome == ScanSessionOutcome.ActiveExists)
                    report.frame = await client.ScanFrameAsync(File.ReadAllBytes("Server/tests/fixtures/laptop-stocksnap.jpg"), report.first.ScanSessionId);
                report.second = await client.StartScanSessionAsync();
                report.passed = report.frame != null && (report.frame.Outcome == ScanFrameOutcome.Infested || report.frame.Outcome == ScanFrameOutcome.Clean || report.frame.Outcome == ScanFrameOutcome.NoObject) &&
                    (report.second.Outcome == ScanSessionOutcome.ActiveExists || report.second.Outcome == ScanSessionOutcome.Cooldown);
            }
            catch (Exception error) { report.error = error.GetType().Name; }
            finally
            {
                report.dirtyAfter = EditorSceneManager.GetActiveScene().isDirty; report.passed &= !report.dirtyAfter;
                Directory.CreateDirectory("Docs/Evidence/CampusGuardian/B7");
                File.WriteAllText("Docs/Evidence/CampusGuardian/B7/smoke.json", JsonUtility.ToJson(report, true));
                Debug.Log("[IUH Guardian B7] " + JsonUtility.ToJson(report));
                UnityEngine.Object.DestroyImmediate(config); running = false;
            }
        }
    }
}
