using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.UI
{
    /// <summary>
    /// In-Game Mini-Game Selection Portal & Teleportation Menu (IUH Campus).
    /// Fully supports both New Input System and Legacy Input.
    /// Toggle via [M], select game via keys [1] - [6] or mouse clicks.
    /// </summary>
    public class IUHMiniGamePortalUI : MonoBehaviour
    {
        public static IUHMiniGamePortalUI Instance { get; private set; }

        [Header("Menu Activation")]
        [SerializeField] private bool m_OpenOnStart = false;
        [SerializeField] private KeyCode m_ToggleKey = KeyCode.M;
        [SerializeField] private GameObject m_MenuPanel;
        [SerializeField] private GameObject m_FloatingButton;

        [Header("Player & Controller")]
        [SerializeField] private GameObject m_PlayerObject;
        [SerializeField] private MonoBehaviour m_PlayerMovementScript;

        [Header("Fast Travel Waypoint Coordinates")]
        [SerializeField] private Vector3 m_Gate1ParkingPos = new Vector3(398.0f, 0.55f, -18.0f);
        [SerializeField] private Vector3 m_KtxDormPos = new Vector3(330.0f, 0.55f, -78.0f);
        [SerializeField] private Vector3 m_LibraryPos = new Vector3(410.0f, 0.55f, -28.0f);
        [SerializeField] private Vector3 m_BlockFRoofPos = new Vector3(362.0f, 1.9f, -53.5f);
        [SerializeField] private Vector3 m_CentralCourtyardPos = new Vector3(375.0f, 0.55f, -50.0f);
        [SerializeField] private Vector3 m_FestivalStagePos = new Vector3(365.0f, 0.55f, -65.0f);

        private bool m_IsOpen = false;
        private AudioSource m_AudioSource;

        public bool IsOpen => m_IsOpen;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;

            if (IUHCampus.Core.IUHGameFlowManager.Instance == null)
            {
                var flowMgr = FindFirstObjectByType<IUHCampus.Core.IUHGameFlowManager>(FindObjectsInactive.Include);
                if (flowMgr == null)
                {
                    gameObject.AddComponent<IUHCampus.Core.IUHGameFlowManager>();
                }
            }

            FindPlayerReferences();
            ConfigureReferenceDestinations();
        }

        public void ConfigureReferenceDestinations()
        {
            if(!IUHCampus.World.IUHReferenceLayout.IsActive)return;
            m_Gate1ParkingPos=IUHCampus.World.IUHReferenceLayout.Entrance;
            m_KtxDormPos=IUHCampus.World.IUHReferenceLayout.DormEntrance;
            m_LibraryPos=IUHCampus.World.IUHReferenceLayout.LibraryEntrance;
            m_BlockFRoofPos=IUHCampus.World.IUHReferenceLayout.FRoof;
            m_CentralCourtyardPos=IUHCampus.World.IUHReferenceLayout.GreenSpawn;
        }

        private void Start()
        {
            AutoWireButtons();
            if (IUHGameFlowManager.Instance == null) SetMenuOpen(m_OpenOnStart);

        }

        private void FindPlayerReferences()
        {
            if (m_PlayerObject == null)
            {
                m_PlayerObject = GameObject.FindGameObjectWithTag("Player");
                if (m_PlayerObject == null) m_PlayerObject = GameObject.Find("PlayerArmature");
            }

            if (m_PlayerObject != null && m_PlayerMovementScript == null)
            {
                m_PlayerMovementScript = m_PlayerObject.GetComponent("ThirdPersonController") as MonoBehaviour;
            }
        }

        private void AutoWireButtons()
        {
            // Floating Button
            if (m_FloatingButton != null)
            {
                var btn = m_FloatingButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(ToggleMenu);
                }
            }

            if (m_MenuPanel != null)
            {
                // Close Button
                var closeBtn = m_MenuPanel.transform.Find("CloseBtn")?.GetComponent<Button>();
                if (closeBtn != null)
                {
                    closeBtn.onClick.RemoveAllListeners();
                    closeBtn.onClick.AddListener(ToggleMenu);
                }

                var btnResume = m_MenuPanel.transform.Find("Footer/BtnResume")?.GetComponent<Button>();
                if (btnResume != null)
                {
                    btnResume.onClick.RemoveAllListeners();
                    btnResume.onClick.AddListener(ToggleMenu);
                }

                var btnFreeRoam = m_MenuPanel.transform.Find("Footer/BtnFreeRoam")?.GetComponent<Button>();
                if (btnFreeRoam != null)
                {
                    btnFreeRoam.onClick.RemoveAllListeners();
                    btnFreeRoam.onClick.AddListener(() => {
                        var grabMgr = IUHGrabCampusManager.Instance;
                        if (grabMgr != null && grabMgr.IsMissionActive) grabMgr.CancelMission();

                        var photoMgr = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
                        if (photoMgr != null && photoMgr.HasActiveSessionOrUI) photoMgr.CancelMission(restoreCursorAndController: false);

                        var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
                        if (greenMgr != null && greenMgr.HasActiveSessionOrUI) greenMgr.CancelMission(false);

                        if (IUHCampus.Core.IUHGameFlowManager.Instance != null)
                        {
                            IUHCampus.Core.IUHGameFlowManager.Instance.EnterFreeRoam();
                        }
                        else
                        {
                            SetMenuOpen(false);
                        }
                    });
                }

                // Customization Wardrobe Buttons
                var btnCustPortal = m_MenuPanel.transform.Find("BtnCustomizationPortal")?.GetComponent<Button>();
                if (btnCustPortal != null)
                {
                    btnCustPortal.onClick.RemoveAllListeners();
                    btnCustPortal.onClick.AddListener(OpenCustomization);
                }

                // Cards Area Buttons
                var cardsArea = m_MenuPanel.transform.Find("CardsArea");
                if (cardsArea != null)
                {
                    for (int i = 0; i < cardsArea.childCount; i++)
                    {
                        var card = cardsArea.GetChild(i);
                        // Make entire card clickable
                        var cardBtn = card.GetComponent<Button>();
                        int gameIndex=i;
                        var nameParts=card.name.Split('_');
                        if(nameParts.Length>1&&int.TryParse(nameParts[1],out int parsed))gameIndex=parsed;
                        bool available = gameIndex >= 0 && gameIndex < 4;
                        if (cardBtn != null)
                        {
                            cardBtn.interactable=available;
                            int index = gameIndex;
                            cardBtn.onClick.RemoveAllListeners();
                            cardBtn.onClick.AddListener(() => SelectMiniGame(index));
                        }

                        // Also action button
                        var actionBtn = card.Find("BtnBar/ActionBtn")?.GetComponent<Button>();
                        if (actionBtn != null)
                        {
                            actionBtn.interactable=available;
                            int index = gameIndex;
                            actionBtn.onClick.RemoveAllListeners();
                            actionBtn.onClick.AddListener(() => SelectMiniGame(index));
                            if(!available)
                            {
                                var label=actionBtn.GetComponentInChildren<Text>();
                                if(label!=null)label.text="ĐANG PHÁT TRIỂN";
                            }
                        }
                    }
                }
                var dorm=m_MenuPanel.transform.Find("Footer/BtnDorm")?.GetComponent<Button>();
                if(dorm!=null){dorm.onClick.RemoveAllListeners();dorm.onClick.AddListener(TeleportToKtxDorm);}
                var library=m_MenuPanel.transform.Find("Footer/BtnLibrary")?.GetComponent<Button>();
                if(library!=null){library.onClick.RemoveAllListeners();library.onClick.AddListener(TeleportToLibrary);}
            }

            // HUD Wardrobe button
            var hudPanel = transform.Find("DeliveryHUD");
            if (hudPanel != null)
            {
                var btnCustHUD = hudPanel.Find("BtnCustomizationHUD")?.GetComponent<Button>();
                if (btnCustHUD != null)
                {
                    btnCustHUD.onClick.RemoveAllListeners();
                    btnCustHUD.onClick.AddListener(OpenCustomization);
                }
            }

            // Results Dialogs buttons
            var victoryDialog = transform.Find("VictoryDialog");
            if (victoryDialog != null)
            {
                var btnCont = victoryDialog.Find("DialogCard/BtnContinue")?.GetComponent<Button>();
                if (btnCont != null)
                {
                    btnCont.onClick.RemoveAllListeners();
                    btnCont.onClick.AddListener(() => {
                        victoryDialog.gameObject.SetActive(false);
                        SetMenuOpen(true);
                    });
                }
            }

            var defeatDialog = transform.Find("DefeatDialog");
            if (defeatDialog != null)
            {
                var btnRetry = defeatDialog.Find("DialogCard/BtnRetry")?.GetComponent<Button>();
                if (btnRetry != null)
                {
                    btnRetry.onClick.RemoveAllListeners();
                    btnRetry.onClick.AddListener(() => {
                        defeatDialog.gameObject.SetActive(false);
                        var mgr = IUHGrabCampusManager.Instance;
                        if (mgr != null) mgr.RetryCurrentMission();
                    });
                }
            }
        }

        private void Update()
        {
            // Toggle Menu with M key
            if (IUHGameFlowManager.Instance == null && IsKeyTriggered(KeyCode.M))
            {
                ToggleMenu();
                return;
            }

            // When menu is open, handle keys [1] - [6] and [ESC]
            if (m_IsOpen)
            {
                if (IUHGameFlowManager.Instance == null && IsKeyTriggered(KeyCode.Escape))
                {
                    SetMenuOpen(false);
                    return;
                }

                if (IsKeyTriggered(KeyCode.Alpha1) || IsKeyTriggered(KeyCode.Keypad1))
                {
                    SelectMiniGame(0);
                }
                else if (IsKeyTriggered(KeyCode.Alpha2) || IsKeyTriggered(KeyCode.Keypad2))
                {
                    SelectMiniGame(1);
                }
                else if (IsKeyTriggered(KeyCode.Alpha3) || IsKeyTriggered(KeyCode.Keypad3))
                {
                    SelectMiniGame(2);
                }
                else if (IsKeyTriggered(KeyCode.Alpha4) || IsKeyTriggered(KeyCode.Keypad4))
                {
                    SelectMiniGame(3);
                }
            }
            else
            {
                // When in gameplay, pressing ESC also opens the menu
                if (IUHGameFlowManager.Instance == null && IsKeyTriggered(KeyCode.Escape))
                {
                    SetMenuOpen(true);
                    return;
                }
            }

            // Press [U] anytime to open Customization Wardrobe
            if (IsKeyTriggered(KeyCode.U))
            {
                OpenCustomization();
                return;
            }
        }

        public void OpenCustomization()
        {
            var customUI = IUHCustomizationUI.Instance;
            if (customUI != null)
            {
                bool wasMenuOpen = m_IsOpen;
                if (m_IsOpen) SetMenuOpen(false);
                customUI.ReturnToPortalMenuOnClose = wasMenuOpen;
                customUI.SetOpen(true);
            }
        }

        private bool IsKeyTriggered(KeyCode code)
        {
            switch (code)
            {
                case KeyCode.M: return IUHInputHelper.IsMenuKeyDown();
                case KeyCode.Escape: return IUHInputHelper.IsEscapeKeyDown();
                case KeyCode.U: return IUHInputHelper.IsCustomizationKeyDown();
                case KeyCode.Alpha1:
                case KeyCode.Keypad1: return IUHInputHelper.IsNumberKeyDown(1);
                case KeyCode.Alpha2:
                case KeyCode.Keypad2: return IUHInputHelper.IsNumberKeyDown(2);
                case KeyCode.Alpha3:
                case KeyCode.Keypad3: return IUHInputHelper.IsNumberKeyDown(3);
                case KeyCode.Alpha4:
                case KeyCode.Keypad4: return IUHInputHelper.IsNumberKeyDown(4);
                case KeyCode.Alpha5:
                case KeyCode.Keypad5: return IUHInputHelper.IsNumberKeyDown(5);
                case KeyCode.Alpha6:
                case KeyCode.Keypad6: return IUHInputHelper.IsNumberKeyDown(6);
                default: return false;
            }
        }

        public void ToggleMenu()
        {
            var flow = IUHGameFlowManager.Instance;
            if (flow != null)
            {
                if (m_IsOpen)
                {
                    if (flow.CurrentState == IUHGameFlowManager.FlowState.Paused || flow.IsPlayingGame) flow.ResumeGame();
                    else flow.EnterFreeRoam();
                }
                else if (flow.CurrentState == IUHGameFlowManager.FlowState.Playing) flow.PauseGame();
                else flow.GoToMiniGameSelection();
                return;
            }
            SetMenuOpen(!m_IsOpen);
        }

        public void SetMenuOpen(bool open)
        {
            m_IsOpen = open;

            if (m_MenuPanel != null)
            {
                m_MenuPanel.SetActive(m_IsOpen);
                if (m_IsOpen) m_MenuPanel.transform.SetAsLastSibling();
            }

            if (m_FloatingButton != null)
            {
                m_FloatingButton.SetActive(!m_IsOpen);
                if (!m_IsOpen) m_FloatingButton.transform.SetAsLastSibling();
            }

            // Hide mobile controls on PC or while menu is open
            var mobile = FindFirstObjectByType<IUHMobileControlsUI>(FindObjectsInactive.Include);
            if (mobile != null)
            {
                mobile.CheckPlatformVisibility();
                if (m_IsOpen) mobile.gameObject.SetActive(false);
            }

            // Mouse Cursor & Camera Control
            if (m_IsOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (m_PlayerMovementScript != null) m_PlayerMovementScript.enabled = false;
                if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetClickSound(), 0.8f);
            }
            else
            {
                // Only lock cursor and enable controller back if other dialogs/modals are not open
                bool otherDialogOpen = false;
                var victory = transform.Find("VictoryDialog");
                if (victory != null && victory.gameObject.activeSelf) otherDialogOpen = true;
                var defeat = transform.Find("DefeatDialog");
                if (defeat != null && defeat.gameObject.activeSelf) otherDialogOpen = true;
                var delivery = transform.Find("DeliveryDialogueModal");
                if (delivery != null && delivery.gameObject.activeSelf) otherDialogOpen = true;
                var custom = transform.Find("CustomizationPanel");
                if (custom != null && custom.gameObject.activeSelf) otherDialogOpen = true;

                // Photo Hunter Modal Check (Briefing or Victory gallery dialog)
                var photoMgr = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
                if (photoMgr != null && photoMgr.HasActiveModal)
                {
                    otherDialogOpen = true;
                }

                // Wardrobe / Customization UI Check
                var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
                if (greenMgr != null && greenMgr.HasActiveModal) otherDialogOpen = true;

                // Wardrobe / Customization UI Check
                if (IUHCustomizationUI.Instance != null && IUHCustomizationUI.Instance.IsOpen)
                {
                    otherDialogOpen = true;
                }

                // Delivery Dialogue Check
                if (IUHDeliveryDialogueUI.Instance != null && IUHDeliveryDialogueUI.Instance.IsOpen)
                {
                    otherDialogOpen = true;
                }

                if (!otherDialogOpen)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    var sittingCtrl = UnityEngine.Object.FindAnyObjectByType<IUHCampus.Player.IUHPlayerSittingController>();
                    if (sittingCtrl == null || !sittingCtrl.IsSitting)
                    {
                        if (m_PlayerMovementScript != null) m_PlayerMovementScript.enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Select a mini-game by index (0 to 3) corresponding to keys [1] to [4].
        /// 0: Grab Campus
        /// 1: AI Object Hunt
        /// 2: Thợ Săn Khoảnh Khắc (Photo Hunter)
        /// 3: Green Campus
        /// </summary>
        public void SelectMiniGame(int gameIndex)
        {
            if (gameIndex < 0 || gameIndex > 3) return;
            if (IUHCampus.Core.IUHGameFlowManager.Instance != null)
            {
                IUHCampus.Core.IUHGameFlowManager.Instance.StartGameSetup(gameIndex);
                return;
            }

            switch (gameIndex)
            {
                case 0:
                    LaunchGrabCampusGame(0);
                    break;
                case 1:
                    LaunchAIObjectHuntGame();
                    break;
                case 2:
                    LaunchPhotoHunterGame();
                    break;
                case 3:
                    LaunchGreenCampusGame();
                    break;
            }
        }

        public void LaunchAIObjectHuntGame()
        {
            var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (greenMgr != null && greenMgr.HasActiveSessionOrUI) greenMgr.CancelMission(false);
            var photoMgr = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            if (photoMgr != null && photoMgr.HasActiveSessionOrUI) photoMgr.CancelMission(false);
            SetMenuOpen(false);
            TeleportToLibrary();
            var prep = IUHMiniGamePrepUI.Instance;
            if (prep != null) prep.OpenPreparation(1);
        }

        // ================= FAST TRAVEL / QUICK PLAY =================

        /// <summary>
        /// Launch Mini-Game 2.4 "Thợ Săn Khoảnh Khắc — Photo Hunter".
        /// </summary>
        public void LaunchPhotoHunterGame()
        {
            var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (greenMgr != null && greenMgr.HasActiveSessionOrUI) greenMgr.CancelMission(false);
            var customUI = IUHCustomizationUI.Instance;
            if (customUI != null && customUI.IsOpen)
            {
                customUI.ReturnToPortalMenuOnClose = false;
                customUI.SetOpen(false);
            }
            if (IUHMainMenuUI.Instance != null && IUHMainMenuUI.Instance.IsOpen)
                IUHMainMenuUI.Instance.SetOpen(false);
            SetMenuOpen(false);
            var photoMgr = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            if (photoMgr != null)
            {
                photoMgr.OpenBriefing();
            }
            else
            {
                Debug.LogWarning("[IUH Portal] IUHPhotoHunterManager chưa được tìm thấy trong Scene, fallback dịch chuyển đến Nóc Block F.");
                TeleportToBlockFRoof();
            }
        }

        /// <summary>
        /// Launch Mini-Game 2.1 "Grab Campus: Chuyến Xe Tâm Linh" directly.
        /// </summary>
        public void LaunchGrabCampusGame(int missionIndex = 0)
        {
            var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (greenMgr != null && greenMgr.HasActiveSessionOrUI) greenMgr.CancelMission(false);
            SetMenuOpen(false);

            var photoMgr = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            if (photoMgr != null && photoMgr.HasActiveSessionOrUI)
            {
                photoMgr.CancelMission(restoreCursorAndController: false);
            }

            var customUI = IUHCustomizationUI.Instance;
            if (customUI != null)
            {
                customUI.SetOpen(true);
            }
            else
            {
                var grabMgr = IUHGrabCampusManager.Instance;
                if (grabMgr != null)
                {
                    grabMgr.StartMission(missionIndex);
                }
                else
                {
                    Debug.LogWarning("[IUH Portal] IUHGrabCampusManager chưa được tìm thấy trong Scene!");
                }
            }
        }

        /// <summary>
        /// Fast travel to Gate 1 Grab Parking.
        /// </summary>
        public void TeleportToGate1Parking()
        {
            TeleportPlayer(IUHCampus.World.IUHReferenceLayout.IsActive?IUHCampus.World.IUHReferenceLayout.Entrance:m_Gate1ParkingPos, "Bãi Xe Cổng 1 (Nguyễn Văn Bảo)",Vector3.forward);
        }

        /// <summary>
        /// Fast travel to KTX Dormitory.
        /// </summary>
        public void TeleportToKtxDorm()
        {
            TeleportPlayer(IUHCampus.World.IUHReferenceLayout.IsActive?IUHCampus.World.IUHReferenceLayout.DormEntrance:m_KtxDormPos, "Ký túc xá I/G",Vector3.right);
        }

        /// <summary>
        /// Fast travel to Library Block A.
        /// </summary>
        public void TeleportToLibrary()
        {
            TeleportPlayer(IUHCampus.World.IUHReferenceLayout.IsActive?IUHCampus.World.IUHReferenceLayout.LibraryEntrance:m_LibraryPos, "Thư Viện Trung Tâm (Tòa A)",Vector3.back);
        }

        /// <summary>
        /// Fast travel to Block F Rooftop Staircase.
        /// </summary>
        public void TeleportToBlockFRoof()
        {
            TeleportPlayer(IUHCampus.World.IUHReferenceLayout.IsActive?IUHCampus.World.IUHReferenceLayout.FRoof:m_BlockFRoofPos, "Nóc nhà F",Vector3.back);
        }

        /// <summary>
        /// Fast travel to Central Courtyard Trash AI Station.
        /// </summary>
        public void TeleportToGreenTrashStation()
        {
            TeleportPlayer(IUHCampus.World.IUHReferenceLayout.IsActive?IUHCampus.World.IUHReferenceLayout.GreenSpawn:m_CentralCourtyardPos, "Trạm Phân Loại Rác Green Campus",Vector3.forward);
        }

        public void LaunchGreenCampusGame()
        {
            var manager = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[IUH Portal] Chưa setup Green Campus trong scene này.");
                return;
            }
            manager.OpenBriefing();
        }

        /// <summary>
        /// Fast travel to Festival Stage.
        /// </summary>
        public void TeleportToFestivalStage()
        {
            SetMenuOpen(true);
            Debug.Log("[IUH Portal] Campus Festival đang phát triển, chưa có nhiệm vụ để bắt đầu.");
        }

        private void TeleportPlayer(Vector3 targetPos, string locationName, Vector3 lookDirection)
        {
            var green=IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if(green!=null&&green.HasActiveSessionOrUI)green.CancelMission();
            var photo=IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            if(photo!=null&&photo.HasActiveSessionOrUI)photo.CancelMission();
            if(IUHGrabCampusManager.Instance!=null)IUHGrabCampusManager.Instance.CancelMission();
            if(IUHCustomizationUI.Instance!=null&&IUHCustomizationUI.Instance.IsOpen)
            {IUHCustomizationUI.Instance.ReturnToPortalMenuOnClose=false;IUHCustomizationUI.Instance.SetOpen(false);}
            SetMenuOpen(false);

            FindPlayerReferences();

            if (m_PlayerObject != null)
            {
                var sitting=m_PlayerObject.GetComponent<IUHCampus.Player.IUHPlayerSittingController>();
                if(sitting!=null&&sitting.IsSitting)sitting.StandUp();
                Vector3 delta=targetPos-m_PlayerObject.transform.position;
                var charController = m_PlayerObject.GetComponent<CharacterController>();
                if (charController != null) charController.enabled = false;

                m_PlayerObject.transform.position = targetPos;

                if (charController != null) charController.enabled = true;
                var movement=m_PlayerObject.GetComponent<StarterAssets.ThirdPersonController>();
                if(movement!=null)
                {
                    movement.ResetMotion();movement.SetCameraOrientation(lookDirection);movement.enabled=true;
                    var brain=Camera.main!=null?Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>():null;
                    var camera=brain!=null?brain.ActiveVirtualCamera as Unity.Cinemachine.CinemachineCamera:null;
                    if(camera!=null&&movement.CinemachineCameraTarget!=null)
                    {
                        camera.Follow=movement.CinemachineCameraTarget.transform;
                        camera.OnTargetObjectWarped(camera.Follow,delta);camera.PreviousStateIsValid=false;
                    }
                }

                if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);
                IUHGameFlowManager.Instance?.NotifyGameStarted(-1);
                Debug.Log($"[IUH Portal Fast Travel] Đã dịch chuyển đến: {locationName} tại {targetPos}");
            }
        }
    }
}
