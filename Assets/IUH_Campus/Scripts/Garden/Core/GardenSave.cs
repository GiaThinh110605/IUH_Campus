using System;
using System.Collections.Generic;
using System.Globalization;

namespace IUHCampus.Garden
{
    // "v3|id=ticks;id=ticks". Ids are generated from flowerbed positions and never contain '=' or ';'.
    public static class GardenSave
    {
        const string Version = "v3|";

        public static string Serialize(IEnumerable<WateringSpot> spots)
        {
            if (spots == null) throw new ArgumentNullException(nameof(spots));
            var rows = new List<string>();
            foreach (var spot in spots)
            {
                if (spot.Id.IndexOf('=') >= 0 || spot.Id.IndexOf(';') >= 0)
                    throw new ArgumentException("Invalid spot id: " + spot.Id, nameof(spots));
                rows.Add(spot.Id + "=" + spot.LastWateredUtcTicks.ToString(CultureInfo.InvariantCulture));
            }
            return Version + string.Join(";", rows);
        }

        // Unknown, corrupt or old (v2 planter) data yields an empty map: every bed is ready to water.
        public static Dictionary<string, long> Deserialize(string data)
        {
            var result = new Dictionary<string, long>(StringComparer.Ordinal);
            if (data == null || !data.StartsWith(Version, StringComparison.Ordinal)) return result;
            var body = data.Substring(Version.Length);
            if (body.Length == 0) return result;
            foreach (var row in body.Split(';'))
            {
                int split = row.IndexOf('=');
                if (split <= 0 || !long.TryParse(row.Substring(split + 1), NumberStyles.None, CultureInfo.InvariantCulture, out long ticks)
                    || ticks > DateTime.MaxValue.Ticks)
                    return new Dictionary<string, long>(StringComparer.Ordinal);
                result[row.Substring(0, split)] = ticks;
            }
            return result;
        }
    }
}
