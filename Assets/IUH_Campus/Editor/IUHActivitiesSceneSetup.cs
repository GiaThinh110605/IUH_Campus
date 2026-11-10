using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.DailyActivities;
using IUHCampus.Guardian;

namespace IUHCampus.EditorTools
{
    public static class IUHActivitiesSceneSetup
    {
        [MenuItem("IUH Campus/Setup 5 Daily Campus Activities", false, 120)]
        public static void SetupDailyActivities()
        {
            Debug.Log("<color=#00E5FF>[IUH Daily Activities]</color> Bắt đầu cấu hình 5 hoạt động khuôn viên...");

            // 1. Manager Object
            GameObject mgrGo = GameObject.Find("IUH_ACTIVITIES_MANAGER");
            if (mgrGo == null)
            {
                mgrGo = new GameObject("IUH_ACTIVITIES_MANAGER");
            }
            var mgr = mgrGo.GetComponent<IUHCampusDailyActivitiesManager>();
            if (mgr == null) mgr = mgrGo.AddComponent<IUHCampusDailyActivitiesManager>();

            // 2. Materials
            Material goldCoinMat = CreateOrGetMaterial("M_DailyCoin_Gold", new Color(1f, 0.82f, 0.1f), 0.9f, 0.8f, new Color(0.8f, 0.6f, 0.1f) * 1.5f);
            Material giftBoxMat = CreateOrGetMaterial("M_MysteryGift_Box", new Color(0.85f, 0.15f, 0.35f), 0.3f, 0.7f, new Color(0.6f, 0.1f, 0.2f));
            Material giftRibbonMat = CreateOrGetMaterial("M_MysteryGift_Ribbon", new Color(0.1f, 0.95f, 0.85f), 0.8f, 0.8f, new Color(0.1f, 0.8f, 0.7f) * 2f);
            Material starMat = CreateOrGetMaterial("M_LuckyStar_Glow", new Color(1f, 0.92f, 0.2f), 0.9f, 0.9f, new Color(1f, 0.8f, 0.1f) * 3f);
            Material beamMat = CreateOrGetMaterial("M_LuckyStar_Beam", new Color(1f, 0.85f, 0.2f, 0.35f), 0f, 0f, new Color(1f, 0.8f, 0.2f) * 1.5f, transparent: true);
            Material questItemMat = CreateOrGetMaterial("M_MascotQuest_Chip", new Color(0.1f, 0.75f, 1f), 0.8f, 0.9f, new Color(0.1f, 0.7f, 1f) * 2.5f);

            // 3. Daily Coins (8 coins)
            SetupDailyCoins(goldCoinMat);

            // 4. Mystery Gift Box
            SetupMysteryGift(giftBoxMat, giftRibbonMat);

            // 5. Mascot NPC & 3 Quest Items
            SetupMascotNPC(questItemMat);

            // 6. Check-in Spots (Ensure all 5 spots exist)
            SetupCheckInSpots();

            // 7. Lucky Star
            SetupLuckyStar(starMat, beamMat);

            // 8. Activities HUD Canvas
            SetupActivitiesHUDCanvas();

            EditorUtility.SetDirty(mgrGo);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("<color=#00FF88>[IUH Daily Activities] Cấu hình hoàn tất thành công cả 5 hoạt động!</color>");
        }

