using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// Title / Main Menu Screen (1. Màn hình chính).
    /// Exactly mirrors Panel 1 of the design specification:
    /// - Title Banner: "Grab Campus — Chuyến Xe Tâm Linh"
    /// - Buttons: Bắt đầu, Cài đặt, Trang phục & Xe máy, Thoát.
    /// - Description: "Khám phá khuôn viên trường và bắt đầu hành trình của bạn!"
    /// </summary>
    public class IUHMainMenuUI : MonoBehaviour
    {
        public static IUHMainMenuUI Instance { get; private set; }

        [Header("Menu Root")]
        [SerializeField] private GameObject m_MenuRoot;

        [Header("Buttons")]
        [SerializeField] private Button m_BtnStart;
        [SerializeField] private Button m_BtnSettings;
        [SerializeField] private Button m_BtnCustomization;
        [SerializeField] private Button m_BtnQuit;

        [Header("Settings Dialog")]
        [SerializeField] private GameObject m_SettingsDialog;
        [SerializeField] private Slider m_SliderVolume;
        [SerializeField] private Button m_BtnCloseSettings;

        private bool m_IsOpen = false;
        public bool IsOpen => m_IsOpen;

        public void Setup(GameObject menuRoot, Button btnStart, Button btnSettings, Button btnCustomization, Button btnQuit, GameObject settingsDialog, Slider sliderVolume, Button btnCloseSettings)
        {
            m_MenuRoot = menuRoot;
            m_BtnStart = btnStart;
            m_BtnSettings = btnSettings;
            m_BtnCustomization = btnCustomization;
            m_BtnQuit = btnQuit;
            m_SettingsDialog = settingsDialog;
            m_SliderVolume = sliderVolume;
            m_BtnCloseSettings = btnCloseSettings;
            SetupButtons();
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            AutoWireReferences();
            SetupButtons();
        }

        private void AutoWireReferences()
        {
            if (m_MenuRoot == null) m_MenuRoot = gameObject;
            if (m_BtnStart == null) m_BtnStart = transform.Find("TitleCard/ButtonsBox/BtnStart")?.GetComponent<Button>();
            if (m_BtnSettings == null) m_BtnSettings = transform.Find("TitleCard/ButtonsBox/BtnSettings")?.GetComponent<Button>();
            if (m_BtnCustomization == null) m_BtnCustomization = transform.Find("TitleCard/ButtonsBox/BtnCustomization")?.GetComponent<Button>();
            if (m_BtnQuit == null) m_BtnQuit = transform.Find("TitleCard/ButtonsBox/BtnQuit")?.GetComponent<Button>();
            if (m_SettingsDialog == null) m_SettingsDialog = transform.Find("SettingsDialog")?.gameObject;
            if (m_SliderVolume == null) m_SliderVolume = transform.Find("SettingsDialog/VolumeSlider")?.GetComponent<Slider>();
            if (m_BtnCloseSettings == null) m_BtnCloseSettings = transform.Find("SettingsDialog/BtnClose")?.GetComponent<Button>();
        }

        private void Start()
        {
            // Initially open title screen or closed depending on preference
            // Keep closed on boot if game directly loads campus, or show with toggle
            if (m_SettingsDialog != null) m_SettingsDialog.SetActive(false);
            SetOpen(false);
        }

        private void SetupButtons()
        {
            if (m_BtnStart != null)
            {
                m_BtnStart.onClick.RemoveAllListeners();
                m_BtnStart.onClick.AddListener(OnStartClicked);
            }

            if (m_BtnSettings != null)
            {
                m_BtnSettings.onClick.RemoveAllListeners();
                m_BtnSettings.onClick.AddListener(OnSettingsClicked);
            }

            if (m_BtnCustomization != null)
            {
                m_BtnCustomization.onClick.RemoveAllListeners();
                m_BtnCustomization.onClick.AddListener(OnCustomizationClicked);
            }

            if (m_BtnQuit != null)
            {
                m_BtnQuit.onClick.RemoveAllListeners();
                m_BtnQuit.onClick.AddListener(OnQuitClicked);
            }

            if (m_BtnCloseSettings != null)
            {
                m_BtnCloseSettings.onClick.RemoveAllListeners();
                m_BtnCloseSettings.onClick.AddListener(() => {
                    if (m_SettingsDialog != null) m_SettingsDialog.SetActive(false);
                });
            }

            if (m_SliderVolume != null)
            {
                m_SliderVolume.value = AudioListener.volume;
                m_SliderVolume.onValueChanged.AddListener((v) => { AudioListener.volume = v; });
            }
        }

        public void SetOpen(bool open)
        {
            m_IsOpen = open;
            if (m_MenuRoot != null) m_MenuRoot.SetActive(open);

            if (open)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                var portal = IUHMiniGamePortalUI.Instance;
                if (portal == null || !portal.IsOpen)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private void OnStartClicked()
        {
            SetOpen(false);
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
                    grabMgr.StartMission(0);
                }
            }
        }

        private void OnSettingsClicked()
        {
            if (m_SettingsDialog != null)
            {
                m_SettingsDialog.SetActive(true);
            }
        }

        private void OnCustomizationClicked()
        {
            var customUI = IUHCustomizationUI.Instance;
            if (customUI != null)
            {
                customUI.SetOpen(true);
            }
        }

        private void OnQuitClicked()
        {
            SetOpen(false);
#if UNITY_EDITOR
            Debug.Log("[IUH Main Menu] Đã bấm Thoát game.");
#else
            Application.Quit();
#endif
        }
    }
}
