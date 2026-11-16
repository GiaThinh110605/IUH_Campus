using System.Collections.Generic;
using NUnit.Framework;
namespace IUHCampus.Guardian.Story.Tests
{
public class StoryProgressTests
{
static IReadOnlyList<StoryChapterDef> Chapters() => new[] { new StoryChapterDef("e", new[] { "A", "B" }), new StoryChapterDef("canteen", new[] { "C" }), new StoryChapterDef("boss", new[] { "D" }) };
static StoryProgress New() => new StoryProgress(3, Chapters());
static StoryProgress AtChapter0() { var p = New(); for (int i=0;i<3;i++) p.CompleteCheckIn(i); p.FinishIncident(); return p; }[Test] public void CheckIns_MustBeInOrder() { var p = New(); Assert.IsFalse(p.CompleteCheckIn(1)); Assert.IsTrue(p.CompleteCheckIn(0)); Assert.AreEqual(1, p.CheckInsDone); }
[Test] public void LastCheckIn_GoesToIncident() { var p = New(); p.CompleteCheckIn(0); p.CompleteCheckIn(1); p.CompleteCheckIn(2); Assert.AreEqual(StoryPhase.Incident, p.Phase); }
[Test] public void Encounters_IgnoredBeforeChapters() { var p = New(); Assert.IsFalse(p.MarkEncounterDefeated("A")); Assert.IsFalse(p.IsDefeated("A")); }
[Test] public void Encounter_FromOtherChapter_Ignored() { var p = AtChapter0(); Assert.IsFalse(p.MarkEncounterDefeated("C")); Assert.AreEqual(0, p.ChapterIndex); }
[Test] public void Chapter_CompletesOnlyWhenAllDefeated() { var p = AtChapter0(); Assert.IsFalse(p.MarkEncounterDefeated("A")); Assert.IsFalse(p.MarkEncounterDefeated("A")); Assert.IsTrue(p.MarkEncounterDefeated("B")); Assert.AreEqual(1, p.ChapterIndex); }
[Test] public void LastChapter_GoesToEnding_ThenCompleted() { var p = AtChapter0(); p.MarkEncounterDefeated("A"); p.MarkEncounterDefeated("B"); p.MarkEncounterDefeated("C"); Assert.IsTrue(p.MarkEncounterDefeated("D")); Assert.AreEqual(StoryPhase.Ending, p.Phase); p.FinishEnding(); Assert.AreEqual(StoryPhase.Completed, p.Phase); }
[Test] public void Deserialize_RestoresDefeatedIds() { var p = AtChapter0(); p.MarkEncounterDefeated("A"); var q = StoryProgress.Deserialize(p.Serialize(), 3, Chapters()); Assert.AreEqual(StoryPhase.Chapter, q.Phase); Assert.AreEqual(0, q.ChapterIndex); Assert.IsTrue(q.IsDefeated("A")); Assert.IsFalse(q.IsDefeated("B")); }
[TestCase(null)][TestCase("")][TestCase("garbage")][TestCase("v1|Chapter|99|3|")][TestCase("v9|Ending|0|0|")]
public void Deserialize_GarbageReturnsFresh(string s) { var q = StoryProgress.Deserialize(s, 3, Chapters()); Assert.AreEqual(StoryPhase.Prologue, q.Phase); Assert.AreEqual(0, q.CheckInsDone); }
[Test] public void Deserialize_DropsUnknownIds() { var q = StoryProgress.Deserialize("v1|Chapter|0|3|A,ZZZ", 3, Chapters()); Assert.IsTrue(q.IsDefeated("A")); Assert.IsFalse(q.IsDefeated("ZZZ")); }

}}
