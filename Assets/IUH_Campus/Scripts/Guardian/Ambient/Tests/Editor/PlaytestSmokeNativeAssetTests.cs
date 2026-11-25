#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class PlaytestSmokeNativeAssetTests
{
    const string Original = "Assets/IUH_Campus/ThirdParty/AssetStore/StylizedSmoke/Prefabs/msVFX_Stylized Smoke 1.prefab";
    const string Installed = "Assets/IUH_Campus/Resources/Guardian/VFX/CorruptionSmoke.prefab";
    [Test] public void NativePrefabAndRuntimePreserveAuthoredParticleSchema()
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Original);
        var installed = AssetDatabase.LoadAssetAtPath<GameObject>(Installed);
        Assert.That(installed, Is.Not.Null, "Run native installer first.");
        var original = source.GetComponentsInChildren<ParticleSystem>(true);
        var clone = installed.GetComponentsInChildren<ParticleSystem>(true);
        Assert.That(clone.Length, Is.EqualTo(original.Length));
        for (int i = 0; i < clone.Length; i++) AssertAuthoredProperties(original[i], clone[i]);
        var type = System.AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("IUHCampus.Guardian.Story.IUHStoryCorruptionZone")).First(t => t != null);
        var zone = (Component)type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null,
            new object[] { new[] { Vector3.left * 5, Vector3.right * 5 }, null });
        try
        {
            var runtime = zone.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in runtime) AssertAuthoredProperties(original[0], ps);
        }
        finally { Object.DestroyImmediate(zone.gameObject); }
    }
    static void AssertAuthoredProperties(ParticleSystem original, ParticleSystem actual)
    {
        var source = new SerializedObject(original);
        var target = new SerializedObject(actual);
        string[] paths = { "scalingMode", "simulationSpeed", "looping", "lengthInSec", "moveWithTransform",
            "InitialModule.startLifetime", "InitialModule.startSpeed", "InitialModule.startSize",
            "InitialModule.startSizeY", "InitialModule.startSizeZ", "InitialModule.gravityModifier",
            "ShapeModule", "VelocityModule", "SizeModule", "RotationModule", "ColorModule", "UVModule",
            "NoiseModule", "ForceModule", "ClampVelocityModule" };
        foreach (string path in paths)
        {
            var a = source.FindProperty(path);
            var b = target.FindProperty(path);
            Assert.That(a, Is.Not.Null, "Schema changed: " + path);
            Assert.That(b, Is.Not.Null, "Schema changed: " + path);
            if (a.propertyType != SerializedPropertyType.Generic)
                Assert.That(SerializedProperty.DataEquals(a, b), Is.True, "Authored property changed: " + path);
            else
            {
                var cursor = a.Copy();
                var end = cursor.GetEndProperty();
                while (cursor.Next(true) && !SerializedProperty.EqualContents(cursor, end))
                {
                    if (cursor.propertyType == SerializedPropertyType.Generic) continue;
                    var counterpart = target.FindProperty(cursor.propertyPath);
                    Assert.That(counterpart, Is.Not.Null, cursor.propertyPath);
                    Assert.That(SerializedProperty.DataEquals(cursor, counterpart), Is.True,
                        "Authored field changed: " + cursor.propertyPath);
                }
            }
        }
        Assert.That(actual.transform.localScale, Is.EqualTo(original.transform.localScale));
    }
    [Test] public void OwnTextureUsesGrayAlphaAndMaterialUsesCameraFadeOnly()
    {
        const string texturePath = "Assets/IUH_Campus/Resources/Guardian/VFX/CorruptionSmokeTexture.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        Assert.That(importer, Is.Not.Null);
        Assert.That(importer.alphaSource, Is.EqualTo(TextureImporterAlphaSource.FromGrayScale));
        Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Resources/Guardian/VFX/CorruptionSmoke.mat");
        Assert.That(AssetDatabase.GetAssetPath(mat.GetTexture("_BaseMap")), Is.EqualTo(texturePath));
        Assert.That(mat.IsKeywordEnabled("_FADING_ON"), Is.True);
        Assert.That(mat.IsKeywordEnabled("_SOFTPARTICLES_ON"), Is.False);
        Assert.That(mat.GetVector("_CameraFadeParams").y, Is.EqualTo(1f / 1.35f).Within(.00001f));
    }
}
#endif