        private static Material CreateOrGetMaterial(string name, Color baseColor, float metallic, float smoothness, Color emission, bool transparent = false)
        {
            string path = $"Assets/IUH_Campus/Materials/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(urpShader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = baseColor;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);

            if (transparent)
            {
                mat.SetFloat("_Surface", 1); // Transparent in URP
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void SetupDailyCoins(Material goldMat)
        {
            GameObject cluster = GameObject.Find("IUH_DAILY_COIN_CLUSTER");
            if (cluster != null) UnityEngine.Object.DestroyImmediate(cluster);
            cluster = new GameObject("IUH_DAILY_COIN_CLUSTER");

            Vector3[] coinPositions = new Vector3[]
            {
                new Vector3(2.5f, 0.85f, -30f),   // Near Gate
                new Vector3(-4f, 0.85f, -22f),    // Walkway to fountain
                new Vector3(0f, 0.85f, -1f),      // Fountain front
                new Vector3(-20f, 0.85f, -16f),   // Canteen entrance
                new Vector3(8.5f, 0.85f, -12f),   // Tech Kiosk
                new Vector3(14.5f, 0.85f, 5f),    // Festival Square
                new Vector3(-12f, 0.85f, 7f),     // Garden path
                new Vector3(0f, 1.4f, 8.5f)       // Behind Fountain pool
            };

            for (int i = 0; i < coinPositions.Length; i++)
            {
                GameObject coinGo = new GameObject($"DAILY_COIN_{i + 1:D2}");
                coinGo.transform.SetParent(cluster.transform);
                coinGo.transform.position = coinPositions[i];

                // Visual disc
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "Coin_Visual";
                visual.transform.SetParent(coinGo.transform, false);
                visual.transform.localScale = new Vector3(0.6f, 0.08f, 0.6f);
                visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // face forward
                var rend = visual.GetComponent<Renderer>();
                if (rend != null) rend.sharedMaterial = goldMat;

                // Sphere trigger
                var col = coinGo.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 1.6f;
                // remove cylinder collider
                var primCol = visual.GetComponent<Collider>();
                if (primCol != null) UnityEngine.Object.DestroyImmediate(primCol);

                // Warm Light
                GameObject lightGo = new GameObject("Coin_Light");
                lightGo.transform.SetParent(coinGo.transform, false);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.85f, 0.2f);
                light.intensity = 1.8f;
                light.range = 3.5f;

                var coinComp = coinGo.AddComponent<IUHDailyCoin>();
                coinComp.CoinIndex = i;

                // Wire serialized fields
                var so = new SerializedObject(coinComp);
                so.FindProperty("m_VisualTransform").objectReferenceValue = visual.transform;
                so.FindProperty("m_CoinLight").objectReferenceValue = light;
                so.ApplyModifiedProperties();
            }
        }

        private static void SetupMysteryGift(Material boxMat, Material ribbonMat)
        {
            GameObject giftGo = GameObject.Find("IUH_MYSTERY_GIFT");
            if (giftGo != null) UnityEngine.Object.DestroyImmediate(giftGo);
            giftGo = new GameObject("IUH_MYSTERY_GIFT");
            giftGo.transform.position = new Vector3(-16f, 0.45f, 8f); // In scenic garden

            // Box Body
            GameObject boxBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxBody.name = "Box_Body";
            boxBody.transform.SetParent(giftGo.transform, false);
            boxBody.transform.localScale = new Vector3(0.9f, 0.7f, 0.9f);
            boxBody.GetComponent<Renderer>().sharedMaterial = boxMat;
            var boxCol = boxBody.GetComponent<Collider>();
            if (boxCol != null) UnityEngine.Object.DestroyImmediate(boxCol);

            // Ribbon vertical & horizontal
            GameObject ribbon1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ribbon1.name = "Ribbon_X";
            ribbon1.transform.SetParent(boxBody.transform, false);
            ribbon1.transform.localScale = new Vector3(1.02f, 1.01f, 0.22f);
            ribbon1.GetComponent<Renderer>().sharedMaterial = ribbonMat;
            UnityEngine.Object.DestroyImmediate(ribbon1.GetComponent<Collider>());

            GameObject ribbon2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ribbon2.name = "Ribbon_Z";
            ribbon2.transform.SetParent(boxBody.transform, false);
            ribbon2.transform.localScale = new Vector3(0.22f, 1.01f, 1.02f);
            ribbon2.GetComponent<Renderer>().sharedMaterial = ribbonMat;
            UnityEngine.Object.DestroyImmediate(ribbon2.GetComponent<Collider>());

            // Box Lid (pivot for opening)
            GameObject lidPivot = new GameObject("Lid_Pivot");
            lidPivot.transform.SetParent(giftGo.transform, false);
            lidPivot.transform.localPosition = new Vector3(0f, 0.35f, -0.45f);

            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "Lid_Mesh";
            lid.transform.SetParent(lidPivot.transform, false);
            lid.transform.localPosition = new Vector3(0f, 0.08f, 0.45f);
            lid.transform.localScale = new Vector3(0.98f, 0.16f, 0.98f);
            lid.GetComponent<Renderer>().sharedMaterial = boxMat;
            UnityEngine.Object.DestroyImmediate(lid.GetComponent<Collider>());

            // Ribbon Bow on lid
            GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bow.name = "Ribbon_Bow";
            bow.transform.SetParent(lid.transform, false);
            bow.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            bow.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
            bow.GetComponent<Renderer>().sharedMaterial = ribbonMat;
            UnityEngine.Object.DestroyImmediate(bow.GetComponent<Collider>());

            // Trigger & Prompt
            var triggerCol = giftGo.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(3.5f, 2.5f, 3.5f);

            // Aura Light
            GameObject lightGo = new GameObject("Gift_Light");
            lightGo.transform.SetParent(giftGo.transform, false);
            lightGo.transform.localPosition = Vector3.up * 0.5f;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.3f, 0.7f);
            light.intensity = 2.2f;
            light.range = 5f;

            // Prompt Billboard
            GameObject prompt = CreateWorldBillboard("🎁 HỘP QUÀ BÍ ẨN\n<color=#FFFF00>[E] Mở Quà</color>", Vector3.up * 1.5f, giftGo.transform);

            var comp = giftGo.AddComponent<IUHMysteryGift>();
            var so = new SerializedObject(comp);
            so.FindProperty("m_LidTransform").objectReferenceValue = lidPivot.transform;
            so.FindProperty("m_PromptBillboard").objectReferenceValue = prompt;
            so.FindProperty("m_AuraLight").objectReferenceValue = light;
            so.ApplyModifiedProperties();
        }

