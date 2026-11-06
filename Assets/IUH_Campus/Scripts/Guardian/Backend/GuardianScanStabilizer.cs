using System;
namespace IUHCampus.Guardian.Backend
{
    // Supply a monotonic clock (for example Time.realtimeSinceStartupAsDouble).
    public sealed class GuardianScanStabilizer
    {
        private readonly int frames;
        private readonly double hold, interval;
        private string label;
        private ScanFrameOutcome outcome;
        private int count;
        private double started, next;
        private bool busy, confirmed;
        public float Progress { get; private set; }
        public GuardianScanStabilizer(int requiredFrames = 2, double holdSeconds = 1.5, double frameInterval = .5)
        {
            if (requiredFrames < 1 || double.IsNaN(holdSeconds) || double.IsInfinity(holdSeconds) || holdSeconds < 0 ||
                double.IsNaN(frameInterval) || double.IsInfinity(frameInterval) || frameInterval < 0) throw new ArgumentException();
            frames = requiredFrames; hold = holdSeconds; interval = frameInterval;
        }
        public bool TryBeginFrame(double now)
        { if (busy || now < next) return false; busy = true; next = now + interval; return true; }
        public void EndFrame() => busy = false;
        public void Reset() { label = null; count = 0; confirmed = false; Progress = 0; }
        public bool Observe(ScanFrameResult value, double now)
        {
            if (value == null || string.IsNullOrEmpty(value.Label) ||
                (value.Outcome != ScanFrameOutcome.Infested && value.Outcome != ScanFrameOutcome.Clean)) { Reset(); return false; }
            if (label != value.Label || outcome != value.Outcome) { Reset(); label = value.Label; outcome = value.Outcome; started = now; }
            count++;
            Progress = (float)Math.Min(1, Math.Min((double)count / frames, hold == 0 ? 1 : Math.Max(0, now - started) / hold));
            if (Progress < 1 || confirmed) return false;
            confirmed = true; return true;
        }
    }
}
