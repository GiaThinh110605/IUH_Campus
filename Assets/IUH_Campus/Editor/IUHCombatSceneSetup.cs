using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Combat;

namespace IUHCampus.EditorTools
{
    public static class IUHCombatSceneSetup
    {
        [MenuItem("IUH Campus/Setup Realtime Combat System", false, 130)]
        public static void SetupRealtimeCombat()
        {
            Debug.Log("<color=#00E5FF>[IUH Combat System]</color> Bắt đầu cài đặt hệ thống súng đạn & kỹ năng [1], [2], [3]...");

            // 1. Build and Attach Cyber Plasma Rifle to Student's Right_Hand
            Transform muzzleTrans = SetupStudentWeapon();

            // 2. Setup Combat Manager
            GameObject mgrGo = GameObject.Find("IUH_COMBAT_MANAGER");
            if (mgrGo == null) mgrGo = new GameObject("IUH_COMBAT_MANAGER");
            var combatMgr = mgrGo.GetComponent<IUHCampusCombatManager>();
            if (combatMgr == null) combatMgr = mgrGo.AddComponent<IUHCampusCombatManager>();

            // Configure muzzle & tracer on CombatManager
            var soMgr = new SerializedObject(combatMgr);
            if (muzzleTrans != null)
            {
                soMgr.FindProperty("m_GunMuzzle").objectReferenceValue = muzzleTrans;
                soMgr.FindProperty("m_LaserTracer").objectReferenceValue = muzzleTrans.GetComponent<LineRenderer>();
                soMgr.FindProperty("m_MuzzleLight").objectReferenceValue = muzzleTrans.GetComponent<Light>();
            }
            soMgr.ApplyModifiedProperties();

            // 3. Setup Monsters as Realtime Targets with Overhead HP Bars
            SetupMonstersTargets();

            // 4. Setup Combat HUD Canvas (3 Skills + Fire + Crosshair)
            SetupCombatHUDCanvas();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("<color=#00FF88>[IUH Combat System] Cài đặt hoàn tất thành công! Sẵn sàng chiến đấu!</color>");
        }

        private static Transform SetupStudentWeapon()
        {
            var student = GameObject.Find("Campus Student");
            if (student == null)
            {
                Debug.LogWarning("[IUH Combat] Không tìm thấy Campus Student!");
                return null;
            }

            // Find Right_Hand bone
            Transform rightHand = null;
            foreach (var t in student.GetComponentsInChildren<Transform>())
            {
                if (t.name.Equals("Right_Hand", StringComparison.OrdinalIgnoreCase))
                {
                    rightHand = t;
                    break;
                }
            }

            if (rightHand == null) rightHand = student.transform;

            // Remove existing gun if any
            var existingGun = rightHand.Find("IUH_CYBER_PLASMA_RIFLE");
            if (existingGun != null) UnityEngine.Object.DestroyImmediate(existingGun.gameObject);

            // Create Cyber Rifle Root
            GameObject gunGo = new GameObject("IUH_CYBER_PLASMA_RIFLE");
            gunGo.transform.SetParent(rightHand, false);
            gunGo.transform.localPosition = new Vector3(0.08f, 0.05f, 0.12f);
            gunGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            gunGo.transform.localScale = Vector3.one * 0.85f;

            // Materials
            Material gunBodyMat = CreateMaterial("M_CyberRifle_Body", new Color(0.12f, 0.14f, 0.18f), 0.8f, 0.8f, Color.black);
            Material neonMat = CreateMaterial("M_CyberRifle_Neon", new Color(0.1f, 0.95f, 1f), 0.9f, 0.9f, new Color(0.1f, 0.9f, 1f) * 3f);

            // Gun Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Receiver";
            body.transform.SetParent(gunGo.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.45f, 0.14f, 0.08f);
            body.GetComponent<Renderer>().sharedMaterial = gunBodyMat;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            // Barrel
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(gunGo.transform, false);
            barrel.transform.localPosition = new Vector3(0.35f, 0.02f, 0f);
            barrel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            barrel.transform.localScale = new Vector3(0.06f, 0.22f, 0.06f);
            barrel.GetComponent<Renderer>().sharedMaterial = neonMat;
            UnityEngine.Object.DestroyImmediate(barrel.GetComponent<Collider>());

            // Grip / Handle
            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Grip";
            grip.transform.SetParent(gunGo.transform, false);
            grip.transform.localPosition = new Vector3(-0.08f, -0.12f, 0f);
            grip.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
            grip.transform.localScale = new Vector3(0.08f, 0.18f, 0.06f);
            grip.GetComponent<Renderer>().sharedMaterial = gunBodyMat;
            UnityEngine.Object.DestroyImmediate(grip.GetComponent<Collider>());

            // Muzzle Point
            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(gunGo.transform, false);
            muzzle.transform.localPosition = new Vector3(0.58f, 0.02f, 0f);

            // Muzzle Flash Light
            var light = muzzle.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.2f, 0.9f, 1f);
            light.intensity = 2.5f;
            light.range = 4f;
            light.enabled = false;

