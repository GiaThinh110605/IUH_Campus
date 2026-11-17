using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IUHCampus.Editor
{
    public static class IUHIOSCommandLineBuild
    {
        public static void EnsureSceneCameras()
        {
            try
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity");
                bool modified = false;

                var student = GameObject.Find("Campus Student");
                if (student != null)
                {
                    var mainCam = GameObject.FindGameObjectWithTag("MainCamera");
                    if (mainCam == null)
                    {
                        var mainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/MainCamera.prefab");
                        if (mainPrefab != null)
                        {
                            var main = (GameObject)PrefabUtility.InstantiatePrefab(mainPrefab);
                            main.name = "MainCamera";
                            modified = true;
                        }
                    }

                    var followCam = Object.FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
                    if (followCam == null)
                    {
                        var followPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/PlayerFollowCamera.prefab");
                        if (followPrefab != null)
                        {
                            var follow = (GameObject)PrefabUtility.InstantiatePrefab(followPrefab);
                            follow.name = "PlayerFollowCamera";
                            var vcam = follow.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                            var controller = student.GetComponent<StarterAssets.ThirdPersonController>();
                            if (vcam != null && controller != null && controller.CinemachineCameraTarget != null)
                            {
                                vcam.Follow = controller.CinemachineCameraTarget.transform;
                            }
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    Debug.Log("[IUH Build] Added missing MainCamera and PlayerFollowCamera to scene.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IUH Build] EnsureSceneCameras warning: {ex.Message}");
            }
        }

        public static void BuildIOS()
        {
            EnsureSceneCameras();

            string[] scenes = new string[] {
                "Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity",
                "Assets/IUH_Campus/Scenes/IUH_CombatArena.unity"
            };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = "Build_iOS";
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[IUH Build] iOS Build succeeded: {summary.totalSize} bytes in {summary.totalTime.TotalSeconds:F1}s");
            }
            else
            {
                Debug.LogError($"[IUH Build] iOS Build failed with {summary.totalErrors} errors.");
                EditorApplication.Exit(1);
            }
        }
    }
}
