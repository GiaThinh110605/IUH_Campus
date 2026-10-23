using System;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.Core;

namespace IUHCampus.UI
{
    /// <summary>
    /// Mission Briefing & Preparation Screen for Mini-Games 2 through 6.
    /// Fulfills Requirement 2 Step 2:
    /// - Shows mission introduction, objectives, guidelines, and location.
    /// - Only initializes gameplay / AI / timers / systems when player clicks 'Bắt đầu nhiệm vụ'.
    /// - Has 'Quay lại' button that returns to MiniGameSelection without starting gameplay.
    /// </summary>
    public class IUHMiniGamePrepUI : MonoBehaviour
    {
        public static IUHMiniGamePrepUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject m_PanelRoot;

        [Header("UI Fields")]
        [SerializeField] private Text m_TxtGameTitle;
        [SerializeField] private Text m_TxtLocation;
        [SerializeField] private Text m_TxtObjective;
        [SerializeField] private Text m_TxtDetails;
        [SerializeField] private Button m_BtnStartMission;
        [SerializeField] private Button m_BtnBack;

        private int m_CurrentGameIndex = 1; // 1 to 5 for minigames 2 to 6
        private bool m_IsOpen = false;
        public bool IsOpen => m_IsOpen;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            EnsureUIHierarchy();
        }

        private void Start()
        {
            if (m_PanelRoot != null) m_PanelRoot.SetActive(false);
        }

