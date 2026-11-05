using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IUHCampus.Guardian.Backend.Tests
{
    public sealed class GuardianBackendTests
    {
        private GuardianBackendConfig config;
        private string directory;
        private string savedUrl;
        private bool hadUrl;
        [SetUp] public void SetUp()
        {
            hadUrl = PlayerPrefs.HasKey(GuardianBackendConfig.UrlPreference);
            savedUrl = PlayerPrefs.GetString(GuardianBackendConfig.UrlPreference);
            PlayerPrefs.DeleteKey(GuardianBackendConfig.UrlPreference);
            config = ScriptableObject.CreateInstance<GuardianBackendConfig>();
            directory = Path.Combine(Path.GetTempPath(), "guardian-b5-" + Guid.NewGuid());
            Directory.CreateDirectory(directory);
        }
        [TearDown] public void TearDown()
        {
            if (hadUrl) PlayerPrefs.SetString(GuardianBackendConfig.UrlPreference, savedUrl);
            else PlayerPrefs.DeleteKey(GuardianBackendConfig.UrlPreference);
            PlayerPrefs.Save();
            UnityEngine.Object.DestroyImmediate(config);
            Directory.Delete(directory, true);
        }
        private static IEnumerator Wait(Task task)
        { while (!task.IsCompleted) yield return null; task.GetAwaiter().GetResult(); }
        private GuardianEvent Event() => GuardianEventFactory.CombatWon("quest", "library_intro");
        private string QueuePath => Path.Combine(directory, "outbox.json");
        private static string[] Keys(string json)
        {
            var keys = new List<string>();
            foreach (Match match in Regex.Matches(json, "\"([^\"]+)\"\\s*:")) keys.Add(match.Groups[1].Value);
            return keys.ToArray();
        }

        [Test] public void EventSchemasMatchBackendWhitelists()
        {
            var cases = new[] { GuardianEventFactory.ScanConfirmed("q", "m", "book", .9f), Event(),
                GuardianEventFactory.CombatLost("q", "m"), GuardianEventFactory.UpgradePurchased("q", "m", "attack", 2) };
            var common = new[] { "event_id", "session_id", "quest_id", "milestone_id", "event_type", "occurred_at" };
            for (int i = 0; i < cases.Length; i++)
            {
                var expected = new List<string>(common);
                if (i == 0) expected.AddRange(new[] { "payload", "label", "confidence" });
                if (i == 3) expected.AddRange(new[] { "payload", "branch", "level" });
                CollectionAssert.AreEquivalent(expected, Keys(cases[i].Json));
                var header = JsonUtility.FromJson<Header>(cases[i].Json);
                Assert.That(Guid.TryParse(header.event_id, out _), Is.True);
                Assert.That(header.session_id, Is.EqualTo(GuardianSession.Id));
                Assert.That(DateTimeOffset.Parse(header.occurred_at).Offset, Is.EqualTo(TimeSpan.Zero));
            }
            Assert.That(cases[1].EventId, Is.Not.EqualTo(cases[2].EventId));
        }

        [TestCase("attack", 1)] [TestCase("defense", 2)] [TestCase("vitality", 1)]
        public void ValidUpgrade(string branch, int level) => Assert.That(GuardianEventFactory.UpgradePurchased("q", "m", branch, level), Is.Not.Null);
        [Test] public void InvalidUpgradeRejectedLocally()
        {
            Assert.Throws<ArgumentException>(() => GuardianEventFactory.UpgradePurchased("q", "m", "core", 1));
            Assert.Throws<ArgumentException>(() => GuardianEventFactory.UpgradePurchased("q", "m", "attack", 3));
        }
        [Test] public void ScanConfidenceAtThresholdDoesNotRoundBelowBackendMinimum()
        {
            string json = GuardianEventFactory.ScanConfirmed("q", "m", "book", .70f).Json;
            var match = Regex.Match(json, "\"confidence\":([0-9.Ee+-]+)");
            Assert.That(double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), Is.GreaterThanOrEqualTo(.70));
        }
        [Test] public void UrlOverrideAndValidation()
        {
            GuardianBackendConfig.SetBaseUrl("http://192.0.2.1:5001/");
            Assert.That(config.BaseUrl, Is.EqualTo("http://192.0.2.1:5001"));
            Assert.Throws<ArgumentException>(() => GuardianBackendConfig.SetBaseUrl("file:///tmp/file"));
        }

        [UnityTest] public IEnumerator ClientMapsHttpAndTimeout() => Wait(CheckMappings());
        private async Task CheckMappings()
        {
            var fake = new Fake(); var client = new GuardianApiClient(config, fake); var e = Event();
            var codes = new[] { 201, 200, 400, 409, 500, 503, 0, 408, 429 };
            var expected = new[] { GuardianEventOutcome.Recorded, GuardianEventOutcome.Duplicate, GuardianEventOutcome.Invalid,
                GuardianEventOutcome.Conflict, GuardianEventOutcome.ServerError, GuardianEventOutcome.ServerError, GuardianEventOutcome.NetworkError,
                GuardianEventOutcome.ServerError, GuardianEventOutcome.ServerError };
            for (int i = 0; i < codes.Length; i++)
            { fake.Code = codes[i]; Assert.That((await client.PostEventAsync(e)).Outcome, Is.EqualTo(expected[i])); }
            fake.Timeout = true;
            Assert.That((await client.PostEventAsync(e)).Outcome, Is.EqualTo(GuardianEventOutcome.NetworkError));
            foreach (var request in fake.Requests) Assert.That(request.Json, Is.EqualTo(e.Json));
        }

        [UnityTest] public IEnumerator PredictionAndHealthMapping() => Wait(CheckPrediction());
        private async Task CheckPrediction()
        {
            var fake = new Fake { Code = 200, Body = "{\"target_found\":true,\"best_confidence\":0.91,\"inference_time_ms\":23.4,\"detections\":[{\"label\":\"book\",\"class_id\":73,\"confidence\":0.91,\"bbox\":[1,2,3,4]}]}" };
            var client = new GuardianApiClient(config, fake);
            var prediction = await client.PredictAsync(new byte[] { 1 }, "book");
            Assert.That(prediction.TargetFound, Is.True); Assert.That(prediction.BestConfidence, Is.EqualTo(.91f).Within(.001));
            Assert.That(prediction.Detections[0].class_id, Is.EqualTo(73));
            Assert.That(prediction.InferenceMs, Is.EqualTo(23.4f));
            Assert.That(fake.Requests[0].TargetClass, Is.EqualTo("book"));
            fake.Body = "{\"target_found\":false,\"best_confidence\":null,\"detections\":[]}";
            Assert.That((await client.PredictAsync(new byte[] { 1 }, "book")).BestConfidence, Is.Null);
            fake.Body = "{\"status\":\"healthy\",\"model_loaded\":true,\"db_ok\":true}";
            Assert.That((await client.GetHealthAsync()).ModelLoaded, Is.True);
            fake.Code = 400; Assert.That((await client.PredictAsync(new byte[] { 1 }, "book")).Status, Is.EqualTo(GuardianRequestStatus.Invalid));
            fake.Code = 503; Assert.That((await client.GetHealthAsync()).Status, Is.EqualTo(GuardianRequestStatus.ServerError));
            fake.Code = 201; fake.Body = "not JSON";
            Assert.That((await client.PostEventAsync(Event())).Outcome, Is.EqualTo(GuardianEventOutcome.ServerError));
            fake.Code = 200; fake.Body = "null";
            Assert.That((await client.PredictAsync(new byte[] { 1 }, "book")).Status, Is.EqualTo(GuardianRequestStatus.ServerError));
            Assert.That((await client.GetHealthAsync()).Status, Is.EqualTo(GuardianRequestStatus.ServerError));
        }

        [UnityTest] public IEnumerator OutboxRestartBackoffAndFifo() => Wait(CheckRestart());
        private async Task CheckRestart()
        {
            var fake = new Fake { Code = 500 }; var client = new GuardianApiClient(config, fake);
            var now = DateTime.UtcNow; var first = Event(); var second = Event();
            var outbox = new GuardianEventOutbox(config, client, QueuePath, () => now);
            outbox.Enqueue(first); outbox.Enqueue(second); await outbox.FlushAsync();
            Assert.That(fake.Requests.Count, Is.EqualTo(1));
            outbox = new GuardianEventOutbox(config, client, QueuePath, () => now);
            await outbox.FlushAsync(); Assert.That(fake.Requests.Count, Is.EqualTo(1));
            now = now.AddSeconds(config.retryBaseSeconds + 1); fake.Code = 200;
            var statuses = new List<GuardianSyncStatus>(); outbox.StatusChanged += (_, s) => statuses.Add(s);
            await outbox.FlushAsync();
            Assert.That(outbox.PendingCount, Is.Zero);
            Assert.That(fake.Requests[0].Json, Is.EqualTo(fake.Requests[1].Json));
            Assert.That(fake.Requests[2].Json, Is.EqualTo(second.Json));
            CollectionAssert.AreEqual(new[] { GuardianSyncStatus.Synced, GuardianSyncStatus.Synced }, statuses);
            Assert.That(new GuardianEventOutbox(config, client, QueuePath).PendingCount, Is.Zero);
        }

        [UnityTest] public IEnumerator OutboxRejectedOnlyOnce() => Wait(CheckRejected());
        private async Task CheckRejected()
        {
            var fake = new Fake { Code = 409 }; var client = new GuardianApiClient(config, fake);
            var outbox = new GuardianEventOutbox(config, client, QueuePath);
            outbox.Enqueue(Event());
            LogAssert.Expect(LogType.Warning, new Regex("\\[IUH Guardian\\] Backend event rejected:"));
            await outbox.FlushAsync(); await outbox.FlushAsync();
            Assert.That(outbox.RejectedCount, Is.EqualTo(1)); Assert.That(outbox.PendingCount, Is.Zero);
            outbox = new GuardianEventOutbox(config, client, QueuePath);
            await outbox.FlushAsync(); Assert.That(fake.Requests.Count, Is.EqualTo(1));
            fake.Code = 400; outbox.Enqueue(Event());
            LogAssert.Expect(LogType.Warning, new Regex("\\[IUH Guardian\\] Backend event rejected:"));
            await outbox.FlushAsync(); Assert.That(outbox.RejectedCount, Is.EqualTo(2));
        }

        [UnityTest] public IEnumerator ConcurrentFlushDoesNotSendTwice() => Wait(CheckSingleFlight());
        private async Task CheckSingleFlight()
        {
            var fake = new Fake { Hold = true }; var e = Event();
            var outbox = new GuardianEventOutbox(config, new GuardianApiClient(config, fake), QueuePath);
            outbox.Enqueue(e);
            var first = AwaitFlush(outbox);
            await outbox.FlushAsync();
            Assert.That(fake.Requests.Count, Is.EqualTo(1));
            fake.Completion.SetResult(new GuardianHttpResponse { StatusCode = 201,
                Body = "{\"event_id\":\"" + e.EventId + "\",\"status\":\"recorded\",\"cloud\":\"pending\",\"core_awarded\":10}" });
            await first;
            Assert.That(outbox.PendingCount, Is.Zero);
        }
        private static async Task AwaitFlush(GuardianEventOutbox outbox) => await outbox.FlushAsync();

        [UnityTest] public IEnumerator BackoffDoublesThenCaps() => Wait(CheckBackoff());
        private async Task CheckBackoff()
        {
            config.retryBaseSeconds = 2; config.maxBackoffSeconds = 3;
            var fake = new Fake { Code = 500 }; var now = DateTime.UtcNow;
            var outbox = new GuardianEventOutbox(config, new GuardianApiClient(config, fake), QueuePath, () => now);
            outbox.Enqueue(Event()); await outbox.FlushAsync();
            now = now.AddSeconds(1); await outbox.FlushAsync(); Assert.That(fake.Requests.Count, Is.EqualTo(1));
            now = now.AddSeconds(1); await outbox.FlushAsync(); Assert.That(fake.Requests.Count, Is.EqualTo(2));
            now = now.AddSeconds(2); await outbox.FlushAsync(); Assert.That(fake.Requests.Count, Is.EqualTo(2));
            now = now.AddSeconds(1); await outbox.FlushAsync(); Assert.That(fake.Requests.Count, Is.EqualTo(3));
        }

        [UnityTest] public IEnumerator RetryCapAndCancellation() => Wait(CheckRetryCap());
        private async Task CheckRetryCap()
        {
            config.maxAttempts = 2; var fake = new Fake { Code = 0 };
            var now = DateTime.UtcNow; var client = new GuardianApiClient(config, fake);
            var outbox = new GuardianEventOutbox(config, client, QueuePath, () => now);
            var statuses = new List<GuardianSyncStatus>(); outbox.StatusChanged += (_, s) => statuses.Add(s);
            outbox.Enqueue(Event()); await outbox.FlushAsync(); now = now.AddHours(1); await outbox.FlushAsync();
            now = now.AddHours(1); await outbox.FlushAsync();
            Assert.That(fake.Requests.Count, Is.EqualTo(2)); Assert.That(statuses[statuses.Count - 1], Is.EqualTo(GuardianSyncStatus.Failed));
            Assert.That(outbox.PendingCount, Is.EqualTo(1)); outbox.RetryFailed();
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            try { await client.PostEventAsync(Event(), cancellation.Token); Assert.Fail("Cancellation was ignored"); }
            catch (OperationCanceledException) { }
            Assert.That(fake.Requests.Count, Is.EqualTo(2));
        }

        [Test] public void CorruptQueuePreservedAsBackup()
        {
            File.WriteAllText(QueuePath, "this is not JSON");
            LogAssert.Expect(LogType.Warning, "[IUH Guardian] Corrupt backend outbox backed up; starting an empty queue.");
            Assert.That(new GuardianEventOutbox(config, new GuardianApiClient(config, new Fake()), QueuePath).PendingCount, Is.Zero);
            Assert.That(Directory.GetFiles(directory, "*.corrupt-*").Length, Is.EqualTo(1));
        }

        [TestCase(1920, 1080, 640, 360)] [TestCase(1080, 1920, 360, 640)] [TestCase(320, 160, 320, 160)]
        public void EncoderDimensions(int width, int height, int x, int y) => Assert.That(GuardianImageEncoder.OutputSize(width, height, 640), Is.EqualTo(new Vector2Int(x, y)));
        [Test] public void EncoderProducesIndependentJpegAndRestoresRenderTarget()
        {
            var texture = new Texture2D(1280, 640); var previous = RenderTexture.active; var decoded = new Texture2D(2, 2);
            try
            {
                byte[] jpg = GuardianImageEncoder.Encode(texture);
                Assert.That(decoded.LoadImage(jpg), Is.True);
                Assert.That(new Vector2Int(decoded.width, decoded.height), Is.EqualTo(new Vector2Int(640, 320)));
                Assert.That(texture.width, Is.EqualTo(1280)); Assert.That(RenderTexture.active, Is.SameAs(previous));
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(decoded); }
        }

        #pragma warning disable CS0649
        [Serializable] private sealed class Header { public string event_id, session_id, occurred_at; }
        #pragma warning restore CS0649
        private sealed class Fake : IGuardianHttpTransport
        {
            public int Code = 201; public string Body; public bool Timeout, Hold;
            public AwaitableCompletionSource<GuardianHttpResponse> Completion;
            public readonly List<GuardianHttpRequest> Requests = new List<GuardianHttpRequest>();
            public Awaitable<GuardianHttpResponse> SendAsync(GuardianHttpRequest request, CancellationToken token)
            {
                Requests.Add(request);
                var source = new AwaitableCompletionSource<GuardianHttpResponse>();
                if (Hold) { Completion = source; return source.Awaitable; }
                if (Timeout) source.SetException(new OperationCanceledException());
                else
                {
                    var id = request.Json == null ? "" : JsonUtility.FromJson<Header>(request.Json).event_id;
                    source.SetResult(new GuardianHttpResponse { StatusCode = Code, NetworkError = Code == 0,
                        Body = Body ?? "{\"event_id\":\"" + id + "\",\"status\":\"" + (Code == 201 ? "recorded" : "duplicate") + "\",\"core_awarded\":10,\"cloud\":\"pending\"}" });
                }
                return source.Awaitable;
            }
        }
    }
}
