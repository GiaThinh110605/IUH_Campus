using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using IUHCampus.Core;
using IUHCampus.UI;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.Editor
{
    [InitializeOnLoad]
    public static class IUHGameFlowRefactorSetup
    {
        static IUHGameFlowRefactorSetup()
        {
            EditorApplication.delayCall += EnsureGameFlowSetup;
        }

        [MenuItem("IUH Campus/Refactor Game Flow Setup (One-Click)")]
        public static void EnsureGameFlowSetup()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "IUH_Campus_Main")
            {
                var scenePath = "Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity";
                if (System.IO.File.Exists(scenePath))
                {
                    activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }

            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO == null)
            {
                Debug.LogWarning("[IUHGameFlowRefactorSetup] IUH_Minigame_Canvas not found in active scene.");
                return;
            }

            // 1. Ensure IUHGameFlowManager on Canvas
            var flowMgr = canvasGO.GetComponent<IUHGameFlowManager>();
            if (flowMgr == null)
            {
                flowMgr = canvasGO.AddComponent<IUHGameFlowManager>();
            }

            // 2. Ensure IUHMiniGamePrepUI on Canvas
            var prepUI = canvasGO.GetComponent<IUHMiniGamePrepUI>();
            if (prepUI == null)
            {
                prepUI = canvasGO.AddComponent<IUHMiniGamePrepUI>();
            }

            // 3. Find UI components
            var mainMenuUI = canvasGO.GetComponentInChildren<IUHMainMenuUI>(true);
            var portalUI = canvasGO.GetComponentInChildren<IUHMiniGamePortalUI>(true);
            var customUI = canvasGO.GetComponentInChildren<IUHCustomizationUI>(true);
            var vicDialog = canvasGO.transform.Find("VictoryDialog")?.gameObject;
            var defDialog = canvasGO.transform.Find("DefeatDialog")?.gameObject;

            // 4. Wire properties on IUHGameFlowManager
            SerializedObject so = new SerializedObject(flowMgr);
            so.FindProperty("m_MainMenuUI").objectReferenceValue = mainMenuUI;
            so.FindProperty("m_PortalUI").objectReferenceValue = portalUI;
            so.FindProperty("m_CustomizationUI").objectReferenceValue = customUI;
            so.FindProperty("m_MiniGamePrepUI").objectReferenceValue = prepUI;
            so.FindProperty("m_VictoryDialog").objectReferenceValue = vicDialog;
            so.FindProperty("m_DefeatDialog").objectReferenceValue = defDialog;
            so.FindProperty("m_StartupState").enumValueIndex = (int)IUHGameFlowManager.FlowState.MiniGameSelection;
            so.ApplyModifiedProperties();

            // 5. Ensure all ghosts under GAMEPLAY_GRAB_CAMPUS are inactive by default in the scene
            var gameplayRoot = GameObject.Find("GAMEPLAY_GRAB_CAMPUS");
            if (gameplayRoot != null)
            {
                var ghosts = gameplayRoot.GetComponentsInChildren<IUHGhostHazard>(true);
                foreach (var g in ghosts)
                {
                    g.gameObject.SetActive(false);
                }

                // Ensure DeliveryHUD is inactive by default
                var hud = canvasGO.transform.Find("DeliveryHUD");
                if (hud != null) hud.gameObject.SetActive(false);

                // Ensure CustomizationPanel is inactive by default
                var customPanel = canvasGO.transform.Find("CustomizationPanel");
                if (customPanel != null) customPanel.gameObject.SetActive(false);

                // Ensure Prep Panel is inactive by default
                var prepPanel = canvasGO.transform.Find("MiniGameSetupPanel");
                if (prepPanel != null) prepPanel.gameObject.SetActive(false);

                // Ensure PortalMenuPanel is active (so player sees 6 mission cards on startup)
                var portalPanel = canvasGO.transform.Find("PortalMenuPanel");
                if (portalPanel != null) portalPanel.gameObject.SetActive(true);
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[IUHGameFlowRefactorSetup] Successfully configured and saved IUHGameFlowManager into IUH_Campus_Main scene!");
        }
    }
}
