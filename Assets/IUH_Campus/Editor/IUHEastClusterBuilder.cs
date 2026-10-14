using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IUHCampus.Editor
{
    public static class IUHEastClusterBuilder
    {
        private const string ROOT_PATH = "_REWORK_V2/02_BUILDINGS";
        private const string MAT_DIR = "Assets/IUH_Campus/Materials/Rework/East_CIG/";

        private const string OLD_C_PATH = "_REWORK_V2/01_MASTERPLAN/CampusWorld/IUH_Masterplan_Blockout_MAP_ALIGNED/06_EAST_CLUSTER/BLK_C";
        private const string OLD_I_PATH = "_REWORK_V2/01_MASTERPLAN/CampusWorld/IUH_Masterplan_Blockout_MAP_ALIGNED/06_EAST_CLUSTER/BLK_I_Dorm_Men";
        private const string OLD_G_PATH = "_REWORK_V2/01_MASTERPLAN/CampusWorld/IUH_Masterplan_Blockout_MAP_ALIGNED/06_EAST_CLUSTER/BLK_G_Dorm_Women";

        [MenuItem("IUH Campus/Build East Cluster (C-I-G)", false, 102)]
        public static void BuildEastCluster()
        {
            var rework = GameObject.Find("_REWORK_V2");
            if (rework == null)
            {
                Debug.LogError("[IUHEastClusterBuilder] _REWORK_V2 not found!");
                return;
            }

            var buildingsParent = GameObject.Find(ROOT_PATH);
            if (buildingsParent == null)
            {
                var t = rework.transform.Find("02_BUILDINGS");
                if (t != null) buildingsParent = t.gameObject;
                else
                {
                    buildingsParent = new GameObject("02_BUILDINGS");
                    buildingsParent.transform.SetParent(rework.transform, false);
                }
            }

            // Load Materials (exact 9 shared materials)
            Material matFacade = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Facade_White.mat");
            Material matBlue = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Accent_IUH_Blue.mat");
            Material matMint = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Accent_Mint.mat");
            Material matWinOpaque = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Window_Opaque.mat");
            Material matWinTrans = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Window_Transparent.mat");
            Material matFrame = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Frame_Dark.mat");
            Material matRailing = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Metal_Railing.mat");
            Material matBase = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Concrete_Base.mat");
            Material matRoof = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_CIG_Roof_Dark.mat");

            // 1. Build Building C (Academic)
            BuildBuildingC(buildingsParent, matFacade, matBlue, matWinOpaque, matWinTrans, matFrame, matRailing, matBase, matRoof);

            // 2. Build Building I (Dorm Men)
            BuildBuildingI(buildingsParent, matFacade, matMint, matWinOpaque, matWinTrans, matFrame, matRailing, matBase, matRoof);

            // 3. Build Building G (Dorm Women)
            BuildBuildingG(buildingsParent, matFacade, matBlue, matWinOpaque, matWinTrans, matFrame, matRailing, matBase, matRoof);

            // 4. Disable Renderers and Colliders of old blockouts
            DisableBlockout(OLD_C_PATH);
            DisableBlockout(OLD_I_PATH);
            DisableBlockout(OLD_G_PATH);

            Debug.Log("[IUHEastClusterBuilder] East Cluster C-I-G successfully built and blockout renderers disabled.");
        }

        private static void DisableBlockout(string path)
        {
            var go = GameObject.Find(path);
            if (go != null)
            {
                foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
                foreach (var c in go.GetComponentsInChildren<Collider>(true))
                {
                    c.enabled = false;
                }
            }
        }

        private static GameObject GetOrCreateBuildingRoot(GameObject parent, string name)
        {
            var existing = parent.transform.Find(name);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            GameObject root = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(root, "Create " + name);
            root.transform.SetParent(parent.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            return root;
        }

        private static void AttachCombinedMesh(GameObject parent, string name, List<CombineInstance> combines, Material mat)
        {
            if (combines.Count == 0) return;

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            Mesh combined = new Mesh();
            combined.name = name + "_Mesh";
            if (combines.Count > 1000) combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combined.CombineMeshes(combines.ToArray(), true, true, false);
            combined.RecalculateNormals();
            combined.RecalculateBounds();
            mf.sharedMesh = combined;
        }

        private static void AddBoxToCombines(List<CombineInstance> list, Vector3 center, Vector3 size)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = center;
            cube.transform.localScale = size;

            CombineInstance ci = new CombineInstance();
            ci.mesh = cube.GetComponent<MeshFilter>().sharedMesh;
            ci.transform = cube.transform.localToWorldMatrix;
            list.Add(ci);

            GameObject.DestroyImmediate(cube);
        }

        // =========================================================================
        // 1. BUILDING C — ACADEMIC BUILDING (5 FLOORS, 21.0m)
        // Footprint: X in [20.5, 32.4], Z in [-32.0, -15.0], Y in [0.0, 21.0]
        // =========================================================================
        private static void BuildBuildingC(GameObject parent, Material matFacade, Material matBlue, 
            Material matWinOpaque, Material matWinTrans, Material matFrame, Material matRailing, Material matBase, Material matRoof)
        {
            GameObject root = GetOrCreateBuildingRoot(parent, "Building_C_Academic");

            float minX = 20.5f, maxX = 32.4f; // Width = 11.9m
            float minZ = -32.0f, maxZ = -15.0f; // Length = 17.0m
            float centerX = (minX + maxX) * 0.5f; // 26.45m
            float centerZ = (minZ + maxZ) * 0.5f; // -23.50m
            float width = maxX - minX;
            float length = maxZ - minZ;

            float[] floorHeights = new float[] { 0.0f, 4.2f, 7.8f, 11.4f, 15.0f, 18.6f };
            float parapetHeight = 21.0f;

            List<CombineInstance> cStructure = new List<CombineInstance>();
            List<CombineInstance> cWinOpaque = new List<CombineInstance>();
            List<CombineInstance> cWinTrans = new List<CombineInstance>();
            List<CombineInstance> cFrames = new List<CombineInstance>();
            List<CombineInstance> cBlue = new List<CombineInstance>();
            List<CombineInstance> cRailings = new List<CombineInstance>();
            List<CombineInstance> cBase = new List<CombineInstance>();
            List<CombineInstance> cRoof = new List<CombineInstance>();

            // --- Floor Slabs ---
            for (int f = 0; f < floorHeights.Length; f++)
            {
                float y = floorHeights[f];
                float thick = (f == 0) ? 0.25f : 0.35f;
                AddBoxToCombines(cStructure, new Vector3(centerX, y + thick * 0.5f, centerZ), new Vector3(width, thick, length));
            }

            // --- Base Plinth & Steps ---
            AddBoxToCombines(cBase, new Vector3(centerX, 0.12f, centerZ), new Vector3(width + 0.1f, 0.24f, length + 0.1f));
            AddBoxToCombines(cBase, new Vector3(minX - 0.15f, 0.08f, centerZ), new Vector3(0.3f, 0.16f, 9.0f));

            // --- Ground Floor Arcade & Lobby (West Facing towards campus avenue) ---
            float arcadeX = minX + 2.0f; // 22.5m
            int colCountZ = 6;
            for (int i = 0; i < colCountZ; i++)
            {
                float z = Mathf.Lerp(minZ + 1.5f, maxZ - 1.5f, (float)i / (colCountZ - 1));
                AddBoxToCombines(cStructure, new Vector3(minX + 0.25f, 2.1f, z), new Vector3(0.5f, 4.2f, 0.5f));
            }
            // Ground floor solid side/rear walls:
            AddBoxToCombines(cStructure, new Vector3(maxX - 0.15f, 2.1f, centerZ), new Vector3(0.3f, 4.2f, length)); // East wall
            AddBoxToCombines(cStructure, new Vector3(centerX, 2.1f, maxZ - 0.15f), new Vector3(width, 4.2f, 0.3f)); // North wall
            AddBoxToCombines(cStructure, new Vector3(centerX, 2.1f, minZ + 0.15f), new Vector3(width, 4.2f, 0.3f)); // South wall
            // Ground floor recessed transparent glass lobby wall at arcadeX:
            AddBoxToCombines(cWinTrans, new Vector3(arcadeX, 2.1f, centerZ), new Vector3(0.06f, 3.8f, length - 3.0f));
            for (float z = minZ + 3.0f; z <= maxZ - 3.0f; z += 2.5f)
            {
                AddBoxToCombines(cFrames, new Vector3(arcadeX, 2.1f, z), new Vector3(0.12f, 3.8f, 0.12f));
            }
            // Entrance Canopy on West side:
            AddBoxToCombines(cFrames, new Vector3(minX + 1.0f, 3.9f, centerZ), new Vector3(2.0f, 0.15f, 4.8f));

            // --- Upper Floors (Levels 2 to 5) ---
            // Full-height solid end shear walls on East and North:
            AddBoxToCombines(cStructure, new Vector3(maxX - 0.15f, (parapetHeight + 4.2f) * 0.5f, centerZ), new Vector3(0.3f, parapetHeight - 4.2f, length));
            AddBoxToCombines(cStructure, new Vector3(centerX, (parapetHeight + 4.2f) * 0.5f, maxZ - 0.15f), new Vector3(width, parapetHeight - 4.2f, 0.3f));

            for (int f = 1; f < floorHeights.Length; f++)
            {
                float floorY = floorHeights[f];
                float nextY = (f < floorHeights.Length - 1) ? floorHeights[f + 1] : 18.6f;
                float storyH = nextY - floorY;

                // West Facade Classroom Window Band & Columns:
                AddBoxToCombines(cStructure, new Vector3(minX + 0.2f, floorY + 0.5f, centerZ), new Vector3(0.4f, 1.0f, length)); // Spandrel
                float winY = floorY + 1.0f + (storyH - 1.0f) * 0.5f;
                float winH = storyH - 1.2f;
                AddBoxToCombines(cWinOpaque, new Vector3(minX + 0.05f, winY, centerZ), new Vector3(0.1f, winH, length - 0.8f));

                // Vertical column mullions on West facade:
                for (int b = 0; b <= 6; b++)
                {
                    float z = Mathf.Lerp(minZ + 0.5f, maxZ - 0.5f, (float)b / 6);
                    AddBoxToCombines(cStructure, new Vector3(minX + 0.2f, floorY + storyH * 0.5f, z), new Vector3(0.4f, storyH, 0.4f));
                    AddBoxToCombines(cFrames, new Vector3(minX + 0.02f, winY, z), new Vector3(0.08f, winH, 0.10f));
                }
                AddBoxToCombines(cBlue, new Vector3(minX - 0.04f, floorY + storyH - 0.08f, centerZ), new Vector3(0.16f, 0.12f, length));

                // South Facade Classroom Windows:
                AddBoxToCombines(cStructure, new Vector3(centerX, floorY + 0.5f, minZ + 0.15f), new Vector3(width, 1.0f, 0.3f));
                AddBoxToCombines(cWinOpaque, new Vector3(centerX, winY, minZ + 0.05f), new Vector3(width - 2.0f, winH, 0.1f));
                AddBoxToCombines(cBlue, new Vector3(centerX, floorY + storyH - 0.08f, minZ - 0.04f), new Vector3(width, 0.12f, 0.16f));
            }

            // --- Roof & Parapet ---
            AddBoxToCombines(cStructure, new Vector3(centerX, 19.8f, maxZ - 0.15f), new Vector3(width, 2.4f, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(centerX, 19.8f, minZ + 0.15f), new Vector3(width, 2.4f, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(minX + 0.15f, 19.8f, centerZ), new Vector3(0.3f, 2.4f, length));
            AddBoxToCombines(cStructure, new Vector3(maxX - 0.15f, 19.8f, centerZ), new Vector3(0.3f, 2.4f, length));
            AddBoxToCombines(cRoof, new Vector3(centerX, 18.7f, centerZ), new Vector3(width - 0.6f, 0.1f, length - 0.6f));

            // Penthouse tum:
            AddBoxToCombines(cStructure, new Vector3(maxX - 2.0f, 20.2f, maxZ - 2.0f), new Vector3(3.4f, 3.2f, 3.4f));
            AddBoxToCombines(cRoof, new Vector3(maxX - 2.0f, 21.85f, maxZ - 2.0f), new Vector3(3.6f, 0.2f, 3.6f));
            // HVAC units:
            AddBoxToCombines(cRoof, new Vector3(centerX - 1.6f, 19.4f, centerZ), new Vector3(2.0f, 1.4f, 1.6f));
            AddBoxToCombines(cRoof, new Vector3(centerX + 1.6f, 19.4f, centerZ), new Vector3(2.0f, 1.4f, 1.6f));

            // Attach all combined meshes
            AttachCombinedMesh(root, "01_Structure", cStructure, matFacade);
            AttachCombinedMesh(root, "02_Windows_Opaque", cWinOpaque, matWinOpaque);
            AttachCombinedMesh(root, "03_Lobby_Glazing", cWinTrans, matWinTrans);
            AttachCombinedMesh(root, "04_Frames_Canopy", cFrames, matFrame);
            AttachCombinedMesh(root, "05_Accents_Blue", cBlue, matBlue);
            AttachCombinedMesh(root, "06_Railings", cRailings, matRailing);
            AttachCombinedMesh(root, "07_Plinth_Steps", cBase, matBase);
            AttachCombinedMesh(root, "08_Roof_MEP", cRoof, matRoof);

            // Colliders
            GameObject goCols = new GameObject("Colliders");
            goCols.transform.SetParent(root.transform, false);

            var colUpper = goCols.AddComponent<BoxCollider>();
            colUpper.center = new Vector3(centerX, (parapetHeight + 4.2f) * 0.5f, centerZ);
            colUpper.size = new Vector3(width, parapetHeight - 4.2f, length);

            var colGroundCore = goCols.AddComponent<BoxCollider>();
            colGroundCore.center = new Vector3((arcadeX + maxX) * 0.5f, 2.1f, centerZ);
            colGroundCore.size = new Vector3(maxX - arcadeX, 4.2f, length);

            var colSouth = goCols.AddComponent<BoxCollider>();
            colSouth.center = new Vector3((minX + arcadeX) * 0.5f, 2.1f, minZ + 0.8f);
            colSouth.size = new Vector3(arcadeX - minX, 4.2f, 1.6f);

            var colNorth = goCols.AddComponent<BoxCollider>();
            colNorth.center = new Vector3((minX + arcadeX) * 0.5f, 2.1f, maxZ - 0.8f);
            colNorth.size = new Vector3(arcadeX - minX, 4.2f, 1.6f);
        }

        // =========================================================================
        // 2. BUILDING I — DORMITORY MEN (9 FLOORS, 35.5m)
        // Footprint: X in [37.2, 51.0], Z in [-34.0, -14.5], Y in [0.0, 35.5]
        // =========================================================================
        private static void BuildBuildingI(GameObject parent, Material matFacade, Material matMint, 
            Material matWinOpaque, Material matWinTrans, Material matFrame, Material matRailing, Material matBase, Material matRoof)
        {
            GameObject root = GetOrCreateBuildingRoot(parent, "Building_I_Dorm_Men");

            float minX = 37.2f, maxX = 51.0f; // Width = 13.8m
            float minZ = -34.0f, maxZ = -14.5f; // Length = 19.5m
            float centerX = (minX + maxX) * 0.5f; // 44.1m
            float centerZ = (minZ + maxZ) * 0.5f; // -24.25m
            float width = maxX - minX;
            float length = maxZ - minZ;

            int storyCount = 9;
            float groundH = 4.0f;
            float upperStoryH = 3.5f;
            float totalH = 35.5f;

            List<CombineInstance> cStructure = new List<CombineInstance>();
            List<CombineInstance> cWinOpaque = new List<CombineInstance>();
            List<CombineInstance> cWinTrans = new List<CombineInstance>();
            List<CombineInstance> cFrames = new List<CombineInstance>();
            List<CombineInstance> cMint = new List<CombineInstance>();
            List<CombineInstance> cRailings = new List<CombineInstance>();
            List<CombineInstance> cBase = new List<CombineInstance>();
            List<CombineInstance> cRoof = new List<CombineInstance>();

            float[] floorHeights = new float[storyCount + 1];
            floorHeights[0] = 0.0f;
            floorHeights[1] = groundH;
            for (int i = 2; i <= storyCount; i++) floorHeights[i] = floorHeights[i - 1] + upperStoryH;

            // --- Floor Slabs ---
            for (int f = 0; f <= storyCount; f++)
            {
                float y = floorHeights[f];
                float thick = (f == 0) ? 0.25f : 0.35f;
                AddBoxToCombines(cStructure, new Vector3(centerX, y + thick * 0.5f, centerZ), new Vector3(width, thick, length));
            }

            // --- Base Plinth & Steps ---
            AddBoxToCombines(cBase, new Vector3(centerX, 0.12f, centerZ), new Vector3(width + 0.1f, 0.24f, length + 0.1f));
            AddBoxToCombines(cBase, new Vector3(minX - 0.1f, 0.08f, centerZ), new Vector3(0.2f, 0.16f, 7.0f));

            // --- Solid Gable Walls on North and South ends ---
            AddBoxToCombines(cStructure, new Vector3(centerX, (totalH + groundH) * 0.5f, minZ + 0.15f), new Vector3(width, totalH - groundH, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(centerX, (totalH + groundH) * 0.5f, maxZ - 0.15f), new Vector3(width, totalH - groundH, 0.3f));

            // --- Ground Floor Entrance (West Facing towards C-I Pathway) ---
            AddBoxToCombines(cStructure, new Vector3(maxX - 0.15f, 2.0f, centerZ), new Vector3(0.3f, 4.0f, length));
            AddBoxToCombines(cStructure, new Vector3(centerX, 2.0f, maxZ - 0.15f), new Vector3(width, 4.0f, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(centerX, 2.0f, minZ + 0.15f), new Vector3(width, 4.0f, 0.3f));
            AddBoxToCombines(cWinTrans, new Vector3(minX + 0.05f, 2.0f, centerZ), new Vector3(0.1f, 3.6f, 6.0f));
            AddBoxToCombines(cFrames, new Vector3(minX + 0.05f, 2.0f, centerZ - 1.5f), new Vector3(0.12f, 3.6f, 0.12f));
            AddBoxToCombines(cFrames, new Vector3(minX + 0.05f, 2.0f, centerZ + 1.5f), new Vector3(0.12f, 3.6f, 0.12f));
            AddBoxToCombines(cFrames, new Vector3(minX + 0.1f, 3.7f, centerZ), new Vector3(0.4f, 0.15f, 6.0f));
            AddBoxToCombines(cStructure, new Vector3(minX + 0.2f, 2.0f, minZ + 3.5f), new Vector3(0.4f, 4.0f, 7.0f));
            AddBoxToCombines(cStructure, new Vector3(minX + 0.2f, 2.0f, maxZ - 3.5f), new Vector3(0.4f, 4.0f, 7.0f));

            // --- Upper Floors: Repetitive Dormitory Room Grid ---
            int roomBaysZ = 5;
            float roomDepth = 1.4f;

            for (int f = 1; f < storyCount; f++)
            {
                float floorY = floorHeights[f];
                float storyH = upperStoryH;
                float winY = floorY + 1.1f + (storyH - 1.1f) * 0.5f;
                float winH = storyH - 1.3f;

                // West Facade Loggias:
                for (int b = 0; b < roomBaysZ; b++)
                {
                    float zStart = Mathf.Lerp(minZ + 0.6f, maxZ - 0.6f, (float)b / roomBaysZ);
                    float zEnd = Mathf.Lerp(minZ + 0.6f, maxZ - 0.6f, (float)(b + 1) / roomBaysZ);
                    float bCenterZ = (zStart + zEnd) * 0.5f;
                    float bWidthZ = zEnd - zStart;

                    AddBoxToCombines(cStructure, new Vector3(minX + roomDepth * 0.5f, floorY + storyH * 0.5f, zStart), new Vector3(roomDepth, storyH, 0.2f));
                    AddBoxToCombines(cRailings, new Vector3(minX + 0.05f, floorY + 0.55f, bCenterZ), new Vector3(0.08f, 1.0f, bWidthZ - 0.1f));
                    AddBoxToCombines(cWinOpaque, new Vector3(minX + roomDepth - 0.05f, winY, bCenterZ), new Vector3(0.1f, winH, bWidthZ - 0.4f));
                    AddBoxToCombines(cMint, new Vector3(minX + 0.02f, floorY + storyH - 0.08f, bCenterZ), new Vector3(0.14f, 0.12f, bWidthZ));
                }

                // East Facade: Dorm room window grid:
                for (int b = 0; b < roomBaysZ; b++)
                {
                    float zStart = Mathf.Lerp(minZ + 0.6f, maxZ - 0.6f, (float)b / roomBaysZ);
                    float zEnd = Mathf.Lerp(minZ + 0.6f, maxZ - 0.6f, (float)(b + 1) / roomBaysZ);
                    float bCenterZ = (zStart + zEnd) * 0.5f;
                    float bWidthZ = zEnd - zStart;

                    AddBoxToCombines(cStructure, new Vector3(maxX - 0.2f, floorY + 0.5f, bCenterZ), new Vector3(0.4f, 1.0f, bWidthZ));
                    AddBoxToCombines(cWinOpaque, new Vector3(maxX - 0.05f, winY, bCenterZ), new Vector3(0.1f, winH, bWidthZ - 0.6f));
                    AddBoxToCombines(cFrames, new Vector3(maxX - 0.05f, winY, bCenterZ), new Vector3(0.12f, winH, 0.1f));
                }

                // North & South End Window Bays:
                AddBoxToCombines(cWinOpaque, new Vector3(centerX, winY, minZ + 0.05f), new Vector3(width - 5.0f, winH, 0.1f));
                AddBoxToCombines(cWinOpaque, new Vector3(centerX, winY, maxZ - 0.05f), new Vector3(width - 5.0f, winH, 0.1f));
            }

            // --- Roof & Elevator Penthouse (Height 32.0m to 35.5m) ---
            float roofY = floorHeights[storyCount];
            AddBoxToCombines(cStructure, new Vector3(centerX, roofY + 0.75f, maxZ - 0.15f), new Vector3(width, 1.5f, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(centerX, roofY + 0.75f, minZ + 0.15f), new Vector3(width, 1.5f, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(minX + 0.15f, roofY + 0.75f, centerZ), new Vector3(0.3f, 1.5f, length));
            AddBoxToCombines(cStructure, new Vector3(maxX - 0.15f, roofY + 0.75f, centerZ), new Vector3(0.3f, 1.5f, length));
            AddBoxToCombines(cRoof, new Vector3(centerX, roofY + 0.1f, centerZ), new Vector3(width - 0.6f, 0.1f, length - 0.6f));

            // Elevator Penthouse:
            AddBoxToCombines(cStructure, new Vector3(centerX, roofY + 1.6f, centerZ), new Vector3(5.0f, 3.2f, 5.0f));
            AddBoxToCombines(cRoof, new Vector3(centerX, roofY + 3.25f, centerZ), new Vector3(5.3f, 0.2f, 5.3f));

            // Rooftop Signage Panel "KÝ TÚC XÁ NAM (NHÀ I)" on South face:
            AddBoxToCombines(cFrames, new Vector3(centerX, roofY + 2.2f, minZ + 0.6f), new Vector3(8.5f, 1.6f, 0.2f));
            AddBoxToCombines(cMint, new Vector3(centerX, roofY + 2.2f, minZ + 0.48f), new Vector3(8.1f, 1.3f, 0.08f));

            // Water Tanks / MEP:
            AddBoxToCombines(cRoof, new Vector3(centerX + 2.6f, roofY + 1.2f, maxZ - 2.6f), new Vector3(2.0f, 2.0f, 2.0f));
            AddBoxToCombines(cRoof, new Vector3(centerX - 2.6f, roofY + 1.2f, maxZ - 2.6f), new Vector3(2.0f, 2.0f, 2.0f));

            // Attach Meshes
            AttachCombinedMesh(root, "01_Structure", cStructure, matFacade);
            AttachCombinedMesh(root, "02_Windows_Opaque", cWinOpaque, matWinOpaque);
            AttachCombinedMesh(root, "03_Lobby_Glazing", cWinTrans, matWinTrans);
            AttachCombinedMesh(root, "04_Frames_Canopy", cFrames, matFrame);
            AttachCombinedMesh(root, "05_Accents_Mint", cMint, matMint);
            AttachCombinedMesh(root, "06_Railings", cRailings, matRailing);
            AttachCombinedMesh(root, "07_Plinth_Steps", cBase, matBase);
            AttachCombinedMesh(root, "08_Roof_MEP", cRoof, matRoof);

            // Colliders
            GameObject goCols = new GameObject("Colliders");
            goCols.transform.SetParent(root.transform, false);

            var colUpper = goCols.AddComponent<BoxCollider>();
            colUpper.center = new Vector3(centerX, (totalH + groundH) * 0.5f, centerZ);
            colUpper.size = new Vector3(width, totalH - groundH, length);

            var colGroundCore = goCols.AddComponent<BoxCollider>();
            colGroundCore.center = new Vector3(minX + 2.0f + (width - 2.0f) * 0.5f, groundH * 0.5f, centerZ);
            colGroundCore.size = new Vector3(width - 2.0f, groundH, length);

            var colGroundNorth = goCols.AddComponent<BoxCollider>();
            colGroundNorth.center = new Vector3(minX + 1.0f, groundH * 0.5f, maxZ - 3.5f);
            colGroundNorth.size = new Vector3(2.0f, groundH, 7.0f);

            var colGroundSouth = goCols.AddComponent<BoxCollider>();
            colGroundSouth.center = new Vector3(minX + 1.0f, groundH * 0.5f, minZ + 3.5f);
            colGroundSouth.size = new Vector3(2.0f, groundH, 7.0f);
        }

        // =========================================================================
        // 3. BUILDING G — DORMITORY WOMEN (7 FLOORS, 25.5m)
        // Footprint: X in [17.5, 48.0], Z in [-50.5, -37.2], Y in [0.0, 25.5]
        // =========================================================================
        private static void BuildBuildingG(GameObject parent, Material matFacade, Material matBlue, 
            Material matWinOpaque, Material matWinTrans, Material matFrame, Material matRailing, Material matBase, Material matRoof)
        {
            GameObject root = GetOrCreateBuildingRoot(parent, "Building_G_Dorm_Women");

            float minX = 17.5f, maxX = 48.0f; // Width = 30.5m along East-West
            float minZ = -50.5f, maxZ = -37.2f; // Depth = 13.3m
            float centerX = (minX + maxX) * 0.5f; // 32.75m
            float centerZ = (minZ + maxZ) * 0.5f; // -43.85m
            float width = maxX - minX;
            float length = maxZ - minZ;

            int storyCount = 7;
            float groundH = 3.9f;
            float upperStoryH = 3.3f;
            float totalH = 25.5f;

            List<CombineInstance> cStructure = new List<CombineInstance>();
            List<CombineInstance> cWinOpaque = new List<CombineInstance>();
            List<CombineInstance> cWinTrans = new List<CombineInstance>();
            List<CombineInstance> cFrames = new List<CombineInstance>();
            List<CombineInstance> cBlue = new List<CombineInstance>();
            List<CombineInstance> cRailings = new List<CombineInstance>();
            List<CombineInstance> cBase = new List<CombineInstance>();
            List<CombineInstance> cRoof = new List<CombineInstance>();

            float[] floorHeights = new float[storyCount + 1];
            floorHeights[0] = 0.0f;
            floorHeights[1] = groundH;
            for (int i = 2; i <= storyCount; i++) floorHeights[i] = floorHeights[i - 1] + upperStoryH;

            // --- Floor Slabs ---
            for (int f = 0; f <= storyCount; f++)
            {
                float y = floorHeights[f];
                float thick = (f == 0) ? 0.25f : 0.35f;
                AddBoxToCombines(cStructure, new Vector3(centerX, y + thick * 0.5f, centerZ), new Vector3(width, thick, length));
            }

            // --- Base Plinth & Steps ---
            AddBoxToCombines(cBase, new Vector3(centerX, 0.12f, centerZ), new Vector3(width + 0.1f, 0.24f, length + 0.1f));
            AddBoxToCombines(cBase, new Vector3(centerX, 0.08f, maxZ + 0.1f), new Vector3(7.0f, 0.16f, 0.2f));

            // --- Full-Height Solid End Shear Walls on West and East Ends ---
            AddBoxToCombines(cStructure, new Vector3(minX + 0.15f, totalH * 0.5f, centerZ), new Vector3(0.3f, totalH, length));
            AddBoxToCombines(cStructure, new Vector3(maxX - 0.15f, totalH * 0.5f, centerZ), new Vector3(0.3f, totalH, length));

            // --- Ground Floor Entrance (North Facing into Campus towards C-G Pathway) ---
            AddBoxToCombines(cStructure, new Vector3(centerX, groundH * 0.5f, minZ + 0.2f), new Vector3(width, groundH, 0.4f)); // South wall
            AddBoxToCombines(cWinTrans, new Vector3(centerX, groundH * 0.5f, maxZ - 0.05f), new Vector3(6.5f, 3.5f, 0.1f));
            AddBoxToCombines(cFrames, new Vector3(centerX - 1.6f, groundH * 0.5f, maxZ - 0.05f), new Vector3(0.12f, 3.5f, 0.12f));
            AddBoxToCombines(cFrames, new Vector3(centerX + 1.6f, groundH * 0.5f, maxZ - 0.05f), new Vector3(0.12f, 3.5f, 0.12f));
            AddBoxToCombines(cFrames, new Vector3(centerX, 3.6f, maxZ + 0.1f), new Vector3(7.0f, 0.15f, 0.4f));
            AddBoxToCombines(cStructure, new Vector3(minX + (centerX - 3.8f - minX) * 0.5f, groundH * 0.5f, maxZ - 0.2f), new Vector3(centerX - 3.8f - minX, groundH, 0.4f));
            AddBoxToCombines(cStructure, new Vector3(centerX + 3.8f + (maxX - (centerX + 3.8f)) * 0.5f, groundH * 0.5f, maxZ - 0.2f), new Vector3(maxX - (centerX + 3.8f), groundH, 0.4f));

            // --- Upper Floors: 30.5m Long Facade with 8 Room Bays ---
            int baysX = 8;
            float bayW = (width - 0.6f) / baysX;
            float loggiaDepth = 1.3f;

            for (int f = 1; f < storyCount; f++)
            {
                float floorY = floorHeights[f];
                float storyH = upperStoryH;
                float winY = floorY + 1.0f + (storyH - 1.0f) * 0.5f;
                float winH = storyH - 1.2f;

                // North Facade: Continuous Loggias with Metal Railings & Doors/Windows:
                for (int b = 0; b < baysX; b++)
                {
                    float xStart = (minX + 0.3f) + b * bayW;
                    float xEnd = xStart + bayW;
                    float bCenterX = (xStart + xEnd) * 0.5f;

                    AddBoxToCombines(cStructure, new Vector3(xStart, floorY + storyH * 0.5f, maxZ - loggiaDepth * 0.5f), new Vector3(0.25f, storyH, loggiaDepth));
                    AddBoxToCombines(cRailings, new Vector3(bCenterX, floorY + 0.55f, maxZ - 0.05f), new Vector3(bayW - 0.15f, 1.0f, 0.08f));
                    AddBoxToCombines(cWinOpaque, new Vector3(bCenterX, winY, maxZ - loggiaDepth + 0.05f), new Vector3(bayW - 0.6f, winH, 0.1f));
                    AddBoxToCombines(cBlue, new Vector3(bCenterX, floorY + storyH - 0.08f, maxZ + 0.02f), new Vector3(bayW, 0.12f, 0.14f));
                }

                // South Facade (Street-facing): Structured room window bands:
                for (int b = 0; b < baysX; b++)
                {
                    float xStart = (minX + 0.3f) + b * bayW;
                    float xEnd = xStart + bayW;
                    float bCenterX = (xStart + xEnd) * 0.5f;

                    AddBoxToCombines(cStructure, new Vector3(bCenterX, floorY + 0.5f, minZ + 0.2f), new Vector3(bayW, 1.0f, 0.4f));
                    AddBoxToCombines(cWinOpaque, new Vector3(bCenterX, winY, minZ + 0.05f), new Vector3(bayW - 0.8f, winH, 0.1f));
                    AddBoxToCombines(cFrames, new Vector3(bCenterX, winY, minZ + 0.05f), new Vector3(0.12f, winH, 0.1f));
                }

                // Central Vertical Circulation Accent (Blue Band on both faces):
                AddBoxToCombines(cBlue, new Vector3(centerX, floorY + storyH * 0.5f, maxZ + 0.04f), new Vector3(1.2f, storyH, 0.08f));
                AddBoxToCombines(cBlue, new Vector3(centerX, floorY + storyH * 0.5f, minZ - 0.04f), new Vector3(1.2f, storyH, 0.08f));
            }

            // --- Roof & Elevator Penthouse (Height 23.7m to 25.5m) ---
            float roofY = floorHeights[storyCount];
            AddBoxToCombines(cStructure, new Vector3(centerX, roofY + 0.75f, maxZ - 0.15f), new Vector3(width, 1.5f, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(centerX, roofY + 0.75f, minZ + 0.15f), new Vector3(width, 1.5f, 0.3f));
            AddBoxToCombines(cStructure, new Vector3(minX + 0.15f, roofY + 0.75f, centerZ), new Vector3(0.3f, 1.5f, length));
            AddBoxToCombines(cStructure, new Vector3(maxX - 0.15f, roofY + 0.75f, centerZ), new Vector3(0.3f, 1.5f, length));
            AddBoxToCombines(cRoof, new Vector3(centerX, roofY + 0.1f, centerZ), new Vector3(width - 0.6f, 0.1f, length - 0.6f));

            // Central Elevator Penthouse Tum:
            AddBoxToCombines(cStructure, new Vector3(centerX, roofY + 1.25f, centerZ), new Vector3(6.0f, 2.5f, 4.5f));
            AddBoxToCombines(cRoof, new Vector3(centerX, roofY + 2.55f, centerZ), new Vector3(6.3f, 0.2f, 4.7f));

            // Rooftop Signage Panel "KÝ TÚC XÁ NỮ (NHÀ G)" on South face towards street:
            AddBoxToCombines(cFrames, new Vector3(centerX, roofY + 1.8f, minZ + 0.6f), new Vector3(10.5f, 1.6f, 0.2f));
            AddBoxToCombines(cBlue, new Vector3(centerX, roofY + 1.8f, minZ + 0.48f), new Vector3(10.1f, 1.3f, 0.08f));

            // Attach Meshes
            AttachCombinedMesh(root, "01_Structure", cStructure, matFacade);
            AttachCombinedMesh(root, "02_Windows_Opaque", cWinOpaque, matWinOpaque);
            AttachCombinedMesh(root, "03_Lobby_Glazing", cWinTrans, matWinTrans);
            AttachCombinedMesh(root, "04_Frames_Canopy", cFrames, matFrame);
            AttachCombinedMesh(root, "05_Accents_Blue", cBlue, matBlue);
            AttachCombinedMesh(root, "06_Railings", cRailings, matRailing);
            AttachCombinedMesh(root, "07_Plinth_Steps", cBase, matBase);
            AttachCombinedMesh(root, "08_Roof_MEP", cRoof, matRoof);

            // Colliders
            GameObject goCols = new GameObject("Colliders");
            goCols.transform.SetParent(root.transform, false);

            var colUpper = goCols.AddComponent<BoxCollider>();
            colUpper.center = new Vector3(centerX, (totalH + groundH) * 0.5f, centerZ);
            colUpper.size = new Vector3(width, totalH - groundH, length);

            var colGroundCore = goCols.AddComponent<BoxCollider>();
            colGroundCore.center = new Vector3(centerX, groundH * 0.5f, minZ + (length - 2.0f) * 0.5f);
            colGroundCore.size = new Vector3(width, groundH, length - 2.0f);

            var colGroundEast = goCols.AddComponent<BoxCollider>();
            colGroundEast.center = new Vector3(maxX - 3.5f, groundH * 0.5f, maxZ - 1.0f);
            colGroundEast.size = new Vector3(7.0f, groundH, 2.0f);

            var colGroundWest = goCols.AddComponent<BoxCollider>();
            colGroundWest.center = new Vector3(minX + 3.5f, groundH * 0.5f, maxZ - 1.0f);
            colGroundWest.size = new Vector3(7.0f, groundH, 2.0f);
        }
    }
}
