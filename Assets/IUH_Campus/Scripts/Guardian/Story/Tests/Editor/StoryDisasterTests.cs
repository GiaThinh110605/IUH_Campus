using IUHCampus.Guardian.Story;
using NUnit.Framework;

public sealed class StoryDisasterTests
{
    [TestCase(StoryPhase.Prologue, false)]
    [TestCase(StoryPhase.Incident, true)]
    [TestCase(StoryPhase.Chapter, true)]
    [TestCase(StoryPhase.Ending, true)]
    [TestCase(StoryPhase.Completed, false)]
    public void DisasterAtStartMatchesPhase(StoryPhase phase, bool expected)
    {
        Assert.That(StoryDisaster.IsDisasterAtStart(phase), Is.EqualTo(expected));
    }
}
