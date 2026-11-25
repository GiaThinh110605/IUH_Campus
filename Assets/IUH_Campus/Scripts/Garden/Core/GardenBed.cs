using System;

namespace IUHCampus.Garden
{
    public static class GardenRules
    {
        public const int WaterGold = 10;
        public const int CooldownSeconds = 90;
    }

    // One campus flowerbed the player can water. Watering pays gold, then the bed rests for CooldownSeconds.
    public sealed class WateringSpot
    {
        public string Id { get; }
        public long LastWateredUtcTicks { get; private set; }

        public WateringSpot(string id, long lastWateredUtcTicks = 0)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            LastWateredUtcTicks = Math.Max(0, lastWateredUtcTicks);
        }

        public int SecondsUntilReady(DateTime nowUtc)
        {
            if (LastWateredUtcTicks == 0) return 0;
            double elapsed = Math.Max(0, nowUtc.Ticks - LastWateredUtcTicks) / (double)TimeSpan.TicksPerSecond;
            return (int)Math.Ceiling(Math.Max(0, GardenRules.CooldownSeconds - elapsed));
        }

        public bool CanWater(DateTime nowUtc) => nowUtc.Ticks > 0 && SecondsUntilReady(nowUtc) == 0;

        public bool Water(DateTime nowUtc, out int gold)
        {
            gold = 0;
            if (!CanWater(nowUtc)) return false;
            LastWateredUtcTicks = nowUtc.Ticks;
            gold = GardenRules.WaterGold;
            return true;
        }
    }
}
