using System.Collections.Generic;
using NUnit.Framework;

namespace IUHCampus.Garden
{
    public class GardenSaveTests
    {
        [Test]
        public void RoundTripKeepsTicksPerId()
        {
            var data = GardenSave.Serialize(new[] { new WateringSpot("bed_-20_-1", 123), new WateringSpot("bed_9_10") });
            Assert.AreEqual("v3|bed_-20_-1=123;bed_9_10=0", data);
            var map = GardenSave.Deserialize(data);
            Assert.AreEqual(123, map["bed_-20_-1"]);
            Assert.AreEqual(0, map["bed_9_10"]);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("v2|1,0,0;0,0,0;0,0,0;0,0,0")]
        [TestCase("v3|bed=abc")]
        [TestCase("v3|=5")]
        [TestCase("v3|bed=-1")]
        public void BadOrOldDataIsEmpty(string data)
        {
            Assert.AreEqual(0, GardenSave.Deserialize(data).Count);
        }

        [Test]
        public void EmptyListSerializes()
        {
            Assert.AreEqual("v3|", GardenSave.Serialize(new List<WateringSpot>()));
            Assert.AreEqual(0, GardenSave.Deserialize("v3|").Count);
        }

        [Test]
        public void InvalidIdThrows()
        {
            Assert.Throws<System.ArgumentException>(() => GardenSave.Serialize(new[] { new WateringSpot("a=b") }));
        }
    }
}
