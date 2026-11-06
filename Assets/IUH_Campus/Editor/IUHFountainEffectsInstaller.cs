#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using IUHCampus.World;
namespace IUHCampus.Editor
{
    public static class IUHFountainEffectsInstaller
    {
        private const string Folder = "Assets/IUH_Campus/SharedCampusConcept/";
        [MenuItem("IUH Campus/Concept/Add Animated Fountain")]
        public static void Install()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before installing fountain effects.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.isDirty) throw new InvalidOperationException("Save the scene before installing fountain effects.");
            var fountain = GameObject.Find("Central fountain and sculpture_7");
            if (fountain == null) throw new InvalidOperationException("Campus fountain not found.");
            var filter = fountain.transform.Find("Ivory sculpture").GetComponent<MeshFilter>();
            string meshPath = Folder + "Meshes/Fountain sculpture without static jets.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                // The original builder appended 12 x 18 sphere instances AFTER the sculpture.
                // Validate every instance before removing the tail; never cut sculpture triangles by position.
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                int count = primitive.GetComponent<MeshFilter>().sharedMesh.vertexCount;
                UnityEngine.Object.DestroyImmediate(primitive);
                var source = filter.sharedMesh; var vertices = source.vertices;
                int keep = vertices.Length - 216 * count;
                if (keep <= 0) throw new InvalidOperationException("Unexpected fountain mesh topology.");
                for (int jet = 0; jet < 12; jet++) for (int step = 0; step < 18; step++)
                {
                    float a = jet * Mathf.PI / 6, f = step / 18f, r = 4.6f - f * 2.5f;
                    var expected = new Vector3(Mathf.Cos(a)*r, .6f + Mathf.Sin(f*Mathf.PI)*1.4f, Mathf.Sin(a)*r);
                    int start = keep + (jet*18+step)*count;
                    Bounds bounds = new Bounds(vertices[start], Vector3.zero);
                    for (int v = 1; v < count; v++) bounds.Encapsulate(vertices[start+v]);
                    if (Vector3.Distance(bounds.center, expected) > .005f || bounds.size.y > .12f)
                        throw new InvalidOperationException("Static jet validation failed; original mesh left untouched.");
                }
                mesh = new Mesh { name = "Fountain sculpture without static jets" };
                var list = new List<Vector3>(vertices); mesh.SetVertices(list.GetRange(0, keep));
                mesh.SetNormals(new List<Vector3>(source.normals).GetRange(0, keep));
                if (source.uv.Length == vertices.Length) mesh.SetUVs(0, new List<Vector2>(source.uv).GetRange(0, keep));
                var triangles = new List<int>(); var original = source.triangles;
                for (int i = 0; i < original.Length; i += 3)
                    if (original[i] < keep && original[i+1] < keep && original[i+2] < keep)
                    { triangles.Add(original[i]); triangles.Add(original[i+1]); triangles.Add(original[i+2]); }
                mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            Undo.RecordObject(filter, "Replace static fountain jets"); filter.sharedMesh = mesh;
            var effects = fountain.GetComponent<IUHFountainEffects>();
            if (effects == null) effects = Undo.AddComponent<IUHFountainEffects>(fountain);
            effects.waterParticles = Material("Fountain spray", false);
            effects.waterRipples = Material("Fountain ripples", true);
            EditorUtility.SetDirty(effects); EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene);
            Debug.Log("[IUH Fountain] Installed 12 animated jets; original mesh preserved. Sculpture vertices: " + mesh.vertexCount);
        }
        private static Material Material(string name, bool ring)
        {
            string path = Folder + "Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("IUH/Fountain Particles")); AssetDatabase.CreateAsset(mat, path); }
            mat.SetFloat("_Ring", ring ? 1 : 0); EditorUtility.SetDirty(mat); return mat;
        }
    }
}
#endif
