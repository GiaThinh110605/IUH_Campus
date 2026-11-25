#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class MonsterIdleGeneratedEditModeTests
{
    const string GeneratedFolder = "Assets/IUH_Campus/Prefabs/Guardian";
    const string MaterialPath = "Assets/IUH_Campus/Models/Monsters/M_Monster_Atlas.mat";

    static GameObject[] LoadGeneratedPrefabs()
    {
        var generated = AssetDatabase.FindAssets("t:Prefab", new[] { GeneratedFolder })
            .Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(p => p != null && p.name.EndsWith("_GuardianIdle", StringComparison.Ordinal)).ToArray();
        Assert.That(generated, Is.Not.Empty, "Generate F3 prefabs before executing read-only tests.");
        return generated;
    }

    [Test]
    public void GeneratedPrefabsHaveBoundLoopingGenericIdleAndSafeMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.That(material, Is.Not.Null);
        Assert.That(material.shader, Is.Not.Null);
        Assert.That(material.shader.isSupported, Is.True);
        Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
        foreach (var prefab in LoadGeneratedPrefabs())
        {
            var animator = prefab.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null, prefab.name);
            Assert.That(animator.applyRootMotion, Is.False);
            Assert.That(animator.cullingMode, Is.EqualTo(AnimatorCullingMode.CullCompletely));
            Assert.That(animator.isHuman, Is.False);
            Assert.That(prefab.GetComponentsInChildren<Animation>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(r => r.bones.Length > 0), Is.True);
            Assert.That(animator.runtimeAnimatorController, Is.Not.Null);
            var clip = animator.runtimeAnimatorController.animationClips.Single();
            Assert.That(clip.legacy, Is.False);
            Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime, Is.True);
            Assert.That(AnimationUtility.GetCurveBindings(clip), Is.Not.Empty);
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                Assert.That(string.IsNullOrEmpty(binding.path) || prefab.transform.Find(binding.path) != null,
                    Is.True, prefab.name + " unbound curve " + binding.path);
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                    foreach (var assigned in renderer.sharedMaterials) Assert.That(assigned, Is.SameAs(material));
        }
    }

    [Test]
    public void SampledIdleArmsLieBelowTPoseShoulderPlane()
    {
        foreach (var prefab in LoadGeneratedPrefabs())
        {
            var instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var clip = instance.GetComponent<Animator>().runtimeAnimatorController.animationClips.Single();
                clip.SampleAnimation(instance, 0f);
                var bones = instance.GetComponentsInChildren<Transform>(true);
                float height = MeshBounds(instance).size.y;
                foreach (var suffix in new[] { ".L", ".R" })
                {
                    var shoulder = Bone(bones, "UpperArm" + suffix);
                    var elbow = Bone(bones, "LowerArm" + suffix);
                    var hand = Bone(bones, "Middle1" + suffix);
                    // Source has no Hand bone; Middle1 is the knuckle landmark. A T-pose keeps elbow/knuckle near shoulder height.
                    float elbowDrop = (shoulder.position.y - elbow.position.y) / height;
                    float handDrop = (shoulder.position.y - hand.position.y) / height;
                    Assert.That(elbowDrop, Is.GreaterThan(.025f), prefab.name + suffix + " elbow drop/height=" + elbowDrop);
                    Assert.That(handDrop, Is.GreaterThan(.05f), prefab.name + suffix + " hand drop/height=" + handDrop);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
    }

    static Transform Bone(IEnumerable<Transform> bones, string name)
    {
        var bone = bones.SingleOrDefault(b => b.name == name);
        Assert.That(bone, Is.Not.Null, "Missing expected source bone " + name);
        return bone;
    }

    static Bounds MeshBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Assert.That(renderers, Is.Not.Empty);
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        Assert.That(bounds.size.y, Is.GreaterThan(.001f));
        return bounds;
    }
}
#endif

