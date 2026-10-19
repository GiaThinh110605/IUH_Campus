using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus.Interaction;
using IUHCampus.Player;

namespace IUHCampus.Editor
{
    public static class IUHSetupStairsAndBenches
    {
        [MenuItem("IUH Campus/Fix Collisions & Setup Stairs & Benches")]
        public static void RunSetup()
        {
            var campus = GameObject.Find("campus_P0_progress");
            if (campus == null)
            {
                Debug.LogError("[IUHSetup] campus_P0_progress GameObject not found in current scene!");
                return;
            }

            int stairsColliders = 0;
            int benchColliders = 0;
            int benchSeats = 0;
            int archColliders = 0;
            int obstacleColliders = 0;

            string[] archKeywords = new string[] {
                "floor", "walkway", "road", "wall", "body", "column", "pillar",
                "stage", "curb", "plaza", "courtyard", "ground", "ramp", "deck",
                "corridor", "hall", "fence", "gate", "barrier"
            };

            string[] obstacleKeywords = new string[] {
                "trashbin", "sign", "bike", "motorbike", "motorcycle", "planter", "treegrate", "hedge"
            };

            // 1. Process all objects in campus_P0_progress
            foreach (Transform t in campus.transform)
            {
                string name = t.name;
                string lower = name.ToLower();

                // Benches
                if (lower.StartsWith("bench_") || lower.StartsWith("stonebench_"))
                {
                    // Ensure MeshCollider
                    if (t.GetComponent<Collider>() == null)
                    {
                        var mf = t.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
                        {
                            var mc = t.gameObject.AddComponent<MeshCollider>();
                            mc.convex = false;
                            benchColliders++;
                        }
                    }

                    // Attach IUHBenchSeat only to actual seats, not legs or backs
                    if (lower.Contains("seat") || (lower.StartsWith("bench_") && !lower.Contains("leg") && !lower.Contains("back")))
                    {
                        if (t.GetComponent<IUHBenchSeat>() == null)
                        {
                            var seat = t.gameObject.AddComponent<IUHBenchSeat>();
                            seat.EnsureCollider();
                            benchSeats++;
                        }
                    }
                    else
                    {
                        // Clean up inadvertent bench seats from non-seat parts
                        var oldSeat = t.GetComponent<IUHBenchSeat>();
                        if (oldSeat != null) Object.DestroyImmediate(oldSeat);
                    }
                    continue;
                }

                // Stairs
                if (lower.Contains("stair"))
                {
                    // If it is an individual step, we will handle it with smooth ramps in SetupOutdoorStairs
                    if (lower.Contains("step")) continue;

                    if (t.GetComponent<Collider>() == null)
                    {
                        var mf = t.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
                        {
                            var mc = t.gameObject.AddComponent<MeshCollider>();
                            mc.convex = false;
                            stairsColliders++;
                        }
                    }
                    continue;
                }

                // Architectural elements (floors, walkways, walls, etc.)
                bool isArch = false;
                foreach (var k in archKeywords)
                {
                    if (lower.Contains(k))
                    {
                        isArch = true;
                        break;
                    }
                }

                if (isArch)
                {
                    if (t.GetComponent<Collider>() == null)
                    {
                        var mf = t.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
                        {
                            var mc = t.gameObject.AddComponent<MeshCollider>();
                            mc.convex = false;
                            archColliders++;
                        }
                    }
                    continue;
                }

                // Obstacles
                bool isObstacle = false;
                foreach (var k in obstacleKeywords)
                {
                    if (lower.Contains(k))
                    {
                        isObstacle = true;
                        break;
                    }
                }

                if (isObstacle)
                {
                    if (t.GetComponent<Collider>() == null)
                    {
                        var mf = t.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
                        {
                            var mc = t.gameObject.AddComponent<MeshCollider>();
                            mc.convex = false;
                            obstacleColliders++;
                        }
                    }
                }
            }

            // Clean up any rogue colliders on root campus container
            foreach (var c in campus.GetComponents<Collider>())
            {
                Object.DestroyImmediate(c);
            }

            // 2. Setup Outdoor Metal Stairs (Block F & Block D) with Smooth Walkable Ramps
            int rampsCreated = SetupOutdoorStairs(campus);

            // 3. Clear Entrance Obstacles at Block F Stair
            ClearStairEntrances();

            // 4. Setup PlayerArmature
            var player = GameObject.Find("PlayerArmature");
            if (player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.stepOffset = 0.50f; // Allows smooth step climbing over any curb/step
                    cc.slopeLimit = 60.0f; // Allows comfortable stair incline climbing
                    cc.skinWidth = 0.02f;
                    cc.detectCollisions = true;
                    cc.enableOverlapRecovery = true;
                    EditorUtility.SetDirty(cc);
                }

                var tpc = player.GetComponent<StarterAssets.ThirdPersonController>();
                if (tpc != null)
                {
                    // Ensure GroundLayers includes Default layer (Layer 0)
                    tpc.GroundLayers = tpc.GroundLayers | (1 << 0);
                    EditorUtility.SetDirty(tpc);
                }

                var sit = player.GetComponent<IUHPlayerSittingController>();
                if (sit == null)
                {
                    sit = player.AddComponent<IUHPlayerSittingController>();
                }
                EditorUtility.SetDirty(sit);
                EditorUtility.SetDirty(player);
            }

