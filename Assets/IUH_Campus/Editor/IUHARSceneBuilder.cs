#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Guardian;
using IUHCampus.Guardian.AR;
using IUHCampus.Guardian.Match3;
using IUHCampus.Minigames.GreenCampus;

namespace IUHCampus.Editor
{
    public static class IUHARSceneBuilder
    {
        private const string AR_SCENE_PATH = "Assets/IUH_Campus/Scenes/IUH_Campus_AR_Encounter.unity";
        private const string TEXTURE_DIR = "Assets/IUH_Campus/Textures/Guardian/";
        private const string MONSTER_MODEL_PATH = "Assets/IUH_Campus/Models/Monsters/BlueDemon.obj";
        private const string MONSTER_MAT_PATH = "Assets/IUH_Campus/Models/Monsters/M_Monster_Atlas.mat";

        [MenuItem("IUH Campus/AR/Build AR iPhone Encounter Scene")]
        public static void BuildAREncounterScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera & Lighting
            GameObject camObj = new GameObject("AR Main Camera", typeof(Camera), typeof(AudioListener), typeof(IUHCameraShake));
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 1.4f, -2.5f);
            camObj.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            var cam = camObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            GameObject dirLight = new GameObject("Directional Light", typeof(Light));
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var dl = dirLight.GetComponent<Light>();
            dl.type = LightType.Directional;
            dl.intensity = 1.2f;
            dl.color = new Color(1f, 0.96f, 0.9f);

            // 2. Editor Simulated Floor (Subtle grid)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Editor_Simulated_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            Material floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Main_Stone.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Asphalt_Road.mat");
            if (floorMat != null) floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 3. Hologram Reticle
            GameObject reticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reticle.name = "AR_Hologram_Reticle";
            reticle.transform.localScale = new Vector3(1.6f, 0.015f, 1.6f);
            Material holoMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Waypoint_Holo.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Podium_Turquoise.mat");
            if (holoMat != null) reticle.GetComponent<Renderer>().sharedMaterial = holoMat;
            foreach (var c in reticle.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);

            // Reticle pulse light
            GameObject reticleLight = new GameObject("ReticleLight", typeof(Light));
            reticleLight.transform.SetParent(reticle.transform, false);
            reticleLight.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            var rl = reticleLight.GetComponent<Light>();
            rl.type = LightType.Point;
            rl.color = new Color(0.15f, 0.95f, 1f);
            rl.intensity = 2.5f;
            rl.range = 3.5f;

            // 4. AR Canvas & UI
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            GameObject canvasGO = new GameObject("AR_Encounter_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var cComp = canvasGO.GetComponent<Canvas>();
            cComp.renderMode = RenderMode.ScreenSpaceOverlay;
            var cs = canvasGO.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920); // Portrait for iPhone
            cs.matchWidthOrHeight = 0.5f;

