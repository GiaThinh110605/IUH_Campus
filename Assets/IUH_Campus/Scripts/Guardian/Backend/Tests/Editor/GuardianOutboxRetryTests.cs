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
    public class GuardianOutboxRetryTests
    {
        private GuardianBackendConfig config;
        private string directory;
        private string QueuePath => Path.Combine(directory, "outbox.json");

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GuardianBackendConfig>();
            directory = Path.Combine(Path.GetTempPath(), "guardian-outbox-retry-" + Guid.NewGuid());
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(config);
            Directory.Delete(directory, true);
        }

        private static IEnumerator Wait(Task task)
        {
            while (!task.IsCompleted) yield return null;
            task.GetAwaiter().GetResult();
        }

        [UnityTest]
        public IEnumerator TenNetworkErrorsStayPendingThenDrainFifo() => Wait(CheckNetworkRecovery());

        private async Task CheckNetworkRecovery()
        {
            var fake = new Fake { Code = 0 };
            var now = DateTime.UtcNow;
            var first = GuardianEventFactory.CombatWon("quest", "library_intro");
            var second = GuardianEventFactory.CombatLost("quest", "library_intro");
            var box = new GuardianEventOutbox(config, new GuardianApiClient(config, fake), QueuePath, () => now);
            var statuses = new List<GuardianSyncStatus>();
            var synced = new List<string>();
            box.StatusChanged += (id, status) => { statuses.Add(status); if (status == GuardianSyncStatus.Synced) synced.Add(id); };
            box.Enqueue(first);
            box.Enqueue(second);
            for (int i = 0; i < 10; i++)
            {
                await box.FlushAsync();
                Assert.That(fake.Requests.Count, Is.EqualTo(i + 1));
                Assert.That(box.PendingCount, Is.EqualTo(2));
                Assert.That(box.RejectedCount, Is.Zero);
                Assert.That(statuses[statuses.Count - 1], Is.EqualTo(GuardianSyncStatus.Pending));
                now = now.AddSeconds(config.maxBackoffSeconds + 1);
            }
            Assert.That(statuses, Has.No.Member(GuardianSyncStatus.Failed));
            fake.Code = 201;
            await box.FlushAsync();
            Assert.That(box.PendingCount, Is.Zero);
            Assert.That(fake.Requests.Count, Is.EqualTo(12));
            Assert.That(fake.Requests[10].Json, Is.EqualTo(first.Json));
            Assert.That(fake.Requests[11].Json, Is.EqualTo(second.Json));
            CollectionAssert.AreEqual(new[] { first.EventId, second.EventId }, synced);
        }

        [UnityTest]
        public IEnumerator ServerRetriesKeepPendingAndBackoffCapped() => Wait(CheckServerBackoff());

        private async Task CheckServerBackoff()
        {
            config.retryBaseSeconds = 2;
            config.maxBackoffSeconds = 3;
            var fake = new Fake { Code = 503 };
            var now = DateTime.UtcNow;
            var box = new GuardianEventOutbox(config, new GuardianApiClient(config, fake), QueuePath, () => now);
            var statuses = new List<GuardianSyncStatus>();
            box.StatusChanged += (_, status) => statuses.Add(status);
            box.Enqueue(GuardianEventFactory.CombatWon("quest", "library_intro"));
            for (int i = 0; i < 10; i++)
            {
                await box.FlushAsync();
                Assert.That(fake.Requests.Count, Is.EqualTo(i + 1));
                Assert.That(statuses[statuses.Count - 1], Is.EqualTo(GuardianSyncStatus.Pending));
                double delay = i == 0 ? 2 : 3;
                now = now.AddSeconds(delay - .01);
                await box.FlushAsync();
                Assert.That(fake.Requests.Count, Is.EqualTo(i + 1), "Retry must wait until due.");
                now = now.AddSeconds(.01);
            }
            fake.Code = 201;
            await box.FlushAsync();
            Assert.That(box.PendingCount, Is.Zero, "Retry must be due at the configured cap.");
            Assert.That(statuses, Has.No.Member(GuardianSyncStatus.Failed));
        }

        [UnityTest]
        public IEnumerator LegacyAttemptEightSnapshotCanDrain() => Wait(CheckLegacySnapshot());

        private async Task CheckLegacySnapshot()
        {
            var first = GuardianEventFactory.CombatWon("quest", "library_intro");
            var second = GuardianEventFactory.CombatLost("quest", "library_intro");
            var snapshot = new Snapshot
            {
                pending = new List<Entry>
                {
                    new Entry { id = first.EventId, json = first.Json, attempts = 8, nextAttemptTicks = 0 },
                    new Entry { id = second.EventId, json = second.Json, attempts = 0, nextAttemptTicks = 0 }
                }
            };
            File.WriteAllText(QueuePath, JsonUtility.ToJson(snapshot));
            var fake = new Fake();
            var box = new GuardianEventOutbox(config, new GuardianApiClient(config, fake), QueuePath);
            await box.FlushAsync();
            Assert.That(fake.Requests.Count, Is.EqualTo(2));
            Assert.That(box.PendingCount, Is.Zero);
            Assert.That(fake.Requests[0].Json, Is.EqualTo(first.Json));
            Assert.That(fake.Requests[1].Json, Is.EqualTo(second.Json));
        }

        [UnityTest]
        public IEnumerator InvalidHeadIsRejectedAndNextEventSendsInSameFlush() => Wait(CheckRejectedHead());

        private async Task CheckRejectedHead()
        {
            var fake = new Fake();
            fake.Codes.Enqueue(400);
            fake.Codes.Enqueue(201);
            var box = new GuardianEventOutbox(config, new GuardianApiClient(config, fake), QueuePath);
            box.Enqueue(GuardianEventFactory.CombatWon("quest", "library_intro"));
            box.Enqueue(GuardianEventFactory.CombatLost("quest", "library_intro"));
            var statuses = new List<GuardianSyncStatus>();
            box.StatusChanged += (_, status) => statuses.Add(status);
            LogAssert.Expect(LogType.Warning, new Regex("\\[IUH Guardian\\] Backend event rejected:"));
            await box.FlushAsync();
            Assert.That(box.RejectedCount, Is.EqualTo(1));
            Assert.That(box.PendingCount, Is.Zero);
            Assert.That(fake.Requests.Count, Is.EqualTo(2));
            CollectionAssert.AreEqual(new[] { GuardianSyncStatus.Failed, GuardianSyncStatus.Synced }, statuses);
        }

        [Serializable] private sealed class Snapshot
        {
            public int version = 1;
            public List<Entry> pending = new List<Entry>();
            public List<Entry> rejected = new List<Entry>();
        }
        [Serializable] private sealed class Entry
        {
            public string id, json;
            public int attempts;
            public long nextAttemptTicks;
        }
        #pragma warning disable CS0649
        [Serializable] private sealed class Header { public string event_id; }
        #pragma warning restore CS0649

        private sealed class Fake : IGuardianHttpTransport
        {
            public int Code = 201;
            public readonly Queue<int> Codes = new Queue<int>();
            public readonly List<GuardianHttpRequest> Requests = new List<GuardianHttpRequest>();

            public Awaitable<GuardianHttpResponse> SendAsync(GuardianHttpRequest request, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                Requests.Add(request);
                int code = Codes.Count > 0 ? Codes.Dequeue() : Code;
                string id = JsonUtility.FromJson<Header>(request.Json).event_id;
                var completion = new AwaitableCompletionSource<GuardianHttpResponse>();
                completion.SetResult(new GuardianHttpResponse
                {
                    StatusCode = code,
                    NetworkError = code == 0,
                    Body = "{\"event_id\":\"" + id + "\",\"status\":\"recorded\",\"core_awarded\":0,\"cloud\":\"pending\"}"
                });
                return completion.Awaitable;
            }
        }
    }
}
