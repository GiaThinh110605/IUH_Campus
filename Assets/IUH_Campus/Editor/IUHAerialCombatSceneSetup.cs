using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Combat;

namespace IUHCampus.EditorTools
{
    public static class IUHAerialCombatSceneSetup
    {
        [MenuItem("IUH Campus/Setup Hot Air Balloon & Flight System", false, 150)]
        public static void SetupAerialCombatSystem()
        {
            Debug.Log("<color=#00E5FF>[IUH Aerial Combat]</color> Bắt đầu cài đặt Khinh Khí Cầu & Cánh Bay Phản Lực...");

            SetupPlayerWings();
            SetupHotAirBalloon();
            SetupFireworksManager();
            AdjustAerialMonstersForSkyCombat();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=#00FF88>[IUH Aerial Combat] CÀI ĐẶT THÀNH CÔNG! Bấm Play và nhấn [F] để bay Khinh Khí Cầu hoặc nhấn [J]/[Space x 2] để bung cánh bay!</color>");
        }

        private static void SetupPlayerWings()
        {
            var student = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
            if (student == null)
            {
                Debug.LogWarning("[IUH Aerial Combat] Không tìm thấy 'Campus Student'!");
                return;
            }

            // Gắn component IUHAerialFlightController
            var flightCtrl = student.GetComponent<IUHAerialFlightController>() ?? student.AddComponent<IUHAerialFlightController>();

            // Tìm hoặc tạo Wings Container
            var existingWings = student.transform.Find("IUH_CYBER_AERO_WINGS");
            if (existingWings != null) UnityEngine.Object.DestroyImmediate(existingWings.gameObject);

            GameObject wingsRoot = new GameObject("IUH_CYBER_AERO_WINGS");
            wingsRoot.transform.SetParent(student.transform, false);
            wingsRoot.transform.localPosition = new Vector3(0f, 1.25f, -0.22f); // Gắn sau lưng/balo
            wingsRoot.transform.localRotation = Quaternion.identity;

            // Vật liệu phát quang Neon Cyan
            Material neonMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_CyberRifle_Neon.mat");
            if (neonMat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                neonMat = new Material(shader);
                neonMat.color = new Color(0f, 0.95f, 1f, 1f);
            }

            Material darkFrameMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Sign_Dark.mat");

            // Cánh trái
            GameObject leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWing.name = "Wing_Left";
            leftWing.transform.SetParent(wingsRoot.transform, false);
            leftWing.transform.localPosition = new Vector3(-0.65f, 0.35f, 0.05f);
            leftWing.transform.localRotation = Quaternion.Euler(0f, 25f, 35f);
            leftWing.transform.localScale = new Vector3(0.95f, 0.18f, 0.04f);
            if (leftWing.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(leftWing.GetComponent<Collider>());
            leftWing.GetComponent<Renderer>().sharedMaterial = neonMat;

            // Cánh phụ trái
            GameObject leftSubWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftSubWing.name = "SubWing_Left";
            leftSubWing.transform.SetParent(wingsRoot.transform, false);
            leftSubWing.transform.localPosition = new Vector3(-0.55f, 0.08f, 0.02f);
            leftSubWing.transform.localRotation = Quaternion.Euler(0f, 20f, 50f);
            leftSubWing.transform.localScale = new Vector3(0.65f, 0.14f, 0.03f);
            if (leftSubWing.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(leftSubWing.GetComponent<Collider>());
            leftSubWing.GetComponent<Renderer>().sharedMaterial = neonMat;

            // Cánh phải
            GameObject rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWing.name = "Wing_Right";
            rightWing.transform.SetParent(wingsRoot.transform, false);
            rightWing.transform.localPosition = new Vector3(0.65f, 0.35f, 0.05f);
            rightWing.transform.localRotation = Quaternion.Euler(0f, -25f, -35f);
            rightWing.transform.localScale = new Vector3(0.95f, 0.18f, 0.04f);
            if (rightWing.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(rightWing.GetComponent<Collider>());
            rightWing.GetComponent<Renderer>().sharedMaterial = neonMat;

            // Cánh phụ phải
            GameObject rightSubWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightSubWing.name = "SubWing_Right";
            rightSubWing.transform.SetParent(wingsRoot.transform, false);
            rightSubWing.transform.localPosition = new Vector3(0.55f, 0.08f, 0.02f);
            rightSubWing.transform.localRotation = Quaternion.Euler(0f, -20f, -50f);
            rightSubWing.transform.localScale = new Vector3(0.65f, 0.14f, 0.03f);
            if (rightSubWing.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(rightSubWing.GetComponent<Collider>());
            rightSubWing.GetComponent<Renderer>().sharedMaterial = neonMat;

            // Động cơ phản lực (Jet Thruster Nozzles)
            GameObject thrusterMount = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            thrusterMount.name = "Thruster_Center";
            thrusterMount.transform.SetParent(wingsRoot.transform, false);
            thrusterMount.transform.localPosition = new Vector3(0f, 0.15f, -0.04f);
            thrusterMount.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            thrusterMount.transform.localScale = new Vector3(0.18f, 0.12f, 0.18f);
            if (thrusterMount.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(thrusterMount.GetComponent<Collider>());
            if (darkFrameMat != null) thrusterMount.GetComponent<Renderer>().sharedMaterial = darkFrameMat;

            // Hạt phản lực trái / phải
            var leftPS = CreateThrusterParticles(wingsRoot.transform, new Vector3(-0.25f, -0.05f, 0f));
            var rightPS = CreateThrusterParticles(wingsRoot.transform, new Vector3(0.25f, -0.05f, 0f));

            flightCtrl.SetupWingsReferences(wingsRoot, leftPS, rightPS);
            Debug.Log("[IUH Aerial Combat] Đã trang bị Cánh Bay Phản Lực Cyber cho Campus Student!");
        }

        private static ParticleSystem CreateThrusterParticles(Transform parent, Vector3 localPos)
        {
            GameObject psGo = new GameObject("Thruster_Particle");
            psGo.transform.SetParent(parent, false);
            psGo.transform.localPosition = localPos;
            psGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.22f;
            main.startSpeed = 5.0f;
            main.startSize = 0.18f;
            main.startColor = new Color(0f, 0.9f, 1f, 0.85f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.08f;

            var psr = psGo.GetComponent<ParticleSystemRenderer>();
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/CombatArena/ArenaGlowCyan.mat");
            if (mat != null) psr.sharedMaterial = mat;

            ps.Stop();
            return ps;
        }

        private static void SetupHotAirBalloon()
        {
            var existing = GameObject.Find("IUH_HOT_AIR_BALLOON");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            Vector3 spawnPos = new Vector3(6.0f, 0.2f, -10.0f); // Courtyard center
            GameObject balloonRoot = new GameObject("IUH_HOT_AIR_BALLOON");
            balloonRoot.transform.position = spawnPos;

            var balloonCtrl = balloonRoot.AddComponent<IUHHotAirBalloonController>();

            // Materials
            Material blueMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Student_BluePolo.mat");
            Material redMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Cloth_Red.mat");
            Material goldMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_DailyCoin_Gold.mat");
            Material darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Sign_Dark.mat");
            Material cyanGlowMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_CyberRifle_Neon.mat");
            Material shieldMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/CombatArena/ArenaGlowCyan.mat");

            // 1. Combat Basket (Giỏ chiến đấu)
            GameObject basket = new GameObject("Combat_Basket");
            basket.transform.SetParent(balloonRoot.transform, false);
            basket.transform.localPosition = Vector3.zero;

            // Sàn giỏ
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "Basket_Floor";
            floor.transform.SetParent(basket.transform, false);
            floor.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            floor.transform.localScale = new Vector3(3.6f, 0.15f, 3.6f);
            if (darkMat != null) floor.GetComponent<Renderer>().sharedMaterial = darkMat;

            // Thay thế collider của sàn bằng BoxCollider phẳng để CharacterController đứng vững tuyệt đối
            var origFloorCol = floor.GetComponent<Collider>();
            if (origFloorCol != null) UnityEngine.Object.DestroyImmediate(origFloorCol);
            var floorBox = floor.AddComponent<BoxCollider>();
            floorBox.size = new Vector3(3.8f, 0.4f, 3.8f);
            floorBox.center = new Vector3(0f, -0.1f, 0f);

            // Thành giỏ (Railing Octagon / Cylindrical ring)
            GameObject railing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            railing.name = "Basket_Railing";
            railing.transform.SetParent(basket.transform, false);
            railing.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            railing.transform.localScale = new Vector3(3.7f, 0.55f, 3.7f);
            if (railing.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(railing.GetComponent<Collider>());
            if (blueMat != null) railing.GetComponent<Renderer>().sharedMaterial = blueMat;

            // 4 bức tường an toàn vật lý (Safety Invisible Walls) ngăn chặn mọi trường hợp rơi khỏi giỏ
            CreateSafetyWall(basket.transform, "Safety_Wall_North", new Vector3(0f, 1.0f, 1.8f), new Vector3(3.8f, 2.2f, 0.3f));
            CreateSafetyWall(basket.transform, "Safety_Wall_South", new Vector3(0f, 1.0f, -1.8f), new Vector3(3.8f, 2.2f, 0.3f));
            CreateSafetyWall(basket.transform, "Safety_Wall_East",  new Vector3(1.8f, 1.0f, 0f),  new Vector3(0.3f, 2.2f, 3.8f));
            CreateSafetyWall(basket.transform, "Safety_Wall_West",  new Vector3(-1.8f, 1.0f, 0f), new Vector3(0.3f, 2.2f, 3.8f));

            // Vành neon bọc giỏ
            GameObject rimNeon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rimNeon.name = "Basket_NeonRim";
            rimNeon.transform.SetParent(basket.transform, false);
            rimNeon.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            rimNeon.transform.localScale = new Vector3(3.8f, 0.08f, 3.8f);
            if (rimNeon.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(rimNeon.GetComponent<Collider>());
            if (cyanGlowMat != null) rimNeon.GetComponent<Renderer>().sharedMaterial = cyanGlowMat;

            // Player Anchor Point
            GameObject playerAnchor = new GameObject("PlayerAnchorPoint");
            playerAnchor.transform.SetParent(basket.transform, false);
            playerAnchor.transform.localPosition = new Vector3(0f, 0.25f, 0f);

            // 2. Burner Unit (Đầu đốt năng lượng)
            GameObject burner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            burner.name = "Burner_Nozzle";
            burner.transform.SetParent(balloonRoot.transform, false);
            burner.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            burner.transform.localScale = new Vector3(0.9f, 0.35f, 0.9f);
            if (darkMat != null) burner.GetComponent<Renderer>().sharedMaterial = darkMat;

            // Burner Light
            var burnerLightGo = new GameObject("Burner_Light");
            burnerLightGo.transform.SetParent(burner.transform, false);
            var bLight = burnerLightGo.AddComponent<Light>();
            bLight.type = LightType.Point;
            bLight.color = new Color(1f, 0.6f, 0.1f);
            bLight.range = 14f;
            bLight.intensity = 3.5f;

            // Burner Flame Particles
            var flamePS = CreateBurnerFlame(burner.transform);

            // 3. Balloon Envelope (Quả Khí Cầu Khổng Lồ)
            GameObject envelope = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            envelope.name = "Balloon_Envelope";
            envelope.transform.SetParent(balloonRoot.transform, false);
            envelope.transform.localPosition = new Vector3(0f, 8.8f, 0f);
            envelope.transform.localScale = new Vector3(7.5f, 9.2f, 7.5f);
            if (envelope.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(envelope.GetComponent<Collider>());
            if (blueMat != null) envelope.GetComponent<Renderer>().sharedMaterial = blueMat;

            // Băng chuyền đỏ trang trí trung tâm khí cầu (IUH Red Band)
            GameObject redEquator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            redEquator.name = "Balloon_RedBand";
            redEquator.transform.SetParent(envelope.transform, false);
            redEquator.transform.localPosition = Vector3.zero;
            redEquator.transform.localScale = new Vector3(1.02f, 0.22f, 1.02f);
            if (redEquator.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(redEquator.GetComponent<Collider>());
            if (redMat != null) redEquator.GetComponent<Renderer>().sharedMaterial = redMat;

            // Vành phát quang cyan
            GameObject neonRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neonRing.name = "Balloon_NeonRing";
            neonRing.transform.SetParent(envelope.transform, false);
            neonRing.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            neonRing.transform.localScale = new Vector3(1.03f, 0.05f, 1.03f);
            if (neonRing.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(neonRing.GetComponent<Collider>());
            if (cyanGlowMat != null) neonRing.GetComponent<Renderer>().sharedMaterial = cyanGlowMat;

            // 4. Suspension Cables (Dây cáp nối khí cầu xuống giỏ)
            Vector3[] cableOffsets = new Vector3[]
            {
                new Vector3(-1.4f, 0f, -1.4f),
                new Vector3(1.4f, 0f, -1.4f),
                new Vector3(-1.4f, 0f, 1.4f),
                new Vector3(1.4f, 0f, 1.4f)
            };
            for (int i = 0; i < cableOffsets.Length; i++)
            {
                GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cable.name = $"Cable_{i}";
                cable.transform.SetParent(balloonRoot.transform, false);
                cable.transform.localPosition = new Vector3(cableOffsets[i].x * 0.85f, 2.8f, cableOffsets[i].z * 0.85f);
                cable.transform.localScale = new Vector3(0.04f, 2.5f, 0.04f);
                if (cable.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(cable.GetComponent<Collider>());
                if (goldMat != null) cable.GetComponent<Renderer>().sharedMaterial = goldMat;
            }

            // 5. Automated Plasma Turrets (Ụ pháo tự động)
            var turrets = new List<Transform>();
            var muzzles = new List<Transform>();
            var lasers = new List<LineRenderer>();

            Vector3[] turretPositions = new Vector3[] { new Vector3(-1.6f, 1.35f, 0.6f), new Vector3(1.6f, 1.35f, 0.6f) };
            for (int i = 0; i < turretPositions.Length; i++)
            {
                GameObject turretGo = new GameObject($"PlasmaTurret_{i}");
                turretGo.transform.SetParent(basket.transform, false);
                turretGo.transform.localPosition = turretPositions[i];

                // Bệ xoay
                GameObject tBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tBase.transform.SetParent(turretGo.transform, false);
                tBase.transform.localScale = new Vector3(0.35f, 0.15f, 0.35f);
                if (tBase.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(tBase.GetComponent<Collider>());
                if (darkMat != null) tBase.GetComponent<Renderer>().sharedMaterial = darkMat;

                // Nòng pháo
                GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                barrel.transform.SetParent(turretGo.transform, false);
                barrel.transform.localPosition = new Vector3(0f, 0.1f, 0.35f);
                barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                barrel.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
                if (barrel.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(barrel.GetComponent<Collider>());
                if (cyanGlowMat != null) barrel.GetComponent<Renderer>().sharedMaterial = cyanGlowMat;

                GameObject muzzlePoint = new GameObject("Muzzle");
                muzzlePoint.transform.SetParent(barrel.transform, false);
                muzzlePoint.transform.localPosition = new Vector3(0f, 1.0f, 0f);

                // Laser Tracer
                var line = turretGo.AddComponent<LineRenderer>();
                line.startWidth = 0.1f;
                line.endWidth = 0.05f;
                line.sharedMaterial = cyanGlowMat;
                line.enabled = false;

                turrets.Add(turretGo.transform);
                muzzles.Add(muzzlePoint.transform);
                lasers.Add(line);
            }

            // 6. Energy Shield Dome Visual
            GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "Energy_Shield_Dome";
            shield.transform.SetParent(basket.transform, false);
            shield.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            shield.transform.localScale = new Vector3(4.2f, 3.2f, 4.2f);
            if (shield.GetComponent<Collider>() != null) UnityEngine.Object.DestroyImmediate(shield.GetComponent<Collider>());
            if (shieldMat != null) shield.GetComponent<Renderer>().sharedMaterial = shieldMat;
            shield.SetActive(false);

            // 7. 3D Billboard Prompt Canvas
            GameObject canvasGo = new GameObject("Balloon_Billboard_Canvas");
            canvasGo.transform.SetParent(balloonRoot.transform, false);
            canvasGo.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 150f);
            rt.localScale = Vector3.one * 0.012f;

            var txtGo = new GameObject("PromptText");
            txtGo.transform.SetParent(canvasGo.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            // Wire serialized fields to IUHHotAirBalloonController
            var so = new SerializedObject(balloonCtrl);
            so.FindProperty("m_BasketTransform").objectReferenceValue = basket.transform;
            so.FindProperty("m_PlayerAnchorPoint").objectReferenceValue = playerAnchor.transform;
            so.FindProperty("m_BillboardPromptText").objectReferenceValue = txt;
            so.FindProperty("m_EnergyShieldVisual").objectReferenceValue = shield;
            so.FindProperty("m_BurnerFlameVFX").objectReferenceValue = flamePS;

            var pivotsProp = so.FindProperty("m_TurretPivots");
            pivotsProp.arraySize = turrets.Count;
            for (int i = 0; i < turrets.Count; i++) pivotsProp.GetArrayElementAtIndex(i).objectReferenceValue = turrets[i];

            var muzzlesProp = so.FindProperty("m_TurretMuzzles");
            muzzlesProp.arraySize = muzzles.Count;
            for (int i = 0; i < muzzles.Count; i++) muzzlesProp.GetArrayElementAtIndex(i).objectReferenceValue = muzzles[i];

            var lasersProp = so.FindProperty("m_TurretLasers");
            lasersProp.arraySize = lasers.Count;
            for (int i = 0; i < lasers.Count; i++) lasersProp.GetArrayElementAtIndex(i).objectReferenceValue = lasers[i];

            so.ApplyModifiedProperties();

            Debug.Log("[IUH Aerial Combat] Đã dựng thành công Khinh Khí Cầu Chiến Đấu IUH tại trung tâm sân trường!");
        }

        private static ParticleSystem CreateBurnerFlame(Transform parent)
        {
            GameObject flameGo = new GameObject("Burner_Flame");
            flameGo.transform.SetParent(parent, false);
            flameGo.transform.localPosition = Vector3.zero;
            flameGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            var ps = flameGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.35f;
            main.startSpeed = 7.0f;
            main.startSize = 0.45f;
            main.startColor = new Color(1f, 0.55f, 0.1f, 0.9f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var psr = flameGo.GetComponent<ParticleSystemRenderer>();
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/CombatArena/ArenaGlowPurple.mat");
            if (mat != null) psr.sharedMaterial = mat;

            ps.Stop();
            return ps;
        }

        private static void CreateSafetyWall(Transform parent, string name, Vector3 localPos, Vector3 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = localPos;
            var col = wall.AddComponent<BoxCollider>();
            col.size = size;
        }

        private static void SetupFireworksManager()
        {
            var existing = GameObject.Find("IUH_FIREWORKS_MANAGER");
            if (existing == null)
            {
                existing = new GameObject("IUH_FIREWORKS_MANAGER");
            }
            if (existing.GetComponent<IUHCampusFireworksManager>() == null)
            {
                existing.AddComponent<IUHCampusFireworksManager>();
            }
        }

        private static void AdjustAerialMonstersForSkyCombat()
        {
            // Nâng các quái vật bay lên độ cao tầng mây 32m - 38m để chiến đấu với khinh khí cầu
            var drone1 = GameObject.Find("SKY_DRONE_PREDATOR_1");
            if (drone1 != null)
            {
                drone1.transform.position = new Vector3(0f, 35f, -12f);
                var ai = drone1.GetComponent<IUHFlyingMonsterAI>();
                if (ai != null) ai.SetZoneAnchor(new Vector3(0f, 35f, -12f));
            }

            var drone2 = GameObject.Find("SKY_DRONE_PREDATOR_2");
            if (drone2 != null)
            {
                drone2.transform.position = new Vector3(20f, 36f, -16f);
                var ai = drone2.GetComponent<IUHFlyingMonsterAI>();
                if (ai != null) ai.SetZoneAnchor(new Vector3(20f, 36f, -16f));
            }

            var drone3 = GameObject.Find("SKY_DRONE_PREDATOR_3");
            if (drone3 != null)
            {
                drone3.transform.position = new Vector3(-20f, 34f, -14f);
                var ai = drone3.GetComponent<IUHFlyingMonsterAI>();
                if (ai != null) ai.SetZoneAnchor(new Vector3(-20f, 34f, -14f));
            }
        }
    }
}