            // 5. Mark Scene Dirty and Save
            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log($"[IUHSetup] Finished successfully!\n" +
                      $"- Stairs colliders added: {stairsColliders}\n" +
                      $"- Outdoor stair ramps configured: {rampsCreated}\n" +
                      $"- Bench colliders added: {benchColliders}\n" +
                      $"- Bench seats configured: {benchSeats}\n" +
                      $"- Architecture colliders added: {archColliders}\n" +
                      $"- Obstacle colliders added: {obstacleColliders}\n" +
                      $"- PlayerArmature configured with CharacterController (stepOffset=0.50, slopeLimit=60) & IUHPlayerSittingController.\n" +
                      $"- Scene '{activeScene.name}' saved.");
        }

        private static int SetupOutdoorStairs(GameObject campus)
        {
            int ramps = 0;
            string[] stairPrefixes = new string[] { "Block_F_Stair", "Block_D_Stair" };
            float[] flight1X = new float[] { 361.41f, 363.41f };
            float[] flight2X = new float[] { 359.87f, 361.87f };
            float[] midCenterX = new float[] { 360.64f, 362.64f };
            float[] zOffsets = new float[] { 0.0f, -25.70f };

            for (int p = 0; p < stairPrefixes.Length; p++)
            {
                string prefix = stairPrefixes[p];
                float f1X = flight1X[p];
                float f2X = flight2X[p];
                float mX = midCenterX[p];
                float zOff = zOffsets[p];

                // Setup Ground Floor Landing Box (Fl1)
                string gBoxName = $"{prefix}_FloorBox_Fl1";
                var gBox = GameObject.Find(gBoxName);
                if (gBox == null)
                {
                    gBox = new GameObject(gBoxName);
                    gBox.transform.SetParent(campus.transform, false);
                }
                gBox.transform.position = new Vector3(mX, 0.48f - 0.05f, -51.70f + zOff);
                gBox.transform.rotation = Quaternion.identity;
                var bgBox = gBox.GetComponent<BoxCollider>();
                if (bgBox == null) bgBox = gBox.AddComponent<BoxCollider>();
                bgBox.size = new Vector3(4.00f, 0.10f, 1.80f);
                EditorUtility.SetDirty(gBox);

                var gLand = GameObject.Find($"{prefix}Landing_Fl1");
                if (gLand != null)
                {
                    var mc = gLand.GetComponent<Collider>();
                    if (mc != null) mc.enabled = false;
                }

                for (int fl = 1; fl <= 5; fl++)
                {
                    float floorY = (fl == 1) ? 0.48f : (0.54f + (fl - 1) * 4.40f);
                    float midY = 2.74f + (fl - 1) * 4.40f;
                    float nextFloorY = 4.94f + (fl - 1) * 4.40f;
                    float railY = 3.38f + (fl - 1) * 4.40f;

                    // 1. Widen visual steps & remove jagged colliders on Flight 1 & Flight 2
                    for (int s = 1; s <= 9; s++)
                    {
                        var s1 = GameObject.Find($"{prefix}_F1_Fl{fl}_S{s}");
                        if (s1 != null)
                        {
                            s1.transform.localScale = new Vector3(140f, 100f, 100f); // +40% wider
                            var c = s1.GetComponent<Collider>();
                            if (c != null)
                            {
                                c.enabled = false;
                                Object.DestroyImmediate(c);
                            }
                            EditorUtility.SetDirty(s1);
                        }

                        var s2 = GameObject.Find($"{prefix}_F2_Fl{fl}_S{s}");
                        if (s2 != null)
                        {
                            s2.transform.localScale = new Vector3(140f, 100f, 100f); // +40% wider
                            var c = s2.GetComponent<Collider>();
                            if (c != null)
                            {
                                c.enabled = false;
                                Object.DestroyImmediate(c);
                            }
                            EditorUtility.SetDirty(s2);
                        }
                    }

                    // 2. Visual MidLanding & Railings (Aligned strictly to OUTER EDGES)
                    var midLanding = GameObject.Find($"{prefix}MidLanding_Fl{fl}");
                    if (midLanding != null)
                    {
                        midLanding.transform.position = new Vector3(mX, midY, -55.80f + zOff);
                        midLanding.transform.localScale = new Vector3(140f, 150f, 100f);
                        var mc = midLanding.GetComponent<Collider>();
                        if (mc != null) mc.enabled = false; // Disabled to eliminate jagged lip overhang
                        EditorUtility.SetDirty(midLanding);
                    }

                    var backRail = GameObject.Find($"{prefix}MidRail_Back_Fl{fl}");
                    if (backRail != null)
                    {
                        backRail.transform.position = new Vector3(mX, railY, -56.80f + zOff);
                        backRail.transform.localScale = new Vector3(140f, 100f, 100f);
                        var mc = backRail.GetComponent<MeshCollider>();
                        if (mc == null) mc = backRail.AddComponent<MeshCollider>();
                        mc.convex = false;
                        EditorUtility.SetDirty(backRail);
                    }

                    var leftRail = GameObject.Find($"{prefix}MidRail_Left_Fl{fl}");
                    if (leftRail != null)
                    {
                        leftRail.transform.position = new Vector3(f1X + 0.94f, railY, -55.80f + zOff);
                        leftRail.transform.localScale = new Vector3(100f, 150f, 100f);
                        var mc = leftRail.GetComponent<MeshCollider>();
                        if (mc == null) mc = leftRail.AddComponent<MeshCollider>();
                        mc.convex = false;
                        EditorUtility.SetDirty(leftRail);
                    }

                    // Align Flight 1 outer railing to true outer right edge (f1X + 0.94f)
                    var f1Rail = GameObject.Find($"{prefix}_F1_Rail_Fl{fl}");
                    if (f1Rail != null)
                    {
                        var pos = f1Rail.transform.position;
                        pos.x = f1X + 0.94f;
                        f1Rail.transform.position = pos;
                        var mc = f1Rail.GetComponent<MeshCollider>();
                        if (mc == null) mc = f1Rail.AddComponent<MeshCollider>();
                        mc.convex = false;
                        EditorUtility.SetDirty(f1Rail);
                    }

                    // Align Flight 2 outer railing to true outer left edge (f2X - 0.94f)
                    var f2Rail = GameObject.Find($"{prefix}_F2_Rail_Fl{fl}");
                    if (f2Rail != null)
                    {
                        var pos = f2Rail.transform.position;
                        pos.x = f2X - 0.94f;
                        f2Rail.transform.position = pos;
                        var mc = f2Rail.GetComponent<MeshCollider>();
                        if (mc == null) mc = f2Rail.AddComponent<MeshCollider>();
                        mc.convex = false;
                        EditorUtility.SetDirty(f2Rail);
                    }

                    // Visual floor landing & bridge (disabled collider to eliminate lip snags)
                    var floorLand = GameObject.Find($"{prefix}Landing_Fl{fl + 1}");
                    if (floorLand != null)
                    {
                        floorLand.transform.localScale = new Vector3(140f, 100f, 100f);
                        var mc = floorLand.GetComponent<Collider>();
                        if (mc != null) mc.enabled = false;
                        EditorUtility.SetDirty(floorLand);
                    }

                    var bridge = GameObject.Find($"{prefix}Bridge_Fl{fl}");
                    if (bridge != null)
                    {
                        bridge.transform.localScale = new Vector3(140f, 100f, 100f);
                        EditorUtility.SetDirty(bridge);
                    }

                    // 3. Flight 1 Smooth Wide Ramp (Climbs South, +X rot)
                    string ramp1Name = $"{prefix}_Ramp_Fl{fl}_F1";
                    GameObject ramp1 = GameObject.Find(ramp1Name);
                    if (ramp1 == null)
                    {
                        ramp1 = new GameObject(ramp1Name);
                        ramp1.transform.SetParent(campus.transform, false);
                    }
                    float startZ = (fl == 1) ? (-51.80f + zOff) : (-52.55f + zOff);
                    float endZ = -54.75f + zOff;
                    float cZ1 = (startZ + endZ) / 2f;
                    float cY1 = (floorY + midY) / 2f;
                    float dz1 = Mathf.Abs(endZ - startZ);
                    float dy1 = midY - floorY;
                    float ang1 = Mathf.Atan2(dy1, dz1) * Mathf.Rad2Deg;
                    float len1 = Mathf.Sqrt(dz1 * dz1 + dy1 * dy1) + 0.15f;

                    ramp1.transform.position = new Vector3(f1X, cY1, cZ1);
                    ramp1.transform.rotation = Quaternion.Euler(ang1, 0, 0);

                    var box1 = ramp1.GetComponent<BoxCollider>();
                    if (box1 == null) box1 = ramp1.AddComponent<BoxCollider>();
                    box1.size = new Vector3(1.88f, 0.10f, len1);
                    EditorUtility.SetDirty(ramp1);
                    ramps++;

                    // 4. Dedicated MidLanding Walkable Platform Box
                    string midBoxName = $"{prefix}_MidBox_Fl{fl}";
                    GameObject midBox = GameObject.Find(midBoxName);
                    if (midBox == null)
                    {
                        midBox = new GameObject(midBoxName);
                        midBox.transform.SetParent(campus.transform, false);
                    }
                    midBox.transform.position = new Vector3(mX, midY - 0.05f, -55.75f + zOff);
                    midBox.transform.rotation = Quaternion.identity;
                    var bmb = midBox.GetComponent<BoxCollider>();
                    if (bmb == null) bmb = midBox.AddComponent<BoxCollider>();
                    bmb.size = new Vector3(4.00f, 0.10f, 2.00f);
                    EditorUtility.SetDirty(midBox);

                    // 5. Flight 2 Smooth Wide Ramp (Climbs North, -X rot, exact 45 deg incline)
                    string ramp2Name = $"{prefix}_Ramp_Fl{fl}_F2";
                    GameObject ramp2 = GameObject.Find(ramp2Name);
                    if (ramp2 == null)
                    {
                        ramp2 = new GameObject(ramp2Name);
                        ramp2.transform.SetParent(campus.transform, false);
                    }
                    ramp2.transform.position = new Vector3(f2X, (midY + nextFloorY) / 2f, -53.65f + zOff);
                    ramp2.transform.rotation = Quaternion.Euler(-45.0f, 0, 0);

                    var box2 = ramp2.GetComponent<BoxCollider>();
                    if (box2 == null) box2 = ramp2.AddComponent<BoxCollider>();
                    box2.size = new Vector3(1.88f, 0.10f, 3.25f);
                    EditorUtility.SetDirty(ramp2);
                    ramps++;

                    // 6. Dedicated Floor Landing Walkable Platform Box
                    string floorBoxName = $"{prefix}_FloorBox_Fl{fl + 1}";
                    GameObject floorBox = GameObject.Find(floorBoxName);
                    if (floorBox == null)
                    {
                        floorBox = new GameObject(floorBoxName);
                        floorBox.transform.SetParent(campus.transform, false);
                    }
                    floorBox.transform.position = new Vector3(mX, nextFloorY - 0.05f, -51.70f + zOff);
                    floorBox.transform.rotation = Quaternion.identity;
                    var bfb = floorBox.GetComponent<BoxCollider>();
                    if (bfb == null) bfb = floorBox.AddComponent<BoxCollider>();
                    bfb.size = new Vector3(4.00f, 0.10f, 1.80f);
                    EditorUtility.SetDirty(floorBox);

                    // 7. Completely remove rogue Bridge_RailL that blocks the walkway
                    var railL = GameObject.Find($"{prefix}Bridge_RailL_Fl{fl}");
                    if (railL != null)
                    {
                        Object.DestroyImmediate(railL);
                    }
                }

                // 8. Build Rooftop Access Scaffold & Bridge onto Building Roof
                BuildRoofAccess(campus, prefix, mX, zOff, (p == 0 ? 358.84f : 360.84f));
            }

            return ramps;
        }

