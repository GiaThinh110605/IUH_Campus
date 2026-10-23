using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.UI
{
    /// <summary>
    /// Wardrobe & Garage Customization Menu (Image 2).
    /// Allows the player to switch between XANH SM, Grab, Be, and Default IUH outfits & bikes.
    /// Updates live 3D character accessories and motorcycle fairings.
    /// </summary>
    public class IUHCustomizationUI : MonoBehaviour
    {
        public static IUHCustomizationUI Instance { get; private set; }

        [Header("Menu Root")]
        [SerializeField] private GameObject m_PanelRoot;
        [SerializeField] private Button m_BtnBack;

        [Header("Action Buttons")]
        [SerializeField] private Button m_BtnStartGame;
        [SerializeField] private Button m_BtnOpenPortal;

        [Header("Theme Selection Buttons")]
        [SerializeField] private Button m_BtnSelectXanhSM;
        [SerializeField] private Button m_BtnSelectGrab;
        [SerializeField] private Button m_BtnSelectBe;
        [SerializeField] private Button m_BtnSelectDefault;

        [Header("Status Labels")]
        [SerializeField] private Text m_TxtStatusXanhSM;
        [SerializeField] private Text m_TxtStatusGrab;
        [SerializeField] private Text m_TxtStatusBe;
        [SerializeField] private Text m_TxtStatusDefault;

        private bool m_IsOpen = false;
        public bool IsOpen => m_IsOpen;

        public void Setup(GameObject panelRoot, Button btnBack, Button btnStartGame, Button btnOpenPortal, Button btnXanhSM, Button btnGrab, Button btnBe, Button btnDefault, Text txtXanhSM, Text txtGrab, Text txtBe, Text txtDefault)
        {
            m_PanelRoot = panelRoot;
            m_BtnBack = btnBack;
            m_BtnStartGame = btnStartGame;
            m_BtnOpenPortal = btnOpenPortal;
            m_BtnSelectXanhSM = btnXanhSM;
            m_BtnSelectGrab = btnGrab;
            m_BtnSelectBe = btnBe;
            m_BtnSelectDefault = btnDefault;
            m_TxtStatusXanhSM = txtXanhSM;
            m_TxtStatusGrab = txtGrab;
            m_TxtStatusBe = txtBe;
            m_TxtStatusDefault = txtDefault;
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
            if (m_PanelRoot == null) m_PanelRoot = gameObject;
            if (m_BtnBack == null) m_BtnBack = transform.Find("BtnBack")?.GetComponent<Button>();
            if (m_BtnStartGame == null) m_BtnStartGame = transform.Find("Footer/BtnStartGame")?.GetComponent<Button>();
            if (m_BtnOpenPortal == null) m_BtnOpenPortal = transform.Find("Footer/BtnOpenPortal")?.GetComponent<Button>();
            if (m_BtnSelectXanhSM == null) m_BtnSelectXanhSM = transform.Find("CardsBox/Card_XANH SM/BtnSelect")?.GetComponent<Button>();
            if (m_BtnSelectGrab == null) m_BtnSelectGrab = transform.Find("CardsBox/Card_Grab/BtnSelect")?.GetComponent<Button>();
            if (m_BtnSelectBe == null) m_BtnSelectBe = transform.Find("CardsBox/Card_be/BtnSelect")?.GetComponent<Button>();
            if (m_BtnSelectDefault == null) m_BtnSelectDefault = transform.Find("CardsBox/Card_Sinh Viên IUH/BtnSelect")?.GetComponent<Button>();

            if (m_TxtStatusXanhSM == null) m_TxtStatusXanhSM = transform.Find("CardsBox/Card_XANH SM/BtnSelect/Txt")?.GetComponent<Text>();
            if (m_TxtStatusGrab == null) m_TxtStatusGrab = transform.Find("CardsBox/Card_Grab/BtnSelect/Txt")?.GetComponent<Text>();
            if (m_TxtStatusBe == null) m_TxtStatusBe = transform.Find("CardsBox/Card_be/BtnSelect/Txt")?.GetComponent<Text>();
            if (m_TxtStatusDefault == null) m_TxtStatusDefault = transform.Find("CardsBox/Card_Sinh Viên IUH/BtnSelect/Txt")?.GetComponent<Text>();
        }

        private void Start()
        {
            // Do NOT auto open wardrobe on boot; game flow manager handles startup menu
            SetOpen(false);
        }

        private void SetupButtons()
        {
            if (m_BtnBack != null)
            {
                m_BtnBack.onClick.RemoveAllListeners();
                m_BtnBack.onClick.AddListener(OnBackClicked);
            }

            if (m_BtnStartGame != null)
            {
                m_BtnStartGame.onClick.RemoveAllListeners();
                m_BtnStartGame.onClick.AddListener(OnStartGameClicked);
            }

            if (m_BtnOpenPortal != null)
            {
                m_BtnOpenPortal.onClick.RemoveAllListeners();
                m_BtnOpenPortal.onClick.AddListener(OnBackClicked);
            }

            if (m_BtnSelectXanhSM != null)
            {
                m_BtnSelectXanhSM.onClick.RemoveAllListeners();
                m_BtnSelectXanhSM.onClick.AddListener(() => SelectTheme(IUHCustomizationManager.BrandTheme.XanhSM));
            }

            if (m_BtnSelectGrab != null)
            {
                m_BtnSelectGrab.onClick.RemoveAllListeners();
                m_BtnSelectGrab.onClick.AddListener(() => SelectTheme(IUHCustomizationManager.BrandTheme.Grab));
            }

            if (m_BtnSelectBe != null)
            {
                m_BtnSelectBe.onClick.RemoveAllListeners();
                m_BtnSelectBe.onClick.AddListener(() => SelectTheme(IUHCustomizationManager.BrandTheme.Be));
            }

            if (m_BtnSelectDefault != null)
            {
                m_BtnSelectDefault.onClick.RemoveAllListeners();
                m_BtnSelectDefault.onClick.AddListener(() => SelectTheme(IUHCustomizationManager.BrandTheme.DefaultIUH));
            }
        }

        public void OnBackClicked()
        {
            SetOpen(false);
            if (IUHCampus.Core.IUHGameFlowManager.Instance != null)
            {
                IUHCampus.Core.IUHGameFlowManager.Instance.GoToMiniGameSelection();
            }
            else
            {
                var portal = IUHMiniGamePortalUI.Instance;
                if (portal != null) portal.SetMenuOpen(true);
            }
        }

        public void OnStartGameClicked()
        {
            var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
            if (greenMgr != null && greenMgr.HasActiveSessionOrUI) greenMgr.CancelMission(false);

            var photoMgr = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            if (photoMgr != null && photoMgr.HasActiveSessionOrUI)
            {
                photoMgr.CancelMission(restoreCursorAndController: false);
            }

            SetOpen(false);

            if (IUHCampus.Core.IUHGameFlowManager.Instance != null)
            {
                IUHCampus.Core.IUHGameFlowManager.Instance.StartPlaying(0);
            }
            else
            {
                var grabMgr = IUHGrabCampusManager.Instance;
                if (grabMgr != null && !grabMgr.IsMissionActive)
                {
                    grabMgr.StartMission(0);
                }
            }
        }

        public bool ReturnToPortalMenuOnClose = false;

        public void SetOpen(bool open)
        {
            m_IsOpen = open;
            if (m_PanelRoot != null) m_PanelRoot.SetActive(open);

            if (open)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UpdateSelectionCards();
            }
            else
            {
                if (IUHCampus.Core.IUHGameFlowManager.Instance != null)
                {
                    if (IUHCampus.Core.IUHGameFlowManager.Instance.CurrentState == IUHCampus.Core.IUHGameFlowManager.FlowState.Playing)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                }
                else
                {
                    var portal = IUHMiniGamePortalUI.Instance;
                    if (portal == null || !portal.IsOpen)
                    {
                        var photoMgr = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
                        bool photoModalOpen = photoMgr != null && photoMgr.HasActiveModal;
                        var greenMgr = IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance;
                        bool greenModalOpen = greenMgr != null && greenMgr.HasActiveModal;
                        if (!photoModalOpen && !greenModalOpen)
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.visible = false;
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (!m_IsOpen) return;

            if (IUHCampus.Core.IUHInputHelper.IsEscapeKeyDown())
            {
                OnBackClicked();
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                OnStartGameClicked();
                return;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnStartGameClicked();
                    return;
                }
            }
            catch { }

            if (IUHCampus.Core.IUHInputHelper.IsNumberKeyDown(1))
            {
                SelectTheme(IUHCustomizationManager.BrandTheme.XanhSM);
            }
            else if (IUHCampus.Core.IUHInputHelper.IsNumberKeyDown(2))
            {
                SelectTheme(IUHCustomizationManager.BrandTheme.Grab);
            }
            else if (IUHCampus.Core.IUHInputHelper.IsNumberKeyDown(3))
            {
                SelectTheme(IUHCustomizationManager.BrandTheme.Be);
            }
            else if (IUHCampus.Core.IUHInputHelper.IsNumberKeyDown(4))
            {
                SelectTheme(IUHCustomizationManager.BrandTheme.DefaultIUH);
            }
        }

        public void SelectTheme(IUHCustomizationManager.BrandTheme theme)
        {
            var mgr = IUHCustomizationManager.Instance;
            if (mgr != null)
            {
                mgr.ApplyTheme(theme);
            }
            UpdateSelectionCards();
        }

        public void UpdateSelectionCards()
        {
            var mgr = IUHCustomizationManager.Instance;
            var current = (mgr != null) ? mgr.CurrentTheme : IUHCustomizationManager.BrandTheme.Grab;

            SetButtonStatus(m_TxtStatusXanhSM, current == IUHCustomizationManager.BrandTheme.XanhSM);
            SetButtonStatus(m_TxtStatusGrab, current == IUHCustomizationManager.BrandTheme.Grab);
            SetButtonStatus(m_TxtStatusBe, current == IUHCustomizationManager.BrandTheme.Be);
            SetButtonStatus(m_TxtStatusDefault, current == IUHCustomizationManager.BrandTheme.DefaultIUH);

            if (m_BtnStartGame != null)
            {
                var txt = m_BtnStartGame.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    var grabMgr = IUHGrabCampusManager.Instance;
                    if (grabMgr != null && grabMgr.IsMissionActive)
                    {
                        txt.text = "✅ ÁP DỤNG & TIẾP TỤC CHƠI [ ENTER ]";
                    }
                    else
                    {
                        txt.text = "▶️ BẮT ĐẦU CHƠI VỚI TRANG PHỤC NÀY [ ENTER / SPACE ]";
                    }
                }
            }
        }

        private void SetButtonStatus(Text txt, bool isSelected)
        {
            if (txt == null) return;
            if (isSelected)
            {
                txt.text = "✓ Đang sử dụng";
                txt.color = new Color(0.2f, 1.0f, 0.4f);
            }
            else
            {
                txt.text = "○ Chọn";
                txt.color = new Color(0.8f, 0.85f, 0.95f);
            }
        }
    }
}
