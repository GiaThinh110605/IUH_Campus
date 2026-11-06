using System;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace IUHCampus.Guardian.Backend
{
    public enum ScanSessionOutcome { Created, ActiveExists, Cooldown, LimitReached, NetworkError, ServerError, Found, SessionNotFound, SessionExpired }
    [Serializable] public sealed class ScanSessionResult
    {
        public ScanSessionOutcome Outcome;
        public string ScanSessionId, Hint, ExpiresAt;
        public int MaxMonsters, MonstersRemaining, RevealedCount, RetryAfterSeconds;
    }
    public enum ScanFrameOutcome { Infested, Clean, NoObject, SessionNotFound, SessionExpired, NetworkError, ServerError }
    [Serializable] public sealed class ScanFrameResult
    {
        public ScanFrameOutcome Outcome;
        public string Label, MonsterToken;
        public float Confidence, InferenceMs;
        public int MonstersRemaining;
    }
    public sealed partial class GuardianApiClient
    {
        public Awaitable<ScanSessionResult> StartScanSessionAsync(CancellationToken token = default) => SessionAsync(null, token);
        public Awaitable<ScanSessionResult> GetScanSessionAsync(string id, CancellationToken token = default) => SessionAsync(id ?? "", token);
        private async Awaitable<ScanSessionResult> SessionAsync(string id, CancellationToken token)
        {
            var r = await Send(id == null ? "/scan/sessions" : "/scan/sessions/" + Uri.EscapeDataString(id),
                id == null ? JsonUtility.ToJson(new StartDto { session_id = GuardianSession.Id }) : null, null, null, token);
            var result = new ScanSessionResult { Outcome = ScanSessionOutcome.ServerError };
            if (r.NetworkError || r.StatusCode == 0) { result.Outcome = ScanSessionOutcome.NetworkError; return result; }
            try
            {
                var d = JsonUtility.FromJson<SessionDto>(r.Body);
                if (d == null) return result;
                if ((id == null && r.StatusCode == 201 || id != null && r.StatusCode == 200) && !string.IsNullOrEmpty(d.scan_session_id) && !string.IsNullOrEmpty(d.expires_at))
                {
                    result.Outcome = id == null ? ScanSessionOutcome.Created : ScanSessionOutcome.Found;
                    result.ScanSessionId = d.scan_session_id; result.Hint = d.hint; result.ExpiresAt = d.expires_at;
                    result.MaxMonsters = d.max_monsters; result.MonstersRemaining = d.monsters_remaining; result.RevealedCount = d.revealed_count;
                }
                else if (r.StatusCode == 409 && d.error == "scan_session_active" && !string.IsNullOrEmpty(d.scan_session_id))
                { result.Outcome = ScanSessionOutcome.ActiveExists; result.ScanSessionId = d.scan_session_id; }
                else if (r.StatusCode == 429 && d.error == "scan_cooldown")
                {
                    result.Outcome = ScanSessionOutcome.Cooldown;
                    if (int.TryParse(r.RetryAfter, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds)) result.RetryAfterSeconds = Math.Max(0, seconds);
                    else if (DateTimeOffset.TryParse(r.RetryAfter, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                        result.RetryAfterSeconds = Math.Max(0, (int)Math.Ceiling((date - DateTimeOffset.UtcNow).TotalSeconds));
                }
                else if (r.StatusCode == 429 && d.error == "scan_limit_reached") result.Outcome = ScanSessionOutcome.LimitReached;
                else if (r.StatusCode == 404 && d.error == "scan_session_not_found") result.Outcome = ScanSessionOutcome.SessionNotFound;
                else if (r.StatusCode == 410 && d.error == "scan_session_expired") result.Outcome = ScanSessionOutcome.SessionExpired;
            }
            catch (ArgumentException) { }
            return result;
        }
        public async Awaitable<ScanFrameResult> ScanFrameAsync(byte[] jpg, string scanSessionId, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var result = new ScanFrameResult { Outcome = ScanFrameOutcome.ServerError };
            if (jpg == null || jpg.Length == 0 || string.IsNullOrEmpty(scanSessionId)) return result;
            GuardianHttpResponse r;
            try { r = await transport.SendAsync(new GuardianHttpRequest { Url = config.BaseUrl + "/predict", Jpg = jpg,
                ScanSessionId = scanSessionId, TimeoutSeconds = config.timeoutSeconds }, token); }
            catch (OperationCanceledException) when (!token.IsCancellationRequested) { result.Outcome = ScanFrameOutcome.NetworkError; return result; }
            catch (System.IO.IOException) { result.Outcome = ScanFrameOutcome.NetworkError; return result; }
            catch (System.Net.WebException) { result.Outcome = ScanFrameOutcome.NetworkError; return result; }
            if (r.NetworkError || r.StatusCode == 0) { result.Outcome = ScanFrameOutcome.NetworkError; return result; }
            try
            {
                var d = JsonUtility.FromJson<FrameDto>(r.Body);
                if (d == null) return result;
                if (r.StatusCode == 404 && d.error == "scan_session_not_found") result.Outcome = ScanFrameOutcome.SessionNotFound;
                else if (r.StatusCode == 410 && d.error == "scan_session_expired") result.Outcome = ScanFrameOutcome.SessionExpired;
                else if (r.StatusCode == 200 && d.scan != null)
                {
                    var s = d.scan;
                    if (s.result == "infested" && !string.IsNullOrEmpty(s.label) && !string.IsNullOrEmpty(s.monster_token)) result.Outcome = ScanFrameOutcome.Infested;
                    else if (s.result == "clean" && !string.IsNullOrEmpty(s.label)) result.Outcome = ScanFrameOutcome.Clean;
                    else if (s.result == "none") result.Outcome = ScanFrameOutcome.NoObject;
                    else return result;
                    result.Label = s.label; result.MonsterToken = s.monster_token; result.MonstersRemaining = s.monsters_remaining;
                    result.InferenceMs = d.inference_time_ms;
                    if (d.detections != null) foreach (var detection in d.detections)
                        if (detection.label == s.label) result.Confidence = Math.Max(result.Confidence, detection.confidence);
                }
            }
            catch (ArgumentException) { }
            return result;
        }
        #pragma warning disable CS0649
        [Serializable] private sealed class StartDto { public string session_id; }
        [Serializable] private sealed class SessionDto { public string scan_session_id, hint, expires_at, error; public int max_monsters, monsters_remaining, revealed_count; }
        [Serializable] private sealed class FrameDto { public string error; public ScanDto scan; public GuardianDetection[] detections; public float inference_time_ms; }
        [Serializable] private sealed class ScanDto { public string label, result, monster_token; public int monsters_remaining; }
        #pragma warning restore CS0649
    }
}
