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
            string path = id == null ? "/scan/sessions" : "/scan/sessions/" + Uri.EscapeDataString(id);
            string body = id == null ? JsonUtility.ToJson(new StartDto { session_id = GuardianSession.Id }) : null;
            var r = await Send(path, body, null, null, token);
            if (r.NetworkError || r.StatusCode == 0)
                return new ScanSessionResult { Outcome = ScanSessionOutcome.NetworkError };
            return ParseSession(r, id == null);
        }

        private static ScanSessionResult ParseSession(GuardianHttpResponse r, bool creating)
        {
            var result = new ScanSessionResult { Outcome = ScanSessionOutcome.ServerError };
            try
            {
                var d = JsonUtility.FromJson<SessionDto>(r.Body);
                if (d == null)
                    return result;

                long expectedStatus = creating ? HttpCreated : HttpOk;
                if (r.StatusCode == expectedStatus &&
                    !string.IsNullOrEmpty(d.scan_session_id) &&
                    !string.IsNullOrEmpty(d.expires_at))
                {
                    result.Outcome = creating ? ScanSessionOutcome.Created : ScanSessionOutcome.Found;
                    result.ScanSessionId = d.scan_session_id;
                    result.Hint = d.hint;
                    result.ExpiresAt = d.expires_at;
                    result.MaxMonsters = d.max_monsters;
                    result.MonstersRemaining = d.monsters_remaining;
                    result.RevealedCount = d.revealed_count;
                }
                else if (r.StatusCode == HttpConflict && d.error == "scan_session_active" && !string.IsNullOrEmpty(d.scan_session_id))
                {
                    result.Outcome = ScanSessionOutcome.ActiveExists;
                    result.ScanSessionId = d.scan_session_id;
                }
                else if (r.StatusCode == HttpTooManyRequests && d.error == "scan_cooldown")
                {
                    result.Outcome = ScanSessionOutcome.Cooldown;
                    result.RetryAfterSeconds = ParseRetryAfterSeconds(r.RetryAfter);
                }
                else if (r.StatusCode == HttpTooManyRequests && d.error == "scan_limit_reached")
                {
                    result.Outcome = ScanSessionOutcome.LimitReached;
                }
                else if (r.StatusCode == HttpNotFound && d.error == "scan_session_not_found")
                {
                    result.Outcome = ScanSessionOutcome.SessionNotFound;
                }
                else if (r.StatusCode == HttpGone && d.error == "scan_session_expired")
                {
                    result.Outcome = ScanSessionOutcome.SessionExpired;
                }
            }
            catch (ArgumentException) { }
            return result;
        }

        private static int ParseRetryAfterSeconds(string retryAfter)
        {
            if (int.TryParse(retryAfter, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                return Math.Max(0, seconds);
            if (DateTimeOffset.TryParse(retryAfter, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                return Math.Max(0, (int)Math.Ceiling((date - DateTimeOffset.UtcNow).TotalSeconds));
            return 0;
        }

        public async Awaitable<ScanFrameResult> ScanFrameAsync(byte[] jpg, string scanSessionId, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (jpg == null || jpg.Length == 0 || string.IsNullOrEmpty(scanSessionId))
                return new ScanFrameResult { Outcome = ScanFrameOutcome.ServerError };

            GuardianHttpResponse r;
            try
            {
                r = await transport.SendAsync(new GuardianHttpRequest
                {
                    Url = config.BaseUrl + "/predict",
                    Jpg = jpg,
                    ScanSessionId = scanSessionId,
                    TimeoutSeconds = config.timeoutSeconds
                }, token);
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                return new ScanFrameResult { Outcome = ScanFrameOutcome.NetworkError };
            }
            catch (System.IO.IOException)
            {
                return new ScanFrameResult { Outcome = ScanFrameOutcome.NetworkError };
            }
            catch (System.Net.WebException)
            {
                return new ScanFrameResult { Outcome = ScanFrameOutcome.NetworkError };
            }

            if (r.NetworkError || r.StatusCode == 0)
                return new ScanFrameResult { Outcome = ScanFrameOutcome.NetworkError };
            return ParseFrame(r);
        }

        private static ScanFrameResult ParseFrame(GuardianHttpResponse r)
        {
            var result = new ScanFrameResult { Outcome = ScanFrameOutcome.ServerError };
            try
            {
                var d = JsonUtility.FromJson<FrameDto>(r.Body);
                if (d == null)
                    return result;

                if (r.StatusCode == HttpNotFound && d.error == "scan_session_not_found")
                {
                    result.Outcome = ScanFrameOutcome.SessionNotFound;
                    return result;
                }
                if (r.StatusCode == HttpGone && d.error == "scan_session_expired")
                {
                    result.Outcome = ScanFrameOutcome.SessionExpired;
                    return result;
                }
                if (r.StatusCode != HttpOk || d.scan == null)
                    return result;

                var s = d.scan;
                if (s.result == "infested" && !string.IsNullOrEmpty(s.label) && !string.IsNullOrEmpty(s.monster_token))
                    result.Outcome = ScanFrameOutcome.Infested;
                else if (s.result == "clean" && !string.IsNullOrEmpty(s.label))
                    result.Outcome = ScanFrameOutcome.Clean;
                else if (s.result == "none")
                    result.Outcome = ScanFrameOutcome.NoObject;
                else
                    return result;

                result.Label = s.label;
                result.MonsterToken = s.monster_token;
                result.MonstersRemaining = s.monsters_remaining;
                result.InferenceMs = d.inference_time_ms;
                result.Confidence = BestConfidence(d.detections, s.label);
            }
            catch (ArgumentException) { }
            return result;
        }

        private static float BestConfidence(GuardianDetection[] detections, string label)
        {
            if (detections == null)
                return 0f;
            float best = 0f;
            foreach (var detection in detections)
            {
                if (detection.label == label)
                    best = Math.Max(best, detection.confidence);
            }
            return best;
        }
        #pragma warning disable CS0649
        [Serializable] private sealed class StartDto { public string session_id; }
        [Serializable] private sealed class SessionDto { public string scan_session_id, hint, expires_at, error; public int max_monsters, monsters_remaining, revealed_count; }
        [Serializable] private sealed class FrameDto { public string error; public ScanDto scan; public GuardianDetection[] detections; public float inference_time_ms; }
        [Serializable] private sealed class ScanDto { public string label, result, monster_token; public int monsters_remaining; }
        #pragma warning restore CS0649
    }
}
