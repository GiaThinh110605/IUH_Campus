using System.Linq;
using NUnit.Framework;

namespace IUHCampus.Guardian.Story.Tests
{
    public class AftershockScheduleTests
    {
        [Test]
        public void FirstAftershockAfter120Seconds()
        {
            var schedule = new AftershockSchedule(() => 0);
            Assert.IsFalse(schedule.Tick(119, true));
            Assert.IsTrue(schedule.Tick(1, true));
            Assert.IsTrue(schedule.IsActive);
        }

        [Test]
        public void DoesNotCountWhenBlocked()
        {
            var schedule = new AftershockSchedule(() => 0);
            Assert.IsFalse(schedule.Tick(500, false));
            Assert.AreEqual(120, schedule.SecondsUntilNext);
        }

        [Test]
        public void NoNewAftershockWhileActive()
        {
            var schedule = new AftershockSchedule(() => 0);
            Assert.IsTrue(schedule.Tick(120, true));
            Assert.IsFalse(schedule.Tick(500, true));
        }

        [Test]
        public void CompleteSchedulesWithinRange()
        {
            foreach (var random in new[] { 0d, .999d })
            {
                var schedule = new AftershockSchedule(() => random);
                schedule.Tick(120, true);
                schedule.Complete();
                Assert.IsFalse(schedule.IsActive);
                Assert.That(schedule.SecondsUntilNext, Is.GreaterThanOrEqualTo(240).And.LessThan(360));
            }
        }

        [Test]
        public void PickZoneAvoidsLast()
        {
            foreach (var random in new[] { 0d, .34d, .67d, .99d })
            {
                int zone = AftershockSchedule.PickZone(() => random, 3, 1);
                Assert.That(zone, Is.InRange(0, 2));
                Assert.AreNotEqual(1, zone);
            }
        }

        [Test]
        public void PickEncountersReturnsOneOrTwoDistinct()
        {
            foreach (var random in new[] { 0d, .999d })
            {
                var names = AftershockSchedule.PickEncounters(() => random, new[] { "A", "B", "C" });
                Assert.That(names.Count, Is.InRange(1, 2));
                Assert.AreEqual(names.Count, names.Distinct().Count());
            }
            Assert.AreEqual(1, AftershockSchedule.PickEncounters(() => .999, new[] { "A" }).Count);
        }
    }
}
