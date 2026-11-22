using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Player.Tests
{
    public sealed class StudentModelTests
    {
        [Test]
        public void StudentPrefabHasHumanoidAvatarAndCompleteMaterials()
        {
            const string path = "Assets/IUH_Campus/Prefabs/Player/PF_Student_VRoid.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null);
            var animator = prefab.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.avatar, Is.Not.Null);
            Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True);

            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.That(renderers, Has.Length.EqualTo(2));
            foreach (var renderer in renderers)
            {
                Assert.That(renderer.rootBone, Is.Not.Null, renderer.name);
                Assert.That(renderer.sharedMaterials, Has.Length.EqualTo(renderer.sharedMesh.subMeshCount));
                foreach (var material in renderer.sharedMaterials)
                {
                    Assert.That(material, Is.Not.Null, renderer.name);
                    Assert.That(material.shader.name, Is.EqualTo("VRM10/Universal Render Pipeline/MToon10"));
                }
            }
        }
    }
}
