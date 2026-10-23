using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus.UI;

namespace IUHCampus.Editor
{
    /// <summary>
    /// Master Environment Artist & Street District Builder.
    /// Builds the vibrant Outside-Campus Street District along Nguyễn Văn Bảo street
    /// directly outside IUH Main Gate (Z = -87 to -186, X = -60 to +60):
    /// - Cụm A: Phố Ẩm Thực Sinh Viên (Quán Phở Bò IUH, Cơm Tấm, Bánh Mì, Trà Sữa, Hẻm sinh viên)
    /// - Cụm B: Khu Cà Phê & Trà Sữa (Highlands Coffee / The Coffee House, Deck ngoài trời, Dù che)
    /// - Cụm C: Cửa Hàng Tiện Lợi & Nhà Sách (FamilyMart 24/7, ATM, Nhà Sách IUH, Văn phòng phẩm)
    /// - Cụm D: Giao Thông & Bãi Xe (Bến xe buýt hiện đại, Bãi giữ xe máy mái vòm, Vạch qua đường, Đèn giao thông)
    /// - Cụm E: Công Viên Nhỏ / Góc Nghỉ Xanh (Bồn hoa tròn, Cây bóng mát, Ghế đá, Trạm phân loại rác)
    /// Connects seamlessly with IUH Main Gate at Z = -87.
    /// </summary>
    [InitializeOnLoad]
    public static class IUHStreetDistrictBuilder
    {
        private const string MatDir = "Assets/IUH_Campus/Materials/";
        private const string TexDir = "Assets/IUH_Campus/Textures/";
        private const string RootName = "IUH_STREET_DISTRICT";

        static IUHStreetDistrictBuilder()
        {
            EditorApplication.delayCall += EnsureStreetDistrictBuilt;
        }

        [MenuItem("IUH Campus/Build Outside-Campus Street District (Full 5 Clusters)")]
        public static void EnsureStreetDistrictBuilt()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "IUH_Campus_Main")
            {
                var scenePath = "Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity";
                if (File.Exists(scenePath))
                {
                    activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }

            Debug.Log("[IUH Street District] Khởi tạo xây dựng khu phố ngoài cổng trường IUH...");

            // 1. Master Root (Prevents duplicates)
            var streetRoot = GameObject.Find(RootName);
            if (streetRoot != null) UnityEngine.Object.DestroyImmediate(streetRoot);
            streetRoot = new GameObject(RootName);

            // 2. Prepare Materials
            InitMaterials();

            // 3. Build Road Network & Sidewalks (Seamless link to Gate at Z = -87)
            BuildRoadNetworkAndSidewalks(streetRoot.transform);

            // 4. Build 5 Distinct Clusters
            BuildClusterA_StudentFoodStreet(streetRoot.transform);
            BuildClusterB_CafeAndBeverages(streetRoot.transform);
            BuildClusterC_ConvenienceAndBookstore(streetRoot.transform);
            BuildClusterD_TransitAndParking(streetRoot.transform);
            BuildClusterE_PocketPark(streetRoot.transform);

            // 5. Build Street Lighting & Traffic Infrastructure
            BuildStreetInfrastructure(streetRoot.transform);

            // 6. Build 3D World Location Pins for the Street District
            BuildStreetLocationPins(streetRoot.transform);

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[IUH Street District] HOÀN THÀNH XUẤT SẮC! Khu phố bên ngoài cổng trường (Đường Nguyễn Văn Bảo) đã kết nối liền mạch với khuôn viên IUH.");
        }

        #region Material Management

        private static Material m_RoadAsphalt;
        private static Material m_SidewalkConcrete;
        private static Material m_CurbStone;
        private static Material m_DarkMetal;
        private static Material m_LightMetal;
        private static Material m_GlassReflective;
        private static Material m_WhiteStucco;
        private static Material m_BenchGranite;
        private static Material m_TreeBark;
        private static Material m_Foliage;
        private static Material m_OrangeFlowers;
        private static Material m_ParkingLine;

        // Street-specific Sign Materials
        private static Material m_MatPhoBo;
        private static Material m_MatComTam;
        private static Material m_MatHighlands;
        private static Material m_MatFamilyMart;
        private static Material m_MatNhaSach;
        private static Material m_MatTraSua;
        private static Material m_MatBusStop;
        private static Material m_MatParkingSign;
        private static Material m_MatCrosswalk;
        private static Material m_MatStreetName;
        private static Material m_MatConvenienceShelves;
        private static Material m_MatBookShelves;

        // Custom Styling Materials
        private static Material m_PlasticRed;
        private static Material m_PlasticBlue;
        private static Material m_AwningRed;
        private static Material m_AwningYellow;
        private static Material m_AwningGreen;
        private static Material m_WoodDeck;
        private static Material m_WarmInterior;
        private static Material m_BusRoofBlue;

        private static void InitMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            m_RoadAsphalt = GetOrCreateMat("M_IUH_Asphalt_Road", litShader, new Color(0.24f, 0.25f, 0.27f), 0.25f, 0f, "T_IUH_Asphalt_Cracks.png", 6f, 15f);
            m_SidewalkConcrete = GetOrCreateMat("M_IUH_Concrete_Ground", litShader, new Color(0.86f, 0.86f, 0.84f), 0.3f, 0f, "T_IUH_Concrete_Seams.png", 4f, 8f);
            m_CurbStone = GetOrCreateMat("M_IUH_Concrete_Curb", litShader, new Color(0.72f, 0.73f, 0.74f), 0.35f, 0f);
            m_DarkMetal = GetOrCreateMat("M_IUH_DarkMetal", litShader, new Color(0.16f, 0.18f, 0.22f), 0.7f, 0.85f);
            m_LightMetal = GetOrCreateMat("M_IUH_LightMetal", litShader, new Color(0.88f, 0.90f, 0.94f), 0.9f, 0.9f);
            m_GlassReflective = GetOrCreateMat("M_IUH_Glass_Reflective", litShader, new Color(0.15f, 0.42f, 0.58f), 0.95f, 0.4f);
            m_WhiteStucco = GetOrCreateMat("M_IUH_WhiteFacade", litShader, new Color(0.95f, 0.94f, 0.92f), 0.35f, 0.05f);
            m_BenchGranite = GetOrCreateMat("M_IUH_Bench_Granite", litShader, new Color(0.90f, 0.89f, 0.86f), 0.5f, 0.05f, "T_Granite_Noise.png", 2f, 2f);
            m_TreeBark = GetOrCreateMat("M_IUH_Tree_Bark", litShader, new Color(0.35f, 0.28f, 0.22f), 0.2f, 0f, "T_IUH_Tree_Bark.png", 1f, 4f);
            m_Foliage = GetOrCreateMat("M_IUH_MatureFoliage", litShader, new Color(0.22f, 0.52f, 0.18f), 0.2f, 0f, "T_Grass_Noise.png", 3f, 3f);
            m_OrangeFlowers = GetOrCreateMat("M_IUH_Flower_Orange", litShader, Color.white, 0.2f, 0f, "T_Flowers_YellowOrange.png", 2f, 2f);
            m_ParkingLine = GetOrCreateMat("M_IUH_Parking_Line", litShader, new Color(0.95f, 0.85f, 0.15f), 0.4f, 0f);

