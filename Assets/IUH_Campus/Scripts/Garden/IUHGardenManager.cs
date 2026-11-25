using System;
using System.Collections.Generic;
using IUHCampus.Guardian;
using UnityEngine;

namespace IUHCampus.Garden
{
    // Campus flowerbeds the player can water for gold. Beds are discovered from the lawn meshes of the
    // garden lawns and the curved fountain beds (one bed per connected lawn piece), so no scene markers are needed.
    public sealed class IUHGardenManager : MonoBehaviour
    {
        const string SaveKey = "IUH_Garden_v3";
        public static readonly string[] LawnParents = { "Garden beds and furniture_11", "Curved fountain gardens_101" };
        readonly List<WateringSpot> spots = new List<WateringSpot>();
        readonly List<Bounds> areas = new List<Bounds>();
        bool applying;

        public static IUHGardenManager Instance { get; private set; }
        public Func<DateTime> Clock { get; set; } = () => DateTime.UtcNow;
        public int SpotCount => spots.Count;
        public IReadOnlyList<Bounds> Areas => areas;
        public event Action<int> OnSpotChanged;

        void Awake()
        {
            Instance = this;
            if (areas.Count == 0) Configure(DiscoverAreas());
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public WateringSpot GetSpot(int index) => index >= 0 && index < spots.Count ? spots[index] : null;
        public Bounds GetArea(int index) => areas[index];

        public static List<Bounds> DiscoverAreas()
        {
            var result = new List<Bounds>();
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (t.name != "Lawn" || t.parent == null || Array.IndexOf(LawnParents, t.parent.name) < 0) continue;
                var mf = t.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable) result.AddRange(FindAreas(mf));
            }
            return result;
        }

        // World bounds of each connected group of upward-facing triangles (vertices welded by position).
        public static List<Bounds> FindAreas(MeshFilter mf)
        {
            var mesh = mf.sharedMesh;
            var v = mesh.vertices;
            var t = mesh.triangles;
            var m = mf.transform.localToWorldMatrix;
            var world = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++) world[i] = m.MultiplyPoint3x4(v[i]);
            var weld = new Dictionary<Vector3Int, int>();
            var parent = new List<int>();
            int Key(Vector3 p)
            {
                var k = new Vector3Int(Mathf.RoundToInt(p.x * 100), Mathf.RoundToInt(p.y * 100), Mathf.RoundToInt(p.z * 100));
                if (!weld.TryGetValue(k, out int id)) { id = parent.Count; weld[k] = id; parent.Add(id); }
                return id;
            }
            int Root(int x) { while (parent[x] != x) x = parent[x] = parent[parent[x]]; return x; }
            var tris = new List<int[]>();
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 a = world[t[i]], b = world[t[i + 1]], c = world[t[i + 2]];
                var n = Vector3.Cross(b - a, c - a);
                if (n.y <= .5f * n.magnitude) continue;
                int ka = Key(a), kb = Key(b), kc = Key(c);
                parent[Root(kb)] = Root(ka);
                parent[Root(kc)] = Root(ka);
                tris.Add(new[] { t[i], t[i + 1], t[i + 2], ka });
            }
            var groups = new Dictionary<int, Bounds>();
            foreach (var tri in tris)
            {
                int r = Root(tri[3]);
                for (int j = 0; j < 3; j++)
                {
                    if (groups.TryGetValue(r, out var b)) { b.Encapsulate(world[tri[j]]); groups[r] = b; }
                    else groups[r] = new Bounds(world[tri[j]], Vector3.zero);
                }
            }
            var list = new List<Bounds>(groups.Values);
            list.RemoveAll(b => b.size.x * b.size.z < 1f);
            list.Sort((x, y) => x.center.x != y.center.x ? x.center.x.CompareTo(y.center.x) : x.center.z.CompareTo(y.center.z));
            return list;
        }

        public static string SpotId(Bounds area) =>
            "bed_" + Mathf.RoundToInt(area.center.x) + "_" + Mathf.RoundToInt(area.center.z);

        public void Configure(IList<Bounds> beds)
        {
            areas.Clear();
            spots.Clear();
            var saved = GardenSave.Deserialize(PlayerPrefs.GetString(SaveKey));
            foreach (var area in beds)
            {
                string id = SpotId(area);
                if (spots.Exists(s => s.Id == id)) continue;
                areas.Add(area);
                spots.Add(new WateringSpot(id, saved.TryGetValue(id, out long ticks) ? ticks : 0));
            }
        }

        public void Save()
        {
            PlayerPrefs.SetString(SaveKey, GardenSave.Serialize(spots));
            PlayerPrefs.Save();
        }

        public bool TryWater(int index, out int gold)
        {
            gold = 0;
            var spot = GetSpot(index);
            if (applying || spot == null) return false;
            applying = true;
            try
            {
                if (!spot.Water(Clock(), out gold)) return false;
                var inventory = IUHGuardianInventory.Instance;
                if (inventory != null) inventory.AddGold(gold);
                else Debug.LogWarning("[IUH Garden] Watered without an inventory.");
                Save();
                OnSpotChanged?.Invoke(index);
                return true;
            }
            finally { applying = false; }
        }
    }
}
