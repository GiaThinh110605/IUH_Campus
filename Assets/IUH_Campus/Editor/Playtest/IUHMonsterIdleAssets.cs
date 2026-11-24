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
                idle.SampleAnimation(root, 0f);
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

        static Bounds VisualBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true).Where(r => r is MeshRenderer || r is SkinnedMeshRenderer).ToArray();
            if (renderers.Length == 0) throw new InvalidOperationException("No mesh renderers: " + root.name);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            if (bounds.size.y <= .001f) throw new InvalidOperationException("Invalid visual height: " + root.name);
            return bounds;
        }

        public static GameObject InstallVisual(GameObject oldObjVisual, GameObject idlePrefab)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Install only in Edit Mode.");
            if (oldObjVisual.GetComponentsInChildren<Collider>(true).Length != 0 || oldObjVisual.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
                throw new InvalidOperationException("Old visual contains logic or collider; select only OBJ model child.");
            var oldBounds = VisualBounds(oldObjVisual);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(idlePrefab, oldObjVisual.transform.parent);
            Undo.RegisterCreatedObjectUndo(instance, "Install monster idle visual");
            instance.name = oldObjVisual.name + "_GuardianIdle";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            var initial = VisualBounds(instance);
            instance.transform.localScale *= oldBounds.size.y / initial.size.y;
            var scaled = VisualBounds(instance);
            instance.transform.position += new Vector3(oldBounds.center.x - scaled.center.x,
                oldBounds.min.y - scaled.min.y, oldBounds.center.z - scaled.center.z);
            var final = VisualBounds(instance);
            if (Mathf.Abs(final.size.y - oldBounds.size.y) > .005f || Mathf.Abs(final.min.y - oldBounds.min.y) > .005f)
            {
                UnityEngine.Object.DestroyImmediate(instance);
                throw new InvalidOperationException("World bounds normalization failed.");
            }
            Undo.RecordObject(oldObjVisual, "Hide old static OBJ");
            oldObjVisual.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(oldObjVisual);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            return instance;
        }
    }
}
#endif
