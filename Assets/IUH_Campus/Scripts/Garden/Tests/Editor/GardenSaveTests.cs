using System;
using NUnit.Framework;

namespace IUHCampus.Garden
{
    public class GardenSaveTests
    {
        [Test]
        public void SaveRoundTrip()
        {
            var beds = GardenSave.Deserialize(null);
            beds[1].Plant();
            beds[1].Water(new DateTime(2026, 11, 19, 8, 0, 0, DateTimeKind.Utc));
            beds[2] = new GardenBed(GardenStage.Sprout, 0, true);
            beds[3] = new GardenBed(GardenStage.Bloom, 0, false);
            var saved = GardenSave.Serialize(beds);
            Assert.AreEqual(saved, GardenSave.Serialize(GardenSave.Deserialize(saved)));
            Assert.AreEqual(GardenRules.BedCount, GardenSave.Deserialize(saved).Count);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("v9|0,0,0")]
        [TestCase("v1|0,0,0;1,0,0;3,0,1")]
        [TestCase("v1|9,0,0;1,0,0;2,0,1;3,0,0;0,0,0;0,0,0")]
        public void InvalidSaveReturnsEmptyBeds(string data)
        {
            var beds = GardenSave.Deserialize(data);
            Assert.AreEqual(6, beds.Count);
            Assert.IsTrue(beds.TrueForAll(bed => bed.Stage == GardenStage.Empty));
        }
    }
}