        public void OpenPreparation(int gameIndex)
        {
            EnsureUIHierarchy();
            m_CurrentGameIndex = gameIndex;
            m_IsOpen = true;

            PopulateMissionData(gameIndex);

            if (m_PanelRoot != null)
            {
                m_PanelRoot.SetActive(true);
                m_PanelRoot.transform.SetAsLastSibling();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ClosePreparation()
        {
            m_IsOpen = false;
            if (m_PanelRoot != null)
            {
                m_PanelRoot.SetActive(false);
            }
        }

        private void PopulateMissionData(int gameIndex)
        {
            switch (gameIndex)
            {
                case 1: // Mini-game 2
                    if (m_TxtGameTitle != null) m_TxtGameTitle.text = "<size=32>👻 <b>2.2. TRỐN TÌM VỚI MA Ở KÝ TÚC XÁ</b></size>";
                    if (m_TxtLocation != null) m_TxtLocation.text = "📍 <color=#67E8F9><b>Vị trí:</b></color> Sảnh Ký Túc Xá Block D (Cổng 4)";
                    if (m_TxtObjective != null) m_TxtObjective.text = "Khám phá phòng tối KTX, né tránh ma tuần tra AI NavMesh 4 trạng thái, tìm chìa khóa & nhật ký để mở lối thoát hiểm.";
                    if (m_TxtDetails != null) m_TxtDetails.text =
                        "• <b>AI Hồn Ma (NavMesh):</b> 4 trạng thái linh hoạt: Tuần tra (Patrol) ➔ Điều tra (Investigate) ➔ Truy đuổi (Chase) ➔ Lùng sục (Search).\n" +
                        "• <b>Ẩn nấp:</b> Tận dụng các phòng mở, tủ đồ và gầm giường để cắt đuôi khi ma phát hiện.\n" +
                        "• <b>Mục tiêu:</b> Thu thập chìa khóa cổng KTX và mảnh nhật ký số 2.\n" +
                        "• <b>Lưu ý:</b> Gameplay và AI chỉ bắt đầu hoạt động sau khi bạn bấm <b>'Bắt Đầu Nhiệm Vụ'</b>.";
                    break;

                case 2: // Mini-game 3
                    if (m_TxtGameTitle != null) m_TxtGameTitle.text = "<size=32>🔍 <b>2.3. AI OBJECT HUNT: TÌM ĐỒ VẬT</b></size>";
                    if (m_TxtLocation != null) m_TxtLocation.text = "📍 <color=#67E8F9><b>Vị trí:</b></color> Thư Viện Trung Tâm Tòa A";
                    if (m_TxtObjective != null) m_TxtObjective.text = "Sử dụng camera quét nhận diện sách cổ / bình chữa cháy qua Deep Learning YOLO & hiển thị AR Hologram.";
                    if (m_TxtDetails != null) m_TxtDetails.text =
                        "• <b>Camera AI:</b> Đưa ống kính đến các vật thể trong thư viện để nhận diện nhãn (books, chair, fire extinguisher).\n" +
                        "• <b>AR Hologram:</b> Khi nhận diện chính xác, mô hình 3D AR kèm chú thích học thuật sẽ xuất hiện.\n" +
                        "• <b>Mục tiêu:</b> Hoàn thành quét đủ 3 vật phẩm chỉ định để nhận thẻ thủ thư danh dự.\n" +
                        "• <b>Lưu ý:</b> Hệ thống camera nhận diện AI chỉ kích hoạt khi bạn bấm <b>'Bắt Đầu Nhiệm Vụ'</b>.";
                    break;

                case 3: // Mini-game 4
                    if (m_TxtGameTitle != null) m_TxtGameTitle.text = "<size=32>📸 <b>2.4. THỢ SĂN KHOẢNH KHẮC</b></size>";
                    if (m_TxtLocation != null) m_TxtLocation.text = "📍 <color=#67E8F9><b>Vị trí:</b></color> Cầu Thang Lên Nóc Nhà Block F";
                    if (m_TxtObjective != null) m_TxtObjective.text = "Khám phá khuôn viên trường, leo thang lên nóc nhà Block F chụp ảnh check-in và thu thập huy hiệu.";
                    if (m_TxtDetails != null) m_TxtDetails.text =
                        "• <b>Góc chụp đẹp:</b> Nóc nhà Block F nhìn toàn cảnh sân trường, Cổng chính Nguyễn Văn Bảo, Hồ nước trung tâm.\n" +
                        "• <b>Chụp ảnh:</b> Căn khung hình và nhấn nút chụp để lưu trữ khoảnh khắc vào Album Sinh Viên.\n" +
                        "• <b>Mục tiêu:</b> Mở khóa danh hiệu 'Nhiếp Ảnh Gia Đại Học Công Nghiệp TP.HCM'.\n" +
                        "• <b>Lưu ý:</b> Hệ thống chụp ảnh và các điểm check-in chỉ mở khi bạn bấm <b>'Bắt Đầu Nhiệm Vụ'</b>.";
                    break;

                case 4: // Mini-game 5
                    if (m_TxtGameTitle != null) m_TxtGameTitle.text = "<size=32>🌱 <b>2.5. GREEN CAMPUS: CỨU LẤY TRƯỜNG</b></size>";
                    if (m_TxtLocation != null) m_TxtLocation.text = "📍 <color=#67E8F9><b>Vị trí:</b></color> Trạm Phân Loại Rác AI Sân Trường";
                    if (m_TxtObjective != null) m_TxtObjective.text = "Thu gom và phân loại rác tái chế bằng AI, tích lũy điểm xanh đẩy lên Cloud Adafruit IO.";
                    if (m_TxtDetails != null) m_TxtDetails.text =
                        "• <b>Thu gom rác:</b> Tìm chai nhựa, vỏ lon và giấy loại rải rác trong khuôn viên sân trường.\n" +
                        "• <b>Phân loại AI:</b> Đưa rác vào đúng ngăn cảm biến (Hữu cơ, Vô cơ, Tái chế) để nhận điểm thưởng.\n" +
                        "• <b>IoT Cloud:</b> Điểm xanh và trạng thái đầy thùng rác được gửi theo thời gian thực tới Adafruit IO.\n" +
                        "• <b>Lưu ý:</b> Gameplay phân loại rác và kết nối IoT chỉ chạy khi bạn bấm <b>'Bắt Đầu Nhiệm Vụ'</b>.";
                    break;

                case 5: // Mini-game 6
                    if (m_TxtGameTitle != null) m_TxtGameTitle.text = "<size=32>🎪 <b>2.6. CAMPUS FESTIVAL: TỔ CHỨC LỄ HỘI</b></size>";
                    if (m_TxtLocation != null) m_TxtLocation.text = "📍 <color=#67E8F9><b>Vị trí:</b></color> Sân Khấu Đại Hội Trung Tâm";
                    if (m_TxtObjective != null) m_TxtObjective.text = "Quản lý ngân sách, trang trí sân khấu đại hội âm nhạc và kết nối đồng bộ Docker backend.";
                    if (m_TxtDetails != null) m_TxtDetails.text =
                        "• <b>Quản lý ngân sách:</b> Cân đối chi phí thuê âm thanh, dàn ánh sáng LED và quà tặng sinh viên.\n" +
                        "• <b>Bố trí sân khấu:</b> Sắp xếp backdrop, bục biểu diễn và hàng rào an ninh trước giờ khai mạc.\n" +
                        "• <b>Docker Backend:</b> Đồng bộ dữ liệu vé mời và tiến trình lễ hội với máy chủ backend đóng gói Docker.\n" +
                        "• <b>Lưu ý:</b> Bộ đếm thời gian và tiến trình lễ hội chỉ bắt đầu khi bạn bấm <b>'Bắt Đầu Nhiệm Vụ'</b>.";
                    break;
            }
        }

        private void EnsureUIHierarchy()
        {
            if (m_PanelRoot != null) return;

            // Look for existing panel in Canvas
            var canvas = GameObject.Find("IUH_Minigame_Canvas");
            if (canvas == null) return;

            var existing = canvas.transform.Find("MiniGameSetupPanel");
            if (existing != null)
            {
                m_PanelRoot = existing.gameObject;
                AutoWireComponents();
                return;
            }

            // Create dynamically
            var panelGO = new GameObject("MiniGameSetupPanel");
            panelGO.transform.SetParent(canvas.transform, false);
            var rect = panelGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.05f, 0.12f, 0.94f);

            // Dialog Card
            var cardGO = new GameObject("PrepCard");
            cardGO.transform.SetParent(panelGO.transform, false);
            var cardRect = cardGO.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.24f, 0.12f);
            cardRect.anchorMax = new Vector2(0.76f, 0.88f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = new Color(0.04f, 0.08f, 0.18f, 0.98f);
            var cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.0f, 0.85f, 0.95f, 0.95f);
            cardOutline.effectDistance = new Vector2(3f, 3f);

            var vlg = cardGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 24, 24);
            vlg.spacing = 14f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Title
            var titleGO = new GameObject("TxtTitle");
            titleGO.transform.SetParent(cardGO.transform, false);
            m_TxtGameTitle = titleGO.AddComponent<Text>();
            m_TxtGameTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtGameTitle.color = Color.white;
            m_TxtGameTitle.alignment = TextAnchor.MiddleCenter;
            titleGO.AddComponent<LayoutElement>().preferredHeight = 44f;

