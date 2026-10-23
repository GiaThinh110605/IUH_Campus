#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IUHCampus.Minigames.GreenCampus.Editor
{
    public static class IUHGreenCampusSetup
    {
        public const string ScenePath = "Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity";
        public const string RootName = "GAMEPLAY_GREEN_CAMPUS";
        private const string MaterialFolder = "Assets/IUH_Campus/Materials/GreenCampus";

        [MenuItem("IUH Campus/Minigames/Setup Green Campus")]
        public static void SetupMenu() { Setup(); }

        public static bool Setup()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
                throw new InvalidOperationException("Open IUH_Campus_Main in Edit Mode before Green Campus setup.");
            var existing = GameObject.Find(RootName);
            if (existing != null)
            {
                var current = existing.GetComponent<IUHGreenCampusManager>();
                if (current != null && current.items.Length == 9 && current.bins.Length == 3 && current.ui != null)
                {
                    Undo.RecordObject(current, "Configure Green Campus spawn");
                    current.spawnPoint = IUHCampus.World.IUHReferenceLayout.IsActive
                        ? IUHCampus.World.IUHReferenceLayout.GreenSpawn : new Vector3(375.8f, 0.65f, -40.5f);
                    TuneLabels(existing);
                    if(IUHCampus.World.IUHReferenceLayout.IsActive)IUHCampus.Editor.IUHReferenceCampusBuilder.ConfigureGreenForReferenceLayout(current);
                    UpdatePortalCard();
                    EditorSceneManager.MarkSceneDirty(scene);
                    return EditorSceneManager.SaveScene(scene);
                }
                throw new InvalidOperationException("Existing Green Campus setup is incomplete; preserve it and inspect its references.");
            }
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets/IUH_Campus/Materials", "GreenCampus");
            var blue = Material("Recycling_Blue", new Color(0.08f, 0.43f, 0.88f), shader);
            var green = Material("Organic_Green", new Color(0.12f, 0.66f, 0.30f), shader);
            var orange = Material("Residual_Orange", new Color(0.94f, 0.48f, 0.08f), shader);
            var dark = Material("Dark_Frame", new Color(0.08f, 0.15f, 0.16f), shader);
            var white = Material("Paper_White", new Color(0.88f, 0.94f, 0.89f), shader);
            var yellow = Material("Banana_Yellow", new Color(1f, 0.80f, 0.10f), shader);
            var brown = Material("Soil_Brown", new Color(0.30f, 0.21f, 0.11f), shader);
            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Setup Green Campus");
            var manager = root.AddComponent<IUHGreenCampusManager>();
            var world = new GameObject("Round_Objects"); world.transform.SetParent(root.transform, false);
            manager.gameWorld = world;
            var rewards = new GameObject("Restored_Green_Corners"); rewards.transform.SetParent(root.transform, false);

            var station = new GameObject("Sorting_Station"); station.transform.SetParent(world.transform, false);
            var binList = new List<IUHGreenBin>();
            var colors = new[] { blue, green, orange };
            for (int i = 0; i < 3; i++)
            {
                var bin = new GameObject("Bin_" + (WasteCategory)i);
                bin.transform.SetParent(station.transform, false);
                bin.transform.position = new Vector3(373f + i * 2.8f, 0.51f, -37.5f);
                var component = bin.AddComponent<IUHGreenBin>(); component.category = (WasteCategory)i;
                Shape(bin.transform, "Body", PrimitiveType.Cube, new Vector3(0, 0.55f, 0), new Vector3(1.15f, 1.1f, 0.95f), colors[i]);
                Shape(bin.transform, "Lid", PrimitiveType.Cube, new Vector3(0, 1.14f, 0), new Vector3(1.25f, 0.16f, 1.05f), dark);
                Shape(bin.transform, "Opening", PrimitiveType.Cube, new Vector3(0, 1.24f, 0), new Vector3(0.65f, 0.06f, 0.52f), dark);
                var collider = bin.AddComponent<BoxCollider>(); collider.center = new Vector3(0, 0.6f, 0); collider.size = new Vector3(1.2f, 1.2f, 1f);
                WorldLabel(bin.transform, "Label", (i + 1) + "  " + component.Label, new Vector3(0, 1.85f, 0), 0.14f, Color.white);
                binList.Add(component);
            }
            manager.bins = binList.ToArray();
            WorldLabel(station.transform, "Station_Title", "TRẠM PHÂN LOẠI • GREEN CAMPUS", new Vector3(375.8f, 3.25f, -37.5f), 0.19f, new Color(0.4f, 1f, 0.6f));

            var itemList = new List<IUHGreenLitter>();
            var zoneList = new List<GreenZoneView>();
            var positions = new[] {
                new Vector3(370,0.65f,-42), new Vector3(375,0.65f,-43), new Vector3(380,0.65f,-42),
                new Vector3(366,0.65f,-46), new Vector3(368,0.65f,-50), new Vector3(371,0.65f,-51),
                new Vector3(380,0.65f,-55), new Vector3(385,0.65f,-55), new Vector3(388,0.65f,-51)
            };
            var zoneNames = new[] { "Sân trung tâm", "Lối đi sân trường", "Góc vườn campus" };
            var litterNames = new[] { "Chai nhựa", "Vỏ chuối", "Vỏ bánh" };
            for (int zoneIndex = 0; zoneIndex < 3; zoneIndex++)
            {
                string zoneId = "green_zone_" + (zoneIndex + 1);
                var zoneRoot = new GameObject(zoneId); zoneRoot.transform.SetParent(world.transform, false);
                Vector3 center = positions[zoneIndex * 3 + 1];
                var dirt = Shape(zoneRoot.transform, "Dirty_Patch", PrimitiveType.Cylinder,
                    new Vector3(center.x, 0.505f, center.z), new Vector3(5.5f, 0.008f, 4f), brown);
                var tree = new GameObject("Restored_" + zoneId); tree.transform.SetParent(rewards.transform, false);
                tree.transform.position = new Vector3(center.x + 1.5f, 0.52f, center.z + 1.7f);
                Shape(tree.transform, "Pot", PrimitiveType.Cylinder, new Vector3(0, 0.25f, 0), new Vector3(0.7f, 0.25f, 0.7f), white);
                Shape(tree.transform, "Trunk", PrimitiveType.Cylinder, new Vector3(0, 0.9f, 0), new Vector3(0.16f, 0.65f, 0.16f), brown);
                Shape(tree.transform, "Leaves", PrimitiveType.Sphere, new Vector3(0, 1.65f, 0), new Vector3(1.5f, 1.4f, 1.5f), green);
                WorldLabel(tree.transform, "Clean_Label", "KHU VỰC ĐÃ SẠCH!", new Vector3(0, 2.8f, 0), 0.14f, new Color(0.4f, 1f, 0.6f));
                tree.SetActive(false);
                zoneList.Add(new GreenZoneView { id = zoneId, displayName = zoneNames[zoneIndex], dirtyVisual = dirt, cleanVisual = tree });
                for (int category = 0; category < 3; category++)
                {
                    var go = new GameObject("Litter_" + zoneIndex + "_" + category);
                    go.transform.SetParent(zoneRoot.transform, false);
                    go.transform.position = positions[zoneIndex * 3 + category];
                    var litter = go.AddComponent<IUHGreenLitter>();
                    litter.itemId = "green_item_" + zoneIndex + "_" + category;
                    litter.zoneId = zoneId; litter.displayName = litterNames[category]; litter.category = (WasteCategory)category;
                    var model = new GameObject("Model"); model.transform.SetParent(go.transform, false); litter.visual = model.transform;
                    if (category == 0)
                    {
                        Shape(model.transform, "Bottle", PrimitiveType.Cylinder, new Vector3(0, 0.25f, 0), new Vector3(0.20f, 0.24f, 0.20f), blue);
                        Shape(model.transform, "Cap", PrimitiveType.Cylinder, new Vector3(0, 0.53f, 0), new Vector3(0.13f, 0.06f, 0.13f), white);
                        Shape(model.transform, "Label", PrimitiveType.Cylinder, new Vector3(0, 0.26f, 0), new Vector3(0.212f, 0.06f, 0.212f), white);
                    }
                    else if (category == 1)
                    {
                        for (int leaf = 0; leaf < 3; leaf++)
                        {
                            var peel = Shape(model.transform, "Peel_" + leaf, PrimitiveType.Capsule,
                                Quaternion.Euler(0, leaf * 120, 0) * new Vector3(0, 0.06f, 0.12f), new Vector3(0.12f, 0.22f, 0.10f), yellow);
                            peel.transform.localRotation = Quaternion.Euler(70, leaf * 120, 0);
                        }
                    }
                    else
                    {
                        var wrapper = Shape(model.transform, "Snack_Wrapper", PrimitiveType.Cube, new Vector3(0, 0.08f, 0), new Vector3(0.5f, 0.13f, 0.30f), orange);
                        wrapper.transform.localRotation = Quaternion.Euler(0, 22, 8);
                        Shape(model.transform, "Stripe", PrimitiveType.Cube, new Vector3(0, 0.16f, 0), new Vector3(0.12f, 0.015f, 0.31f), white);
                    }
                    var box = go.AddComponent<BoxCollider>(); box.center = new Vector3(0, 0.2f, 0); box.size = new Vector3(0.65f, 0.6f, 0.65f); box.isTrigger = true;
                    litter.marker = WorldLabel(go.transform, "Marker", litter.displayName + "  [F]", new Vector3(0, 1.05f, 0), 0.11f, Color.white);
                    itemList.Add(litter);
                }
            }
            manager.items = itemList.ToArray(); manager.zones = zoneList.ToArray();
            manager.ui = IUHGreenCampusUI.Build(root.transform);
            TuneLabels(root);
            UpdatePortalCard();
            world.SetActive(false);
            if(IUHCampus.World.IUHReferenceLayout.IsActive)IUHCampus.Editor.IUHReferenceCampusBuilder.ConfigureGreenForReferenceLayout(manager);
            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            return EditorSceneManager.SaveScene(scene);
        }

        private static Material Material(string name, Color color, Shader shader)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(shader); material.name = name; material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", 0.12f);
            AssetDatabase.CreateAsset(material, path); return material;
        }
        private static void TuneLabels(GameObject root)
        {
            foreach (var text in root.GetComponentsInChildren<TextMesh>(true))
            {
                Undo.RecordObject(text, "Size Green Campus labels");
                text.characterSize = IUHCampus.World.IUHReferenceLayout.IsActive
                    ? (text.name == "Station_Title" ? 0.045f : text.name == "Label" ? 0.04f : 0.035f)
                    : (text.name == "Station_Title" ? 0.085f : text.name == "Label" ? 0.065f : 0.055f);
            }
        }
        private static void UpdatePortalCard()
        {
            var portal = UnityEngine.Object.FindAnyObjectByType<IUHCampus.UI.IUHMiniGamePortalUI>(FindObjectsInactive.Include);
            var card = portal != null ? portal.transform.Find("PortalMenuPanel/CardsArea/Card_4_Game5") : null;
            if (card == null) return;
            var description = card.Find("Desc")?.GetComponent<UnityEngine.UI.Text>();
            if (description != null)
            {
                Undo.RecordObject(description, "Describe Green Campus gameplay");
                description.text = "Dọn 9 món rác, phân loại vào 3 thùng và phục hồi 3 góc xanh trong 4 phút. Chế độ thủ công, lưu điểm trên thiết bị.";
            }
            var action = card.Find("BtnBar/ActionBtn")?.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (action != null)
            {
                Undo.RecordObject(action, "Name Green Campus action");
                action.text = "CHƠI GREEN CAMPUS [ 5 ]";
            }
        }
        private static GameObject Shape(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(primitive); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
        private static GameObject WorldLabel(Transform parent, string name, string text, Vector3 position, float characterSize, Color color)
        {
            var go = new GameObject(name, typeof(TextMesh), typeof(IUHGreenBillboard)); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var mesh = go.GetComponent<TextMesh>(); mesh.text = text; mesh.fontSize = 48; mesh.characterSize = characterSize;
            mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center; mesh.color = color;
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            go.GetComponent<MeshRenderer>().sharedMaterial = mesh.font.material;
            return go;
        }
    }
}
#endif