        private static void BuildRoofAccess(GameObject campus, string prefix, float mX, float zOff, float parapetX)
        {
            var matBlue = GameObject.Find("Block_F_Stair_F1_Rail_Fl1")?.GetComponent<Renderer>()?.sharedMaterial;
            var matSteel = GameObject.Find("Block_F_Stair_F1_Fl1_S1")?.GetComponent<Renderer>()?.sharedMaterial;
            var matConc = GameObject.Find("Block_F_StairLanding_Fl1")?.GetComponent<Renderer>()?.sharedMaterial;

            string rootName = $"{prefix}_RoofAccess";
            var root = GameObject.Find(rootName);
            if (root != null) Object.DestroyImmediate(root);
            root = new GameObject(rootName);
            root.transform.SetParent(campus.transform, false);

            float landY = 22.49f;
            float parapetTopY = 23.45f;
            float roofY = 22.50f;
            float zCenter = -51.70f + zOff;

            // 1. Up Ramp from stair landing (X=mX) to parapet top (X=parapetX)
            float dx1 = mX - parapetX;
            float dy1 = parapetTopY - landY;
            float ang1 = Mathf.Atan2(dy1, dx1) * Mathf.Rad2Deg;
            float len1 = Mathf.Sqrt(dx1 * dx1 + dy1 * dy1);

            var upRamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upRamp.name = $"{prefix}_RoofAccess_UpRamp";
            upRamp.transform.SetParent(root.transform, false);
            upRamp.transform.position = new Vector3((mX + parapetX) / 2f, (landY + parapetTopY) / 2f - 0.05f, zCenter);
            upRamp.transform.rotation = Quaternion.Euler(0, 0, -ang1);
            upRamp.transform.localScale = new Vector3(len1 + 0.1f, 0.12f, 1.80f);
            if (matSteel != null) upRamp.GetComponent<Renderer>().sharedMaterial = matSteel;

            // 2. Parapet Overpass Bridge
            var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridge.name = $"{prefix}_RoofAccess_Overpass";
            bridge.transform.SetParent(root.transform, false);
            bridge.transform.position = new Vector3(parapetX - 0.6f, parapetTopY - 0.05f, zCenter);
            bridge.transform.localScale = new Vector3(1.3f, 0.12f, 1.80f);
            if (matConc != null) bridge.GetComponent<Renderer>().sharedMaterial = matConc;

            // 3. Down Ramp from parapet to roof deck
            float roofX = parapetX - 3.2f;
            float dx2 = 2.0f;
            float dy2 = parapetTopY - roofY;
            float ang2 = Mathf.Atan2(dy2, dx2) * Mathf.Rad2Deg;
            float len2 = Mathf.Sqrt(dx2 * dx2 + dy2 * dy2);

            var downRamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            downRamp.name = $"{prefix}_RoofAccess_DownRamp";
            downRamp.transform.SetParent(root.transform, false);
            downRamp.transform.position = new Vector3(parapetX - 1.2f - (dx2 / 2f), (parapetTopY + roofY) / 2f - 0.05f, zCenter);
            downRamp.transform.rotation = Quaternion.Euler(0, 0, ang2);
            downRamp.transform.localScale = new Vector3(len2 + 0.1f, 0.12f, 1.80f);
            if (matSteel != null) downRamp.GetComponent<Renderer>().sharedMaterial = matSteel;

            // 4. Roof Landing Deck
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = $"{prefix}_RoofAccess_Deck";
            pad.transform.SetParent(root.transform, false);
            pad.transform.position = new Vector3(roofX - 0.8f, roofY - 0.05f, zCenter);
            pad.transform.localScale = new Vector3(1.8f, 0.10f, 2.20f);
            if (matConc != null) pad.GetComponent<Renderer>().sharedMaterial = matConc;

            // 5. Giá đỡ (Support Columns & Trusses)
            float[] colXs = new float[] { parapetX + 0.3f, parapetX - 0.6f };
            for (int i = 0; i < colXs.Length; i++)
            {
                for (int side = 0; side < 2; side++)
                {
                    float sZ = (side == 0) ? (zCenter - 0.80f) : (zCenter + 0.80f);
                    var col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    col.name = $"{prefix}_RoofAccess_Support_{i}_{side}";
                    col.transform.SetParent(root.transform, false);
                    col.transform.position = new Vector3(colXs[i], landY - 2.0f, sZ);
                    col.transform.localScale = new Vector3(0.18f, 2.0f, 0.18f);
                    if (matSteel != null) col.GetComponent<Renderer>().sharedMaterial = matSteel;
                }

                var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beam.name = $"{prefix}_RoofAccess_Beam_{i}";
                beam.transform.SetParent(root.transform, false);
                beam.transform.position = new Vector3(colXs[i], landY - 0.20f, zCenter);
                beam.transform.localScale = new Vector3(0.20f, 0.25f, 1.80f);
                if (matSteel != null) beam.GetComponent<Renderer>().sharedMaterial = matSteel;
            }

            for (int side = 0; side < 2; side++)
            {
                float sZ = (side == 0) ? (zCenter - 0.80f) : (zCenter + 0.80f);
                var diag = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                diag.name = $"{prefix}_RoofAccess_Truss_{side}";
                diag.transform.SetParent(root.transform, false);
                diag.transform.position = new Vector3(parapetX + 0.6f, landY - 1.2f, sZ);
                diag.transform.rotation = Quaternion.Euler(0, 0, -35f);
                diag.transform.localScale = new Vector3(0.12f, 1.6f, 0.12f);
                if (matSteel != null) diag.GetComponent<Renderer>().sharedMaterial = matSteel;
            }

            // 6. Protective Blue Edge Railings with Solid Colliders
            for (int side = 0; side < 2; side++)
            {
                float sZ = (side == 0) ? (zCenter - 0.95f) : (zCenter + 0.95f);
                var r = GameObject.CreatePrimitive(PrimitiveType.Cube);
                r.name = $"{prefix}_RoofAccess_Rail_{side}";
                r.transform.SetParent(root.transform, false);
                float totalLen = mX - (roofX - 1.5f);
                r.transform.position = new Vector3((mX + roofX - 1.5f) / 2f, parapetTopY + 0.50f, sZ);
                r.transform.localScale = new Vector3(totalLen, 1.05f, 0.06f);
                if (matBlue != null) r.GetComponent<Renderer>().sharedMaterial = matBlue;
                var bc = r.GetComponent<BoxCollider>();
                if (bc != null) { bc.enabled = true; bc.isTrigger = false; }
            }

            EditorUtility.SetDirty(root);
        }

        private static void ClearStairEntrances()
        {
            // Clear the entrance to Block_F_Stair by repositioning Courtyard R planter & hedge
            // from X=361.84 (which blocked the stair landing X=359..362) to X=365.0 (nestled with trees/benches)
            string[] entranceObstacles = new string[] {
                "ShrubBed_Courtyard_R_Wall",
                "ShrubBed_Courtyard_R_Soil",
                "Hedge_Courtyard_R",
                "Bench_R_2"
            };

            foreach (var name in entranceObstacles)
            {
                var go = GameObject.Find(name);
                if (go != null)
                {
                    Vector3 pos = go.transform.position;
                    if (pos.x < 364.0f)
                    {
                        pos.x = 365.0f;
                        go.transform.position = pos;
                        EditorUtility.SetDirty(go);
                    }
                }
            }
        }
    }
}
