using System;

namespace IUHCampus.Guardian.Backend
{
    public enum GuardianRequestStatus { Success, Invalid, Conflict, NetworkError, ServerError }
    public enum GuardianEventOutcome { Recorded, Duplicate, Invalid, Conflict, NetworkError, ServerError }
    [Serializable] public sealed class GuardianDetection
    {
        public string label;
        public int class_id;
        public float confidence;
        public float[] bbox;
    }
    [Serializable] public sealed class PredictResult
    {
        public bool TargetFound;
        public float? BestConfidence { get; internal set; }
        public GuardianDetection[] Detections = Array.Empty<GuardianDetection>();
        public float InferenceMs;
        public GuardianRequestStatus Status;
        public long HttpStatus;
        public string Error;
    }
    [Serializable] public sealed class EventPostResult
    {
        public GuardianEventOutcome Outcome;
        public int CoreAwarded;
        public string Cloud, Error, Field;
        public long HttpStatus;
    }
    [Serializable] public sealed class HealthResult
    {
        public GuardianRequestStatus Status;
        public bool ModelLoaded, DbOk, CloudConfigured;
        public int OutboxPending, OutboxFailed;
        public string Error;
        public long HttpStatus;
    }
}
