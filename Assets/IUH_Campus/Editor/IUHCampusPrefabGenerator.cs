using System.IO;
using UnityEngine;
using UnityEditor;
using IUHCampus;

namespace IUHCampus.Editor
{
    public static class IUHCampusPrefabGenerator
    {
        private const string PrefabDir = "Assets/IUH_Campus/Prefabs/";
        private const string MatPath = "Assets/IUH_Campus/Materials/";

        public static void GenerateAllStandardPrefabs()
        {
            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
            }

            CreateMotorcyclePrefab();
            CreateACUnitPrefab();
            CreateBenchPrefab();
            CreateTrashBinPrefab();
            CreateTreePrefab();
            CreateSignPrefab();
            CreateDoorPrefab();
            CreateWindowModulePrefab();
            CreateLampPrefab();
            CreateFireExtinguisherPrefab();

            AssetDatabase.Refresh();
            Debug.Log("[IUH Prefab Generator] All 10 Standard Prefabs generated successfully in " + PrefabDir);
        }

        private static void SaveAndDestroyPrefab(GameObject go, string name)
        {
            string path = Path.Combine(PrefabDir, name + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
        }

        private static void CreateMotorcyclePrefab()
        {
            GameObject root = new GameObject("Motorcycle");
            root.layer = LayerMask.NameToLayer("Decoration") != -1 ? LayerMask.NameToLayer("Decoration") : 0;

            Material bodyMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Red.mat");
            Material darkMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Motorbike_Dark.mat");
            Material helmetMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Helmet_Red.mat");
            Material chromeMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_Chrome.mat");

            // Main Body chassis
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0, 0.52f, 0);
            body.transform.localScale = new Vector3(0.42f, 0.45f, 1.25f);
            if (bodyMat != null) body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

            // Front cowl / headlight shield
            GameObject cowl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cowl.name = "FrontCowl";
            cowl.transform.SetParent(root.transform, false);
            cowl.transform.localPosition = new Vector3(0, 0.72f, 0.52f);
            cowl.transform.localScale = new Vector3(0.38f, 0.38f, 0.35f);
            if (bodyMat != null) cowl.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.SetParent(root.transform, false);
            seat.transform.localPosition = new Vector3(0, 0.78f, -0.15f);
            seat.transform.localScale = new Vector3(0.36f, 0.12f, 0.75f);
            if (darkMat != null) seat.GetComponent<MeshRenderer>().sharedMaterial = darkMat;

            // Wheels (Front & Rear)
            for (int w = 0; w < 2; w++)
            {
                float z = (w == 0) ? 0.65f : -0.65f;
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = (w == 0) ? "Wheel_Front" : "Wheel_Rear";
                wheel.transform.SetParent(root.transform, false);
                wheel.transform.localPosition = new Vector3(0, 0.28f, z);
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                wheel.transform.localScale = new Vector3(0.56f, 0.10f, 0.56f);
                if (darkMat != null) wheel.GetComponent<MeshRenderer>().sharedMaterial = darkMat;
            }

            // Handlebar
            GameObject hBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hBar.name = "Handlebar";
            hBar.transform.SetParent(root.transform, false);
            hBar.transform.localPosition = new Vector3(0, 0.95f, 0.48f);
            hBar.transform.localScale = new Vector3(0.68f, 0.04f, 0.05f);
            if (chromeMat != null) hBar.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;

            // Helmet on Seat
            GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helmet.name = "Helmet";
            helmet.transform.SetParent(root.transform, false);
            helmet.transform.localPosition = new Vector3(0.04f, 0.92f, 0.05f);
            helmet.transform.localScale = new Vector3(0.24f, 0.22f, 0.26f);
            if (helmetMat != null) helmet.GetComponent<MeshRenderer>().sharedMaterial = helmetMat;

            // Simple Box Collider for entire motorcycle
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.55f, 0);
            col.size = new Vector3(0.7f, 1.1f, 1.8f);

            // NavMesh Obstacle
            var nmo = root.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            nmo.carving = true;
            nmo.size = new Vector3(0.8f, 1.2f, 1.9f);

