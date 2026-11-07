using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Combat;

namespace IUHCampus.EditorTools
{
    public static class IUHFlyingMonsterSetup
    {
        [MenuItem("IUH Campus/Setup Sky Flying Monsters", false, 140)]
        public static void SetupSkyFlyingMonsters()
        {
            Debug.Log("<color=#00E5FF>[IUH Sky Monsters]</color> Bắt đầu tạo các quái vật bay trên trời...");

            // Clean up existing
            string[] names = new string[] { "SKY_DRONE_PREDATOR_1", "SKY_DRONE_PREDATOR_2", "SKY_DRONE_PREDATOR_3" };
            foreach (var n in names)
            {
                var existing = GameObject.Find(n);
                if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
            }

            // Create 3 Sky Predators at strategic positions above campus (lowered hover height for direct visibility)
            CreateSingleSkyMonster(
                name: "SKY_DRONE_PREDATOR_1",
                displayName: "Phi Cơ Mã Độc Alpha",
                spawnPos: new Vector3(0f, 3.6f, -12f), // Over central courtyard at clear line of sight
                patrolRadius: 8f,
                maxHp: 150
            );

            CreateSingleSkyMonster(
                name: "SKY_DRONE_PREDATOR_2",
                displayName: "Phi Cơ Mã Độc Beta (Khu Nhà E)",
                spawnPos: new Vector3(18f, 3.8f, -15f), // Over Building E plaza
                patrolRadius: 8f,
                maxHp: 140
            );

            CreateSingleSkyMonster(
                name: "SKY_DRONE_PREDATOR_3",
                displayName: "Phi Cơ Mã Độc Gamma (Khu Vườn)",
                spawnPos: new Vector3(-18f, 3.6f, -12f), // Over Canteen / Garden
                patrolRadius: 8f,
                maxHp: 130
            );

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("<color=#00FF88>[IUH Sky Monsters] Hoàn tất tạo 3 Phi Cơ Quái Vật trên bầu trời!</color>");
        }

        private static void CreateSingleSkyMonster(string name, string displayName, Vector3 spawnPos, float patrolRadius, int maxHp)
        {
            GameObject root = new GameObject(name);
            root.transform.position = spawnPos;

            // Hit Collider for bullets in the sky
            var col = root.AddComponent<SphereCollider>();
            col.radius = 1.35f;

            // Materials
            Material darkArmorMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            darkArmorMat.color = new Color(0.12f, 0.14f, 0.18f);

            Material glowingRedMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            glowingRedMat.color = new Color(1f, 0.2f, 0.15f);
            glowingRedMat.EnableKeyword("_EMISSION");
            glowingRedMat.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.15f) * 4f);