            // Laser Tracer LineRenderer
            var lr = muzzle.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.02f;
            lr.sharedMaterial = neonMat;
            lr.enabled = false;

            return muzzle.transform;
        }

        private static void SetupMonstersTargets()
        {
            string[] monsterNames = new string[]
            {
                "MONSTER_01_BLUEDEMON",
                "MONSTER_02_ORC",
                "MONSTER_03_PHANTOM",
                "MONSTER_04_DEMON_BOSS",
                "MONSTER_05_CORRUPTED_BACKPACK"
            };

            int[] monsterHps = new int[] { 140, 180, 220, 450, 160 };

            for (int i = 0; i < monsterNames.Length; i++)
            {
                var mGo = GameObject.Find(monsterNames[i]);
                if (mGo == null) continue;

                var targetComp = mGo.GetComponent<IUHRealtimeMonsterTarget>();
                if (targetComp == null) targetComp = mGo.AddComponent<IUHRealtimeMonsterTarget>();

                // Configure HP
                var so = new SerializedObject(targetComp);
                so.FindProperty("m_MonsterName").stringValue = monsterNames[i].Replace("MONSTER_", "QUÁI VẬT ");
                so.FindProperty("m_MaxHp").intValue = monsterHps[i];
                so.FindProperty("m_CurrentHp").intValue = monsterHps[i];
                so.FindProperty("m_RewardGold").intValue = 60 + i * 25;

                // Create or find overhead HP bar
                Transform hpRoot = mGo.transform.Find("Realtime_Hp_Bar");
                if (hpRoot == null)
                {
                    GameObject hpCanvas = new GameObject("Realtime_Hp_Bar", typeof(RectTransform), typeof(Canvas));
                    hpCanvas.transform.SetParent(mGo.transform, false);
                    hpCanvas.transform.localPosition = Vector3.up * 2.8f;
                    var c = hpCanvas.GetComponent<Canvas>();
                    c.renderMode = RenderMode.WorldSpace;
                    var cRt = hpCanvas.GetComponent<RectTransform>();
                    cRt.sizeDelta = new Vector2(160f, 22f);
                    cRt.localScale = Vector3.one * 0.01f;

                    // BG
                    GameObject bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
                    bg.transform.SetParent(hpCanvas.transform, false);
                    bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
                    var bgRt = bg.GetComponent<RectTransform>();
                    bgRt.anchorMin = Vector2.zero;
                    bgRt.anchorMax = Vector2.one;
                    bgRt.sizeDelta = Vector2.zero;

                    // Fill
                    GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                    fill.transform.SetParent(hpCanvas.transform, false);
                    var fImg = fill.GetComponent<Image>();
                    fImg.color = new Color(1f, 0.2f, 0.3f);
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
                    txt.text = $"{monsterHps[i]}/{monsterHps[i]}";
                    txt.fontSize = 11;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = Color.white;
                    var tRt = txtGo.GetComponent<RectTransform>();
                    tRt.anchorMin = Vector2.zero;
                    tRt.anchorMax = Vector2.one;
                    tRt.sizeDelta = Vector2.zero;

                    so.FindProperty("m_HpBarRoot").objectReferenceValue = hpCanvas;
                    so.FindProperty("m_HpFillImage").objectReferenceValue = fImg;
                    so.FindProperty("m_HpText").objectReferenceValue = txt;
                }

                so.ApplyModifiedProperties();
            }
        }

        private static void SetupCombatHUDCanvas()
        {
            GameObject hudGo = GameObject.Find("IUH_COMBAT_HUD_CANVAS");
            if (hudGo != null) UnityEngine.Object.DestroyImmediate(hudGo);
            hudGo = new GameObject("IUH_COMBAT_HUD_CANVAS");

            var canvas = hudGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 75;

            var scaler = hudGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            hudGo.AddComponent<GraphicRaycaster>();
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Center Crosshair Reticle
            GameObject crosshair = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            crosshair.transform.SetParent(hudGo.transform, false);
            var chImg = crosshair.GetComponent<Image>();
            chImg.color = new Color(0.2f, 0.95f, 1f, 0.75f);
            chImg.raycastTarget = false;
            var chRt = crosshair.GetComponent<RectTransform>();
            chRt.anchorMin = new Vector2(0.5f, 0.5f);
            chRt.anchorMax = new Vector2(0.5f, 0.5f);
            chRt.sizeDelta = new Vector2(10f, 10f);

            var hudComp = hudGo.AddComponent<IUHCombatHUD>();
            var so = new SerializedObject(hudComp);
            so.FindProperty("m_CrosshairRoot").objectReferenceValue = crosshair;
            so.ApplyModifiedProperties();
        }

        private static Material CreateMaterial(string name, Color baseColor, float metallic, float smoothness, Color emission)
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

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}

