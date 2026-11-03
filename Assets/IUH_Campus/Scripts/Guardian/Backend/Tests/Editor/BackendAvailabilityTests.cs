using System;
using NUnit.Framework;
namespace IUHCampus.Guardian.Backend.Tests
{
    public class BackendAvailabilityTests
    {
        [Test] public void IndependentRuntimeAssemblyExists()
        {
            Assert.That(Type.GetType("IUHCampus.Guardian.Backend.GuardianApiClient, IUHCampus.Guardian.Backend"), Is.Not.Null);
        }
    }
}
