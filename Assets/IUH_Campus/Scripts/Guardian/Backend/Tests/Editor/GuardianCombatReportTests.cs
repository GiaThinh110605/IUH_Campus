using NUnit.Framework;

namespace IUHCampus.Guardian.Backend.Tests
{
    public class GuardianCombatReportTests
    {
        private static GuardianEvent Create(bool won, string quest, string milestone)
        {
            return GuardianCombatReport.Create(won, quest, milestone);
        }

        [TestCase(true, "combat_won")]
        [TestCase(false, "combat_lost")]
        public void OutcomeCreatesEventWithoutRewardPayload(bool won, string kind)
        {
            var result = Create(won, "campus", "campus_d");
            Assert.That(result.Json, Does.Contain("\"event_type\":\"" + kind + "\""));
            Assert.That(result.Json, Does.Contain("\"quest_id\":\"campus\""));
            Assert.That(result.Json, Does.Contain("\"milestone_id\":\"campus_d\""));
            Assert.That(result.Json, Does.Not.Contain("payload"));
            Assert.That(result.Json, Does.Not.Contain("gold").IgnoreCase);
            Assert.That(result.Json, Does.Not.Contain("core_awarded"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("campus d")]
        [TestCase("campus/d")]
        [TestCase("campus_d\n")]
        [TestCase("khu_đ")]
        public void InvalidMilestoneReturnsNull(string milestone)
        {
            Assert.That(Create(true, "campus", milestone), Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("invalid quest")]
        [TestCase("campus\n")]
        public void InvalidQuestReturnsNull(string quest)
        {
            Assert.That(Create(false, quest, "campus_d"), Is.Null);
        }

        [Test]
        public void IdentifierLengthBoundariesAreEnforced()
        {
            Assert.That(Create(true, new string('a', 64), "A_1-"), Is.Not.Null);
            Assert.That(Create(true, new string('a', 65), "campus_d"), Is.Null);
            Assert.That(Create(false, "campus", new string('b', 64)), Is.Not.Null);
            Assert.That(Create(false, "campus", new string('b', 65)), Is.Null);
        }

        [Test]
        public void EachCallCreatesANewEventId()
        {
            Assert.That(Create(true, "campus", "campus_d").EventId,
                Is.Not.EqualTo(Create(true, "campus", "campus_d").EventId));
        }
    }
}
