using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WorldLockPlayModeTests
{
    [UnityTest]
    public IEnumerator SpawnerGameObjectIsLockedAndOriginalStateRestored()
    {
        var spawnerType = Type.GetType("IUHCampus.Combat.IUHBuildingMonsterSpawner, Assembly-CSharp", true);
        var lockType = Type.GetType("IUHCampus.Guardian.Story.IUHStoryWorldLock, Assembly-CSharp", true);
        var go = new GameObject("StorySpawnerTest");
        var spawner = (Behaviour)go.AddComponent(spawnerType);
        spawner.enabled = false;
        var world = Activator.CreateInstance(lockType);
        try
        {
            lockType.GetMethod("CaptureAndLock").Invoke(world, null);
            yield return null;
            Assert.IsFalse(go.activeInHierarchy);
            lockType.GetMethod("RestoreAll").Invoke(world, new object[] { new Func<string, bool>(_ => false) });
            Assert.IsTrue(go.activeSelf);
            Assert.IsFalse(spawner.enabled, "Original component state must be preserved");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
