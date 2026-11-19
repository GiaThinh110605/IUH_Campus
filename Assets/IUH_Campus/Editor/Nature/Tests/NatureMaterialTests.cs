using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Nature.Tests
{
    public sealed class NatureMaterialTests
    {
        [Test]
        public void FoliageShaderSupportsForwardShadowAndDepthPasses()
        {
            var shader = Shader.Find("IUH/Stylized Foliage");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
            Assert.That(shader.FindPassTagValue(0, new UnityEngine.Rendering.ShaderTagId("LightMode")).name,
                Is.EqualTo("UniversalForward"));
            Assert.That(shader.passCount, Is.EqualTo(3));
        }

        [TestCase("Leaves", "IUH/Stylized Foliage")]
        [TestCase("Bush", "IUH/Stylized Foliage")]
        [TestCase("LeafAtlas", "IUH/Stylized Foliage")]
        [TestCase("Flowers", "IUH/Stylized Foliage")]
        [TestCase("Grass", "IUH/Stylized Foliage")]
        [TestCase("Bark", "Universal Render Pipeline/Lit")]
        public void MaterialHasSupportedShaderAndTexture(string name, string shaderName)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                $"Assets/IUH_Campus/Materials/Nature/M_Nature_{name}.mat");
            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo(shaderName));
            Assert.That(material.shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
            Assert.That(material.GetTexture("_BaseMap"), Is.Not.Null);
            Assert.That(material.enableInstancing, Is.True);
        }
    }
}
