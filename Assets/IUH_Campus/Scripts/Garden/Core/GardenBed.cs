using System;

namespace IUHCampus.Garden
{
    public enum GardenStage { Empty, Seed, Sprout, Bloom }

    public static class GardenRules
    {
        public const int SeedCost = 10;
        public const int FertilizerCost = 10;
        public const int HarvestGold = 30;
        public const int FertilizedHarvestGold = 45;
        public const int StageSeconds = 60;
        public const int BedCount = 6;
    }

    public sealed class GardenBed
    {
        public GardenStage Stage { get; private set; }
        public long WateredAtUtcTicks { get; private set; }
        public bool Fertilized { get; private set; }
        public bool IsWatered => WateredAtUtcTicks != 0;

        public GardenBed() { }

        public GardenBed(GardenStage stage, long wateredAtUtcTicks, bool fertilized)
        {
            Stage = stage;
            WateredAtUtcTicks = wateredAtUtcTicks;
            Fertilized = fertilized;
        }

        public bool CanPlant() => Stage == GardenStage.Empty;
        public bool CanWater() => (Stage == GardenStage.Seed || Stage == GardenStage.Sprout) && !IsWatered;
        public bool CanFertilize() => Stage != GardenStage.Empty && !Fertilized;
        public bool CanHarvest() => Stage == GardenStage.Bloom;

        public bool Plant()
        {
            if (!CanPlant()) return false;
            Stage = GardenStage.Seed;
            return true;
        }

        public bool Water(DateTime nowUtc)
        {
            if (!CanWater() || nowUtc.Ticks == 0) return false;
            WateredAtUtcTicks = nowUtc.Ticks;
            return true;
        }

        public bool Fertilize()
        {
            if (!CanFertilize()) return false;
            Fertilized = true;
            return true;
        }

        public bool Harvest(out int gold)
        {
            gold = 0;
            if (!CanHarvest()) return false;
            gold = Fertilized ? GardenRules.FertilizedHarvestGold : GardenRules.HarvestGold;
            Stage = GardenStage.Empty;
            WateredAtUtcTicks = 0;
            Fertilized = false;
            return true;
        }

        public int? SecondsRemaining(DateTime nowUtc)
        {
            if (!IsWatered) return null;
            long elapsed = Math.Max(0, nowUtc.Ticks - WateredAtUtcTicks);
            double remaining = GardenRules.StageSeconds - elapsed / (double)TimeSpan.TicksPerSecond;
            return (int)Math.Ceiling(Math.Max(0, remaining));
        }

        public bool Advance(DateTime nowUtc)
        {
            if (SecondsRemaining(nowUtc) != 0) return false;
            if (Stage != GardenStage.Seed && Stage != GardenStage.Sprout) return false;
            Stage = Stage == GardenStage.Seed ? GardenStage.Sprout : GardenStage.Bloom;
            WateredAtUtcTicks = 0;
            return true;
        }
    }
}
