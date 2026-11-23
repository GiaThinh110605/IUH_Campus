using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Player.Tests
{
    public class NpcVroidTests
    {
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void NpcPrefabHasHumanoidAndBoundedTextures(int index)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/IUH_Campus/Prefabs/NPC/PF_NPC_VRoid_0{index}.prefab");
            Assert.That(root, Is.Not.Null);
            var animator = root.GetComponent<Animator>();
            Assert.That(animator.avatar.isValid && animator.avatar.isHuman, Is.True);
            Assert.That(animator.cullingMode, Is.EqualTo(AnimatorCullingMode.CullCompletely));
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                foreach (var material in renderer.sharedMaterials)
                {
                    Assert.That(material, Is.Not.Null);
                    Assert.That(material.shader.name, Is.EqualTo("VRM10/Universal Render Pipeline/MToon10"));
                    Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
                    foreach (string property in material.GetTexturePropertyNames())
                    {
                        var texture = material.GetTexture(property);
                        if (texture == null) continue;
                        Assert.That(texture.width, Is.LessThanOrEqualTo(1024), property);
                        Assert.That(texture.height, Is.LessThanOrEqualTo(1024), property);
                    }
                }
        }
    }
}
