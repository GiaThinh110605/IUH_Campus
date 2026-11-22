using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Player.Tests
{
    public sealed class VroidStudentTests
    {
        [Test]
        public void VroidPrefabHasHumanoidAvatarAndCompilableUrpMaterials()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/IUH_Campus/Prefabs/Player/PF_Student_VRoid.prefab");
            Assert.That(prefab, Is.Not.Null);
            var animator = prefab.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.avatar, Is.Not.Null);
            Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True);
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.That(renderers.Length, Is.GreaterThan(0));
            foreach (var renderer in renderers)
            {
                Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(renderer.sharedMesh.subMeshCount));
                foreach (var material in renderer.sharedMaterials)
                {
                    Assert.That(material, Is.Not.Null);
                    Assert.That(material.shader.name, Is.EqualTo("VRM10/Universal Render Pipeline/MToon10"));
                    ShaderUtil.CompilePass(material, 0, true);
                    Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
                }
            }
        }
    }
}
