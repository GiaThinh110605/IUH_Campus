#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class MonsterIdleActualFeetEditModeTests
{
    const string PrefabPath = "Assets/IUH_Campus/Prefabs/Guardian/Monsters/PF_BlueDemon_GuardianIdle.prefab";

    [UnityTest]
    public IEnumerator NormalizationMatchesBakedFeetAndHeightToStaticReference()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);
        GameObject visual = null;
        var reference = GameObject.CreatePrimitive(PrimitiveType.Cube);
        try
        {
            UnityEngine.Object.DestroyImmediate(reference.GetComponent<Collider>());
            reference.transform.position = new Vector3(2f, .89f, -1f);
            reference.transform.localScale = new Vector3(.8f, 1.7f, .8f);

            var helper = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("IUHCampus.Guardian.EditorTools.IUHMonsterIdleAssets"))
                .FirstOrDefault(t => t != null);
            Assert.That(helper, Is.Not.Null);
            var normalize = helper.GetMethod("InstallVisual");
            Assert.That(normalize, Is.Not.Null);
            visual = (GameObject)normalize.Invoke(null, new object[] { reference, prefab });
            yield return null;
            yield return null;
            var expected = reference.GetComponent<MeshRenderer>().bounds;
            var actual = BakeWorldBounds(visual);
            Assert.That(actual.min.y, Is.EqualTo(expected.min.y).Within(.005f));
            Assert.That(actual.size.y, Is.EqualTo(expected.size.y).Within(.005f));
            Assert.That(actual.center.x, Is.EqualTo(expected.center.x).Within(.005f));
            Assert.That(actual.center.z, Is.EqualTo(expected.center.z).Within(.005f));
        }
        finally { if (visual != null) UnityEngine.Object.DestroyImmediate(visual); UnityEngine.Object.DestroyImmediate(reference); }
    }

    static Bounds BakeWorldBounds(GameObject visual)
    {
        bool first = true;
        var result = new Bounds();
        var mesh = new Mesh();
        try
        {
            foreach (var skin in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                mesh.Clear(); skin.BakeMesh(mesh, true);
                foreach (var vertex in mesh.vertices)
                {
                    var world = skin.transform.TransformPoint(vertex);
                    if (first) { result = new Bounds(world, Vector3.zero); first = false; }
                    else result.Encapsulate(world);
                }
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(mesh); }
        Assert.That(first, Is.False, "No baked vertices.");
        return result;
    }
}
#endif
