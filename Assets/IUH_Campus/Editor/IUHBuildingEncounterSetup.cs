using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Combat;

namespace IUHCampus.EditorTools
{
    public static class IUHBuildingEncounterSetup
    {
        [MenuItem("IUH Campus/Setup Building Monster Ambushes", false, 135)]
        public static void SetupBuildingAmbushes()
        {
            Debug.Log("<color=#00E5FF>[IUH Building Ambushes]</color> Đang tạo các điểm phục kích quái vật có đầy đủ hoạt ảnh...");

            GameObject footmanPolyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Mini Legion Footman PBR HP Polyart/Prefabs/FootmanPolyart.prefab");
            GameObject footmanHpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Mini Legion Footman PBR HP Polyart/Prefabs/FootmanHP.prefab") ?? footmanPolyPrefab;
            GameObject footmanPbrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Mini Legion Footman PBR HP Polyart/Prefabs/FootmanPBR.prefab") ?? footmanPolyPrefab;

            GameObject[] prefabs = new GameObject[] { footmanPolyPrefab, footmanHpPrefab, footmanPbrPrefab };

            // 1. Building E Ambush (Tòa Nhà Kỹ Thuật E)
            SetupSingleAmbush(
                spawnerName: "BUILDING_E_AMBUSH",
                buildingName: "TÒA NHÀ KỸ THUẬT E",
                alertMessage: "Vệ Binh Nhiễm Mã Độc Xuất Hiện!",
                spawnerPos: new Vector3(18f, 0.1f, -15f),
                triggerRadius: 6.5f,
                monsterPositions: new Vector3[] {
                    new Vector3(16.5f, 0.1f, -13.5f),
                    new Vector3(19.5f, 0.1f, -14.2f)
                },
                prefabs
            );

            // 2. Canteen Ambush (Khu Căn Tin)
            SetupSingleAmbush(
                spawnerName: "CANTEEN_AMBUSH",
                buildingName: "KHU CĂN TIN SINH VIÊN",
                alertMessage: "Chiến Binh Mã Độc Rình Rập!",
                spawnerPos: new Vector3(-24f, 0.1f, -18f),
                triggerRadius: 7.0f,
                monsterPositions: new Vector3[] {
                    new Vector3(-22.5f, 0.1f, -17f),
                    new Vector3(-25.5f, 0.1f, -19.5f)
                },
                prefabs
            );

            // 3. Garden Ambush (Khu Vườn Tri Thức)
            SetupSingleAmbush(
                spawnerName: "GARDEN_AMBUSH",
                buildingName: "VƯỜN CÂY TRI THỨC",
                alertMessage: "Vệ Binh Bóng Đêm Trỗi Dậy!",
                spawnerPos: new Vector3(-15f, 0.1f, 6f),
                triggerRadius: 6.0f,
                monsterPositions: new Vector3[] {
                    new Vector3(-13.5f, 0.1f, 7.5f),
                    new Vector3(-16.2f, 0.1f, 5.2f)
                },
                prefabs
            );

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("<color=#00FF88>[IUH Building Ambushes] Hoàn tất cài đặt các điểm phục kích quái vật!</color>");
        }

        private static void SetupSingleAmbush(
            string spawnerName,
            string buildingName,
            string alertMessage,
            Vector3 spawnerPos,
            float triggerRadius,
            Vector3[] monsterPositions,
            GameObject[] prefabs)
        {
            GameObject spawnerGo = GameObject.Find(spawnerName);
            if (spawnerGo != null) UnityEngine.Object.DestroyImmediate(spawnerGo);
            spawnerGo = new GameObject(spawnerName);
            spawnerGo.transform.position = spawnerPos;

            var spawnerCol = spawnerGo.AddComponent<SphereCollider>();
            spawnerCol.isTrigger = true;
            spawnerCol.radius = triggerRadius;

            var spawnerComp = spawnerGo.AddComponent<IUHBuildingMonsterSpawner>();

            GameObject[] monsterGos = new GameObject[monsterPositions.Length];

            for (int i = 0; i < monsterPositions.Length; i++)
            {
                GameObject chosenPrefab = prefabs[i % prefabs.Length];
                GameObject monster = (GameObject)PrefabUtility.InstantiatePrefab(chosenPrefab);
                monster.name = $"{spawnerName}_WARRIOR_{i + 1}";

                monster.transform.position = monsterPositions[i];
                monster.transform.SetParent(spawnerGo.transform, true);

                // Add Collider & CharacterController for shooting, melee punch & movement
                var col = monster.GetComponent<CapsuleCollider>();
                if (col == null) col = monster.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0f, 0.9f, 0f);
                col.height = 1.8f;
                col.radius = 0.5f;

                var cc = monster.GetComponent<CharacterController>();
                if (cc == null) cc = monster.AddComponent<CharacterController>();
                cc.center = new Vector3(0f, 0.9f, 0f);
                cc.height = 1.8f;
                cc.radius = 0.5f;

                // Add Realtime Target
                var target = monster.GetComponent<IUHRealtimeMonsterTarget>();
                if (target == null) target = monster.AddComponent<IUHRealtimeMonsterTarget>();

                // Add Monster AI
                var ai = monster.GetComponent<IUHMonsterAI>();
                if (ai == null) ai = monster.AddComponent<IUHMonsterAI>();

                // Overhead HP Canvas
                CreateOverheadHpBar(monster, target, 120 + i * 20);

                // Hide initially
                monster.SetActive(false);
                monsterGos[i] = monster;
            }

            var so = new SerializedObject(spawnerComp);
            so.FindProperty("m_BuildingName").stringValue = buildingName;
            so.FindProperty("m_AlertMessage").stringValue = alertMessage;
            so.FindProperty("m_TriggerRadius").floatValue = triggerRadius;

            var listProp = so.FindProperty("m_MonsterInstancesToActivate");
            listProp.arraySize = monsterGos.Length;
            for (int i = 0; i < monsterGos.Length; i++)
            {
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = monsterGos[i];
            }
            so.ApplyModifiedProperties();
        }

        private static void CreateOverheadHpBar(GameObject monster, IUHRealtimeMonsterTarget target, int maxHp)
        {
            var so = new SerializedObject(target);
            so.FindProperty("m_MonsterName").stringValue = monster.name.Contains("FOOTMAN") ? "Vệ Binh Hắc Ám" : "Quái Dữ Liệu";
            so.FindProperty("m_MaxHp").intValue = maxHp;
            so.FindProperty("m_CurrentHp").intValue = maxHp;
            so.FindProperty("m_RewardGold").intValue = 45;

            Transform hpRoot = monster.transform.Find("Realtime_Hp_Bar");
            if (hpRoot == null)
            {
                GameObject hpCanvas = new GameObject("Realtime_Hp_Bar", typeof(RectTransform), typeof(Canvas));
                hpCanvas.transform.SetParent(monster.transform, false);
                hpCanvas.transform.localPosition = Vector3.up * 2.2f;
                var c = hpCanvas.GetComponent<Canvas>();
                c.renderMode = RenderMode.WorldSpace;
                var cRt = hpCanvas.GetComponent<RectTransform>();
                cRt.sizeDelta = new Vector2(150f, 20f);
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
                fImg.color = new Color(1f, 0.25f, 0.35f);
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
}
