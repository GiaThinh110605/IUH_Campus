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
        static IUHDepthLabel()
        {
            Application.quitting+=ReleaseMaterials;
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload+=ReleaseMaterials;
#endif
        }
        static void ReleaseMaterials()
        {
            foreach(var material in Materials.Values)
                if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
            Materials.Clear();
        }
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void QueueOrphanCleanup(){UnityEditor.EditorApplication.delayCall+=()=>CleanupUnusedMaterials();}
#endif
        public static int CleanupUnusedMaterials()
        {
            var keep=new HashSet<Material>(Materials.Values);
            foreach(var renderer in Resources.FindObjectsOfTypeAll<MeshRenderer>())
                if(renderer.gameObject.scene.IsValid()&&renderer.gameObject.activeInHierarchy)
                    foreach(var material in renderer.sharedMaterials)if(material!=null)keep.Add(material);
            int count=0;
            foreach(var material in Resources.FindObjectsOfTypeAll<Material>())
            {
                if(material.name!="IUH World Font"||material.shader==null||material.shader.name!="IUH/Depth World Text"||keep.Contains(material)||(material.hideFlags&HideFlags.DontSave)==0)continue;
                if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);count++;
            }
            return count;
        }
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
