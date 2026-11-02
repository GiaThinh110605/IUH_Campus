using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Minigames.GrabCampus;
using IUHCampus.UI;

namespace IUHCampus.Core
{

    public class IUHGameFlowManager : MonoBehaviour
    {
        public enum FlowState
        {
            MainMenu,
            MiniGameSelection,
            GameSetup,
            Playing,
            Paused,
            Result
        }

        public static IUHGameFlowManager Instance { get; private set; }

        [Header("Runtime State")]
        [SerializeField] private FlowState m_CurrentState = FlowState.Playing;
        [SerializeField] private int m_ActiveGameIndex = -1;
        [SerializeField] private FlowState m_StartupState = FlowState.Playing;

        [Header("UI Managers")]
        [SerializeField] private IUHMainMenuUI m_MainMenuUI;
        [SerializeField] private IUHMiniGamePortalUI m_PortalUI;
        [SerializeField] private IUHCustomizationUI m_CustomizationUI;
        [SerializeField] private IUHMiniGamePrepUI m_MiniGamePrepUI;

        [Header("Result Dialogs")]
        [SerializeField] private GameObject m_VictoryDialog;
        [SerializeField] private GameObject m_DefeatDialog;

        private float m_LastStateChangeTime = 0f;
        private const float CLICK_COOLDOWN = 0.25f;

        public FlowState CurrentState => m_CurrentState;
        public int ActiveGameIndex => m_ActiveGameIndex;
        public bool IsPlayingGame => m_CurrentState == FlowState.Playing && m_ActiveGameIndex >= 0;
        public bool IsFreeRoam => m_CurrentState == FlowState.Playing && m_ActiveGameIndex == -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            FindReferences();
        }

        private void Start()
        {
            FindReferences();

            DeactivateAllMiniGames();

            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);

            ConfigureResultDialogButtons();

