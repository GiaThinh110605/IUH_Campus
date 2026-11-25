using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IUHCampus.Guardian.EditorTools
{
public static class PlaytestSmokeInstaller
{
    const string Source = "Assets/IUH_Campus/ThirdParty/AssetStore/StylizedSmoke/Prefabs/msVFX_Stylized Smoke 1.prefab";
    const string Folder = "Assets/IUH_Campus/Resources/Guardian/VFX";
    [MenuItem("IUH/Playtest/Install F2 smoke resources")]
    public static void Install()
    {
        EnsureFolder(Folder);
        string destination = Folder + "/CorruptionSmoke.prefab";
        string materialPath = Folder + "/CorruptionSmoke.mat";
        string texturePath = Folder + "/CorruptionSmokeTexture.png";
        if (AssetDatabase.LoadAssetAtPath<Object>(destination) || AssetDatabase.LoadAssetAtPath<Object>(materialPath) || AssetDatabase.LoadAssetAtPath<Object>(texturePath))
            throw new System.InvalidOperationException("Review existing smoke resources before replacing them.");
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
        var original = source.GetComponentInChildren<ParticleSystemRenderer>(true).sharedMaterial;
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null || !shader.isSupported) throw new System.InvalidOperationException("URP particle shader unavailable.");
        var material = CreateMaterial(original, shader, texturePath);
        AssetDatabase.CreateAsset(material, materialPath);
        if (!AssetDatabase.CopyAsset(Source, destination)) throw new System.InvalidOperationException("CopyAsset failed.");
        var root = PrefabUtility.LoadPrefabContents(destination);
        try
        {
            foreach (var renderer in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
                renderer.sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(root, destination);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssetIfDirty(material);
    }
    static Material CreateMaterial(Material original, Shader shader, string texturePath)
    {
        string originalTexture = AssetDatabase.GetAssetPath(original.GetTexture("_BaseMap"));
        if (!AssetDatabase.CopyAsset(originalTexture, texturePath)) throw new System.InvalidOperationException("Texture CopyAsset failed.");
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType = TextureImporterType.Default;
        importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var material = new Material(original) { name = "CorruptionSmoke", shader = shader };
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Surface", 1);
        material.SetFloat("_Blend", 0);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0);
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = 3000;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetFloat("_CameraFadingEnabled", 1);
        material.SetFloat("_CameraNearFadeDistance", .15f);
        material.SetFloat("_CameraFarFadeDistance", 1.5f);
        material.SetVector("_CameraFadeParams", new Vector4(.15f, 1f / 1.35f, 0, 0));
        material.EnableKeyword("_FADING_ON");
        material.SetFloat("_SoftParticlesEnabled", 0);
        material.DisableKeyword("_SOFTPARTICLES_ON");
        material.SetVector("_SoftParticleFadeParams", Vector4.zero);
        return material;
    }
    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int split = path.LastIndexOf('/');
        EnsureFolder(path.Substring(0, split));
        AssetDatabase.CreateFolder(path.Substring(0, split), path.Substring(split + 1));
    }
}

}
