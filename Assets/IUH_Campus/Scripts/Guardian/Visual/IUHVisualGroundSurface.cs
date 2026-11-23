using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian.Visual
{
    [CreateAssetMenu(menuName = "IUH Campus/Visual Ground Surface")]
    public sealed class IUHVisualGroundSurface : ScriptableObject
    {
        [SerializeField] private Vector3[] worldTriangles = System.Array.Empty<Vector3>();
        [SerializeField, Min(1f)] private float cellSize = 4f;
        private Dictionary<Vector2Int, List<int>> cells;

        public void SetTriangles(Vector3[] triangles)
        {
            worldTriangles = triangles ?? System.Array.Empty<Vector3>();
            cells = null;
        }

        public bool TryGetHeight(Vector3 probe, out float height)
        {
            if (cells == null) BuildCells();
            height = float.NegativeInfinity;
            if (!cells.TryGetValue(Cell(probe), out var indices)) return false;
            foreach (int index in indices)
            {
                Vector3 a = worldTriangles[index], b = worldTriangles[index + 1], c = worldTriangles[index + 2];
                float determinant = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                if (Mathf.Abs(determinant) < .000001f) continue;
                float u = ((b.z - c.z) * (probe.x - c.x) + (c.x - b.x) * (probe.z - c.z)) / determinant;
                float v = ((c.z - a.z) * (probe.x - c.x) + (a.x - c.x) * (probe.z - c.z)) / determinant;
                if (u < -.0001f || v < -.0001f || u + v > 1.0001f) continue;
                float y = u * a.y + v * b.y + (1f - u - v) * c.y;
                if (y <= probe.y && y > height) height = y;
            }
            return !float.IsNegativeInfinity(height);
        }

        private Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x / cellSize), Mathf.FloorToInt(p.z / cellSize));

        private void BuildCells()
        {
            cells = new Dictionary<Vector2Int, List<int>>();
            for (int i = 0; i + 2 < worldTriangles.Length; i += 3)
            {
                Vector3 min = Vector3.Min(worldTriangles[i], Vector3.Min(worldTriangles[i + 1], worldTriangles[i + 2]));
                Vector3 max = Vector3.Max(worldTriangles[i], Vector3.Max(worldTriangles[i + 1], worldTriangles[i + 2]));
                Vector2Int first = Cell(min), last = Cell(max);
                for (int x = first.x; x <= last.x; x++)
                    for (int z = first.y; z <= last.y; z++)
                    {
                        var key = new Vector2Int(x, z);
                        if (!cells.TryGetValue(key, out var entries)) cells[key] = entries = new List<int>();
                        entries.Add(i);
                    }
            }
        }

        private void OnEnable() => cells = null;
        private void OnValidate() { cellSize = Mathf.Max(1f, cellSize); cells = null; }
    }
}
