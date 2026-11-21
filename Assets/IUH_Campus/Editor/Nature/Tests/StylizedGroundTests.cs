using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Nature.Tests
{
    public sealed class StylizedGroundTests
    {
        [TestCase("Grass", "_GROUND_GRASS")]
        [TestCase("PavingPale", "_GROUND_PAVING")]
        [TestCase("Sandstone", "_GROUND_PAVING")]
        public void GroundMaterialUsesSupportedShaderAndSurfaceMode(string suffix, string keyword)
        {
            var shader = Shader.Find("IUH/Stylized Ground");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                $"Assets/IUH_Campus/Materials/Environment/M_Ground_{suffix}.mat");
            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader, Is.SameAs(shader));
            Assert.That(material.IsKeywordEnabled(keyword), Is.True);
        }
    }
}