        private static void SetupMascotNPC(Material questItemMat)
        {
            GameObject mascotGo = GameObject.Find("IUH_MASCOT_ROBOT");
            if (mascotGo != null) UnityEngine.Object.DestroyImmediate(mascotGo);
            mascotGo = new GameObject("IUH_MASCOT_ROBOT");
            mascotGo.transform.position = new Vector3(4.5f, 0.1f, -6f); // Prominent spot near fountain

            // Cute procedural mascot model
            GameObject model = new GameObject("Mascot_Visual");
            model.transform.SetParent(mascotGo.transform, false);

            // Body (Pill / Capsule)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(model.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            var bodyMat = CreateOrGetMaterial("M_Mascot_Body", new Color(0.1f, 0.45f, 0.95f), 0.6f, 0.7f, new Color(0.05f, 0.2f, 0.5f));
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            // Head (Sphere)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(model.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            head.transform.localScale = new Vector3(0.7f, 0.65f, 0.7f);
            var headMat = CreateOrGetMaterial("M_Mascot_White", new Color(0.95f, 0.95f, 0.98f), 0.3f, 0.8f, Color.black);
            head.GetComponent<Renderer>().sharedMaterial = headMat;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());

            // Glowing Eyes (Cyber Visor)
            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor_Eyes";
            visor.transform.SetParent(head.transform, false);
            visor.transform.localPosition = new Vector3(0f, 0.05f, 0.45f);
            visor.transform.localScale = new Vector3(0.55f, 0.18f, 0.15f);
            var visorMat = CreateOrGetMaterial("M_Mascot_Visor", new Color(0.1f, 1f, 0.85f), 0.9f, 0.9f, new Color(0.1f, 1f, 0.85f) * 3f);
            visor.GetComponent<Renderer>().sharedMaterial = visorMat;
            UnityEngine.Object.DestroyImmediate(visor.GetComponent<Collider>());

            // IUH Graduation Cap
            GameObject capBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            capBase.name = "GradCap_Base";
            capBase.transform.SetParent(head.transform, false);
            capBase.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            capBase.transform.localScale = new Vector3(0.4f, 0.08f, 0.4f);
            var capMat = CreateOrGetMaterial("M_Mascot_GradCap", new Color(0.08f, 0.12f, 0.25f), 0.2f, 0.5f, Color.black);
            capBase.GetComponent<Renderer>().sharedMaterial = capMat;
            UnityEngine.Object.DestroyImmediate(capBase.GetComponent<Collider>());

            GameObject capDiamond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            capDiamond.name = "GradCap_Diamond";
            capDiamond.transform.SetParent(head.transform, false);
            capDiamond.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            capDiamond.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            capDiamond.transform.localScale = new Vector3(0.7f, 0.03f, 0.7f);
            capDiamond.GetComponent<Renderer>().sharedMaterial = capMat;
            UnityEngine.Object.DestroyImmediate(capDiamond.GetComponent<Collider>());

            // Light Glow
            GameObject lightGo = new GameObject("Mascot_Light");
            lightGo.transform.SetParent(mascotGo.transform, false);
            lightGo.transform.localPosition = Vector3.up * 1.2f;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.2f, 0.8f, 1f);
            light.intensity = 2f;
            light.range = 5f;

            // Prompt Billboard
            GameObject prompt = CreateWorldBillboard("🤖 LINH VẬT IUH\n<color=#FFFF00>[E] Nhận Thử Thách</color>", Vector3.up * 2.2f, mascotGo.transform);

            // 3 Quest Items to find around campus
            Vector3[] itemPositions = new Vector3[]
            {
                new Vector3(-22f, 0.8f, -20f),  // Canteen plaza
                new Vector3(12f, 0.8f, -16f),   // Building E entrance
                new Vector3(-13f, 0.8f, 3f)     // Garden nook
            };

            GameObject questCluster = GameObject.Find("IUH_MASCOT_QUEST_ITEMS");
            if (questCluster != null) UnityEngine.Object.DestroyImmediate(questCluster);
            questCluster = new GameObject("IUH_MASCOT_QUEST_ITEMS");

            GameObject[] questItemGos = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject itemGo = new GameObject($"MASCOT_QUEST_ITEM_{i + 1}");
                itemGo.transform.SetParent(questCluster.transform);
                itemGo.transform.position = itemPositions[i];

                // Hologram chip cube
                GameObject chip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chip.name = "Chip_Mesh";
                chip.transform.SetParent(itemGo.transform, false);
                chip.transform.localScale = new Vector3(0.45f, 0.1f, 0.45f);
                chip.transform.localRotation = Quaternion.Euler(30f, 45f, 20f);
                chip.GetComponent<Renderer>().sharedMaterial = questItemMat;
                UnityEngine.Object.DestroyImmediate(chip.GetComponent<Collider>());

                // Beacon beam
                GameObject bLight = new GameObject("Beacon_Light");
                bLight.transform.SetParent(itemGo.transform, false);
                var l = bLight.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(0.2f, 0.85f, 1f);
                l.intensity = 2.5f;
                l.range = 4f;

