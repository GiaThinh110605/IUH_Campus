using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace IUHCampus.Nature.Tests
{
    public sealed class StylizedGroundTests
    {
        const string GrassKeyword = "_GROUND_GRASS";
        const string PavingKeyword = "_GROUND_PAVING";

        [Test]
        public void GrassMaterialUsesCampusLawnDetail()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/IUH_Campus/Materials/Environment/M_Ground_Grass.mat");
            Assert.That(material, Is.Not.Null);
            Assert.That(material.HasProperty("_DetailTex"), Is.True);
            Assert.That(material.HasProperty("_DetailScale"), Is.True);
            Assert.That(material.HasProperty("_DetailStrength"), Is.True);

            var lawnTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/IUH_Campus/SharedCampusConcept/Textures/grass.png");
            Assert.That(lawnTexture, Is.Not.Null);
            Assert.That(material.GetTexture("_DetailTex"), Is.SameAs(lawnTexture));
        }

        [TestCase("Grass", GrassKeyword)]
        [TestCase("PavingPale", PavingKeyword)]
        [TestCase("Sandstone", PavingKeyword)]
        public void GroundMaterialUsesSupportedShaderAndSurfaceMode(string suffix, string keyword)
        {
            var shader = Shader.Find("IUH/Stylized Ground");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                $"Assets/IUH_Campus/Materials/Environment/M_Ground_{suffix}.mat");
            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader, Is.SameAs(shader));
            Assert.That(material.IsKeywordEnabled(keyword), Is.True);
            Assert.That(material.IsKeywordEnabled(keyword == GrassKeyword ? PavingKeyword : GrassKeyword), Is.False);

            var forwardPass = material.FindPass("ForwardLit");
            Assert.That(forwardPass, Is.GreaterThanOrEqualTo(0));
            ShaderUtil.CompilePass(material, forwardPass, forceSync: true);

            var errors = ShaderUtil.GetShaderMessages(shader)
                .Where(message => message.severity == ShaderCompilerMessageSeverity.Error)
                .Select(message => message.message);
            Assert.That(errors, Is.Empty, $"{suffix} ({keyword})");
            Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
        }
    }
}
