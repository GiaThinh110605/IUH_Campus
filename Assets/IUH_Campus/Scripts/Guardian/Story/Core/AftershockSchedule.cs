using System;
using System.Collections.Generic;

namespace IUHCampus.Guardian.Story
{
    public sealed class AftershockSchedule
    {
        readonly Func<double> random01;
        readonly float minDelay;
        readonly float maxDelay;

        public bool IsActive { get; private set; }
        public float SecondsUntilNext { get; private set; }

        public AftershockSchedule(Func<double> random01, float firstDelay = 120f, float minDelay = 240f, float maxDelay = 360f)
        {
            this.random01 = random01 ?? throw new ArgumentNullException(nameof(random01));
            if (firstDelay < 0 || minDelay < 0 || maxDelay < minDelay)
                throw new ArgumentOutOfRangeException(nameof(firstDelay));
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
            SecondsUntilNext = firstDelay;
        }

        public bool Tick(float deltaSeconds, bool canCount)
        {
            if (!canCount || IsActive || float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds)) return false;
            SecondsUntilNext = Math.Max(0, SecondsUntilNext - Math.Max(0, deltaSeconds));
            if (SecondsUntilNext > 0) return false;
            IsActive = true;
            return true;
        }

        public void Complete()
        {
            IsActive = false;
            SecondsUntilNext = minDelay + (float)Unit(random01) * (maxDelay - minDelay);
        }

        static double Unit(Func<double> random)
        {
            double value = random();
            return double.IsNaN(value) ? 0 : Math.Max(0, Math.Min(1, value));
        }

        static int Index(Func<double> random, int count) => Math.Min(count - 1, (int)(Unit(random) * count));

        public static int PickZone(Func<double> random01, int zoneCount, int lastZone)
        {
            if (zoneCount < 1) throw new ArgumentOutOfRangeException(nameof(zoneCount));
            bool excludeLast = zoneCount > 1 && lastZone >= 0 && lastZone < zoneCount;
            int zone = Index(random01, excludeLast ? zoneCount - 1 : zoneCount);
            return excludeLast && zone >= lastZone ? zone + 1 : zone;
        }

        public static IReadOnlyList<string> PickEncounters(Func<double> random01, IReadOnlyList<string> names)
        {
            var pool = new List<string>();
            foreach (var name in names)
            {
                if (!string.IsNullOrEmpty(name) && !pool.Contains(name)) pool.Add(name);
            }
            var selected = new List<string>();
            int count = Math.Min(pool.Count, Unit(random01) < .5 ? 1 : 2);
            for (int i = 0; i < count; i++)
            {
                int index = Index(random01, pool.Count);
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return selected;
        }
    }
}
