#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using IUHCampus.Core;
using IUHCampus.UI;

namespace IUHCampus.Editor
{
    [InitializeOnLoad]
    public static class IUHCleanMenus
    {
        private const string CONCEPT_SCENE_PATH = "Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity";

        static IUHCleanMenus()
        {
            EditorApplication.delayCall += ExecuteCleanOnce;
        }

        private static void ExecuteCleanOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            CleanMenusNow();
        }

        [MenuItem("IUH Campus/UI/Clean All Menus from Scene")]
        public static void CleanMenusNow()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            bool needSave = false;

            if (activeScene.path != CONCEPT_SCENE_PATH)
            {
                if (System.IO.File.Exists(CONCEPT_SCENE_PATH))
                {
                    activeScene = EditorSceneManager.OpenScene(CONCEPT_SCENE_PATH, OpenSceneMode.Single);
                }
            }

            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO == null) return;

            string[] menuNames = new string[]
            {
                "PortalMenuPanel",
                "MainMenuPanel",
                "CustomizationPanel",
                "FloatingPortalBtn",
                "MiniGameSetupPanel"
            };

            foreach (var menuName in menuNames)
            {
                var target = canvasGO.transform.Find(menuName);
                if (target != null)
                {
                    Undo.DestroyObjectImmediate(target.gameObject);
                    needSave = true;
                }
            }

            var mobileControls = canvasGO.transform.Find("MobileControls");
            if (mobileControls != null && !mobileControls.gameObject.activeSelf)
            {
                Undo.RecordObject(mobileControls.gameObject, "Activate Mobile Controls");
                mobileControls.gameObject.SetActive(true);
                needSave = true;
            }

            var portalUI = canvasGO.GetComponent<IUHMiniGamePortalUI>();
            if (portalUI != null)
            {
                SerializedObject soPortal = new SerializedObject(portalUI);
                soPortal.FindProperty("m_MenuPanel").objectReferenceValue = null;
                soPortal.FindProperty("m_FloatingButton").objectReferenceValue = null;
                soPortal.FindProperty("m_OpenOnStart").boolValue = false;
                soPortal.ApplyModifiedProperties();
                needSave = true;
            }

            var flowMgr = canvasGO.GetComponent<IUHGameFlowManager>();
            if (flowMgr != null)
            {
                SerializedObject soFlow = new SerializedObject(flowMgr);
                soFlow.FindProperty("m_StartupState").enumValueIndex = (int)IUHGameFlowManager.FlowState.Playing;
                soFlow.FindProperty("m_MainMenuUI").objectReferenceValue = null;
                soFlow.FindProperty("m_CustomizationUI").objectReferenceValue = null;
                soFlow.ApplyModifiedProperties();
                needSave = true;
            }

            if (needSave)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
