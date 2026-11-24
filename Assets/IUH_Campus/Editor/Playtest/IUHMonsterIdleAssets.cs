#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace IUHCampus.Guardian.EditorTools
{
    public static class IUHMonsterIdleAssets
    {
        const string CampusMaterialPath = "Assets/IUH_Campus/Models/Monsters/M_Monster_Atlas.mat";

        public static string Inspect(string path)
        {
            return string.Join("\n", AssetDatabase.LoadAllAssetsAtPath(path).Select(a => a.GetType().Name + ": " + a.name));
        }

        public static GameObject GenerateAssets(string sourcePath, string idlePath, string controllerPath, string prefabPath)
        {
            foreach (var path in new[] { idlePath, controllerPath, prefabPath })
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                    throw new InvalidOperationException("Destination exists: " + path);
            var assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            var sourceIdle = assets.OfType<AnimationClip>().FirstOrDefault(c => c.name.Equals("Idle", StringComparison.OrdinalIgnoreCase));
            if (source == null || sourceIdle == null) throw new InvalidOperationException("Missing imported skinned root or exact Idle: " + sourcePath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(CampusMaterialPath);
            if (material == null || material.shader == null || !material.shader.isSupported || ShaderUtil.ShaderHasError(material.shader))
                throw new InvalidOperationException("Campus monster material shader unsupported or has compile errors.");
            var root = UnityEngine.Object.Instantiate(source);
            try
            {
                PrepareVisualRoot(root, source.name, sourceIdle, material);
                var idle = CreateIdle(sourceIdle, source.name, idlePath);
                var controller = CreateController(idle, controllerPath);
                AssignAnimator(root, controller);
                RefitBounds(root);
                return PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        static void PrepareVisualRoot(GameObject root, string sourceName, AnimationClip sourceIdle, Material material)
        {
            root.name = sourceName + "_GuardianIdle";
            root.SetActive(true);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one;
            foreach (var binding in AnimationUtility.GetCurveBindings(sourceIdle))
                if (!string.IsNullOrEmpty(binding.path) && root.transform.Find(binding.path) == null)
                    throw new InvalidOperationException("Unbound Idle curve path: " + binding.path);
            foreach (var legacy in root.GetComponentsInChildren<Animation>(true)) UnityEngine.Object.DestroyImmediate(legacy);
            foreach (var animator in root.GetComponentsInChildren<Animator>(true)) UnityEngine.Object.DestroyImmediate(animator);
            if (root.GetComponentsInChildren<Collider>(true).Length != 0 || root.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
                throw new InvalidOperationException("Imported root contains gameplay components; inspect before proceeding.");
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                    renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
        }

        static AnimationClip CreateIdle(AnimationClip sourceIdle, string sourceName, string idlePath)
        {
            var idle = UnityEngine.Object.Instantiate(sourceIdle);
            idle.name = sourceName + "_GuardianIdle";
            idle.legacy = false;
            var settings = AnimationUtility.GetAnimationClipSettings(idle);
            settings.loopTime = true;
            settings.loopBlend = true;
            AnimationUtility.SetAnimationClipSettings(idle, settings);
            AssetDatabase.CreateAsset(idle, idlePath);
            AssetDatabase.SaveAssetIfDirty(idle);
            return idle;
        }

        static AnimatorController CreateController(AnimationClip idle, string controllerPath)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            var state = machine.AddState("Idle");
            state.motion = idle;
            machine.defaultState = state;
            EditorUtility.SetDirty(state);
            EditorUtility.SetDirty(machine);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssetIfDirty(controller);
            return controller;
        }

        static void AssignAnimator(GameObject root, RuntimeAnimatorController controller)
        {
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }

        public static Bounds ActualVisualBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true).Where(r => r is MeshRenderer || r is SkinnedMeshRenderer).ToArray();
            if (renderers.Length == 0) throw new InvalidOperationException("No mesh renderers: " + root.name);
            Bounds bounds = default(Bounds);
            bool first = true;
            foreach (var renderer in renderers)
            {
                var skin = renderer as SkinnedMeshRenderer;
                if (skin == null)
                {
                    if (first) { bounds = renderer.bounds; first = false; } else bounds.Encapsulate(renderer.bounds);
                    continue;
                }
                var baked = new Mesh();
                try
                {
                    skin.BakeMesh(baked, true);
                    foreach (var vertex in baked.vertices)
                    {
                        var world = skin.transform.TransformPoint(vertex);
                        if (first) { bounds = new Bounds(world, Vector3.zero); first = false; } else bounds.Encapsulate(world);
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(baked); }
            }
            if (first || bounds.size.y <= .001f) throw new InvalidOperationException("Invalid actual visual height: " + root.name);
            return bounds;
        }

        static void RefitBounds(GameObject root)
        {
            var animator = root.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null) throw new InvalidOperationException("Idle animator missing.");
            var idle = animator.runtimeAnimatorController.animationClips.Single();
            var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var union = new Bounds[skins.Length];
            var mesh = new Mesh();
            try
            {
                for (int sample = 0; sample < 4; sample++)
                {
                    idle.SampleAnimation(root, idle.length * sample * .25f);
                    for (int i = 0; i < skins.Length; i++)
                    {
                        mesh.Clear();
                        skins[i].BakeMesh(mesh, true);
                        mesh.RecalculateBounds();
                        if (sample == 0) union[i] = mesh.bounds; else union[i].Encapsulate(mesh.bounds);
                    }
                }
                for (int i = 0; i < skins.Length; i++)
                {
                    Vector3 minimum = union[i].min, maximum = union[i].max;
                    minimum.x -= .02f; minimum.z -= .02f;
                    maximum += new Vector3(.02f, .02f, .02f);
                    var fitted = new Bounds(); fitted.SetMinMax(minimum, maximum);
                    skins[i].localBounds = fitted;
                    EditorUtility.SetDirty(skins[i]);
                }
            }
            finally { idle.SampleAnimation(root, 0f); UnityEngine.Object.DestroyImmediate(mesh); }
        }

        public static void RefitPrefabBounds(string prefabPath)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Refit only in Edit Mode.");
            var contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try { RefitBounds(contents); PrefabUtility.SaveAsPrefabAsset(contents, prefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        public static void RenormalizeVisual(GameObject oldObjVisual, GameObject newVisual)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Normalize only in Edit Mode.");
            var animator = newVisual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null) throw new InvalidOperationException("New Idle animator missing.");
            animator.runtimeAnimatorController.animationClips.Single().SampleAnimation(newVisual, 0f);
            var target = ActualVisualBounds(oldObjVisual);
            var current = ActualVisualBounds(newVisual);
            float ratio = target.size.y / current.size.y;
            Vector3 origin = newVisual.transform.position;
            Vector3 scaledCenter = origin + (current.center - origin) * ratio;
            float scaledMinimumY = origin.y + (current.min.y - origin.y) * ratio;
            Undo.RecordObject(newVisual.transform, "Normalize actual monster geometry");
            newVisual.transform.localScale *= ratio;
            newVisual.transform.position += new Vector3(target.center.x - scaledCenter.x,
                target.min.y - scaledMinimumY, target.center.z - scaledCenter.z);
            PrefabUtility.RecordPrefabInstancePropertyModifications(newVisual.transform);
        }

        public static GameObject InstallVisual(GameObject oldObjVisual, GameObject idlePrefab)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Install only in Edit Mode.");
            if (oldObjVisual.GetComponentsInChildren<Collider>(true).Length != 0 || oldObjVisual.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
                throw new InvalidOperationException("Old visual contains logic or collider; select only OBJ model child.");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(idlePrefab, oldObjVisual.transform.parent);
            Undo.RegisterCreatedObjectUndo(instance, "Install monster idle visual");
            instance.name = oldObjVisual.name + "_GuardianIdle";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            RenormalizeVisual(oldObjVisual, instance);
            Undo.RecordObject(oldObjVisual, "Hide old static OBJ");
            oldObjVisual.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(oldObjVisual);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            return instance;
        }
    }
}
#endif
