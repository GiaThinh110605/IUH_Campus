using System;
using System.Collections.Generic;
using System.Linq;

namespace IUHCampus.Guardian.Story
{
    public enum StoryPhase { Prologue, Incident, Chapter, Ending, Completed }

    public sealed class StoryChapterDef
    {
        public string Id;
        public IReadOnlyList<string> EncounterIds;
        public StoryChapterDef(string id, IReadOnlyList<string> encounterIds)
        {
            Id = id;
            EncounterIds = encounterIds;
        }
    }

    public sealed class StoryProgress
    {
        readonly int checkInCount;
        readonly IReadOnlyList<StoryChapterDef> chapters;
        readonly HashSet<string> defeated = new HashSet<string>(StringComparer.Ordinal);
        public StoryPhase Phase { get; private set; }
        public int CheckInsDone { get; private set; }
        public int ChapterIndex { get; private set; }
        public StoryChapterDef CurrentChapter => Phase == StoryPhase.Chapter ? chapters[ChapterIndex] : null;
        public IReadOnlyCollection<string> DefeatedIds => defeated.ToList().AsReadOnly();

        public StoryProgress(int checkInCount, IReadOnlyList<StoryChapterDef> chapters)
        {
            if (checkInCount < 1) throw new ArgumentOutOfRangeException(nameof(checkInCount));
            if (chapters == null || chapters.Count == 0) throw new ArgumentException("Chapters required", nameof(chapters));
            this.checkInCount = checkInCount;
            this.chapters = chapters;
        }

        public bool CompleteCheckIn(int index)
        {
            if (Phase != StoryPhase.Prologue || index != CheckInsDone) return false;
            CheckInsDone++;
            if (CheckInsDone == checkInCount) Phase = StoryPhase.Incident;
            return true;
        }

        public void FinishIncident()
        {
            if (Phase == StoryPhase.Incident) Phase = StoryPhase.Chapter;
        }

        public bool MarkEncounterDefeated(string id)
        {
            if (CurrentChapter == null || !CurrentChapter.EncounterIds.Contains(id) || !defeated.Add(id)) return false;
            if (!CurrentChapter.EncounterIds.All(defeated.Contains)) return false;
            if (ChapterIndex == chapters.Count - 1) Phase = StoryPhase.Ending;
            else ChapterIndex++;
            return true;
        }

        public bool IsDefeated(string id) => id != null && defeated.Contains(id);
        public void FinishEnding() { if (Phase == StoryPhase.Ending) Phase = StoryPhase.Completed; }
        public string Serialize() => $"v1|{Phase}|{ChapterIndex}|{CheckInsDone}|{string.Join(",", defeated.OrderBy(x => x, StringComparer.Ordinal))}";

        public static StoryProgress Deserialize(string data, int checkInCount, IReadOnlyList<StoryChapterDef> chapters)
        {
            var fresh = new StoryProgress(checkInCount, chapters);
            if (string.IsNullOrEmpty(data)) return fresh;
            var parts = data.Split('|');
            if (parts.Length != 5 || parts[0] != "v1" ||
                !Enum.TryParse(parts[1], out StoryPhase phase) || !Enum.IsDefined(typeof(StoryPhase), phase) ||
                !int.TryParse(parts[2], out int chapter) || chapter < 0 || chapter >= chapters.Count ||
                !int.TryParse(parts[3], out int done) || done < 0 || done > checkInCount) return fresh;
            if (phase == StoryPhase.Prologue ? done >= checkInCount || chapter != 0 : done != checkInCount) return fresh;
            if (phase == StoryPhase.Incident && chapter != 0) return fresh;
            if ((phase == StoryPhase.Ending || phase == StoryPhase.Completed) && chapter != chapters.Count - 1) return fresh;
            fresh.Phase = phase;
            fresh.ChapterIndex = chapter;
            fresh.CheckInsDone = done;
            var known = new HashSet<string>(chapters.SelectMany(c => c.EncounterIds), StringComparer.Ordinal);
            foreach (var id in parts[4].Split(',')) if (known.Contains(id)) fresh.defeated.Add(id);
            return fresh;
        }
    }
}
