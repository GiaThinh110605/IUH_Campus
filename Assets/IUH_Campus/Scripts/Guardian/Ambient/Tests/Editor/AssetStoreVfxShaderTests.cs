#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Guardian.Tests
{
    public sealed class AssetStoreVfxShaderTests
    {
        [Test]
        public void ImportedVfxPrefabsUseSupportedErrorFreeShaders()
        {
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/IUH_Campus/ThirdParty/AssetStore" });
            Assert.That(paths.Length, Is.GreaterThan(0));
            foreach (var guid in paths)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                for (int index = 0; index < renderer.sharedMaterials.Length; index++)
                {
                    var material = renderer.sharedMaterials[index];
                    var particles = renderer.GetComponent<ParticleSystem>();
                    if (index == 1 && particles != null && !particles.trails.enabled)
                        continue;
                    Assert.That(material, Is.Not.Null, path);
                    Assert.That(material.shader, Is.Not.Null, path);
                    Assert.That(material.shader.isSupported, Is.True, path + ": " + material.shader.name);
                    Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False, path);
                    Assert.That(material.shader.name.StartsWith("Legacy Shaders/"), Is.False, path);
                    Assert.That(material.shader.name == "EGA/Particles/FireSphere", Is.False, path);
                }
            }
        }
    }
}
#endif