            SaveAndDestroyPrefab(root, "Motorcycle");
        }

        private static void CreateACUnitPrefab()
        {
            GameObject root = new GameObject("ACUnit");
            Material louversMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_AC_Louvers.mat");
            Material metalMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");
            Material pvcMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_PVC_Pipe.mat");

            // Condenser Casing
            GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            casing.name = "Casing";
            casing.transform.SetParent(root.transform, false);
            casing.transform.localScale = new Vector3(0.85f, 0.65f, 0.45f);
            if (louversMat != null) casing.GetComponent<MeshRenderer>().sharedMaterial = louversMat;

            // Mounting Bracket
            GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "Bracket";
            bracket.transform.SetParent(root.transform, false);
            bracket.transform.localPosition = new Vector3(0, -0.38f, 0.10f);
            bracket.transform.localScale = new Vector3(0.75f, 0.08f, 0.35f);
            if (metalMat != null) bracket.GetComponent<MeshRenderer>().sharedMaterial = metalMat;

            // Condensate Drain Tube
            GameObject drain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            drain.name = "DrainTube";
            drain.transform.SetParent(root.transform, false);
            drain.transform.localPosition = new Vector3(0.35f, -0.7f, 0.12f);
            drain.transform.localScale = new Vector3(0.04f, 0.65f, 0.04f);
            if (pvcMat != null) drain.GetComponent<MeshRenderer>().sharedMaterial = pvcMat;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0, 0);
            col.size = new Vector3(0.9f, 0.7f, 0.5f);

            SaveAndDestroyPrefab(root, "ACUnit");
        }

        private static void CreateBenchPrefab()
        {
            GameObject root = new GameObject("Bench");
            Material graniteMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Bench_Granite.mat");

            // Seat Slab
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "SeatSlab";
            seat.transform.SetParent(root.transform, false);
            seat.transform.localPosition = new Vector3(0, 0.44f, 0);
            seat.transform.localScale = new Vector3(1.8f, 0.08f, 0.55f);
            if (graniteMat != null) seat.GetComponent<MeshRenderer>().sharedMaterial = graniteMat;

            // Backrest Slab
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Backrest";
            back.transform.SetParent(root.transform, false);
            back.transform.localPosition = new Vector3(0, 0.75f, -0.24f);
            back.transform.localScale = new Vector3(1.8f, 0.38f, 0.08f);
            if (graniteMat != null) back.GetComponent<MeshRenderer>().sharedMaterial = graniteMat;

            // Concrete Legs
            for (int l = -1; l <= 1; l += 2)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.name = "Leg_" + l;
                leg.transform.SetParent(root.transform, false);
                leg.transform.localPosition = new Vector3(l * 0.65f, 0.20f, -0.05f);
                leg.transform.localScale = new Vector3(0.16f, 0.40f, 0.42f);
                if (graniteMat != null) leg.GetComponent<MeshRenderer>().sharedMaterial = graniteMat;
            }

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.45f, 0);
            col.size = new Vector3(1.9f, 0.9f, 0.65f);

            // Add IUHInteractable component
            var inter = root.AddComponent<IUHInteractable>();
            inter.interactionType = InteractionType.Chair;
            inter.promptText = "Nhấn E để ngồi nghỉ";
            inter.interactionRange = 2.0f;

            SaveAndDestroyPrefab(root, "Bench");
        }

        private static void CreateTrashBinPrefab()
        {
            GameObject root = new GameObject("TrashBin");
            Material curbMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Concrete_Curb.mat");
            Material greenMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Foliage_Tree.mat");
            Material orangeMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Flower_Orange.mat");
            Material darkMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");

            // Base plinth
            GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plinth.name = "BasePlinth";
            plinth.transform.SetParent(root.transform, false);
            plinth.transform.localPosition = new Vector3(0, 0.05f, 0);
            plinth.transform.localScale = new Vector3(0.95f, 0.10f, 0.50f);
            if (curbMat != null) plinth.GetComponent<MeshRenderer>().sharedMaterial = curbMat;

            // Bin Left (Green - Organic Waste)
            GameObject binL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            binL.name = "Bin_Organic";
            binL.transform.SetParent(root.transform, false);
            binL.transform.localPosition = new Vector3(-0.24f, 0.52f, 0);
            binL.transform.localScale = new Vector3(0.40f, 0.85f, 0.40f);
            if (greenMat != null) binL.GetComponent<MeshRenderer>().sharedMaterial = greenMat;

            // Bin Right (Orange - Inorganic / Recyclable)
            GameObject binR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            binR.name = "Bin_Recyclable";
            binR.transform.SetParent(root.transform, false);
            binR.transform.localPosition = new Vector3(0.24f, 0.52f, 0);
            binR.transform.localScale = new Vector3(0.40f, 0.85f, 0.40f);
            if (orangeMat != null) binR.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;

            // Rain Hood Top
            GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hood.name = "RainHood";
            hood.transform.SetParent(root.transform, false);
            hood.transform.localPosition = new Vector3(0, 1.02f, 0);
            hood.transform.localScale = new Vector3(0.98f, 0.08f, 0.48f);
            if (darkMat != null) hood.GetComponent<MeshRenderer>().sharedMaterial = darkMat;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.55f, 0);
            col.size = new Vector3(1.05f, 1.1f, 0.55f);

            var inter = root.AddComponent<IUHInteractable>();
            inter.interactionType = InteractionType.InspectionPoint;
            inter.promptText = "Thùng rác phân loại IUH (Hữu cơ & Vô cơ)";
            inter.interactionRange = 1.8f;

            SaveAndDestroyPrefab(root, "TrashBin");
        }

        private static void CreateTreePrefab()
        {
            GameObject root = new GameObject("Tree");
            Material curbMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Concrete_Curb.mat");
            Material soilMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Planter_Soil.mat");
            Material barkMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Tree_Bark.mat");
            Material foliageMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_MatureFoliage.mat");

            // Raised Curb Tree Pit
            GameObject pit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pit.name = "TreePit_Curb";
            pit.transform.SetParent(root.transform, false);
            pit.transform.localPosition = new Vector3(0, 0.12f, 0);
            pit.transform.localScale = new Vector3(3.2f, 0.24f, 3.2f);
            if (curbMat != null) pit.GetComponent<MeshRenderer>().sharedMaterial = curbMat;

            // Soil
            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soil.name = "Soil";
            soil.transform.SetParent(pit.transform, false);
            soil.transform.localPosition = new Vector3(0, 0.35f, 0);
            soil.transform.localScale = new Vector3(0.85f, 0.40f, 0.85f);
            if (soilMat != null) soil.GetComponent<MeshRenderer>().sharedMaterial = soilMat;

            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localPosition = new Vector3(0, 4.5f, 0);
            trunk.transform.localScale = new Vector3(1.2f, 4.5f, 1.2f);
            if (barkMat != null) trunk.GetComponent<MeshRenderer>().sharedMaterial = barkMat;

            // Canopy Clusters (3 organic overlapping tiers)
            for (int c = 0; c < 3; c++)
            {
                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.name = "Canopy_Tier_" + c;
                crown.transform.SetParent(root.transform, false);
                float y = 9.0f + c * 1.8f;
                float r = 5.2f - c * 0.8f;
                float ox = (c == 1) ? 0.8f : -0.6f;
                float oz = (c == 2) ? 0.6f : -0.6f;
                crown.transform.localPosition = new Vector3(ox, y, oz);
                crown.transform.localScale = new Vector3(r * 2.0f, r * 1.15f, r * 2.0f);
                if (foliageMat != null) crown.GetComponent<MeshRenderer>().sharedMaterial = foliageMat;
            }

            // Trunk Box Collider (keeps player from walking into trunk)
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 2.5f, 0);
            col.size = new Vector3(1.4f, 5.0f, 1.4f);

            var nmo = root.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            nmo.carving = true;
            nmo.size = new Vector3(3.2f, 3.0f, 3.2f);

            SaveAndDestroyPrefab(root, "Tree");
        }

        private static void CreateSignPrefab()
        {
            GameObject root = new GameObject("Sign");
            Material wayfindingMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Sign_Wayfinding.mat");
            Material metalMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");

            // Dual support poles
            for (int p = -1; p <= 1; p += 2)
            {
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Pole_" + p;
                pole.transform.SetParent(root.transform, false);
                pole.transform.localPosition = new Vector3(p * 0.70f, 1.25f, 0);
                pole.transform.localScale = new Vector3(0.08f, 1.25f, 0.08f);
                if (metalMat != null) pole.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
            }

            // Signboard frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "SignFrame";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localPosition = new Vector3(0, 2.05f, 0);
            frame.transform.localScale = new Vector3(1.75f, 0.95f, 0.08f);
            if (metalMat != null) frame.GetComponent<MeshRenderer>().sharedMaterial = metalMat;

            // Sign face plate with wayfinding texture
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "SignPlate";
            plate.transform.SetParent(frame.transform, false);
            plate.transform.localPosition = new Vector3(0, 0, 0.045f);
            plate.transform.localScale = new Vector3(0.96f, 0.94f, 0.20f);
            if (wayfindingMat != null) plate.GetComponent<MeshRenderer>().sharedMaterial = wayfindingMat;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.5f, 0);
            col.size = new Vector3(1.8f, 2.6f, 0.25f);

            var inter = root.AddComponent<IUHInteractable>();
            inter.interactionType = InteractionType.InfoBoard;
            inter.promptText = "Xem sơ đồ hướng dẫn khuôn viên IUH";
            inter.interactionRange = 2.2f;

            SaveAndDestroyPrefab(root, "Sign");
        }

        private static void CreateDoorPrefab()
        {
            GameObject root = new GameObject("Door");
            Material glassMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_GlassClear.mat");
            Material decalMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_GlassFrosted.mat");
            Material mullionMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_DarkMullion.mat");
            Material chromeMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_Chrome.mat");

            // Outer Door Portal Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame_Top";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localPosition = new Vector3(0, 2.35f, 0);
            frame.transform.localScale = new Vector3(2.4f, 0.12f, 0.18f);
            if (mullionMat != null) frame.GetComponent<MeshRenderer>().sharedMaterial = mullionMat;

            for (int f = -1; f <= 1; f += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "Frame_Post_" + f;
                post.transform.SetParent(root.transform, false);
                post.transform.localPosition = new Vector3(f * 1.15f, 1.18f, 0);
                post.transform.localScale = new Vector3(0.10f, 2.36f, 0.18f);
                if (mullionMat != null) post.GetComponent<MeshRenderer>().sharedMaterial = mullionMat;
            }

            // Glass door leaf
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "DoorGlass";
            glass.transform.SetParent(root.transform, false);
            glass.transform.localPosition = new Vector3(0, 1.15f, 0);
            glass.transform.localScale = new Vector3(2.15f, 2.25f, 0.04f);
            if (glassMat != null) glass.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

            // Frosted Decal Band
            GameObject decal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            decal.name = "DecalBand";
            decal.transform.SetParent(glass.transform, false);
            decal.transform.localPosition = new Vector3(0, -0.1f, 0.005f);
            decal.transform.localScale = new Vector3(1.0f, 0.45f, 0.15f);
            if (decalMat != null) decal.GetComponent<MeshRenderer>().sharedMaterial = decalMat;

            // Chrome Handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(glass.transform, false);
            handle.transform.localPosition = new Vector3(0.40f, -0.1f, 0.06f);
            handle.transform.localScale = new Vector3(0.04f, 0.50f, 0.08f);
            if (chromeMat != null) handle.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.15f, 0);
            col.size = new Vector3(2.2f, 2.3f, 0.15f);

            var inter = root.AddComponent<IUHInteractable>();
            inter.interactionType = InteractionType.Door;
            inter.promptText = "Nhấn E để mở cửa tự động";
            inter.interactionRange = 2.0f;

            SaveAndDestroyPrefab(root, "Door");
        }

        private static void CreateWindowModulePrefab()
        {
            GameObject root = new GameObject("WindowModule");
            Material glassMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_G_WindowGlass.mat");
            Material blindsMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Window_Blinds.mat");
            Material mullionMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");

            // Mullion Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "WindowFrame";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localScale = new Vector3(1.85f, 2.15f, 0.08f);
            if (mullionMat != null) frame.GetComponent<MeshRenderer>().sharedMaterial = mullionMat;

            // Glass Pane
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "Glass";
            glass.transform.SetParent(frame.transform, false);
            glass.transform.localPosition = new Vector3(0, 0, 0.01f);
            glass.transform.localScale = new Vector3(0.92f, 0.92f, 0.25f);
            if (glassMat != null) glass.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

            // Internal Blinds
            GameObject blinds = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blinds.name = "Blinds";
            blinds.transform.SetParent(frame.transform, false);
            blinds.transform.localPosition = new Vector3(0, 0.05f, 0.035f);
            blinds.transform.localScale = new Vector3(0.88f, 0.88f, 0.15f);
            if (blindsMat != null) blinds.GetComponent<MeshRenderer>().sharedMaterial = blindsMat;

            SaveAndDestroyPrefab(root, "WindowModule");
        }

        private static void CreateLampPrefab()
        {
            GameObject root = new GameObject("Lamp");
            Material poleMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");
            Material whiteMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_MatteWhite.mat");

            // Vertical Pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(root.transform, false);
            pole.transform.localPosition = new Vector3(0, 2.4f, 0);
            pole.transform.localScale = new Vector3(0.12f, 2.4f, 0.12f);
            if (poleMat != null) pole.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

            // Horizontal Overhang Arm
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "OverhangArm";
            arm.transform.SetParent(root.transform, false);
            arm.transform.localPosition = new Vector3(0.45f, 4.8f, 0);
            arm.transform.localScale = new Vector3(1.1f, 0.08f, 0.10f);
            if (poleMat != null) arm.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

            // Downward Luminaire Lamp Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "LampHead";
            head.transform.SetParent(arm.transform, false);
            head.transform.localPosition = new Vector3(0.40f, -0.06f, 0);
            head.transform.localScale = new Vector3(0.45f, 0.06f, 0.22f);
            if (whiteMat != null) head.GetComponent<MeshRenderer>().sharedMaterial = whiteMat;

            // Light component
            Light light = head.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1.0f, 0.95f, 0.88f);
            light.range = 14.0f;
            light.spotAngle = 70.0f;
            light.intensity = 2.0f;
            head.transform.localRotation = Quaternion.Euler(90f, 0, 0);

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 2.4f, 0);
            col.size = new Vector3(0.3f, 4.8f, 0.3f);

            SaveAndDestroyPrefab(root, "Lamp");
        }

        private static void CreateFireExtinguisherPrefab()
        {
            GameObject root = new GameObject("FireExtinguisher");
            Material redMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Fire_Extinguisher.mat");
            Material chromeMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Int_Chrome.mat");
            Material darkMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath + "M_IUH_Main_DarkMetal.mat");

            // Wall mounting plate
            GameObject backPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backPlate.name = "BackPlate";
            backPlate.transform.SetParent(root.transform, false);
            backPlate.transform.localPosition = new Vector3(0, 0, 0.02f);
            backPlate.transform.localScale = new Vector3(0.32f, 0.65f, 0.02f);
            if (darkMat != null) backPlate.GetComponent<MeshRenderer>().sharedMaterial = darkMat;

            // Cylinder Tank
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = "Cylinder";
            cylinder.transform.SetParent(root.transform, false);
            cylinder.transform.localPosition = new Vector3(0, -0.05f, 0.14f);
            cylinder.transform.localScale = new Vector3(0.18f, 0.25f, 0.18f);
            if (redMat != null) cylinder.GetComponent<MeshRenderer>().sharedMaterial = redMat;

            // Discharge Valve & Handle
            GameObject valve = GameObject.CreatePrimitive(PrimitiveType.Cube);
            valve.name = "Valve";
            valve.transform.SetParent(cylinder.transform, false);
            valve.transform.localPosition = new Vector3(0, 1.15f, 0);
            valve.transform.localScale = new Vector3(0.25f, 0.15f, 0.15f);
            if (chromeMat != null) valve.GetComponent<MeshRenderer>().sharedMaterial = chromeMat;

            // Discharge Hose
            GameObject hose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hose.name = "Hose";
            hose.transform.SetParent(cylinder.transform, false);
            hose.transform.localPosition = new Vector3(0.45f, 0.35f, 0);
            hose.transform.localScale = new Vector3(0.08f, 0.60f, 0.08f);
            if (darkMat != null) hose.GetComponent<MeshRenderer>().sharedMaterial = darkMat;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0, 0.14f);
            col.size = new Vector3(0.35f, 0.70f, 0.28f);

            var inter = root.AddComponent<IUHInteractable>();
            inter.interactionType = InteractionType.FireEquipment;
            inter.promptText = "Bình chữa cháy CO2 - Nhấn E để kiểm tra";
            inter.interactionRange = 1.8f;

            SaveAndDestroyPrefab(root, "FireExtinguisher");
        }
    }
}
