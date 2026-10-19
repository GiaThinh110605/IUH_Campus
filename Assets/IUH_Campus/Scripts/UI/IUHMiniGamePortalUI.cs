using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// In-Game Mini-Game Selection Portal & Teleportation Menu (IUH Campus).
    /// Allows players & evaluators to press [M] or click the floating HUD button
    /// to browse 6 minigames (3 Gia Thinh, 3 Minh Nhat), launch Grab Campus instantly,
    /// or fast-travel across the 3D campus.
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

        private void Start()
        {
            AutoWireButtons();
            SetMenuOpen(false);
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
                        var actionBtn = card.Find("BtnBar/ActionBtn")?.GetComponent<Button>();
                        if (actionBtn != null)
                        {
                            int index = i;
                            actionBtn.onClick.RemoveAllListeners();
                            actionBtn.onClick.AddListener(() => OnCardActionClicked(index));
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

        private void OnCardActionClicked(int cardIndex)
        {
            switch (cardIndex)
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

        private void Update()
        {
            if (Input.GetKeyDown(m_ToggleKey))
            {
                ToggleMenu();
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
                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetClickSound(), 0.8f);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (m_PlayerMovementScript != null) m_PlayerMovementScript.enabled = true;
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

            if (m_PlayerObject != null)
            {
                var charController = m_PlayerObject.GetComponent<CharacterController>();
                if (charController != null) charController.enabled = false;

                m_PlayerObject.transform.position = targetPos;

                if (charController != null) charController.enabled = true;

                m_AudioSource.PlayOneShot(IUHSoundSynthesizer.GetPickupSound(), 1.0f);
                Debug.Log($"[IUH Portal Fast Travel] Đã dịch chuyển đến: {locationName} tại {targetPos}");
            }
        }
    }
}
