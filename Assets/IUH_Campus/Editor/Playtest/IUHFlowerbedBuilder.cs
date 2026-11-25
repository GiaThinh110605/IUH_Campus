using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IUHCampus.EditorTools
{
    // Plants flowerbeds from existing nature prefabs, merges geometry per material and adds an
    // invisible boundary wall so the player cannot walk into / onto the bed. Deterministic (seeded).
    public static class IUHFlowerbedBuilder
    {
        public const string MeshFolder = "Assets/IUH_Campus/Meshes/Nature/Flowerbeds";
        const string NaturePrefabs = "Assets/IUH_Campus/Prefabs/Nature/";

        public struct Plant
        {
            public string Prefab;
            public float Weight, BaseScale;
            public Plant(string prefab, float weight, float baseScale) { Prefab = prefab; Weight = weight; BaseScale = baseScale; }
        }

        public static readonly Plant[] DefaultMix =
        {
            new Plant("PF_Flower_3_Group", 3f, .34f),
            new Plant("PF_Flower_4_Group", 3f, .30f),
            new Plant("PF_Bush_Flowers", 2f, .42f),
            new Plant("PF_Bush", 1f, .40f),
            new Plant("PF_Plant_1", 1f, .55f),
        };

        public sealed class Result
        {
            public GameObject Root;
            public int Placed, Rejected, Renderers;
            public float WallHeight;
            public override string ToString() => $"root={Root?.name} placed={Placed} rejected={Rejected} renderers={Renderers} wall={WallHeight}";
        }

        // Samples points on `surface` (world space), keeps `edgeMargin` from the surface boundary and
        // `keepOut` radius around the given points, plants `count` clumps and merges them under `rootName`.
        public static Result Build(string rootName, MeshFilter surface, int count, int seed, float edgeMargin,
            IList<Vector3> keepOut, float keepOutRadius, Plant[] mix, float wallHeight, Func<Vector3, bool> extraReject = null,
            int clusterSize = 1, float clusterRadius = .8f)
        {
            var result = new Result { WallHeight = wallHeight };
            var old = GameObject.Find(rootName);
            if (old != null) UnityEngine.Object.DestroyImmediate(old);

            var tris = WorldTriangles(surface, out float totalArea);
            var boundary = BoundaryEdges(surface);
            var rng = new System.Random(seed);
            float totalWeight = 0f;
            foreach (var p in mix) totalWeight += p.Weight;

            var temp = new GameObject(rootName + "_TEMP");
            int attempts = 0;
            while (result.Placed < count && attempts < count * 40)
            {
                attempts++;
                Vector3 center = SamplePoint(tris, totalArea, rng);
                for (int k = 0; k < Mathf.Max(1, clusterSize) && result.Placed < count; k++)
                {
                    Vector3 point = center;
                    if (k > 0)
                    {
                        float angle = (float)rng.NextDouble() * Mathf.PI * 2f, radius = clusterRadius * Mathf.Sqrt((float)rng.NextDouble());
                        point += new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                        if (!OnSurface(tris, ref point)) { result.Rejected++; continue; }
                    }
                    if (DistanceToEdges(point, boundary) < edgeMargin || NearAny(point, keepOut, keepOutRadius)
                        || (extraReject != null && extraReject(point)))
                    {
                        result.Rejected++;
                        continue;
                    }
                    var plant = Pick(mix, totalWeight, rng);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NaturePrefabs + plant.Prefab + ".prefab");
                    if (prefab == null) throw new InvalidOperationException("Missing prefab " + plant.Prefab);
                    var go = UnityEngine.Object.Instantiate(prefab, temp.transform);
                    go.transform.position = point;
                    go.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                    go.transform.localScale = Vector3.one * plant.BaseScale * Mathf.Lerp(.8f, 1.2f, (float)rng.NextDouble());
                    result.Placed++;
                }
            }

            result.Root = new GameObject(rootName);
            Directory.CreateDirectory(MeshFolder);
            result.Renderers = MergeByMaterial(temp, result.Root.transform, rootName);
            UnityEngine.Object.DestroyImmediate(temp);
            if (wallHeight > 0f) AddWall(result.Root.transform, rootName, surface, boundary, wallHeight);
            GameObjectUtility.SetStaticEditorFlags(result.Root, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic);
            foreach (Transform child in result.Root.transform)
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic);
            AssetDatabase.SaveAssets();
            return result;
        }

        // Projects the point onto the surface triangles (XZ containment); false when it falls outside the surface.
        static bool OnSurface(List<Vector3[]> tris, ref Vector3 p)
        {
            foreach (var t in tris)
            {
                Vector2 a = new Vector2(t[0].x, t[0].z), b = new Vector2(t[1].x, t[1].z), c = new Vector2(t[2].x, t[2].z), q = new Vector2(p.x, p.z);
                float d = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
                if (Mathf.Abs(d) < 1e-8f) continue;
                float u = ((b.y - c.y) * (q.x - c.x) + (c.x - b.x) * (q.y - c.y)) / d;
                float v = ((c.y - a.y) * (q.x - c.x) + (a.x - c.x) * (q.y - c.y)) / d;
                float w = 1f - u - v;
                if (u < 0f || v < 0f || w < 0f) continue;
                p.y = t[0].y * u + t[1].y * v + t[2].y * w;
                return true;
            }
            return false;
        }

        static Plant Pick(Plant[] mix, float total, System.Random rng)
        {
            double r = rng.NextDouble() * total;
            foreach (var p in mix) { r -= p.Weight; if (r <= 0) return p; }
            return mix[mix.Length - 1];
        }

        static bool NearAny(Vector3 p, IList<Vector3> points, float radius)
        {
            if (points == null) return false;
            foreach (var q in points)
                if (new Vector2(p.x - q.x, p.z - q.z).sqrMagnitude < radius * radius) return true;
            return false;
        }

        static List<Vector3[]> WorldTriangles(MeshFilter mf, out float total)
        {
            var mesh = mf.sharedMesh; var v = mesh.vertices; var t = mesh.triangles;
            var m = mf.transform.localToWorldMatrix;
            var list = new List<Vector3[]>(); total = 0f;
            for (int i = 0; i < t.Length; i += 3)
            {
                var a = m.MultiplyPoint3x4(v[t[i]]); var b = m.MultiplyPoint3x4(v[t[i + 1]]); var c = m.MultiplyPoint3x4(v[t[i + 2]]);
                var n = Vector3.Cross(b - a, c - a);
                if (n.y <= 0.5f * n.magnitude) continue; // only upward-facing triangles
                list.Add(new[] { a, b, c, new Vector3(n.magnitude * .5f, 0, 0) });
                total += n.magnitude * .5f;
            }
            return list;
        }

        static Vector3 SamplePoint(List<Vector3[]> tris, float total, System.Random rng)
        {
            double r = rng.NextDouble() * total;
            foreach (var tri in tris)
            {
                r -= tri[3].x;
                if (r > 0) continue;
                float u = (float)rng.NextDouble(), w = (float)rng.NextDouble();
                if (u + w > 1f) { u = 1f - u; w = 1f - w; }
                return tri[0] + (tri[1] - tri[0]) * u + (tri[2] - tri[0]) * w;
            }
            return tris[tris.Count - 1][0];
        }

        // Boundary edges (used by exactly one upward triangle), world space, welding by position.
        public static List<Vector3[]> BoundaryEdges(MeshFilter mf)
        {
            var mesh = mf.sharedMesh; var v = mesh.vertices; var t = mesh.triangles;
            var m = mf.transform.localToWorldMatrix;
            Func<Vector3, Vector3Int> key = p => new Vector3Int(Mathf.RoundToInt(p.x * 1000), Mathf.RoundToInt(p.y * 1000), Mathf.RoundToInt(p.z * 1000));
            var counts = new Dictionary<(Vector3Int, Vector3Int), int>();
            var pos = new Dictionary<(Vector3Int, Vector3Int), Vector3[]>();
            for (int i = 0; i < t.Length; i += 3)
            {
                var a = m.MultiplyPoint3x4(v[t[i]]); var b = m.MultiplyPoint3x4(v[t[i + 1]]); var c = m.MultiplyPoint3x4(v[t[i + 2]]);
                var n = Vector3.Cross(b - a, c - a);
                if (n.y <= 0.5f * n.magnitude) continue;
                foreach (var e in new[] { (a, b), (b, c), (c, a) })
                {
                    var ka = key(e.Item1); var kb = key(e.Item2);
                    var k = Compare(ka, kb) < 0 ? (ka, kb) : (kb, ka);
                    counts[k] = counts.TryGetValue(k, out int n0) ? n0 + 1 : 1;
                    pos[k] = new[] { e.Item1, e.Item2 };
                }
            }
            var edges = new List<Vector3[]>();
            foreach (var kv in counts) if (kv.Value == 1) edges.Add(pos[kv.Key]);
            return edges;
        }

        static int Compare(Vector3Int a, Vector3Int b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y != b.y ? a.y.CompareTo(b.y) : a.z.CompareTo(b.z);

        public static float DistanceToEdges(Vector3 p, List<Vector3[]> edges)
        {
            float best = float.PositiveInfinity;
            var q = new Vector2(p.x, p.z);
            foreach (var e in edges)
            {
                var a = new Vector2(e[0].x, e[0].z); var b = new Vector2(e[1].x, e[1].z);
                var ab = b - a; float len = ab.sqrMagnitude;
                float s = len > 1e-8f ? Mathf.Clamp01(Vector2.Dot(q - a, ab) / len) : 0f;
                best = Mathf.Min(best, (q - (a + ab * s)).magnitude);
            }
            return best;
        }

        static int MergeByMaterial(GameObject temp, Transform parent, string name)
        {
            var byMaterial = new Dictionary<Material, List<CombineInstance>>();
            foreach (var mf in temp.GetComponentsInChildren<MeshFilter>())
            {
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr == null || mf.sharedMesh == null) continue;
                var mats = mr.sharedMaterials;
                for (int s = 0; s < mf.sharedMesh.subMeshCount && s < mats.Length; s++)
                {
                    if (mats[s] == null) continue;
                    if (!byMaterial.TryGetValue(mats[s], out var list)) byMaterial[mats[s]] = list = new List<CombineInstance>();
                    list.Add(new CombineInstance { mesh = mf.sharedMesh, subMeshIndex = s, transform = mf.transform.localToWorldMatrix });
                }
            }
            int index = 0;
            foreach (var kv in byMaterial)
            {
                var mesh = new Mesh { name = $"{name}_{kv.Key.name}", indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(kv.Value.ToArray(), true, true);
                // IUH/Stylized Foliage reads position, normal, color and uv0 only: drop tangents and compress to keep assets small.
                mesh.tangents = null;
                mesh.RecalculateBounds();
                mesh.Optimize();
                MeshUtility.SetMeshCompression(mesh, ModelImporterMeshCompression.Medium);
                string path = $"{MeshFolder}/{name}_{kv.Key.name}.asset";
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(mesh, path);
                var go = new GameObject(kv.Key.name);
                go.transform.SetParent(parent, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = kv.Key;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = true;
                mr.lightProbeUsage = LightProbeUsage.Off;
                mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
                index++;
            }
            return index;
        }

        // Removes triangles whose world-space centroid lies inside the XZ rectangle. Edits the mesh asset in place;
        // callers must ensure the mesh is not shared. Returns the number of triangles removed.
        public static int RemoveTrianglesInRect(MeshFilter mf, Rect xz)
        {
            var mesh = mf.sharedMesh;
            var v = mesh.vertices; var m = mf.transform.localToWorldMatrix;
            int removed = 0;
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                if (mesh.GetTopology(s) != MeshTopology.Triangles) continue;
                var src = mesh.GetTriangles(s);
                var keep = new List<int>(src.Length);
                for (int i = 0; i < src.Length; i += 3)
                {
                    var c = (m.MultiplyPoint3x4(v[src[i]]) + m.MultiplyPoint3x4(v[src[i + 1]]) + m.MultiplyPoint3x4(v[src[i + 2]])) / 3f;
                    if (xz.Contains(new Vector2(c.x, c.z))) { removed++; continue; }
                    keep.Add(src[i]); keep.Add(src[i + 1]); keep.Add(src[i + 2]);
                }
                mesh.SetTriangles(keep, s, true);
            }
            if (removed > 0) EditorUtility.SetDirty(mesh);
            return removed;
        }

        // Moves every vertex whose world XZ lies in `region` by `worldDelta` (mesh asset edited in place; unshared meshes only).
        public static int ShiftVerticesInRegion(MeshFilter mf, Rect region, Vector3 worldDelta)
        {
            var mesh = mf.sharedMesh;
            var v = mesh.vertices; var m = mf.transform.localToWorldMatrix; var inv = mf.transform.worldToLocalMatrix;
            int moved = 0;
            for (int i = 0; i < v.Length; i++)
            {
                var w = m.MultiplyPoint3x4(v[i]);
                if (!region.Contains(new Vector2(w.x, w.z))) continue;
                v[i] = inv.MultiplyPoint3x4(w + worldDelta);
                moved++;
            }
            if (moved == 0) return 0;
            mesh.vertices = v;
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return moved;
        }

        // Invisible boundary wall around the upward faces of `surface`, under a new root (no planting).
        public static GameObject BuildWallOnly(string rootName, MeshFilter surface, float height)
        {
            var old = GameObject.Find(rootName);
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            Directory.CreateDirectory(MeshFolder);
            var root = new GameObject(rootName);
            AddWall(root.transform, rootName, surface, BoundaryEdges(surface), height);
            GameObjectUtility.SetStaticEditorFlags(root, StaticEditorFlags.BatchingStatic);
            AssetDatabase.SaveAssets();
            return root;
        }

        static void AddWall(Transform parent, string name, MeshFilter surface, List<Vector3[]> edges, float height)
        {
            var verts = new List<Vector3>(); var tris = new List<int>();
            foreach (var e in edges)
            {
                int i = verts.Count;
                float bottom = Mathf.Min(e[0].y, e[1].y) - .2f;
                verts.Add(new Vector3(e[0].x, bottom, e[0].z)); verts.Add(new Vector3(e[1].x, bottom, e[1].z));
                verts.Add(new Vector3(e[1].x, bottom + height, e[1].z)); verts.Add(new Vector3(e[0].x, bottom + height, e[0].z));
                tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3, i, i + 2, i + 1, i, i + 3, i + 2 });
            }
            var mesh = new Mesh { name = name + "_Wall", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(verts); mesh.SetTriangles(tris, 0); mesh.RecalculateBounds();
            string path = $"{MeshFolder}/{name}_Wall.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            var go = new GameObject("BedWall_Collider");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
