using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ArenaLifecycleTests
{
    // Assembly-CSharp cannot be referenced by an asmdef. Reflection keeps its existing dependencies intact.
    private MonoBehaviour stage;
    private GameObject host, monster;
    private Type type;
    private bool Active => (bool)type.GetProperty("IsActive").GetValue(stage);
    private void Enter() => stage.StartCoroutine((IEnumerator)type.GetMethod("Enter").Invoke(stage, new object[] { null, monster.transform, null }));
    private void Exit() => type.GetMethod("Exit").Invoke(stage, null);
    [UnitySetUp] public IEnumerator SetUp()
    {
        var scene = SceneManager.GetSceneByName("IUH_CombatArena");
        if (scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        type = Type.GetType("IUHCampus.Guardian.Match3.Match3BattleStage, Assembly-CSharp", true);
        host = new GameObject("ArenaLifecycleTest"); stage = (MonoBehaviour)host.AddComponent(type);
        monster = GameObject.CreatePrimitive(PrimitiveType.Cube);
    }
    [UnityTearDown] public IEnumerator TearDown()
    {
        Exit(); UnityEngine.Object.Destroy(host); UnityEngine.Object.Destroy(monster);
        yield return WaitFor(() => !SceneManager.GetSceneByName("IUH_CombatArena").isLoaded);
    }
    private static IEnumerator WaitFor(Func<bool> condition)
    {
        float deadline = Time.realtimeSinceStartup + 5;
        while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(condition(), "Arena lifecycle timed out after 5 seconds");
    }
    private static void AssertClean()
    {
        Assert.IsFalse(SceneManager.GetSceneByName("IUH_CombatArena").isLoaded);
        foreach (var volume in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsInactive.Include))
            Assert.AreNotEqual(10f, volume.priority, "Arena volume leaked into the loaded scenes");
    }
    [UnityTest] public IEnumerator ExitDuringLoadCleansScene()
    {
        Enter(); yield return null; Exit();
        // Let the uncancellable async scene load finish even after the entry coroutine is stopped.
        yield return new WaitForSecondsRealtime(2);
        yield return WaitFor(() => !SceneManager.GetSceneByName("IUH_CombatArena").isLoaded);
        AssertClean();
    }
    [UnityTest] public IEnumerator NormalExitCleansScene()
    {
        Enter(); yield return WaitFor(() => Active); Exit();
        yield return WaitFor(() => !SceneManager.GetSceneByName("IUH_CombatArena").isLoaded); AssertClean();
    }
    [UnityTest] public IEnumerator ImmediateReentryHasOneArena()
    {
        Enter(); yield return null; Exit(); Enter(); yield return WaitFor(() => Active);
        int count = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).name == "IUH_CombatArena") count++;
        Assert.AreEqual(1, count);
    }
    [UnityTest] public IEnumerator ReentryDuringUnloadWaitsForUnload()
    {
        Enter(); yield return WaitFor(() => Active); Exit(); Enter();
        yield return WaitFor(() => Active);
        Assert.IsTrue(SceneManager.GetSceneByName("IUH_CombatArena").isLoaded);
    }
}
