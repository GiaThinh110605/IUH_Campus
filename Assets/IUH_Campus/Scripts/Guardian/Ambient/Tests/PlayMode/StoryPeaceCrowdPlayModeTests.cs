#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IUHCampus.Guardian.Tests
{
    public class StoryPeaceCrowdPlayModeTests
    {
        static readonly Type CrowdType = Type.GetType("IUHCampus.Guardian.Story.IUHStoryPeaceCrowd, Assembly-CSharp", true);
        static readonly Type MascotType = Type.GetType("IUHCampus.DailyActivities.IUHMascotNPC, Assembly-CSharp", true);

        GameObject students, mascot, director;

        [SetUp]
        public void SetUp()
        {
            students = new GameObject("NPC_STUDENTS");
            new GameObject("Student_01").transform.SetParent(students.transform, false);
            mascot = new GameObject("IUH_MASCOT_ROBOT_Test");
            mascot.AddComponent(MascotType);
            director = new GameObject("StoryDirectorTest");
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(students);
            UnityEngine.Object.DestroyImmediate(mascot);
            UnityEngine.Object.DestroyImmediate(director);
        }

        void Set(Component crowd, bool disaster) =>
            CrowdType.GetMethod("SetDisaster").Invoke(crowd, new object[] { disaster });

        [Test]
        public void DisasterHidesStudentsAndMascotPeaceShowsThem()
        {
            var crowd = director.AddComponent(CrowdType);
            Set(crowd, true);
            Assert.That(students.activeSelf, Is.False);
            Assert.That(mascot.activeSelf, Is.False);
            Assert.That((bool)CrowdType.GetProperty("IsDisaster").GetValue(crowd), Is.True);
            Set(crowd, false);
            Assert.That(students.activeSelf, Is.True);
            Assert.That(mascot.activeSelf, Is.True);
        }

        [Test]
        public void StartingInsideDisasterFindsAlreadyHiddenCrowdAfterReload()
        {
            var crowd = director.AddComponent(CrowdType);
            Set(crowd, true);
            Set(crowd, true);
            Assert.That(students.activeSelf, Is.False);
            Set(crowd, false);
            Assert.That(students.activeSelf, Is.True, "inactive roots captured at first call must be restored");
        }

        [Test]
        public void MissingCrowdDoesNotThrow()
        {
            UnityEngine.Object.DestroyImmediate(students);
            UnityEngine.Object.DestroyImmediate(mascot);
            var crowd = director.AddComponent(CrowdType);
            Assert.DoesNotThrow(() => { Set(crowd, true); Set(crowd, false); });
        }

        [Test]
        public void DisasterChangedEventFiresOnlyOnChange()
        {
            int count = 0;
            var evt = CrowdType.GetEvent("DisasterChanged");
            Action<bool> handler = _ => count++;
            evt.AddEventHandler(null, handler);
            try
            {
                var crowd = director.AddComponent(CrowdType);
                Set(crowd, true); Set(crowd, true); Set(crowd, false);
                Assert.That(count, Is.EqualTo(2));
            }
            finally { evt.RemoveEventHandler(null, handler); }
        }
    }
}
#endif