            // Street-specific Signboards
            m_MatPhoBo = GetOrCreateMat("M_Street_PhoBo", litShader, Color.white, 0.4f, 0f, "T_Street_PhoBo.png");
            m_MatComTam = GetOrCreateMat("M_Street_ComTam", litShader, Color.white, 0.4f, 0f, "T_Street_ComTam.png");
            m_MatHighlands = GetOrCreateMat("M_Street_Highlands", litShader, Color.white, 0.5f, 0.1f, "T_Street_Highlands.png");
            m_MatFamilyMart = GetOrCreateMat("M_Street_FamilyMart", litShader, Color.white, 0.4f, 0f, "T_Street_FamilyMart.png");
            m_MatNhaSach = GetOrCreateMat("M_Street_NhaSach", litShader, Color.white, 0.4f, 0f, "T_Street_NhaSach.png");
            m_MatTraSua = GetOrCreateMat("M_Street_TraSua", litShader, Color.white, 0.4f, 0f, "T_Street_TraSua.png");
            m_MatBusStop = GetOrCreateMat("M_Street_BusStop", litShader, Color.white, 0.5f, 0f, "T_Street_BusStop.png");
            m_MatParkingSign = GetOrCreateMat("M_Street_ParkingSign", litShader, Color.white, 0.5f, 0f, "T_Street_ParkingSign.png");
            m_MatCrosswalk = GetOrCreateMat("M_Street_Crosswalk", litShader, Color.white, 0.3f, 0f, "T_Street_Crosswalk.png");
            m_MatStreetName = GetOrCreateMat("M_Street_StreetNameSign", litShader, Color.white, 0.6f, 0.1f, "T_Street_StreetNameSign.png");
            m_MatConvenienceShelves = GetOrCreateMat("M_Street_ConvenienceShelves", litShader, Color.white, 0.4f, 0f, "T_Street_ConvenienceShelves.png");
            m_MatBookShelves = GetOrCreateMat("M_Street_BookShelves", litShader, Color.white, 0.3f, 0f, "T_Street_BookShelves.png");

            // Props & Details
            m_PlasticRed = GetOrCreateMat("M_Street_PlasticRed", litShader, new Color(0.92f, 0.18f, 0.15f), 0.6f, 0.05f);
            m_PlasticBlue = GetOrCreateMat("M_Street_PlasticBlue", litShader, new Color(0.12f, 0.48f, 0.88f), 0.6f, 0.05f);
            m_AwningRed = GetOrCreateMat("M_Street_Awning_Red", litShader, new Color(0.85f, 0.15f, 0.15f), 0.25f, 0f);
            m_AwningYellow = GetOrCreateMat("M_Street_Awning_Yellow", litShader, new Color(0.95f, 0.75f, 0.15f), 0.25f, 0f);
            m_AwningGreen = GetOrCreateMat("M_Street_Awning_Green", litShader, new Color(0.15f, 0.65f, 0.25f), 0.25f, 0f);
            m_WoodDeck = GetOrCreateMat("M_Street_WoodDeck", litShader, new Color(0.48f, 0.32f, 0.22f), 0.45f, 0f);
            m_WarmInterior = GetOrCreateMat("M_Street_WarmInterior", litShader, new Color(1.0f, 0.92f, 0.75f), 0.2f, 0f);
            m_WarmInterior.EnableKeyword("_EMISSION");
            m_WarmInterior.SetColor("_EmissionColor", new Color(0.45f, 0.35f, 0.2f));

