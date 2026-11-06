#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IUHCampus.Editor
{
    /// <summary>
    /// Swaps the look of "Campus Student" for the free "Little Guys" character (Luceed Studio, Unity Asset Store).
    /// The student keeps its controller, camera root, overhead HUD and StarterAssets animations: the new
    /// skeleton is placed where the animator expects it and the Animator's avatar is switched to match.
    /// The old StarterAssets mesh and skeleton are only deactivated, so the swap can be reverted.
    /// </summary>
    public static class IUHStudentCharacterSwap
    {
        private const string SCENE_PATH = "Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity";
        private const string LITTLE_GUY_DIR = "Assets/LuceedStudio/Character Lab/Little Guys/Little Guys - Free Sample/Man/";
        private const string LITTLE_GUY_PREFAB = LITTLE_GUY_DIR + "Free Man/Free Man.prefab";
        private const string LITTLE_GUY_AVATAR = LITTLE_GUY_DIR + "Guys_FreeMan_Avatar.asset";
        private const string STARTER_AVATAR_GUID = "36078ab0369161e49a29d349ae3e0739";

        // The Little Guy mesh is about 1.21 m tall; this brings it close to the 1.8 m controller.
        private const float TARGET_SCALE = 1.4f;

        private static readonly string[] OldVisuals = { "Geometry", "Skeleton" };

        [MenuItem("IUH Campus/Characters/Use Little Guy for Campus Student")]
        public static void UseLittleGuy()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LITTLE_GUY_PREFAB);
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(LITTLE_GUY_AVATAR);
            if (prefab == null || avatar == null)
            {
                EditorUtility.DisplayDialog("IUH Campus",
                    "Không tìm thấy gói 'Little Guys - Free Sample'.\nHãy import gói từ Package Manager > My Assets trước.", "OK");
                return;
            }

            GameObject student = FindStudent();
            if (student == null) return;

            var animator = student.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("[IUH Characters] Campus Student has no Animator.");
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(student))
            {
                PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(student),
                    PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            Undo.RegisterFullObjectHierarchyUndo(student, "Use Little Guy for Campus Student");
            RemoveLittleGuyParts(student.transform);

            // Hide the old look but keep it for Revert.
            foreach (string name in OldVisuals)
            {
                Transform t = student.transform.Find(name);
                if (t != null) t.gameObject.SetActive(false);
            }

            // Bring the Little Guy's armature and mesh in at the same relative paths its avatar was built for.
            var temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(temp, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var parts = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in temp.transform) parts.Add(child);
            foreach (Transform part in parts)
            {
                part.SetParent(student.transform, false);
                part.localScale = part.localScale * TARGET_SCALE;
                part.gameObject.layer = student.layer;
                Undo.RegisterCreatedObjectUndo(part.gameObject, "Use Little Guy for Campus Student");
            }
            Object.DestroyImmediate(temp);

            animator.avatar = avatar;
            animator.Rebind();

            EditorUtility.SetDirty(student);
            EditorSceneManager.MarkSceneDirty(student.scene);
            EditorSceneManager.SaveScene(student.scene);
            Selection.activeGameObject = student;
            Debug.Log("[IUH Characters] Campus Student now uses the Little Guy model. Revert: IUH Campus > Characters > Restore StarterAssets look.");
        }

        [MenuItem("IUH Campus/Characters/Restore StarterAssets look")]
        public static void RestoreStarterLook()
        {
            GameObject student = FindStudent();
            if (student == null) return;

            Undo.RegisterFullObjectHierarchyUndo(student, "Restore StarterAssets look");
            RemoveLittleGuyParts(student.transform);
            foreach (string name in OldVisuals)
            {
                Transform t = student.transform.Find(name);
                if (t != null) t.gameObject.SetActive(true);
            }

            var animator = student.GetComponent<Animator>();
            var starterAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(AssetDatabase.GUIDToAssetPath(STARTER_AVATAR_GUID));
            if (animator != null && starterAvatar != null)
            {
                animator.avatar = starterAvatar;
                animator.Rebind();
            }

            EditorSceneManager.MarkSceneDirty(student.scene);
            EditorSceneManager.SaveScene(student.scene);
            Debug.Log("[IUH Characters] Campus Student restored to the StarterAssets look.");
        }

        private static GameObject FindStudent()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != SCENE_PATH && System.IO.File.Exists(SCENE_PATH))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;
                EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            }

            var student = GameObject.Find("Campus Student");
            if (student == null) Debug.LogError("[IUH Characters] Could not find 'Campus Student' in the open scene.");
            return student;
        }

        private static void RemoveLittleGuyParts(Transform student)
        {
            for (int i = student.childCount - 1; i >= 0; i--)
            {
                Transform c = student.GetChild(i);
                if (c.name.StartsWith("Guys_") || c.name == "Combined character")
                {
                    Undo.DestroyObjectImmediate(c.gameObject);
                }
            }
        }
    }
}
#endif
