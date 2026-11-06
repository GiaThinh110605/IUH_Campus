#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using IUHCampus.Guardian.Match3;

namespace IUHCampus.Editor
{
    /// <summary>
    /// Builds IUH_CombatArena.unity, the "data world" the Match-3 duel is filmed in:
    /// a glowing grid floor, a round stage, data pillars and floating cubes against a dark horizon.
    /// Everything is generated, so the scene can be rebuilt at any time.
    /// </summary>
    public static class IUHCombatArenaSceneBuilder
    {
        public const string SCENE_PATH = "Assets/IUH_Campus/Scenes/IUH_CombatArena.unity";
        private const string ART_DIR = "Assets/IUH_Campus/Materials/CombatArena/";

        // Placed far below the campus so the two scenes never see each other.
        private static readonly Vector3 ArenaOrigin = new Vector3(0f, -2000f, 0f);

        private static readonly Color Navy = new Color(0.03f, 0.04f, 0.12f);
        private static readonly Color Cyan = new Color(0.13f, 0.83f, 0.93f);
        private static readonly Color Purple = new Color(0.66f, 0.33f, 0.97f);
        private static readonly Color Magenta = new Color(0.93f, 0.28f, 0.6f);

        [MenuItem("IUH Campus/Guardian/Build Combat Arena Scene (Data World)")]
        public static void BuildArenaScene()
        {
            if (!Directory.Exists(ART_DIR)) Directory.CreateDirectory(ART_DIR);

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(SCENE_PATH);
            if (existing.isLoaded) EditorSceneManager.CloseScene(existing, true);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);

            BuildContents();

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
            EditorSceneManager.CloseScene(scene, true);

            AddToBuildSettings(SCENE_PATH);
            AssetDatabase.SaveAssets();
            Debug.Log("[IUH Guardian] Combat arena scene built: " + SCENE_PATH);
        }

        private static void BuildContents()
        {
            Texture2D gridTex = MakeTexture("arena_grid.png", 128, 128, true, (x, y) =>
            {
                bool line = x < 3 || y < 3;
                bool dot = x < 6 && y < 6;
                return dot ? Color.white : line ? new Color(0.55f, 0.85f, 1f) : Color.black;
            });
            Texture2D horizonTex = MakeTexture("arena_horizon.png", 4, 256, false, (x, y) =>
            {
                float t = y / 255f;
                Color low = Color.Lerp(Purple, Cyan, 0.25f) * 1.2f;
                Color mid = Navy * 1.5f;
                return t < 0.25f ? Color.Lerp(low, mid, t / 0.25f) : Color.Lerp(mid, Color.black, (t - 0.25f) / 0.75f);
            });

            Material floorMat = MakeLit("ArenaFloor", Navy, Cyan * 1.6f, gridTex, 0.7f);
            floorMat.SetTextureScale("_EmissionMap", new Vector2(60f, 60f));
            floorMat.SetTextureScale("_BaseMap", new Vector2(60f, 60f));
            Material platformMat = MakeLit("ArenaPlatform", new Color(0.08f, 0.1f, 0.24f), Color.black, null, 0.85f);
            Material rimMat = MakeLit("ArenaRim", Purple * 0.5f, Purple * 3f, null, 0.5f);
            Material circuitMat = MakeLit("ArenaCircuit", Cyan * 0.4f, Cyan * 2.5f, null, 0.5f);
            Material pillarMat = MakeLit("ArenaPillar", new Color(0.05f, 0.07f, 0.17f), Color.black, null, 0.6f);
            Material glowCyan = MakeLit("ArenaGlowCyan", Cyan * 0.4f, Cyan * 2.2f, null, 0.4f);
            Material glowPurple = MakeLit("ArenaGlowPurple", Purple * 0.4f, Purple * 2.2f, null, 0.4f);
            Material glowMagenta = MakeLit("ArenaGlowMagenta", Magenta * 0.4f, Magenta * 2f, null, 0.4f);
            Material horizonMat = MakeLit("ArenaHorizon", Color.black, Color.white, horizonTex, 0f);
            Material bitsMat = MakeParticleMaterial("ArenaDataBits");

            var root = new GameObject("COMBAT_ARENA");
            root.transform.position = ArenaOrigin;

            var center = new GameObject("Center").transform;
            center.SetParent(root.transform, false);

            // Floor, stage and its glowing rim.
            Prim(PrimitiveType.Plane, "GridFloor", root.transform, new Vector3(0f, -0.3f, 0f), new Vector3(14f, 1f, 14f), floorMat);
            Prim(PrimitiveType.Cylinder, "StageRim", root.transform, new Vector3(0f, -0.2f, 0f), new Vector3(17f, 0.1f, 17f), rimMat);
            Prim(PrimitiveType.Cylinder, "Stage", root.transform, new Vector3(0f, -0.15f, 0f), new Vector3(16f, 0.15f, 16f), platformMat);

            // A broken "data circle" on the stage.
            var circle = new GameObject("DataCircle").transform;
            circle.SetParent(root.transform, false);
            const int segments = 40;
            for (int i = 0; i < segments; i++)
            {
                if (i % 7 == 3) continue;
                float a = i * Mathf.PI * 2f / segments;
                var seg = Prim(PrimitiveType.Cube, "Seg" + i, circle, new Vector3(Mathf.Cos(a) * 5.6f, 0.01f, Mathf.Sin(a) * 5.6f), new Vector3(0.7f, 0.03f, 0.08f), circuitMat);
                seg.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg + 90f, 0f);
            }

