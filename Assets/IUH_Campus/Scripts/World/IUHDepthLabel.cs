using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.World
{
    // Legacy TextMesh's default font shader draws through walls. These world labels obey scene depth.
    [ExecuteAlways, RequireComponent(typeof(TextMesh))]
    public class IUHDepthLabel : MonoBehaviour
    {
        static readonly Dictionary<Font, Material> Materials = new Dictionary<Font, Material>();
        [SerializeField] Shader depthShader;
        TextMesh label;
        public void BindShader(Shader shader)
        {
            depthShader=shader;
            if(label==null)label=GetComponent<TextMesh>();
            Refresh(label.font);
        }
        void OnEnable()
        {
            label = GetComponent<TextMesh>();
            Font.textureRebuilt += Refresh;
            Refresh(label.font);
        }
        void OnDisable() { Font.textureRebuilt -= Refresh; }
        void Refresh(Font font)
        {
            if (label == null || font != label.font || font == null) return;
            if (!Materials.TryGetValue(font, out var mat) || mat == null)
            {
                var shader = depthShader != null ? depthShader : Shader.Find("IUH/Depth World Text");
                if (shader == null) return;
                mat = new Material(shader) { name = "IUH World Font", hideFlags = HideFlags.HideAndDontSave };
                Materials[font] = mat;
            }
            mat.mainTexture = font.material.mainTexture;
            GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
    }
}