            // Header instruction banner
            GameObject bannerObj = new GameObject("HeaderBanner", typeof(RectTransform), typeof(Image));
            bannerObj.transform.SetParent(canvasGO.transform, false);
            var bnRt = bannerObj.GetComponent<RectTransform>();
            bnRt.anchorMin = new Vector2(0.5f, 0.93f);
            bnRt.anchorMax = new Vector2(0.5f, 0.93f);
            bnRt.sizeDelta = new Vector2(900f, 80f);
            bannerObj.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.12f, 0.88f);

            GameObject bannerTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            bannerTxtGo.transform.SetParent(bannerObj.transform, false);
            var bnTxtRt = bannerTxtGo.GetComponent<RectTransform>();
            bnTxtRt.anchorMin = Vector2.zero; bnTxtRt.anchorMax = Vector2.one; bnTxtRt.sizeDelta = Vector2.zero;
            var bnTxt = bannerTxtGo.GetComponent<Text>();
            bnTxt.font = font;
            bnTxt.fontSize = 28;
            bnTxt.fontStyle = FontStyle.Bold;
            bnTxt.alignment = TextAnchor.MiddleCenter;
            bnTxt.color = new Color(0.35f, 0.95f, 1f);
            bnTxt.text = "📱 <b>HÃY QUÉT CAMERA VÀO SÀN NHÀ / MẶT ĐẤT</b>";

            // 5. Corrupted Object Spawner Root
            GameObject spawnerRoot = new GameObject("AR_CORRUPTED_ENCOUNTER");
            spawnerRoot.transform.position = new Vector3(0f, 0f, 1.8f);

            // Corrupted Backpack Model
            GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backpack.name = "CorruptedBackpack";
            backpack.transform.SetParent(spawnerRoot.transform, false);
            backpack.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            backpack.transform.localScale = new Vector3(0.55f, 0.65f, 0.35f);
            Material bpMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/SharedCampusConcept/Materials/Student jacket.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/Materials/M_IUH_Student_BluePolo.mat");
            if (bpMat != null) backpack.GetComponent<Renderer>().sharedMaterial = bpMat;

            // Dark Purple Smoke Particle System
            GameObject smokeGO = new GameObject("PurpleSmoke", typeof(ParticleSystem));
            smokeGO.transform.SetParent(spawnerRoot.transform, false);
            smokeGO.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            var ps = smokeGO.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.8f;
            main.startSpeed = 0.8f;
            main.startSize = 0.6f;
            main.maxParticles = 50;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.6f, 0.1f, 0.9f, 0.85f),
                new Color(0.85f, 0.2f, 1f, 0.95f)
            );
            var emission = ps.emission;
            emission.rateOverTime = 22f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.25f;

            // Point Light Aura
            GameObject auraLightGO = new GameObject("PurpleAuraLight", typeof(Light));
            auraLightGO.transform.SetParent(spawnerRoot.transform, false);
            auraLightGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var auraLight = auraLightGO.GetComponent<Light>();
            auraLight.type = LightType.Point;
            auraLight.color = new Color(0.75f, 0.15f, 1f);
            auraLight.intensity = 4.0f;
            auraLight.range = 5.0f;

            // 3D Monster Instance
            GameObject monsterRoot = new GameObject("AR_Monster");
            monsterRoot.transform.SetParent(spawnerRoot.transform, false);
            monsterRoot.transform.localPosition = new Vector3(0f, 0f, 0.6f);
            GameObject monsterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MONSTER_MODEL_PATH);
            if (monsterPrefab != null)
            {
                var monsterInst = (GameObject)PrefabUtility.InstantiatePrefab(monsterPrefab, monsterRoot.transform);
                monsterInst.transform.localPosition = Vector3.zero;
                monsterInst.transform.localScale = Vector3.one * 1.35f;
                Material monsterMat = AssetDatabase.LoadAssetAtPath<Material>(MONSTER_MAT_PATH);
                if (monsterMat != null)
                {
                    foreach (var r in monsterInst.GetComponentsInChildren<Renderer>()) r.sharedMaterial = monsterMat;
                }
            }
            var monsterCol = monsterRoot.AddComponent<CapsuleCollider>();
            monsterCol.center = new Vector3(0f, 0.8f, 0f);
            monsterCol.radius = 0.6f;
            monsterCol.height = 1.8f;

            // 3D Billboard Tap Prompt
            GameObject tapPrompt = new GameObject("PROMPT_TAP", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(IUHGreenBillboard));
            tapPrompt.transform.SetParent(monsterRoot.transform, false);
            tapPrompt.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            var tc = tapPrompt.GetComponent<Canvas>();
            tc.renderMode = RenderMode.WorldSpace;
            var trt = tapPrompt.GetComponent<RectTransform>();
            trt.sizeDelta = new Vector2(300f, 80f);
            trt.localScale = Vector3.one * 0.007f;

            GameObject tpCard = new GameObject("Card", typeof(RectTransform), typeof(Image));
            tpCard.transform.SetParent(tapPrompt.transform, false);
            var tpCardRt = tpCard.GetComponent<RectTransform>();
            tpCardRt.anchorMin = Vector2.zero; tpCardRt.anchorMax = Vector2.one; tpCardRt.sizeDelta = Vector2.zero;
            tpCard.GetComponent<Image>().color = new Color(0.06f, 0.09f, 0.16f, 0.92f);

            var tpText = CreateText(tpCard.transform, "Text", "⚔️ <b>CHẠM ĐỂ THANH TẨY!</b>\n<size=12><color=#FDE047>[MATCH-3 COMBAT]</color></size>", 15, new Vector2(0.5f, 0.5f), new Vector2(280f, 60f), font, new Color(0.3f, 0.95f, 1f));
            tpText.alignment = TextAnchor.MiddleCenter;

            // Add Audio, Shake, and Combat UI under Canvas
            GameObject audioObj = new GameObject("IUH_Combat_Audio", typeof(IUHCombatAudio));
            audioObj.transform.SetParent(canvasGO.transform, false);

            GameObject invObj = new GameObject("IUH_Guardian_Inventory", typeof(IUHGuardianInventory));
            invObj.transform.SetParent(canvasGO.transform, false);

            // Wire Reticle
            var reticleComp = camObj.AddComponent<IUHARPlaneReticle>();
            var encComp = spawnerRoot.AddComponent<IUHARCorruptedObjectEncounter>();

            SerializedObject soReticle = new SerializedObject(reticleComp);
            soReticle.FindProperty("m_ReticleObject").objectReferenceValue = reticle;
            soReticle.FindProperty("m_StatusInstructionText").objectReferenceValue = bnTxt;
            soReticle.FindProperty("m_EncounterSpawner").objectReferenceValue = encComp;
            soReticle.ApplyModifiedProperties();

            // Wire Encounter
            SerializedObject soEnc = new SerializedObject(encComp);
            soEnc.FindProperty("m_CorruptedBackpack").objectReferenceValue = backpack;
            soEnc.FindProperty("m_MonsterObject").objectReferenceValue = monsterRoot;
            soEnc.FindProperty("m_PurpleSmokeParticles").objectReferenceValue = ps;
            soEnc.FindProperty("m_PurpleAuraLight").objectReferenceValue = auraLight;
            soEnc.FindProperty("m_ReticleObject").objectReferenceValue = reticle;
            soEnc.FindProperty("m_TapPromptCanvas").objectReferenceValue = tapPrompt;
            soEnc.ApplyModifiedProperties();

            // Save Scene
            EditorSceneManager.SaveScene(scene, AR_SCENE_PATH);
            Debug.Log("[IUH AR] AR Encounter scene built successfully at: " + AR_SCENE_PATH);

            // Add to Build Settings
            var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in buildScenes) if (s.path == AR_SCENE_PATH) { found = true; break; }
            if (!found)
            {
                buildScenes.Insert(0, new EditorBuildSettingsScene(AR_SCENE_PATH, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 anchor, Vector2 size, Font font, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = size;
            var t = go.GetComponent<Text>();
            t.text = text; t.fontSize = fontSize; t.font = font; t.alignment = TextAnchor.MiddleCenter; t.color = color; t.raycastTarget = false;
            return t;
        }
    }
}
#endif
