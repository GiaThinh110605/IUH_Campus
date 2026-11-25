using System;
using NUnit.Framework;

namespace IUHCampus.Garden
{
    public class GardenBedTests
    {
        static readonly DateTime Start = new DateTime(2026, 11, 25, 8, 0, 0, DateTimeKind.Utc);

        [Test]
        public void FreshSpotIsReadyAndPaysGold()
        {
            var spot = new WateringSpot("bed_1_2");
            Assert.IsTrue(spot.CanWater(Start));
            Assert.IsTrue(spot.Water(Start, out int gold));
            Assert.AreEqual(GardenRules.WaterGold, gold);
            Assert.AreEqual(Start.Ticks, spot.LastWateredUtcTicks);
        }

        [Test]
        public void CooldownBlocksUntilElapsed()
        {
            var spot = new WateringSpot("bed_1_2");
            spot.Water(Start, out _);
            Assert.IsFalse(spot.Water(Start.AddSeconds(GardenRules.CooldownSeconds - 1), out int gold));
            Assert.AreEqual(0, gold);
            Assert.AreEqual(1, spot.SecondsUntilReady(Start.AddSeconds(GardenRules.CooldownSeconds - 1)));
            Assert.IsTrue(spot.Water(Start.AddSeconds(GardenRules.CooldownSeconds), out gold));
            Assert.AreEqual(GardenRules.WaterGold, gold);
        }

        [Test]
        public void ClockGoingBackwardsDoesNotGrantEarlyWatering()
        {
            var spot = new WateringSpot("bed_1_2", Start.Ticks);
            Assert.AreEqual(GardenRules.CooldownSeconds, spot.SecondsUntilReady(Start.AddMinutes(-5)));
            Assert.IsFalse(spot.CanWater(Start.AddMinutes(-5)));
        }

        [Test]
        public void ZeroClockIsRejected()
        {
            Assert.IsFalse(new WateringSpot("bed_1_2").CanWater(new DateTime(0)));
        }
    }
}
