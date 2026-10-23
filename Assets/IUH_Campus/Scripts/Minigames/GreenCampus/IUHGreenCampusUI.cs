using System;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Minigames.GreenCampus
{
    public class IUHGreenCampusUI : MonoBehaviour
    {
        public GameObject briefing;
        public GameObject hud;
        public GameObject result;
        public Text briefingStatus;
        public Text stats;
        public Text hint;
        public Text notice;
        public Text resultTitle;
        public Text resultStats;
        public Button begin;
        public Button newRound;
        public Button briefingClose;
        public Button pickup;
        public Button drop;
        public Button hudClose;
        public Button retry;
        public Button resultClose;
        public Button[] sortButtons;
        private static readonly Color Ink = new Color(0.04f, 0.11f, 0.12f, 0.97f);
        private static readonly Color Green = new Color(0.02f, 0.58f, 0.35f);

        private void Start()
        {
            begin.onClick.AddListener(() => IUHGreenCampusManager.Instance?.BeginRound());
            newRound.onClick.AddListener(() => IUHGreenCampusManager.Instance?.BeginRound(true));
            briefingClose.onClick.AddListener(() => IUHGreenCampusManager.Instance?.CancelMission());
            pickup.onClick.AddListener(() => IUHGreenCampusManager.Instance?.Interact());
            drop.onClick.AddListener(() => IUHGreenCampusManager.Instance?.DropCarried());
            hudClose.onClick.AddListener(() => IUHGreenCampusManager.Instance?.CancelMission());
            retry.onClick.AddListener(() => IUHGreenCampusManager.Instance?.BeginRound(true));
            resultClose.onClick.AddListener(() => IUHGreenCampusManager.Instance?.CancelMission());
            for (int i = 0; i < sortButtons.Length; i++)
            {
                WasteCategory category = (WasteCategory)i;
                sortButtons[i].onClick.AddListener(() => IUHGreenCampusManager.Instance?.TrySort(category));
            }
        }

        public void Refresh(IUHGreenCampusManager manager)
        {
            bool visible = manager.HasActiveSessionOrUI && !manager.ExternalUIOpen();
            briefing.SetActive(visible && manager.State == IUHGreenCampusManager.GameState.Briefing);
            hud.SetActive(visible && manager.State == IUHGreenCampusManager.GameState.Playing);
            bool ended = manager.State == IUHGreenCampusManager.GameState.Victory || manager.State == IUHGreenCampusManager.GameState.Defeat;
            result.SetActive(visible && ended);
            if (briefingStatus != null)
                briefingStatus.text = manager.Progress.hasRound
                    ? $"Đã phân loại {manager.SortedCount}/{manager.TotalItems} • Kỷ lục {manager.Progress.bestScore} điểm"
                    : "Một hành động nhỏ, một khuôn viên xanh hơn.";
            begin.GetComponentInChildren<Text>().text = manager.Progress.hasRound ? "TIẾP TỤC LƯỢT ĐÃ LƯU" : "BẮT ĐẦU DỌN DẸP";
            int seconds = Mathf.CeilToInt(manager.Progress.secondsRemaining);
            stats.text = $"<b>{manager.CurrentScore} ĐIỂM XANH</b>     {seconds / 60:00}:{seconds % 60:00}\n" +
                $"Đã phân loại: {manager.SortedCount}/{manager.TotalItems}     Khu sạch: {manager.CleanZoneCount}/3\n" +
                "Đang cầm: " + (manager.CarriedItem != null ? manager.CarriedItem.displayName : "chưa có — nhấn F để nhặt");
            hint.text = manager.CarriedItem == null
                ? "Tìm rác có nhãn trong sân trường.\nF: nhặt • Tab: con trỏ • M: menu"
                : "Mang về đúng thùng. F: bỏ vào thùng gần nhất.\n1: tái chế • 2: hữu cơ • 3: còn lại • Q: đặt lại";
            bool canSort = manager.CarriedItem != null && manager.NearStation;
            foreach (var button in sortButtons) button.interactable = canSort;
            drop.interactable = manager.CarriedItem != null;
            notice.gameObject.SetActive(visible && !string.IsNullOrEmpty(manager.Toast));
            notice.text = manager.Toast;
            if (ended)
            {
                bool won = manager.State == IUHGreenCampusManager.GameState.Victory;
                resultTitle.text = won ? "KHUÔN VIÊN ĐÃ XANH TRỞ LẠI!" : "HẾT GIỜ — THỬ LẠI NHÉ!";
                resultStats.text = $"<size=42><b>{manager.CurrentScore} ĐIỂM XANH</b></size>\n\n" +
                    $"Phân loại đúng: {manager.SortedCount}/{manager.TotalItems}\n" +
                    $"Khu vực phục hồi: {manager.CleanZoneCount}/3\n" +
                    $"Bỏ sai thùng: {manager.Progress.wrongAttempts} lần\n" +
                    $"Kỷ lục: {manager.Progress.bestScore} điểm\n\n" +
                    (won ? "Cảm ơn bạn đã góp sức chăm sóc campus!" : "Tiếp tục khám phá hoặc bắt đầu lượt mới.");
            }
        }

        public static IUHGreenCampusUI Build(Transform parent)
        {
            var root = new GameObject("GreenCampus_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(IUHGreenCampusUI));
            root.transform.SetParent(parent, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 220;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var ui = root.GetComponent<IUHGreenCampusUI>();

            ui.briefing = Panel(root.transform, "Briefing", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.01f, 0.04f, 0.04f, 0.82f));
            var card = Panel(ui.briefing.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-510, -350), new Vector2(510, 350), Ink);
            Label(card.transform, "Title", "GREEN CAMPUS", 45, new Vector2(35, -90), new Vector2(-35, -22), TextAnchor.UpperCenter);
            ui.briefingStatus = Label(card.transform, "Status", "", 23, new Vector2(40, -135), new Vector2(-40, -100), TextAnchor.MiddleCenter);
            var body = Label(card.transform, "Instructions",
                "<b>4 PHÚT • 9 MÓN RÁC • 3 KHU VỰC</b>\n\n" +
                "Đi quanh sân trường, nhặt rác và mang về trạm phân loại.\n" +
                "<color=#58baff>CHAI NHỰA → TÁI CHẾ</color>\n" +
                "<color=#67dc8c>VỎ CHUỐI → HỮU CƠ</color>\n" +
                "<color=#ffca66>VỎ BÁNH → CÒN LẠI</color>\n\n" +
                "Đúng món +20 điểm • Sạch cả khu +30 điểm • Sai thùng -5 điểm.\n" +
                "F: nhặt / bỏ rác • 1/2/3: chọn thùng khi đứng gần trạm.\n" +
                "Q: đặt lại món đang cầm • Tab: mở con trỏ để dùng nút.\n\n" +
                "<size=19>Chế độ phân loại thủ công • Tiến trình lưu trên thiết bị.</size>",
                24, new Vector2(55, -170), new Vector2(-55, -550), TextAnchor.UpperLeft);
            ui.begin = ButtonAt(card.transform, "Begin", "BẮT ĐẦU DỌN DẸP", 23, 0.06f, 0.50f, 0.07f, 0.17f, Green);
            ui.newRound = ButtonAt(card.transform, "New", "LƯỢT MỚI", 22, 0.53f, 0.81f, 0.07f, 0.17f, new Color(0.13f, 0.30f, 0.36f));
            ui.briefingClose = ButtonAt(card.transform, "Close", "ĐÓNG", 22, 0.84f, 0.94f, 0.07f, 0.17f, new Color(0.42f, 0.16f, 0.16f));

            ui.hud = Panel(root.transform, "HUD", Vector2.zero, Vector2.zero, new Vector2(30, 30), new Vector2(600, 390), Ink);
            Label(ui.hud.transform, "Heading", "GREEN CAMPUS  •  PHÂN LOẠI THỦ CÔNG", 23, new Vector2(20, -42), new Vector2(-20, -10), TextAnchor.UpperLeft);
            ui.stats = Label(ui.hud.transform, "Stats", "", 25, new Vector2(20, -150), new Vector2(-20, -55), TextAnchor.UpperLeft);
            ui.hint = Label(ui.hud.transform, "Hint", "", 20, new Vector2(20, -213), new Vector2(-20, -155), TextAnchor.UpperLeft);
            ui.sortButtons = new Button[3];
            var colors = new[] { new Color(0.08f, 0.38f, 0.75f), Green, new Color(0.65f, 0.38f, 0.08f) };
            for (int i = 0; i < 3; i++)
                ui.sortButtons[i] = ButtonAt(ui.hud.transform, "Sort" + i, (i + 1) + ": " + IUHGreenBin.CategoryLabel((WasteCategory)i), 19,
                    0.035f + i * 0.32f, 0.335f + i * 0.32f, 0.21f, 0.35f, colors[i]);
            ui.pickup = ButtonAt(ui.hud.transform, "Interact", "F • TƯƠNG TÁC", 21, 0.035f, 0.49f, 0.04f, 0.18f, Green);
            ui.drop = ButtonAt(ui.hud.transform, "Drop", "Q • ĐẶT LẠI", 20, 0.51f, 0.80f, 0.04f, 0.18f, new Color(0.15f, 0.29f, 0.32f));
            ui.hudClose = ButtonAt(ui.hud.transform, "Leave", "THOÁT", 18, 0.82f, 0.97f, 0.04f, 0.18f, new Color(0.42f, 0.16f, 0.16f));

            ui.result = Panel(root.transform, "Result", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.01f, 0.04f, 0.04f, 0.8f));
            var resultCard = Panel(ui.result.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-480, -320), new Vector2(480, 320), Ink);
            ui.resultTitle = Label(resultCard.transform, "Title", "", 33, new Vector2(35, -85), new Vector2(-35, -30), TextAnchor.UpperCenter);
            ui.resultStats = Label(resultCard.transform, "Stats", "", 27, new Vector2(45, -125), new Vector2(-45, -500), TextAnchor.UpperCenter);
            ui.retry = ButtonAt(resultCard.transform, "Retry", "CHƠI LẠI", 24, 0.08f, 0.46f, 0.06f, 0.18f, Green);
            ui.resultClose = ButtonAt(resultCard.transform, "Close", "VỀ KHÁM PHÁ CAMPUS", 23, 0.50f, 0.92f, 0.06f, 0.18f, new Color(0.12f, 0.30f, 0.36f));
            ui.notice = Label(root.transform, "Notice", "", 25, new Vector2(240, -180), new Vector2(-240, -115), TextAnchor.MiddleCenter);
            ui.notice.color = new Color(1f, 0.88f, 0.48f);
            ui.briefing.SetActive(false); ui.hud.SetActive(false); ui.result.SetActive(false); ui.notice.gameObject.SetActive(false);
            return ui;
        }

        private static GameObject Rect(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax; return go;
        }
        private static GameObject Panel(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = Rect(parent, name, min, max, offsetMin, offsetMax); go.AddComponent<Image>().color = color;
            var outline = go.AddComponent<Outline>(); outline.effectColor = new Color(0.16f, 0.72f, 0.50f, 0.7f); return go;
        }
        private static Text Label(Transform parent, string name, string value, int size, Vector2 offsetMin, Vector2 offsetMax, TextAnchor alignment)
        {
            var go = Rect(parent, name, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(offsetMin.x, Mathf.Min(offsetMin.y, offsetMax.y)),
                new Vector2(offsetMax.x, Mathf.Max(offsetMin.y, offsetMax.y)));
            var text = go.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.color = Color.white; text.alignment = alignment; text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false; text.text = value; return text;
        }
        private static Button ButtonAt(Transform parent, string name, string title, int size, float x0, float x1, float y0, float y1, Color color)
        {
            var go = Rect(parent, name, new Vector2(x0, y0), new Vector2(x1, y1), Vector2.zero, Vector2.zero);
            go.AddComponent<Image>().color = color; var button = go.AddComponent<Button>();
            var labelGo = Rect(go.transform, "Text", Vector2.zero, Vector2.one, new Vector2(4, 3), new Vector2(-4, -3));
            var label = labelGo.AddComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size; label.color = Color.white; label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false; label.text = title;
            return button;
        }
    }
}
