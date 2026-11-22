using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Guardian.Tests
{
    public class StylizedFacadeTests
    {
        [TestCase("_FACADE_TEXTURED")]
        [TestCase("_FACADE_PLAIN")]
        [TestCase("_FACADE_BRICK")]
        [TestCase("_FACADE_GLASS")]
        public void FacadeVariantCompilesWithDepthAndShadowPasses(string keyword)
        {
            var shader = Shader.Find("IUH/Stylized Facade");
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            var material = new Material(shader);
            try
            {
                foreach (var mode in new[] { "_FACADE_TEXTURED", "_FACADE_PLAIN", "_FACADE_BRICK", "_FACADE_GLASS" })
                    material.DisableKeyword(mode);
                material.EnableKeyword(keyword);
                foreach (var pass in new[] { "ForwardLit", "ShadowCaster", "DepthOnly", "DepthNormals" })
                {
                    int index = material.FindPass(pass);
                    Assert.That(index, Is.GreaterThanOrEqualTo(0), pass);
                    ShaderUtil.CompilePass(material, index, true);
                }
                Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
                foreach (var message in ShaderUtil.GetShaderMessages(shader))
                    Assert.That(message.severity.ToString(), Is.Not.EqualTo("Error"), message.message);
            }
            finally { Object.DestroyImmediate(material); }
        }
    }
}
