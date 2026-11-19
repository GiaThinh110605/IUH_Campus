using System;
using System.Collections.Generic;
using System.Globalization;

namespace IUHCampus.Garden
{
    public static class GardenSave
    {
        const string Version = "v1|";

        public static string Serialize(IReadOnlyList<GardenBed> beds)
        {
            if (beds == null || beds.Count != GardenRules.BedCount)
                throw new ArgumentException("Expected six garden beds.", nameof(beds));
            var rows = new string[beds.Count];
            for (int i = 0; i < beds.Count; i++)
            {
                var bed = beds[i];
                rows[i] = FormattableString.Invariant($"{(int)bed.Stage},{bed.WateredAtUtcTicks},{(bed.Fertilized ? 1 : 0)}");
            }
            return Version + string.Join(";", rows);
        }

        public static List<GardenBed> Deserialize(string data)
        {
            if (data == null || !data.StartsWith(Version, StringComparison.Ordinal)) return EmptyBeds();
            var rows = data.Substring(Version.Length).Split(';');
            if (rows.Length != GardenRules.BedCount) return EmptyBeds();
            var beds = new List<GardenBed>(rows.Length);
            foreach (var row in rows)
            {
                var values = row.Split(',');
                if (values.Length != 3 || !int.TryParse(values[0], NumberStyles.None, CultureInfo.InvariantCulture, out int stage) ||
                    stage < 0 || stage > (int)GardenStage.Bloom ||
                    !long.TryParse(values[1], NumberStyles.None, CultureInfo.InvariantCulture, out long ticks) ||
                    ticks > DateTime.MaxValue.Ticks || (values[2] != "0" && values[2] != "1"))
                    return EmptyBeds();

                bool fertilized = values[2] == "1";
                if ((stage == (int)GardenStage.Empty || stage == (int)GardenStage.Bloom) && ticks != 0)
                    return EmptyBeds();
                if (stage == (int)GardenStage.Empty && fertilized)
                    return EmptyBeds();

                beds.Add(new GardenBed((GardenStage)stage, ticks, fertilized));
            }
            return beds;
        }

        static List<GardenBed> EmptyBeds()
        {
            var beds = new List<GardenBed>(GardenRules.BedCount);
            for (int i = 0; i < GardenRules.BedCount; i++) beds.Add(new GardenBed());
            return beds;
        }
    }
}
