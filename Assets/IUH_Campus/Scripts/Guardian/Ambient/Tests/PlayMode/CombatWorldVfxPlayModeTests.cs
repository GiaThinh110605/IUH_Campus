#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IUHCampus.Guardian.Tests
{
    public class CombatWorldVfxPlayModeTests
    {
        [Test]
        public void BodyBoundsPreferSkinAndRejectBeaconMeshFallback()
        {
            Type stage = Type.GetType("IUHCampus.Guardian.Match3.Match3BattleStage, Assembly-CSharp", true);
            MethodInfo method = stage.GetMethod("TryGetBodyEffectBounds", BindingFlags.Static | BindingFlags.NonPublic);
            var proxy = new GameObject("ArenaBodyBoundsTest");
            try
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.transform.SetParent(proxy.transform, false); body.layer = 10;
                var beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beacon.name = "Beacon_Pillar_Beam"; beacon.transform.SetParent(proxy.transform, false);
                beacon.transform.localScale = new Vector3(1,14,1);
                object[] args = { proxy, default(Bounds), 0 };
                Assert.That((bool)method.Invoke(null, args), Is.True);
                Assert.That(((Bounds)args[1]).size.y, Is.EqualTo(1f).Within(.01f));
                Assert.That((int)args[2], Is.EqualTo(10));
                var skinObject = new GameObject("ActorSkin"); skinObject.layer = 11;
                skinObject.transform.SetParent(proxy.transform, false);
                var skin = skinObject.AddComponent<SkinnedMeshRenderer>();
                skin.sharedMesh = body.GetComponent<MeshFilter>().sharedMesh;
                skin.bounds = new Bounds(Vector3.up, new Vector3(.8f,2f,.8f));
                args = new object[] { proxy, default(Bounds), 0 };
                Assert.That((bool)method.Invoke(null, args), Is.True);
                Assert.That(((Bounds)args[1]).size.y, Is.EqualTo(2f).Within(.01f));
                Assert.That((int)args[2], Is.EqualTo(11));
            }
            finally { UnityEngine.Object.DestroyImmediate(proxy); }
        }
        [Test]
        public void OwnerUiScaleDoesNotShrinkWorldEmitter()
        {
            Type type = Type.GetType("IUHCampus.Guardian.Match3.IUHCombatWorldVfx, Assembly-CSharp", true);
            var root = new GameObject("ScaledUiPoolOwner");
            root.transform.localScale = Vector3.one * .01f;
            try
            {
                var pool = root.AddComponent(type);
                Assert.That((bool)type.GetMethod("Emit").Invoke(pool, new object[] { 0, new Vector3(3,-1999,2), 1.8f, 0 }), Is.True);
                var prefab = Resources.Load<GameObject>("Guardian/Combat/HitMonster");
                Assert.That(prefab, Is.Not.Null);
                var effect = root.GetComponentsInChildren<ParticleSystem>().First(ps => ps.transform.parent == root.transform);
                Vector3 expected = prefab.transform.localScale * (1.8f * .38f);
                Assert.That(Mathf.Abs(effect.transform.lossyScale.x), Is.EqualTo(Mathf.Abs(expected.x)).Within(.0001f));
                Assert.That(Mathf.Abs(effect.transform.lossyScale.y), Is.EqualTo(Mathf.Abs(expected.y)).Within(.0001f));
                Assert.That(Mathf.Abs(effect.transform.lossyScale.z), Is.EqualTo(Mathf.Abs(expected.z)).Within(.0001f));
                Assert.That(effect.transform.position, Is.EqualTo(new Vector3(3,-1999,2)));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        [UnityTest]
        public IEnumerator FiftyEmitsRemainBoundedAndReturnWithoutCamerasOrUi()
        {
            Type type = Type.GetType("IUHCampus.Guardian.Match3.IUHCombatWorldVfx, Assembly-CSharp", true);
            var root = new GameObject("CombatVfxTest");
            int cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            int textures = Resources.FindObjectsOfTypeAll<RenderTexture>().Length;
            int canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            try
            {
                var pool = root.AddComponent(type);
                MethodInfo emit = type.GetMethod("Emit");
                var position = new Vector3(3f, -1999f, 2f);
                for (int i = 0; i < 50; i++)
                {
                    emit.Invoke(pool, new object[] { i % 4, position, 1.8f, 0 });
                    Assert.That((int)type.GetProperty("InstanceCount").GetValue(pool), Is.InRange(1, 12));
                }
                Assert.That((int)type.GetProperty("InstanceCount").GetValue(pool), Is.EqualTo(12), "Install all four native prefab aliases first");
                Assert.That((int)type.GetProperty("ActiveCount").GetValue(pool), Is.EqualTo(12));
                foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
                {
                    Assert.That(ps.main.loop, Is.False);
                    Assert.That(ps.main.stopAction, Is.EqualTo(ParticleSystemStopAction.None));
                    Assert.That(ps.transform.position.y, Is.LessThan(-1990f), "Effect must stay in real arena space");
                }
                foreach (var helper in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (helper == pool) continue;
                    Assert.That(helper.enabled, Is.False);
                    var shake = helper.GetType().GetField("cameraShake");
                    if (shake != null) Assert.That(shake.GetValue(helper), Is.Null);
                }
                foreach (var light in root.GetComponentsInChildren<Light>(true)) Assert.That(light.enabled, Is.False);
                Assert.That(root.GetComponentsInChildren<Camera>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<Canvas>(true), Is.Empty);
                // Exercise fifty successful emissions through reused slots, then natural return.
                for (int i = 0; i < 50; i++)
                {
                    if (i % 12 == 0) type.GetMethod("Clear").Invoke(pool, null);
                    Assert.That((bool)emit.Invoke(pool, new object[] { i % 4, position, 1.8f, 0 }), Is.True);
                    Assert.That((int)type.GetProperty("InstanceCount").GetValue(pool), Is.EqualTo(12));
                    yield return null;
                }
                yield return new WaitForSecondsRealtime(5.2f);
                Assert.That((int)type.GetProperty("ActiveCount").GetValue(pool), Is.Zero);
                Assert.That((bool)emit.Invoke(pool, new object[] { 0, position, 1.8f, 0 }), Is.True);
                root.SetActive(false);
                Assert.That((int)type.GetProperty("ActiveCount").GetValue(pool), Is.Zero);
                Assert.That(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(cameras));
                Assert.That(Resources.FindObjectsOfTypeAll<RenderTexture>().Length, Is.EqualTo(textures));
                Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(canvases));
                LogAssert.NoUnexpectedReceived();
            }
            finally { UnityEngine.Object.Destroy(root); }
            yield return null;
        }
    }
}




#endif