            m_BusRoofBlue = GetOrCreateMat("M_Street_BusRoofBlue", litShader, new Color(0.12f, 0.42f, 0.78f), 0.7f, 0.3f);
        }

        private static Material GetOrCreateMat(string matName, Shader shader, Color baseCol, float smoothness, float metallic, string texName = null, float tileX = 1f, float tileY = 1f)
        {
            string path = MatDir + matName + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.name = matName;
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", baseCol);
            mat.SetColor("_Color", baseCol);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            if (!string.IsNullOrEmpty(texName))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + texName);
                if (tex != null)
                {
                    mat.mainTexture = tex;
                    mat.mainTextureScale = new Vector2(tileX, tileY);
                }
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        #endregion

        #region 1. Road Network & Sidewalks

        private static void BuildRoadNetworkAndSidewalks(Transform root)
        {
            var roadGO = new GameObject("01_RoadNetwork_And_Sidewalks");
            roadGO.transform.SetParent(root, false);

            // A. Main Avenue (Đại lộ Nguyễn Văn Bảo): Z = -87 to -180 (Length 93m, Width 14m, X: -7 to +7)
            var mainAvenue = CreateCube(roadGO.transform, "MainAvenue_Asphalt", new Vector3(0f, -0.02f, -133.5f), new Vector3(14.0f, 0.08f, 93.0f), m_RoadAsphalt);

            // Double Yellow Center Road Divider Line
            var yellowLineL = CreateCube(roadGO.transform, "CenterLine_Yellow_L", new Vector3(-0.15f, 0.03f, -133.5f), new Vector3(0.12f, 0.02f, 93.0f), m_ParkingLine);
            var yellowLineR = CreateCube(roadGO.transform, "CenterLine_Yellow_R", new Vector3(0.15f, 0.03f, -133.5f), new Vector3(0.12f, 0.02f, 93.0f), m_ParkingLine);

            // White Dashed Lane Markers (for 4 lanes total, 2 on left, 2 on right)
            for (int i = 0; i < 15; i++)
            {
                float zDash = -90f - i * 6f;
                if (zDash >= -131f && zDash <= -119f) continue; // Skip intersection center
                CreateCube(roadGO.transform, $"LaneDash_L_{i}", new Vector3(-3.5f, 0.03f, zDash), new Vector3(0.18f, 0.02f, 3.2f), m_LightMetal);
                CreateCube(roadGO.transform, $"LaneDash_R_{i}", new Vector3(3.5f, 0.03f, zDash), new Vector3(0.18f, 0.02f, 3.2f), m_LightMetal);
            }

            // B. Intersecting Cross-Street at Z = -125 (Phố Ẩm Thực & Thương Mại: X from -55 to +55, Width 12m)
            var crossStreet = CreateCube(roadGO.transform, "CrossStreet_Asphalt", new Vector3(0f, -0.02f, -125.0f), new Vector3(110.0f, 0.08f, 12.0f), m_RoadAsphalt);

            // C. 4 Zebra Pedestrian Crosswalks at 4-way intersection
            CreateCrosswalk(roadGO.transform, "Crosswalk_North", new Vector3(0f, 0.03f, -117.5f), new Vector3(13.5f, 0.02f, 2.8f));
            CreateCrosswalk(roadGO.transform, "Crosswalk_South", new Vector3(0f, 0.03f, -132.5f), new Vector3(13.5f, 0.02f, 2.8f));
            CreateCrosswalk(roadGO.transform, "Crosswalk_West", new Vector3(-8.5f, 0.03f, -125.0f), new Vector3(2.8f, 0.02f, 11.5f));
            CreateCrosswalk(roadGO.transform, "Crosswalk_East", new Vector3(8.5f, 0.03f, -125.0f), new Vector3(2.8f, 0.02f, 11.5f));

            // D. Sidewalk Platforms (4 Quadrants with curbs)
            // 1. North-West Sidewalk (X: -7 to -14, Z: -87 to -119)
            CreateSidewalkBlock(roadGO.transform, "Sidewalk_NW", new Vector3(-10.5f, 0.10f, -103.0f), new Vector3(7.0f, 0.20f, 32.0f));
            // 2. North-East Sidewalk (X: +7 to +14, Z: -87 to -119)
            CreateSidewalkBlock(roadGO.transform, "Sidewalk_NE", new Vector3(10.5f, 0.10f, -103.0f), new Vector3(7.0f, 0.20f, 32.0f));
            // 3. South-West Sidewalk (X: -7 to -14, Z: -131 to -165)
            CreateSidewalkBlock(roadGO.transform, "Sidewalk_SW", new Vector3(-10.5f, 0.10f, -148.0f), new Vector3(7.0f, 0.20f, 34.0f));
            // 4. South-East Sidewalk (X: +7 to +14, Z: -131 to -165)
            CreateSidewalkBlock(roadGO.transform, "Sidewalk_SE", new Vector3(10.5f, 0.10f, -148.0f), new Vector3(7.0f, 0.20f, 34.0f));
        }

        private static void CreateCrosswalk(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var cw = CreateCube(parent, name, pos, size, m_MatCrosswalk);
            var col = cw.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = true; // Decorative overlay, no physics bump
        }

        private static void CreateSidewalkBlock(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;

            // Surface
            CreateCube(go.transform, "Pavement", Vector3.zero, size, m_SidewalkConcrete);
            // Curb edge facing road
            float curbX = pos.x < 0 ? size.x * 0.5f - 0.15f : -size.x * 0.5f + 0.15f;
            CreateCube(go.transform, "CurbStone", new Vector3(curbX, 0.02f, 0f), new Vector3(0.3f, size.y + 0.04f, size.z), m_CurbStone);
        }

        #endregion

        #region 2. Cụm A: Khu Ẩm Thực Sinh Viên (North-West)

        private static void BuildClusterA_StudentFoodStreet(Transform root)
        {
            var cluster = new GameObject("02_Cluster_A_StudentFoodStreet");
            cluster.transform.SetParent(root, false);
            cluster.transform.localPosition = new Vector3(-25.0f, 0f, -103.0f); // Center of NW block

            // 1. Shophouse Complex Structure (3-story building body: Width 22m, Depth 18m, Height 11m)
            var buildingBody = CreateCube(cluster.transform, "Shophouse_FoodBlock", new Vector3(0f, 5.5f, 0f), new Vector3(22.0f, 11.0f, 18.0f), m_WhiteStucco);
            // Roof Balustrade / Parapet
            CreateCube(cluster.transform, "Roof_Parapet", new Vector3(0f, 11.3f, 0f), new Vector3(22.4f, 0.6f, 18.4f), m_DarkMetal);

            // ================= STORE 1: QUÁN PHỞ BÒ IUH (Z = -90 to -98 => local Z = +9m) =================
            var phoStore = new GameObject("Store_PhoBo_IUH");
            phoStore.transform.SetParent(cluster.transform, false);
            phoStore.transform.localPosition = new Vector3(0f, 0f, 7.5f);

            // Signboard Billboard
            var phoSign = CreateCube(phoStore.transform, "Sign_PhoBo", new Vector3(11.1f, 3.8f, 0f), new Vector3(0.15f, 1.8f, 6.5f), m_MatPhoBo);
            phoSign.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Red Fabric Awning over Sidewalk
            var phoAwning = CreateCube(phoStore.transform, "Awning_PhoBo", new Vector3(12.5f, 3.1f, 0f), new Vector3(2.8f, 0.15f, 6.8f), m_AwningRed);
            phoAwning.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);

            // Street Food Counter (Stainless Steel & Glass)
            var counter = CreateCube(phoStore.transform, "Pho_ServiceCounter", new Vector3(12.0f, 0.5f, 0f), new Vector3(1.2f, 1.0f, 4.2f), m_LightMetal);
            CreateCube(counter.transform, "GlassSneezeGuard", new Vector3(0.2f, 0.45f, 0f), new Vector3(0.1f, 0.5f, 4.0f), m_GlassReflective);
            // Giant Steaming Broth Pot (Cylinder)
            var pot = CreateCylinder(counter.transform, "BrothPot", new Vector3(-0.1f, 0.65f, 1.0f), new Vector3(0.6f, 0.45f, 0.6f), m_LightMetal);

            // Sidewalk Plastic Tables & Stools (3 sets: Bàn ghế nhựa đỏ/xanh chuẩn vỉa hè Việt Nam)
            for (int t = -1; t <= 1; t++)
            {
                var tableSet = new GameObject($"PlasticDiningSet_{t}");
                tableSet.transform.SetParent(phoStore.transform, false);
                tableSet.transform.localPosition = new Vector3(14.2f, 0.15f, t * 2.0f);

                // Table (Red plastic)
                var table = CreateCube(tableSet.transform, "Table", new Vector3(0f, 0.28f, 0f), new Vector3(0.9f, 0.56f, 0.9f), m_PlasticRed);
                // 4 Stools around table (Blue plastic)
                CreateCube(tableSet.transform, "Stool_1", new Vector3(-0.6f, 0.15f, 0f), new Vector3(0.32f, 0.30f, 0.32f), m_PlasticBlue);
                CreateCube(tableSet.transform, "Stool_2", new Vector3(0.6f, 0.15f, 0f), new Vector3(0.32f, 0.30f, 0.32f), m_PlasticBlue);
                CreateCube(tableSet.transform, "Stool_3", new Vector3(0f, 0.15f, -0.6f), new Vector3(0.32f, 0.30f, 0.32f), m_PlasticBlue);
                CreateCube(tableSet.transform, "Stool_4", new Vector3(0f, 0.15f, 0.6f), new Vector3(0.32f, 0.30f, 0.32f), m_PlasticBlue);
            }

            // ================= STORE 2: CƠM TẤM ĐÊM SINH VIÊN (local Z = 0m) =================
            var comStore = new GameObject("Store_ComTam_IUH");
            comStore.transform.SetParent(cluster.transform, false);
            comStore.transform.localPosition = new Vector3(0f, 0f, 0f);

            var comSign = CreateCube(comStore.transform, "Sign_ComTam", new Vector3(11.1f, 3.8f, 0f), new Vector3(0.15f, 1.8f, 6.5f), m_MatComTam);
            comSign.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            var comAwning = CreateCube(comStore.transform, "Awning_ComTam", new Vector3(12.5f, 3.1f, 0f), new Vector3(2.8f, 0.15f, 6.8f), m_AwningYellow);
            comAwning.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);

            // Sidewalk Barbecue Smoke Grill Cart
            var bbqCart = CreateCube(comStore.transform, "BBQ_GrillCart", new Vector3(12.6f, 0.55f, 2.2f), new Vector3(0.85f, 1.1f, 1.4f), m_DarkMetal);
            CreateCylinder(bbqCart.transform, "SmokeChimney", new Vector3(0f, 0.8f, 0f), new Vector3(0.18f, 0.7f, 0.18f), m_DarkMetal);

            // ================= STORE 3: TRÀ SỮA & BÁNH MÌ (local Z = -7.5m) =================
            var traSuaStore = new GameObject("Store_TraSua_BanhMi");
            traSuaStore.transform.SetParent(cluster.transform, false);
            traSuaStore.transform.localPosition = new Vector3(0f, 0f, -7.5f);

            var traSuaSign = CreateCube(traSuaStore.transform, "Sign_TraSua", new Vector3(11.1f, 3.8f, 0f), new Vector3(0.15f, 1.8f, 6.5f), m_MatTraSua);
            traSuaSign.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Mobile Bánh Mì Cart on Sidewalk
            var banhMiCart = CreateCube(traSuaStore.transform, "BanhMi_GlassCart", new Vector3(12.8f, 0.65f, -1.5f), new Vector3(0.9f, 1.3f, 1.8f), m_GlassReflective);
            CreateCube(banhMiCart.transform, "CartRoof", new Vector3(0f, 0.72f, 0f), new Vector3(1.1f, 0.1f, 2.0f), m_AwningYellow);
            CreateCylinder(banhMiCart.transform, "Wheel_1", new Vector3(0f, -0.45f, -0.6f), new Vector3(0.4f, 0.1f, 0.4f), m_DarkMetal);
            CreateCylinder(banhMiCart.transform, "Wheel_2", new Vector3(0f, -0.45f, 0.6f), new Vector3(0.4f, 0.1f, 0.4f), m_DarkMetal);

            // ================= ALLEYWAY: HẺM SINH VIÊN (Horror / Escape Mini-game 2) =================
            var alleyway = new GameObject("Alleyway_StudentAlley");
            alleyway.transform.SetParent(cluster.transform, false);
            alleyway.transform.localPosition = new Vector3(0f, 0f, -14.0f); // Edge of building

            var alleyDoor = CreateCube(alleyway.transform, "Alley_SecretDoor", new Vector3(11.05f, 1.2f, 0f), new Vector3(0.1f, 2.4f, 1.4f), m_DarkMetal);
            var dimLantern = CreateSphere(alleyway.transform, "Alley_DimLantern", new Vector3(11.4f, 2.7f, 0f), new Vector3(0.35f, 0.35f, 0.35f), m_MatComTam);
            var light = dimLantern.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.45f, 0.15f);
            light.range = 6.0f;
            light.intensity = 1.2f;
        }

        #endregion

        #region 3. Cụm B: Khu Cà Phê & Trà Sữa Thư Giãn (North-East)

        private static void BuildClusterB_CafeAndBeverages(Transform root)
        {
            var cluster = new GameObject("03_Cluster_B_CafeAndBeverages");
            cluster.transform.SetParent(root, false);
            cluster.transform.localPosition = new Vector3(25.0f, 0f, -103.0f); // Center of NE block

            // 1. Highlands Coffee Commercial Building (2-story: Width 22m, Depth 18m, Height 9.5m)
            var building = CreateCube(cluster.transform, "Highlands_Building", new Vector3(0f, 4.75f, 0f), new Vector3(22.0f, 9.5f, 18.0f), m_DarkMetal);

            // Front Floor-to-Ceiling Tinted Glass Facade
            var glassFacade = CreateCube(cluster.transform, "GlassFacade", new Vector3(-10.9f, 4.75f, 0f), new Vector3(0.2f, 9.0f, 17.0f), m_GlassReflective);
            // Warm Interior Illumination
            var interiorLight = CreateCube(cluster.transform, "InteriorGlow", new Vector3(-8.0f, 4.75f, 0f), new Vector3(1.0f, 8.5f, 16.0f), m_WarmInterior);

            // Signboard Header with Wood Slats
            var sign = CreateCube(cluster.transform, "Sign_HighlandsCoffee", new Vector3(-11.1f, 4.8f, 0f), new Vector3(0.15f, 2.2f, 14.0f), m_MatHighlands);
            sign.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

            // 2. Raised Outdoor Timber Patio Deck on Sidewalk (Width 6m, Length 16m, Height 0.25m)
            var deck = CreateCube(cluster.transform, "OutdoorTimberDeck", new Vector3(-13.5f, 0.22f, 0f), new Vector3(5.0f, 0.25f, 16.0f), m_WoodDeck);
            // Deck Railings
            CreateCube(deck.transform, "Railing_North", new Vector3(0f, 0.5f, 7.9f), new Vector3(4.8f, 0.9f, 0.08f), m_DarkMetal);
            CreateCube(deck.transform, "Railing_South", new Vector3(0f, 0.5f, -7.9f), new Vector3(4.8f, 0.9f, 0.08f), m_DarkMetal);
            CreateCube(deck.transform, "Railing_West", new Vector3(-2.4f, 0.5f, 0f), new Vector3(0.08f, 0.9f, 15.8f), m_DarkMetal);

            // 3. 4 Large Red Highlands Patio Umbrellas with Tables & Chairs
            float[] zOffsets = new float[] { -5.5f, -1.8f, 1.8f, 5.5f };
            for (int i = 0; i < zOffsets.Length; i++)
            {
                var cafeSet = new GameObject($"Highlands_TableSet_{i + 1}");
                cafeSet.transform.SetParent(cluster.transform, false);
                cafeSet.transform.localPosition = new Vector3(-13.5f, 0.35f, zOffsets[i]);

                // Large Red Umbrella
                var mast = CreateCylinder(cafeSet.transform, "UmbrellaMast", new Vector3(0f, 1.4f, 0f), new Vector3(0.08f, 2.8f, 0.08f), m_DarkMetal);
                var canopy = CreateCylinder(cafeSet.transform, "UmbrellaCanopy", new Vector3(0f, 2.6f, 0f), new Vector3(2.4f, 0.35f, 2.4f), m_AwningRed);

                // Round Wooden Table
                CreateCylinder(cafeSet.transform, "RoundTable", new Vector3(0f, 0.38f, 0f), new Vector3(0.95f, 0.75f, 0.95f), m_WoodDeck);

                // 2 Modern Curved Armchairs
                CreateCube(cafeSet.transform, "Chair_L", new Vector3(-0.75f, 0.25f, 0f), new Vector3(0.45f, 0.5f, 0.45f), m_DarkMetal);
                CreateCube(cafeSet.transform, "Chair_R", new Vector3(0.75f, 0.25f, 0f), new Vector3(0.45f, 0.5f, 0.45f), m_DarkMetal);
            }

            // 4. Barista Street Takeaway Pickup Window (For Grab Campus Bike Orders)
            var pickupWindow = CreateCube(cluster.transform, "BaristaPickupWindow", new Vector3(-11.1f, 1.5f, -6.5f), new Vector3(0.25f, 1.4f, 2.2f), m_LightMetal);
            var pickupAwning = CreateCube(pickupWindow.transform, "PickupAwning", new Vector3(0f, 0.9f, 0f), new Vector3(0.9f, 0.1f, 2.4f), m_AwningRed);
        }

        #endregion

        #region 4. Cụm C: Cửa Hàng Tiện Lợi & Nhà Sách (South-West)

        private static void BuildClusterC_ConvenienceAndBookstore(Transform root)
        {
            var cluster = new GameObject("04_Cluster_C_ConvenienceAndBookstore");
            cluster.transform.SetParent(root, false);
            cluster.transform.localPosition = new Vector3(-25.0f, 0f, -148.0f); // Center of SW block

            // Building Structure
            CreateCube(cluster.transform, "Building_CommerceBlock", new Vector3(0f, 5.0f, 0f), new Vector3(22.0f, 10.0f, 28.0f), m_WhiteStucco);

            // ================= 1. FAMILYMART 24/7 (Z = -133 to -146 => local Z = +7m) =================
            var martGO = new GameObject("Store_FamilyMart_24H");
            martGO.transform.SetParent(cluster.transform, false);
            martGO.transform.localPosition = new Vector3(0f, 0f, 7.0f);

            // Billboard Sign
            var martSign = CreateCube(martGO.transform, "Sign_FamilyMart", new Vector3(11.1f, 4.0f, 0f), new Vector3(0.15f, 2.0f, 12.0f), m_MatFamilyMart);
            martSign.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Modern Canopy Overhang
            CreateCube(martGO.transform, "Canopy_Overhang", new Vector3(12.4f, 3.5f, 0f), new Vector3(2.6f, 0.25f, 12.5f), m_LightMetal);

            // Glass Shopfront with Shelves Display Texture
            var glassDisplay = CreateCube(martGO.transform, "Showcase_Shelves", new Vector3(11.05f, 1.6f, 2.5f), new Vector3(0.1f, 2.6f, 6.0f), m_MatConvenienceShelves);
            // Sliding Glass Doors
            CreateCube(martGO.transform, "AutomaticSlidingDoors", new Vector3(11.05f, 1.4f, -3.2f), new Vector3(0.1f, 2.8f, 2.6f), m_GlassReflective);

            // Outdoor Bank ATM Kiosk (Built into exterior corner)
            var atm = CreateCube(martGO.transform, "ATM_BankingKiosk", new Vector3(11.3f, 1.2f, -5.5f), new Vector3(0.6f, 2.4f, 1.4f), m_DarkMetal);
            CreateCube(atm.transform, "ATMScreen", new Vector3(0.32f, 0.3f, 0f), new Vector3(0.05f, 0.5f, 0.7f), m_MatFamilyMart);

            // ================= 2. NHÀ SÁCH IUH & VĂN PHÒNG PHẨM (local Z = -7m) =================
            var bookGO = new GameObject("Store_NhaSach_IUH");
            bookGO.transform.SetParent(cluster.transform, false);
            bookGO.transform.localPosition = new Vector3(0f, 0f, -7.0f);

            // Billboard Sign
            var bookSign = CreateCube(bookGO.transform, "Sign_NhaSachIUH", new Vector3(11.1f, 4.0f, 0f), new Vector3(0.15f, 2.0f, 12.0f), m_MatNhaSach);
            bookSign.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Bookstore Filled Shelves Window
            CreateCube(bookGO.transform, "Showcase_BookShelves", new Vector3(11.05f, 1.6f, 0f), new Vector3(0.1f, 2.6f, 10.0f), m_MatBookShelves);

            // Exterior Promotional Book Stand & Photocopy Service Sign
            var bookStand = CreateCube(bookGO.transform, "SidewalkBookRack", new Vector3(12.5f, 0.65f, 2.5f), new Vector3(0.7f, 1.3f, 2.4f), m_DarkMetal);
            CreateCube(bookStand.transform, "StandBooks", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.4f, 2.2f), m_MatBookShelves);
        }

        #endregion

        #region 5. Cụm D: Giao Thông, Bãi Giữ Xe & Bến Xe Buýt (South-East)

        private static void BuildClusterD_TransitAndParking(Transform root)
        {
            var cluster = new GameObject("05_Cluster_D_TransitAndParking");
            cluster.transform.SetParent(root, false);
            cluster.transform.localPosition = new Vector3(25.0f, 0f, -148.0f); // Center of SE block

            // ================= 1. BẾN XE BUÝT HIỆN ĐẠI (Bus Stop on Sidewalk at Z = -138) =================
            var busShelter = new GameObject("BusStop_ModernShelter");
            busShelter.transform.SetParent(cluster.transform, false);
            busShelter.transform.localPosition = new Vector3(-13.5f, 0.15f, 9.5f); // Near sidewalk curb

            // Curved Steel Canopy Roof
            var busRoof = CreateCube(busShelter.transform, "CurvedCanopyRoof", new Vector3(0f, 2.85f, 0f), new Vector3(3.2f, 0.25f, 8.5f), m_BusRoofBlue);
            // 4 Steel Support Pillars
            CreateCylinder(busShelter.transform, "Pillar_1", new Vector3(1.2f, 1.4f, -3.8f), new Vector3(0.12f, 2.8f, 0.12f), m_LightMetal);
            CreateCylinder(busShelter.transform, "Pillar_2", new Vector3(1.2f, 1.4f, 3.8f), new Vector3(0.12f, 2.8f, 0.12f), m_LightMetal);
            CreateCylinder(busShelter.transform, "Pillar_3", new Vector3(-1.2f, 1.4f, -3.8f), new Vector3(0.12f, 2.8f, 0.12f), m_LightMetal);
            CreateCylinder(busShelter.transform, "Pillar_4", new Vector3(-1.2f, 1.4f, 3.8f), new Vector3(0.12f, 2.8f, 0.12f), m_LightMetal);

            // Glass Windbreak Wall with Green Transit Poster
            var posterWall = CreateCube(busShelter.transform, "TransitPosterBoard", new Vector3(1.15f, 1.4f, 0f), new Vector3(0.08f, 2.2f, 6.5f), m_MatBusStop);
            posterWall.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

            // Stainless Steel Commuter Waiting Bench
            CreateCube(busShelter.transform, "WaitingBench", new Vector3(0.5f, 0.45f, 0f), new Vector3(0.6f, 0.45f, 4.5f), m_LightMetal);

            // Iconic Blue Bus Stop Signpost 🚏
            var signPost = CreateCylinder(busShelter.transform, "BusSignPost", new Vector3(-1.2f, 1.6f, 4.6f), new Vector3(0.08f, 3.2f, 0.08f), m_LightMetal);
            var busSign = CreateCube(signPost.transform, "BusRouteSign", new Vector3(0f, 1.35f, 0f), new Vector3(0.1f, 0.8f, 0.8f), m_MatBusStop);

            // Road Yellow Bus Bay Marking
            var busBay = CreateCube(cluster.transform, "Road_BusBay_Marking", new Vector3(-16.8f, 0.02f, 9.5f), new Vector3(2.5f, 0.02f, 14.0f), m_ParkingLine);
            var colBay = busBay.GetComponent<BoxCollider>();
            if (colBay != null) colBay.isTrigger = true;

            // ================= 2. BÃI GIỮ XE MÁY SINH VIÊN NGOÀI CỔNG (Z = -148 to -162) =================
            var parkingStation = new GameObject("MotorbikeParking_Station");
            parkingStation.transform.SetParent(cluster.transform, false);
            parkingStation.transform.localPosition = new Vector3(0f, 0f, -4.5f);

            // Large Blue Arched Corrugated Canopy (Width 18m, Length 14m)
            var parkRoof = CreateCube(parkingStation.transform, "StationRoofCanopy", new Vector3(0f, 3.6f, 0f), new Vector3(18.0f, 0.35f, 14.0f), m_BusRoofBlue);
            // Sign Header
            var parkSign = CreateCube(parkingStation.transform, "Sign_ParkingStation", new Vector3(-8.9f, 3.6f, 0f), new Vector3(0.15f, 1.2f, 8.0f), m_MatParkingSign);
            parkSign.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

            // Security Guard Booth with Automated Boom Barrier
            var booth = CreateCube(parkingStation.transform, "GuardBooth", new Vector3(-7.5f, 1.5f, 5.0f), new Vector3(2.8f, 3.0f, 2.6f), m_WhiteStucco);
            CreateCube(booth.transform, "BoothWindow", new Vector3(-1.42f, 0.2f, 0f), new Vector3(0.08f, 1.2f, 1.8f), m_GlassReflective);
            // Boom Barrier (Thanh chắn tự động)
            var barrierPost = CreateCylinder(parkingStation.transform, "BarrierPost", new Vector3(-8.8f, 0.6f, 3.0f), new Vector3(0.25f, 1.2f, 0.25f), m_DarkMetal);
            var barrierArm = CreateCube(barrierPost.transform, "BarrierArm", new Vector3(0f, 0.45f, -2.0f), new Vector3(0.1f, 0.12f, 4.0f), m_AwningRed);

            // Yellow Parking Bay Lines & Parked Motorbikes (2 Rows x 6 bays)
            for (int row = -1; row <= 1; row += 2)
            {
                float xRow = row * 4.5f;
                for (int b = -2; b <= 2; b++)
                {
                    float zBay = b * 2.2f;
                    // Parking bay line
                    CreateCube(parkingStation.transform, $"BayLine_{row}_{b}", new Vector3(xRow, 0.03f, zBay), new Vector3(2.8f, 0.02f, 0.1f), m_ParkingLine);

                    // Parked Motorbike
                    var bike = CreateCube(parkingStation.transform, $"ParkedMotorbike_{row}_{b}", new Vector3(xRow, 0.55f, zBay + 0.8f), new Vector3(0.6f, 1.0f, 1.6f), (b % 2 == 0) ? m_PlasticRed : m_PlasticBlue);
                    CreateCylinder(bike.transform, "WheelF", new Vector3(0f, -0.25f, 0.7f), new Vector3(0.45f, 0.12f, 0.45f), m_DarkMetal);
                    CreateCylinder(bike.transform, "WheelR", new Vector3(0f, -0.25f, -0.7f), new Vector3(0.45f, 0.12f, 0.45f), m_DarkMetal);
                    CreateCube(bike.transform, "Handlebars", new Vector3(0f, 0.45f, 0.5f), new Vector3(0.85f, 0.08f, 0.08f), m_LightMetal);
                }
            }
        }

        #endregion

        #region 6. Cụm E: Công Viên Nhỏ / Góc Nghỉ Xanh (South End)

        private static void BuildClusterE_PocketPark(Transform root)
        {
            var cluster = new GameObject("06_Cluster_E_PocketPark");
            cluster.transform.SetParent(root, false);
            cluster.transform.localPosition = new Vector3(0f, 0f, -178.0f); // South end turnaround

            // 1. Paved Plaza Ground (Circular/Octagonal Paved Area: Radius 16m)
            CreateCylinder(cluster.transform, "ParkPlazaPavement", new Vector3(0f, 0.08f, 0f), new Vector3(32.0f, 0.16f, 32.0f), m_SidewalkConcrete);

            // 2. Central Raised Stone Planter Ring (Diameter 9m)
            var planter = CreateCylinder(cluster.transform, "CentralFlowerPlanter", new Vector3(0f, 0.45f, 0f), new Vector3(9.0f, 0.75f, 9.0f), m_CurbStone);
            CreateCylinder(planter.transform, "PlanterSoil", new Vector3(0f, 0.15f, 0f), new Vector3(8.2f, 0.65f, 8.2f), m_OrangeFlowers);

            // Central Mature Tropical Shade Tree
            var tree = new GameObject("CentralParkTree");
            tree.transform.SetParent(cluster.transform, false);
            tree.transform.localPosition = Vector3.zero;
            CreateCylinder(tree.transform, "Trunk", new Vector3(0f, 2.5f, 0f), new Vector3(0.7f, 5.0f, 0.7f), m_TreeBark);
            CreateSphere(tree.transform, "Canopy", new Vector3(0f, 5.8f, 0f), new Vector3(6.5f, 4.5f, 6.5f), m_Foliage);

            // IUH Stone Crest Plaque in front of tree
            var monument = CreateCube(cluster.transform, "IUH_ParkMonument", new Vector3(0f, 1.1f, 3.2f), new Vector3(2.4f, 1.8f, 0.45f), m_BenchGranite);
            var crest = CreateCylinder(monument.transform, "BlueCrest", new Vector3(0f, 0.35f, 0.25f), new Vector3(0.9f, 0.08f, 0.9f), m_BusRoofBlue);

            // 3. 4 Curved Granite Park Benches facing the central flowers
            for (int i = 0; i < 4; i++)
            {
                float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                float r = 7.2f;
                Vector3 bPos = new Vector3(Mathf.Sin(angle) * r, 0.35f, Mathf.Cos(angle) * r);
                var bench = CreateCube(cluster.transform, $"ParkBench_{i + 1}", bPos, new Vector3(2.0f, 0.5f, 0.65f), m_BenchGranite);
                bench.transform.rotation = Quaternion.Euler(0f, (i * 90f + 45f) + 180f, 0f);
            }

            // 4. Smart 3-Bin Waste Sorting Station (Green Campus mini-game integration)
            var binStation = new GameObject("WasteSorting_Station");
            binStation.transform.SetParent(cluster.transform, false);
            binStation.transform.localPosition = new Vector3(-8.5f, 0.15f, 4.5f);

            var baseBoard = CreateCube(binStation.transform, "StationBase", new Vector3(0f, 0.1f, 0f), new Vector3(3.5f, 0.2f, 1.4f), m_DarkMetal);
            CreateCube(binStation.transform, "Bin_Green_Organic", new Vector3(-1.0f, 0.65f, 0f), new Vector3(0.75f, 1.1f, 0.75f), m_AwningGreen);
            CreateCube(binStation.transform, "Bin_Blue_Recycle", new Vector3(0f, 0.65f, 0f), new Vector3(0.75f, 1.1f, 0.75f), m_BusRoofBlue);
            CreateCube(binStation.transform, "Bin_Red_Hazardous", new Vector3(1.0f, 0.65f, 0f), new Vector3(0.75f, 1.1f, 0.75f), m_PlasticRed);

            // 5. Bicycle Parking Racks (Curved metal tubes)
            var bikeRack = new GameObject("BicycleParkingRack");
            bikeRack.transform.SetParent(cluster.transform, false);
            bikeRack.transform.localPosition = new Vector3(8.5f, 0.15f, 4.5f);
            for (int k = -2; k <= 2; k++)
            {
                CreateCylinder(bikeRack.transform, $"RackTube_{k}", new Vector3(k * 0.7f, 0.45f, 0f), new Vector3(0.06f, 0.9f, 0.06f), m_LightMetal);
            }
        }

        #endregion

        #region 7. Street Infrastructure (Lighting, Traffic, Trees, Signs)

        private static void BuildStreetInfrastructure(Transform root)
        {
            var infra = new GameObject("07_StreetInfrastructure");
            infra.transform.SetParent(root, false);

            // A. Dual-Head Street Lampposts along sidewalks (4 along West, 4 along East)
            float[] zPoles = new float[] { -95f, -112f, -140f, -160f };
            foreach (float zp in zPoles)
            {
                // West pole (X = -8.5m)
                CreateLamppost(infra.transform, $"LampPost_W_{zp}", new Vector3(-8.5f, 0.15f, zp));
                // East pole (X = +8.5m)
                CreateLamppost(infra.transform, $"LampPost_E_{zp}", new Vector3(8.5f, 0.15f, zp));
            }

            // B. Traffic Signal Light Poles at the 4-way intersection (Z = -125)
            CreateTrafficLight(infra.transform, "TrafficLight_NW", new Vector3(-8.2f, 0.15f, -118.5f), 180f);
            CreateTrafficLight(infra.transform, "TrafficLight_SE", new Vector3(8.2f, 0.15f, -131.5f), 0f);

            // C. Vietnamese Street Name Signs: "ĐƯỜNG NGUYỄN VĂN BẢO"
            CreateStreetSign(infra.transform, "StreetSign_NW", new Vector3(-8.2f, 0.15f, -119.5f), 0f);
            CreateStreetSign(infra.transform, "StreetSign_SE", new Vector3(8.2f, 0.15f, -130.5f), 180f);

            // D. Sidewalk Street Trees with square curb grates
            float[] zTrees = new float[] { -100f, -152f };
            foreach (float zt in zTrees)
            {
                CreateStreetTree(infra.transform, $"StreetTree_W_{zt}", new Vector3(-12.5f, 0.15f, zt));
                CreateStreetTree(infra.transform, $"StreetTree_E_{zt}", new Vector3(12.5f, 0.15f, zt));
            }
        }

        private static void CreateLamppost(Transform parent, string name, Vector3 pos)
        {
            var poleGO = new GameObject(name);
            poleGO.transform.SetParent(parent, false);
            poleGO.transform.localPosition = pos;

            // Base & Column (Height 6.5m)
            CreateCylinder(poleGO.transform, "Base", new Vector3(0f, 0.4f, 0f), new Vector3(0.45f, 0.8f, 0.45f), m_DarkMetal);
            CreateCylinder(poleGO.transform, "Shaft", new Vector3(0f, 3.4f, 0f), new Vector3(0.18f, 6.0f, 0.18f), m_DarkMetal);

            // Crossarm & Lamps
            var arm = CreateCube(poleGO.transform, "CrossArm", new Vector3(0f, 6.3f, 0f), new Vector3(2.2f, 0.14f, 0.14f), m_DarkMetal);
            var lampL = CreateSphere(arm.transform, "LanternL", new Vector3(-0.95f, -0.2f, 0f), new Vector3(0.45f, 0.35f, 0.45f), m_WarmInterior);
            var lampR = CreateSphere(arm.transform, "LanternR", new Vector3(0.95f, -0.2f, 0f), new Vector3(0.45f, 0.35f, 0.45f), m_WarmInterior);

            // Light Source
            var light = lampL.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.95f, 0.85f);
            light.range = 14.0f;
            light.intensity = 1.35f;
        }

        private static void CreateTrafficLight(Transform parent, string name, Vector3 pos, float rotY)
        {
            var tl = new GameObject(name);
            tl.transform.SetParent(parent, false);
            tl.transform.localPosition = pos;
            tl.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Pole
            CreateCylinder(tl.transform, "Pole", new Vector3(0f, 2.5f, 0f), new Vector3(0.14f, 5.0f, 0.14f), m_DarkMetal);
            // Housing
            var box = CreateCube(tl.transform, "Housing", new Vector3(0f, 4.4f, 0f), new Vector3(0.45f, 1.4f, 0.35f), m_DarkMetal);
            // Red, Yellow, Green Lenses
            CreateCylinder(box.transform, "RedLens", new Vector3(0f, 0.42f, 0.18f), new Vector3(0.28f, 0.08f, 0.28f), m_PlasticRed).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateCylinder(box.transform, "YellowLens", new Vector3(0f, 0f, 0.18f), new Vector3(0.28f, 0.08f, 0.28f), m_AwningYellow).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateCylinder(box.transform, "GreenLens", new Vector3(0f, -0.42f, 0.18f), new Vector3(0.28f, 0.08f, 0.28f), m_AwningGreen).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void CreateStreetSign(Transform parent, string name, Vector3 pos, float rotY)
        {
            var signGO = new GameObject(name);
            signGO.transform.SetParent(parent, false);
            signGO.transform.localPosition = pos;
            signGO.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            CreateCylinder(signGO.transform, "Post", new Vector3(0f, 1.6f, 0f), new Vector3(0.08f, 3.2f, 0.08f), m_DarkMetal);
            var plate = CreateCube(signGO.transform, "Plate", new Vector3(0.6f, 2.9f, 0f), new Vector3(1.3f, 0.38f, 0.04f), m_MatStreetName);
        }

        private static void CreateStreetTree(Transform parent, string name, Vector3 pos)
        {
            var tree = new GameObject(name);
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            // Square concrete planter curb
            CreateCube(tree.transform, "CurbGril", new Vector3(0f, 0.05f, 0f), new Vector3(1.8f, 0.12f, 1.8f), m_CurbStone);
            // Trunk
            CreateCylinder(tree.transform, "Trunk", new Vector3(0f, 2.0f, 0f), new Vector3(0.38f, 4.0f, 0.38f), m_TreeBark);
            // Canopy
            CreateSphere(tree.transform, "Canopy", new Vector3(0f, 4.5f, 0f), new Vector3(4.2f, 3.4f, 4.2f), m_Foliage);
        }

        #endregion

        #region 8. 3D World Location Pins

        private static void BuildStreetLocationPins(Transform root)
        {
            var pinsRoot = new GameObject("StreetLocationPins");
            pinsRoot.transform.SetParent(root, false);

            // 1. Quán Phở Bò IUH (Mini-game 1: Grab Campus)
            CreatePin(pinsRoot.transform, "Pin_Street_PhoBo", new Vector3(-13.5f, 0.5f, -95.5f), "QUÁN PHỞ SINH VIÊN", "Nhận đơn Grab Campus 2.1", "🍜", new Color(1.0f, 0.65f, 0.1f), 0, 4.2f);

            // 2. Highlands Coffee & The Coffee House (Mini-game 4: Thợ săn khoảnh khắc)
            CreatePin(pinsRoot.transform, "Pin_Street_Highlands", new Vector3(13.5f, 0.5f, -95.5f), "HIGHLANDS COFFEE", "Cà phê & Check-in góc phố", "☕", new Color(0.85f, 0.25f, 0.2f), 3, 4.5f);

            // 3. Bến Xe Buýt IUH (Check-in & Mini-game 3: AI Object Hunt)
            CreatePin(pinsRoot.transform, "Pin_Street_BusStop", new Vector3(13.5f, 0.5f, -138.0f), "BẾN XE BUÝT IUH", "Xe buýt xanh & Điểm check-in", "🚏", new Color(0.1f, 0.85f, 0.45f), 2, 4.2f);

            // 4. Bãi Giữ Xe Máy Sinh Viên
            CreatePin(pinsRoot.transform, "Pin_Street_Parking", new Vector3(25.0f, 0.5f, -152.0f), "BÃI XE NGOÀI CỔNG", "Bãi giữ xe máy sinh viên", "🛵", new Color(0.2f, 0.75f, 1.0f), -1, 4.5f);

            // 5. FamilyMart & Nhà Sách IUH
            CreatePin(pinsRoot.transform, "Pin_Street_FamilyMart", new Vector3(-13.5f, 0.5f, -141.0f), "CỬA HÀNG TIỆN LỢI", "FamilyMart 24/7 & Nhà Sách", "🏪", new Color(0.0f, 0.85f, 0.55f), 2, 4.5f);

            // 6. Công Viên Nhỏ Góc Phố (Mini-game 5: Green Campus)
            CreatePin(pinsRoot.transform, "Pin_Street_PocketPark", new Vector3(0f, 0.5f, -178.0f), "CÔNG VIÊN GÓC PHỐ", "Trạm phân loại rác & Nghỉ ngơi", "🌳", new Color(0.25f, 0.85f, 0.25f), 4, 4.6f);
        }

        private static void CreatePin(Transform parent, string name, Vector3 pos, string title, string sub, string emoji, Color col, int gameIdx, float height)
        {
            var pinGO = new GameObject(name);
            pinGO.transform.SetParent(parent, false);
            pinGO.transform.position = pos;

            var pin = pinGO.AddComponent<IUHWorldLocationPin>();
            pin.Setup(title, sub, emoji, col, gameIdx, height);
        }

        #endregion

        #region Helpers

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateSphere(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        #endregion
    }
}