            // Data pillars in an arc behind the stage (the camera looks toward +Z).
            var rng = new System.Random(2026);
            var pillars = new GameObject("DataPillars").transform;
            pillars.SetParent(root.transform, false);
            Material[] glows = { glowCyan, glowPurple, glowCyan, glowMagenta };
            for (int i = 0; i < 18; i++)
            {
                float angle = Mathf.Lerp(-78f, 78f, i / 17f) * Mathf.Deg2Rad;
                float radius = 20f + (float)rng.NextDouble() * 12f;
                float height = 4f + (float)rng.NextDouble() * 12f;
                float width = 1.2f + (float)rng.NextDouble() * 1.6f;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * radius, height * 0.5f - 0.3f, Mathf.Cos(angle) * radius);
                Material glow = glows[rng.Next(glows.Length)];

                var p = Prim(PrimitiveType.Cube, "Pillar" + i, pillars, pos, new Vector3(width, height, width), pillarMat);
                p.transform.localRotation = Quaternion.LookRotation(-new Vector3(pos.x, 0f, pos.z));
                var cap = Prim(PrimitiveType.Cube, "Cap", p.transform, new Vector3(0f, 0.5f, 0f), new Vector3(1.04f, 0.03f, 1.04f), glow);
                cap.transform.localScale = new Vector3(1.04f, 0.15f / height, 1.04f);
                var strip = Prim(PrimitiveType.Cube, "Strip", p.transform, new Vector3(0f, 0f, 0.51f), new Vector3(0.06f, 0.85f, 0.02f), glow);
                strip.transform.localPosition = new Vector3(((float)rng.NextDouble() - 0.5f) * 0.6f, -0.05f, 0.51f);
            }

            // Floating data cubes.
            var cubes = new GameObject("FloatingData").transform;
            cubes.SetParent(root.transform, false);
            for (int i = 0; i < 26; i++)
            {
                Vector3 pos = new Vector3(((float)rng.NextDouble() - 0.5f) * 64f, 3f + (float)rng.NextDouble() * 14f, 12f + (float)rng.NextDouble() * 28f);
                float size = 0.3f + (float)rng.NextDouble() * 0.9f;
                var cube = Prim(PrimitiveType.Cube, "Bit" + i, cubes, pos, Vector3.one * size, glows[rng.Next(glows.Length)]);
                cube.transform.localRotation = Quaternion.Euler(rng.Next(360), rng.Next(360), rng.Next(360));
                cube.AddComponent<IUHDataSpin>();
            }

            // Horizon glow far behind everything.
            Prim(PrimitiveType.Cube, "Horizon", root.transform, new Vector3(0f, 28f, 70f), new Vector3(220f, 60f, 1f), horizonMat);

            BuildDataBits(root.transform, bitsMat);
            BuildLights(root.transform);
            Camera cam = BuildCamera(root.transform);
            BuildVolume(root.transform);

            var anchors = root.AddComponent<Match3ArenaAnchors>();
            var so = new SerializedObject(anchors);
            so.FindProperty("m_Center").objectReferenceValue = center;
            so.FindProperty("m_Camera").objectReferenceValue = cam;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildDataBits(Transform parent, Material mat)
        {
            var go = new GameObject("DataBits", typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, -0.3f, 12f);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(Cyan, Purple);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 500;

            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(60f, 30f, 0.5f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mat;
        }

        private static void BuildLights(Transform parent)
        {
            AddPointLight(parent, "StageGlow", new Vector3(0f, 1.2f, -4f), Purple, 8f, 18f);
            AddPointLight(parent, "BackRim", new Vector3(0f, 7f, 9f), Cyan, 14f, 32f);
            AddPointLight(parent, "FillLeft", new Vector3(-9f, 4f, -6f), new Color(0.75f, 0.85f, 1f), 6f, 20f);
            AddPointLight(parent, "FillRight", new Vector3(9f, 4f, -6f), new Color(0.95f, 0.75f, 1f), 6f, 20f);
        }

        private static void AddPointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
        {
            var go = new GameObject(name, typeof(Light));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var l = go.GetComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.None;
        }

        private static Camera BuildCamera(Transform parent)
        {
            var go = new GameObject("ArenaCamera", typeof(Camera));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 3f, -16f);
            var cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.09f);
            cam.fieldOfView = 40f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 400f;
            cam.enabled = false;

            var data = go.GetComponent<UniversalAdditionalCameraData>();
            if (data == null) data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            return cam;
        }

        private static void BuildVolume(Transform parent)
        {
            string path = ART_DIR + "ArenaVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);

                var bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(1.3f);
                bloom.threshold.Override(0.85f);
                bloom.scatter.Override(0.7f);
                AssetDatabase.AddObjectToAsset(bloom, profile);

                var vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(0.32f);
                AssetDatabase.AddObjectToAsset(vignette, profile);

                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }

            var go = new GameObject("ArenaVolume", typeof(Volume));
            go.transform.SetParent(parent, false);
            var volume = go.GetComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;
        }

        // ── Helpers ──

        private static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private static Material MakeLit(string name, Color baseColor, Color emission, Texture2D emissionMap, float smoothness)
        {
            string path = ART_DIR + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Glossiness", smoothness);
            if (emission.maxColorComponent > 0.001f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                if (emissionMap != null) mat.SetTexture("_EmissionMap", emissionMap);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material MakeParticleMaterial(string name)
        {
            string path = ART_DIR + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", Color.white * 2f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Texture2D MakeTexture(string fileName, int w, int h, bool repeat, System.Func<int, int, Color> pixel)
        {
            string path = ART_DIR + fileName;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, pixel(x, y));
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
            {
                if (s.path == path) { s.enabled = true; EditorBuildSettings.scenes = scenes.ToArray(); return; }
            }
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
