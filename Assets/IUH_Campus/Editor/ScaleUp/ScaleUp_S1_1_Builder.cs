using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IUHCampus.EditorScripts
{
    public static class ScaleUp_S1_1_Builder
    {
        private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private static GameObject _deskPrefab;
        private static GameObject _lightPrefab;

        private static void LoadResources()
        {
            _materials.Clear();
            var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Models/campus_P0_progress.fbx");
            foreach (var a in assets)
            {
                if (a is Material m && !_materials.ContainsKey(m.name))
                {
                    _materials[m.name] = m;
                }
            }

            _deskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/IUH_Campus/Prefabs/Architecture/Modular/StudentDeskSet.prefab");
            _lightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/IUH_Campus/Prefabs/Architecture/Modular/CorridorLightModule.prefab");
        }

        private static Material GetMat(string name)
        {
            if (_materials.TryGetValue(name, out var m)) return m;
            Debug.LogWarning($"[S1.1] Material not found in FBX: {name}");
            return null;
        }

        [MenuItem("IUH Campus/Build Step S1.1 (Tầng 2 Mẫu A, H, B)")]
        public static string BuildStepS1_1()
        {
            LoadResources();

            var interiorRoot = GameObject.Find("A_H_B_Interior_P0");
            if (interiorRoot == null)
            {
                interiorRoot = new GameObject("A_H_B_Interior_P0");
                Undo.RegisterCreatedObjectUndo(interiorRoot, "Create Interior Root");
            }

            // 1. BUILD NHÀ A - TẦNG 2 (Y = 4.53m)
            BuildNhaA_Floor02(interiorRoot.transform);

            // 2. BUILD NHÀ H - TẦNG 2 (Y = 4.53m)
            BuildNhaH_Floor02(interiorRoot.transform);

            // 3. BUILD NHÀ B - TẦNG 2 (Y = 4.86m)
            BuildNhaB_Floor02(interiorRoot.transform);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            return "Step S1.1 build completed successfully and scene saved.";
        }

        private static void BuildNhaA_Floor02(Transform root)
        {
            var bldA = root.Find("Building_A");
            if (bldA == null)
            {
                bldA = new GameObject("Building_A").transform;
                bldA.SetParent(root, false);
            }

            var existingFl2 = bldA.Find("Floor_02");
            if (existingFl2 != null)
            {
                Undo.DestroyObjectImmediate(existingFl2.gameObject);
            }

            var fl2 = new GameObject("Floor_02");
            fl2.transform.SetParent(bldA, false);
            Undo.RegisterCreatedObjectUndo(fl2, "Create Nha A Floor 02");

            float yFloor = 4.53f;
            float slabThick = 0.12f;
            float ySlabCenter = yFloor - slabThick * 0.5f; // 4.47m
            float wallH = 3.92f;
            float yWallCenter = yFloor + wallH * 0.5f;

            var matFloor = GetMat("Mat_Floor_Tile");
            var matWall = GetMat("Mat_Wall_Campus_Stucco");
            var matDoor = GetMat("Mat_Wood_Door_Oak");
            var matFrame = GetMat("Mat_Aluminum_DarkCharcoal");
            var matRail = GetMat("Mat_Stainless_Rail_PBR");
            var matDeskWood = GetMat("Mat_Lobby_DeskWood");
            var matTrim = GetMat("Mat_Concrete_Trim");
            var matPlaque = GetMat("Mat_Sign_Plaque_Blue");

            // Slabs
            var slabRoot = new GameObject("Slabs").transform;
            slabRoot.SetParent(fl2.transform, false);

            CreateBox("A_Fl2_Slab_RoomA201", slabRoot, new Vector3(414.75f, ySlabCenter, -26.15f), new Vector3(11.00f, slabThick, 14.70f), matFloor, true);
            CreateBox("A_Fl2_Slab_Corridor", slabRoot, new Vector3(407.20f, ySlabCenter, -26.15f), new Vector3(4.10f, slabThick, 14.70f), matFloor, true);
            CreateBox("A_Fl2_Slab_StairLink", slabRoot, new Vector3(404.20f, ySlabCenter, -24.25f), new Vector3(1.90f, slabThick, 1.40f), matFloor, true);

            // Ceilings
            var ceilRoot = new GameObject("Ceilings").transform;
            ceilRoot.SetParent(fl2.transform, false);
            CreateBox("A_Fl2_Ceil_RoomA201", ceilRoot, new Vector3(414.75f, yFloor + wallH, -26.15f), new Vector3(11.00f, slabThick, 14.70f), matTrim, true);
            CreateBox("A_Fl2_Ceil_Corridor", ceilRoot, new Vector3(407.20f, yFloor + wallH, -26.15f), new Vector3(4.10f, slabThick, 14.70f), matTrim, true);

            // Guardrails around stairwell void
            var guardRoot = new GameObject("Guardrails").transform;
            guardRoot.SetParent(fl2.transform, false);
            CreateBox("Rail_StairVoid_East", guardRoot, new Vector3(405.15f, yFloor + 0.50f, -26.70f), new Vector3(0.06f, 1.00f, 3.50f), matRail, true);
            CreateBox("Rail_StairVoid_South", guardRoot, new Vector3(403.48f, yFloor + 0.50f, -28.45f), new Vector3(3.40f, 1.00f, 0.06f), matRail, true);

            // Corridor & Room Walls
            var wallRoot = new GameObject("Walls").transform;
            wallRoot.SetParent(fl2.transform, false);

            CreateBox("Wall_Corr_East_South", wallRoot, new Vector3(409.00f, yWallCenter, -28.25f), new Vector3(0.20f, wallH, 10.50f), matWall, true);
            CreateBox("Wall_Corr_East_North", wallRoot, new Vector3(409.00f, yWallCenter, -20.40f), new Vector3(0.20f, wallH, 3.20f), matWall, true);
            CreateBox("Wall_Corr_East_Lintel", wallRoot, new Vector3(409.00f, yFloor + 2.20f + (wallH - 2.20f) * 0.5f, -22.50f), new Vector3(0.20f, wallH - 2.20f, 1.00f), matWall, true);

            CreateBox("Wall_A201_North", wallRoot, new Vector3(414.62f, yWallCenter, -18.80f), new Vector3(11.25f, wallH, 0.20f), matWall, true);
            CreateBox("Wall_A201_South", wallRoot, new Vector3(414.62f, yWallCenter, -33.50f), new Vector3(11.25f, wallH, 0.20f), matWall, true);

            // Room A201 Setup
            var roomA201 = new GameObject("Room_A201").transform;
            roomA201.SetParent(fl2.transform, false);

            CreateBox("Door_A201_Frame", roomA201, new Vector3(409.00f, yFloor + 1.10f, -22.50f), new Vector3(0.22f, 2.20f, 1.10f), matFrame, false);
            var doorLeaf = CreateBox("Door_A201_Leaf", roomA201, new Vector3(409.30f, yFloor + 1.05f, -22.85f), new Vector3(0.06f, 2.10f, 0.95f), matDoor, true);
            doorLeaf.transform.localRotation = Quaternion.Euler(0, 30f, 0);

            CreateBox("Sign_Plaque_A201", roomA201, new Vector3(408.88f, yFloor + 1.70f, -21.80f), new Vector3(0.03f, 0.25f, 0.45f), matPlaque, true);
            CreateTextMesh("Text_A201", roomA201, new Vector3(408.86f, yFloor + 1.70f, -21.80f), "A201\nGIANG DUONG", 24, 0.032f, Quaternion.Euler(0, 90f, 0));

            CreateBox("Classroom_Board_A201", roomA201, new Vector3(414.75f, yFloor + 1.70f, -18.92f), new Vector3(4.20f, 1.30f, 0.04f), matFrame, true);
            CreateBox("Teacher_Platform_A201", roomA201, new Vector3(414.75f, yFloor + 0.08f, -20.20f), new Vector3(3.60f, 0.16f, 1.40f), matTrim, true);
            CreateBox("Teacher_Desk_A201", roomA201, new Vector3(414.75f, yFloor + 0.55f, -20.20f), new Vector3(1.40f, 0.75f, 0.60f), matDeskWood, true);

            var deskGroup = new GameObject("StudentDesks").transform;
            deskGroup.SetParent(roomA201, false);

            float[] xCols = new float[] { 412.00f, 414.80f, 417.60f };
            float[] zRows = new float[] { -23.50f, -25.80f, -28.10f };

            if (_deskPrefab != null)
            {
                for (int r = 0; r < zRows.Length; r++)
                {
                    for (int c = 0; c < xCols.Length; c++)
                    {
                        var deskInst = PrefabUtility.InstantiatePrefab(_deskPrefab) as GameObject;
                        deskInst.name = $"DeskSet_R{r}_C{c}";
                        deskInst.transform.SetParent(deskGroup, false);
                        deskInst.transform.position = new Vector3(xCols[c], yFloor, zRows[r]);
                        deskInst.transform.rotation = Quaternion.Euler(0, 180f, 0);
                    }
                }
            }

            var lightGroup = new GameObject("Lights").transform;
            lightGroup.SetParent(fl2.transform, false);

            if (_lightPrefab != null)
            {
                float[] zCorrLights = new float[] { -20.0f, -25.0f, -30.0f };
                for (int i = 0; i < zCorrLights.Length; i++)
                {
                    var l = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                    l.name = $"Light_Corr_{i + 1}";
                    l.transform.SetParent(lightGroup, false);
                    l.transform.position = new Vector3(407.20f, yFloor + 3.40f, zCorrLights[i]);
                }

                float[] xRoomLights = new float[] { 412.5f, 417.0f };
                float[] zRoomLights = new float[] { -22.0f, -26.5f, -31.0f };
                for (int rx = 0; rx < xRoomLights.Length; rx++)
                {
                    for (int rz = 0; rz < zRoomLights.Length; rz++)
                    {
                        var l = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                        l.name = $"Light_A201_{rx}_{rz}";
                        l.transform.SetParent(lightGroup, false);
                        l.transform.position = new Vector3(xRoomLights[rx], yFloor + 3.40f, zRoomLights[rz]);
                    }
                }
            }
        }

        private static void BuildNhaH_Floor02(Transform root)
        {
            var bldH = root.Find("Building_H");
            if (bldH == null)
            {
                bldH = new GameObject("Building_H").transform;
                bldH.SetParent(root, false);
            }

            var existingFl2 = bldH.Find("Floor_02");
            if (existingFl2 != null)
            {
                Undo.DestroyObjectImmediate(existingFl2.gameObject);
            }

            var fl2 = new GameObject("Floor_02");
            fl2.transform.SetParent(bldH, false);
            Undo.RegisterCreatedObjectUndo(fl2, "Create Nha H Floor 02");

            float yFloor = 4.53f;
            float slabThick = 0.12f;
            float ySlabCenter = yFloor - slabThick * 0.5f; // 4.47m
            float wallH = 3.92f;
            float yWallCenter = yFloor + wallH * 0.5f;

            var matFloor = GetMat("Mat_Floor_Tile");
            var matWall = GetMat("Mat_Wall_Campus_Stucco");
            var matDoor = GetMat("Mat_Wood_Door_Oak");
            var matFrame = GetMat("Mat_Aluminum_DarkCharcoal");
            var matRail = GetMat("Mat_Stainless_Rail_PBR");
            var matDeskWood = GetMat("Mat_Lobby_DeskWood");
            var matTrim = GetMat("Mat_Concrete_Trim");
            var matPlaque = GetMat("Mat_Sign_Plaque_Blue");

            // Slabs around central elevator core
            var slabRoot = new GameObject("Slabs").transform;
            slabRoot.SetParent(fl2.transform, false);

            CreateBox("H_Fl2_Slab_West_North", slabRoot, new Vector3(406.50f, ySlabCenter, -58.50f), new Vector3(5.00f, slabThick, 7.00f), matFloor, true);
            CreateBox("H_Fl2_Slab_West_South", slabRoot, new Vector3(406.50f, ySlabCenter, -69.50f), new Vector3(5.00f, slabThick, 6.00f), matFloor, true);
            CreateBox("H_Fl2_Slab_StairExit", slabRoot, new Vector3(407.50f, ySlabCenter, -62.25f), new Vector3(3.00f, slabThick, 1.50f), matFloor, true);

            CreateBox("H_Fl2_Slab_North", slabRoot, new Vector3(414.50f, ySlabCenter, -58.50f), new Vector3(11.00f, slabThick, 5.00f), matFloor, true);
            CreateBox("H_Fl2_Slab_South", slabRoot, new Vector3(414.50f, ySlabCenter, -69.50f), new Vector3(11.00f, slabThick, 5.00f), matFloor, true);
            CreateBox("H_Fl2_Slab_East", slabRoot, new Vector3(417.80f, ySlabCenter, -64.10f), new Vector3(6.60f, slabThick, 6.30f), matFloor, true);

            // Ceilings for North & South corridors and Room H2.1
            var ceilRoot = new GameObject("Ceilings").transform;
            ceilRoot.SetParent(fl2.transform, false);
            CreateBox("H_Fl2_Ceil_North", ceilRoot, new Vector3(414.50f, yFloor + wallH, -58.50f), new Vector3(11.00f, slabThick, 5.00f), matTrim, true);
            CreateBox("H_Fl2_Ceil_South", ceilRoot, new Vector3(414.50f, yFloor + wallH, -69.50f), new Vector3(11.00f, slabThick, 5.00f), matTrim, true);
            CreateBox("H_Fl2_Ceil_East", ceilRoot, new Vector3(417.80f, yFloor + wallH, -64.10f), new Vector3(6.60f, slabThick, 6.30f), matTrim, true);

            // Guardrails around central elevator core opening and stairwell
            var guardRoot = new GameObject("Guardrails").transform;
            guardRoot.SetParent(fl2.transform, false);

            CreateBox("Rail_Core_North", guardRoot, new Vector3(411.84f, yFloor + 0.50f, -60.95f), new Vector3(5.40f, 1.00f, 0.06f), matRail, true);
            CreateBox("Rail_Core_South", guardRoot, new Vector3(411.84f, yFloor + 0.50f, -67.25f), new Vector3(5.40f, 1.00f, 0.06f), matRail, true);
            // Stair void guardrails (Flight 1 at X=405.61 is open down to Fl1; Flight 2 at X=404.06 must be completely open to Fl2 landing!)
            CreateBox("Rail_Core_West", guardRoot, new Vector3(409.14f, yFloor + 0.50f, -64.10f), new Vector3(0.06f, 1.00f, 6.30f), matRail, true);
            CreateBox("Rail_H_StairVoid_North", guardRoot, new Vector3(405.61f, yFloor + 0.50f, -62.25f), new Vector3(1.40f, 1.00f, 0.06f), matRail, true);
            CreateBox("Rail_H_StairVoid_East", guardRoot, new Vector3(406.31f, yFloor + 0.50f, -64.10f), new Vector3(0.06f, 1.00f, 3.70f), matRail, true);
            CreateBox("Rail_H_StairVoid_Center", guardRoot, new Vector3(404.84f, yFloor + 0.50f, -64.10f), new Vector3(0.06f, 1.00f, 3.70f), matRail, true);

            // Room H2.1 (Phòng hội thảo / chuyên đề mẫu Tầng 2)
            var roomH21 = new GameObject("Room_H201").transform;
            roomH21.SetParent(fl2.transform, false);

            CreateBox("Wall_H21_West_North", roomH21, new Vector3(414.54f, yWallCenter, -58.80f), new Vector3(0.20f, wallH, 4.40f), matWall, true);
            CreateBox("Wall_H21_West_South", roomH21, new Vector3(414.54f, yWallCenter, -67.50f), new Vector3(0.20f, wallH, 8.00f), matWall, true);
            CreateBox("Wall_H21_West_Lintel", roomH21, new Vector3(414.54f, yFloor + 2.20f + (wallH - 2.20f) * 0.5f, -62.00f), new Vector3(0.20f, wallH - 2.20f, 1.20f), matWall, true);

            CreateBox("Door_H21_Frame", roomH21, new Vector3(414.54f, yFloor + 1.10f, -62.00f), new Vector3(0.22f, 2.20f, 1.20f), matFrame, false);
            var doorLeaf = CreateBox("Door_H21_Leaf", roomH21, new Vector3(414.85f, yFloor + 1.05f, -62.45f), new Vector3(0.06f, 2.10f, 1.00f), matDoor, true);
            doorLeaf.transform.localRotation = Quaternion.Euler(0, 35f, 0);

            // Plaque facing west into corridor
            CreateBox("Sign_Plaque_H21", roomH21, new Vector3(414.40f, yFloor + 1.70f, -61.20f), new Vector3(0.03f, 0.25f, 0.45f), matPlaque, true);
            CreateTextMesh("Text_H21", roomH21, new Vector3(414.37f, yFloor + 1.70f, -61.20f), "H2.1\nPHONG HOI THAO", 24, 0.032f, Quaternion.Euler(0, 90f, 0));

            CreateBox("Board_H21", roomH21, new Vector3(418.00f, yFloor + 1.70f, -56.65f), new Vector3(3.60f, 1.30f, 0.04f), matFrame, true);
            CreateBox("Podium_H21", roomH21, new Vector3(419.50f, yFloor + 0.55f, -58.00f), new Vector3(0.80f, 1.10f, 0.60f), matDeskWood, true);
            CreateBox("Presenter_Table_H21", roomH21, new Vector3(417.50f, yFloor + 0.38f, -58.00f), new Vector3(1.60f, 0.75f, 0.70f), matDeskWood, true);

            var deskGroup = new GameObject("SeminarDesks").transform;
            deskGroup.SetParent(roomH21, false);

            float[] xDesksH = new float[] { 416.20f, 418.20f, 420.20f };
            float[] zDesksH = new float[] { -61.00f, -63.50f, -66.00f };

            if (_deskPrefab != null)
            {
                for (int r = 0; r < zDesksH.Length; r++)
                {
                    for (int c = 0; c < xDesksH.Length; c++)
                    {
                        var d = PrefabUtility.InstantiatePrefab(_deskPrefab) as GameObject;
                        d.name = $"Desk_H21_R{r}_C{c}";
                        d.transform.SetParent(deskGroup, false);
                        d.transform.position = new Vector3(xDesksH[c], yFloor, zDesksH[r]);
                        d.transform.rotation = Quaternion.Euler(0, 0f, 0);
                    }
                }
            }

            var lightGroup = new GameObject("Lights").transform;
            lightGroup.SetParent(fl2.transform, false);

            if (_lightPrefab != null)
            {
                var lN = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                lN.name = "Light_H_North";
                lN.transform.SetParent(lightGroup, false);
                lN.transform.position = new Vector3(414.50f, yFloor + 3.40f, -58.50f);

                var lS = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                lS.name = "Light_H_South";
                lS.transform.SetParent(lightGroup, false);
                lS.transform.position = new Vector3(414.50f, yFloor + 3.40f, -69.50f);

                var lW = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                lW.name = "Light_H_West";
                lW.transform.SetParent(lightGroup, false);
                lW.transform.position = new Vector3(406.50f, yFloor + 3.40f, -62.25f);

                var lR1 = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                lR1.name = "Light_H21_1";
                lR1.transform.SetParent(lightGroup, false);
                lR1.transform.position = new Vector3(418.00f, yFloor + 3.40f, -59.50f);

                var lR2 = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                lR2.name = "Light_H21_2";
                lR2.transform.SetParent(lightGroup, false);
                lR2.transform.position = new Vector3(418.00f, yFloor + 3.40f, -64.50f);
            }
        }

        private static void BuildNhaB_Floor02(Transform root)
        {
            var bldB = root.Find("Building_B");
            if (bldB == null)
            {
                bldB = new GameObject("Building_B").transform;
                bldB.SetParent(root, false);
            }

            // Disable solid BoxCollider on NhaB_Head_Main_Body so interior is completely walkable
            var headBody = GameObject.Find("A_H_B_Additive_Architecture/NhaB_Head_Additive/NhaB_Head_Main_Body");
            if (headBody != null)
            {
                var bc = headBody.GetComponent<BoxCollider>();
                if (bc != null) bc.enabled = false;
            }

            BuildNhaB_Floor01_And_Stair(bldB);

            var existingFl2 = bldB.Find("Floor_02");
            if (existingFl2 != null)
            {
                Undo.DestroyObjectImmediate(existingFl2.gameObject);
            }

            var fl2 = new GameObject("Floor_02");
            fl2.transform.SetParent(bldB, false);
            Undo.RegisterCreatedObjectUndo(fl2, "Create Nha B Floor 02");

            float yFloor = 4.86f; // EXACT finish floor level for Nha B!
            float slabThick = 0.12f;
            float ySlabCenter = yFloor - slabThick * 0.5f; // 4.80m
            float wallH = 4.25f;
            float yWallCenter = yFloor + wallH * 0.5f;

            var matFloor = GetMat("Mat_Floor_Tile");
            var matWall = GetMat("Mat_Wall_Campus_Stucco");
            var matDoor = GetMat("Mat_Wood_Door_Oak");
            var matFrame = GetMat("Mat_Aluminum_DarkCharcoal");
            var matRail = GetMat("Mat_Stainless_Rail_PBR");
            var matDeskWood = GetMat("Mat_Lobby_DeskWood");
            var matTrim = GetMat("Mat_Concrete_Trim");
            var matPlaque = GetMat("Mat_Sign_Plaque_Blue");

            // Slabs
            var slabRoot = new GameObject("Slabs").transform;
            slabRoot.SetParent(fl2.transform, false);

            CreateBox("B_Fl2_Slab_Corridor", slabRoot, new Vector3(405.50f, ySlabCenter, -89.90f), new Vector3(4.00f, slabThick, 33.20f), matFloor, true);
            CreateBox("B_Fl2_Slab_StairLanding_Link", slabRoot, new Vector3(408.57f, ySlabCenter, -76.50f), new Vector3(6.14f, slabThick, 6.40f), matFloor, true);
            CreateBox("B_Fl2_Slab_RoomB201", slabRoot, new Vector3(412.75f, ySlabCenter, -93.00f), new Vector3(10.50f, slabThick, 12.00f), matFloor, true);

            // Ceilings
            var ceilRoot = new GameObject("Ceilings").transform;
            ceilRoot.SetParent(fl2.transform, false);
            CreateBox("B_Fl2_Ceil_Corridor", ceilRoot, new Vector3(405.50f, yFloor + wallH, -89.90f), new Vector3(4.00f, slabThick, 33.20f), matTrim, true);
            CreateBox("B_Fl2_Ceil_RoomB201", ceilRoot, new Vector3(412.75f, yFloor + wallH, -93.00f), new Vector3(10.50f, slabThick, 12.00f), matTrim, true);

            // Guardrail along stairwell opening
            var guardRoot = new GameObject("Guardrails").transform;
            guardRoot.SetParent(fl2.transform, false);
            CreateBox("Rail_B_StairVoid_West", guardRoot, new Vector3(407.50f, yFloor + 0.50f, -81.70f), new Vector3(0.06f, 1.00f, 3.60f), matRail, true);
            CreateBox("Rail_B_StairVoid_South", guardRoot, new Vector3(409.75f, yFloor + 0.50f, -83.50f), new Vector3(4.50f, 1.00f, 0.06f), matRail, true);

            // Room B201 Setup
            var roomB201 = new GameObject("Room_B201").transform;
            roomB201.SetParent(fl2.transform, false);

            CreateBox("Wall_B201_Corridor_North", roomB201, new Vector3(407.50f, yWallCenter, -87.85f), new Vector3(0.20f, wallH, 1.70f), matWall, true);
            CreateBox("Wall_B201_Corridor_South", roomB201, new Vector3(407.50f, yWallCenter, -94.35f), new Vector3(0.20f, wallH, 9.30f), matWall, true);
            CreateBox("Wall_B201_Corridor_Lintel", roomB201, new Vector3(407.50f, yFloor + 2.20f + (wallH - 2.20f) * 0.5f, -89.20f), new Vector3(0.20f, wallH - 2.20f, 1.00f), matWall, true);

            CreateBox("Wall_B201_North", roomB201, new Vector3(412.75f, yWallCenter, -87.00f), new Vector3(10.50f, wallH, 0.20f), matWall, true);
            CreateBox("Wall_B201_South", roomB201, new Vector3(412.75f, yWallCenter, -99.00f), new Vector3(10.50f, wallH, 0.20f), matWall, true);
            CreateBox("Wall_B201_East", roomB201, new Vector3(418.00f, yWallCenter, -93.00f), new Vector3(0.20f, wallH, 12.00f), matWall, true);

            CreateBox("Door_B201_Frame", roomB201, new Vector3(407.50f, yFloor + 1.10f, -89.20f), new Vector3(0.22f, 2.20f, 1.10f), matFrame, false);
            var doorLeaf = CreateBox("Door_B201_Leaf", roomB201, new Vector3(407.80f, yFloor + 1.05f, -89.55f), new Vector3(0.06f, 2.10f, 0.95f), matDoor, true);
            doorLeaf.transform.localRotation = Quaternion.Euler(0, 30f, 0);

            CreateBox("Sign_Plaque_B201", roomB201, new Vector3(407.38f, yFloor + 1.70f, -88.40f), new Vector3(0.03f, 0.25f, 0.45f), matPlaque, true);
            CreateTextMesh("Text_B201", roomB201, new Vector3(407.36f, yFloor + 1.70f, -88.40f), "B201\nPHONG LY THUYET", 24, 0.032f, Quaternion.Euler(0, 90f, 0));

            CreateBox("Board_B201", roomB201, new Vector3(412.75f, yFloor + 1.70f, -87.12f), new Vector3(4.20f, 1.30f, 0.04f), matFrame, true);
            CreateBox("Platform_B201", roomB201, new Vector3(412.75f, yFloor + 0.08f, -88.40f), new Vector3(3.60f, 0.16f, 1.40f), matTrim, true);
            CreateBox("Teacher_Desk_B201", roomB201, new Vector3(412.75f, yFloor + 0.55f, -88.40f), new Vector3(1.40f, 0.75f, 0.60f), matDeskWood, true);

            var deskGroup = new GameObject("StudentDesks").transform;
            deskGroup.SetParent(roomB201, false);

            float[] xDesksB = new float[] { 410.00f, 412.80f, 415.60f };
            float[] zDesksB = new float[] { -91.50f, -94.00f, -96.50f };

            if (_deskPrefab != null)
            {
                for (int r = 0; r < zDesksB.Length; r++)
                {
                    for (int c = 0; c < xDesksB.Length; c++)
                    {
                        var d = PrefabUtility.InstantiatePrefab(_deskPrefab) as GameObject;
                        d.name = $"Desk_B201_R{r}_C{c}";
                        d.transform.SetParent(deskGroup, false);
                        d.transform.position = new Vector3(xDesksB[c], yFloor, zDesksB[r]);
                        d.transform.rotation = Quaternion.Euler(0, 180f, 0);
                    }
                }
            }

            var lightGroup = new GameObject("Lights").transform;
            lightGroup.SetParent(fl2.transform, false);

            if (_lightPrefab != null)
            {
                float[] zCorrLightsB = new float[] { -76.0f, -85.0f, -94.0f, -103.0f };
                for (int i = 0; i < zCorrLightsB.Length; i++)
                {
                    var l = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                    l.name = $"Light_B_Corr_{i + 1}";
                    l.transform.SetParent(lightGroup, false);
                    l.transform.position = new Vector3(405.50f, yFloor + 3.40f, zCorrLightsB[i]);
                }

                float[] xRoomLightsB = new float[] { 410.5f, 415.0f };
                float[] zRoomLightsB = new float[] { -90.0f, -94.5f };
                for (int rx = 0; rx < xRoomLightsB.Length; rx++)
                {
                    for (int rz = 0; rz < zRoomLightsB.Length; rz++)
                    {
                        var l = PrefabUtility.InstantiatePrefab(_lightPrefab) as GameObject;
                        l.name = $"Light_B201_{rx}_{rz}";
                        l.transform.SetParent(lightGroup, false);
                        l.transform.position = new Vector3(xRoomLightsB[rx], yFloor + 3.40f, zRoomLightsB[rz]);
                    }
                }
            }
        }

        private static void BuildNhaB_Floor01_And_Stair(Transform bldB)
        {
            var matFloor = GetMat("Mat_Floor_Tile");
            var matStep = GetMat("Mat_Granite_Flamed_Step");
            var matRail = GetMat("Mat_Stainless_Rail_PBR");

            var fl1 = bldB.Find("Floor_01");
            if (fl1 == null)
            {
                fl1 = new GameObject("Floor_01").transform;
                fl1.SetParent(bldB, false);

                CreateBox("B_Fl1_Slab_Corridor", fl1, new Vector3(405.50f, 0.47f, -89.90f), new Vector3(4.00f, 0.12f, 33.20f), matFloor, true);
                CreateBox("B_Fl1_Slab_StairLink", fl1, new Vector3(408.57f, 0.47f, -76.50f), new Vector3(6.14f, 0.12f, 6.40f), matFloor, true);
            }

            var existingStair = bldB.Find("Stair_Colliders_Fl1_to_Fl2");
            if (existingStair != null)
            {
                Undo.DestroyObjectImmediate(existingStair.gameObject);
            }

            var stairRoot = new GameObject("Stair_Colliders_Fl1_to_Fl2").transform;
            stairRoot.SetParent(bldB, false);
            Undo.RegisterCreatedObjectUndo(stairRoot.gameObject, "Create Nha B Stairs");

            CreateBox("B_Stair_Fl1_Land_Col", stairRoot, new Vector3(410.41f, 0.53f, -78.80f), new Vector3(1.60f, 0.10f, 1.60f), null, true);

            var f1Ramp = CreateBox("B_Stair_Flight1_Ramp", stairRoot, new Vector3(410.41f, 1.62f, -81.00f), new Vector3(1.50f, 0.15f, 3.40f), null, true);
            f1Ramp.transform.localRotation = Quaternion.Euler(39.8f, 0f, 0f);

            CreateBox("B_Stair_MidLand_Col", stairRoot, new Vector3(409.64f, 2.70f, -83.20f), new Vector3(3.20f, 0.10f, 1.60f), matStep, true);

            var f2Ramp = CreateBox("B_Stair_Flight2_Ramp", stairRoot, new Vector3(408.87f, 3.78f, -81.00f), new Vector3(1.50f, 0.15f, 3.40f), null, true);
            f2Ramp.transform.localRotation = Quaternion.Euler(320.2f, 0f, 0f);

            CreateBox("B_Stair_Fl2_Land_Col", stairRoot, new Vector3(409.64f, 4.86f, -78.80f), new Vector3(3.20f, 0.10f, 1.60f), matStep, true);

            int stepsPerFlight = 9;
            for (int i = 0; i < stepsPerFlight; i++)
            {
                float t = (float)i / (stepsPerFlight - 1);
                float y1 = Mathf.Lerp(0.65f, 2.60f, t);
                float z1 = Mathf.Lerp(-79.50f, -82.50f, t);
                CreateBox($"Step_F1_{i}", stairRoot, new Vector3(410.41f, y1, z1), new Vector3(1.50f, 0.22f, 0.35f), matStep, false);

                float y2 = Mathf.Lerp(2.80f, 4.75f, t);
                float z2 = Mathf.Lerp(-82.50f, -79.50f, t);
                CreateBox($"Step_F2_{i}", stairRoot, new Vector3(408.87f, y2, z2), new Vector3(1.50f, 0.22f, 0.35f), matStep, false);
            }

            CreateBox("Stair_Central_Rail", stairRoot, new Vector3(409.64f, 2.70f, -81.00f), new Vector3(0.06f, 3.50f, 3.60f), matRail, true);
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, bool addCollider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = size;

            var r = go.GetComponent<MeshRenderer>();
            if (mat != null)
            {
                r.sharedMaterial = mat;
            }
            else
            {
                r.enabled = false;
            }

            var col = go.GetComponent<BoxCollider>();
            if (!addCollider && col != null)
            {
                UnityEngine.Object.DestroyImmediate(col);
            }

            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            return go;
        }

        private static GameObject CreateTextMesh(string name, Transform parent, Vector3 pos, string text, int fontSize, float charSize, Quaternion rot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localRotation = rot;

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = fontSize;
            tm.characterSize = charSize;
            tm.color = Color.white;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;

            Font font = null;
            var existingText = GameObject.Find("A_H_B_Interior_P0/Building_A/Floor_01/Room_A101/Text_A101_Fixed");
            if (existingText != null)
            {
                var tmExisting = existingText.GetComponent<TextMesh>();
                if (tmExisting != null) font = tmExisting.font;
            }
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            }
            if (font != null)
            {
                tm.font = font;
                var r = go.GetComponent<MeshRenderer>();
                if (r != null) r.sharedMaterial = font.material;
            }

            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            return go;
        }
    }
}
