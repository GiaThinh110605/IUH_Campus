using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace IUHCampus.Guardian.Backend
{
    public interface IGuardianScanService
    {
        Awaitable<ScanSessionResult> StartScanSessionAsync(CancellationToken token = default);
        Awaitable<ScanSessionResult> GetScanSessionAsync(string id, CancellationToken token = default);
        Awaitable<ScanFrameResult> ScanFrameAsync(byte[] jpg, string id, CancellationToken token = default);
    }
    public sealed class BackendScanService : IGuardianScanService
    {
        private readonly GuardianApiClient client;
        public BackendScanService(GuardianApiClient client) { this.client = client; }
        public Awaitable<ScanSessionResult> StartScanSessionAsync(CancellationToken token = default) => client.StartScanSessionAsync(token);
        public Awaitable<ScanSessionResult> GetScanSessionAsync(string id, CancellationToken token = default) => client.GetScanSessionAsync(id, token);
        public Awaitable<ScanFrameResult> ScanFrameAsync(byte[] jpg, string id, CancellationToken token = default) => client.ScanFrameAsync(jpg, id, token);
    }
    public sealed class FakeScanService : IGuardianScanService
    {
        private readonly HashSet<string> infected;
        private readonly Queue<string> labels;
        private readonly Dictionary<string, string> tokens = new Dictionary<string, string>();
        private readonly string hint;
        private string id, expires;
        public FakeScanService(IEnumerable<string> infectedClasses, IEnumerable<string> simulatedLabels, string hint = "Fake scan")
        {
            if (!Application.isEditor && !Debug.isDebugBuild) throw new InvalidOperationException("Fake scan requires Editor or Development build");
            infected = new HashSet<string>(infectedClasses); labels = new Queue<string>(simulatedLabels); this.hint = hint;
        }
        private static Awaitable<T> Done<T>(T value) { var source = new AwaitableCompletionSource<T>(); source.SetResult(value); return source.Awaitable; }
        public Awaitable<ScanSessionResult> StartScanSessionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (id != null) return Done(new ScanSessionResult { Outcome = ScanSessionOutcome.ActiveExists, ScanSessionId = id });
            id = Guid.NewGuid().ToString(); expires = DateTime.UtcNow.AddMinutes(15).ToString("o");
            return Done(new ScanSessionResult { Outcome = ScanSessionOutcome.Created, ScanSessionId = id, Hint = hint, MaxMonsters = infected.Count, ExpiresAt = expires });
        }
        public Awaitable<ScanSessionResult> GetScanSessionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Done(new ScanSessionResult { Outcome = id != null && value == id ? ScanSessionOutcome.Found : ScanSessionOutcome.SessionNotFound,
                ScanSessionId = value, Hint = hint, ExpiresAt = expires, MonstersRemaining = infected.Count - tokens.Count, RevealedCount = tokens.Count });
        }
        public Awaitable<ScanFrameResult> ScanFrameAsync(byte[] jpg, string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (id == null || value != id) return Done(new ScanFrameResult { Outcome = ScanFrameOutcome.SessionNotFound });
            string label = labels.Count == 0 ? null : labels.Dequeue();
            bool infested = label != null && infected.Contains(label);
            if (infested && !tokens.ContainsKey(label)) tokens[label] = Guid.NewGuid().ToString("N");
            return Done(new ScanFrameResult { Outcome = label == null ? ScanFrameOutcome.NoObject : infested ? ScanFrameOutcome.Infested : ScanFrameOutcome.Clean,
                Label = label, Confidence = label == null ? 0 : .95f, MonsterToken = infested ? tokens[label] : null, MonstersRemaining = infected.Count - tokens.Count });
        }
    }
}
