using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IUHCampus.Editor
{
    public static class IUHBuildingEBuilder
    {
        private const string ROOT_PATH = "_REWORK_V2/02_BUILDINGS";
        private const string BUILDING_NAME = "Building_E_Admin";
        private const string MAT_DIR = "Assets/IUH_Campus/Materials/Rework/E_Admin/";

        [MenuItem("IUH Campus/Build Building E (Admin)", false, 101)]
        public static void BuildBuildingE()
        {
            var rework = GameObject.Find("_REWORK_V2");
            if (rework == null)
            {
                Debug.LogError("[IUHBuildingEBuilder] _REWORK_V2 not found!");
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

            // Remove previous version if exists
            var existing = buildingsParent.transform.Find(BUILDING_NAME);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            // Load Materials
            Material matFacade = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Facade_White.mat");
            Material matGlass = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Glass.mat");
            Material matFrame = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Frame_Dark.mat");
            Material matLouvers = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Louvers_Cyan.mat");
            Material matSign = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Sign_CurtainWall.mat");
            Material matStone = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Stone_Lobby.mat");
            Material matWood = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Wood_Wall.mat");
            Material matGranite = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Granite_Steps.mat");
            Material matMonument = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Monument_Stone.mat");
            Material matLawn = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Lawn_Island.mat");
            Material matRoof = AssetDatabase.LoadAssetAtPath<Material>(MAT_DIR + "MAT_E_Roof_Dark.mat");

            // Root GameObject
            GameObject root = new GameObject(BUILDING_NAME);
            Undo.RegisterCreatedObjectUndo(root, "Create Building E Admin");
            root.transform.SetParent(buildingsParent.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            // --- PARAMETERS ---
            // Building envelope: X in [-14, 14], Y in [0, 20.6], Z in [2.5, 19.0]
            float width = 28f;
            float halfWidth = 14f;
            float rearZ = 19.0f; // strictly <= 19.0 to ensure 1.0m gap to E4 (Z=20.0) -> ZERO overlap across X, Y, Z
            float frontCenterZ = 3.0f;
            float frontCornerZ = 6.0f;
            float totalHeight = 20.6f;

            // 5 readable architectural stories + roof parapet
            float[] floorY = new float[] { 0.0f, 4.5f, 8.3f, 12.1f, 15.9f, 19.4f, 20.6f };

            // Helper function for front facade curved profile Z at X
            System.Func<float, float> getFrontZ = (x) =>
            {
                float t = Mathf.Clamp01(Mathf.Abs(x) / halfWidth);
                return Mathf.Lerp(frontCenterZ, frontCornerZ, t * t);
            };

            // -------------------------------------------------------------
            // 1. STRUCTURE CORE
            // -------------------------------------------------------------
            GameObject goCore = new GameObject("01_Structure_Core");
            goCore.transform.SetParent(root.transform, false);

            // Floor Slabs (Ground, L2, L3, L4, L5, Roof)
            List<CombineInstance> slabCombines = new List<CombineInstance>();
            for (int f = 0; f < floorY.Length - 1; f++)
            {
                float y = floorY[f];
                float thickness = (f == 0) ? 0.2f : 0.35f;
                float slabDepth = rearZ - frontCenterZ;
                float slabCenterZ = (rearZ + frontCenterZ) * 0.5f;
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(0f, y + thickness * 0.5f, slabCenterZ);
                cube.transform.localScale = new Vector3(width, thickness, slabDepth);

                CombineInstance ci = new CombineInstance();
                ci.mesh = cube.GetComponent<MeshFilter>().sharedMesh;
                ci.transform = cube.transform.localToWorldMatrix;
                slabCombines.Add(ci);
                Object.DestroyImmediate(cube);
            }
            GameObject goSlabs = new GameObject("Floor_Slabs");
            goSlabs.transform.SetParent(goCore.transform, false);
            var mfSlabs = goSlabs.AddComponent<MeshFilter>();
            var mrSlabs = goSlabs.AddComponent<MeshRenderer>();
            Mesh combinedSlabs = new Mesh();
            combinedSlabs.name = "Combined_Slabs";
            combinedSlabs.CombineMeshes(slabCombines.ToArray(), true, true);
            mfSlabs.sharedMesh = combinedSlabs;
            mrSlabs.sharedMaterial = matFacade;

            // Rear Wall (North facade facing E4 / walkway): strictly Z = 18.75 to 19.0
            GameObject goRear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goRear.name = "Rear_Wall_North";
            goRear.transform.SetParent(goCore.transform, false);
            goRear.transform.position = new Vector3(0f, totalHeight * 0.5f, rearZ - 0.25f);
            goRear.transform.localScale = new Vector3(width, totalHeight, 0.5f);
            goRear.GetComponent<MeshRenderer>().sharedMaterial = matFacade;
            Object.DestroyImmediate(goRear.GetComponent<Collider>());

            // Side Walls (West and East)
            List<CombineInstance> sideCombines = new List<CombineInstance>();
            float sideWallDepth = rearZ - frontCornerZ;
            float sideWallCenterZ = (rearZ + frontCornerZ) * 0.5f;

            // West Wall
            var westCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westCube.transform.position = new Vector3(-halfWidth + 0.25f, totalHeight * 0.5f, sideWallCenterZ);
            westCube.transform.localScale = new Vector3(0.5f, totalHeight, sideWallDepth);
            CombineInstance ciWest = new CombineInstance();
            ciWest.mesh = westCube.GetComponent<MeshFilter>().sharedMesh;
            ciWest.transform = westCube.transform.localToWorldMatrix;
            sideCombines.Add(ciWest);
            Object.DestroyImmediate(westCube);

            // East Wall
            var eastCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastCube.transform.position = new Vector3(halfWidth - 0.25f, totalHeight * 0.5f, sideWallCenterZ);
            eastCube.transform.localScale = new Vector3(0.5f, totalHeight, sideWallDepth);
            CombineInstance ciEast = new CombineInstance();
            ciEast.mesh = eastCube.GetComponent<MeshFilter>().sharedMesh;
            ciEast.transform = eastCube.transform.localToWorldMatrix;
            sideCombines.Add(ciEast);
            Object.DestroyImmediate(eastCube);

            GameObject goSideWalls = new GameObject("Side_Walls");
            goSideWalls.transform.SetParent(goCore.transform, false);
            var mfSides = goSideWalls.AddComponent<MeshFilter>();
            var mrSides = goSideWalls.AddComponent<MeshRenderer>();
            Mesh combinedSides = new Mesh();
            combinedSides.name = "Combined_SideWalls";
            combinedSides.CombineMeshes(sideCombines.ToArray(), true, true);
            mfSides.sharedMesh = combinedSides;
            mrSides.sharedMaterial = matFacade;

            // Structural Interior Columns
            List<CombineInstance> colCombines = new List<CombineInstance>();
            float[] colX = new float[] { -10f, -5f, 0f, 5f, 10f };
            float[] colZ = new float[] { 8f, 14f };
            foreach (float cx in colX)
            {
                foreach (float cz in colZ)
                {
                    var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cyl.transform.position = new Vector3(cx, totalHeight * 0.5f, cz);
                    cyl.transform.localScale = new Vector3(0.7f, totalHeight * 0.5f, 0.7f);
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = cyl.GetComponent<MeshFilter>().sharedMesh;
                    ci.transform = cyl.transform.localToWorldMatrix;
                    colCombines.Add(ci);
                    Object.DestroyImmediate(cyl);
                }
            }
            GameObject goColumns = new GameObject("Structural_Columns");
            goColumns.transform.SetParent(goCore.transform, false);
            var mfCols = goColumns.AddComponent<MeshFilter>();
            var mrCols = goColumns.AddComponent<MeshRenderer>();
            Mesh combinedCols = new Mesh();
            combinedCols.name = "Combined_Columns";
            combinedCols.CombineMeshes(colCombines.ToArray(), true, true);
            mfCols.sharedMesh = combinedCols;
            mrCols.sharedMaterial = matFacade;

            // -------------------------------------------------------------
            // 2. FACADE SOUTH CURVED (Center Atrium + Louver Wings)
            // -------------------------------------------------------------
            GameObject goFacade = new GameObject("02_Facade_South_Curved");
            goFacade.transform.SetParent(root.transform, false);

            int numSegments = 14;
            float segWidth = width / numSegments; // 2.0m

            List<CombineInstance> glassCombines = new List<CombineInstance>();
            List<CombineInstance> frameCombines = new List<CombineInstance>();
            List<CombineInstance> louverCombines = new List<CombineInstance>();

            // Spandrel horizontal bands across the curved facade
            for (int f = 1; f < floorY.Length - 1; f++)
            {
                float y = floorY[f];
                float spandrelH = 0.5f;

                for (int s = 0; s < numSegments; s++)
                {
                    float xMid = -halfWidth + (s + 0.5f) * segWidth;
                    float zMid = getFrontZ(xMid);

                    float x1 = -halfWidth + s * segWidth;
                    float z1 = getFrontZ(x1);
                    float x2 = -halfWidth + (s + 1) * segWidth;
                    float z2 = getFrontZ(x2);

                    Vector3 dir = new Vector3(x2 - x1, 0f, z2 - z1);
                    float segLen = dir.magnitude;
                    Quaternion rot = Quaternion.LookRotation(new Vector3(-dir.z, 0f, dir.x));

                    var spandrel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    spandrel.transform.position = new Vector3(xMid, y, zMid);
                    spandrel.transform.rotation = rot;
                    spandrel.transform.localScale = new Vector3(segLen * 1.02f, spandrelH, 0.35f);

                    CombineInstance ci = new CombineInstance();
                    ci.mesh = spandrel.GetComponent<MeshFilter>().sharedMesh;
                    ci.transform = spandrel.transform.localToWorldMatrix;
                    frameCombines.Add(ci);
                    Object.DestroyImmediate(spandrel);
                }
            }

            // Glass panels and vertical louvers for all floors
            for (int f = 0; f < floorY.Length - 2; f++)
            {
                float yBot = floorY[f] + ((f == 0) ? 0f : 0.25f);
                float yTop = floorY[f + 1] - 0.25f;
                float panelH = yTop - yBot;
                float panelYMid = (yTop + yBot) * 0.5f;

                for (int s = 0; s < numSegments; s++)
                {
                    float xMid = -halfWidth + (s + 0.5f) * segWidth;
                    float zMid = getFrontZ(xMid);

                    float x1 = -halfWidth + s * segWidth;
                    float z1 = getFrontZ(x1);
                    float x2 = -halfWidth + (s + 1) * segWidth;
                    float z2 = getFrontZ(x2);

                    Vector3 dir = new Vector3(x2 - x1, 0f, z2 - z1);
                    float segLen = dir.magnitude;
                    Quaternion rot = Quaternion.LookRotation(new Vector3(-dir.z, 0f, dir.x));

                    // At level 0 (Ground), leave center opening at x in [-1.5, 1.5] for entrance doorway
                    bool isDoorOpening = (f == 0 && Mathf.Abs(xMid) < 1.6f);

                    if (!isDoorOpening)
                    {
                        var glassCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        glassCube.transform.position = new Vector3(xMid, panelYMid, zMid);
                        glassCube.transform.rotation = rot;
                        glassCube.transform.localScale = new Vector3(segLen * 0.98f, panelH, 0.08f);

                        CombineInstance ciG = new CombineInstance();
                        ciG.mesh = glassCube.GetComponent<MeshFilter>().sharedMesh;
                        ciG.transform = glassCube.transform.localToWorldMatrix;
                        glassCombines.Add(ciG);
                        Object.DestroyImmediate(glassCube);
                    }
                    else
                    {
                        // Doorway frame posts
                        var doorFrameL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        doorFrameL.transform.position = new Vector3(x1, panelYMid, z1);
                        doorFrameL.transform.rotation = rot;
                        doorFrameL.transform.localScale = new Vector3(0.12f, panelH, 0.25f);
                        CombineInstance ciDF = new CombineInstance();
                        ciDF.mesh = doorFrameL.GetComponent<MeshFilter>().sharedMesh;
                        ciDF.transform = doorFrameL.transform.localToWorldMatrix;
                        frameCombines.Add(ciDF);
                        Object.DestroyImmediate(doorFrameL);
                    }

                    // Vertical Mullions
                    var mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    mullion.transform.position = new Vector3(x1, panelYMid, z1);
                    mullion.transform.rotation = rot;
                    mullion.transform.localScale = new Vector3(0.12f, panelH, 0.25f);

                    CombineInstance ciM = new CombineInstance();
                    ciM.mesh = mullion.GetComponent<MeshFilter>().sharedMesh;
                    ciM.transform = mullion.transform.localToWorldMatrix;
                    frameCombines.Add(ciM);
                    Object.DestroyImmediate(mullion);

                    // Vertical louvers (fins) on wings (Floors 2, 3, 4, 5, outside center bay |x| > 4.5)
                    if (f >= 1 && Mathf.Abs(xMid) > 4.5f)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            float subX = x1 + (k + 0.5f) * (segWidth / 2f);
                            float subZ = getFrontZ(subX);

                            var fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            fin.transform.position = new Vector3(subX, panelYMid, subZ - 0.22f);
                            fin.transform.rotation = rot;
                            fin.transform.localScale = new Vector3(0.12f, panelH, 0.35f);

                            CombineInstance ciF = new CombineInstance();
                            ciF.mesh = fin.GetComponent<MeshFilter>().sharedMesh;
                            ciF.transform = fin.transform.localToWorldMatrix;
                            louverCombines.Add(ciF);
                            Object.DestroyImmediate(fin);
                        }
                    }
                }
            }

            // Combine into single GameObject per category
            GameObject goGlass = new GameObject("Curved_Glass_Curtain");
            goGlass.transform.SetParent(goFacade.transform, false);
            var mfGlass = goGlass.AddComponent<MeshFilter>();
            var mrGlass = goGlass.AddComponent<MeshRenderer>();
            Mesh combinedGlass = new Mesh();
            combinedGlass.name = "Combined_Glass";
            combinedGlass.CombineMeshes(glassCombines.ToArray(), true, true);
            mfGlass.sharedMesh = combinedGlass;
            mrGlass.sharedMaterial = matGlass;

            GameObject goFrames = new GameObject("Curved_Frames_Spandrels");
            goFrames.transform.SetParent(goFacade.transform, false);
            var mfFrames = goFrames.AddComponent<MeshFilter>();
            var mrFrames = goFrames.AddComponent<MeshRenderer>();
            Mesh combinedFrames = new Mesh();
            combinedFrames.name = "Combined_Frames";
            combinedFrames.CombineMeshes(frameCombines.ToArray(), true, true);
            mfFrames.sharedMesh = combinedFrames;
            mrFrames.sharedMaterial = matFrame;

            GameObject goLouvers = new GameObject("Vertical_Louvers_Cyan");
            goLouvers.transform.SetParent(goFacade.transform, false);
            var mfLouvers = goLouvers.AddComponent<MeshFilter>();
            var mrLouvers = goLouvers.AddComponent<MeshRenderer>();
            Mesh combinedLouvers = new Mesh();
            combinedLouvers.name = "Combined_Louvers";
            combinedLouvers.CombineMeshes(louverCombines.ToArray(), true, true);
            mfLouvers.sharedMesh = combinedLouvers;
            mrLouvers.sharedMaterial = matLouvers;

            // 4. Center Signage Panel (Dedicated IUH Wordmark panel)
            // Exact 2:1 aspect ratio: 6.0m wide x 3.0m high, facing South (rotation = 0, 0, 0)
            GameObject goSign = GameObject.CreatePrimitive(PrimitiveType.Quad);
            goSign.name = "Signage_Panel_IUH";
            goSign.transform.SetParent(goFacade.transform, false);
            goSign.transform.position = new Vector3(0f, 10.0f, frontCenterZ - 0.12f);
            goSign.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // South-facing (normal = 0, 0, -1)
            goSign.transform.localScale = new Vector3(6.0f, 3.0f, 1.0f);
            goSign.GetComponent<MeshRenderer>().sharedMaterial = matSign;
            Object.DestroyImmediate(goSign.GetComponent<Collider>());

            // Slim dark frame around sign
            var signBacking = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBacking.name = "Signage_Backing_Frame";
            signBacking.transform.SetParent(goSign.transform, false);
            signBacking.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            signBacking.transform.localScale = new Vector3(1.04f, 1.04f, 0.1f);
            signBacking.GetComponent<MeshRenderer>().sharedMaterial = matFrame;
            Object.DestroyImmediate(signBacking.GetComponent<Collider>());

            // 5. Entrance Canopy (Mái đón cong vươn ra sảnh)
            GameObject goCanopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goCanopy.name = "Entrance_Canopy";
            goCanopy.transform.SetParent(goFacade.transform, false);
            goCanopy.transform.position = new Vector3(0f, 3.8f, 1.6f);
            goCanopy.transform.localScale = new Vector3(9.0f, 0.22f, 2.8f);
            goCanopy.GetComponent<MeshRenderer>().sharedMaterial = matFrame;
            Object.DestroyImmediate(goCanopy.GetComponent<Collider>());

            var postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postL.name = "Canopy_Post_L";
            postL.transform.SetParent(goCanopy.transform, false);
            postL.transform.localPosition = new Vector3(-0.45f, -8.6f, -0.42f);
            postL.transform.localScale = new Vector3(0.035f, 8.6f, 0.035f);
            postL.GetComponent<MeshRenderer>().sharedMaterial = matFrame;
            Object.DestroyImmediate(postL.GetComponent<Collider>());

            var postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postR.name = "Canopy_Post_R";
            postR.transform.SetParent(goCanopy.transform, false);
            postR.transform.localPosition = new Vector3(0.45f, -8.6f, -0.42f);
            postR.transform.localScale = new Vector3(0.035f, 8.6f, 0.035f);
            postR.GetComponent<MeshRenderer>().sharedMaterial = matFrame;
            Object.DestroyImmediate(postR.GetComponent<Collider>());

            // -------------------------------------------------------------
            // 3. GROUND LOBBY SHALLOW INTERIOR
            // -------------------------------------------------------------
            GameObject goLobby = new GameObject("03_Ground_Lobby_Shallow");
            goLobby.transform.SetParent(root.transform, false);

            GameObject lobbyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lobbyFloor.name = "Lobby_Floor_Marble";
            lobbyFloor.transform.SetParent(goLobby.transform, false);
            lobbyFloor.transform.position = new Vector3(0f, 0.02f, 7.0f);
            lobbyFloor.transform.localScale = new Vector3(20.0f, 0.04f, 8.0f);
            lobbyFloor.GetComponent<MeshRenderer>().sharedMaterial = matStone;
            Object.DestroyImmediate(lobbyFloor.GetComponent<Collider>());

            GameObject lobbyWoodWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lobbyWoodWall.name = "Lobby_Feature_WoodWall";
            lobbyWoodWall.transform.SetParent(goLobby.transform, false);
            lobbyWoodWall.transform.position = new Vector3(0f, 2.25f, 9.5f);
            lobbyWoodWall.transform.localScale = new Vector3(14.0f, 4.5f, 0.3f);
            lobbyWoodWall.GetComponent<MeshRenderer>().sharedMaterial = matWood;
            Object.DestroyImmediate(lobbyWoodWall.GetComponent<Collider>());

            GameObject recepDesk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            recepDesk.name = "Lobby_Reception_Desk";
            recepDesk.transform.SetParent(goLobby.transform, false);
            recepDesk.transform.position = new Vector3(0f, 0.55f, 6.8f);
            recepDesk.transform.localScale = new Vector3(5.0f, 1.1f, 1.2f);
            recepDesk.GetComponent<MeshRenderer>().sharedMaterial = matFacade;
            Object.DestroyImmediate(recepDesk.GetComponent<Collider>());

            var recepAccent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            recepAccent.name = "Reception_Accent_Trim";
            recepAccent.transform.SetParent(recepDesk.transform, false);
            recepAccent.transform.localPosition = new Vector3(0f, -0.1f, -0.52f);
            recepAccent.transform.localScale = new Vector3(0.96f, 0.6f, 0.1f);
            recepAccent.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(recepAccent.GetComponent<Collider>());

            var stairL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairL.name = "Stair_West_Silhouette";
            stairL.transform.SetParent(goLobby.transform, false);
            stairL.transform.position = new Vector3(-8.5f, 2.25f, 7.5f);
            stairL.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            stairL.transform.localScale = new Vector3(2.5f, 0.3f, 5.0f);
            stairL.GetComponent<MeshRenderer>().sharedMaterial = matFrame;
            Object.DestroyImmediate(stairL.GetComponent<Collider>());

            var stairR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairR.name = "Stair_East_Silhouette";
            stairR.transform.SetParent(goLobby.transform, false);
            stairR.transform.position = new Vector3(8.5f, 2.25f, 7.5f);
            stairR.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            stairR.transform.localScale = new Vector3(2.5f, 0.3f, 5.0f);
            stairR.GetComponent<MeshRenderer>().sharedMaterial = matFrame;
            Object.DestroyImmediate(stairR.GetComponent<Collider>());

            // -------------------------------------------------------------
            // 4. ENTRANCE PLAZA & MONUMENT ISLAND
            // -------------------------------------------------------------
            GameObject goPlaza = new GameObject("04_Entrance_Plaza");
            goPlaza.transform.SetParent(root.transform, false);

            // Granite Steps (3 steps of 0.12m rise, width 10m): Z from 1.6m to 2.8m
            for (int st = 0; st < 3; st++)
            {
                float sy = (st + 0.5f) * 0.12f;
                float sz = 1.6f + st * 0.45f;
                var stepCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stepCube.name = "Entrance_Step_" + (st + 1);
                stepCube.transform.SetParent(goPlaza.transform, false);
                stepCube.transform.position = new Vector3(0f, sy, sz);
                stepCube.transform.localScale = new Vector3(10.0f, 0.12f, 0.45f);
                stepCube.GetComponent<MeshRenderer>().sharedMaterial = matGranite;
                // Leave clean BoxCollider on step: 0.12m rise is well within stepOffset (0.28m)
            }

            // Accessible Ramp on West side of steps
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Entrance_Ramp_West";
            ramp.transform.SetParent(goPlaza.transform, false);
            ramp.transform.position = new Vector3(-6.2f, 0.18f, 2.0f);
            ramp.transform.rotation = Quaternion.Euler(7.0f, 0f, 0f);
            ramp.transform.localScale = new Vector3(2.2f, 0.12f, 3.0f);
            ramp.GetComponent<MeshRenderer>().sharedMaterial = matGranite;

            // Entrance Landing: Z from 2.9m to 3.8m
            var landing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landing.name = "Entrance_Landing";
            landing.transform.SetParent(goPlaza.transform, false);
            landing.transform.position = new Vector3(0f, 0.18f, 3.2f);
            landing.transform.localScale = new Vector3(12.0f, 0.36f, 1.2f);
            landing.GetComponent<MeshRenderer>().sharedMaterial = matGranite;

            // Island Monument in Courtyard (Oval grass island + white rock monument)
            // Position: X = 0, Z = -1.2m
            GameObject goIsland = new GameObject("IUH_Monument_Island");
            goIsland.transform.SetParent(goPlaza.transform, false);
            goIsland.transform.position = new Vector3(0f, 0f, -1.2f);

            // Island Curb (White beveled ring, low 0.10m curb so player can step or walk around)
            var curb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            curb.name = "Island_Curb";
            curb.transform.SetParent(goIsland.transform, false);
            curb.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            curb.transform.localScale = new Vector3(6.5f, 0.06f, 3.8f);
            curb.GetComponent<MeshRenderer>().sharedMaterial = matFacade;
            Object.DestroyImmediate(curb.GetComponent<Collider>());

            // Island Lawn (Green grass surface)
            var lawn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lawn.name = "Island_Lawn";
            lawn.transform.SetParent(goIsland.transform, false);
            lawn.transform.localPosition = new Vector3(0f, 0.10f, 0f);
            lawn.transform.localScale = new Vector3(6.1f, 0.04f, 3.4f);
            lawn.GetComponent<MeshRenderer>().sharedMaterial = matLawn;
            Object.DestroyImmediate(lawn.GetComponent<Collider>());

            // Sculpted Stone Pedestal for "IUH"
            var stoneBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stoneBase.name = "Monument_WhiteStone";
            stoneBase.transform.SetParent(goIsland.transform, false);
            stoneBase.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            stoneBase.transform.localScale = new Vector3(3.6f, 0.8f, 1.2f);
            stoneBase.GetComponent<MeshRenderer>().sharedMaterial = matMonument;
            // Stone has BoxCollider so player doesn't walk through the rock

            // 3D "IUH" letters on stone
            // Letter 'I'
            var letI = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letI.name = "Letter_I";
            letI.transform.SetParent(stoneBase.transform, false);
            letI.transform.localPosition = new Vector3(-0.30f, 0.65f, -0.32f);
            letI.transform.localScale = new Vector3(0.18f, 0.9f, 0.22f);
            letI.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(letI.GetComponent<Collider>());

            // Letter 'U'
            var letUL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letUL.name = "Letter_U_Left";
            letUL.transform.SetParent(stoneBase.transform, false);
            letUL.transform.localPosition = new Vector3(-0.12f, 0.65f, -0.32f);
            letUL.transform.localScale = new Vector3(0.12f, 0.9f, 0.22f);
            letUL.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(letUL.GetComponent<Collider>());

            var letUR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letUR.name = "Letter_U_Right";
            letUR.transform.SetParent(stoneBase.transform, false);
            letUR.transform.localPosition = new Vector3(0.12f, 0.65f, -0.32f);
            letUR.transform.localScale = new Vector3(0.12f, 0.9f, 0.22f);
            letUR.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(letUR.GetComponent<Collider>());

            var letUB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letUB.name = "Letter_U_Bottom";
            letUB.transform.SetParent(stoneBase.transform, false);
            letUB.transform.localPosition = new Vector3(0f, 0.25f, -0.32f);
            letUB.transform.localScale = new Vector3(0.36f, 0.15f, 0.22f);
            letUB.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(letUB.GetComponent<Collider>());

            // Letter 'H'
            var letHL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letHL.name = "Letter_H_Left";
            letHL.transform.SetParent(stoneBase.transform, false);
            letHL.transform.localPosition = new Vector3(0.24f, 0.65f, -0.32f);
            letHL.transform.localScale = new Vector3(0.12f, 0.9f, 0.22f);
            letHL.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(letHL.GetComponent<Collider>());

            var letHR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letHR.name = "Letter_H_Right";
            letHR.transform.SetParent(stoneBase.transform, false);
            letHR.transform.localPosition = new Vector3(0.44f, 0.65f, -0.32f);
            letHR.transform.localScale = new Vector3(0.12f, 0.9f, 0.22f);
            letHR.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(letHR.GetComponent<Collider>());

            var letHM = GameObject.CreatePrimitive(PrimitiveType.Cube);
            letHM.name = "Letter_H_Mid";
            letHM.transform.SetParent(stoneBase.transform, false);
            letHM.transform.localPosition = new Vector3(0.34f, 0.65f, -0.32f);
            letHM.transform.localScale = new Vector3(0.32f, 0.15f, 0.22f);
            letHM.GetComponent<MeshRenderer>().sharedMaterial = matLouvers;
            Object.DestroyImmediate(letHM.GetComponent<Collider>());

            // -------------------------------------------------------------
            // 5. ROOF & MECHANICAL
            // -------------------------------------------------------------
            GameObject goRoof = new GameObject("05_Roof_Mechanical");
            goRoof.transform.SetParent(root.transform, false);

            // Roof Parapet Cornice (Curved top cap)
            List<CombineInstance> parapetCombines = new List<CombineInstance>();
            for (int s = 0; s < numSegments; s++)
            {
                float xMid = -halfWidth + (s + 0.5f) * segWidth;
                float zMid = getFrontZ(xMid);
                float x1 = -halfWidth + s * segWidth;
                float z1 = getFrontZ(x1);
                float x2 = -halfWidth + (s + 1) * segWidth;
                float z2 = getFrontZ(x2);

                Vector3 dir = new Vector3(x2 - x1, 0f, z2 - z1);
                float segLen = dir.magnitude;
                Quaternion rot = Quaternion.LookRotation(new Vector3(-dir.z, 0f, dir.x));

                var parCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                parCube.transform.position = new Vector3(xMid, 20.0f, zMid);
                parCube.transform.rotation = rot;
                parCube.transform.localScale = new Vector3(segLen * 1.02f, 1.2f, 0.45f);

                CombineInstance ci = new CombineInstance();
                ci.mesh = parCube.GetComponent<MeshFilter>().sharedMesh;
                ci.transform = parCube.transform.localToWorldMatrix;
                parapetCombines.Add(ci);
                Object.DestroyImmediate(parCube);
            }
            GameObject goParapet = new GameObject("Roof_Parapet_Cornice");
            goParapet.transform.SetParent(goRoof.transform, false);
            var mfPar = goParapet.AddComponent<MeshFilter>();
            var mrPar = goParapet.AddComponent<MeshRenderer>();
            Mesh combinedPar = new Mesh();
            combinedPar.name = "Combined_Parapet";
            combinedPar.CombineMeshes(parapetCombines.ToArray(), true, true);
            mfPar.sharedMesh = combinedPar;
            mrPar.sharedMaterial = matFrame;

            // Stair/Elevator Headhouse
            var headhouse = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headhouse.name = "Elevator_Headhouse";
            headhouse.transform.SetParent(goRoof.transform, false);
            headhouse.transform.position = new Vector3(0f, 21.3f, 15.0f);
            headhouse.transform.localScale = new Vector3(6.0f, 2.6f, 4.5f);
            headhouse.GetComponent<MeshRenderer>().sharedMaterial = matFacade;
            Object.DestroyImmediate(headhouse.GetComponent<Collider>());

            // 3 Industrial HVAC Chillers
            for (int h = 0; h < 3; h++)
            {
                float hx = -6f + h * 6f;
                var chiller = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chiller.name = "HVAC_Chiller_" + (h + 1);
                chiller.transform.SetParent(goRoof.transform, false);
                chiller.transform.position = new Vector3(hx, 20.4f, 10.0f);
                chiller.transform.localScale = new Vector3(2.4f, 1.4f, 1.8f);
                chiller.GetComponent<MeshRenderer>().sharedMaterial = matFrame;
                Object.DestroyImmediate(chiller.GetComponent<Collider>());
            }

            // -------------------------------------------------------------
            // 6. COLLIDERS (Clean Compound BoxColliders)
            // -------------------------------------------------------------
            GameObject goColl = new GameObject("06_Colliders");
            goColl.transform.SetParent(root.transform, false);

            var colL = goColl.AddComponent<BoxCollider>();
            colL.center = new Vector3(-8.5f, totalHeight * 0.5f, 11.5f);
            colL.size = new Vector3(11.0f, totalHeight, 14.5f);

            var colR = goColl.AddComponent<BoxCollider>();
            colR.center = new Vector3(8.5f, totalHeight * 0.5f, 11.5f);
            colR.size = new Vector3(11.0f, totalHeight, 14.5f);

            var colRear = goColl.AddComponent<BoxCollider>();
            colRear.center = new Vector3(0f, totalHeight * 0.5f, rearZ - 0.5f);
            colRear.size = new Vector3(width, totalHeight, 1.0f);

            var colFloor = goColl.AddComponent<BoxCollider>();
            colFloor.center = new Vector3(0f, 0.225f, 6.0f);
            colFloor.size = new Vector3(width, 0.45f, 8.0f);

            // -------------------------------------------------------------
            // 7. DISABLE OLD BLOCKOUT (BLK_E_Admin) to prevent Z-Fighting
            // -------------------------------------------------------------
            var blkE = GameObject.Find("BLK_E_Admin");
            if (blkE != null)
            {
                foreach (var mr in blkE.GetComponentsInChildren<MeshRenderer>())
                {
                    mr.enabled = false;
                }
                foreach (var c in blkE.GetComponentsInChildren<Collider>())
                {
                    c.enabled = false;
                }
                Debug.Log("[IUHBuildingEBuilder] Disabled Renderers and Colliders on BLK_E_Admin.");
            }

            EditorUtility.SetDirty(root);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            int goCount = root.GetComponentsInChildren<Transform>(true).Length;
            int rendCount = root.GetComponentsInChildren<Renderer>(true).Length;
            HashSet<Material> mats = new HashSet<Material>();
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var m in r.sharedMaterials)
                {
                    if (m != null) mats.Add(m);
                }
            }

            Debug.Log($"[IUHBuildingEBuilder] Built Building_E_Admin successfully! Total GameObjects={goCount}, Renderers={rendCount}, Materials={mats.Count}");
        }
    }
}
