using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IUHCampus.EditorTools
{
    // Completes the campus boundary in the style of the main gate: granite wall (2.4 m x 0.5 m) with
    // dark bronze bars (0.08 m, every 0.5 m, 0.85-3.15 m), plus invisible box colliders along every side.
    public static class IUHPerimeterFenceBuilder
    {
        public const string RootName = "CAMPUS_PERIMETER_FENCE";
        const string MeshFolder = "Assets/IUH_Campus/Meshes/Campus";
        const float WallHeight = 2.4f, WallThickness = .5f;
        const float BarSize = .08f, BarPeriod = .5f, BarBottom = .85f, BarTop = 3.15f;
        const float ColliderHeight = 3.4f, ColliderThickness = .6f;

        public struct Segment
        {
            public Vector2 A, B;           // XZ endpoints
            public bool Wall, Bars, Collider;
            public float SkipBarsInsideAbsX; // on the south line, existing gate bars cover |x| below this
            public Segment(Vector2 a, Vector2 b, bool wall, bool bars, bool collider, float skip = 0f)
            { A = a; B = b; Wall = wall; Bars = bars; Collider = collider; SkipBarsInsideAbsX = skip; }
        }

        public static GameObject Build(IList<Segment> segments, Material wallMaterial, Material barMaterial)
        {
            var old = GameObject.Find(RootName);
            if (old != null) Object.DestroyImmediate(old);
            Directory.CreateDirectory(MeshFolder);
            var root = new GameObject(RootName);
            var wv = new List<Vector3>(); var wt = new List<int>(); var wu = new List<Vector2>();
            var bv = new List<Vector3>(); var bt = new List<int>(); var bu = new List<Vector2>();
            int index = 0;
            foreach (var s in segments)
            {
                var a = new Vector3(s.A.x, 0, s.A.y); var b = new Vector3(s.B.x, 0, s.B.y);
                var dir = (b - a); float len = dir.magnitude; dir /= len;
                var side = Vector3.Cross(Vector3.up, dir);
                if (s.Wall) AddBox(wv, wt, wu, a, b, side, WallThickness, 0f, WallHeight);
                if (s.Bars)
                {
                    for (float d = BarPeriod * .5f; d < len; d += BarPeriod)
                    {
                        var c = a + dir * d;
                        if (s.SkipBarsInsideAbsX > 0f && Mathf.Abs(c.x) < s.SkipBarsInsideAbsX) continue;
                        AddBox(bv, bt, bu, c - dir * BarSize * .5f, c + dir * BarSize * .5f, side, BarSize, BarBottom, BarTop);
                    }
                }
                if (s.Collider)
                {
                    var go = new GameObject("Boundary_Collider_" + index);
                    go.transform.SetParent(root.transform, false);
                    go.transform.position = (a + b) * .5f + Vector3.up * ColliderHeight * .5f;
                    go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                    var box = go.AddComponent<BoxCollider>();
                    box.size = new Vector3(ColliderThickness, ColliderHeight, len);
                }
                index++;
            }
            AddRenderer(root.transform, "Perimeter wall", wv, wt, wu, wallMaterial);
            AddRenderer(root.transform, "Perimeter bars", bv, bt, bu, barMaterial);
            GameObjectUtility.SetStaticEditorFlags(root, StaticEditorFlags.BatchingStatic);
            foreach (Transform child in root.transform)
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic);
            AssetDatabase.SaveAssets();
            return root;
        }

        // Axis-aligned-in-segment box from a to b (centre line), given thickness and vertical range; UVs in metres / 2.
        static void AddBox(List<Vector3> v, List<int> t, List<Vector2> uv, Vector3 a, Vector3 b, Vector3 side, float thick, float y0, float y1)
        {
            var h = side * thick * .5f;
            Vector3[] p =
            {
                a - h + Vector3.up * y0, b - h + Vector3.up * y0, b + h + Vector3.up * y0, a + h + Vector3.up * y0,
                a - h + Vector3.up * y1, b - h + Vector3.up * y1, b + h + Vector3.up * y1, a + h + Vector3.up * y1,
            };
            float len = (b - a).magnitude, height = y1 - y0;
            void Quad(int i0, int i1, int i2, int i3, float w, float hh)
            {
                int s = v.Count;
                v.Add(p[i0]); v.Add(p[i1]); v.Add(p[i2]); v.Add(p[i3]);
                uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(w * .5f, 0)); uv.Add(new Vector2(w * .5f, hh * .5f)); uv.Add(new Vector2(0, hh * .5f));
                t.AddRange(new[] { s, s + 2, s + 1, s, s + 3, s + 2 });
            }
            Quad(0, 1, 5, 4, len, height);    // side -
            Quad(2, 3, 7, 6, len, height);    // side +
            Quad(3, 0, 4, 7, thick, height);  // end a
            Quad(1, 2, 6, 5, thick, height);  // end b
            Quad(4, 5, 6, 7, len, thick);     // top
        }

        static void AddRenderer(Transform parent, string name, List<Vector3> v, List<int> t, List<Vector2> uv, Material material)
        {
            if (v.Count == 0) return;
            var mesh = new Mesh { name = RootName + "_" + name.Replace(' ', '_'), indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(t, 0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
            string path = $"{MeshFolder}/{mesh.name}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            mr.shadowCastingMode = ShadowCastingMode.On;
        }
    }
}