            if (m_StartupState == FlowState.MainMenu)
            {
                GoToMainMenu(true);
            }
            else if (m_StartupState == FlowState.MiniGameSelection)
            {
                GoToMiniGameSelection(true);
            }
            else
            {
                EnterFreeRoam();
            }
        }

        private void FindReferences()
        {
            if (m_MainMenuUI == null) m_MainMenuUI = FindFirstObjectByType<IUHMainMenuUI>(FindObjectsInactive.Include);
            if (m_PortalUI == null) m_PortalUI = FindFirstObjectByType<IUHMiniGamePortalUI>(FindObjectsInactive.Include);
            if (m_CustomizationUI == null) m_CustomizationUI = FindFirstObjectByType<IUHCustomizationUI>(FindObjectsInactive.Include);

            if (m_MiniGamePrepUI == null)
            {
                m_MiniGamePrepUI = FindFirstObjectByType<IUHMiniGamePrepUI>(FindObjectsInactive.Include);
                if (m_MiniGamePrepUI == null)
                {
                    var canvas = GameObject.Find("IUH_Minigame_Canvas");
                    if (canvas != null)
                    {
                        m_MiniGamePrepUI = canvas.AddComponent<IUHMiniGamePrepUI>();
                    }
                }
            }

            var canvasGO = GameObject.Find("IUH_Minigame_Canvas");
            if (canvasGO != null)
            {
                if (m_VictoryDialog == null)
                {
                    var t = canvasGO.transform.Find("VictoryDialog");
                    if (t != null) m_VictoryDialog = t.gameObject;
                }
                if (m_DefeatDialog == null)
                {
                    var t = canvasGO.transform.Find("DefeatDialog");
                    if (t != null) m_DefeatDialog = t.gameObject;
                }
            }
        }

        private bool CanTriggerAction()
        {
            if (Time.unscaledTime - m_LastStateChangeTime < CLICK_COOLDOWN)
            {
                return false;
            }
            m_LastStateChangeTime = Time.unscaledTime;
            return true;
        }

        public void GoToMainMenu(bool force = false)
        {
            if (!force && !CanTriggerAction()) return;

            DeactivateAllMiniGames();
            CloseAllOverlays();

            m_CurrentState = FlowState.MainMenu;
            m_ActiveGameIndex = -1;

            if (m_MainMenuUI != null) m_MainMenuUI.SetOpen(true);
            if (m_PortalUI != null) m_PortalUI.SetMenuOpen(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("[GameFlow] State changed to: MainMenu");
        }

        public void GoToMiniGameSelection(bool force = false)
        {
            if (!force && !CanTriggerAction()) return;

            DeactivateAllMiniGames();
            CloseAllOverlays();

            m_CurrentState = FlowState.MiniGameSelection;
            m_ActiveGameIndex = -1;

            if (m_MainMenuUI != null) m_MainMenuUI.SetOpen(false);
            if (m_PortalUI != null) m_PortalUI.SetMenuOpen(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("[GameFlow] State changed to: MiniGameSelection");
        }

        public void StartGameSetup(int gameIndex)
        {
            if (gameIndex < 0 || gameIndex > 3) return;
            if (!CanTriggerAction()) return;

            DeactivateAllMiniGames();

            if (m_MainMenuUI != null) m_MainMenuUI.SetOpen(false);
            if (m_PortalUI != null) m_PortalUI.SetMenuOpen(false);
            CloseAllOverlays();

            m_CurrentState = FlowState.GameSetup;
            m_ActiveGameIndex = gameIndex;

            if (gameIndex == 0)
            {

                if (m_CustomizationUI != null)
                {
                    m_CustomizationUI.SetOpen(true);
                }
            }
            else if (gameIndex == 1)
            {

                if (m_MiniGamePrepUI != null)
                {
                    m_MiniGamePrepUI.OpenPreparation(1);
                }
            }
            else if (gameIndex == 2)
            {

                var photo = Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
                if (photo != null) photo.OpenBriefing();
                else if (m_MiniGamePrepUI != null) m_MiniGamePrepUI.OpenPreparation(2);
            }
            else if (gameIndex == 3)
            {

                var green = Minigames.GreenCampus.IUHGreenCampusManager.Instance;
                if (green != null) green.OpenBriefing();
                else if (m_MiniGamePrepUI != null) m_MiniGamePrepUI.OpenPreparation(3);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log($"[GameFlow] State changed to: GameSetup for Game #{gameIndex + 1}");
        }

        public void StartPlaying(int gameIndex, bool force = false)
        {
            if (gameIndex < 0 || gameIndex > 3) return;
            if (!force && !CanTriggerAction()) return;

            CloseAllOverlays();
            if (m_MainMenuUI != null) m_MainMenuUI.SetOpen(false);
            if (m_PortalUI != null) m_PortalUI.SetMenuOpen(false);
            if (m_CustomizationUI != null) m_CustomizationUI.SetOpen(false);
            if (m_MiniGamePrepUI != null) m_MiniGamePrepUI.ClosePreparation();

            m_CurrentState = FlowState.Playing;
            m_ActiveGameIndex = gameIndex;

            if (gameIndex == 0)
            {

                PlayerLoadout.Current.SetTheme(PlayerLoadout.Current.theme);

                var grabMgr = IUHGrabCampusManager.Instance;
                if (grabMgr != null)
                {
                    grabMgr.gameObject.SetActive(true);
                    grabMgr.StartMission(0);
                }
            }
            else if (gameIndex == 1)
            {

                var grabMgr = IUHGrabCampusManager.Instance;
                if (grabMgr != null && grabMgr.IsMissionActive) grabMgr.CancelMission();
                if (m_PortalUI != null) m_PortalUI.TeleportToLibrary();
            }
            else if (gameIndex == 2)
            {

                var photo = Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
                if (photo != null) photo.StartMission(true);
            }
            else if (gameIndex == 3)
            {

                var green = Minigames.GreenCampus.IUHGreenCampusManager.Instance;
                if (green != null) green.BeginRound(true);
            }
            else
            {

                var grabMgr = IUHGrabCampusManager.Instance;
                if (grabMgr != null && grabMgr.IsMissionActive)
                {
                    grabMgr.CancelMission();
                }

                if (m_PortalUI != null)
                {
                    switch (gameIndex)
                    {
                        case 1: m_PortalUI.TeleportToKtxDorm(); break;
                        case 2: m_PortalUI.TeleportToLibrary(); break;
                        case 3: m_PortalUI.TeleportToBlockFRoof(); break;
                        case 4: m_PortalUI.TeleportToGreenTrashStation(); break;
                        case 5: m_PortalUI.TeleportToFestivalStage(); break;
                    }
                }
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log($"[GameFlow] State changed to: Playing Game #{gameIndex + 1}");
        }

        public void EnterFreeRoam()
        {
            if (!CanTriggerAction()) return;

            DeactivateAllMiniGames();
            CloseAllOverlays();

            if (m_MainMenuUI != null) m_MainMenuUI.SetOpen(false);
            if (m_PortalUI != null) m_PortalUI.SetMenuOpen(false);

            m_CurrentState = FlowState.Playing;
            m_ActiveGameIndex = -1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("[GameFlow] Entered Free Roam mode.");
        }

        public void PauseGame()
        {
            if (m_CurrentState != FlowState.Playing) return;

            m_CurrentState = FlowState.Paused;
            if (m_PortalUI != null)
            {
                m_PortalUI.SetMenuOpen(true);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("[GameFlow] Game Paused.");
        }

        public void ResumeGame()
        {
            if (m_CurrentState != FlowState.Paused && m_CurrentState != FlowState.Playing) return;

            if (m_PortalUI != null)
            {
                m_PortalUI.SetMenuOpen(false);
            }

            m_CurrentState = FlowState.Playing;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("[GameFlow] Game Resumed.");
        }

        public void RestartCurrentGame()
        {
            if (!CanTriggerAction()) return;

            int targetGame = m_ActiveGameIndex;
            if (targetGame < 0) targetGame = 0;

            DeactivateAllMiniGames();
            CloseAllOverlays();

            if (targetGame == 3) Minigames.PhotoHunter.IUHPhotoHunterManager.Instance?.RestartMission();
            StartPlaying(targetGame, true);
            Debug.Log($"[GameFlow] Restarting current mini-game #{targetGame + 1}");
        }

        public void ShowResult(bool isVictory, string title, string detail)
        {
            m_CurrentState = FlowState.Result;
            CloseAllOverlays();

            if (isVictory)
            {
                if (m_VictoryDialog != null)
                {
                    m_VictoryDialog.SetActive(true);
                    m_VictoryDialog.transform.SetAsLastSibling();
                    var t = m_VictoryDialog.transform.Find("DialogCard/TxtTitle")?.GetComponent<Text>();
                    if (t != null && !string.IsNullOrEmpty(title)) t.text = title;
                }
            }
            else
            {
                if (m_DefeatDialog != null)
                {
                    m_DefeatDialog.SetActive(true);
                    m_DefeatDialog.transform.SetAsLastSibling();
                    var r = m_DefeatDialog.transform.Find("DialogCard/TxtReason")?.GetComponent<Text>();
                    if (r != null && !string.IsNullOrEmpty(detail)) r.text = detail;
                }
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log($"[GameFlow] Showing result: {(isVictory ? "VICTORY" : "DEFEAT")}");
        }

        public void DeactivateAllMiniGames()
        {
            var photo = Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            if (photo != null && photo.HasActiveSessionOrUI) photo.CancelMission(false);
            var green = Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (green != null && green.HasActiveSessionOrUI) green.CancelMission(false);
            var grabMgr = IUHGrabCampusManager.Instance;
            if (grabMgr != null)
            {
                grabMgr.CancelMission();
            }
        }

        private void CloseAllOverlays()
        {
            if (m_CustomizationUI != null) m_CustomizationUI.SetOpen(false);
            if (m_MiniGamePrepUI != null) m_MiniGamePrepUI.ClosePreparation();
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_DefeatDialog != null) m_DefeatDialog.SetActive(false);
        }

        private void ConfigureResultDialogButtons()
        {

            if (m_VictoryDialog != null)
            {
                var card = m_VictoryDialog.transform.Find("DialogCard");
                if (card != null)
                {
                    EnsureResultButtons(card, true);
                }
            }

            if (m_DefeatDialog != null)
            {
                var card = m_DefeatDialog.transform.Find("DialogCard");
                if (card != null)
                {
                    EnsureResultButtons(card, false);
                }
            }
        }

        private void EnsureResultButtons(Transform card, bool isVictory)
        {

            var btnRow = card.Find("ButtonRow");
            if (btnRow == null)
            {

                var rowGO = new GameObject("ButtonRow");
                rowGO.transform.SetParent(card, false);
                var rect = rowGO.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.05f, 0.05f);
                rect.anchorMax = new Vector2(0.95f, 0.20f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 14f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;

                btnRow = rowGO.transform;

                var oldBtn = card.Find(isVictory ? "BtnContinue" : "BtnRetry");
                if (oldBtn != null) oldBtn.gameObject.SetActive(false);

                CreateResultBtn(btnRow, "BtnReplay", "🔄 CHƠI LẠI", new Color(0.2f, 0.65f, 0.35f), () => RestartCurrentGame());

                CreateResultBtn(btnRow, "BtnSelectOther", "📋 CHỌN TRÒ KHÁC", new Color(0.18f, 0.45f, 0.85f), () => GoToMiniGameSelection());

                CreateResultBtn(btnRow, "BtnMainMenu", "🏠 VỀ MENU CHÍNH", new Color(0.35f, 0.38f, 0.45f), () => GoToMainMenu());
            }
        }

        private void CreateResultBtn(Transform parent, string name, string text, Color color, UnityEngine.Events.UnityAction action)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var img = btnGO.AddComponent<Image>();
            img.color = color;
            var btn = btnGO.AddComponent<Button>();
            btn.onClick.AddListener(action);

            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = Color.white * 0.7f;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(btnGO.transform, false);
            var tRect = txtGO.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.fontSize = 15;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
        }

        private void Update()
        {

            if (IUHInputHelper.IsEscapeKeyDown())
            {
                HandleEscapeKey();
                return;
            }

            if (IUHInputHelper.IsMenuKeyDown())
            {
                HandleMenuKey();
                return;
            }
        }

        private void HandleEscapeKey()
        {
            var photo = Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            var green = Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if ((photo != null && (photo.HasActiveModal || photo.CurrentState == Minigames.PhotoHunter.IUHPhotoHunterManager.PhotoHunterState.ViewfinderMode)) ||
                (green != null && green.HasActiveModal) ||
                (m_CustomizationUI != null && m_CustomizationUI.IsOpen) ||
                (m_MiniGamePrepUI != null && m_MiniGamePrepUI.IsOpen)) return;
            switch (m_CurrentState)
            {
                case FlowState.Playing:
                    PauseGame();
                    break;
                case FlowState.Paused:
                    ResumeGame();
                    break;
                case FlowState.GameSetup:
                    GoToMiniGameSelection();
                    break;
                case FlowState.MiniGameSelection:

                    if (m_ActiveGameIndex >= 0) ResumeGame();
                    else EnterFreeRoam();
                    break;
                case FlowState.Result:
                    GoToMiniGameSelection();
                    break;
                case FlowState.MainMenu:

                    break;
            }
        }

        private void HandleMenuKey()
        {
            m_PortalUI?.ToggleMenu();
        }

        public void NotifyGameStarted(int gameIndex)
        {
            m_ActiveGameIndex = gameIndex;
            m_CurrentState = FlowState.Playing;
        }
    }
}