                CreateWorldBillboard($"📦 VI MẠCH #{i + 1}\n<color=#00E5FF>Nhiệm Vụ Linh Vật</color>", Vector3.up * 1.2f, itemGo.transform);

                var qComp = itemGo.AddComponent<IUHMascotQuestItem>();
                qComp.ItemIndex = i;

                questItemGos[i] = itemGo;
            }

            var mascotComp = mascotGo.AddComponent<IUHMascotNPC>();
            var so = new SerializedObject(mascotComp);
            so.FindProperty("m_ModelTransform").objectReferenceValue = model.transform;
            so.FindProperty("m_PromptBillboard").objectReferenceValue = prompt;
            so.FindProperty("m_MascotGlow").objectReferenceValue = light;
            so.ApplyModifiedProperties();

            mascotComp.SetQuestItemInstances(questItemGos);
        }

        private static void SetupCheckInSpots()
        {
            // Ensure 5 check-in spots exist
            EnsureCheckInSpot("CHECKIN_SPOT_FOUNTAIN", "Đài Phun Nước Trung Tâm", "Huy Hiệu: Giọt Nước Tri Thức", new Vector3(0f, 0.05f, -4f));
            EnsureCheckInSpot("CHECKIN_SPOT_GATE", "Cổng Chào IUH Nguyễn Văn Bảo", "Huy Hiệu: Khởi Đầu Tương Lai", new Vector3(0f, 0.05f, -32f));
            EnsureCheckInSpot("CHECKIN_SPOT_FESTIVAL", "Quảng Trường Lễ Hội & Âm Nhạc", "Huy Hiệu: Trái Tim Sôi Động", new Vector3(16f, 0.05f, 3f));
            EnsureCheckInSpot("CHECKIN_SPOT_BUILDING_E", "Tòa Nhà Kỹ Thuật E", "Huy Hiệu: Kỹ Sư Công Nghệ", new Vector3(18f, 0.05f, -15f));
            EnsureCheckInSpot("CHECKIN_SPOT_GARDEN", "Vườn Cây Tri Thức IUH", "Huy Hiệu: Khoảng Lặng Bình Yên", new Vector3(-15f, 0.05f, 6f));
        }

        private static void EnsureCheckInSpot(string spotId, string spotName, string badgeName, Vector3 pos)
        {
            GameObject spotGo = GameObject.Find(spotId);
            if (spotGo == null)
            {
                spotGo = new GameObject(spotId);
                spotGo.transform.position = pos;

                // Cyan hologram disc on ground
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "Holo_Ring";
                ring.transform.SetParent(spotGo.transform, false);
                ring.transform.localScale = new Vector3(3.5f, 0.02f, 3.5f);
                var ringMat = CreateOrGetMaterial("M_CheckIn_HoloRing", new Color(0f, 0.9f, 1f, 0.4f), 0f, 0f, new Color(0f, 0.8f, 1f) * 2f, transparent: true);
                ring.GetComponent<Renderer>().sharedMaterial = ringMat;
                UnityEngine.Object.DestroyImmediate(ring.GetComponent<Collider>());

                // Billboard
                CreateWorldBillboard($"📸 {spotName}\n<color=#FFFF00>[E] Check-in Điểm Danh</color>", Vector3.up * 1.8f, spotGo.transform);
            }

            var comp = spotGo.GetComponent<IUHCheckInSpot>();
            if (comp == null) comp = spotGo.AddComponent<IUHCheckInSpot>();

            var so = new SerializedObject(comp);
            so.FindProperty("m_SpotId").stringValue = spotId;
            so.FindProperty("m_SpotName").stringValue = spotName;
            so.FindProperty("m_SouvenirBadge").stringValue = badgeName;
            so.ApplyModifiedProperties();
        }

        private static void SetupLuckyStar(Material starMat, Material beamMat)
        {
            GameObject starGo = GameObject.Find("IUH_LUCKY_STAR");
            if (starGo != null) UnityEngine.Object.DestroyImmediate(starGo);
            starGo = new GameObject("IUH_LUCKY_STAR");
            starGo.transform.position = new Vector3(0f, 2.8f, 12f); // Elevated behind fountain monument

            // 3D Star: Composed of 2 interlocked tetrahedrons or 5-cone star
            GameObject starVisual = new GameObject("Star_Visual");
            starVisual.transform.SetParent(starGo.transform, false);

            // Center core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.SetParent(starVisual.transform, false);
            core.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            core.GetComponent<Renderer>().sharedMaterial = starMat;
            UnityEngine.Object.DestroyImmediate(core.GetComponent<Collider>());

            // 5 Pointed cones/pyramids
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f;
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                point.name = $"StarPoint_{i}";
                point.transform.SetParent(starVisual.transform, false);
                point.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                point.transform.localPosition = point.transform.up * 0.45f;
                point.transform.localScale = new Vector3(0.18f, 0.35f, 0.18f);
                point.GetComponent<Renderer>().sharedMaterial = starMat;
                UnityEngine.Object.DestroyImmediate(point.GetComponent<Collider>());
            }

            // Beacon ray beam reaching into sky (15m high)
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "Sky_Beacon_Beam";
            beam.transform.SetParent(starGo.transform, false);
            beam.transform.localPosition = Vector3.up * 7.5f;
            beam.transform.localScale = new Vector3(0.3f, 7.5f, 0.3f);
            beam.GetComponent<Renderer>().sharedMaterial = beamMat;
            UnityEngine.Object.DestroyImmediate(beam.GetComponent<Collider>());

            // Radiant Star Light
            GameObject lightGo = new GameObject("Star_Light");
            lightGo.transform.SetParent(starGo.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.9f, 0.3f);
            light.intensity = 3.5f;
            light.range = 8f;

            // Trigger
            var col = starGo.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.8f;

            CreateWorldBillboard("⭐ NGÔI SAO MAY MẮN\n<color=#FFFF00>Khám Phá Nhận Quà</color>", Vector3.up * 1.5f, starGo.transform);

            var comp = starGo.AddComponent<IUHLuckyStar>();
            var so = new SerializedObject(comp);
            so.FindProperty("m_StarTransform").objectReferenceValue = starVisual.transform;
            so.FindProperty("m_StarLight").objectReferenceValue = light;
            so.FindProperty("m_BeaconBeam").objectReferenceValue = beam;
            so.ApplyModifiedProperties();
        }

        private static GameObject CreateWorldBillboard(string text, Vector3 localOffset, Transform parent)
        {
            GameObject canvasGo = new GameObject("Billboard_Prompt", typeof(RectTransform), typeof(Canvas));
            canvasGo.transform.SetParent(parent, false);
            canvasGo.transform.localPosition = localOffset;

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240f, 90f);
            rt.localScale = Vector3.one * 0.012f;

            // Text
            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(canvasGo.transform, false);
            var txt = txtGo.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.text = text;
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            // Outline
            var ol = txtGo.AddComponent<Outline>();
            ol.effectColor = Color.black;
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            // Add auto-face camera component
            canvasGo.AddComponent<BillboardFaceCamera>();

            return canvasGo;
        }

        private static void SetupActivitiesHUDCanvas()
        {
            GameObject hudGo = GameObject.Find("IUH_CAMPUS_ACTIVITIES_CANVAS");
            if (hudGo != null) UnityEngine.Object.DestroyImmediate(hudGo);
            hudGo = new GameObject("IUH_CAMPUS_ACTIVITIES_CANVAS");

            var canvas = hudGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 180;

            var scaler = hudGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            hudGo.AddComponent<GraphicRaycaster>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Top-Right Activities Panel
            GameObject panelGo = new GameObject("Activities_Widget_Panel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(hudGo.transform, false);
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.color = new Color(0.08f, 0.12f, 0.18f, 0.88f);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1f, 1f);
            panelRt.anchorMax = new Vector2(1f, 1f);
            panelRt.pivot = new Vector2(1f, 1f);
            panelRt.anchoredPosition = new Vector2(-20f, -60f);
            panelRt.sizeDelta = new Vector2(330f, 240f);

            var panelOutline = panelGo.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0f, 0.8f, 1f, 0.6f);
            panelOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleTxt = titleGo.GetComponent<Text>();
            titleTxt.font = defaultFont;
            titleTxt.text = "📅 SỰ KIỆN KHUÔN VIÊN HÔM NAY";
            titleTxt.fontSize = 15;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(0.3f, 0.95f, 1f);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -8f);
            titleRt.sizeDelta = new Vector2(-10f, 26f);

            // 5 Status Text Lines
            Text coinTxt = CreateStatusLine(panelGo.transform, "CoinStatus", "🪙 Săn Xu: 0/8", -38f, defaultFont);
            Text giftTxt = CreateStatusLine(panelGo.transform, "GiftStatus", "🎁 Hộp Quà: Đang giấu", -68f, defaultFont);
            Text mascotTxt = CreateStatusLine(panelGo.transform, "MascotStatus", "🤖 Linh Vật: Có thử thách", -98f, defaultFont);
            Text checkinTxt = CreateStatusLine(panelGo.transform, "CheckinStatus", "📸 Check-in: 0/5", -128f, defaultFont);
            Text starTxt = CreateStatusLine(panelGo.transform, "StarStatus", "⭐ Sao May Mắn: Đang ẩn", -158f, defaultFont);

            // Open Album Button
            GameObject albumBtnGo = new GameObject("Btn_OpenAlbum", typeof(RectTransform), typeof(Image), typeof(Button));
            albumBtnGo.transform.SetParent(panelGo.transform, false);
            var albumBtnImg = albumBtnGo.GetComponent<Image>();
            albumBtnImg.color = new Color(0.1f, 0.5f, 0.8f, 0.9f);
            var albumBtnRt = albumBtnGo.GetComponent<RectTransform>();
            albumBtnRt.anchorMin = new Vector2(0.5f, 0f);
            albumBtnRt.anchorMax = new Vector2(0.5f, 0f);
            albumBtnRt.pivot = new Vector2(0.5f, 0f);
            albumBtnRt.anchoredPosition = new Vector2(0f, 10f);
            albumBtnRt.sizeDelta = new Vector2(290f, 34f);

            GameObject albumBtnTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            albumBtnTxtGo.transform.SetParent(albumBtnGo.transform, false);
            var abTxt = albumBtnTxtGo.GetComponent<Text>();
            abTxt.font = defaultFont;
            abTxt.text = "📖 MỞ ALBUM KỶ NIỆM IUH";
            abTxt.fontSize = 13;
            abTxt.fontStyle = FontStyle.Bold;
            abTxt.alignment = TextAnchor.MiddleCenter;
            abTxt.color = Color.white;
            var abTxtRt = albumBtnTxtGo.GetComponent<RectTransform>();
            abTxtRt.anchorMin = Vector2.zero;
            abTxtRt.anchorMax = Vector2.one;
            abTxtRt.sizeDelta = Vector2.zero;

            // 2. Memorial Album Modal Window
            GameObject albumModal = SetupAlbumModalUI(hudGo.transform, defaultFont);

            // 3. Mascot Dialogue Window
            GameObject mascotDialog = SetupMascotDialogUI(hudGo.transform, defaultFont, out Text mTitle, out Text mMsg, out Button mActBtn, out Text mActTxt, out Button mCloseBtn);

            // 4. Shutter Flash Overlay (White fullscreen image, blocksRaycasts = false)
            GameObject flashGo = new GameObject("Camera_Shutter_Flash", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            flashGo.transform.SetParent(hudGo.transform, false);
            var flashImg = flashGo.GetComponent<Image>();
            flashImg.color = Color.white;
            flashImg.raycastTarget = false;
            var flashCg = flashGo.GetComponent<CanvasGroup>();
            flashCg.alpha = 0f;
            flashCg.blocksRaycasts = false;
            var flashRt = flashGo.GetComponent<RectTransform>();
            flashRt.anchorMin = Vector2.zero;
            flashRt.anchorMax = Vector2.one;
            flashRt.sizeDelta = Vector2.zero;

            // 5. Celebration Banner Toast
            GameObject bannerToast = SetupCelebrationBanner(hudGo.transform, defaultFont, out Text bTitle, out Text bMsg);

            // Hook up to HUD component
            var hudComp = hudGo.AddComponent<IUHCampusActivitiesHUD>();
            var so = new SerializedObject(hudComp);
            so.FindProperty("m_WidgetPanel").objectReferenceValue = panelGo;
            so.FindProperty("m_CoinStatusText").objectReferenceValue = coinTxt;
            so.FindProperty("m_MysteryGiftText").objectReferenceValue = giftTxt;
            so.FindProperty("m_MascotStatusText").objectReferenceValue = mascotTxt;
            so.FindProperty("m_CheckInStatusText").objectReferenceValue = checkinTxt;
            so.FindProperty("m_LuckyStarText").objectReferenceValue = starTxt;
            so.FindProperty("m_OpenAlbumButton").objectReferenceValue = albumBtnGo.GetComponent<Button>();

            so.FindProperty("m_MascotDialogRoot").objectReferenceValue = mascotDialog;
            so.FindProperty("m_MascotDialogTitle").objectReferenceValue = mTitle;
            so.FindProperty("m_MascotDialogMessage").objectReferenceValue = mMsg;
            so.FindProperty("m_MascotActionButton").objectReferenceValue = mActBtn;
            so.FindProperty("m_MascotActionText").objectReferenceValue = mActTxt;
            so.FindProperty("m_MascotCloseButton").objectReferenceValue = mCloseBtn;

            so.FindProperty("m_AlbumModalRoot").objectReferenceValue = albumModal;
            so.FindProperty("m_CloseAlbumButton").objectReferenceValue = albumModal.transform.Find("Btn_CloseAlbum")?.GetComponent<Button>();
            so.FindProperty("m_AlbumCardsContainer").objectReferenceValue = albumModal.transform.Find("Cards_Container");

            so.FindProperty("m_FlashCanvasGroup").objectReferenceValue = flashCg;
            so.FindProperty("m_BannerToastRoot").objectReferenceValue = bannerToast;
            so.FindProperty("m_BannerTitle").objectReferenceValue = bTitle;
            so.FindProperty("m_BannerMessage").objectReferenceValue = bMsg;

            so.ApplyModifiedProperties();
        }

        private static Text CreateStatusLine(Transform parent, string name, string text, float yPos, Font font)
        {
            GameObject lineGo = new GameObject(name, typeof(RectTransform), typeof(Text));
            lineGo.transform.SetParent(parent, false);
            var txt = lineGo.GetComponent<Text>();
            txt.font = font;
            txt.text = text;
            txt.fontSize = 13;
            txt.color = Color.white;
            txt.supportRichText = true;
            var rt = lineGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(16f, yPos);
            rt.sizeDelta = new Vector2(-32f, 24f);
            return txt;
        }

        private static GameObject SetupAlbumModalUI(Transform parent, Font font)
        {
            GameObject modal = new GameObject("Album_Modal_Window", typeof(RectTransform), typeof(Image));
            modal.transform.SetParent(parent, false);
            var img = modal.GetComponent<Image>();
            img.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);
            var rt = modal.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1360f, 760f);

            var outline = modal.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.85f, 1f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Modal Header
            GameObject headerGo = new GameObject("Header_Title", typeof(RectTransform), typeof(Text));
            headerGo.transform.SetParent(modal.transform, false);
            var hTxt = headerGo.GetComponent<Text>();
            hTxt.font = font;
            hTxt.text = "📖 ALBUM KỶ NIỆM KHUÔN VIÊN IUH";
            hTxt.fontSize = 24;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.88f, 0.3f);
            var hRt = headerGo.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.anchoredPosition = new Vector2(0f, -20f);
            hRt.sizeDelta = new Vector2(0f, 40f);

            // Subtitle
            GameObject subGo = new GameObject("Header_Subtitle", typeof(RectTransform), typeof(Text));
            subGo.transform.SetParent(modal.transform, false);
            var sTxt = subGo.GetComponent<Text>();
            sTxt.font = font;
            sTxt.text = "Khám phá các địa danh nổi bật, chụp ảnh lưu giữ kỷ niệm và thu thập huy hiệu sinh viên!";
            sTxt.fontSize = 14;
            sTxt.alignment = TextAnchor.MiddleCenter;
            sTxt.color = new Color(0.8f, 0.9f, 1f, 0.85f);
            var sRt = subGo.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0f, 1f);
            sRt.anchorMax = new Vector2(1f, 1f);
            sRt.pivot = new Vector2(0.5f, 1f);
            sRt.anchoredPosition = new Vector2(0f, -60f);
            sRt.sizeDelta = new Vector2(0f, 24f);

            // Cards Horizontal Layout Container
            GameObject cardsContainer = new GameObject("Cards_Container", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardsContainer.transform.SetParent(modal.transform, false);
            var hlg = cardsContainer.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            var cRt = cardsContainer.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.anchoredPosition = new Vector2(0f, -20f);
            cRt.sizeDelta = new Vector2(-40f, -160f);

            // Close Button
            GameObject closeBtn = new GameObject("Btn_CloseAlbum", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtn.transform.SetParent(modal.transform, false);
            var cImg = closeBtn.GetComponent<Image>();
            cImg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
            var cBtnRt = closeBtn.GetComponent<RectTransform>();
            cBtnRt.anchorMin = new Vector2(0.5f, 0f);
            cBtnRt.anchorMax = new Vector2(0.5f, 0f);
            cBtnRt.anchoredPosition = new Vector2(0f, 25f);
            cBtnRt.sizeDelta = new Vector2(180f, 44f);

            GameObject closeTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            closeTxtGo.transform.SetParent(closeBtn.transform, false);
            var cTxt = closeTxtGo.GetComponent<Text>();
            cTxt.font = font;
            cTxt.text = "Đóng Album [ESC]";
            cTxt.fontSize = 16;
            cTxt.fontStyle = FontStyle.Bold;
            cTxt.alignment = TextAnchor.MiddleCenter;
            cTxt.color = Color.white;
            var ctRt = closeTxtGo.GetComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.sizeDelta = Vector2.zero;

            modal.SetActive(false);
            return modal;
        }

        private static GameObject SetupMascotDialogUI(Transform parent, Font font, out Text title, out Text msg, out Button actBtn, out Text actTxt, out Button closeBtn)
        {
            GameObject dialog = new GameObject("Mascot_Dialogue_Window", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(parent, false);
            var img = dialog.GetComponent<Image>();
            img.color = new Color(0.08f, 0.12f, 0.2f, 0.97f);
            var rt = dialog.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(650f, 380f);

            var outline = dialog.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.8f, 1f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(dialog.transform, false);
            title = titleGo.GetComponent<Text>();
            title.font = font;
            title.text = "🤖 LINH VẬT IUH";
            title.fontSize = 20;
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(0.2f, 0.95f, 1f);
            var tRt = titleGo.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.anchoredPosition = new Vector2(24f, -20f);
            tRt.sizeDelta = new Vector2(-48f, 32f);

            // Message
            GameObject msgGo = new GameObject("Message", typeof(RectTransform), typeof(Text));
            msgGo.transform.SetParent(dialog.transform, false);
            msg = msgGo.GetComponent<Text>();
            msg.font = font;
            msg.text = "Nội dung thử thách của linh vật...";
            msg.fontSize = 15;
            msg.color = new Color(0.9f, 0.95f, 1f);
            msg.lineSpacing = 1.25f;
            var mRt = msgGo.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0f, 0f);
            mRt.anchorMax = new Vector2(1f, 1f);
            mRt.anchoredPosition = new Vector2(0f, 10f);
            mRt.sizeDelta = new Vector2(-48f, -140f);

            // Action Button
            GameObject actBtnGo = new GameObject("Btn_Action", typeof(RectTransform), typeof(Image), typeof(Button));
            actBtnGo.transform.SetParent(dialog.transform, false);
            var actImg = actBtnGo.GetComponent<Image>();
            actImg.color = new Color(0.1f, 0.75f, 0.35f, 0.95f);
            actBtn = actBtnGo.GetComponent<Button>();
            var abRt = actBtnGo.GetComponent<RectTransform>();
            abRt.anchorMin = new Vector2(1f, 0f);
            abRt.anchorMax = new Vector2(1f, 0f);
            abRt.pivot = new Vector2(1f, 0f);
            abRt.anchoredPosition = new Vector2(-24f, 20f);
            abRt.sizeDelta = new Vector2(220f, 44f);

            GameObject abTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            abTxtGo.transform.SetParent(actBtnGo.transform, false);
            actTxt = abTxtGo.GetComponent<Text>();
            actTxt.font = font;
            actTxt.text = "Nhận Thử Thách";
            actTxt.fontSize = 15;
            actTxt.fontStyle = FontStyle.Bold;
            actTxt.alignment = TextAnchor.MiddleCenter;
            actTxt.color = Color.white;
            var atRt = abTxtGo.GetComponent<RectTransform>();
            atRt.anchorMin = Vector2.zero;
            atRt.anchorMax = Vector2.one;
            atRt.sizeDelta = Vector2.zero;

            // Close Button
            GameObject closeBtnGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(dialog.transform, false);
            var cImg = closeBtnGo.GetComponent<Image>();
            cImg.color = new Color(0.3f, 0.35f, 0.45f, 0.9f);
            closeBtn = closeBtnGo.GetComponent<Button>();
            var cbRt = closeBtnGo.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(0f, 0f);
            cbRt.anchorMax = new Vector2(0f, 0f);
            cbRt.pivot = new Vector2(0f, 0f);
            cbRt.anchoredPosition = new Vector2(24f, 20f);
            cbRt.sizeDelta = new Vector2(130f, 44f);

            GameObject cbTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            cbTxtGo.transform.SetParent(closeBtnGo.transform, false);
            var cbTxt = cbTxtGo.GetComponent<Text>();
            cbTxt.font = font;
            cbTxt.text = "Đóng [ESC]";
            cbTxt.fontSize = 15;
            cbTxt.alignment = TextAnchor.MiddleCenter;
            cbTxt.color = Color.white;
            var cbtRt = cbTxtGo.GetComponent<RectTransform>();
            cbtRt.anchorMin = Vector2.zero;
            cbtRt.anchorMax = Vector2.one;
            cbtRt.sizeDelta = Vector2.zero;

            dialog.SetActive(false);
            return dialog;
        }

        private static GameObject SetupCelebrationBanner(Transform parent, Font font, out Text title, out Text msg)
        {
            GameObject banner = new GameObject("Celebration_Banner_Toast", typeof(RectTransform), typeof(Image));
            banner.transform.SetParent(parent, false);
            var img = banner.GetComponent<Image>();
            img.color = new Color(0.08f, 0.12f, 0.22f, 0.95f);
            var rt = banner.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -25f);
            rt.sizeDelta = new Vector2(620f, 95f);

            var outline = banner.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.2f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(banner.transform, false);
            title = titleGo.GetComponent<Text>();
            title.font = font;
            title.text = "🎉 CHÚC MỪNG HOÀN THÀNH!";
            title.fontSize = 16;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(1f, 0.9f, 0.2f);
            var tRt = titleGo.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.anchoredPosition = new Vector2(0f, -8f);
            tRt.sizeDelta = new Vector2(0f, 26f);

            // Message
            GameObject msgGo = new GameObject("Message", typeof(RectTransform), typeof(Text));
            msgGo.transform.SetParent(banner.transform, false);
            msg = msgGo.GetComponent<Text>();
            msg.font = font;
            msg.text = "Thông điệp phần thưởng...";
            msg.fontSize = 13;
            msg.alignment = TextAnchor.MiddleCenter;
            msg.color = Color.white;
            var mRt = msgGo.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0f, 0f);
            mRt.anchorMax = new Vector2(1f, 1f);
            mRt.anchoredPosition = new Vector2(0f, -12f);
            mRt.sizeDelta = new Vector2(-20f, -36f);

            banner.SetActive(false);
            return banner;
        }
    }

    public class BillboardFaceCamera : MonoBehaviour
    {
        private Camera m_Cam;

        private void Start()
        {
            m_Cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (m_Cam == null) m_Cam = Camera.main;
            if (m_Cam != null)
            {
                transform.rotation = m_Cam.transform.rotation;
            }
        }
    }
}