            // Location
            var locGO = new GameObject("TxtLocation");
            locGO.transform.SetParent(cardGO.transform, false);
            m_TxtLocation = locGO.AddComponent<Text>();
            m_TxtLocation.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtLocation.fontSize = 17;
            m_TxtLocation.color = new Color(0.4f, 0.9f, 1f);
            m_TxtLocation.alignment = TextAnchor.MiddleCenter;
            locGO.AddComponent<LayoutElement>().preferredHeight = 30f;

            // Objective
            var objGO = new GameObject("TxtObjective");
            objGO.transform.SetParent(cardGO.transform, false);
            m_TxtObjective = objGO.AddComponent<Text>();
            m_TxtObjective.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtObjective.fontSize = 16;
            m_TxtObjective.fontStyle = FontStyle.Italic;
            m_TxtObjective.color = new Color(0.95f, 0.88f, 0.45f);
            m_TxtObjective.alignment = TextAnchor.MiddleCenter;
            objGO.AddComponent<LayoutElement>().preferredHeight = 44f;

            // Details Container
            var detGO = new GameObject("TxtDetails");
            detGO.transform.SetParent(cardGO.transform, false);
            m_TxtDetails = detGO.AddComponent<Text>();
            m_TxtDetails.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_TxtDetails.fontSize = 15;
            m_TxtDetails.color = new Color(0.9f, 0.95f, 1.0f);
            m_TxtDetails.alignment = TextAnchor.UpperLeft;
            m_TxtDetails.lineSpacing = 1.35f;
            detGO.AddComponent<LayoutElement>().preferredHeight = 190f;

