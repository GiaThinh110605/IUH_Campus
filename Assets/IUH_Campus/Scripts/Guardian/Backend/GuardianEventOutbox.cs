using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace IUHCampus.Guardian.Backend
{
    public enum GuardianSyncStatus { Pending, Synced, Failed }

    /// <summary>Single owner on the Unity main thread; never open two writers for the same path.</summary>
    public sealed class GuardianEventOutbox
    {
        [Serializable] private sealed class Entry
        { public string id, json; public int attempts; public long nextAttemptTicks; }
        [Serializable] private sealed class Snapshot
        { public int version = 1; public List<Entry> pending = new List<Entry>(); public List<Entry> rejected = new List<Entry>(); }
        private readonly string path;
        private readonly GuardianBackendConfig config;
        private readonly GuardianApiClient client;
        private readonly Func<DateTime> utcNow;
        private Snapshot state;
        private bool flushing;
        public int PendingCount => state.pending.Count;
        public int RejectedCount => state.rejected.Count;
        public event Action<string, GuardianSyncStatus> StatusChanged;

        public GuardianEventOutbox(GuardianBackendConfig config, GuardianApiClient client, string path = null, Func<DateTime> utcNow = null)
        {
            this.config = config; this.client = client;
            this.path = path ?? Path.Combine(Application.persistentDataPath, "guardian_outbox.json");
            this.utcNow = utcNow ?? (() => DateTime.UtcNow);
            state = Load();
        }

        public void Enqueue(GuardianEvent value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (state.pending.Exists(e => e.id == value.EventId) || state.rejected.Exists(e => e.id == value.EventId)) return;
            var entry = new Entry { id = value.EventId, json = value.Json };
            state.pending.Add(entry);
            try { Save(); } catch { state.pending.Remove(entry); throw; }
            StatusChanged?.Invoke(value.EventId, GuardianSyncStatus.Pending);
        }

        public void RetryFailed()
        {
            foreach (var entry in state.pending) { entry.attempts = 0; entry.nextAttemptTicks = 0; }
            Save();
        }

        public async Awaitable FlushAsync(CancellationToken cancellationToken = default)
        {
            if (flushing) return;
            flushing = true;
            try
            {
                while (state.pending.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var entry = state.pending[0];
                    if (entry.attempts >= Math.Max(1, config.maxAttempts) || entry.nextAttemptTicks > utcNow().Ticks) return;
                    var result = await client.PostEventAsync(new GuardianEvent(entry.id, entry.json), cancellationToken);
                    Apply(entry, result);
                    if (result.Outcome == GuardianEventOutcome.NetworkError || result.Outcome == GuardianEventOutcome.ServerError) return;
                }
            }
            finally { flushing = false; }
        }

        private void Apply(Entry entry, EventPostResult result)
        {
            if (result.Outcome == GuardianEventOutcome.Recorded || result.Outcome == GuardianEventOutcome.Duplicate)
            {
                state.pending.RemoveAt(0); Save();
                StatusChanged?.Invoke(entry.id, GuardianSyncStatus.Synced); return;
            }
            if (result.Outcome == GuardianEventOutcome.Invalid || result.Outcome == GuardianEventOutcome.Conflict)
            {
                state.pending.RemoveAt(0); state.rejected.Add(entry); Save();
                Debug.LogWarning("[IUH Guardian] Backend event rejected: " + entry.id + " (" + result.Outcome + ").");
                StatusChanged?.Invoke(entry.id, GuardianSyncStatus.Failed); return;
            }
            entry.attempts++;
            double seconds = Math.Min(Math.Max(0.1, config.maxBackoffSeconds),
                Math.Max(0.1, config.retryBaseSeconds) * Math.Pow(2, Math.Min(30, entry.attempts - 1)));
            entry.nextAttemptTicks = utcNow().AddSeconds(seconds).Ticks;
            Save();
            StatusChanged?.Invoke(entry.id, entry.attempts >= Math.Max(1, config.maxAttempts) ? GuardianSyncStatus.Failed : GuardianSyncStatus.Pending);
        }

        private Snapshot Load()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            if (!File.Exists(path) && File.Exists(path + ".tmp")) File.Move(path + ".tmp", path);
            if (!File.Exists(path)) return new Snapshot();
            try
            {
                var loaded = JsonUtility.FromJson<Snapshot>(File.ReadAllText(path));
                if (loaded == null || loaded.version != 1 || loaded.pending == null || loaded.rejected == null)
                    throw new ArgumentException();
                ValidateEntries(loaded.pending); ValidateEntries(loaded.rejected);
                return loaded;
            }
            catch (ArgumentException)
            {
                File.Move(path, path + ".corrupt-" + Guid.NewGuid().ToString("N"));
                Debug.LogWarning("[IUH Guardian] Corrupt backend outbox backed up; starting an empty queue.");
                return new Snapshot();
            }
        }

        private static void ValidateEntries(List<Entry> entries)
        {
            foreach (var entry in entries)
                if (entry == null || !Guid.TryParse(entry.id, out _) || string.IsNullOrEmpty(entry.json) || entry.attempts < 0)
                    throw new ArgumentException();
        }

        private void Save()
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(state));
            using (var stream = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(path)) File.Replace(path + ".tmp", path, null);
            else File.Move(path + ".tmp", path);
        }
    }
}
