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

            FindPlayerReferences();
        }

        private void Start()
        {
            AutoWireButtons();
            SetMenuOpen(false);
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
                    closeBtn.onClick.AddListener(() => SetMenuOpen(false));
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
                        if (cardBtn != null)
                        {
                            int index = i;
                            cardBtn.onClick.RemoveAllListeners();
                            cardBtn.onClick.AddListener(() => SelectMiniGame(index));
                        }

                        // Also action button
                        var actionBtn = card.Find("BtnBar/ActionBtn")?.GetComponent<Button>();
                        if (actionBtn != null)
                        {
                            int index = i;
                            actionBtn.onClick.RemoveAllListeners();
                            actionBtn.onClick.AddListener(() => SelectMiniGame(index));
                        }
                    }
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
            if (IsKeyTriggered(KeyCode.M))
            {
                ToggleMenu();
                return;
            }

            // When menu is open, handle keys [1] - [6] and [ESC]
            if (m_IsOpen)
            {
                if (IsKeyTriggered(KeyCode.Escape))
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
                else if (IsKeyTriggered(KeyCode.Alpha5) || IsKeyTriggered(KeyCode.Keypad5))
                {
                    SelectMiniGame(4);
                }
                else if (IsKeyTriggered(KeyCode.Alpha6) || IsKeyTriggered(KeyCode.Keypad6))
                {
                    SelectMiniGame(5);
                }
            }
        }

        private bool IsKeyTriggered(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (code)
                {
                    case KeyCode.M: return Keyboard.current.mKey.wasPressedThisFrame;
                    case KeyCode.Escape: return Keyboard.current.escapeKey.wasPressedThisFrame;
                    case KeyCode.Alpha1:
                    case KeyCode.Keypad1: return Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame;
                    case KeyCode.Alpha2:
                    case KeyCode.Keypad2: return Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame;
                    case KeyCode.Alpha3:
                    case KeyCode.Keypad3: return Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame;
                    case KeyCode.Alpha4:
                    case KeyCode.Keypad4: return Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame;
                    case KeyCode.Alpha5:
                    case KeyCode.Keypad5: return Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame;
                    case KeyCode.Alpha6:
                    case KeyCode.Keypad6: return Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame;
                }
            }
#endif
            try
            {
                return Input.GetKeyDown(code);
            }
            catch
            {
                return false;
            }
        }

        public void ToggleMenu()
        {
            SetMenuOpen(!m_IsOpen);
        }

        public void SetMenuOpen(bool open)
        {
            m_IsOpen = open;

            if (m_MenuPanel != null)
            {
                m_MenuPanel.SetActive(m_IsOpen);
            }

            if (m_FloatingButton != null)
            {
                m_FloatingButton.SetActive(!m_IsOpen);
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
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (m_PlayerMovementScript != null) m_PlayerMovementScript.enabled = true;
            }
        }

        /// <summary>
        /// Select a mini-game by index (0 to 5) corresponding to keys [1] to [6].
        /// </summary>
        public void SelectMiniGame(int gameIndex)
        {
            switch (gameIndex)
            {
                case 0:
                    LaunchGrabCampusGame(0);
                    break;
                case 1:
                    TeleportToKtxDorm();
                    break;
                case 2:
                    TeleportToLibrary();
                    break;
                case 3:
                    TeleportToBlockFRoof();
                    break;
                case 4:
                    TeleportToGreenTrashStation();
                    break;
                case 5:
                    TeleportToFestivalStage();
                    break;
            }
        }

        // ================= FAST TRAVEL / QUICK PLAY =================

        /// <summary>
        /// Launch Mini-Game 2.1 "Grab Campus: Chuyến Xe Tâm Linh" directly.
        /// </summary>
        public void LaunchGrabCampusGame(int missionIndex = 0)
        {
            SetMenuOpen(false);

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

        /// <summary>
        /// Fast travel to Gate 1 Grab Parking.
        /// </summary>
        public void TeleportToGate1Parking()
        {
            TeleportPlayer(m_Gate1ParkingPos, "Bãi Xe Cổng 1 (Nguyễn Văn Bảo)");
        }

        /// <summary>
        /// Fast travel to KTX Dormitory.
        /// </summary>
        public void TeleportToKtxDorm()
        {
            TeleportPlayer(m_KtxDormPos, "Khu Ký Túc Xá Cổng 4 (Block X)");
        }

        /// <summary>
        /// Fast travel to Library Block A.
        /// </summary>
        public void TeleportToLibrary()
        {
            TeleportPlayer(m_LibraryPos, "Thư Viện Trung Tâm (Tòa A)");
        }

        /// <summary>
        /// Fast travel to Block F Rooftop Staircase.
        /// </summary>
        public void TeleportToBlockFRoof()
        {
            TeleportPlayer(m_BlockFRoofPos, "Cầu Thang Lên Nóc Nhà Block F");
        }

        /// <summary>
        /// Fast travel to Central Courtyard Trash AI Station.
        /// </summary>
        public void TeleportToGreenTrashStation()
        {
            TeleportPlayer(m_CentralCourtyardPos, "Trạm Phân Loại Rác AI Sân Trường");
        }

        /// <summary>
        /// Fast travel to Festival Stage.
        /// </summary>
        public void TeleportToFestivalStage()
        {
            TeleportPlayer(m_FestivalStagePos, "Sân Khấu Đại Hội Âm Nhạc IUH");
        }

        private void TeleportPlayer(Vector3 targetPos, string locationName)
        {
            SetMenuOpen(false);

            FindPlayerReferences();

            if (m_PlayerObject != null)
            {
                var charController = m_PlayerObject.GetComponent<CharacterController>();
                if (charController != null) charController.enabled = false;

                m_PlayerObject.transform.position = targetPos;

                if (charController != null) charController.enabled = true;

                if (m_AudioSource != null) m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);
                Debug.Log($"[IUH Portal Fast Travel] Đã dịch chuyển đến: {locationName} tại {targetPos}");
            }
        }
    }
}