            // Button Bar
            var btnRow = new GameObject("ButtonBar");
            btnRow.transform.SetParent(cardGO.transform, false);
            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            btnRow.AddComponent<LayoutElement>().preferredHeight = 56f;

            // Back Button
            var backGO = new GameObject("BtnBack");
            backGO.transform.SetParent(btnRow.transform, false);
            var backImg = backGO.AddComponent<Image>();
            backImg.color = new Color(0.20f, 0.28f, 0.40f);
            m_BtnBack = backGO.AddComponent<Button>();
            var backOutline = backGO.AddComponent<Outline>();
            backOutline.effectColor = new Color(0.4f, 0.6f, 0.8f);
            backOutline.effectDistance = new Vector2(2f, 2f);

            var backTxtGO = new GameObject("Txt");
            backTxtGO.transform.SetParent(backGO.transform, false);
            var btr = backTxtGO.AddComponent<RectTransform>();
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            var bTxt = backTxtGO.AddComponent<Text>();
            bTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bTxt.text = "⬅️ QUAY LẠI [ ESC ]";
            bTxt.fontSize = 17;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;

            // Start Button
            var startGO = new GameObject("BtnStartMission");
            startGO.transform.SetParent(btnRow.transform, false);
            var startImg = startGO.AddComponent<Image>();
            startImg.color = new Color(0.0f, 0.68f, 0.32f);
            m_BtnStartMission = startGO.AddComponent<Button>();
            var startOutline = startGO.AddComponent<Outline>();
            startOutline.effectColor = new Color(0.2f, 1f, 0.5f);
            startOutline.effectDistance = new Vector2(2f, 2f);

            var startTxtGO = new GameObject("Txt");
            startTxtGO.transform.SetParent(startGO.transform, false);
            var str = startTxtGO.AddComponent<RectTransform>();
            str.anchorMin = Vector2.zero;
            str.anchorMax = Vector2.one;
            var sTxt = startTxtGO.AddComponent<Text>();
            sTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sTxt.text = "▶️ BẮT ĐẦU NHIỆM VỤ [ ENTER / SPACE ]";
            sTxt.fontSize = 17;
            sTxt.fontStyle = FontStyle.Bold;
            sTxt.alignment = TextAnchor.MiddleCenter;
            sTxt.color = Color.white;

            m_PanelRoot = panelGO;
            AutoWireComponents();
        }

        private void AutoWireComponents()
        {
            if (m_BtnBack != null)
            {
                m_BtnBack.onClick.RemoveAllListeners();
                m_BtnBack.onClick.AddListener(OnBackClicked);
            }

            if (m_BtnStartMission != null)
            {
                m_BtnStartMission.onClick.RemoveAllListeners();
                m_BtnStartMission.onClick.AddListener(OnStartMissionClicked);
            }
        }

        private void Update()
        {
            if (!m_IsOpen) return;

            if (IUHInputHelper.IsEscapeKeyDown())
            {
                OnBackClicked();
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
                 UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                OnStartMissionClicked();
                return;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnStartMissionClicked();
                    return;
                }
            }
            catch { }
        }

        private void OnBackClicked()
        {
            ClosePreparation();
            if (IUHGameFlowManager.Instance != null)
            {
                IUHGameFlowManager.Instance.GoToMiniGameSelection();
            }
        }

        private void OnStartMissionClicked()
        {
            ClosePreparation();
            if (IUHGameFlowManager.Instance != null)
            {
                IUHGameFlowManager.Instance.StartPlaying(m_CurrentGameIndex);
            }
        }
    }
}
