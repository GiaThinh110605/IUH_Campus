using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AmbientStudentPlayModeTests
{
    private readonly List<GameObject> objects = new List<GameObject>();
    private Type StudentType => Type.GetType("IUHCampus.Guardian.Ambient.IUHAmbientStudent, Assembly-CSharp", true);

    [TearDown]
    public void TearDown()
    {
        foreach (var go in objects) if (go != null) UnityEngine.Object.DestroyImmediate(go);
        objects.Clear();
        Type.GetType("IUHCampus.Core.IUHPlayerLocator, Assembly-CSharp", true)
            .GetMethod("Reset").Invoke(null, null);
    }

    private GameObject Create(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        objects.Add(go);
        return go;
    }

    private Component Student(Vector3[] route, Transform player = null)
    {
        var go = Create("Ambient student test", new Vector3(1000f, 0f, 1000f));
        go.SetActive(false);
        var student = go.AddComponent(StudentType);
        Set(student, "walkRoute", route);
        Set(student, "pauseRange", new Vector2(0.03f, 0.03f));
        Set(student, "player", player);
        var camera = Create("Ambient test camera", go.transform.position + Vector3.up * 5f).AddComponent<Camera>();
        Set(student, "viewCamera", camera);
        go.SetActive(true);
        return student;
    }

    private void Set(Component student, string field, object value) =>
        StudentType.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(student, value);

    [UnityTest]
    public IEnumerator WalkerReachesNextWaypointAndReturns()
    {
        var origin = new Vector3(1000f, 0f, 1000f);
        var student = Student(new[] { origin, origin + Vector3.right * 0.4f });
        bool reached = false;
        float deadline = Time.time + 3f;
        while (Time.time < deadline)
        {
            reached |= student.transform.position.x > origin.x + 0.3f;
            if (reached && student.transform.position.x < origin.x + 0.08f) break;
            yield return null;
        }
        Assert.IsTrue(reached, "Walker must reach second waypoint");
        Assert.Less(student.transform.position.x, origin.x + 0.08f, "Walker must return to first waypoint");
    }

    [UnityTest]
    public IEnumerator WalkerStopsForNearbyPlayer()
    {
        var origin = new Vector3(1000f, 0f, 1000f);
        var player = Create("Ambient blocker", origin + Vector3.forward * 0.5f).transform;
        var student = Student(new[] { origin, origin + Vector3.right * 2f }, player);
        yield return null;
        Vector3 before = student.transform.position;
        yield return new WaitForSeconds(0.2f);
        Assert.Less(Vector3.Distance(before, student.transform.position), 0.001f);
        player.position += Vector3.forward * 5f;
        yield return new WaitForSeconds(0.2f);
        Assert.Greater(Vector3.Distance(before, student.transform.position), 0.05f);
    }

    [UnityTest]
    public IEnumerator DistantCameraDisablesAnimatorAndPausesMovement()
    {
        var origin = new Vector3(1000f, 0f, 1000f);
        var student = Student(new[] { origin, origin + Vector3.right * 2f });
        var animator = student.gameObject.AddComponent<Animator>();
        Set(student, "animator", animator);
        var camera = (Camera)StudentType.GetField("viewCamera", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(student);
        camera.transform.position = origin + Vector3.forward * 50f;
        yield return null;
        var before = student.transform.position;
        yield return new WaitForSeconds(0.2f);
        Assert.IsFalse(animator.enabled);
        Assert.AreEqual(before, student.transform.position);
        camera.transform.position = origin + Vector3.up * 5f;
        yield return null;
        Assert.IsTrue(animator.enabled);
    }

    [UnityTest]
    public IEnumerator MissingPlayerDoesNotThrowAndWalkerContinues()
    {
        var origin = new Vector3(1000f, 0f, 1000f);
        var student = Student(new[] { origin, origin + Vector3.right * 2f });
        yield return new WaitForSeconds(0.25f);
        Assert.Greater(student.transform.position.x, origin.x + 0.1f);
        LogAssert.NoUnexpectedReceived();
    }
}
