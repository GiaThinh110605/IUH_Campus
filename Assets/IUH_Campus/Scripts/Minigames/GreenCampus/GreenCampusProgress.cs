using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IUHCampus.Minigames.GreenCampus
{
    public enum WasteCategory { Recyclable, Organic, Residual }

    [Serializable]
    public class GreenItemDefinition
    {
        public string id;
        public string zoneId;
        public WasteCategory category;
        public GreenItemDefinition(string id, string zoneId, WasteCategory category)
        { this.id = id; this.zoneId = zoneId; this.category = category; }
    }

    [Serializable]
    public class GreenCampusProgress
    {
        public const float RoundDuration = 240f;
        public const int ItemPoints = 20;
        public const int ZonePoints = 30;
        public const int WrongPenalty = 5;
        public List<string> sortedIds = new List<string>();
        public int wrongAttempts;
        public float secondsRemaining = RoundDuration;
        public int bestScore;
        public int completedRounds;
        public bool hasRound;
        public bool completionRecorded;

        public GreenCampusProgress Copy() => JsonUtility.FromJson<GreenCampusProgress>(JsonUtility.ToJson(this));
        public bool IsSorted(string id) => sortedIds != null && sortedIds.Contains(id);

        public int CompletedZones(IReadOnlyList<GreenItemDefinition> definitions)
        {
            return definitions.GroupBy(d => d.zoneId).Count(zone => zone.All(d => IsSorted(d.id)));
        }

        public int Score(IReadOnlyList<GreenItemDefinition> definitions)
        {
            int count = definitions.Count(d => IsSorted(d.id));
            return Math.Max(0, count * ItemPoints + CompletedZones(definitions) * ZonePoints - wrongAttempts * WrongPenalty);
        }

        public bool IsComplete(IReadOnlyList<GreenItemDefinition> definitions) =>
            definitions.Count > 0 && definitions.All(d => IsSorted(d.id));

        public bool TrySort(string id, WasteCategory bin, IReadOnlyList<GreenItemDefinition> definitions, out bool correct)
        {
            correct = false;
            if (!hasRound || secondsRemaining <= 0 || IsSorted(id)) return false;
            var item = definitions.FirstOrDefault(d => d.id == id);
            if (item == null) return false;
            correct = item.category == bin;
            if (!correct) { wrongAttempts++; return true; }
            sortedIds.Add(item.id);
            if (IsComplete(definitions) && !completionRecorded)
            {
                completionRecorded = true;
                completedRounds++;
                bestScore = Math.Max(bestScore, Score(definitions));
            }
            return true;
        }

        public void Normalize(IReadOnlyList<GreenItemDefinition> definitions)
        {
            var valid = new HashSet<string>(definitions.Select(d => d.id));
            sortedIds = (sortedIds ?? new List<string>()).Where(id => id != null && valid.Contains(id)).Distinct().ToList();
            wrongAttempts = Math.Max(0, Math.Min(wrongAttempts, 10000));
            if (float.IsNaN(secondsRemaining) || float.IsInfinity(secondsRemaining)) secondsRemaining = RoundDuration;
            secondsRemaining = Mathf.Clamp(secondsRemaining, 0, RoundDuration);
            int maximum = definitions.Count * ItemPoints + definitions.Select(d => d.zoneId).Distinct().Count() * ZonePoints;
            bestScore = Mathf.Clamp(bestScore, 0, maximum);
            completedRounds = Math.Max(0, completedRounds);
            if (!IsComplete(definitions)) completionRecorded = false;
        }

        public GreenCampusProgress NewRound() => new GreenCampusProgress
        { hasRound = true, bestScore = bestScore, completedRounds = completedRounds };
    }

    public static class GreenCampusStorage
    {
        public const string FileName = "IUH_GreenCampus_Save.json";
        public static string DirectoryOverride { get; set; }
        public static bool SimulateWriteFailure { get; set; }
        public static string SavePath => Path.Combine(string.IsNullOrEmpty(DirectoryOverride)
            ? Application.persistentDataPath : DirectoryOverride, FileName);

        public static bool TrySave(GreenCampusProgress progress)
        {
            if (progress == null || SimulateWriteFailure) return false;
            string temporary = SavePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                File.WriteAllText(temporary, JsonUtility.ToJson(progress, true));
                if (File.Exists(SavePath)) File.Replace(temporary, SavePath, null);
                else File.Move(temporary, SavePath);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            { Debug.LogWarning("[Green Campus] Không lưu được tiến trình: " + ex.Message); return false; }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        public static GreenCampusProgress Load()
        {
            try { return File.Exists(SavePath) ? JsonUtility.FromJson<GreenCampusProgress>(File.ReadAllText(SavePath)) ?? new GreenCampusProgress() : new GreenCampusProgress(); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            { Debug.LogWarning("[Green Campus] File lưu không đọc được: " + ex.Message); return new GreenCampusProgress(); }
        }
    }
}