            Material turbineGlowMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            turbineGlowMat.color = new Color(0.9f, 0.4f, 0.1f);
            turbineGlowMat.EnableKeyword("_EMISSION");
            turbineGlowMat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 3f);

            // 1. Fuselage
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Fuselage";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.9f, 0.32f, 1.5f);
            body.GetComponent<Renderer>().sharedMaterial = darkArmorMat;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            // 2. Ominous Eye Canopy
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Cyber_Core_Eye";
            eye.transform.SetParent(root.transform, false);
            eye.transform.localPosition = new Vector3(0f, 0.05f, 0.5f);
            eye.transform.localScale = new Vector3(0.5f, 0.22f, 0.6f);
            eye.GetComponent<Renderer>().sharedMaterial = glowingRedMat;
            UnityEngine.Object.DestroyImmediate(eye.GetComponent<Collider>());

            // 3. Swept Wings
            GameObject wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingL.name = "Wing_Left";
            wingL.transform.SetParent(root.transform, false);
            wingL.transform.localPosition = new Vector3(-0.95f, 0.02f, -0.1f);
            wingL.transform.localRotation = Quaternion.Euler(0f, -18f, 5f);
            wingL.transform.localScale = new Vector3(1.4f, 0.06f, 0.65f);
            wingL.GetComponent<Renderer>().sharedMaterial = darkArmorMat;
            UnityEngine.Object.DestroyImmediate(wingL.GetComponent<Collider>());

            GameObject wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingR.name = "Wing_Right";
            wingR.transform.SetParent(root.transform, false);
            wingR.transform.localPosition = new Vector3(0.95f, 0.02f, -0.1f);
            wingR.transform.localRotation = Quaternion.Euler(0f, 18f, -5f);
            wingR.transform.localScale = new Vector3(1.4f, 0.06f, 0.65f);
            wingR.GetComponent<Renderer>().sharedMaterial = darkArmorMat;
            UnityEngine.Object.DestroyImmediate(wingR.GetComponent<Collider>());

            // 4. Quad Turbines & Spinning Blades
            List<Transform> rotors = new List<Transform>();
            Vector3[] turbineOffsets = new Vector3[] {
                new Vector3(-0.75f, 0.08f,  0.55f),
                new Vector3( 0.75f, 0.08f,  0.55f),
                new Vector3(-1.15f, 0.08f, -0.45f),
                new Vector3( 1.15f, 0.08f, -0.45f)
            };

            for (int t = 0; t < turbineOffsets.Length; t++)
            {
                GameObject pod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pod.name = $"TurbinePod_{t}";
                pod.transform.SetParent(root.transform, false);
                pod.transform.localPosition = turbineOffsets[t];
                pod.transform.localScale = new Vector3(0.35f, 0.12f, 0.35f);
                pod.GetComponent<Renderer>().sharedMaterial = darkArmorMat;
                UnityEngine.Object.DestroyImmediate(pod.GetComponent<Collider>());

                // Rotor Blade
                GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prop.name = $"Propeller_{t}";
                prop.transform.SetParent(pod.transform, false);
                prop.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                prop.transform.localScale = new Vector3(1.8f, 0.1f, 0.15f);
                prop.GetComponent<Renderer>().sharedMaterial = turbineGlowMat;
                UnityEngine.Object.DestroyImmediate(prop.GetComponent<Collider>());
                rotors.Add(prop.transform);

                // Thrust Light
                GameObject lightGo = new GameObject($"ThrustLight_{t}");
                lightGo.transform.SetParent(pod.transform, false);
                lightGo.transform.localPosition = Vector3.down * 0.5f;
                var l = lightGo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1f, 0.3f, 0.05f);
                l.range = 2.5f;
                l.intensity = 2.0f;
            }

            // 5. Dual Underslung Cannons
            GameObject gunL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gunL.name = "Cannon_Left";
            gunL.transform.SetParent(root.transform, false);
            gunL.transform.localPosition = new Vector3(-0.35f, -0.16f, 0.45f);
            gunL.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            gunL.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
            gunL.GetComponent<Renderer>().sharedMaterial = darkArmorMat;
            UnityEngine.Object.DestroyImmediate(gunL.GetComponent<Collider>());

            GameObject gunR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gunR.name = "Cannon_Right";
            gunR.transform.SetParent(root.transform, false);
            gunR.transform.localPosition = new Vector3(0.35f, -0.16f, 0.45f);
            gunR.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            gunR.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
            gunR.GetComponent<Renderer>().sharedMaterial = darkArmorMat;
            UnityEngine.Object.DestroyImmediate(gunR.GetComponent<Collider>());

            // 6. Targeting Laser Beam
            GameObject laserGo = new GameObject("Targeting_Laser");
            laserGo.transform.SetParent(root.transform, false);
            var lr = laserGo.AddComponent<LineRenderer>();
            lr.startWidth = 0.06f;
            lr.endWidth = 0.03f;
            lr.material = glowingRedMat;
            lr.enabled = false;

            // 7. IUHRealtimeMonsterTarget
            var targetComp = root.AddComponent<IUHRealtimeMonsterTarget>();
            var soTarget = new SerializedObject(targetComp);
            soTarget.FindProperty("m_MonsterName").stringValue = displayName;
            soTarget.FindProperty("m_MaxHp").intValue = maxHp;
            soTarget.FindProperty("m_CurrentHp").intValue = maxHp;
            soTarget.FindProperty("m_RewardGold").intValue = 75;
            soTarget.FindProperty("m_RewardItem").stringValue = "Lõi Năng Lượng Bay";
            soTarget.ApplyModifiedProperties();

            // Overhead HP bar
            CreateOverheadHpBar(root, targetComp, maxHp);

            // 8. IUHFlyingMonsterAI
            var ai = root.AddComponent<IUHFlyingMonsterAI>();
            var soAi = new SerializedObject(ai);
            soAi.FindProperty("m_FlightAltitude").floatValue = spawnPos.y;
            soAi.FindProperty("m_PatrolRadius").floatValue = patrolRadius;
            soAi.FindProperty("m_LaserBeam").objectReferenceValue = null;

            var muzzlesProp = soAi.FindProperty("m_FireMuzzles");
            muzzlesProp.arraySize = 2;
            muzzlesProp.GetArrayElementAtIndex(0).objectReferenceValue = gunL.transform;
            muzzlesProp.GetArrayElementAtIndex(1).objectReferenceValue = gunR.transform;

            var rotorsProp = soAi.FindProperty("m_RotorBlades");
            rotorsProp.arraySize = rotors.Count;
            for (int r = 0; r < rotors.Count; r++)
            {
                rotorsProp.GetArrayElementAtIndex(r).objectReferenceValue = rotors[r];
            }

            soAi.ApplyModifiedProperties();
        }

        private static void CreateOverheadHpBar(GameObject root, IUHRealtimeMonsterTarget target, int maxHp)
        {
            GameObject hpCanvas = new GameObject("Realtime_Hp_Bar", typeof(RectTransform), typeof(Canvas));
            hpCanvas.transform.SetParent(root.transform, false);
            hpCanvas.transform.localPosition = Vector3.up * 1.3f;
            var c = hpCanvas.GetComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            var cRt = hpCanvas.GetComponent<RectTransform>();
            cRt.sizeDelta = new Vector2(160f, 22f);
            cRt.localScale = Vector3.one * 0.015f;

            // Billboard script or look at camera
            var lookAt = hpCanvas.AddComponent<IUHCampusBillboard>();

            // BG
            GameObject bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(hpCanvas.transform, false);
            bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(hpCanvas.transform, false);
            var fImg = fill.GetComponent<Image>();
            fImg.color = new Color(1f, 0.35f, 0.1f);
            fImg.type = Image.Type.Filled;
            fImg.fillMethod = Image.FillMethod.Horizontal;
            fImg.fillAmount = 1f;
            var fRt = fill.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;

            // Text
            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(hpCanvas.transform, false);
            var txt = txtGo.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.text = $"{maxHp}/{maxHp}";
            txt.fontSize = 11;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            var tRt = txtGo.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            var so = new SerializedObject(target);
            so.FindProperty("m_HpBarRoot").objectReferenceValue = hpCanvas;
            so.FindProperty("m_HpFillImage").objectReferenceValue = fImg;
            so.FindProperty("m_HpText").objectReferenceValue = txt;
            so.ApplyModifiedProperties();
        }
    }

    public class IUHCampusBillboard : MonoBehaviour
    {
        private Camera m_Cam;
        private void Start() { m_Cam = Camera.main; }
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

