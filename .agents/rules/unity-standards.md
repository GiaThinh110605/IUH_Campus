# Quy chuẩn Phát triển Unity (Unity Standards)

Áp dụng cho toàn bộ dự án Unity IUH Campus (IUH Campus Guardian, UI, Touch Input, Scene Architecture).

## 1. Hiệu năng & Vòng lặp Khung hình (Performance & Lifecycle)
- **Cấm gọi `GetComponent()`, `Find()`, `FindObjectOfType()` trong `Update()` / `FixedUpdate()`**:
  - Tất cả reference phải được cache trong `Awake()` hoặc `Start()`, hoặc gán qua `[SerializeField] private` trong Inspector.
- **Tối ưu hóa bộ nhớ (GC Allocation)**:
  - Tránh khởi tạo đối tượng mới (`new List()`, chuỗi string cộng chuỗi `+` nhiều lần) liên tục trong mỗi frame.
  - Sử dụng object pooling nếu có hiệu ứng hoặc gem sinh ra liên tục.

## 2. Giao diện & Điều khiển Đa nền tảng (UI & Multiplatform Controls)
- **Định dạng Màn hình chuẩn (Screen Orientation)**:
  - Game chạy cố định theo chiều ngang (**Landscape Left / Right**). Tránh các thành phần UI bị tràn cạnh hoặc vỡ layout trên tỉ lệ màn hình điện thoại (16:9, 19.5:9, 20:9).
- **Hệ thống Điều khiển Cảm ứng (Touch & Pointer Controls)**:
  - Sử dụng các interface chuẩn của Unity EventSystem: `IPointerDownHandler`, `IPointerUpHandler`, `IDragHandler`, `IPointerClickHandler`.
  - Tách bạch rõ ràng giữa xử lý điều khiển nhân vật ảo trên điện thoại (Virtual D-Pad / Joystick) và tương tác UI Canvas.
  - Tránh xung đột sự kiện chạm (Raycast Target): Chỉ bật `Raycast Target` trên các Image/Text thực sự cần nhận sự kiện click/chạm; tắt trên các hình nền, icon trang trí.

## 3. Quản lý Scene & Độc lập Module (Scene Isolation)
- **Bảo toàn Scene gốc**:
  - Không chỉnh sửa trực tiếp logic các mini-game khác (`Grab Campus`, `Thợ săn khoảnh khắc`, `Green Campus`).
  - Toàn bộ tính năng mới (Guardian, AI Detection AR, Match-3 Combat) phải nằm trong thư mục và namespace riêng biệt:
    - Code: `Assets/IUH_Campus/Scripts/Guardian/`
    - Prefabs: `Assets/IUH_Campus/Prefabs/Guardian/`
    - Scene test / Gameplay: `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity` hoặc scene chuyên biệt.
- **Không phá vỡ cấu trúc Menu chung**:
  - Các nút chuyển tiếp và menu navigation chính của IUH Campus phải được tôn trọng, không tự ý gỡ bỏ Canvas chung hay thay đổi ID điều hướng.

