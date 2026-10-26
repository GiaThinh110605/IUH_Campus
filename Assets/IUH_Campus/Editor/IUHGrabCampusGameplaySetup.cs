using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;
using IUHCampus.UI;

namespace IUHCampus.Editor
{
    /// <summary>
    /// Master Setup & Integrator for Mini-Game 2.1 "Grab Campus: Chuyến xe tâm linh".
    /// Features:
    /// 1. Configures Survival Items Inventory & Action Bar (Đèn pin, Pin, Bùa, Pháo, Gậy, Sơ cứu).
    /// 2. Spawns 3D Pickups across campus & outside-campus street district.
    /// 3. Sets up 4 Intelligent Ghost AI (Observer, PathBlocker, Chasers).
    /// 4. Connects Order Acceptance Board & Result Summary with 3 actions (Replay, Other Game, Main Menu).
    /// 5. Ensures zero duplication and idempotent execution.
    /// </summary>
    [InitializeOnLoad]
    public static class IUHGrabCampusGameplaySetup
    {
        private const string RootName = "GAMEPLAY_GRAB_CAMPUS";
        private const string ItemContainerName = "GRAB_SURVIVAL_ITEMS";

        static IUHGrabCampusGameplaySetup()
        {
            EditorApplication.delayCall += SetupCompleteGameplay;
        }

        [MenuItem("IUH Campus/Minigames/Setup Grab Campus 2.1 Complete Gameplay")]
        public static void SetupCompleteGameplay()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            Debug.Log("[Grab Campus Setup] Bắt đầu thiết lập hoàn thiện gameplay 2.1 Chuyến Xe Tâm Linh...");

            // 1. Find or Ensure Root
            var gameplayRoot = GameObject.Find(RootName);
            if (gameplayRoot == null)
            {
                IUHGrabCampusBuilder.BuildGrabCampusAndMenu();
                gameplayRoot = GameObject.Find(RootName);
            }

            if (gameplayRoot == null)
            {
                Debug.LogError("[Grab Campus Setup] Không tìm thấy root GAMEPLAY_GRAB_CAMPUS!");
                return;
            }

            // 2. Ensure Inventory Manager on Root
            var invMgr = gameplayRoot.GetComponent<IUHGrabInventoryManager>();
            if (invMgr == null) invMgr = gameplayRoot.AddComponent<IUHGrabInventoryManager>();

            // 3. Ensure Canvas & HUD
            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO != null)
            {
                var grabHud = canvasGO.GetComponent<IUHGrabCampusHUD>();
                if (grabHud == null) grabHud = canvasGO.AddComponent<IUHGrabCampusHUD>();

                var horrorUI = canvasGO.GetComponent<IUHHorrorPursuitUI>();
                if (horrorUI == null) horrorUI = canvasGO.AddComponent<IUHHorrorPursuitUI>();
            }

            // 4. Setup 3D Survival Items Pickups
            SetupSurvivalItemPickups(gameplayRoot.transform);

            // 5. Configure Ghosts with new Intelligent AI
            ConfigureIntelligentGhosts(gameplayRoot.transform);

            // 6. Connect References to IUHGrabCampusManager
            var grabMgr = gameplayRoot.GetComponent<IUHGrabCampusManager>();
            if (grabMgr != null)
            {
                var so = new SerializedObject(grabMgr);
                var ghostsInScene = GameObject.FindObjectsByType<IUHGhostHazard>(FindObjectsSortMode.None);
                var ghostListProp = so.FindProperty("m_GhostHazards");
                ghostListProp.ClearArray();
                for (int i = 0; i < ghostsInScene.Length; i++)
                {
                    ghostListProp.InsertArrayElementAtIndex(i);
                    ghostListProp.GetArrayElementAtIndex(i).objectReferenceValue = ghostsInScene[i].gameObject;
                }
                so.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[Grab Campus Setup] HOÀN TẤT THIẾT LẬP GAMEPLAY 2.1 THÀNH CÔNG RỰC RỠ! Sẵn sàng chơi trong Play Mode.");
        }

        private static void SetupSurvivalItemPickups(Transform root)
        {
            var container = root.Find(ItemContainerName);
            if (container != null) UnityEngine.Object.DestroyImmediate(container.gameObject);

            var containerGO = new GameObject(ItemContainerName);
            containerGO.transform.SetParent(root, false);

            // Spawn 8 strategically placed items:
            // 1. Flashlight Battery near Courtyard bench
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.Battery, "Pin Dự Phòng", "Nạp 100% pin cho đèn pin", new Vector3(378f, 0.60f, -42f), 1);

