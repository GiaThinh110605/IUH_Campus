using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Nature.Tests
{
    public sealed class NaturePrefabTests
    {
        [Test]
        public void PrefabsHaveMaterialsAndOnlyTreesHaveTrunkColliders()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/IUH_Campus/Prefabs/Nature" });
            Assert.That(guids, Has.Length.EqualTo(14));
            foreach (string guid in guids)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
                Assert.That(renderers, Is.Not.Empty, prefab.name);
                foreach (var renderer in renderers)
                {
                    var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                    Assert.That(renderer.sharedMaterials, Has.Length.EqualTo(mesh.subMeshCount), prefab.name);
                    Assert.That(renderer.sharedMaterials.All(material =>
                        material != null && material.shader.isSupported && !ShaderUtil.ShaderHasError(material.shader)),
                        Is.True, prefab.name);
                }

                foreach (var child in prefab.GetComponentsInChildren<Transform>(true))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject),
                        Is.Zero, child.name);

                bool tree = prefab.name.StartsWith("PF_Tree_");
                var colliders = prefab.GetComponentsInChildren<Collider>(true);
                Assert.That(colliders, Has.Length.EqualTo(tree ? 1 : 0), prefab.name);
                if (tree)
                {
                    Assert.That(colliders[0], Is.TypeOf<CapsuleCollider>(), prefab.name);
                    Assert.That(prefab.GetComponent<LODGroup>().GetLODs().Single().screenRelativeTransitionHeight,
                        Is.EqualTo(0.03f).Within(0.0001f), prefab.name);
                }
            }
        }
    }
}
