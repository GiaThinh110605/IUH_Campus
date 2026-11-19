using System;
using NUnit.Framework;

namespace IUHCampus.Garden
{
    public class GardenBedTests
    {
        static readonly DateTime Start = new DateTime(2026, 11, 19, 8, 0, 0, DateTimeKind.Utc);

        [TestCase(false, 30)]
        [TestCase(true, 45)]
        public void FullCycle(bool fertilized, int expectedGold)
        {
            var bed = new GardenBed();
            Assert.IsFalse(bed.Water(Start));
            Assert.IsFalse(bed.Fertilize());
            Assert.IsTrue(bed.Plant());
            Assert.IsFalse(bed.Plant());
            Assert.IsFalse(bed.Harvest(out _));
            if (fertilized)
            {
                Assert.IsTrue(bed.Fertilize());
                Assert.IsFalse(bed.Fertilize());
            }
            Assert.IsFalse(bed.Advance(Start.AddHours(1)));
            Assert.IsNull(bed.SecondsRemaining(Start));
            Assert.IsTrue(bed.Water(Start));
            Assert.IsFalse(bed.Water(Start.AddSeconds(5)));
            Assert.AreEqual(Start.Ticks, bed.WateredAtUtcTicks);
            Assert.AreEqual(42, bed.SecondsRemaining(Start.AddSeconds(18)));
            Assert.IsFalse(bed.Advance(Start.AddSeconds(59)));
            Assert.IsTrue(bed.Advance(Start.AddSeconds(60)));
            Assert.AreEqual(GardenStage.Sprout, bed.Stage);
            Assert.IsFalse(bed.IsWatered);
            Assert.IsFalse(bed.Advance(Start.AddHours(1)));
            Assert.IsTrue(bed.Water(Start.AddHours(1)));
            Assert.IsTrue(bed.Advance(Start.AddHours(2)));
            Assert.AreEqual(GardenStage.Bloom, bed.Stage);
            Assert.IsFalse(bed.Water(Start.AddHours(2)));
            Assert.IsTrue(bed.Harvest(out int gold));
            Assert.AreEqual(expectedGold, gold);
            Assert.AreEqual(GardenStage.Empty, bed.Stage);
            Assert.IsFalse(bed.Fertilized);
            Assert.IsFalse(bed.Harvest(out gold));
            Assert.AreEqual(0, gold);
        }

        [Test]
        public void ClockRollbackAndLongAbsenceAdvanceAtMostOneStage()
        {
            var bed = new GardenBed();
            bed.Plant();
            bed.Water(Start);
            Assert.IsFalse(bed.Advance(Start.AddMinutes(-10)));
            Assert.AreEqual(GardenStage.Seed, bed.Stage);
            Assert.AreEqual(60, bed.SecondsRemaining(Start.AddMinutes(-10)));
            Assert.IsTrue(bed.Advance(Start.AddHours(1)));
            Assert.AreEqual(GardenStage.Sprout, bed.Stage);
            Assert.IsFalse(bed.Advance(Start.AddHours(1)));
        }
    }
}
