using System;
using System.Threading;
using UnityEngine;

namespace IUHCampus.Guardian.Backend
{
    public sealed partial class GuardianApiClient
    {
        private readonly GuardianBackendConfig config;
        private readonly IGuardianHttpTransport transport;
        public GuardianApiClient(GuardianBackendConfig config, IGuardianHttpTransport transport = null)
        { this.config = config; this.transport = transport ?? new UnityWebRequestTransport(); }

        private async Awaitable<GuardianHttpResponse> Send(string path, string json, byte[] jpg, string target, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try { return await transport.SendAsync(new GuardianHttpRequest { Url = config.BaseUrl + path,
                Json = json, Jpg = jpg, TargetClass = target, TimeoutSeconds = config.timeoutSeconds }, token); }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            { return new GuardianHttpResponse { NetworkError = true }; }
            catch (System.IO.IOException) { return new GuardianHttpResponse { NetworkError = true }; }
            catch (System.Net.WebException) { return new GuardianHttpResponse { NetworkError = true }; }
        }

        private static GuardianRequestStatus Status(GuardianHttpResponse r) => r.NetworkError || r.StatusCode == 0 ?
            GuardianRequestStatus.NetworkError : r.StatusCode >= 500 || r.StatusCode == 408 || r.StatusCode == 429 ? GuardianRequestStatus.ServerError :
            r.StatusCode == 409 ? GuardianRequestStatus.Conflict : r.StatusCode >= 200 && r.StatusCode < 300 ?
            GuardianRequestStatus.Success : GuardianRequestStatus.Invalid;

        public async Awaitable<HealthResult> GetHealthAsync(CancellationToken cancellationToken = default)
        {
            var r = await Send("/health", null, null, null, cancellationToken);
            var result = new HealthResult { Status = Status(r), HttpStatus = r.StatusCode };
            if (result.Status != GuardianRequestStatus.Success) return result;
            try { var dto = JsonUtility.FromJson<HealthDto>(r.Body);
                if (dto == null || (dto.status != "healthy" && dto.status != "degraded")) throw new ArgumentException();
                result.ModelLoaded = dto.model_loaded;
                result.DbOk = dto.db_ok; result.CloudConfigured = dto.cloud_configured;
                result.OutboxPending = dto.outbox_pending; result.OutboxFailed = dto.outbox_failed; }
            catch (ArgumentException) { result.Status = GuardianRequestStatus.ServerError; result.Error = "invalid_response"; }
            return result;
        }

        public async Awaitable<PredictResult> PredictAsync(byte[] jpg, string targetClass, CancellationToken cancellationToken = default)
        {
            if (jpg == null || jpg.Length == 0) return new PredictResult { Status = GuardianRequestStatus.Invalid, Error = "image_required" };
            var r = await Send("/predict", null, jpg, targetClass, cancellationToken);
            var result = new PredictResult { Status = Status(r), HttpStatus = r.StatusCode };
            if (result.Status != GuardianRequestStatus.Success) { result.Error = Error(r.Body).error; return result; }
            try
            {
                var dto = JsonUtility.FromJson<PredictDto>(r.Body);
                if (dto == null || dto.detections == null) throw new ArgumentException();
                result.TargetFound = dto.target_found; result.InferenceMs = dto.inference_time_ms;
                result.Detections = dto.detections ?? Array.Empty<GuardianDetection>();
                // JsonUtility cannot represent nullable numbers; derive from the same matching detections as the backend.
                foreach (var detection in result.Detections)
                    if (targetClass == null || detection.label == targetClass)
                        result.BestConfidence = Math.Max(result.BestConfidence ?? 0, detection.confidence);
            }
            catch (ArgumentException) { result.Status = GuardianRequestStatus.ServerError; result.Error = "invalid_response"; }
            return result;
        }

        public async Awaitable<EventPostResult> PostEventAsync(GuardianEvent value, CancellationToken cancellationToken = default)
        {
            if (value == null) return new EventPostResult { Outcome = GuardianEventOutcome.Invalid, Error = "event_required" };
            var r = await Send("/gameplay/events", value.Json, null, null, cancellationToken);
            var error = Error(r.Body);
            var result = new EventPostResult { HttpStatus = r.StatusCode, Error = error.error, Field = error.field };
            var status = Status(r);
            if (status != GuardianRequestStatus.Success)
            { result.Outcome = status == GuardianRequestStatus.NetworkError ? GuardianEventOutcome.NetworkError :
                status == GuardianRequestStatus.ServerError ? GuardianEventOutcome.ServerError :
                status == GuardianRequestStatus.Conflict ? GuardianEventOutcome.Conflict : GuardianEventOutcome.Invalid; return result; }
            try
            {
                var dto = JsonUtility.FromJson<EventDto>(r.Body);
                if (dto == null || dto.event_id != value.EventId || (r.StatusCode == 201 ? dto.status != "recorded" :
                    r.StatusCode != 200 || dto.status != "duplicate")) throw new ArgumentException();
                result.Outcome = dto.status == "recorded" ? GuardianEventOutcome.Recorded : GuardianEventOutcome.Duplicate;
                result.CoreAwarded = dto.core_awarded; result.Cloud = dto.cloud;
            }
            catch (ArgumentException) { result.Outcome = GuardianEventOutcome.ServerError; result.Error = "invalid_response"; }
            return result;
        }

        private static ErrorDto Error(string body)
        { try { return string.IsNullOrEmpty(body) ? new ErrorDto() : JsonUtility.FromJson<ErrorDto>(body) ?? new ErrorDto(); }
          catch (ArgumentException) { return new ErrorDto(); } }
        // Wire DTO fields intentionally retain the server's snake_case names.
        #pragma warning disable CS0649
        [Serializable] private sealed class HealthDto { public string status; public bool model_loaded, db_ok, cloud_configured; public int outbox_pending, outbox_failed; }
        [Serializable] private sealed class PredictDto { public bool target_found; public float inference_time_ms; public GuardianDetection[] detections; }
        [Serializable] private sealed class EventDto { public string event_id, status, cloud; public int core_awarded; }
        [Serializable] private sealed class ErrorDto { public string error, field; }
        #pragma warning restore CS0649
    }
}