            // 2. Roadside Flare near Gate 1 boulevard
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.Flare, "Pháo Sáng Khẩn Cấp", "Ném ra xua đuổi ma trong 8 giây", new Vector3(398f, 0.60f, -38f), 2);

            // 3. Talisman near KTX Dorm Hallway
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.Talisman, "Bùa Hộ Mệnh Cổ", "Tạo lá chắn tâm linh chặn đòn ma", new Vector3(342f, 0.60f, -74f), 1);

            // 4. Defense Bat near Block V study tables
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.DefenseBat, "Gậy Tự Vệ Sinh Viên", "Đánh cận chiến làm choáng ma 4.5s", new Vector3(362f, 0.60f, -86f), 1);

            // 5. First Aid Kit near IUH Canteen
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.FirstAidKit, "Bộ Sơ Cứu Khẩn Cấp", "Hồi phục +35% độ bền kiện hàng", new Vector3(388f, 0.60f, -32f), 1);

            // 6. Flare near Outside Street District (FamilyMart)
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.Flare, "Pháo Sáng Đỏ", "Làm ma mất dấu và bỏ chạy", new Vector3(-20f, 0.45f, -140f), 2);

            // 7. Battery near Highlands Coffee patio
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.Battery, "Pin Dự Phòng Siêu Tốc", "Nạp đầy pin đèn pin", new Vector3(22f, 0.45f, -108f), 1);

            // 8. Talisman at Pocket Park shrine
            CreateItemPickup(containerGO.transform, IUHGrabSurvivalItem.ItemType.Talisman, "Bùa Bình An IUH", "Bảo vệ tài xế an toàn qua đêm vắng", new Vector3(0f, 0.45f, -170f), 1);
        }

        private static GameObject CreateItemPickup(Transform parent, IUHGrabSurvivalItem.ItemType type, string name, string desc, Vector3 pos, int count)
        {
            var itemGO = new GameObject($"Item_{type}_{name.Replace(" ", "")}");
            itemGO.transform.SetParent(parent, false);
            itemGO.transform.position = pos;

            // 3D Visual Mesh
            PrimitiveType pType = PrimitiveType.Cylinder;
            Vector3 scale = new Vector3(0.35f, 0.22f, 0.35f);

            switch (type)
            {
                case IUHGrabSurvivalItem.ItemType.Battery:
                    pType = PrimitiveType.Cube;
                    scale = new Vector3(0.24f, 0.35f, 0.16f);
                    break;
                case IUHGrabSurvivalItem.ItemType.Talisman:
                    pType = PrimitiveType.Quad;
                    scale = new Vector3(0.35f, 0.55f, 0.35f);
                    break;
                case IUHGrabSurvivalItem.ItemType.Flare:
                    pType = PrimitiveType.Cylinder;
                    scale = new Vector3(0.12f, 0.45f, 0.12f);
                    break;
                case IUHGrabSurvivalItem.ItemType.DefenseBat:
                    pType = PrimitiveType.Capsule;
                    scale = new Vector3(0.14f, 0.65f, 0.14f);
                    break;
                case IUHGrabSurvivalItem.ItemType.FirstAidKit:
                    pType = PrimitiveType.Cube;
                    scale = new Vector3(0.42f, 0.32f, 0.24f);
                    break;
            }

            var meshGO = GameObject.CreatePrimitive(pType);
            meshGO.name = "Mesh";
            meshGO.transform.SetParent(itemGO.transform, false);
            meshGO.transform.localPosition = Vector3.zero;
            meshGO.transform.localScale = scale;

            // Destroy mesh collider to prevent vehicle physical collisions
            var col = meshGO.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            // Material
            var r = meshGO.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            Color c = GetItemColor(type);
            mat.color = c;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * 2.2f);
            r.sharedMaterial = mat;

            // Add Pickup Component
            var pickup = itemGO.AddComponent<IUHGrabSurvivalItem>();
            pickup.type = type;
            pickup.displayName = name;
            pickup.description = desc;
            pickup.amount = count;

            return itemGO;
        }

        private static Color GetItemColor(IUHGrabSurvivalItem.ItemType type)
        {
            switch (type)
            {
                case IUHGrabSurvivalItem.ItemType.Flashlight: return new Color(0.2f, 0.8f, 1.0f);
                case IUHGrabSurvivalItem.ItemType.Battery: return new Color(0.2f, 1.0f, 0.5f);
                case IUHGrabSurvivalItem.ItemType.Talisman: return new Color(1.0f, 0.85f, 0.1f);
                case IUHGrabSurvivalItem.ItemType.Flare: return new Color(1.0f, 0.25f, 0.1f);
                case IUHGrabSurvivalItem.ItemType.DefenseBat: return new Color(0.7f, 0.4f, 1.0f);
                case IUHGrabSurvivalItem.ItemType.FirstAidKit: return new Color(1.0f, 0.2f, 0.3f);
                case IUHGrabSurvivalItem.ItemType.Key: return new Color(1.0f, 0.95f, 0.4f);
                default: return Color.yellow;
            }
        }

        private static void ConfigureIntelligentGhosts(Transform root)
        {
            var ghosts = root.GetComponentsInChildren<IUHGhostHazard>(true);
            if (ghosts == null || ghosts.Length == 0) return;

            for (int i = 0; i < ghosts.Length; i++)
            {
                var g = ghosts[i];
                if (i == 0)
                {
                    // Ghost 1: Observer (Spectral shadow near trees watching player)
                    g.behavior = IUHGhostHazard.GhostBehaviorType.Observer;
                }
                else if (i == 1)
                {
                    // Ghost 2: Path Blocker (Blocking the lane)
                    g.behavior = IUHGhostHazard.GhostBehaviorType.PathBlocker;
                }
                else
                {
                    // Ghosts 3 & 4: Active Pursuit Chasers
                    g.behavior = IUHGhostHazard.GhostBehaviorType.Chaser;
                }
            }
        }
    }
}
